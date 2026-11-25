// Dunia Kita — UI/PanelPeristiwaMultiplayer
// Menampilkan daftar peristiwa & sengketa di Dunia Bersama (seperti
// penyerobotan tanah, pencurian, kebisingan, darurat RS/Klinik, subsidi)
// dengan pilihan aduan ke Polisi atau Wali Kota.

using UnityEngine;
using UnityEngine.UI;

public class PanelPeristiwaMultiplayer
{
    public static PanelPeristiwaMultiplayer Instance { get; private set; }

    private GameObject panel;
    private RectTransform isiDaftar;
    private Text statusTeks;

    public static void Tampilkan()
    {
        PastikanAda();
        if (Instance != null)
        {
            Instance.Buka();
        }
    }

    public static void PerbaruiDaftar(PeristiwaDuniaDto[] daftar)
    {
        if (Instance != null)
        {
            Instance.RenderDaftar(daftar);
        }
    }

    public static void PastikanAda()
    {
        if (Instance != null) return;
        Canvas canvas = UiFactory.CariCanvasUI();
        if (canvas == null) return;
        Instance = new PanelPeristiwaMultiplayer();
        Instance.Bangun(canvas.GetComponent<RectTransform>());
    }

    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelPeristiwaMultiplayer", "#FAF6EE", 0.98f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        UiFactory.AturPosisi(rt, 360, 120, 1200, 820);

        Text judul = UiFactory.BuatTeks(rt, "JudulPeristiwaMP",
            "Laporan & Peristiwa Dunia Bersama", 42, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 40, 20, 900, 60);

        Button tutup = UiFactory.BuatTombol(rt, "TombolTutupPeristiwaMP",
            "Tutup", "#C0392B", "#FFFFFF", delegate { Sembunyikan(); }, 28);
        UiFactory.AturPosisi(tutup.GetComponent<RectTransform>(), 1000, 20, 160, 60);

        statusTeks = UiFactory.BuatTeks(rt, "StatusPeristiwaMP",
            "Memuat peristiwa...", 28, "#5A6B7A", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(statusTeks.GetComponent<RectTransform>(), 40, 88, 1120, 44);

        isiDaftar = UiFactory.BuatScroll(rt, "ScrollPeristiwaMP", 40, 140, 1120, 640);
        panel.SetActive(false);
    }

    private void Buka()
    {
        panel.SetActive(true);
        statusTeks.text = "Mengambil laporan terbaru dari server...";
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            ServerManager.Instance.MintaDaftarPeristiwa();
        }
        else
        {
            statusTeks.text = "Masuk ke Dunia Bersama dulu untuk melihat peristiwa multiplayer.";
        }
    }

    public void Sembunyikan()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void RenderDaftar(PeristiwaDuniaDto[] daftar)
    {
        if (isiDaftar == null) return;
        foreach (Transform anak in isiDaftar)
        {
            Object.Destroy(anak.gameObject);
        }
        if (daftar == null || daftar.Length == 0)
        {
            statusTeks.text = "Belum ada laporan peristiwa aktif saat ini.";
            isiDaftar.sizeDelta = new Vector2(1120f, 100f);
            return;
        }
        statusTeks.text = "Pilih tindakan untuk tiap laporan (Polisi atau Wali Kota):";
        float atas = 10f;
        foreach (PeristiwaDuniaDto ev in daftar)
        {
            PeristiwaDuniaDto item = ev;
            GameObject kartu = UiFactory.BuatPanel(isiDaftar, "KartuEv_" + item.id, "#FFFFFF", 0.96f);
            RectTransform krt = kartu.GetComponent<RectTransform>();
            UiFactory.AturPosisi(krt, 10, atas, 1080, 200);

            Text tJudul = UiFactory.BuatTeks(krt, "JudulEv", item.judul, 30, "#2F3B2C", TextAnchor.MiddleLeft);
            UiFactory.AturPosisi(tJudul.GetComponent<RectTransform>(), 20, 10, 1040, 42);

            Text tDesk = UiFactory.BuatTeks(krt, "DeskEv", item.deskripsi, 24, "#4A5568", TextAnchor.UpperLeft);
            UiFactory.AturPosisi(tDesk.GetComponent<RectTransform>(), 20, 52, 1040, 66);

            Button bPolisi = UiFactory.BuatTombol(krt, "BtnPolisi",
                "Adukan ke Polisi", "#2B6CB0", "#FFFFFF",
                delegate
                {
                    if (ServerManager.Instance != null)
                        ServerManager.Instance.MintaTindakPeristiwa(item.id, "polisi");
                }, 24);
            UiFactory.AturPosisi(bPolisi.GetComponent<RectTransform>(), 20, 124, 500, 62);

            Button bWali = UiFactory.BuatTombol(krt, "BtnWaliKota",
                "Adukan ke Wali Kota", "#2E7D32", "#FFFFFF",
                delegate
                {
                    if (ServerManager.Instance != null)
                        ServerManager.Instance.MintaTindakPeristiwa(item.id, "walikota");
                }, 24);
            UiFactory.AturPosisi(bWali.GetComponent<RectTransform>(), 550, 124, 500, 62);

            atas += 220f;
        }
        isiDaftar.sizeDelta = new Vector2(1120f, atas + 20f);
    }
}
