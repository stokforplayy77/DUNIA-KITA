# Catatan Verifikasi — Fase 1–6: Rilis

**Tanggal:** 24 November 2026
**Status:** Struktur lengkap & terverifikasi sejauh yang bisa dilakukan
tanpa Unity Editor. **Game BELUM pernah dijalankan di Unity Editor.**
**Pembaruan 24 Nov sore:** UI dirombak dari portrait ke **landscape
1920×1080** (GDD §17) + tombol user dilengkapi (Bangun, Misi,
Pengaturan); verifikasi geometri UI 106/106 lolos (lihat §5, diperbarui Fase 2).
**Pembaruan 24 Nov malam — Fase 3:** progresi level 1–3, unlock Kost +
Toko Kelontong, misi tutorial Fase 3, kejadian acak pertama, lalu
**revisi desain user**: tombol "Lewati Hari" & "Simpan" DIHAPUS —
hari bergulir otomatis tiap 5 menit, simpan otomatis; tombol Bangun
jadi ikon saja. Verifikasi 159/159 UI, 132/132 tes Fase 3 (lihat § Fase 3).
**Pembaruan 24 Nov malam — Fase 4B:** interior tembus pandang 4 ruangan +
10 perabot, peristiwa penghuni 6 jenis + reputasi, 2 misi interior,
simpanan naik ke versi "5.0". Verifikasi: kompilasi 31 script 0 error,
tes Fase 4B 161/161, UI 675/675 (lihat § Fase 4B).
**Pembaruan 24 Nov malam — Balai Kota + Gambar Ulang Kartun:**
gedung Balai Kota (Level 3, Rp3.000.000, bahagia +5, reputasi +2/hari,
maks 1 per dunia), simpanan naik ke versi "5.1", semua sprite digambar
ulang ke gaya kartun bersih + warga chibi berframe jalan (lihat
§ Balai Kota). Verifikasi: kompilasi 32 script 0 error, 0 warning,
tes Balai Kota 42/42, tes lama 22/22 + 47/47 + 142/142 + 199/199 +
163/163, YAML/GUID lolos, UI 695/695, contact sheet diperiksa visual.
**Pembaruan 24 Nov malam — Fase 6 (Rilis):** QA akhir tanpa fitur
baru (qa-fase6.py 419/419 + test-fase6.cs 147/147, 0 gagal — tidak
ada bug data), skrip build Editor `BuildAndroid.cs` (BuildAPK +
BuildAAB) + `build-android.bat`, materi rilis Play Store di
`../rilis/` (listing, feature graphic 1024×500, kebijakan privasi,
panduan screenshot, rencana beta), matriks uji Alpha 12 baris di
README. Verifikasi: kompilasi 36 script runtime + BuildAndroid
0 error 0 warning, semua tes lama 671/671, YAML/GUID lolos,
UI 708/708 (lihat § Fase 6).

## Fase 2 — Ekonomi & Warga (24 Nov 2026)

**Status:** Selesai & terverifikasi sejauh yang bisa dilakukan tanpa
Unity Editor. **Game tetap BELUM pernah dijalankan di Unity Editor.**

### Yang berubah di Fase 2
- **Pajak kota 2,5%** dari pemasukan kotor tiap hari (per-mil 25, hitungan
  bulat ke bawah). Laporan harian menampilkan rincian: pemasukan warung,
  pemasukan lain, pajak, perawatan, bersih, kebahagiaan + efeknya ke warung.
- **Pengali warung dari kebahagiaan**: < 40% → 80%, 40–70% → 100%,
  > 70% → 110% (angka di `GameBalance`).
- **Tunggakan upkeep**: koin tidak cukup → koin 0, kekurangan dicatat
  sebagai tunggakan dan dilunasi otomatis dari pemasukan hari berikutnya;
  menunggak 2 hari berturut-turut → kebahagiaan -5. Peringatan di laporan
  + toast. Tidak ada bangkrut langsung.
- **Kebahagiaan Fase 2**: 50 + 2/taman (maks +20) + 5 bila semua warga
  punya rumah − 3 bila warga melebihi kapasitas − 5 bila menunggak 2 hari
  berturut-turut; dibatasi 0–100.
- **Warga berjalan**: `PejalanKaki.cs` baru — sprite chibi (3 varian warna,
  `warga1/2/3.png`, digambar dari kode) berjalan antar petak kosong acak
  (1,2 unit/detik, istirahat 1–3 detik, ayunan langkah ±0,05 unit).
  Maks 12 sprite tampil; populasi = kapasitas hunian.
- **Simpanan versi "2.0"**: field baru populasi, kebahagiaan, tunggakan,
  hari menunggak berturut-turut. Simpanan "1.0" tetap dimuat (field baru
  default 0). Posisi warga TIDAK disimpan — dibangkitkan ulang acak saat muat.
- **Misi baru**: "Lewati 3 Hari" (Rp25.000) dan "Capai kebahagiaan 60%"
  (Rp50.000), memakai sistem klaim yang sudah ada. Panel misi diperbesar
  (760×800, 5 baris); panel laporan diperbesar (760×700).
- **Biaya perawatan dipertahankan dari Fase 1** (Rumah Rp1.000, Warung
  Rp2.000, Taman Rp500/hari di file `.asset`) — angka contoh di perintah
  tugas (Rp2.000/Rp5.000/Rp1.000) sengaja TIDAK dipakai supaya simulasi
  10 hari tetap sehat dan tes Fase 1 tetap valid.

### Verifikasi Fase 2 (semua dijalankan, dari biner baru)
- **Kompilasi**: `mcs -target:library -warn:4` terhadap 19 script + stub —
  0 error, 0 warning. File terpanjang 374 baris (UIManager), di bawah 600.
- **Tes ekonomi (diperbarui)**: 22/22 lolos — dunia standar kini bersih
  Rp10.125/hari (masuk 15.000, pajak 375, upkeep 4.500); 10 hari:
  5.000.000 → 5.101.250; kebahagiaan 57 (50+2+5).
- **Tes Fase 2 baru** (`tools/test-fase2.cs`): **47/47 lolos** — pengali
  warung di 8 titik batas (39→80%, 40→100%, 70→100%, 71→110%); pajak
  (375, 412); ringkasan lengkap (bahagia 30 → bersih 7.200; bahagia 80 →
  11.588); 5 kasus pembayaran/tunggakan; 8 kasus kebahagiaan; 3 simulasi
  10 hari (5.101.250 / 5.261.750 / 5.072.000); migrasi versi (2.0 & 1.0
  didukung, 0.9/kosong/null ditolak, default field Fase 2 = 0).
- **YAML/GUID**: `validasi-yaml.py` lolos — 54 file `.meta`, scene 16 objek,
  semua guid ter-resolve. Scene `MainScene.unity` hasil edit tangan
  diverifikasi SAMA PERSIS (byte-identik) dengan keluaran
  `generate-project.py` (dijalankan di folder /tmp) — generator sudah
  diperbarui untuk Fase 2 (sprite warga + `spriteWarga` di scene +
  `PejalanKaki.cs` + `PanelMisi.cs`/`PanelPengaturan.cs` di daftar script).
- **UI**: `cek-ui-bounds.py` **159/159 lolos** (81 elemen dalam batas
  1920×1080, 26 tombol ≥90, tidak ada tombol tumpang tindih, panel
  18–37% layar).
- **Sprite warga diperiksa visual**: `warga1.png` (baju merah) tampil
  sebagai sosok chibi jelas — rambut, mata, baju, kaki, bayangan;
  sprite lama tidak berubah (hash md5 sama sebelum/sesudah regenerate).
- **Perbaikan kritis dipertahankan**: manifest `particlesystem` (bukan
  `particles`), font `LegacyRuntime.ttf` (bukan `Arial.ttf`), landscape
  1920×1080 — ketiganya dicek ulang dengan grep (0 kemunculan nilai lama).

### Yang BELUM terverifikasi di Fase 2 (jujur dicatat)
- **Warga berjalan belum pernah terlihat di Editor** — gerakan, ayunan
  langkah, dan sorting sprite vs bangunan baru teruji di level logika
  (kode), bukan di layar. Cek di Game View: warga harus berjalan antar
  petak kosong dan berhenti sejenak sebelum pindah target.
- **Alur tunggakan belum pernah dimainkan** — butuh koin sengaja dihabiskan
  (bangun banyak taman tanpa warung, lalu lewati hari) sampai menunggak,
  lalu periksa laporan + toast + kebahagiaan turun di hari ke-2.
- **Migrasi simpanan 1.0 nyata belum diuji** — hanya aturan versinya yang
  dites (logika murni). Bila laptop user masih punya simpanan Fase 1,
  game harus memuatnya tanpa error dan melanjutkan dengan tunggakan 0.
- **Posisi warga saat muat** (acak ulang) belum dilihat — warga harus
  muncul kembali di petak acak, bukan menumpuk di satu titik.

## Fase 3 — Progresi Level 1–3 + Kejadian Acak + Revisi Desain Hari Otomatis (24 Nov 2026)

**Status:** Selesai & terverifikasi sejauh yang bisa dilakukan tanpa
Unity Editor. **Game tetap BELUM pernah dijalankan di Unity Editor.**

### Yang berubah di Fase 3
- **Level dunia 1–3 (GDD §8)**: Level 2 Dusun = 10 warga (hadiah
  Rp100.000); Level 3 Desa = 30 warga + kebahagiaan ≥60% (hadiah
  Rp250.000). Naik level otomatis di akhir hari dengan panel perayaan
  ("Selamat! Duniamu naik ke Level 2 — Dusun"). Level 4–6 BELUM dibuka
  (dibatasi `GameBalance.LevelMaksFase3 = 3`; `BisaNaikKe(4, ...)`
  selalu false). Nama level tampil di HUD (sudah ada sejak Fase 1).
- **Unlock bangunan**: Kost 2×2, 8 warga, Rp2.000.000, sewa
  Rp40.000/hari, perawatan Rp5.000/hari (Level 2); Toko Kelontong 1×1,
  Rp1.500.000, +Rp30.000/hari, perawatan Rp4.000/hari (Level 3).
  Bangunan terkunci tampil abu-abu di menu bangun (nama + syarat level
  terlihat, klik hanya menampilkan syarat lewat toast). Field baru
  `BuildingData.levelBuka` (default 1); `.asset` lama tanpa field ini
  diperlakukan terbuka (levelBuka 0 → 1). Penempatan 2×2 memakai
  `GridManager.ApakahKosong/TandaiTerisi` yang sudah ada; posisi
  visual memakai titik tengah area (`IsometricGrid.GridKeDuniaTengah`)
  dan sorting memakai sudut paling depan supaya tidak tertutup bangunan
  1×1 di depannya; `BuildManager.CariBangunanDi` kini mengenali
  petak manapun di dalam area bangunan 2×2.
- **Misi tutorial Fase 3**: "Naik ke Level 2" (Rp50.000), "Bangun 1
  Kost" (Rp75.000), "Capai 10 Warga" (Rp50.000) — memakai sistem
  PanelMisi + klaim yang sudah ada. Misi Fase 2 "Lewati 3 Hari"
  diganti namanya menjadi **"Bertahan 3 Hari"** (revisi desain; id
  `misi_3hari` tidak berubah supaya klaim lama tetap tercatat).
  Panel misi diperbesar (760×1000, 8 baris); panel bangun diperbesar
  (760×880, 5 baris); panel laporan diperbesar (760×760).
- **Kejadian acak pertama (GDD §9, peluang 20% tiap pergantian hari)**:
  Festival Desa (bayar Rp50.000 → kebahagiaan +10 besok / tolak),
  Hujan Deras (pemasukan warung -20% besok / siapkan payung Rp20.000
  → hanya -10%), Pedagang Keliling (beli Rp100.000 → +1 Permata /
  tolak). Panel pilihan baru `PanelKejadian.cs` (760×640, 2 tombol
  berdampingan); efek dicatat di laporan harian. TIDAK ada pilihan
  yang selalu benar. Kejadian penghuni GDD §9.1 BELUM dikerjakan
  (rencana Fase 4).
- **Simpanan versi "3.0"**: field baru bonus kebahagiaan besok +
  pengali hujan besok (keadaan kejadian). Simpanan "2.0" dan "1.0"
  tetap dimuat (field baru default 0; pengali hujan 0 dinormalkan
  menjadi 100 saat muat).

### Revisi desain user (24 Nov 2026, diprioritaskan di atas Fase 3 awal)
- **Tombol "Lewati Hari" DIHAPUS.** Hari bergulir OTOMATIS waktu
  nyata: 1 hari game = 5 menit (`GameBalance.DurasiHariDetik = 300`).
  `GameManager.Update` menjalankan `PewaktuHari` (logika murni baru,
  `Utils/PewaktuHari.cs`); saat pewaktu habis, `ProsesPergantianHari`
  menjalankan proses yang sama seperti Lewati Hari lama (ekonomi →
  cek level → roll kejadian → laporan → simpan). Selama panel
  perayaan/kejadian terbuka, pewaktu hari DIJEDA. Kelebihan waktu
  (mis. aplikasi dijeda lama) dibuang — maksimal 1 hari per batas.
- **Tombol "Simpan" DIHAPUS.** Simpan OTOMATIS: tiap pergantian hari
  + tiap 60 detik (`GameBalance.IntervalSimpanOtomatisDetik`) + saat
  membangun/menjual + saat aplikasi dijeda/ditutup. Logika
  `SaveManager.Simpan()` tidak berubah, hanya pemicunya.
- **Tombol "Bangun" jadi IKON saja** (tanpa tulisan): sprite baru
  `ikon-bangun.png` (256×256, palu + rumah kecil, digambar dari kode
  PIL) dipakai lewat helper baru `UiFactory.BuatTombolIkon`
  (ikon 96×96 di tengah tombol 560×120, preserveAspect). Tombol
  Misi & Pengaturan di bar atas tetap berteks.
- **HUD menampilkan hitung mundur** hari: "Hari 2 • 03:12"
  (`GameManager.TeksSisaHari()`; `PerbaruiHUD` dipanggil tiap frame
  oleh `GameManager.Update` supaya hitung mundur bergerak).

### Verifikasi Fase 3 (semua dijalankan, dari biner baru)
- **Kompilasi**: `mcs -target:library -warn:4` terhadap 24 script +
  stub — **0 error, 0 warning**. File terpanjang 358 baris (UIManager
  — setelah revisi; PanelMisi 197, GameManager ~330), semua di bawah 600.
  Catatan: kompilasi pertama sempat gagal 1 error (`Random` tidak ada
  di stub) — stub dilengkapi `UnityEngine.Random` (nilai 0,5
  deterministik) + field `Image.sprite`/`preserveAspect` untuk
  `BuatTombolIkon`.
- **Tes ekonomi**: **22/22 lolos** (tidak berubah).
- **Tes Fase 2** (`tools/test-fase2.cs`, diperbarui): **47/47 lolos**
  (sebelumnya 45 — Tes G tambah 2 pemeriksaan: versi 3.0 didukung +
  field Fase 3 default 0).
- **Tes Fase 3 baru** (`tools/test-fase3.cs`): **138/138 lolos**
  (diperbarui Fase 4 — sebelumnya 132; tambah 6: hadiah L4/L5/L6 +
  BisaNaikKe L4 di batasnya + 3 asersi versi di Tes J) —
  syarat level di batasnya (L2 9 vs 10 warga; L3 bahagia 59 vs 60;
  L4 ditolak walau syarat terpenuhi); unlock per level + teks syarat;
  roll kejadian di 9 titik (0,00/0,03 festival; 0,07/0,13 hujan;
  0,14/0,199 pedagang; 0,20/0,50 tidak ada); teks + biaya tiap
  kejadian; efek pilihan (festival +10, hujan 90/80, pedagang tanpa
  efek keadaan); ekonomi Fase 3 (dunia 5 rumah + 1 kost + 1 warung +
  1 toko + 2 taman: populasi 18, bahagia 59, bersih Rp65.875/hari;
  hujan 80% → Rp57.100; hujan + payung 90% → Rp61.488; pajak
  2.125/1.900/2.012); titik tengah 2×2 ((0,0)→(0,-5,0); (5,5)→(0,0));
  simulasi 10 hari (5.658.750 normal / 5.571.000 hujan terus);
  migrasi 2.0→3.0 (3.0/2.0/1.0 didukung, 4.0 ditolak);
  **Tes K pewaktu hari** (300 detik awal, "05:00"; 299 detik →
  "00:01"; +1 detik → hari berganti + reset 300; 192 detik → "03:12";
  600 detik sekaligus → tetap 1 hari; negatif diabaikan).
- **YAML/GUID**: `validasi-yaml.py` lolos — 54 file `.meta`, scene
  16 objek, 5 file `.asset` (termasuk Kost + TokoKelontong yang baru
  ditambahkan ke daftar periksa validator). Scene hasil edit tangan
  diverifikasi SAMA dengan keluaran `generate-project.py` kecuali
  perubahan yang diharapkan (2 entri daftarBangunan baru + field
  `ikonBangun` di UIManager); generator sudah diperbarui (script
  ProgresiDunia/SistemKejadian/PewaktuHari/PanelKejadian, sprite
  kost/toko/ikon-bangun, `.asset` Kost/TokoKelontong, template
  `.asset` dengan field `levelBuka` + ukuran parametrized).
  `.asset` lama (Rumah/Warung/Taman) hanya bertambah 1 baris
  (`levelBuka: 1`). Sprite lama tidak berubah (hash md5 sama).
  Catatan: regenerasi kali ini JUGA membuat `.meta` PanelMisi.cs /
  PanelPengaturan.cs yang ternyata belum ada sebelumnya (tertinggal
  dari Fase 2) — kini lengkap.
- **UI**: `cek-ui-bounds.py` **159/159 lolos** (81 elemen dalam batas
  1920×1080; tombol ikon Bangun 560×120 di (680,950), tinggi 120 ≥ 90;
  2 tombol kejadian berdampingan tidak tumpang tindih; panel 18–37%
  layar). Alat cek diperbarui: rect panel bangun/laporan/misi baru,
  5 baris menu + 8 baris misi, PanelKejadian + PanelPerayaan.
- **Sprite baru diperiksa visual**: `kost.png` (512×256, bangunan dua
  lantai atap marun, 6 jendela, pintu, papan nama) dan `toko.png`
  (atap hijau, papan nama, awning, rak barang) terbaca jelas sebagai
  placeholder; `ikon-bangun.png` (palu abu + rumah atap terakota,
  pintu/jendela, alas rumput) terbaca jelas sebagai ikon.
- **Perbaikan kritis dipertahankan**: manifest `particlesystem` (1×,
  `particles` lama 0×), font `LegacyRuntime.ttf` (2 pemakaian kode,
  `Arial` 0×), landscape 1920×1080 — dicek ulang dengan grep.

### Yang BELUM terverifikasi di Fase 3 (jujur dicatat)
- **Alur pergantian hari otomatis belum pernah terlihat di Editor** —
  pewaktu 5 menit, panel perayaan, panel kejadian, dan laporan
  otomatis baru teruji di level logika. Cek di Game View: hitung
  mundur HUD harus berkurang tiap detik; sesudah 5 menit laporan
  harian harus muncul sendiri tanpa tombol apa pun.
- **Naik level + unlock Kost/Toko belum pernah dimainkan** — butuh
  5 Rumah Sederhana (10 warga) lalu tunggu pergantian hari; panel
  perayaan + hadiah Rp100.000 harus muncul; Kost harus tidak lagi
  abu-abu di menu bangun dan bisa dipasang di area kosong 2×2.
- **Kejadian acak belum pernah muncul di layar** — peluang 20%/hari
  berarti rata-rata 1 kejadian tiap 5 hari; untuk uji cepat, mainkan
  beberapa hari (atau ubah sementara `PeluangKejadianPerHari` di
  GameBalance menjadi 1.0 lalu kembalikan ke 0.20).
- **Migrasi simpanan 2.0 nyata belum diuji** — hanya aturan versinya
  yang dites (logika murni). Bila laptop user masih punya simpanan
  Fase 2, game harus memuatnya tanpa error dan melanjutkan dengan
  level + keadaan kejadian default.
- **Tombol ikon Bangun belum dilihat di layar** — geometri lolos cek
  (560×120), tapi keterbacaan ikon palu+rumah pada ukuran tampil
  harus dicek di Game View 16:9.

## Fase 4 — Konten Penuh: Level 4–6 + 12 Bangunan + Jalan + Perluasan Lahan + Pencapaian + Misi Harian (24 Nov 2026)

### Yang berubah di Fase 4
- **Level dunia 4–6** (GDD §8): L4 Kota Kecil = 80 warga + bahagia
  ≥70% (hadiah Rp500.000); L5 Kota = 150 warga + bahagia ≥75%
  (hadiah Rp1.000.000); L6 Metropolis = 300 warga + bahagia ≥80%
  (hadiah Rp2.000.000). `GameBalance.LevelMaksDunia` naik 3 → 6
  (menggantikan `LevelMaksFase3`). Panel perayaan kini menghitung
  bangunan yang terbuka di level baru dari `daftarBangunan`
  (sebelumnya hardcode "Kost!"/"Toko Kelontong!").
- **12 bangunan baru** (total 17, urutan menu: Rumah, Warung, Taman,
  Jalan; Kost, Toko, Perpustakaan; Sekolah, Pasar, Pos; Apartemen,
  Kafe, Klinik, Pembangkit; Perumahan, Kampus; Alun-alun):
  Jalan 1×1 Rp50.000 (L1); Perpustakaan 1×1 Rp800.000, bahagia +2
  (L2); Sekolah Dasar 2×2 Rp3.000.000, bahagia +4 (L3); Pasar 2×2
  Rp4.000.000, +Rp60.000/hari (L3); Pos Keamanan 1×1 Rp1.000.000,
  bahagia +2 (L3); Apartemen Kecil 2×2 Rp5.000.000, 12 warga, sewa
  Rp60.000/hari (L4); Kafe 1×1 Rp2.000.000, +Rp25.000/hari, bahagia
  +2 (L4); Klinik 2×2 Rp4.000.000, bahagia +4 (L4); Pembangkit
  Listrik 2×2 Rp6.000.000, bahagia +4 (L4); Perumahan 3×3
  Rp8.000.000, 20 warga, sewa Rp100.000/hari (L5); Kampus Kecil 3×3
  Rp10.000.000, +Rp80.000/hari, bahagia +6 (L5); Alun-alun Kota 3×3
  Rp12.000.000, bahagia +10 (L6). Field baru `BuildingData.bonusBahagia`
  (Taman = 0, dihitung khusus lewat jumlahTaman); semua `.asset`
  lama hanya bertambah 1 baris (`bonusBahagia: 0`).
- **Aturan Jalan** (GDD §5.5, diputuskan di Fase 4 — GDD hanya
  menyebut Jalan sebagai penghubung): bangunan yang bersebelahan
  dengan Jalan (4 arah, TIDAK diagonal) berfungsi penuh; yang tidak
  bersebelahan, pemasukan dan bonus kebahagiaannya −50%
  (`GameBalance.PengaliTanpaJalan = 50`). Kapasitas hunian TIDAK
  terpengaruh (warga tetap tinggal). Jalan itu sendiri tidak butuh
  jalan. Dampak migrasi: dunia Fase 3 tanpa Jalan kehilangan
  setengah pemasukan komersial + sewa sampai pemain membangun Jalan
  (murah Rp50.000) — disengaja, supaya Jalan punya fungsi nyata.
- **Perluasan lahan** (GDD §8, L5): di Level 5, grid 12×12 → 16×16
  seharga Rp5.000.000 lewat tombol di atas menu bangun. Diputuskan
  DIIMPLEMENTASIKAN (bukan dilewat): 300 warga Level 6 hampir tidak
  muat di 144 petak (12 Apartemen 144 warga/48 petak + 8 Perumahan
  160 warga/72 petak = 304 warga di 120 petak, sisa 24 petak untuk
  toko/jalan/taman — terlalu sempit, apalagi Jalan kini wajib untuk
  pemasukan penuh). Risiko dikelola: perubahan aditif (metode baru
  `IsometricGrid.AturUkuran` + `GridManager.Perluas`), perilaku
  12×12 tidak berubah bila tidak diperluas; pusat grid tetap (0,0)
  dunia sesudah perluasan.
- **Pencapaian** (GDD §8, baru `Utils/SistemPencapaian.cs` +
  `UI/PanelPencapaian.cs`): 6 pencapaian — Juragan Kost (5 Kost,
  3 Permata), Kota Pelajar (Sekolah + Kampus, 3 Permata), Kota Hijau
  (20 Taman, 2 Permata), Metropolis (Level 6, 10 Permata), Juragan
  Pasar (3 komersial, 3 Permata), Kota Sejahtera (bahagia ≥80%,
  5 Permata). Tombol "Pencapaian" emas di bar atas (di antara Misi
  dan Pengaturan). Klaim sekali per pencapaian; status klaim di
  `SaveData.pencapaianDiklaim` (bukan PlayerPrefs); toast sekali per
  sesi saat pencapaian baru tercapai.
- **Misi harian** (GDD §9, baru `Utils/SistemMisiHarian.cs`):
  kolam 8 misi, 3 misi per hari dipilih deterministik dari nomor
  hari (`((hari-1)×3) % 8`, melilit) — hari 1 = Bangun/Taman/
  Bahagia65, hari 2 = Pemasukan/Warga15/Komersial3, hari 3 =
  Bahagia75/Taman5/Bangun (melilit), siklus 8 hari. Hadiah
  Rp25.000–Rp40.000 koin, 1 Permata untuk Bahagia 75%. Tampil di
  panel misi bagian "Misi Harian" (di bawah 8 misi tutorial) dengan
  hitung mundur ke pergantian hari; klaim memakai kunci PlayerPrefs
  berhari (`DK_Harian_<hari>_<id>`) sehingga misi hangus saat hari
  berganti. Koreksi desain: misi "Pemasukan Rp50.000" dinilai dari
  PERKIRAAN pemasukan harian (`EconomyManager.PerkiraanHarian`),
  bukan pemasukan yang sudah dibayar — karena pemasukan riil baru
  diketahui saat hari berakhir, saat itu misinya sudah berganti set
  baru.
- **Simpanan naik ke versi "4.0"** (`SaveData` field baru:
  `ukuranGrid`, `pencapaianDiklaim`, `hariMisiTerakhir`,
  `dibangunHariIni`, `tamanHariIni`); simpanan 3.0/2.0/1.0 tetap
  dimuat (field baru default aman; `UkuranGridEfektif(0) = 12`).
  `GameManager` memulihkan pencapaian + penghitung harian (hanya
  bila `hariMisiTerakhir == hari`, bila tidak reset 0) dan memperluas
  grid SEBELUM memuat bangunan bila simpanan >12.
- **UI**: bar atas dirombak — 3 tombol (Misi #E07840, Pencapaian
  emas #C99A2B, Pengaturan #5A6B7A); menu bangun + panel misi kini
  bergulir (`UiFactory.BuatScroll`, ScrollRect + RectMask2D) karena
  17 bangunan + 12 baris misi tidak muat; tombol perluasan lahan di
  atas menu bangun; label laporan "Pemasukan warung" →
  "Pemasukan komersial" (kini mencakup Toko/Pasar/Kafe).
- **Sprite baru (12, PIL, tanpa unduhan)**: jalan, perpustakaan,
  pos, kafe (256×256); sekolah, pasar, apartemen, klinik,
  pembangkit (512×256); perumahan, kampus, alun-alun (768×384).
  Sprite lama tidak berubah (hash md5 sama).

### Verifikasi Fase 4 (semua dijalankan, dari biner baru)
- **Kompilasi**: `mcs -target:library -warn:4` terhadap **26 script**
  + stub — **0 error, 0 warning**. File terpanjang 535 baris
  (UIManager), semua di bawah 600.
- **Tes ekonomi**: **22/22 lolos** (tidak berubah).
- **Tes Fase 2** (`tools/test-fase2.cs`, diperbarui): **47/47 lolos**
  — satu asersi versi usang diperbarui (simpanan baru kini berversi
  4.0, bukan 3.0). BUKAN regresi: versi default memang harus versi
  saat ini. Semua angka ekonomi Fase 2 tidak berubah.
- **Tes Fase 3** (`tools/test-fase3.cs`, diperbarui): **138/138
  lolos** (sebelumnya 132 — tambah 6: hadiah L4 Rp500.000, L5
  Rp1.000.000, L6 Rp2.000.000; BisaNaikKe L4 di batasnya; dan 3
  asersi versi di Tes J diperbarui ke 4.0, termasuk "versi 4.0
  ditolak" → kini didukung). Batas sementara Fase 3 (`LevelMaksFase3
  = 3`) dicabut dengan sengaja — dicatat di sini, bukan kerusakan.
- **Tes Fase 4 baru** (`tools/test-fase4.cs`): **194/194 lolos** —
  Tes A syarat level 4–6 di batasnya (L4 79/80 warga, bahagia 69/70;
  L5 149/150, 74/75; L6 299/300, 79/80; hadiah 500rb/1jt/2jt; total
  hadiah L2–L6 Rp3.850.000); Tes B unlock 17 bangunan (terbuka di
  levelnya, terkunci satu level di bawah); Tes C aturan Jalan
  (kanan/atas/bawah 1×1 bersebelahan, diagonal TIDAK, jauh TIDAK;
  tepi 2×2/3×3; jalan sendiri false; daftar kosong aman); Tes D
  bonus fasilitas (perpustakaan +2 → +1 tanpa jalan; kampus +6 → +3;
  alun-alun +10 → +5; 2 taman tanpa jalan −2; campuran 2+1−1=2;
  dunia kaya bonus 34; kebahagiaan dunia kaya 93; cap 100); Tes E
  ekonomi dunia kaya (dengan jalan: komersial 143000, lain 320000,
  kotor 463000, pajak 11575, upkeep 139000, bersih 312425; tanpa
  jalan: 71500/160000/231500/5787/86713; populasi 56); Tes F 6
  pencapaian di batasnya (4 vs 5 Kost, 19 vs 20 Taman, 79 vs 80
  bahagia, dst.) + hadiah Permata 3/3/2/10/3/5 + id tidak dikenal
  ditolak; Tes G misi harian (rotasi hari 1/2/3/4, siklus 8 hari,
  deterministik, tanpa duplikat; ApakahSelesai tiap misi di
  batasnya; hadiah 25000–40000 + 1 Permata; kunci klaim berhari);
  Tes H simulasi 10 hari dunia standar Fase 4 (kebahagiaan 63;
  bersih Rp136.750/hari → koin 6.367.500; tanpa jalan Rp53.875/hari
  → 5.538.750; hujan terus Rp111.400/hari → 6.114.000); Tes I
  migrasi (4.0/3.0/2.0/1.0 didukung, 5.0/kosong/null ditolak;
  UkuranGridEfektif 0/negatif → 12, 16 → 16; field Fase 4 default
  aman); Tes J perluasan grid (pusat 16×16 (0,0) = (0,-7.5);
  round-trip (7,3)/(15,15)/(0,0); batas dunia 34×17 vs 26×13;
  AturUkuran(0,0)/(-1,5) ditolak).
  Catatan: 6 pemeriksaan Tes H sempat gagal di percobaan pertama
  karena salah hitung tangan penyusun tes (upkeep dunia standar
  ditulis 30000, seharusnya 29000) — kode yang benar; ekspektasi
  tes diperbaiki dan lolos 194/194.
- **YAML/GUID**: `validasi-yaml.py` lolos — 81 file `.meta`, scene
  33 GUID unik, 17 file `.asset`. Perbaikan di tengah jalan: 3
  `.asset` baru (PosKeamanan, Kampus, AlunAlun) sempat gagal parse
  karena deskripsi mengandung ": " (titik dua + spasi = pemisah
  mapping YAML) — diperbaiki menjadi koma di generator, lalu
  regenerasi lolos penuh. Scene hasil edit tangan diverifikasi SAMA
  dengan keluaran `generate-project.py` kecuali perubahan yang
  diharapkan (daftarBangunan 5 → 17 entri); `.asset` lama hanya
  bertambah 1 baris (`bonusBahagia`); ProjectSettings + manifest
  tidak berubah.
- **UI**: `cek-ui-bounds.py` **453/453 lolos** (150 elemen dalam
  batas horizontal; panel 18–37% layar; PanelPencapaian 760×900 =
  33%). Alat cek diperbarui: jumlah baris per (berkas, metode)
  (menu bangun 17, misi 12, pencapaian 6); metode bergulir hanya
  dicek horizontal (baris di bawah viewport sengaja di luar layar);
  dedupe entri identik sebelum cek tumpang tindih (tombol perluasan
  lahan berposisi literal dievaluasi 17× tanpa dedupe akan positif
  palsu).
- **Sprite baru diperiksa visual** (contact sheet 12 sprite):
  jalan (aspal + garis kuning putus-putus), perpustakaan (atap biru
  + kolom + buku), pos (gardu biru + palang), kafe (atap cokelat +
  awning + cangkir), sekolah (kuning + atap merah + jam), pasar
  (atap hijau + awning merah/putih + peti), apartemen (3 lantai,
  banyak jendela), klinik (putih + palang merah), pembangkit
  (abu gelap + petir kuning), perumahan (blok besar atap terakota),
  kampus (marmer + kubah biru + kolom), alun-alun (plaza + air
  mancur + pohon + bangku) — semua terbaca jelas sebagai
  placeholder.
- **Perbaikan kritis dipertahankan**: manifest `particlesystem` (1×,
  `particles` lama 0×), font `LegacyRuntime.ttf` (3 pemakaian,
  `Arial` 0×), landscape 1920×1080, TIDAK ada tombol "Lewati Hari"/
  "Simpan" (4 kecocokan grep hanya komentar penjelas), tombol Bangun
  tetap ikon (`BuatTombolIkon`) — dicek ulang dengan grep.

### Yang BELUM terverifikasi di Fase 4 (jujur dicatat)
- **Panel pencapaian + misi harian + menu bangun bergulir belum
  pernah terlihat di Editor** — ScrollRect + RectMask2D di stub
  hanya memastikan kompilasi; apakah gulirnya terasa nyaman di
  Game View 16:9 (jari/mouse) harus dicek user. Perhatian: konten
  scroll dihitung dari `sizeDelta` yang diatur di akhir
  `PerbaruiIsi`/`TampilkanMenuBangun` — bila Unity menimpa
  `sizeDelta` konten ScrollRect saat layout, baris bawah bisa
  terpotong; laporkan bila terjadi.
- **Perluasan lahan belum pernah diklik di Editor** — `GridManager.
  Perluas` + gambar petak baru + kamera (batas kamera dihitung dari
  `BatasDunia()`) baru teruji di level logika grid. Uji: capai
  Level 5, klik tombol perluasan — grid harus melebar ke 16×16,
  bangunan lama tetap di tempat, kamera tetap membingkai dunia.
- **Klaim pencapaian + misi harian belum pernah diklik** — alur
  klaim (koin/permata bertambah, tombol berubah "Diklaim", klaim
  kedua ditolak) baru teruji di level logika murni; `KlaimPencapaian`
  di GameManager dan klaim harian di PanelMisi memakai PlayerPrefs/
  SaveData yang sama seperti misi tutorial (sudah teruji polanya).
- **Migrasi simpanan 3.0 nyata belum diuji** — hanya aturan versinya
  yang dites. Bila laptop user masih punya simpanan Fase 3, game
  harus memuatnya tanpa error; dunia tanpa Jalan akan terasa
  pemasukan komersialnya turun setengah sampai Jalan dibangun —
  itu disengaja (lihat atas), bukan bug.
- **Keseimbangan ekonomi Level 5–6 belum dimainkan** — simulasi
  10 hari teruji angka (dunia standar + dunia kaya), tapi laju
  sebenarnya menuju 300 warga (butuh ±8 Perumahan + 12 Apartemen
  ≈ Rp124.000.000 biaya bangun) belum dirasakan di permainan nyata;
  bila terasa terlalu lambat/cepat, angka di GameBalance/.asset
  yang diubah, bukan logika.

## Fase 4B — Interior Tembus Pandang + Peristiwa Penghuni (24 Nov 2026)

### Yang berubah di Fase 4B
- **Interior tembus pandang** (GDD §5.6, baru `Managers/InteriorManager.cs`
  + `UI/PanelInterior.cs` + `Utils/SistemInterior.cs` + `Grid/BuildingView.cs`
  diubah): HANYA untuk hunian (Rumah Sederhana, Kost, Apartemen,
  Perumahan). Panel detail hunian mendapat tombol "Lihat Interior";
  menekannya menukar sprite luar bangunan dengan sprite denah
  (4 ruangan, tanpa dinding/atap) DI TEMPAT yang sama di grid —
  bukan scene terpisah, tidak ada karakter yang masuk. Perabot
  terpasang + 1–2 warga penghuni digambar sebagai anak (child) di
  atas denah. Satu bangunan dalam mode interior dalam satu waktu;
  membuka bangunan lain / panel lain / menutup panel mengembalikan
  sprite luar. InteriorManager melacak bangunan yang sedang dibuka
  (`BangunanDalamInterior`) dan data interior per posisi grid
  ("x,y") supaya tetap cocok sesudah muat ulang.
- **4 ruangan, 10 slot perabot tetap** (GDD §5.6 — slot TETAP, bukan
  penempatan bebas, supaya bisa dites dan disimpan sederhana;
  urutan slot JANGAN diubah karena simpanan menyimpan id perabot
  per indeks): Kamar Tidur (slot 0 Kasur Rp150.000/+2, slot 1
  Lemari Rp100.000/+1), Dapur (slot 2 Kompor Rp200.000/+2, slot 3
  Meja Rp80.000/+1, slot 4 Kursi Rp50.000/+1), Kamar Mandi (slot 5
  Bak Mandi Rp150.000/+2, slot 6 Wastafel Rp80.000/+1), Ruang Tamu
  (slot 7 TV Rp300.000/+3, slot 8 Sofa Rp250.000/+2, slot 9 Lampu
  Rp60.000/+1). Melengkapi 1 hunian = Rp1.420.000, bonus +16.
  Semua harga + bonus di `GameBalance` (bukan hardcode di panel).
  Beli ditolak bila slot sudah terisi / slot tidak valid / koin
  tidak cukup; hapus mengembalikan 50% harga.
- **Denah interior** (sprite baru dari kode PIL, bukan unduhan):
  diamond alas bangunan dibagi salib (garis vertikal + horizontal)
  menjadi 4 segitiga — atas = Kamar Tidur (krem hangat), kanan =
  Dapur (ubin biru muda), bawah = Ruang Tamu (kayu), kiri = Kamar
  Mandi (biru pucat) — + garis dinding cokelat + label ruangan
  kecil (KAMAR/DAPUR/MANDI/TAMU). 4 ukuran: `interior-rumah.png`
  256×256, `interior-kost.png` + `interior-apartemen.png` 512×256
  (denah sama, ukuran sama — kedua bangunan 2×2), `interior-perumahan.png`
  768×384. 10 sprite perabot 128×128 (`perabot-kasur.png` dst.,
  tampak depan sederhana, palet game). Posisi slot dihitung dari
  jangkar ruangan × ukuran bangunan, dikurangi `offsetVisualY`
  (titik tengah sprite berada 0,4 unit di atas tengah petak).
- **Efek interior** (GDD §5.6 + §6 + §7): hunian tanpa perabot sama
  sekali = −5 kebahagiaan (penghuni tidak betah). Total bonus
  perabot semua hunian masuk ke kebahagiaan dunia lewat
  `bonusFasilitas` yang sudah ada di
  `EconomyCalculator.HitungKebahagiaan` (ditambahkan
  `InteriorManager.BonusInteriorTotal()` oleh CitizenManager) —
  TIDAK dibuat sistem kebahagiaan kedua. Total dibatasi −20..+20
  (`GameBalance.BatasBonusInterior`) supaya 6+ hunian kosong tidak
  menghancurkan kebahagiaan dunia sendirian. **Kost yang 10
  slotnya terisi penuh = sewa +25%** (`GameBalance.PengaliSewaKostLengkap
  = 125`, Rp40.000 → Rp50.000/hari) lewat field baru
  `CatatanBangunan.PengaliSewaPersen` di EconomyCalculator
  (diterapkan sesudah pengali warung/hujan, sebelum pengali
  tanpa-Jalan; nilai 0 = 100 supaya catatan/tes lama tetap jalan).
- **Peristiwa penghuni** (GDD §9.1, baru `Utils/SistemPeristiwaPenghuni.cs`
  + `UI/PanelPeristiwaPenghuni.cs`): 6 peristiwa, masing-masing 2–3
  pilihan, TIDAK ada pilihan yang selalu benar (uang vs reputasi vs
  kebahagiaan): tidak bisa bayar (3 pilihan — keringanan Rep +5,
  sewa Kost besok hilang / tagih tegas +Rp20.000, Rep −5, kapasitas
  hilang 2 besok / cicil +Rp10.000 sekarang + Rp10.000 besok),
  melarikan diri (biarkan −Rp40.000, Rep −3 / laporkan −Rp15.000,
  Rep +5, kapasitas hilang 2 besok), tetangga berisik (tegur Rep +3,
  bahagia +3 besok / abaikan bahagia −5 besok), merusak perabot
  (potong deposit +Rp25.000, Rep −3 / maafkan −Rp30.000, Rep +5),
  rekomendasi teman (terima Rep +5, bahagia +2 besok / tolak Rep −2),
  minta pindah kamar (setujui bahagia +3 besok / tolak Rep −2,
  bahagia −3 besok). Roll di `GameManager.LanjutkanSesudahPerayaan`:
  sesudah roll kejadian biasa (20%) tidak ada → roll peristiwa 10%
  (`GameBalance.PeluangPeristiwaPenghuniPerHari = 0.10`), hanya bila
  punya ≥1 Kost; paling banyak satu panel pilihan per hari. Efek
  "besok" diterapkan di awal `ProsesPergantianHari` bersama efek
  kejadian biasa, lalu direset; koin rugi yang melebihi koin pemain
  dipotong sampai koin 0 (bukan utang). Hasil pilihan dicatat di
  laporan harian (`TeksHasil`).
- **Reputasi** (baru, GDD §9.1): mulai 50 (`GameBalance.ReputasiAwal`),
  dibatasi 0–100 (`SistemPeristiwaPenghuni.BatasiReputasi`), tampil
  di HUD baris 2 ("Reputasi: N"). Tata ulang baris 2 bar atas:
  warga (30,58,300,44) font 30, bahagia (340,58,260,44) font 30,
  reputasi (610,58,230,44) font 30, level (850,58,490,44) font 30.
  Baris 1 tidak berubah.
- **Misi interior** (2 misi tutorial baru di PanelMisi): "Beli 1
  Perabot" (id `misi_perabot`, Rp25.000, selesai bila total perabot
  ≥ 1) dan "Lengkapi 1 Kost" (id `misi_kost_lengkap`, Rp75.000,
  selesai bila ada Kost 10 slot terisi). Panel Misi kini 10 tutorial
  + 1 judul bagian + 3 harian = 14 baris (cek-ui-bounds diperbarui
  12 → 14).
- **Simpanan naik ke versi "5.0"**: field baru `SaveData.reputasi`,
  `interior` (list `DataInteriorTersimpan`: x, y, perabot 10 entri),
  `sewaKostHilangBesok`, `kapasitasHilangBesok`, `tambahanKoinBesok`,
  `bonusBahagiaPeristiwaBesok`. Simpanan 4.0/3.0/2.0/1.0 tetap
  dimuat: reputasi 0 di simpanan lama = "tidak ada data" → 50 saat
  muat (reputasi 0 yang sah hanya ada di simpanan "5.0" — dibedakan
  lewat `data.versi`); interior kosong; efek peristiwa tidak ada.
  Interior bangunan yang dijual ikut dihapus
  (`InteriorManager.HapusInterior` di `BuildManager.JualBangunan`).
- **Panel detail hunian** menampilkan ringkasan interior
  ("Perabot: N/10 • Bonus bahagia: +N", + "Sewa +25%" bila Kost
  lengkap); tombol "Lihat Interior" disembunyikan untuk non-hunian.
  Panel interior (760×1000) bergulir: ringkasan + 4 judul ruangan +
  10 slot (14 baris); tiap slot: nama + status + tombol Beli
  ("Beli RpX") atau Hapus ("Hapus (+RpY)"). Panel peristiwa
  (760×700): judul + deskripsi + 2–3 tombol pilihan (tombol ke-3
  disembunyikan bila peristiwa hanya punya 2 pilihan).
- **Scene**: InteriorManager komponen baru fileID **210** di
  GameRoot (spriteInterior[4] urutan rumah/kost/apartemen/perumahan,
  spritePerabot[10] urutan = indeks slot, spriteWarga[3]).
  Generator (`generate-project.py`) + MainScene.unity diperbarui
  bersamaan; diff generator di /tmp menunjukkan HANYA penambahan
  fileID 210 — tidak ada perbedaan lain. 19 file `.meta` baru
  (5 script + 14 sprite).
- **Penyederhanaan yang dicatat (bukan bug)**: "sewa 1 bulan" di GDD
  dimodelkan sebagai sewa 1 hari Kost (Rp40.000) supaya terasa tapi
  tidak menghancurkan ekonomi; peristiwa "merekomendasikan teman"
  di GDD hanya punya 1 pilihan (Terima) — Fase 4B menambah pilihan
  "Tolak dengan sopan" (Rep −2) supaya selalu ada pilihan;
  `KapasitasHilangHariIni` TIDAK direset di `CitizenManager.HitungUlang`
  (supaya penalti kamar kosong bertahan seharian walau ada bangunan
  dibangun di tengah hari) — GameManager yang menimpa nilainya tiap
  pagi; `SewaKostHilangHariIni` direset di `EconomyManager.TerapkanHari`
  sesudah `BuatCatatan` (pola sama seperti `PengaliHujanBesok`).
- **Sprite baru diperiksa visual** (contact sheet 13 sprite + denah
  terpisah): denah 4 segitiga warna berbeda + label KAMAR/DAPUR/
  MANDI/TAMU terbaca (label dipindah dari 1/3 ke 1/2 jalan ke puncak
  segitiga supaya tidak menumpuk di tengah salib); kasur (rangka +
  selimut merah), lemari (2 pintu + gagang emas), kompor (2 tungku +
  oven), meja (piring + gelas), kursi (hijau), bak mandi (air biru
  + keran), wastafel (cermin + keran), TV (layar biru + antena),
  sofa (merah + 2 bantal kuning), lampu (kap kuning + cahaya) —
  semua terbaca jelas sebagai placeholder.

### Yang BELUM terverifikasi di Fase 4B (jujur dicatat)
- **Mode tembus pandang belum pernah terlihat di Editor** — apakah
  denah 4 ruangan benar-benar menggantikan sprite luar di tempat
  yang sama, apakah perabot + warga penghuni muncul di posisi yang
  benar di dalam denah (bukan melayang/tertutup), dan apakah
  "transparan" terasa sebagai tembus pandang (bukan bangunan
  menghilang) hanya bisa dibuktikan di Game View. Perhatian:
  posisi slot memakai `offsetVisualY` (0,4 unit) — bila di Editor
  perabot terlihat bergeser ke atas/bawah dari denah, angka itu
  yang dikoreksi.
- **Panel interior + panel peristiwa belum pernah diklik di Editor**
  — alur beli/hapus perabot (koin berkurang, slot berubah, bonus
  bahagia berubah), klaim 2 misi interior, dan pilihan peristiwa
  (reputasi berubah, efek besok masuk laporan hari berikut) baru
  teruji di level logika murni.
- **Migrasi simpanan 4.0 nyata belum diuji** — hanya aturan versinya
  yang dites. Bila laptop user masih punya simpanan Fase 4, game
  harus memuatnya tanpa error; reputasi harus mulai 50, interior
  kosong, dan dunia tanpa Jalan tetap terkena penalti −50% seperti
  Fase 4.
- **Keseimbangan peristiwa belum dimainkan** — 10%/hari berarti
  rata-rata 1 peristiwa per 10 hari; bila terasa terlalu jarang/
  sering, `PeluangPeristiwaPenghuniPerHari` di GameBalance yang
  diubah. Dampak uang peristiwa (Rp10.000–Rp40.000) kecil dibanding
  pemasukan dunia Fase 4 (Rp50.000+/hari) — sengaja, supaya peristiwa
  terasa sebagai cerita, bukan hukuman ekonomi.

## Balai Kota + Gambar Ulang Kartun (24 Nov 2026)

### Yang berubah
- **Balai Kota (bangunan baru, ke-18)**: fasilitas/landmark 2×2,
  terbuka Level 3 (Desa), harga Rp3.000.000, perawatan Rp5.000/hari,
  kebahagiaan +5, reputasi +2/hari. File baru `BalaiKota.asset`
  (kategori 2 fasilitas, `bonusBahagia: 5`) + `Utils/SistemBalaiKota.cs`
  (logika murni: `BolehBangun`, `BonusReputasiHarian`,
  `ApakahTerbuka`). Kebahagiaan +5 mengalir lewat
  `EconomyCalculator.HitungBonusFasilitas` seperti fasilitas Fase 4
  lain (+5 bila bersebelahan Jalan, +2 bila tidak — aturan Jalan
  §5.5); reputasi +2/hari diterapkan `GameManager.ProsesPergantianHari`
  dan menjadi **sumber reputasi pasif pertama** (sebelum ini reputasi
  hanya berubah lewat peristiwa penghuni §9.1).
- **Hanya 1 Balai Kota per dunia**: batas ditegakkan di dua tempat —
  `BuildManager.CobaBangun` (menolak + toast "Balai Kota sudah
  dibangun — hanya 1 per dunia.") dan menu bangun (`UIManager`:
  baris Balai Kota menampilkan "Sudah dibangun", abu-abu, klik →
  toast yang sama).
- **Balai Kota di single player = fasilitas + sumber reputasi.**
  Wali Kota sebagai NPC yang mengurus izin tanah tetap di **Fase 7
  (multiplayer)** — gedungnya sudah ada, orangnya menyusul.
- **Simpanan naik ke versi 5.1** (perubahan kecil, bukan 6.0):
  simpanan 5.0/4.0/3.0/2.0/1.0 tetap dimuat (`VersiSimpanFase4B =
  "5.0"` dicatat di GameBalance). Status "sudah ada Balai Kota"
  tersimpan sebagai bangunan biasa (id `balaikota` di daftar bangunan).
- **Semua sprite digambar ulang ke gaya kartun bersih** (dari kode
  `tools/generate-sprites.py`, tanpa unduhan): 18 bangunan, 3 warga
  (idle + 2 frame jalan tiap varian), 4 denah interior, 10 perabot —
  ikon Bangun dibiarkan, sudah disetujui user; tile/highlight
  dibiarkan, sudah selaras. Ciri gaya: outline cokelat gelap tipis,
  shading sisi terang vs teduh, detail khas tiap bangunan
  (cerobong + kotak bunga untuk rumah; etalase + papan nama untuk
  toko; pohon + bangku untuk taman; 4 kolom + pedimen + jam + bendera
  merah-putih untuk Balai Kota), bayangan di bawah bangunan, palet
  hangat cerah yang sama. **Dimensi kanvas tiap sprite TETAP**
  (1×1 256×256, 2×2 512×256, 3×3 768×384, perabot 128×128,
  warga 256×256) — scene, sorting, dan bounds tidak berubah.
  Total 41 PNG digambar ulang (18 bangunan + 9 warga + 4 denah +
  10 perabot); tile/highlight/ikon-bangun tidak disentuh.
- **Warga chibi kartun proper + frame jalan**: 3 varian warna baju
  tetap berbeda, kini berlengan + bertangan, wajah hidup (mata
  berkilau, senyum, pipi merona), sepatu, jambul rambut. Tiap varian
  punya 2 frame jalan (`warga1-a.png`/`warga1-b.png`, dst. — kaki
  kiri maju / kaki kanan maju, lengan berayun berlawanan).
  `PejalanKaki.Atur` kini menerima pasangan frame jalan;
  `CitizenManager.spriteWargaJalan` (6 sprite, urutan 1a,1b,2a,2b,3a,3b)
  diisi dari Scene. Saat berjalan, frame A/B bergantian tiap 0,3
  detik (`GameBalance.IntervalFrameJalanWarga`, metode murni
  `PejalanKaki.IndeksFrameJalan`); saat istirahat kembali ke sprite
  diam. Bila scene lama belum mengisi `spriteWargaJalan`, warga tetap
  jalan dengan sprite diam (ayunan langkah saja) — tidak error.
- **Perubahan alat**: `tools/generate-sprites.py` ditulis ulang penuh
  (helper `poli`, `jendela_iso`, `pintu_iso`, `cerobong`,
  `garis_genteng`, `gedung2x2`, `gedung3x3`, `gambar_warga`,
  `denah_interior`, `perabot`); `tools/generate-project.py`
  menambahkan Balai Kota (asset + entri scene + .meta sprite baru +
  `SistemBalaiKota.cs`); `tools/contact-sheet.py` BARU (merangkai
  semua sprite berlabel); `tools/test-balaikota.cs` BARU (42
  pemeriksaan); `tools/cek-ui-bounds.py`: menu bangun 17 → 18 baris.
- **Batas baris file**: UIManager 598, GameManager 599 (keduanya
  ≤600 — komentar dipangkas untuk memberi ruang pada logika baru,
  tanpa menghapus penjelasan yang dibutuhkan).

### Verifikasi Balai Kota + Kartun (semua dijalankan, dari biner baru)
- Kompilasi 32 script + stub (`mcs -warn:4 -target:library`):
  **0 error, 0 warning**.
- `tools/test-balaikota.cs` (BARU): **42/42 lolos** — angka Balai
  Kota (Level 3, Rp3.000.000, Rp5.000/hari, +5, +2/hari, maks 1),
  batas 1 (`BolehBangun(0)` true / `(1)` false), syarat level,
  bonus reputasi (+0 tanpa / +2 dengan Balai Kota), efek bahagia
  (+5 dengan Jalan, +2 tanpa Jalan, +7 bersama Perpustakaan),
  efek perawatan (dunia 1 Balai Kota saja: bersih −Rp5.000),
  migrasi simpanan (5.1/5.0/4.0/3.0/2.0/1.0 didukung; 6.0/kosong
  ditolak), frame jalan (0/1 bergantian tiap 0,3 detik; t=0,9 →
  frame 1 — sempat gagal karena galat float 0,9f/0,3f = 2,9999998,
  diperbaiki dengan epsilon 0,001 di `IndeksFrameJalan`).
- Tes lama dijalankan ulang dari biner baru — **semua tetap lolos**:
  test-ekonomi 22/22, test-fase2 47/47, test-fase3 142/142,
  test-fase4 199/199, test-fase4b 163/163. (Kenaikan angka vs catatan
  lama karena tes lama diperbarui: pin versi 5.0 → 5.1 di
  test-fase2/3/4/4b + 1 pemeriksaan "5.1 didukung" tiap tes; tidak
  ada logika lama yang berubah.)
- `tools/validasi-yaml.py`: **lolos** — 23 file YAML (MainScene,
  prefab, 18 .asset, 3 ProjectSettings); 109 file `.meta`; semua
  referensi guid ter-resolve (termasuk BalaiKota.asset +
  6 frame jalan warga + SistemBalaiKota.cs).
- `tools/cek-ui-bounds.py`: **695/695 lolos** (menu bangun kini
  18 baris bangunan + baris perluasan; geometri tidak berubah
  karena dimensi sprite tetap).
- **Contact sheet diperiksa visual** (`tools/contact-sheet.png`,
  2700×1704, 6 baris berlabel: 18 bangunan + 9 warga + 4 interior
  + 10 perabot): semua sprite terbaca jelas sebagai bangunannya,
  tidak ada yang rusak/terpotong. Tiga cacat ditemukan saat
  pemeriksaan dan sudah diperbaiki: (1) tangga Perpustakaan
  tampak seperti garis diagonal melayang — tangga dihapus;
  (2) label "KAMAR" tertutup karpet + label "MANDI"/"DAPUR"
  tertimpa garis salib denah — karpet digeser ke bawah label,
  label MANDI/DAPUR digeser ke atas garis; (3) bendera Balai Kota
  terpotong tepi atas kanvas — bendera dikecilkan/diturunkan
  (y=2..18). Penilaian jujur: sprite **paling sederhana** yang
  tersisa adalah **Jalan** (diamond aspal + garis putus-putus —
  memang jalan), **Pos Keamanan** (palang kuning-hitam kecil,
  bangunan polos), **Kursi** (agak menyerupai kotak hijau kecil),
  dan **Klinik** (kotak putih yang mengandalkan palang merah);
  perbedaan frame jalan A/B halus (kaki bergeser ±6 piksel) —
  terlihat saat bergantian, tapi tidak dramatis.
- Generator ↔ project diverifikasi sinkron: sebelum mengubah
  generator, `generate-project.py` dijalankan di salinan /tmp dan
  hasilnya **identik 0 baris** dengan project; sesudah diubah,
  regenerasi hanya mengubah yang dimaksud (scene: +1 referensi
  asset Balai Kota + 6 referensi frame jalan; file baru
  BalaiKota.asset/.meta, SistemBalaiKota.cs/.meta, 7 .meta sprite
  baru) — tidak ada churn lain. Perbaikan kritis dipertahankan:
  manifest `particlesystem`, font `LegacyRuntime.ttf`.

### Yang BELUM terverifikasi di Balai Kota + Kartun (jujur dicatat)
- **Balai Kota belum pernah dibangun di Editor** — apakah menu
  bangun menampilkan "Sudah dibangun" sesudah 1 Balai Kota berdiri,
  apakah reputasi naik +2 tiap pergantian hari (HUD), dan apakah
  kebahagiaan +5 masuk hitungan (cek panel detail / laporan harian)
  hanya bisa dibuktikan di Game View.
- **Frame jalan warga belum pernah terlihat bergerak** — apakah
  pergantian A/B tiap 0,3 detik terlihat sebagai langkah (bukan
  kedip) dan apakah sprite diam kembali saat istirahat hanya bisa
  dibuktikan di Game View. Bila langkah terasa terlalu halus,
  `IntervalFrameJalanWarga` (0,3) yang diubah, bukan sprite-nya.
- **Tampilan kartun di Game View belum dinilai** — contact sheet
  membuktikan sprite rapi di atas latar abu-abu, tapi kesan akhir
  (apakah bangunan terbaca di antara tile hijau, apakah outline
  cukup tegas di layar HP kecil) baru terlihat di Editor/HP.
  Sprite paling sederhana (Jalan, Pos, Kursi, Klinik) adalah
  kandidat pertama bila user minta perbaikan lanjutan.
- **Migrasi simpanan 5.0 nyata belum diuji** — hanya aturan versinya
  yang dites. Bila laptop user masih punya simpanan 5.0, game harus
  memuatnya tanpa error dan Balai Kota (bila sudah dibangun di 5.0
  — tidak mungkin, bangunan baru) tidak relevan; dunia 5.0 tanpa
  Balai Kota tetap jalan normal.

## Fase 5 — Polish: Audio, Tutorial, Optimasi HP, Persiapan Build (24 Nov 2026)

### Yang berubah di Fase 5

**Audio (semua disintesis dari kode, tanpa unduhan/rekaman):**
- `tools/generate-audio.py` (baru) mensintesis 7 WAV mono 44100 Hz
  int16: 6 SFX (tap 0,09 dtk; bangun 0,37 dtk; koin 0,52 dtk;
  naik-level 0,94 dtk; klaim 0,50 dtk; tolak 0,30 dtk) + musik latar
  ambient 48 detik (progresi C–Am–F–G, pad + melodi pelan, awal &
  akhir sunyi supaya loop mulus). Total 4,27 MB di `Assets/Audio/`.
- `Managers/AudioManager.cs` (baru, 184 baris, komponen GameRoot
  fileID 211 di MainScene): 6 SFX + musik loop (volume terpisah,
  `GameBalance.VolumeMusik` 0,35); metode statis
  `MainkanTap/Bangun/Koin/NaikLevel/Klaim/Tolak()` aman dipanggil
  sebelum Instance siap dan bila klip kosong; menghormati toggle
  Suara (`PlayerPrefs "DuniaKita_Suara"`, kunci yang sama dengan
  PanelPengaturan — kini satu sumber kebenaran di
  `AudioManager.KunciSuara`).
- Hook SFX (satu baris tiap tempat, tanpa refactor): tap di
  `UiFactory.BuatTombol` + `BuatTombolIkon` (semua tombol game);
  tolak + bangun di `BuildManager.CobaBangun`; klaim di
  `PanelMisi` (tutorial + harian) dan `GameManager.KlaimPencapaian`;
  naik level + koin harian di `GameManager`; bangun/tolak perabot di
  `PanelInterior`.
- Toggle Suara di `PanelPengaturan` kini juga memulai/menghentikan
  musik latar (sebelumnya hanya `AudioListener.volume`).

**Tutorial pemain baru (first-run, bisa dilewati):**
- `Utils/SistemTutorial.cs` (baru, logika murni): 6 langkah
  (selamat-datang, bangun-rumah, tombol-bangun, hari-otomatis,
  detail-interior, misi-pencapaian); hanya muncul sekali
  (`PlayerPrefs "DuniaKita_TutorialSelesai"`); langkah "bangun-rumah"
  dilewati otomatis bila Rumah Sederhana sudah dibangun;
  `ResetUntukUlang()` untuk tombol "Ulangi Tutorial".
- `UI/PanelTutorial.cs` (baru): panel 1200×300 di bawah layar
  (y 600..900, di atas bar bawah) + tombol "Lanjut"/"Lewati" di
  tiap langkah; dibuka otomatis sekali oleh
  `GameManager.Start` → `PanelTutorial.CobaTampilkanAwal()`.
- `PanelPengaturan` mendapat tombol "Ulangi Tutorial" + teks versi
  "(Fase 5)".
- `UI/PanelPerayaan.cs` (baru): panel perayaan naik level PINDAH
  dari `UIManager.cs` tanpa perubahan isi/perilaku — UIManager
  598 → 590 baris, GameManager tetap 599 baris (5 baris kode Fase 5
  diimbangi pemangkasan 5 baris komentar).

**Optimasi HP + persiapan build:**
- `ProjectSettings/QualitySettings.asset` (baru): 6 level bawaan;
  Medium (indeks 2) jadi default + default Android — pixelLightCount
  1, tanpa bayangan real-time, tekstur penuh + kompresi, MSAA 2x,
  vSync mati; target 60 fps (`GameBalance.TargetFrameRate`,
  `GameManager.Awake` — 1 baris).
- `ProjectSettings.asset`: companyName "Uhlabs" (sebelumnya
  "Dunia Kita"), productName "Dunia Kita", package
  `com.uhlabs.duniakita` (Android/iPhone/Standalone), bundleVersion
  1.0, androidBundleVersionCode 1, orientasi AutoRotation + hanya
  Landscape Left/Right diizinkan, `m_Icons` 5× ikon aplikasi.
- `Assets/Sprites/ikon-aplikasi.png` (baru, 512×512, RGBA, sudut
  membulat transparan): rumah krem beratap merah + palu, latar hijau
  — dibuat `tools/generate-ikon-aplikasi.py`, diperiksa visual
  (terbaca jelas sebagai rumah + palu).
- Semua 44 sprite tetap terkompresi (`textureType 8`,
  `textureFormat -3`); tidak ada sprite duplikat/tak terpakai di
  scene (satu-satunya PNG tanpa rujukan scene adalah ikon aplikasi
  — dirujuk dari ProjectSettings, benar); warga tetap maks 12 sprite;
  interior hanya dirender saat mode interior aktif (verifikasi kode
  `InteriorManager.MasukModeInterior` / `KeluarModeInterior`).
- `tools/generate-project.py` diperbarui: script baru (AudioManager,
  SistemTutorial, PanelTutorial, PanelPerayaan), template .meta
  audio (SFX DecompressOnLoad, musik Streaming) + ikon (Default),
  komponen AudioManager fileID 211 di scene, ProjectSettings +
  QualitySettings. **Regenerasi penuh di /tmp lalu diff: hanya file
  yang dimaksud yang berubah** (MainScene +211, ProjectSettings,
  QualitySettings baru, 12 .meta baru) — semua .meta/.asset/prefab
  lama regenerasi identik; project nyata kini sama persis dengan
  keluaran generator.
- `tools/unity-stubs.cs` diperluas (Fase 5): `AudioClip`,
  `AudioSource` (Play/Stop/PlayOneShot/isPlaying),
  `Application.targetFrameRate`, `PlayerPrefs` kini benar-benar
  menyimpan (kamus di memori + `HapusSemuaUntukTes()`), dan
  `Component`/`GameObject.transform` jadi properti lazy (di Unity
  asli transform tidak pernah null — sebelumnya stub null sehingga
  `AudioManager.Siapa()` tidak bisa dites).
- `tools/validasi-yaml.py`: QualitySettings.asset masuk daftar cek.
  `tools/cek-ui-bounds.py`: PanelPerayaan pindah ke file sendiri +
  PanelTutorial baru masuk daftar cek.
- Dua header daftar kompilasi tes yang tidak lengkap diperbaiki:
  `test-ekonomi.cs` (GameBalance.cs tertinggal) dan
  `test-balaikota.cs` (IsometricGrid.cs tertinggal) — pelajaran
  Fase 3/4B.

### Verifikasi Fase 5 (semua dijalankan, dari biner baru)

- **Kompilasi: 36 script game + stub, `mcs -warn:4` → 0 error,
  0 warning.** (Naik dari 32 script: +AudioManager, +SistemTutorial,
  +PanelTutorial, +PanelPerayaan.)
- **Tes logika baru `tools/test-fase5.cs`: 56/56 lolos** — tutorial
  (6 langkah + urutan id + isi, tampil sekali, lewati otomatis
  bangun-rumah, jalan penuh tanpa rumah 6 langkah / dengan rumah
  5 langkah, ulangi dari Pengaturan) + audio (VolumeMusik 0,35,
  TargetFrameRate 60, kunci suara sama, aman tanpa Instance,
  pemetaan 6 momen→klip, putar SFX + musik mulai/berhenti,
  toggle Suara membisukan SFX + musik, klip kosong aman).
- **Tes lama (biner baru, tetap lolos):** ekonomi 22/22, Fase 2
  47/47, Fase 3 142/142, Fase 4 199/199, Fase 4B 163/163,
  Balai Kota 42/42.
- **WAV numerik (semua mono 44100 Hz, tidak clip):**

  | File | Durasi | Puncak | RMS |
  |---|---|---|---|
  | sfx-tap.wav | 0,09 dtk | 0,395 | 0,107 |
  | sfx-bangun.wav | 0,37 dtk | 0,667 | 0,183 |
  | sfx-koin.wav | 0,52 dtk | 0,784 | 0,202 |
  | sfx-naik-level.wav | 0,94 dtk | 0,947 | 0,209 |
  | sfx-klaim.wav | 0,50 dtk | 0,764 | 0,160 |
  | sfx-tolak.wav | 0,30 dtk | 0,756 | 0,204 |
  | musik-latar.wav | 48,00 dtk | 0,543 | 0,113 |

  Loop musik: RMS 100 sampel awal 0,00006 dan akhir 0,00000 —
  sambungan mulus (tidak berbunyi "klik"). Puncak naik-level sempat
  mentok di batas clip 0,98 saat gain 0,55 — gain diturunkan ke 0,42
  sehingga puncak 0,947 dengan headroom.
- **Validasi YAML/GUID lolos:** 24 file YAML (tambah
  QualitySettings.asset), 121 .meta, MainScene kini 18 dokumen /
  18 objek (tambah komponen AudioManager fileID 211 + 7 referensi
  AudioClip fileID 8300000); semua guid ter-resolve; manifest valid.
- **UI bounds lolos 708/708** (naik dari 695: +panel tutorial
  1200×300 = 17% layar + tombol "Ulangi Tutorial" + panel perayaan
  di file baru) — semua elemen dalam layar, tombol ≥90, tidak ada
  tumpang tindih, panel 17–46% layar.
- **Jumlah baris:** GameManager 599, UIManager 590 — keduanya ≤600.
- **Insiden saat pengerjaan (dicatat jujur):** satu edit
  `GameManager.SelesaikanHari` sempat menghapus baris
  `TampilkanLaporanHari` (laporan harian tidak akan muncul) —
  ketahuan saat pengecekan ulang dan langsung dikembalikan; tes
  ekonomi + kompilasi ulang sesudahnya tetap lolos. Dua perintah
  tes lama pertama kali gagal kompilasi karena daftar file di
  perintah saya kurang (GameBalance di test-ekonomi,
  IsometricGrid di test-balaikota) — bukan bug kode; daftar file
  + header komentar diperbaiki, tes ulang lolos.

### Yang BELUM terverifikasi di Fase 5 (jujur dicatat)

- **Suara di telinga user belum dinilai.** Asisten tidak bisa
  mendengar — angka WAV (durasi, puncak, RMS, loop mulus) hanya
  membuktikan file sehat, bukan enak. User harus menilai di Editor:
  apakah tap/bangun/koin/naik-level/klaim/tolak terdengar pas dan
  musik latar tidak mengganggu (volume musik 0,35 bisa diubah di
  GameBalance bila terlalu pelan/keras).
- **Tutorial di layar belum dilihat.** Panel 1200×300 + tombol
  Lanjut/Lewati lolos cek geometri, tapi keterbacaan teks 34pt,
  apakah panel menutupi petak yang sedang dijelaskan, dan apakah
  langkah (2) benar-benar terlewati saat rumah sudah dibangun hanya
  bisa dibuktikan di Game View.
- **Performa di HP nyata belum diukur.** Quality Medium + target
  60 fps + kompresi sprite adalah pengaturan, bukan hasil ukur.
  FPS, suhu, dan baterai baru bisa diukur sesudah build Fase 6
  terpasang di HP user.
- **Build Android belum dicoba.** ProjectSettings sudah disiapkan
  (package, versi, orientasi, ikon, kualitas), tapi Switch Platform
  + Build And Run belum pernah dijalankan — itu Fase 6, dari laptop.
- **Ikon aplikasi belum dilihat di launcher HP.** File 512×512
  diperiksa visual di sandbox (rumah + palu terbaca jelas), tapi
  tampilannya sebagai ikon kecil di HP baru terlihat sesudah build.

## Fase 6 — Rilis: QA Akhir, Skrip Build Android, Materi Play Store (24 Nov 2026)

**Status:** Selesai & terverifikasi sejauh yang bisa dilakukan tanpa
Unity Editor. **Game tetap BELUM pernah dijalankan di Unity Editor
dan build Android BELUM pernah dijalankan.** Fase ini sengaja TIDAK
menambah fitur/gameplay baru — hanya QA + perbaikan bug yang
ditemukan QA (tidak ada yang ditemukan) + materi rilis.

### Yang berubah di Fase 6

- **QA data 18 bangunan** (`tools/qa-fase6.py`, baru): membaca
  langsung 18 file `.asset` + `GameBalance.cs` + `ProjectSettings` —
  id unik, nama/deskripsi terisi, harga > 0, perawatan ≥ 0,
  levelBuka 1–6, kategori 0–2, ukuran 1×1/2×2/3×3, hunian punya
  kapasitas, komersial punya pemasukan, pemasukan > perawatan,
  sprite PNG ada + dimensi kanvas cocok ukuran (1×1 256×256,
  2×2 512×256, 3×3 768×384) + GUID sprite `.asset` sama dengan
  `.meta` PNG + `.meta` terkompresi, angka `.asset` cocok dengan
  `GameBalance.cs` (Balai Kota, Kost) dan README (16 bangunan lain),
  total harga + bonus 10 perabot = Rp1.420.000 / +16,
  ProjectSettings (Uhlabs, Dunia Kita, `com.uhlabs.duniakita`,
  versi 1.0, orientasi LandscapeLeft + autorotate kiri/kanan,
  portrait mati), Build Settings hanya MainScene, QualitySettings
  default Medium (indeks 2), manifest `particlesystem`, font
  `LegacyRuntime` (Arial 0), tidak ada tombol "Lewati Hari"/"Simpan"
  (hanya komentar yang menyatakan keduanya dihapus), 7 WAV + .meta
  ada, ikon aplikasi 512×512 ada, kapasitas hunian maks teoretis
  grid 16×16 (25 Perumahan = 500 warga) ≥ 300 syarat Metropolis.
  Hasil: **419/419, 0 gagal**. Dua kegagalan pertama adalah
  pemeriksaan QA-nya yang terlalu naif (mencari teks "Lewati Hari"/
  "Simpan" termasuk di dalam komentar) — diperbaiki di QA-nya
  (komentar diabaikan), bukan di game.
- **Tes logika rilis** (`tools/test-fase6.cs`, baru, 147 pemeriksaan):
  rantai migrasi simpanan 1.0–5.1 didukung + versi lain ditolak,
  ukuran grid efektif simpanan lama/rusak → 12, VersiSimpan "5.1" +
  VersiGame "1.0", hadiah naik level naik terus, perluasan lahan
  12→16, Balai Kota silang GameBalance, syarat populasi/bahagia
  naik terus tiap level, 6 pencapaian total 26 Permata, 8 misi
  harian (3/hari deterministik, berbeda-beda), harga jual 50%,
  pengali warung 80/100/110, interior 10 slot + total harga/bonus +
  penalti rumah kosong −5 + batas bonus ±20. Hasil: **147/147,
  0 gagal**. Satu harapan tes awal salah (bonus interior kosong
  dikira 0, padahal −5 sesuai GDD §5.6) — diperbaiki di tesnya.
- **Skrip build Android** (`Assets/Editor/BuildAndroid.cs`, baru —
  skrip Editor, TIDAK ikut ke game di HP): `BuildAPK` (APK debug)
  dan `BuildAAB` (AAB release, `buildAppBundle = true`), hanya
  scene MainScene, keluaran `C:\Dunia Kita\build\DuniaKita-v1.0.apk`
  / `.aab`, versi dari `GameBalance.VersiGame`. Kompilasi diperiksa
  dengan stub UnityEditor minimal (`tools/unity-editor-stubs.cs`):
  **0 error, 0 warning**. `.meta`-nya dibuat generator
  (`generate-project.py` kini mencakup `Assets/Editor/` + skripnya;
  regenerasi penuh hanya menambah 2 `.meta` baru, tidak ada file
  lain yang berubah).
- **`build-android.bat`** (baru, di root project): CRLF, gaya goto,
  tanpa `)` di blok if. Double-klik → pilih 1 (APK) atau 2 (AAB) →
  Unity 2022.3.20f1 batchmode (`-quit -batchmode -nographics
  -executeMethod BuildAndroid.BuildAPK/AAB -logFile
  C:\Dunia Kita\build.log`) → laporkan BERHASIL + lokasi hasil atau
  GAGAL + penyebab umum. Path Unity.exe default instalasi Unity Hub
  (`C:\Program Files\Unity\Hub\Editor\2022.3.20f1\Editor\Unity.exe`,
  bisa diubah di baris atas .bat). Sebelum AAB pertama: keystore
  wajib dibuat dulu (diingatkan .bat + README).
- **Materi rilis** (baru, folder `../rilis/`): `listing-play-store.md`
  (judul, deskripsi singkat 79 karakter dari maks 80, deskripsi
  lengkap, kata kunci, kategori Simulasi, jawaban kuesioner IARC —
  tanpa kekerasan/iklan/pembelian/obrolan/data, perkiraan PEGI 3 /
  ESRB Everyone; **listing TIDAK menjanjikan multiplayer**),
  `feature-graphic.png` 1024×500 (dibuat `tools/generate-feature-
  graphic.py` dari sprite yang sudah ada, diperiksa visual — judul
  + tagline terbaca, Balai Kota + rumah + taman + 3 warga terlihat,
  pil "18 bangunan - 6 level" sempat terpotong lalu diperbaiki
  dengan mengukur lebar teks), `kebijakan-privasi.md` + `.html`
  (game tidak mengumpulkan data apa pun), `panduan-screenshot.md`
  (8 tangkapan yang disarankan, dari HP — belum ada),
  `rencana-beta.md` (APK ke 3–5 teman → perbaiki → AAB).
- **Matriks uji Alpha** (baru, README): 12 baris A1–A12 (18 bangunan,
  Level 6, simpan-muat, tutorial sekali, audio, interior, peristiwa
  penghuni, misi harian, pencapaian sekali, Balai Kota maks 1,
  aturan Jalan, perluasan lahan) — langkah + hasil yang diharapkan
  untuk user di Editor.
- **README diperbarui:** judul "Fase 1–6: Rilis", seksi Fase 6,
  matriks Alpha, build dua jalur (A .bat otomatis, B manual Editor)
  + "Bila build gagal" (baca build.log, penyebab umum), struktur
  project (+ `Assets/Editor/`), "Yang BELUM ada" kini Fase 7 saja
  (build + Play Store tinggal dijalankan dari laptop).
- **Tidak ada perubahan angka ekonomi/bangunan di Fase 6** (bukan
  bug — tidak ada yang diubah) dan **tidak ada perubahan GameManager/
  UIManager** (tetap 599/590 baris).

### Verifikasi Fase 6 (semua dijalankan, dari biner baru)

- **Kompilasi**: `mcs -target:library -warn:4` terhadap 36 script
  game + stub — **0 error, 0 warning**. BuildAndroid.cs terhadap
  stub Unity + stub UnityEditor — **0 error, 0 warning**.
- **QA data**: `python3 tools/qa-fase6.py` — **419 lolos, 0 gagal**.
- **Tes logika**: test-fase6 **147/147**; tes lama diulang semua —
  ekonomi 22/22, fase2 47/47, fase3 142/142, fase4 199/199,
  fase4b 163/163, balaikota 42/42, fase5 56/56 — **total 818
  pemeriksaan (671 lama + 147 baru), 0 gagal**.
- **Validasi YAML/GUID**: `tools/validasi-yaml.py` — lolos (24 file
  YAML, 121 .meta sesudah 2 .meta Editor baru).
- **Cek UI bounds**: `tools/cek-ui-bounds.py` — **708/708** (tidak
  ada perubahan UI di Fase 6, angka sama seperti Fase 5).
- **Feature graphic diperiksa visual** (lihat § di atas).
- **Regenerasi generator**: `generate-project.py` dijalankan ulang —
  diff hanya 2 `.meta` baru (BuildAndroid.cs.meta GUID = md5 path,
  Editor.meta); tidak ada file lain yang berubah.

### Yang BELUM terverifikasi di Fase 6 (jujur dicatat)

- **Build Android belum pernah dijalankan.** BuildAndroid.cs lolos
  kompilasi terhadap stub, tapi `BuildPipeline.BuildPlayer` yang
  asli, Switch Platform, dan Android SDK/NDK/JDK di laptop belum
  teruji. Langkah pertama: double-klik `build-android.bat`, pilih 1,
  dan laporkan isi `C:\Dunia Kita\build.log` bila gagal.
- **Path Unity.exe di .bat belum dikonfirmasi di laptop ini.**
  Path default instalasi Unity Hub dipakai (`C:\Program
  Files\Unity\Hub\Editor\2022.3.20f1\Editor\Unity.exe`) — bila
  Unity terpasang di tempat lain, ubah baris `UNITY_EXE` di .bat.
- **Keystore belum dibuat** (tugas user di Player Settings sebelum
  AAB pertama) dan **akun developer Play Store belum ada** ($25,
  sekali bayar — dibuat user saat siap mengunggah).
- **Tangkapan layar Play Store belum ada** (baru bisa diambil dari
  HP sesudah APK terpasang) dan **listing belum diisi ke Play
  Console** (butuh akun developer).
- **Beta tertutup belum berjalan** (butuh APK + 3–5 penguji).

## Fase 7 — Dunia Bersama + Wali Kota NPC (24 Nov 2026, FASE TERAKHIR)

**Status:** Selesai & terverifikasi sejauh yang bisa dilakukan tanpa
Unity Editor. **Game tetap BELUM pernah dijalankan di Unity Editor
(versi mana pun) dan dua pemain nyata bersamaan BELUM pernah dites.**
Fase arsitektur terbesar: server + akun + izin tanah + sinkronisasi.

### Yang berubah di Fase 7

- **Server** (baru, folder `../server/`): `server.py` (HTTP
  `http.server`, ThreadingHTTPServer, batas laju 60 req/menit/IP,
  argumen `--host/--port/--db`, mencetak `DUNIAKITA_SIAP`) +
  `db.py` (SQLite: pemain/token/petak/izin/kas_kota/riwayat_kas;
  PBKDF2-HMAC-SHA256 + salt per akun; token acak
  `secrets.token_hex(24)`). Endpoint: daftar, masuk, dunia,
  ajukan-izin, bayar-izin, bangun, jual, pemain, kas-kota, sinkron,
  impor-tanah. Semua aksi divalidasi DI SERVER. Peta 24×24;
  pemain baru modal Rp5.000.000 di server.
- **Wali kota NPC** (baru, `Utils/SistemWaliKota.cs` + cerminnya di
  `server/db.py` — rumus SAMA di kedua tempat, server otoritas
  akhir): harga tanah pinggir Rp200.000 + Rp100.000 tiap langkah
  Chebyshev ke pusat (11,11) → pusat Rp1.400.000; biaya izin 10%
  harga tanah; reputasi ≥70 → −30%, <30 → +50%, <15 → DITOLAK
  otomatis; kas kota <Rp1jt → +50%, >Rp20jt → −20%; ada Balai Kota
  → −10%; paling murah Rp10.000; skor 50 awal (hunian +10,
  fasilitas +15, tiap bangunan sejenis di atas 2 dalam area 5×5
  mengurangi 15, variasi deterministik −5..+5 dari hash id pemain +
  petak — BUKAN Random, BUKAN `string.GetHashCode`); ≥40 disetujui,
  25–39 bersyarat, <25 ditolak; 4+ bangunan sejenis di kawasan →
  DITOLAK. Bersyarat → biaya ×1,5 + wajib Jalan bersebelahan
  (kecuali bangunan Jalan sendiri → hanya biaya lebih mahal).
  Dua perbaikan selama penulisan server: petak memakai kolom
  jangkar_x/jangkar_y (bangunan 2×2/3×3 hanya dihitung/dijual dari
  jangkarnya) dan kepadatan dihitung per bangunan berbeda (DISTINCT
  jangkar), bukan per petak.
- **Aturan dunia bersama** (baru, `Utils/AturanDuniaBersama.cs`):
  KunciPetak, ValidasiBangun (tapak harus milik + kosong + koin
  cukup + syarat jalan), HitungPemasukanOffline (kotor − pajak 2,5%
  − upkeep, maks 24 hari), DiffDunia (tambah/lepas per petak+id+
  pemilik — dipakai sinkronisasi).
- **Klien REST** (baru): `Managers/ServerManager.cs` (523 baris) +
  `Managers/ServerManagerAksi.cs` (391 baris, partial class — alur
  masuk online + impor + aksi) + `Utils/HttpDuniaBersama.cs`
  (GET/POST UnityWebRequest + normalisasi alamat + escape JSON) +
  `Utils/DtoDuniaBersama.cs` (DTO JsonUtility, field public supaya
  tanpa warning). Token di PlayerPrefs `DuniaKita_Token` (kata sandi
  TIDAK PERNAH disimpan); alamat server di PlayerPrefs
  `DuniaKita_Server` (default `http://127.0.0.1:8471`); sinkron
  tiap 10 detik + sesudah aksi; label status HUD
  ("Online • nama" / "Offline — main sendiri dulu"). Bila server
  tidak terjangkau: game jalan normal single player lokal.
- **Sinkron sesi** (`POST /api/sinkron`, baru di server + klien):
  klien mendorong koin + reputasi tiap 10 detik; server membatasi
  perubahan maks Rp6.000.000/sinkron (penjualan termahal kampus
  Rp10jt → jual 50% = Rp5jt + hadiah level Rp2jt). Klien menahan
  sinkron selama aksi bangun/jual/bayar berjalan. Batas ini BUKAN
  anti-curang penuh (klien masih bisa mendorong +Rp6jt tiap 10
  detik) — pembelian bangunan tetap divalidasi penuh di server.
- **Impor tanah single player** (`POST /api/impor-tanah`, baru di
  server + klien): saat masuk online pertama kali, bila pemain
  BELUM punya tanah di server, bangunan lokalnya didaftarkan
  TANPA bayar (maks 256 bangunan; tapak milik pemain lain
  dilewati; interior perabot tidak ikut). Impor kedua ditolak
  server ("hanya sekali").
- **UI baru**: `UI/PanelIzinTanah.cs` (pilih bangunan → ajukan →
  keputusan + bayar → info pemilik → kas kota; dibuat lazy oleh
  ServerManager — NOL perubahan UIManager) + `UI/PanelAkun.cs`
  (daftar/masuk/keluar; dibuat lazy dari PanelPengaturan) +
  `UiFactory.BuatInput` (kolom teks + placeholder + kata sandi
  berbintang) + tombol "Akun / Dunia Bersama" di PanelPengaturan
  (panel diperbesar 520→620). Label status online dibuat lazy
  ServerManager di Canvas (kanan atas, di bawah bar atas).
- **Hook BuildManager** (+155 baris → 541): TapLayar cabang online
  (petak kota → panel izin; milik sendiri → menu bangun; milik
  orang → info pemilik), CobaBangun cabang online (tapak milik +
  kosong dicek lokal; koin/izin/jalan divalidasi server),
  JualBangunan cabang online (milik orang lain ditolak; milik
  sendiri lewat server), BangunDariServer / LepasBangunan /
  BersihkanSemua (tanpa biaya/pengembalian — koin diurus server).
- **Guard SaveManager** (+9 baris): selama ModeOnline, `Simpan()`
  tidak menimpa berkas lokal (berkas lokal = cadangan dunia
  sendiri; disimpan sekali sebelum masuk online). Keluar online:
  dunia sendiri kembali setelah game dimuat ulang (GameManager
  tidak punya metode muat-ulang publik — sengaja, supaya tidak
  merusak simpanan).
- **Simpanan naik ke "6.0"** (5.1/5.0/4.0/3.0/2.0/1.0 tetap
  dimuat; pin versi di test-fase6 + test-balaikota + test-fase2/3/4/
  4b diperbarui — 6.0 kini didukung, 7.0 ditolak). GameManager dan
  UIManager TIDAK berubah di Fase 7 (tetap 600/590 baris).
- **Generator + scene**: `generate-project.py` mencakup 8 berkas
  baru (ServerManager, ServerManagerAksi, HttpDuniaBersama,
  DtoDuniaBersama, PanelIzinTanah, PanelAkun, SistemWaliKota,
  AturanDuniaBersama) + komponen ServerManager fileID 212 di
  GameRoot. Regenerasi di /tmp + diff: scene hanya bertambah
  komponen 212 + blok MonoBehaviour-nya; semua `.meta` lama
  byte-identik; 8 `.meta` baru dibuat; `.asset` + ProjectSettings
  + manifest tidak berubah.
- **README + server/README diperbarui**: seksi Fase 7, cara jalan
  server, cara uji online (satu Editor + server lokal + akun kedua
  lewat curl), matriks uji online O1–O10, aturan impor + sinkron,
  deploy (BELUM diputuskan; JANGAN ke VPS utama — ToS melarang
  game server).
- **Perbaikan tes yang ditemukan selama Fase 7** (bukan bug game):
  test-server sempat gagal 2× karena ekspektasi tes yang salah
  (biaya izin pusat Rp210.000, bukan Rp140.000 — kas kota awal 0
  memicu +50% "kas menipis"; bangun kampus di (0,1) ditolak karena
  tapak 3×3 belum dimiliki, bukan karena koin — tes diganti ke
  kafe 1×1); sinkron 500 karena `_sekarang()` tidak ada di db.py
  (dipakai `int(time.time())`); impor membuat plot cari_petak Uji
  jatuh di tanah Budi/Citra/Raka (hindaran ditambah). Semua
  diperbaiki di tesnya, server + game tidak diubah untuk itu.

### Verifikasi Fase 7 (semua dijalankan, dari biner baru)

- **Kompilasi**: `mcs -warn:4 -target:library` terhadap 44 script
  game + stub — **0 error, 0 warning**. Semua berkas ≤ 600 baris
  (ServerManager 523, ServerManagerAksi 391, BuildManager 541,
  GameManager 600, UIManager 590).
- **Tes server NYATA** (`python3 tools/test-server.py` — server
  localhost + HTTP end-to-end): **69/69, 0 gagal**. Dua pemain
  (Raka + Sinta), izin pusat Rp1.400.000 + biaya Rp210.000
  (kas menipis +50%), bayar, bangun rumah + warung, bangun di
  tanah orang lain DITOLAK, koin kurang DITOLAK, dunia sama untuk
  pemain kedua, kas kota = total 3 izin, pemasukan offline 2 hari
  Rp23.250 + batas 24 hari (Rp279.000), jual 50% (Rp375.000),
  sinkron koin/reputasi diterima + batas Rp6jt/negatif/reputasi
  150 ditolak + tanpa token 401, impor 2 bangunan Budi + impor
  kedua ditolak + Citra 1 masuk/2 dilewati, cabang wali kota
  db-level (reputasi 10 ditolak, Bersyarat + syarat jalan +
  bangun tanpa jalan ditolak + sesudah jalan boleh, kepadatan
  4 taman ditolak), batas laju 429.
- **Tes logika Fase 7** (`tools/test-fase7.cs`, baru): **71/71,
  0 gagal** — harga tanah per jarak (pusat 1,4jt, pinggir
  200–300rb, deterministik), biaya izin (dasar 10%, reputasi,
  kas, Balai Kota, minimal Rp10.000, gabungan 0,7×0,8×0,9),
  keputusan (setuju/tolak/bersyarat + alasan, fasilitas
  didukung kota, deterministik), syarat jalan, validasi bangun
  (milik sendiri/kosong/koin/syarat jalan), pemasukan offline
  (2 hari Rp23.250, batas 24 hari, hari negatif, tanpa
  bangunan), diff dunia (tambah/lepas/pemilik berubah),
  migrasi 6.0 (1.0–6.0 didukung, 7.0 tidak).
- **Tes lama diulang semua (pin versi diperbarui ke 6.0)** —
  ekonomi 22/22, fase2 47/47, fase3 143/143 (+1: cek 7.0
  ditolak), fase4 200/200 (+1), fase4b 164/164 (+1),
  balaikota 43/43 (+1), fase5 56/56, fase6 147/147 —
  **total 893 pemeriksaan (822 lama + 71 baru), 0 gagal**.
  Dengan server: **962 pemeriksaan, 0 gagal**.
- **Validasi YAML/GUID**: `tools/validasi-yaml.py` — lolos
  (scene + 131 .meta, termasuk 8 .meta baru Fase 7).
- **Cek UI bounds**: `tools/cek-ui-bounds.py` — **940/940, 0 gagal**
  (naik dari 708: panel izin + panel akun + tombol Akun).
  Dua tombol Tutup baru sempat 60 unit (di bawah minimal 90) —
  diperbaiki jadi 90 + panel diperbesar sebelum cek lolos.
- **QA data Fase 6 diulang** (`python3 tools/qa-fase6.py`): **419/419,
  0 gagal** — tidak ada perubahan data bangunan/`.asset`/
  ProjectSettings di Fase 7, angka Fase 6 tetap berlaku.

### Yang BELUM terverifikasi di Fase 7 (jujur dicatat)

- **Game BELUM pernah dijalankan di Unity Editor (versi mana
  pun).** Semua alur online (daftar/masuk, panel izin, bayar,
  bangun/jual online, sinkron, label status, impor tanah) hanya
  terverifikasi di level kode + kompilasi + tes logika/server —
  alur klik nyata, tampilan panel, dan perilaku coroutine
  UnityWebRequest di Editor BELUM terlihat. Matriks O1–O10 di
  README adalah langkah uji untuk user di Editor.
- **Dua pemain nyata bersamaan BELUM dites** (satu Editor + akun
  kedua lewat curl adalah cara yang didokumentasikan; belum
  dijalankan).
- **Performa server dengan banyak pemain BELUM dites** (satu
  pemain + 2 akun uji; SQLite + ThreadingHTTPServer, tanpa
  beban).
- **Deploy server BELUM diputuskan** (localhost saja; JANGAN ke
  VPS utama — ToS melarang game server).
- **Kamera + bounds grid 24×24 BELUM terlihat di Editor** (ZoomMaks
  14 dihitung cukup dari angka — lebar dunia 46 unit, ortho 14 →
  ±49,8 lebar di 16:9 — tapi belum dilihat).
- **Tombol "Perluas Lahan" di menu bangun masih tampil saat online**
  (UIManager tidak diubah; menekannya gagal dengan aman — koin
  dikembalikan + pesan — karena grid sudah 24; kosmetik, dicatat).
- **Tombol Jual di panel detail masih tampil untuk bangunan pemain
  lain** (ditolak dengan toast saat ditekan; kosmetik, dicatat).
- **Level server selalu 1** (syarat level bangunan hanya
  ditegakkan klien — pemain yang memodifikasi klien bisa
  membangun bangunan level tinggi; dicatat sebagai keterbatasan
  v1, bukan bug tersembunyi).
- **Sinkron koin dari klien dibatasi, bukan dibuktikan** (batas
  Rp6jt/sinkron; anti-curang penuh = pengembangan lanjutan).

## Yang SUDAH diverifikasi (dengan cara yang tertulis)

### 1. Semua script C# lolos kompilasi — 0 error, 0 warning
— Perintah: `mcs -target:library -warn:4` terhadap 36 script game +
  `tools/unity-stubs.cs` (stub API Unity minimal).
- Hasil: `Compilation succeeded - 0 warning(s)`.
- Catatan jujur: kompilasi memakai **stub**, bukan UnityEngine asli.
  Yang terbukti: sintaks C# benar dan pemakaian API konsisten dengan
  signature stub. Stub hanya berisi API yang dipakai script, dengan
  signature yang sama seperti Unity 2022 LTS yang umum dipakai
  (MonoBehaviour, GameObject, SpriteRenderer, Canvas, Button, Text,
  JsonUtility, Input, Touch, EventSystem, dsb.). Bila satu API ternyata
  berbeda signature di Unity asli, error baru muncul saat buka project
  di Editor — periksa Console di Unity saat pertama membuka project.

### 2. Logika murni teruji otomatis — 22/22 lolos (diperbarui Fase 2)
- Perintah: kompilasi `tools/test-ekonomi.cs` + `EconomyCalculator.cs` +
  `FormatUtil.cs` + `GameBalance.cs` + `IsometricGrid.cs` + stub,
  lalu jalankan dengan `mono`.
- Hasil: **22 pemeriksaan lolos, 0 gagal**, meliputi:
  - Ekonomi harian: 2 rumah + 1 warung + 1 taman = masuk Rp15.000,
    pajak Rp375, upkeep Rp4.500, keluar Rp4.875, bersih Rp10.125 (persis).
  - Populasi: 4 warga dari 2 rumah.
  - Kebahagiaan: 50 + 1 taman × 2 + 5 semua warga punya rumah = 57;
    bonus taman dibatasi +20 (30 taman → 75).
  - Harga jual: 50% × Rp500.000 = Rp250.000.
  - Simulasi 10 hari: koin 5.000.000 → 5.101.250 (persis).
  - Grid isometric: 144 petak (12×12) — `GridKeDunia` lalu
    `DuniaKeGrid` kembali ke petak yang sama; batas grid benar.
  - Format Rupiah: 5000000 → "Rp5.000.000".
- Tes Fase 2 terpisah (`tools/test-fase2.cs`): **47/47 lolos** — lihat
  § Fase 2 di atas (diperbarui Fase 3).
— Tes Fase 3 terpisah (`tools/test-fase3.cs`): **142/142 lolos** —
  lihat § Fase 3 di atas (diperbarui Balai Kota).
— Tes Fase 4 terpisah (`tools/test-fase4.cs`): **199/199 lolos** —
  lihat § Fase 4 di atas (diperbarui Balai Kota: unlock Balai Kota L3).
— Tes Fase 4B terpisah (`tools/test-fase4b.cs`): **163/163 lolos** —
  lihat § Fase 4B di atas (diperbarui Balai Kota).
— Tes Balai Kota terpisah (`tools/test-balaikota.cs`): **42/42 lolos** —
  lihat § Balai Kota di atas.
— Tes Fase 5 terpisah (`tools/test-fase5.cs`): **56/56 lolos** —
  lihat § Fase 5 di atas (tutorial 6 langkah + audio 6 SFX + musik).
— Tes Fase 6 terpisah (`tools/test-fase6.cs`): **147/147 lolos** —
  lihat § Fase 6 di atas (migrasi 1.0–5.1, versi rilis, progresi,
  pencapaian 26 Permata, misi harian, ekonomi dasar, interior).
— QA data Fase 6 (`tools/qa-fase6.py`): **419/419 lolos** — lihat
  § Fase 6 di atas (18 .asset silang GameBalance + README + sprite
  + ProjectSettings + invarian rilis).

### 3. Struktur & konsistensi file Unity — lolos
- Perintah: `python3 tools/validasi-yaml.py`.
— Hasil: 24 file YAML (MainScene, prefab, 18 .asset, 4 ProjectSettings)
  ter-parse; 121 file `.meta` ditemukan; semua referensi `guid:` di
  scene/prefab/.asset ter-resolve ke file `.meta` yang ada; semua
  referensi script (`fileID: 11500000`) menunjuk file `.cs` yang ada;
  tidak ada `fileID` duplikat dalam satu file; `manifest.json` valid.
— Pengecekan tambahan: 63 GUID unik di MainScene.unity masing-masing
  ter-resolve (11 script, tile, highlight, ikon Bangun, 18 .asset,
  prefab, 3 sprite warga diam, 6 frame jalan warga, 4 denah interior,
  10 sprite perabot, 7 klip audio).
— Jumlah baris per file C#: GameManager 599, UIManager 590 — semua
  di bawah/sama dengan batas 600 baris.

### 5. UI landscape terverifikasi geometris — 708/708 lolos (diperbarui Fase 5)
— Perintah: `python3 tools/cek-ui-bounds.py` (mem-parsing semua
  panggilan `UiFactory.AturPosisi` di UIManager, PanelMisi,
  PanelPengaturan, PanelKejadian, PanelPencapaian, PanelInterior,
  PanelPeristiwaPenghuni, PanelPerayaan, PanelTutorial; menghitung
  posisi absolut tiap elemen).
— Hasil: **708 pemeriksaan lolos, 0 gagal** (Fase 5: panel tutorial
  1200×300 + tombol "Ulangi Tutorial" di Pengaturan + panel perayaan
  di file baru; geometri lama tidak berubah), meliputi:
  - 235 elemen UI berada dalam batas layar 1920×1080 (metode
    bergulir hanya dicek horizontal — baris di bawah viewport
    sengaja di luar layar).
  - 76 tombol tingginya ≥90 unit acuan (tombol ikon Bangun 120,
    tombol panel 110, tombol bar atas 90).
  - Tombol dalam satu bar/panel tidak saling tumpang tindih.
  - Panel hanya menutupi 17–46% layar (menu bangun 32%, misi 37%,
    pencapaian 33%, pengaturan 18%, laporan 28%, perayaan 19%,
    kejadian 23%, interior 46%, peristiwa 40%, tutorial 17%) — grid
    tetap terlihat saat panel dibuka (panel interior paling besar,
    760×1000).
  - Catatan: tombol "Lihat Interior" dibuat lewat
    `PanelInterior.BuatTombolLihatInterior` (bukan metode void,
    jadi alat cek mengevaluasinya di bawah `PanelInterior.Bangun` —
    lolos di (620,460) 680×90). Tumpang tindihnya vs tombol Jual/Tutup
    (beda metode, tidak dicakup alat) dicek tangan: tombol y 420..510,
    Jual/Tutup y 520..630 — tidak bersentuhan.
- Framing kamera (dihitung, bukan diukur di Editor): grid 12×12 =
  dunia 26×13 unit (batas `BatasDunia()` ±13 / ±6,5). Pada ortho size 8
  dan aspek 16:9, lebar pandang 28,4 unit dan tinggi 16 unit — grid
  mengisi 91% lebar dan 81% tinggi layar. `UkuranKameraAwal = 8f`
  dipertahankan.
- Resolusi default PlayerSettings diubah ke landscape 1920×1080
  (`ProjectSettings.asset` + template `tools/generate-project.py`).
— **Bug yang ditemukan pemeriksa ini dan sudah diperbaiki:** tombol bar
  bawah semula diposisikan dengan koordinat absolut layar (y=950)
  padahal anak dari bar bawah harus relatif terhadap bar (y=10) —
  tombol akan jatuh di luar layar (y absolut 1890). Sudah diperbaiki
  ke y=10 dan lolos cek ulang 89/89 (Fase 1); cek ulang Fase 2: 106/106;
  cek ulang Fase 3: 159/159; cek ulang Fase 4: 453/453; cek ulang
  Fase 4B: 675/675; cek ulang Balai Kota: 695/695; cek ulang Fase 5:
  708/708.

### 4. Sprite kartun proper diperiksa visual
- Contact sheet `tools/contact-sheet.png` (2700×1704, 6 baris berlabel)
  diperiksa visual oleh agen: 18 bangunan + 9 warga + 4 denah interior
  + 10 perabot — semua terbaca jelas sebagai bangunannya, tidak ada yang
  rusak/terpotong. Lihat penilaian jujur + 3 cacat yang ditemukan dan
  diperbaiki di § Balai Kota di atas (tangga Perpustakaan, label denah,
  bendera Balai Kota).
- Semua sprite digambar prosedural dari kode (`tools/generate-sprites.py`),
  tanpa aset unduhan — tidak ada masalah lisensi. Dimensi kanvas tiap
  sprite sama seperti Fase 1–4B (hanya isi gambar yang digambar ulang).

## Yang BELUM terverifikasi (jujur dicatat)

1. **Game belum dijalankan di Unity Editor.** Tidak ada Unity di sandbox
   ini. Langkah pertama user di laptop: buka project di Unity Hub
   (2022.3.20f1), buka MainScene, tekan Play, dan laporkan apa yang
   terlihat di Console / Game View bila ada error.
2. **Impor aset di Editor belum teruji.** File `.meta` dan pengaturan
   importer sprite (`textureType: 8` = Sprite) ditulis mengikuti format
   Unity 2022; bila Unity mengimpor ulang dengan pengaturan berbeda,
   jalankan `tools/generate-project.py` ulang atau perbaiki lewat Inspector.
3. **Alur permainan end-to-end (tap → bangun → tunggu hari berganti
   otomatis → tutup → buka lagi) belum dimainkan.** Baru teruji di level
   logika (Tes 2 di atas). Muat simpanan dari JSON (`JsonUtility`) belum
   teruji runtime-nya.
4. **Tampilan UI di layar HP nyata belum dilihat.** Ukuran memakai
   referensi **landscape 1920×1080** dan tombol minimal 90 unit acuan
   (tombol ikon Bangun 120); geometri sudah lolos cek otomatis 708/708, tapi
   tampilan nyata (keterbacaan teks, warna, rasa sentuh, keterbacaan
   ikon palu+rumah) harus dicek di Editor (Game View 16:9) & HP.
5. **Build Android/iOS belum dicoba.** Skrip build + .bat sudah
   disiapkan di Fase 6 (BuildAndroid.cs lolos kompilasi terhadap
   stub; `build-android.bat` siap double-klik) — Switch Platform +
   Build And Run / batchmode tetap menunggu laptop user.
   Build iOS tetap butuh Mac + Xcode.
6. **Balai Kota, frame jalan warga, dan tampilan kartun belum dilihat
   di Editor.** Lihat "Yang BELUM terverifikasi di Balai Kota + Kartun"
   di atas — menu "Sudah dibangun", reputasi +2/hari, langkah warga,
   dan kesan kartun di Game View masih menunggu uji user.
7. **Suara, tutorial, dan performa HP (Fase 5) belum dinilai di
   perangkat nyata.** Lihat "Yang BELUM terverifikasi di Fase 5"
   di atas — bunyi SFX/musik di telinga, panel tutorial di layar,
   dan fps di HP nyata masih menunggu uji user.
8. **Build Android, tangkapan layar, listing Play Store, dan beta
   tertutup (Fase 6) belum dijalankan.** Lihat "Yang BELUM
   terverifikasi di Fase 6" di atas — .bat + materi rilis siap,
   eksekusinya menunggu laptop dan akun developer user.

## Keputusan desain yang dicatat

- **Grid kustom, bukan Tilemap Unity** (GDD §13 menyebut Tilemap sebagai
  pilihan): logika penempatan, pengecekan hunian petak, dan simpan/muat
  lebih sederhana dengan grid data sendiri; tile hanya digambar sebagai
  sprite diamond oleh GridManager.
- **Satu prefab generik "Bangunan"** (bukan 3 prefab per jenis bangunan):
  `BuildingView` diisi `BuildingData` + sprite saat runtime. Menambah
  jenis bangunan baru = tambah 1 file `.asset`, tanpa prefab baru.
- **UI dibangun lewat kode** (UIManager + UiFactory), bukan prefab UI
  di Editor — supaya scene tetap kecil dan semua UI bisa direview
  dari kode.
- **Modal awal Rp5.000.000 & harga bangunan** (Rumah 500rb, Warung 750rb,
  Taman 250rb; Warung +Rp15.000/hari) adalah keputusan Fase 1 —
  GDD tidak menetapkan angka-angka ini. Semua bisa diubah tanpa
  menyentuh logika (lihat README "Mengubah angka game").
- **UI landscape 1920×1080** (GDD §17, 24 Nov 2026): CanvasScaler
  acuan (1920,1080); semua elemen ditata dalam koordinat kiri-atas
  acuan itu; bar atas = 6 angka status + Misi & Pengaturan, bar bawah =
  tombol Bangun IKON saja (sejak revisi Fase 3 — sebelumnya tiga tombol
  Bangun / Lewati Hari / Simpan). Panel misi, pengaturan & kejadian
  adalah file terpisah (`UI/PanelMisi.cs`, `UI/PanelPengaturan.cs`,
  `UI/PanelKejadian.cs`) supaya UIManager tetap di bawah 600 baris
  (358 baris).
- **Hadiah misi tutorial** (keputusan Fase 1, 24 Nov 2026): Rumah
  Rp25.000, Warung Rp50.000, 4 Warga Rp50.000 — kecil dibanding modal
  awal Rp5.000.000; sekali klaim per misi (dicatat di PlayerPrefs).
- **Aturan tunawisma (-3) belum bisa terpicu dalam permainan normal
  Fase 2**: populasi selalu sama dengan kapasitas hunian (warga langsung
  menempati rumah yang dibangun), jadi kondisi "warga melebihi kapasitas"
  tidak terjadi. Aturan ini diimplementasikan + diuji dengan angka
  sintetis (6 warga vs kapasitas 4 → 47) dan baru relevan bila ada
  mekanisme yang menambah warga melebihi kapasitas (Fase 3 ke atas).
- **Posisi warga saat memuat game dibangkitkan ulang acak** (dipilih
  karena sederhana; posisi warga hanya kosmetik). Populasi/kebahagiaan
  disimpan di JSON tapi dihitung ulang dari bangunan saat muat, supaya
  nilainya selalu konsisten dengan dunia.
- **Biaya perawatan tetap dari .asset Fase 1** (Rumah Rp1.000, Warung
  Rp2.000, Taman Rp500/hari) — angka contoh di perintah tugas
  (Rp2.000/Rp5.000/Rp1.000) sengaja tidak dipakai supaya simulasi
  10 hari tetap sehat dan tes Fase 1 tetap valid.
- **Generator diperbarui untuk Fase 2**: `generate-project.py` kini
  menulis `.meta` warga1/2/3 + `PejalanKaki.cs` (+ `PanelMisi.cs` /
  `PanelPengaturan.cs` yang tertinggal dari perombakan landscape) dan
  field `spriteWarga` di scene — regenerasi tidak lagi menghilangkan
  Fase 2. Scene hasil edit tangan diverifikasi sama persis dengan
  keluaran generator.
- **Keputusan angka Fase 3 (GDD tidak menetapkan — dicatat alasannya)**:
  hadiah naik level L2 Rp100.000 / L3 Rp250.000 (seimbang dengan
  pemasukan harian dunia standar ±Rp10.000/hari dan biaya Kost
  Rp2.000.000); Kost Rp2.000.000 / sewa Rp40.000 / perawatan
  Rp5.000 (sewa ≈ 2,7× warung — Kost 2×2 menampung 8 warga, jadi
  sewa per warga Rp5.000/hari); Toko Kelontong Rp1.500.000 /
  +Rp30.000 / perawatan Rp4.000 (2× warung, setara posisinya sebagai
  toko besar); peluang kejadian 20%/hari (≈1 tiap 5 hari); biaya
  festival Rp50.000, payung Rp20.000, pedagang Rp100.000 → 1 Permata.
- **Toko Kelontong dihitung sebagai "warung" di ekonomi**
  (`EconomyManager.BuatCatatan`: `AdalahWarung` untuk `warung` dan
  `toko_kelontong`) — pemasukannya dipengaruhi kebahagiaan + hujan
  seperti warung, sesuai deskripsi GDD §4 (toko komersial). Sewa Kost
  masuk "pemasukan lain" (tidak dipengaruhi kebahagiaan/hujan).
- **Hari otomatis + simpan otomatis (revisi desain user 24 Nov 2026)**:
  durasi hari 300 detik dan interval simpan 60 detik di GameBalance;
  pewaktu dijeda selama panel perayaan/kejadian terbuka; kelebihan
  waktu dibuang (maks 1 hari per batas) supaya tidak ada lonjakan
  hari sesudah jeda panjang. `PerbaruiHUD` dipanggil tiap frame dari
  `GameManager.Update` demi hitung mundur — 6 assignment teks per
  frame, biayanya dapat diabaikan.
- **Generator diperbarui untuk Fase 3**: script ProgresiDunia /
  SistemKejadian / PewaktuHari / PanelKejadian, sprite kost / toko /
  ikon-bangun, `.asset` Kost / TokoKelontong, template `.asset`
  (field `levelBuka` + ukuran parametrized), field `ikonBangun` di
  scene (UIManager). Regenerasi hanya mengubah file yang diharapkan
  (diff diverifikasi).
- **Stub Unity diperluas (Fase 3)**: `UnityEngine.Random` (nilai 0,5
  deterministik — dipakai roll kejadian; di Unity asli acak sungguhan)
  dan field `Image.sprite` / `preserveAspect` (dipakai
  `UiFactory.BuatTombolIkon`).
- **Stub Unity diperluas**: `Mathf.Sin` dan `Mathf.Abs` ditambahkan ke
  `tools/unity-stubs.cs` (API asli Unity; dipakai ayunan langkah warga).
- **Toggle suara** menyimpan preferensi di PlayerPrefs dan mengatur
  `AudioListener.volume`; belum ada sumber suara di game, jadi toggle
  baru terasa efeknya setelah audio ditambahkan (fase polish).
- **Generator diperbarui untuk Fase 4**: script PanelPencapaian /
  SistemPencapaian / SistemMisiHarian, 12 sprite baru, 12 `.asset`
  baru, template `.asset` (field `bonusBahagia`), daftarBangunan
  scene 5 → 17 entri. Regenerasi hanya mengubah file yang diharapkan
  (diff diverifikasi); deskripsi `.asset` TIDAK boleh mengandung
  ": " (titik dua + spasi) — YAML tidak valid (3 `.asset` sempat
  gagal parse karena ini).
- **Stub Unity diperluas (Fase 4)**: `RectMask2D` + `ScrollRect`
  (dipakai `UiFactory.BuatScroll`).
- **Keputusan angka Fase 4 (GDD tidak menetapkan — dicatat alasannya)**:
  hadiah naik level L4 Rp500.000 / L5 Rp1.000.000 / L6 Rp2.000.000
  (naik 2× tiap level, seimbang dengan biaya bangunan L4–L6
  Rp2–12 juta); Jalan Rp50.000 (murah supaya tidak ada alasan tidak
  membangunnya); Perpustakaan Rp800.000/bahagia +2, Sekolah
  Rp3.000.000/+4, Pasar Rp4.000.000/+Rp60.000 (2× Toko Kelontong),
  Pos Rp1.000.000/+2, Apartemen Rp5.000.000/12 warga/sewa Rp60.000
  (sewa per warga Rp5.000, sama seperti Kost), Kafe Rp2.000.000/
  +Rp25.000/+2, Klinik Rp4.000.000/+4, Pembangkit Rp6.000.000/+4,
  Perumahan Rp8.000.000/20 warga/sewa Rp100.000 (sewa per warga
  Rp5.000, sama), Kampus Rp10.000.000/+Rp80.000/+6, Alun-alun
  Rp12.000.000/+10 (landmark termahal, bonus terbesar); perluasan
  lahan Rp5.000.000 (setara 1 Apartemen — mahal tapi terjangkau di
  Level 5); hadiah Permata pencapaian 2–5 (Metropolis 10 — paling
  sulit); hadiah misi harian Rp25.000–Rp40.000 (kecil, harian).
- **Penyederhanaan yang dicatat (bukan bug)**: Pos Keamanan +2
  bahagia (rasa aman) — tidak ada sistem kejahatan di game;
  Pembangkit +4 bahagia (listrik stabil) — tidak ada sistem listrik;
  Kampus memberi pemasukan + bahagia (GDD: "membuka warga bergaji
  tinggi" — diimplementasikan sederhana, tanpa sistem pekerjaan/
  gaji terpisah). Bila sistem-sistem itu ditambahkan nanti, angka
  bonus ini perlu ditinjau ulang.
- **Aturan Jalan (GDD §5.5)**: GDD hanya menyebut Jalan sebagai
  penghubung; aturan "tanpa Jalan = −50% pemasukan & bonus" adalah
  keputusan Fase 4 supaya Jalan punya fungsi mekanik. Kapasitas
  hunian sengaja TIDAK terpengaruh (warga tetap tinggal — pindah
  paksa warga akan terasa menghukum).
- **Misi harian dinilai dari perkiraan, bukan realisasi**: pemasukan
  "hari ini" memakai `PerkiraanHarian()` (forecast) karena misi
  berganti set saat hari berakhir — memakai realisasi akan membuat
  misi itu tidak pernah bisa selesai di hari yang sama.
- **Batas 12 sprite warga dipertahankan** (performa): kapasitas
  hunian total kini 300+ (Apartemen 12, Perumahan 20), tapi
  `CitizenManager` tetap membatasi pejalan kaki yang digambar ke 12
  sprite; populasi/kebahagiaan/level tetap dihitung penuh.
- **Unity 2022.3.20f1** dipakai sebagai versi LTS (ProjectVersion.txt).
- **Stub Unity diperluas (Fase 5)**: `AudioClip` + `AudioSource`
  (Play/Stop/PlayOneShot/isPlaying), `Application.targetFrameRate`,
  `PlayerPrefs` kini benar-benar menyimpan (kamus di memori, supaya
  tes tutorial/audio bisa menguji putaran SetInt→GetInt), dan
  `Component`/`GameObject.transform` jadi properti lazy (di Unity
  asli transform tidak pernah null; stub lama null sehingga
  `AudioManager` tidak bisa dites). Tes lama tidak terpengaruh
  (tidak ada kode yang mengandalkan transform null).
- **Audio Fase 5 (keputusan teknis)**: SFX DecompressOnLoad
  (file kecil, langsung siap), musik latar Streaming (file 4,2 MB
  tidak membebani memori HP); volume musik 0,35 terpisah dari SFX
  supaya tidak menenggelamkan efek; kunci suara
  `DuniaKita_Suara` satu sumber kebenaran di `AudioManager`
  (PanelPengaturan + AudioManager memakai kunci yang sama);
  `AudioListener.volume` tetap saklar utama (0 = bisu total).
- **Generator diperbarui untuk Fase 5**: script baru, template .meta
  audio + ikon, komponen AudioManager fileID 211 di scene,
  ProjectSettings (Uhlabs/com.uhlabs.duniakita/1.0/Landscape/ikon)
  + QualitySettings (Medium default). Regenerasi penuh di /tmp lalu
  diff — hanya file yang dimaksud yang berubah; project nyata sama
  persis dengan keluaran generator.

## Cara menjalankan ulang semua pemeriksaan

```bash
cd ~/workspace/dunia-kita
python3 tools/generate-sprites.py        # gambar ulang sprite
python3 tools/generate-audio.py          # sintesis ulang SFX + musik (Fase 5)
python3 tools/generate-ikon-aplikasi.py  # gambar ulang ikon aplikasi (Fase 5)
python3 tools/generate-project.py        # tulis ulang scene/prefab/.asset/.meta
python3 tools/validasi-yaml.py           # validasi YAML + GUID
python3 tools/cek-ui-bounds.py           # cek geometri UI landscape 1920x1080
mcs -target:library -out:/tmp/DuniaKita.dll -warn:4 tools/unity-stubs.cs \
  unity-project/Assets/Scripts/Data/*.cs unity-project/Assets/Scripts/Utils/*.cs \
  unity-project/Assets/Scripts/Grid/*.cs unity-project/Assets/Scripts/Managers/*.cs \
  unity-project/Assets/Scripts/UI/*.cs
mcs -out:/tmp/test-ekonomi.exe -warn:4 tools/unity-stubs.cs tools/test-ekonomi.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/FormatUtil.cs \
  unity-project/Assets/Scripts/Grid/IsometricGrid.cs
mono /tmp/test-ekonomi.exe
mcs -out:/tmp/test-fase2.exe -warn:4 tools/unity-stubs.cs tools/test-fase2.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/SaveData.cs \
  unity-project/Assets/Scripts/Utils/FormatUtil.cs
mono /tmp/test-fase2.exe
mcs -out:/tmp/test-fase3.exe -warn:4 tools/unity-stubs.cs tools/test-fase3.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/SaveData.cs \
  unity-project/Assets/Scripts/Utils/FormatUtil.cs \
  unity-project/Assets/Scripts/Utils/ProgresiDunia.cs \
  unity-project/Assets/Scripts/Utils/SistemKejadian.cs \
  unity-project/Assets/Scripts/Utils/PewaktuHari.cs \
  unity-project/Assets/Scripts/Grid/IsometricGrid.cs
mono /tmp/test-fase3.exe
mcs -out:/tmp/test-fase4.exe -warn:4 tools/unity-stubs.cs tools/test-fase4.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/SaveData.cs \
  unity-project/Assets/Scripts/Utils/FormatUtil.cs \
  unity-project/Assets/Scripts/Utils/ProgresiDunia.cs \
  unity-project/Assets/Scripts/Utils/SistemPencapaian.cs \
  unity-project/Assets/Scripts/Utils/SistemMisiHarian.cs \
  unity-project/Assets/Scripts/Grid/IsometricGrid.cs
mono /tmp/test-fase4.exe
mcs -out:/tmp/test-fase4b.exe -warn:4 tools/unity-stubs.cs tools/test-fase4b.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/FormatUtil.cs \
  unity-project/Assets/Scripts/Utils/SaveData.cs \
  unity-project/Assets/Scripts/Utils/SistemInterior.cs \
  unity-project/Assets/Scripts/Utils/SistemPeristiwaPenghuni.cs
mono /tmp/test-fase4b.exe
mcs -out:/tmp/test-balaikota.exe -warn:4 tools/unity-stubs.cs tools/test-balaikota.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/SaveData.cs \
  unity-project/Assets/Scripts/Utils/SistemBalaiKota.cs \
  unity-project/Assets/Scripts/Managers/PejalanKaki.cs \
  unity-project/Assets/Scripts/Grid/IsometricGrid.cs \
  unity-project/Assets/Scripts/Grid/GridManager.cs
mono /tmp/test-balaikota.exe
mcs -out:/tmp/test-fase5.exe -warn:4 tools/unity-stubs.cs tools/test-fase5.cs \
  unity-project/Assets/Scripts/Managers/AudioManager.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/SistemTutorial.cs
mono /tmp/test-fase5.exe
mcs -out:/tmp/test-fase6.exe -warn:4 tools/unity-stubs.cs tools/test-fase6.cs \
  unity-project/Assets/Scripts/Utils/EconomyCalculator.cs \
  unity-project/Assets/Scripts/Utils/FormatUtil.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Scripts/Utils/SaveData.cs \
  unity-project/Assets/Scripts/Utils/SistemInterior.cs \
  unity-project/Assets/Scripts/Utils/SistemBalaiKota.cs \
  unity-project/Assets/Scripts/Utils/ProgresiDunia.cs \
  unity-project/Assets/Scripts/Utils/SistemPencapaian.cs \
  unity-project/Assets/Scripts/Utils/SistemMisiHarian.cs
mono /tmp/test-fase6.exe
python3 tools/qa-fase6.py
mcs -target:library -warn:4 -out:/tmp/cek-build.dll tools/unity-stubs.cs \
  tools/unity-editor-stubs.cs \
  unity-project/Assets/Scripts/Data/GameBalance.cs \
  unity-project/Assets/Editor/BuildAndroid.cs
```
