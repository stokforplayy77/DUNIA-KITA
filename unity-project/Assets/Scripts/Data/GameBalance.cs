// Dunia Kita — GameBalance
// Semua angka penyeimbang Fase 1 terpusat di sini supaya mudah diubah.
// CATATAN: Dokumen perancangan (GDD) tidak menetapkan angka modal awal
// maupun biaya bangunan — angka di bawah ini adalah keputusan Fase 1
// (asumsi awal yang seimbang untuk prototipe) dan boleh diubah di sini
// tanpa menyentuh logika game lainnya.

public static class GameBalance
{
    // Dunia
    public const int UkuranGrid = 12;          // Grid 12x12 petak (GDD §4)
    public const float LebarTile = 2f;         // Lebar satu petak isometric (unit dunia)
    public const float TinggiTile = 1f;        // Tinggi satu petak isometric (unit dunia)

    // Ekonomi awal
    public const long KoinAwal = 5000000;      // Modal awal pemain (Rp5.000.000)
    public const int PermataAwal = 0;
    public const int HariAwal = 1;
    public const int LevelDuniaAwal = 1;

    // Warga & kebahagiaan (Fase 2: warga berjalan, kebahagiaan dinamis)
    public const int KebahagiaanAwal = 50;
    public const int BonusKebahagiaanPerTaman = 2;
    public const int MaksBonusKebahagiaanTaman = 20; // +2/taman, maks +20
    public const int KebahagiaanMaks = 100;
    public const int BonusBahagiaSemuaPunyaRumah = 5; // Semua warga punya rumah
    public const int HukumanBahagiaMenunggak = 5;     // Upkeep menunggak 2 hari berturut-turut
    public const int HukumanBahagiaTunawisma = 3;     // Warga melebihi kapasitas hunian

    // Warga berjalan (Fase 2): tampil di layar dibatasi demi performa HP.
    public const int MaksWargaTampil = 12;
    public const float KecepatanJalanWarga = 1.2f;     // Unit dunia per detik
    public const float IstirahatWargaMinDetik = 1f;
    public const float IstirahatWargaMaksDetik = 3f;
    public const float AmplitudoLangkahWarga = 0.05f; // Naik-turun saat berjalan
    // Frame jalan warga (kartun proper): sprite A/B bergantian tiap
    // 0,3 detik selama berjalan; sprite diam saat istirahat.
    public const float IntervalFrameJalanWarga = 0.3f;

    // Ekonomi harian Fase 2: pajak kota + efek kebahagiaan ke warung.
    // Pajak = 2,5% dari pemasukan kotor (per-mil 25 supaya hitungan bulat).
    public const int PajakKotaPerMil = 25;
    public const int AmbangBahagiaRendah = 40;   // Di bawah ini warung -20%
    public const int PengaliWarungRendah = 80;   // Persen pemasukan warung
    public const int AmbangBahagiaTinggi = 70;   // Di atas ini warung +10%
    public const int PengaliWarungTinggi = 110;  // Persen pemasukan warung

    // Biaya perawatan bangunan hidup di file .asset (BuildingData) —
    // Fase 1: Rumah Rp1.000, Warung Rp2.000, Taman Rp500 per hari.
    // Sengaja TIDAK diubah di Fase 2 supaya simulasi 10 hari tetap sehat
    // (bersih +Rp10.125/hari dengan 2 rumah + 1 warung + 1 taman).

    // Kamera
    public const float ZoomMin = 3f;           // Orthographic size minimum
    public const float ZoomMaks = 22f;         // Orthographic size maksimum (muat peta 40x40)
    public const float UkuranKameraAwal = 8f;

    // Simpan game: nama file (versi ada di blok Fase 3 di bawah).
    public const string NamaFileSimpan = "save-dunia-kita.json";

    // Jual bangunan: kembali 50% dari biaya bangun
    public const float RasioJualKembali = 0.5f;

    // Misi tutorial Fase 1 (PanelMisi): hadiah koin sekali klaim per misi.
    // Kecil dibanding modal awal Rp5.000.000 — sekadar pemanis, bukan
    // jalan pintas ekonomi.
    public const long HadiahMisiBangunRumah = 25000;
    public const long HadiahMisiBangunWarung = 50000;
    public const long HadiahMisiCapai4Warga = 50000;

    // Misi tutorial Fase 2 (PanelMisi).
    public const long HadiahMisiLewati3Hari = 25000;
    public const long HadiahMisiBahagia60 = 50000;

    // Waktu nyata (Fase 3 revisi desain 24 Nov 2026): hari game
    // bergulir OTOMATIS, 1 hari game = 5 menit waktu nyata.
    // TIDAK ada lagi tombol "Lewati Hari" — GameManager memakai timer
    // ini dan memproses pergantian hari sendiri. Durasi bisa diubah
    // di sini (mis. 60 untuk uji cepat di Editor — JANGAN lupa
    // kembalikan ke 300 sebelum build).
    public const float DurasiHariDetik = 300f;
    // Simpan otomatis tiap 60 detik + tiap pergantian hari +
    // saat aplikasi berhenti. TIDAK ada lagi tombol "Simpan".
    public const float IntervalSimpanOtomatisDetik = 60f;

    // Misi tutorial Fase 3 (PanelMisi).
    public const long HadiahMisiNaikLevel2 = 50000;
    public const long HadiahMisiBangunKost = 75000;
    public const long HadiahMisiCapai10Warga = 50000;

    // Progresi dunia Fase 3 (GDD §8: syarat naik level + hadiah).
    // Hadiah naik level TIDAK ditetapkan GDD — keputusan Fase 3:
    // Rp100.000 (Level 2) dan Rp250.000 (Level 3), seimbang dengan
    // pemasukan harian dunia standar Fase 2 (±Rp10.000/hari) dan
    // biaya Kost (Rp2.000.000) yang terbuka di Level 2.
    // FASE 4: batas level naik dari 3 menjadi 6 (LevelMaksDunia);
    // hadiah L4-L6 juga keputusan Fase 4 (naik mengikuti pemasukan
    // dunia kaya Fase 4, ±Rp200.000+/hari, dan biaya bangunan
    // besar seperti Perumahan Rp8.000.000 / Kampus Rp10.000.000).
    public const int LevelMaksDunia = 6;
    public const long HadiahNaikLevel2 = 100000;
    public const long HadiahNaikLevel3 = 250000;
    public const long HadiahNaikLevel4 = 500000;
    public const long HadiahNaikLevel5 = 1000000;
    public const long HadiahNaikLevel6 = 2000000;

    // Perluasan lahan Fase 4 (GDD §8, Level 5): sekali beli, grid
    // 12x12 -> 16x16. Harga = biaya 1 Apartemen (Rp5.000.000) —
    // mahal tapi sekali saja; tanpa perluasan, 300 warga Level 6
    // hampir tidak muat di 144 petak (lihat CATATAN-VERIFIKASI.md).
    public const long HargaPerluasanLahan = 5000000;
    public const int UkuranGridDiperluas = 16;

    // Aturan Jalan Fase 4 (GDD §5.5): bangunan yang TIDAK bersebelahan
    // dengan petak Jalan berfungsi setengah (pemasukan & bonus
    // kebahagiaan x 50%). Kapasitas hunian TIDAK terpengaruh (warga
    // tetap tinggal; hanya pemasukan/efek yang turun).
    public const int PengaliTanpaJalan = 50;

    // Hadiah Permata pencapaian Fase 4 (GDD §8; GDD tidak menetapkan
    // angka — keputusan Fase 4: 2-5 Permata untuk pencapaian biasa,
    // 10 untuk Metropolis supaya terasa istimewa).
    public const int PermataJuraganKost = 3;
    public const int PermataKotaPelajar = 3;
    public const int PermataKotaHijau = 2;
    public const int PermataMetropolis = 10;
    public const int PermataJuraganPasar = 3;
    public const int PermataKotaSejahtera = 5;

    // Hadiah misi harian Fase 4 (GDD §9; GDD tidak menetapkan angka —
    // keputusan Fase 4: Rp25.000-40.000 koin, 1 Permata untuk yang
    // paling sulit; kecil supaya tidak merusak ekonomi).
    public const long HadiahHarianBangun = 25000;
    public const long HadiahHarianTaman = 25000;
    public const long HadiahHarianBahagia65 = 30000;
    public const long HadiahHarianPemasukan = 30000;
    public const long HadiahHarianWarga15 = 30000;
    public const long HadiahHarianKomersial3 = 40000;
    public const int HadiahHarianBahagia75Permata = 1;
    public const long HadiahHarianTaman5 = 40000;

    // Kejadian acak Fase 3 (GDD §9): peluang muncul tiap pergantian
    // hari otomatis.
    // 20% = kira-kira 1 kejadian tiap 5 hari; tiap kejadian punya
    // 2 pilihan dan TIDAK ada pilihan yang selalu benar.
    public const double PeluangKejadianPerHari = 0.20;
    // Festival Desa: bayar sukarela, kebahagiaan +10 untuk besok.
    public const long BiayaFestival = 50000;
    public const int BonusBahagiaFestival = 10;
    // Hujan Deras: pemasukan warung besok turun; siapkan payung
    // (bayar) untuk mengurangi dampaknya.
    public const long BiayaPayungHujan = 20000;
    public const int PengaliHujanTanpaPayung = 80;  // -20%
    public const int PengaliHujanDenganPayung = 90; // -10%
    // Pedagang Keliling: beli dagangannya (koin) untuk 1 Permata.
    public const long HargaPedagangKeliling = 100000;
    public const int PermataPedagangKeliling = 1;

    // Simpan game (versi "6.0" — Fase 7 Dunia Bersama; simpanan
    // "5.1", "5.0", "4.0", "3.0", "2.0" dan "1.0" tetap dimuat;
    // lihat SaveData.VersiDidukung).
    public const string VersiSimpan = "6.0";
    public const string VersiSimpanFase5_1 = "5.1";
    public const string VersiSimpanFase4B = "5.0";
    public const string VersiSimpanFase4 = "4.0";
    public const string VersiSimpanFase3 = "3.0";
    public const string VersiSimpanFase2 = "2.0";
    public const string VersiSimpanLama = "1.0";

    // Versi tampil di Panel Pengaturan.
    public const string VersiGame = "1.0";

    // ---- Interior Fase 4B (GDD §5.6) ----
    // Reputasi pemain (dipakai peristiwa penghuni §9.1 dan Wali
    // Kota Fase 7 nanti): mulai 50, dibatasi 0-100.
    public const int ReputasiAwal = 50;
    public const int ReputasiMaks = 100;
    // Total bonus kebahagiaan dari interior se-dunia dibatasi
    // -20..+20 (seperti cap taman +20) supaya furnitur mahal
    // tidak membuat kebahagiaan langsung 100 dari rumah pertama.
    public const int BatasBonusInterior = 20;
    // Hunian tanpa perabot sama sekali: penghuni tidak betah,
    // kebahagiaan dunia -5 per hunian kosong (GDD §5.6).
    public const int HukumanRumahKosong = 5;
    // Kost yang semua 10 slot perabotnya terisi: sewa naik +25%
    // (pengali 125) — GDD §5.6: kamar berperabot lengkap bisa
    // dipasang harga sewa lebih tinggi.
    public const int PengaliSewaKostLengkap = 125;

    // Harga perabot (koin) — keputusan Fase 4B (GDD tidak
    // menetapkan angka): total melengkapi 1 hunian Rp1.420.000,
    // seimbang dengan modal awal Rp5.000.000 dan sewa Kost
    // Rp40.000/hari.
    public const long HargaKasur = 150000;
    public const long HargaLemari = 100000;
    public const long HargaKompor = 200000;
    public const long HargaMeja = 80000;
    public const long HargaKursi = 50000;
    public const long HargaBakMandi = 150000;
    public const long HargaWastafel = 80000;
    public const long HargaTV = 300000;
    public const long HargaSofa = 250000;
    public const long HargaLampu = 60000;
    // Bonus kebahagiaan tiap perabot (GDD §5.6: kasur/TV/kompor
    // menaikkan kebahagiaan; angka keputusan Fase 4B).
    public const int BonusKasur = 2;
    public const int BonusLemari = 1;
    public const int BonusKompor = 2;
    public const int BonusMeja = 1;
    public const int BonusKursi = 1;
    public const int BonusBakMandi = 2;
    public const int BonusWastafel = 1;
    public const int BonusTV = 3;
    public const int BonusSofa = 2;
    public const int BonusLampu = 1;

    // ---- Balai Kota (fasilitas + sumber reputasi pasif) ----
    // Gedung 2x2, terbuka Level 3 (Desa). Harga/perawatan di sini
    // sebagai angka acuan; nilai yang dipakai game ada di
    // BalaiKota.asset (biayaBangun/perawatan) dan bonusBahagia (+5).
    // Reputasi +2/hari adalah sumber reputasi pasif pertama (sebelum
    // ini reputasi hanya berubah lewat peristiwa penghuni §9.1).
    // Wali Kota sebagai NPC yang mengurus izin tanah tetap di Fase 7
    // (multiplayer) — gedungnya sudah ada, orangnya menyusul.
    public const int LevelBukaBalaiKota = 3;
    public const long HargaBalaiKota = 3000000;
    public const long PerawatanBalaiKota = 5000;
    public const int BonusBahagiaBalaiKota = 5;
    public const int BonusReputasiBalaiKotaPerHari = 2;
    public const int MaksBalaiKotaPerDunia = 1;

    // Misi tutorial Fase 4B (PanelMisi).
    public const long HadiahMisiBeliPerabot = 25000;
    public const long HadiahMisiLengkapiKost = 75000;

    // ---- Audio & kinerja Fase 5 (GDD §12) ----
    // Volume musik latar (0-1) — terpisah dari SFX supaya musik
    // tidak menenggelamkan efek suara; saklar utama tetap
    // AudioListener.volume dari toggle Suara (PanelPengaturan).
    public const float VolumeMusik = 0.35f;
    // Target frame rate HP (Fase 5: vSync mati + target 60 fps,
    // diterapkan GameManager.Awake).
    public const int TargetFrameRate = 60;

    // ---- Peristiwa penghuni Fase 4B (GDD §9.1) ----
    // Peluang muncul tiap pergantian hari (hanya bila pemain punya
    // minimal 1 Kost): 10% = kira-kira 1 peristiwa tiap 10 hari.
    public const double PeluangPeristiwaPenghuniPerHari = 0.10;
    // Sewa Kost 1 hari (Rp40.000, sama dengan Kost.asset) — dipakai
    // sebagai satuan "kehilangan sewa" peristiwa (GDD menyebut
    // "sewa 1 bulan", dimodelkan 1 hari sewa supaya terasa tapi
    // tidak menghancurkan ekonomi; lihat CATATAN-VERIFIKASI.md).
    public const long SewaKostSehari = 40000;
    // Anak kost tidak bisa bayar: tagih tegas -> kembali setengah
    // sewa; cicil -> setengah sekarang + setengah besok.
    public const long KembaliTagihTegas = 20000;
    public const long CicilanSekarang = 10000;
    public const long CicilanBesok = 10000;
    // Anak kost melarikan diri: biaya urus laporan/blacklist.
    public const long BiayaLaporPengurus = 15000;
    // Penghuni merusak perabot: potong deposit / biaya perbaikan.
    public const long PotongDeposit = 25000;
    public const long BiayaPerbaikanPerabot = 30000;
    // Kamar kosong sesudah usir/lapor: kapasitas hilang 2 warga
    // untuk 1 hari (sampai ada penghuni baru).
    public const int KapasitasHilangKamarKosong = 2;
    // Delta reputasi peristiwa (GDD §9.1; angka keputusan Fase 4B).
    public const int RepKeringanan = 5;
    public const int RepUsir = -5;
    public const int RepLapor = 5;
    public const int RepBiarkanLari = -3;
    public const int RepTegurTetangga = 3;
    public const int RepPotongDeposit = -3;
    public const int RepMaafkan = 5;
    public const int RepTerimaTeman = 5;
    public const int RepTolakTeman = -2;
    public const int RepTolakPindah = -2;
    // Delta kebahagiaan "besok" peristiwa (lewat BonusSementara).
    public const int BonusTegurTetangga = 3;
    public const int HukumanAbaikanBerisik = -5;
    public const int BonusTerimaTeman = 2;
    public const int BonusSetujuPindah = 3;
    public const int HukumanTolakPindah = -3;

    // ---- Dunia Bersama Fase 7 (GDD §15, Fase 7) ----
    // Peta bersama diperluas: 40x40 petak (1.600 petak) supaya banyak
    // pemain leluasa membangun di satu dunia yang sama.
    public const int UkuranGridDuniaBersama = 40;
    // Pusat peta bersama (petak 19,19) — Balai Kota, RS/Klinik, & Pos Polisi
    // berdiri di sini sejak awal.
    public const int PusatPetaBersama = 19;
    // Harga tanah per petak: pinggir Rp200.000, tiap langkah
    // Chebyshev mendekati pusat +Rp60.000 (pusat Rp1.340.000).
    public const long HargaTanahPinggir = 200000;
    public const long HargaTanahPerLangkahPusat = 60000;
    public const long BiayaSerobotTanah = 500000;
    // Biaya izin = 10% harga tanah (per-mil 100), diubah oleh
    // reputasi, kas kota, dan Balai Kota (SistemWaliKota.BiayaIzin).
    public const int BiayaIzinPerMil = 100;
    // Biaya izin paling murah (supaya izin tanah pinggir tetap
    // terasa ada harganya, bukan gratis).
    public const long BiayaIzinMinimum = 10000;
    // Reputasi: >= 70 -> biaya izin -30%; < 30 -> biaya izin +50%;
    // < 15 -> izin DITOLAK (wali kota tidak percaya padamu).
    public const int ReputasiDiskonIzin = 70;
    public const int DiskonIzinReputasiBagusPerMil = 300;
    public const int ReputasiIzinMahal = 30;
    public const int KenaikanIzinReputasiBurukPerMil = 500;
    public const int ReputasiTolakIzin = 15;
    // Kas kota: < Rp1.000.000 (menipis) -> biaya izin +50%;
    // > Rp20.000.000 (penuh) -> biaya izin -20%.
    public const long KasKotaMenipis = 1000000;
    public const int KenaikanIzinKasMenipisPerMil = 500;
    public const long KasKotaPenuh = 20000000;
    public const int DiskonIzinKasPenuhPerMil = 200;
    // Balai Kota di dunia (gedungnya sudah ada sejak tugas
    // sebelumnya): biaya izin -10% — wali kota bekerja dari
    // kantornya sendiri, urusan lebih lancar.
    public const int DiskonIzinBalaiKotaPerMil = 100;
    // Persetujuan bersyarat: biaya izin x1,5 (per-mil 1500).
    public const int PengaliIzinBersyaratPerMil = 1500;
    // Kepadatan kawasan: 4+ bangunan sejenis dalam area 5x5
    // (jari-jari 2 petak) -> izin DITOLAK ("kawasan sudah padat").
    public const int KepadatanMaksKawasan = 4;
    public const int JariJariKawasan = 2;
    // Skor keputusan wali kota: mulai 50; disetujui bila >= 40,
    // bersyarat bila 25-39, ditolak bila < 25 (ditambah penolakan
    // otomatis reputasi < 15 dan kawasan padat di atas).
    public const int SkorIzinAwal = 50;
    public const int SkorIzinDisetujui = 40;
    public const int SkorIzinBersyarat = 25;
    // Variasi deterministik skor (-5..+5) dari id pemain + petak —
    // supaya keputusan wali kota terasa "macam-macam" tapi stabil
    // (input sama -> keputusan sama, GDD §17).
    public const int VariasiSkorIzin = 5;
    // Sinkronisasi dunia bersama: tarik state tiap 10 detik +
    // sesudah tiap aksi (cukup untuk city builder; bukan game
    // real-time di bawah 1 detik).
    public const float IntervalSinkronisasiDetik = 10f;
    // Ekonomi saat offline (dihitung server saat masuk lagi):
    // pemasukan bangunan selama pemain pergi, maks 24 hari game
    // (= 2 jam waktu nyata; 1 hari game = 5 menit).
    public const int MaksHariOffline = 24;
    // Server Dunia Bersama (Python stdlib, ~/workspace/dunia-kita/
    // server/): alamat default localhost — di HP ganti dengan IP
    // LAN / alamat VPS server lewat kolom "Alamat server" di
    // panel Akun (disimpan di PlayerPrefs "DuniaKita_Server").
    public const int PortServerDefault = 8471;
    public const string AlamatServerDefault = "http://127.0.0.1:8471";
    // Batas laju server: maks 60 request per menit per IP
    // (server/db.py — angka yang sama harus dipakai di sana).
    public const int BatasLajuPerMenit = 60;
}
