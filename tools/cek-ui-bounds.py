#!/usr/bin/env python3
"""Cek geometri UI Dunia Kita (landscape 1920x1080) tanpa Unity Editor.

Mem-parsing semua panggilan UiFactory.AturPosisi di UIManager.cs,
PanelMisi.cs, PanelPengaturan.cs, PanelKejadian.cs,
PanelPencapaian.cs, PanelInterior.cs, PanelPeristiwaPenghuni.cs,
PanelIzinTanah.cs dan PanelAkun.cs,
menghitung posisi ABSOLUT tiap elemen (posisi di dalam panel/bar
dijumlahkan dengan posisi panel/bar induknya), lalu memeriksa:
  1. Semua elemen berada dalam batas layar 1920x1080. (FASE 4:
     metode dengan isi bergulir — TampilkanMenuBangun dan
     PanelMisi.PerbaruiIsi — hanya dicek horizontal, karena baris di
     bawah viewport sengaja berada di luar layar dan digulir.)
  2. Tinggi semua tombol >= 90 unit acuan (target sentuh).
  3. Tombol-tombol dalam satu bar/panel tidak saling tumpang tindih.
     Entri identik (target + rect sama) dihapus dulu supaya elemen
     berposisi literal yang dievaluasi sekali per baris tidak
     menimbulkan positif palsu.
  4. Panel tidak menutupi seluruh layar (luas panel < 50% luas layar).

Jalankan:  python3 tools/cek-ui-bounds.py
Keluar kode 0 bila semua pemeriksaan lolos.
"""

import re
import sys
from pathlib import Path

LEBAR_LAYAR = 1920
TINGGI_LAYAR = 1080
TINGGI_TOMBOL_MIN = 90

DASAR = Path(__file__).resolve().parent.parent / "unity-project" / "Assets" / "Scripts"

# (berkas, metode) -> (variabel panel/bar, rect panel (x, y, w, h)).
# Elemen anak dihitung relatif terhadap rect ini; panel/bar itu sendiri
# relatif terhadap akar (0, 0, 1920, 1080).
PANEL = {
    ("Managers/UIManager.cs", "BangunBarAtas"): ("bar", (0, 0, 1920, 110)),
    ("Managers/UIManager.cs", "BangunBarBawah"): ("bar", (0, 940, 1920, 140)),
    ("Managers/UIManager.cs", "BangunPanelBangun"): ("panelBangun", (580, 100, 760, 880)),
    ("Managers/UIManager.cs", "BangunPanelDetail"): ("panelDetail", (580, 220, 760, 640)),
    ("Managers/UIManager.cs", "BangunPanelLaporan"): ("panelLaporan", (580, 160, 760, 760)),
    # FASE 5: panel perayaan pindah ke PanelPerayaan.cs; panel tutorial baru.
    ("UI/PanelPerayaan.cs", "Bangun"): ("panel", (580, 280, 760, 520)),
    ("UI/PanelTutorial.cs", "Bangun"): ("panel", (360, 600, 1200, 300)),
    ("Managers/UIManager.cs", "BangunToast"): ("panelToast", (560, 140, 800, 90)),
    ("Managers/UIManager.cs", "TampilkanMenuBangun"): ("isiMenuBangun", (580, 100, 760, 880)),
    ("UI/PanelMisi.cs", "Bangun"): ("panel", (580, 40, 760, 1000)),
    ("UI/PanelMisi.cs", "PerbaruiIsi"): ("isi", (580, 40, 760, 1000)),
    ("UI/PanelPencapaian.cs", "Bangun"): ("panel", (580, 90, 760, 900)),
    ("UI/PanelPencapaian.cs", "PerbaruiIsi"): ("isi", (580, 90, 760, 900)),
    ("UI/PanelPengaturan.cs", "Bangun"): ("panel", (610, 230, 700, 620)),
    ("UI/PanelKejadian.cs", "Bangun"): ("panel", (580, 220, 760, 640)),
    # FASE 7: panel izin tanah + panel akun (daftar pilihan bangunan
    # bergulir — rect isiDaftar = posisi scroll di dalam panel).
    ("UI/PanelIzinTanah.cs", "Bangun"): ("panel", (580, 170, 760, 740)),
    ("UI/PanelIzinTanah.cs", "TampilkanPilihanBangunan"):
        ("isiDaftar", (620, 380, 680, 310)),
    ("UI/PanelAkun.cs", "Bangun"): ("panel", (580, 60, 760, 980)),
    # FASE 4B: panel interior (gulir) + panel peristiwa penghuni.
    # (Tombol "Lihat Interior" dibuat PanelInterior.BuatTombol-
    # LihatInterior untuk panel detail — karena metodenya bukan void,
    # AturPosisi-nya dievaluasi di bawah PanelInterior.Bangun di atas;
    # tumpang tindihnya vs Jual/Tutup dicek tangan: tombol y 420..510,
    # Jual/Tutup y 520..630 — tidak bersentuhan.)
    ("UI/PanelInterior.cs", "Bangun"): ("panel", (580, 40, 760, 1000)),
    ("UI/PanelInterior.cs", "PerbaruiIsi"): ("isi", (580, 40, 760, 1000)),
    ("UI/PanelPeristiwaPenghuni.cs", "Bangun"): ("panel", (580, 190, 760, 700)),
}

# Metode dengan isi bergulir (ScrollRect): baris di bawah viewport
# sengaja berada di luar layar — cek batas layar hanya horizontal.
SCROLL = {
    ("Managers/UIManager.cs", "TampilkanMenuBangun"),
    ("UI/PanelMisi.cs", "PerbaruiIsi"),
    ("UI/PanelInterior.cs", "PerbaruiIsi"),
    ("UI/PanelIzinTanah.cs", "TampilkanPilihanBangunan"),
}

# Jumlah baris per (berkas, metode) untuk evaluasi nilai 'atas'.
JUMLAH_BARIS = {
    ("Managers/UIManager.cs", "TampilkanMenuBangun"): 18,
    ("UI/PanelMisi.cs", "PerbaruiIsi"): 14,
    ("UI/PanelPencapaian.cs", "PerbaruiIsi"): 6,
    ("UI/PanelInterior.cs", "PerbaruiIsi"): 14,
    ("UI/PanelIzinTanah.cs", "TampilkanPilihanBangunan"): 18,
}

POLA_METODE = re.compile(r"(?:private|public)\s+(?:static\s+)?void\s+(\w+)\s*\(")
POLA_ATUR = re.compile(
    r"AturPosisi\(\s*([\w\.]+)\.GetComponent<RectTransform>\(\)\s*,"
    r"\s*([^,]+?)\s*,\s*([^,]+?)\s*,\s*([^,]+?)\s*,\s*([^)]+?)\)",
    re.DOTALL,
)
POLA_ANGKA = re.compile(r"^(-?\d+(?:\.\d+)?)f?$")


def nilai(arg, nilai_atas):
    """Evaluasi argumen posisi: literal angka, atau 'atas' / 'atas + N'."""
    arg = " ".join(arg.split())
    m = POLA_ANGKA.match(arg)
    if m:
        return float(m.group(1))
    m = re.match(r"^atas(?:\s*\+\s*(\d+(?:\.\d+)?))?$", arg)
    if m and nilai_atas is not None:
        return nilai_atas + (float(m.group(1)) if m.group(1) else 0.0)
    return None


def periksa_berkas(relatif):
    teks = (DASAR / relatif).read_text()
    hasil = []  # (metode, target, (x, y, w, h) absolut)
    # Bagi berkas menjadi rentang per metode (berdasarkan urutan kemunculan).
    batas = [(m.group(1), m.start()) for m in POLA_METODE.finditer(teks)]
    batas.append((None, len(teks)))
    for i in range(len(batas) - 1):
        metode, awal = batas[i]
        akhir = batas[i + 1][1]
        kunci = (relatif, metode)
        if kunci not in PANEL:
            continue
        var_panel, rect_panel = PANEL[kunci]
        badan = teks[awal:akhir]
        # Nilai 'atas' untuk loop di metode ini (jumlah baris per metode).
        jumlah_baris = JUMLAH_BARIS.get((relatif, metode), 3)
        nilai_atas_list = [None]
        m_awal = re.search(r"float atas = (\d+(?:\.\d+)?)f;", badan)
        m_tambah = re.search(r"atas \+= (\d+(?:\.\d+)?)f;", badan)
        if m_awal and m_tambah:
            awal_n = float(m_awal.group(1))
            langkah = float(m_tambah.group(1))
            nilai_atas_list = [awal_n + k * langkah for k in range(jumlah_baris)]
        for am in POLA_ATUR.finditer(badan):
            target = am.group(1)
            args = [am.group(j) for j in range(2, 6)]
            for nilai_atas in nilai_atas_list:
                angka = [nilai(a, nilai_atas) for a in args]
                if any(v is None for v in angka):
                    print("  LEWAT (tak bisa dinilai): %s %s" % (relatif, metode))
                    continue
                x, y, w, h = angka
                if target == var_panel:
                    absolut = (x, y, w, h)
                else:
                    absolut = (rect_panel[0] + x, rect_panel[1] + y, w, h)
                hasil.append((metode, target, absolut))
    return hasil


def main():
    semua = []
    for relatif in ["Managers/UIManager.cs", "UI/PanelMisi.cs",
                    "UI/PanelPengaturan.cs", "UI/PanelKejadian.cs",
                    "UI/PanelPencapaian.cs", "UI/PanelInterior.cs",
                    "UI/PanelPeristiwaPenghuni.cs", "UI/PanelPerayaan.cs",
                    "UI/PanelTutorial.cs", "UI/PanelIzinTanah.cs",
                    "UI/PanelAkun.cs"]:
        semua += [(relatif,) + e for e in periksa_berkas(relatif)]
    # Hapus entri identik (berkas, metode, target, rect sama) — elemen
    # berposisi literal dievaluasi sekali per baris loop, padahal di
    # layar ia hanya ada satu (mis. tombol perluasan lahan di
    # TampilkanMenuBangun). Tanpa ini, pemeriksaan tumpang tindih
    # melihat tombol yang sama bertabrakan dengan dirinya sendiri.
    semua = list(dict.fromkeys(semua))

    lolos = 0
    gagal = 0

    def catat(ok, pesan):
        nonlocal lolos, gagal
        print(("  OK  " if ok else "  GAGAL  ") + pesan)
        if ok:
            lolos += 1
        else:
            gagal += 1

    print("== Batas layar 1920x1080 (%d elemen) ==" % len(semua))
    for berkas, metode, target, (x, y, w, h) in semua:
        if (berkas, metode) in SCROLL:
            # Isi bergulir: hanya horizontal yang dicek.
            dalam = x >= 0 and x + w <= LEBAR_LAYAR
        else:
            dalam = x >= 0 and y >= 0 and x + w <= LEBAR_LAYAR and y + h <= TINGGI_LAYAR
        catat(dalam, "%s.%s %s di (%.0f,%.0f) %.0fx%.0f%s" % (
            metode, target, berkas.split("/")[-1], x, y, w, h,
            "" if dalam else " -- KELUAR LAYAR!"))

    print("== Tinggi tombol >= %d ==" % TINGGI_TOMBOL_MIN)
    tombol = [(b, m, t, r) for (b, m, t, r) in semua if "tombol" in t.lower()]
    for berkas, metode, target, (x, y, w, h) in tombol:
        catat(h >= TINGGI_TOMBOL_MIN,
              "%s %s tinggi %.0f" % (metode, target, h))

    print("== Tombol tidak tumpang tindih (per induk) ==")
    # Kelompokkan tombol per (berkas, metode) lalu cek irisan berpasangan.
    from collections import defaultdict
    kelompok = defaultdict(list)
    for berkas, metode, target, rect in tombol:
        kelompok[(berkas, metode)].append((target, rect))
    for (berkas, metode), daftar in sorted(kelompok.items()):
        for i in range(len(daftar)):
            for j in range(i + 1, len(daftar)):
                (t1, (x1, y1, w1, h1)) = daftar[i]
                (t2, (x2, y2, w2, h2)) = daftar[j]
                iris = not (x1 + w1 <= x2 or x2 + w2 <= x1 or y1 + h1 <= y2 or y2 + h2 <= y1)
                catat(not iris, "%s: %s vs %s" % (metode, t1, t2))

    print("== Panel tidak menutupi seluruh layar ==")
    luas_layar = LEBAR_LAYAR * TINGGI_LAYAR
    for (berkas, metode), (var_panel, (x, y, w, h)) in sorted(PANEL.items()):
        if "panel" not in var_panel.lower() and var_panel not in ("isi", "isiMenuBangun"):
            continue
        persen = 100.0 * w * h / luas_layar
        catat(persen < 50.0, "%s %s %.0f%% layar (%.0fx%.0f)" % (metode, var_panel, persen, w, h))

    print("\nHasil: %d lolos, %d gagal." % (lolos, gagal))
    return 0 if gagal == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
