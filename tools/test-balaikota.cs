// Dunia Kita — tools/test-balaikota.cs
// Test harness logika Balai Kota + frame jalan warga (tanpa Unity):
// dikompilasi dengan mcs bersama EconomyCalculator.cs, GameBalance.cs,
// SaveData.cs, SistemBalaiKota.cs, PejalanKaki.cs, GridManager.cs,
// IsometricGrid.cs dan unity-stubs.cs.
// Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji: angka Balai Kota (Level 3, harga Rp3.000.000,
// perawatan Rp5.000/hari, bahagia +5, reputasi +2/hari, maks 1),
// batas 1 Balai Kota (BolehBangun), bonus reputasi harian,
// efek bahagia lewat HitungBonusFasilitas (+5 bila ada Jalan,
// +2 bila tidak — aturan Jalan Fase 4), efek perawatan lewat
// HitungHarian (dunia 1 Balai Kota saja: bersih -Rp5.000),
// migrasi simpanan 5.1 -> 6.0 (6.0/5.1/5.0/4.0/3.0/2.0/1.0 didukung),
// dan frame jalan warga (IndeksFrameJalan bergantian 0/1 tiap
// 0,3 detik).
//
// Catatan: kebahagiaan +5 Balai Kota hidup di BalaiKota.asset
// (bonusBahagia) dan mengalir lewat HitungBonusFasilitas seperti
// fasilitas Fase 4 lain; reputasi +2/hari diterapkan GameManager
// tiap pergantian hari (tidak bisa diuji murni di sini —
// GameManager butuh scene Unity).

using System;
using System.Collections.Generic;
using UnityEngine;

public static class TestBalaiKota
{
    private static int lolos = 0;
    private static int gagal = 0;

    private static void Periksa(bool kondisi, string nama)
    {
        if (kondisi)
        {
            lolos++;
            Console.WriteLine("  OK  " + nama);
        }
        else
        {
            gagal++;
            Console.WriteLine("  GAGAL  " + nama);
        }
    }

    private static EconomyCalculator.CatatanBangunan Catatan(string id,
        long masuk, long keluar, int warga, bool taman, int bonus,
        bool tanpaJalan)
    {
        return new EconomyCalculator.CatatanBangunan
        {
            Id = id,
            PemasukanHarian = masuk,
            BiayaPerawatanHarian = keluar,
            KapasitasWarga = warga,
            AdalahTaman = taman,
            AdalahWarung = false,
            BonusBahagia = bonus,
            TanpaJalan = tanpaJalan,
            PengaliSewaPersen = 0
        };
    }

    // Satu Balai Kota (angka dari GameBalance + BalaiKota.asset).
    private static EconomyCalculator.CatatanBangunan BalaiKota(
        bool tanpaJalan)
    {
        return Catatan("balaikota", 0, GameBalance.PerawatanBalaiKota,
            0, false, GameBalance.BonusBahagiaBalaiKota, tanpaJalan);
    }

    public static void Main()
    {
        Console.WriteLine("== Tes A: angka Balai Kota (GameBalance) ==");
        Periksa(GameBalance.LevelBukaBalaiKota == 3, "terbuka Level 3 (Desa)");
        Periksa(GameBalance.HargaBalaiKota == 3000000, "harga Rp3.000.000");
        Periksa(GameBalance.PerawatanBalaiKota == 5000, "perawatan Rp5.000/hari");
        Periksa(GameBalance.BonusBahagiaBalaiKota == 5, "kebahagiaan +5");
        Periksa(GameBalance.BonusReputasiBalaiKotaPerHari == 2, "reputasi +2/hari");
        Periksa(GameBalance.MaksBalaiKotaPerDunia == 1, "maks 1 per dunia");
        Periksa(SistemBalaiKota.IdBalaiKota == "balaikota", "id = balaikota");

        Console.WriteLine("== Tes B: batas 1 + syarat level (SistemBalaiKota) ==");
        Periksa(SistemBalaiKota.BolehBangun(0), "boleh bangun bila belum ada");
        Periksa(!SistemBalaiKota.BolehBangun(1), "tidak boleh bangun bila sudah ada 1");
        Periksa(!SistemBalaiKota.BolehBangun(2), "tidak boleh bangun bila sudah ada 2");
        Periksa(!SistemBalaiKota.ApakahTerbuka(1), "terkunci di Level 1");
        Periksa(!SistemBalaiKota.ApakahTerbuka(2), "terkunci di Level 2");
        Periksa(SistemBalaiKota.ApakahTerbuka(3), "terbuka di Level 3");
        Periksa(SistemBalaiKota.ApakahTerbuka(6), "terbuka di Level 6");

        Console.WriteLine("== Tes C: bonus reputasi harian ==");
        Periksa(SistemBalaiKota.BonusReputasiHarian(0) == 0, "tanpa Balai Kota +0");
        Periksa(SistemBalaiKota.BonusReputasiHarian(1) == 2, "1 Balai Kota +2/hari");

        Console.WriteLine("== Tes D: efek bahagia (HitungBonusFasilitas) ==");
        List<EconomyCalculator.CatatanBangunan> denganJalan =
            new List<EconomyCalculator.CatatanBangunan>();
        denganJalan.Add(BalaiKota(false));
        Periksa(EconomyCalculator.HitungBonusFasilitas(denganJalan) == 5,
            "Balai Kota + Jalan = +5");
        List<EconomyCalculator.CatatanBangunan> tanpaJalan =
            new List<EconomyCalculator.CatatanBangunan>();
        tanpaJalan.Add(BalaiKota(true));
        Periksa(EconomyCalculator.HitungBonusFasilitas(tanpaJalan) == 2,
            "Balai Kota tanpa Jalan = +2 (aturan Jalan Fase 4)");
        List<EconomyCalculator.CatatanBangunan> gabung =
            new List<EconomyCalculator.CatatanBangunan>();
        gabung.Add(BalaiKota(false));
        gabung.Add(Catatan("perpustakaan", 0, 2000, 0, false, 2, false));
        Periksa(EconomyCalculator.HitungBonusFasilitas(gabung) == 7,
            "Balai Kota + Perpustakaan = +7");

        Console.WriteLine("== Tes E: efek perawatan (HitungHarian) ==");
        EconomyCalculator.RingkasanHarian ringkasan =
            EconomyCalculator.HitungHarian(tanpaJalan, 50, 100);
        Periksa(ringkasan.Pemasukan == 0, "1 Balai Kota saja: pemasukan 0");
        Periksa(ringkasan.Upkeep == 5000, "1 Balai Kota saja: upkeep Rp5.000");
        Periksa(ringkasan.Pajak == 0, "1 Balai Kota saja: pajak 0");
        Periksa(ringkasan.Bersih == -5000, "1 Balai Kota saja: bersih -Rp5.000");

        Console.WriteLine("== Tes F: migrasi simpanan 5.1 -> 6.0 ==");
        Periksa(GameBalance.VersiSimpan == "6.0", "versi simpanan sekarang 6.0");
        Periksa(GameBalance.VersiSimpanFase5_1 == "5.1", "versi Fase 5.1 tercatat 5.1");
        Periksa(SaveData.VersiDidukung("6.0"), "6.0 didukung");
        Periksa(SaveData.VersiDidukung("5.1"), "5.1 (Fase 5.1) tetap dimuat");
        Periksa(SaveData.VersiDidukung("5.0"), "5.0 (Fase 4B) tetap dimuat");
        Periksa(SaveData.VersiDidukung("4.0"), "4.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("3.0"), "3.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("2.0"), "2.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("1.0"), "1.0 tetap dimuat");
        Periksa(!SaveData.VersiDidukung("7.0"), "7.0 tidak didukung");
        Periksa(!SaveData.VersiDidukung(""), "versi kosong tidak didukung");

        Console.WriteLine("== Tes G: frame jalan warga (PejalanKaki) ==");
        Periksa(GameBalance.IntervalFrameJalanWarga == 0.3f, "interval frame 0,3 detik");
        Periksa(PejalanKaki.IndeksFrameJalan(0f) == 0, "t=0 -> frame 0");
        Periksa(PejalanKaki.IndeksFrameJalan(0.29f) == 0, "t=0,29 -> frame 0");
        Periksa(PejalanKaki.IndeksFrameJalan(0.3f) == 1, "t=0,3 -> frame 1");
        Periksa(PejalanKaki.IndeksFrameJalan(0.59f) == 1, "t=0,59 -> frame 1");
        Periksa(PejalanKaki.IndeksFrameJalan(0.6f) == 0, "t=0,6 -> frame 0");
        Periksa(PejalanKaki.IndeksFrameJalan(0.9f) == 1, "t=0,9 -> frame 1");
        Periksa(PejalanKaki.IndeksFrameJalan(1.2f) == 0, "t=1,2 -> frame 0");
        Periksa(PejalanKaki.IndeksFrameJalan(1.5f) == 1, "t=1,5 -> frame 1");

        Console.WriteLine();
        Console.WriteLine("HASIL: " + lolos + " lolos, " + gagal + " gagal");
        if (gagal > 0)
        {
            Environment.Exit(1);
        }
    }
}
