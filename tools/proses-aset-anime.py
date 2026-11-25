#!/usr/bin/env python3
"""Dunia Kita — tools/proses-aset-anime.py
Mengubah ilustrasi Anime HD (.jpg) menjadi sprite PNG transparan
beresolusi HD (1024x1024 untuk 1x1, 2048x1024 untuk 2x2) dengan
flood-fill dari tepi luar agar bagian putih di dalam bangunan
(dinding klinik, pilar balai kota) tetap utuh.
Juga menyetel spritePixelsToUnits = 400 di .png.meta terkait.
"""

import glob
import os
import re
from collections import deque
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPRITES = os.path.join(ROOT, "unity-project", "Assets", "Sprites")
ART_DIR = r"C:\Users\ricar\.gemini\antigravity\brain\da521feb-c43d-421f-b506-1b4cf6df2391"

PETA_ASET = {
    "anime_balaikota_*.jpg": ("balaikota.png", (2048, 1024), 400),
    "anime_klinik_*.jpg": ("klinik.png", (2048, 1024), 400),
    "anime_pos_*.jpg": ("pos.png", (1024, 1024), 400),
    "anime_rumah_*.jpg": ("rumah.png", (1024, 1024), 400),
    "anime_warung_*.jpg": ("warung.png", (1024, 1024), 400),
    "anime_taman_*.jpg": ("taman.png", (1024, 1024), 400),
    "anime_kost_*.jpg": ("kost.png", (2048, 1024), 400),
    "anime_sekolah_*.jpg": ("sekolah.png", (2048, 1024), 400),
    "anime_pasar_*.jpg": ("pasar.png", (2048, 1024), 400),
    "anime_apartemen_*.jpg": ("apartemen.png", (2048, 1024), 400),
    "anime_pembangkit_*.jpg": ("pembangkit.png", (2048, 1024), 400),
    "anime_kafe_*.jpg": ("kafe.png", (1024, 1024), 400),
}


def hapus_latar_putih_luar(img, ambang=236):
    img = img.convert("RGBA")
    w, h = img.size
    px = img.load()
    terkunjungi = bytearray(w * h)
    q = deque()

    def cek_putih(x, y):
        r, g, b, _a = px[x, y]
        return r >= ambang and g >= ambang and b >= ambang

    for x in range(w):
        for y in (0, h - 1):
            idx = y * w + x
            if not terkunjungi[idx] and cek_putih(x, y):
                terkunjungi[idx] = 1
                q.append((x, y))
    for y in range(h):
        for x in (0, w - 1):
            idx = y * w + x
            if not terkunjungi[idx] and cek_putih(x, y):
                terkunjungi[idx] = 1
                q.append((x, y))

    while q:
        cx, cy = q.popleft()
        px[cx, cy] = (255, 255, 255, 0)
        for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
            if 0 <= nx < w and 0 <= ny < h:
                idx = ny * w + nx
                if not terkunjungi[idx] and cek_putih(nx, ny):
                    terkunjungi[idx] = 1
                    q.append((nx, ny))

    # Feather halus 1px pada piksel tepi yang berbatasan dengan transparan
    for y in range(1, h - 1):
        for x in range(1, w - 1):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            if r >= 218 and g >= 218 and b >= 218:
                tetangga_kosong = (
                    px[x + 1, y][3] == 0 or px[x - 1, y][3] == 0
                    or px[x, y + 1][3] == 0 or px[x, y - 1][3] == 0
                )
                if tetangga_kosong:
                    px[x, y] = (r, g, b, 90)
    return img


def proses_satu(sumber, tujuan_nama, ukuran, ppu):
    img = Image.open(sumber)
    img = hapus_latar_putih_luar(img)
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)

    tw, th = ukuran
    # Skala proporsional agar pas di kanvas target dengan sedikit margin
    margin_w = int(tw * 0.88)
    margin_h = int(th * 0.90)
    rasio = min(margin_w / img.width, margin_h / img.height)
    nw = max(1, int(img.width * rasio))
    nh = max(1, int(img.height * rasio))
    img = img.resize((nw, nh), Image.Resampling.LANCZOS)

    kanvas = Image.new("RGBA", (tw, th), (0, 0, 0, 0))
    ox = (tw - nw) // 2
    oy = int(th * 0.92) - nh
    if oy < 0:
        oy = (th - nh) // 2
    kanvas.paste(img, (ox, oy), img)

    out_png = os.path.join(SPRITES, tujuan_nama)
    kanvas.save(out_png, "PNG")
    print("OK sprite HD:", out_png, kanvas.size)

    meta_path = out_png + ".meta"
    if os.path.exists(meta_path):
        with open(meta_path, encoding="utf-8") as f:
            teks = f.read()
        teks_baru = re.sub(
            r"spritePixelsToUnits:\s*\d+",
            "spritePixelsToUnits: " + str(ppu),
            teks,
        )
        if teks_baru != teks:
            with open(meta_path, "w", encoding="utf-8") as f:
                f.write(teks_baru)
            print("OK meta PPU=" + str(ppu) + ":", meta_path)


def main():
    for pola, (nama_out, ukuran, ppu) in PETA_ASET.items():
        hasil = sorted(glob.glob(os.path.join(ART_DIR, pola)))
        if not hasil:
            print("LEWATI (belum ada):", pola)
            continue
        proses_satu(hasil[-1], nama_out, ukuran, ppu)


if __name__ == "__main__":
    main()
