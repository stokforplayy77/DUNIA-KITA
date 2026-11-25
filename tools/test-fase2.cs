// Dunia Kita — tools/test-fase2.cs
// Test harness logika murni Fase 2 (tanpa Unity): dikompilasi dengan
// mcs bersama EconomyCalculator.cs, GameBalance.cs, SaveData.cs dan
// unity-stubs.cs. Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji: pengali pemasukan warung dari kebahagiaan, pajak kota
// 2,5%, pembayaran + tunggakan, aturan kebahagiaan Fase 2, simulasi
// 10 hari dengan angka persis, dan migrasi simpanan Fase 1 -> Fase 2.

using System;
using System.Collections.Generic;

public static class TestFase2
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

    private static EconomyCalculator.CatatanBangunan Catatan(
        string id, long masuk, long keluar, int warga, bool taman)
    {
        return new EconomyCalculator.CatatanBangunan
        {
            Id = id,
            PemasukanHarian = masuk,
            BiayaPerawatanHarian = keluar,
            KapasitasWarga = warga,
            AdalahTaman = taman,
            AdalahWarung = id == "warung"
        };
    }

    // Dunia standar Fase 2: 2 rumah + 1 warung + 1 taman.
    private static List<EconomyCalculator.CatatanBangunan> DuniaStandar()
    {
        var daftar = new List<EconomyCalculator.CatatanBangunan>();
        daftar.Add(Catatan("rumah", 0, 1000, 2, false));
        daftar.Add(Catatan("rumah", 0, 1000, 2, false));
        daftar.Add(Catatan("warung", 15000, 2000, 0, false));
        daftar.Add(Catatan("taman", 0, 500, 0, true));
        return daftar;
    }

    public static int Main()
    {
        Console.WriteLine("== Tes A: pengali warung dari kebahagiaan ==");
        Periksa(EconomyCalculator.PengaliWarung(0) == 80, "bahagia 0 -> 80%");
        Periksa(EconomyCalculator.PengaliWarung(30) == 80, "bahagia 30 -> 80%");
        Periksa(EconomyCalculator.PengaliWarung(39) == 80, "bahagia 39 -> 80%");
        Periksa(EconomyCalculator.PengaliWarung(40) == 100, "bahagia 40 -> 100%");
        Periksa(EconomyCalculator.PengaliWarung(50) == 100, "bahagia 50 -> 100%");
        Periksa(EconomyCalculator.PengaliWarung(70) == 100, "bahagia 70 -> 100%");
        Periksa(EconomyCalculator.PengaliWarung(71) == 110, "bahagia 71 -> 110%");
        Periksa(EconomyCalculator.PengaliWarung(100) == 110, "bahagia 100 -> 110%");

        Console.WriteLine("== Tes B: pajak kota 2,5% dari pemasukan kotor ==");
        var r = EconomyCalculator.HitungHarian(DuniaStandar(), 50);
        Periksa(r.Pajak == 375, "pajak 375 (2,5% x 15000)");
        r = EconomyCalculator.HitungHarian(DuniaStandar(), 80);
        Periksa(r.PemasukanWarung == 16500, "warung 16500 saat bahagia 80");
        Periksa(r.Pajak == 412, "pajak 412 (2,5% x 16500, dibulatkan ke bawah)");
        r = EconomyCalculator.HitungHarian(new List<EconomyCalculator.CatatanBangunan>(), 50);
        Periksa(r.Pajak == 0 && r.Pemasukan == 0, "dunia kosong: pajak 0");

        Console.WriteLine("== Tes C: ringkasan harian lengkap ==");
        r = EconomyCalculator.HitungHarian(DuniaStandar(), 30);
        Periksa(r.PemasukanWarung == 12000, "warung 12000 saat bahagia 30 (-20%)");
        Periksa(r.PemasukanLain == 0, "pemasukan lain 0");
        Periksa(r.Pajak == 300, "pajak 300 (2,5% x 12000)");
        Periksa(r.Upkeep == 4500, "upkeep 4500");
        Periksa(r.Pengeluaran == 4800, "pengeluaran 4800 (4500 + 300)");
        Periksa(r.Bersih == 7200, "bersih 7200 (12000 - 4800)");
        r = EconomyCalculator.HitungHarian(DuniaStandar(), 80);
        Periksa(r.Bersih == 11588, "bersih 11588 saat bahagia 80 (16500 - 412 - 4500)");

        Console.WriteLine("== Tes D: pembayaran & tunggakan ==");
        var b = EconomyCalculator.HitungPembayaran(100000, 10000, 0);
        Periksa(b.KoinBaru == 110000 && b.TunggakanBaru == 0 && !b.Menunggak,
            "koin cukup: 100000 + 10000 = 110000, tanpa tunggakan");
        b = EconomyCalculator.HitungPembayaran(1000, -5000, 0);
        Periksa(b.KoinBaru == 0 && b.TunggakanBaru == 4000 && b.Menunggak,
            "koin kurang: koin 0, tunggakan 4000, menunggak");
        b = EconomyCalculator.HitungPembayaran(1000, 5000, 4000);
        Periksa(b.KoinBaru == 2000 && b.TunggakanBaru == 0 && !b.Menunggak,
            "lunasi tunggakan: 1000+5000=6000, bayar 4000, sisa 2000");
        b = EconomyCalculator.HitungPembayaran(500, 100, 4000);
        Periksa(b.KoinBaru == 0 && b.TunggakanBaru == 3400 && !b.Menunggak,
            "bayar sebagian: 500+100=600, tunggakan 4000 -> 3400");
        b = EconomyCalculator.HitungPembayaran(0, -1000, 2000);
        Periksa(b.KoinBaru == 0 && b.TunggakanBaru == 3000 && b.Menunggak,
            "koin 0 + bersih negatif: tunggakan 2000 -> 3000");

        Console.WriteLine("== Tes E: aturan kebahagiaan Fase 2 ==");
        Periksa(EconomyCalculator.HitungKebahagiaan(0, 0, 0, 0) == 50,
            "dunia kosong: 50 (tanpa +5 karena belum ada warga)");
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 4, 4, 0) == 57,
            "1 taman + semua punya rumah: 50+2+5 = 57");
        Periksa(EconomyCalculator.HitungKebahagiaan(15, 4, 4, 0) == 75,
            "15 taman: bonus taman dibatasi +20 -> 50+20+5 = 75");
        Periksa(EconomyCalculator.HitungKebahagiaan(0, 6, 4, 0) == 47,
            "tunawisma (6 warga, kapasitas 4): 50-3 = 47, tanpa +5");
        Periksa(EconomyCalculator.HitungKebahagiaan(0, 4, 4, 1) == 55,
            "menunggak 1 hari: 50+5 = 55 (belum dihukum)");
        Periksa(EconomyCalculator.HitungKebahagiaan(0, 4, 4, 2) == 50,
            "menunggak 2 hari: 50+5-5 = 50");
        Periksa(EconomyCalculator.HitungKebahagiaan(0, 4, 4, 5) == 50,
            "menunggak 5 hari: tetap -5 (50+5-5 = 50)");
        Periksa(EconomyCalculator.HitungKebahagiaan(3, 4, 4, 0) == 61,
            "3 taman: 50+6+5 = 61 (misi bahagia 60% tercapai)");

        Console.WriteLine("== Tes F: simulasi 10 hari (angka persis) ==");
        long koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaStandar(), 50).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 10125,
            "10 hari bahagia 50: 5101250 (dapat " + koin + ")");
        // Skenario ramai: 1 rumah + 2 warung + 2 taman, bahagia 80.
        var ramai = new List<EconomyCalculator.CatatanBangunan>();
        ramai.Add(Catatan("rumah", 0, 1000, 2, false));
        ramai.Add(Catatan("warung", 15000, 2000, 0, false));
        ramai.Add(Catatan("warung", 15000, 2000, 0, false));
        ramai.Add(Catatan("taman", 0, 500, 0, true));
        ramai.Add(Catatan("taman", 0, 500, 0, true));
        koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(ramai, 80).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 26175,
            "10 hari bahagia 80 (2 warung): 5261750 (dapat " + koin + ")");
        // Skenario murung: dunia standar, bahagia 30, 10 hari.
        koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaStandar(), 30).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 7200,
            "10 hari bahagia 30: 5072000 (dapat " + koin + ")");

        Console.WriteLine("== Tes G: migrasi simpanan Fase 1 -> Fase 2 -> Fase 3 ==");
        Periksa(SaveData.VersiDidukung("3.0"), "versi 3.0 didukung");
        Periksa(SaveData.VersiDidukung("2.0"), "versi 2.0 didukung");
        Periksa(SaveData.VersiDidukung("1.0"), "versi 1.0 didukung (migrasi)");
        Periksa(!SaveData.VersiDidukung("0.9"), "versi 0.9 ditolak");
        Periksa(!SaveData.VersiDidukung(""), "versi kosong ditolak");
        Periksa(!SaveData.VersiDidukung(null), "versi null ditolak");
        var lama = new SaveData();
        lama.versi = "1.0";
        Periksa(lama.tunggakan == 0 && lama.hariMenunggakBerturut == 0,
            "simpanan 1.0: field Fase 2 default 0 (migrasi aman)");
        Periksa(lama.populasi == 0 && lama.kebahagiaan == 0,
            "simpanan 1.0: populasi/kebahagiaan default 0 (dihitung ulang saat muat)");
        Periksa(lama.bonusBahagiaBesok == 0 && lama.pengaliHujanBesok == 0,
            "simpanan 1.0: field Fase 3 default 0 (migrasi aman)");
        var baru = new SaveData();
        Periksa(baru.versi == "6.0", "simpanan baru berversi 6.0 (dulu 5.1 — naik Dunia Bersama)");

        Console.WriteLine("== Tes H: populasi = kapasitas hunian ==");
        Periksa(EconomyCalculator.HitungPopulasi(DuniaStandar()) == 4,
            "populasi 4 (2 rumah x 2 warga)");
        Periksa(EconomyCalculator.HitungPopulasi(ramai) == 2,
            "populasi 2 (1 rumah x 2 warga)");

        Console.WriteLine();
        Console.WriteLine("Hasil: " + lolos + " lolos, " + gagal + " gagal.");
        return gagal == 0 ? 0 : 1;
    }
}
