// Dunia Kita — SistemMisiHarian
// Logika MURNI misi harian (GDD §9), tanpa dependensi UnityEngine,
// supaya bisa diuji di luar Unity (tools/test-fase4.cs).
//
// FASE 4: 8 misi kecil dalam satu kolam; tiap hari game dipilih
// 3 misi secara DETERMINISTIK dari nomor hari (bukan acak sungguhan)
// supaya misi hari yang sama selalu sama — pemain yang menutup lalu
// membuka game di hari yang sama tetap melihat misi yang sama.
// Misi yang tidak selesai HANGUS saat hari berganti (set baru
// dipilih untuk hari berikutnya). Hadiah koin/permata kecil di
// GameBalance; klaim dicatat di PlayerPrefs dengan kunci yang
// memuat nomor hari ("DK_Harian_<hari>_<id>") supaya misi yang
// sama bisa diklaim lagi di hari lain, tapi tidak dua kali di
// hari yang sama.

public static class SistemMisiHarian
{
    public const string Bangun1 = "harian_bangun1";
    public const string Taman1 = "harian_taman1";
    public const string Bahagia65 = "harian_bahagia65";
    public const string Pemasukan50rb = "harian_pemasukan50rb";
    public const string Warga15 = "harian_warga15";
    public const string Komersial3 = "harian_komersial3";
    public const string Bahagia75 = "harian_bahagia75";
    public const string Taman5 = "harian_taman5";

    // Keadaan hari ini yang dibutuhkan untuk menilai misi harian.
    // Catatan: "pemasukan hari ini" dinilai dari PERKIRAAN pemasukan
    // harian (EconomyManager.PerkiraanHarian) — bukan pemasukan yang
    // sudah dibayar — supaya misi bisa selesai kapan saja di hari
    // itu, bukan hanya di detik pergantian hari (saat misinya sudah
    // berganti set baru).
    public struct KeadaanHarian
    {
        public int DibangunHariIni;   // Bangunan dibangun hari ini
        public int TamanHariIni;      // Taman dibangun hari ini
        public int Kebahagiaan;       // Kebahagiaan saat ini
        public long PerkiraanPemasukanHarian; // Perkiraan pemasukan kotor/hari
        public int Populasi;          // Jumlah warga saat ini
        public int JumlahKomersial;   // Warung + Toko + Pasar + Kafe
        public int JumlahTaman;       // Total taman di dunia
    }

    // Semua id misi harian, sesuai urutan kolam (dipakai rotasi).
    public static string[] SemuaId()
    {
        return new string[]
        {
            Bangun1, Taman1, Bahagia65, Pemasukan50rb,
            Warga15, Komersial3, Bahagia75, Taman5
        };
    }

    // Pilih 3 misi untuk satu hari game (hari >= 1). Deterministik:
    // mulai dari ((hari - 1) * 3) % 8, ambil 3 berurutan (melilit).
    public static string[] PilihUntukHari(int hari)
    {
        string[] kolam = SemuaId();
        int jumlah = kolam.Length;
        int awal = ((hari - 1) * 3) % jumlah;
        if (awal < 0)
        {
            awal = 0;
        }
        return new string[]
        {
            kolam[awal],
            kolam[(awal + 1) % jumlah],
            kolam[(awal + 2) % jumlah]
        };
    }

    public static string Judul(string id)
    {
        switch (id)
        {
            case Bangun1: return "Bangun 1 Bangunan";
            case Taman1: return "Bangun 1 Taman";
            case Bahagia65: return "Capai kebahagiaan 65%";
            case Pemasukan50rb: return "Raih pemasukan harian Rp50.000";
            case Warga15: return "Punya 15 warga";
            case Komersial3: return "Punya 3 bangunan komersial";
            case Bahagia75: return "Capai kebahagiaan 75%";
            case Taman5: return "Punya 5 Taman";
            default: return "";
        }
    }

    public static string Deskripsi(string id)
    {
        switch (id)
        {
            case Bangun1: return "Bangun bangunan apa saja hari ini.";
            case Taman1: return "Bangun 1 Taman hari ini.";
            case Bahagia65: return "Jaga kebahagiaan warga sampai 65%.";
            case Pemasukan50rb: return "Total pemasukan bangunanmu mencapai Rp50.000 per hari.";
            case Warga15: return "Duniamu dihuni 15 warga.";
            case Komersial3: return "Punya 3 bangunan komersial (Warung, Toko, Pasar, atau Kafe).";
            case Bahagia75: return "Jaga kebahagiaan warga sampai 75%.";
            case Taman5: return "Punya 5 Taman di duniamu.";
            default: return "";
        }
    }

    // Hadiah koin (0 bila hadiahnya Permata).
    public static long HadiahKoin(string id)
    {
        switch (id)
        {
            case Bangun1: return GameBalance.HadiahHarianBangun;
            case Taman1: return GameBalance.HadiahHarianTaman;
            case Bahagia65: return GameBalance.HadiahHarianBahagia65;
            case Pemasukan50rb: return GameBalance.HadiahHarianPemasukan;
            case Warga15: return GameBalance.HadiahHarianWarga15;
            case Komersial3: return GameBalance.HadiahHarianKomersial3;
            case Bahagia75: return 0;
            case Taman5: return GameBalance.HadiahHarianTaman5;
            default: return 0;
        }
    }

    // Hadiah Permata (0 bila hadiahnya koin).
    public static int HadiahPermata(string id)
    {
        switch (id)
        {
            case Bahagia75: return GameBalance.HadiahHarianBahagia75Permata;
            default: return 0;
        }
    }

    // Teks hadiah untuk tombol klaim ("Rp25.000" atau "1 Permata").
    public static string TeksHadiah(string id)
    {
        int permata = HadiahPermata(id);
        if (permata > 0)
        {
            return permata + " Permata";
        }
        return FormatUtil.Rupiah(HadiahKoin(id));
    }

    // Apakah misi ini selesai dengan keadaan hari ini?
    public static bool ApakahSelesai(string id, KeadaanHarian keadaan)
    {
        switch (id)
        {
            case Bangun1: return keadaan.DibangunHariIni >= 1;
            case Taman1: return keadaan.TamanHariIni >= 1;
            case Bahagia65: return keadaan.Kebahagiaan >= 65;
            case Pemasukan50rb: return keadaan.PerkiraanPemasukanHarian >= 50000;
            case Warga15: return keadaan.Populasi >= 15;
            case Komersial3: return keadaan.JumlahKomersial >= 3;
            case Bahagia75: return keadaan.Kebahagiaan >= 75;
            case Taman5: return keadaan.JumlahTaman >= 5;
            default: return false;
        }
    }

    // Kunci PlayerPrefs untuk klaim misi harian (memuat nomor hari
    // supaya misi yang sama bisa diklaim lagi di hari lain).
    public static string KunciKlaim(int hari, string id)
    {
        return "DK_Harian_" + hari + "_" + id;
    }
}
