#!/usr/bin/env python3
# Dunia Kita — tools/qa-fase6.py
# QA AKHIR (Alpha) Fase 6: konsistensi data 18 bangunan + invarian rilis.
#
# Yang diperiksa (semua dari berkas, tanpa Unity Editor):
#   A. 18 file .asset: id unik, nama/deskripsi terisi, harga > 0,
#      perawatan >= 0, levelBuka 1-6, kategori 0-2, ukuran 1x1/2x2/3x3.
#   B. Aturan kategori: hunian punya kapasitas > 0, komersial punya
#      pemasukan > 0, semua yang berpenghasilan untung sendiri
#      (pemasukan > perawatan).
#   C. Sprite: berkas PNG ada, dimensi kanvas cocok ukuran pada
#      PPU 100 (1x1=256x256, 2x2=512x256, 3x3=768x384) atau kelipatannya
#      sesuai spritePixelsToUnits di .meta (sprite HD: PPU 400 ->
#      1024x1024 / 2048x1024), GUID sprite di .asset
#      sama dengan GUID di .meta PNG, .meta terkompresi
#      (textureType 8, textureFormat -3).
#   D. Silang dengan GameBalance.cs + README (angka yang didokumentasi):
#      Balai Kota, Kost, dan 16 bangunan lain cocok dengan angka acuan.
#   E. ProjectSettings: perusahaan/produk/package/versi/orientasi
#      landscape (kiri+kanan, portrait mati); EditorBuildSettings hanya
#      MainScene yang aktif; QualitySettings default = Medium (indeks 2).
#   F. Invarian kritis: manifest memakai particlesystem (bukan
#      particles), font LegacyRuntime (Arial 0), tidak ada tombol
#      "Lewati Hari"/"Simpan" di script, 7 WAV audio + .meta ada,
#      ikon aplikasi 512x512 ada.
#   G. Kelayakan Level 6: kapasitas hunian maksimum teoretis di grid
#      16x16 (isi penuh Perumahan) >= 300 warga syarat Metropolis.
#
# Keluar kode 0 bila semua pemeriksaan lolos, 1 bila ada yang gagal.
# Jalankan: python3 tools/qa-fase6.py

import glob
import os
import re
import sys
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PROYEK = os.path.join(ROOT, "unity-project")
ASET_DIR = os.path.join(PROYEK, "Assets", "Data", "Buildings")
SPRITE_DIR = os.path.join(PROYEK, "Assets", "Sprites")
GAMEBALANCE = os.path.join(PROYEK, "Assets", "Scripts", "Data",
                           "GameBalance.cs")

lolos = 0
gagal = 0


def periksa(kondisi, pesan):
    global lolos, gagal
    if kondisi:
        lolos += 1
    else:
        gagal += 1
        print("GAGAL: " + pesan)


def baca(path):
    with open(path, encoding="utf-8") as f:
        return f.read()


def ambil(teks, kunci):
    m = re.search(r"^  " + re.escape(kunci) + r": (.+)$", teks, re.M)
    return m.group(1).strip() if m else None


# Pemetaan id bangunan -> berkas sprite (sama seperti generate-project.py).
SPRITE = {
    "rumah": "rumah.png", "warung": "warung.png", "taman": "taman.png",
    "kost": "kost.png", "toko_kelontong": "toko.png", "jalan": "jalan.png",
    "perpustakaan": "perpustakaan.png", "sekolah": "sekolah.png",
    "pasar": "pasar.png", "pos": "pos.png", "apartemen": "apartemen.png",
    "kafe": "kafe.png", "klinik": "klinik.png",
    "pembangkit": "pembangkit.png", "perumahan": "perumahan.png",
    "kampus": "kampus.png", "alun_alun": "alun-alun.png",
    "balaikota": "balaikota.png",
}

# Angka yang didokumentasi di README (harga, pemasukan, perawatan,
# kapasitas, bonus bahagia, level buka) — harus cocok dengan .asset.
DOKUMENTASI = {
    "rumah": dict(harga=500000, rawat=1000, kap=2, lvl=1),
    "warung": dict(harga=750000, masuk=15000, rawat=2000, lvl=1),
    "taman": dict(harga=250000, rawat=500, lvl=1),
    "kost": dict(harga=2000000, masuk=40000, rawat=5000, kap=8, lvl=2),
    "toko_kelontong": dict(harga=1500000, masuk=30000, rawat=4000, lvl=3),
    "jalan": dict(harga=50000, lvl=1),
    "perpustakaan": dict(harga=800000, bahagia=2, lvl=2),
    "sekolah": dict(harga=3000000, bahagia=4, lvl=3),
    "pasar": dict(harga=4000000, masuk=60000, lvl=3),
    "pos": dict(harga=1000000, bahagia=2, lvl=3),
    "apartemen": dict(harga=5000000, masuk=60000, kap=12, lvl=4),
    "kafe": dict(harga=2000000, masuk=25000, bahagia=2, lvl=4),
    "klinik": dict(harga=4000000, bahagia=4, lvl=4),
    "pembangkit": dict(harga=6000000, bahagia=4, lvl=4),
    "perumahan": dict(harga=8000000, masuk=100000, kap=20, lvl=5),
    "kampus": dict(harga=10000000, masuk=80000, bahagia=6, lvl=5),
    "alun_alun": dict(harga=12000000, bahagia=10, lvl=6),
    "balaikota": dict(harga=3000000, rawat=5000, bahagia=5, lvl=3),
}

DIMENSI = {(1, 1): (256, 256), (2, 2): (512, 256), (3, 3): (768, 384)}

print("== A. Jumlah & identitas 18 bangunan ==")
berkas = sorted(glob.glob(os.path.join(ASET_DIR, "*.asset")))
periksa(len(berkas) == 18, "jumlah .asset = %d, bukan 18" % len(berkas))

data = {}
for path in berkas:
    teks = baca(path)
    bid = ambil(teks, "id")
    periksa(bid is not None, "%s: field id hilang" % os.path.basename(path))
    if bid is None:
        continue
    periksa(bid not in data, "id duplikat: " + bid)
    data[bid] = {
        "nama": ambil(teks, "nama"),
        "deskripsi": ambil(teks, "deskripsi"),
        "kategori": int(ambil(teks, "kategori")),
        "levelBuka": int(ambil(teks, "levelBuka")),
        "ukuran": ambil(teks, "ukuran"),
        "biayaBangun": int(ambil(teks, "biayaBangun")),
        "pemasukanHarian": int(ambil(teks, "pemasukanHarian")),
        "biayaPerawatanHarian": int(ambil(teks, "biayaPerawatanHarian")),
        "kapasitasWarga": int(ambil(teks, "kapasitasWarga")),
        "bonusBahagia": int(ambil(teks, "bonusBahagia")),
        "guid_sprite": re.search(r"sprite: \{fileID: \d+, guid: ([0-9a-f]+)",
                                 teks).group(1),
    }

periksa(set(data) == set(DOKUMENTASI),
        "id .asset tidak sama dengan daftar dokumentasi: %s"
        % sorted(set(data) ^ set(DOKUMENTASI)))

print("== B. Aturan tiap bangunan ==")
for bid, d in sorted(data.items()):
    periksa(bool(d["nama"]), bid + ": nama kosong")
    periksa(bool(d["deskripsi"]), bid + ": deskripsi kosong")
    periksa(d["biayaBangun"] > 0, bid + ": biayaBangun tidak > 0")
    periksa(d["biayaPerawatanHarian"] >= 0,
            bid + ": perawatan negatif")
    periksa(1 <= d["levelBuka"] <= 6,
            bid + ": levelBuka %d di luar 1-6" % d["levelBuka"])
    periksa(d["kategori"] in (0, 1, 2),
            bid + ": kategori %d di luar 0-2" % d["kategori"])
    m = re.match(r"\{x: (\d), y: (\d)\}", d["ukuran"])
    periksa(m is not None, bid + ": ukuran tidak terbaca: " + d["ukuran"])
    if m:
        ux, uy = int(m.group(1)), int(m.group(2))
        periksa(ux == uy and ux in (1, 2, 3),
                bid + ": ukuran %dx%d bukan 1x1/2x2/3x3" % (ux, uy))
        d["ux"], d["uy"] = ux, uy
    periksa(0 <= d["bonusBahagia"] <= 10,
            bid + ": bonusBahagia %d di luar 0-10" % d["bonusBahagia"])
    if d["kategori"] == 0:
        periksa(d["kapasitasWarga"] > 0,
                bid + ": hunian tanpa kapasitas warga")
    if d["kategori"] == 1:
        periksa(d["pemasukanHarian"] > 0,
                bid + ": komersial tanpa pemasukan")
    if d["pemasukanHarian"] > 0:
        periksa(d["pemasukanHarian"] > d["biayaPerawatanHarian"],
                bid + ": pemasukan tidak menutup perawatan sendiri")

print("== C. Sprite: berkas, dimensi, GUID, kompresi ==")
for bid, d in sorted(data.items()):
    nama_sprite = SPRITE[bid]
    path_png = os.path.join(SPRITE_DIR, nama_sprite)
    periksa(os.path.isfile(path_png), bid + ": sprite hilang " + nama_sprite)
    if not os.path.isfile(path_png):
        continue
    lebar, tinggi = Image.open(path_png).size
    meta = baca(path_png + ".meta")
    ppu = int(re.search(r"spritePixelsToUnits: (\d+)", meta).group(1))
    acuan = DIMENSI[(d["ux"], d["uy"])]
    periksa(lebar * 100 == acuan[0] * ppu and tinggi * 100 == acuan[1] * ppu,
            "%s: sprite %dx%d @PPU %d, seharusnya setara %s @PPU 100 "
            "untuk ukuran %dx%d"
            % (bid, lebar, tinggi, ppu, acuan, d["ux"], d["uy"]))
    guid_meta = re.search(r"^guid: ([0-9a-f]+)$", meta, re.M).group(1)
    periksa(guid_meta == d["guid_sprite"],
            bid + ": GUID sprite .asset != .meta PNG")
    periksa("textureType: 8" in meta,
            bid + ": sprite bukan textureType 8 (Sprite)")
    periksa("textureFormat: -3" in meta,
            bid + ": sprite bukan textureFormat -3 (terkompresi)")

print("== D. Silang dengan GameBalance + README ==")
gb = baca(GAMEBALANCE)


def konstanta(nama):
    m = re.search(r"const (?:int|long|float|string) " + nama
                  + r" = ([^;]+);", gb)
    return m.group(1).strip().strip('"') if m else None


periksa(konstanta("HargaBalaiKota") == "3000000",
        "GameBalance.HargaBalaiKota != 3000000")
periksa(konstanta("PerawatanBalaiKota") == "5000",
        "GameBalance.PerawatanBalaiKota != 5000")
periksa(konstanta("BonusBahagiaBalaiKota") == "5",
        "GameBalance.BonusBahagiaBalaiKota != 5")
periksa(konstanta("LevelBukaBalaiKota") == "3",
        "GameBalance.LevelBukaBalaiKota != 3")
periksa(konstanta("SewaKostSehari") == "40000",
        "GameBalance.SewaKostSehari != 40000 (Kost.asset)")
for bid, dok in sorted(DOKUMENTASI.items()):
    d = data[bid]
    if "harga" in dok:
        periksa(d["biayaBangun"] == dok["harga"],
                "%s: harga .asset %d != README %d"
                % (bid, d["biayaBangun"], dok["harga"]))
    if "masuk" in dok:
        periksa(d["pemasukanHarian"] == dok["masuk"],
                "%s: pemasukan .asset %d != README %d"
                % (bid, d["pemasukanHarian"], dok["masuk"]))
    if "rawat" in dok:
        periksa(d["biayaPerawatanHarian"] == dok["rawat"],
                "%s: perawatan .asset %d != README %d"
                % (bid, d["biayaPerawatanHarian"], dok["rawat"]))
    if "kap" in dok:
        periksa(d["kapasitasWarga"] == dok["kap"],
                "%s: kapasitas .asset %d != README %d"
                % (bid, d["kapasitasWarga"], dok["kap"]))
    if "bahagia" in dok:
        periksa(d["bonusBahagia"] == dok["bahagia"],
                "%s: bonusBahagia .asset %d != README %d"
                % (bid, d["bonusBahagia"], dok["bahagia"]))
    if "lvl" in dok:
        periksa(d["levelBuka"] == dok["lvl"],
                "%s: levelBuka .asset %d != README %d"
                % (bid, d["levelBuka"], dok["lvl"]))
# Balai Kota silang .asset vs GameBalance.
b = data["balaikota"]
periksa(b["biayaBangun"] == int(konstanta("HargaBalaiKota")),
        "BalaiKota.asset harga != GameBalance.HargaBalaiKota")
periksa(b["biayaPerawatanHarian"] == int(konstanta("PerawatanBalaiKota")),
        "BalaiKota.asset perawatan != GameBalance.PerawatanBalaiKota")
periksa(b["bonusBahagia"] == int(konstanta("BonusBahagiaBalaiKota")),
        "BalaiKota.asset bonus != GameBalance.BonusBahagiaBalaiKota")
periksa(b["levelBuka"] == int(konstanta("LevelBukaBalaiKota")),
        "BalaiKota.asset level != GameBalance.LevelBukaBalaiKota")
periksa(data["kost"]["pemasukanHarian"] == int(konstanta("SewaKostSehari")),
        "Kost.asset pemasukan != GameBalance.SewaKostSehari")
# Total harga + bonus 10 perabot = Rp1.420.000 / +16 (README Fase 4B).
harga_perabot = [int(x) for x in re.findall(
    r"public const long Harga(?:Kasur|Lemari|Kompor|Meja|Kursi|BakMandi"
    r"|Wastafel|TV|Sofa|Lampu) = (\d+);", gb)]
bonus_perabot = [int(x) for x in re.findall(
    r"public const int Bonus(?:Kasur|Lemari|Kompor|Meja|Kursi|BakMandi"
    r"|Wastafel|TV|Sofa|Lampu) = (\d+);", gb)]
periksa(len(harga_perabot) == 10, "harga perabot di GameBalance != 10")
periksa(sum(harga_perabot) == 1420000,
        "total harga 10 perabot = %d, bukan 1420000" % sum(harga_perabot))
periksa(sum(bonus_perabot) == 16,
        "total bonus 10 perabot = %d, bukan 16" % sum(bonus_perabot))

print("== E. ProjectSettings / BuildSettings / QualitySettings ==")
ps = baca(os.path.join(PROYEK, "ProjectSettings", "ProjectSettings.asset"))
periksa("companyName: Uhlabs" in ps, "companyName bukan Uhlabs")
periksa("productName: Dunia Kita" in ps, "productName bukan Dunia Kita")
periksa("bundleVersion: 1.0" in ps, "bundleVersion bukan 1.0")
periksa("Android: com.uhlabs.duniakita" in ps,
        "package Android bukan com.uhlabs.duniakita")
periksa("defaultInterfaceOrientation: 4" in ps,
        "orientasi default bukan 4 (LandscapeLeft)")
periksa("allowedAutorotateToLandscapeLeft: 1" in ps,
        "rotasi LandscapeLeft tidak diizinkan")
periksa("allowedAutorotateToLandscapeRight: 1" in ps,
        "rotasi LandscapeRight tidak diizinkan")
periksa("allowedAutorotateToPortrait: 0" in ps,
        "rotasi Portrait masih diizinkan (harus 0)")
periksa("allowedAutorotateToPortraitUpsideDown: 0" in ps,
        "rotasi PortraitUpsideDown masih diizinkan (harus 0)")
ebs = baca(os.path.join(PROYEK, "ProjectSettings",
                       "EditorBuildSettings.asset"))
periksa("path: Assets/Scenes/MainScene.unity" in ebs,
        "MainScene tidak ada di Build Settings")
periksa(ebs.count("path: Assets/Scenes/") == 1,
        "Build Settings berisi scene selain MainScene")
qs = baca(os.path.join(PROYEK, "ProjectSettings", "QualitySettings.asset"))
nama_kualitas = re.findall(r"^    name: (.+)$", qs, re.M)
periksa(nama_kualitas[2] == "Medium",
        "kualitas indeks 2 bukan Medium: %s" % nama_kualitas)
periksa("m_CurrentQuality: 2" in qs,
        "kualitas default bukan indeks 2 (Medium)")

print("== F. Invarian kritis rilis ==")
manifest = baca(os.path.join(PROYEK, "Packages", "manifest.json"))
periksa("com.unity.modules.particlesystem" in manifest,
        "manifest tidak memakai particlesystem")
periksa("com.unity.modules.particles\"" not in manifest,
        "manifest masih memakai nama lama particles")
skrip = ""
for path in glob.glob(os.path.join(PROYEK, "Assets", "Scripts", "**",
                                  "*.cs"), recursive=True):
    # Hapus komentar // supaya teks di komentar (mis. "TIDAK ada lagi
    # tombol ...") tidak terbaca sebagai tombol yang masih ada.
    for baris in baca(path).splitlines():
        skrip += baris.split("//")[0] + "\n"
periksa("LegacyRuntime" in skrip, "font LegacyRuntime tidak ditemukan")
periksa("Arial" not in skrip, "string Arial masih ada di script")
periksa("Lewati Hari" not in skrip,
        "teks tombol 'Lewati Hari' masih ada di script")
periksa('"Simpan"' not in skrip,
        "teks tombol 'Simpan' masih ada di script")
periksa("TombolIkon" in skrip or "BuatTombolIkon" in skrip,
        "tombol Bangun ikon tidak ditemukan di script")
for wav in ["sfx-tap", "sfx-bangun", "sfx-koin", "sfx-naik-level",
            "sfx-klaim", "sfx-tolak", "musik-latar"]:
    periksa(os.path.isfile(os.path.join(PROYEK, "Assets", "Audio",
                                       wav + ".wav")),
            "audio hilang: " + wav + ".wav")
    periksa(os.path.isfile(os.path.join(PROYEK, "Assets", "Audio",
                                       wav + ".wav.meta")),
            "meta audio hilang: " + wav + ".wav.meta")
ikon = os.path.join(SPRITE_DIR, "ikon-aplikasi.png")
periksa(os.path.isfile(ikon), "ikon aplikasi hilang")
if os.path.isfile(ikon):
    periksa(Image.open(ikon).size == (512, 512),
            "ikon aplikasi bukan 512x512")

print("== G. Kelayakan Level 6 ==")
# Kapasitas maksimum teoretis: isi grid 16x16 penuh Perumahan 3x3.
maks_perumahan = (16 // 3) ** 2
kap_maks = maks_perumahan * data["perumahan"]["kapasitasWarga"]
periksa(kap_maks >= 300,
        "kapasitas maks grid 16x16 = %d warga, kurang dari 300 (Metropolis)"
        % kap_maks)
print("   (info: %d Perumahan muat di 16x16 -> %d warga maks)"
      % (maks_perumahan, kap_maks))

print()
print("HASIL QA FASE 6: %d lolos, %d gagal" % (lolos, gagal))
sys.exit(1 if gagal > 0 else 0)
