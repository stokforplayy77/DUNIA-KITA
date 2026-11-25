// Dunia Kita — tools/test-fase3.cs
// Test harness logika murni Fase 3 (tanpa Unity): dikompilasi dengan
// mcs bersama EconomyCalculator.cs, FormatUtil.cs, GameBalance.cs, SaveData.cs,
// ProgresiDunia.cs, SistemKejadian.cs, IsometricGrid.cs, PewaktuHari.cs dan
// unity-stubs.cs. Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji: syarat naik level dunia 1-3 (GDD §8), unlock bangunan
// per level, roll + efek tiap kejadian acak (GDD §9), ekonomi dengan
// pengali hujan + Toko Kelontong + sewa Kost, titik tengah bangunan
// 2x2 di grid, simulasi 10 hari dengan angka persis, dan migrasi
// simpanan Fase 2 -> Fase 3, dan pewaktu hari otomatis (Fase 3
// revisi desain: 1 hari game = 5 menit waktu nyata, tanpa tombol
// "Lewati Hari").

using System;
using System.Collections.Generic;
using UnityEngine;

public static class TestFase3
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
            // Warung dan Toko Kelontong sama-sama toko: pemasukan
            // dipengaruhi kebahagiaan + hujan (lihat EconomyManager).
            AdalahWarung = id == "warung" || id == "toko_kelontong"
        };
    }

    // Dunia Fase 3: 5 rumah + 1 kost + 1 warung + 1 toko + 2 taman.
    // Populasi 18, kapasitas 18, kebahagiaan 50+4+5 = 59.
    private static List<EconomyCalculator.CatatanBangunan> DuniaFase3()
    {
        var daftar = new List<EconomyCalculator.CatatanBangunan>();
        for (int i = 0; i < 5; i++)
        {
            daftar.Add(Catatan("rumah", 0, 1000, 2, false));
        }
        daftar.Add(Catatan("kost", 40000, 5000, 8, false));
        daftar.Add(Catatan("warung", 15000, 2000, 0, false));
        daftar.Add(Catatan("toko_kelontong", 30000, 4000, 0, false));
        daftar.Add(Catatan("taman", 0, 500, 0, true));
        daftar.Add(Catatan("taman", 0, 500, 0, true));
        return daftar;
    }

    public static int Main()
    {
        Console.WriteLine("== Tes A: syarat naik level (GDD §8) ==");
        Periksa(ProgresiDunia.SyaratPopulasi(2) == 10, "L2 butuh 10 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(3) == 30, "L3 butuh 30 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(4) == 80, "L4 butuh 80 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(5) == 150, "L5 butuh 150 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(6) == 300, "L6 butuh 300 warga");
        Periksa(ProgresiDunia.SyaratBahagia(2) == 0, "L2 tanpa syarat bahagia");
        Periksa(ProgresiDunia.SyaratBahagia(3) == 60, "L3 butuh bahagia 60");
        Periksa(ProgresiDunia.SyaratBahagia(4) == 70, "L4 butuh bahagia 70");
        Periksa(ProgresiDunia.SyaratBahagia(5) == 75, "L5 butuh bahagia 75");
        Periksa(ProgresiDunia.SyaratBahagia(6) == 80, "L6 butuh bahagia 80");
        Periksa(ProgresiDunia.NamaLevel(1) == "Tanah Kosong", "nama L1");
        Periksa(ProgresiDunia.NamaLevel(2) == "Dusun", "nama L2");
        Periksa(ProgresiDunia.NamaLevel(3) == "Desa", "nama L3");
        Periksa(ProgresiDunia.NamaLevel(4) == "Kota Kecil", "nama L4");
        Periksa(ProgresiDunia.NamaLevel(5) == "Kota", "nama L5");
        Periksa(ProgresiDunia.NamaLevel(6) == "Metropolis", "nama L6");
        Periksa(ProgresiDunia.HadiahNaikLevel(2) == 100000, "hadiah L2 Rp100.000");
        Periksa(ProgresiDunia.HadiahNaikLevel(3) == 250000, "hadiah L3 Rp250.000");
        // FASE 4: hadiah L4-L6 kini dibuka (sebelumnya 0 karena
        // LevelMaksFase3 = 3 — batas sementara Fase 3, bukan kerusakan).
        Periksa(ProgresiDunia.HadiahNaikLevel(4) == 500000, "hadiah L4 Rp500.000");
        Periksa(ProgresiDunia.HadiahNaikLevel(5) == 1000000, "hadiah L5 Rp1.000.000");
        Periksa(ProgresiDunia.HadiahNaikLevel(6) == 2000000, "hadiah L6 Rp2.000.000");

        Console.WriteLine("== Tes B: bisa naik level di batasnya ==");
        Periksa(ProgresiDunia.BisaNaikKe(2, 10, 0), "L2: 10 warga cukup");
        Periksa(!ProgresiDunia.BisaNaikKe(2, 9, 100), "L2: 9 warga kurang");
        Periksa(ProgresiDunia.BisaNaikKe(2, 10, 50), "L2: bahagia tidak disyaratkan");
        Periksa(ProgresiDunia.BisaNaikKe(3, 30, 60), "L3: 30 warga + bahagia 60 cukup");
        Periksa(!ProgresiDunia.BisaNaikKe(3, 30, 59), "L3: bahagia 59 kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(3, 29, 60), "L3: 29 warga kurang");
        Periksa(ProgresiDunia.BisaNaikKe(3, 100, 100), "L3: 100 warga + bahagia 100 cukup");
        // FASE 4: L4 kini diizinkan (sebelumnya ditolak LevelMaksFase3 = 3).
        Periksa(ProgresiDunia.BisaNaikKe(4, 80, 70), "L4: 80 warga + bahagia 70 cukup");
        Periksa(!ProgresiDunia.BisaNaikKe(4, 79, 70), "L4: 79 warga kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(4, 80, 69), "L4: bahagia 69 kurang");
        Periksa(!ProgresiDunia.BisaNaikKe(1, 999, 100), "tidak bisa naik ke L1");
        Periksa(!ProgresiDunia.BisaNaikKe(7, 999, 100), "tidak bisa naik ke L7");
        Periksa(GameBalance.LevelMaksDunia == 6, "LevelMaksDunia = 6");

        Console.WriteLine("== Tes C: unlock bangunan per level ==");
        Periksa(ProgresiDunia.ApakahTerbuka(1, 1), "rumah terbuka di L1");
        Periksa(!ProgresiDunia.ApakahTerbuka(2, 1), "kost terkunci di L1");
        Periksa(ProgresiDunia.ApakahTerbuka(2, 2), "kost terbuka di L2");
        Periksa(!ProgresiDunia.ApakahTerbuka(3, 2), "toko terkunci di L2");
        Periksa(ProgresiDunia.ApakahTerbuka(3, 3), "toko terbuka di L3");
        Periksa(ProgresiDunia.ApakahTerbuka(0, 1),
            "levelBuka 0 diperlakukan 1 (aman untuk .asset lama)");
        Periksa(ProgresiDunia.TeksSyarat(2) == "10 warga", "teks syarat L2");
        Periksa(ProgresiDunia.TeksSyarat(3) == "30 warga + kebahagiaan 60%",
            "teks syarat L3");

        Console.WriteLine("== Tes D: roll kejadian acak (peluang 20%) ==");
        Periksa(SistemKejadian.AcakKejadian(0.0) == SistemKejadian.Festival,
            "roll 0,00 -> festival");
        Periksa(SistemKejadian.AcakKejadian(0.03) == SistemKejadian.Festival,
            "roll 0,03 -> festival");
        Periksa(SistemKejadian.AcakKejadian(0.07) == SistemKejadian.Hujan,
            "roll 0,07 -> hujan");
        Periksa(SistemKejadian.AcakKejadian(0.13) == SistemKejadian.Hujan,
            "roll 0,13 -> hujan");
        Periksa(SistemKejadian.AcakKejadian(0.14) == SistemKejadian.Pedagang,
            "roll 0,14 -> pedagang");
        Periksa(SistemKejadian.AcakKejadian(0.199) == SistemKejadian.Pedagang,
            "roll 0,199 -> pedagang");
        Periksa(SistemKejadian.AcakKejadian(0.2) == SistemKejadian.TidakAda,
            "roll 0,20 -> tidak ada kejadian (batas peluang)");
        Periksa(SistemKejadian.AcakKejadian(0.5) == SistemKejadian.TidakAda,
            "roll 0,50 -> tidak ada kejadian");
        Periksa(SistemKejadian.AcakKejadian(-0.1) == SistemKejadian.TidakAda,
            "roll negatif -> tidak ada kejadian");
        Periksa(GameBalance.PeluangKejadianPerHari == 0.20, "peluang 20%");

        Console.WriteLine("== Tes E: teks + biaya tiap kejadian ==");
        Periksa(SistemKejadian.Judul(SistemKejadian.Festival) == "Festival Desa",
            "judul festival");
        Periksa(SistemKejadian.Judul(SistemKejadian.Hujan) == "Hujan Deras",
            "judul hujan");
        Periksa(SistemKejadian.Judul(SistemKejadian.Pedagang) == "Pedagang Keliling",
            "judul pedagang");
        Periksa(SistemKejadian.Judul("aneh") == "", "judul id tidak dikenal kosong");
        Periksa(SistemKejadian.Deskripsi(SistemKejadian.Festival).Length > 20,
            "deskripsi festival terisi");
        Periksa(SistemKejadian.Deskripsi(SistemKejadian.Hujan).Length > 20,
            "deskripsi hujan terisi");
        Periksa(SistemKejadian.Deskripsi(SistemKejadian.Pedagang).Length > 20,
            "deskripsi pedagang terisi");
        Periksa(SistemKejadian.LabelPilihan1(SistemKejadian.Festival).Contains("Rp50.000"),
            "label festival memuat biaya Rp50.000");
        Periksa(SistemKejadian.LabelPilihan2(SistemKejadian.Festival) == "Tidak dulu",
            "label tolak festival");
        Periksa(SistemKejadian.LabelPilihan2(SistemKejadian.Hujan) == "Biarkan saja",
            "label biarkan hujan");
        Periksa(SistemKejadian.LabelPilihan2(SistemKejadian.Pedagang) == "Tolak",
            "label tolak pedagang");
        Periksa(SistemKejadian.Biaya(SistemKejadian.Festival, 0) == 50000,
            "biaya festival Rp50.000");
        Periksa(SistemKejadian.Biaya(SistemKejadian.Festival, 1) == 0,
            "tolak festival gratis");
        Periksa(SistemKejadian.Biaya(SistemKejadian.Hujan, 0) == 20000,
            "biaya payung Rp20.000");
        Periksa(SistemKejadian.Biaya(SistemKejadian.Pedagang, 0) == 100000,
            "harga pedagang Rp100.000");
        Periksa(SistemKejadian.Biaya("aneh", 0) == 0, "biaya id tidak dikenal 0");

        Console.WriteLine("== Tes F: efek pilihan kejadian ==");
        var k = SistemKejadian.KeadaanAwal();
        Periksa(k.BonusBahagiaBesok == 0 && k.PengaliHujanBesok == 100,
            "keadaan awal: bonus 0, hujan 100");
        SistemKejadian.TerapkanPilihan(SistemKejadian.Festival, 0, ref k);
        Periksa(k.BonusBahagiaBesok == 10, "festival diadakan: bonus bahagia +10");
        k = SistemKejadian.KeadaanAwal();
        SistemKejadian.TerapkanPilihan(SistemKejadian.Festival, 1, ref k);
        Periksa(k.BonusBahagiaBesok == 0, "festival ditolak: bonus tetap 0");
        k = SistemKejadian.KeadaanAwal();
        SistemKejadian.TerapkanPilihan(SistemKejadian.Hujan, 0, ref k);
        Periksa(k.PengaliHujanBesok == 90, "hujan + payung: pengali 90 (-10%)");
        k = SistemKejadian.KeadaanAwal();
        SistemKejadian.TerapkanPilihan(SistemKejadian.Hujan, 1, ref k);
        Periksa(k.PengaliHujanBesok == 80, "hujan dibiarkan: pengali 80 (-20%)");
        k = SistemKejadian.KeadaanAwal();
        SistemKejadian.TerapkanPilihan(SistemKejadian.Pedagang, 0, ref k);
        Periksa(k.BonusBahagiaBesok == 0 && k.PengaliHujanBesok == 100,
            "pedagang: tidak mengubah keadaan (permata ditangani GameManager)");
        Periksa(SistemKejadian.TeksHasil(SistemKejadian.Festival, 0, true)
            .Contains("+10"), "teks hasil festival memuat +10");
        Periksa(SistemKejadian.TeksHasil(SistemKejadian.Hujan, 1, true)
            .Contains("-20%"), "teks hasil hujan dibiarkan memuat -20%");
        Periksa(SistemKejadian.TeksHasil(SistemKejadian.Pedagang, 0, true)
            .Contains("Permata"), "teks hasil pedagang memuat Permata");
        Periksa(SistemKejadian.TeksHasil(SistemKejadian.Festival, 0, false)
            .Contains("tidak cukup"), "teks gagal bayar memuat 'tidak cukup'");

        Console.WriteLine("== Tes G: ekonomi Fase 3 (hujan + toko + sewa kost) ==");
        var dunia = DuniaFase3();
        Periksa(EconomyCalculator.HitungPopulasi(dunia) == 18,
            "populasi 18 (5 rumah x 2 + kost 8)");
        Periksa(EconomyCalculator.HitungKebahagiaan(2, 18, 18, 0) == 59,
            "kebahagiaan 59 (50+4 taman+5 semua punya rumah)");
        var r = EconomyCalculator.HitungHarian(dunia, 59, 100);
        Periksa(r.PemasukanWarung == 45000, "warung+toko 45000 (hujan normal)");
        Periksa(r.PemasukanLain == 40000, "sewa kost 40000 (pemasukan lain)");
        Periksa(r.Pemasukan == 85000, "pemasukan kotor 85000");
        Periksa(r.Pajak == 2125, "pajak 2125 (2,5% x 85000)");
        Periksa(r.Upkeep == 17000, "upkeep 17000");
        Periksa(r.Bersih == 65875, "bersih 65875 (85000 - 2125 - 17000)");
        r = EconomyCalculator.HitungHarian(dunia, 59, 80);
        Periksa(r.PemasukanWarung == 36000, "hujan tanpa payung: warung+toko 36000");
        Periksa(r.PemasukanLain == 40000, "hujan: sewa kost tetap 40000");
        Periksa(r.Pajak == 1900, "pajak 1900 (2,5% x 76000)");
        Periksa(r.Bersih == 57100, "bersih hujan 57100");
        r = EconomyCalculator.HitungHarian(dunia, 59, 90);
        Periksa(r.PemasukanWarung == 40500, "hujan + payung: warung+toko 40500");
        Periksa(r.Pajak == 2012, "pajak 2012 (2,5% x 80500, dibulatkan ke bawah)");
        Periksa(r.Bersih == 61488, "bersih hujan+payung 61488");
        var r2 = EconomyCalculator.HitungHarian(dunia, 59);
        Periksa(r2.Bersih == 65875, "overload 2-arg = hujan normal 100");
        var r3 = EconomyCalculator.HitungHarian(dunia);
        Periksa(r3.PemasukanWarung == 45000, "overload 1-arg: bahagia 50 -> pengali 100%");
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 4, 4, 0, 10) == 67,
            "bonus festival: 57 + 10 = 67");
        Periksa(EconomyCalculator.HitungKebahagiaan(15, 4, 4, 0, 10) == 85,
            "bonus festival: 75 + 10 = 85");
        Periksa(EconomyCalculator.HitungKebahagiaan(15, 4, 4, 0, 50) == 100,
            "bonus festival dibatasi maks 100");
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 4, 4, 0) == 57,
            "overload 4-arg tanpa bonus tetap 57");

        Console.WriteLine("== Tes H: titik tengah bangunan 2x2 di grid ==");
        var grid = new IsometricGrid();
        var tengah1 = grid.GridKeDuniaTengah(new Vector2Int(0, 0), new Vector2Int(1, 1));
        var pusat1 = grid.GridKeDunia(new Vector2Int(0, 0));
        Periksa(Math.Abs(tengah1.x - pusat1.x) < 0.001f
            && Math.Abs(tengah1.y - pusat1.y) < 0.001f,
            "tengah 1x1 = tengah petak (" + pusat1.x + ", " + pusat1.y + ")");
        var tengah2 = grid.GridKeDuniaTengah(new Vector2Int(0, 0), new Vector2Int(2, 2));
        // Sudut (0,0)=(0,-5.5) (1,0)=(1,-5.0) (0,1)=(-1,-5.0) (1,1)=(0,-4.5):
        // rata-rata = (0, -5.0).
        Periksa(Math.Abs(tengah2.x - 0f) < 0.001f
            && Math.Abs(tengah2.y - (-5.0f)) < 0.001f,
            "tengah 2x2 di (0,0) = (0, -5.0), dapat (" + tengah2.x + ", " + tengah2.y + ")");
        var tengah3 = grid.GridKeDuniaTengah(new Vector2Int(5, 5), new Vector2Int(2, 2));
        // Sudut (5,5)=(0,-0.5) (6,5)=(1,0) (5,6)=(-1,0) (6,6)=(0,0.5):
        // rata-rata = (0, 0).
        Periksa(Math.Abs(tengah3.x - 0f) < 0.001f
            && Math.Abs(tengah3.y - 0f) < 0.001f,
            "tengah 2x2 di (5,5) = (0, 0), dapat (" + tengah3.x + ", " + tengah3.y + ")");
        var sudut = grid.GridKeDunia(new Vector2Int(6, 6));
        Periksa(Math.Abs(sudut.x - 0f) < 0.001f
            && Math.Abs(sudut.y - 0.5f) < 0.001f,
            "sudut depan (6,6) = (0, 0.5) — tengah 2x2 berada di belakangnya");
        var bulat = grid.DuniaKeGrid(grid.GridKeDunia(new Vector2Int(7, 3)));
        Periksa(bulat.x == 7 && bulat.y == 3, "round-trip petak (7,3) tetap (7,3)");

        Console.WriteLine("== Tes I: simulasi 10 hari Fase 3 (angka persis) ==");
        long koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaFase3(), 59, 100).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 65875,
            "10 hari normal: 5658750 (dapat " + koin + ")");
        koin = GameBalance.KoinAwal;
        for (int hari = 0; hari < 10; hari++)
        {
            koin += EconomyCalculator.HitungHarian(DuniaFase3(), 59, 80).Bersih;
        }
        Periksa(koin == 5000000 + 10 * 57100,
            "10 hari hujan terus: 5571000 (dapat " + koin + ")");
        // Hadiah naik level: L2 Rp100.000 + L3 Rp250.000 = Rp350.000.
        Periksa(ProgresiDunia.HadiahNaikLevel(2) + ProgresiDunia.HadiahNaikLevel(3)
            == 350000, "total hadiah naik level L2+L3 = Rp350.000");
        Periksa(GameBalance.HadiahMisiNaikLevel2 == 50000, "hadiah misi L2 Rp50.000");
        Periksa(GameBalance.HadiahMisiBangunKost == 75000, "hadiah misi kost Rp75.000");
        Periksa(GameBalance.HadiahMisiCapai10Warga == 50000, "hadiah misi 10 warga Rp50.000");

        Console.WriteLine("== Tes J: migrasi simpanan Fase 2 -> Fase 3 ==");
        Periksa(SaveData.VersiDidukung("5.1"), "versi 5.1 didukung (naik Balai Kota)");
        Periksa(SaveData.VersiDidukung("5.0"), "versi 5.0 didukung (naik di Fase 4B)");
        Periksa(SaveData.VersiDidukung("4.0"), "versi 4.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("3.0"), "versi 3.0 didukung");
        Periksa(SaveData.VersiDidukung("2.0"), "versi 2.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("1.0"), "versi 1.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("6.0"), "versi 6.0 didukung (naik di Fase 7)");
        Periksa(!SaveData.VersiDidukung("7.0"), "versi 7.0 ditolak");
        Periksa(!SaveData.VersiDidukung(""), "versi kosong ditolak");
        Periksa(!SaveData.VersiDidukung(null), "versi null ditolak");
        var lama = new SaveData();
        lama.versi = "2.0";
        Periksa(lama.bonusBahagiaBesok == 0 && lama.pengaliHujanBesok == 0,
            "simpanan 2.0: field Fase 3 default 0 (GameManager menormalkan hujan 0 -> 100)");
        var baru = new SaveData();
        Periksa(baru.versi == "6.0", "simpanan baru berversi 6.0 (dulu 5.1 — naik Dunia Bersama)");
        Periksa(GameBalance.VersiSimpan == "6.0", "GameBalance.VersiSimpan = 6.0");
        Periksa(GameBalance.VersiSimpanFase4B == "5.0", "GameBalance.VersiSimpanFase4B = 5.0");
        Periksa(GameBalance.VersiSimpanFase4 == "4.0", "GameBalance.VersiSimpanFase4 = 4.0");
        Periksa(GameBalance.VersiSimpanFase3 == "3.0", "GameBalance.VersiSimpanFase3 = 3.0");
        Periksa(GameBalance.VersiSimpanFase2 == "2.0", "GameBalance.VersiSimpanFase2 = 2.0");
        Periksa(GameBalance.VersiSimpanLama == "1.0", "GameBalance.VersiSimpanLama = 1.0");

        Console.WriteLine("== Tes K: pewaktu hari otomatis (revisi desain) ==");
        Periksa(GameBalance.DurasiHariDetik == 300f, "durasi 1 hari game = 300 detik");
        Periksa(GameBalance.IntervalSimpanOtomatisDetik == 60f,
            "interval simpan otomatis = 60 detik");
        var jam = new PewaktuHari();
        Periksa(Math.Abs(jam.SisaDetik - 300f) < 0.001f, "pewaktu baru: sisa 300 detik");
        Periksa(jam.TeksSisaWaktu() == "05:00", "teks sisa awal 05:00");
        Periksa(!jam.Tambah(299f), "tambah 299 detik: belum berganti hari");
        Periksa(Math.Abs(jam.SisaDetik - 1f) < 0.001f, "sisa 1 detik sesudah 299 detik");
        Periksa(jam.TeksSisaWaktu() == "00:01", "teks sisa 00:01");
        Periksa(jam.Tambah(1f), "tambah 1 detik lagi: hari berganti");
        Periksa(Math.Abs(jam.SisaDetik - 300f) < 0.001f, "pewaktu reset ke 300 detik");
        Periksa(jam.TeksSisaWaktu() == "05:00", "teks sisa kembali 05:00");
        var jam2 = new PewaktuHari();
        jam2.Tambah(108f);
        Periksa(jam2.TeksSisaWaktu() == "03:12", "sisa 192 detik -> 03:12");
        var jam3 = new PewaktuHari();
        Periksa(jam3.Tambah(600f), "tambah 600 detik sekaligus: tetap 1 hari berganti");
        Periksa(Math.Abs(jam3.SisaDetik - 300f) < 0.001f,
            "kelebihan waktu dibuang (tidak ada lonjakan hari)");
        var jam4 = new PewaktuHari();
        Periksa(!jam4.Tambah(-10f), "tambah negatif diabaikan (tidak berganti hari)");
        Periksa(Math.Abs(jam4.SisaDetik - 300f) < 0.001f, "sisa tetap 300 detik");

        Console.WriteLine();
        Console.WriteLine("Hasil: " + lolos + " lolos, " + gagal + " gagal.");
        return gagal == 0 ? 0 : 1;
    }
}
