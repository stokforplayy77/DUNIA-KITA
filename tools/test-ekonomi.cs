// Dunia Kita — tests/test-ekonomi.cs
// Test harness logika murni (tanpa Unity): dikompilasi dengan mcs
// bersama EconomyCalculator.cs, FormatUtil.cs, GameBalance.cs,
// IsometricGrid.cs dan unity-stubs.cs. Keluar kode 0 bila semua
// pemeriksaan lolos.

using System;
using System.Collections.Generic;

public static class TestEkonomi
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

    public static int Main()
    {
        Console.WriteLine("== Tes 1: dunia kosong ==");
        var kosong = new List<EconomyCalculator.CatatanBangunan>();
        var r = EconomyCalculator.HitungHarian(kosong);
        Periksa(r.Pemasukan == 0 && r.Pengeluaran == 0 && r.Bersih == 0,
            "dunia kosong: 0/0/0");
        Periksa(EconomyCalculator.HitungPopulasi(kosong) == 0,
            "populasi 0");

        Console.WriteLine("== Tes 2: 2 rumah + 1 warung + 1 taman (bahagia 50) ==");
        var daftar = new List<EconomyCalculator.CatatanBangunan>();
        daftar.Add(Catatan("rumah", 0, 1000, 2, false));
        daftar.Add(Catatan("rumah", 0, 1000, 2, false));
        daftar.Add(Catatan("warung", 15000, 2000, 0, false));
        daftar.Add(Catatan("taman", 0, 500, 0, true));
        r = EconomyCalculator.HitungHarian(daftar);
        Periksa(r.Pemasukan == 15000, "pemasukan 15000 (warung, bahagia 50 = normal)");
        Periksa(r.PemasukanWarung == 15000, "pemasukan warung 15000");
        Periksa(r.Pajak == 375, "pajak 375 (2,5% x 15000)");
        Periksa(r.Upkeep == 4500, "upkeep 4500 (1000+1000+2000+500)");
        Periksa(r.Pengeluaran == 4875, "pengeluaran 4875 (4500 + 375)");
        Periksa(r.Bersih == 10125, "bersih 10125");
        Periksa(EconomyCalculator.HitungPopulasi(daftar) == 4,
            "populasi 4 (2 rumah x 2 warga)");
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 4, 4, 0) == 57,
            "kebahagiaan 57 (50 + 1 taman x 2 + 5 semua warga punya rumah)");

        Console.WriteLine("== Tes 3: harga jual & batas kebahagiaan ==");
        Periksa(EconomyCalculator.HitungHargaJual(500000, 0.5f) == 250000,
            "jual rumah: 50% x 500000 = 250000");
        Periksa(EconomyCalculator.HitungHargaJual(750000, 0.5f) == 375000,
            "jual warung: 50% x 750000 = 375000");
        Periksa(EconomyCalculator.HitungKebahagiaan(30, 4, 4, 0) == 75,
            "kebahagiaan 75 (50 + bonus taman dibatasi +20 + 5 punya rumah)");

        Console.WriteLine("== Tes 4: simulasi 10 hari (1 warung + 2 rumah) ==");
        long koin = GameBalance.KoinAwal; // 5.000.000
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(daftar).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 10125,
            "koin setelah 10 hari: 5101250 (dapat " + koin + ")");

        Console.WriteLine("== Tes 5: grid isometric bolak-balik ==");
        var grid = new IsometricGrid();
        bool semuaCocok = true;
        for (int x = 0; x < 12 && semuaCocok; x++)
        {
            for (int y = 0; y < 12 && semuaCocok; y++)
            {
                var petak = new UnityEngine.Vector2Int(x, y);
                var dunia = grid.GridKeDunia(petak);
                var kembali = grid.DuniaKeGrid(dunia);
                if (kembali != petak)
                {
                    semuaCocok = false;
                    Console.WriteLine("    petak (" + x + "," + y +
                        ") kembali sebagai (" + kembali.x + "," + kembali.y + ")");
                }
            }
        }
        Periksa(semuaCocok, "144 petak: GridKeDunia lalu DuniaKeGrid kembali sama");
        Periksa(grid.DiDalamGrid(new UnityEngine.Vector2Int(0, 0)), "(0,0) di dalam grid");
        Periksa(grid.DiDalamGrid(new UnityEngine.Vector2Int(11, 11)), "(11,11) di dalam grid");
        Periksa(!grid.DiDalamGrid(new UnityEngine.Vector2Int(12, 0)), "(12,0) di luar grid");
        Periksa(!grid.DiDalamGrid(new UnityEngine.Vector2Int(-1, 5)), "(-1,5) di luar grid");

        Console.WriteLine("== Tes 6: format Rupiah ==");
        Periksa(FormatUtil.Rupiah(5000000) == "Rp5.000.000",
            "5000000 -> Rp5.000.000 (dapat '" + FormatUtil.Rupiah(5000000) + "')");
        Periksa(FormatUtil.Rupiah(15000) == "Rp15.000",
            "15000 -> Rp15.000");
        Periksa(FormatUtil.Hari(3) == "Hari 3", "Hari(3) -> 'Hari 3'");

        Console.WriteLine();
        Console.WriteLine("Hasil: " + lolos + " lolos, " + gagal + " gagal.");
        return gagal == 0 ? 0 : 1;
    }
}
