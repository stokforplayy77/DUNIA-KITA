#!/usr/bin/env python3
# Dunia Kita — generate-ikon-aplikasi.py
# Menggambar ikon aplikasi 512x512 (Fase 5) dari kode PIL — tanpa
# unduhan. Gaya kartun bersih yang sama dengan sprite game
# (generate-sprites.py): outline cokelat gelap, rumah krem beratap
# merah + palu di sudut kanan bawah, di atas latar hijau membulat.
# Ikon dipakai ProjectSettings (m_Icons) sebagai ikon aplikasi Android.
#
# Jalankan:  python3 tools/generate-ikon-aplikasi.py

import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                   "..", "unity-project", "Assets", "Sprites")

OL = (74, 52, 32, 255)            # outline cokelat gelap
PUTIH = (255, 255, 255, 255)
KACA = (191, 227, 255, 255)
KAYU = (150, 100, 50, 255)
KREM = (255, 224, 179, 255)
KREM_GELAP = (242, 184, 128, 255)
MERAH_ATAP = (217, 108, 63, 255)
MERAH_ATAP_GELAP = (198, 90, 58, 255)
RUMPUT = (126, 200, 80, 255)
RUMPUT_TERANG = (142, 212, 94, 255)
RUMPUT_GELAP = (93, 163, 60, 255)
ABU = (150, 150, 160, 255)
ABU_GELAP = (110, 110, 122, 255)

S = 512
img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d = ImageDraw.Draw(img)

# Latar: kotak membulat hijau (gradien vertikal halus: interpolasi
# terang -> gelap dalam 32 pita).
ATAS = RUMPUT_TERANG
BAWAH = RUMPUT_GELAP
for i in range(32):
    y0 = i * 16
    f = i / 31.0
    warna = tuple(int(ATAS[k] + (BAWAH[k] - ATAS[k]) * f) for k in range(3)) + (255,)
    d.rectangle([0, y0, S, y0 + 17], fill=warna)
# Bulatkan sudut latar dengan menutup keempat sudut (transparan).
d.rounded_rectangle([0, 0, S, S], radius=112, outline=None)
mask = Image.new("L", (S, S), 0)
dm = ImageDraw.Draw(mask)
dm.rounded_rectangle([0, 0, S, S], radius=112, fill=255)
img.putalpha(mask)

# Lingkaran terang di belakang rumah (pusat perhatian).
d.ellipse([56, 96, 456, 496], fill=(168, 222, 120, 255), outline=OL, width=6)

# ---- Rumah (tengah, sedikit ke atas) ----
# Dinding krem.
d.rectangle([136, 250, 376, 420], fill=KREM, outline=OL, width=6)
# Sisi teduh kanan (pita lebih gelap).
d.rectangle([336, 250, 376, 420], fill=KREM_GELAP)
d.rectangle([336, 250, 376, 420], outline=OL, width=6)
# Atap merah (segitiga + lis bawah).
d.polygon([(96, 262), (256, 128), (416, 262)], fill=MERAH_ATAP,
          outline=OL)
d.line([(96, 262), (256, 128), (416, 262)], fill=OL, width=6, joint="curve")
d.rectangle([96, 252, 416, 272], fill=MERAH_ATAP_GELAP, outline=OL, width=4)
# Cerobong.
d.rectangle([316, 168, 348, 226], fill=KREM_GELAP, outline=OL, width=4)
d.rectangle([310, 158, 354, 172], fill=MERAH_ATAP_GELAP, outline=OL, width=4)
# Pintu cokelat + gagang.
d.rectangle([226, 318, 286, 420], fill=KAYU, outline=OL, width=5)
d.ellipse([270, 366, 282, 378], fill=(255, 209, 102, 255), outline=OL, width=2)
# Jendela kiri + kanan (kaca biru berbingkai putih).
for x0 in (156, 330):
    d.rectangle([x0, 292, x0 + 52, 344], fill=PUTIH, outline=OL, width=4)
    d.rectangle([x0 + 6, 298, x0 + 46, 338], fill=KACA)
    d.line([x0 + 26, 298, x0 + 26, 338], fill=OL, width=3)
    d.line([x0 + 6, 318, x0 + 46, 318], fill=OL, width=3)

# ---- Palu (sudut kanan bawah, diagonal naik ke kanan) ----
# Gagang: garis gelap lebar di bawah, kayu di atas (efek outline).
d.line([(330, 428), (452, 306)], fill=OL, width=30, joint="curve")
d.line([(330, 428), (452, 306)], fill=KAYU, width=20, joint="curve")
# Kepala palu: balok abu-abu tegak lurus gagang (ujung atas gagang).
d.line([(420, 272), (484, 336)], fill=OL, width=40, joint="curve")
d.line([(420, 272), (484, 336)], fill=ABU, width=30, joint="curve")
d.line([(420, 272), (484, 336)], fill=ABU_GELAP, width=10, joint="curve")

# Rumput kecil di kaki rumah (3 rumpun segitiga hijau gelap).
for x in (150, 396, 250):
    d.polygon([(x - 14, 420), (x, 392), (x + 14, 420)], fill=RUMPUT_GELAP,
              outline=OL)

jalur = os.path.join(OUT, "ikon-aplikasi.png")
img.save(jalur, "PNG")
print("tulis", jalur, img.size)
