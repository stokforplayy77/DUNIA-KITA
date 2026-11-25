// Dunia Kita — JalanPathfinder
// Logika MURNI pathfinding BFS di atas jaringan petak Jalan (tanpa
// dependensi UnityEngine, supaya bisa diuji di luar Unity).
// Dipakai PejalanKaki untuk berjalan mengikuti jalanan kota.

using System.Collections.Generic;

public static class JalanPathfinder
{
    public struct TitikGrid
    {
        public int X;
        public int Y;

        public TitikGrid(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    private static readonly int[] DX = { 1, -1, 0, 0 };
    private static readonly int[] DY = { 0, 0, 1, -1 };

    public static long Kunci(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    // Cari jalur terpendek dari (asalX, asalY) ke (tujuanX, tujuanY)
    // hanya melewati petak yang ada di `petakJalan`.
    // Mengembalikan daftar titik (tidak termasuk asal, termasuk tujuan).
    // Bila tidak ada jalur atau asal == tujuan, kembalikan list kosong.
    public static List<TitikGrid> CariJalur(
        int lebar, int tinggi,
        HashSet<long> petakJalan,
        int asalX, int asalY,
        int tujuanX, int tujuanY)
    {
        List<TitikGrid> hasil = new List<TitikGrid>();
        if (petakJalan == null || petakJalan.Count == 0)
        {
            return hasil;
        }
        if (asalX == tujuanX && asalY == tujuanY)
        {
            return hasil;
        }
        long kunciTujuan = Kunci(tujuanX, tujuanY);
        if (!petakJalan.Contains(kunciTujuan))
        {
            return hasil;
        }

        Queue<TitikGrid> antrean = new Queue<TitikGrid>();
        Dictionary<long, long> induk = new Dictionary<long, long>();
        long kunciAsal = Kunci(asalX, asalY);
        antrean.Enqueue(new TitikGrid(asalX, asalY));
        induk[kunciAsal] = kunciAsal;

        bool ketemu = false;
        while (antrean.Count > 0)
        {
            TitikGrid sekarang = antrean.Dequeue();
            if (sekarang.X == tujuanX && sekarang.Y == tujuanY)
            {
                ketemu = true;
                break;
            }
            for (int i = 0; i < 4; i++)
            {
                int nx = sekarang.X + DX[i];
                int ny = sekarang.Y + DY[i];
                if (nx < 0 || ny < 0 || nx >= lebar || ny >= tinggi)
                {
                    continue;
                }
                long kn = Kunci(nx, ny);
                if (!petakJalan.Contains(kn) || induk.ContainsKey(kn))
                {
                    continue;
                }
                induk[kn] = Kunci(sekarang.X, sekarang.Y);
                antrean.Enqueue(new TitikGrid(nx, ny));
            }
        }

        if (!ketemu)
        {
            return hasil;
        }

        // Susun balik jalur dari tujuan ke asal.
        long jejak = kunciTujuan;
        while (jejak != kunciAsal)
        {
            int jx = (int)(jejak >> 32);
            int jy = (int)(jejak & 0xFFFFFFFF);
            hasil.Add(new TitikGrid(jx, jy));
            jejak = induk[jejak];
        }
        hasil.Reverse();
        return hasil;
    }
}
