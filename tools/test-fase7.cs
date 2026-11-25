// Dunia Kita — tools/test-fase7.cs
// Test harness logika murni Fase 7 (tanpa Unity): dikompilasi dengan
// mcs bersama SistemWaliKota.cs, AturanDuniaBersama.cs,
// EconomyCalculator.cs, FormatUtil.cs, GameBalance.cs, SaveData.cs,
// BuildingData.cs (enum KategoriBangunan) dan unity-stubs.cs. Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji:
//   A. Harga tanah wali kota menurut jarak ke pusat (pinggir murah,
//      pusat mahal) + deterministik.
//   B. Biaya izin: 10% dasar; diskon reputasi bagus, mahal reputasi
//      buruk, kas menipis/pemuh, Balai Kota, paling murah Rp10.000.
//   C. Keputusan wali kota: Disetujui / Ditolak / Bersyarat —
//      pengaruh reputasi, kategori bangunan, kepadatan kawasan,
//      variasi deterministik (input sama -> hasil sama).
//   D. Syarat izin: jalan bersebelahan.
//   E. Validasi bangun dunia bersama: tapak harus milik sendiri +
//      kosong, koin cukup, syarat jalan (cermin server/db.py).
//   F. Pemasukan offline: rumus kotor - pajak 2,5% - upkeep, batas
//      24 hari (cermin server/db.py).
//   G. Diff dunia: tambah/lepas saat sinkronisasi.
//   H. Migrasi simpanan 5.1 -> 6.0 (6.0 didukung, 5.1 tetap dimuat).

using System;
using System.Collections.Generic;

public static class TesFase7
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
        Console.WriteLine("== Tes A: harga tanah menurut jarak ==");
        Periksa(SistemWaliKota.HargaTanah(0, 0) == 200000,
            "pinggir (0,0) jarak 19 -> Rp200.000");
        Periksa(SistemWaliKota.HargaTanah(39, 39) == 200000,
            "pinggir terjauh (39,39) jarak 20 -> Rp200.000");
        Periksa(SistemWaliKota.HargaTanah(19, 19) == 920000,
            "pusat (19,19) Rp920.000");
        Periksa(SistemWaliKota.HargaTanah(19, 18) == 860000,
            "pusat (19,18) jarak 1 -> Rp860.000");
        Periksa(SistemWaliKota.HargaTanah(0, 19) == 200000,
            "(0,19) jarak 19 -> Rp200.000");
        Periksa(SistemWaliKota.HargaTanah(13, 13) == 560000,
            "(5,5) jarak 6 -> Rp800.000");
        Periksa(SistemWaliKota.HargaTanah(11, 11)
                == SistemWaliKota.HargaTanah(11, 11),
            "harga tanah deterministik");

        Console.WriteLine("== Tes B: biaya izin ==");
        // Kas 5jt (normal), tanpa Balai Kota.
        Periksa(SistemWaliKota.BiayaIzin(1000000, 50, 5000000, false) == 100000,
            "biaya izin dasar 10% = Rp100.000");
        Periksa(SistemWaliKota.BiayaIzin(1000000, 80, 5000000, false) == 70000,
            "reputasi 80 -> -30% = Rp70.000");
        Periksa(SistemWaliKota.BiayaIzin(1000000, 20, 5000000, false) == 150000,
            "reputasi 20 -> +50% = Rp150.000");
        Periksa(SistemWaliKota.BiayaIzin(1000000, 50, 500000, false) == 150000,
            "kas menipis -> +50% = Rp150.000");
        Periksa(SistemWaliKota.BiayaIzin(1000000, 50, 25000000, false) == 80000,
            "kas penuh -> -20% = Rp80.000");
        Periksa(SistemWaliKota.BiayaIzin(1000000, 50, 5000000, true) == 90000,
            "ada Balai Kota -> -10% = Rp90.000");
        Periksa(SistemWaliKota.BiayaIzin(20000, 50, 5000000, false) == 10000,
            "biaya izin paling murah Rp10.000");
        // Gabungan: reputasi bagus + kas penuh + Balai Kota.
        Periksa(SistemWaliKota.BiayaIzin(1000000, 80, 25000000, true) == 50400,
            "gabungan 0,7 x 0,8 x 0,9 = Rp50.400");

        Console.WriteLine("== Tes C: keputusan wali kota ==");
        // Reputasi bagus + hunian di pinggir -> disetujui.
        KeputusanIzin k1 = SistemWaliKota.Putuskan(
            0, 0, "rumah", KategoriBangunan.Hunian, 80, 5000000, 0, false, "Raka");
        Periksa(k1.Hasil == HasilIzin.Disetujui, "rep 80 + rumah -> Disetujui");
        Periksa(k1.HargaTotal == k1.HargaTanah + k1.BiayaIzin,
            "total disetujui = tanah + izin");
        Periksa(k1.Syarat == SyaratIzin.TanpaSyarat, "disetujui tanpa syarat");
        // Reputasi 10 -> ditolak otomatis.
        KeputusanIzin k2 = SistemWaliKota.Putuskan(
            0, 0, "rumah", KategoriBangunan.Hunian, 10, 5000000, 0, false, "Raka");
        Periksa(k2.Hasil == HasilIzin.Ditolak, "rep 10 -> Ditolak otomatis");
        Periksa(k2.HargaTotal == 0, "ditolak total 0");
        Periksa(k2.Alasan.IndexOf("reputasi") >= 0, "alasan tolak menyebut reputasi");
        // Kawasan padat (4 sejenis) -> ditolak otomatis.
        KeputusanIzin k3 = SistemWaliKota.Putuskan(
            5, 5, "taman", KategoriBangunan.Fasilitas, 80, 5000000, 4, false, "Raka");
        Periksa(k3.Hasil == HasilIzin.Ditolak, "4 sejenis di kawasan -> Ditolak");
        Periksa(k3.Alasan.IndexOf("padat") >= 0, "alasan tolak menyebut padat");
        // Deterministik: putusan sama untuk input sama.
        KeputusanIzin k4a = SistemWaliKota.Putuskan(
            3, 7, "warung", KategoriBangunan.Komersial, 50, 5000000, 0, false, "Sinta");
        KeputusanIzin k4b = SistemWaliKota.Putuskan(
            3, 7, "warung", KategoriBangunan.Komersial, 50, 5000000, 0, false, "Sinta");
        Periksa(k4a.Hasil == k4b.Hasil && k4a.BiayaIzin == k4b.BiayaIzin,
            "putusan deterministik (input sama -> hasil sama)");
        // Variasi skor dalam -5..+5.
        int v = SistemWaliKota.VariasiSkor("Raka", 3, 7);
        Periksa(v >= -5 && v <= 5, "variasi skor -5..+5 (dapat " + v + ")");
        // Bersyarat: reputasi 20 + komersial + variasi negatif.
        // Cari petak yang variasinya <= -1 supaya skor 25-39.
        bool ketemuBersyarat = false;
        for (int x = 0; x < 24 && !ketemuBersyarat; x++)
        {
            for (int y = 0; y < 24 && !ketemuBersyarat; y++)
            {
                int var = SistemWaliKota.VariasiSkor("Uji", x, y);
                int skor = SistemWaliKota.SkorKeputusan(
                    20, KategoriBangunan.Komersial, 0, "Uji", x, y);
                if (skor >= 25 && skor < 40)
                {
                    KeputusanIzin kb = SistemWaliKota.Putuskan(
                        x, y, "warung", KategoriBangunan.Komersial,
                        20, 5000000, 0, false, "Uji");
                    Periksa(kb.Hasil == HasilIzin.Bersyarat,
                        "skor " + skor + " (var " + var + ") -> Bersyarat");
                    Periksa(kb.Syarat == SyaratIzin.JalanBersebelahan,
                        "syarat Bersyarat warung = jalan bersebelahan");
                    Periksa(kb.BiayaIzin > 0 && kb.HargaTotal > kb.HargaTanah,
                        "biaya izin Bersyarat lebih mahal dari dasar");
                    ketemuBersyarat = true;
                }
            }
        }
        Periksa(ketemuBersyarat, "ditemukan petak Bersyarat untuk Uji");
        // Bersyarat untuk bangunan Jalan -> syarat biaya (bukan jalan).
        // Jalan itu fasilitas (+15) jadi butuh 3 fasilitas sejenis di
        // kawasan (-15) supaya skor turun ke 25-39.
        bool ketemuJalan = false;
        for (int x = 0; x < 24 && !ketemuJalan; x++)
        {
            for (int y = 0; y < 24 && !ketemuJalan; y++)
            {
                int skor = SistemWaliKota.SkorKeputusan(
                    20, KategoriBangunan.Fasilitas, 3, "Uji", x, y);
                if (skor >= 25 && skor < 40)
                {
                    KeputusanIzin kj = SistemWaliKota.Putuskan(
                        x, y, "jalan", KategoriBangunan.Fasilitas,
                        20, 5000000, 3, false, "Uji");
                    Periksa(kj.Hasil == HasilIzin.Bersyarat,
                        "jalan skor " + skor + " -> Bersyarat");
                    Periksa(kj.Syarat == SyaratIzin.BiayaLebihMahal,
                        "syarat Bersyarat jalan = biaya lebih mahal");
                    ketemuJalan = true;
                }
            }
        }
        Periksa(ketemuJalan, "ditemukan petak Bersyarat jalan untuk Uji");
        // Fasilitas didukung kota: skor fasilitas > komersial.
        int skorFas = SistemWaliKota.SkorKeputusan(
            50, KategoriBangunan.Fasilitas, 0, "Raka", 5, 5);
        int skorKom = SistemWaliKota.SkorKeputusan(
            50, KategoriBangunan.Komersial, 0, "Raka", 5, 5);
        Periksa(skorFas > skorKom, "fasilitas didukung kota (skor lebih tinggi)");

        Console.WriteLine("== Tes D: syarat izin ==");
        Periksa(SistemWaliKota.SyaratTerpenuhi(SyaratIzin.TanpaSyarat, false),
            "tanpa syarat selalu terpenuhi");
        Periksa(SistemWaliKota.SyaratTerpenuhi(SyaratIzin.BiayaLebihMahal, false),
            "syarat biaya selalu terpenuhi");
        Periksa(!SistemWaliKota.SyaratTerpenuhi(SyaratIzin.JalanBersebelahan, false),
            "syarat jalan tanpa jalan -> tidak terpenuhi");
        Periksa(SistemWaliKota.SyaratTerpenuhi(SyaratIzin.JalanBersebelahan, true),
            "syarat jalan dengan jalan -> terpenuhi");

        Console.WriteLine("== Tes E: validasi bangun dunia bersama ==");
        Dictionary<string, string> pemilik = new Dictionary<string, string>();
        pemilik["0,0"] = "Raka";
        pemilik["1,0"] = "Raka";
        pemilik["0,1"] = "Raka";
        pemilik["1,1"] = "Raka";
        pemilik["5,5"] = "Sinta";
        Dictionary<string, bool> terisi = new Dictionary<string, bool>();
        AturanDuniaBersama.HasilValidasi hv;
        hv = AturanDuniaBersama.ValidasiBangun(0, 0, 2, 2, 2000000, 5000000,
            "Raka", pemilik, terisi, SyaratIzin.TanpaSyarat, false);
        Periksa(hv.Boleh, "bangun Kost 2x2 di 4 petak milik sendiri -> boleh");
        hv = AturanDuniaBersama.ValidasiBangun(1, 1, 2, 2, 2000000, 5000000,
            "Raka", pemilik, terisi, SyaratIzin.TanpaSyarat, false);
        Periksa(!hv.Boleh, "tapak 2x2 yang 1 petaknya bukan milik -> tolak");
        hv = AturanDuniaBersama.ValidasiBangun(5, 5, 1, 1, 500000, 5000000,
            "Raka", pemilik, terisi, SyaratIzin.TanpaSyarat, false);
        Periksa(!hv.Boleh, "bangun di tanah Sinta -> tolak");
        Periksa(hv.Alasan.IndexOf("Sinta") < 0
                && hv.Alasan.IndexOf("bukan tanah milikmu") >= 0,
            "alasan tolak tanah orang lain jelas");
        terisi["0,0"] = true;
        hv = AturanDuniaBersama.ValidasiBangun(0, 0, 1, 1, 500000, 5000000,
            "Raka", pemilik, terisi, SyaratIzin.TanpaSyarat, false);
        Periksa(!hv.Boleh, "bangun di petak sudah terisi -> tolak");
        terisi["0,0"] = false;
        hv = AturanDuniaBersama.ValidasiBangun(0, 0, 1, 1, 500000, 100000,
            "Raka", pemilik, terisi, SyaratIzin.TanpaSyarat, false);
        Periksa(!hv.Boleh, "koin tidak cukup -> tolak");
        hv = AturanDuniaBersama.ValidasiBangun(0, 0, 1, 1, 500000, 5000000,
            "Raka", pemilik, terisi, SyaratIzin.JalanBersebelahan, false);
        Periksa(!hv.Boleh, "syarat jalan tidak terpenuhi -> tolak");
        hv = AturanDuniaBersama.ValidasiBangun(0, 0, 1, 1, 500000, 5000000,
            "Raka", pemilik, terisi, SyaratIzin.JalanBersebelahan, true);
        Periksa(hv.Boleh, "syarat jalan terpenuhi -> boleh");

        Console.WriteLine("== Tes F: pemasukan offline ==");
        List<EconomyCalculator.CatatanBangunan> daftar =
            new List<EconomyCalculator.CatatanBangunan>();
        daftar.Add(new EconomyCalculator.CatatanBangunan
        {
            Id = "warung", PemasukanHarian = 15000, BiayaPerawatanHarian = 2000
        });
        daftar.Add(new EconomyCalculator.CatatanBangunan
        {
            Id = "rumah", PemasukanHarian = 0, BiayaPerawatanHarian = 1000
        });
        AturanDuniaBersama.HasilOffline off =
            AturanDuniaBersama.HitungPemasukanOffline(daftar, 2);
        // Kotor 30000, pajak 750, upkeep 6000, bersih 23250.
        Periksa(off.Hari == 2, "offline 2 hari");
        Periksa(off.Kotor == 30000, "kotor offline 2 hari Rp30.000");
        Periksa(off.Pajak == 750, "pajak offline 2,5% = Rp750");
        Periksa(off.Upkeep == 6000, "upkeep offline Rp6.000");
        Periksa(off.Bersih == 23250, "bersih offline Rp23.250");
        off = AturanDuniaBersama.HitungPemasukanOffline(daftar, 30);
        Periksa(off.Hari == 24, "offline dibatasi 24 hari");
        Periksa(off.Bersih == 23250 * 12, "bersih 24 hari = 12x 2 hari");
        off = AturanDuniaBersama.HitungPemasukanOffline(daftar, -3);
        Periksa(off.Hari == 0 && off.Bersih == 0, "hari negatif -> 0");
        off = AturanDuniaBersama.HitungPemasukanOffline(
            new List<EconomyCalculator.CatatanBangunan>(), 5);
        Periksa(off.Bersih == 0, "tanpa bangunan -> bersih 0");

        Console.WriteLine("== Tes G: diff dunia ==");
        List<AturanDuniaBersama.BangunanBersama> lokal =
            new List<AturanDuniaBersama.BangunanBersama>();
        lokal.Add(new AturanDuniaBersama.BangunanBersama
            { X = 0, Y = 0, Id = "rumah", Pemilik = "Raka" });
        lokal.Add(new AturanDuniaBersama.BangunanBersama
            { X = 5, Y = 5, Id = "warung", Pemilik = "Raka" });
        List<AturanDuniaBersama.BangunanBersama> server =
            new List<AturanDuniaBersama.BangunanBersama>();
        server.Add(new AturanDuniaBersama.BangunanBersama
            { X = 0, Y = 0, Id = "rumah", Pemilik = "Raka" });
        server.Add(new AturanDuniaBersama.BangunanBersama
            { X = 1, Y = 1, Id = "taman", Pemilik = "Sinta" });
        AturanDuniaBersama.HasilDiff diff =
            AturanDuniaBersama.DiffDunia(lokal, server);
        Periksa(diff.Tambah.Count == 1 && diff.Tambah[0].Id == "taman",
            "diff: taman Sinta ditambah");
        Periksa(diff.Lepas.Count == 1 && diff.Lepas[0].Id == "warung",
            "diff: warung Raka dilepas (dijual orang lain/dihapus)");
        diff = AturanDuniaBersama.DiffDunia(server, server);
        Periksa(diff.Tambah.Count == 0 && diff.Lepas.Count == 0,
            "diff: dunia sama -> tidak ada perubahan");
        // Bangunan sama petak tapi pemilik berubah -> lepas + tambah.
        List<AturanDuniaBersama.BangunanBersama> server2 =
            new List<AturanDuniaBersama.BangunanBersama>();
        server2.Add(new AturanDuniaBersama.BangunanBersama
            { X = 0, Y = 0, Id = "rumah", Pemilik = "Sinta" });
        diff = AturanDuniaBersama.DiffDunia(lokal, server2);
        Periksa(diff.Tambah.Count == 1 && diff.Lepas.Count == 2,
            "diff: pemilik berubah -> lepas lama + tambah baru");

        Console.WriteLine("== Tes H: migrasi simpanan 5.1 -> 6.0 ==");
        Periksa(GameBalance.VersiSimpan == "6.0", "versi simpanan sekarang 6.0");
        Periksa(GameBalance.VersiSimpanFase5_1 == "5.1",
            "versi 5.1 tercatat sebagai Fase 5.1");
        Periksa(SaveData.VersiDidukung("6.0"), "6.0 didukung");
        Periksa(SaveData.VersiDidukung("5.1"), "5.1 tetap dimuat");
        Periksa(SaveData.VersiDidukung("5.0"), "5.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("4.0"), "4.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("3.0"), "3.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("2.0"), "2.0 tetap dimuat");
        Periksa(SaveData.VersiDidukung("1.0"), "1.0 tetap dimuat");
        Periksa(!SaveData.VersiDidukung("7.0"), "7.0 tidak didukung");
        Periksa(!SaveData.VersiDidukung(""), "versi kosong tidak didukung");
        Periksa(GameBalance.UkuranGridDuniaBersama == 40,
            "peta bersama 40x40");
        Periksa(GameBalance.UkuranGridDuniaBersama
                > GameBalance.UkuranGridDiperluas,
            "peta bersama lebih besar dari perluasan single player");

        Console.WriteLine("HASIL: " + lolos + " lolos, " + gagal + " gagal");
        if (gagal > 0)
        {
            Environment.Exit(1);
        }
    }
}
