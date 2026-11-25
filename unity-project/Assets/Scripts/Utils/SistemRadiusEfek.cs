// Dunia Kita — SistemRadiusEfek
// Logika MURNI jangkauan (radius) fasilitas kota (tanpa dependensi
// UnityEngine, supaya bisa diuji di luar Unity).
// Menjadikan tata letak kota sebagai strategi nyata: hunian & toko
// mendapat bonus bila berada di dalam radius fasilitas aktif.

using System;
using System.Collections.Generic;

public static class SistemRadiusEfek
{
    public struct InfoPosisiBangunan
    {
        public string Id;
        public int X;
        public int Y;
        public int Lebar;
        public int Tinggi;
        public bool TanpaJalan;
    }

    // Radius jangkauan (petak) untuk bangunan fasilitas. 0 = tidak punya radius.
    public static int RadiusUntuk(string id)
    {
        switch (id)
        {
            case "taman": return 3;
            case "pos": return 4;
            case "perpustakaan": return 4;
            case "sekolah": return 5;
            case "klinik": return 5;
            case "pembangkit": return 6;
            case "alun_alun": return 6;
            case "kampus": return 6;
            default: return 0;
        }
    }

    public static bool ApakahHunian(string id)
    {
        return id == "rumah" || id == "kost"
            || id == "apartemen" || id == "perumahan";
    }

    public static bool ApakahKomersial(string id)
    {
        return id == "warung" || id == "toko_kelontong"
            || id == "pasar" || id == "kafe";
    }

    // Jarak Chebyshev antar dua bangunan (tepi terdekat ke tepi terdekat).
    public static int JarakBangunan(InfoPosisiBangunan a, InfoPosisiBangunan b)
    {
        int dx = 0;
        if (a.X + a.Lebar - 1 < b.X) dx = b.X - (a.X + a.Lebar - 1);
        else if (b.X + b.Lebar - 1 < a.X) dx = a.X - (b.X + b.Lebar - 1);

        int dy = 0;
        if (a.Y + a.Tinggi - 1 < b.Y) dy = b.Y - (a.Y + a.Tinggi - 1);
        else if (b.Y + b.Tinggi - 1 < a.Y) dy = a.Y - (b.Y + b.Tinggi - 1);

        return Math.Max(dx, dy);
    }

    // Bonus kebahagiaan sinergi lokal (maks +15) dari hunian yang
    // tercakup radius fasilitas (taman, sekolah, klinik, perpustakaan,
    // pos keamanan, alun-alun).
    public static int HitungBonusKebahagiaanRadius(IList<InfoPosisiBangunan> daftar)
    {
        if (daftar == null || daftar.Count == 0)
        {
            return 0;
        }
        int totalBonus = 0;
        for (int i = 0; i < daftar.Count; i++)
        {
            InfoPosisiBangunan target = daftar[i];
            if (!ApakahHunian(target.Id))
            {
                continue;
            }
            bool dapatTaman = false;
            bool dapatLayanan = false;
            for (int j = 0; j < daftar.Count; j++)
            {
                if (i == j) continue;
                InfoPosisiBangunan fas = daftar[j];
                if (fas.TanpaJalan) continue;
                int rad = RadiusUntuk(fas.Id);
                if (rad <= 0) continue;
                if (JarakBangunan(target, fas) <= rad)
                {
                    if (fas.Id == "taman" || fas.Id == "alun_alun")
                    {
                        dapatTaman = true;
                    }
                    else
                    {
                        dapatLayanan = true;
                    }
                }
            }
            if (dapatTaman) totalBonus += 2;
            if (dapatLayanan) totalBonus += 2;
        }
        return totalBonus > 15 ? 15 : totalBonus;
    }

    // Apakah bangunan komersial ini terjangkau Pembangkit Listrik aktif?
    // Bila ya, mendapat bonus efisiensi listrik (+10% pemasukan).
    public static bool ApakahTerjangkauListrik(int indeksTarget, IList<InfoPosisiBangunan> daftar)
    {
        if (daftar == null || indeksTarget < 0 || indeksTarget >= daftar.Count)
        {
            return false;
        }
        InfoPosisiBangunan target = daftar[indeksTarget];
        for (int j = 0; j < daftar.Count; j++)
        {
            InfoPosisiBangunan fas = daftar[j];
            if (fas.Id != "pembangkit" || fas.TanpaJalan) continue;
            if (JarakBangunan(target, fas) <= RadiusUntuk("pembangkit"))
            {
                return true;
            }
        }
        return false;
    }

    // Daftar petak dalam radius untuk sorotan visual di GridManager.
    public static List<JalanPathfinder.TitikGrid> PetakDalamRadius(
        int asalX, int asalY, int lebar, int tinggi, int radius, int ukuranGrid)
    {
        List<JalanPathfinder.TitikGrid> hasil = new List<JalanPathfinder.TitikGrid>();
        if (radius <= 0) return hasil;
        int minX = Math.Max(0, asalX - radius);
        int maxX = Math.Min(ukuranGrid - 1, asalX + lebar - 1 + radius);
        int minY = Math.Max(0, asalY - radius);
        int maxY = Math.Min(ukuranGrid - 1, asalY + tinggi - 1 + radius);
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                hasil.Add(new JalanPathfinder.TitikGrid(x, y));
            }
        }
        return hasil;
    }
}
