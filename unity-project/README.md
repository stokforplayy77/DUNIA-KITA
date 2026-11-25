# Dunia Kita — Project Unity (Fase 1–7: Dunia Bersama — Fase Terakhir)

Game mobile simulasi membangun dunia 2,5D isometric (Unity 2022 LTS, C#).
Dokumen perancangan: `../perancangan-game.md`.

**Orientasi: LANDSCAPE 1920×1080** (GDD §17). Semua UI ditata dalam
koordinat acuan 1920×1080; kamera membingkai seluruh grid 12×12 dalam
tampilan mendatar. Sebelum build Android, kunci orientasi di Editor:
`Edit → Project Settings → Player → Resolution and Presentation →
Default Orientation → Landscape Left`.

## Yang sudah berfungsi (Fase 1)

- Grid isometric 12×12 dengan petak tanah (sprite diamond).
- Kamera: geser (drag 1 jari / drag mouse) + zoom (cubit 2 jari / roda mouse).
  Grid 12×12 (dunia 26×13 unit) terbingkai penuh pada ortho size 8
  di layar 16:9 (lebar pandang 28,4 unit, tinggi 16 unit).
- 3 bangunan pertama: **Rumah Sederhana** (Rp500.000, 2 warga),
  **Warung** (Rp750.000, +Rp15.000/hari), **Taman** (Rp250.000, +2 kebahagiaan).
- Tap tanah kosong → menu bangun → bangunan terpasang di petak.
  Tombol **Bangun** di bar bawah membuka menu yang sama untuk petak
  yang terakhir diketuk (bila belum ada petak yang dipilih, game
  meminta pemain mengetuk petak kosong dulu).
- Tap bangunan → panel detail (pemasukan, perawatan, kapasitas) + tombol jual
  (kembali 50% biaya bangun).
- Ekonomi harian: hari bergulir OTOMATIS tiap 5 menit waktu nyata
  (Fase 3 revisi — TIDAK ada lagi tombol "Lewati Hari"); tiap
  pergantian hari menerapkan pemasukan/pengeluaran lalu menampilkan
  laporan hari.
- HUD (bar atas): koin, permata, hari game + hitung mundur ke hari
  berikutnya ("Hari 2 • 03:12"), jumlah warga, kebahagiaan,
  level dunia (Level 1) + tombol **Misi** dan **Pengaturan**.
- Panel **Misi**: 3 misi tutorial Fase 1 (Bangun 1 Rumah, Bangun 1 Warung,
  Capai 4 Warga) + 2 misi Fase 2 (Bertahan 3 Hari, Capai kebahagiaan 60%)
  + 3 misi Fase 3 (Naik ke Level 2, Bangun 1 Kost, Capai 10 Warga)
  dinilai dari state game nyata; misi selesai bisa diklaim
  hadiah koin sekali (Fase 1: Rp25.000 / Rp50.000 / Rp50.000;
  Fase 2: Rp25.000 / Rp50.000; Fase 3: Rp50.000 / Rp75.000 / Rp50.000
  — lihat `Assets/Scripts/Data/GameBalance.cs`).
- Panel **Pengaturan**: tombol Suara Nyala/Mati (preferensi tersimpan di
  PlayerPrefs, diterapkan ke `AudioListener.volume`) + teks versi game.
  Belum ada sumber suara di game — toggle disiapkan untuk fase polish.
- Simpan/muat OTOMATIS ke JSON di `Application.persistentDataPath`
  (tersimpan saat membangun, tiap pergantian hari, tiap 60 detik, dan
  saat aplikasi dijeda/ditutup — TIDAK ada lagi tombol "Simpan").
  Simpanan versi "3.0" (Fase 3) — simpanan lama "2.0" dan "1.0" tetap
  bisa dimuat (field baru terisi default 0).
- Modal awal: Rp5.000.000 (keputusan Fase 1 — GDD tidak menetapkan angka;
  ubah di `Assets/Scripts/Data/GameBalance.cs`).

## Yang sudah berfungsi (Fase 2: Ekonomi & Warga)

- **Pajak kota 2,5%** dari pemasukan kotor tiap hari (otomatis, masuk kas kota).
- **Kebahagiaan memengaruhi warung**: di bawah 40% pemasukan warung -20%,
  di atas 70% +10%, di antaranya normal.
- **Upkeep menunggak**: bila koin tidak cukup membayar perawatan + pajak,
  koin jadi 0 dan kekurangannya dicatat sebagai tunggakan (dilunasi otomatis
  dari pemasukan hari-hari berikutnya). Menunggak 2 hari berturut-turut
  menurunkan kebahagiaan 5 poin. Peringatan tampil di laporan harian + toast.
- **Kebahagiaan dinamis**: mulai 50; +2 per Taman (maks +20); +5 bila semua
  warga punya rumah; -3 bila warga melebihi kapasitas hunian.
- **Warga berjalan**: tiap rumah menambah warga sampai kapasitasnya; warga
  tampil sebagai sprite chibi kecil (3 varian warna) yang berjalan antar
  petak kosong dengan ayunan langkah. Maks 12 sprite di layar demi performa
  HP (sisanya hanya angka populasi). Posisi warga dibangkitkan ulang acak
  saat memuat game (tidak disimpan).
- **Laporan harian rinci**: pemasukan warung, pemasukan lain, pajak,
  perawatan, bersih, kebahagiaan + efeknya ke warung, dan peringatan
  tunggakan bila ada.
- **Biaya perawatan dipertahankan dari Fase 1** (Rumah Rp1.000, Warung
  Rp2.000, Taman Rp500/hari — angka di file `.asset`).

## Yang sudah berfungsi (Fase 3: Progresi Level 1–3 + Kejadian Acak)

- **Hari bergulir otomatis waktu nyata** (revisi desain 24 Nov 2026):
  1 hari game = 5 menit (`GameBalance.DurasiHariDetik = 300`, bisa
  diubah). Hitung mundur tampil di HUD ("Hari 2 • 03:12"). Saat hari
  berganti: ekonomi harian → cek naik level → roll kejadian →
  laporan hari → simpan otomatis. Selama panel perayaan/kejadian
  terbuka, pewaktu hari dijeda supaya pemain sempat membaca.
- **Simpan otomatis**: tiap pergantian hari + tiap 60 detik
  (`GameBalance.IntervalSimpanOtomatisDetik`) + saat membangun/menjual
  + saat aplikasi dijeda/ditutup. TIDAK ada lagi tombol "Simpan".
- **Tombol "Bangun" jadi IKON saja** (sprite palu + rumah kecil,
  `Assets/Sprites/ikon-bangun.png`) — tanpa tulisan, di tengah bar
  bawah. Tombol Misi & Pengaturan di bar atas tetap berteks.
- **Level dunia 1–3** (GDD §8): Level 2 Dusun = 10 warga (hadiah
  Rp100.000); Level 3 Desa = 30 warga + kebahagiaan ≥60%
  (hadiah Rp250.000). Naik level otomatis di akhir hari dengan panel
  perayaan. Level 4–6 dibuka di Fase 4 (lihat bawah).
- **Unlock bangunan**: **Kost** 2×2, 8 warga, Rp2.000.000, sewa
  Rp40.000/hari, perawatan Rp5.000/hari (Level 2); **Toko Kelontong**
  1×1, Rp1.500.000, +Rp30.000/hari, perawatan Rp4.000/hari (Level 3,
  pemasukan dipengaruhi kebahagiaan + hujan seperti warung).
  Bangunan terkunci tampil abu-abu di menu bangun dengan syarat level.
- **Kejadian acak** (GDD §9, peluang 20% tiap pergantian hari):
  Festival Desa (bayar Rp50.000 → kebahagiaan +10 besok, atau tolak),
  Hujan Deras (pemasukan warung -20% besok, atau siapkan payung
  Rp20.000 → hanya -10%), Pedagang Keliling (beli Rp100.000 →
  +1 Permata, atau tolak). Pilihan tampil sebagai panel; efeknya
  dicatat di laporan harian. Kejadian penghuni (GDD §9.1) BELUM
  dikerjakan — rencana sesudah Fase 4.
- **Misi tutorial Fase 3**: Naik ke Level 2 (Rp50.000), Bangun 1 Kost
  (Rp75.000), Capai 10 Warga (Rp50.000).

## Yang sudah berfungsi (Fase 4: Konten Penuh — Level 4–6, Pencapaian, Misi Harian)

- **Level dunia 4–6** (GDD §8): Level 4 Kota Kecil = 80 warga +
  kebahagiaan ≥70% (hadiah Rp500.000); Level 5 Kota = 150 warga +
  kebahagiaan ≥75% (hadiah Rp1.000.000); Level 6 Metropolis = 300
  warga + kebahagiaan ≥80% (hadiah Rp2.000.000). Panel perayaan
  menampilkan bangunan yang terbuka di level baru (dihitung dari
  daftar bangunan, bukan hardcode).
- **12 bangunan baru** (total 17): **Jalan** 1×1 Rp50.000 (Level 1);
  **Perpustakaan** 1×1 Rp800.000, bahagia +2 (Level 2); **Sekolah
  Dasar** 2×2 Rp3.000.000, bahagia +4 (Level 3); **Pasar** 2×2
  Rp4.000.000, +Rp60.000/hari (Level 3, komersial seperti warung);
  **Pos Keamanan** 1×1 Rp1.000.000, bahagia +2 (Level 3);
  **Apartemen Kecil** 2×2 Rp5.000.000, 12 warga, sewa Rp60.000/hari
  (Level 4); **Kafe** 1×1 Rp2.000.000, +Rp25.000/hari, bahagia +2
  (Level 4, komersial); **Klinik** 2×2 Rp4.000.000, bahagia +4
  (Level 4); **Pembangkit Listrik** 2×2 Rp6.000.000, bahagia +4
  (Level 4); **Perumahan** 3×3 Rp8.000.000, 20 warga, sewa
  Rp100.000/hari (Level 5); **Kampus Kecil** 3×3 Rp10.000.000,
  +Rp80.000/hari, bahagia +6 (Level 5); **Alun-alun Kota** 3×3
  Rp12.000.000, bahagia +10 (Level 6). Semua angka di
  `Assets/Data/Buildings/*.asset` + `GameBalance.cs`.
- **Aturan Jalan** (GDD §5.5): bangunan yang bersebelahan dengan
  Jalan (atas/bawah/kiri/kanan, TIDAK diagonal) berfungsi penuh;
  yang tidak bersebelahan, pemasukan dan bonus kebahagiaannya
  −50%. Jalan itu sendiri tidak butuh jalan. Dunia lama tanpa Jalan
  akan kehilangan setengah pemasukan komersial sampai pemain
  membangun Jalan (murah, Rp50.000).
- **Perluasan lahan** (GDD §8, Level 5): di Level 5 pemain bisa
  membeli perluasan grid 12×12 → 16×16 seharga Rp5.000.000 lewat
  tombol di atas menu bangun. Bangunan lama tetap di tempatnya.
- **Pencapaian** (GDD §8, 6 buah, hadiah Permata — sumber Permata
  pertama di game): Juragan Kost (5 Kost, 3 Permata), Kota Pelajar
  (Sekolah + Kampus, 3 Permata), Kota Hijau (20 Taman, 2 Permata),
  Metropolis (capai Level 6, 10 Permata), Juragan Pasar (3 bangunan
  komersial, 3 Permata), Kota Sejahtera (bahagia ≥80%, 5 Permata).
  Panel "Pencapaian" di bar atas (tombol emas); klaim sekali per
  pencapaian, status klaim tersimpan di simpanan.
- **Misi harian** (GDD §9): 3 misi kecil tiap hari game, dipilih
  deterministik dari nomor hari (kolam 8 misi: Bangun 1 bangunan,
  Bangun 1 Taman, Bahagia 65%, Pemasukan Rp50.000/hari, 15 warga,
  3 komersial, Bahagia 75%, 5 Taman). Hadiah Rp25.000–Rp40.000
  koin, atau 1 Permata (Bahagia 75%). Tampil di panel misi bagian
  "Misi Harian" dengan hitung mundur ke pergantian hari; misi yang
  tidak selesai hangus saat hari berganti. Catatan: "Pemasukan
  Rp50.000" dinilai dari perkiraan pemasukan harian (bukan yang
  sudah dibayar), supaya bisa selesai kapan saja di hari itu.
- **Simpanan naik ke versi 4.0**: simpanan 3.0/2.0/1.0 tetap
  dimuat (field baru default aman); pencapaian, penghitung harian,
  dan ukuran grid tersimpan. Menu bangun + panel misi kini
  bergulir (ScrollRect) karena 18 bangunan (17 Fase 4 + Balai Kota)
  dan 12 baris misi tidak muat dalam satu layar.
- **Warga berjalan**: kapasitas hunian total naik (Apartemen 12,
  Perumahan 20), tapi **maks 12 sprite warga di layar tetap
  dipertahankan** demi performa HP — warga ke-13 dan seterusnya
  tetap dihitung (populasi, kebahagiaan, level) tapi tidak
  digambarkan sebagai pejalan kaki.

## Yang sudah berfungsi (Fase 4B: Interior Tembus Pandang + Peristiwa Penghuni)

- **Interior tembus pandang** (GDD §5.6) — HANYA untuk hunian
  (Rumah Sederhana, Kost, Apartemen, Perumahan); sekolah/toko belum
  bisa. Cara: ketuk bangunan hunian → panel detail → tombol
  **"Lihat Interior"** → bangunan beralih ke sprite denah 4 ruangan
  DI TEMPAT di grid (dinding/atap hilang, bukan scene terpisah) dan
  panel perabot terbuka. Tombol "Tutup Interior" (atau membuka
  bangunan lain / panel lain) mengembalikan bangunan ke bentuk luar.
  Hanya satu bangunan dalam mode interior dalam satu waktu.
- **4 ruangan, 10 slot perabot tetap** (GDD §5.6): Kamar Tidur
  (Kasur Rp150.000/+2, Lemari Rp100.000/+1), Dapur (Kompor
  Rp200.000/+2, Meja Rp80.000/+1, Kursi Rp50.000/+1), Kamar Mandi
  (Bak Mandi Rp150.000/+2, Wastafel Rp80.000/+1), Ruang Tamu (TV
  Rp300.000/+3, Sofa Rp250.000/+2, Lampu Rp60.000/+1). Melengkapi
  1 hunian = Rp1.420.000, bonus +16. Beli lewat tombol "Beli RpX"
  per slot; hapus lewat "Hapus (+RpY)" — uang kembali 50%.
  Panel interior menampilkan jumlah perabot (N/10), bonus bahagia,
  dan "Sewa +25%" bila Kost lengkap. Di mode interior, 1–2 sprite
  warga chibi berdiri di dalam ruangan sebagai penghuni (kosmetik).
- **Efek interior** (GDD §5.6 + §6 + §7): hunian tanpa perabot sama
  sekali = penghuni tidak betah, kebahagiaan −5 per hunian kosong.
  Total bonus perabot masuk ke kebahagiaan dunia (dibatasi
  −20..+20) lewat perhitungan yang sama dengan fasilitas lain —
  bukan sistem kedua. **Kost yang 10 slotnya terisi penuh = sewa
  naik +25%** (Rp40.000 → Rp50.000/hari).
- **Peristiwa penghuni** (GDD §9.1, 6 peristiwa, peluang 10%/hari,
  hanya bila pemain punya minimal 1 Kost, paling banyak 1 panel
  pilihan per hari — sesudah roll kejadian biasa): anak kost tidak
  bisa bayar (3 pilihan: keringanan Rep +5 tapi sewa besok hilang /
  tagih tegas +Rp20.000, Rep −5, kamar kosong besok / cicil
  +Rp10.000 sekarang + Rp10.000 besok), anak kost melarikan diri
  (biarkan −Rp40.000, Rep −3 / laporkan −Rp15.000, Rep +5, kamar
  kosong besok), tetangga berisik (tegur Rep +3, bahagia +3 besok /
  abaikan bahagia −5 besok), merusak perabot (potong deposit
  +Rp25.000, Rep −3 / maafkan −Rp30.000, Rep +5), rekomendasi teman
  (terima Rep +5, bahagia +2 besok / tolak Rep −2), minta pindah
  kamar (setujui bahagia +3 besok / tolak Rep −2, bahagia −3 besok).
  Tidak ada pilihan yang selalu benar (uang vs reputasi vs
  kebahagiaan). Efek "besok" diterapkan di pergantian hari
  berikutnya; hasilnya dicatat di laporan harian.
- **Reputasi** (baru, GDD §9.1): mulai 50, 0–100, tampil di bar
  atas baris 2 ("Reputasi: N") di samping Warga dan Bahagia.
- **Misi interior** (2 misi tutorial baru): "Beli 1 Perabot"
  Rp25.000 dan "Lengkapi 1 Kost" Rp75.000 — klaim sekali seperti
  misi lain (panel Misi kini 10 tutorial + 3 harian = 14 baris).
- **Simpanan naik ke versi 5.0**: simpanan 4.0/3.0/2.0/1.0 tetap
  dimuat (reputasi simpanan lama 0 → 50 saat muat; interior kosong;
  efek peristiwa tidak ada); interior per bangunan, reputasi, dan
  efek "besok" peristiwa tersimpan. Interior bangunan yang dijual
  ikut hilang.

## Yang sudah berfungsi (Balai Kota + Gambar Ulang Kartun)

- **Balai Kota** (bangunan ke-18, fasilitas/landmark 2×2): terbuka
  di Level 3 (Desa), harga Rp3.000.000, perawatan Rp5.000/hari,
  kebahagiaan +5 (seperti fasilitas Fase 4 lain: +5 bila bersebelahan
  Jalan, +2 bila tidak) dan **reputasi +2 tiap hari** — sumber
  reputasi pasif pertama (sebelum ini reputasi hanya berubah lewat
  peristiwa penghuni). **Hanya 1 Balai Kota per dunia**: bila sudah
  dibangun, menu bangun menampilkan "Balai Kota - Sudah dibangun"
  (abu-abu, klik → toast penjelasan) dan `BuildManager` menolak
  pembangunan kedua.
- **Balai Kota di single player = fasilitas + sumber reputasi.**
  Wali Kota sebagai NPC yang mengurus izin tanah tetap di **Fase 7
  (multiplayer "Dunia Bersama")** — gedungnya sudah ada, orangnya
  menyusul.
- **Simpanan naik ke versi 5.1**: simpanan 5.0/4.0/3.0/2.0/1.0 tetap
  dimuat.
- **Semua sprite digambar ulang ke gaya kartun bersih** (dari kode
  `tools/generate-sprites.py`, tanpa unduhan): 18 bangunan, 3 warga,
  4 denah interior, 10 perabot (total 41 PNG; ikon Bangun dibiarkan,
  sudah disetujui user). Ciri gaya: outline gelap tipis, shading
  terang vs teduh, detail khas tiap bangunan, bayangan di bawah
  bangunan, palet hangat cerah. Dimensi kanvas tiap sprite TETAP —
  scene, sorting, dan bounds tidak berubah. Bukti visual:
  `tools/contact-sheet.png` (sudah diperiksa visual — lihat
  CATATAN-VERIFIKASI.md § Balai Kota untuk penilaian jujur).
- **Warga chibi kartun + frame jalan**: 3 varian warna baju, kini
  berlengan + bertangan, wajah hidup (mata, senyum, pipi merona).
  Tiap varian punya 2 frame jalan (kaki kiri/kanan maju) yang
  bergantian tiap 0,3 detik saat berjalan; sprite diam saat istirahat.

## Yang sudah berfungsi (Fase 5: Polish — Audio, Tutorial, Optimasi HP, Persiapan Build)

- **Audio (semua dari kode, tanpa unduhan)**: 6 SFX + 1 musik latar
  ambient 48 detik, disintesis sebagai WAV oleh
  `tools/generate-audio.py` (mono 44100 Hz, total 4,27 MB) —
  `sfx-tap` (ketuk tombol, 0,09 dtk), `sfx-bangun` (bangunan/perabot
  selesai, 0,37 dtk), `sfx-koin` (pemasukan harian bersih, 0,52 dtk),
  `sfx-naik-level` (arpeggio naik level, 0,94 dtk), `sfx-klaim`
  (klaim hadiah misi/pencapaian, 0,50 dtk), `sfx-tolak` (aksi
  ditolak: koin kurang/petak terisi/Balai Kota sudah ada, 0,30 dtk),
  `musik-latar` (pad chord C–Am–F–G + melodi pelan, loop mulus).
  `AudioManager.cs` (baru, di GameRoot) memutar SFX di momen yang
  tepat dan musik latar loop dengan volume terpisah
  (`GameBalance.VolumeMusik` 0,35); toggle Suara di Pengaturan tetap
  saklar utama (`AudioListener.volume` 0 = bisu total, musik ikut
  berhenti/berjalan). **Catatan jujur: asisten tidak bisa mendengar
  — penilaian enak/tidaknya suara ada di telinga user di Editor.**
- **Tutorial pemain baru (first-run, bisa dilewati)**: 6 langkah
  singkat — (1) ketuk petak kosong, (2) bangun Rumah Sederhana,
  (3) tombol palu+rumah membuka menu bangun, (4) hari berganti
  otomatis tiap 5 menit, (5) ketuk bangunan untuk detail & interior,
  (6) Misi & Pencapaian di bar atas. Panel kecil di bawah layar +
  tombol "Lanjut"/"Lewati"; muncul SEKALI saat pertama kali main
  (`PlayerPrefs DuniaKita_TutorialSelesai`); langkah (2) dilewati
  otomatis bila pemain sudah membangun rumah duluan; bisa diulang
  dari Pengaturan → "Ulangi Tutorial". Logika di
  `Utils/SistemTutorial.cs`, tampilan di `UI/PanelTutorial.cs`.
- **Panel perayaan pindah ke `UI/PanelPerayaan.cs`** (dulu di
  `UIManager.cs`) supaya `UIManager.cs` tetap di bawah 600 baris —
  isi dan perilakunya sama persis.
- **Optimasi HP**: Quality Settings Medium sebagai default (dan
  default Android) — `ProjectSettings/QualitySettings.asset`:
  pixelLightCount 1, tanpa bayangan real-time (game 2D sprite),
  tekstur penuh + kompresi, MSAA 2x, vSync mati; target 60 fps
  (`GameBalance.TargetFrameRate`, diterapkan `GameManager.Awake`).
  Semua 44 sprite memakai kompresi (`textureFormat -3` di .meta);
  tidak ada sprite duplikat/tak terpakai di scene; warga tetap maks
  12 sprite; interior (denah + perabot + warga penghuni) hanya
  dirender saat mode interior aktif. Lihat bagian "Kinerja".
- **Persiapan build Android** (build-nya sendiri di Fase 6, dari
  Unity Editor di laptop): `ProjectSettings.asset` sudah berisi
  perusahaan "Uhlabs", produk "Dunia Kita", package name
  `com.uhlabs.duniakita` (bisa diubah di Player Settings), versi 1.0
  (Android version code 1), orientasi Landscape Left/Right saja,
  dan ikon aplikasi `Assets/Sprites/ikon-aplikasi.png` (512×512,
  rumah + palu gaya kartun, dibuat `tools/generate-ikon-aplikasi.py`).

## Yang sudah berfungsi (Fase 6: Rilis — QA Akhir, Materi Play Store, Skrip Build)

- **QA akhir (Alpha)** — sapuan menyeluruh tanpa fitur baru:
  `tools/qa-fase6.py` memeriksa 18 bangunan dari berkas `.asset`
  (id unik, harga > 0, perawatan ≥ 0, level 1–6, kategori, ukuran,
  aturan hunian/komersial, sprite cocok ukuran + GUID + kompresi)
  dan menyilangkannya dengan `GameBalance.cs` + angka di README ini;
  `tools/test-fase6.cs` memeriksa rantai migrasi simpanan 1.0–5.1,
  versi game, hadiah naik level, progresi, pencapaian (total
  26 Permata), misi harian (8 kolam, 3/hari deterministik), ekonomi
  dasar, dan interior (10 slot, Rp1.420.000, +16). Hasil: QA 419/419
  dan tes 147/147, 0 gagal — **tidak ada bug data yang ditemukan**.
  Satu harapan tes yang salah (bonus interior kosong dikira 0,
  padahal −5 sesuai GDD §5.6) sudah diperbaiki di tesnya, bukan di
  game. Periksa juga: tidak ada `Debug.Log` yang spam tiap frame
  (semua log hanya saat kejadian: simpan/muat, akhir hari, bangun)
  dan tidak ada TODO/FIXME di script.
- **Skrip build Android** — `Assets/Editor/BuildAndroid.cs` (skrip
  Editor, tidak ikut ke game): `BuildAPK` (APK debug) dan `BuildAAB`
  (AAB release, `buildAppBundle = true`), hanya scene MainScene,
  keluaran di `C:\Dunia Kita\build\`, versi dari
  `GameBalance.VersiGame`. `build-android.bat` di root project:
  double-klik → pilih 1 (APK) atau 2 (AAB) → Unity batchmode
  membangun dan melaporkan hasilnya. Sebelum AAB pertama: buat
  keystore dulu (`Player Settings → Publishing Settings → Keystore
  Manager`) dan naikkan Bundle Version Code tiap rilis.
- **Materi rilis Play Store** (folder `../rilis/`): teks listing
  Bahasa Indonesia (judul, deskripsi singkat 79 karakter, deskripsi
  lengkap, kata kunci, kategori Simulasi, jawaban kuesioner rating
  konten IARC — tanpa kekerasan/iklan/pembelian/obrolan, perkiraan
  PEGI 3 / ESRB Everyone), `feature-graphic.png` 1024×500 (sudah
  diperiksa visual), kebijakan privasi (`.md` + `.html`, game tidak
  mengumpulkan data apa pun), panduan 8 tangkapan layar, dan rencana
  beta tertutup (APK ke 3–5 teman → perbaiki → AAB).
- **Matriks uji Alpha** (untuk user di Editor — lihat bawah, bagian
  "Matriks Uji Alpha (Fase 6)").
- **Yang BELUM terverifikasi** (jujur): build Android nyata, performa
  di HP nyata (fps/suhu/baterai), suara di telinga user, tutorial di
  layar, dan isi listing Play Store (butuh akun developer user).
  Lihat CATATAN-VERIFIKASI.md § Fase 6.

## Kinerja (target HP menengah, 60 fps)

- **Target**: HP Android menengah, 60 fps, landscape 1920×1080.
- **Yang sudah dioptimasi**: Quality Medium default (pixelLightCount
  1, tanpa bayangan real-time, MSAA 2x, vSync mati + targetFrameRate
  60); semua texture sprite terkompresi (format -3, maks 2048);
  musik latar di-streaming dari disk (bukan dimuat penuh ke memori);
  SFX pendek di-decompress saat muat (kecil, 8–83 KB per file);
  warga dibatasi 12 sprite di layar; interior hanya dirender saat
  mode interior aktif; tidak ada sprite duplikat di scene.
- **Yang belum teruji**: performa di HP nyata (fps, suhu, baterai)
  — baru bisa diukur sesudah build Fase 6 terpasang di HP user.

## Yang sudah berfungsi (Fase 7: Dunia Bersama + Wali Kota NPC)

Fase terakhir. Satu peta besar **24×24** dipakai banyak pemain; dunia
tersimpan di server Python (`../server/`, stdlib + SQLite) dan berjalan
walau pemain offline.

- **Server** (`../server/server.py` + `db.py`): daftar/masuk (kata sandi
  hash + salt, token acak), dunia bersama, izin tanah, bangun, jual,
  kas kota, sinkron sesi. Semua aksi divalidasi DI SERVER (koin,
  kepemilikan, harga dihitung ulang di server). Batas laju 60
  request/menit per IP. Jalankan: `python3 ../server/server.py`
  (default `http://127.0.0.1:8471`). Detail endpoint + aturan:
  `../server/README.md`.
- **Wali Kota NPC** (`Utils/SistemWaliKota.cs`): keputusan izin dari
  lokasi tanah (pusat Rp1.400.000, pinggir Rp200.000–300.000, naik
  Rp100.000 tiap langkah ke pusat), reputasi (≥70 diskon izin 30%,
  <30 mahal 50%, <15 ditolak otomatis), jenis bangunan (fasilitas
  didukung kota), kas kota (menipis +50%, penuh −20%), kepadatan
  kawasan (4+ sejenis ditolak). Hasil: **Disetujui / Ditolak
  (alasan jelas) / Bersyarat** (biaya ×1,5 + wajib Jalan di samping,
  kecuali bangunan Jalan sendiri — hanya biaya lebih mahal).
  Deterministik (variasi −5..+5 dari hash id pemain + petak).
  Ada Balai Kota di dunia → biaya izin −10%. Belum ada Balai Kota →
  izin tetap bisa diajukan (wali kota bekerja dari kantor sementara).
- **Akun** (Pengaturan → "Akun / Dunia Bersama"): daftar/masuk nama +
  kata sandi (tanpa email — akun game lokal server sendiri, bukan
  akun Google/Play). Token di PlayerPrefs `DuniaKita_Token`; kata
  sandi TIDAK PERNAH disimpan di HP.
- **Alur beli tanah**: ketuk petak kosong milik kota → panel izin →
  pilih bangunan → "Ajukan Izin" → keputusan wali kota (alasan +
  harga tanah + biaya izin + total) → "Bayar" → petak jadi milik
  (uang masuk kas kota) → baru boleh bangun. Petak milik pemain lain:
  ketuk → info pemilik + tidak bisa dibangun/dijual.
- **Sinkronisasi**: tiap 10 detik + sesudah tiap aksi — dunia server
  di-diff (tambah/lepas bangunan), koin + reputasi sesi didorong ke
  server (`POST /api/sinkron`, perubahan maks Rp6.000.000/sinkron —
  batas anti-curang sederhana; pembelian bangunan tetap divalidasi
  penuh di server). Label status di HUD: "Online • nama" /
  "Offline — main sendiri dulu".
- **Pemasukan offline**: saat masuk lagi, server menghitung pemasukan
  dari bangunanmu selama offline: (pemasukan − perawatan) per hari
  game − pajak 2,5%, **maks 24 hari game** (24 × 5 menit = 2 jam
  nyata). Tanpa pengali warung/kebahagiaan/hujan/jalan (penyederhanaan
  yang didokumentasikan — ekonomi detail tetap dihitung lokal saat
  bermain).
- **Fallback offline WAJIB**: bila server tidak terjangkau, game jalan
  normal sebagai single player lokal (simpanan 6.0/5.1 tetap dipakai,
  tanpa error, tanpa macet). Selama mode online, simpanan lokal TIDAK
  ditimpa — berkas lokal adalah cadangan dunia sendiri. Keluar dari
  mode online: dunia sendiri kembali setelah game dimuat ulang.
- **Migrasi simpanan**: versi naik ke **"6.0"** (5.1/5.0/4.0/3.0/2.0/1.0
  tetap dimuat). Petak single player yang sudah dibangun dianggap
  milik pemain itu saat masuk online pertama kali: klien mengirim
  daftar bangunannya ke `POST /api/impor-tanah` dan server
  mengakuinya TANPA bayar (impor hanya sekali, saat pemain belum
  punya tanah di server; interior perabot tidak ikut — hanya di
  save lokal).
- **Mode single player tetap 12→16** seperti sebelumnya; grid 24×24
  hanya di mode online. Level/hari/permata tetap dihitung lokal;
  koin + reputasi dicerminkan dari server.

## Yang BELUM ada (jujur)

- Obrolan antar pemain, papan peringkat online, wali kota sebagai
  karakter 3D di peta, pembangunan fasilitas kota otomatis dari kas
  kota (kas kota baru dicatat + ditampilkan), sinkronisasi real-time
  di bawah 1 detik (10 detik + sesudah aksi sudah cukup untuk city
  builder), deploy server ke VPS (tujuan deploy BELUM diputuskan —
  VPS utama melarang game server di ToS-nya; opsi: Node VPS lain /
  laptop sebagai host sementara, keputusan menunggu user).
- Build Android final + halaman Play Store: materinya sudah siap
  (Fase 6, folder `../rilis/` + `build-android.bat`) — tinggal
  dijalankan dari laptop ini (build + akun developer user).
- Monetisasi (sengaja tidak ada di prototipe).

## Matriks Uji Alpha (Fase 6)

Daftar periksa untuk user di Editor (Game View 16:9 Landscape).
Tiap baris: langkah → hasil yang diharapkan. Centang bila lolos;
laporkan baris yang gagal apa adanya.

| # | Uji | Hasil yang diharapkan |
|---|---|---|
| A1 | Bangun ke-18 bangunan (satu per satu, dari Level 1 sampai 6) | Semua bisa dibangun di petak kosong yang cukup besar; harga di menu sama dengan README |
| A2 | Capai Level 6 | Panel perayaan muncul tiap naik level; hadiah masuk (L2 Rp100rb, L3 Rp250rb, L4 Rp500rb, L5 Rp1jt, L6 Rp2jt) |
| A3 | Simpan-muat | Bangun 2 bangunan → tutup Play → tekan Play lagi → kedua bangunan masih ada, koin/hari sama |
| A4 | Tutorial muncul sekali | Hapus `PlayerPrefs` tutorial → Play → panel 6 langkah muncul; "Lanjut" sampai habis → Play lagi → tidak muncul |
| A5 | Audio mati/nyala | Musik + SFX terdengar saat Play; matikan Suara di Pengaturan → semua bunyi berhenti; nyalakan → musik jalan lagi |
| A6 | Interior + beli perabot | Ketuk Rumah → "Lihat Interior" → denah 4 ruangan; beli Kasur → koin −Rp150.000, Kasur muncul, bonus +2 |
| A7 | Peristiwa penghuni | Punya 1 Kost + `PeluangPeristiwaPenghuniPerHari` sementara 1.0 → tiap pergantian hari panel pilihan muncul; pilih satu → reputasi berubah |
| A8 | Misi harian berganti | Catat 3 misi hari ini → ubah `DurasiHariDetik` sementara 60 → tunggu 1 menit → 3 misi baru untuk hari berikut |
| A9 | Pencapaian diklaim sekali | Bangun 5 Kost → panel Pencapaian → "Juragan Kost" bisa diklaim +3 Permata; buka lagi → sudah diklaim, tidak bisa dua kali |
| A10 | Balai Kota maks 1 | Capai Level 3 → bangun Balai Kota → buka menu bangun → baris Balai Kota "Sudah dibangun"; reputasi +2 tiap hari |
| A11 | Aturan Jalan | Apartemen di samping Jalan → sewa penuh Rp60.000/hari; Apartemen jauh dari Jalan → sewa Rp30.000/hari |
| A12 | Perluasan lahan | Capai Level 5 → menu bangun → "Perluas Lahan" → grid 16×16, koin −Rp5.000.000 |

## Matriks Uji Online (Fase 7)

Server dulu, baru Editor. Di laptop (PowerShell/terminal):

```
cd "C:\Dunia Kita\server"      # atau folder server di sandbox: ~/workspace/dunia-kita/server
python server.py               # jalan di http://127.0.0.1:8471
```

Dua pemain nyata bersamaan TIDAK bisa dites dari satu Editor — cara
yang benar: **satu Editor + satu server lokal + akun kedua lewat
curl/tes** (atau dua HP sesudah build). Akun kedua: `curl` ke
`POST /api/daftar` + `/api/ajukan-izin` + `/api/bayar-izin` +
`/api/bangun` (lihat `../server/README.md` untuk contoh), lalu tekan
Play di Editor dan lihat bangunannya muncul di peta sesudah sinkron
(≤10 detik).

| # | Uji | Hasil yang diharapkan |
|---|---|---|
| O1 | Server mati → main sendiri | Tutup server → Play → label HUD "Offline — main sendiri dulu"; semua fitur Fase 1–6 jalan normal; bangun/jual/misi tidak error |
| O2 | Daftar + masuk | Pengaturan → "Akun / Dunia Bersama" → isi nama + kata sandi → "Daftar" → label "Online • nama"; tutup Play → Play lagi → otomatis masuk (token tersimpan) |
| O3 | Ajukan izin + bayar | Online → ketuk petak kosong milik kota → pilih Warung → "Ajukan Izin" → keputusan tampil (harga tanah + biaya izin + total) → "Bayar" → petak jadi milik; ketuk lagi → menu bangun aktif |
| O4 | Ditolak wali kota | Buat akun baru, turunkan reputasi (atau pilih petak padat) → ajukan izin → "Ditolak" + alasan jelas; tombol Bayar tidak ada |
| O5 | Bersyarat jalan | Ajukan Warung di petak bersyarat → "Bersyarat" + "wajib ada Jalan di samping" → bangun Warung ditolak server sampai Jalan dibangun di sampingnya |
| O6 | Tanah orang lain | Akun kedua (curl) beli tanah + bangun Rumah → di Editor ketuk petak itu → info pemilik; tombol bangun/jual tidak bisa dipakai |
| O7 | Pemasukan offline | Main online, bangun Warung → tutup game 15 menit → masuk lagi → toast pemasukan offline (±3 hari game, maks 24 hari) |
| O8 | Kas kota | Panel izin → "Kas Kota" → saldo = total harga tanah + biaya izin yang sudah dibayar; riwayat tampil |
| O9 | Keluar + kembali | Panel akun → "Keluar" → label kembali Offline; muat ulang game (tutup Play → Play) → dunia sendiri (simpanan lokal) utuh seperti sebelum online |
| O10 | Sinkron 2 arah | Di Editor bangun Rumah di tanah sendiri → cek `curl /api/dunia` → Rumah ada; akun kedua bangun Taman (curl) → ≤10 detik kemudian Taman muncul di Editor |

## Cara membuka di Unity Hub

1. Install **Unity Hub**, lalu install **Unity 2022.3.20f1 (LTS)**
   beserta modul **Android Build Support** (untuk build HP nanti).
2. Di Unity Hub: **Add → pilih folder `unity-project` ini** → buka project.
3. Tunggu Unity selesai mengimpor aset (pertama kali agak lama).
4. Buka scene **Assets → Scenes → MainScene.unity**.
5. Tekan **Play** (tombol ▶ di atas). Game berjalan di Game View.

## Kontrol yang bisa dites di Editor

Atur Game View ke aspek **16:9 Landscape** (mis. 1920×1080) supaya
tampilan sama seperti di HP mendatar.

| Aksi | Cara (Editor) | Cara (HP) |
|---|---|---|
| Pilih petak kosong | Klik kiri | Ketuk |
| Bangun (lewat petak) | Klik petak kosong → klik bangunan di menu | Ketuk petak → ketuk bangunan |
| Bangun (lewat tombol ikon) | Klik petak kosong, tutup menu, klik tombol ikon palu+rumah | Sama |
| Lihat detail bangunan | Klik bangunan | Ketuk bangunan |
| Lihat interior hunian | Panel detail → "Lihat Interior" → beli/hapus perabot → "Tutup Interior" | Sama |
| Jual bangunan | Panel detail → Jual Bangunan | Sama |
| Lihat misi | Tombol "Misi" (bar atas) → klaim hadiah bila selesai | Sama |
| Lihat pencapaian | Tombol "Pencapaian" (bar atas, emas) → klaim Permata bila tercapai | Sama |
| Misi harian | Panel "Misi" bagian bawah → 3 misi hari ini + hitung mundur | Sama |
| Perluas lahan (Level 5) | Menu bangun → tombol "Perluas Lahan ke 16x16" di atas daftar | Sama |
| Pengaturan suara | Tombol "Pengaturan" (bar atas) → Suara Nyala/Mati | Sama |
| Ulangi tutorial | Pengaturan → "Ulangi Tutorial" → panel panduan 6 langkah | Sama |
| Geser kamera | Drag mouse kiri | Drag 1 jari |
| Zoom | Roda mouse | Cubit 2 jari |
| Hari berganti | Otomatis tiap 5 menit (hitung mundur di HUD) — laporan muncul sendiri | Sama |
| Simpan | Otomatis (tiap hari + tiap 60 detik + saat keluar) | Sama |

**Uji cepat pergantian hari di Editor:** ubah sementara
`GameBalance.DurasiHariDetik` dari `300f` menjadi `60f`, tekan Play,
tunggu 1 menit — laporan harian harus muncul sendiri. Kembalikan ke
`300f` sesudah uji. Untuk uji naik level: bangun 5 Rumah Sederhana
(10 warga) lalu tunggu pergantian hari — panel perayaan "Duniamu naik
ke Level 2 — Dusun" harus muncul + koin bertambah Rp100.000. Untuk uji
Kost: sesudah Level 2, buka menu bangun — Kost tidak lagi abu-abu;
bangun di area kosong 2×2. Untuk uji Fase 4: di Level 4, bangun
Apartemen di samping Jalan — pemasukan sewa Rp60.000/hari penuh;
bangun Apartemen lain jauh dari Jalan — sewanya hanya Rp30.000/hari.
Untuk uji pencapaian: bangun 5 Kost lalu buka panel Pencapaian —
"Juragan Kost" bisa diklaim +3 Permata. Untuk uji perluasan lahan:
capai Level 5, buka menu bangun, klik "Perluas Lahan" — grid jadi
16×16 dan koin berkurang Rp5.000.000. Untuk uji Fase 4B: bangun 1
Rumah Sederhana, ketuk → "Lihat Interior" — bangunan berubah jadi
denah 4 ruangan (Kamar/Dapur/Mandi/Tamu) dengan 1 warga chibi di
dalam; beli Kasur (Rp150.000) — koin berkurang, Kasur muncul di
Kamar Tidur, panel detail menampilkan "Perabot: 1/10 • Bonus
bahagia: +2". Beli semua 10 slot di 1 Kost — panel detail
menampilkan "Sewa +25%" dan laporan harian hari berikut
menunjukkan sewa Kost Rp50.000 (bukan Rp40.000). Untuk uji
peristiwa penghuni: bangun 1 Kost, ubah sementara
`GameBalance.PeluangPeristiwaPenghuniPerHari` dari `0.10` menjadi
`1.0f` — tiap pergantian hari (5 menit, atau ubah juga
`DurasiHariDetik` ke `60f`) panel pilihan peristiwa harus muncul;
pilih salah satu — reputasi di HUD berubah dan hasilnya tercatat
di laporan harian. Kembalikan kedua angka sesudah uji. Untuk uji
Balai Kota: capai Level 3, buka menu bangun — Balai Kota
(Rp3.000.000) bisa dibangun di area kosong 2×2; sesudah dibangun,
buka menu bangun lagi — baris Balai Kota menampilkan "Sudah
dibangun" dan tidak bisa dibangun kedua kalinya; tunggu pergantian
hari — reputasi di HUD naik +2 dan kebahagiaan naik +5 (bila Balai
Kota bersebelahan Jalan). Untuk uji frame jalan: bangun 1 Rumah —
warga chibi berlengan berjalan antar petak dengan kaki bergantian;
saat berhenti, warga kembali ke pose diam. Untuk uji tutorial:
hapus dulu `PlayerPrefs` tutorial — di Editor, jalankan sekali
`PlayerPrefs.DeleteKey("DuniaKita_TutorialSelesai")` dari jendela
Console (atau hapus simpanan game), lalu tekan Play — panel
panduan harus muncul di bawah layar; tekan "Lanjut" sampai habis
(6 langkah) lalu tutup Play dan tekan Play lagi — tutorial tidak
muncul lagi. Untuk uji lewati otomatis: sebelum tutorial selesai,
bangun 1 Rumah Sederhana, lalu tekan "Lanjut" — langkah "Bangun
Rumah Sederhana" dilewati. Untuk uji audio: tekan Play dengan
speaker menyala — musik latar harus terdengar pelan dan berulang
tanpa putus; ketuk tombol apa pun (bunyi tap), bangun 1 Rumah
(bunyi bangun), klaim hadiah misi (bunyi klaim); matikan Suara di
Pengaturan — semua bunyi + musik berhenti; nyalakan lagi — musik
berjalan lagi. Bila suatu bunyi tidak terdengar, cek Inspector
GameRoot → AudioManager: 7 klip harus terisi (6 SFX + musik latar).

## Build ke HP Android (langkah demi langkah)

ProjectSettings sudah disiapkan (Fase 5): perusahaan "Uhlabs",
produk "Dunia Kita", package `com.uhlabs.duniakita` (bisa diubah di
Player Settings → Android → Other Settings → Identification),
versi 1.0 (version code 1), orientasi Landscape Left/Right, ikon
aplikasi dari `ikon-aplikasi.png`, kualitas Medium default Android.
Unity 2022.3.20f1 + Android Build Support sudah terpasang di laptop.

### Jalur A — otomatis (disarankan): build-android.bat

1. Double-klik `build-android.bat` di folder project ini.
2. Pilih **1** untuk APK debug (uji di HP) atau **2** untuk AAB
   release (Play Store).
3. Tunggu sampai selesai — build pertama 10–20 menit, berikutnya
   lebih cepat. Hasil: `C:\Dunia Kita\build\DuniaKita-v1.0.apk`
   (atau `.aab`). Log lengkap: `C:\Dunia Kita\build.log`.
4. Untuk APK: salin ke HP (kabel USB / Drive / WhatsApp), instal,
   izinkan "instal dari sumber tidak dikenal" sekali saja.
5. Sebelum AAB pertama: buat keystore dulu di Unity —
   `Player Settings → Publishing Settings → Keystore Manager`
   (tanpa keystore, build AAB gagal). Naikkan **Bundle Version
   Code** tiap rilis (`Player Settings → Android → Other
   Settings`).

### Jalur B — manual lewat Editor

1. Buka project ini di Unity Hub → buka scene MainScene.
2. `File → Build Settings` → pilih **Android** → klik
   **Switch Platform** (sekali saja; tunggu sampai selesai).
3. Periksa `Player Settings → Android`:
   - **Company Name**: Uhlabs, **Product Name**: Dunia Kita.
   - **Other Settings → Identification → Package Name**:
     `com.uhlabs.duniakita` (ubah bila perlu).
   - **Version**: 1.0, **Bundle Version Code**: 1 (naikkan tiap rilis).
   - **Resolution and Presentation → Default Orientation**:
     Landscape Left (rotasi ke Landscape Right diizinkan).
   - **Icon**: ikon rumah+palu sudah terisi.
   - **Quality → Android**: Medium (sudah default).
   - **Other Settings → Configuration → Scripting Backend**: IL2CPP
     (disarankan untuk rilis Play Store; Mono lebih cepat untuk uji
     internal), **Target Architectures**: ARM64 (wajib untuk Play
     Store; centang juga ARMv7 bila mau mendukung HP lama).
4. Di HP Android: aktifkan **Opsi developer → USB debugging**,
   hubungkan HP ke laptop dengan kabel USB, izinkan debugging di HP.
5. Di Build Settings: centang scene MainScene (sudah), klik
   **Build And Run** — Unity membangun APK lalu menginstalnya di HP.
6. Mainkan di HP: cek tutorial muncul sekali, musik + SFX terdengar,
   hari berganti tiap 5 menit, dan fps terasa mulus.
7. Untuk rilis Play Store: buat keystore
   (`Player Settings → Publishing Settings → Keystore Manager`),
   naikkan Version + Bundle Version Code tiap rilis, build **AAB**
   (`Build App Bundle (Google Play)`), lalu unggah ke Play Console.
   Materi listing ada di folder `../rilis/` (teks, feature graphic,
   kebijakan privasi, panduan screenshot, rencana beta).

### Bila build gagal

- Baca 20 baris terakhir `C:\Dunia Kita\build.log` (Jalur A) atau
  jendela Console di Editor (Jalur B) — penyebabnya tertulis di sana.
- Penyebab umum: Android Build Support belum terpasang (cek di Unity
  Hub → Installs → 2022.3.20f1 → Add Modules → Android Build
  Support); keystore belum dibuat (untuk AAB); ruang disk C: habis
  (build butuh beberapa GB kosong); JDK/SDK/NDK belum terpasang
  (ikut modul Android Build Support — lihat `tools/install-unity.bat`
  yang dipakai saat instalasi).
- Bila gagal karena satu baris error yang tidak dimengerti, salin
  baris itu apa adanya untuk diperiksa — jangan menebak-nebak.

Build iOS memerlukan Mac dengan Xcode — belum bisa dari laptop
Windows (lihat GDD §17).

## Struktur penting

```
Assets/
  Scenes/MainScene.unity        # Scene utama (Main Camera + GameRoot)
  Scripts/
    Managers/  GameManager, BuildManager, EconomyManager,
               CitizenManager, PejalanKaki, SaveManager, UIManager,
               CameraController, InteriorManager (Fase 4B),
               AudioManager (Fase 5: SFX + musik latar),
               ServerManager + ServerManagerAksi (Fase 7: klien
               REST Dunia Bersama — partial class, 2 berkas)
    Grid/      IsometricGrid, GridManager, BuildingView
    Data/      BuildingData (ScriptableObject), GameBalance
    UI/        UiFactory, PanelMisi, PanelPengaturan, PanelKejadian,
               PanelPencapaian, PanelInterior, PanelPeristiwaPenghuni
               (2 terakhir = Fase 4B), PanelPerayaan (Fase 5, pindah
               dari UIManager), PanelTutorial (Fase 5),
               PanelIzinTanah + PanelAkun (Fase 7)
    Utils/     EconomyCalculator, SaveData, FormatUtil,
               ProgresiDunia, SistemKejadian, PewaktuHari,
               SistemPencapaian, SistemMisiHarian, SistemInterior,
               SistemPeristiwaPenghuni (2 terakhir = Fase 4B),
               SistemBalaiKota (Balai Kota), SistemTutorial (Fase 5),
               SistemWaliKota, AturanDuniaBersama, HttpDuniaBersama,
               DtoDuniaBersama (4 terakhir = Fase 7)
  Audio/                        # 6 SFX + 1 musik latar (WAV, dibuat
                                # tools/generate-audio.py; total 4,27 MB)
  Data/Buildings/               # 18 file .asset (Rumah, Warung, Taman,
                                # Kost, Toko Kelontong, Jalan,
                                # Perpustakaan, Sekolah, Balai Kota,
                                # Pasar, Pos, Apartemen, Kafe, Klinik,
                                # Pembangkit, Perumahan, Kampus, Alun-alun)
  Editor/BuildAndroid.cs        # Skrip build Editor (Fase 6): BuildAPK
                                # + BuildAAB, dipanggil build-android.bat
  Sprites/                      # Sprite kartun proper (dibuat prosedural
                                # dari kode; 44 PNG: 18 bangunan +
                                # 9 warga [diam + 2 frame jalan/varian] +
                                # 4 denah interior + 10 perabot +
                                # tile/highlight/ikon-bangun) + ikon
                                # aplikasi 512x512 (ikon-aplikasi.png)
  Prefabs/Bangunan.prefab       # Prefab generik (diisi data saat runtime)
```

## Mengubah angka game

- Harga/pemasukan bangunan: ubah file `.asset` di `Assets/Data/Buildings/`
  (atau edit lewat Inspector di Unity) — tanpa menyentuh kode.
- Modal awal, ukuran grid, pajak kota, pengali warung, aturan kebahagiaan,
  hadiah misi: `Assets/Scripts/Data/GameBalance.cs`.

## Regenerasi file hasil generate

File `MainScene.unity`, `Bangunan.prefab`, file `.asset`, dan semua
`.meta` dibuat oleh `../tools/generate-project.py` (GUID deterministik).
Sprite dibuat oleh `../tools/generate-sprites.py`, audio oleh
`../tools/generate-audio.py`, ikon aplikasi oleh
`../tools/generate-ikon-aplikasi.py`, feature graphic oleh
`../tools/generate-feature-graphic.py`. QA data Fase 6:
`../tools/qa-fase6.py`; tes logika Fase 6: `../tools/test-fase6.cs`.
Bila menambah aset baru, jalankan ulang script yang sesuai lalu
jalankan `../tools/validasi-yaml.py` untuk memeriksa konsistensi GUID.

**Perhatian — jangan sampai perbaikan kritis ini hilang:**
- `Packages/manifest.json` WAJIB memakai `com.unity.modules.particlesystem`
  (bukan `com.unity.modules.particles` — nama lama itu tidak ada di
  Unity 2022.3 dan membuat project gagal dibuka). Generator sudah
  diperbaiki ke nama yang benar.
- `Assets/Scripts/UI/UiFactory.cs` WAJIB memakai font
  `LegacyRuntime.ttf` (bukan `Arial.ttf` — tidak berlaku lagi di
  Unity 2022.3). File `.cs` ini TIDAK ditulis ulang oleh generator
  (generator hanya membuat `.meta`-nya), jadi perbaikan font aman.
