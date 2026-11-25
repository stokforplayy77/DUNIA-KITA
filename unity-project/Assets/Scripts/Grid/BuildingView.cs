// Dunia Kita — BuildingView
// Komponen visual pada satu bangunan yang sudah terpasang di dunia.
// Menempel pada prefab generik "Bangunan" (Assets/Prefabs/);
// data dan sprite diisi saat runtime oleh BuildManager.
//
// FASE 4B — mode interior tembus pandang (GDD §5.6): saat pemain
// membuka interior hunian, sprite luar diganti sprite denah
// interior (4 ruangan) DI TEMPAT yang sama; perabot dan 1-2 warga
// penghuni ditambahkan sebagai anak (child) di atas denah. Tutup
// interior = sprite luar kembali, anak-anak dihapus.

using System.Collections.Generic;
using UnityEngine;

public class BuildingView : MonoBehaviour
{
    public BuildingData Data { get; private set; }
    public Vector2Int PosisiGrid { get; private set; }

    private SpriteRenderer spriteRenderer;
    // Sprite luar (disimpan saat Atur) supaya bisa kembali sesudah
    // mode interior ditutup.
    private Sprite spriteLuar;
    // Anak-anak interior (perabot + warga penghuni) yang dibuat
    // InteriorManager — dihapus semua saat mode interior ditutup.
    private List<GameObject> anakInterior;
    private Color tintWaktu = Color.white;
    private float timerAnimasi = -1f;
    private float durasiAnimasi = 0.25f;
    private bool animasiBangun;
    private float faseIdle;
    private float timerAsap;
    private static Sprite spriteAsapCache;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        anakInterior = new List<GameObject>();
        faseIdle = Random.Range(0f, 6.28f);
    }

    private void Update()
    {
        if (timerAnimasi >= 0f)
        {
            timerAnimasi += Time.deltaTime;
            float t = Mathf.Clamp01(timerAnimasi / durasiAnimasi);
            float skala;
            if (animasiBangun)
            {
                // Pop-in: 0.35 -> 1.08 -> 1.0
                skala = t < 0.7f
                    ? Mathf.Lerp(0.35f, 1.08f, t / 0.7f)
                    : Mathf.Lerp(1.08f, 1.0f, (t - 0.7f) / 0.3f);
            }
            else
            {
                // Tap bounce: 1.0 -> 1.06 -> 1.0
                skala = 1f + Mathf.Sin(t * Mathf.PI) * 0.06f;
            }
            transform.localScale = new Vector3(skala, skala, 1f);
            if (t >= 1f)
            {
                transform.localScale = Vector3.one;
                timerAnimasi = -1f;
            }
            return;
        }

        // Animasi idle hidup: ayunan lembut pohon/taman & nafas halus bangunan,
        // plus kepulan asap cerobong untuk rumah/kost/pembangkit/warung/kafe.
        if (Data != null && Data.id != "jalan")
        {
            float waktu = Time.time + faseIdle;
            if (Data.id == "taman" || Data.id == "alun_alun")
            {
                float ayun = Mathf.Sin(waktu * 1.8f) * 0.018f;
                transform.localScale = new Vector3(1f + ayun, 1f - ayun * 0.6f, 1f);
            }
            else
            {
                float nafas = Mathf.Sin(waktu * 1.4f) * 0.008f;
                transform.localScale = new Vector3(1f, 1f + nafas, 1f);
            }

            if (Data.id == "rumah" || Data.id == "kost" || Data.id == "pembangkit"
                || Data.id == "warung" || Data.id == "kafe")
            {
                timerAsap += Time.deltaTime;
                float interval = Data.id == "pembangkit" ? 0.85f : 1.8f;
                if (timerAsap >= interval)
                {
                    timerAsap = 0f;
                    MunculkanAsap();
                }
            }
        }
    }

    private void MunculkanAsap()
    {
        if (spriteAsapCache == null)
        {
            Texture2D tex = new Texture2D(16, 16);
            Color[] px = new Color[256];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float dx = (x - 7.5f) / 7.5f;
                    float dy = (y - 7.5f) / 7.5f;
                    float a = Mathf.Clamp01(1f - (dx * dx + dy * dy));
                    px[y * 16 + x] = new Color(1f, 1f, 1f, a * 0.7f);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            spriteAsapCache = Sprite.Create(tex, new Rect(0, 0, 16, 16),
                new Vector2(0.5f, 0.5f), 32f);
        }
        GameObject asap = new GameObject("Asap");
        asap.transform.SetParent(transform, false);
        asap.transform.localPosition = new Vector3(0.15f, 0.75f, 0f);
        SpriteRenderer sr = asap.AddComponent<SpriteRenderer>();
        sr.sprite = spriteAsapCache;
        sr.sortingOrder = UrutanGambar + 3;
        asap.AddComponent<PartikelAsap>();
    }

    public void MulaiAnimasiBangun()
    {
        animasiBangun = true;
        durasiAnimasi = 0.25f;
        timerAnimasi = 0f;
        transform.localScale = new Vector3(0.35f, 0.35f, 1f);
    }

    public void MulaiAnimasiTap()
    {
        animasiBangun = false;
        durasiAnimasi = 0.16f;
        timerAnimasi = 0f;
    }

    public void AturTintWaktu(Color tint)
    {
        tintWaktu = tint;
        if (spriteRenderer != null && Data != null)
        {
            spriteRenderer.color = Data.warna * tintWaktu;
        }
    }

    // Tampilkan indikator level kecil di atas bangunan bila level > 1.
    public void PerbaruiVisualLevel(int level)
    {
        Transform lama = transform.Find("LencanaLevel");
        if (level <= 1)
        {
            if (lama != null) Destroy(lama.gameObject);
            return;
        }
        GameObject lencana = lama != null ? lama.gameObject : new GameObject("LencanaLevel");
        lencana.transform.SetParent(transform, false);
        lencana.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        TextMesh tm = lencana.GetComponent<TextMesh>();
        if (tm == null) tm = lencana.AddComponent<TextMesh>();
        tm.text = level == 2 ? "★★" : "★★★";
        tm.fontSize = 24;
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = level == 3 ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.75f, 0.92f, 1f, 1f);
        MeshRenderer mr = lencana.GetComponent<MeshRenderer>();
        if (mr != null && spriteRenderer != null)
        {
            mr.sortingOrder = spriteRenderer.sortingOrder + 2;
        }
    }

    // Dipanggil BuildManager tepat setelah prefab dibuat.
    public void Atur(BuildingData data, Vector2Int posisi, IsometricGrid grid)
    {
        Data = data;
        PosisiGrid = posisi;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        gameObject.name = "Bangunan_" + data.id + "_" + posisi.x + "_" + posisi.y;
        Vector3 pusat = grid.GridKeDuniaTengah(posisi, data.ukuran);

        if (grid != null && grid.mode3D)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform anak = transform.GetChild(i);
                if (anak != null && anak.name != "LencanaLevel")
                {
                    Destroy(anak.gameObject);
                }
            }
            Model3DBangunan.BangunModel3D(data, transform);
            transform.position = new Vector3(pusat.x, 0f, pusat.z);
            return;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        spriteRenderer.sprite = data.sprite;
        spriteLuar = data.sprite;
        spriteRenderer.color = data.warna;
        int sudutDepan = posisi.x + posisi.y
            + (data.ukuran.x - 1) + (data.ukuran.y - 1);
        spriteRenderer.sortingOrder = data.id == "jalan" ? 2 : 500 - sudutDepan;
        transform.position = new Vector3(pusat.x, pusat.y + data.offsetVisualY, 0f);
        // Bayangan lembut di bawah bangunan (bukan jalan)
        if (data.id != "jalan")
        {
            BayanganBangunan.TambahBayangan(gameObject, data.ukuran);
        }
        // Cahaya jendela malam (bangunan berpenghuni)
        if (data.id != "jalan" && data.id != "taman"
            && data.id != "alun_alun" && GetComponent<CahayaMalamBangunan>() == null)
        {
            CahayaMalamBangunan cahaya = gameObject.AddComponent<CahayaMalamBangunan>();
            cahaya.Atur(data.id, data.ukuran);
        }
    }

    // Dibersihkan: sprite jalan.png sudah menyambung rapi antar petak isometrik.
    public void PerbaruiKoneksiJalan(bool kananX, bool kiriX, bool atasY, bool bawahY, IsometricGrid grid)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform t = transform.GetChild(i);
            if (t != null && t.name.StartsWith("Koneksi"))
            {
                Destroy(t.gameObject);
            }
        }
    }

    // Urutan gambar sprite luar (untuk anak interior: digambar di atas).
    public int UrutanGambar
    {
        get { return spriteRenderer != null ? spriteRenderer.sortingOrder : 10; }
    }

    // FASE 4B — mode interior: di 3D sembunyikan atap & dinding luar lalu
    // bangun sekat 4 ruangan 3D; di 2D ganti sprite luar dengan denah.
    public void TampilkanInterior(Sprite spriteInterior)
    {
        IsometricGrid grid = FindObjectOfType<IsometricGrid>();
        if (grid != null && grid.mode3D)
        {
            Model3DInterior.BersihkanInterior3D(transform);
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform anak = transform.GetChild(i);
                if (anak != null && (anak.name.StartsWith("Atap")
                    || anak.name == "Badan" || anak.name == "Pintu"
                    || anak.name == "Jendela"))
                {
                    anak.gameObject.SetActive(false);
                }
            }
            Model3DInterior.BangunRuangan3D(Data, transform);
            return;
        }
        if (spriteRenderer == null || spriteInterior == null)
        {
            return;
        }
        spriteRenderer.sprite = spriteInterior;
    }

    // Kembali ke tampilan luar (dipanggil saat mode interior ditutup).
    public void TampilkanLuar()
    {
        IsometricGrid grid = FindObjectOfType<IsometricGrid>();
        if (grid != null && grid.mode3D)
        {
            Model3DInterior.BersihkanInterior3D(transform);
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform anak = transform.GetChild(i);
                if (anak != null && (anak.name.StartsWith("Atap")
                    || anak.name == "Badan" || anak.name == "Pintu"
                    || anak.name == "Jendela"))
                {
                    anak.gameObject.SetActive(true);
                }
            }
            return;
        }
        if (spriteRenderer == null || spriteLuar == null)
        {
            return;
        }
        spriteRenderer.sprite = spriteLuar;
    }

    // Tambah satu anak interior (perabot / warga penghuni) pada
    // posisi lokal relatif terhadap tengah bangunan. Mengembalikan
    // GameObject anak supaya pemanggil bisa melacaknya.
    public GameObject TambahAnakInterior(Sprite sprite, Vector2 offsetLokal,
        float skala, int urutan)
    {
        if (sprite == null)
        {
            return null;
        }
        if (anakInterior == null)
        {
            anakInterior = new List<GameObject>();
        }
        GameObject anak = new GameObject("InteriorAnak_" + anakInterior.Count);
        anak.transform.SetParent(transform, false);
        anak.transform.localPosition = new Vector3(offsetLokal.x, offsetLokal.y, 0f);
        anak.transform.localScale = new Vector3(skala, skala, 1f);
        SpriteRenderer sr = anak.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = urutan;
        anakInterior.Add(anak);
        return anak;
    }

    // Hapus semua anak interior (perabot + warga) saat mode ditutup.
    public void BersihkanAnakInterior()
    {
        if (anakInterior == null)
        {
            return;
        }
        foreach (GameObject anak in anakInterior)
        {
            if (anak != null)
            {
                Destroy(anak);
            }
        }
        anakInterior.Clear();
    }
}
