#!/usr/bin/env python3
# Dunia Kita — validasi-yaml.py
# Memeriksa file YAML Unity (scene, prefab, .asset, ProjectSettings):
#  1. Semua dokumen YAML bisa di-parse.
#  2. Setiap referensi {guid: X} punya file .meta dengan guid X.
#  3. Setiap script yang dirujuk scene/prefab/.asset ada sebagai file .cs.
#  4. Tidak ada fileID duplikat dalam satu file.
# Keluar dengan kode 0 bila semua lolos, 1 bila ada masalah.

import os
import re
import sys
import yaml

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                    "..", "unity-project")

# Kumpulkan semua guid dari .meta
GUID_KE_PATH = {}
for dirpath, _, files in os.walk(ROOT):
    for nama in files:
        if nama.endswith(".meta"):
            jalur = os.path.join(dirpath, nama)
            with open(jalur, encoding="utf-8") as f:
                isi = f.read()
            m = re.search(r"^guid: ([0-9a-f]{32})\s*$", isi, re.M)
            if not m:
                print("GAGAL: .meta tanpa guid:", jalur)
                sys.exit(1)
            GUID_KE_PATH[m.group(1)] = os.path.relpath(jalur, ROOT)

print("meta ditemukan:", len(GUID_KE_PATH))

YAML_FILES = [
    "Assets/Scenes/MainScene.unity",
    "Assets/Prefabs/Bangunan.prefab",
    "Assets/Data/Buildings/RumahSederhana.asset",
    "Assets/Data/Buildings/Warung.asset",
    "Assets/Data/Buildings/Taman.asset",
    "Assets/Data/Buildings/Kost.asset",
    "Assets/Data/Buildings/TokoKelontong.asset",
    "Assets/Data/Buildings/Jalan.asset",
    "Assets/Data/Buildings/Perpustakaan.asset",
    "Assets/Data/Buildings/Sekolah.asset",
    "Assets/Data/Buildings/Pasar.asset",
    "Assets/Data/Buildings/PosKeamanan.asset",
    "Assets/Data/Buildings/Apartemen.asset",
    "Assets/Data/Buildings/Kafe.asset",
    "Assets/Data/Buildings/Klinik.asset",
    "Assets/Data/Buildings/Pembangkit.asset",
    "Assets/Data/Buildings/Perumahan.asset",
    "Assets/Data/Buildings/Kampus.asset",
    "Assets/Data/Buildings/AlunAlun.asset",
    "Assets/Data/Buildings/BalaiKota.asset",
    "ProjectSettings/ProjectSettings.asset",
    "ProjectSettings/TagManager.asset",
    "ProjectSettings/EditorBuildSettings.asset",
    "ProjectSettings/QualitySettings.asset",  # FASE 5
]

masalah = 0
for rel in YAML_FILES:
    jalur = os.path.join(ROOT, rel)
    with open(jalur, encoding="utf-8") as f:
        isi = f.read()
    # 1. Parse YAML multi-dokumen (hapus tag khusus Unity !u!N
    #    yang tidak dikenal parser YAML generik).
    isi_parse = re.sub(r"!u!\d+ ?", "", isi)
    try:
        docs = list(yaml.safe_load_all(isi_parse))
    except yaml.YAMLError as e:
        print("GAGAL parse YAML:", rel, "->", e)
        masalah += 1
        continue
    # 2. Referensi guid harus punya .meta
    for m in re.finditer(r"guid: ([0-9a-f]{32})", isi):
        g = m.group(1)
        if g == "0" * 32:
            continue
        if g not in GUID_KE_PATH:
            print("GAGAL: guid tanpa .meta di", rel, "->", g)
            masalah += 1
    # 3. Referensi script (fileID 11500000) harus file .cs yang ada
    for m in re.finditer(
            r"\{fileID: 11500000, guid: ([0-9a-f]{32}), type: 3\}", isi):
        g = m.group(1)
        path_meta = GUID_KE_PATH.get(g, "")
        if not path_meta.endswith(".cs.meta"):
            print("GAGAL: referensi script bukan file .cs di", rel, "->", g)
            masalah += 1
    # 4. fileID unik dalam satu file
    fids = re.findall(r"^--- !u!\d+ &(\d+)", isi, re.M)
    if len(fids) != len(set(fids)):
        print("GAGAL: fileID duplikat di", rel)
        masalah += 1
    print("OK  ", rel, f"({len(docs)} dokumen, {len(fids)} objek)")

# 5. manifest.json valid + ProjectVersion.txt ada
import json
with open(os.path.join(ROOT, "Packages", "manifest.json")) as f:
    json.load(f)
print("OK   Packages/manifest.json (JSON valid)")
assert os.path.exists(os.path.join(ROOT, "ProjectSettings",
                                   "ProjectVersion.txt"))
print("OK   ProjectSettings/ProjectVersion.txt ada")

if masalah:
    print("HASIL: %d masalah" % masalah)
    sys.exit(1)
print("HASIL: semua pemeriksaan YAML/GUID lolos")
