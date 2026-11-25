// Dunia Kita — UI/PanelAkun
// Panel akun Dunia Bersama (Fase 7, GDD §17): daftar / masuk ke
// server Dunia Bersama dengan nama pemain + kata sandi (tanpa email).
// Ini akun game lokal milik server sendiri — BUKAN akun Google/Play.
// Kata sandi dikirim sekali saat masuk/daftar, TIDAK PERNAH disimpan
// di HP (hanya token acak di PlayerPrefs "DuniaKita_Token").
//
// Dibuat LAZY dari PanelPengaturan (tombol "Akun / Dunia Bersama"),
// bukan oleh UIManager — supaya UIManager tetap < 600 baris.

using UnityEngine;
using UnityEngine.UI;

public class PanelAkun
{
    public static PanelAkun Instance { get; private set; }

    private GameObject panel;
    private InputField kolomNama;
    private InputField kolomSandi;
    private InputField kolomServer;
    private Text teksStatus;
    private Button tombolKeluar;

    public static void Tampilkan()
    {
        PastikanAda();
        if (Instance != null)
        {
            Instance.TampilkanInternal();
        }
    }

    public static void PastikanAda()
    {
        if (Instance != null)
        {
            return;
        }
        Canvas canvas = UiFactory.CariCanvasUI();
        if (canvas == null)
        {
            return;
        }
        Instance = new PanelAkun();
        Instance.Bangun(canvas.GetComponent<RectTransform>());
    }

    // Panel dibuat sekali; disembunyikan sampai dibutuhkan.
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelAkun", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, y=60, ukuran 760x980
        // (konten sampai y=950: 3 kolom + 3 baris tombol).
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 60, 760, 980);
        RectTransform isi = panel.GetComponent<RectTransform>();
        Text judul = UiFactory.BuatTeks(isi, "JudulAkun",
            "Akun Dunia Bersama", 44, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 60);
        Text info = UiFactory.BuatTeks(isi, "InfoAkun",
            "Main bersama di satu peta besar (24x24). Akun game lokal "
            + "server sendiri, bukan akun Google/Play. Kata sandi tidak "
            + "disimpan di HP ini.", 30, "#5A5A5A", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(info.GetComponent<RectTransform>(), 40, 100, 680, 110);

        Text labelNama = UiFactory.BuatTeks(isi, "LabelNamaAkun",
            "Nama pemain", 34, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(labelNama.GetComponent<RectTransform>(), 40, 220, 300, 50);
        kolomNama = UiFactory.BuatInput(isi, "KolomNamaAkun",
            "Nama pemain (3-20 huruf)", InputField.ContentType.Standard);
        UiFactory.AturPosisi(kolomNama.GetComponent<RectTransform>(), 40, 270, 680, 80);

        Text labelSandi = UiFactory.BuatTeks(isi, "LabelSandiAkun",
            "Kata sandi", 34, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(labelSandi.GetComponent<RectTransform>(), 40, 360, 300, 50);
        kolomSandi = UiFactory.BuatInput(isi, "KolomSandiAkun",
            "Kata sandi (min. 4 karakter)", InputField.ContentType.Password);
        UiFactory.AturPosisi(kolomSandi.GetComponent<RectTransform>(), 40, 410, 680, 80);

        Text labelServer = UiFactory.BuatTeks(isi, "LabelServerAkun",
            "Alamat server", 34, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(labelServer.GetComponent<RectTransform>(), 40, 500, 300, 50);
        kolomServer = UiFactory.BuatInput(isi, "KolomServerAkun",
            "http://127.0.0.1:8471", InputField.ContentType.Standard, 30);
        UiFactory.AturPosisi(kolomServer.GetComponent<RectTransform>(), 40, 550, 680, 80);
        kolomServer.text = PlayerPrefs.GetString(ServerManager.KunciAlamatServer,
            GameBalance.AlamatServerDefault);

        teksStatus = UiFactory.BuatTeks(isi, "StatusAkun", "", 30, "#B00020",
            TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(teksStatus.GetComponent<RectTransform>(), 40, 640, 680, 40);

        Button tombolMasuk = UiFactory.BuatTombol(isi, "TombolMasukAkun",
            "Masuk", "#4E7A51", "#FFFFFF",
            delegate { Kirim(true); }, 36);
        UiFactory.AturPosisi(tombolMasuk.GetComponent<RectTransform>(), 40, 690, 320, 90);
        Button tombolDaftar = UiFactory.BuatTombol(isi, "TombolDaftarAkun",
            "Daftar", "#4E7A51", "#FFFFFF",
            delegate { Kirim(false); }, 36);
        UiFactory.AturPosisi(tombolDaftar.GetComponent<RectTransform>(), 400, 690, 320, 90);

        tombolKeluar = UiFactory.BuatTombol(isi, "TombolKeluarAkun",
            "Keluar", "#8A8A8A", "#FFFFFF",
            delegate
            {
                if (ServerManager.Instance != null)
                {
                    ServerManager.Instance.Keluar();
                }
                PerbaruiTombolKeluar();
                teksStatus.text = "Keluar. Muat ulang game untuk kembali "
                    + "ke dunia sendiri.";
            }, 36);
        UiFactory.AturPosisi(tombolKeluar.GetComponent<RectTransform>(), 220, 790, 320, 90);

        Button tombolTutup = UiFactory.BuatTombol(isi, "TombolTutupAkun",
            "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { Sembunyikan(); }, 36);
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 220, 890, 320, 90);
        panel.SetActive(false);
    }

    private void TampilkanInternal()
    {
        ServerManager sm = ServerManager.Instance;
        if (sm != null && !string.IsNullOrEmpty(sm.NamaPemain))
        {
            kolomNama.text = sm.NamaPemain;
        }
        if (sm != null)
        {
            kolomServer.text = sm.AlamatServer;
        }
        teksStatus.text = "";
        PerbaruiTombolKeluar();
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void PerbaruiTombolKeluar()
    {
        if (tombolKeluar == null)
        {
            return;
        }
        ServerManager sm = ServerManager.Instance;
        bool masuk = sm != null && sm.ModeOnline;
        tombolKeluar.gameObject.SetActive(masuk);
    }

    private void Kirim(bool masuk)
    {
        ServerManager sm = ServerManager.Instance;
        if (sm == null)
        {
            teksStatus.text = "ServerManager belum siap — coba lagi.";
            return;
        }
        string nama = kolomNama != null ? kolomNama.text : "";
        string sandi = kolomSandi != null ? kolomSandi.text : "";
        string alamat = kolomServer != null ? kolomServer.text : "";
        string alasan;
        if (!ServerManager.NamaValid(nama, out alasan))
        {
            teksStatus.text = alasan;
            return;
        }
        if (!ServerManager.SandiValid(sandi, out alasan))
        {
            teksStatus.text = alasan;
            return;
        }
        teksStatus.text = masuk ? "Masuk…" : "Mendaftar…";
        if (masuk)
        {
            sm.Masuk(nama, sandi, alamat, delegate (bool ok, string pesan)
            {
                Selesai(ok, pesan);
            });
        }
        else
        {
            sm.Daftar(nama, sandi, alamat, delegate (bool ok, string pesan)
            {
                Selesai(ok, pesan);
            });
        }
    }

    private void Selesai(bool ok, string pesan)
    {
        if (ok)
        {
            Sembunyikan();
            PerbaruiTombolKeluar();
        }
        else
        {
            teksStatus.text = pesan;
        }
    }
}
