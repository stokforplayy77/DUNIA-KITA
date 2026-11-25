// Dunia Kita — PanelPerayaan
// Panel perayaan naik level (Fase 3). Dipisah dari UIManager di
// Fase 5 supaya UIManager tetap di bawah 600 baris — isinya sama
// persis seperti sebelumnya (judul "Selamat!" + isi + tombol
// "Lanjutkan" yang meneruskan alur pergantian hari GameManager).
//
// Dipanggil GameManager saat dunia naik level di akhir hari;
// bangunan yang terbuka di level baru dihitung dari daftarBangunan
// (bukan hardcode).

using UnityEngine;
using UnityEngine.UI;

public class PanelPerayaan
{
    private GameObject panel;
    private Text isiPerayaan;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // ada level baru yang dirayakan.
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelPerayaan", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-520)/2=280.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 280, 760, 520);
        Text judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulPerayaan", "Selamat!", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 70);
        isiPerayaan = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "IsiPerayaan", "", 38, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(isiPerayaan.GetComponent<RectTransform>(), 40, 110, 680, 230);
        Button tombolLanjut = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolLanjutPerayaan", "Lanjutkan", "#4E7A51", "#FFFFFF",
            delegate
            {
                panel.SetActive(false);
                GameManager.Instance.LanjutkanSesudahPerayaan();
            });
        UiFactory.AturPosisi(tombolLanjut.GetComponent<RectTransform>(), 180, 380, 400, 110);
        panel.SetActive(false);
    }

    // Tampilkan perayaan naik level (dipanggil GameManager).
    public void Tampilkan(int levelBaru, long hadiah, BuildingData[] daftar)
    {
        string terbuka = "";
        if (daftar != null)
        {
            foreach (BuildingData d in daftar)
            {
                if (d != null && d.levelBuka == levelBaru)
                {
                    terbuka += (terbuka.Length > 0 ? ", " : "") + d.nama;
                }
            }
        }
        isiPerayaan.text = "Duniamu naik ke Level " + levelBaru + " - "
            + ProgresiDunia.NamaLevel(levelBaru) + "!\n\n"
            + "Hadiah: " + FormatUtil.Rupiah(hadiah) + "\n"
            + (terbuka.Length > 0
                ? "Bangunan baru terbuka: " + terbuka + "!"
                : "Tidak ada bangunan baru di level ini.");
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}
