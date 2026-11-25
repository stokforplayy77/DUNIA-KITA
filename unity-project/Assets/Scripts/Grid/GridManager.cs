// Dunia Kita — GridManager
// Pemilik grid: menggambar petak tanah (sprite diamond), mencatat
// petak mana yang terisi bangunan, dan menampilkan sorotan petak
// yang sedang dipilih pemain.

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Sprite (diisi dari Scene)")]
    public Sprite tileSprite;       // Sprite diamond rumput
    public Sprite highlightSprite;  // Sprite diamond sorotan kuning

    private IsometricGrid grid;
    private bool[,] terisi;
    private GameObject sorotan;
    private Material matRumputA;
    private Material matRumputB;
    private Material matSorotan;
    private Material matRadius;
    private System.Collections.Generic.List<GameObject> sorotanRadius =
        new System.Collections.Generic.List<GameObject>();

    private void Awake()
    {
        grid = GetComponent<IsometricGrid>();
        if (grid == null)
        {
            grid = gameObject.AddComponent<IsometricGrid>();
        }
        terisi = new bool[grid.lebar, grid.tinggi];
        GambarTanah();
        BuatSorotan();
    }

    public IsometricGrid Grid
    {
        get { return grid; }
    }

    // Ukuran grid saat ini (12 = awal, 16 = sesudah perluasan lahan).
    public int UkuranGrid
    {
        get { return grid != null ? grid.lebar : GameBalance.UkuranGrid; }
    }

    // FASE 4 — perluasan lahan (GDD §8, Level 5): perbesar grid ke
    // ukuran baru (hanya membesar, tidak mengecil). Petak lama tetap
    // terisi seperti semula; petak baru digambar + dicatat kosong.
    // Mengembalikan false bila ukuran baru tidak lebih besar.
    public bool Perluas(int ukuranBaru)
    {
        if (grid == null || ukuranBaru <= grid.lebar || ukuranBaru <= grid.tinggi)
        {
            return false;
        }
        int lebarLama = grid.lebar;
        int tinggiLama = grid.tinggi;
        bool[,] terisiLama = terisi;
        terisi = new bool[ukuranBaru, ukuranBaru];
        for (int x = 0; x < lebarLama; x++)
        {
            for (int y = 0; y < tinggiLama; y++)
            {
                terisi[x, y] = terisiLama[x, y];
            }
        }
        grid.AturUkuran(ukuranBaru, ukuranBaru);
        HapusSemuaTile();
        GambarTanah();
        return true;
    }

    private void HapusSemuaTile()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform anak = transform.GetChild(i);
            if (anak != null && anak.name.StartsWith("Tile_"))
            {
                Destroy(anak.gameObject);
            }
        }
    }

    // Gambar petak tanah yang baru muncul karena perluasan (hanya
    // petak dengan x >= lebarLama ATAU y >= tinggiLama).
    private void GambarTanahBaru(int lebarLama, int tinggiLama)
    {
        if (tileSprite == null)
        {
            return;
        }
        for (int x = 0; x < grid.lebar; x++)
        {
            for (int y = 0; y < grid.tinggi; y++)
            {
                if (x < lebarLama && y < tinggiLama)
                {
                    continue;
                }
                GameObject petak = new GameObject("Tile_" + x + "_" + y);
                petak.transform.SetParent(transform);
                petak.transform.position = grid.GridKeDunia(new Vector2Int(x, y));
                SpriteRenderer sr = petak.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.sortingOrder = 0;
            }
        }
    }

    // Gambar seluruh petak tanah saat game dimulai.
    private void GambarTanah()
    {
        if (grid != null && grid.mode3D)
        {
            SiapkanMaterial3D();
            for (int x = 0; x < grid.lebar; x++)
            {
                for (int y = 0; y < grid.tinggi; y++)
                {
                    GameObject petak = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    petak.name = "Tile_" + x + "_" + y;
                    petak.transform.SetParent(transform);
                    Vector3 pos = grid.GridKeDunia(new Vector2Int(x, y));
                    petak.transform.position = new Vector3(pos.x, -0.08f, pos.z);
                    petak.transform.localScale = new Vector3(
                        grid.lebarTile * 0.97f, 0.16f, grid.lebarTile * 0.97f);
                    Renderer r = petak.GetComponent<Renderer>();
                    if (r != null)
                    {
                        r.sharedMaterial = ((x + y) % 2 == 0) ? matRumputA : matRumputB;
                    }
                }
            }
            return;
        }
        if (tileSprite == null)
        {
            Debug.LogWarning("[GridManager] tileSprite belum diisi di Scene.");
            return;
        }
        for (int x = 0; x < grid.lebar; x++)
        {
            for (int y = 0; y < grid.tinggi; y++)
            {
                GameObject petak = new GameObject("Tile_" + x + "_" + y);
                petak.transform.SetParent(transform);
                Vector3 pos = grid.GridKeDunia(new Vector2Int(x, y));
                petak.transform.position = pos;
                SpriteRenderer sr = petak.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.sortingOrder = 0;
                // Variasi warna rumput halus (agar tidak monoton)
                float variasi = ((x * 7919 + y * 6271) & 0x7FFFFFFF) % 100 / 100f;
                float h = 0.32f + variasi * 0.03f; // Hijau sedikit bervariasi
                float s = 0.42f + variasi * 0.08f;
                float v = 0.90f + variasi * 0.06f;
                sr.color = Color.HSVToRGB(h, s, v);
                // Dekorasi kecil (bunga, batu, semak)
                DekorasiTile.TambahDekorasiAcak(transform, pos, x, y);
            }
        }
    }

    private void SiapkanMaterial3D()
    {
        if (matRumputA == null)
        {
            matRumputA = BuatMaterial3D(new Color(0.50f, 0.85f, 0.36f, 1f));
            matRumputB = BuatMaterial3D(new Color(0.54f, 0.88f, 0.40f, 1f));
            matSorotan = BuatMaterial3D(new Color(1f, 0.85f, 0.25f, 0.75f));
            matRadius = BuatMaterial3D(new Color(0.30f, 0.80f, 1f, 0.55f));
        }
    }

    private static Material BuatMaterial3D(Color warna)
    {
        Shader s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Sprites/Default");
        Material m = new Material(s);
        m.color = warna;
        m.enableInstancing = true;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        return m;
    }

    private void BuatSorotan()
    {
        if (grid != null && grid.mode3D)
        {
            SiapkanMaterial3D();
            sorotan = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sorotan.name = "SorotanPetak";
            sorotan.transform.SetParent(transform);
            sorotan.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            sorotan.transform.localScale = new Vector3(grid.lebarTile * 0.96f, grid.lebarTile * 0.96f, 1f);
            Renderer r = sorotan.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = matSorotan;
            sorotan.SetActive(false);
            return;
        }
        sorotan = new GameObject("SorotanPetak");
        sorotan.transform.SetParent(transform);
        SpriteRenderer sr = sorotan.AddComponent<SpriteRenderer>();
        sr.sprite = highlightSprite;
        sr.sortingOrder = 5;
        sorotan.SetActive(false);
    }

    public void TampilkanSorotan(Vector2Int petak)
    {
        if (sorotan == null || !grid.DiDalamGrid(petak))
        {
            return;
        }
        Vector3 pos = grid.GridKeDunia(petak);
        sorotan.transform.position = grid.mode3D
            ? new Vector3(pos.x, 0.05f, pos.z)
            : pos;
        sorotan.SetActive(true);
    }

    public void SembunyikanSorotan()
    {
        if (sorotan != null)
        {
            sorotan.SetActive(false);
        }
        SembunyikanSorotanRadius();
    }

    // Tampilkan sorotan area jangkauan fasilitas (warna biru muda transparan).
    public void TampilkanSorotanRadius(System.Collections.Generic.List<JalanPathfinder.TitikGrid> petak)
    {
        SembunyikanSorotanRadius();
        if (petak == null) return;
        if (grid != null && grid.mode3D)
        {
            SiapkanMaterial3D();
            foreach (JalanPathfinder.TitikGrid t in petak)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "SorotanRadius";
                go.transform.SetParent(transform);
                Vector3 pos = grid.GridKeDunia(new Vector2Int(t.X, t.Y));
                go.transform.position = new Vector3(pos.x, 0.03f, pos.z);
                go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                go.transform.localScale = new Vector3(grid.lebarTile * 0.9f, grid.lebarTile * 0.9f, 1f);
                Renderer r = go.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = matRadius;
                sorotanRadius.Add(go);
            }
            return;
        }
        if (highlightSprite == null)
        {
            return;
        }
        foreach (JalanPathfinder.TitikGrid t in petak)
        {
            GameObject go = new GameObject("SorotanRadius");
            go.transform.SetParent(transform);
            go.transform.position = grid.GridKeDunia(new Vector2Int(t.X, t.Y));
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = highlightSprite;
            sr.color = new Color(0.4f, 0.85f, 1f, 0.35f);
            sr.sortingOrder = 4;
            sorotanRadius.Add(go);
        }
    }

    public void SembunyikanSorotanRadius()
    {
        for (int i = 0; i < sorotanRadius.Count; i++)
        {
            if (sorotanRadius[i] != null)
            {
                Destroy(sorotanRadius[i]);
            }
        }
        sorotanRadius.Clear();
    }

    // Cek apakah seluruh petak yang dibutuhkan bangunan masih kosong.
    public bool ApakahKosong(Vector2Int petak, Vector2Int ukuran)
    {
        for (int x = petak.x; x < petak.x + ukuran.x; x++)
        {
            for (int y = petak.y; y < petak.y + ukuran.y; y++)
            {
                Vector2Int p = new Vector2Int(x, y);
                if (!grid.DiDalamGrid(p))
                {
                    return false;
                }
                if (terisi[x, y])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void TandaiTerisi(Vector2Int petak, Vector2Int ukuran)
    {
        IsiPetak(petak, ukuran, true);
    }

    public void TandaiKosong(Vector2Int petak, Vector2Int ukuran)
    {
        IsiPetak(petak, ukuran, false);
    }

    private void IsiPetak(Vector2Int petak, Vector2Int ukuran, bool nilai)
    {
        for (int x = petak.x; x < petak.x + ukuran.x; x++)
        {
            for (int y = petak.y; y < petak.y + ukuran.y; y++)
            {
                if (grid.DiDalamGrid(new Vector2Int(x, y)))
                {
                    terisi[x, y] = nilai;
                }
            }
        }
    }
}
