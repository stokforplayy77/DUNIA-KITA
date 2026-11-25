// Dunia Kita — SistemBalaiKota
// Logika MURNI Balai Kota (tanpa dependensi UnityEngine) supaya bisa
// dikompilasi dan diuji di luar Unity (lihat tools/test-balaikota.cs).
//
// Balai Kota: fasilitas/landmark 2x2, terbuka Level 3 (Desa), harga
// Rp3.000.000, perawatan Rp5.000/hari, kebahagiaan +5 (nilai ini di
// BalaiKota.asset sebagai bonusBahagia — mengalir lewat
// EconomyCalculator.HitungBonusFasilitas seperti fasilitas Fase 4
// lain: +5 bila bersebelahan Jalan, +2 bila tidak) dan reputasi
// +2/hari (sumber reputasi pasif pertama; sebelum ini reputasi
// hanya berubah lewat peristiwa penghuni, GDD §9.1).
// Hanya 1 Balai Kota per dunia — batas ditegakkan BuildManager
// (CobaBangun) dan ditampilkan UIManager (menu bangun).
// Wali Kota sebagai NPC yang mengurus izin tanah tetap di Fase 7
// (multiplayer) — gedungnya sudah ada, orangnya menyusul.

public static class SistemBalaiKota
{
    public const string IdBalaiKota = "balaikota";

    // Boleh membangun bila belum ada Balai Kota di dunia.
    public static bool BolehBangun(int sudahAda)
    {
        return sudahAda < GameBalance.MaksBalaiKotaPerDunia;
    }

    // Bonus reputasi harian: +2 bila ada Balai Kota, 0 bila tidak.
    public static int BonusReputasiHarian(int jumlahBalaiKota)
    {
        return jumlahBalaiKota > 0
            ? GameBalance.BonusReputasiBalaiKotaPerHari
            : 0;
    }

    // Syarat level: Balai Kota terbuka di Level 3 (Desa).
    public static bool ApakahTerbuka(int levelDunia)
    {
        return levelDunia >= GameBalance.LevelBukaBalaiKota;
    }
}
