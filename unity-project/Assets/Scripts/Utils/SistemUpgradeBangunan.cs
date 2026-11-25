// Dunia Kita — SistemUpgradeBangunan
// Logika MURNI peningkatan level bangunan (Level 1 -> 2 -> 3)
// tanpa dependensi UnityEngine supaya bisa diuji di luar Unity.

public static class SistemUpgradeBangunan
{
    public const int LevelMaks = 3;

    public static bool BisaUpgrade(string id, int levelSaatIni)
    {
        if (string.IsNullOrEmpty(id) || levelSaatIni >= LevelMaks)
        {
            return false;
        }
        // Jalan, Balai Kota, dan Alun-Alun tidak bisa di-upgrade.
        if (id == "jalan" || id == "balaikota" || id == "alun_alun")
        {
            return false;
        }
        return true;
    }

    public static int NormalisasiLevel(int level)
    {
        if (level < 1) return 1;
        if (level > LevelMaks) return LevelMaks;
        return level;
    }

    // Biaya upgrade: Lv 1->2 = 75% biaya bangun; Lv 2->3 = 125% biaya bangun.
    public static long BiayaUpgrade(long biayaBangun, int levelSaatIni)
    {
        if (biayaBangun <= 0) return 0;
        if (levelSaatIni <= 1) return (biayaBangun * 75) / 100;
        if (levelSaatIni == 2) return (biayaBangun * 125) / 100;
        return 0;
    }

    // Pengali pemasukan harian dalam persen: Lv 1 = 100%, Lv 2 = 135%, Lv 3 = 175%.
    public static int PengaliPemasukanPersen(int level)
    {
        int lv = NormalisasiLevel(level);
        if (lv == 2) return 135;
        if (lv == 3) return 175;
        return 100;
    }

    // Kapasitas warga efektif setelah upgrade.
    public static int KapasitasEfektif(int kapasitasDasar, int level)
    {
        if (kapasitasDasar <= 0) return 0;
        int lv = NormalisasiLevel(level);
        if (lv == 1) return kapasitasDasar;
        if (lv == 2)
        {
            int tambah = (kapasitasDasar * 25) / 100;
            return kapasitasDasar + (tambah < 1 ? 1 : tambah);
        }
        int tambah3 = (kapasitasDasar * 60) / 100;
        return kapasitasDasar + (tambah3 < 2 ? 2 : tambah3);
    }
}
