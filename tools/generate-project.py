#!/usr/bin/env python3
# Dunia Kita — generate-project.py
# Membangun file-file project Unity yang tidak bisa ditulis tangan
# dengan aman: .meta (GUID deterministik), MainScene.unity,
# PrefabBangunan.prefab, dan 3 file .asset BuildingData.
#
# GUID dihitung deterministik dari path aset (md5) supaya stabil
# antar-jalannya script dan semua referensi antar-file konsisten.
# Jalankan ulang script ini setelah menambah/mengubah aset.

import hashlib
import os

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                    "..", "unity-project")
ASSETS = os.path.join(ROOT, "Assets")


def guid(path_rel):
    """GUID Unity: 32 karakter hex, deterministik dari path aset."""
    return hashlib.md5(path_rel.encode("utf-8")).hexdigest()


def tulis(path, isi):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(isi)
    print("tulis", os.path.relpath(path, ROOT))


# ---------------------------------------------------------------- .meta
META_SCRIPT = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

META_TEXTURE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  externalObjects: {{}}
  serializedVersion: 10
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapScale: 1
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: -3
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  platformSettings: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

META_PREFAB = """fileFormatVersion: 2
guid: {guid}
PrefabImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

META_DEFAULT = """fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

META_FOLDER = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

META_ASSET = """fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

# FASE 5: .meta audio (AudioImporter). loadType 1 = DecompressOnLoad
# (tepat untuk SFX pendek); musik latar memakai 0 = Streaming
# (hemat memori HP). format 0 = PCM (sesuai file WAV).
META_AUDIO = """fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  audioData:
    serializedVersion: 6
    loadType: {load_type}
    channels: 1
    frequency: 44100
    format: 0
    threeD: 0
    useHardware: 0
    stream: 0
    sampleRateOverride: 0
  usePreloadAudio: 0
  preloadAudioData: 1
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

# FASE 5: .meta ikon aplikasi — TextureImporter biasa (textureType 0,
# BUKAN Sprite): ikon dipakai PlayerSettings, bukan SpriteRenderer.
META_ICON = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  externalObjects: {{}}
  serializedVersion: 10
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapScale: 1
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: -3
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 0
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  platformSettings: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def meta_untuk(path_rel, template):
    path_abs = os.path.join(ROOT, path_rel)
    tulis(path_abs + ".meta", template.format(guid=guid(path_rel)))


# Script C#
SCRIPTS = [
    "Assets/Scripts/Data/GameBalance.cs",
    "Assets/Scripts/Data/BuildingData.cs",
    "Assets/Scripts/Utils/EconomyCalculator.cs",
    "Assets/Scripts/Utils/SaveData.cs",
    "Assets/Scripts/Utils/FormatUtil.cs",
    "Assets/Scripts/Utils/ProgresiDunia.cs",
    "Assets/Scripts/Utils/SistemKejadian.cs",
    "Assets/Scripts/Utils/PewaktuHari.cs",
    "Assets/Scripts/Grid/IsometricGrid.cs",
    "Assets/Scripts/Grid/GridManager.cs",
    "Assets/Scripts/Grid/BuildingView.cs",
    "Assets/Scripts/Managers/GameManager.cs",
    "Assets/Scripts/Managers/BuildManager.cs",
    "Assets/Scripts/Managers/EconomyManager.cs",
    "Assets/Scripts/Managers/CitizenManager.cs",
    "Assets/Scripts/Managers/PejalanKaki.cs",
    "Assets/Scripts/Managers/SaveManager.cs",
    "Assets/Scripts/Managers/CameraController.cs",
    "Assets/Scripts/Managers/UIManager.cs",
    "Assets/Scripts/Managers/InteriorManager.cs",
    "Assets/Scripts/Managers/AudioManager.cs",  # FASE 5
    "Assets/Scripts/Managers/ServerManager.cs",  # FASE 7
    "Assets/Scripts/Managers/ServerManagerAksi.cs",  # FASE 7 (partial)
    "Assets/Scripts/UI/UiFactory.cs",
    "Assets/Scripts/UI/PanelMisi.cs",
    "Assets/Scripts/UI/PanelPengaturan.cs",
    "Assets/Scripts/UI/PanelKejadian.cs",
    "Assets/Scripts/UI/PanelPencapaian.cs",
    "Assets/Scripts/UI/PanelInterior.cs",
    "Assets/Scripts/UI/PanelPeristiwaPenghuni.cs",
    "Assets/Scripts/UI/PanelPerayaan.cs",  # FASE 5 (pindah dari UIManager)
    "Assets/Scripts/UI/PanelTutorial.cs",  # FASE 5
    "Assets/Scripts/UI/PanelIzinTanah.cs",  # FASE 7
    "Assets/Scripts/UI/PanelAkun.cs",  # FASE 7
    "Assets/Scripts/Utils/SistemPencapaian.cs",
    "Assets/Scripts/Utils/SistemMisiHarian.cs",
    "Assets/Scripts/Utils/SistemInterior.cs",
    "Assets/Scripts/Utils/SistemPeristiwaPenghuni.cs",
    "Assets/Scripts/Utils/SistemBalaiKota.cs",
    "Assets/Scripts/Utils/SistemTutorial.cs",  # FASE 5
    "Assets/Scripts/Utils/SistemWaliKota.cs",  # FASE 7
    "Assets/Scripts/Utils/AturanDuniaBersama.cs",  # FASE 7
    "Assets/Scripts/Utils/HttpDuniaBersama.cs",  # FASE 7
    "Assets/Scripts/Utils/DtoDuniaBersama.cs",  # FASE 7
    "Assets/Editor/BuildAndroid.cs",  # FASE 6 (skrip build Editor)
]
for s in SCRIPTS:
    meta_untuk(s, META_SCRIPT)

# Sprite PNG
for nama in ["tile.png", "rumah.png", "warung.png", "taman.png",
             "highlight.png", "warga1.png", "warga2.png", "warga3.png",
             "kost.png", "toko.png", "ikon-bangun.png",
             "jalan.png", "perpustakaan.png", "pos.png", "kafe.png",
             "sekolah.png", "pasar.png", "apartemen.png", "klinik.png",
             "pembangkit.png", "perumahan.png", "kampus.png",
             "alun-alun.png", "balaikota.png",
             "warga1-a.png", "warga1-b.png",
             "warga2-a.png", "warga2-b.png",
             "warga3-a.png", "warga3-b.png",
             "interior-rumah.png", "interior-kost.png",
             "interior-apartemen.png", "interior-perumahan.png",
             "perabot-kasur.png", "perabot-lemari.png",
             "perabot-kompor.png", "perabot-meja.png",
             "perabot-kursi.png", "perabot-bak_mandi.png",
             "perabot-wastafel.png", "perabot-tv.png",
             "perabot-sofa.png", "perabot-lampu.png"]:
    meta_untuk("Assets/Sprites/" + nama, META_TEXTURE)

# Prefab, scene, dan .asset
meta_untuk("Assets/Prefabs/Bangunan.prefab", META_PREFAB)
meta_untuk("Assets/Scenes/MainScene.unity", META_DEFAULT)
# FASE 5: ikon aplikasi 512x512 (bukan sprite — .meta Default).
meta_untuk("Assets/Sprites/ikon-aplikasi.png", META_ICON)
# FASE 5: aset audio (SFX DecompressOnLoad, musik Streaming).
# (replace, bukan .format — supaya escape {{}} di template tetap
# utuh untuk satu .format(guid=...) di meta_untuk.)
for nama in ["sfx-tap.wav", "sfx-bangun.wav", "sfx-koin.wav",
             "sfx-naik-level.wav", "sfx-klaim.wav", "sfx-tolak.wav"]:
    meta_untuk("Assets/Audio/" + nama,
               META_AUDIO.replace("{load_type}", "1"))
meta_untuk("Assets/Audio/musik-latar.wav",
           META_AUDIO.replace("{load_type}", "0"))
for nama in ["RumahSederhana.asset", "Warung.asset", "Taman.asset",
             "Kost.asset", "TokoKelontong.asset",
             "Jalan.asset", "Perpustakaan.asset", "Sekolah.asset",
             "Pasar.asset", "PosKeamanan.asset", "Apartemen.asset",
             "Kafe.asset", "Klinik.asset", "Pembangkit.asset",
             "Perumahan.asset", "Kampus.asset", "AlunAlun.asset",
             "BalaiKota.asset"]:
    meta_untuk("Assets/Data/Buildings/" + nama, META_ASSET)

# Folder
FOLDERS = ["Assets", "Assets/Scenes", "Assets/Scripts", "Assets/Scripts/Data",
           "Assets/Scripts/Grid", "Assets/Scripts/Managers", "Assets/Scripts/UI",
           "Assets/Scripts/Utils", "Assets/Sprites", "Assets/Prefabs",
           "Assets/Data", "Assets/Data/Buildings", "Assets/Audio",
           "Assets/Editor"]  # FASE 6
for d in FOLDERS:
    meta_untuk(d, META_FOLDER)

# ---------------------------------------------------------------- GUID
G = {p: guid(p) for p in SCRIPTS}
G["tile"] = guid("Assets/Sprites/tile.png")
G["highlight"] = guid("Assets/Sprites/highlight.png")
G["rumah_sprite"] = guid("Assets/Sprites/rumah.png")
G["warung_sprite"] = guid("Assets/Sprites/warung.png")
G["taman_sprite"] = guid("Assets/Sprites/taman.png")
G["warga1"] = guid("Assets/Sprites/warga1.png")
G["warga2"] = guid("Assets/Sprites/warga2.png")
G["warga3"] = guid("Assets/Sprites/warga3.png")
G["kost_sprite"] = guid("Assets/Sprites/kost.png")
G["toko_sprite"] = guid("Assets/Sprites/toko.png")
G["ikon_bangun"] = guid("Assets/Sprites/ikon-bangun.png")
G["jalan_sprite"] = guid("Assets/Sprites/jalan.png")
G["perpustakaan_sprite"] = guid("Assets/Sprites/perpustakaan.png")
G["sekolah_sprite"] = guid("Assets/Sprites/sekolah.png")
G["pasar_sprite"] = guid("Assets/Sprites/pasar.png")
G["pos_sprite"] = guid("Assets/Sprites/pos.png")
G["apartemen_sprite"] = guid("Assets/Sprites/apartemen.png")
G["kafe_sprite"] = guid("Assets/Sprites/kafe.png")
G["klinik_sprite"] = guid("Assets/Sprites/klinik.png")
G["pembangkit_sprite"] = guid("Assets/Sprites/pembangkit.png")
G["perumahan_sprite"] = guid("Assets/Sprites/perumahan.png")
G["kampus_sprite"] = guid("Assets/Sprites/kampus.png")
G["alun_alun_sprite"] = guid("Assets/Sprites/alun-alun.png")
G["balaikota_sprite"] = guid("Assets/Sprites/balaikota.png")
# FASE 5: ikon aplikasi + klip audio (fileID AudioClip = 8300000).
G["ikon_aplikasi"] = guid("Assets/Sprites/ikon-aplikasi.png")
G["sfx_tap"] = guid("Assets/Audio/sfx-tap.wav")
G["sfx_bangun"] = guid("Assets/Audio/sfx-bangun.wav")
G["sfx_koin"] = guid("Assets/Audio/sfx-koin.wav")
G["sfx_naik_level"] = guid("Assets/Audio/sfx-naik-level.wav")
G["sfx_klaim"] = guid("Assets/Audio/sfx-klaim.wav")
G["sfx_tolak"] = guid("Assets/Audio/sfx-tolak.wav")
G["musik_latar"] = guid("Assets/Audio/musik-latar.wav")
# Frame jalan warga: pasangan A/B per varian warna.
G["warga1_a"] = guid("Assets/Sprites/warga1-a.png")
G["warga1_b"] = guid("Assets/Sprites/warga1-b.png")
G["warga2_a"] = guid("Assets/Sprites/warga2-a.png")
G["warga2_b"] = guid("Assets/Sprites/warga2-b.png")
G["warga3_a"] = guid("Assets/Sprites/warga3-a.png")
G["warga3_b"] = guid("Assets/Sprites/warga3-b.png")
# FASE 4B: denah interior + perabot
G["interior_rumah_sprite"] = guid("Assets/Sprites/interior-rumah.png")
G["interior_kost_sprite"] = guid("Assets/Sprites/interior-kost.png")
G["interior_apartemen_sprite"] = guid("Assets/Sprites/interior-apartemen.png")
G["interior_perumahan_sprite"] = guid("Assets/Sprites/interior-perumahan.png")
G["perabot_kasur_sprite"] = guid("Assets/Sprites/perabot-kasur.png")
G["perabot_lemari_sprite"] = guid("Assets/Sprites/perabot-lemari.png")
G["perabot_kompor_sprite"] = guid("Assets/Sprites/perabot-kompor.png")
G["perabot_meja_sprite"] = guid("Assets/Sprites/perabot-meja.png")
G["perabot_kursi_sprite"] = guid("Assets/Sprites/perabot-kursi.png")
G["perabot_bak_mandi_sprite"] = guid("Assets/Sprites/perabot-bak_mandi.png")
G["perabot_wastafel_sprite"] = guid("Assets/Sprites/perabot-wastafel.png")
G["perabot_tv_sprite"] = guid("Assets/Sprites/perabot-tv.png")
G["perabot_sofa_sprite"] = guid("Assets/Sprites/perabot-sofa.png")
G["perabot_lampu_sprite"] = guid("Assets/Sprites/perabot-lampu.png")
G["prefab"] = guid("Assets/Prefabs/Bangunan.prefab")
G["scene"] = guid("Assets/Scenes/MainScene.unity")
G["rumah_asset"] = guid("Assets/Data/Buildings/RumahSederhana.asset")
G["warung_asset"] = guid("Assets/Data/Buildings/Warung.asset")
G["taman_asset"] = guid("Assets/Data/Buildings/Taman.asset")
G["kost_asset"] = guid("Assets/Data/Buildings/Kost.asset")
G["toko_asset"] = guid("Assets/Data/Buildings/TokoKelontong.asset")
G["jalan_asset"] = guid("Assets/Data/Buildings/Jalan.asset")
G["perpustakaan_asset"] = guid("Assets/Data/Buildings/Perpustakaan.asset")
G["sekolah_asset"] = guid("Assets/Data/Buildings/Sekolah.asset")
G["pasar_asset"] = guid("Assets/Data/Buildings/Pasar.asset")
G["pos_asset"] = guid("Assets/Data/Buildings/PosKeamanan.asset")
G["apartemen_asset"] = guid("Assets/Data/Buildings/Apartemen.asset")
G["kafe_asset"] = guid("Assets/Data/Buildings/Kafe.asset")
G["klinik_asset"] = guid("Assets/Data/Buildings/Klinik.asset")
G["pembangkit_asset"] = guid("Assets/Data/Buildings/Pembangkit.asset")
G["perumahan_asset"] = guid("Assets/Data/Buildings/Perumahan.asset")
G["kampus_asset"] = guid("Assets/Data/Buildings/Kampus.asset")
G["alun_alun_asset"] = guid("Assets/Data/Buildings/AlunAlun.asset")
G["balaikota_asset"] = guid("Assets/Data/Buildings/BalaiKota.asset")
G["produk"] = guid("Dunia Kita")

# ---------------------------------------------------------------- .asset
ASSET_TEMPLATE = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script}, type: 3}}
  m_Name: {nama}
  m_EditorClassIdentifier: 
  id: {id}
  nama: {nama_tampil}
  deskripsi: {deskripsi}
  kategori: {kategori}
  levelBuka: {level_buka}
  ukuran: {{x: {ukuran_x}, y: {ukuran_y}}}
  biayaBangun: {biaya}
  pemasukanHarian: {pemasukan}
  biayaPerawatanHarian: {perawatan}
  kapasitasWarga: {kapasitas}
  bonusBahagia: {bonus}
  sprite: {{fileID: 21300000, guid: {sprite}, type: 3}}
  warna: {{r: 1, g: 1, b: 1, a: 1}}
  offsetVisualY: {offset}
"""

BANGUNAN = [
    dict(nama="RumahSederhana", id="rumah", nama_tampil="Rumah Sederhana",
         deskripsi="Hunian sederhana untuk 2 warga. Warga yang tinggal di sini berbelanja di warung sekitar.",
         kategori=0, level_buka=1, ukuran_x=1, ukuran_y=1,
         biaya=500000, pemasukan=0, perawatan=1000, kapasitas=2, bonus=0,
         sprite=G["rumah_sprite"], offset="0.4"),
    dict(nama="Warung", id="warung", nama_tampil="Warung",
         deskripsi="Toko kecil yang menjual kebutuhan harian warga. Menghasilkan pemasukan tiap hari.",
         kategori=1, level_buka=1, ukuran_x=1, ukuran_y=1,
         biaya=750000, pemasukan=15000, perawatan=2000, kapasitas=0, bonus=0,
         sprite=G["warung_sprite"], offset="0.4"),
    dict(nama="Taman", id="taman", nama_tampil="Taman",
         deskripsi="Ruang hijau yang membuat warga lebih bahagia (+2 kebahagiaan tiap taman).",
         kategori=2, level_buka=1, ukuran_x=1, ukuran_y=1,
         biaya=250000, pemasukan=0, perawatan=500, kapasitas=0, bonus=0,
         sprite=G["taman_sprite"], offset="0.5"),
    dict(nama="Jalan", id="jalan", nama_tampil="Jalan",
         deskripsi="Petak jalan murah. Bangunan yang bersebelahan dengan Jalan berfungsi penuh; yang tidak bersebelahan pemasukan dan efeknya berkurang setengah.",
         kategori=2, level_buka=1, ukuran_x=1, ukuran_y=1,
         biaya=50000, pemasukan=0, perawatan=0, kapasitas=0, bonus=0,
         sprite=G["jalan_sprite"], offset="0.4"),
    dict(nama="Kost", id="kost", nama_tampil="Kost",
         deskripsi="Hunian 2x2 untuk 8 warga. Terbuka di Level 2 (Dusun). Pemasukan sewa tiap hari, perawatan lebih besar.",
         kategori=0, level_buka=2, ukuran_x=2, ukuran_y=2,
         biaya=2000000, pemasukan=40000, perawatan=5000, kapasitas=8, bonus=0,
         sprite=G["kost_sprite"], offset="0.4"),
    dict(nama="TokoKelontong", id="toko_kelontong", nama_tampil="Toko Kelontong",
         deskripsi="Toko sembako yang menjual kebutuhan harian warga. Terbuka di Level 3 (Desa). Pemasukan dipengaruhi kebahagiaan dan hujan seperti warung.",
         kategori=1, level_buka=3, ukuran_x=1, ukuran_y=1,
         biaya=1500000, pemasukan=30000, perawatan=4000, kapasitas=0, bonus=0,
         sprite=G["toko_sprite"], offset="0.4"),
    dict(nama="Perpustakaan", id="perpustakaan", nama_tampil="Perpustakaan",
         deskripsi="Ruang baca warga. Terbuka di Level 2 (Dusun). Menambah kebahagiaan +2.",
         kategori=2, level_buka=2, ukuran_x=1, ukuran_y=1,
         biaya=800000, pemasukan=0, perawatan=2000, kapasitas=0, bonus=2,
         sprite=G["perpustakaan_sprite"], offset="0.4"),
    dict(nama="Sekolah", id="sekolah", nama_tampil="Sekolah Dasar",
         deskripsi="Sekolah dasar 2x2. Terbuka di Level 3 (Desa). Menambah kebahagiaan +4.",
         kategori=2, level_buka=3, ukuran_x=2, ukuran_y=2,
         biaya=3000000, pemasukan=0, perawatan=8000, kapasitas=0, bonus=4,
         sprite=G["sekolah_sprite"], offset="0.4"),
    dict(nama="BalaiKota", id="balaikota", nama_tampil="Balai Kota",
         deskripsi="Kantor wali kota 2x2, landmark desa. Terbuka di Level 3 (Desa). Kebahagiaan +5 dan reputasi +2 tiap hari. Hanya 1 per dunia.",
         kategori=2, level_buka=3, ukuran_x=2, ukuran_y=2,
         biaya=3000000, pemasukan=0, perawatan=5000, kapasitas=0, bonus=5,
         sprite=G["balaikota_sprite"], offset="0.4"),
    dict(nama="Pasar", id="pasar", nama_tampil="Pasar",
         deskripsi="Pasar tradisional 2x2. Terbuka di Level 3 (Desa). Pemasukan besar, dipengaruhi kebahagiaan dan hujan seperti warung.",
         kategori=1, level_buka=3, ukuran_x=2, ukuran_y=2,
         biaya=4000000, pemasukan=60000, perawatan=8000, kapasitas=0, bonus=0,
         sprite=G["pasar_sprite"], offset="0.4"),
    dict(nama="PosKeamanan", id="pos", nama_tampil="Pos Keamanan",
         deskripsi="Pos jaga lingkungan. Terbuka di Level 3 (Desa). Warga merasa lebih aman, kebahagiaan +2.",
         kategori=2, level_buka=3, ukuran_x=1, ukuran_y=1,
         biaya=1000000, pemasukan=0, perawatan=3000, kapasitas=0, bonus=2,
         sprite=G["pos_sprite"], offset="0.4"),
    dict(nama="Apartemen", id="apartemen", nama_tampil="Apartemen Kecil",
         deskripsi="Hunian 2x2 untuk 12 warga. Terbuka di Level 4 (Kota Kecil). Pemasukan sewa tiap hari.",
         kategori=0, level_buka=4, ukuran_x=2, ukuran_y=2,
         biaya=5000000, pemasukan=60000, perawatan=10000, kapasitas=12, bonus=0,
         sprite=G["apartemen_sprite"], offset="0.4"),
    dict(nama="Kafe", id="kafe", nama_tampil="Kafe",
         deskripsi="Kafe kecil tempat warga bersantai. Terbuka di Level 4 (Kota Kecil). Pemasukan dipengaruhi kebahagiaan dan hujan seperti warung, kebahagiaan +2.",
         kategori=1, level_buka=4, ukuran_x=1, ukuran_y=1,
         biaya=2000000, pemasukan=25000, perawatan=5000, kapasitas=0, bonus=2,
         sprite=G["kafe_sprite"], offset="0.4"),
    dict(nama="Klinik", id="klinik", nama_tampil="Klinik",
         deskripsi="Klinik kesehatan 2x2. Terbuka di Level 4 (Kota Kecil). Menambah kebahagiaan +4.",
         kategori=2, level_buka=4, ukuran_x=2, ukuran_y=2,
         biaya=4000000, pemasukan=0, perawatan=10000, kapasitas=0, bonus=4,
         sprite=G["klinik_sprite"], offset="0.4"),
    dict(nama="Pembangkit", id="pembangkit", nama_tampil="Pembangkit Listrik",
         deskripsi="Pembangkit listrik kecil 2x2. Terbuka di Level 4 (Kota Kecil). Listrik stabil membuat warga lebih bahagia +4.",
         kategori=2, level_buka=4, ukuran_x=2, ukuran_y=2,
         biaya=6000000, pemasukan=0, perawatan=12000, kapasitas=0, bonus=4,
         sprite=G["pembangkit_sprite"], offset="0.4"),
    dict(nama="Perumahan", id="perumahan", nama_tampil="Perumahan",
         deskripsi="Kompleks perumahan 3x3 untuk 20 warga. Terbuka di Level 5 (Kota). Pemasukan sewa tiap hari.",
         kategori=0, level_buka=5, ukuran_x=3, ukuran_y=3,
         biaya=8000000, pemasukan=100000, perawatan=15000, kapasitas=20, bonus=0,
         sprite=G["perumahan_sprite"], offset="0.4"),
    dict(nama="Kampus", id="kampus", nama_tampil="Kampus Kecil",
         deskripsi="Kampus kecil 3x3. Terbuka di Level 5 (Kota). Membuka warga bergaji tinggi, pemasukan tiap hari dan kebahagiaan +6.",
         kategori=2, level_buka=5, ukuran_x=3, ukuran_y=3,
         biaya=10000000, pemasukan=80000, perawatan=20000, kapasitas=0, bonus=6,
         sprite=G["kampus_sprite"], offset="0.4"),
    dict(nama="AlunAlun", id="alun_alun", nama_tampil="Alun-alun Kota",
         deskripsi="Landmark kota 3x3. Terbuka di Level 6 (Metropolis). Pusat keramaian, kebahagiaan +10.",
         kategori=2, level_buka=6, ukuran_x=3, ukuran_y=3,
         biaya=12000000, pemasukan=0, perawatan=25000, kapasitas=0, bonus=10,
         sprite=G["alun_alun_sprite"], offset="0.4"),
]
for b in BANGUNAN:
    b["script"] = G["Assets/Scripts/Data/BuildingData.cs"]
    tulis(os.path.join(ASSETS, "Data", "Buildings", b["nama"] + ".asset"),
          ASSET_TEMPLATE.format(**b))

# ---------------------------------------------------------------- prefab
PREFAB = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 100001}}
  - component: {{fileID: 100002}}
  - component: {{fileID: 100003}}
  m_Layer: 0
  m_Name: Bangunan
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children: []
  m_Father: {{fileID: 0}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &100002
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 0
  m_ReflectionProbeUsage: 0
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_Sprite: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 0
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!114 &100003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {view}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
""".format(view=G["Assets/Scripts/Grid/BuildingView.cs"])
tulis(os.path.join(ASSETS, "Prefabs", "Bangunan.prefab"), PREFAB)

# ---------------------------------------------------------------- scene
MONO_TEMPLATE = """--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
{fields}"""


def mono(fid, go, script_path, fields=""):
    return MONO_TEMPLATE.format(fid=fid, go=go,
                                script=G[script_path], fields=fields)


SCENE = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {{r: 0.5, g: 0.5, b: 0.5, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {{r: 0.212, g: 0.227, b: 0.31, a: 1}}
  m_AmbientEquatorColor: {{r: 0.114, g: 0.125, b: 0.16, a: 1}}
  m_AmbientGroundColor: {{r: 0.047, g: 0.05, b: 0.06, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 0
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.478, b: 0.627, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {{fileID: 0}}
  m_SpotCookie: {{fileID: 0}}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {{fileID: 0}}
  m_Sun: {{fileID: 0}}
  m_IndirectSpecularColor: {{r: 0, g: 0, b: 0, a: 1}}
  m_UseRadianceAmbientProbe: 0
--- !u!1 &100
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 101}}
  - component: {{fileID: 102}}
  - component: {{fileID: 103}}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &101
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -10}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children: []
  m_Father: {{fileID: 0}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!20 &102
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 1
  m_BackGroundColor: {{r: 0.62, g: 0.8, b: 0.9, a: 1}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
  m_FocalLength: 50
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 1
  orthographic size: 8
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
  m_StereoMirrorMode: 0
--- !u!1 &200
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 201}}
  - component: {{fileID: 202}}
  - component: {{fileID: 203}}
  - component: {{fileID: 204}}
  - component: {{fileID: 205}}
  - component: {{fileID: 206}}
  - component: {{fileID: 207}}
  - component: {{fileID: 208}}
  - component: {{fileID: 209}}
  - component: {{fileID: 210}}
  - component: {{fileID: 211}}
  - component: {{fileID: 212}}
  m_Layer: 0
  m_Name: GameRoot
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &201
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children: []
  m_Father: {{fileID: 0}}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""

bagian = [SCENE]
bagian.append(mono(103, 100, "Assets/Scripts/Managers/CameraController.cs"))
bagian.append(mono(202, 200, "Assets/Scripts/Grid/IsometricGrid.cs"))
bagian.append(mono(203, 200, "Assets/Scripts/Grid/GridManager.cs",
                   "  tileSprite: {fileID: 21300000, guid: %s, type: 3}\n"
                   "  highlightSprite: {fileID: 21300000, guid: %s, type: 3}"
                   % (G["tile"], G["highlight"])))
bagian.append(mono(204, 200, "Assets/Scripts/Managers/GameManager.cs"))
bagian.append(mono(205, 200, "Assets/Scripts/Managers/BuildManager.cs",
                   "  daftarBangunan:\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  - {fileID: 11400000, guid: %s, type: 2}\n"
                   "  prefabBangunan: {fileID: 100000, guid: %s, type: 3}"
                   % (G["rumah_asset"], G["warung_asset"], G["taman_asset"],
                      G["jalan_asset"], G["kost_asset"], G["toko_asset"],
                      G["perpustakaan_asset"], G["sekolah_asset"],
                      G["balaikota_asset"],
                      G["pasar_asset"], G["pos_asset"],
                      G["apartemen_asset"], G["kafe_asset"],
                      G["klinik_asset"], G["pembangkit_asset"],
                      G["perumahan_asset"], G["kampus_asset"],
                      G["alun_alun_asset"], G["prefab"])))
bagian.append(mono(206, 200, "Assets/Scripts/Managers/EconomyManager.cs"))
bagian.append(mono(207, 200, "Assets/Scripts/Managers/CitizenManager.cs",
                   "  spriteWarga:\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  spriteWargaJalan:\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}"
                   % (G["warga1"], G["warga2"], G["warga3"],
                      G["warga1_a"], G["warga1_b"],
                      G["warga2_a"], G["warga2_b"],
                      G["warga3_a"], G["warga3_b"])))
bagian.append(mono(208, 200, "Assets/Scripts/Managers/SaveManager.cs"))
bagian.append(mono(209, 200, "Assets/Scripts/Managers/UIManager.cs",
                   "  ikonBangun: {fileID: 21300000, guid: %s, type: 3}"
                   % G["ikon_bangun"]))
# FASE 4B: InteriorManager — sprite denah (4, urutan: rumah, kost,
# apartemen, perumahan), sprite perabot (10, urutan = indeks slot
# SistemInterior), sprite warga penghuni (3, sama seperti
# CitizenManager).
bagian.append(mono(210, 200, "Assets/Scripts/Managers/InteriorManager.cs",
                   "  spriteInterior:\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  spritePerabot:\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  spriteWarga:\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}\n"
                   "  - {fileID: 21300000, guid: %s, type: 3}"
                   % (G["interior_rumah_sprite"], G["interior_kost_sprite"],
                      G["interior_apartemen_sprite"], G["interior_perumahan_sprite"],
                      G["perabot_kasur_sprite"], G["perabot_lemari_sprite"],
                      G["perabot_kompor_sprite"], G["perabot_meja_sprite"],
                      G["perabot_kursi_sprite"], G["perabot_bak_mandi_sprite"],
                      G["perabot_wastafel_sprite"], G["perabot_tv_sprite"],
                      G["perabot_sofa_sprite"], G["perabot_lampu_sprite"],
                      G["warga1"], G["warga2"], G["warga3"])))
# FASE 5: AudioManager — 6 SFX + 1 musik latar (fileID AudioClip
# 8300000; urutan field = urutan field di AudioManager.cs).
bagian.append(mono(211, 200, "Assets/Scripts/Managers/AudioManager.cs",
                   "  sfxTap: {fileID: 8300000, guid: %s, type: 3}\n"
                   "  sfxBangun: {fileID: 8300000, guid: %s, type: 3}\n"
                   "  sfxKoin: {fileID: 8300000, guid: %s, type: 3}\n"
                   "  sfxNaikLevel: {fileID: 8300000, guid: %s, type: 3}\n"
                   "  sfxKlaim: {fileID: 8300000, guid: %s, type: 3}\n"
                   "  sfxTolak: {fileID: 8300000, guid: %s, type: 3}\n"
                   "  musikLatar: {fileID: 8300000, guid: %s, type: 3}"
                   % (G["sfx_tap"], G["sfx_bangun"], G["sfx_koin"],
                      G["sfx_naik_level"], G["sfx_klaim"], G["sfx_tolak"],
                      G["musik_latar"])))
# FASE 7: ServerManager — klien REST Dunia Bersama. Tidak ada field
# yang diisi dari Scene (semua referensi dicari lewat
# FindObjectOfType di Awake; token + alamat di PlayerPrefs).
bagian.append(mono(212, 200, "Assets/Scripts/Managers/ServerManager.cs"))
# SCENE memakai kurung ganda {{ }} (ditulis sebagai template Python)
# sedangkan bagian mono() sudah diformat -> ubah ganda jadi tunggal.
isi_scene = "\n".join(bagian).replace("{{", "{").replace("}}", "}")
tulis(os.path.join(ASSETS, "Scenes", "MainScene.unity"), isi_scene + "\n")

# ---------------------------------------------------------------- settings
tulis(os.path.join(ROOT, "ProjectSettings", "ProjectVersion.txt"),
      "m_EditorVersion: 2022.3.20f1\n")

tulis(os.path.join(ROOT, "ProjectSettings", "EditorBuildSettings.asset"),
      """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1045 &1
EditorBuildSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Scenes:
  - enabled: 1
    path: Assets/Scenes/MainScene.unity
    guid: {scene}
""".format(scene=G["scene"]))

tulis(os.path.join(ROOT, "ProjectSettings", "TagManager.asset"),
      """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!78 &1
TagManager:
  serializedVersion: 2
  tags: []
  layers:
  - Default
  - TransparentFX
  - Ignore Raycast
  - 
  - Water
  - UI
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
""")

# FASE 5: identitas + orientasi + ikon aplikasi. Nama perusahaan
# "Uhlabs", produk "Dunia Kita", package "com.uhlabs.duniakita"
# (user bisa mengubahnya di Player Settings), versi 1.0 (code 1
# untuk Android), orientasi Landscape Left/Right saja
# (AutoRotation + hanya landscape yang diizinkan).
tulis(os.path.join(ROOT, "ProjectSettings", "ProjectSettings.asset"),
      """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!129 &1
PlayerSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 19
  productGUID: {produk}
  companyName: Uhlabs
  productName: Dunia Kita
  defaultScreenWidth: 1920
  defaultScreenHeight: 1080
  defaultScreenWidthWeb: 960
  defaultScreenHeightWeb: 600
  bundleVersion: 1.0
  androidBundleVersionCode: 1
  applicationIdentifier:
    Android: com.uhlabs.duniakita
    iPhone: com.uhlabs.duniakita
    Standalone: com.uhlabs.duniakita
  defaultInterfaceOrientation: 4
  allowedAutorotateToPortrait: 0
  allowedAutorotateToPortraitUpsideDown: 0
  allowedAutorotateToLandscapeLeft: 1
  allowedAutorotateToLandscapeRight: 1
  m_SplashScreenStyle: 1
  runInBackground: 1
  m_ActiveColorSpace: 0
  m_MTRendering: 1
  m_UsePlayerLog: 1
  m_Icons:
  - {{fileID: 2800000, guid: {ikon}, type: 3}}
  - {{fileID: 2800000, guid: {ikon}, type: 3}}
  - {{fileID: 2800000, guid: {ikon}, type: 3}}
  - {{fileID: 2800000, guid: {ikon}, type: 3}}
  - {{fileID: 2800000, guid: {ikon}, type: 3}}
  m_BuildTargetIcons: []
""".format(produk=G["produk"], ikon=G["ikon_aplikasi"]))

# FASE 5: QualitySettings — 6 level bawaan Unity; Medium (indeks 2)
# jadi default (dan default Android): pixelLightCount 1, tanpa
# bayangan real-time (game 2D sprite tidak membutuhkannya),
# tekstur penuh + kompresi, MSAA 2x, vSync mati (target 60 fps
# diatur GameManager.Awake dari GameBalance.TargetFrameRate).
LEVEL_KUALITAS = [
    dict(nama="Very Low", pixelLightCount=0, shadows=0, shadowResolution=0,
         shadowProjection=0, shadowCascades=1, shadowDistance=15,
         skinWeights=1, textureQuality=1, anisotropicTextures=0,
         antiAliasing=0, softParticles=0, softVegetation=0,
         realtimeReflectionProbes=0, billboardsFaceCameraPosition=0,
         vSyncCount=0, lodBias=0.3, particleRaycastBudget=16),
    dict(nama="Low", pixelLightCount=1, shadows=0, shadowResolution=0,
         shadowProjection=1, shadowCascades=1, shadowDistance=20,
         skinWeights=1, textureQuality=0, anisotropicTextures=0,
         antiAliasing=0, softParticles=0, softVegetation=0,
         realtimeReflectionProbes=0, billboardsFaceCameraPosition=0,
         vSyncCount=0, lodBias=0.4, particleRaycastBudget=16),
    dict(nama="Medium", pixelLightCount=1, shadows=0, shadowResolution=1,
         shadowProjection=1, shadowCascades=2, shadowDistance=40,
         skinWeights=2, textureQuality=0, anisotropicTextures=0,
         antiAliasing=2, softParticles=0, softVegetation=0,
         realtimeReflectionProbes=0, billboardsFaceCameraPosition=0,
         vSyncCount=0, lodBias=1, particleRaycastBudget=64),
    dict(nama="High", pixelLightCount=2, shadows=2, shadowResolution=2,
         shadowProjection=1, shadowCascades=2, shadowDistance=60,
         skinWeights=2, textureQuality=0, anisotropicTextures=1,
         antiAliasing=2, softParticles=1, softVegetation=0,
         realtimeReflectionProbes=0, billboardsFaceCameraPosition=0,
         vSyncCount=0, lodBias=1, particleRaycastBudget=128),
    dict(nama="Very High", pixelLightCount=3, shadows=2, shadowResolution=3,
         shadowProjection=1, shadowCascades=4, shadowDistance=100,
         skinWeights=3, textureQuality=0, anisotropicTextures=2,
         antiAliasing=4, softParticles=1, softVegetation=1,
         realtimeReflectionProbes=1, billboardsFaceCameraPosition=0,
         vSyncCount=0, lodBias=1, particleRaycastBudget=256),
    dict(nama="Ultra", pixelLightCount=4, shadows=2, shadowResolution=3,
         shadowProjection=1, shadowCascades=4, shadowDistance=150,
         skinWeights=3, textureQuality=0, anisotropicTextures=2,
         antiAliasing=8, softParticles=1, softVegetation=1,
         realtimeReflectionProbes=1, billboardsFaceCameraPosition=1,
         vSyncCount=1, lodBias=2, particleRaycastBudget=1024),
]

TEMPLATE_LEVEL = """  - serializedVersion: 2
    name: {nama}
    pixelLightCount: {pixelLightCount}
    shadows: {shadows}
    shadowResolution: {shadowResolution}
    shadowProjection: {shadowProjection}
    shadowCascades: {shadowCascades}
    shadowDistance: {shadowDistance}
    shadowNearPlaneOffset: 2
    shadowCascade2Split: 0.33333334
    shadowCascade4Split: {{x: 0.06666667, y: 0.13333334, z: 0.26666668}}
    shadowmaskMode: 0
    skinWeights: {skinWeights}
    textureQuality: {textureQuality}
    anisotropicTextures: {anisotropicTextures}
    antiAliasing: {antiAliasing}
    softParticles: {softParticles}
    softVegetation: {softVegetation}
    realtimeReflectionProbes: {realtimeReflectionProbes}
    billboardsFaceCameraPosition: {billboardsFaceCameraPosition}
    vSyncCount: {vSyncCount}
    lodBias: {lodBias}
    maximumLODLevel: 0
    particleRaycastBudget: {particleRaycastBudget}
    asyncUploadTimeSlice: 2
    asyncUploadBufferSize: 16
    asyncUploadPersistentBuffer: 1
    resolutionScalingFixedDPIFactor: 1
    customRenderPipeline: {{fileID: 0}}
    excludedTargetPlatforms: []"""

tulis(os.path.join(ROOT, "ProjectSettings", "QualitySettings.asset"),
      "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"
      "--- !u!47 &1\n"
      "QualitySettings:\n"
      "  m_ObjectHideFlags: 0\n"
      "  serializedVersion: 5\n"
      "  m_QualitySettings:\n"
      + "\n".join(TEMPLATE_LEVEL.format(**lvl) for lvl in LEVEL_KUALITAS)
      + "\n  m_CurrentQuality: 2\n"
      "  m_PerPlatformDefaultQuality:\n"
      "    Standalone: 2\n"
      "    WebGL: 1\n"
      "    Android: 2\n"
      "    iPhone: 2\n")

# ---------------------------------------------------------------- manifest
tulis(os.path.join(ROOT, "Packages", "manifest.json"), """{
  "dependencies": {
    "com.unity.collab-proxy": "2.0.5",
    "com.unity.ide.rider": "3.0.26",
    "com.unity.ide.visualstudio": "2.0.22",
    "com.unity.ide.vscode": "1.2.5",
    "com.unity.test-framework": "1.3.9",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.timeline": "1.7.6",
    "com.unity.ugui": "1.0.0",
    "com.unity.modules.ai": "1.0.0",
    "com.unity.modules.androidjni": "1.0.0",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.assetbundle": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.cloth": "1.0.0",
    "com.unity.modules.director": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.particlesystem": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.screencapture": "1.0.0",
    "com.unity.modules.terrain": "1.0.0",
    "com.unity.modules.terrainphysics": "1.0.0",
    "com.unity.modules.tilemap": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0",
    "com.unity.modules.umbra": "1.0.0",
    "com.unity.modules.unityanalytics": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0",
    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
    "com.unity.modules.unitywebrequestaudio": "1.0.0",
    "com.unity.modules.unitywebrequesttexture": "1.0.0",
    "com.unity.modules.unitywebrequestwww": "1.0.0",
    "com.unity.modules.vehicles": "1.0.0",
    "com.unity.modules.video": "1.0.0",
    "com.unity.modules.vr": "1.0.0",
    "com.unity.modules.wind": "1.0.0",
    "com.unity.modules.xr": "1.0.0"
  }
}
""")

print("SELESAI")
