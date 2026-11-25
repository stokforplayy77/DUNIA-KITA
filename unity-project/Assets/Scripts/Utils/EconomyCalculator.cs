// Dunia Kita — EconomyCalculator
// Logika ekonomi MURNI (tanpa dependensi UnityEngine) sehingga bisa
// dikompilasi dan diuji di luar Unity (lihat tools/test-ekonomi.cs
// dan tools/test-fase2.cs). Semua hitung-hitungan harian game memakai
// class ini.
//
// FASE 2: pemasukan warung dipengaruhi kebahagiaan, pajak kota 2,5%
// dari pemasukan kotor, dan pembayaran harian bisa menunggak bila
// koin tidak cukup (tanpa bangkrut langsung — tunggakan dicatat dan
// kebahagiaan turun bila menunggak 2 hari berturut-turut).

using System.Collections.Generic;

public static class EconomyCalculator
{
    // Satu baris data bangunan yang dibutuhkan perhitungan ekonomi.
    // Sengaja berupa struct sederhana, bukan BuildingData, supaya
    // class ini tidak bergantung pada UnityEngine.
    public struct CatatanBangunan
    {
        public string Id;
        public long PemasukanHarian;
        public long BiayaPerawatanHarian;
        public int KapasitasWarga;
        public bool AdalahTaman;
        public bool AdalahWarung;
        // FASE 4: bonus kebahagiaan dari .asset (BuildingData.bonusBahagia;
        // Taman = 0, dihitung khusus lewat jumlahTaman) dan apakah
        // bangunan ini TIDAK bersebelahan dengan Jalan (GDD §5.5:
        // pemasukan & bonus kebahagiaan jadi 50%).
        public int BonusBahagia;
        public bool TanpaJalan;
        // FASE 4B: pengali pemasukan (persen) dari interior — Kost
        // berperabot lengkap = 125 (+25% sewa, GDD §5.6). 0 berarti
        // "tidak diisi" (tes lama) dan diperlakukan sebagai 100.
        public int PengaliSewaPersen;
    }

    // Posisi satu bangunan di grid (untuk aturan Jalan — murni, tanpa
    // dependensi GridManager/BuildManager supaya bisa diuji).
    public struct PetakBangunan
    {
        public string Id;
        public int X;
        public int Y;
        public int Lebar;
        public int Tinggi;
    }

    // FASE 4 — aturan Jalan (GDD §5.5): bangunan berfungsi penuh bila
    // salah satu petak di sekeliling area-nya (4 arah: atas/bawah/
    // kiri/kanan, TIDAK diagonal) ditempati Jalan (id "jalan").
    // Mengembalikan satu bool per bangunan (seurutan dengan daftar):
    // true = TIDAK ada Jalan di sebelahnya. Jalan itu sendiri selalu
    // false (jalan tidak butuh jalan).
    public static bool[] HitungTanpaJalan(IList<PetakBangunan> daftar)
    {
        bool[] hasil = new bool[daftar.Count];
        for (int i = 0; i < daftar.Count; i++)
        {
            PetakBangunan b = daftar[i];
            if (b.Id == "jalan")
            {
                hasil[i] = false;
                continue;
            }
            bool adaJalan = false;
            for (int j = 0; j < daftar.Count && !adaJalan; j++)
            {
                if (daftar[j].Id != "jalan")
                {
                    continue;
                }
                PetakBangunan jalan = daftar[j];
                // Jalan 1x1 di petak (jx, jy) bersebelahan bila petak
                // itu tepat di luar salah satu sisi area bangunan.
                bool diSampingKiri = jalan.X == b.X - 1
                    && jalan.Y >= b.Y && jalan.Y < b.Y + b.Tinggi;
                bool diSampingKanan = jalan.X == b.X + b.Lebar
                    && jalan.Y >= b.Y && jalan.Y < b.Y + b.Tinggi;
                bool diSampingBawah = jalan.Y == b.Y - 1
                    && jalan.X >= b.X && jalan.X < b.X + b.Lebar;
                bool diSampingAtas = jalan.Y == b.Y + b.Tinggi
                    && jalan.X >= b.X && jalan.X < b.X + b.Lebar;
                adaJalan = diSampingKiri || diSampingKanan
                    || diSampingBawah || diSampingAtas;
            }
            hasil[i] = !adaJalan;
        }
        return hasil;
    }

    // FASE 4 — bonus kebahagiaan dari fasilitas (di luar Taman):
    // jumlahkan BonusBahagia tiap bangunan; bangunan tanpa Jalan
    // hanya memberi setengah (dibulatkan ke bawah). Taman tanpa
    // Jalan memberi -1 (penalti dari bonus +2/taman yang dihitung
    // terpisah lewat jumlahTaman di HitungKebahagiaan).
    public static int HitungBonusFasilitas(IList<CatatanBangunan> bangunan)
    {
        int total = 0;
        foreach (CatatanBangunan b in bangunan)
        {
            if (b.AdalahTaman)
            {
                if (b.TanpaJalan)
                {
                    total -= 1;
                }
                continue;
            }
            if (b.BonusBahagia == 0)
            {
                continue;
            }
            total += b.TanpaJalan
                ? b.BonusBahagia * GameBalance.PengaliTanpaJalan / 100
                : b.BonusBahagia;
        }
        return total;
    }

    public struct RingkasanHarian
    {
        public long Pemasukan;    // Pemasukan kotor (sesudah pengali warung)
        public long PemasukanWarung; // Bagian pemasukan dari warung (sesudah pengali)
        public long PemasukanLain;   // Pemasukan dari selain warung
        public long Pajak;        // Pajak kota 2,5% dari pemasukan kotor
        public long Upkeep;       // Total biaya perawatan bangunan
        public long Pengeluaran;  // Upkeep + pajak
        public long Bersih;       // Pemasukan - Pengeluaran (boleh negatif)
        // Diisi EconomyManager setelah pembayaran (bukan oleh HitungHarian).
        public bool Menunggak;    // Koin tidak cukup membayar hari ini
        public long Tunggakan;    // Total tunggakan sesudah hari ini
        public int HariMenunggakBerturut; // Hari menunggak berturut-turut
    }

    // Hasil pembayaran satu hari (murni, tanpa akses GameManager).
    public struct HasilPembayaran
    {
        public long KoinBaru;
        public long TunggakanBaru;
        public bool Menunggak;
    }

    // Pengali pemasukan warung dari kebahagiaan (persen):
    // < 40 -> 80% (warga murung, warung sepi), > 70 -> 110%
    // (warga senang, warung ramai), selebihnya 100%.
    public static int PengaliWarung(int kebahagiaan)
    {
        if (kebahagiaan < GameBalance.AmbangBahagiaRendah)
        {
            return GameBalance.PengaliWarungRendah;
        }
        if (kebahagiaan > GameBalance.AmbangBahagiaTinggi)
        {
            return GameBalance.PengaliWarungTinggi;
        }
        return 100;
    }

    // Hitung pemasukan & pengeluaran total untuk satu hari game.
    // kebahagiaan = kebahagiaan saat ini (dari hari sebelumnya) —
    // menentukan pengali pemasukan warung hari ini.
    // pengaliHujan = pengali pemasukan warung dari cuaca besok
    // (Fase 3: 100 = normal, 80 = hujan tanpa payung, 90 = dengan
    // payung) — dikalikan bersama pengali kebahagiaan.
    // FASE 4: bangunan tanpa Jalan (CatatanBangunan.TanpaJalan) hanya
    // menghasilkan 50% pemasukan (GDD §5.5); pembulatan ke bawah di
    // tiap langkah (perkalian bertahap dengan /100).
    public static RingkasanHarian HitungHarian(IList<CatatanBangunan> bangunan,
        int kebahagiaan, int pengaliHujan)
    {
        int pengali = PengaliWarung(kebahagiaan);
        long masukWarung = 0;
        long masukLain = 0;
        long upkeep = 0;
        foreach (CatatanBangunan b in bangunan)
        {
            long masuk = b.PemasukanHarian;
            if (b.AdalahWarung)
            {
                masuk = masuk * pengali / 100;
                masuk = masuk * pengaliHujan / 100;
            }
            // FASE 4B: pengali sewa interior (Kost lengkap = 125%).
            // Nilai 0 (catatan lama/tes lama) berarti 100%.
            int pengaliSewa = b.PengaliSewaPersen <= 0
                ? 100 : b.PengaliSewaPersen;
            masuk = masuk * pengaliSewa / 100;
            if (b.TanpaJalan)
            {
                masuk = masuk * GameBalance.PengaliTanpaJalan / 100;
            }
            if (b.AdalahWarung)
            {
                masukWarung += masuk;
            }
            else
            {
                masukLain += masuk;
            }
            upkeep += b.BiayaPerawatanHarian;
        }
        long kotor = masukWarung + masukLain;
        long pajak = kotor * GameBalance.PajakKotaPerMil / 1000;
        long keluar = upkeep + pajak;
        return new RingkasanHarian
        {
            Pemasukan = kotor,
            PemasukanWarung = masukWarung,
            PemasukanLain = masukLain,
            Pajak = pajak,
            Upkeep = upkeep,
            Pengeluaran = keluar,
            Bersih = kotor - keluar,
            Menunggak = false,
            Tunggakan = 0,
            HariMenunggakBerturut = 0
        };
    }

    // Tanpa kebahagiaan: pakai 50 (netral, pengali warung 100%).
    // Dipakai panel perkiraan dan tes Fase 1.
    public static RingkasanHarian HitungHarian(IList<CatatanBangunan> bangunan)
    {
        return HitungHarian(bangunan, GameBalance.KebahagiaanAwal, 100);
    }

    // Tanpa hujan: pengali hujan 100 (normal).
    public static RingkasanHarian HitungHarian(IList<CatatanBangunan> bangunan,
        int kebahagiaan)
    {
        return HitungHarian(bangunan, kebahagiaan, 100);
    }

    // Bayar hasil harian dari koin yang ada.
    //   total = koin + bersih (bersih boleh negatif).
    //   total >= 0: koin jadi total, lalu lunasi tunggakan lama
    //   sebisanya dari koin itu. Tidak menunggak.
    //   total < 0: koin jadi 0, kekurangannya ditambah ke tunggakan.
    //   Menunggak hari ini.
    public static HasilPembayaran HitungPembayaran(long koin, long bersih,
        long tunggakanLama)
    {
        long total = koin + bersih;
        if (total >= 0)
        {
            long koinBaru = total;
            long bayar = tunggakanLama;
            if (bayar > koinBaru)
            {
                bayar = koinBaru;
            }
            koinBaru -= bayar;
            return new HasilPembayaran
            {
                KoinBaru = koinBaru,
                TunggakanBaru = tunggakanLama - bayar,
                Menunggak = false
            };
        }
        return new HasilPembayaran
        {
            KoinBaru = 0,
            TunggakanBaru = tunggakanLama + (-total),
            Menunggak = true
        };
    }

    // Populasi = total kapasitas hunian yang sudah dibangun.
    public static int HitungPopulasi(IList<CatatanBangunan> bangunan)
    {
        int total = 0;
        foreach (CatatanBangunan b in bangunan)
        {
            total += b.KapasitasWarga;
        }
        return total;
    }

    // FASE 2 — kebahagiaan:
    //   mulai 50
    //   +2 per taman, maks +20 dari taman
    //   +5 bila ada warga dan semua warga punya rumah (tidak ada tunawisma)
    //   -3 bila warga melebihi kapasitas hunian (tunawisma)
    //   -5 bila upkeep menunggak 2 hari berturut-turut atau lebih
    //   +bonusSementara (Fase 3: festival desa +10 untuk besok)
    //   dibatasi 0..100.
    // Catatan: dengan bangunan Fase 1, populasi selalu sama dengan
    // kapasitas (warga langsung menempati rumah yang dibangun), jadi
    // aturan tunawisma (-3) belum bisa terpicu dalam permainan normal —
    // aturan ini diuji dengan angka sintetis dan baru relevan bila ada
    // bangunan/pindah warga yang membuat populasi melebihi kapasitas
    // (Fase 3 ke atas). Lihat CATATAN-VERIFIKASI.md.
    public static int HitungKebahagiaan(int jumlahTaman, int jumlahWarga,
        int kapasitasHunian, int hariMenunggakBerturut)
    {
        return HitungKebahagiaan(jumlahTaman, jumlahWarga, kapasitasHunian,
            hariMenunggakBerturut, 0);
    }

    public static int HitungKebahagiaan(int jumlahTaman, int jumlahWarga,
        int kapasitasHunian, int hariMenunggakBerturut, int bonusSementara)
    {
        return HitungKebahagiaan(jumlahTaman, jumlahWarga, kapasitasHunian,
            hariMenunggakBerturut, bonusSementara, 0);
    }

    // FASE 4: + bonusFasilitas (dari HitungBonusFasilitas — bonus
    // kebahagiaan fasilitas + penalti taman tanpa Jalan).
    public static int HitungKebahagiaan(int jumlahTaman, int jumlahWarga,
        int kapasitasHunian, int hariMenunggakBerturut, int bonusSementara,
        int bonusFasilitas)
    {
        int nilai = GameBalance.KebahagiaanAwal;
        int bonusTaman = jumlahTaman * GameBalance.BonusKebahagiaanPerTaman;
        if (bonusTaman > GameBalance.MaksBonusKebahagiaanTaman)
        {
            bonusTaman = GameBalance.MaksBonusKebahagiaanTaman;
        }
        nilai += bonusTaman;
        int tunawisma = jumlahWarga - kapasitasHunian;
        if (tunawisma < 0)
        {
            tunawisma = 0;
        }
        if (jumlahWarga > 0 && tunawisma == 0)
        {
            nilai += GameBalance.BonusBahagiaSemuaPunyaRumah;
        }
        if (tunawisma > 0)
        {
            nilai -= GameBalance.HukumanBahagiaTunawisma;
        }
        if (hariMenunggakBerturut >= 2)
        {
            nilai -= GameBalance.HukumanBahagiaMenunggak;
        }
        nilai += bonusSementara;
        nilai += bonusFasilitas;
        if (nilai < 0)
        {
            nilai = 0;
        }
        if (nilai > GameBalance.KebahagiaanMaks)
        {
            nilai = GameBalance.KebahagiaanMaks;
        }
        return nilai;
    }

    // Harga jual kembali = 50% dari biaya bangun (dibulatkan ke bawah).
    public static long HitungHargaJual(long biayaBangun, float rasio)
    {
        return (long)(biayaBangun * rasio);
    }
}
