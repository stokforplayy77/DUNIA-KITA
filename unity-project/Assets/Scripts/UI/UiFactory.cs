// Dunia Kita — UiFactory
// Pembantu membangun elemen UI Unity (Canvas, teks, tombol, panel)
// lewat kode — karena Fase 1 membangun seluruh UI saat runtime
// (tidak ada prefab UI yang dibuat di Editor).
//
// Semua teks memakai font bawaan Unity (LegacyRuntime) supaya tidak butuh
// aset font tambahan. Warna memakai kode hex HTML.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UiFactory
{
    public static Canvas CanvasUtama { get; private set; }
    private static Sprite spriteBulat;

    // Sprite 9-slice dengan sudut membulat (radius 14px pada kanvas 64x64)
    // untuk tampilan UI modern di seluruh panel, tombol, dan input.
    private static Sprite SpriteKotakBulat()
    {
        if (spriteBulat != null)
        {
            return spriteBulat;
        }
        const int uk = 64;
        const float r = 14f;
        Texture2D tex = new Texture2D(uk, uk);
        Color[] px = new Color[uk * uk];
        for (int y = 0; y < uk; y++)
        {
            for (int x = 0; x < uk; x++)
            {
                float dx = 0f;
                if (x < r) dx = r - x;
                else if (x >= uk - r) dx = x - (uk - 1 - r);
                float dy = 0f;
                if (y < r) dy = r - y;
                else if (y >= uk - r) dy = y - (uk - 1 - r);
                float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);
                float alpha = dist <= r ? 1f : (dist < r + 1.2f ? 1f - (dist - r) / 1.2f : 0f);
                px[y * uk + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        spriteBulat = Sprite.Create(tex, new Rect(0, 0, uk, uk),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(16f, 16f, 16f, 16f));
        return spriteBulat;
    }

    private static void TerapkanGayaBulat(Image img)
    {
        img.sprite = SpriteKotakBulat();
        img.type = Image.Type.Sliced;
    }

    public static Color Warna(string hex)
    {
        Color hasil;
        if (ColorUtility.TryParseHtmlString(hex, out hasil))
        {
            return hasil;
        }
        return Color.white;
    }

    private static RectTransform safeAreaRoot;

    // Canvas layar penuh (Screen Space - Overlay) + scaler LANDSCAPE.
    // Acuan 1920x1080 (GDD §17): semua UI ditata dalam koordinat acuan
    // ini; scaler menyesuaikan ke ukuran layar HP sebenarnya.
    public static Canvas BuatCanvas(string nama)
    {
        GameObject go = new GameObject(nama);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        if (nama == "CanvasUI")
        {
            CanvasUtama = canvas;
            GameObject safeGo = new GameObject("SafeAreaRoot");
            RectTransform rt = safeGo.AddComponent<RectTransform>();
            rt.SetParent(go.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            safeGo.AddComponent<PenyesuaiSafeArea>();
            safeAreaRoot = rt;
        }
        return canvas;
    }

    // Selalu kembalikan CanvasUI utama (yang punya GraphicRaycaster),
    // bukan CanvasEfekDunia.
    public static Canvas CariCanvasUI()
    {
        if (CanvasUtama != null)
        {
            return CanvasUtama;
        }
        GameObject go = GameObject.Find("CanvasUI");
        if (go != null)
        {
            CanvasUtama = go.GetComponent<Canvas>();
            return CanvasUtama;
        }
        return Object.FindObjectOfType<Canvas>();
    }

    public static RectTransform BuatRect(GameObject go, RectTransform induk,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        if (safeAreaRoot != null && CanvasUtama != null && induk != null
            && induk == CanvasUtama.GetComponent<RectTransform>()
            && go.name != "SafeAreaRoot")
        {
            induk = safeAreaRoot;
        }
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = go.AddComponent<RectTransform>();
        }
        rt.SetParent(induk, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    // Posisi dengan sistem koordinat kiri-atas pada layar acuan 1920x1080:
    // (kiri, atas) = jarak dari sudut kiri atas layar, (lebar, tinggi)
    // = ukuran elemen. Dipakai semua panel supaya konsisten.
    public static void AturPosisi(RectTransform rt, float kiri, float atas,
        float lebar, float tinggi)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(kiri, -atas);
        rt.sizeDelta = new Vector2(lebar, tinggi);
    }

    // Panel persegi membulat modern dengan bayangan & garis tepi halus.
    public static GameObject BuatPanel(RectTransform induk, string nama,
        string warnaHex, float alpha)
    {
        GameObject go = new GameObject(nama);
        BuatRect(go, induk,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        Image img = go.AddComponent<Image>();
        TerapkanGayaBulat(img);
        Color warna = Warna(warnaHex);
        warna.a = alpha;
        img.color = warna;
        Shadow bayangan = go.AddComponent<Shadow>();
        bayangan.effectColor = new Color(0f, 0f, 0f, 0.28f);
        bayangan.effectDistance = new Vector2(0f, -6f);
        Outline garis = go.AddComponent<Outline>();
        garis.effectColor = new Color(1f, 1f, 1f, 0.18f);
        garis.effectDistance = new Vector2(2f, -2f);
        return go;
    }

    // Teks dengan font bawaan Unity.
    public static Text BuatTeks(RectTransform induk, string nama, string isi,
        int ukuran, string warnaHex, TextAnchor posisi)
    {
        GameObject go = new GameObject(nama);
        BuatRect(go, induk,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f));
        Text teks = go.AddComponent<Text>();
        teks.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        teks.text = isi;
        teks.fontSize = ukuran;
        teks.color = Warna(warnaHex);
        teks.alignment = posisi;
        teks.horizontalOverflow = HorizontalWrapMode.Wrap;
        teks.verticalOverflow = VerticalWrapMode.Truncate;
        teks.raycastTarget = false;
        return teks;
    }

    // Tombol penuh (latar + label di tengah) dengan aksi klik.
    // Tinggi tombol di layar acuan minimal ~90 unit supaya mudah disentuh
    // di HP (GDD §10: target sentuh memadai). ukuranFont bisa dikecilkan
    // untuk tombol sempit (mis. tombol di bar atas).
    public static Button BuatTombol(RectTransform induk, string nama,
        string label, string warnaLatarHex, string warnaTeksHex,
        UnityAction aksi, int ukuranFont = 40)
    {
        GameObject go = new GameObject(nama);
        RectTransform rt = BuatRect(go, induk,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f));
        rt.sizeDelta = new Vector2(0f, 110f);
        Image latar = go.AddComponent<Image>();
        TerapkanGayaBulat(latar);
        Color warnaDasar = Warna(warnaLatarHex);
        latar.color = warnaDasar;
        Shadow bayangan = go.AddComponent<Shadow>();
        bayangan.effectColor = new Color(0f, 0f, 0f, 0.25f);
        bayangan.effectDistance = new Vector2(0f, -4f);
        Button tombol = go.AddComponent<Button>();
        ColorBlock cb = ColorBlock.defaultColorBlock;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        cb.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        tombol.colors = cb;
        // FASE 5: semua tombol bersuara tap (satu tempat — semua
        // tombol game dibuat lewat sini).
        tombol.onClick.AddListener(delegate { AudioManager.MainkanTap(); });
        tombol.onClick.AddListener(aksi);
        GameObject labelGo = new GameObject("Label");
        BuatRect(labelGo, rt,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f));
        Text labelTeks = labelGo.AddComponent<Text>();
        labelTeks.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelTeks.text = label;
        labelTeks.fontSize = ukuranFont;
        labelTeks.color = Warna(warnaTeksHex);
        labelTeks.alignment = TextAnchor.MiddleCenter;
        return tombol;
    }

    // Tombol IKON saja tanpa tulisan (Fase 3 revisi desain: tombol
    // "Bangun" jadi ikon palu+rumah). Ikon 96x96 di tengah tombol,
    // menjaga rasio aslinya; tinggi tombol tetap diatur belakangan
    // lewat AturPosisi (di bar bawah 120 unit).
    public static Button BuatTombolIkon(RectTransform induk, string nama,
        Sprite ikon, string warnaLatarHex, UnityAction aksi)
    {
        GameObject go = new GameObject(nama);
        RectTransform rt = BuatRect(go, induk,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f));
        rt.sizeDelta = new Vector2(0f, 110f);
        Image latar = go.AddComponent<Image>();
        TerapkanGayaBulat(latar);
        Color warnaDasar = Warna(warnaLatarHex);
        latar.color = warnaDasar;
        Shadow bayangan = go.AddComponent<Shadow>();
        bayangan.effectColor = new Color(0f, 0f, 0f, 0.25f);
        bayangan.effectDistance = new Vector2(0f, -4f);
        Button tombol = go.AddComponent<Button>();
        ColorBlock cb = ColorBlock.defaultColorBlock;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        cb.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        tombol.colors = cb;
        // FASE 5: tombol ikon juga bersuara tap (seperti BuatTombol).
        tombol.onClick.AddListener(delegate { AudioManager.MainkanTap(); });
        tombol.onClick.AddListener(aksi);
        GameObject ikonGo = new GameObject("Ikon");
        RectTransform ikonRt = BuatRect(ikonGo, rt,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        ikonRt.sizeDelta = new Vector2(96f, 96f);
        Image ikonImg = ikonGo.AddComponent<Image>();
        ikonImg.sprite = ikon;
        ikonImg.preserveAspect = true;
        return tombol;
    }

    // Area gulir vertikal (Fase 4): menu bangun (17 bangunan) dan
    // panel misi (8 tutorial + 3 harian) tidak muat dalam satu layar,
    // jadi isinya digulir. Struktur: wadah (posisi di dalam panel
    // induk) -> viewport (RectMask2D memotong isi di luar area) ->
    // konten (anak-anak diposisikan dari kiri-atas konten dengan
    // AturPosisi seperti biasa). Mengembalikan RectTransform konten;
    // tinggi konten diatur belakangan lewat isi.sizeDelta.
    public static RectTransform BuatScroll(RectTransform induk, string nama,
        float kiri, float atas, float lebar, float tinggi)
    {
        GameObject wadahGo = new GameObject(nama);
        RectTransform wadahRt = BuatRect(wadahGo, induk,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f));
        AturPosisi(wadahRt, kiri, atas, lebar, tinggi);

        GameObject viewportGo = new GameObject("Viewport");
        RectTransform viewportRt = BuatRect(viewportGo, wadahRt,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f));
        viewportGo.AddComponent<RectMask2D>();

        GameObject kontenGo = new GameObject("Konten");
        RectTransform kontenRt = BuatRect(kontenGo, viewportRt,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f));
        kontenRt.anchoredPosition = Vector2.zero;
        kontenRt.sizeDelta = new Vector2(lebar, tinggi);

        ScrollRect gulir = wadahGo.AddComponent<ScrollRect>();
        gulir.content = kontenRt;
        gulir.viewport = viewportRt;
        gulir.horizontal = false;
        gulir.vertical = true;
        return kontenRt;
    }

    // Teks label di dalam tombol (untuk mengubah tulisan tombol belakangan,
    // mis. tombol Suara Nyala/Mati di Panel Pengaturan).
    public static Text LabelTombol(Button tombol)
    {
        return tombol.GetComponentInChildren<Text>();
    }

    // FASE 7: kolom isian teks (PanelAkun: nama, kata sandi, alamat
    // server). Latar putih + garis tepi gelap, teks isi + teks
    // penanda (placeholder) abu-abu. Kata sandi memakai
    // ContentType.Password (berbintang di Editor / build).
    public static InputField BuatInput(RectTransform induk, string nama,
        string penanda, InputField.ContentType jenis, int ukuranFont = 36)
    {
        GameObject go = new GameObject(nama);
        RectTransform rt = BuatRect(go, induk,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f));
        rt.sizeDelta = new Vector2(0f, 90f);
        Image latar = go.AddComponent<Image>();
        latar.color = Warna("#FFFFFF");

        GameObject isiGo = new GameObject("TeksIsi");
        BuatRect(isiGo, rt,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f));
        Text teksIsi = isiGo.AddComponent<Text>();
        teksIsi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        teksIsi.text = "";
        teksIsi.fontSize = ukuranFont;
        teksIsi.color = Warna("#2F3B2C");
        teksIsi.alignment = TextAnchor.MiddleLeft;

        GameObject tandaGo = new GameObject("Penanda");
        BuatRect(tandaGo, rt,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f));
        Text teksTanda = tandaGo.AddComponent<Text>();
        teksTanda.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        teksTanda.text = penanda;
        teksTanda.fontSize = ukuranFont;
        teksTanda.color = Warna("#8A8A8A");
        teksTanda.alignment = TextAnchor.MiddleLeft;

        InputField kolom = go.AddComponent<InputField>();
        kolom.textComponent = teksIsi;
        kolom.placeholder = teksTanda;
        kolom.contentType = jenis;
        return kolom;
    }
}

// Menyesuaikan RectTransform SafeAreaRoot agar UI tidak tertutup notch/poni HP.
public class PenyesuaiSafeArea : MonoBehaviour
{
    private RectTransform rt;
    private Rect areaTerakhir;
    private int lebarTerakhir;
    private int tinggiTerakhir;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        Terapkan();
    }

    private void Update()
    {
        if (Screen.width != lebarTerakhir || Screen.height != tinggiTerakhir
            || !Sama(Screen.safeArea, areaTerakhir))
        {
            Terapkan();
        }
    }

    private static bool Sama(Rect a, Rect b)
    {
        return Mathf.Abs(a.x - b.x) < 0.5f && Mathf.Abs(a.y - b.y) < 0.5f
            && Mathf.Abs(a.width - b.width) < 0.5f && Mathf.Abs(a.height - b.height) < 0.5f;
    }

    private void Terapkan()
    {
        if (rt == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect aman = Screen.safeArea;
        areaTerakhir = aman;
        lebarTerakhir = Screen.width;
        tinggiTerakhir = Screen.height;
        rt.anchorMin = new Vector2(aman.x / Screen.width, aman.y / Screen.height);
        rt.anchorMax = new Vector2((aman.x + aman.width) / Screen.width,
            (aman.y + aman.height) / Screen.height);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
