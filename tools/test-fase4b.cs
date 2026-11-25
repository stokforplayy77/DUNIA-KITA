// Dunia Kita — tools/test-fase4b.cs
// Test harness logika murni Fase 4B (tanpa Unity): dikompilasi dengan
// mcs bersama EconomyCalculator.cs, FormatUtil.cs, GameBalance.cs, SaveData.cs,
// SistemInterior.cs, SistemPeristiwaPenghuni.cs dan unity-stubs.cs.
// Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji: slot & ruangan interior (GDD §5.6: 4 ruangan, 10 slot
// tetap, harga + bonus tiap perabot), beli/ganti/hapus perabot
// (koin berkurang, slot terisi, uang kembali 50%, penolakan bila
// slot penuh/tidak valid/koin kurang), penalti rumah kosong (-5)
// dan cap total bonus interior (-20..+20), pengali sewa Kost
// berperabot lengkap (+25% -> sewa Rp40.000 jadi Rp50.000), tiap
// pilihan peristiwa penghuni §9.1 (uang/reputasi/bahagia sesuai
// tabel), batas roll peristiwa (peluang 10%/hari, 6 jenis sama rata),
// clamp reputasi (0-100), simulasi 10 hari dunia standar Fase 4B
// dengan angka persis, dan migrasi simpanan 4.0 -> 5.0.
//
// Catatan: "dunia standar Fase 4B" di tes ini memakai Jalan di semua
// bangunan (TanpaJalan=false); 2 Rumah (satu kosong, satu berisi
// 1 Kasur) + 1 Kost lengkap + 1 Warung + 1 Taman.

using System;
using System.Collections.Generic;
using UnityEngine;

public static class TestFase4B
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

    // Satu baris bangunan untuk HitungHarian (pola test-fase4).
    // pengaliSewa: 0 = default (dianggap 100, kompatibel tes lama).
    private static EconomyCalculator.CatatanBangunan Catatan(string id,
        long masuk, long keluar, int warga, bool taman, int bonus,
        bool tanpaJalan, int pengaliSewa)
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
            TanpaJalan = tanpaJalan,
            PengaliSewaPersen = pengaliSewa
        };
    }

    // Dunia standar Fase 4B: 2 Rumah + 1 Kost + 1 Warung + 1 Taman,
    // semua dengan Jalan. Interior: Rumah1 kosong (-5), Rumah2 berisi
    // 1 Kasur (+2), Kost lengkap (+16) -> total interior +13.
    // kostLengkap: true -> Kost berisi 10 perabot (pengali sewa 125);
    // false -> Kost berisi 1 Kasur saja (pengali sewa 100).
    private static List<EconomyCalculator.CatatanBangunan> DuniaStandarFase4B(
        bool kostLengkap)
    {
        return new List<EconomyCalculator.CatatanBangunan>
        {
            Catatan("rumah", 0, 1000, 2, false, 0, false, 0),
            Catatan("rumah", 0, 1000, 2, false, 0, false, 0),
            Catatan("kost", 40000, 5000, 8, false, 0, false,
                kostLengkap ? GameBalance.PengaliSewaKostLengkap : 100),
            Catatan("warung", 15000, 2000, 0, false, 0, false, 0),
            Catatan("taman", 0, 500, 0, true, 0, false, 0)
        };
    }

    // Bonus interior dunia standar: Rumah1 kosong -5, Rumah2 Kasur +2,
    // Kost lengkap +16 (atau Kasur saja +2 bila tidak lengkap).
    private static int BonusInteriorDunia(bool kostLengkap)
    {
        var rumahKosong = SistemInterior.InteriorKosong();
        var rumahKasur = SistemInterior.InteriorKosong();
        rumahKasur[0] = "kasur";
        var kost = SistemInterior.InteriorKosong();
        if (kostLengkap)
        {
            for (int i = 0; i < SistemInterior.JumlahSlot; i++)
            {
                kost[i] = SistemInterior.IdPerabotSlot(i);
            }
        }
        else
        {
            kost[0] = "kasur";
        }
        int total = SistemInterior.BonusBahagia(rumahKosong)
            + SistemInterior.BonusBahagia(rumahKasur)
            + SistemInterior.BonusBahagia(kost);
        return SistemInterior.BatasiBonusTotal(total);
    }

    private static void PeriksaDampak(string id, int pilihan,
        long koin, int reputasi, int bahagiaBesok,
        bool sewaHilang, int kapasitasHilang, long koinBesok, string nama)
    {
        SistemPeristiwaPenghuni.DampakPeristiwa d =
            SistemPeristiwaPenghuni.Dampak(id, pilihan);
        Periksa(d.Koin == koin && d.Reputasi == reputasi
            && d.BonusBahagiaBesok == bahagiaBesok
            && d.SewaKostHilangBesok == sewaHilang
            && d.KapasitasHilangBesok == kapasitasHilang
            && d.TambahanKoinBesok == koinBesok, nama);
    }

    public static void Main()
    {
        Console.WriteLine("== Tes A: slot & ruangan interior (GDD §5.6) ==");
        Periksa(SistemInterior.JumlahSlot == 10, "jumlah slot per hunian = 10");
        Periksa(SistemInterior.NamaRuangan(0) == "Kamar Tidur", "slot 0 = Kamar Tidur");
        Periksa(SistemInterior.NamaRuangan(1) == "Kamar Tidur", "slot 1 = Kamar Tidur");
        Periksa(SistemInterior.NamaRuangan(2) == "Dapur", "slot 2 = Dapur");
        Periksa(SistemInterior.NamaRuangan(3) == "Dapur", "slot 3 = Dapur");
        Periksa(SistemInterior.NamaRuangan(4) == "Dapur", "slot 4 = Dapur");
        Periksa(SistemInterior.NamaRuangan(5) == "Kamar Mandi", "slot 5 = Kamar Mandi");
        Periksa(SistemInterior.NamaRuangan(6) == "Kamar Mandi", "slot 6 = Kamar Mandi");
        Periksa(SistemInterior.NamaRuangan(7) == "Ruang Tamu", "slot 7 = Ruang Tamu");
        Periksa(SistemInterior.NamaRuangan(8) == "Ruang Tamu", "slot 8 = Ruang Tamu");
        Periksa(SistemInterior.NamaRuangan(9) == "Ruang Tamu", "slot 9 = Ruang Tamu");
        Periksa(SistemInterior.NamaRuangan(-1) == "", "slot -1 tidak punya ruangan");
        Periksa(SistemInterior.NamaRuangan(10) == "", "slot 10 tidak punya ruangan");
        Periksa(SistemInterior.IdPerabotSlot(0) == "kasur", "slot 0 = Kasur");
        Periksa(SistemInterior.IdPerabotSlot(1) == "lemari", "slot 1 = Lemari");
        Periksa(SistemInterior.IdPerabotSlot(2) == "kompor", "slot 2 = Kompor");
        Periksa(SistemInterior.IdPerabotSlot(3) == "meja", "slot 3 = Meja");
        Periksa(SistemInterior.IdPerabotSlot(4) == "kursi", "slot 4 = Kursi");
        Periksa(SistemInterior.IdPerabotSlot(5) == "bak_mandi", "slot 5 = Bak Mandi");
        Periksa(SistemInterior.IdPerabotSlot(6) == "wastafel", "slot 6 = Wastafel");
        Periksa(SistemInterior.IdPerabotSlot(7) == "tv", "slot 7 = TV");
        Periksa(SistemInterior.IdPerabotSlot(8) == "sofa", "slot 8 = Sofa");
        Periksa(SistemInterior.IdPerabotSlot(9) == "lampu", "slot 9 = Lampu");

        Console.WriteLine("== Tes B: harga & bonus tiap perabot (GameBalance) ==");
        Periksa(SistemInterior.HargaPerabot("kasur") == 150000, "Kasur Rp150.000");
        Periksa(SistemInterior.HargaPerabot("lemari") == 100000, "Lemari Rp100.000");
        Periksa(SistemInterior.HargaPerabot("kompor") == 200000, "Kompor Rp200.000");
        Periksa(SistemInterior.HargaPerabot("meja") == 80000, "Meja Rp80.000");
        Periksa(SistemInterior.HargaPerabot("kursi") == 50000, "Kursi Rp50.000");
        Periksa(SistemInterior.HargaPerabot("bak_mandi") == 150000, "Bak Mandi Rp150.000");
        Periksa(SistemInterior.HargaPerabot("wastafel") == 80000, "Wastafel Rp80.000");
        Periksa(SistemInterior.HargaPerabot("tv") == 300000, "TV Rp300.000");
        Periksa(SistemInterior.HargaPerabot("sofa") == 250000, "Sofa Rp250.000");
        Periksa(SistemInterior.HargaPerabot("lampu") == 60000, "Lampu Rp60.000");
        Periksa(SistemInterior.HargaPerabot("tidak_ada") == 0, "perabot tidak dikenal harga 0");
        Periksa(SistemInterior.BonusBahagiaPerabot("kasur") == 2, "Kasur +2");
        Periksa(SistemInterior.BonusBahagiaPerabot("lemari") == 1, "Lemari +1");
        Periksa(SistemInterior.BonusBahagiaPerabot("kompor") == 2, "Kompor +2");
        Periksa(SistemInterior.BonusBahagiaPerabot("meja") == 1, "Meja +1");
        Periksa(SistemInterior.BonusBahagiaPerabot("kursi") == 1, "Kursi +1");
        Periksa(SistemInterior.BonusBahagiaPerabot("bak_mandi") == 2, "Bak Mandi +2");
        Periksa(SistemInterior.BonusBahagiaPerabot("wastafel") == 1, "Wastafel +1");
        Periksa(SistemInterior.BonusBahagiaPerabot("tv") == 3, "TV +3");
        Periksa(SistemInterior.BonusBahagiaPerabot("sofa") == 2, "Sofa +2");
        Periksa(SistemInterior.BonusBahagiaPerabot("lampu") == 1, "Lampu +1");
        Periksa(SistemInterior.NamaPerabot("tv") == "TV", "nama TV");
        Periksa(SistemInterior.NamaPerabot("bak_mandi") == "Bak Mandi", "nama Bak Mandi");
        Periksa(SistemInterior.NamaSpritePerabot("kasur") == "perabot-kasur.png",
            "sprite Kasur = perabot-kasur.png");
        Periksa(SistemInterior.NamaSpritePerabot("bak_mandi") == "perabot-bak_mandi.png",
            "sprite Bak Mandi = perabot-bak_mandi.png");
        // Melengkapi 1 hunian = Rp1.420.000, bonus +16.
        long totalHarga = 0;
        int totalBonus = 0;
        for (int i = 0; i < SistemInterior.JumlahSlot; i++)
        {
            totalHarga += SistemInterior.HargaPerabot(SistemInterior.IdPerabotSlot(i));
            totalBonus += SistemInterior.BonusBahagiaPerabot(SistemInterior.IdPerabotSlot(i));
        }
        Periksa(totalHarga == 1420000, "lengkapi 1 hunian = Rp1.420.000");
        Periksa(totalBonus == 16, "lengkapi 1 hunian = bonus +16");

        Console.WriteLine("== Tes C: beli / hapus perabot ==");
        var isi = SistemInterior.InteriorKosong();
        Periksa(SistemInterior.HitungPerabot(isi) == 0, "interior baru kosong (0 perabot)");
        Periksa(SistemInterior.ApakahKosong(isi), "interior baru = kosong");
        Periksa(!SistemInterior.ApakahLengkap(isi), "interior baru = belum lengkap");
        long koinBaru;
        Periksa(SistemInterior.Beli(isi, 0, 5000000, out koinBaru)
            && koinBaru == 5000000 - 150000 && isi[0] == "kasur",
            "beli Kasur: koin 5.000.000 -> 4.850.000, slot 0 terisi");
        Periksa(!SistemInterior.Beli(isi, 0, koinBaru, out koinBaru)
            && isi[0] == "kasur",
            "beli slot sudah terisi ditolak (slot tetap Kasur)");
        Periksa(!SistemInterior.Beli(isi, -1, 5000000, out koinBaru),
            "beli slot -1 ditolak");
        Periksa(!SistemInterior.Beli(isi, 10, 5000000, out koinBaru),
            "beli slot 10 ditolak");
        Periksa(!SistemInterior.Beli(isi, 7, 100000, out koinBaru),
            "beli TV (Rp300.000) dengan koin Rp100.000 ditolak");
        Periksa(SistemInterior.Beli(isi, 7, 500000, out koinBaru)
            && koinBaru == 200000 && isi[7] == "tv",
            "beli TV: koin 500.000 -> 200.000");
        Periksa(SistemInterior.HitungPerabot(isi) == 2, "2 perabot terpasang");
        Periksa(SistemInterior.HargaJualPerabot("tv") == 150000,
            "jual TV kembali 50% = Rp150.000");
        Periksa(SistemInterior.Hapus(isi, 7, 200000, out koinBaru)
            && koinBaru == 350000 && isi[7] == "",
            "hapus TV: koin 200.000 -> 350.000, slot 7 kosong");
        Periksa(!SistemInterior.Hapus(isi, 7, 350000, out koinBaru),
            "hapus slot kosong ditolak");
        Periksa(!SistemInterior.Hapus(isi, -1, 350000, out koinBaru),
            "hapus slot -1 ditolak");

        Console.WriteLine("== Tes D: bonus kebahagiaan interior & penalti rumah kosong ==");
        var kosong = SistemInterior.InteriorKosong();
        Periksa(SistemInterior.BonusBahagia(kosong) == -5,
            "rumah tanpa perabot = -5 (penghuni tidak betah)");
        var satuKasur = SistemInterior.InteriorKosong();
        satuKasur[0] = "kasur";
        Periksa(SistemInterior.BonusBahagia(satuKasur) == 2,
            "rumah berisi 1 Kasur = +2 (tanpa penalti kosong)");
        var lengkap = SistemInterior.InteriorKosong();
        for (int i = 0; i < SistemInterior.JumlahSlot; i++)
        {
            lengkap[i] = SistemInterior.IdPerabotSlot(i);
        }
        Periksa(SistemInterior.BonusBahagia(lengkap) == 16,
            "hunian lengkap 10 perabot = +16");
        Periksa(SistemInterior.ApakahLengkap(lengkap), "hunian lengkap = ApakahLengkap true");
        Periksa(SistemInterior.BatasiBonusTotal(25) == 20, "cap bonus total +25 -> +20");
        Periksa(SistemInterior.BatasiBonusTotal(-25) == -20, "cap bonus total -25 -> -20");
        Periksa(SistemInterior.BatasiBonusTotal(13) == 13, "bonus total +13 tidak di-cap");
        Periksa(SistemInterior.BatasiBonusTotal(-5) == -5, "bonus total -5 tidak di-cap");
        Periksa(BonusInteriorDunia(true) == 13,
            "bonus interior dunia standar (Kost lengkap) = -5 + 2 + 16 = +13");
        Periksa(BonusInteriorDunia(false) == -1,
            "bonus interior dunia standar (Kost 1 Kasur) = -5 + 2 + 2 = -1");
        // Normalisasi: id tidak dikenal dibuang, daftar pendek dipadatkan.
        var kotor = new List<string> { "kasur", "bukan_perabot", "tv" };
        var bersih = SistemInterior.Normalisasi(kotor);
        Periksa(bersih.Count == 10 && bersih[0] == "kasur" && bersih[1] == ""
            && bersih[2] == "tv", "normalisasi: id tidak dikenal dibuang, daftar jadi 10 slot");
        Periksa(SistemInterior.Normalisasi(null).Count == 10,
            "normalisasi null -> 10 slot kosong");

        Console.WriteLine("== Tes E: pengali sewa Kost berperabot lengkap ==");
        Periksa(GameBalance.PengaliSewaKostLengkap == 125,
            "pengali sewa Kost lengkap = 125%");
        var duniaLengkap = DuniaStandarFase4B(true);
        var ringkasanLengkap = EconomyCalculator.HitungHarian(duniaLengkap, 70, 100);
        Periksa(ringkasanLengkap.PemasukanLain == 50000,
            "sewa Kost lengkap: 40.000 x 125% = 50.000");
        var duniaBiasa = DuniaStandarFase4B(false);
        var ringkasanBiasa = EconomyCalculator.HitungHarian(duniaBiasa, 56, 100);
        Periksa(ringkasanBiasa.PemasukanLain == 40000,
            "sewa Kost 1 Kasur: 40.000 x 100% = 40.000");
        // Pengali 0 (catatan tes lama) dianggap 100 — kompatibilitas mundur.
        var duniaLama = new List<EconomyCalculator.CatatanBangunan>
        {
            Catatan("kost", 40000, 5000, 8, false, 0, false, 0)
        };
        Periksa(EconomyCalculator.HitungHarian(duniaLama, 50, 100).PemasukanLain == 40000,
            "pengali sewa 0 (tes lama) dianggap 100%");

        Console.WriteLine("== Tes F: kebahagiaan dunia standar Fase 4B ==");
        // Kost lengkap: 50 + 2 (taman) + 5 (semua punya rumah) + 13 = 70.
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 12, 12, 0, 0,
            BonusInteriorDunia(true)) == 70,
            "kebahagiaan dunia standar (Kost lengkap) = 70");
        // Kost 1 Kasur: 50 + 2 + 5 - 1 = 56.
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 12, 12, 0, 0,
            BonusInteriorDunia(false)) == 56,
            "kebahagiaan dunia standar (Kost 1 Kasur) = 56");
        // Semua hunian kosong: 50 + 2 + 5 - 15 = 42.
        Periksa(EconomyCalculator.HitungKebahagiaan(1, 12, 12, 0, 0, -15) == 42,
            "kebahagiaan 3 hunian kosong = 42 (interior -15)");
        Periksa(EconomyCalculator.PengaliWarung(70) == 100,
            "pengali warung di bahagia 70 = 100% (70 tidak > 70)");

        Console.WriteLine("== Tes G: dampak tiap pilihan peristiwa §9.1 ==");
        Periksa(SistemPeristiwaPenghuni.JumlahPilihan("tidak_bayar") == 3,
            "tidak bisa bayar punya 3 pilihan");
        Periksa(SistemPeristiwaPenghuni.JumlahPilihan("lari") == 2,
            "melarikan diri punya 2 pilihan");
        Periksa(SistemPeristiwaPenghuni.JumlahPilihan("tidak_ada") == 0,
            "id tidak dikenal punya 0 pilihan");
        // Tidak bisa bayar: keringanan / tagih tegas / cicil.
        PeriksaDampak("tidak_bayar", 0, 0, 5, 0, true, 0, 0,
            "keringanan: Rep +5, sewa Kost besok hilang");
        PeriksaDampak("tidak_bayar", 1, 20000, -5, 0, false, 2, 0,
            "tagih tegas: +Rp20.000, Rep -5, kapasitas hilang 2 besok");
        PeriksaDampak("tidak_bayar", 2, 10000, 0, 0, false, 0, 10000,
            "cicil: +Rp10.000 sekarang, +Rp10.000 besok");
        // Melarikan diri: biarkan / laporkan.
        PeriksaDampak("lari", 0, -40000, -3, 0, false, 0, 0,
            "biarkan lari: -Rp40.000, Rep -3");
        PeriksaDampak("lari", 1, -15000, 5, 0, false, 2, 0,
            "laporkan: -Rp15.000, Rep +5, kapasitas hilang 2 besok");
        // Tetangga berisik: tegur / abaikan.
        PeriksaDampak("berisik", 0, 0, 3, 3, false, 0, 0,
            "tegur tetangga: Rep +3, bahagia +3 besok");
        PeriksaDampak("berisik", 1, 0, 0, -5, false, 0, 0,
            "abaikan berisik: bahagia -5 besok");
        // Merusak perabot: potong deposit / maafkan.
        PeriksaDampak("rusak", 0, 25000, -3, 0, false, 0, 0,
            "potong deposit: +Rp25.000, Rep -3");
        PeriksaDampak("rusak", 1, -30000, 5, 0, false, 0, 0,
            "maafkan & perbaiki: -Rp30.000, Rep +5");
        // Rekomendasi teman: terima / tolak.
        PeriksaDampak("rekomendasi", 0, 0, 5, 2, false, 0, 0,
            "terima teman: Rep +5, bahagia +2 besok");
        PeriksaDampak("rekomendasi", 1, 0, -2, 0, false, 0, 0,
            "tolak teman: Rep -2");
        // Minta pindah kamar: setujui / tolak.
        PeriksaDampak("pindah", 0, 0, 0, 3, false, 0, 0,
            "setujui pindah: bahagia +3 besok");
        PeriksaDampak("pindah", 1, 0, -2, -3, false, 0, 0,
            "tolak pindah: Rep -2, bahagia -3 besok");
        // Pilihan di luar jangkauan = tanpa dampak; id tidak dikenal sama.
        PeriksaDampak("tidak_bayar", 3, 0, 0, 0, false, 0, 0,
            "pilihan 3 di peristiwa 3 pilihan = tanpa dampak");
        PeriksaDampak("lari", 2, 0, 0, 0, false, 0, 0,
            "pilihan 2 di peristiwa 2 pilihan = tanpa dampak");
        PeriksaDampak("tidak_ada", 0, 0, 0, 0, false, 0, 0,
            "id tidak dikenal = tanpa dampak");
        Periksa(SistemPeristiwaPenghuni.LabelPilihan("tidak_bayar", 3) == "",
            "label pilihan 3 di peristiwa 3 pilihan = kosong");
        Periksa(SistemPeristiwaPenghuni.TeksHasil("lari", 2) == "",
            "teks hasil pilihan 2 di peristiwa 2 pilihan = kosong");
        // Judul, deskripsi, label & teks hasil tidak kosong.
        Periksa(SistemPeristiwaPenghuni.Judul("tidak_bayar").Length > 0
            && SistemPeristiwaPenghuni.Deskripsi("tidak_bayar").Length > 0,
            "judul + deskripsi tidak bisa bayar terisi");
        Periksa(SistemPeristiwaPenghuni.LabelPilihan("tidak_bayar", 0).Length > 0
            && SistemPeristiwaPenghuni.LabelPilihan("tidak_bayar", 2).Length > 0,
            "label pilihan tidak bisa bayar terisi");
        Periksa(SistemPeristiwaPenghuni.TeksHasil("lari", 1).Length > 0,
            "teks hasil laporkan terisi (masuk laporan harian)");
        Periksa(SistemPeristiwaPenghuni.Judul("tidak_ada") == "",
            "judul id tidak dikenal kosong");

        Console.WriteLine("== Tes H: batas roll peristiwa (10%/hari, 6 jenis) ==");
        Periksa(GameBalance.PeluangPeristiwaPenghuniPerHari == 0.10,
            "peluang peristiwa 10%/hari");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0f) == "tidak_bayar",
            "roll 0.0000 -> tidak bisa bayar");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0166f) == "tidak_bayar",
            "roll 0.0166 -> tidak bisa bayar (di bawah 1/60)");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0167f) == "lari",
            "roll 0.0167 -> melarikan diri");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0333f) == "lari",
            "roll 0.0333 -> melarikan diri (di bawah 2/60)");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0334f) == "berisik",
            "roll 0.0334 -> tetangga berisik");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0499f) == "berisik",
            "roll 0.0499 -> tetangga berisik (di bawah 3/60)");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.05f) == "rusak",
            "roll 0.0500 -> merusak perabot");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0666f) == "rusak",
            "roll 0.0666 -> merusak perabot (di bawah 4/60)");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0667f) == "rekomendasi",
            "roll 0.0667 -> rekomendasi teman");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0833f) == "rekomendasi",
            "roll 0.0833 -> rekomendasi teman (di bawah 5/60)");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0834f) == "pindah",
            "roll 0.0834 -> minta pindah kamar");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.0999f) == "pindah",
            "roll 0.0999 -> minta pindah kamar");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.10f) == "",
            "roll 0.1000 -> tidak ada peristiwa");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(0.5f) == "",
            "roll 0.5000 -> tidak ada peristiwa");
        Periksa(SistemPeristiwaPenghuni.AcakPeristiwa(-0.01f) == "",
            "roll negatif -> tidak ada peristiwa");

        Console.WriteLine("== Tes I: clamp reputasi (0-100) ==");
        Periksa(GameBalance.ReputasiAwal == 50, "reputasi awal 50");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(50) == 50, "reputasi 50 tetap 50");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(0) == 0, "reputasi 0 tetap 0");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(100) == 100, "reputasi 100 tetap 100");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(-5) == 0, "reputasi -5 -> 0");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(150) == 100, "reputasi 150 -> 100");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(97 + 5) == 100,
            "reputasi 97 + 5 (terima teman) -> 100 (bukan 102)");
        Periksa(SistemPeristiwaPenghuni.BatasiReputasi(2 - 5) == 0,
            "reputasi 2 - 5 (tagih tegas) -> 0 (bukan -3)");

        Console.WriteLine("== Tes J: simulasi 10 hari dunia standar Fase 4B ==");
        // Kost lengkap: pemasukan 15.000 + 50.000 = 65.000; pajak
        // 65.000 x 25/1000 = 1.625; upkeep 1.000+1.000+5.000+2.000+500
        // = 9.500; bersih 65.000 - 1.625 - 9.500 = 53.875/hari.
        var r1 = EconomyCalculator.HitungHarian(DuniaStandarFase4B(true), 70, 100);
        Periksa(r1.PemasukanWarung == 15000, "10 hari (lengkap): warung 15.000/hari");
        Periksa(r1.PemasukanLain == 50000, "10 hari (lengkap): lain 50.000/hari");
        Periksa(r1.Pajak == 1625, "10 hari (lengkap): pajak 1.625/hari");
        Periksa(r1.Upkeep == 9500, "10 hari (lengkap): upkeep 9.500/hari");
        Periksa(r1.Bersih == 53875, "10 hari (lengkap): bersih 53.875/hari");
        Periksa(5000000 + 10 * 53875 == 5538750,
            "10 hari (lengkap): 5.000.000 -> 5.538.750");
        // Kost 1 Kasur: pemasukan 15.000 + 40.000 = 55.000; pajak
        // 55.000 x 25/1000 = 1.375; bersih 55.000 - 1.375 - 9.500
        // = 44.125/hari.
        var r2 = EconomyCalculator.HitungHarian(DuniaStandarFase4B(false), 56, 100);
        Periksa(r2.PemasukanWarung == 15000, "10 hari (1 Kasur): warung 15.000/hari");
        Periksa(r2.PemasukanLain == 40000, "10 hari (1 Kasur): lain 40.000/hari");
        Periksa(r2.Pajak == 1375, "10 hari (1 Kasur): pajak 1.375/hari");
        Periksa(r2.Bersih == 44125, "10 hari (1 Kasur): bersih 44.125/hari");
        Periksa(5000000 + 10 * 44125 == 5441250,
            "10 hari (1 Kasur): 5.000.000 -> 5.441.250");
        Periksa(5538750 - 5441250 == 97500,
            "Kost lengkap vs 1 Kasur: selisih 97.500 per 10 hari");

        Console.WriteLine("== Tes K: migrasi simpanan 4.0 -> 5.0 ==");
        Periksa(SaveData.VersiDidukung("5.1"), "versi 5.1 didukung (naik Balai Kota)");
        Periksa(SaveData.VersiDidukung("5.0"), "versi 5.0 didukung");
        Periksa(SaveData.VersiDidukung("4.0"), "versi 4.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("3.0"), "versi 3.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("2.0"), "versi 2.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("1.0"), "versi 1.0 didukung (migrasi)");
        Periksa(SaveData.VersiDidukung("6.0"), "versi 6.0 didukung (naik di Fase 7)");
        Periksa(!SaveData.VersiDidukung("7.0"), "versi 7.0 ditolak");
        var baru = new SaveData();
        Periksa(baru.versi == "6.0", "simpanan baru versi 6.0");
        Periksa(baru.reputasi == 0, "simpanan baru reputasi 0 (GameManager menormalkan 0 -> 50)");
        Periksa(baru.interior != null && baru.interior.Count == 0,
            "simpanan baru interior kosong (bukan null)");
        Periksa(baru.sewaKostHilangBesok == false
            && baru.kapasitasHilangBesok == 0
            && baru.tambahanKoinBesok == 0
            && baru.bonusBahagiaPeristiwaBesok == 0,
            "simpanan baru efek peristiwa besok kosong");
        var lama = new SaveData();
        lama.versi = "4.0";
        Periksa(lama.reputasi == 0 && lama.interior.Count == 0,
            "simpanan 4.0: field Fase 4B default aman");
        Periksa(GameBalance.VersiSimpan == "6.0", "VersiSimpan = 6.0");
        Periksa(GameBalance.VersiSimpanFase4B == "5.0", "VersiSimpanFase4B = 5.0");
        Periksa(GameBalance.VersiSimpanFase4 == "4.0", "VersiSimpanFase4 = 4.0");
        // Data interior tersimpan: 10 slot, normalisasi saat muat.
        var simpanInterior = new DataInteriorTersimpan();
        simpanInterior.x = 3;
        simpanInterior.y = 5;
        simpanInterior.perabot = new List<string> { "kasur", "tv" };
        var normal = SistemInterior.Normalisasi(simpanInterior.perabot);
        Periksa(normal.Count == 10 && normal[0] == "kasur" && normal[1] == "tv"
            && normal[2] == "", "interior tersimpan (3,5): 2 perabot, 10 slot sesudah muat");
        Periksa(GameBalance.HadiahMisiBeliPerabot == 25000,
            "hadiah misi Beli 1 Perabot Rp25.000");
        Periksa(GameBalance.HadiahMisiLengkapiKost == 75000,
            "hadiah misi Lengkapi 1 Kost Rp75.000");

        Console.WriteLine();
        Console.WriteLine("Hasil: " + lolos + " lolos, " + gagal + " gagal.");
        Environment.Exit(gagal == 0 ? 0 : 1);
    }
}
