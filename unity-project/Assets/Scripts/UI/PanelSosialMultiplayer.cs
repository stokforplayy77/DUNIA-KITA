// Dunia Kita — UI/PanelSosialMultiplayer
// Panel Sosial Dunia Bersama: Chat Distrik + Papan Peringkat (Leaderboard)
// pemain paling makmur.

using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PanelSosialMultiplayer
{
    public static PanelSosialMultiplayer Instance { get; private set; }

    private GameObject panel;
    private Text teksChat;
    private Text teksLeaderboard;
    private InputField inputPesan;

    public static void Tampilkan()
    {
        PastikanAda();
        if (Instance != null)
        {
            Instance.Buka();
        }
    }

    public static void PerbaruiChat(PesanChatDto[] pesan)
    {
        if (Instance != null)
        {
            Instance.RenderChat(pesan);
        }
    }

    public static void PerbaruiLeaderboard(PeringkatDto[] daftar)
    {
        if (Instance != null)
        {
            Instance.RenderLeaderboard(daftar);
        }
    }

    public static void PastikanAda()
    {
        if (Instance != null) return;
        Canvas canvas = UiFactory.CariCanvasUI();
        if (canvas == null) return;
        Instance = new PanelSosialMultiplayer();
        Instance.Bangun(canvas.GetComponent<RectTransform>());
    }

    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelSosialMP", "#FAF6EE", 0.98f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        UiFactory.AturPosisi(rt, 260, 110, 1400, 840);

        Text judul = UiFactory.BuatTeks(rt, "JudulSosialMP",
            "Sosial • Chat Distrik & Peringkat", 40, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 40, 20, 960, 58);

        Button tutup = UiFactory.BuatTombol(rt, "TombolTutupSosial",
            "Tutup", "#C0392B", "#FFFFFF", delegate { Sembunyikan(); }, 28);
        UiFactory.AturPosisi(tutup.GetComponent<RectTransform>(), 1200, 20, 160, 58);

        // Kolom kiri: Chat Distrik
        Text lblChat = UiFactory.BuatTeks(rt, "LblChat", "Chat Distrik", 30, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(lblChat.GetComponent<RectTransform>(), 40, 95, 640, 42);

        GameObject boxChat = UiFactory.BuatPanel(rt, "BoxChat", "#FFFFFF", 0.95f);
        UiFactory.AturPosisi(boxChat.GetComponent<RectTransform>(), 40, 145, 640, 520);
        teksChat = UiFactory.BuatTeks(boxChat.GetComponent<RectTransform>(),
            "IsiChat", "Memuat chat...", 24, "#2D3748", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(teksChat.GetComponent<RectTransform>(), 16, 16, 608, 488);

        inputPesan = UiFactory.BuatInput(rt, "InputChat", "Tulis pesan ke distrik...",
            InputField.ContentType.Standard, 26);
        UiFactory.AturPosisi(inputPesan.GetComponent<RectTransform>(), 40, 685, 460, 90);

        Button btnKirim = UiFactory.BuatTombol(rt, "TombolKirimChat",
            "Kirim", "#2E7D32", "#FFFFFF", delegate { Kirim(); }, 26);
        UiFactory.AturPosisi(btnKirim.GetComponent<RectTransform>(), 520, 685, 160, 90);

        // Kolom kanan: Leaderboard
        Text lblRank = UiFactory.BuatTeks(rt, "LblRank", "Papan Peringkat", 30, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(lblRank.GetComponent<RectTransform>(), 720, 95, 640, 42);

        GameObject boxRank = UiFactory.BuatPanel(rt, "BoxRank", "#FFFFFF", 0.95f);
        UiFactory.AturPosisi(boxRank.GetComponent<RectTransform>(), 720, 145, 640, 630);
        teksLeaderboard = UiFactory.BuatTeks(boxRank.GetComponent<RectTransform>(),
            "IsiRank", "Memuat peringkat...", 24, "#2D3748", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(teksLeaderboard.GetComponent<RectTransform>(), 16, 16, 608, 598);

        panel.SetActive(false);
    }

    private void Buka()
    {
        panel.SetActive(true);
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            ServerManager.Instance.MintaDaftarChat();
            ServerManager.Instance.MintaLeaderboard();
        }
        else
        {
            teksChat.text = "Masuk ke Dunia Bersama dulu untuk membuka chat distrik.";
            teksLeaderboard.text = "Masuk ke Dunia Bersama dulu untuk melihat peringkat pemain.";
        }
    }

    public void Sembunyikan()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void Kirim()
    {
        if (inputPesan == null || ServerManager.Instance == null || !ServerManager.Instance.ModeOnline)
            return;
        string teks = (inputPesan.text ?? "").Trim();
        if (teks.Length == 0) return;
        inputPesan.text = "";
        ServerManager.Instance.MintaKirimChat(teks);
    }

    private void RenderChat(PesanChatDto[] pesan)
    {
        if (teksChat == null) return;
        if (pesan == null || pesan.Length == 0)
        {
            teksChat.text = "Belum ada pesan di distrik ini. Mulai obrolan!";
            return;
        }
        StringBuilder sb = new StringBuilder();
        int mulai = pesan.Length > 14 ? pesan.Length - 14 : 0;
        for (int i = mulai; i < pesan.Length; i++)
        {
            sb.Append("[").Append(pesan[i].nama).Append("] ").AppendLine(pesan[i].isi);
        }
        teksChat.text = sb.ToString();
    }

    private void RenderLeaderboard(PeringkatDto[] daftar)
    {
        if (teksLeaderboard == null) return;
        if (daftar == null || daftar.Length == 0)
        {
            teksLeaderboard.text = "Belum ada pemain terdaftar.";
            return;
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < daftar.Length; i++)
        {
            PeringkatDto p = daftar[i];
            sb.Append(i + 1).Append(". ").Append(p.nama)
                .Append(" — ").Append(FormatUtil.Rupiah(p.koin))
                .Append(" • Rep ").Append(p.reputasi)
                .Append(" • ").Append(p.bangunan).AppendLine(" bangunan");
        }
        teksLeaderboard.text = sb.ToString();
    }
}
