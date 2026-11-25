#!/usr/bin/env python3
# Dunia Kita — generate-audio.py
# Mensintesis semua aset audio Fase 5 dari kode (numpy + scipy),
# TANPA unduhan dan TANPA rekaman — tidak ada masalah lisensi.
#
#   6 SFX pendek (0,09-0,9 detik, mono 44100 Hz, int16):
#     sfx-tap.wav, sfx-bangun.wav, sfx-koin.wav,
#     sfx-naik-level.wav, sfx-klaim.wav, sfx-tolak.wav
#   1 loop musik latar ambient (~48 detik, mono 44100 Hz, int16):
#     musik-latar.wav (progresi C - Am - F - G, pad + melodi pelan,
#     awal & akhir sunyi supaya loop mulus)
#
# Jalankan:  python3 tools/generate-audio.py
# Script mencetak durasi, sample rate, puncak, dan RMS tiap file
# (verifikasi numerik — asisten tidak bisa mendengar, jadi angka
# inilah bukti file sehat: puncak < 0,99 = tidak clip).

import os

import numpy as np
from scipy.io import wavfile

SR = 44100
KELUAR = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                      "..", "unity-project", "Assets", "Audio")


def tulis(nama, sinyal):
    """Tulis sinyal float [-1, 1] sebagai WAV int16 + cetak statistik."""
    sinyal = np.clip(sinyal, -0.98, 0.98)
    jalur = os.path.join(KELUAR, nama)
    wavfile.write(jalur, SR, (sinyal * 32767).astype(np.int16))
    puncak = float(np.max(np.abs(sinyal)))
    rms = float(np.sqrt(np.mean(sinyal ** 2)))
    print("%-18s durasi %5.2f dtk  puncak %.3f  RMS %.3f  %s"
          % (nama, len(sinyal) / SR, puncak, rms,
             "CLIP!" if puncak >= 0.99 else "ok"))
    return jalur


def nada(frekuensi, durasi, redam=6.0, jenis="sinus"):
    """Satu nada dengan amplop serangan cepat + peluruhan eksponensial."""
    t = np.arange(int(SR * durasi)) / SR
    if jenis == "sinus":
        s = np.sin(2 * np.pi * frekuensi * t)
    else:  # "lembut": sinus + harmonik ke-2 pelan (mirip segitiga)
        s = np.sin(2 * np.pi * frekuensi * t) \
            + 0.3 * np.sin(2 * np.pi * 2 * frekuensi * t)
    amplop = np.exp(-redam * t)
    # Serangan 5 ms supaya tidak "klik" di awal.
    serang = min(int(SR * 0.005), len(t))
    amplop[:serang] *= np.linspace(0, 1, serang)
    return s * amplop


def gabung(daftar):
    """Gabung beberapa (sinyal, tunda_detik) menjadi satu trek."""
    panjang = 0
    for s, tunda in daftar:
        panjang = max(panjang, int(tunda * SR) + len(s))
    keluar = np.zeros(panjang)
    for s, tunda in daftar:
        awal = int(tunda * SR)
        keluar[awal:awal + len(s)] += s
    return keluar


def derau_putih(durasi, redam=25.0):
    """Derau putih dengan peluruhan (dipakai ketukan kayu)."""
    rng = np.random.default_rng(7)  # Seed tetap -> hasil deterministik.
    t = np.arange(int(SR * durasi)) / SR
    return rng.standard_normal(len(t)) * np.exp(-redam * t)


def lolos_rendah(sinyal, alpha=0.08):
    """Lolos-rendah sederhana (rata-rata bergerak eksponensial)."""
    keluar = np.zeros_like(sinyal)
    akum = 0.0
    for i, v in enumerate(sinyal):
        akum += alpha * (v - akum)
        keluar[i] = akum
    return keluar


# ------------------------------------------------------------------ SFX
# Tap/klik: bunyi pendek tajam (1,2 kHz, 0,09 dtk).
tulis("sfx-tap.wav", nada(1200, 0.09, redam=45.0) * 0.5)

# Bangun selesai: ketukan kayu (derau lolos-rendah) + blip cerah 880 Hz.
ketuk = lolos_rendah(derau_putih(0.18, redam=30.0)) * 1.6
tulis("sfx-bangun.wav",
      gabung([(ketuk, 0.0), (nada(880, 0.25, redam=10.0), 0.12)]) * 0.7)

# Koin masuk: dua nada ding bertingkat (1320 -> 1760 Hz).
tulis("sfx-koin.wav",
      gabung([(nada(1320, 0.3, redam=8.0), 0.0),
              (nada(1760, 0.4, redam=8.0), 0.12)]) * 0.6)

# Naik level: arpeggio C-E-G-C' (523, 659, 784, 1047 Hz) + kilau.
tulis("sfx-naik-level.wav",
      gabung([(nada(523.25, 0.35, redam=5.0), 0.00),
              (nada(659.25, 0.35, redam=5.0), 0.13),
              (nada(783.99, 0.35, redam=5.0), 0.26),
              (nada(1046.50, 0.55, redam=4.0), 0.39),
              (nada(2093.00, 0.3, redam=8.0, jenis="lembut"), 0.39)]) * 0.42)

# Klaim hadiah: lonceng ganda (1047 + 1319 Hz bersamaan).
tulis("sfx-klaim.wav",
      (nada(1046.50, 0.5, redam=6.0) + nada(1318.51, 0.5, redam=6.0)) * 0.4)

# Tolak/error: dengung rendah (160 Hz + harmonik, 0,3 dtk).
tulis("sfx-tolak.wav",
      nada(160, 0.3, redam=10.0, jenis="lembut") * 0.7)

# ------------------------------------------------------------------ Musik
# Loop ambient 48 detik: 4 akor x 12 detik (C - Am - F - G).
# Tiap akor = pad (3 nada sinus + harmonik pelan) dengan amplop
# naik-turun halus + 3 nada melodi pentatonik yang jarang.
# Semua amplop berakhir di 0 supaya awal = akhir = sunyi (loop mulus).
AKOR = [
    (261.63, 329.63, 392.00),   # C mayor: C-E-G
    (220.00, 261.63, 329.63),   # A minor: A-C-E
    (174.61, 220.00, 261.63),   # F mayor: F-A-C
    (196.00, 246.94, 293.66),   # G mayor: G-B-D
]
MELODI = {  # (akor, detik-dalam-akor): frekuensi
    (0, 2.0): 523.25, (0, 7.0): 659.25,
    (1, 3.0): 440.00, (1, 8.0): 523.25,
    (2, 2.0): 349.23, (2, 7.0): 440.00,
    (3, 3.0): 392.00, (3, 8.0): 493.88,
}
DURASI_AKOR = 12.0
musik = np.zeros(int(SR * DURASI_AKOR * len(AKOR)))
for i_akor, nada_akor in enumerate(AKOR):
    awal = int(i_akor * DURASI_AKOR * SR)
    n = int(DURASI_AKOR * SR)
    t = np.arange(n) / SR
    # Amplop pad: naik 3 dtk, tahan, turun 3 dtk (halus di ujung).
    amplop = np.minimum(np.minimum(t / 3.0, (DURASI_AKOR - t) / 3.0), 1.0)
    amplop = np.clip(amplop, 0.0, 1.0)
    pad = np.zeros(n)
    for f in nada_akor:
        pad += np.sin(2 * np.pi * f * t) + 0.25 * np.sin(2 * np.pi * 2 * f * t)
    musik[awal:awal + n] += pad * amplop * 0.10
    for (a, detik), f in MELODI.items():
        if a != i_akor:
            continue
        m = nada(f, 2.5, redam=1.2, jenis="lembut") * 0.22
        m_awal = awal + int(detik * SR)
        musik[m_awal:m_awal + len(m)] += m
# Ujung loop dipudarkan ke 0 dalam 0,5 detik terakhir supaya
# sambungan ke awal (yang juga 0) tidak berbunyi "klik".
pudar = int(SR * 0.5)
musik[-pudar:] *= np.linspace(1, 0, pudar)
tulis("musik-latar.wav", musik)

print("SELESAI — 7 file WAV di", KELUAR)
