#!/usr/bin/env python3
# Dunia Kita — tools/generate-feature-graphic.py
# Feature graphic Play Store 1024x500 (Fase 6), digambar dari kode
# (PIL + sprite yang sudah ada), tanpa unduhan.
# Komposisi: langit biru cerah + sabuk rumput hijau di bawah, judul
# "Dunia Kita" besar di kiri, Balai Kota + rumah + warga di kanan.
# Font DejaVu Sans Bold (sistem). Jalankan: python3
# tools/generate-feature-graphic.py — keluaran rilis/feature-graphic.png
# lalu PERIKSA visual sebelum dipakai (buka berkasnya dan lihat).

import os
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPRITE = os.path.join(ROOT, "unity-project", "Assets", "Sprites")
KELUAR = os.path.join(ROOT, "rilis", "feature-graphic.png")

LEBAR, TINGGI = 1024, 500
FONT_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
FONT_BIASA = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"


def gradasi_langit():
    atas = (126, 199, 235)
    bawah = (214, 240, 250)
    gambar = Image.new("RGB", (LEBAR, TINGGI))
    piksel = gambar.load()
    for y in range(TINGGI):
        t = y / (TINGGI - 1)
        r = int(atas[0] + (bawah[0] - atas[0]) * t)
        g = int(atas[1] + (bawah[1] - atas[1]) * t)
        b = int(atas[2] + (bawah[2] - atas[2]) * t)
        for x in range(LEBAR):
            piksel[x, y] = (r, g, b)
    return gambar


def tempel(gambar, nama, ukuran, posisi):
    sprite = Image.open(os.path.join(SPRITE, nama)).convert("RGBA")
    sprite = sprite.resize(ukuran, Image.LANCZOS)
    gambar.paste(sprite, posisi, sprite)


def main():
    gambar = gradasi_langit()
    lukis = ImageDraw.Draw(gambar, "RGBA")
    # Awan sederhana.
    for (x, y, r) in [(820, 60, 46), (880, 78, 34), (120, 70, 40)]:
        lukis.ellipse([x - r, y - r // 2, x + r, y + r // 2],
                      fill=(255, 255, 255, 220))
    # Sabuk rumput di bawah.
    lukis.rectangle([0, 380, LEBAR, TINGGI], fill=(106, 176, 76))
    lukis.rectangle([0, 380, LEBAR, 388], fill=(84, 148, 60))
    # Bangunan di atas rumput (kanan).
    tempel(gambar, "balaikota.png", (384, 192), (600, 210))
    tempel(gambar, "rumah.png", (170, 170), (470, 250))
    tempel(gambar, "taman.png", (150, 150), (880, 260))
    tempel(gambar, "warga1.png", (64, 64), (560, 330))
    tempel(gambar, "warga2.png", (64, 64), (640, 340))
    tempel(gambar, "warga3.png", (64, 64), (940, 330))
    # Judul + tagline (kiri).
    judul = ImageFont.truetype(FONT_BOLD, 92)
    tagline = ImageFont.truetype(FONT_BIASA, 30)
    kecil = ImageFont.truetype(FONT_BOLD, 26)
    lukis.text((52, 96), "Dunia Kita", font=judul, fill=(24, 64, 32))
    lukis.text((56, 210), "Bangun kotamu, satu petak", font=tagline,
               fill=(32, 80, 44))
    lukis.text((56, 248), "demi satu petak.", font=tagline,
               fill=(32, 80, 44))
    teks_pil = "18 bangunan - 6 level"
    lebar_pil = lukis.textlength(teks_pil, font=kecil)
    lukis.rounded_rectangle([52, 310, 52 + lebar_pil + 48, 356], radius=22,
                            fill=(255, 255, 255))
    lukis.text((76, 322), teks_pil, font=kecil,
               fill=(24, 64, 32))
    os.makedirs(os.path.dirname(KELUAR), exist_ok=True)
    gambar.save(KELUAR)
    print("ditulis", KELUAR, gambar.size)


if __name__ == "__main__":
    main()
