#!/usr/bin/env python3
"""Tes end-to-end server Dunia Bersama (Fase 7) — HTTP nyata.

Menjalankan server.py di localhost (port bebas + DB sementara),
lalu menguji alur lengkap lewat HTTP sungguhan:
  daftar 2 pemain -> masuk -> ajukan izin -> bayar -> bangun di
  tanah sendiri -> bangun di tanah orang lain (DITOLAK) -> bangun
  tanpa koin (DITOLAK) -> syarat jalan (DITOLAK sampai ada Jalan)
  -> dunia terlihat sama oleh pemain kedua -> kas kota bertambah
  -> pemasukan offline (2 hari + batas 24 hari) -> jual bangunan
  -> batas laju 60/menit.
Ditambah cek langsung logika wali kota di db.py (cabang Bersyarat
dan Ditolak yang butuh reputasi rendah — tidak bisa dicapai lewat
API karena reputasi belum berubah di v1).

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
    for x in range(24):
        for y in range(24):
            if (x, y) in hindari:
                continue
            v = variasi(nama, x, y)
            if variasi_min <= v <= variasi_maks:
                return x, y, v
    return None


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

        # ---- Auth + dunia kosong ----
        kode, jawab = req(base, "GET", "/api/dunia")
        periksa(kode == 401, "dunia tanpa token -> 401")
        kode, jawab = req(base, "GET", "/api/dunia", token=token_raka)
        periksa(kode == 200 and jawab.get("ukuran") == 24,
                "dunia ukuran 24x24")
        periksa(jawab.get("bangunan") == [] and jawab.get("tanah") == [],
                "dunia awal kosong")
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 24, "y": 0, "bangunan": "rumah"})
        periksa(kode == 400, "izin di luar peta ditolak")
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 5, "y": 5, "bangunan": "tidak_ada"})
        periksa(kode == 400, "izin bangunan tak dikenal ditolak")

        # ---- Alur izin + bangun (Raka, pusat) ----
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 11, "y": 11, "bangunan": "rumah"})
        periksa(kode == 200, "ajukan izin rumah di pusat -> 200")
        kep = jawab.get("keputusan", {})
        periksa(kep.get("hasil") == "Disetujui",
                "izin rumah pusat disetujui (skor tinggi)")
        periksa(kep.get("harga_tanah") == 1400000,
                "harga tanah pusat Rp1.400.000 (dapat %s)"
                % kep.get("harga_tanah"))
        # Kas kota awal 0 (< Rp1jt, "menipis") -> biaya izin +50%:
        # 10% x 1,5 = Rp210.000.
        periksa(kep.get("biaya_izin") == 210000,
                "biaya izin pusat 10%% +50%% kas menipis = Rp210.000 (dapat %s)"
                % kep.get("biaya_izin"))
        izin_id = jawab.get("izin_id")
        total_pusat = kep.get("total", 0)
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_raka,
                          badan={"izin_id": izin_id})
        periksa(kode == 200 and jawab.get("koin") == 5000000 - total_pusat,
                "bayar izin pusat: koin 5.000.000 - %d" % total_pusat)
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_raka,
                          badan={"izin_id": izin_id})
        periksa(kode == 400, "bayar izin yang sama 2x ditolak")
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_sinta,
                          badan={"x": 11, "y": 11, "bangunan": "rumah"})
        periksa(kode == 400, "Sinta ajukan izin petak milik Raka ditolak")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_sinta,
                          badan={"x": 11, "y": 11, "bangunan": "rumah"})
        periksa(kode == 400, "Sinta bangun di tanah Raka DITOLAK")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 11, "y": 11, "bangunan": "rumah"})
        periksa(kode == 200 and jawab.get("koin") == 5000000 - total_pusat - 500000,
                "Raka bangun rumah di tanah sendiri (koin -Rp500.000)")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 11, "y": 11, "bangunan": "rumah"})
        periksa(kode == 400, "bangun di petak sudah terisi ditolak")

        # ---- Tanah pinggir untuk warung (izin kampus, bangun warung) ----
        # (0,1): jarak Chebyshev 11 dari pusat -> Rp300.000. Kas kota
        # kini Rp1.610.000 (> Rp1jt) jadi biaya izin normal 10%.
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 0, "y": 1, "bangunan": "kampus"})
        periksa(kode == 200, "ajukan izin kampus di pinggir -> 200")
        kep2 = jawab.get("keputusan", {})
        periksa(kep2.get("harga_tanah") == 300000,
                "harga tanah (0,1) Rp300.000 (dapat %s)"
                % kep2.get("harga_tanah"))
        periksa(kep2.get("biaya_izin") == 30000,
                "biaya izin (0,1) 10%% = Rp30.000 (dapat %s)"
                % kep2.get("biaya_izin"))
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_raka,
                          badan={"izin_id": jawab.get("izin_id")})
        periksa(kode == 200, "bayar izin kampus (tanah pinggir)")
        total_kampus = kep2.get("total", 0)

        # ---- Warung untuk pemasukan offline ----
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 0, "y": 1, "bangunan": "warung"})
        periksa(kode == 200, "Raka bangun warung di (0,1)")

        # ---- Koin tidak cukup (kafe Rp2jt di tanah sendiri) ----
        # Raka: 5.000.000 - 1.610.000 (pusat) - 500.000 (rumah)
        # - 330.000 (tanah kampus) - 750.000 (warung) = Rp1.810.000.
        kode, jawab = req(base, "POST", "/api/ajukan-izin", token=token_raka,
                          badan={"x": 0, "y": 2, "bangunan": "kafe"})
        periksa(kode == 200, "ajukan izin kafe di (0,2) -> 200")
        kep_kafe = jawab.get("keputusan", {})
        total_kafe = kep_kafe.get("total", 0)
        kode, jawab = req(base, "POST", "/api/bayar-izin", token=token_raka,
                          badan={"izin_id": jawab.get("izin_id")})
        periksa(kode == 200, "bayar izin kafe (tanah pinggir)")
        kode, jawab = req(base, "POST", "/api/bangun", token=token_raka,
                          badan={"x": 0, "y": 2, "bangunan": "kafe"})
        periksa(kode == 400 and "Koin tidak cukup" in jawab.get("kesalahan", ""),
                "bangun kafe Rp2jt dengan koin ~Rp1.480.000 DITOLAK")

        # ---- Dunia terlihat sama oleh pemain kedua + kas kota ----
        kode, jawab = req(base, "GET", "/api/dunia", token=token_sinta)
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa(bang.get((11, 11), {}).get("id") == "rumah"
                and bang.get((11, 11), {}).get("pemilik") == "Raka",
                "Sinta melihat rumah Raka di (11,11)")
        periksa(bang.get((0, 1), {}).get("id") == "warung",
                "Sinta melihat warung Raka di (0,1)")
        tanah = {(t["x"], t["y"]): t["pemilik"]
                 for t in jawab.get("tanah", [])}
        periksa(tanah.get((11, 11)) == "Raka"
                and tanah.get((0, 1)) == "Raka",
                "tanah Raka terlihat di dunia Sinta")
        kode, jawab = req(base, "GET", "/api/kas-kota", token=token_raka)
        # Kas kota = total izin pusat + total izin kampus + total izin kafe.
        periksa(jawab.get("saldo") == total_pusat + total_kampus + total_kafe,
                "kas kota = %d (dapat %s)"
                % (total_pusat + total_kampus + total_kafe,
                   jawab.get("saldo")))
        periksa(len(jawab.get("riwayat", [])) == 3,
                "riwayat kas kota 3 entri")

        # ---- Pemasukan offline (2 hari, lalu batas 24 hari) ----
        kon = sqlite3.connect(db_path)
        now = int(time.time())
        # Mundurkan terakhir_masuk Raka 2 hari game + 10 detik.
        kon.execute("UPDATE pemain SET terakhir_masuk = ? WHERE nama = 'Raka'",
                    (now - 2 * 300 - 10,))
        kon.commit()
        kode, jawab = req(base, "POST", "/api/masuk",
                          badan={"nama": "Raka", "sandi": "sandi123"})
        # Raka: rumah (0 - 1000) + warung (15000 - 2000) per hari.
        # kotor 2 hari = 30000, pajak 750, upkeep 6000, bersih 23250.
        periksa(jawab.get("hari_offline") == 2, "hari offline = 2")
        periksa(jawab.get("pemasukan_offline") == 23250,
                "pemasukan offline 2 hari = Rp23.250 (dapat %s)"
                % jawab.get("pemasukan_offline"))
        token_raka = jawab.get("token")
        kon.execute("UPDATE pemain SET terakhir_masuk = ? WHERE nama = 'Raka'",
                    (now - 30 * 24 * 3600,))
        kon.commit()
        kode, jawab = req(base, "POST", "/api/masuk",
                          badan={"nama": "Raka", "sandi": "sandi123"})
        periksa(jawab.get("hari_offline") == 24,
                "hari offline dibatasi 24 (dapat %s)"
                % jawab.get("hari_offline"))
        token_raka = jawab.get("token")
        kon.close()

        # ---- Jual: kembali 50%, tanah tetap milik ----
        kode, jawab = req(base, "POST", "/api/jual", token=token_raka,
                          badan={"x": 0, "y": 1})
        periksa(kode == 200 and jawab.get("harga_jual") == 375000,
                "jual warung kembali 50%% = Rp375.000")
        kode, jawab = req(base, "GET", "/api/dunia", token=token_raka)
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa((0, 1) not in bang, "warung hilang dari dunia sesudah jual")
        tanah = {(t["x"], t["y"]): t["pemilik"]
                 for t in jawab.get("tanah", [])}
        periksa(tanah.get((0, 1)) == "Raka",
                "tanah (0,1) tetap milik Raka sesudah jual")
        kode, jawab = req(base, "POST", "/api/jual", token=token_raka,
                          badan={"x": 0, "y": 1})
        periksa(kode == 400, "jual petak tanpa bangunan ditolak")
        kode, jawab = req(base, "POST", "/api/jual", token=token_sinta,
                          badan={"x": 11, "y": 11})
        periksa(kode == 400, "Sinta jual rumah Raka DITOLAK")

        # ---- Sinkron sesi (koin/reputasi klien, terbatas) ----
        # Koin Raka saat ini: 5.000.000 - 1.610.000 - 500.000
        # - 330.000 - 750.000 - 330.000 + 23.250 (offline 2 hari)
        # + 279.000 (offline 24 hari) + 375.000 (jual) = Rp2.157.250.
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": 2157250, "reputasi": 52})
        periksa(kode == 200 and jawab.get("koin") == 2157250,
                "sinkron koin Rp2.157.250 + reputasi 52 diterima")
        kode, jawab = req(base, "GET", "/api/pemain", token=token_raka)
        periksa(jawab.get("pemain", {}).get("reputasi") == 52,
                "reputasi 52 tersimpan lewat sinkron")
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": 99999999, "reputasi": 52})
        periksa(kode == 400 and "batas sinkron" in jawab.get("kesalahan", ""),
                "sinkron +Rp98jt DITOLAK (melebihi batas Rp6jt)")
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": -100, "reputasi": 52})
        periksa(kode == 400,
                "sinkron koin negatif DITOLAK")
        kode, jawab = req(base, "POST", "/api/sinkron", token=token_raka,
                          badan={"koin": 2157250, "reputasi": 150})
        periksa(kode == 400,
                "sinkron reputasi 150 DITOLAK")
        kode, jawab = req(base, "POST", "/api/sinkron",
                          badan={"koin": 2157250, "reputasi": 52})
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
                "impor 2 bangunan Budi (diimpor %s, dilewati %s)"
                % (jawab.get("diimpor"), jawab.get("dilewati")))
        kode, jawab = req(base, "GET", "/api/dunia", token=token_budi)
        bang = {(b["x"], b["y"]): b for b in jawab.get("bangunan", [])}
        periksa(bang.get((0, 0), {}).get("pemilik") == "Budi"
                and bang.get((0, 0), {}).get("id") == "rumah",
                "rumah Budi (0,0) terlihat di dunia")
        periksa(bang.get((1, 0), {}).get("id") == "warung",
                "warung Budi (1,0) terlihat di dunia")
        kode, jawab = req(base, "POST", "/api/impor-tanah", token=token_budi,
                          badan={"bangunan": [{"x": 5, "y": 5, "id": "taman"}]})
        periksa(kode == 400,
                "impor kedua Budi DITOLAK (hanya sekali)")
        kode, jawab = req(base, "POST", "/api/daftar",
                          badan={"nama": "Citra", "sandi": "citra1234"})
        token_citra = jawab.get("token")
        kode, jawab = req(base, "POST", "/api/impor-tanah", token=token_citra,
                          badan={"bangunan": [{"x": 0, "y": 0, "id": "taman"},
                                              {"x": 5, "y": 5, "id": "tidak_ada"},
                                              {"x": 6, "y": 6, "id": "taman"}]})
        periksa(kode == 200 and jawab.get("diimpor") == 1
                and jawab.get("dilewati") == 2,
                "impor Citra: 1 masuk, 2 dilewati (bentrok + id salah)")
        kode, jawab = req(base, "POST", "/api/impor-tanah",
                          badan={"bangunan": [{"x": 7, "y": 7, "id": "taman"}]})
        periksa(kode == 401, "impor tanpa token -> 401")

        # ---- Cabang wali kota di db.py (reputasi rendah, langsung) ----
        import db as dbmod
        db = dbmod.DuniaDB(db_path)
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
                                    (11, 11), (0, 1), (0, 2),
                                    (20, 20), (21, 20), (20, 21),
                                    (21, 21), (22, 22)})
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
            # Bayar izin bersyarat, lalu bangun TANPA jalan -> ditolak.
            hasil, salah = db.ajukan_izin(pemain, x, y, "warung")
            periksa(salah is None and hasil is not None,
                    "ajukan izin Bersyarat tersimpan")
            if hasil:
                # Ambil ulang profil (koin di dict bisa basi).
                pemain = dict(db.koneksi.execute(
                    "SELECT * FROM pemain WHERE nama = 'Uji'").fetchone())
                hasil2, salah2 = db.bayar_izin(pemain, hasil["izin_id"])
                periksa(salah2 is None, "bayar izin Bersyarat berhasil")
                hasil3, salah3 = db.bangun(pemain, x, y, "warung")
                periksa(salah3 is not None
                        and "Jalan" in (salah3 or ""),
                        "bangun tanpa Jalan bersebelahan DITOLAK")
                # Beli tanah sebelah, bangun Jalan, lalu warung boleh.
                px, py = (x + 1, y) if x + 1 < 24 else (x - 1, y)
                h4, s4 = db.ajukan_izin(pemain, px, py, "jalan")
                periksa(s4 is None, "ajukan izin Jalan sebelah")
                if h4 and h4["keputusan"]["hasil"] != "Ditolak":
                    h5, s5 = db.bayar_izin(pemain, h4["izin_id"])
                    periksa(s5 is None, "bayar izin Jalan")
                    h6, s6 = db.bangun(pemain, px, py, "jalan")
                    periksa(s6 is None, "bangun Jalan di sebelah")
                    h7, s7 = db.bangun(pemain, x, y, "warung")
                    periksa(s7 is None,
                            "warung boleh dibangun sesudah ada Jalan")
        # Kepadatan: 4+ sejenis di kawasan -> Ditolak.
        db.koneksi.execute(
            "UPDATE pemain SET reputasi = 50 WHERE nama = 'Uji'")
        db.koneksi.commit()
        pemain["reputasi"] = 50
        # Isi kawasan (20..21, 20..21) dengan 4 taman milik Uji.
        for (ax, ay) in ((20, 20), (21, 20), (20, 21), (21, 21)):
            db.koneksi.execute(
                "INSERT OR REPLACE INTO petak (x, y, pemilik_id,"
                " bangunan_id, jangkar_x, jangkar_y) VALUES (?, ?, ?, 'taman', ?, ?)",
                (ax, ay, pemain["id"], ax, ay))
        db.koneksi.commit()
        kep = db.putuskan_izin(pemain, 22, 22, "taman")
        periksa(kep["hasil"] == "Ditolak",
                "4 taman di kawasan -> izin taman ke-5 Ditolak")
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
