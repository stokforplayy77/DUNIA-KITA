"""Dunia Kita — server Dunia Bersama: lapisan data (SQLite).

Python stdlib SAJA (sqlite3, hashlib, secrets, json, time) — tanpa
dependensi luar, tanpa pip install. Dipakai server.py.

SKEMA:
  pemain   — akun game (nama unik, kata sandi di-hash + salt, koin,
             permata, reputasi, level, terakhir_masuk, tunggakan).
  token    — token sesi acak (satu token aktif per pemain; login
             baru mengganti token lama).
  petak    — satu baris per petak yang pernah dimiliki/dibangun
             (x, y, pemilik_id, bangunan_id, syarat, izin_id).
             Petak tanpa baris = tanah kota (belum dimiliki siapa pun).
  izin     — pengajuan izin wali kota (status: menunggu/dibayar/
             ditolak — ditolak tidak disimpan, hanya dikembalikan).
  kas_kota — satu baris (id=1): saldo kas kota.
  riwayat_kas — pemasukan kas kota (pembelian tanah + biaya izin).

RUMUS WALI KOTA (CERMIN SistemWaliKota.cs — bila mengubah rumus di
sini, ubah juga di sana, dan sebaliknya; server adalah otoritas
akhir):
  harga tanah: pinggir Rp200.000, +Rp100.000 tiap langkah Chebyshev
  mendekati pusat (11,11) -> pusat Rp1.400.000.
  biaya izin: 10% harga tanah; reputasi >=70 -> -30%, <30 -> +50%;
  kas < Rp1jt -> +50%, kas > Rp20jt -> -20%; ada Balai Kota -> -10%;
  paling murah Rp10.000; bersyarat -> x1,5.
  skor: 50 awal; reputasi >=70 +15, >=50 +5, <30 -10; hunian +10,
  fasilitas +15; tiap bangunan sejenis di atas 2 dalam area 5x5
  mengurangi 15; variasi deterministik -5..+5 dari id pemain + petak.
  ditolak otomatis: reputasi < 15, atau 4+ bangunan sejenis di
  kawasan. Disetujui bila skor >= 40, bersyarat bila 25-39.

WARGA NPC (tycoon): uang pemain datang dari WARGA, bukan dari angka
tetap per bangunan. Tiap distrik punya 40 warga NPC yang hidup di
server (tabel warga) sehingga semua pemain melihat orang yang sama:
  - warga menyewa KAMAR di hunian milik pemain (kost/apartemen/
    perumahan/rumah); sewa dibayar tiap hari game ke pemiliknya;
  - warga BELANJA tiap hari di satu warung/toko/kafe/pasar milik
    pemain; tamu dibatasi kapasitas bangunan, jadi membangun terlalu
    banyak toko membuat tamu terbagi (bersaing dengan pemain lain);
  - warga bergaji tiap hari; bila uangnya habis ia berhenti membayar
    sewa hari itu tetapi tetap warga yang sama.
Simulasi berjalan "malas" (saat ada request) dan memakai jam distrik
(tabel simulasi). 1 hari game = 300 detik, maks 24 hari per jeda.
Pajak kota 2,5% dari pemasukan kotor warga; perawatan bangunan tetap
dipotong tiap hari. Fasilitas umum (kategori 2: balai kota, klinik,
sekolah, taman, pos, dll.) dibangun dan diurus KOTA — pemain tidak
bisa membeli tanah/izin/membangunnya.

KEAMANAN DASAR: kata sandi PBKDF2-HMAC-SHA256 (100.000 iterasi,
salt 16 byte per akun — TIDAK PERNAH disimpan polos); token
secrets.token_hex(24); batas laju 60 request/menit per IP
(diterapkan di server.py, angka sama dengan GameBalance).
"""

import hashlib
import json
import os
import secrets
import sqlite3
import threading
import time

# ---------------------------------------------------------------- data bangunan
# CERMIN file .asset (Assets/Data/Buildings/): id -> (biaya, pemasukan,
# perawatan, kapasitas, kategori, level_buka, lebar, tinggi).
# Kategori: 0 = Hunian, 1 = Komersial, 2 = Fasilitas (sama seperti
# enum KategoriBangunan di BuildingData.cs).
BANGUNAN = {
    "rumah": (500000, 0, 1000, 2, 0, 1, 1, 1),
    "warung": (750000, 15000, 2000, 0, 1, 1, 1, 1),
    "taman": (250000, 0, 500, 0, 2, 1, 1, 1),
    "jalan": (50000, 0, 0, 0, 2, 1, 1, 1),
    "kost": (2000000, 40000, 5000, 8, 0, 2, 2, 2),
    "toko_kelontong": (1500000, 30000, 4000, 0, 1, 3, 1, 1),
    "perpustakaan": (800000, 0, 2000, 0, 2, 2, 1, 1),
    "sekolah": (3000000, 0, 8000, 0, 2, 3, 2, 2),
    "balaikota": (3000000, 0, 5000, 0, 2, 3, 2, 2),
    "pasar": (4000000, 60000, 8000, 0, 1, 3, 2, 2),
    "pos": (1000000, 0, 3000, 0, 2, 3, 1, 1),
    "apartemen": (5000000, 60000, 10000, 12, 0, 4, 2, 2),
    "kafe": (2000000, 25000, 5000, 0, 1, 4, 1, 1),
    "klinik": (4000000, 0, 10000, 0, 2, 4, 2, 2),
    "pembangkit": (6000000, 0, 12000, 0, 2, 4, 2, 2),
    "perumahan": (8000000, 100000, 15000, 20, 0, 5, 3, 3),
    "kampus": (10000000, 80000, 20000, 0, 2, 5, 3, 3),
    "alun_alun": (12000000, 0, 25000, 0, 2, 6, 3, 3),
}

# ---------------------------------------------------------------- data warga
# Sewa per kamar per hari (bangunan hunian; kapasitas = jumlah kamar).
SEWA_PER_KAMAR = {
    "rumah": 0,
    "kost": 5000,
    "apartemen": 5000,
    "perumahan": 5000,
}
# Bangunan komersial: id -> (tamu maks per hari, belanja per tamu).
# Angka dipilih supaya pemasukan penuh kira-kira sama dengan angka
# pemasukan lama di BANGUNAN (warung 16rb, toko 30rb, kafe 25rb,
# pasar 60rb), jadi keseimbangan ekonomi tidak melonjak.
BELANJA_WARGA = {
    "warung": (8, 2000),
    "toko_kelontong": (12, 2500),
    "kafe": (10, 2500),
    "pasar": (30, 2000),
}
GAJI_WARGA = 9000          # upah harian tiap warga
UANG_AWAL_WARGA = 20000
WARGA_PER_DISTRIK = 40
# Fasilitas umum yang hanya boleh dibangun Kota/NPC. Jalan TIDAK
# termasuk: pemain tetap boleh membeli/membangun jalan untuk
# menghubungkan usaha (syarat izin Bersyarat butuh jalan bersebelahan).
KOTA_SAJA = {"taman", "perpustakaan", "sekolah", "balaikota", "pos",
             "klinik", "pembangkit", "kampus", "alun_alun"}
NAMA_WARGA = [
    "Adi", "Bunga", "Citra", "Dewi", "Eko", "Fitri", "Gilang", "Hana",
    "Indra", "Joko", "Kartika", "Lestari", "Made", "Nina", "Oki", "Putri",
    "Raka", "Sari", "Tono", "Umi", "Vina", "Wawan", "Yani", "Zaki",
]

# Konstanta Fase 7 (sama dengan GameBalance.cs — ubah di kedua tempat).
UKURAN_PETA = 40
PUSAT_PETA = 19
HARGA_TANAH_PINGGIR = 200000
HARGA_TANAH_PER_LANGKAH = 60000
BIAYA_SEROBOT_TANAH = 500000
BIAYA_IZIN_PERMIL = 100
BIAYA_IZIN_MINIMUM = 10000
REPUTASI_DISKON = 70
DISKON_REPUTASI_PERMIL = 300
REPUTASI_MAHAL = 30
KENAIKAN_REPUTASI_PERMIL = 500
REPUTASI_TOLAK = 15
KAS_MENIPIS = 1000000
KENAIKAN_KAS_PERMIL = 500
KAS_PENUH = 20000000
DISKON_KAS_PERMIL = 200
DISKON_BALAIKOTA_PERMIL = 100
PENGALI_BERSYARAT_PERMIL = 1500
KEPADATAN_MAKS = 4
JARI_JARI_KAWASAN = 2
SKOR_AWAL = 50
SKOR_DISETUJUI = 40
SKOR_BERSYARAT = 25
VARIASI_SKOR = 5
DURASI_HARI_DETIK = 300
MAKS_HARI_OFFLINE = 24
PAJAK_PERMIL = 25
KOIN_AWAL = 5000000
REPUTASI_AWAL = 50

SKEMA = """
CREATE TABLE IF NOT EXISTS pemain (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nama TEXT UNIQUE NOT NULL,
    garam TEXT NOT NULL,
    hash_sandi TEXT NOT NULL,
    koin INTEGER NOT NULL DEFAULT 5000000,
    permata INTEGER NOT NULL DEFAULT 0,
    reputasi INTEGER NOT NULL DEFAULT 50,
    level INTEGER NOT NULL DEFAULT 1,
    tunggakan INTEGER NOT NULL DEFAULT 0,
    terakhir_masuk INTEGER NOT NULL DEFAULT 0,
    distrik_aktif INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS distrik (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nama TEXT UNIQUE NOT NULL,
    deskripsi TEXT NOT NULL,
    dibuat INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS token (
    token TEXT PRIMARY KEY,
    pemain_id INTEGER NOT NULL,
    dibuat INTEGER NOT NULL,
    FOREIGN KEY (pemain_id) REFERENCES pemain (id)
);
CREATE TABLE IF NOT EXISTS petak (
    distrik_id INTEGER NOT NULL DEFAULT 1,
    x INTEGER NOT NULL,
    y INTEGER NOT NULL,
    pemilik_id INTEGER,
    bangunan_id TEXT,
    jangkar_x INTEGER,
    jangkar_y INTEGER,
    level INTEGER NOT NULL DEFAULT 1,
    syarat TEXT NOT NULL DEFAULT '',
    izin_id INTEGER,
    PRIMARY KEY (distrik_id, x, y)
);
CREATE TABLE IF NOT EXISTS izin (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    distrik_id INTEGER NOT NULL DEFAULT 1,
    pemain_id INTEGER NOT NULL,
    x INTEGER NOT NULL,
    y INTEGER NOT NULL,
    bangunan_id TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'menunggu',
    harga_tanah INTEGER NOT NULL,
    biaya_izin INTEGER NOT NULL,
    syarat TEXT NOT NULL DEFAULT '',
    alasan TEXT NOT NULL DEFAULT '',
    dibuat INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS kas_kota (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    saldo INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS riwayat_kas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    jumlah INTEGER NOT NULL,
    keterangan TEXT NOT NULL,
    waktu INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS peristiwa_dunia (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    distrik_id INTEGER NOT NULL DEFAULT 1,
    jenis TEXT NOT NULL,
    judul TEXT NOT NULL,
    deskripsi TEXT NOT NULL,
    target_x INTEGER NOT NULL DEFAULT -1,
    target_y INTEGER NOT NULL DEFAULT -1,
    pelaku_id INTEGER,
    korban_id INTEGER,
    status TEXT NOT NULL DEFAULT 'aktif',
    ditangani_oleh TEXT NOT NULL DEFAULT '',
    dibuat INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS pesan_chat (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    distrik_id INTEGER NOT NULL DEFAULT 1,
    pemain_id INTEGER NOT NULL,
    nama TEXT NOT NULL,
    isi TEXT NOT NULL,
    dibuat INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS warga (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    distrik_id INTEGER NOT NULL DEFAULT 1,
    nama TEXT NOT NULL,
    uang INTEGER NOT NULL DEFAULT 0,
    hunian_x INTEGER NOT NULL DEFAULT -1,
    hunian_y INTEGER NOT NULL DEFAULT -1,
    pemilik_id INTEGER,
    toko_x INTEGER NOT NULL DEFAULT -1,
    toko_y INTEGER NOT NULL DEFAULT -1,
    dibuat INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS simulasi (
    distrik_id INTEGER PRIMARY KEY,
    hari_terakhir INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS pendapatan_warga (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    pemain_id INTEGER NOT NULL,
    distrik_id INTEGER NOT NULL,
    hari INTEGER NOT NULL,
    sewa INTEGER NOT NULL DEFAULT 0,
    penjualan INTEGER NOT NULL DEFAULT 0,
    perawatan INTEGER NOT NULL DEFAULT 0,
    dibuat INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_pendapatan_pemain
    ON pendapatan_warga (pemain_id, hari);
INSERT OR IGNORE INTO kas_kota (id, saldo) VALUES (1, 0);
"""


class DuniaDB:
    """Satu koneksi SQLite + kunci tulis (dipakai server multi-thread)."""

    def __init__(self, jalur_db):
        self.jalur = jalur_db
        self.kunci = threading.Lock()
        self.koneksi = sqlite3.connect(jalur_db, check_same_thread=False)
        self.koneksi.row_factory = sqlite3.Row
        with self.kunci:
            self.koneksi.executescript(SKEMA)
            try:
                self.koneksi.execute(
                    "ALTER TABLE petak ADD COLUMN level INTEGER NOT NULL DEFAULT 1")
            except sqlite3.OperationalError:
                pass
            try:
                self.koneksi.execute(
                    "ALTER TABLE pendapatan_warga ADD COLUMN dibuat INTEGER"
                    " NOT NULL DEFAULT 0")
            except sqlite3.OperationalError:
                pass
            self._inisialisasi_kota_awal()
            self._inisialisasi_warga_awal()
            self.koneksi.commit()

    def _inisialisasi_kota_awal(self):
        """Buat 4 distrik awal + fasilitas umum (Balai Kota, Klinik/RS,
        Pos Polisi, Taman, Jalan) di tiap distrik jika masih kosong."""
        now = int(time.time())
        distrik_awal = [
            ("Distrik Pusat (Nusantara)", "Pusat pemerintahan & niaga utama"),
            ("Distrik Timur (Harapan)", "Kawasan hunian & pendidikan"),
            ("Distrik Barat (Sejahtera)", "Kawasan pasar & perdagangan"),
            ("Distrik Selatan (Mandiri)", "Wilayah baru untuk pendatang"),
        ]
        for nama_d, desk_d in distrik_awal:
            self.koneksi.execute(
                "INSERT OR IGNORE INTO distrik (nama, deskripsi, dibuat)"
                " VALUES (?, ?, ?)", (nama_d, desk_d, now))

        kota = self.koneksi.execute(
            "SELECT id FROM pemain WHERE nama = 'Kota'").fetchone()
        if kota is None:
            garam = secrets.token_hex(16)
            cur = self.koneksi.execute(
                "INSERT INTO pemain (nama, garam, hash_sandi, koin,"
                " reputasi, level, terakhir_masuk) VALUES ('Kota', ?, ?, 0, 100, 6, ?)",
                (garam, self._hash_sandi(secrets.token_hex(16), garam), now))
            kota_id = cur.lastrowid
        else:
            kota_id = kota["id"]

        semua_distrik = self.koneksi.execute("SELECT id FROM distrik").fetchall()
        for d in semua_distrik:
            did = d["id"]
            jumlah_petak = self.koneksi.execute(
                "SELECT COUNT(*) AS c FROM petak WHERE distrik_id = ?", (did,)).fetchone()["c"]
            if jumlah_petak == 0:
                fasilitas = [
                    ("balaikota", 18, 18, 2, 2),
                    ("klinik", 15, 18, 2, 2),
                    ("pos", 21, 18, 1, 1),
                    ("taman", 21, 19, 1, 1),
                ]
                for bid, x, y, l, t in fasilitas:
                    for px in range(x, x + l):
                        for py in range(y, y + t):
                            self.koneksi.execute(
                                "INSERT OR IGNORE INTO petak (distrik_id, x, y, pemilik_id,"
                                " bangunan_id, jangkar_x, jangkar_y, syarat, izin_id)"
                                " VALUES (?, ?, ?, ?, ?, ?, ?, '', NULL)",
                                (did, px, py, kota_id, bid, x, y))
                jalan_petak = []
                for jx in range(14, 23):
                    jalan_petak.append((jx, 17))
                    jalan_petak.append((jx, 20))
                jalan_petak.extend([(17, 18), (17, 19), (20, 18), (20, 19)])
                for jx, jy in jalan_petak:
                    self.koneksi.execute(
                        "INSERT OR IGNORE INTO petak (distrik_id, x, y, pemilik_id,"
                        " bangunan_id, jangkar_x, jangkar_y, syarat, izin_id)"
                        " VALUES (?, ?, ?, ?, 'jalan', ?, ?, '', NULL)",
                        (did, jx, jy, kota_id, jx, jy))
            self._pastikan_peristiwa_beragam(did)

    def _inisialisasi_warga_awal(self):
        """Isi tiap distrik dengan warga NPC bila belum ada (sekali saja)."""
        now = int(time.time())
        for d in self.koneksi.execute("SELECT id FROM distrik").fetchall():
            did = d["id"]
            self.koneksi.execute(
                "INSERT OR IGNORE INTO simulasi (distrik_id, hari_terakhir)"
                " VALUES (?, ?)", (did, now // DURASI_HARI_DETIK))
            ada = self.koneksi.execute(
                "SELECT COUNT(*) AS c FROM warga WHERE distrik_id = ?",
                (did,)).fetchone()["c"]
            if ada:
                continue
            for i in range(WARGA_PER_DISTRIK):
                nama = NAMA_WARGA[i % len(NAMA_WARGA)] + " " + str(i // len(NAMA_WARGA) + 1)
                self.koneksi.execute(
                    "INSERT INTO warga (distrik_id, nama, uang, hunian_x,"
                    " hunian_y, pemilik_id, toko_x, toko_y, dibuat)"
                    " VALUES (?, ?, ?, -1, -1, NULL, -1, -1, ?)",
                    (did, nama, UANG_AWAL_WARGA, now))

    def _pastikan_peristiwa_beragam(self, distrik_id=1):
        aktif = self.koneksi.execute(
            "SELECT COUNT(*) AS c FROM peristiwa_dunia"
            " WHERE distrik_id = ? AND status = 'aktif'", (distrik_id,)).fetchone()["c"]
        if aktif >= 2:
            return
        katalog = [
            ("pencurian_fasilitas", "Laporan Pencurian Fasilitas",
             "Warga melaporkan aksi pencurian material di sekitar kawasan pusat."),
            ("sengketa_kebisingan", "Keluhan Kebisingan Kawasan",
             "Beberapa penghuni mengeluhkan aktivitas bising di dekat permukiman."),
            ("darurat_kesehatan", "Antrean Darurat Klinik/RS",
             "Klinik Kota membutuhkan pengamanan dan dukungan penanganan pasien."),
            ("usulan_subsidi", "Pengajuan Dana Bantuan Warga",
             "Perwakilan warga mengajukan bantuan fasilitas lewat Balai Kota."),
        ]
        now = int(time.time())
        for jenis, judul, desk in katalog:
            sudah = self.koneksi.execute(
                "SELECT id FROM peristiwa_dunia"
                " WHERE distrik_id = ? AND jenis = ? AND status = 'aktif'",
                (distrik_id, jenis)).fetchone()
            if sudah is None:
                self.koneksi.execute(
                    "INSERT INTO peristiwa_dunia (distrik_id, jenis, judul, deskripsi, status, dibuat)"
                    " VALUES (?, ?, ?, ?, 'aktif', ?)",
                    (distrik_id, jenis, judul, desk, now))
                aktif += 1
                if aktif >= 3:
                    break

    def tutup(self):
        self.koneksi.close()

    # ------------------------------------------------------------------ akun
    @staticmethod
    def _hash_sandi(sandi, garam_hex):
        garam = bytes.fromhex(garam_hex)
        hasil = hashlib.pbkdf2_hmac("sha256", sandi.encode("utf-8"),
                                    garam, 100000)
        return hasil.hex()

    def daftar(self, nama, sandi):
        nama = (nama or "").strip()
        if not nama or len(nama) > 20:
            return None, "Nama pemain 1-20 karakter."
        if not sandi or len(sandi) < 4:
            return None, "Kata sandi minimal 4 karakter."
        garam = secrets.token_hex(16)
        now = int(time.time())
        with self.kunci:
            try:
                cur = self.koneksi.execute(
                    "INSERT INTO pemain (nama, garam, hash_sandi, koin,"
                    " reputasi, level, terakhir_masuk) VALUES (?, ?, ?, ?, ?, 1, ?)",
                    (nama, garam, self._hash_sandi(sandi, garam),
                     KOIN_AWAL, REPUTASI_AWAL, now))
                pemain_id = cur.lastrowid
            except sqlite3.IntegrityError:
                return None, "Nama '" + nama + "' sudah dipakai."
            token = secrets.token_hex(24)
            self.koneksi.execute(
                "INSERT INTO token (token, pemain_id, dibuat) VALUES (?, ?, ?)",
                (token, pemain_id, now))
            self.koneksi.commit()
        return {"id": pemain_id, "nama": nama, "token": token,
                "koin": KOIN_AWAL, "reputasi": REPUTASI_AWAL, "level": 1}, None

    def masuk(self, nama, sandi):
        nama = (nama or "").strip()
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT * FROM pemain WHERE nama = ?", (nama,)).fetchone()
            if baris is None:
                return None, "Nama pemain tidak ditemukan."
            if self._hash_sandi(sandi or "", baris["garam"]) != baris["hash_sandi"]:
                return None, "Kata sandi salah."
            pemain_id = baris["id"]
            token = secrets.token_hex(24)
            now = int(time.time())
            self.koneksi.execute(
                "DELETE FROM token WHERE pemain_id = ?", (pemain_id,))
            self.koneksi.execute(
                "INSERT INTO token (token, pemain_id, dibuat) VALUES (?, ?, ?)",
                (token, pemain_id, now))
            # Pemasukan offline dihitung SEBELUM terakhir_masuk diperbarui.
            pemasukan = self._pemasukan_offline_terkunci(baris, now)
            self.koneksi.execute(
                "UPDATE pemain SET terakhir_masuk = ? WHERE id = ?",
                (now, pemain_id))
            self.koneksi.commit()
        profil = self.profil(pemain_id)
        profil["token"] = token
        profil["pemasukan_offline"] = pemasukan["bersih"]
        profil["hari_offline"] = pemasukan["hari"]
        return profil, None

    def pemain_dari_token(self, token):
        if not token:
            return None
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT p.* FROM pemain p JOIN token t ON t.pemain_id = p.id"
                " WHERE t.token = ?", (token,)).fetchone()
        return dict(baris) if baris else None

    def profil(self, pemain_id):
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT * FROM pemain WHERE id = ?", (pemain_id,)).fetchone()
            petak = self.koneksi.execute(
                "SELECT x, y, bangunan_id FROM petak WHERE pemilik_id = ?",
                (pemain_id,)).fetchall()
        return {
            "id": baris["id"],
            "nama": baris["nama"],
            "koin": baris["koin"],
            "permata": baris["permata"],
            "reputasi": baris["reputasi"],
            "level": baris["level"],
            "tunggakan": baris["tunggakan"],
            "tanah": [{"x": p["x"], "y": p["y"],
                       "bangunan": p["bangunan_id"]} for p in petak],
        }

    # ------------------------------------------------------- warga & pendapatan
    @staticmethod
    def _pengali_level(level):
        lv = max(1, min(3, int(level or 1)))
        return (100, 125, 150)[lv - 1]

    def _jalankan_simulasi_terkunci(self, distrik_id, sekarang):
        """Jalankan hari-hari game yang terlewat untuk satu distrik.

        WAJIB dipanggil di dalam self.kunci. Warga menyewa kamar dan
        berbelanja; sewa, penjualan, perawatan dan pajak langsung
        mengubah koin pemilik dan kas kota. Mengembalikan jumlah hari
        yang dijalankan.
        """
        row = self.koneksi.execute(
            "SELECT hari_terakhir FROM simulasi WHERE distrik_id = ?",
            (distrik_id,)).fetchone()
        hari_kini = sekarang // DURASI_HARI_DETIK
        if row is None:
            self.koneksi.execute(
                "INSERT INTO simulasi (distrik_id, hari_terakhir) VALUES (?, ?)",
                (distrik_id, hari_kini))
            return 0
        hari = min(hari_kini - row["hari_terakhir"], MAKS_HARI_OFFLINE)
        if hari <= 0:
            return 0

        hunian = {}
        toko = []
        perawatan = {}
        bangunan = self.koneksi.execute(
            "SELECT p.x, p.y, p.bangunan_id, p.level, p.pemilik_id"
            " FROM petak p WHERE p.distrik_id = ? AND p.bangunan_id IS NOT NULL"
            " AND p.x = p.jangkar_x AND p.y = p.jangkar_y",
            (distrik_id,)).fetchall()
        for b in bangunan:
            data = BANGUNAN.get(b["bangunan_id"])
            if data is None or not b["pemilik_id"]:
                continue
            pengali = self._pengali_level(b["level"])
            perawatan[b["pemilik_id"]] = perawatan.get(b["pemilik_id"], 0) + data[2]
            if data[4] == 0 and data[3] > 0:
                hunian[(b["x"], b["y"])] = {
                    "pemilik": b["pemilik_id"],
                    "kamar": max(1, data[3] * pengali // 100),
                    "terisi": 0,
                    "sewa": SEWA_PER_KAMAR.get(b["bangunan_id"], 0) * pengali // 100,
                }
            elif b["bangunan_id"] in BELANJA_WARGA:
                tamu, harga = BELANJA_WARGA[b["bangunan_id"]]
                toko.append({
                    "x": b["x"], "y": b["y"],
                    "pemilik": b["pemilik_id"],
                    "tamu": max(1, tamu * pengali // 100),
                    "harga": harga,
                    "terisi": 0,
                })
        toko.sort(key=lambda t: (t["x"], t["y"]))

        warga = self.koneksi.execute(
            "SELECT id, uang, hunian_x, hunian_y FROM warga"
            " WHERE distrik_id = ? ORDER BY id", (distrik_id,)).fetchall()
        if not warga:
            self.koneksi.execute(
                "UPDATE simulasi SET hari_terakhir = ? WHERE distrik_id = ?",
                (hari_kini, distrik_id))
            return hari

        # Tempatkan warga: yang sudah punya kamar valid tetap, sisanya
        # mengisi kamar kosong milik pemain.
        rumah_warga = {}
        for w in warga:
            kunci = (w["hunian_x"], w["hunian_y"])
            h = hunian.get(kunci)
            if h is not None and h["terisi"] < h["kamar"]:
                h["terisi"] += 1
                rumah_warga[w["id"]] = kunci
        for w in warga:
            if w["id"] in rumah_warga:
                continue
            for kunci in hunian:
                h = hunian[kunci]
                if h["terisi"] < h["kamar"]:
                    h["terisi"] += 1
                    rumah_warga[w["id"]] = kunci
                    break

        sewa_total = {}
        jual_total = {}
        uang_warga = {w["id"]: w["uang"] for w in warga}
        toko_warga = {}
        for hari_ke in range(hari_kini - hari + 1, hari_kini + 1):
            for t in toko:
                t["terisi"] = 0
            for w in warga:
                uid = w["id"]
                uang_warga[uid] += GAJI_WARGA
                kunci = rumah_warga.get(uid)
                if kunci is not None:
                    sewa = hunian[kunci]["sewa"]
                    if sewa > 0 and uang_warga[uid] >= sewa:
                        uang_warga[uid] -= sewa
                        pemilik = hunian[kunci]["pemilik"]
                        sewa_total[pemilik] = sewa_total.get(pemilik, 0) + sewa
                pilihan = [i for i, t in enumerate(toko)
                           if t["terisi"] < t["tamu"]]
                if not pilihan:
                    toko_warga[uid] = None
                    continue
                pilih = pilihan[(uid * 7 + hari_ke * 13) % len(pilihan)]
                t = toko[pilih]
                if uang_warga[uid] < t["harga"]:
                    toko_warga[uid] = None
                    continue
                uang_warga[uid] -= t["harga"]
                t["terisi"] += 1
                jual_total[t["pemilik"]] = jual_total.get(t["pemilik"], 0) + t["harga"]
                toko_warga[uid] = (t["x"], t["y"])

        pemilik_terlibat = set(sewa_total) | set(jual_total) | set(perawatan)
        for pid in pemilik_terlibat:
            sewa = sewa_total.get(pid, 0)
            jual = jual_total.get(pid, 0)
            rawat = perawatan.get(pid, 0) * hari
            pajak = (sewa + jual) * PAJAK_PERMIL // 1000
            bersih = sewa + jual - rawat - pajak
            baris = self.koneksi.execute(
                "SELECT koin, tunggakan FROM pemain WHERE id = ?",
                (pid,)).fetchone()
            if baris is None:
                continue
            koin = baris["koin"] + bersih
            tunggakan = baris["tunggakan"]
            if koin < 0:
                tunggakan += -koin
                koin = 0
            self.koneksi.execute(
                "UPDATE pemain SET koin = ?, tunggakan = ? WHERE id = ?",
                (koin, tunggakan, pid))
            self.koneksi.execute(
                "INSERT INTO pendapatan_warga (pemain_id, distrik_id, hari,"
                " sewa, penjualan, perawatan, dibuat)"
                " VALUES (?, ?, ?, ?, ?, ?, ?)",
                (pid, distrik_id, hari_kini, sewa, jual, rawat + pajak,
                 sekarang))
            self.koneksi.execute(
                "UPDATE kas_kota SET saldo = saldo + ? WHERE id = 1", (pajak,))

        for w in warga:
            uid = w["id"]
            kunci = rumah_warga.get(uid)
            tujuan = toko_warga.get(uid)
            hx, hy = kunci if kunci is not None else (-1, -1)
            tx, ty = tujuan if tujuan is not None else (-1, -1)
            self.koneksi.execute(
                "UPDATE warga SET uang = ?, hunian_x = ?, hunian_y = ?,"
                " pemilik_id = ?, toko_x = ?, toko_y = ? WHERE id = ?",
                (max(0, uang_warga[uid]), hx, hy,
                 hunian[kunci]["pemilik"] if kunci is not None else None,
                 tx, ty, uid))
        self.koneksi.execute(
            "UPDATE simulasi SET hari_terakhir = ? WHERE distrik_id = ?",
            (hari_kini, distrik_id))
        self.koneksi.commit()
        return hari

    def _pemasukan_offline_terkunci(self, baris_pemain, sekarang):
        """Dihitung di dalam kunci tulis; baris_pemain = Row pemain.

        Menjalankan simulasi distrik pemain lebih dulu (uangnya sudah
        masuk ke koin), lalu melaporkan pendapatan warga selama ia pergi.
        """
        terakhir = baris_pemain["terakhir_masuk"] or sekarang
        hari = (sekarang - terakhir) // DURASI_HARI_DETIK
        hari = max(0, min(hari, MAKS_HARI_OFFLINE))
        did = baris_pemain["distrik_aktif"] \
            if "distrik_aktif" in baris_pemain.keys() else 1
        self._jalankan_simulasi_terkunci(did, sekarang)
        if hari <= 0:
            return {"bersih": 0, "hari": 0}
        baris = self.koneksi.execute(
            "SELECT COALESCE(SUM(sewa), 0) AS sewa,"
            " COALESCE(SUM(penjualan), 0) AS jual,"
            " COALESCE(SUM(perawatan), 0) AS rawat FROM pendapatan_warga"
            " WHERE pemain_id = ? AND dibuat > ?",
            (baris_pemain["id"], terakhir)).fetchone()
        return {"bersih": baris["sewa"] + baris["jual"] - baris["rawat"],
                "hari": hari}

    # ------------------------------------------------------------------ dunia
    def dunia(self, distrik_id=1, pemain=None):
        with self.kunci:
            self._jalankan_simulasi_terkunci(distrik_id, int(time.time()))
            drow = self.koneksi.execute(
                "SELECT id, nama FROM distrik WHERE id = ?", (distrik_id,)).fetchone()
            nama_distrik = drow["nama"] if drow is not None else "Distrik Pusat"
            bangunan = self.koneksi.execute(
                "SELECT p.x, p.y, p.bangunan_id, p.level, pl.nama AS pemilik"
                " FROM petak p LEFT JOIN pemain pl ON pl.id = p.pemilik_id"
                " WHERE p.distrik_id = ? AND p.bangunan_id IS NOT NULL"
                " AND p.x = p.jangkar_x AND p.y = p.jangkar_y",
                (distrik_id,)).fetchall()
            tanah = self.koneksi.execute(
                "SELECT p.x, p.y, pl.nama AS pemilik FROM petak p"
                " LEFT JOIN pemain pl ON pl.id = p.pemilik_id"
                " WHERE p.distrik_id = ? AND p.pemilik_id IS NOT NULL",
                (distrik_id,)).fetchall()
            kas = self.koneksi.execute(
                "SELECT saldo FROM kas_kota WHERE id = 1").fetchone()
            jumlah = self.koneksi.execute(
                "SELECT COUNT(*) AS n FROM pemain").fetchone()
            warga = self.koneksi.execute(
                "SELECT COUNT(*) AS jumlah,"
                " SUM(CASE WHEN hunian_x >= 0 THEN 1 ELSE 0 END) AS penyewa"
                " FROM warga WHERE distrik_id = ?", (distrik_id,)).fetchone()
            baris_pemain = None
            if pemain is not None:
                baris_pemain = self.koneksi.execute(
                    "SELECT id, nama, koin, permata, reputasi, level, tunggakan"
                    " FROM pemain WHERE id = ?", (pemain["id"],)).fetchone()
        hasil = {
            "distrik_id": distrik_id,
            "nama_distrik": nama_distrik,
            "ukuran": UKURAN_PETA,
            "bangunan": [{"x": b["x"], "y": b["y"],
                          "id": b["bangunan_id"], "pemilik": b["pemilik"],
                          "level": b["level"] if "level" in b.keys() else 1}
                         for b in bangunan],
            "tanah": [{"x": t["x"], "y": t["y"], "pemilik": t["pemilik"]}
                      for t in tanah],
            "kas_kota": kas["saldo"],
            "jumlah_pemain": jumlah["n"],
            "warga": {"jumlah": warga["jumlah"] or 0,
                      "penyewa": warga["penyewa"] or 0},
        }
        if baris_pemain is not None:
            hasil["profil"] = {
                "id": baris_pemain["id"],
                "nama": baris_pemain["nama"],
                "koin": baris_pemain["koin"],
                "permata": baris_pemain["permata"],
                "reputasi": baris_pemain["reputasi"],
                "level": baris_pemain["level"],
                "tunggakan": baris_pemain["tunggakan"],
            }
        return hasil

    def daftar_warga(self, distrik_id=1):
        """Daftar warga NPC satu distrik untuk ditampilkan klien."""
        with self.kunci:
            self._jalankan_simulasi_terkunci(distrik_id, int(time.time()))
            rows = self.koneksi.execute(
                "SELECT w.id, w.nama, w.uang, w.hunian_x, w.hunian_y,"
                " w.pemilik_id, w.toko_x, w.toko_y, p.nama AS pemilik"
                " FROM warga w LEFT JOIN pemain p ON p.id = w.pemilik_id"
                " WHERE w.distrik_id = ? ORDER BY w.id", (distrik_id,)).fetchall()
        return {"warga": [dict(r) for r in rows]}

    def daftar_distrik(self, pemain):
        with self.kunci:
            rows = self.koneksi.execute(
                "SELECT d.id, d.nama, d.deskripsi,"
                " (SELECT COUNT(DISTINCT p.pemilik_id) FROM petak p"
                "  WHERE p.distrik_id = d.id AND p.pemilik_id IS NOT NULL) AS jumlah_pemain,"
                " (SELECT COUNT(*) FROM petak p WHERE p.distrik_id = d.id"
                "  AND p.bangunan_id IS NOT NULL AND p.x = p.jangkar_x AND p.y = p.jangkar_y) AS jumlah_bangunan"
                " FROM distrik d ORDER BY d.id ASC").fetchall()
        return {
            "distrik_aktif": pemain["distrik_aktif"],
            "distrik": [dict(r) for r in rows],
        }

    def pindah_distrik(self, pemain, distrik_id):
        with self.kunci:
            d = self.koneksi.execute(
                "SELECT id, nama FROM distrik WHERE id = ?", (distrik_id,)).fetchone()
            if d is None:
                return None, "Distrik tidak ditemukan."
            self.koneksi.execute(
                "UPDATE pemain SET distrik_aktif = ? WHERE id = ?",
                (distrik_id, pemain["id"]))
            self.koneksi.commit()
        return {"distrik_id": d["id"], "nama": d["nama"]}, None

    # Sinkron sesi (koin/reputasi dari klien, terbatas).
    def sinkron(self, pemain, koin, reputasi):
        """Terima koin + reputasi sesi dari klien.
        Dipakai klien tiap sinkronisasi (10 detik) supaya koin server
        mengikuti ekonomi lokal yang berjalan di klien (hari, kejadian,
        misi — semua dihitung lokal). Server TIDAK percaya begitu saja:
        koin >= 0, reputasi 0-100, dan perubahan per sinkron dibatasi
        Rp6.000.000 (penjualan termahal kampus Rp10jt -> jual 50% =
        Rp5jt, ditambah hadiah naik level Rp2jt — di atas itu pasti
        bukan aksi permainan normal). Pembelian bangunan TETAP
        divalidasi penuh di bangun() dari koin server, dan klien
        menahan sinkron selama aksi bangun/jual/bayar berjalan supaya
        tidak menimpa potongan server. Mengembalikan
        (profil_baru, kesalahan)."""
        if koin is None or reputasi is None:
            return None, "koin dan reputasi wajib diisi"
        try:
            koin = int(koin)
            reputasi = int(reputasi)
        except (TypeError, ValueError):
            return None, "koin dan reputasi harus angka"
        if koin < 0:
            return None, "Koin tidak boleh negatif"
        if not 0 <= reputasi <= 100:
            return None, "Reputasi harus 0-100"
        if abs(koin - pemain["koin"]) > 6000000:
            return None, "Perubahan koin melebihi batas sinkron (Rp6.000.000)"
        sekarang = int(time.time())
        with self.kunci:
            baris_koin = self.koneksi.execute(
                "SELECT koin FROM pemain WHERE id = ?",
                (pemain["id"],)).fetchone()
            koin_server = baris_koin["koin"] if baris_koin else pemain["koin"]
            # Koin pemain tidak pernah turun karena sinkron: semua
            # pengeluaran divalidasi server (bangun, izin, jual, upgrade)
            # dan pendapatan warga sudah ditambahkan server. Klien yang
            # belum melihat pendapatan itu tidak boleh menghapusnya.
            koin = max(koin, koin_server)
            self.koneksi.execute(
                "UPDATE pemain SET koin = ?, reputasi = ?,"
                " terakhir_masuk = ? WHERE id = ?",
                (koin, reputasi, sekarang, pemain["id"]))
            self.koneksi.commit()
            baris = self.koneksi.execute(
                "SELECT * FROM pemain WHERE id = ?",
                (pemain["id"],)).fetchone()
        return dict(baris), None

    # ---------- Impor tanah single player (sekali, saat masuk pertama) ----------
    def impor_tanah(self, pemain, daftar):
        """Akui kota single player sebagai tanah milik di server.
        Dipakai klien saat masuk online pertama kali: bangunan lokal
        (jangkar + id) didaftarkan ke server TANPA bayar (tanahnya
        sudah "dibeli" dengan permainan single player — di sana tanah
        gratis). Aturan: hanya bila pemain BELUM punya tanah apa pun
        di server (impor hanya sekali); maks 256 bangunan; tiap
        bangunan yang tapaknya sudah dimiliki pemain lain DILEWATI
        (tidak menimpa). Interior perabot TIDAK ikut (hanya di save
        lokal). Mengembalikan ({"diimpor": n, "dilewati": m}, kesalahan).
        """
        if not isinstance(daftar, list):
            return None, "Daftar bangunan harus list"
        if len(daftar) > 256:
            return None, "Maks 256 bangunan per impor"
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            punya = self.koneksi.execute(
                "SELECT COUNT(*) AS n FROM petak WHERE distrik_id = ? AND pemilik_id = ?",
                (did, pemain["id"])).fetchone()["n"]
            if punya > 0:
                return None, "Sudah punya tanah di server — impor hanya sekali"
            diimpor = 0
            dilewati = 0
            for item in daftar:
                try:
                    x = int(item["x"])
                    y = int(item["y"])
                    bangunan_id = item["id"]
                except (KeyError, TypeError, ValueError):
                    dilewati += 1
                    continue
                if bangunan_id not in BANGUNAN:
                    dilewati += 1
                    continue
                if bangunan_id in KOTA_SAJA:
                    dilewati += 1
                    continue
                _b, _m, _r, _k, _kat, _lvl, lebar, tinggi = BANGUNAN[bangunan_id]
                if not (0 <= x and 0 <= y and x + lebar <= UKURAN_PETA
                        and y + tinggi <= UKURAN_PETA):
                    dilewati += 1
                    continue
                bentrok = False
                for px in range(x, x + lebar):
                    for py in range(y, y + tinggi):
                        baris = self.koneksi.execute(
                            "SELECT pemilik_id, bangunan_id FROM petak"
                            " WHERE distrik_id = ? AND x = ? AND y = ?", (did, px, py)).fetchone()
                        if baris is not None and (
                                baris["pemilik_id"] is not None
                                or baris["bangunan_id"] is not None):
                            bentrok = True
                if bentrok:
                    dilewati += 1
                    continue
                for px in range(x, x + lebar):
                    for py in range(y, y + tinggi):
                        self.koneksi.execute(
                            "INSERT INTO petak (distrik_id, x, y, pemilik_id, bangunan_id,"
                            " jangkar_x, jangkar_y, syarat, izin_id)"
                            " VALUES (?, ?, ?, ?, ?, ?, ?, '', NULL)"
                            " ON CONFLICT (distrik_id, x, y) DO UPDATE SET pemilik_id = ?,"
                            " bangunan_id = ?, jangkar_x = ?, jangkar_y = ?",
                            (did, px, py, pemain["id"], bangunan_id, x, y,
                             pemain["id"], bangunan_id, x, y))
                diimpor += 1
            self.koneksi.commit()
        return {"diimpor": diimpor, "dilewati": dilewati}, None

    def kas_kota(self):
        with self.kunci:
            saldo = self.koneksi.execute(
                "SELECT saldo FROM kas_kota WHERE id = 1").fetchone()["saldo"]
            riwayat = self.koneksi.execute(
                "SELECT jumlah, keterangan, waktu FROM riwayat_kas"
                " ORDER BY id DESC LIMIT 10").fetchall()
        return {"saldo": saldo,
                "riwayat": [{"jumlah": r["jumlah"],
                             "keterangan": r["keterangan"],
                             "waktu": r["waktu"]} for r in riwayat]}

    # -------------------------------------------------------------- wali kota
    @staticmethod
    def _hash_stabil(teks):
        h = 0
        for ch in (teks or ""):
            h = h * 31 + ord(ch)
        return abs(h)

    @staticmethod
    def harga_tanah(x, y):
        jarak = max(abs(x - PUSAT_PETA), abs(y - PUSAT_PETA))
        langkah = 12 - jarak
        if langkah < 0:
            langkah = 0
        return HARGA_TANAH_PINGGIR + langkah * HARGA_TANAH_PER_LANGKAH

    @staticmethod
    def biaya_izin(harga_tanah, reputasi, kas_kota, ada_balaikota):
        biaya = harga_tanah * BIAYA_IZIN_PERMIL // 1000
        if reputasi >= REPUTASI_DISKON:
            biaya = biaya * (1000 - DISKON_REPUTASI_PERMIL) // 1000
        elif reputasi < REPUTASI_MAHAL:
            biaya = biaya * (1000 + KENAIKAN_REPUTASI_PERMIL) // 1000
        if kas_kota < KAS_MENIPIS:
            biaya = biaya * (1000 + KENAIKAN_KAS_PERMIL) // 1000
        elif kas_kota > KAS_PENUH:
            biaya = biaya * (1000 - DISKON_KAS_PERMIL) // 1000
        if ada_balaikota:
            biaya = biaya * (1000 - DISKON_BALAIKOTA_PERMIL) // 1000
        return max(biaya, BIAYA_IZIN_MINIMUM)

    def _ada_balaikota(self, distrik_id=1):
        baris = self.koneksi.execute(
            "SELECT COUNT(*) AS n FROM petak"
            " WHERE distrik_id = ? AND bangunan_id = 'balaikota'",
            (distrik_id,)).fetchone()
        return baris["n"] > 0

    def _jumlah_sejenis_kawasan(self, x, y, kategori, distrik_id=1):
        x0, x1 = x - JARI_JARI_KAWASAN, x + JARI_JARI_KAWASAN
        y0, y1 = y - JARI_JARI_KAWASAN, y + JARI_JARI_KAWASAN
        # Hitung bangunan BERBEDA (per jangkar), bukan per petak —
        # bangunan 2x2/3x3 menempati beberapa petak tapi satu bangunan.
        petak = self.koneksi.execute(
            "SELECT DISTINCT jangkar_x, jangkar_y, bangunan_id FROM petak"
            " WHERE distrik_id = ? AND x BETWEEN ? AND ? AND y BETWEEN ? AND ?"
            " AND bangunan_id IS NOT NULL",
            (distrik_id, x0, x1, y0, y1)).fetchall()
        jumlah = 0
        for p in petak:
            data = BANGUNAN.get(p["bangunan_id"])
            if data is not None and data[4] == kategori:
                jumlah += 1
        return jumlah

    def putuskan_izin(self, pemain, x, y, bangunan_id):
        """Keputusan wali kota — CERMIN SistemWaliKota.Putuskan."""
        data = BANGUNAN[bangunan_id]
        kategori = data[4]
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        if bangunan_id in KOTA_SAJA:
            harga = self.harga_tanah(x, y)
            return {"hasil": "Ditolak", "harga_tanah": harga,
                    "biaya_izin": 0, "total": 0, "syarat": "",
                    "alasan": "Fasilitas umum (" + bangunan_id.replace("_", " ")
                    + ") dibangun dan diurus Kota oleh warga NPC. Pemain "
                    "mengelola usaha dan hunian, bukan fasilitas umum."}
        kas = self.koneksi.execute(
            "SELECT saldo FROM kas_kota WHERE id = 1").fetchone()["saldo"]
        ada_balaikota = self._ada_balaikota(did)
        harga = self.harga_tanah(x, y)
        biaya = self.biaya_izin(harga, pemain["reputasi"], kas, ada_balaikota)
        jumlah = self._jumlah_sejenis_kawasan(x, y, kategori, did)
        if pemain["reputasi"] < REPUTASI_TOLAK:
            return {"hasil": "Ditolak", "harga_tanah": harga,
                    "biaya_izin": biaya, "total": 0, "syarat": "",
                    "alasan": "Wali kota menolak: reputasimu terlalu rendah ("
                    + str(pemain["reputasi"]) + "). Bangun reputasi dulu — "
                    "layani penghunimu dengan baik, lalu ajukan lagi."}
        if jumlah >= KEPADATAN_MAKS:
            return {"hasil": "Ditolak", "harga_tanah": harga,
                    "biaya_izin": biaya, "total": 0, "syarat": "",
                    "alasan": "Wali kota menolak: kawasan ini sudah padat "
                    "bangunan sejenis (" + str(jumlah) + " dalam area "
                    "sekitar). Coba petak di kawasan lain."}
        skor = SKOR_AWAL
        if pemain["reputasi"] >= REPUTASI_DISKON:
            skor += 15
        elif pemain["reputasi"] >= 50:
            skor += 5
        elif pemain["reputasi"] < REPUTASI_MAHAL:
            skor -= 10
        if kategori == 0:
            skor += 10
        elif kategori == 2:
            skor += 15
        lebih = jumlah - 2
        if lebih > 0:
            skor -= lebih * 15
        variasi = (self._hash_stabil(pemain["nama"]) + x * 131 + y * 17) \
            % (VARIASI_SKOR * 2 + 1) - VARIASI_SKOR
        skor += variasi
        if skor >= SKOR_DISETUJUI:
            return {"hasil": "Disetujui", "harga_tanah": harga,
                    "biaya_izin": biaya, "total": harga + biaya,
                    "syarat": "",
                    "alasan": "Wali kota menyetujui. Lokasi bagus, "
                    "reputasimu baik — bayar harga tanah + biaya izin, "
                    "tanah ini jadi milikmu."}
        if skor >= SKOR_BERSYARAT:
            biaya = biaya * PENGALI_BERSYARAT_PERMIL // 1000
            if bangunan_id == "jalan":
                return {"hasil": "Bersyarat", "harga_tanah": harga,
                        "biaya_izin": biaya, "total": harga + biaya,
                        "syarat": "biaya_lebih_mahal",
                        "alasan": "Wali kota menyetujui dengan syarat: "
                        "biaya izin naik 1,5x karena lokasimu kurang "
                        "strategis. Bayar harga di atas bila setuju."}
            return {"hasil": "Bersyarat", "harga_tanah": harga,
                    "biaya_izin": biaya, "total": harga + biaya,
                    "syarat": "jalan_bersebelahan",
                    "alasan": "Wali kota menyetujui dengan syarat: "
                    "biaya izin naik 1,5x DAN wajib ada Jalan di petak "
                    "bersebelahan sebelum bangunan dipasang. Bayar "
                    "harga di atas bila setuju."}
        return {"hasil": "Ditolak", "harga_tanah": harga,
                "biaya_izin": biaya, "total": 0, "syarat": "",
                "alasan": "Wali kota menolak: lokasimu kurang cocok untuk "
                "bangunan ini (skor " + str(skor) + "). Coba petak lain, "
                "naikkan reputasi, atau pilih jenis bangunan lain."}

    # ------------------------------------------------------------ alur izin
    def ajukan_izin(self, pemain, x, y, bangunan_id):
        if bangunan_id not in BANGUNAN:
            return None, "Bangunan tidak dikenal: " + str(bangunan_id)
        if bangunan_id in KOTA_SAJA:
            return None, "Fasilitas umum dibangun dan diurus Kota oleh warga NPC."
        if not (0 <= x < UKURAN_PETA and 0 <= y < UKURAN_PETA):
            return None, ("Petak di luar peta bersama (0-%d)."
                          % (UKURAN_PETA - 1))
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            lama = self.koneksi.execute(
                "SELECT pemilik_id FROM petak WHERE distrik_id = ? AND x = ? AND y = ?",
                (did, x, y)).fetchone()
            if lama is not None and lama["pemilik_id"] is not None:
                if lama["pemilik_id"] == pemain["id"]:
                    return None, "Petak ini sudah milikmu."
                return None, "Petak ini sudah dimiliki pemain lain."
            keputusan = self.putuskan_izin(pemain, x, y, bangunan_id)
            if keputusan["hasil"] == "Ditolak":
                self.koneksi.commit()
                return {"keputusan": keputusan, "izin_id": None}, None
            now = int(time.time())
            cur = self.koneksi.execute(
                "INSERT INTO izin (distrik_id, pemain_id, x, y, bangunan_id, status,"
                " harga_tanah, biaya_izin, syarat, alasan, dibuat)"
                " VALUES (?, ?, ?, ?, ?, 'menunggu', ?, ?, ?, ?, ?)",
                (did, pemain["id"], x, y, bangunan_id,
                 keputusan["harga_tanah"], keputusan["biaya_izin"],
                 keputusan["syarat"], keputusan["alasan"], now))
            self.koneksi.commit()
            return {"keputusan": keputusan, "izin_id": cur.lastrowid}, None

    def bayar_izin(self, pemain, izin_id):
        with self.kunci:
            izin = self.koneksi.execute(
                "SELECT * FROM izin WHERE id = ?", (izin_id,)).fetchone()
            if izin is None:
                return None, "Izin tidak ditemukan."
            if izin["pemain_id"] != pemain["id"]:
                return None, "Izin ini milik pemain lain."
            if izin["status"] != "menunggu":
                return None, "Izin ini sudah dibayar / kedaluwarsa."
            total = izin["harga_tanah"] + izin["biaya_izin"]
            if pemain["koin"] < total:
                return None, "Koin tidak cukup (butuh Rp" + str(total) + ")."
            now = int(time.time())
            izin_did = izin["distrik_id"] if "distrik_id" in izin.keys() else 1
            self.koneksi.execute(
                "UPDATE pemain SET koin = koin - ? WHERE id = ?",
                (total, pemain["id"]))
            self.koneksi.execute(
                "INSERT INTO petak (distrik_id, x, y, pemilik_id, bangunan_id, syarat,"
                " izin_id) VALUES (?, ?, ?, ?, NULL, ?, ?)"
                " ON CONFLICT (distrik_id, x, y) DO UPDATE SET pemilik_id = excluded.pemilik_id,"
                " syarat = excluded.syarat, izin_id = excluded.izin_id",
                (izin_did, izin["x"], izin["y"], pemain["id"],
                 izin["syarat"], izin["id"]))
            self.koneksi.execute(
                "UPDATE izin SET status = 'dibayar' WHERE id = ?",
                (izin["id"],))
            self.koneksi.execute(
                "UPDATE kas_kota SET saldo = saldo + ? WHERE id = 1",
                (total,))
            self.koneksi.execute(
                "INSERT INTO riwayat_kas (jumlah, keterangan, waktu)"
                " VALUES (?, ?, ?)",
                (total, "Pembelian tanah (" + str(izin["x"]) + ", "
                 + str(izin["y"]) + ") oleh " + pemain["nama"], now))
            self.koneksi.commit()
        return {"x": izin["x"], "y": izin["y"],
                "koin": pemain["koin"] - total,
                "total": total}, None

    # ------------------------------------------------------------ bangun/jual
    def _ada_jalan_bersebelahan(self, x, y, lebar, tinggi, distrik_id=1):
        for px in range(x, x + lebar):
            for py in range(y, y + tinggi):
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    baris = self.koneksi.execute(
                        "SELECT bangunan_id FROM petak"
                        " WHERE distrik_id = ? AND x = ? AND y = ?",
                        (distrik_id, px + dx, py + dy)).fetchone()
                    if baris is not None and baris["bangunan_id"] == "jalan":
                        return True
        return False

    def bangun(self, pemain, x, y, bangunan_id):
        if bangunan_id not in BANGUNAN:
            return None, "Bangunan tidak dikenal: " + str(bangunan_id)
        biaya, _masuk, _rawat, _kap, _kat, _lvl, lebar, tinggi = \
            BANGUNAN[bangunan_id]
        if bangunan_id in KOTA_SAJA:
            return None, "Fasilitas umum dibangun dan diurus Kota oleh warga NPC."
        if not (0 <= x and 0 <= y and x + lebar <= UKURAN_PETA
                and y + tinggi <= UKURAN_PETA):
            return None, "Tapak bangunan keluar dari peta bersama."
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            for px in range(x, x + lebar):
                for py in range(y, y + tinggi):
                    baris = self.koneksi.execute(
                        "SELECT pemilik_id, bangunan_id FROM petak"
                        " WHERE distrik_id = ? AND x = ? AND y = ?",
                        (did, px, py)).fetchone()
                    if baris is None or baris["pemilik_id"] != pemain["id"]:
                        return None, "Petak (" + str(px) + ", " + str(py) \
                            + ") bukan tanah milikmu — beli dulu lewat Wali Kota."
                    if baris["bangunan_id"] is not None:
                        return None, "Petak (" + str(px) + ", " + str(py) \
                            + ") sudah terisi bangunan."
            # Syarat izin (jalan bersebelahan) dicek dari petak pojok.
            pojok = self.koneksi.execute(
                "SELECT syarat FROM petak WHERE distrik_id = ? AND x = ? AND y = ?",
                (did, x, y)).fetchone()
            if pojok is not None and pojok["syarat"] == "jalan_bersebelahan":
                if not self._ada_jalan_bersebelahan(x, y, lebar, tinggi, did):
                    return None, "Izinmu bersyarat: wajib ada Jalan di petak" \
                        " bersebelahan sebelum bangunan dipasang."
            if pemain["koin"] < biaya:
                return None, "Koin tidak cukup (butuh Rp" + str(biaya) + ")."
            now = int(time.time())
            self.koneksi.execute(
                "UPDATE pemain SET koin = koin - ? WHERE id = ?",
                (biaya, pemain["id"]))
            for px in range(x, x + lebar):
                for py in range(y, y + tinggi):
                    self.koneksi.execute(
                        "INSERT INTO petak (distrik_id, x, y, pemilik_id, bangunan_id,"
                        " jangkar_x, jangkar_y, syarat, izin_id)"
                        " VALUES (?, ?, ?, ?, ?, ?, ?, '', NULL)"
                        " ON CONFLICT (distrik_id, x, y) DO UPDATE SET bangunan_id = ?,"
                        " jangkar_x = ?, jangkar_y = ?, level = 1",
                        (did, px, py, pemain["id"], bangunan_id, x, y,
                         bangunan_id, x, y))
            self.koneksi.commit()
        return {"x": x, "y": y, "bangunan": bangunan_id,
                "koin": pemain["koin"] - biaya}, None

    def jual(self, pemain, x, y):
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT pemilik_id, bangunan_id, jangkar_x, jangkar_y"
                " FROM petak WHERE distrik_id = ? AND x = ? AND y = ?",
                (did, x, y)).fetchone()
            if baris is None or baris["pemilik_id"] != pemain["id"]:
                return None, "Petak ini bukan tanah milikmu."
            if baris["bangunan_id"] is None:
                return None, "Tidak ada bangunan di petak ini."
            data = BANGUNAN.get(baris["bangunan_id"])
            if data is None:
                return None, "Bangunan tidak dikenal."
            harga_jual = data[0] // 2
            pojok_x = baris["jangkar_x"]
            pojok_y = baris["jangkar_y"]
            if pojok_x is None or pojok_y is None:
                pojok_x, pojok_y = x, y
            lebar, tinggi = data[6], data[7]
            for px in range(pojok_x, pojok_x + lebar):
                for py in range(pojok_y, pojok_y + tinggi):
                    self.koneksi.execute(
                        "UPDATE petak SET bangunan_id = NULL,"
                        " jangkar_x = NULL, jangkar_y = NULL"
                        " WHERE distrik_id = ? AND x = ? AND y = ?",
                        (did, px, py))
            self.koneksi.execute(
                "UPDATE pemain SET koin = koin + ? WHERE id = ?",
                (harga_jual, pemain["id"]))
            self.koneksi.commit()
        return {"x": pojok_x, "y": pojok_y, "bangunan": baris["bangunan_id"],
                "harga_jual": harga_jual,
                "koin": pemain["koin"] + harga_jual}, None

    # ------------------------------------------------- sengketa & peristiwa
    def serobot_tanah(self, pemain, x, y):
        """Ambil paksa petak milik pemain lain (biaya sengketa + turun reputasi,
        memunculkan peristiwa pengaduan ke Polisi / Wali Kota)."""
        if not (0 <= x < UKURAN_PETA and 0 <= y < UKURAN_PETA):
            return None, "Petak di luar peta."
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT p.pemilik_id, u.nama AS pemilik_nama, p.bangunan_id,"
                " p.jangkar_x, p.jangkar_y"
                " FROM petak p LEFT JOIN pemain u ON p.pemilik_id = u.id"
                " WHERE p.distrik_id = ? AND p.x = ? AND p.y = ?", (did, x, y)).fetchone()
            if baris is None or baris["pemilik_id"] is None:
                return None, "Tanah ini milik kota — ajukan izin resmi ke Wali Kota."
            if baris["pemilik_id"] == pemain["id"]:
                return None, "Petak ini sudah menjadi milikmu."
            if baris["pemilik_nama"] == "Kota":
                return None, "Fasilitas umum milik Kota tidak dapat diambil paksa!"
            if pemain["koin"] < BIAYA_SEROBOT_TANAH:
                return None, "Koin tidak cukup untuk klaim paksa (butuh Rp" \
                    + str(BIAYA_SEROBOT_TANAH) + ")."

            korban_id = baris["pemilik_id"]
            korban_nama = baris["pemilik_nama"] or "Warga"
            rep_baru = max(0, pemain["reputasi"] - 15)
            self.koneksi.execute(
                "UPDATE pemain SET koin = koin - ?, reputasi = ? WHERE id = ?",
                (BIAYA_SEROBOT_TANAH, rep_baru, pemain["id"]))
            self.koneksi.execute(
                "UPDATE petak SET pemilik_id = ? WHERE distrik_id = ? AND x = ? AND y = ?",
                (pemain["id"], did, x, y))
            now = int(time.time())
            judul = "Penyerobotan Tanah (" + str(x) + "," + str(y) + ")"
            desk = pemain["nama"] + " mengambil paksa tanah milik " + korban_nama \
                + " di petak (" + str(x) + "," + str(y) + ")! Adukan ke Polisi atau Wali Kota."
            self.koneksi.execute(
                "INSERT INTO peristiwa_dunia (distrik_id, jenis, judul, deskripsi, target_x, target_y,"
                " pelaku_id, korban_id, status, dibuat)"
                " VALUES (?, 'serobot_tanah', ?, ?, ?, ?, ?, ?, 'aktif', ?)",
                (did, judul, desk, x, y, pemain["id"], korban_id, now))
            self.koneksi.commit()
        return {"x": x, "y": y, "koin": pemain["koin"] - BIAYA_SEROBOT_TANAH,
                "reputasi": rep_baru}, None

    def daftar_peristiwa(self, distrik_id=1):
        with self.kunci:
            self._pastikan_peristiwa_beragam(distrik_id)
            self.koneksi.commit()
            baris = self.koneksi.execute(
                "SELECT id, jenis, judul, deskripsi, target_x, target_y, status, dibuat"
                " FROM peristiwa_dunia WHERE distrik_id = ? AND status = 'aktif'"
                " ORDER BY id DESC LIMIT 10", (distrik_id,)).fetchall()
        return {"peristiwa": [dict(r) for r in baris]}

    def tindak_peristiwa(self, pemain, peristiwa_id, tujuan):
        if tujuan not in ("polisi", "walikota"):
            return None, "Tujuan aduan harus 'polisi' atau 'walikota'."
        with self.kunci:
            ev = self.koneksi.execute(
                "SELECT * FROM peristiwa_dunia WHERE id = ?",
                (peristiwa_id,)).fetchone()
            if ev is None or ev["status"] != "aktif":
                return None, "Peristiwa sudah ditangani atau tidak ditemukan."

            delta_koin = 0
            delta_rep = 0
            pesan = ""
            jenis = ev["jenis"]

            if jenis == "serobot_tanah":
                # Kembalikan tanah ke korban.
                ev_did = ev["distrik_id"] if "distrik_id" in ev.keys() else 1
                if ev["korban_id"] is not None and ev["target_x"] >= 0:
                    self.koneksi.execute(
                        "UPDATE petak SET pemilik_id = ? WHERE distrik_id = ? AND x = ? AND y = ?",
                        (ev["korban_id"], ev_did, ev["target_x"], ev["target_y"]))
                if tujuan == "polisi":
                    denda = 300000
                    delta_rep = 5
                    delta_koin = 100000
                    pesan = "Polisi menindak tegas! Tanah dikembalikan & pelaku didenda."
                else:
                    denda = 200000
                    delta_rep = 8
                    delta_koin = 50000
                    self._catat_kas(denda, "Denda sengketa tanah ("
                                    + str(ev["target_x"]) + "," + str(ev["target_y"]) + ")")
                    pesan = "Wali Kota memediasi sengketa! Tanah dikembalikan & denda masuk kas kota."
                if ev["pelaku_id"] is not None:
                    self.koneksi.execute(
                        "UPDATE pemain SET koin = MAX(0, koin - ?) WHERE id = ?",
                        (denda, ev["pelaku_id"]))
            elif jenis == "pencurian_fasilitas":
                if tujuan == "polisi":
                    delta_koin = 150000
                    delta_rep = 5
                    pesan = "Pos Polisi menangkap pelaku! Kamu mendapat imbalan keamanan."
                else:
                    delta_rep = 3
                    pesan = "Wali Kota mencatat laporan dan memerintahkan perbaikan fasilitas."
            elif jenis == "sengketa_kebisingan":
                if tujuan == "polisi":
                    delta_rep = 4
                    pesan = "Polisi menertibkan kawasan agar warga kembali nyaman."
                else:
                    delta_koin = 100000
                    delta_rep = 6
                    pesan = "Wali Kota memediasi warga dan memberi kompensasi ketertiban."
            elif jenis == "darurat_kesehatan":
                if tujuan == "polisi":
                    delta_koin = 100000
                    delta_rep = 5
                    pesan = "Polisi membantu pengamanan antrean Klinik/RS Kota!"
                else:
                    delta_rep = 7
                    pesan = "Wali Kota menyalurkan dana darurat untuk Klinik/RS Kota."
            else:
                if tujuan == "walikota":
                    delta_koin = 250000
                    delta_rep = 4
                    pesan = "Wali Kota menyetujui bantuan subsidi warga!"
                else:
                    delta_rep = 10
                    pesan = "Dana dialihkan untuk patroli Pos Polisi — reputasi naik pesat!"

            koin_baru = max(0, pemain["koin"] + delta_koin)
            rep_baru = max(0, min(100, pemain["reputasi"] + delta_rep))
            self.koneksi.execute(
                "UPDATE pemain SET koin = ?, reputasi = ? WHERE id = ?",
                (koin_baru, rep_baru, pemain["id"]))
            self.koneksi.execute(
                "UPDATE peristiwa_dunia SET status = 'selesai', ditangani_oleh = ?"
                " WHERE id = ?", (tujuan, peristiwa_id))
            ev_did2 = ev["distrik_id"] if "distrik_id" in ev.keys() else 1
            self._pastikan_peristiwa_beragam(ev_did2)
            self.koneksi.commit()
        return {"id": peristiwa_id, "tujuan": tujuan, "pesan": pesan,
                "koin": koin_baru, "reputasi": rep_baru}, None

    # ------------------------------------------------- chat & leaderboard
    def daftar_chat(self, distrik_id=1):
        with self.kunci:
            rows = self.koneksi.execute(
                "SELECT id, nama, isi, dibuat FROM pesan_chat"
                " WHERE distrik_id = ? ORDER BY id DESC LIMIT 25",
                (distrik_id,)).fetchall()
        hasil = [dict(r) for r in rows]
        hasil.reverse()
        return {"pesan": hasil}

    def kirim_chat(self, pemain, isi):
        teks = (isi or "").strip()
        if not teks or len(teks) > 140:
            return None, "Pesan harus 1-140 karakter."
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        now = int(time.time())
        with self.kunci:
            self.koneksi.execute(
                "INSERT INTO pesan_chat (distrik_id, pemain_id, nama, isi, dibuat)"
                " VALUES (?, ?, ?, ?, ?)",
                (did, pemain["id"], pemain["nama"], teks, now))
            self.koneksi.commit()
        return self.daftar_chat(did), None

    def leaderboard(self):
        with self.kunci:
            rows = self.koneksi.execute(
                "SELECT u.nama, u.koin, u.reputasi,"
                " (SELECT COUNT(*) FROM petak p WHERE p.pemilik_id = u.id"
                "  AND p.bangunan_id IS NOT NULL AND p.x = p.jangkar_x AND p.y = p.jangkar_y) AS bangunan"
                " FROM pemain u WHERE u.nama != 'Kota'"
                " ORDER BY u.koin DESC, u.reputasi DESC LIMIT 15").fetchall()
        return {"peringkat": [dict(r) for r in rows]}

    # ------------------------------------------------- upgrade & pindah
    def upgrade_bangunan(self, pemain, x, y):
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT pemilik_id, bangunan_id, jangkar_x, jangkar_y, level"
                " FROM petak WHERE distrik_id = ? AND x = ? AND y = ?",
                (did, x, y)).fetchone()
            if baris is None or baris["bangunan_id"] is None:
                return None, "Tidak ada bangunan di petak ini."
            if baris["pemilik_id"] != pemain["id"]:
                return None, "Bukan bangunan milikmu."
            bid = baris["bangunan_id"]
            if bid in ("jalan", "balaikota", "alun_alun"):
                return None, "Bangunan ini tidak dapat di-upgrade."
            jx, jy = baris["jangkar_x"], baris["jangkar_y"]
            lv = int(baris["level"] or 1)
            if lv >= 3:
                return None, "Bangunan ini sudah mencapai level maksimal."
            biaya_dasar = BANGUNAN[bid][0]
            biaya = biaya_dasar * (75 if lv == 1 else 125) // 100
            if pemain["koin"] < biaya:
                return None, "Koin tidak cukup untuk upgrade (butuh Rp" + str(biaya) + ")."
            lv_baru = lv + 1
            self.koneksi.execute(
                "UPDATE pemain SET koin = koin - ? WHERE id = ?",
                (biaya, pemain["id"]))
            self.koneksi.execute(
                "UPDATE petak SET level = ? WHERE distrik_id = ? AND jangkar_x = ? AND jangkar_y = ?",
                (lv_baru, did, jx, jy))
            self.koneksi.commit()
        return {"x": jx, "y": jy, "level": lv_baru, "koin": pemain["koin"] - biaya}, None

    def pindah_bangunan(self, pemain, x_lama, y_lama, x_baru, y_baru):
        did = pemain["distrik_aktif"] if "distrik_aktif" in pemain.keys() else 1
        with self.kunci:
            baris = self.koneksi.execute(
                "SELECT pemilik_id, bangunan_id, jangkar_x, jangkar_y, level"
                " FROM petak WHERE distrik_id = ? AND x = ? AND y = ?",
                (did, x_lama, y_lama)).fetchone()
            if baris is None or baris["bangunan_id"] is None:
                return None, "Tidak ada bangunan di petak asal."
            if baris["pemilik_id"] != pemain["id"]:
                return None, "Bukan bangunan milikmu."
            bid = baris["bangunan_id"]
            jx, jy = baris["jangkar_x"], baris["jangkar_y"]
            lv = int(baris["level"] or 1)
            if x_baru == jx and y_baru == jy:
                return None, "Posisi tujuan sama dengan posisi sekarang."
            lebar, tinggi = BANGUNAN[bid][6], BANGUNAN[bid][7]
            if not (0 <= x_baru and 0 <= y_baru and x_baru + lebar <= UKURAN_PETA
                    and y_baru + tinggi <= UKURAN_PETA):
                return None, "Tapak tujuan keluar dari peta bersama."
            for px in range(x_baru, x_baru + lebar):
                for py in range(y_baru, y_baru + tinggi):
                    tujuan = self.koneksi.execute(
                        "SELECT pemilik_id, bangunan_id, jangkar_x, jangkar_y"
                        " FROM petak WHERE distrik_id = ? AND x = ? AND y = ?",
                        (did, px, py)).fetchone()
                    milik_sendiri_lama = (
                        tujuan is not None
                        and tujuan["bangunan_id"] is not None
                        and tujuan["jangkar_x"] == jx
                        and tujuan["jangkar_y"] == jy
                    )
                    if not milik_sendiri_lama:
                        if tujuan is None or tujuan["pemilik_id"] != pemain["id"]:
                            return None, "Petak (" + str(px) + ", " + str(py) + ") bukan tanah milikmu."
                        if tujuan["bangunan_id"] is not None:
                            return None, "Petak (" + str(px) + ", " + str(py) + ") sudah terisi bangunan."
            self.koneksi.execute(
                "UPDATE petak SET bangunan_id = NULL, jangkar_x = NULL, jangkar_y = NULL, level = 1"
                " WHERE distrik_id = ? AND jangkar_x = ? AND jangkar_y = ?",
                (did, jx, jy))
            for px in range(x_baru, x_baru + lebar):
                for py in range(y_baru, y_baru + tinggi):
                    self.koneksi.execute(
                        "UPDATE petak SET bangunan_id = ?, jangkar_x = ?, jangkar_y = ?, level = ?"
                        " WHERE distrik_id = ? AND x = ? AND y = ?",
                        (bid, x_baru, y_baru, lv, did, px, py))
            self.koneksi.commit()
        return {"x_lama": jx, "y_lama": jy, "x": x_baru, "y": y_baru, "level": lv}, None
