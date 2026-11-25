// Dunia Kita — UI/PanelDistrik
// Panel Multi-Distrik untuk Dunia Bersama: menampilkan daftar distrik
// (tiap distrik punya peta 40x40 & fasilitas awal kota sendiri) agar
// ratusan-ribuan pemain dapat tersebar rapi dan berpindah distrik.

using UnityEngine;
using UnityEngine.UI;

public class PanelDistrik
{
    public static PanelDistrik Instance { get; private set; }

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

    public static void SembunyikanJikaAda()
    {
        if (Instance != null)
        {
            Instance.Sembunyikan();
        }
    }

    public static void PerbaruiDaftar(DistrikDto[] daftar, int distrikAktif)
    {
        if (Instance != null)
        {
            Instance.RenderDaftar(daftar, distrikAktif);
        }
    }

    public static void PastikanAda()
    {
        if (Instance != null) return;
        Canvas canvas = UiFactory.CariCanvasUI();
        if (canvas == null) return;
        Instance = new PanelDistrik();
        Instance.Bangun(canvas.GetComponent<RectTransform>());
    }

    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelDistrik", "#FAF6EE", 0.98f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        UiFactory.AturPosisi(rt, 360, 120, 1200, 820);

        Text judul = UiFactory.BuatTeks(rt, "JudulDistrik",
            "Pilih Distrik Dunia Bersama", 42, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 40, 20, 900, 60);

        Button tutup = UiFactory.BuatTombol(rt, "TombolTutupDistrik",
            "Tutup", "#C0392B", "#FFFFFF", delegate { Sembunyikan(); }, 28);
        UiFactory.AturPosisi(tutup.GetComponent<RectTransform>(), 1000, 20, 160, 60);

        statusTeks = UiFactory.BuatTeks(rt, "StatusDistrik",
            "Memuat daftar distrik...", 28, "#5A6B7A", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(statusTeks.GetComponent<RectTransform>(), 40, 88, 1120, 44);

        isiDaftar = UiFactory.BuatScroll(rt, "ScrollDistrik", 40, 140, 1120, 640);
        panel.SetActive(false);
    }

    private void Buka()
    {
        panel.SetActive(true);
        statusTeks.text = "Mengambil daftar distrik dari server...";
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            ServerManager.Instance.MintaDaftarDistrik();
        }
        else
        {
            statusTeks.text = "Masuk ke Dunia Bersama dulu untuk memilih distrik.";
        }
    }

    public void Sembunyikan()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void RenderDaftar(DistrikDto[] daftar, int distrikAktif)
    {
        if (isiDaftar == null) return;
        foreach (Transform anak in isiDaftar)
        {
            Object.Destroy(anak.gameObject);
        }
        if (daftar == null || daftar.Length == 0)
        {
            statusTeks.text = "Tidak ada distrik tersedia.";
            isiDaftar.sizeDelta = new Vector2(1120f, 100f);
            return;
        }
        statusTeks.text = "Pilih distrik untuk dikunjungi atau tempat membangun:";
        float atas = 10f;
        foreach (DistrikDto d in daftar)
        {
            DistrikDto item = d;
            bool aktif = item.id == distrikAktif;
            GameObject kartu = UiFactory.BuatPanel(isiDaftar, "KartuDistrik_" + item.id,
                aktif ? "#E8F5E9" : "#FFFFFF", 0.96f);
            RectTransform krt = kartu.GetComponent<RectTransform>();
            UiFactory.AturPosisi(krt, 10, atas, 1080, 140);

            Text tNama = UiFactory.BuatTeks(krt, "NamaDistrik",
                item.nama + (aktif ? " [Aktif]" : ""), 30, "#2F3B2C", TextAnchor.MiddleLeft);
            UiFactory.AturPosisi(tNama.GetComponent<RectTransform>(), 20, 12, 740, 42);

            Text tInfo = UiFactory.BuatTeks(krt, "InfoDistrik",
                item.deskripsi + " • " + item.jumlah_pemain + " pemilik, "
                + item.jumlah_bangunan + " bangunan", 24, "#4A5568", TextAnchor.MiddleLeft);
            UiFactory.AturPosisi(tInfo.GetComponent<RectTransform>(), 20, 62, 740, 52);

            Button bMasuk = UiFactory.BuatTombol(krt, "BtnMasukDistrik",
                aktif ? "Sedang di sini" : "Masuk Distrik",
                aktif ? "#8A8A8A" : "#2E7D32", "#FFFFFF",
                delegate
                {
                    if (!aktif && ServerManager.Instance != null)
                        ServerManager.Instance.MintaPindahDistrik(item.id);
                }, 24);
            UiFactory.AturPosisi(bMasuk.GetComponent<RectTransform>(), 780, 25, 270, 90);

            atas += 160f;
        }
        isiDaftar.sizeDelta = new Vector2(1120f, atas + 20f);
    }
}
