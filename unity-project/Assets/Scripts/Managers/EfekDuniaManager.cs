// Dunia Kita — EfekDuniaManager
// Mengelola efek visual dunia agar terasa hidup:
//   1. Siklus siang-malam (warna kamera + tint bangunan)
//   2. Efek hujan saat kejadian Musim Hujan aktif
//   3. Teks melayang (+Rp / -Rp / info singkat) saat membangun/jual/gajian

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EfekDuniaManager : MonoBehaviour
{
    public static EfekDuniaManager Instance { get; private set; }

    private struct TeksMelayangItem
    {
        public GameObject Objek;
        public RectTransform Rect;
        public Text Teks;
        public float SisaDetik;
        public float DurasiTotal;
    }

    private Camera kameraUtama;
    private BuildManager buildManager;
    private Light matahari3D;
    private Canvas canvasEfek;
    private List<TeksMelayangItem> daftarTeks;

    // Partikel hujan sederhana (berbasis sprite/garis kecil di depan kamera).
    private GameObject wadahHujan;
    private List<Transform> tetesHujan;
    private bool hujanAktif;

    // Partikel ambien (daun, kupu-kupu, debu kilau, burung)
    private float timerAmbien;
    private const float IntervalAmbien = 1.2f;
    private const int MaksAmbien = 20;
    private List<PartikelAmbien> daftarAmbien;
    private bool malamSebelumnya;

    private void Awake()
    {
        Instance = this;
        daftarTeks = new List<TeksMelayangItem>();
        tetesHujan = new List<Transform>();
        daftarAmbien = new List<PartikelAmbien>();
    }

    private void Start()
    {
        kameraUtama = Camera.main;
        buildManager = FindObjectOfType<BuildManager>();
        SiapkanCanvasTeks();
        SiapkanHujan();
        SiapkanMatahari3D();
    }

    private void SiapkanMatahari3D()
    {
        matahari3D = FindObjectOfType<Light>();
        if (matahari3D == null)
        {
            GameObject go = new GameObject("Matahari3D");
            go.transform.SetParent(transform);
            matahari3D = go.AddComponent<Light>();
            matahari3D.type = LightType.Directional;
            matahari3D.intensity = 1.15f;
            go.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        }
    }

    private void SiapkanCanvasTeks()
    {
        GameObject go = new GameObject("CanvasEfekDunia");
        go.transform.SetParent(transform);
        canvasEfek = go.AddComponent<Canvas>();
        canvasEfek.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasEfek.sortingOrder = 50;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
    }

    private void SiapkanHujan()
    {
        wadahHujan = new GameObject("EfekHujan");
        wadahHujan.transform.SetParent(transform);
        // Buat tekstur putih kecil 2x8 untuk tetes hujan.
        Texture2D tex = new Texture2D(2, 8);
        Color[] px = new Color[16];
        for (int i = 0; i < px.Length; i++)
        {
            px[i] = new Color(0.75f, 0.88f, 1f, 0.55f);
        }
        tex.SetPixels(px);
        tex.Apply();
        Sprite spriteTetes = Sprite.Create(tex, new Rect(0, 0, 2, 8),
            new Vector2(0.5f, 0.5f), 16f);

        for (int i = 0; i < 45; i++)
        {
            GameObject tetes = new GameObject("Hujan_" + i);
            tetes.transform.SetParent(wadahHujan.transform);
            SpriteRenderer sr = tetes.AddComponent<SpriteRenderer>();
            sr.sprite = spriteTetes;
            sr.sortingOrder = 90;
            tetes.transform.position = new Vector3(
                Random.Range(-14f, 14f), Random.Range(-8f, 8f), 0f);
            tetes.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
            tetesHujan.Add(tetes.transform);
        }
        wadahHujan.SetActive(false);
    }

    private void Update()
    {
        PerbaruiSiklusWaktuDanCuaca();
        PerbaruiTeksMelayang();
        PerbaruiPartikelAmbien();
    }

    private void PerbaruiSiklusWaktuDanCuaca()
    {
        if (GameManager.Instance == null || GameManager.Instance.Pewaktu == null)
        {
            return;
        }
        float progres = SiklusWaktuVisual.HitungProgresHari(
            GameManager.Instance.Pewaktu.SisaDetik, GameBalance.DurasiHariDetik);
        bool sedangHujan = GameManager.Instance.KeadaanKejadian.PengaliHujanBesok < 100;
        bool malam = SiklusWaktuVisual.TentukanFase(progres) == SiklusWaktuVisual.FaseWaktu.Malam;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.AturSuasanaMalam(malam || sedangHujan);
        }

        if (kameraUtama != null)
        {
            float r, g, b;
            SiklusWaktuVisual.HitungWarnaLangit(progres, sedangHujan, out r, out g, out b);
            kameraUtama.backgroundColor = new Color(r, g, b, 1f);
        }

        if (matahari3D != null)
        {
            float tr, tg, tb;
            SiklusWaktuVisual.HitungTintDunia(progres, sedangHujan, out tr, out tg, out tb);
            matahari3D.color = new Color(tr, tg, tb, 1f);
            float malamF = SiklusWaktuVisual.HitungIntensitasMalam(progres);
            matahari3D.intensity = Mathf.Lerp(1.18f, 0.45f, malamF);
            matahari3D.transform.rotation = Quaternion.Euler(
                Mathf.Lerp(52f, 22f, progres), -35f + progres * 40f, 0f);
        }

        if (buildManager != null)
        {
            float tr, tg, tb;
            SiklusWaktuVisual.HitungTintDunia(progres, sedangHujan, out tr, out tg, out tb);
            Color tint = new Color(tr, tg, tb, 1f);
            foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
            {
                if (b != null && b.Objek != null)
                {
                    BuildingView view = b.Objek.GetComponent<BuildingView>();
                    if (view != null)
                    {
                        view.AturTintWaktu(tint);
                    }
                    // Cahaya jendela malam
                    CahayaMalamBangunan cahaya = b.Objek.GetComponent<CahayaMalamBangunan>();
                    if (cahaya != null)
                    {
                        cahaya.AturMalam(malam);
                    }
                }
            }
        }

        if (wadahHujan != null)
        {
            if (sedangHujan != hujanAktif)
            {
                hujanAktif = sedangHujan;
                wadahHujan.SetActive(hujanAktif);
            }
            if (hujanAktif && kameraUtama != null)
            {
                Vector3 camPos = kameraUtama.transform.position;
                for (int i = 0; i < tetesHujan.Count; i++)
                {
                    Transform t = tetesHujan[i];
                    t.position += new Vector3(-2.5f, -12f, 0f) * Time.deltaTime;
                    if (t.position.y < camPos.y - 9f)
                    {
                        t.position = new Vector3(
                            camPos.x + Random.Range(-14f, 14f),
                            camPos.y + Random.Range(7f, 11f), 0f);
                    }
                }
            }
        }
    }

    // Tampilkan teks melayang di atas posisi dunia tertentu.
    public void TampilkanTeksMelayang(Vector3 posisiDunia, string isi, Color warna)
    {
        if (canvasEfek == null || kameraUtama == null)
        {
            return;
        }
        Vector3 layar = kameraUtama.WorldToScreenPoint(posisiDunia);
        GameObject go = new GameObject("TeksMelayang");
        go.transform.SetParent(canvasEfek.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        float skalaX = 1920f / Mathf.Max(1, Screen.width);
        float skalaY = 1080f / Mathf.Max(1, Screen.height);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(layar.x * skalaX, layar.y * skalaY);
        rt.sizeDelta = new Vector2(420f, 60f);

        Text txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 30;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = warna;
        txt.text = isi;
        txt.raycastTarget = false;

        Shadow bayangan = go.AddComponent<Shadow>();
        bayangan.effectColor = new Color(0f, 0f, 0f, 0.75f);
        bayangan.effectDistance = new Vector2(2f, -2f);

        daftarTeks.Add(new TeksMelayangItem
        {
            Objek = go,
            Rect = rt,
            Teks = txt,
            SisaDetik = 1.4f,
            DurasiTotal = 1.4f
        });
    }

    private void PerbaruiTeksMelayang()
    {
        for (int i = daftarTeks.Count - 1; i >= 0; i--)
        {
            TeksMelayangItem item = daftarTeks[i];
            item.SisaDetik -= Time.deltaTime;
            if (item.SisaDetik <= 0f)
            {
                if (item.Objek != null)
                {
                    Destroy(item.Objek);
                }
                daftarTeks.RemoveAt(i);
                continue;
            }
            if (item.Rect != null)
            {
                item.Rect.anchoredPosition += new Vector2(0f, 70f * Time.deltaTime);
            }
            if (item.Teks != null)
            {
                Color c = item.Teks.color;
                c.a = Mathf.Clamp01(item.SisaDetik / (item.DurasiTotal * 0.5f));
                item.Teks.color = c;
            }
            daftarTeks[i] = item;
        }
    }

    private void PerbaruiPartikelAmbien()
    {
        if (kameraUtama == null || hujanAktif) return;
        for (int i = daftarAmbien.Count - 1; i >= 0; i--)
        {
            if (daftarAmbien[i] == null) daftarAmbien.RemoveAt(i);
        }
        if (daftarAmbien.Count >= MaksAmbien) return;

        timerAmbien += Time.deltaTime;
        if (timerAmbien < IntervalAmbien) return;
        timerAmbien = 0f;

        Vector3 cam = kameraUtama.transform.position;
        Vector3 pos = new Vector3(
            cam.x + Random.Range(-10f, 10f),
            cam.y + Random.Range(-5f, 6f),
            0f);

        GameObject go = new GameObject("PartikelAmbien");
        go.transform.SetParent(transform);
        PartikelAmbien p = go.AddComponent<PartikelAmbien>();
        int r = Random.Range(0, 4);
        PartikelAmbien.JenisPartikel jenis = (PartikelAmbien.JenisPartikel)r;
        p.Atur(jenis, pos);
        daftarAmbien.Add(p);
    }
}
