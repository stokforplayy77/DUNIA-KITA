#!/usr/bin/env python3
# Dunia Kita — generate-sprites.py
# Menggambar SEMUA sprite game dari kode PIL (tanpa unduhan).
# Gaya: kartun bersih (GDD §12) — outline cokelat gelap yang rapi di
# semua bentuk, dua sisi dinding (terang vs teduh), bayangan tanah
# di bawah tiap bangunan/warga, dan detail khas tiap bangunan
# (cerobong, jendela berbingkai, papan nama, dsb.).
#
# ATURAN DIMENSI (jangan diubah — scene/sorting/bounds bergantung):
#   1x1  -> kanvas 256x256, diamond petak 200x100, footprint 160x80
#   2x2  -> kanvas 512x256, diamond 400x200
#   3x3  -> kanvas 768x384, diamond 600x300
#   warga     -> 256x256 (sosok ~85px, kaki di y~210)
#   perabot   -> 128x128, latar transparan
#   tile/highlight -> 256x256
# Sprite memakai Pixels Per Unit 100 -> petak = 2.0 x 1.0 unit dunia.
# Ikon Bangun (ikon-bangun.png) SENGAJA tidak digambar ulang di sini
# (sudah disetujui user) — berkasnya dibiarkan apa adanya.

import os
from PIL import Image, ImageDraw, ImageFont

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                   "..", "unity-project", "Assets", "Sprites")
os.makedirs(OUT, exist_ok=True)

W, H = 256, 256
DIAMOND = [(128, 78), (228, 128), (128, 178), (28, 128)]
FOOT = [(128, 88), (208, 128), (128, 168), (48, 128)]

# ---- palet kartun modern (hangat, bersih, lembut di mata) ----
OL = (64, 52, 46, 255)            # outline cokelat-abu lembut
OL_HALUS = (96, 82, 72, 255)
PUTIH = (255, 255, 255, 255)
KACA = (165, 216, 255, 255)
KACA_TERANG = (224, 242, 254, 255)
KAYU = (166, 114, 68, 255)
KAYU_GELAP = (130, 84, 46, 255)
KULIT = (255, 218, 180, 255)
KULIT_BAYANG = (238, 190, 146, 255)
RAMBUT = (86, 58, 38, 255)
RUMPUT = (123, 201, 80, 255)
RUMPUT_TERANG = (136, 212, 92, 255)
RUMPUT_GELAP = (104, 178, 66, 255)
ABU_GELAP = (72, 82, 96, 255)
EMAS = (255, 209, 102, 255)
MERAH_ATAP = (224, 102, 74, 255)
MERAH_ATAP_GELAP = (196, 82, 58, 255)
KREM = (255, 236, 204, 255)
KREM_GELAP = (240, 208, 165, 255)


class Draw2x:
    """Wrapper ImageDraw untuk menggambar di kanvas 2x lalu di-downsample
    LANCZOS supaya seluruh tepi poligon, garis, dan jendela anti-aliased."""

    def __init__(self, img):
        self.d = ImageDraw.Draw(img, "RGBA")

    @staticmethod
    def _s(pts):
        return [(p[0] * 2, p[1] * 2) for p in pts]

    def polygon(self, xy, fill=None, outline=None, width=1):
        self.d.polygon(self._s(xy), fill=fill, outline=outline, width=width * 2)

    def line(self, xy, fill=None, width=1):
        self.d.line(self._s(xy), fill=fill, width=max(1, int(width * 2)))

    def ellipse(self, xy, fill=None, outline=None, width=1):
        self.d.ellipse([v * 2 for v in xy], fill=fill, outline=outline,
                       width=max(1, int(width * 2)))

    def rectangle(self, xy, fill=None, outline=None, width=1):
        self.d.rectangle([v * 2 for v in xy], fill=fill, outline=outline,
                         width=max(1, int(width * 2)))


def baru():
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img, "RGBA")


SPRITE_HD = {
    "apartemen.png", "balaikota.png", "kafe.png", "klinik.png",
    "kost.png", "pasar.png", "pembangkit.png", "pos.png", "rumah.png",
    "sekolah.png", "taman.png", "toko.png", "warung.png",
    # Gelombang HD 25 Nov 2026: 4 bangunan + interior + perabot + warga
    # + ikon tombol bangun, semua dari tools/proses-aset-anime.py.
    "perpustakaan.png", "perumahan.png", "kampus.png", "alun-alun.png",
    "interior-rumah.png", "interior-kost.png", "interior-perumahan.png",
    "interior-apartemen.png",
    "perabot-bak_mandi.png", "perabot-kasur.png", "perabot-kompor.png",
    "perabot-kursi.png", "perabot-lampu.png", "perabot-lemari.png",
    "perabot-meja.png", "perabot-sofa.png", "perabot-tv.png",
    "perabot-wastafel.png",
    "warga1.png", "warga1-a.png", "warga1-b.png",
    "warga2.png", "warga2-a.png", "warga2-b.png",
    "warga3.png", "warga3-a.png", "warga3-b.png",
    "ikon-bangun.png",
}


def simpan(img, nama):
    if nama in SPRITE_HD:
        print("lewati (sprite HD disetujui user)", nama)
        return
    jalur = os.path.join(OUT, nama)
    img.save(jalur, "PNG")
    print("tulis", jalur, img.size)


def simpan2x(img, nama, w, h):
    kecil = img.resize((w, h), Image.Resampling.LANCZOS)
    simpan(kecil, nama)


def bayangan(d, cx, cy, rx, ry, alpha=55):
    """Elips bayangan lembut di bawah bangunan/warga."""
    d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(0, 0, 0, alpha))


def poli(d, titik, isi, garis=OL, lebar=3):
    """Poligon isi + outline rapi (gaya kartun)."""
    d.polygon(titik, fill=isi)
    d.line(titik + [titik[0]], fill=garis, width=lebar)


def jendela_iso(d, x0, y0, x1, y1):
    """Jendela isometric (belah ketupat kecil): kaca biru + bingkai
    cokelat + silang + kilau putih. Koordinat seperti Fase 3
    (y0 = atas, y1 = bawah; sisi miring naik setengah lebar)."""
    w2 = (x1 - x0) // 2
    titik = [(x0, y0), (x1, y0 + w2), (x1, y1), (x0, y1 - w2)]
    d.polygon(titik, fill=KACA)
    d.line(titik + [titik[0]], fill=KAYU_GELAP, width=2)
    # silang bingkai + kilau
    d.line([(x0, (y0 + y1) // 2), (x1, (y0 + y1) // 2 + w2)],
           fill=KAYU_GELAP, width=2)
    d.line([((x0 + x1) // 2, y0 + w2 // 2),
            ((x0 + x1) // 2, y1 - w2 // 2)], fill=KAYU_GELAP, width=2)
    d.line([(x0 + 4, y0 + w2 // 2 + 2), (x0 + w2, y0 + 2)],
           fill=KACA_TERANG, width=3)


def pintu_iso(d, x0, y0, x1, y1):
    """Pintu isometric cokelat + gagang emas kecil."""
    w2 = (x1 - x0) // 2
    titik = [(x0, y0), (x1, y0 + w2), (x1, y1), (x0, y1 - w2)]
    d.polygon(titik, fill=KAYU_GELAP)
    d.line(titik + [titik[0]], fill=OL, width=2)
    d.ellipse([(x1 - 8), (y0 + y1) // 2 - 2, (x1 - 3), (y0 + y1) // 2 + 3],
              fill=EMAS, outline=OL, width=1)


def cerobong(d, x, y, w=16, h=26):
    """Cerobong bata di atap + asap tipis (2 gelembung abu)."""
    d.rectangle([x, y, x + w, y + h], fill=(168, 80, 60, 255),
                outline=OL, width=2)
    d.line([(x, y + h // 3), (x + w, y + h // 3)], fill=OL, width=2)
    d.ellipse([x + 2, y - 14, x + 12, y - 4], fill=(220, 220, 220, 200))
    d.ellipse([x + 8, y - 24, x + 20, y - 12], fill=(220, 220, 220, 150))


def garis_genteng(d, p1, p2, p3, jumlah=3):
    """Garis-garis genteng sejajar di atap segitiga (p1 puncak,
    p2-p3 alas): garis pendek dari tepi kiri ke tepi kanan."""
    for i in range(1, jumlah + 1):
        t = i / (jumlah + 1.0)
        xa = p1[0] + (p2[0] - p1[0]) * t
        ya = p1[1] + (p2[1] - p1[1]) * t
        xb = p1[0] + (p3[0] - p1[0]) * t
        yb = p1[1] + (p3[1] - p1[1]) * t
        d.line([(xa, ya), (xb, yb)], fill=(150, 70, 35, 255), width=2)


def papan_nama(d, titik, isi=(255, 255, 255, 255)):
    """Papan nama putih berbingkai gelap (tanpa teks — ikon saja)."""
    poli(d, titik, isi, garis=OL, lebar=2)


def bunga(x, y, c):
    """Satu bunga kecil: kelopak kuning + tengah oranye."""
    return (x, y, c)


# ---- tile rumput (sentuhan rumput + bunga supaya tidak polos) ----
img, d = baru()
# ---- tile rumput bersih & modern (supersample 2x anti-aliased) ----
img_hi = Image.new("RGBA", (W * 2, H * 2), (0, 0, 0, 0))
dh = ImageDraw.Draw(img_hi)
DIAMOND_HI = [(x * 2, y * 2) for (x, y) in DIAMOND]
dh.polygon(DIAMOND_HI, fill=RUMPUT)
dh.polygon([(256, 176), (416, 256), (256, 336), (96, 256)], fill=RUMPUT_TERANG)
dh.line(DIAMOND_HI + [DIAMOND_HI[0]], fill=(108, 184, 68, 140), width=2)
for (x, y) in [(190, 240), (310, 260), (250, 290)]:
    dh.line([(x, y), (x - 4, y - 10)], fill=(96, 172, 60, 180), width=3)
    dh.line([(x, y), (x + 4, y - 10)], fill=(112, 190, 74, 180), width=3)
img = img_hi.resize((W, H), Image.Resampling.LANCZOS)
simpan(img, "tile.png")

# ---- sorotan petak (tidak berubah) ----
img, d = baru()
d.polygon(DIAMOND, fill=(255, 209, 102, 70))
d.line(DIAMOND + [DIAMOND[0]], fill=(255, 209, 102, 255), width=6)
simpan(img, "highlight.png")

# ================= BANGUNAN 1x1 (kanvas 256x256) =================
# Geometri terbukti Fase 1-3 (dinding naik dari footprint 160x80,
# atap piramida/datar) — dipertahankan; yang berubah: outline rapi,
# bayangan tanah, genteng, cerobong, jendela berbingkai, gagang pintu.

# ---- rumah sederhana ----
def rumah_detail():
    """Rumah gaya pixel-art iso detail (referensi Townian): digambar di
    kanvas 4x lalu diturunkan ke 256 agar ukuran dunia tetap sama."""
    Z = 4
    cv = Image.new("RGBA", (256 * Z, 256 * Z), (0, 0, 0, 0))
    d = ImageDraw.Draw(cv, "RGBA")
    Zp = lambda x, y: (x * Z, y * Z)

    # -- bayangan tanah lembut --
    d.ellipse([Zp(28, 120), Zp(228, 190)], fill=(0, 0, 0, 60))
    d.ellipse([Zp(48, 132), Zp(208, 184)], fill=(0, 0, 0, 50))

    # -- geometri: footprint 160x80, pusat (128,148) --
    L, R, B, T = (48, 148), (208, 148), (128, 188), (128, 108)
    hw = 52             # tinggi dinding (koordinat dasar 256)
    up = lambda p, dy: (p[0], p[1] - dy)

    # -- dinding (kiri terang, kanan teduh) dengan siding --
    KIRI, KANAN = (247, 240, 224, 255), (231, 218, 196, 255)
    d.polygon([Zp(*L), Zp(*B), Zp(*up(B, hw)), Zp(*up(L, hw))], fill=KIRI)
    d.polygon([Zp(*R), Zp(*B), Zp(*up(B, hw)), Zp(*up(R, hw))], fill=KANAN)
    # garis siding horizontal
    for i in range(1, 5):
        t = i / 5.0
        dy = hw * t
        d.line([Zp(L[0], L[1] - dy), Zp(B[0], B[1] - dy)],
               fill=(224, 212, 190, 255), width=Z)
        d.line([Zp(R[0], R[1] - dy), Zp(B[0], B[1] - dy)],
               fill=(206, 192, 168, 255), width=Z)

    # -- fondasi batu --
    d.polygon([Zp(*L), Zp(*B), Zp(B[0], B[1] + 10 * Z), Zp(L[0], L[1] + 10 * Z)],
              fill=(168, 164, 158, 255))
    d.polygon([Zp(*R), Zp(*B), Zp(B[0], B[1] + 10 * Z), Zp(R[0], R[1] + 10 * Z)],
              fill=(148, 144, 140, 255))
    d.line([Zp(L[0], L[1] + 10 * Z), Zp(B[0], B[1] + 10 * Z), Zp(R[0], R[1] + 10 * Z)],
           fill=OL, width=Z)

    # -- atap gable: bubungan memanjang kiri-atas -> kanan-atas,
    # dua bidang mengalir ke depan (L-B-R) --
    e1 = up(L, hw)          # tepi kiri atas
    e2 = up(R, hw)          # tepi kanan atas
    g1 = (128, 26)          # ujung bubungan kiri (belakang)
    g2 = (128, 116)         # ujung bubungan kanan (depan, di atas B)
    # bidang atap kiri & kanan
    d.polygon([Zp(*e1), Zp(*g1), Zp(*g2), Zp(*up(B, hw + 10))],
              fill=(196, 88, 52, 255))
    d.polygon([Zp(*e2), Zp(*g1), Zp(*g2), Zp(*up(B, hw + 10))],
              fill=(160, 66, 40, 255))
    # barisan genteng sejajar bubungan (dua sisi)
    for i in range(1, 5):
        t = i / 5.0
        ax = e1[0] + (up(B, hw + 10)[0] - e1[0]) * t
        ay = e1[1] + (up(B, hw + 10)[1] - e1[1]) * t
        gx = g1[0] + (g2[0] - g1[0]) * t
        gy = g1[1] + (g2[1] - g1[1]) * t
        d.line([Zp(ax, ay), Zp(gx, gy)], fill=(150, 60, 34, 255), width=Z)
        bx = e2[0] + (up(B, hw + 10)[0] - e2[0]) * t
        by = e2[1] + (up(B, hw + 10)[1] - e2[1]) * t
        d.line([Zp(bx, by), Zp(gx, gy)], fill=(120, 50, 30, 255), width=Z)
    # bubungan + outline
    d.line([Zp(*g1), Zp(*g2)], fill=OL, width=3 * Z)
    d.line([Zp(*e1), Zp(*up(B, hw + 10)), Zp(*e2)], fill=OL, width=2 * Z)
    d.line([Zp(*e1), Zp(*g1)], fill=OL, width=2 * Z)
    d.line([Zp(*e2), Zp(*g1)], fill=OL, width=2 * Z)

    # -- cerobong bata di kanan atap + asap --
    d.rectangle([Zp(172, 28), Zp(192, 62)], fill=(178, 92, 66, 255),
                outline=OL, width=Z)
    d.rectangle([Zp(170, 24), Zp(194, 32)], fill=(146, 70, 50, 255),
                outline=OL, width=Z)
    d.ellipse([Zp(176, 12), Zp(188, 24)], fill=(226, 226, 226, 190))
    d.ellipse([Zp(186, 2), Zp(200, 16)], fill=(226, 226, 226, 140))

    # -- fasad depan kiri: pintu kayu + jendela shutter --
    # pintu
    dp = [(70, 118), (104, 135), (104, 172), (70, 155)]
    d.polygon([Zp(*p) for p in dp], fill=(122, 74, 43, 255))
    d.line([Zp(*p) for p in dp] + [Zp(*dp[0])], fill=OL, width=Z)
    d.ellipse([Zp(97, 141), Zp(102, 146)], fill=EMAS)
    # jendela dengan shutter
    jw = [(132, 116), (168, 134), (168, 166), (132, 148)]
    d.polygon([Zp(*p) for p in jw], fill=(240, 234, 218, 255))
    d.line([Zp(*p) for p in jw] + [Zp(*jw[0])], fill=OL, width=Z)
    kaca = [(136, 120), (164, 134), (164, 160), (136, 146)]
    d.polygon([Zp(*p) for p in kaca], fill=KACA)
    d.line([Zp(136, 133), Zp(164, 147)], fill=KAYU_GELAP, width=Z)
    d.line([Zp(150, 127), Zp(150, 153)], fill=KAYU_GELAP, width=Z)
    d.line([Zp(139, 124), Zp(150, 119)], fill=KACA_TERANG, width=2 * Z)
    # kotak bunga di bawah jendela
    d.polygon([Zp(132, 166), Zp(168, 148), Zp(168, 158), Zp(132, 176)],
              fill=(122, 74, 43, 255))
    for (bx, by) in [(140, 158), (150, 153), (160, 148)]:
        d.ellipse([Zp(bx - 4, by - 4), Zp(bx + 4, by + 4)],
                  fill=(255, 143, 179, 255), outline=OL, width=Z)

    # -- fasad depan kanan: dua jendela kecil --
    for (x0, y0) in [(160, 158), (188, 144)]:
        pts = [(x0, y0), (x0 + 22, y0 + 11), (x0 + 22, y0 + 27),
               (x0, y0 + 16)]
        d.polygon([Zp(*p) for p in pts], fill=KACA)
        d.line([Zp(*p) for p in pts] + [Zp(*pts[0])], fill=OL, width=Z)

    # -- semak & bunga di sekitar pintu --
    for (sx, sy, r) in [(52, 162, 12), (64, 170, 14), (116, 178, 12)]:
        d.ellipse([Zp(sx - r, sy - r), Zp(sx + r, sy + r)],
                  fill=(92, 168, 74, 255), outline=(60, 120, 52, 255), width=Z)
        d.ellipse([Zp(sx - r + 4, sy - r + 3), Zp(sx, sy + 2)],
                  fill=(118, 192, 92, 255))
    for (fx, fy) in [(46, 156), (76, 174), (110, 184)]:
        d.ellipse([Zp(fx - 3, fy - 3), Zp(fx + 3, fy + 3)],
                  fill=(255, 190, 92, 255), outline=OL, width=Z)

    simpan(cv.resize((256, 256), Image.Resampling.LANCZOS), "rumah.png")


rumah_detail()

# ---- warung ----
img, d = baru()
bayangan(d, 128, 172, 88, 13)
poli(d, [(48, 128), (128, 168), (128, 96), (48, 56)], (255, 243, 214, 255))
poli(d, [(208, 128), (128, 168), (128, 96), (208, 56)], (232, 200, 143, 255))
poli(d, [(48, 56), (128, 96), (208, 56), (128, 16)], (91, 122, 153, 255))
garis_genteng(d, (128, 16), (48, 56), (208, 56))
# papan nama putih + keranjang kecil di atasnya
papan_nama(d, [(64, 84), (112, 108), (112, 124), (64, 100)])
d.polygon([(78, 100), (98, 110), (98, 118), (78, 108)], fill=(224, 120, 64, 255))
d.line([(78, 100), (98, 110), (98, 118), (78, 108), (78, 100)],
       fill=OL, width=2)
for (x, y) in [(84, 103), (92, 107)]:
    d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(70, 168, 78, 255))
# pintu + jendela etalase (kanan) berisi barang warna-warni
pintu_iso(d, 76, 118, 100, 156)
jendela_iso(d, 152, 108, 184, 138)
for (x, y, c) in [(158, 128, (224, 120, 64, 255)),
                  (168, 124, (70, 168, 78, 255)),
                  (176, 121, (255, 209, 102, 255))]:
    d.ellipse([x - 3, y - 3, x + 3, y + 3], fill=c)
# awning garis-garis di tepi bawah dinding kiri
for i in range(4):
    t0, t1 = i / 4.0, (i + 1) / 4.0
    atas0 = (48 + 80 * t0, 108 + 40 * t0)
    atas1 = (48 + 80 * t1, 108 + 40 * t1)
    bawah1 = (48 + 80 * t1, 128 + 40 * t1)
    bawah0 = (48 + 80 * t0, 128 + 40 * t0)
    warna = (224, 120, 64, 255) if i % 2 == 0 else (255, 255, 255, 255)
    d.polygon([atas0, atas1, bawah1, bawah0], fill=warna)
    d.line([atas0, atas1, bawah1, bawah0, atas0], fill=OL, width=1)
# lampion gantung di sudut kanan atap
d.line([(200, 56), (200, 72)], fill=OL, width=2)
d.ellipse([192, 72, 208, 88], fill=(255, 120, 60, 255), outline=OL, width=2)
simpan(img, "warung.png")

# ---- taman ----
img, d = baru()
d.polygon(DIAMOND, fill=(111, 191, 74, 255))
d.line(DIAMOND + [DIAMOND[0]], fill=(78, 154, 51, 255), width=3)
# jalan setapak vertikal + tepi batu
d.polygon([(120, 78), (136, 78), (136, 178), (120, 178)],
          fill=(232, 217, 168, 255))
d.line([(120, 78), (120, 178)], fill=(180, 165, 120, 255), width=2)
d.line([(136, 78), (136, 178)], fill=(180, 165, 120, 255), width=2)


def pohon(x, y):
    # batang + 2 lapis tajuk (terang atas, teduh bawah) + outline
    d.rectangle([x - 3, y, x + 3, y + 14], fill=KAYU_GELAP, outline=OL, width=1)
    d.ellipse([x - 16, y - 26, x + 16, y + 2], fill=(47, 143, 58, 255),
              outline=OL, width=2)
    d.ellipse([x - 11, y - 36, x + 11, y - 14], fill=(70, 168, 78, 255),
              outline=OL, width=2)
    d.ellipse([x - 8, y - 32, x + 2, y - 22], fill=(120, 200, 110, 255))


pohon(84, 110)
pohon(172, 112)
pohon(128, 150)
# bunga + bangku kecil
for (x, y, c) in [(100, 140, (255, 143, 179, 255)),
                  (156, 142, (255, 209, 102, 255)),
                  (128, 122, (255, 143, 179, 255)),
                  (108, 158, (255, 255, 255, 255))]:
    d.ellipse([x - 3, y - 3, x + 3, y + 3], fill=c, outline=OL, width=1)
d.polygon([(60, 150), (84, 162), (84, 168), (60, 156)], fill=KAYU,
          outline=OL, width=2)
d.line([(64, 156), (64, 166)], fill=OL, width=2)
d.line([(80, 164), (80, 174)], fill=OL, width=2)
simpan(img, "taman.png")

# ---- jalan (1x1): aspal modern rapi + trotoar terang (supersample 2x) ----
img_hi = Image.new("RGBA", (W * 2, H * 2), (0, 0, 0, 0))
dh = ImageDraw.Draw(img_hi)
DIAMOND_HI = [(x * 2, y * 2) for (x, y) in DIAMOND]
dh.polygon(DIAMOND_HI, fill=(214, 220, 226, 255))  # trotoar rapi di tepi
dh.polygon([(256, 172), (424, 256), (256, 340), (88, 256)],
           fill=(88, 98, 112, 255))  # aspal halus
dh.line(DIAMOND_HI + [DIAMOND_HI[0]], fill=(165, 174, 184, 255), width=3)
# garis marka tengah putus-putus
for t in range(3):
    x0 = 196 + t * 44
    y0 = 286 - t * 22
    dh.line([(x0, y0), (x0 + 22, y0 - 11)], fill=(255, 214, 102, 255), width=5)
img = img_hi.resize((W, H), Image.Resampling.LANCZOS)
simpan(img, "jalan.png")

# ---- toko kelontong: seperti warung, hijau + rak depan + pintu kaca ----
img, d = baru()
bayangan(d, 128, 172, 88, 13)
poli(d, [(48, 128), (128, 168), (128, 96), (48, 56)], (232, 245, 220, 255))
poli(d, [(208, 128), (128, 168), (128, 96), (208, 56)], (200, 225, 175, 255))
poli(d, [(48, 56), (128, 96), (208, 56), (128, 16)], (46, 125, 70, 255))
garis_genteng(d, (128, 16), (48, 56), (208, 56))
# papan nama hijau tua + daun
papan_nama(d, [(64, 84), (112, 108), (112, 124), (64, 100)],
           isi=(46, 125, 70, 255))
d.ellipse([82, 100, 96, 110], fill=(120, 200, 110, 255), outline=PUTIH, width=1)
# pintu kaca (kiri) + rak barang (kanan)
poli(d, [(76, 118), (100, 130), (100, 156), (76, 144)], KACA, garis=OL, lebar=2)
d.line([(88, 124), (88, 150)], fill=KAYU_GELAP, width=2)
for (x0, y0) in [(150, 108), (168, 118), (150, 128)]:
    poli(d, [(x0, y0), (x0 + 16, y0 - 8), (x0 + 16, y0 + 8),
             (x0, y0 + 16)], (224, 120, 64, 255), garis=OL, lebar=2)
# awning hijau/putih
for i in range(4):
    t0, t1 = i / 4.0, (i + 1) / 4.0
    atas0 = (48 + 80 * t0, 108 + 40 * t0)
    atas1 = (48 + 80 * t1, 108 + 40 * t1)
    bawah1 = (48 + 80 * t1, 128 + 40 * t1)
    bawah0 = (48 + 80 * t0, 128 + 40 * t0)
    warna = (46, 125, 70, 255) if i % 2 == 0 else (255, 255, 255, 255)
    d.polygon([atas0, atas1, bawah1, bawah0], fill=warna)
    d.line([atas0, atas1, bawah1, bawah0, atas0], fill=OL, width=1)
simpan(img, "toko.png")

# ---- perpustakaan: gedung biru tua + kolom + buku + tangga ----
img, d = baru()
bayangan(d, 128, 172, 88, 13)
poli(d, [(48, 128), (128, 168), (128, 96), (48, 56)], (235, 240, 250, 255))
poli(d, [(208, 128), (128, 168), (128, 96), (208, 56)], (205, 215, 235, 255))
poli(d, [(48, 56), (128, 96), (208, 56), (128, 16)], (40, 70, 130, 255))
garis_genteng(d, (128, 16), (48, 56), (208, 56))
# kolom putih di dinding kiri
for (x0, y0, x1, y1) in [(64, 64, 72, 68), (88, 76, 96, 80), (112, 88, 120, 92)]:
    d.polygon([(x0, y0), (x1, y1), (x1, y1 + 72), (x0, y0 + 72)],
              fill=(255, 255, 255, 255), outline=OL_HALUS)
# papan nama + buku terbuka di dinding kanan
papan_nama(d, [(150, 96), (190, 76), (190, 96), (150, 116)])
d.polygon([(162, 98), (170, 94), (170, 104), (162, 108)],
          fill=(224, 120, 64, 255), outline=OL)
d.polygon([(170, 94), (178, 98), (178, 108), (170, 104)],
          fill=(255, 209, 102, 255), outline=OL)
# jendela kecil kiri bawah
jendela_iso(d, 152, 122, 176, 144)
simpan(img, "perpustakaan.png")

# ---- pos keamanan: gardu kecil atap biru + palang + lampu ----
img, d = baru()
bayangan(d, 128, 172, 88, 13)
poli(d, [(48, 128), (128, 168), (128, 104), (48, 64)], (240, 240, 235, 255))
poli(d, [(208, 128), (128, 168), (128, 104), (208, 64)], (210, 210, 200, 255))
poli(d, [(48, 64), (128, 104), (208, 64), (128, 24)], (40, 90, 160, 255))
garis_genteng(d, (128, 24), (48, 64), (208, 64))
# jendela + pintu + papan "POS" tanpa teks: perisai kecil
jendela_iso(d, 152, 112, 176, 134)
pintu_iso(d, 76, 118, 100, 156)
d.polygon([(84, 84), (96, 90), (96, 102), (84, 96)], fill=(40, 90, 160, 255),
          outline=PUTIH, width=2)
# palang garis kuning-hitam di depan
for i, c in enumerate([(255, 209, 102, 255), (40, 40, 40, 255),
                       (255, 209, 102, 255), (40, 40, 40, 255)]):
    d.polygon([(108 + i * 12, 170), (120 + i * 12, 176),
               (120 + i * 12, 184), (108 + i * 12, 178)], fill=c,
              outline=OL, width=1)
# tiang lampu kecil di kanan
d.line([(196, 128), (196, 92)], fill=OL, width=3)
d.ellipse([190, 84, 202, 96], fill=EMAS, outline=OL, width=2)
simpan(img, "pos.png")

# ---- kafe: cokelat hangat + awning + cangkir + meja luar ----
img, d = baru()
bayangan(d, 128, 172, 88, 13)
poli(d, [(48, 128), (128, 168), (128, 96), (48, 56)], (255, 240, 214, 255))
poli(d, [(208, 128), (128, 168), (128, 96), (208, 56)], (232, 200, 150, 255))
poli(d, [(48, 56), (128, 96), (208, 56), (128, 16)], (120, 70, 40, 255))
garis_genteng(d, (128, 16), (48, 56), (208, 56))
# papan nama + cangkir kopi beruap
papan_nama(d, [(64, 84), (112, 108), (112, 124), (64, 100)])
d.polygon([(80, 100), (96, 108), (96, 118), (80, 110)], fill=(120, 70, 40, 255),
          outline=OL)
d.line([(96, 108), (102, 111), (102, 117), (96, 114)], fill=(120, 70, 40, 255),
       width=2)
d.line([(84, 96), (84, 92)], fill=(200, 200, 200, 255), width=2)
d.line([(90, 96), (90, 91)], fill=(200, 200, 200, 255), width=2)
# pintu + jendela
pintu_iso(d, 76, 118, 100, 156)
jendela_iso(d, 152, 108, 180, 136)
# meja bundar + kursi di depan kanan
d.ellipse([186, 146, 206, 158], fill=KAYU, outline=OL, width=2)
d.line([(196, 158), (196, 168)], fill=OL, width=2)
d.ellipse([176, 156, 186, 164], fill=(78, 122, 81, 255), outline=OL, width=2)
# awning oranye/putih
for i in range(4):
    t0, t1 = i / 4.0, (i + 1) / 4.0
    atas0 = (48 + 80 * t0, 108 + 40 * t0)
    atas1 = (48 + 80 * t1, 108 + 40 * t1)
    bawah1 = (48 + 80 * t1, 128 + 40 * t1)
    bawah0 = (48 + 80 * t0, 128 + 40 * t0)
    warna = (224, 120, 64, 255) if i % 2 == 0 else (255, 255, 255, 255)
    d.polygon([atas0, atas1, bawah1, bawah0], fill=warna)
    d.line([atas0, atas1, bawah1, bawah0, atas0], fill=OL, width=1)
simpan(img, "kafe.png")

# ================= BANGUNAN 2x2 (kanvas 512x256) =================
# Helper gedung 2x2 (pola kost Fase 3): alas rumput diamond, 2 dinding,
# atap piramida, jendela, pintu. Kartun: outline di semua sisi,
# bayangan tanah, garis genteng, dan pemisah lantai.

def jendela2(d, x0, y0, x1, y1):
    w2 = (x1 - x0) // 2
    titik = [(x0, y0), (x1, y0 + w2), (x1, y1), (x0, y1 - w2)]
    d.polygon(titik, fill=KACA)
    d.line(titik + [titik[0]], fill=KAYU_GELAP, width=2)
    d.line([(x0, (y0 + y1) // 2), (x1, (y0 + y1) // 2 + w2)],
           fill=KAYU_GELAP, width=2)
    d.line([(x0 + 4, y0 + w2 // 2 + 2), (x0 + w2, y0 + 2)],
           fill=KACA_TERANG, width=2)


def gedung2x2(nama, kiri, kanan, atap_kiri, atap_kanan, jendela,
              pintu=True, pemisah=(), ekstra=None, cerobong_di=None):
    img = Image.new("RGBA", (1024, 512), (0, 0, 0, 0))
    d = Draw2x(img)
    # bayangan tanah lembut berlapis di bawah alas
    d.ellipse([66, 218, 446, 248], fill=(0, 0, 0, 30))
    d.ellipse([86, 224, 426, 242], fill=(0, 0, 0, 40))
    poli(d, [(256, 28), (456, 128), (256, 228), (56, 128)], RUMPUT_TERANG,
         garis=RUMPUT_GELAP, lebar=2)
    poli(d, [(66, 128), (256, 218), (256, 108), (66, 18)], kiri, lebar=2)
    poli(d, [(446, 128), (256, 218), (256, 108), (446, 18)], kanan, lebar=2)
    # bayangan atap (ambient occlusion) di bagian atas kedua dinding
    d.polygon([(66, 18), (256, 108), (256, 122), (66, 32)], fill=(0, 0, 0, 35))
    d.polygon([(446, 18), (256, 108), (256, 122), (446, 32)], fill=(0, 0, 0, 45))
    for y in pemisah:
        d.line([(66, y), (256, y + 90)], fill=(190, 155, 115, 255), width=2)
        d.line([(446, y), (256, y + 90)], fill=(190, 155, 115, 255), width=2)
    poli(d, [(66, 18), (256, 108), (256, 8)], atap_kiri, lebar=2)
    poli(d, [(446, 18), (256, 108), (256, 8)], atap_kanan, lebar=2)
    # kilau tepi atap atas
    d.line([(256, 8), (256, 108)], fill=(255, 255, 255, 70), width=2)
    garis_genteng(d, (256, 8), (66, 18), (446, 18))
    if cerobong_di is not None:
        cerobong(d, cerobong_di[0], cerobong_di[1], w=20, h=30)
    for (x0, y0, x1, y1) in jendela:
        jendela2(d, x0, y0, x1, y1)
    if pintu:
        pintu_iso(d, 100, 130, 132, 196)
        d.polygon([(96, 196), (136, 212), (136, 218), (96, 202)],
                  fill=(210, 210, 205, 255), outline=OL_HALUS)
    if ekstra is not None:
        ekstra(d)
    simpan2x(img, nama, 512, 256)


JENDELA_KOST = [(100, 44, 132, 66), (168, 78, 200, 100),
                (312, 78, 344, 100), (380, 44, 412, 66),
                (312, 148, 344, 170), (380, 114, 412, 136)]

# ---- kost: dua lantai + balkon lantai 2 + papan nama + bunga ----
def ekstra_kost(d):
    # balkon lantai 2 (dinding kanan): lantai + pagar
    d.polygon([(300, 130), (420, 70), (420, 80), (300, 140)],
              fill=(180, 130, 70, 255), outline=OL, width=2)
    for x in range(308, 420, 18):
        d.line([(x, 130 - (x - 300) // 2), (x, 118 - (x - 300) // 2)],
               fill=OL, width=2)
    # papan nama putih di dinding kanan lantai 2
    papan_nama(d, [(330, 60), (400, 95), (400, 112), (330, 77)])
    d.ellipse([358, 78, 372, 88], fill=(224, 120, 64, 255), outline=OL, width=1)
    # kotak bunga di jendela lantai 1
    for (x0, y0) in [(312, 170), (380, 136)]:
        d.polygon([(x0, y0), (x0 + 32, y0 + 16), (x0 + 32, y0 + 24),
                   (x0, y0 + 8)], fill=KAYU_GELAP, outline=OL, width=1)
        d.ellipse([x0 + 6, y0 - 2, x0 + 12, y0 + 4],
                  fill=(255, 143, 179, 255), outline=OL, width=1)
        d.ellipse([x0 + 18, y0 + 4, x0 + 24, y0 + 10],
                  fill=(255, 209, 102, 255), outline=OL, width=1)


gedung2x2("kost.png", (255, 236, 205, 255), (240, 200, 150, 255),
          (140, 60, 90, 255), (168, 72, 100, 255), JENDELA_KOST,
          pemisah=(73,), ekstra=ekstra_kost, cerobong_di=(380, 30))

# ---- sekolah dasar: kuning + jam + tiang bendera merah-putih ----
def ekstra_sekolah(d):
    d.ellipse([336, 84, 372, 112], fill=(255, 255, 255, 255),
              outline=OL, width=2)
    d.line([(354, 98), (354, 88)], fill=OL, width=2)
    d.line([(354, 98), (362, 102)], fill=OL, width=2)
    # tiang bendera di puncak atap
    d.line([(256, 8), (256, -2 + 8)], fill=OL, width=2)
    d.polygon([(256, 2), (286, 8), (256, 14)], fill=(200, 40, 40, 255),
              outline=OL, width=1)
    d.polygon([(256, 14), (286, 20), (256, 26)], fill=(255, 255, 255, 255),
              outline=OL, width=1)


gedung2x2("sekolah.png", (255, 236, 180, 255), (245, 205, 130, 255),
          (180, 50, 50, 255), (200, 65, 65, 255), JENDELA_KOST,
          ekstra=ekstra_sekolah, cerobong_di=(380, 30))

# ---- pasar: awning merah + peti buah warna-warni ----
def ekstra_pasar(d):
    for i in range(4):
        t0, t1 = i / 4.0, (i + 1) / 4.0
        atas0 = (66 + 190 * t0, 108 + 90 * t0)
        atas1 = (66 + 190 * t1, 108 + 90 * t1)
        bawah1 = (66 + 190 * t1, 128 + 90 * t1)
        bawah0 = (66 + 190 * t0, 128 + 90 * t0)
        warna = (200, 60, 60, 255) if i % 2 == 0 else (255, 255, 255, 255)
        d.polygon([atas0, atas1, bawah1, bawah0], fill=warna)
        d.line([atas0, atas1, bawah1, bawah0, atas0], fill=OL, width=1)
    # peti dagangan berisi buah di dinding kanan bawah
    for (x0, y0, buah) in [(320, 150, (255, 140, 40, 255)),
                           (350, 165, (120, 200, 80, 255)),
                           (320, 175, (220, 60, 60, 255))]:
        d.polygon([(x0, y0), (x0 + 24, y0 + 12), (x0 + 24, y0 + 26),
                   (x0, y0 + 14)], fill=(180, 130, 70, 255), outline=OL, width=2)
        for (fx, fy) in [(x0 + 6, y0 + 6), (x0 + 14, y0 + 10), (x0 + 10, y0 + 2)]:
            d.ellipse([fx - 3, fy - 3, fx + 3, fy + 3], fill=buah, outline=OL, width=1)


gedung2x2("pasar.png", (250, 240, 210, 255), (230, 210, 170, 255),
          (46, 125, 70, 255), (60, 145, 85, 255), JENDELA_KOST,
          ekstra=ekstra_pasar)

# ---- apartemen: 3 lantai + balkon tiap lantai + pintu kaca ----
JENDELA_APARTEMEN = JENDELA_KOST + [
    (100, 87, 132, 109), (168, 121, 200, 143),
    (312, 121, 344, 143), (380, 87, 412, 109),
    (168, 164, 200, 186)]


def ekstra_apartemen(d):
    # balkon lantai 2 & 3 (dinding kanan): lantai + pagar
    for y0 in (96, 152):
        d.polygon([(300, y0), (420, y0 - 60), (420, y0 - 52), (300, y0 + 8)],
                  fill=(180, 130, 70, 255), outline=OL, width=2)
        for x in range(308, 420, 18):
            d.line([(x, y0 - (x - 300) // 2), (x, y0 - 12 - (x - 300) // 2)],
                   fill=OL, width=2)


gedung2x2("apartemen.png", (215, 225, 240, 255), (185, 200, 220, 255),
          (70, 90, 120, 255), (90, 110, 145, 255), JENDELA_APARTEMEN,
          pemisah=(43, 103), ekstra=ekstra_apartemen)

# ---- klinik: putih + palang merah + taman kecil depan ----
def ekstra_klinik(d):
    d.polygon([(340, 90), (352, 96), (352, 140), (340, 134)],
              fill=(200, 40, 40, 255), outline=OL, width=2)
    d.polygon([(320, 108), (372, 134), (372, 146), (320, 120)],
              fill=(200, 40, 40, 255), outline=OL, width=2)
    # semak hijau di depan dinding kiri
    for (x, y) in [(90, 200), (130, 210), (170, 205)]:
        d.ellipse([x - 12, y - 10, x + 12, y + 6], fill=(70, 168, 78, 255),
                  outline=OL, width=2)


gedung2x2("klinik.png", (250, 250, 250, 255), (225, 225, 225, 255),
          (120, 120, 120, 255), (140, 140, 140, 255), JENDELA_KOST,
          ekstra=ekstra_klinik)

# ---- pembangkit listrik: abu gelap + petir + cerobong asap ----
def ekstra_pembangkit(d):
    d.polygon([(330, 80), (352, 80), (340, 105), (360, 105),
               (325, 150), (336, 115), (320, 115)],
              fill=(255, 209, 102, 255), outline=(150, 100, 0, 255), width=2)
    # cerobong asap di sisi kanan atap (kepala cerobong di y=18,
    # asap di atasnya masih di dalam kanvas)
    d.rectangle([372, 18, 392, 52], fill=(90, 90, 95, 255), outline=OL, width=2)
    d.ellipse([374, 4, 388, 14], fill=(220, 220, 220, 200))
    d.ellipse([382, 0, 396, 10], fill=(220, 220, 220, 150))


gedung2x2("pembangkit.png", (170, 175, 185, 255), (140, 148, 160, 255),
          (60, 60, 70, 255), (80, 80, 90, 255), JENDELA_KOST,
          ekstra=ekstra_pembangkit)

# ---- BALAI KOTA (baru): gedung berwibawa 2x2 — kolom, pedimen
# segitiga, jam dinding, tiang bendera, tangga lebar, atap kubah kecil.
def ekstra_balaikota(d):
    # pedimen segitiga putih di atas dinding kanan (tengah atap)
    d.polygon([(300, 96), (380, 56), (440, 86), (360, 116)],
              fill=(250, 245, 230, 255), outline=OL, width=3)
    d.ellipse([362, 78, 378, 92], fill=EMAS, outline=OL, width=2)
    # kolom putih di dinding kiri (4 kolom mengikuti kemiringan)
    for (x0, y0) in [(90, 52), (130, 72), (170, 92), (210, 112)]:
        d.polygon([(x0, y0), (x0 + 14, y0 + 7), (x0 + 14, y0 + 96),
                   (x0, y0 + 89)], fill=(255, 255, 255, 255),
                  outline=OL_HALUS, width=2)
        d.polygon([(x0 - 3, y0 - 4), (x0 + 17, y0 + 3), (x0 + 17, y0 + 9),
                   (x0 - 3, y0 + 2)], fill=(230, 225, 210, 255), outline=OL, width=1)
    # jam dinding bundar di dinding kanan
    d.ellipse([336, 116, 366, 140], fill=(255, 255, 255, 255),
              outline=OL, width=2)
    d.line([(351, 128), (351, 120)], fill=OL, width=2)
    d.line([(351, 128), (358, 131)], fill=OL, width=2)
    # tiang bendera merah-putih di puncak atap (tiang 6px, bendera
    # penuh di dalam kanvas y=2..18 supaya tidak terpotong)
    d.line([(256, 8), (256, 2)], fill=OL, width=3)
    d.polygon([(256, 2), (286, 6), (256, 10)], fill=(200, 40, 40, 255),
              outline=OL, width=1)
    d.polygon([(256, 10), (286, 14), (256, 18)], fill=(255, 255, 255, 255),
              outline=OL, width=1)
    # semak + bunga di depan
    for (x, y) in [(150, 205), (420, 130)]:
        d.ellipse([x - 14, y - 10, x + 14, y + 6], fill=(70, 168, 78, 255),
                  outline=OL, width=2)
        d.ellipse([x - 3, y - 12, x + 3, y - 6], fill=(255, 143, 179, 255),
                  outline=OL, width=1)


gedung2x2("balaikota.png", (250, 244, 224, 255), (232, 220, 190, 255),
          (90, 110, 150, 255), (110, 130, 175, 255), JENDELA_KOST,
          pemisah=(73,), ekstra=ekstra_balaikota)

# ================= BANGUNAN 3x3 (kanvas 768x384) =================
# Helper gedung 3x3 (pola 2x2 diskala 1,5x) + outline, bayangan,
# genteng, dan pemisah lantai.

def s15(p):
    return (int(p[0] * 1.5), int(p[1] * 1.5))


def gedung3x3(nama, kiri, kanan, atap_kiri, atap_kanan, jendela,
              pintu=True, pemisah=(), ekstra=None):
    img = Image.new("RGBA", (1536, 768), (0, 0, 0, 0))
    d = Draw2x(img)
    d.ellipse([104, 330, 664, 368], fill=(0, 0, 0, 30))
    d.ellipse([130, 336, 638, 362], fill=(0, 0, 0, 40))
    poli(d, [s15(p) for p in [(256, 28), (456, 128), (256, 228), (56, 128)]],
         RUMPUT_TERANG, garis=RUMPUT_GELAP, lebar=2)
    poli(d, [s15(p) for p in [(66, 128), (256, 218), (256, 108), (66, 18)]],
         kiri, lebar=2)
    poli(d, [s15(p) for p in [(446, 128), (256, 218), (256, 108), (446, 18)]],
         kanan, lebar=2)
    d.polygon([s15(p) for p in [(66, 18), (256, 108), (256, 120), (66, 30)]], fill=(0, 0, 0, 35))
    d.polygon([s15(p) for p in [(446, 18), (256, 108), (256, 120), (446, 30)]], fill=(0, 0, 0, 45))
    for y in pemisah:
        d.line([s15((66, y)), s15((256, y + 90))],
               fill=(190, 155, 115, 255), width=2)
        d.line([s15((446, y)), s15((256, y + 90))],
               fill=(190, 155, 115, 255), width=2)
    poli(d, [s15(p) for p in [(66, 18), (256, 108), (256, 8)]], atap_kiri, lebar=2)
    poli(d, [s15(p) for p in [(446, 18), (256, 108), (256, 8)]], atap_kanan, lebar=2)
    d.line([s15((256, 8)), s15((256, 108))], fill=(255, 255, 255, 70), width=2)
    garis_genteng(d, s15((256, 8)), s15((66, 18)), s15((446, 18)))
    for (x0, y0, x1, y1) in jendela:
        jendela2(d, int(x0 * 1.5), int(y0 * 1.5), int(x1 * 1.5), int(y1 * 1.5))
    if pintu:
        pintu_iso(d, 150, 195, 198, 294)
        d.polygon([s15(p) for p in [(100, 196), (136, 212), (136, 218), (100, 202)]],
                  fill=(210, 210, 205, 255), outline=OL_HALUS)
    if ekstra is not None:
        ekstra(d)
    simpan2x(img, nama, 768, 384)


# ---- perumahan: blok hunian besar 2 lantai + pohon depan ----
def ekstra_perumahan(d):
    for (x, y) in [(170, 320), (600, 250)]:
        d.rectangle([x - 5, y, x + 5, y + 22], fill=KAYU_GELAP, outline=OL, width=1)
        d.ellipse([x - 26, y - 42, x + 26, y + 4], fill=(47, 143, 58, 255),
                  outline=OL, width=2)
        d.ellipse([x - 18, y - 56, x + 18, y - 22], fill=(70, 168, 78, 255),
                  outline=OL, width=2)


gedung3x3("perumahan.png", (255, 230, 200, 255), (240, 195, 145, 255),
          (198, 90, 58, 255), (217, 108, 63, 255),
          JENDELA_KOST + [(100, 87, 132, 109), (168, 121, 200, 143),
                          (312, 121, 344, 143), (380, 87, 412, 109)],
          pemisah=(73,), ekstra=ekstra_perumahan)

# ---- kampus kecil: marmer putih + kubah + kolom + tangga ----
def ekstra_kampus(d):
    d.ellipse([344, 0, 424, 56], fill=(90, 140, 200, 255),
              outline=OL, width=3)
    # jendela kecil di kubah
    d.ellipse([374, 22, 394, 42], fill=KACA, outline=OL, width=2)
    for (x0, y0) in [(150, 66), (210, 96), (270, 126)]:
        d.polygon([s15((x0, y0)), s15((x0 + 12, y0 + 6)),
                   s15((x0 + 12, y0 + 108)), s15((x0, y0 + 102))],
                  fill=(255, 255, 255, 255), outline=OL_HALUS, width=2)


gedung3x3("kampus.png", (245, 245, 240, 255), (220, 220, 215, 255),
          (90, 110, 150, 255), (110, 130, 175, 255),
          JENDELA_KOST, ekstra=ekstra_kampus)

# ---- alun-alun kota: plaza + air mancur + pohon + bangku + lampu ----
img = Image.new("RGBA", (768, 384), (0, 0, 0, 0))
d = ImageDraw.Draw(img)
d.ellipse([84, 330, 684, 362], fill=(0, 0, 0, 45))
poli(d, [(384, 42), (684, 192), (384, 342), (84, 192)], (200, 200, 195, 255))
d.line([(234, 117), (534, 267)], fill=(170, 170, 165, 255), width=3)
d.line([(534, 117), (234, 267)], fill=(170, 170, 165, 255), width=3)
# ubin kecil di sudut plaza
for (x, y) in [(200, 150), (560, 150), (200, 230), (560, 230)]:
    d.polygon([(x, y), (x + 40, y + 20), (x, y + 40), (x - 40, y + 20)],
              fill=(215, 215, 210, 255), outline=(170, 170, 165, 255))
# air mancur (kolam + semburan + tetesan)
d.ellipse([314, 152, 454, 232], fill=(120, 180, 230, 255),
          outline=(255, 255, 255, 255), width=4)
d.ellipse([344, 172, 424, 212], fill=(90, 150, 210, 255),
          outline=(255, 255, 255, 255), width=2)
for (x, y) in [(384, 140), (364, 150), (404, 150), (374, 132), (394, 132)]:
    d.ellipse([x - 4, y - 4, x + 4, y + 4], fill=(255, 255, 255, 255))
d.line([(384, 150), (384, 172)], fill=(255, 255, 255, 255), width=3)


def pohon3(x, y):
    d.rectangle([x - 5, y, x + 5, y + 22], fill=KAYU_GELAP, outline=OL, width=1)
    d.ellipse([x - 26, y - 42, x + 26, y + 4], fill=(47, 143, 58, 255),
              outline=OL, width=2)
    d.ellipse([x - 18, y - 56, x + 18, y - 22], fill=(70, 168, 78, 255),
              outline=OL, width=2)
    d.ellipse([x - 14, y - 50, x + 2, y - 34], fill=(120, 200, 110, 255))


pohon3(180, 180)
pohon3(588, 180)
pohon3(384, 290)
# bangku + tiang lampu
for (x0, y0) in [(250, 240), (470, 240)]:
    d.polygon([(x0, y0), (x0 + 48, y0 + 24), (x0 + 48, y0 + 34),
               (x0, y0 + 10)], fill=(150, 100, 50, 255), outline=OL, width=2)
for (x, y) in [(140, 200), (620, 200)]:
    d.line([(x, y), (x, y - 46)], fill=OL, width=3)
    d.ellipse([x - 7, y - 54, x + 7, y - 40], fill=EMAS, outline=OL, width=2)
simpan(img, "alun-alun.png")

# ================= WARGA (256x256) =================
# Chibi kartun proper: lengan + tangan, wajah hidup (mata berkilau,
# senyum, pipi merona), sepatu, jambul rambut. Tiap varian punya
# 3 gambar: diam (kaki rapat) + frame jalan A (kaki kiri maju) +
# frame jalan B (kaki kanan maju) — PejalanKaki bergantian A/B
# saat berjalan, diam saat istirahat.

def gambar_warga(d, warna_baju, pose):
    # pose: "diam" | "a" | "b"
    bayangan(d, 128, 215, 32, 8, alpha=50)
    # --- kaki + sepatu (posisi berbeda per pose) ---
    if pose == "diam":
        kaki = [(116, 186, 126, 208), (130, 186, 140, 208)]
        sepatu = [(112, 206, 128, 214), (128, 206, 144, 214)]
    elif pose == "a":
        kaki = [(110, 184, 120, 210), (132, 188, 142, 204)]
        sepatu = [(104, 208, 122, 216), (130, 202, 146, 210)]
    else:  # "b"
        kaki = [(114, 188, 124, 204), (136, 184, 146, 210)]
        sepatu = [(110, 202, 126, 210), (134, 208, 152, 216)]
    for (x0, y0, x1, y1) in kaki:
        d.rectangle([x0, y0, x1, y1], fill=(64, 64, 78, 255), outline=OL, width=2)
    for (x0, y0, x1, y1) in sepatu:
        d.ellipse([x0, y0, x1, y1], fill=(40, 40, 55, 255), outline=OL, width=2)
    # --- badan (trapesium baju) + kerah + kancing ---
    d.polygon([(112, 152), (144, 152), (150, 190), (106, 190)],
              fill=warna_baju, outline=OL, width=2)
    d.polygon([(122, 152), (134, 152), (128, 162)], fill=PUTIH, outline=OL, width=1)
    d.ellipse([126, 166, 130, 170], fill=PUTIH, outline=OL, width=1)
    d.ellipse([126, 176, 130, 180], fill=PUTIH, outline=OL, width=1)
    # saku kecil di dada kiri
    d.rectangle([116, 168, 124, 176], fill=(255, 255, 255, 90), outline=OL, width=1)
    # --- lengan + tangan kulit (ayun berlawanan dengan kaki) ---
    if pose == "diam":
        lengan_kiri = [(112, 156), (104, 180), (110, 184), (118, 160)]
        lengan_kanan = [(144, 156), (152, 180), (146, 184), (138, 160)]
        tangan_kiri, tangan_kanan = (107, 183), (149, 183)
    elif pose == "a":
        lengan_kiri = [(112, 156), (100, 176), (106, 182), (118, 160)]
        lengan_kanan = [(144, 156), (154, 178), (148, 182), (138, 160)]
        tangan_kiri, tangan_kanan = (103, 180), (151, 180)
    else:
        lengan_kiri = [(112, 156), (102, 178), (108, 182), (118, 160)]
        lengan_kanan = [(144, 156), (156, 176), (150, 182), (138, 160)]
        tangan_kiri, tangan_kanan = (105, 180), (153, 180)
    d.polygon(lengan_kiri, fill=warna_baju, outline=OL, width=2)
    d.polygon(lengan_kanan, fill=warna_baju, outline=OL, width=2)
    d.ellipse([tangan_kiri[0] - 4, tangan_kiri[1] - 4,
               tangan_kiri[0] + 4, tangan_kiri[1] + 4],
              fill=KULIT, outline=OL, width=1)
    d.ellipse([tangan_kanan[0] - 4, tangan_kanan[1] - 4,
               tangan_kanan[0] + 4, tangan_kanan[1] + 4],
              fill=KULIT, outline=OL, width=1)
    # --- kepala + rambut + wajah ---
    d.ellipse([112, 118, 144, 150], fill=KULIT, outline=OL, width=2)
    d.pieslice([112, 112, 144, 148], 180, 360, fill=RAMBUT, outline=OL, width=2)
    # jambul kecil di atas kepala
    d.line([(128, 114), (132, 106)], fill=RAMBUT, width=3)
    d.line([(120, 116), (116, 110)], fill=RAMBUT, width=2)
    # mata berkilau + senyum + pipi merona
    for (x, y) in [(121, 134), (135, 134)]:
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(40, 40, 50, 255))
        d.ellipse([x, y - 2, x + 1, y - 1], fill=PUTIH)
    d.arc([121, 136, 135, 148], 20, 160, fill=(120, 60, 40, 255), width=2)
    d.ellipse([113, 140, 119, 145], fill=(255, 150, 150, 140))
    d.ellipse([137, 140, 143, 145], fill=(255, 150, 150, 140))


def warga(nama, warna_baju):
    for pose, akhir in [("diam", ""), ("a", "-a"), ("b", "-b")]:
        img, d = baru()
        gambar_warga(d, warna_baju, pose)
        simpan(img, nama.replace(".png", akhir + ".png"))


warga("warga1.png", (224, 90, 70, 255))    # baju merah
warga("warga2.png", (70, 130, 220, 255))   # baju biru
warga("warga3.png", (240, 190, 60, 255))   # baju kuning

# ================= INTERIOR (denah 4 ruangan) =================
# Denah: diamond alas dibagi salib menjadi 4 segitiga — atas Kamar
# Tidur, kanan Dapur, bawah Ruang Tamu, kiri Kamar Mandi. Kartun:
# pola lantai per ruangan (karpet, ubin, papan kayu), dinding luar
# tebal + salib pemisah, label ruangan.

from PIL import ImageFont


def font_label(ukuran):
    try:
        return ImageFont.load_default(size=ukuran)
    except TypeError:
        return ImageFont.load_default()


def _mask_segitiga(size, poly):
    m = Image.new("L", size, 0)
    ImageDraw.Draw(m).polygon(poly, fill=255)
    return m


def _tempel_gradien(img, poly, warna_atas, warna_bawah):
    """Isi poligon dengan gradien vertikal lembut (atas -> bawah)."""
    mask = _mask_segitiga(img.size, poly)
    bbox = mask.getbbox()
    if bbox is None:
        return
    grad = Image.new("RGBA", img.size, (0, 0, 0, 0))
    gd = ImageDraw.Draw(grad)
    y0, y1 = bbox[1], bbox[3]
    for y in range(y0, y1 + 1):
        t = (y - y0) / max(1, y1 - y0)
        c = tuple(int(a + (b - a) * t) for a, b in zip(warna_atas, warna_bawah))
        gd.line([(0, y), (img.size[0], y)], fill=c)
    img.paste(grad, (0, 0), mask)
    return mask


def _tempel_pola(img, mask, gambar_pola):
    """Gambar pola di lapisan terpisah lalu di-clip ke mask ruangan."""
    lapisan = Image.new("RGBA", img.size, (0, 0, 0, 0))
    gambar_pola(ImageDraw.Draw(lapisan))
    lapisan.putalpha(Image.composite(lapisan.split()[3], Image.new("L", img.size, 0), mask))
    img.alpha_composite(lapisan)


def denah_interior(nama, w, h, cx, cy, rx, ry):
    """Denah 4 ruangan bergaya kartun: digambar di kanvas 2x lalu
    di-downsample LANCZOS (tepi halus), lantai gradien per ruangan,
    pola lantai (karpet, ubin, papan kayu) yang rapi di dalam ruangan,
    band dinding kayu dengan bayangan, dan label dalam pil putih."""
    S = 2
    img = Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0))
    X = lambda v: int(v * S)
    atas = (X(cx), X(cy - ry))
    kanan = (X(cx + rx), X(cy))
    bawah = (X(cx), X(cy + ry))
    kiri = (X(cx - rx), X(cy))
    tengah = (X(cx), X(cy))
    luar = [atas, kanan, bawah, kiri]

    def ke_tengah(p, f):
        return (int(tengah[0] + (p[0] - tengah[0]) * f),
                int(tengah[1] + (p[1] - tengah[1]) * f))

    d = ImageDraw.Draw(img, "RGBA")

    # bayangan tanah lembut di bawah denah
    d.polygon([(p[0], p[1] + 10 * S) for p in luar], fill=(0, 0, 0, 60))
    # band dinding kayu luar (ruangan digambar lebih kecil ke arah tengah)
    d.polygon(luar, fill=(146, 104, 62, 255), outline=OL, width=3 * S)

    f_inset = 0.93
    a2, k2, b2, l2, t2 = (ke_tengah(p, f_inset) for p in (atas, kanan, bawah, kiri, tengah))

    ruangan = [
        ([a2, k2, t2], (255, 233, 214), (247, 202, 184)),   # kamar tidur
        ([k2, b2, t2], (223, 243, 226), (193, 227, 199)),   # dapur
        ([b2, l2, t2], (251, 233, 199), (231, 205, 162)),   # ruang tamu
        ([l2, a2, t2], (216, 240, 248), (183, 218, 233)),   # kamar mandi
    ]
    mask_ruangan = []
    for poly, c_atas, c_bawah in ruangan:
        mask_ruangan.append(_tempel_gradien(img, poly, c_atas, c_bawah))

    # -- pola lantai per ruangan (di-clip ke segitiga masing-masing) --
    # kamar tidur: karpet bulat lembut + bintik krem
    mask_kamar = mask_ruangan[0]
    def pola_kamar(pd):
        pd.ellipse([t2[0] - rx * S // 3, a2[1] + ry * S // 6,
                    t2[0] + rx * S // 3, a2[1] + ry * S // 2],
                   fill=(255, 176, 198, 255), outline=(214, 120, 148, 255),
                   width=2 * S)
        pd.ellipse([t2[0] - rx * S // 5, a2[1] + ry * S // 3,
                    t2[0] + rx * S // 5, a2[1] + ry * S // 2 - 6 * S],
                   fill=(255, 205, 220, 255))
    _tempel_pola(img, mask_kamar, pola_kamar)

    # dapur: ubin diagonal halus
    def pola_dapur(pd):
        langkah = 16 * S
        x0, x1 = k2[0] - rx * S, k2[0]
        for i in range(-4, 10):
            off = i * langkah
            pd.line([(x0 + off, b2[1]), (x0 + off + h * S, b2[1] - h * S)],
                    fill=(255, 255, 255, 70), width=S)
            pd.line([(x0 + off, b2[1] - h * S), (x0 + off + h * S, b2[1])],
                    fill=(255, 255, 255, 70), width=S)
    _tempel_pola(img, mask_ruangan[1], pola_dapur)

    # ruang tamu: papan kayu horizontal
    def pola_tamu(pd):
        langkah = 14 * S
        for y in range(b2[1] - int(ry * S), b2[1] + 1, langkah):
            pd.line([(0, y), (img.size[0], y)], fill=(178, 138, 92, 110), width=S)
    _tempel_pola(img, mask_ruangan[2], pola_tamu)

    # kamar mandi: kotak ubin kecil
    def pola_mandi(pd):
        langkah = 18 * S
        for x in range(l2[0], a2[0] + 1, langkah):
            pd.line([(x, 0), (x, img.size[1])], fill=(255, 255, 255, 80), width=S)
        for y in range(a2[1], b2[1] + 1, langkah):
            pd.line([(0, y), (img.size[0], y)], fill=(255, 255, 255, 80), width=S)
    _tempel_pola(img, mask_ruangan[3], pola_mandi)

    # -- dinding --
    garis = max(3, w // 110) * S
    # sambungan dinding dalam: titik bulat di persimpangan tengah
    d.ellipse([t2[0] - garis, t2[1] - garis, t2[0] + garis, t2[1] + garis],
              fill=KAYU_GELAP)
    d.line([a2, b2], fill=KAYU_GELAP, width=garis)
    d.line([l2, k2], fill=KAYU_GELAP, width=garis)
    # garis highlight dinding dalam (sisi terang)
    d.line([a2, k2, b2, l2, a2], fill=(196, 152, 104, 255), width=S)
    # outline luar tegas
    d.line(luar + [luar[0]], fill=OL, width=3 * S)

    # -- label ruangan dalam pil putih semi-transparan --
    f = font_label(max(12, w // 22) * S)
    for (teks, px, py) in [("KAMAR", X(cx), X(cy - ry / 2)),
                           ("DAPUR", X(cx + rx / 2), X(cy - 14)),
                           ("TAMU", X(cx), X(cy + ry / 2)),
                           ("MANDI", X(cx - rx / 2), X(cy - 14))]:
        lebar = d.textlength(teks, font=f)
        tinggi = f.size if hasattr(f, "size") else w // 22
        pad = 5 * S
        kotak = [px - lebar / 2 - pad, py - pad - 2 * S,
                 px + lebar / 2 + pad, py + tinggi + pad]
        d.rounded_rectangle(kotak, radius=tinggi, fill=(255, 255, 255, 215),
                            outline=OL_HALUS, width=S)
        d.text((px - lebar / 2, py - 2 * S), teks, font=f, fill=(74, 62, 52, 255))

    simpan2x(img, nama, w, h)


denah_interior("interior-rumah.png", 256, 256, 128, 128, 100, 50)
denah_interior("interior-kost.png", 512, 256, 256, 128, 200, 100)
denah_interior("interior-apartemen.png", 512, 256, 256, 128, 200, 100)
denah_interior("interior-perumahan.png", 768, 384, 384, 192, 300, 150)


# ================= INTERIOR CUTAWAY (gaya dollhouse) =================
# Bangunan tetap berdiri: dinding depan dibuka rendah, dinding belakang
# + atap terangkat, di dalamnya terlihat 4 ruangan berlantai warna dengan
# pola masing-masing (mirip referensi cutaway).

def cutaway_interior(nama, w, h, cx, cy, rx, ry):
    S = 2
    img = Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0))
    X = lambda v: int(v * S)
    L = (X(cx - rx), X(cy))            # sudut kiri
    R = (X(cx + rx), X(cy))            # sudut kanan
    T = (X(cx), X(cy - ry))            # sudut belakang (atas)
    B = (X(cx), X(cy + ry))            # sudut depan (bawah)
    C = (X(cx), X(cy))                 # tengah lantai
    lantai = [T, R, B, L]

    hw = int(ry * S * 0.9)             # tinggi dinding belakang
    fw = int(ry * S * 0.30) + 2 * S    # tinggi dinding depan rendah
    gap = int(ry * S * 0.25)           # celah angkat atap
    hr = int(ry * S * 0.9)             # tinggi atap (puncak)
    up = lambda p, dy: (p[0], p[1] - dy)

    d = ImageDraw.Draw(img, "RGBA")

    # -- bayangan tanah --
    d.ellipse([X(cx - rx) + 10 * S, X(cy) - ry * S // 5,
               X(cx + rx) - 10 * S, X(cy) + ry * S // 5 + 10 * S],
              fill=(0, 0, 0, 55))

    # -- lantai 4 ruangan (sedikit diinset ke tengah agar dinding rapi) --
    def ketengah(p, f):
        return (C[0] + int((p[0] - C[0]) * f), C[1] + int((p[1] - C[1]) * f))

    ins = 0.94
    Li, Ri, Ti, Bi, Ci = (ketengah(p, ins) for p in (L, R, T, B, C))
    ruangan = [
        ([Ti, Ri, Ci], (255, 233, 214), (247, 202, 184)),   # kamar tidur
        ([Ri, Bi, Ci], (223, 243, 226), (193, 227, 199)),   # dapur
        ([Bi, Li, Ci], (251, 233, 199), (231, 205, 162)),   # ruang tamu
        ([Li, Ti, Ci], (216, 240, 248), (183, 218, 233)),   # kamar mandi
    ]
    mask_r = []
    for poly, ca, cb in ruangan:
        mask_r.append(_tempel_gradien(img, poly, ca, cb))

    # -- pola lantai ringkas per ruangan --
    def pola_kamar(pd):
        pd.ellipse([Ci[0] - rx * S // 3, Ti[1] + (Ci[1] - Ti[1]) // 2 - 8 * S,
                    Ci[0] + rx * S // 3, Ci[1] - 4 * S],
                   fill=(255, 176, 198, 255), outline=(214, 120, 148, 255),
                   width=2 * S)
    _tempel_pola(img, mask_r[0], pola_kamar)

    def pola_dapur(pd):
        langkah = 18 * S
        for i in range(-4, 12):
            off = i * langkah
            pd.line([(Li[0] + off, B[1]), (Li[0] + off + h * S, B[1] - h * S)],
                    fill=(255, 255, 255, 70), width=S)
    _tempel_pola(img, mask_r[1], pola_dapur)

    def pola_tamu(pd):
        for y in range(Bi[1], Ci[1] - 2 * S, 14 * S):
            pd.line([(0, y), (img.size[0], y)], fill=(178, 138, 92, 100), width=S)
    _tempel_pola(img, mask_r[2], pola_tamu)

    def pola_mandi(pd):
        for x in range(Li[0], Ci[0] + 1, 18 * S):
            pd.line([(x, 0), (x, img.size[1])], fill=(255, 255, 255, 80), width=S)
        for y in range(Ti[1], Ci[1] + 1, 18 * S):
            pd.line([(0, y), (img.size[0], y)], fill=(255, 255, 255, 80), width=S)
    _tempel_pola(img, mask_r[3], pola_mandi)

    # -- dinding belakang: naik dari dua sisi atas belah ketupat --
    dinding_kiri = [L, T, up(T, hw), up(L, hw)]
    dinding_kanan = [T, R, up(R, hw), up(T, hw)]
    d.polygon(dinding_kiri, fill=(247, 226, 196, 255))
    d.polygon(dinding_kanan, fill=(238, 210, 172, 255))
    # wallpaper: garis garis vertikal halus di dinding
    for i in range(1, 12):
        t = i / 12.0
        x0 = int(L[0] + (T[0] - L[0]) * t)
        yb = int(L[1] + (T[1] - L[1]) * t)
        d.line([(x0, yb - 2 * S), (x0, yb - hw + 2 * S)],
               fill=(224, 196, 158, 255), width=S)
        x1 = int(T[0] + (R[0] - T[0]) * t)
        yb1 = int(T[1] + (R[1] - T[1]) * t)
        d.line([(x1, yb1 - 2 * S), (x1, yb1 - hw + 2 * S)],
               fill=(216, 186, 146, 255), width=S)
    # garis skirting bawah dinding
    d.line([L, T], fill=KAYU_GELAP, width=3 * S)
    d.line([T, R], fill=KAYU_GELAP, width=3 * S)
    d.line([up(L, hw), up(T, hw)], fill=OL_HALUS, width=S)
    d.line([up(T, hw), up(R, hw)], fill=OL_HALUS, width=S)
    # outline dinding
    d.line(dinding_kiri + [dinding_kiri[0]], fill=OL, width=2 * S)
    d.line(dinding_kanan + [dinding_kanan[0]], fill=OL, width=2 * S)

    # -- dinding depan rendah (bangunan "dibuka"): dua sisi bawah --
    depan_kiri = [L, B, up(B, fw), up(L, fw)]
    depan_kanan = [B, R, up(R, fw), up(B, fw)]
    d.polygon(depan_kiri, fill=(255, 236, 204, 255))
    d.polygon(depan_kanan, fill=(244, 220, 182, 255))
    d.line(depan_kiri + [depan_kiri[0]], fill=OL, width=2 * S)
    d.line(depan_kanan + [depan_kanan[0]], fill=OL, width=2 * S)

    # -- sekat antar ruangan (garis tipis di lantai) --
    d.line([Ti, Bi], fill=(140, 118, 96, 255), width=S)
    d.line([Li, Ri], fill=(140, 118, 96, 255), width=S)

    # -- atap melayang gable terbuka di atas dinding belakang --
    Ap = (C[0], C[1] - hw - gap - hr)
    b1 = up(L, hw + gap)
    b2 = up(T, hw + gap)
    b3 = up(R, hw + gap)
    # sisi bawah atap (tidus) tampak sebagai strip gelap tipis
    d.polygon([b1, b3, (b3[0], b3[1] + 8 * S), (b1[0], b1[1] + 8 * S)],
              fill=(120, 66, 40, 255))
    # dua bidang atap
    d.polygon([b1, b2, Ap], fill=MERAH_ATAP_GELAP)
    d.polygon([b2, b3, Ap], fill=MERAH_ATAP)
    garis_genteng2 = 4
    for i in range(1, garis_genteng2):
        t = i / float(garis_genteng2)
        p0 = (int(b1[0] + (Ap[0] - b1[0]) * t), int(b1[1] + (Ap[1] - b1[1]) * t))
        p1 = (int(b2[0] + (Ap[0] - b2[0]) * t), int(b2[1] + (Ap[1] - b2[1]) * t))
        d.line([p0, p1], fill=(180, 76, 52, 255), width=S)
    d.line([b1, b2, b3], fill=OL, width=3 * S)
    d.line([b1, Ap], fill=OL, width=3 * S)
    d.line([b3, Ap], fill=OL, width=3 * S)

    simpan2x(img, nama, w, h)


cutaway_interior("interior-rumah.png", 256, 256, 128, 118, 100, 44)
cutaway_interior("interior-kost.png", 512, 256, 256, 118, 200, 88)
cutaway_interior("interior-apartemen.png", 512, 256, 256, 118, 200, 88)
cutaway_interior("interior-perumahan.png", 768, 384, 384, 236, 300, 132)


# ================= PERABOT (128x128) =================
# Ikon tampak depan, latar transparan, outline rapi + bayangan kecil
# di bawah tiap perabot supaya "menempel" di lantai denah.

def perabot(nama, gambar):
    img = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([20, 110, 108, 120], fill=(0, 0, 0, 45))
    gambar(d)
    simpan(img, nama)


def gambar_kasur(d):
    d.rectangle([14, 78, 114, 96], fill=KAYU_GELAP, outline=OL, width=2)
    # sandaran kepala
    d.rectangle([14, 44, 22, 78], fill=KAYU, outline=OL, width=2)
    d.rectangle([14, 64, 114, 80], fill=(255, 255, 255, 255),
                outline=(200, 200, 200, 255), width=2)
    # bantal + selimut (lipatan)
    d.rectangle([18, 64, 44, 76], fill=KACA, outline=OL, width=2)
    d.ellipse([24, 66, 32, 72], fill=KACA_TERANG)
    d.rectangle([48, 64, 114, 80], fill=(224, 90, 70, 255), outline=OL, width=2)
    d.line([(48, 64), (48, 80)], fill=(180, 60, 45, 255), width=2)
    d.rectangle([18, 96, 26, 112], fill=ABU_GELAP, outline=OL, width=1)
    d.rectangle([102, 96, 110, 112], fill=ABU_GELAP, outline=OL, width=1)


def gambar_lemari(d):
    d.rectangle([34, 16, 94, 112], fill=KAYU, outline=OL, width=3)
    d.line([(64, 16), (64, 112)], fill=OL, width=3)
    # panel pintu + gagang emas + atap lemari
    d.rectangle([40, 24, 58, 104], fill=(170, 120, 65, 255), outline=OL_HALUS, width=1)
    d.rectangle([70, 24, 88, 104], fill=(170, 120, 65, 255), outline=OL_HALUS, width=1)
    d.rectangle([56, 56, 62, 66], fill=EMAS, outline=OL, width=1)
    d.rectangle([66, 56, 72, 66], fill=EMAS, outline=OL, width=1)
    d.rectangle([34, 16, 94, 26], fill=(180, 130, 70, 255), outline=OL, width=2)
    d.rectangle([40, 104, 48, 112], fill=ABU_GELAP)
    d.rectangle([80, 104, 88, 112], fill=ABU_GELAP)


def gambar_kompor(d):
    d.rectangle([24, 44, 104, 112], fill=(200, 200, 205, 255),
                outline=OL, width=3)
    # 2 tungku + api
    d.ellipse([34, 48, 58, 62], fill=(60, 60, 60, 255), outline=OL, width=2)
    d.ellipse([70, 48, 94, 62], fill=(60, 60, 60, 255), outline=OL, width=2)
    d.ellipse([40, 52, 48, 60], fill=(255, 120, 60, 255))
    d.ellipse([76, 52, 84, 60], fill=(255, 120, 60, 255))
    # pintu oven + kaca + kenop
    d.rectangle([32, 70, 96, 104], fill=(90, 90, 90, 255), outline=OL, width=2)
    d.rectangle([38, 76, 90, 98], fill=KACA, outline=OL, width=2)
    d.line([(44, 90), (62, 78)], fill=KACA_TERANG, width=2)
    d.ellipse([100, 48, 106, 54], fill=(255, 120, 60, 255), outline=OL, width=1)


def gambar_meja(d):
    d.rectangle([16, 56, 112, 70], fill=KAYU, outline=OL, width=2)
    d.line([(16, 62), (112, 62)], fill=(180, 130, 70, 255), width=2)
    d.rectangle([22, 70, 30, 112], fill=KAYU_GELAP, outline=OL, width=1)
    d.rectangle([98, 70, 106, 112], fill=KAYU_GELAP, outline=OL, width=1)
    # piring + gelas di atas meja
    d.ellipse([52, 40, 76, 56], fill=(255, 255, 255, 255), outline=OL, width=2)
    d.ellipse([60, 45, 68, 51], fill=(224, 120, 64, 255))
    d.rectangle([84, 36, 94, 56], fill=KACA, outline=OL, width=2)


def gambar_kursi(d):
    d.rectangle([40, 24, 88, 62], fill=(78, 122, 81, 255), outline=OL, width=3)
    d.line([(40, 38), (88, 38)], fill=(60, 100, 62, 255), width=2)
    d.line([(40, 50), (88, 50)], fill=(60, 100, 62, 255), width=2)
    # bantal duduk + kaki
    d.rectangle([34, 62, 94, 76], fill=(96, 145, 98, 255), outline=OL, width=3)
    d.rectangle([40, 76, 48, 112], fill=ABU_GELAP, outline=OL, width=1)
    d.rectangle([80, 76, 88, 112], fill=ABU_GELAP, outline=OL, width=1)


def gambar_bak_mandi(d):
    d.polygon([(18, 62), (110, 62), (100, 104), (28, 104)],
              fill=(255, 255, 255, 255), outline=OL, width=3)
    d.polygon([(26, 66), (102, 66), (96, 76), (32, 76)],
              fill=(120, 180, 230, 255))
    d.line([(32, 70), (96, 70)], fill=KACA_TERANG, width=2)
    # keran + gelembung
    d.rectangle([92, 30, 100, 62], fill=(180, 180, 180, 255), outline=OL, width=2)
    d.rectangle([92, 30, 112, 38], fill=(180, 180, 180, 255), outline=OL, width=2)
    d.ellipse([40, 68, 46, 74], fill=(255, 255, 255, 255), outline=OL, width=1)
    d.ellipse([56, 70, 62, 76], fill=(255, 255, 255, 255), outline=OL, width=1)
    d.rectangle([24, 104, 32, 112], fill=ABU_GELAP, outline=OL, width=1)
    d.rectangle([96, 104, 104, 112], fill=ABU_GELAP, outline=OL, width=1)


def gambar_wastafel(d):
    d.rectangle([44, 16, 84, 48], fill=KACA, outline=OL, width=2)
    d.line([(50, 42), (66, 20)], fill=KACA_TERANG, width=3)
    # keran + bak + tiang + laci
    d.rectangle([60, 48, 68, 62], fill=(180, 180, 180, 255), outline=OL, width=2)
    d.rectangle([52, 56, 76, 62], fill=(180, 180, 180, 255), outline=OL, width=2)
    d.polygon([(36, 62), (92, 62), (84, 82), (44, 82)],
              fill=(255, 255, 255, 255), outline=OL, width=3)
    d.ellipse([52, 66, 76, 76], fill=(120, 180, 230, 255))
    d.rectangle([58, 82, 70, 112], fill=(220, 220, 220, 255),
                outline=OL, width=2)
    d.line([(58, 96), (70, 96)], fill=OL, width=2)


def gambar_tv(d):
    d.line([(52, 28), (44, 16)], fill=OL, width=3)
    d.line([(76, 28), (84, 16)], fill=OL, width=3)
    d.ellipse([42, 12, 48, 18], fill=(200, 40, 40, 255), outline=OL, width=1)
    d.rectangle([24, 28, 104, 88], fill=(40, 40, 40, 255), outline=OL, width=3)
    # layar: langit + gunung + matahari
    d.rectangle([30, 34, 98, 82], fill=(120, 180, 230, 255))
    d.ellipse([84, 38, 94, 48], fill=EMAS, outline=OL, width=1)
    d.polygon([(30, 82), (52, 52), (74, 82)], fill=(70, 168, 78, 255),
              outline=OL, width=1)
    d.polygon([(58, 82), (82, 48), (98, 82)], fill=(47, 143, 58, 255),
              outline=OL, width=1)
    d.rectangle([56, 88, 72, 100], fill=(60, 60, 60, 255), outline=OL, width=1)
    d.rectangle([44, 100, 84, 108], fill=(60, 60, 60, 255), outline=OL, width=1)


def gambar_sofa(d):
    d.rectangle([20, 40, 108, 78], fill=(140, 60, 90, 255), outline=OL, width=3)
    d.line([(20, 52), (108, 52)], fill=(110, 45, 70, 255), width=2)
    d.rectangle([14, 62, 34, 100], fill=(140, 60, 90, 255), outline=OL, width=3)
    d.rectangle([94, 62, 114, 100], fill=(140, 60, 90, 255), outline=OL, width=3)
    d.rectangle([34, 78, 94, 100], fill=(168, 72, 100, 255), outline=OL, width=2)
    # bantal kuning + kaki
    d.rectangle([38, 46, 60, 76], fill=EMAS, outline=OL, width=2)
    d.rectangle([68, 46, 90, 76], fill=EMAS, outline=OL, width=2)
    d.rectangle([20, 100, 28, 110], fill=ABU_GELAP, outline=OL, width=1)
    d.rectangle([100, 100, 108, 110], fill=ABU_GELAP, outline=OL, width=1)


def gambar_lampu(d):
    d.ellipse([44, 24, 84, 52], fill=(255, 209, 102, 120))
    d.polygon([(48, 52), (80, 52), (72, 76), (56, 76)],
              fill=EMAS, outline=(200, 150, 40, 255), width=2)
    d.line([(56, 56), (72, 56)], fill=(200, 150, 40, 255), width=2)
    d.line([(54, 64), (74, 64)], fill=(200, 150, 40, 255), width=2)
    d.rectangle([62, 76, 66, 104], fill=KAYU_GELAP, outline=OL, width=1)
    d.rectangle([48, 104, 80, 112], fill=(90, 55, 25, 255), outline=OL, width=2)


perabot("perabot-kasur.png", gambar_kasur)
perabot("perabot-lemari.png", gambar_lemari)
perabot("perabot-kompor.png", gambar_kompor)
perabot("perabot-meja.png", gambar_meja)
perabot("perabot-kursi.png", gambar_kursi)
perabot("perabot-bak_mandi.png", gambar_bak_mandi)
perabot("perabot-wastafel.png", gambar_wastafel)
perabot("perabot-tv.png", gambar_tv)
perabot("perabot-sofa.png", gambar_sofa)
perabot("perabot-lampu.png", gambar_lampu)

print("SELESAI")
