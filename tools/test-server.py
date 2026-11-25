#!/usr/bin/env python3
"""Tes end-to-end server Dunia Bersama — HTTP nyata.

Menjalankan server.py di localhost (port bebas + DB sementara),
lalu menguji alur lengkap lewat HTTP sungguhan dengan aturan TYCOON
MULTIPEMAIN (diputuskan 25 Nov 2026):
  daftar 2 pemain -> masuk -> fasilitas umum DITOLAK untuk pemain
  -> izin + bangun kost & warung -> warga NPC menyewa kamar dan
  berbelanja (sewa & penjualan masuk ke koin pemilik) -> dunia sama
  untuk kedua pemain -> pemasukan offline 2 hari + batas 24 hari
  -> jual bangunan -> sinkron (koin tidak pernah turun dari sinkron)
  -> impor tanah -> cabang wali kota -> batas laju 60/menit.

Jalankan:  python3 tools/test-server.py
Keluar kode 0 bila semua pemeriksaan lolos.
"""

import json
import os
import socket
import sqlite3
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.request

DIR = os.path.dirname(os.path.abspath(__file__))
SERVER = os.path.join(DIR, "..", "server")
sys.path.insert(0, SERVER)

import db as dbmod  # noqa: E402 — dipakai untuk konstanta & cabang langsung

U = dbmod.UKURAN_PETA
HARI = dbmod.DURASI_HARI_DETIK

LOLOS = 0
GAGAL = 0


def periksa(kondisi, pesan):
    global LOLOS, GAGAL
    if kondisi:
        LOLOS += 1
    else:
        GAGAL += 1
        print("GAGAL: " + pesan)


def req(base, metode, jalur, token=None, badan=None):
    url = base + jalur
    data = json.dumps(badan).encode("utf-8") if badan is not None else None
    r = urllib.request.Request(url, data=data, method=metode)
    r.add_header("Content-Type", "application/json")
    if token:
        r.add_header("Authorization", "Bearer " + token)
    try:
        with urllib.request.urlopen(r, timeout=10) as resp:
            return resp.status, json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read().decode("utf-8"))


def variasi(nama, x, y):
    """Salinan rumus variasi skor db.py (untuk memilih petak uji)."""
    h = 0
    for ch in nama:
        h = h * 31 + ord(ch)
    h = abs(h)
    return (h + x * 131 + y * 17) % 11 - 5


def cari_petak(nama, variasi_min, variasi_maks, hindari=()):
    for x in range(U):
        for y in range(U):
            if (x, y) in hindari:
                continue
            if 14 <= x <= 22 and 17 <= y <= 21:
                continue  # kawasan fasilitas & jalan Kota
            v = variasi(nama, x, y)
            if variasi_min <= v <= variasi_maks:
                return x, y, v
    return None


def koin_di(base, token):
    kode, jawab = req(base, "GET", "/api/dunia", token=token)
    return jawab.get("profil", {}).get("koin"), jawab


def main():
    global LOLOS, GAGAL
    tmp = tempfile.mkdtemp(prefix="duniakita-test-")
    db_path = os.path.join(tmp, "uji.db")
    s = socket.socket()
    s.bind(("127.0.0.1", 0))
    port = s.getsockname()[1]
    s.close()
    base = "http://127.0.0.1:%d" % port
    proc = subprocess.Popen(
        [sys.executable, os.path.join(SERVER, "server.py"),
         "--host", "127.0.0.1", "--port", str(port), "--db", db_path],
        stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    try:
        # Tunggu server siap (maks 10 detik).
        siap = False
        mulai = time.time()
        while time.time() - mulai < 10:
            baris = proc.stdout.readline()
            if "DUNIAKITA_SIAP" in baris:
                siap = True
                break
            if proc.poll() is not None:
                break
        periksa(siap, "server mencetak DUNIAKITA_SIAP")
        if not siap:
            print("Keluaran server: " + (baris or ""))
            return 1

        kode, jawab = req(base, "GET", "/")
        periksa(kode == 200 and jawab.get("ok"), "GET / ok")

        # ---- Daftar + masuk ----
        kode, r_raka = req(base, "POST", "/api/daftar",
                           badan={"nama": "Raka", "sandi": "sandi123"})
        periksa(kode == 200 and r_raka.get("token"), "daftar Raka -> token")
        token_raka = r_raka.get("token")
        kode, r_sinta = req(base, "POST", "/api/daftar",
                            badan={"nama": "Sinta", "sandi": "sandi456"})
        periksa(kode == 200 and r_sinta.get("token"), "daftar Sinta -> token")
        token_sinta = r_sinta.get("token")
        kode, jawab = req(base, "POST", "/api/daftar",
                          badan={"nama": "Raka", "sandi": "lain"})
        periksa(kode == 400, "daftar nama sama ditolak (400)")
        kode, jawab = req(base, "POST", "/api/masuk",
                          badan={"nama": "Raka", "sandi": "salah"})
        periksa(kode == 400, "masuk kata sandi salah ditolak (400)")
        kode, jawab = req(base, "POST", "/api/masuk",
                          badan={"nama": "Raka", "sandi": "sandi123"})
        periksa(kode == 200 and jawab.get("pemasukan_offline") == 0,
                "masuk Raka benar, pemasukan offline 0 (baru daftar)")
        token_raka = jawab.get("token")

        # ---- Auth + dunia kota ----
        kode, jawab = req(base, "GET", "/api/dunia")
        periksa(kode == 401, "dunia tanpa token -> 401")
        kode, jawab = req(base, "GET", "/api/dunia", token=token_raka)
        periksa(kode == 200 and jawab.get("ukuran") == U,
                "dunia ukuran %dx%d" % (U, U))
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa(bang.get((18, 18), {}).get("id") == "balaikota"
                and bang.get((18, 18), {}).get("pemilik") == "Kota",
                "Balai Kota milik Kota ada di (18,18)")
        milik_pemain = [b for b in jawab.get("bangunan", [])
                        if b.get("pemilik") not in (None, "Kota")]
        periksa(milik_pemain == [], "belum ada bangunan milik pemain")
        periksa(jawab.get("warga", {}).get("jumlah") == dbmod.WARGA_PER_DISTRIK,
                "tiap distrik punya %d warga NPC" % dbmod.WARGA_PER_DISTRIK)

        # ---- Fasilitas umum milik Kota: pemain DITOLAK ----
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": U, "y": 0, "bangunan": "rumah"})
        periksa(kode == 400, "izin di luar peta ditolak")
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 5, "y": 5, "bangunan": "tidak_ada"})
        periksa(kode == 400, "izin bangunan tak dikenal ditolak")
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 24, "y": 24, "bangunan": "sekolah"})
        periksa(kode == 400 and "Kota" in jawab.get("kesalahan", ""),
                "izin sekolah DITOLAK (fasilitas Kota)")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 24, "y": 24, "bangunan": "klinik"})
        periksa(kode == 400 and "Kota" in jawab.get("kesalahan", ""),
                "bangun klinik DITOLAK (fasilitas Kota)")

        # ---- Raka: kost di (10,10) ----
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 10, "y": 10, "bangunan": "kost"})
        periksa(kode == 200, "ajukan izin kost di (10,10) -> 200")
        kep = jawab.get("keputusan", {})
        harga = dbmod.DuniaDB.harga_tanah(10, 10)
        biaya = harga * dbmod.BIAYA_IZIN_PERMIL // 1000
        biaya = biaya * (1000 + dbmod.KENAIKAN_KAS_PERMIL) // 1000
        biaya = biaya * (1000 - dbmod.DISKON_BALAIKOTA_PERMIL) // 1000
        periksa(kep.get("hasil") == "Disetujui",
                "izin kost (10,10) disetujui (dapat %s)" % kep.get("hasil"))
        periksa(kep.get("harga_tanah") == harga,
                "harga tanah (10,10) Rp%d (dapat %s)"
                % (harga, kep.get("harga_tanah")))
        periksa(kep.get("biaya_izin") == biaya,
                "biaya izin (10,10) Rp%d +50%% kas menipis (dapat %s)"
                % (biaya, kep.get("biaya_izin")))
        total_kost = kep.get("total", 0)
        izin_id = jawab.get("izin_id")
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_raka,
                          badan={"izin_id": izin_id})
        periksa(kode == 200 and jawab.get("koin") == 5000000 - total_kost,
                "bayar izin kost: koin 5.000.000 - %d" % total_kost)
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_raka,
                          badan={"izin_id": izin_id})
        periksa(kode == 400, "bayar izin yang sama 2x ditolak")
        # Kost 2x2: beli juga 3 petak lain supaya seluruh tapak milik Raka.
        koin_sekarang = 5000000 - total_kost
        total_izin_kost = total_kost
        for px, py in ((11, 10), (10, 11), (11, 11)):
            kode, jawab = req(base, "POST", "/api/ajukan-izin",
                              token=token_raka,
                              badan={"x": px, "y": py, "bangunan": "kost"})
            periksa(kode == 200 and jawab.get("keputusan", {}).get("hasil")
                    == "Disetujui", "izin petak kost (%d,%d)" % (px, py))
            total_izin_kost += jawab.get("keputusan", {}).get("total", 0)
            kode, jawab = req(base, "POST", "/api/bayar-izin",
                              token=token_raka,
                              badan={"izin_id": jawab.get("izin_id")})
            periksa(kode == 200, "bayar izin petak kost (%d,%d)" % (px, py))
            koin_sekarang = jawab.get("koin", koin_sekarang)
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 10, "y": 10, "bangunan": "kost"})
        biaya_kost = dbmod.BANGUNAN["kost"][0]
        periksa(kode == 200 and jawab.get("koin") == koin_sekarang - biaya_kost,
                "bangun kost (koin -Rp%d, dapat %s)"
                % (biaya_kost, jawab.get("kesalahan")))
        koin_setelah_kost = koin_sekarang - biaya_kost
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 10, "y": 10, "bangunan": "kost"})
        periksa(kode == 400, "bangun di petak sudah terisi ditolak")
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_sinta,
                          badan={"x": 10, "y": 10, "bangunan": "warung"})
        periksa(kode == 400, "Sinta ajukan izin petak milik Raka ditolak")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_sinta,
                          badan={"x": 10, "y": 10, "bangunan": "warung"})
        periksa(kode == 400, "Sinta bangun di tanah Raka DITOLAK")

        # ---- Sinta: warung di (13,13) ----
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_sinta,
                          badan={"x": 13, "y": 13, "bangunan": "warung"})
        periksa(kode == 200, "ajukan izin warung (13,13) -> 200")
        kep_warung = jawab.get("keputusan", {})
        total_warung = kep_warung.get("total", 0)
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_sinta,
                          badan={"izin_id": jawab.get("izin_id")})
        periksa(kode == 200, "bayar izin warung")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_sinta,
                          badan={"x": 13, "y": 13, "bangunan": "warung"})
        biaya_warung = dbmod.BANGUNAN["warung"][0]
        koin_sinta = 5000000 - total_warung - biaya_warung
        periksa(kode == 200 and jawab.get("koin") == koin_sinta,
                "Sinta bangun warung (koin Rp%d)" % koin_sinta)

        # ---- Dunia sama untuk kedua pemain ----
        kode, jawab = req(base, "GET", "/api/dunia", token=token_sinta)
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa(bang.get((10, 10), {}).get("id") == "kost"
                and bang.get((10, 10), {}).get("pemilik") == "Raka",
                "Sinta melihat kost Raka di (10,10)")
        tanah = {(t["x"], t["y"]): t["pemilik"]
                 for t in jawab.get("tanah", [])}
        periksa(tanah.get((13, 13)) == "Sinta"
                and tanah.get((10, 10)) == "Raka",
                "tanah kedua pemain terlihat di dunia bersama")

        # ---- Warga menyewa kamar & berbelanja (2 hari game) ----
        kon = sqlite3.connect(db_path, timeout=30)
        now = int(time.time())
        kon.execute("UPDATE simulasi SET hari_terakhir = ? WHERE distrik_id = 1",
                    (now // HARI - 2,))
        kon.commit()
        koin_raka, jawab = koin_di(base, token_raka)
        periksa(jawab.get("warga", {}).get("penyewa") == 8,
                "8 warga menyewa kamar kost Raka (dapat %s)"
                % jawab.get("warga", {}).get("penyewa"))
        sewa_harian = 8 * dbmod.SEWA_PER_KAMAR["kost"]
        jual_harian = 8 * dbmod.BELANJA_WARGA["warung"][1]
        bersih_raka = (sewa_harian - dbmod.BANGUNAN["kost"][2]
                       - (sewa_harian * dbmod.PAJAK_PERMIL // 1000)) * 2
        koin_raka_harap = koin_setelah_kost + bersih_raka
        periksa(koin_raka == koin_raka_harap,
                "koin Raka sesudah 2 hari = %d (dapat %s)"
                % (koin_raka_harap, koin_raka))
        koin_sinta_harap = (5000000 - total_warung - biaya_warung
                            + (jual_harian - dbmod.BANGUNAN["warung"][2]
                               - (jual_harian * dbmod.PAJAK_PERMIL // 1000)) * 2)
        koin_sinta_nyata, _ = koin_di(base, token_sinta)
        periksa(koin_sinta_nyata == koin_sinta_harap,
                "koin Sinta sesudah 2 hari = %d (dapat %s)"
                % (koin_sinta_harap, koin_sinta_nyata))
        kode, jawab = req(base, "GET", "/api/warga", token=token_sinta)
        daftar = jawab.get("warga", [])
        periksa(kode == 200 and len(daftar) == dbmod.WARGA_PER_DISTRIK,
                "daftar warga %d orang (dapat %d)"
                % (dbmod.WARGA_PER_DISTRIK, len(daftar)))
        penyewa = [w for w in daftar if w.get("hunian_x") == 10]
        tamu = [w for w in daftar if w.get("toko_x") == 13]
        periksa(len(penyewa) == 8 and len(tamu) == 8,
                "warga terlihat: 8 penyewa kost, 8 tamu warung "
                "(dapat %d & %d)" % (len(penyewa), len(tamu)))
        periksa(all(w.get("pemilik") == "Raka" for w in penyewa),
                "penyewa kost tercatat atas nama Raka")
        kode, jawab = req(base, "GET", "/api/kas-kota", token=token_raka)
        pajak_2hari = 2 * ((sewa_harian + jual_harian)
                           * dbmod.PAJAK_PERMIL // 1000)
        periksa(jawab.get("saldo") == total_izin_kost + total_warung
                + pajak_2hari,
                "kas kota = izin + pajak warga (dapat %s, harap %d)"
                % (jawab.get("saldo"), total_kost + total_warung + pajak_2hari))

        # ---- Pemasukan offline (2 hari, lalu batas 24 hari) ----
        kon.execute("UPDATE pemain SET terakhir_masuk = ? WHERE nama = 'Raka'",
                    (now - 2 * HARI - 10,))
        kon.execute("UPDATE simulasi SET hari_terakhir = ? WHERE distrik_id = 1",
                    (now // HARI - 2,))
        kon.execute("DELETE FROM pendapatan_warga WHERE pemain_id ="
                    " (SELECT id FROM pemain WHERE nama = 'Raka')")
        kon.commit()
        kode, jawab = req(base, "POST", "/api/masuk",
                          badan={"nama": "Raka", "sandi": "sandi123"})
        harian_raka = (sewa_harian - dbmod.BANGUNAN["kost"][2]
                       - (sewa_harian * dbmod.PAJAK_PERMIL // 1000))
        periksa(jawab.get("hari_offline") == 2, "hari offline = 2")
        periksa(jawab.get("pemasukan_offline") == harian_raka * 2,
                "pemasukan offline 2 hari = Rp%d (dapat %s)"
                % (harian_raka * 2, jawab.get("pemasukan_offline")))
        token_raka = jawab.get("token")
        koin_raka, _ = koin_di(base, token_raka)
        periksa(koin_raka == koin_raka_harap + harian_raka * 2,
                "koin Raka bertambah pajak-bersih 2 hari (dapat %s)" % koin_raka)
        kon.execute("UPDATE pemain SET terakhir_masuk = ? WHERE nama = 'Raka'",
                    (now - 30 * 24 * 3600,))
        kon.execute("UPDATE simulasi SET hari_terakhir = ? WHERE distrik_id = 1",
                    (now // HARI - 30,))
        kon.commit()
        kode, jawab = req(base, "POST", "/api/masuk",
                          badan={"nama": "Raka", "sandi": "sandi123"})
        periksa(jawab.get("hari_offline") == dbmod.MAKS_HARI_OFFLINE,
                "hari offline dibatasi %d (dapat %s)"
                % (dbmod.MAKS_HARI_OFFLINE, jawab.get("hari_offline")))
        token_raka = jawab.get("token")
        kon.close()

        # ---- Jual: kembali 50%, tanah tetap milik ----
        kode, jawab = req(base, "POST", "/api/jual", token=token_sinta,
                          badan={"x": 13, "y": 13})
        periksa(kode == 200 and jawab.get("harga_jual") == biaya_warung // 2,
                "jual warung kembali 50%% = Rp%d" % (biaya_warung // 2))
        kode, jawab = req(base, "GET", "/api/dunia", token=token_sinta)
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa((13, 13) not in bang, "warung hilang dari dunia sesudah jual")
        tanah = {(t["x"], t["y"]): t["pemilik"]
                 for t in jawab.get("tanah", [])}
        periksa(tanah.get((13, 13)) == "Sinta",
                "tanah (13,13) tetap milik Sinta sesudah jual")
        kode, jawab = req(base, "POST", "/api/jual", token=token_raka,
                          badan={"x": 13, "y": 13})
        periksa(kode == 400, "jual petak tanpa bangunan ditolak")
        kode, jawab = req(base, "POST", "/api/jual", token=token_sinta,
                          badan={"x": 10, "y": 10})
        periksa(kode == 400, "Sinta jual kost Raka DITOLAK")

        # ---- Sinkron: koin tidak pernah turun dari sinkron ----
        koin_raka, _ = koin_di(base, token_raka)
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": 100, "reputasi": 52})
        periksa(kode == 200 and jawab.get("koin") == koin_raka,
                "sinkron koin lebih rendah tidak menghapus pendapatan warga")
        kode, jawab = req(base, "GET", "/api/pemain", token=token_raka)
        periksa(jawab.get("pemain", {}).get("reputasi") == 52,
                "reputasi 52 tersimpan lewat sinkron")
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": 99999999, "reputasi": 52})
        periksa(kode == 400 and "batas sinkron" in jawab.get("kesalahan", ""),
                "sinkron +Rp98jt DITOLAK (melebihi batas Rp6jt)")
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": -100, "reputasi": 52})
        periksa(kode == 400, "sinkron koin negatif DITOLAK")
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": koin_raka, "reputasi": 150})
        periksa(kode == 400, "sinkron reputasi 150 DITOLAK")
        kode, jawab = req(base, "POST", "/api/sinkron",
                          badan={"koin": koin_raka, "reputasi": 52})
        periksa(kode == 401, "sinkron tanpa token -> 401")

        # ---- Impor tanah single player (sekali, tanpa bayar) ----
        kode, jawab = req(base, "POST", "/api/daftar",
                          badan={"nama": "Budi", "sandi": "budi1234"})
        periksa(kode == 200, "daftar Budi -> 200")
        token_budi = jawab.get("token")
        kode, jawab = req(base, "POST", "/api/impor-tanah", token=token_budi,
                          badan={"bangunan": [{"x": 0, "y": 0, "id": "rumah"},
                                              {"x": 1, "y": 0, "id": "warung"}]})
        periksa(kode == 200 and jawab.get("diimpor") == 2
                and jawab.get("dilewati") == 0,
                "impor 2 usaha Budi (diimpor %s, dilewati %s)"
                % (jawab.get("diimpor"), jawab.get("dilewati")))
        kode, jawab = req(base, "GET", "/api/dunia", token=token_budi)
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa(bang.get((0, 0), {}).get("pemilik") == "Budi"
                and bang.get((0, 0), {}).get("id") == "rumah",
                "rumah Budi (0,0) terlihat di dunia")
        periksa(bang.get((1, 0), {}).get("id") == "warung",
                "warung Budi (1,0) terlihat di dunia")
        kode, jawab = req(base, "POST", "/api/impor-tanah", token=token_budi,
                          badan={"bangunan": [{"x": 5, "y": 5, "id": "warung"}]})
        periksa(kode == 400, "impor kedua Budi DITOLAK (hanya sekali)")
        kode, jawab = req(base, "POST", "/api/daftar",
                          badan={"nama": "Citra", "sandi": "citra1234"})
        token_citra = jawab.get("token")
        kode, jawab = req(base, "POST", "/api/impor-tanah", token=token_citra,
                          badan={"bangunan": [{"x": 0, "y": 0, "id": "kafe"},
                                              {"x": 5, "y": 5, "id": "tidak_ada"},
                                              {"x": 6, "y": 6, "id": "taman"}]})
        periksa(kode == 200 and jawab.get("diimpor") == 0
                and jawab.get("dilewati") == 3,
                "impor Citra: 0 masuk, 3 dilewati (bentrok, id salah, taman Kota)")
        kode, jawab = req(base, "POST", "/api/impor-tanah",
                          badan={"bangunan": [{"x": 7, "y": 7, "id": "warung"}]})
        periksa(kode == 401, "impor tanpa token -> 401")

        # ---- Cabang wali kota di db.py (reputasi rendah, langsung) ----
        import db as dbmod2
        db = dbmod2.DuniaDB(db_path)
        pemain_uji, _ = db.daftar("Uji", "uji1234")
        baris = db.koneksi.execute(
            "SELECT * FROM pemain WHERE nama = 'Uji'").fetchone()
        pemain = dict(baris)
        # Reputasi 10 -> ditolak otomatis (di bawah 15).
        db.koneksi.execute(
            "UPDATE pemain SET reputasi = 10 WHERE nama = 'Uji'")
        db.koneksi.commit()
        pemain["reputasi"] = 10
        kep = db.putuskan_izin(pemain, 5, 5, "rumah")
        periksa(kep["hasil"] == "Ditolak" and kep["total"] == 0,
                "reputasi 10 -> Ditolak otomatis")
        # Reputasi 20 + komersial + variasi negatif -> Bersyarat.
        db.koneksi.execute(
            "UPDATE pemain SET reputasi = 20 WHERE nama = 'Uji'")
        db.koneksi.commit()
        pemain["reputasi"] = 20
        petak = cari_petak("Uji", -5, -1,
                           hindari={(0, 0), (1, 0), (6, 6),
                                    (10, 10), (11, 10), (10, 11), (11, 11),
                                    (13, 13)})
        periksa(petak is not None, "ditemukan petak variasi negatif untuk Uji")
        if petak:
            x, y, v = petak
            kep = db.putuskan_izin(pemain, x, y, "warung")
            # skor = 50 -10 + 0 + v -> 35..39 bila v -5..-1.
            periksa(kep["hasil"] == "Bersyarat",
                    "reputasi 20 + variasi %d -> Bersyarat (dapat %s)"
                    % (v, kep["hasil"]))
            periksa(kep["syarat"] == "jalan_bersebelahan",
                    "syarat Bersyarat warung = jalan bersebelahan")
            hasil, salah = db.ajukan_izin(pemain, x, y, "warung")
            periksa(salah is None and hasil is not None,
                    "ajukan izin Bersyarat tersimpan")
            if hasil:
                pemain = dict(db.koneksi.execute(
                    "SELECT * FROM pemain WHERE nama = 'Uji'").fetchone())
                hasil2, salah2 = db.bayar_izin(pemain, hasil["izin_id"])
                periksa(salah2 is None, "bayar izin Bersyarat berhasil")
                hasil3, salah3 = db.bangun(pemain, x, y, "warung")
                periksa(salah3 is not None and "Jalan" in (salah3 or ""),
                        "bangun tanpa Jalan bersebelahan DITOLAK")
                px, py = (x + 1, y) if x + 1 < U else (x - 1, y)
                h4, s4 = db.ajukan_izin(pemain, px, py, "jalan")
                periksa(s4 is None, "ajukan izin Jalan sebelah (%s)" % (s4,))
                if h4 and h4["keputusan"]["hasil"] != "Ditolak":
                    h5, s5 = db.bayar_izin(pemain, h4["izin_id"])
                    periksa(s5 is None, "bayar izin Jalan")
                    h6, s6 = db.bangun(pemain, px, py, "jalan")
                    periksa(s6 is None, "bangun Jalan di sebelah (%s)" % (s6,))
                    h7, s7 = db.bangun(pemain, x, y, "warung")
                    periksa(s7 is None,
                            "warung boleh dibangun sesudah ada Jalan (%s)"
                            % (s7,))
        # Kepadatan: 4+ sejenis di kawasan -> Ditolak.
        db.koneksi.execute(
            "UPDATE pemain SET reputasi = 50 WHERE nama = 'Uji'")
        db.koneksi.commit()
        pemain["reputasi"] = 50
        for (ax, ay) in ((30, 30), (31, 30), (30, 31), (31, 31)):
            db.koneksi.execute(
                "INSERT OR REPLACE INTO petak (distrik_id, x, y, pemilik_id,"
                " bangunan_id, jangkar_x, jangkar_y) VALUES"
                " (1, ?, ?, ?, 'warung', ?, ?)",
                (ax, ay, pemain["id"], ax, ay))
        db.koneksi.commit()
        kep = db.putuskan_izin(pemain, 32, 31, "warung")
        periksa(kep["hasil"] == "Ditolak",
                "4 warung di kawasan -> izin warung ke-5 Ditolak")
        # Fasilitas Kota juga ditolak lewat db.py langsung.
        pemain["reputasi"] = 50
        kep = db.putuskan_izin(pemain, 5, 5, "sekolah")
        periksa(kep["hasil"] == "Ditolak" and "Kota" in kep.get("alasan", ""),
                "sekolah Ditolak: fasilitas Kota (alasan wali kota)")
        db.tutup()

        # ---- Batas laju: 65 request cepat -> minimal satu 429 ----
        kena_429 = False
        for _ in range(65):
            kode, _ = req(base, "GET", "/api/dunia", token=token_raka)
            if kode == 429:
                kena_429 = True
                break
        periksa(kena_429, "batas laju 60/menit: ada 429")
    finally:
        proc.terminate()
        try:
            proc.wait(timeout=5)
        except subprocess.TimeoutExpired:
            proc.kill()
        import shutil
        shutil.rmtree(tmp, ignore_errors=True)

    print("HASIL: %d lolos, %d gagal" % (LOLOS, GAGAL))
    return 0 if GAGAL == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
