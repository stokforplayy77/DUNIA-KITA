// Dunia Kita — IsometricGrid
// Matematika grid isometric 2,5D (GDD §4).
//
// PILIHAN DESAIN (dicatat sesuai tugas): grid KUSTOM, bukan Tilemap Unity.
// Alasan: logika penempatan bangunan, pengecekan hunian petak, dan
// simpan/muat dunia jauh lebih sederhana dengan grid data sendiri;
// tile hanya digambar sebagai sprite diamond oleh GridManager.
// Rumus konversi standar isometric 2:1:
//
//   dunia.x = (grid.x - grid.y) * (lebarTile / 2)
//   dunia.y = (grid.x + grid.y) * (tinggiTile / 2)
//
// Titik (0,0) grid digeser supaya seluruh grid berada di tengah layar
// (pusat grid di titik dunia 0,0).

using UnityEngine;

public class IsometricGrid : MonoBehaviour
{
    public bool mode3D = false;
    public int lebar = GameBalance.UkuranGrid;
    public int tinggi = GameBalance.UkuranGrid;
    public float lebarTile = GameBalance.LebarTile;
    public float tinggiTile = GameBalance.TinggiTile;

    // Offset supaya pusat grid berada di (0,0) dunia.
    private Vector2 offsetPusat;
    private bool offsetDihitung;

    private void Awake()
    {
        PastikanOffset();
    }

    // Dihitung malas (lazy) supaya konversi tetap benar bila
    // dipanggil sebelum Awake — berguna juga untuk pengujian.
    private void PastikanOffset()
    {
        if (offsetDihitung)
        {
            return;
        }
        HitungOffsetPusat();
        offsetDihitung = true;
    }

    private void HitungOffsetPusat()
    {
        float tengahX = (lebar - 1) * 0.5f;
        float tengahY = (tinggi - 1) * 0.5f;
        if (mode3D)
        {
            offsetPusat = new Vector2(tengahX * lebarTile, tengahY * lebarTile);
            return;
        }
        float pusatX = (tengahX - tengahY) * (lebarTile * 0.5f);
        float pusatY = (tengahX + tengahY) * (tinggiTile * 0.5f);
        offsetPusat = new Vector2(pusatX, pusatY);
    }

    // FASE 4 — perluasan lahan (GDD §8, Level 5): ubah ukuran grid
    // saat runtime (12x12 -> 16x16). Offset pusat dihitung ulang
    // (tandai belum dihitung supaya PastikanOffset menghitung lagi
    // dengan lebar/tinggi baru). Dipanggil GridManager.Perluas;
    // GridManager yang menggambar petak baru + menyalin data terisi.
    public void AturUkuran(int lebarBaru, int tinggiBaru)
    {
        if (lebarBaru < 1 || tinggiBaru < 1)
        {
            return;
        }
        lebar = lebarBaru;
        tinggi = tinggiBaru;
        offsetDihitung = false;
    }

    // Petak grid -> posisi dunia (tengah petak).
    public Vector3 GridKeDunia(Vector2Int petak)
    {
        PastikanOffset();
        if (mode3D)
        {
            return new Vector3(
                petak.x * lebarTile - offsetPusat.x,
                0f,
                petak.y * lebarTile - offsetPusat.y);
        }
        float x = (petak.x - petak.y) * (lebarTile * 0.5f) - offsetPusat.x;
        float y = (petak.x + petak.y) * (tinggiTile * 0.5f) - offsetPusat.y;
        return new Vector3(x, y, 0f);
    }

    // Titik tengah AREA berukuran lebar x tinggi petak.
    public Vector3 GridKeDuniaTengah(Vector2Int pojok, Vector2Int ukuran)
    {
        Vector3 a = GridKeDunia(pojok);
        Vector3 b = GridKeDunia(new Vector2Int(pojok.x + ukuran.x - 1, pojok.y));
        Vector3 c = GridKeDunia(new Vector2Int(pojok.x, pojok.y + ukuran.y - 1));
        Vector3 d = GridKeDunia(new Vector2Int(
            pojok.x + ukuran.x - 1, pojok.y + ukuran.y - 1));
        return new Vector3(
            (a.x + b.x + c.x + d.x) * 0.25f,
            (a.y + b.y + c.y + d.y) * 0.25f,
            (a.z + b.z + c.z + d.z) * 0.25f);
    }

    // Posisi dunia -> petak grid terdekat.
    public Vector2Int DuniaKeGrid(Vector3 posisiDunia)
    {
        PastikanOffset();
        if (mode3D)
        {
            float tx = (posisiDunia.x + offsetPusat.x) / lebarTile;
            float tz = (posisiDunia.z + offsetPusat.y) / lebarTile;
            return new Vector2Int(Mathf.RoundToInt(tx), Mathf.RoundToInt(tz));
        }
        float x = posisiDunia.x + offsetPusat.x;
        float y = posisiDunia.y + offsetPusat.y;
        float gx = (x / (lebarTile * 0.5f) + y / (tinggiTile * 0.5f)) * 0.5f;
        float gy = (y / (tinggiTile * 0.5f) - x / (lebarTile * 0.5f)) * 0.5f;
        return new Vector2Int(Mathf.RoundToInt(gx), Mathf.RoundToInt(gy));
    }

    public bool DiDalamGrid(Vector2Int petak)
    {
        return petak.x >= 0 && petak.x < lebar && petak.y >= 0 && petak.y < tinggi;
    }

    // Sudut-sudut dunia grid (untuk membatasi gerak kamera).
    public void BatasDunia(out float minX, out float maxX, out float minY, out float maxY)
    {
        PastikanOffset();
        if (mode3D)
        {
            Vector3 p0 = GridKeDunia(new Vector2Int(0, 0));
            Vector3 p1 = GridKeDunia(new Vector2Int(lebar - 1, tinggi - 1));
            minX = p0.x - lebarTile;
            maxX = p1.x + lebarTile;
            minY = p0.z - lebarTile;
            maxY = p1.z + lebarTile;
            return;
        }
        Vector3 a = GridKeDunia(new Vector2Int(0, 0));
        Vector3 b = GridKeDunia(new Vector2Int(lebar - 1, 0));
        Vector3 c = GridKeDunia(new Vector2Int(0, tinggi - 1));
        Vector3 d = GridKeDunia(new Vector2Int(lebar - 1, tinggi - 1));
        minX = Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c.x, d.x)) - lebarTile;
        maxX = Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c.x, d.x)) + lebarTile;
        minY = Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c.y, d.y)) - tinggiTile;
        maxY = Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c.y, d.y)) + tinggiTile;
    }
}
