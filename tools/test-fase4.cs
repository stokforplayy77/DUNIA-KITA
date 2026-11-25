// Dunia Kita — tools/test-fase4.cs
// Test harness logika murni Fase 4 (tanpa Unity): dikompilasi dengan
// mcs bersama EconomyCalculator.cs, FormatUtil.cs, GameBalance.cs, SaveData.cs,
// ProgresiDunia.cs, SistemPencapaian.cs, SistemMisiHarian.cs, IsometricGrid.cs
// dan unity-stubs.cs. Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji: syarat naik level dunia 4-6 di batasnya (GDD §8),
// unlock 18 bangunan per level (17 Fase 4 + Balai Kota Level 3),
// aturan Jalan (GDD §5.5: bersebelahan
// 4 arah vs diagonal/tidak), efek tiap bangunan baru ke ekonomi dan
// kebahagiaan, pencapaian (syarat di batas + hadiah Permata + id
// tidak dikenal), misi harian (rotasi deterministik per hari +
// ApakahSelesai tiap misi + kunci klaim), simulasi 10 hari dunia
// standar Fase 4 dengan angka persis, migrasi simpanan 3.0 -> 4.0,
// dan matematika perluasan grid 12x12 -> 16x16.
//
// Catatan: "dunia standar Fase 4" di tes ini memakai Jalan di semua
// bangunan (TanpaJalan=false); varian tanpa Jalan diuji terpisah
// untuk membuktikan penalti 50%.

using System;
using System.Collections.Generic;
using UnityEngine;

public static class TestFase4
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

    // Satu baris bangunan untuk HitungHarian. AdalahWarung = komersial
    // (Warung, Toko Kelontong, Pasar, Kafe) — sama seperti
    // EconomyManager.BuatCatatan: pemasukan dipengaruhi kebahagiaan
    // + hujan.
    private static EconomyCalculator.CatatanBangunan Catatan(string id,
        long masuk, long keluar, int warga, bool taman, int bonus, bool tanpaJalan)
    {
        return new EconomyCalculator.CatatanBangunan
        {
            Id = id,
            PemasukanHarian = masuk,
            BiayaPerawatanHarian = keluar,
            KapasitasWarga = warga,
            AdalahTaman = taman,
            AdalahWarung = id == "warung" || id == "toko_kelontong"
                || id == "pasar" || id == "kafe",
            BonusBahagia = bonus,
            TanpaJalan = tanpaJalan
        };
    }

    private static EconomyCalculator.CatatanBangunan Catatan(string id,
        long masuk, long keluar, int warga, bool taman, int bonus)
    {
        return Catatan(id, masuk, keluar, warga, taman, bonus, false);
    }

    // Dunia kaya Fase 4 (Tes E): 4 Rumah, 2 Kost, 1 Apartemen,
    // 1 Warung, 1 Toko, 1 Pasar, 1 Kafe, 1 Kampus, 1 Perumahan,
    // 2 Taman, 1 Perpustakaan, 1 Klinik, 1 Sekolah, 1 Pos,
    // 1 Pembangkit, 1 Alun-alun, 1 Jalan.
    // Populasi 56 (8+16+12+20), kapasitas 56.
    // bonusFasilitas = 2+4+2+2+4+4+6+10 = 34.
    private static List<EconomyCalculator.CatatanBangunan> DuniaKaya(bool tanpaJalan)
    {
        var d = new List<EconomyCalculator.CatatanBangunan>();
        for (int i = 0; i < 4; i++)
        {
            d.Add(Catatan("rumah", 0, 1000, 2, false, 0, tanpaJalan));
        }
        for (int i = 0; i < 2; i++)
        {
            d.Add(Catatan("kost", 40000, 5000, 8, false, 0, tanpaJalan));
        }
        d.Add(Catatan("apartemen", 60000, 10000, 12, false, 0, tanpaJalan));
        d.Add(Catatan("warung", 15000, 2000, 0, false, 0, tanpaJalan));
        d.Add(Catatan("toko_kelontong", 30000, 4000, 0, false, 0, tanpaJalan));
        d.Add(Catatan("pasar", 60000, 8000, 0, false, 0, tanpaJalan));
        d.Add(Catatan("kafe", 25000, 5000, 0, false, 2, tanpaJalan));
        d.Add(Catatan("kampus", 80000, 20000, 0, false, 6, tanpaJalan));
        d.Add(Catatan("perumahan", 100000, 15000, 20, false, 0, tanpaJalan));
        d.Add(Catatan("taman", 0, 500, 0, true, 0, tanpaJalan));
        d.Add(Catatan("taman", 0, 500, 0, true, 0, tanpaJalan));
        d.Add(Catatan("perpustakaan", 0, 2000, 0, false, 2, tanpaJalan));
        d.Add(Catatan("klinik", 0, 10000, 0, false, 4, tanpaJalan));
        d.Add(Catatan("sekolah", 0, 8000, 0, false, 4, tanpaJalan));
        d.Add(Catatan("pos", 0, 3000, 0, false, 2, tanpaJalan));
        d.Add(Catatan("pembangkit", 0, 12000, 0, false, 4, tanpaJalan));
        d.Add(Catatan("alun_alun", 0, 25000, 0, false, 10, tanpaJalan));
        d.Add(Catatan("jalan", 0, 0, 0, false, 0, false));
        return d;
    }

    // Dunia standar Fase 4 (Tes H): 2 Rumah, 1 Kost, 1 Warung,
    // 1 Toko, 1 Pasar, 1 Kafe, 2 Taman, 1 Perpustakaan, 1 Jalan.
    // Populasi 12, kapasitas 12.
    // bonusFasilitas = perpustakaan 2 + kafe 2 = 4.
    // Kebahagiaan = 50 + 4 (taman) + 5 (semua punya rumah) + 4 = 63.
    private static List<EconomyCalculator.CatatanBangunan> DuniaStandarFase4(bool tanpaJalan)
    {
        var d = new List<EconomyCalculator.CatatanBangunan>();
        d.Add(Catatan("rumah", 0, 1000, 2, false, 0, tanpaJalan));
        d.Add(Catatan("rumah", 0, 1000, 2, false, 0, tanpaJalan));
        d.Add(Catatan("kost", 40000, 5000, 8, false, 0, tanpaJalan));
        d.Add(Catatan("warung", 15000, 2000, 0, false, 0, tanpaJalan));
        d.Add(Catatan("toko_kelontong", 30000, 4000, 0, false, 0, tanpaJalan));
        d.Add(Catatan("pasar", 60000, 8000, 0, false, 0, tanpaJalan));
        d.Add(Catatan("kafe", 25000, 5000, 0, false, 2, tanpaJalan));
        d.Add(Catatan("taman", 0, 500, 0, true, 0, tanpaJalan));
        d.Add(Catatan("taman", 0, 500, 0, true, 0, tanpaJalan));
        d.Add(Catatan("perpustakaan", 0, 2000, 0, false, 2, tanpaJalan));
        d.Add(Catatan("jalan", 0, 0, 0, false, 0, false));
        return d;
    }

    private static void PeriksaBuka(string id, int levelBuka)
    {
        Periksa(ProgresiDunia.ApakahTerbuka(levelBuka, levelBuka),
            id + " terbuka di L" + levelBuka);
        if (levelBuka > 1)
        {
            Periksa(!ProgresiDunia.ApakahTerbuka(levelBuka, levelBuka - 1),
                id + " terkunci di L" + (levelBuka - 1));
        }
    }

    private static EconomyCalculator.PetakBangunan Petak(string id,
        int x, int y, int lebar, int tinggi)
    {
        return new EconomyCalculator.PetakBangunan
        {
            Id = id, X = x, Y = y, Lebar = lebar, Tinggi = tinggi
        };
    }

    public static int Main()
    {
        Console.WriteLine("== Tes A: syarat naik level 4-6 di batasnya (GDD §8) ==");
        Periksa(ProgresiDunia.SyaratPopulasi(4) == 80, "L4 butuh 80 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(5) == 150, "L5 butuh 150 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(6) == 300, "L6 butuh 300 warga");
        Periksa(ProgresiDunia.SyaratBahagia(4) == 70, "L4 butuh bahagia 70");
        Periksa(ProgresiDunia.SyaratBahagia(5) == 75, "L5 butuh bahagia 75");
        Periksa(ProgresiDunia.SyaratBahagia(6) == 80, "L6 butuh bahagia 80");
        Periksa(ProgresiDunia.NamaLevel(4) == "Kota Kecil", "nama L4");
        Periksa(ProgresiDunia.NamaLevel(5) == "Kota", "nama L5");
        Periksa(ProgresiDunia.NamaLevel(6) == "Metropolis", "nama L6");
        Periksa(ProgresiDunia.HadiahNaikLevel(4) == 500000, "hadiah L4 Rp500.000");
        Periksa(ProgresiDunia.HadiahNaikLevel(5) == 1000000, "hadiah L5 Rp1.000.000");
        Periksa(ProgresiDunia.HadiahNaikLevel(6) == 2000000, "hadiah L6 Rp2.000.000");
        Periksa(ProgresiDunia.BisaNaikKe(4, 80, 70), "L4: 80 warga + bahagia 70 cukup");
        Periksa(!ProgresiDunia.BisaNaikKe(4, 79, 70), "L4: 79 warga kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(4, 80, 69), "L4: bahagia 69 kurang");
        Periksa(ProgresiDunia.BisaNaikKe(5, 150, 75), "L5: 150 warga + bahagia 75 cukup");
        Periksa(!ProgresiDunia.BisaNaikKe(5, 149, 75), "L5: 149 warga kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(5, 150, 74), "L5: bahagia 74 kurang");
        Periksa(ProgresiDunia.BisaNaikKe(6, 300, 80), "L6: 300 warga + bahagia 80 cukup");
        Periksa(!ProgresiDunia.BisaNaikKe(6, 299, 80), "L6: 299 warga kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(6, 300, 79), "L6: bahagia 79 kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(7, 999, 100), "tidak bisa naik ke L7");
        Periksa(GameBalance.LevelMaksDunia == 6, "LevelMaksDunia = 6");
        Periksa(ProgresiDunia.HadiahNaikLevel(2) + ProgresiDunia.HadiahNaikLevel(3)
            + ProgresiDunia.HadiahNaikLevel(4) + ProgresiDunia.HadiahNaikLevel(5)
            + ProgresiDunia.HadiahNaikLevel(6) == 3850000,
            "total hadiah naik level L2-L6 = Rp3.850.000");

        Console.WriteLine("== Tes B: unlock 18 bangunan per level ==");
        PeriksaBuka("rumah", 1);
        PeriksaBuka("warung", 1);
        PeriksaBuka("taman", 1);
        PeriksaBuka("jalan", 1);
        PeriksaBuka("kost", 2);
        PeriksaBuka("perpustakaan", 2);
        PeriksaBuka("toko_kelontong", 3);
        PeriksaBuka("sekolah", 3);
        PeriksaBuka("balaikota", 3);
        PeriksaBuka("pasar", 3);
        PeriksaBuka("pos", 3);
        PeriksaBuka("apartemen", 4);
        PeriksaBuka("kafe", 4);
        PeriksaBuka("klinik", 4);
        PeriksaBuka("pembangkit", 4);
        PeriksaBuka("perumahan", 5);
        PeriksaBuka("kampus", 5);
        PeriksaBuka("alun_alun", 6);

        Console.WriteLine("== Tes C: aturan Jalan (GDD §5.5) ==");
        // Rumah 1x1 di (0,0) + Jalan di kanan/bawah/atas = bersebelahan.
        var r1 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("rumah", 0, 0, 1, 1), Petak("jalan", 1, 0, 1, 1) });
        Periksa(!r1[0] && !r1[1], "jalan di kanan rumah 1x1 = bersebelahan");
        var r2 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("rumah", 0, 0, 1, 1), Petak("jalan", 0, 1, 1, 1) });
        Periksa(!r2[0], "jalan di atas rumah 1x1 = bersebelahan");
        var r3 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("rumah", 0, 0, 1, 1), Petak("jalan", 0, -1, 1, 1) });
        Periksa(!r3[0], "jalan di bawah rumah 1x1 = bersebelahan");
        // Diagonal TIDAK dihitung bersebelahan.
        var r4 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("rumah", 0, 0, 1, 1), Petak("jalan", 1, 1, 1, 1) });
        Periksa(r4[0], "jalan diagonal = TIDAK bersebelahan");
        var r5 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("rumah", 0, 0, 1, 1), Petak("jalan", 2, 0, 1, 1) });
        Periksa(r5[0], "jalan 2 petak jauhnya = TIDAK bersebelahan");
        // Kost 2x2 di (0,0): jalan di (2,1) dan (1,2) bersebelahan,
        // jalan di (2,2) hanya menyentuh sudut (diagonal).
        var r6 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("kost", 0, 0, 2, 2), Petak("jalan", 2, 1, 1, 1) });
        Periksa(!r6[0], "jalan di kanan kost 2x2 = bersebelahan");
        var r7 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("kost", 0, 0, 2, 2), Petak("jalan", 1, 2, 1, 1) });
        Periksa(!r7[0], "jalan di atas kost 2x2 = bersebelahan");
        var r8 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("kost", 0, 0, 2, 2), Petak("jalan", 2, 2, 1, 1) });
        Periksa(r8[0], "jalan di sudut kost 2x2 = TIDAK bersebelahan");
        // Perumahan 3x3 di (0,0): jalan di (3,1) dan (1,-1) bersebelahan.
        var r9 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("perumahan", 0, 0, 3, 3), Petak("jalan", 3, 1, 1, 1) });
        Periksa(!r9[0], "jalan di kanan perumahan 3x3 = bersebelahan");
        var r10 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("perumahan", 0, 0, 3, 3), Petak("jalan", 1, -1, 1, 1) });
        Periksa(!r10[0], "jalan di bawah perumahan 3x3 = bersebelahan");
        // Jalan itu sendiri tidak butuh jalan; daftar kosong aman.
        var r11 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("jalan", 5, 5, 1, 1) });
        Periksa(!r11[0], "jalan sendiri selalu berfungsi penuh");
        var r12 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>());
        Periksa(r12.Length == 0, "daftar kosong -> hasil kosong");
        // Tanpa jalan sama sekali: semua bangunan tanpa jalan.
        var r13 = EconomyCalculator.HitungTanpaJalan(new List<EconomyCalculator.PetakBangunan>
            { Petak("rumah", 0, 0, 1, 1), Petak("warung", 3, 3, 1, 1) });
        Periksa(r13[0] && r13[1], "tanpa jalan di dunia: semua tanpa jalan");

        Console.WriteLine("== Tes D: bonus kebahagiaan fasilitas + overload HitungKebahagiaan ==");
        // HitungBonusFasilitas: fasilitas tanpa Jalan = setengah.
        var f1 = new List<EconomyCalculator.CatatanBangunan>
            { Catatan("perpustakaan", 0, 2000, 0, false, 2) };
        Periksa(EconomyCalculator.HitungBonusFasilitas(f1) == 2,
            "perpustakaan dengan jalan: bonus +2");
        var f2 = new List<EconomyCalculator.CatatanBangunan>
            { Catatan("perpustakaan", 0, 2000, 0, false, 2, true) };
        Periksa(EconomyCalculator.HitungBonusFasilitas(f2) == 1,
            "perpustakaan tanpa jalan: bonus +1 (setengah)");
        var f3 = new List<EconomyCalculator.CatatanBangunan>
            { Catatan("kampus", 80000, 20000, 0, false, 6, true) };
        Periksa(EconomyCalculator.HitungBonusFasilitas(f3) == 3,
            "kampus tanpa jalan: bonus +3 (setengah dari 6)");
        var f4 = new List<EconomyCalculator.CatatanBangunan>
            { Catatan("alun_alun", 0, 25000, 0, false, 10, true) };
        Periksa(EconomyCalculator.HitungBonusFasilitas(f4) == 5,
            "alun-alun tanpa jalan: bonus +5 (setengah dari 10)");
        var f5 = new List<EconomyCalculator.CatatanBangunan>
            { Catatan("taman", 0, 500, 0, true, 0, true),
              Catatan("taman", 0, 500, 0, true, 0, true) };
        Periksa(EconomyCalculator.HitungBonusFasilitas(f5) == -2,
            "2 taman tanpa jalan: penalti -2");
        var f6 = new List<EconomyCalculator.CatatanBangunan>
            { Catatan("perpustakaan", 0, 2000, 0, false, 2),
              Catatan("kafe", 25000, 5000, 0, false, 2, true),
              Catatan("taman", 0, 500, 0, true, 0, true) };
        Periksa(EconomyCalculator.HitungBonusFasilitas(f6) == 2,
            "campuran: 2 + 1 - 1 = 2");
        // Dunia kaya: bonus fasilitas 34 (2+4+2+2+4+4+6+10).
        Periksa(EconomyCalculator.HitungBonusFasilitas(DuniaKaya(false)) == 34,
            "dunia kaya dengan jalan: bonus fasilitas 34");
        // HitungKebahagiaan overload 6-argumen.
        Periksa(EconomyCalculator.HitungKebahagiaan(2, 56, 56, 0, 0, 34) == 93,
            "dunia kaya: 50+4 taman+5 rumah+34 fasilitas = 93");
        Periksa(EconomyCalculator.HitungKebahagiaan(0, 4, 4, 0, 0, 0) == 55,
            "tanpa fasilitas: 50+5 = 55");
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 4, 4, 0, 10, 6) == 73,
            "festival + fasilitas: 50+2+5+10+6 = 73");
        Periksa(EconomyCalculator.HitungKebahagiaan(15, 4, 4, 0, 10, 50) == 100,
            "bonus besar dibatasi maks 100 (cap taman +20 tetap berlaku)");
        Periksa(EconomyCalculator.HitungKebahagiaan(2, 12, 12, 0, 0, 4) == 63,
            "dunia standar Fase 4: 50+4+5+4 = 63");

        Console.WriteLine("== Tes E: ekonomi dunia kaya Fase 4 (dengan vs tanpa jalan) ==");
        // Dengan jalan, bahagia 75 (pengali warung 110%), hujan 100.
        // Komersial: (15000+30000+60000+25000) x 110% = 143000.
        // Lain: kost 80000 + apartemen 60000 + kampus 80000
        //   + perumahan 100000 = 320000. Kotor 463000.
        // Pajak 463000 x 25/1000 = 11575. Upkeep 139000.
        // Bersih = 463000 - 11575 - 139000 = 312425.
        var e1 = EconomyCalculator.HitungHarian(DuniaKaya(false), 75, 100);
        Periksa(e1.PemasukanWarung == 143000, "kaya+jln: komersial 143000");
        Periksa(e1.PemasukanLain == 320000, "kaya+jln: lain 320000");
        Periksa(e1.Pemasukan == 463000, "kaya+jln: kotor 463000");
        Periksa(e1.Pajak == 11575, "kaya+jln: pajak 11575");
        Periksa(e1.Upkeep == 139000, "kaya+jln: upkeep 139000");
        Periksa(e1.Bersih == 312425, "kaya+jln: bersih 312425");
        // Tanpa jalan: semua pemasukan setengah (komersial 71500,
        // lain 160000, kotor 231500, pajak 5787, bersih 86713).
        var e2 = EconomyCalculator.HitungHarian(DuniaKaya(true), 75, 100);
        Periksa(e2.PemasukanWarung == 71500, "kaya-jln: komersial 71500");
        Periksa(e2.PemasukanLain == 160000, "kaya-jln: lain 160000");
        Periksa(e2.Pemasukan == 231500, "kaya-jln: kotor 231500");
        Periksa(e2.Pajak == 5787, "kaya-jln: pajak 5787 (231500 x 25/1000, ke bawah)");
        Periksa(e2.Bersih == 86713, "kaya-jln: bersih 86713");
        Periksa(EconomyCalculator.HitungPopulasi(DuniaKaya(false)) == 56,
            "populasi dunia kaya 56 (8+16+12+20)");

        Console.WriteLine("== Tes F: pencapaian (GDD §8) ==");
        Periksa(SistemPencapaian.SemuaId().Length == 6, "6 pencapaian");
        var p0 = new SistemPencapaian.KeadaanPencapaian();
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.JuraganKost, p0),
            "juragan kost: 0 kost belum tercapai");
        var pKost4 = new SistemPencapaian.KeadaanPencapaian { JumlahKost = 4 };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.JuraganKost, pKost4),
            "juragan kost: 4 kost belum tercapai (batas bawah)");
        var pKost5 = new SistemPencapaian.KeadaanPencapaian { JumlahKost = 5 };
        Periksa(SistemPencapaian.ApakahTercapai(SistemPencapaian.JuraganKost, pKost5),
            "juragan kost: 5 kost tercapai");
        var pSekolah = new SistemPencapaian.KeadaanPencapaian { PunyaSekolah = true };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaPelajar, pSekolah),
            "kota pelajar: hanya sekolah belum tercapai");
        var pKampus = new SistemPencapaian.KeadaanPencapaian { PunyaKampus = true };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaPelajar, pKampus),
            "kota pelajar: hanya kampus belum tercapai");
        var pPelajar = new SistemPencapaian.KeadaanPencapaian
            { PunyaSekolah = true, PunyaKampus = true };
        Periksa(SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaPelajar, pPelajar),
            "kota pelajar: sekolah + kampus tercapai");
        var pHijau19 = new SistemPencapaian.KeadaanPencapaian { JumlahTaman = 19 };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaHijau, pHijau19),
            "kota hijau: 19 taman belum tercapai (batas bawah)");
        var pHijau20 = new SistemPencapaian.KeadaanPencapaian { JumlahTaman = 20 };
        Periksa(SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaHijau, pHijau20),
            "kota hijau: 20 taman tercapai");
        var pL5 = new SistemPencapaian.KeadaanPencapaian { LevelDunia = 5 };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.Metropolis, pL5),
            "metropolis: level 5 belum tercapai");
        var pL6 = new SistemPencapaian.KeadaanPencapaian { LevelDunia = 6 };
        Periksa(SistemPencapaian.ApakahTercapai(SistemPencapaian.Metropolis, pL6),
            "metropolis: level 6 tercapai");
        var pPasar2 = new SistemPencapaian.KeadaanPencapaian { JumlahKomersial = 2 };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.JuraganPasar, pPasar2),
            "juragan pasar: 2 komersial belum tercapai");
        var pPasar3 = new SistemPencapaian.KeadaanPencapaian { JumlahKomersial = 3 };
        Periksa(SistemPencapaian.ApakahTercapai(SistemPencapaian.JuraganPasar, pPasar3),
            "juragan pasar: 3 komersial tercapai");
        var pSejahtera79 = new SistemPencapaian.KeadaanPencapaian { Kebahagiaan = 79 };
        Periksa(!SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaSejahtera, pSejahtera79),
            "kota sejahtera: bahagia 79 belum tercapai (batas bawah)");
        var pSejahtera80 = new SistemPencapaian.KeadaanPencapaian { Kebahagiaan = 80 };
        Periksa(SistemPencapaian.ApakahTercapai(SistemPencapaian.KotaSejahtera, pSejahtera80),
            "kota sejahtera: bahagia 80 tercapai");
        Periksa(SistemPencapaian.HadiahPermata(SistemPencapaian.JuraganKost) == 3,
            "hadiah juragan kost 3 Permata");
        Periksa(SistemPencapaian.HadiahPermata(SistemPencapaian.KotaPelajar) == 3,
            "hadiah kota pelajar 3 Permata");
        Periksa(SistemPencapaian.HadiahPermata(SistemPencapaian.KotaHijau) == 2,
            "hadiah kota hijau 2 Permata");
        Periksa(SistemPencapaian.HadiahPermata(SistemPencapaian.Metropolis) == 10,
            "hadiah metropolis 10 Permata");
        Periksa(SistemPencapaian.HadiahPermata(SistemPencapaian.JuraganPasar) == 3,
            "hadiah juragan pasar 3 Permata");
        Periksa(SistemPencapaian.HadiahPermata(SistemPencapaian.KotaSejahtera) == 5,
            "hadiah kota sejahtera 5 Permata");
        Periksa(SistemPencapaian.Judul(SistemPencapaian.JuraganKost) == "Juragan Kost",
            "judul juragan kost");
        Periksa(SistemPencapaian.Deskripsi(SistemPencapaian.KotaHijau).Length > 0,
            "deskripsi kota hijau tidak kosong");
        Periksa(!SistemPencapaian.ApakahTercapai("tidak_ada", pL6),
            "id tidak dikenal: tidak tercapai");
        Periksa(SistemPencapaian.HadiahPermata("tidak_ada") == 0,
            "id tidak dikenal: hadiah 0");

        Console.WriteLine("== Tes G: misi harian (GDD §9) ==");
        Periksa(SistemMisiHarian.SemuaId().Length == 8, "kolam misi harian 8 misi");
        var h1 = SistemMisiHarian.PilihUntukHari(1);
        Periksa(h1.Length == 3, "hari 1: 3 misi");
        Periksa(h1[0] == SistemMisiHarian.Bangun1 && h1[1] == SistemMisiHarian.Taman1
            && h1[2] == SistemMisiHarian.Bahagia65, "hari 1: bangun, taman, bahagia65");
        var h2 = SistemMisiHarian.PilihUntukHari(2);
        Periksa(h2[0] == SistemMisiHarian.Pemasukan50rb
            && h2[1] == SistemMisiHarian.Warga15
            && h2[2] == SistemMisiHarian.Komersial3,
            "hari 2: pemasukan, warga15, komersial3");
        var h3 = SistemMisiHarian.PilihUntukHari(3);
        Periksa(h3[0] == SistemMisiHarian.Bahagia75
            && h3[1] == SistemMisiHarian.Taman5
            && h3[2] == SistemMisiHarian.Bangun1,
            "hari 3: bahagia75, taman5, bangun1 (melilit)");
        var h4 = SistemMisiHarian.PilihUntukHari(4);
        Periksa(h4[0] == SistemMisiHarian.Taman1
            && h4[1] == SistemMisiHarian.Bahagia65
            && h4[2] == SistemMisiHarian.Pemasukan50rb,
            "hari 4: taman1, bahagia65, pemasukan");
        // Deterministik: hari sama -> set sama; tanpa duplikat.
        var h5a = SistemMisiHarian.PilihUntukHari(5);
        var h5b = SistemMisiHarian.PilihUntukHari(5);
        Periksa(h5a[0] == h5b[0] && h5a[1] == h5b[1] && h5a[2] == h5b[2],
            "hari sama dipanggil 2x -> set sama");
        Periksa(h5a[0] != h5a[1] && h5a[1] != h5a[2] && h5a[0] != h5a[2],
            "satu hari: 3 misi berbeda (tanpa duplikat)");
        var h9 = SistemMisiHarian.PilihUntukHari(9);
        Periksa(h9[0] == h1[0] && h9[1] == h1[1] && h9[2] == h1[2],
            "siklus 8 hari: hari 9 = hari 1");
        // ApakahSelesai tiap misi (batas bawah/atas).
        var kosong = new SistemMisiHarian.KeadaanHarian();
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Bangun1, kosong),
            "bangun1: 0 dibangun belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Bangun1,
            new SistemMisiHarian.KeadaanHarian { DibangunHariIni = 1 }),
            "bangun1: 1 dibangun selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Taman1, kosong),
            "taman1: 0 taman belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Taman1,
            new SistemMisiHarian.KeadaanHarian { TamanHariIni = 1 }),
            "taman1: 1 taman selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Bahagia65,
            new SistemMisiHarian.KeadaanHarian { Kebahagiaan = 64 }),
            "bahagia65: 64 belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Bahagia65,
            new SistemMisiHarian.KeadaanHarian { Kebahagiaan = 65 }),
            "bahagia65: 65 selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Pemasukan50rb,
            new SistemMisiHarian.KeadaanHarian { PerkiraanPemasukanHarian = 49999 }),
            "pemasukan50rb: 49999 belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Pemasukan50rb,
            new SistemMisiHarian.KeadaanHarian { PerkiraanPemasukanHarian = 50000 }),
            "pemasukan50rb: 50000 selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Warga15,
            new SistemMisiHarian.KeadaanHarian { Populasi = 14 }),
            "warga15: 14 belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Warga15,
            new SistemMisiHarian.KeadaanHarian { Populasi = 15 }),
            "warga15: 15 selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Komersial3,
            new SistemMisiHarian.KeadaanHarian { JumlahKomersial = 2 }),
            "komersial3: 2 belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Komersial3,
            new SistemMisiHarian.KeadaanHarian { JumlahKomersial = 3 }),
            "komersial3: 3 selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Bahagia75,
            new SistemMisiHarian.KeadaanHarian { Kebahagiaan = 74 }),
            "bahagia75: 74 belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Bahagia75,
            new SistemMisiHarian.KeadaanHarian { Kebahagiaan = 75 }),
            "bahagia75: 75 selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Taman5,
            new SistemMisiHarian.KeadaanHarian { JumlahTaman = 4 }),
            "taman5: 4 belum selesai");
        Periksa(SistemMisiHarian.ApakahSelesai(SistemMisiHarian.Taman5,
            new SistemMisiHarian.KeadaanHarian { JumlahTaman = 5 }),
            "taman5: 5 selesai");
        Periksa(!SistemMisiHarian.ApakahSelesai("tidak_ada", kosong),
            "misi tidak dikenal: tidak selesai");
        // Hadiah.
        Periksa(SistemMisiHarian.HadiahKoin(SistemMisiHarian.Bangun1) == 25000,
            "hadiah bangun1 Rp25.000");
        Periksa(SistemMisiHarian.HadiahKoin(SistemMisiHarian.Komersial3) == 40000,
            "hadiah komersial3 Rp40.000");
        Periksa(SistemMisiHarian.HadiahKoin(SistemMisiHarian.Bahagia75) == 0,
            "hadiah bahagia75 bukan koin");
        Periksa(SistemMisiHarian.HadiahPermata(SistemMisiHarian.Bahagia75) == 1,
            "hadiah bahagia75 1 Permata");
        Periksa(SistemMisiHarian.TeksHadiah(SistemMisiHarian.Bahagia75) == "1 Permata",
            "teks hadiah bahagia75 = 1 Permata");
        Periksa(SistemMisiHarian.TeksHadiah(SistemMisiHarian.Bangun1) == "Rp25.000",
            "teks hadiah bangun1 = Rp25.000");
        Periksa(SistemMisiHarian.KunciKlaim(3, SistemMisiHarian.Bangun1)
            == "DK_Harian_3_harian_bangun1", "kunci klaim hari 3");
        Periksa(SistemMisiHarian.KunciKlaim(3, SistemMisiHarian.Bangun1)
            != SistemMisiHarian.KunciKlaim(4, SistemMisiHarian.Bangun1),
            "kunci klaim beda hari beda kunci (misi hangus saat ganti hari)");

        Console.WriteLine("== Tes H: simulasi 10 hari dunia standar Fase 4 ==");
        // Kebahagiaan dunia standar = 63 (pengali warung 100%).
        // Harian normal: komersial 130000 + kost 40000 = 170000 kotor,
        // pajak 4250, upkeep 29000 (2000+5000+2000+4000+8000+5000
        // +1000+2000), bersih 136750.
        var s1 = EconomyCalculator.HitungHarian(DuniaStandarFase4(false), 63, 100);
        Periksa(s1.PemasukanWarung == 130000, "standar: komersial 130000");
        Periksa(s1.PemasukanLain == 40000, "standar: sewa kost 40000");
        Periksa(s1.Pemasukan == 170000, "standar: kotor 170000");
        Periksa(s1.Pajak == 4250, "standar: pajak 4250");
        Periksa(s1.Upkeep == 29000, "standar: upkeep 29000");
        Periksa(s1.Bersih == 136750, "standar: bersih 136750");
        long koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaStandarFase4(false), 63, 100).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 136750,
            "10 hari normal: 6367500 (dapat " + koin + ")");
        // Tanpa jalan: komersial 65000 + kost 20000 = 85000 kotor,
        // pajak 2125, bersih 53875.
        var s2 = EconomyCalculator.HitungHarian(DuniaStandarFase4(true), 63, 100);
        Periksa(s2.Bersih == 53875, "standar tanpa jalan: bersih 53875");
        koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaStandarFase4(true), 63, 100).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 53875,
            "10 hari tanpa jalan: 5538750 (dapat " + koin + ")");
        // Hujan terus (80) dengan jalan: komersial 104000, bersih 111400.
        koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaStandarFase4(false), 63, 80).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 111400,
            "10 hari hujan terus: 6114000 (dapat " + koin + ")");

        Console.WriteLine("== Tes I: migrasi simpanan 4.0 -> 5.0 ==");
        Periksa(SaveData.VersiDidukung("5.1"), "versi 5.1 didukung (naik Balai Kota)");
        Periksa(SaveData.VersiDidukung("5.0"), "versi 5.0 didukung (naik di Fase 4B)");
        Periksa(SaveData.VersiDidukung("4.0"), "versi 4.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("3.0"), "versi 3.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("2.0"), "versi 2.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("1.0"), "versi 1.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("6.0"), "versi 6.0 didukung (naik di Fase 7)");
        Periksa(!SaveData.VersiDidukung("7.0"), "versi 7.0 ditolak");
        Periksa(!SaveData.VersiDidukung(""), "versi kosong ditolak");
        Periksa(!SaveData.VersiDidukung(null), "versi null ditolak");
        Periksa(SaveData.UkuranGridEfektif(0) == 12, "ukuran grid 0 -> 12 (simpanan lama)");
        Periksa(SaveData.UkuranGridEfektif(-3) == 12, "ukuran grid negatif -> 12");
        Periksa(SaveData.UkuranGridEfektif(12) == 12, "ukuran grid 12 tetap 12");
        Periksa(SaveData.UkuranGridEfektif(16) == 16, "ukuran grid 16 tetap 16");
        var baru = new SaveData();
        Periksa(baru.versi == "6.0", "simpanan baru versi 6.0");
        Periksa(baru.ukuranGrid == 0, "simpanan baru ukuranGrid 0 (normal jadi 12 saat muat)");
        Periksa(baru.pencapaianDiklaim != null && baru.pencapaianDiklaim.Count == 0,
            "simpanan baru pencapaianDiklaim kosong (bukan null)");
        Periksa(baru.hariMisiTerakhir == 0 && baru.dibangunHariIni == 0
            && baru.tamanHariIni == 0, "simpanan baru penghitung harian 0");
        var lama = new SaveData();
        lama.versi = "4.0";
        Periksa(lama.ukuranGrid == 0 && lama.pencapaianDiklaim.Count == 0,
            "simpanan 4.0: field Fase 4B default aman (reputasi 0, interior kosong)");
        Periksa(lama.reputasi == 0 && lama.interior.Count == 0,
            "simpanan 4.0: reputasi 0 + interior kosong (GameManager menormalkan 0 -> 50)");
        Periksa(GameBalance.VersiSimpan == "6.0", "VersiSimpan = 6.0");
        Periksa(GameBalance.HargaPerluasanLahan == 5000000,
            "harga perluasan lahan Rp5.000.000");
        Periksa(GameBalance.UkuranGridDiperluas == 16, "ukuran grid diperluas 16");
        Periksa(GameBalance.PengaliTanpaJalan == 50, "pengali tanpa jalan 50%");

        Console.WriteLine("== Tes J: matematika perluasan grid 12x12 -> 16x16 ==");
        var grid = new IsometricGrid();
        Periksa(grid.lebar == 12 && grid.tinggi == 12, "grid awal 12x12");
        float minX12, maxX12, minY12, maxY12;
        grid.BatasDunia(out minX12, out maxX12, out minY12, out maxY12);
        grid.AturUkuran(16, 16);
        Periksa(grid.lebar == 16 && grid.tinggi == 16, "sesudah AturUkuran(16,16): 16x16");
        // Pusat grid 16x16 tetap di (0,0) dunia: petak (0,0) = (0,-7.5).
        var awal16 = grid.GridKeDunia(new Vector2Int(0, 0));
        Periksa(Math.Abs(awal16.x - 0f) < 0.001f
            && Math.Abs(awal16.y - (-7.5f)) < 0.001f,
            "pusat 16x16: petak (0,0) = (0, -7.5), dapat (" + awal16.x + ", " + awal16.y + ")");
        // Round-trip di grid 16x16 (termasuk petak paling ujung).
        var bulat = grid.DuniaKeGrid(grid.GridKeDunia(new Vector2Int(7, 3)));
        Periksa(bulat.x == 7 && bulat.y == 3, "round-trip petak (7,3) tetap (7,3)");
        var ujung = grid.DuniaKeGrid(grid.GridKeDunia(new Vector2Int(15, 15)));
        Periksa(ujung.x == 15 && ujung.y == 15, "round-trip petak (15,15) tetap (15,15)");
        var nol = grid.DuniaKeGrid(grid.GridKeDunia(new Vector2Int(0, 0)));
        Periksa(nol.x == 0 && nol.y == 0, "round-trip petak (0,0) tetap (0,0)");
        // Batas dunia 16x16 lebih lebar dari 12x12.
        float minX16, maxX16, minY16, maxY16;
        grid.BatasDunia(out minX16, out maxX16, out minY16, out maxY16);
        Periksa(maxX16 - minX16 > maxX12 - minX12,
            "lebar dunia 16x16 (" + (maxX16 - minX16) + ") > 12x12 (" + (maxX12 - minX12) + ")");
        Periksa(maxY16 - minY16 > maxY12 - minY12,
            "tinggi dunia 16x16 (" + (maxY16 - minY16) + ") > 12x12 (" + (maxY12 - minY12) + ")");
        Periksa(Math.Abs(minX16 - (-17f)) < 0.001f && Math.Abs(maxX16 - 17f) < 0.001f,
            "batas X 16x16: -17..17");
        Periksa(Math.Abs(minY16 - (-8.5f)) < 0.001f && Math.Abs(maxY16 - 8.5f) < 0.001f,
            "batas Y 16x16: -8.5..8.5");
        Periksa(grid.DiDalamGrid(new Vector2Int(15, 15)), "petak (15,15) di dalam grid 16x16");
        Periksa(!grid.DiDalamGrid(new Vector2Int(16, 0)), "petak (16,0) di luar grid 16x16");
        // AturUkuran menolak ukuran tidak valid (tetap 16x16).
        grid.AturUkuran(0, 0);
        Periksa(grid.lebar == 16 && grid.tinggi == 16, "AturUkuran(0,0) ditolak");
        grid.AturUkuran(-1, 5);
        Periksa(grid.lebar == 16 && grid.tinggi == 16, "AturUkuran(-1,5) ditolak");

        Console.WriteLine("\nHasil Fase 4: " + lolos + " lolos, " + gagal + " gagal.");
        return gagal == 0 ? 0 : 1;
    }
}
