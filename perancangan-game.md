# Perancangan Game — Dunia Kita

**Disusun:** 24 November 2026
**Status:** Draf perancangan v1 — menunggu persetujuan sebelum pembangunan
**Sifat proyek:** Proyek PRIBADI (bukan untuk lomba BTN). Skala besar, dibangun bertahap.

## 1. Identitas Proyek

| Item | Isi |
|---|---|
| Nama | **Dunia Kita** |
| Genre | Simulasi **tycoon** (pengelola usaha & sewa) 2,5D — bukan city builder |
| Platform | Mobile — **Android + iOS**; satu project Unity dapat di-build ke berbagai platform (Windows/Mac/WebGL menyusul bila diperlukan) |
| Engine | **Unity** (versi LTS, 2022 LTS) |
| Bahasa | C# |
| Tampilan | Isometric 2,5D — dunia terlihat berdimensi dari sudut miring, logika permainan tetap 2D |
| Mode | **Multipemain: satu peta bersama, banyak pemain sekaligus** (diputuskan user 25 Nov 2026) |
| Pemain | Tiap pemain adalah pemilik & pengelola usaha (warung, toko, kafe) dan sewa (kost, apartemen, kontrakan) di satu peta bersama; warga NPC adalah pembeli dan penyewa |

## 2. Visi & Pilar Desain

**Visi:** satu dunia bersama di genggaman, tempat tiap pemain berdagang dan menyewakan kamar kepada warga NPC yang sama dengan pemain lain — kost, warung, toko, kafe, apartemen — dan melihat kota itu hidup dari kosong sampai ramai.

**Pilar desain (tidak boleh dilanggar):**
1. **Pemain pedagang, bukan pembangun kota** — pemain membangun dan mengelola usaha serta hunian sewa; rumah-rumah warga, sekolah, klinik, taman dan balai kota disediakan Kota.
2. **Mudah dimainkan di HP** — semua aksi cukup tap, geser, dan cubit (pinch); tidak ada kontrol rumit.
3. **Dunia yang hidup dan dibagi** — warga NPC yang sama terlihat oleh semua pemain: mereka berjalan, menyewa kamar, berbelanja, dan masuk-keluar bangunan.
4. **Progresi yang terasa** — dari satu warung sampai jaringan usaha yang bersaing dengan pemain lain.

## 3. Konsep Permainan

Pemain memulai dengan modal awal dan membangun usaha pertama: warung, lalu kost, lalu toko/kafe. Warga NPC datang sendiri sebagai pembeli dan penyewa; dari merekalah koin pemain berasal. Karena satu peta dipakai bersama, warga yang sama bisa memilih warung pemain lain — jumlah tamu dan penyewa dihitung server, sehingga pemain bersaing sehat.

**Core loop (satu putaran permainan):**
Bangun/tingkatkan usaha & hunian → warga NPC menyewa kamar dan berbelanja → sewa & penjualan masuk (server) → perluas usaha, tawarkan kamar lebih banyak → hadapi kejadian (warga protes, hujan, festival) → ulangi.

**Kondisi menang:** tidak ada "tamat" — ini game sandbox. Kemenangan diukur lewat **tingkatan dunia** (Desa → Kota Kecil → Kota → Metropolis) dan pencapaian (achievement). Pemain bebas terus bermain setelah mencapai Metropolis.

## 4. Dunia & Kamera 2,5D

- **Grid isometric:** dunia dibagi petak (tile) isometric; tiap bangunan menempati 1×1, 2×2, atau 3×3 petak.
- **Kamera:** sudut miring tetap (tidak bisa diputar bebas seperti game 3D); pemain bisa menggeser (pan) dan memperbesar/memperkecil (pinch zoom).
- **Waktu:** satu hari game = beberapa menit waktu nyata (angka pasti diputuskan saat prototipe); ada siklus siang–malam yang terlihat dari pencahayaan.
- **Ukuran dunia awal:** 12×12 petak; dapat diperluas dengan uang atau naik level (maks diputuskan saat pengembangan, target 24×24).

## 5. Sistem Bangunan

Bangunan dikelompokkan dalam 5 kategori. Tiap bangunan punya: biaya bangun, ukuran petak, pemasukan/biaya per hari, kebutuhan (listrik, air, jalan), dan efek ke kebahagiaan warga sekitar.

### 5.1 Hunian (tempat warga tinggal)
| Bangunan | Ukuran | Fungsi |
|---|---|---|
| Rumah Sederhana | 1×1 | Menampung 2 warga |
| Kost (Rumah Sewa) | 2×2 | Menampung 8 warga + pemasukan sewa harian |
| Apartemen Kecil | 2×2 | Menampung 12 warga |
| Perumahan | 3×3 | Menampung 20 warga, butuh jalan + air |

### 5.2 Pendidikan
| Bangunan | Ukuran | Fungsi |
|---|---|---|
| Sekolah Dasar | 2×2 | Menaikkan kebahagiaan & membuka warga "pelajar" |
| Perpustakaan | 1×1 | Kebahagiaan area sekitar naik |
| Kampus Kecil | 3×3 | (Fase lanjutan) membuka pekerjaan bergaji tinggi |

### 5.3 Komersial (sumber pemasukan utama)
| Bangunan | Ukuran | Fungsi |
|---|---|---|
| Warung | 1×1 | Pemasukan kecil, butuh warga di dekatnya |
| Toko Kelontong | 1×1 | Pemasukan menengah |
| Pasar | 2×2 | Pemasukan besar, butuh jalan |
| Kafe | 1×1 | Pemasukan + kebahagiaan |

### 5.4 Fasilitas & Layanan — **milik Kota, bukan pemain**
| Bangunan | Ukuran | Fungsi |
|---|---|---|
| Taman | 1×1 | Kebahagiaan area sekitar naik |
| Pos Keamanan | 1×1 | Keamanan antar pemain dijaga NPC Kota |
| Klinik | 2×2 | Kesehatan warga, kebahagiaan naik |
| Sekolah / Perpustakaan / Kampus | 2×2 | Pendidikan warga; diurus Kota |
| Balai Kota | 3×3 | Wali kota NPC, izin & kas kota |
| Pembangkit Listrik Kecil | 2×2 | Menyediakan listrik untuk area |
| Sumur / Menara Air | 1×1 | Menyediakan air untuk area |

Pemain tidak membangun bagian ini: server menolak izin, pembangunan, dan pembelian tanah untuk bangunan kategori Fasilitas (diputuskan user 25 Nov 2026).

### 5.5 Jalan & Dekorasi
- Jalan (1 petak): menghubungkan bangunan; bangunan tanpa jalan tidak berfungsi penuh.
- Pohon, lampu jalan, bangku: dekorasi murah yang sedikit menaikkan kebahagiaan.

**Aturan buka (unlock):** 3–4 bangunan tersedia sejak awal (Rumah Sederhana, Warung, Taman, Jalan). Sisanya terbuka bertahap seiring level dunia naik — supaya pemain tidak kewalahan di awal.

### 5.6 Sistem Interior (Penuh) — diputuskan user 24 Nov 2026

Game ini memiliki interior penuh, dengan cara kerja **tembus pandang (x-ray)** — BUKAN masuk ke dalam bangunan sebagai karakter 3D, dan BUKAN scene interior terpisah.

**Cara kerja:** pemain mengetuk bangunan hunian → dinding dan atap bangunan itu menjadi transparan di tempatnya (bangunan tetap di petaknya di grid isometric) → ruangan dan perabot di dalamnya langsung terlihat → dalam mode ini pemain bisa menambah, memindah, atau mengganti perabot → ketuk lagi / tombol tutup → bangunan kembali padat seperti semula.

**Bangunan yang punya interior:** semua hunian (Rumah Sederhana, Kost, Apartemen, Perumahan). Sekolah dan toko mendapat interior di fase lanjutan (diputuskan saat fase interior berjalan).

**Ruangan di dalam hunian:**
| Ruangan | Fungsi |
|---|---|
| Kamar Tidur | Tempat warga beristirahat; kasur menaikkan kebahagiaan |
| Dapur | Memasak; kompor + meja makan menaikkan kebahagiaan |
| Kamar Mandi | Kebutuhan dasar; tanpa kamar mandi kebahagiaan turun |
| Ruang Tamu | Ruang berkumpul; TV/sofa menaikkan kebahagiaan |

**Perabot:** tiap ruangan diisi perabot yang dibeli dengan koin (contoh: kasur, meja, kursi, kompor, TV, lemari, lampu). Tiap perabot punya harga dan efek (kebahagiaan +, kadang pemasukan kost naik bila kamar kost berperabot lengkap).

**Aturan interior:**
- Rumah kosong tanpa perabot = penghuni tidak betah (kebahagiaan rendah, bisa pindah).
- Kost dengan kamar berperabot lengkap bisa dipasang harga sewa lebih tinggi.
- Interior disimpan per bangunan (bagian dari data simpanan game).
- Aset interior: sprite perabot + lantai/dinding dalam, digambar terpisah dari aset luar (beban seni naik — lihat §16).

**Keterkaitan dengan sistem lain:** kebahagiaan dari interior masuk ke perhitungan kebahagiaan warga (§7); harga sewa kost dipengaruhi kelengkapan perabot (§6); misi dapat meminta pemain melengkapi interior tertentu.

## 6. Sistem Ekonomi

- **Mata uang:** Koin (diperoleh dari **sewa kamar dan belanja warga NPC**) dan Permata (langka, dari pencapaian).
- **Pemasukan harian:** sewa per kamar terisi (kost/apartemen/perumahan) + belanja tamu di warung/toko/kafe/pasar. Tidak ada lagi angka pemasukan tetap per bangunan.
- **Pengeluaran harian:** perawatan bangunan (dipotong server tiap hari) + pajak kota 2,5% dari pemasukan kotor.
- **Aturan penting:** warga NPC adalah sumber uang satu-satunya. Bangunan komersial punya kapasitas tamu per hari, jadi pemain yang membangun terlalu banyak toko akan berebut tamu yang sama — dengan pemain lain di peta yang sama.
- **Fasilitas umum** (balai kota, klinik/RS, sekolah, perpustakaan, taman, pos keamanan, pembangkit, kampus, alun-alun) milik **Kota** dan diurus NPC: pemain tidak bisa membeli tanahnya, mengajukan izin, atau membangunnya.
- **Pinjaman (rencana fase lanjutan):** pemain bisa meminjam modal dengan cicilan harian — mengajarkan manajemen utang.

## 7. Sistem Warga

- **Warga adalah NPC milik server**, bukan angka hasil jumlah hunian. Tiap distrik punya 40 warga bernama Indonesia; semua pemain melihat warga yang sama (`server/db.py` tabel `warga`).
- **Menyewa:** warga mengisi kamar kosong di hunian milik pemain dan membayar sewa tiap hari game ke pemiliknya. Kamar kosong berarti pemasukan berhenti.
- **Berbelanja:** tiap hari tiap warga memilih satu warung/toko/kafe/pasar milik pemain; jumlah tamu dibatasi kapasitas bangunan.
- **Bergaji:** warga menerima upah harian; bila uangnya habis ia melewati sewa hari itu.
- **Satuan waktu:** 1 hari game = 300 detik; simulasi berjalan di server saat ada permintaan (maks 24 hari per jeda), jadi pemain tetap mendapat sewa saat aplikasi ditutup.
- **Visual:** warga tampil sebagai karakter kecil yang berjalan, lalu **masuk ke bangunan** dan keluar lagi (pintu, animasi mengecil/memudar, muncul kembali).
- **Karakter penghuni ditambahkan nanti** (diputuskan user 24 Nov 2026): sprite + animasi penghuni dibuat di fase lanjutan. Di mode tembus pandang, penghuni terlihat berada di dalam ruangan (mis. di kamar tidur / ruang tamu).

## 8. Sistem Level & Progresi Dunia

| Tingkat Dunia | Syarat naik | Yang terbuka |
|---|---|---|
| 1 — Tanah Kosong | Awal permainan | Rumah Sederhana, Warung, Taman, Jalan |
| 2 — Dusun | 10 warga | Kost, Toko Kelontong, Perpustakaan |
| 3 — Desa | 30 warga + kebahagiaan ≥60 | Sekolah Dasar, Pasar, Pos Keamanan |
| 4 — Kota Kecil | 80 warga + kebahagiaan ≥70 | Apartemen, Kafe, Klinik, Pembangkit Listrik |
| 5 — Kota | 150 warga + kebahagiaan ≥75 | Perumahan, Kampus Kecil, perluasan lahan |
| 6 — Metropolis | 300 warga + kebahagiaan ≥80 | Bangunan landmark (alun-alun kota) |

- **XP** diperoleh dari membangun, menaikkan kebahagiaan, dan menyelesaikan misi.
- **Pencapaian (achievement):** misalnya "Juragan Kost" (punya 5 kost), "Kota Pelajar" (punya sekolah + kampus), "Kota Hijau" (20 taman).

## 9. Misi & Kejadian

- **Misi tutorial (awal):** membangun rumah pertama, membangun warung, menerima 5 warga pertama, mencapai Tingkat 2. Misi memandu pemain, bukan memaksa.
- **Misi harian (rencana):** tugas kecil berhadiah koin/permata.
- **Kejadian acak:** warga protes (kurang taman), kebakaran kecil (butuh pos keamanan), musim hujan (pendapatan toko turun), festival (pemasukan naik sehari). Pemain memilih tindakan; tiap pilihan punya konsekuensi.

### 9.1 Peristiwa Penghuni (diputuskan user 24 Nov 2026)

Kejadian yang berpusat pada penghuni, dengan pilihan dan konsekuensi yang berbeda — bukan sekadar notifikasi:

| Peristiwa | Pilihan pemain | Konsekuensi |
|---|---|---|
| Anak kost tidak bisa bayar uang kos | Beri keringanan 1 bulan | Reputasi + , pemasukan bulan itu hilang |
| | Tagih tegas / usir | Pemasukan kembali sebagian, reputasi −, kamar kosong |
| | Cicil pembayarannya | Pemasukan masuk bertahap 2 bulan, reputasi tetap |
| Anak kost melarikan diri tanpa bayar | Biarkan saja | Kehilangan sewa 1 bulan, reputasi − kecil |
| | Laporkan ke pengurus / blacklist | Biaya urus, reputasi + (tegas), kamar kosong sampai ada penghuni baru |
| Penghuni mengeluh tetangga berisik | Tegur tetangga | Kebahagiaan area naik, reputasi + |
| | Abaikan | Kebahagiaan penghuni turun, bisa pindah |
| Penghuni merusak perabot | Potong uang deposit | Uang kembali sebagian, reputasi − kecil |
| | Maafkan & perbaiki sendiri | Biaya perbaikan, reputasi + |
| Penghuni merekomendasikan teman | Terima temannya | Kamar terisi lebih cepat, reputasi + |
| Penghuni minta pindah kamar | Setujui | Kebahagiaan penghuni naik |
| | Tolak | Kebahagiaan turun, bisa pindah keluar |

**Aturan peristiwa:** muncul acak (tidak setiap hari); tiap peristiwa punya 2–3 pilihan; tidak ada pilihan yang selalu benar — pemain menimbang uang vs reputasi vs kebahagiaan.

## 10. Kontrol Mobile (Touch)

| Gestur | Fungsi |
|---|---|
| Tap bangunan | Buka panel detail (info, tingkatkan, pindahkan, jual) |
| Tap tanah kosong | Buka menu bangun |
| Drag 1 jari | Geser kamera (pan) |
| Pinch (2 jari) | Zoom masuk/keluar |
| Tap tombol menu | Buka toko bangunan, misi, pencapaian, pengaturan |

- Target sentuh minimal 44px. Semua teks Bahasa Indonesia. Tidak ada kontrol yang butuh keyboard.

## 11. Layar & Antarmuka

1. **Layar utama (dunia):** grid isometric, panel atas (koin, permata, level dunia, populasi, kebahagiaan), tombol menu bawah.
2. **Menu bangun:** daftar bangunan per kategori dengan harga dan syarat.
3. **Panel detail bangunan:** info, pemasukan, kondisi, tombol tingkatkan/pindahkan/jual.
4. **Misi & pencapaian:** daftar misi aktif dan pencapaian yang sudah diraih.
5. **Pengaturan:** suara, simpan manual, mulai baru (dengan konfirmasi custom di dalam halaman).

## 12. Seni & Audio

- **Gaya visual:** kartun bersih, warna hangat dan cerah; bangunan isometric digambar sebagai sprite 2D dari sudut miring.
- **Tahap awal:** aset placeholder sederhana (bentuk geometris berwarna + label) supaya sistem bisa dites sebelum aset final ada.
- **Aset final:** digambar terpisah atau dari Unity Asset Store (diputuskan saat fase seni; perhatikan lisensi).
- **Audio:** musik latar santai + efek suara tap/bangun (disiapkan saat fase polish; tidak menghambat prototipe).

## 13. Teknologi

| Lapisan | Pilihan | Alasan |
|---|---|---|
| Engine | Unity 2022 LTS (2D, URP tidak wajib) | Standar industri game mobile; isometric 2,5D didukung penuh |
| Bahasa | C# | Bahasa utama Unity |
| Render dunia | Tilemap isometric Unity + sprite 2D | Cara baku membangun dunia isometric di Unity |
| UI | Unity UI (Canvas) | Menu, panel, dan HUD |
| Simpan game | JSON ke file lokal (Application.persistentDataPath) | Sederhana, aman, mudah di-backup; tanpa server di fase awal |
| Platform pertama | Android + iOS | Satu project Unity; build iOS memerlukan Mac dengan Xcode — pengembangan awal di Android, build iOS setelah ada akses Mac |
| Arsitektur kode | Manager pattern: GameManager, EconomyManager, CitizenManager, BuildManager, SaveManager, UIManager | Tiap manager satu tanggung jawab; satu file maksimal ±600 baris |

### 13.1 Struktur Project Unity (rencana)
```
Assets/
  Scripts/
    Managers/      (GameManager, EconomyManager, CitizenManager, BuildManager, SaveManager, UIManager)
    Data/          (BuildingData, CitizenData, definisi bangunan sebagai ScriptableObject)
    Grid/          (IsometricGrid, TileSelector, PathWalker)
    UI/            (panel, menu bangun, HUD)
    Utils/         (penyimpanan, format angka Rupiah)
  Sprites/         (placeholder dulu, aset final menyusul)
  Prefabs/         (prefab tiap bangunan)
  Scenes/          (MainScene)
  Audio/           (musik & SFX, fase polish)
```

### 13.2 Alur Kerja Pembangunan (yang tidak bisa dijalankan dari sandbox)
- Asisten menyiapkan seluruh project (script + struktur + scene) sebagai folder yang dikirim ke laptop user.
- User membuka project di Unity Editor di laptop, tekan Play untuk menguji.
- Build ke HP Android dilakukan dari laptop user (Unity Android Build Support + SDK).
- Perbaikan dilakukan berdasar hasil yang user lihat di layar/HP-nya.

## 14. Monetisasi (Rencana — Bukan Prioritas Awal)

Diputuskan SETELAH game bisa dimainkan dan seru. Opsi: iklan berhadiah (tonton iklan → dapat koin), pembelian permata, dan paket bangunan khusus. **Tidak ada monetisasi di prototipe.**

## 15. Tahapan Pengembangan

| Fase | Isi | Keluaran |
|---|---|---|
| 0 — Perancangan | Dokumen ini disetujui | GDD v1 final |
| 1 — Kerangka | Project Unity + grid isometric + kamera (pan/zoom) + bangun 3 bangunan (Rumah, Warung, Taman) + simpan/muat | Prototipe dapat dimainkan di Editor |
| 2 — Ekonomi & warga | Uang, pemasukan/pengeluaran harian, warga muncul & berjalan, kebahagiaan | Dunia mulai "hidup" |
| 3 — Progresi | Level dunia 1–3, unlock bangunan, misi tutorial, kejadian acak pertama | Game utuh skala kecil |
| 4 — Konten penuh | Semua bangunan, level 4–6, pencapaian, misi harian | Game lengkap (versi beta) |
| 4B — Interior penuh (tembus pandang) | Ketuk hunian → dinding/atap transparan di tempat, 4 ruangan, perabot dengan fungsi, pengaruh ke kebahagiaan & harga sewa | Interior dapat dilihat & diatur di semua hunian |
| 5 — Polish | Aset final (luar + interior), audio, tutorial halus, optimasi HP, uji di perangkat nyata | Siap uji publik terbatas |
| 6 — Rilis | Build Android final, halaman Play Store, rilis bertahap | Game rilis |
| 7 — Dunia Bersama (multiplayer) | **Satu peta besar dipakai banyak orang** (visi user 24 Nov 2026): tiap pemain bisa membangun rumah, usaha, dan apa saja di peta yang sama; dunia tersimpan di server, bukan di HP masing-masing; akun + kepemilikan lahan + sinkronisasi antar pemain. **Wali Kota** (diputuskan user 24 Nov 2026): tidak bisa bangun seenaknya — tanah harus DIBELI dulu lewat wali kota: ajukan beli petak → urus izin → bayar harga tanah + biaya izin → disetujui → tanah jadi milikmu, baru boleh bangun. Uang pembelian masuk kas kota (dana fasilitas umum). Tanpa tanah milik sendiri, tidak bisa membangun. **Wali kota adalah NPC dengan cara berpikir yang macam-macam** (diputuskan user 24 Nov 2026): keputusan izin TIDAK selalu sama, melainkan menimbang lokasi tanah, reputasi pemain, jenis bangunan yang direncanakan, kondisi kas kota, dan kepadatan kawasan — jadi ada penolakan, persetujuan bersyarat, dan harga yang berubah-ubah. | Dunia bersama yang hidup terus |

> Catatan Fase 7: dikerjakan SETELAH game single player (Fase 1–6) terbukti seru dan stabil. Multiplayer mengubah arsitektur secara mendasar (server + database + akun + sinkronisasi), jadi bukan tempelan di akhir — tapi jalurnya disiapkan sejak awal dengan menyimpan data dunia dalam format yang bisa dipindah ke server.

## 16. Risiko & Mitigasi

| Risiko | Mitigasi |
|---|---|
| Proyek terlalu besar untuk satu tim kecil | Dibangun per fase; tiap fase menghasilkan game yang bisa dimainkan, bukan menunggu "selesai semua" |
| Aset seni isometric memakan waktu | Placeholder dulu; aset final dari Asset Store atau digambar setelah sistem terbukti seru |
| Interior penuh menggandakan beban seni | Interior dikerjakan di fase sendiri (4B) setelah dunia luar terbukti jalan; aset interior dibuat per ruangan, bukan sekaligus semua bangunan |
| Performa di HP menengah ke bawah | Batasi jumlah warga beranimasi; pakai sprite, bukan model 3D; uji di HP nyata sejak Fase 1 |
| Scope merambat (fitur terus bertambah) | Fitur baru hanya masuk lewat dokumen ini (revisi GDD), bukan dari ide spontan di tengah fase |
| Unity & build Android butuh setup di laptop | Setup sekali di awal (Unity Hub + Android Build Support); asisten pandu langkahnya |

## 17. Keputusan User (24 Nov 2026 — sudah diputuskan)

- **Nama game: Dunia Kita** (final).
- **Platform: Android + iOS** (multi-platform; satu project Unity).
- **Arah final: TYCOON MULTIPEMAIN, satu peta bersama** (ditegaskan user 25 Nov 2026). Pemain = pemilik & pengelola usaha (warung, toko, kafe) dan sewa (kost, apartemen, kontrakan); warga NPC = pembeli & penyewa yang sama untuk semua pemain. Pemain TIDAK membangun rumah untuk warga. Sekolah, klinik, taman, balai kota, pos keamanan = milik Kota/NPC.
- **Single player dulu** di Fase 1–6 (sudah dikerjakan); sejak 25 Nov 2026 arah lanjutannya tycoon multipemain seperti di atas.
- **Dunia Bersama (diputuskan user 24 Nov 2026):** satu peta BESAR dipakai banyak orang; tiap pemain bisa membangun rumah, usaha, dan apa saja di peta yang sama; dunia tersimpan di server dan terus berjalan walau pemain offline. **Wali Kota** (diputuskan user 24 Nov 2026): supaya tidak seenaknya membangun — tanah harus dibeli lewat wali kota (ajukan → urus izin → bayar → disetujui → baru boleh bangun); uang masuk kas kota. **Wali kota tetap NPC** (diputuskan user 24 Nov 2026, BUKAN peran pemain), **tapi cara berpikirnya macam-macam**: keputusan izin menimbang lokasi tanah, reputasi pemain, jenis bangunan yang direncanakan, kondisi kas kota, dan kepadatan kawasan — bisa menyetujui, menolak, atau menyetujui dengan syarat/harga berbeda.
- **Wali kota HANYA di Fase 7 (multiplayer), TIDAK di Fase 2** (diputuskan user 24 Nov 2026: "nanti saja").
- **Bangun semua sistem dulu secara detail** (perintah user 24 Nov 2026): kerjakan sistem inti single player (Fase 2–4, 4B) sampai detail sebelum masuk multiplayer.
- **Orientasi game: LANDSCAPE** (diputuskan user 24 Nov 2026): semua UI (HUD, tombol, menu, panel) dirancang untuk layar mendatar 1920×1080; UI Fase 1 yang semula portrait (1080×1920) WAJIB dirombak ke landscape beserta tombol-tombol untuk user (Bangun, Lewati Hari, Simpan, Misi, Pengaturan, detail bangunan).
- **Hari berjalan OTOMATIS waktu nyata, tombol "Lewati Hari" DIHAPUS** (diputuskan user 24 Nov 2026): 1 hari game = 5 menit waktu nyata (GameBalance.DurasiHariDetik = 300); pergantian hari memicu ekonomi harian, cek naik level, kejadian acak, laporan harian, dan simpan otomatis.
- **Tombol "Simpan" DIHAPUS** (diputuskan user 24 Nov 2026 — game menuju online multiplayer): simpan OTOMATIS tiap pergantian hari + tiap 60 detik + saat aplikasi berhenti; di multiplayer nanti simpanan di server.
- **Tombol "Bangun" berupa IKON/LOGO mandiri tanpa tulisan** (diputuskan user 24 Nov 2026 — "ini game bukan saas"): sprite ikon palu + rumah, PNG transparan, gaya palet game.
- **Interior: PENUH, mode tembus pandang** (diputuskan & dikoreksi user 24 Nov 2026) — ketuk bangunan → dinding/atap jadi transparan di tempat, interior (4 ruangan + perabot) terlihat langsung di grid; BUKAN masuk sebagai karakter 3D dan BUKAN scene terpisah; interior memengaruhi kebahagiaan dan harga sewa; dikerjakan di Fase 4B setelah dunia luar jadi.
- Gaya seni final: kartun bersih (diusulkan; menunggu persetujuan).
