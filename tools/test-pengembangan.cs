// Dunia Kita — tools/test-pengembangan.cs
// Unit test logika murni untuk fitur pengembangan baru:
//   A. JalanPathfinder (BFS di jaringan jalan)
//   B. SiklusWaktuVisual (fase pagi/siang/sore/malam & cuaca hujan)
//   C. SistemRadiusEfek (radius layanan, bonus bahagia, listrik)
//   D. SistemUpgradeBangunan (level 1-3, biaya, pemasukan, kapasitas)
//   E. SistemDiagnosisKota (analisis & saran prioritas kota)

using System;
using System.Collections.Generic;

public static class TesPengembangan
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
        UjiPathfinder();
        UjiSiklusWaktu();
        UjiRadiusEfek();
        UjiUpgradeBangunan();
        UjiDiagnosisKota();

        Console.WriteLine("HASIL TES PENGEMBANGAN: " + lolos + " lolos, " + gagal + " gagal");
        Environment.Exit(gagal == 0 ? 0 : 1);
    }

    private static void UjiPathfinder()
    {
        HashSet<long> jalan = new HashSet<long>
        {
            JalanPathfinder.Kunci(1, 1),
            JalanPathfinder.Kunci(2, 1),
            JalanPathfinder.Kunci(2, 2),
            JalanPathfinder.Kunci(3, 2),
            JalanPathfinder.Kunci(10, 10) // Terputus
        };

        List<JalanPathfinder.TitikGrid> jalur = JalanPathfinder.CariJalur(
            12, 12, jalan, 1, 1, 3, 2);
        Periksa(jalur.Count == 3, "Pathfinder: panjang jalur (1,1)->(3,2) harus 3");
        Periksa(jalur[jalur.Count - 1].X == 3 && jalur[jalur.Count - 1].Y == 2,
            "Pathfinder: titik akhir harus (3,2)");

        List<JalanPathfinder.TitikGrid> putus = JalanPathfinder.CariJalur(
            12, 12, jalan, 1, 1, 10, 10);
        Periksa(putus.Count == 0, "Pathfinder: jalan terputus harus mengembalikan list kosong");
    }

    private static void UjiSiklusWaktu()
    {
        Periksa(SiklusWaktuVisual.TentukanFase(0.10f) == SiklusWaktuVisual.FaseWaktu.Pagi,
            "SiklusWaktu: 0.10 harus Pagi");
        Periksa(SiklusWaktuVisual.TentukanFase(0.40f) == SiklusWaktuVisual.FaseWaktu.Siang,
            "SiklusWaktu: 0.40 harus Siang");
        Periksa(SiklusWaktuVisual.TentukanFase(0.70f) == SiklusWaktuVisual.FaseWaktu.Sore,
            "SiklusWaktu: 0.70 harus Sore");
        Periksa(SiklusWaktuVisual.TentukanFase(0.90f) == SiklusWaktuVisual.FaseWaktu.Malam,
            "SiklusWaktu: 0.90 harus Malam");

        Periksa(SiklusWaktuVisual.HitungIntensitasMalam(0.50f) == 0f,
            "SiklusWaktu: siang intensitas malam = 0");
        Periksa(SiklusWaktuVisual.HitungIntensitasMalam(0.90f) == 1f,
            "SiklusWaktu: malam penuh intensitas = 1");

        float rCerah, gCerah, bCerah, rHujan, gHujan, bHujan;
        SiklusWaktuVisual.HitungWarnaLangit(0.4f, false, out rCerah, out gCerah, out bCerah);
        SiklusWaktuVisual.HitungWarnaLangit(0.4f, true, out rHujan, out gHujan, out bHujan);
        Periksa(bHujan < bCerah, "SiklusWaktu: langit hujan lebih gelap dari cerah");
    }

    private static void UjiRadiusEfek()
    {
        List<SistemRadiusEfek.InfoPosisiBangunan> daftar =
            new List<SistemRadiusEfek.InfoPosisiBangunan>
        {
            new SistemRadiusEfek.InfoPosisiBangunan { Id = "rumah", X = 2, Y = 2, Lebar = 1, Tinggi = 1, TanpaJalan = false },
            new SistemRadiusEfek.InfoPosisiBangunan { Id = "taman", X = 4, Y = 2, Lebar = 1, Tinggi = 1, TanpaJalan = false },
            new SistemRadiusEfek.InfoPosisiBangunan { Id = "klinik", X = 3, Y = 3, Lebar = 2, Tinggi = 2, TanpaJalan = false },
            new SistemRadiusEfek.InfoPosisiBangunan { Id = "warung", X = 5, Y = 5, Lebar = 1, Tinggi = 1, TanpaJalan = false },
            new SistemRadiusEfek.InfoPosisiBangunan { Id = "pembangkit", X = 6, Y = 6, Lebar = 2, Tinggi = 2, TanpaJalan = false }
        };

        int bonus = SistemRadiusEfek.HitungBonusKebahagiaanRadius(daftar);
        Periksa(bonus == 4, "RadiusEfek: rumah dekat taman (+2) & klinik (+2) harus +4");
        Periksa(SistemRadiusEfek.ApakahTerjangkauListrik(3, daftar),
            "RadiusEfek: warung di (5,5) harus terjangkau pembangkit di (6,6)");
        Periksa(SistemRadiusEfek.PetakDalamRadius(5, 5, 1, 1, 2, 12).Count == 25,
            "RadiusEfek: radius 2 pada grid 12x12 harus 5x5 = 25 petak");
    }

    private static void UjiUpgradeBangunan()
    {
        Periksa(SistemUpgradeBangunan.BisaUpgrade("rumah", 1), "Upgrade: rumah Lv1 bisa upgrade");
        Periksa(!SistemUpgradeBangunan.BisaUpgrade("rumah", 3), "Upgrade: rumah Lv3 tidak bisa upgrade");
        Periksa(!SistemUpgradeBangunan.BisaUpgrade("jalan", 1), "Upgrade: jalan tidak bisa upgrade");
        Periksa(!SistemUpgradeBangunan.BisaUpgrade("balaikota", 1), "Upgrade: balaikota tidak bisa upgrade");

        Periksa(SistemUpgradeBangunan.BiayaUpgrade(1000000, 1) == 750000,
            "Upgrade: biaya Lv1->2 harus 75%");
        Periksa(SistemUpgradeBangunan.BiayaUpgrade(1000000, 2) == 1250000,
            "Upgrade: biaya Lv2->3 harus 125%");
        Periksa(SistemUpgradeBangunan.PengaliPemasukanPersen(2) == 135,
            "Upgrade: pengali pemasukan Lv2 = 135%");
        Periksa(SistemUpgradeBangunan.PengaliPemasukanPersen(3) == 175,
            "Upgrade: pengali pemasukan Lv3 = 175%");
        Periksa(SistemUpgradeBangunan.KapasitasEfektif(2, 2) == 3,
            "Upgrade: kapasitas rumah 2 di Lv2 = 3");
        Periksa(SistemUpgradeBangunan.KapasitasEfektif(2, 3) == 4,
            "Upgrade: kapasitas rumah 2 di Lv3 = 4");
    }

    private static void UjiDiagnosisKota()
    {
        SistemDiagnosisKota.InputDiagnosis bermasalah = new SistemDiagnosisKota.InputDiagnosis
        {
            Populasi = 10,
            KapasitasHunian = 8,
            Kebahagiaan = 45,
            JumlahTaman = 0,
            JumlahBangunanTanpaJalan = 2,
            HariMenunggak = 1,
            JumlahKomersial = 0,
            JumlahHunian = 2,
            PemasukanBersihHarian = -5000,
            LevelDunia = 1
        };
        List<string> saran = SistemDiagnosisKota.BuatSaran(bermasalah);
        Periksa(saran.Count >= 5, "Diagnosis: kota bermasalah harus memunculkan >= 5 saran");

        SistemDiagnosisKota.InputDiagnosis sehat = new SistemDiagnosisKota.InputDiagnosis
        {
            Populasi = 8,
            KapasitasHunian = 8,
            Kebahagiaan = 75,
            JumlahTaman = 2,
            JumlahBangunanTanpaJalan = 0,
            HariMenunggak = 0,
            JumlahKomersial = 2,
            JumlahHunian = 2,
            PemasukanBersihHarian = 20000,
            LevelDunia = 2
        };
        List<string> saranSehat = SistemDiagnosisKota.BuatSaran(sehat);
        Periksa(saranSehat.Count == 1 && saranSehat[0].Contains("sehat"),
            "Diagnosis: kota sehat harus memunculkan pesan sehat");
    }
}
