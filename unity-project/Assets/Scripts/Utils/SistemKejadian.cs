// Dunia Kita — SistemKejadian
// Logika MURNI kejadian acak (GDD §9), tanpa dependensi UnityEngine,
// supaya bisa diuji di luar Unity (tools/test-fase3.cs).
//
// Aturan main (Fase 3):
// - Tiap kali hari berganti otomatis, ada peluang 20% sebuah
//   kejadian muncul (kira-kira 1 kejadian tiap 5 hari).
// - Kejadian yang muncul dipilih acak merata dari 3 jenis.
// - Tiap kejadian punya 2 pilihan; TIDAK ada pilihan yang selalu
//   benar — pemain menimbang uang vs kebahagiaan vs permata.
// - Efek "besok" (bonus kebahagiaan, pengali hujan) disimpan di
//   KeadaanKejadian dan dipakai game pada hari berikutnya, lalu
//   dipakai sekali dan kembali normal.
//
// BUKAN bagian Fase 3: peristiwa penghuni GDD §9.1 (tidak bisa bayar
// kos, melarikan diri, dsb.) — itu rencana Fase 4.

public static class SistemKejadian
{
    public const string Festival = "festival";
    public const string Hujan = "hujan";
    public const string Pedagang = "pedagang";
    public const string TidakAda = "";

    // Keadaan efek "besok" yang disimpan game (dan di simpanan).
    public struct KeadaanKejadian
    {
        // Bonus kebahagiaan sementara dari festival (0 bila tidak ada).
        public int BonusBahagiaBesok;
        // Pengali pemasukan warung besok dalam persen (100 = normal,
        // 80 = hujan tanpa payung, 90 = hujan dengan payung).
        public int PengaliHujanBesok;
    }

    public static KeadaanKejadian KeadaanAwal()
    {
        KeadaanKejadian k = new KeadaanKejadian();
        k.BonusBahagiaBesok = 0;
        k.PengaliHujanBesok = 100;
        return k;
    }

    // Roll kejadian untuk satu hari. acak01 = angka acak [0, 1).
    // < peluang -> kejadian (dibagi rata 3 jenis), selebihnya tidak ada.
    public static string AcakKejadian(double acak01)
    {
        if (acak01 < 0.0 || acak01 >= GameBalance.PeluangKejadianPerHari)
        {
            return TidakAda;
        }
        double bagian = acak01 / GameBalance.PeluangKejadianPerHari;
        if (bagian < 1.0 / 3.0)
        {
            return Festival;
        }
        if (bagian < 2.0 / 3.0)
        {
            return Hujan;
        }
        return Pedagang;
    }

    // ---- Teks tampilan (Bahasa Indonesia) ----
    public static string Judul(string id)
    {
        switch (id)
        {
            case Festival: return "Festival Desa";
            case Hujan: return "Hujan Deras";
            case Pedagang: return "Pedagang Keliling";
            default: return "";
        }
    }

    public static string Deskripsi(string id)
    {
        switch (id)
        {
            case Festival:
                return "Warga mengusulkan festival desa besok! Musik, makanan, "
                    + "dan lampu warna-warni. Biayanya "
                    + FormatUtil.Rupiah(GameBalance.BiayaFestival)
                    + " dari kasmu, tapi warga akan senang (kebahagiaan +"
                    + GameBalance.BonusBahagiaFestival + " besok).";
            case Hujan:
                return "Langit mendung — hujan deras diperkirakan turun semalaman. "
                    + "Pemasukan warung besok turun 20% bila tidak disiapkan. "
                    + "Siapkan payung dan terpal seharga "
                    + FormatUtil.Rupiah(GameBalance.BiayaPayungHujan)
                    + " supaya turunnya hanya 10%?";
            case Pedagang:
                return "Seorang pedagang keliling menawarkan barang langka seharga "
                    + FormatUtil.Rupiah(GameBalance.HargaPedagangKeliling)
                    + ". Katanya barang ini bisa ditukar dengan 1 Permata. Beli?";
            default: return "";
        }
    }

    // Label tombol pilihan 1 (kiri) dan pilihan 2 (kanan).
    public static string LabelPilihan1(string id)
    {
        switch (id)
        {
            case Festival:
                return "Adakan (" + FormatUtil.Rupiah(GameBalance.BiayaFestival) + ")";
            case Hujan:
                return "Siapkan (" + FormatUtil.Rupiah(GameBalance.BiayaPayungHujan) + ")";
            case Pedagang:
                return "Beli (" + FormatUtil.Rupiah(GameBalance.HargaPedagangKeliling) + ")";
            default: return "";
        }
    }

    public static string LabelPilihan2(string id)
    {
        switch (id)
        {
            case Festival: return "Tidak dulu";
            case Hujan: return "Biarkan saja";
            case Pedagang: return "Tolak";
            default: return "";
        }
    }

    // Biaya koin pilihan (0 = gratis). Pilihan 1 selalu yang berbayar.
    public static long Biaya(string id, int pilihan)
    {
        if (pilihan != 0)
        {
            return 0;
        }
        switch (id)
        {
            case Festival: return GameBalance.BiayaFestival;
            case Hujan: return GameBalance.BiayaPayungHujan;
            case Pedagang: return GameBalance.HargaPedagangKeliling;
            default: return 0;
        }
    }

    // Terapkan efek pilihan ke keadaan "besok".
    // Pilihan 0 = pilihan berbayar (festival diadakan / payung disiapkan /
    // dagangan dibeli), pilihan 1 = tolak/biarkan. Efek permata pedagang
    // TIDAK di sini (ditangani GameManager — butuh akses state game).
    public static void TerapkanPilihan(string id, int pilihan,
        ref KeadaanKejadian keadaan)
    {
        switch (id)
        {
            case Festival:
                if (pilihan == 0)
                {
                    keadaan.BonusBahagiaBesok = GameBalance.BonusBahagiaFestival;
                }
                break;
            case Hujan:
                if (pilihan == 0)
                {
                    keadaan.PengaliHujanBesok = GameBalance.PengaliHujanDenganPayung;
                }
                else
                {
                    keadaan.PengaliHujanBesok = GameBalance.PengaliHujanTanpaPayung;
                }
                break;
            default:
                break;
        }
    }

    // Baris teks untuk laporan harian sesudah pilihan diambil.
    public static string TeksHasil(string id, int pilihan, bool berhasilBayar)
    {
        if (!berhasilBayar)
        {
            return "Koin tidak cukup — rencana batal.";
        }
        switch (id)
        {
            case Festival:
                return pilihan == 0
                    ? "Festival desa digelar! Kebahagiaan +"
                        + GameBalance.BonusBahagiaFestival + " besok."
                    : "Festival ditolak. Warga biasa saja.";
            case Hujan:
                return pilihan == 0
                    ? "Payung dan terpal disiapkan. Pemasukan warung besok hanya -10%."
                    : "Hujan deras semalaman. Pemasukan warung besok -20%.";
            case Pedagang:
                return pilihan == 0
                    ? "Membeli dagangan keliling: +"
                        + GameBalance.PermataPedagangKeliling + " Permata."
                    : "Pedagang keliling berlalu tanpa transaksi.";
            default: return "";
        }
    }
}
