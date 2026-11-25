// Dunia Kita — tools/test-fase6.cs
// Test harness QA akhir (Alpha) Fase 6 (tanpa Unity): dikompilasi dengan
// mcs bersama EconomyCalculator.cs, FormatUtil.cs, GameBalance.cs, SaveData.cs,
// SistemInterior.cs, SistemBalaiKota.cs, ProgresiDunia.cs,
// SistemPencapaian.cs, SistemMisiHarian.cs dan unity-stubs.cs.
// Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji (konsistensi rilis — angka yang harus saling cocok):
//   A. Rantai migrasi simpanan: versi 1.0-6.0 didukung, versi lain ditolak;
//      ukuran grid efektif simpanan lama/rusak -> 12.
//   B. Versi game & rilis: VersiSimpan "6.0", VersiGame "1.0", hadiah
//      naik level naik terus, perluasan lahan 12 -> 16.
//   C. Balai Kota silang GameBalance: batas 1, reputasi +2/hari,
//      terbuka Level 3 (lihat juga test-balaikota.cs untuk angka .asset).
//   D. Progresi dunia: syarat populasi & bahagia naik terus tiap level,
//      nama level 1-6 berbeda dan terisi.
//   E. Pencapaian: 6 id, hadiah Permata tiap id > 0, total 26 Permata.
//   F. Misi harian: 8 id, 3 misi/hari deterministik, tiap misi punya
//      hadiah koin atau Permata.
//   G. Ekonomi dasar: harga jual = 50% biaya bangun; pengali warung
//      80/100/110 menurut kebahagiaan.
//   H. Interior: 10 slot, total harga perabot Rp1.420.000, total bonus
//      +16, kosong vs lengkap, batas bonus total.

using System;
using System.Collections.Generic;

public static class TesFase6
{
    private static int lolos = 0;
    private static int gagal = 0;

    private static void Periksa(bool kondisi, string pesan)
    {
        if (kondisi)
        {
            lolos++;
        }
        else
        {
            gagal++;
            Console.WriteLine("GAGAL: " + pesan);
        }
    }

    public static void Main()
    {
        Console.WriteLine("== Tes A: rantai migrasi simpanan 1.0-6.0 ==");
        Periksa(SaveData.VersiDidukung("1.0"), "simpanan 1.0 didukung");
        Periksa(SaveData.VersiDidukung("2.0"), "simpanan 2.0 didukung");
        Periksa(SaveData.VersiDidukung("3.0"), "simpanan 3.0 didukung");
        Periksa(SaveData.VersiDidukung("4.0"), "simpanan 4.0 didukung");
        Periksa(SaveData.VersiDidukung("5.0"), "simpanan 5.0 didukung");
        Periksa(SaveData.VersiDidukung("5.1"), "simpanan 5.1 didukung");
        Periksa(SaveData.VersiDidukung(GameBalance.VersiSimpan),
            "VersiSimpan saat ini didukung");
        Periksa(!SaveData.VersiDidukung("9.9"), "versi 9.9 ditolak");
        Periksa(!SaveData.VersiDidukung("5.2"), "versi 5.2 ditolak");
        Periksa(!SaveData.VersiDidukung("0.9"), "versi 0.9 ditolak");
        Periksa(!SaveData.VersiDidukung(""), "versi kosong ditolak");
        Periksa(SaveData.UkuranGridEfektif(0) == 12,
            "simpanan lama (grid 0) -> 12");
        Periksa(SaveData.UkuranGridEfektif(-3) == 12,
            "simpanan rusak (grid -3) -> 12");
        Periksa(SaveData.UkuranGridEfektif(16) == 16,
            "simpanan 16 -> 16 (sesudah perluasan)");

        Console.WriteLine("== Tes B: versi game & rilis ==");
        Periksa(GameBalance.VersiSimpan == "6.0",
            "VersiSimpan = 6.0");
        Periksa(GameBalance.VersiGame == "1.0",
            "VersiGame = 1.0 (rilis pertama)");
        Periksa(GameBalance.HadiahNaikLevel2 < GameBalance.HadiahNaikLevel3,
            "hadiah L2 < L3");
        Periksa(GameBalance.HadiahNaikLevel3 < GameBalance.HadiahNaikLevel4,
            "hadiah L3 < L4");
        Periksa(GameBalance.HadiahNaikLevel4 < GameBalance.HadiahNaikLevel5,
            "hadiah L4 < L5");
        Periksa(GameBalance.HadiahNaikLevel5 < GameBalance.HadiahNaikLevel6,
            "hadiah L5 < L6");
        Periksa(GameBalance.UkuranGrid == 12, "grid awal 12x12");
        Periksa(GameBalance.UkuranGridDiperluas == 16,
            "grid diperluas 16x16");
        Periksa(GameBalance.UkuranGridDiperluas > GameBalance.UkuranGrid,
            "perluasan benar-benar memperbesar grid");
        Periksa(GameBalance.HargaPerluasanLahan == 5000000,
            "harga perluasan Rp5.000.000");
        Periksa(GameBalance.LevelMaksDunia == 6, "level maks dunia 6");
        Periksa(GameBalance.TargetFrameRate == 60, "target 60 fps");
        Periksa(GameBalance.DurasiHariDetik == 300f,
            "1 hari game = 300 detik (5 menit, bukan 60 uji)");

        Console.WriteLine("== Tes C: Balai Kota silang GameBalance ==");
        Periksa(GameBalance.LevelBukaBalaiKota == 3, "Balai Kota L3");
        Periksa(GameBalance.HargaBalaiKota == 3000000,
            "Balai Kota Rp3.000.000");
        Periksa(GameBalance.PerawatanBalaiKota == 5000,
            "Balai Kota perawatan Rp5.000");
        Periksa(GameBalance.BonusBahagiaBalaiKota == 5,
            "Balai Kota bahagia +5");
        Periksa(GameBalance.BonusReputasiBalaiKotaPerHari == 2,
            "Balai Kota reputasi +2/hari");
        Periksa(GameBalance.MaksBalaiKotaPerDunia == 1,
            "Balai Kota maks 1 per dunia");
        Periksa(SistemBalaiKota.BolehBangun(0), "boleh bangun bila 0 ada");
        Periksa(!SistemBalaiKota.BolehBangun(1),
            "tidak boleh bangun bila 1 sudah ada");
        Periksa(SistemBalaiKota.BonusReputasiHarian(1) == 2,
            "reputasi +2 bila ada Balai Kota");
        Periksa(SistemBalaiKota.BonusReputasiHarian(0) == 0,
            "reputasi +0 bila tidak ada Balai Kota");
        Periksa(SistemBalaiKota.ApakahTerbuka(3), "terbuka di Level 3");
        Periksa(!SistemBalaiKota.ApakahTerbuka(2),
            "belum terbuka di Level 2");

        Console.WriteLine("== Tes D: progresi dunia 1-6 ==");
        int[] populasi = new int[6];
        int[] bahagia = new int[6];
        for (int level = 1; level <= 6; level++)
        {
            string nama = ProgresiDunia.NamaLevel(level);
            Periksa(nama != null && nama.Length > 0,
                "nama level " + level + " terisi");
            if (level > 1)
            {
                populasi[level - 1] = ProgresiDunia.SyaratPopulasi(level);
                bahagia[level - 1] = ProgresiDunia.SyaratBahagia(level);
            }
        }
        Periksa(ProgresiDunia.SyaratPopulasi(2) == 10, "L2: 10 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(3) == 30, "L3: 30 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(4) == 80, "L4: 80 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(5) == 150, "L5: 150 warga");
        Periksa(ProgresiDunia.SyaratPopulasi(6) == 300, "L6: 300 warga");
        Periksa(ProgresiDunia.SyaratBahagia(2) == 0, "L2 tanpa syarat bahagia");
        Periksa(ProgresiDunia.SyaratBahagia(3) == 60, "L3: bahagia 60");
        Periksa(ProgresiDunia.SyaratBahagia(4) == 70, "L4: bahagia 70");
        Periksa(ProgresiDunia.SyaratBahagia(5) == 75, "L5: bahagia 75");
        Periksa(ProgresiDunia.SyaratBahagia(6) == 80, "L6: bahagia 80");
        for (int i = 2; i < 6; i++)
        {
            Periksa(populasi[i] > populasi[i - 1],
                "syarat populasi naik terus (level " + (i + 1) + ")");
            Periksa(bahagia[i] >= bahagia[i - 1],
                "syarat bahagia tidak turun (level " + (i + 1) + ")");
        }
        HashSet<string> namaUnik = new HashSet<string>();
        for (int level = 1; level <= 6; level++)
        {
            namaUnik.Add(ProgresiDunia.NamaLevel(level));
        }
        Periksa(namaUnik.Count == 6, "6 nama level berbeda");

        Console.WriteLine("== Tes E: pencapaian (6, total 26 Permata) ==");
        string[] pencapaian = SistemPencapaian.SemuaId();
        Periksa(pencapaian.Length == 6, "6 pencapaian");
        int totalPermata = 0;
        foreach (string id in pencapaian)
        {
            int hadiah = SistemPencapaian.HadiahPermata(id);
            Periksa(hadiah > 0, "pencapaian " + id + " berhadiah Permata");
            Periksa(SistemPencapaian.Judul(id).Length > 0,
                "pencapaian " + id + " punya judul");
            Periksa(SistemPencapaian.Deskripsi(id).Length > 0,
                "pencapaian " + id + " punya deskripsi");
            totalPermata += hadiah;
        }
        Periksa(totalPermata == 26,
            "total hadiah pencapaian = 26 Permata (dapat " + totalPermata
            + ")");

        Console.WriteLine("== Tes F: misi harian (8, 3/hari, deterministik) ==");
        string[] misi = SistemMisiHarian.SemuaId();
        Periksa(misi.Length == 8, "8 misi harian di kolam");
        HashSet<string> misiValid = new HashSet<string>(misi);
        foreach (string id in misi)
        {
            bool adaHadiah = SistemMisiHarian.HadiahKoin(id) > 0
                || SistemMisiHarian.HadiahPermata(id) > 0;
            Periksa(adaHadiah, "misi " + id + " punya hadiah");
            Periksa(SistemMisiHarian.Judul(id).Length > 0,
                "misi " + id + " punya judul");
        }
        string[] hari1a = SistemMisiHarian.PilihUntukHari(1);
        string[] hari1b = SistemMisiHarian.PilihUntukHari(1);
        Periksa(hari1a.Length == 3, "3 misi per hari");
        Periksa(hari1a[0] == hari1b[0] && hari1a[1] == hari1b[1]
            && hari1a[2] == hari1b[2],
            "pilihan misi hari yang sama deterministik");
        foreach (string id in hari1a)
        {
            Periksa(misiValid.Contains(id),
                "misi hari 1 '" + id + "' ada di kolam");
        }
        Periksa(hari1a[0] != hari1a[1] && hari1a[1] != hari1a[2]
            && hari1a[0] != hari1a[2],
            "3 misi dalam satu hari berbeda-beda");

        Console.WriteLine("== Tes G: ekonomi dasar ==");
        Periksa(EconomyCalculator.HitungHargaJual(500000, 0.5f) == 250000,
            "jual Rumah = Rp250.000 (50%)");
        Periksa(EconomyCalculator.HitungHargaJual(3000000, 0.5f) == 1500000,
            "jual Balai Kota = Rp1.500.000 (50%)");
        Periksa(EconomyCalculator.PengaliWarung(30) == 80,
            "warung 80% bila bahagia 30");
        Periksa(EconomyCalculator.PengaliWarung(50) == 100,
            "warung 100% bila bahagia 50");
        Periksa(EconomyCalculator.PengaliWarung(80) == 110,
            "warung 110% bila bahagia 80");
        Periksa(GameBalance.PajakKotaPerMil == 25, "pajak kota 2,5%");
        Periksa(GameBalance.RasioJualKembali == 0.5f, "rasio jual 50%");

        Console.WriteLine("== Tes H: interior (10 slot, Rp1.420.000, +16) ==");
        Periksa(SistemInterior.PerabotPerSlot.Length == 10,
            "10 slot perabot per hunian");
        long totalHarga = 0;
        int totalBonus = 0;
        foreach (string id in SistemInterior.PerabotPerSlot)
        {
            totalHarga += SistemInterior.HargaPerabot(id);
            totalBonus += SistemInterior.BonusBahagiaPerabot(id);
            Periksa(SistemInterior.HargaPerabot(id) > 0,
                "perabot " + id + " punya harga");
            Periksa(SistemInterior.NamaPerabot(id).Length > 0,
                "perabot " + id + " punya nama");
        }
        Periksa(totalHarga == 1420000,
            "total harga 10 perabot = Rp1.420.000 (dapat " + totalHarga
            + ")");
        Periksa(totalBonus == 16,
            "total bonus 10 perabot = +16 (dapat " + totalBonus + ")");
        List<string> kosong = SistemInterior.InteriorKosong();
        Periksa(SistemInterior.ApakahKosong(kosong),
            "interior baru kosong");
        Periksa(!SistemInterior.ApakahLengkap(kosong),
            "interior baru belum lengkap");
        Periksa(SistemInterior.HitungPerabot(kosong) == 0,
            "interior baru 0 perabot");
        Periksa(SistemInterior.BonusBahagia(kosong)
            == -GameBalance.HukumanRumahKosong,
            "interior kosong bonus -5 (penghuni tidak betah, GDD 5.6)");
        List<string> lengkap = new List<string>(SistemInterior.PerabotPerSlot);
        Periksa(SistemInterior.ApakahLengkap(lengkap),
            "10 perabot = lengkap");
        Periksa(SistemInterior.HitungPerabot(lengkap) == 10,
            "interior lengkap 10 perabot");
        Periksa(SistemInterior.BonusBahagia(lengkap) == 16,
            "interior lengkap bonus +16");
        Periksa(SistemInterior.BatasiBonusTotal(25) == 20,
            "bonus total dibatasi 20");
        Periksa(SistemInterior.BatasiBonusTotal(-25) == -20,
            "bonus total dibatasi -20");
        Periksa(SistemInterior.BatasiBonusTotal(10) == 10,
            "bonus total 10 tidak diubah");

        Console.WriteLine();
        Console.WriteLine("HASIL: " + lolos + " lolos, " + gagal + " gagal");
        if (gagal > 0)
        {
            Environment.Exit(1);
        }
    }
}
