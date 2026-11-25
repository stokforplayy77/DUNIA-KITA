// Dunia Kita — SistemPeristiwaPenghuni
// Logika MURNI peristiwa penghuni (GDD §9.1), tanpa dependensi
// UnityEngine, supaya bisa diuji di luar Unity (tools/test-fase4b.cs).
//
// Aturan main (Fase 4B):
// - Tiap pergantian hari ada peluang 10% sebuah peristiwa muncul
//   (kira-kira 1 peristiwa tiap 10 hari), HANYA bila pemain punya
//   minimal 1 Kost (semua peristiwa berpusat pada penghuni kost).
// - Peristiwa dipilih acak merata dari 6 jenis.
// - Tiap peristiwa punya 2-3 pilihan; TIDAK ada pilihan yang selalu
//   benar — pemain menimbang uang vs reputasi vs kebahagiaan.
// - Efek "besok" (sewa hilang, kamar kosong, cicilan masuk,
//   bonus kebahagiaan) disimpan di KeadaanPeristiwa dan dipakai
//   game pada hari berikutnya, lalu dipakai sekali dan kembali normal.
// - "Sewa 1 bulan" di GDD dimodelkan sebagai sewa 1 hari Kost
//   (Rp40.000, GameBalance.SewaKostSehari) supaya terasa tapi tidak
//   menghancurkan ekonomi — lihat CATATAN-VERIFIKASI.md.
// - Peristiwa "merekomendasikan teman" di GDD hanya punya 1 pilihan
//   (Terima); Fase 4B menambah pilihan "Tolak dengan sopan"
//   (reputasi -2) supaya selalu ada pilihan.
// - Reputasi dibatasi 0-100 (BatasiReputasi); koin rugi yang
//   melebihi koin pemain dipotong sampai koin 0 (bukan utang).

public static class SistemPeristiwaPenghuni
{
    public const string TidakBayar = "tidak_bayar";
    public const string MelarikanDiri = "lari";
    public const string Berisik = "berisik";
    public const string RusakPerabot = "rusak";
    public const string Rekomendasi = "rekomendasi";
    public const string PindahKamar = "pindah";
    public const string TidakAda = "";

    // Keadaan efek "besok" yang disimpan game (dan di simpanan).
    public struct KeadaanPeristiwa
    {
        // Sewa Kost hari besok hilang (keringanan 1 bulan).
        public bool SewaKostHilangBesok;
        // Kapasitas hunian berkurang besok (kamar kosong sesudah
        // usir / lapor — sampai ada penghuni baru).
        public int KapasitasHilangBesok;
        // Koin yang masuk besok (cicilan pembayaran).
        public long TambahanKoinBesok;
        // Bonus kebahagiaan sementara besok (tegur tetangga,
        // terima teman, setujui pindah kamar, dsb.).
        public int BonusBahagiaBesok;
    }

    public static KeadaanPeristiwa KeadaanAwal()
    {
        KeadaanPeristiwa k = new KeadaanPeristiwa();
        k.SewaKostHilangBesok = false;
        k.KapasitasHilangBesok = 0;
        k.TambahanKoinBesok = 0;
        k.BonusBahagiaBesok = 0;
        return k;
    }

    // Dampak LANGSUNG satu pilihan (sebelum efek "besok" dipisah
    // ke KeadaanPeristiwa oleh GameManager).
    public struct DampakPeristiwa
    {
        public long Koin;             // Langsung (negatif = rugi)
        public int Reputasi;          // Delta reputasi
        public int BonusBahagiaBesok; // Untuk KeadaanPeristiwa
        public bool SewaKostHilangBesok;
        public int KapasitasHilangBesok;
        public long TambahanKoinBesok;
    }

    // Reputasi selalu 0..100.
    public static int BatasiReputasi(int nilai)
    {
        if (nilai < 0)
        {
            return 0;
        }
        if (nilai > GameBalance.ReputasiMaks)
        {
            return GameBalance.ReputasiMaks;
        }
        return nilai;
    }

    // Roll peristiwa untuk satu hari. acak01 = angka acak [0, 1).
    // < peluang -> peristiwa (dibagi rata 6 jenis), selebihnya tidak ada.
    public static string AcakPeristiwa(double acak01)
    {
        if (acak01 < 0.0 || acak01 >= GameBalance.PeluangPeristiwaPenghuniPerHari)
        {
            return TidakAda;
        }
        double bagian = acak01 / GameBalance.PeluangPeristiwaPenghuniPerHari;
        if (bagian < 1.0 / 6.0)
        {
            return TidakBayar;
        }
        if (bagian < 2.0 / 6.0)
        {
            return MelarikanDiri;
        }
        if (bagian < 3.0 / 6.0)
        {
            return Berisik;
        }
        if (bagian < 4.0 / 6.0)
        {
            return RusakPerabot;
        }
        if (bagian < 5.0 / 6.0)
        {
            return Rekomendasi;
        }
        return PindahKamar;
    }

    public static int JumlahPilihan(string id)
    {
        switch (id)
        {
            case TidakBayar: return 3;
            case MelarikanDiri: return 2;
            case Berisik: return 2;
            case RusakPerabot: return 2;
            case Rekomendasi: return 2;
            case PindahKamar: return 2;
            default: return 0;
        }
    }

    // ---- Teks tampilan (Bahasa Indonesia) ----
    public static string Judul(string id)
    {
        switch (id)
        {
            case TidakBayar: return "Anak Kost Tidak Bisa Bayar";
            case MelarikanDiri: return "Anak Kost Melarikan Diri";
            case Berisik: return "Penghuni Mengeluh Berisik";
            case RusakPerabot: return "Penghuni Merusak Perabot";
            case Rekomendasi: return "Penghuni Merekomendasikan Teman";
            case PindahKamar: return "Penghuni Minta Pindah Kamar";
            default: return "";
        }
    }

    public static string Deskripsi(string id)
    {
        switch (id)
        {
            case TidakBayar:
                return "Seorang anak kost menunggak uang kos bulan ini. "
                    + "Katanya gajinya telat turun. Apa yang kamu lakukan?";
            case MelarikanDiri:
                return "Seorang anak kost pergi diam-diam tanpa membayar "
                    + "sewa " + FormatUtil.Rupiah(GameBalance.SewaKostSehari)
                    + ". Kamarnya kosong. Apa yang kamu lakukan?";
            case Berisik:
                return "Seorang penghuni mengeluh tetangga sebelah terlalu "
                    + "berisik setiap malam. Ia mengancam akan pindah bila "
                    + "tidak ditangani.";
            case RusakPerabot:
                return "Seorang penghuni merusak perabot kamarnya — meja "
                    + "patah dan lampu pecah. Depositnya masih kamu pegang.";
            case Rekomendasi:
                return "Seorang penghuni merekomendasikan temannya untuk "
                    + "tinggal di kostmu. Katanya temannya rajin dan "
                    + "selalu bayar tepat waktu.";
            case PindahKamar:
                return "Seorang penghuni minta pindah kamar karena kamarnya "
                    + "terlalu panas di siang hari. Kamar kosong di lantai "
                    + "lain masih ada.";
            default: return "";
        }
    }

    // Label tombol pilihan (indeks 0, 1, 2). Pilihan ke-3 hanya ada
    // untuk TidakBayar; panel menyembunyikan tombol ke-3 bila tidak ada.
    public static string LabelPilihan(string id, int pilihan)
    {
        // Pilihan di luar jangkauan = tanpa label (panel tidak
        // menampilkan tombol itu).
        if (pilihan < 0 || pilihan >= JumlahPilihan(id))
        {
            return "";
        }
        switch (id)
        {
            case TidakBayar:
                if (pilihan == 0)
                {
                    return "Beri keringanan 1 bulan";
                }
                if (pilihan == 1)
                {
                    return "Tagih tegas / usir (+" + FormatUtil.Rupiah(GameBalance.KembaliTagihTegas) + ")";
                }
                return "Cicil pembayarannya";
            case MelarikanDiri:
                if (pilihan == 0)
                {
                    return "Biarkan saja";
                }
                return "Laporkan / blacklist (-" + FormatUtil.Rupiah(GameBalance.BiayaLaporPengurus) + ")";
            case Berisik:
                if (pilihan == 0)
                {
                    return "Tegur tetangga";
                }
                return "Abaikan";
            case RusakPerabot:
                if (pilihan == 0)
                {
                    return "Potong uang deposit (+" + FormatUtil.Rupiah(GameBalance.PotongDeposit) + ")";
                }
                return "Maafkan & perbaiki (-" + FormatUtil.Rupiah(GameBalance.BiayaPerbaikanPerabot) + ")";
            case Rekomendasi:
                if (pilihan == 0)
                {
                    return "Terima temannya";
                }
                return "Tolak dengan sopan";
            case PindahKamar:
                if (pilihan == 0)
                {
                    return "Setujui pindah";
                }
                return "Tolak";
            default: return "";
        }
    }

    // Dampak satu pilihan (GDD §9.1). Koin negatif = rugi langsung;
    // GameManager memotong sampai koin 0 (tidak jadi utang).
    public static DampakPeristiwa Dampak(string id, int pilihan)
    {
        DampakPeristiwa d = new DampakPeristiwa();
        // Pilihan di luar jangkauan = tanpa dampak (bukan pilihan
        // terakhir — guard ini mencegah else menangkap pilihan liar).
        if (pilihan < 0 || pilihan >= JumlahPilihan(id))
        {
            return d;
        }
        switch (id)
        {
            case TidakBayar:
                if (pilihan == 0)
                {
                    // Keringanan: reputasi naik, sewa besok hilang.
                    d.Reputasi = GameBalance.RepKeringanan;
                    d.SewaKostHilangBesok = true;
                }
                else if (pilihan == 1)
                {
                    // Tagih tegas / usir: sebagian sewa kembali,
                    // reputasi turun, kamar kosong 1 hari.
                    d.Koin = GameBalance.KembaliTagihTegas;
                    d.Reputasi = GameBalance.RepUsir;
                    d.KapasitasHilangBesok = GameBalance.KapasitasHilangKamarKosong;
                }
                else
                {
                    // Cicil: setengah sekarang, setengah besok.
                    d.Koin = GameBalance.CicilanSekarang;
                    d.TambahanKoinBesok = GameBalance.CicilanBesok;
                }
                break;
            case MelarikanDiri:
                if (pilihan == 0)
                {
                    // Biarkan: kehilangan sewa, reputasi turun sedikit.
                    d.Koin = -GameBalance.SewaKostSehari;
                    d.Reputasi = GameBalance.RepBiarkanLari;
                }
                else
                {
                    // Laporkan: biaya urus, reputasi naik (tegas),
                    // kamar kosong 1 hari.
                    d.Koin = -GameBalance.BiayaLaporPengurus;
                    d.Reputasi = GameBalance.RepLapor;
                    d.KapasitasHilangBesok = GameBalance.KapasitasHilangKamarKosong;
                }
                break;
            case Berisik:
                if (pilihan == 0)
                {
                    // Tegur: kebahagiaan naik, reputasi naik.
                    d.BonusBahagiaBesok = GameBalance.BonusTegurTetangga;
                    d.Reputasi = GameBalance.RepTegurTetangga;
                }
                else
                {
                    // Abaikan: kebahagiaan penghuni turun.
                    d.BonusBahagiaBesok = GameBalance.HukumanAbaikanBerisik;
                }
                break;
            case RusakPerabot:
                if (pilihan == 0)
                {
                    // Potong deposit: uang kembali sebagian,
                    // reputasi turun sedikit.
                    d.Koin = GameBalance.PotongDeposit;
                    d.Reputasi = GameBalance.RepPotongDeposit;
                }
                else
                {
                    // Maafkan & perbaiki: biaya perbaikan,
                    // reputasi naik.
                    d.Koin = -GameBalance.BiayaPerbaikanPerabot;
                    d.Reputasi = GameBalance.RepMaafkan;
                }
                break;
            case Rekomendasi:
                if (pilihan == 0)
                {
                    // Terima: kamar terisi lebih cepat, reputasi naik.
                    d.BonusBahagiaBesok = GameBalance.BonusTerimaTeman;
                    d.Reputasi = GameBalance.RepTerimaTeman;
                }
                else
                {
                    // Tolak (tambahan Fase 4B): reputasi turun sedikit.
                    d.Reputasi = GameBalance.RepTolakTeman;
                }
                break;
            case PindahKamar:
                if (pilihan == 0)
                {
                    // Setujui: kebahagiaan penghuni naik.
                    d.BonusBahagiaBesok = GameBalance.BonusSetujuPindah;
                }
                else
                {
                    // Tolak: kebahagiaan turun, reputasi turun sedikit.
                    d.BonusBahagiaBesok = GameBalance.HukumanTolakPindah;
                    d.Reputasi = GameBalance.RepTolakPindah;
                }
                break;
            default:
                break;
        }
        return d;
    }

    // Baris teks untuk laporan harian sesudah pilihan diambil.
    public static string TeksHasil(string id, int pilihan)
    {
        // Pilihan di luar jangkauan = tanpa teks hasil.
        if (pilihan < 0 || pilihan >= JumlahPilihan(id))
        {
            return "";
        }
        switch (id)
        {
            case TidakBayar:
                if (pilihan == 0)
                {
                    return "Keringanan diberikan. Reputasi +" + GameBalance.RepKeringanan
                        + ", sewa Kost besok hilang.";
                }
                if (pilihan == 1)
                {
                    return "Penghuni diusir. Sewa kembali sebagian (+"
                        + FormatUtil.Rupiah(GameBalance.KembaliTagihTegas)
                        + "), reputasi " + GameBalance.RepUsir + ", kamar kosong besok.";
                }
                return "Pembayaran dicicil: +" + FormatUtil.Rupiah(GameBalance.CicilanSekarang)
                    + " sekarang, +" + FormatUtil.Rupiah(GameBalance.CicilanBesok) + " besok.";
            case MelarikanDiri:
                if (pilihan == 0)
                {
                    return "Dibiarkan pergi. Kehilangan sewa "
                        + FormatUtil.Rupiah(GameBalance.SewaKostSehari)
                        + ", reputasi " + GameBalance.RepBiarkanLari + ".";
                }
                return "Dilaporkan ke pengurus (biaya "
                    + FormatUtil.Rupiah(GameBalance.BiayaLaporPengurus)
                    + "). Reputasi +" + GameBalance.RepLapor + ", kamar kosong besok.";
            case Berisik:
                if (pilihan == 0)
                {
                    return "Tetangga ditegur. Kebahagiaan +"
                        + GameBalance.BonusTegurTetangga + " besok, reputasi +"
                        + GameBalance.RepTegurTetangga + ".";
                }
                return "Keluhan diabaikan. Kebahagiaan "
                    + GameBalance.HukumanAbaikanBerisik + " besok.";
            case RusakPerabot:
                if (pilihan == 0)
                {
                    return "Deposit dipotong: +" + FormatUtil.Rupiah(GameBalance.PotongDeposit)
                        + ". Reputasi " + GameBalance.RepPotongDeposit + ".";
                }
                return "Dimaafkan, perabot diperbaiki sendiri (-"
                    + FormatUtil.Rupiah(GameBalance.BiayaPerbaikanPerabot)
                    + "). Reputasi +" + GameBalance.RepMaafkan + ".";
            case Rekomendasi:
                if (pilihan == 0)
                {
                    return "Teman penghuni diterima. Kamar terisi lebih cepat, reputasi +"
                        + GameBalance.RepTerimaTeman + ".";
                }
                return "Rekomendasi ditolak dengan sopan. Reputasi "
                    + GameBalance.RepTolakTeman + ".";
            case PindahKamar:
                if (pilihan == 0)
                {
                    return "Pindah kamar disetujui. Kebahagiaan +"
                        + GameBalance.BonusSetujuPindah + " besok.";
                }
                return "Pindah kamar ditolak. Kebahagiaan "
                    + GameBalance.HukumanTolakPindah + " besok, reputasi "
                    + GameBalance.RepTolakPindah + ".";
            default: return "";
        }
    }
}
