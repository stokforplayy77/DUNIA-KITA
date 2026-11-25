// Dunia Kita — ProgresiDunia
// Logika MURNI progresi level dunia (GDD §8), tanpa dependensi
// UnityEngine, supaya bisa diuji di luar Unity (tools/test-fase3.cs).
//
// Tabel tingkatan dunia (GDD §8):
//   1 Tanah Kosong : awal permainan
//   2 Dusun        : 10 warga
//   3 Desa         : 30 warga + kebahagiaan >= 60
//   4 Kota Kecil   : 80 warga + kebahagiaan >= 70
//   5 Kota         : 150 warga + kebahagiaan >= 75
//   6 Metropolis   : 300 warga + kebahagiaan >= 80
//
// FASE 3: level dunia dibatasi sampai Level 3 (Desa). Level 4-6
// dibuka di Fase 4 — BisaNaikKe() kini mengizinkan sampai Level 6
// (lihat GameBalance.LevelMaksDunia = 6).

public static class ProgresiDunia
{
    // Populasi minimum untuk naik KE level tujuan.
    public static int SyaratPopulasi(int levelTujuan)
    {
        switch (levelTujuan)
        {
            case 2: return 10;
            case 3: return 30;
            case 4: return 80;
            case 5: return 150;
            case 6: return 300;
            default: return int.MaxValue;
        }
    }

    // Kebahagiaan minimum untuk naik KE level tujuan.
    // Level 2 (Dusun) tidak mensyaratkan kebahagiaan (GDD §8).
    public static int SyaratBahagia(int levelTujuan)
    {
        switch (levelTujuan)
        {
            case 2: return 0;
            case 3: return 60;
            case 4: return 70;
            case 5: return 75;
            case 6: return 80;
            default: return int.MaxValue;
        }
    }

    // Nama tampilan tiap tingkatan dunia (GDD §8).
    public static string NamaLevel(int level)
    {
        switch (level)
        {
            case 1: return "Tanah Kosong";
            case 2: return "Dusun";
            case 3: return "Desa";
            case 4: return "Kota Kecil";
            case 5: return "Kota";
            default: return "Metropolis";
        }
    }

    // Hadiah koin sekali saat naik ke level tujuan.
    // Angka TIDAK ditetapkan GDD — keputusan Fase 3 (L2-L3) dan
    // Fase 4 (L4-L6, lihat GameBalance).
    public static long HadiahNaikLevel(int levelTujuan)
    {
        switch (levelTujuan)
        {
            case 2: return GameBalance.HadiahNaikLevel2;
            case 3: return GameBalance.HadiahNaikLevel3;
            case 4: return GameBalance.HadiahNaikLevel4;
            case 5: return GameBalance.HadiahNaikLevel5;
            case 6: return GameBalance.HadiahNaikLevel6;
            default: return 0;
        }
    }

    // Apakah dunia boleh naik ke level tujuan dengan populasi dan
    // kebahagiaan saat ini? Fase 4 membuka sampai Level 6.
    public static bool BisaNaikKe(int levelTujuan, int populasi, int kebahagiaan)
    {
        if (levelTujuan < 2 || levelTujuan > GameBalance.LevelMaksDunia)
        {
            return false;
        }
        return populasi >= SyaratPopulasi(levelTujuan)
            && kebahagiaan >= SyaratBahagia(levelTujuan);
    }

    // Apakah bangunan dengan syarat level ini sudah terbuka?
    // levelBuka <= 0 diperlakukan sebagai 1 (aman untuk .asset lama
    // yang belum punya field levelBuka).
    public static bool ApakahTerbuka(int levelBuka, int levelDunia)
    {
        int syarat = levelBuka < 1 ? 1 : levelBuka;
        return levelDunia >= syarat;
    }

    // Teks syarat naik ke level tujuan, untuk menu bangun yang terkunci.
    public static string TeksSyarat(int levelTujuan)
    {
        int bahagia = SyaratBahagia(levelTujuan);
        if (bahagia <= 0)
        {
            return SyaratPopulasi(levelTujuan) + " warga";
        }
        return SyaratPopulasi(levelTujuan) + " warga + kebahagiaan "
            + bahagia + "%";
    }
}
