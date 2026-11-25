#!/usr/bin/env python3
# Dunia Kita — contact-sheet.py
# Merangkai semua sprite game menjadi satu lembar kontak berlabel
# (latar abu terang supaya sprite transparan terlihat) untuk
# pemeriksaan visual gaya kartun. Jalankan sesudah generate-sprites.py.
import os
from PIL import Image, ImageDraw, ImageFont

DASAR = os.path.dirname(os.path.abspath(__file__))
SPR = os.path.join(DASAR, "..", "unity-project", "Assets", "Sprites")

BARIS = [
    ("Bangunan 1x1", ["rumah.png", "warung.png", "taman.png", "jalan.png",
                      "toko.png", "perpustakaan.png", "pos.png", "kafe.png"]),
    ("Bangunan 2x2", ["kost.png", "sekolah.png", "pasar.png", "apartemen.png",
                      "klinik.png", "pembangkit.png", "balaikota.png"]),
    ("Bangunan 3x3", ["perumahan.png", "kampus.png", "alun-alun.png"]),
    ("Warga (diam + jalan A/B)", ["warga1.png", "warga1-a.png", "warga1-b.png",
                                  "warga2.png", "warga2-a.png", "warga2-b.png",
                                  "warga3.png", "warga3-a.png", "warga3-b.png"]),
    ("Interior", ["interior-rumah.png", "interior-kost.png",
                 "interior-apartemen.png", "interior-perumahan.png"]),
    ("Perabot", ["perabot-kasur.png", "perabot-lemari.png", "perabot-kompor.png",
                 "perabot-meja.png", "perabot-kursi.png", "perabot-bak_mandi.png",
                 "perabot-wastafel.png", "perabot-tv.png", "perabot-sofa.png",
                 "perabot-lampu.png"]),
]

LEBAR_SEL = 270
TINGGI_SEL = 240
TINGGI_JUDUL = 44
LATAR = (238, 238, 235, 255)
WARNA_JUDUL = (47, 59, 44, 255)


def font(ukuran):
    try:
        return ImageFont.load_default(size=ukuran)
    except TypeError:
        return ImageFont.load_default()


f_judul = font(28)
f_label = font(20)

lebar = max(len(nama) for _, nama in BARIS) * LEBAR_SEL
tinggi = sum(TINGGI_JUDUL + TINGGI_SEL for _, _ in BARIS)
lembar = Image.new("RGB", (lebar, tinggi), LATAR)
g = ImageDraw.Draw(lembar)

y = 0
for judul, daftar in BARIS:
    g.rectangle([0, y, lebar, y + TINGGI_JUDUL], fill=(47, 59, 44, 255))
    g.text((12, y + 8), judul, font=f_judul, fill=(255, 255, 255, 255))
    y += TINGGI_JUDUL
    for i, nama in enumerate(daftar):
        jalur = os.path.join(SPR, nama)
        img = Image.open(jalur).convert("RGBA")
        # Skala supaya muat di sel (jaga rasio).
        skala = min((LEBAR_SEL - 16) / img.width, (TINGGI_SEL - 40) / img.height)
        baru = img.resize((max(1, int(img.width * skala)),
                           max(1, int(img.height * skala))),
                          Image.LANCZOS)
        x = i * LEBAR_SEL + (LEBAR_SEL - baru.width) // 2
        yy = y + (TINGGI_SEL - 40 - baru.height) // 2
        lembar.paste(baru, (x, yy), baru)
        label = nama.replace(".png", "")
        lw = g.textlength(label, font=f_label)
        g.text((i * LEBAR_SEL + (LEBAR_SEL - lw) / 2, y + TINGGI_SEL - 28),
               label, font=f_label, fill=WARNA_JUDUL)
    y += TINGGI_SEL

keluar = os.path.join(DASAR, "contact-sheet.png")
lembar.save(keluar, "PNG")
print("tulis", keluar, lembar.size)
