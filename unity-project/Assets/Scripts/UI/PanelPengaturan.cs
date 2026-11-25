// Dunia Kita — PanelPengaturan
// Panel pengaturan (GDD §11):
//   - Suara Nyala/Mati (preferensi disimpan di PlayerPrefs dan langsung
//     diterapkan ke AudioListener.volume; FASE 5: saklar yang sama
//     dipakai AudioManager untuk SFX + musik latar).
//   - Ulangi Tutorial (FASE 5) — tampilkan lagi tutorial pemain baru.
//   - Teks versi game.
//   - Tombol Tutup.
// Tidak ada slider volume dulu (sesuai keputusan Fase 1).

using UnityEngine;
using UnityEngine.UI;

public class PanelPengaturan
{
    // Kunci suara hidup di AudioManager (dipakai PanelPengaturan +
    // AudioManager — satu sumber kebenaran, FASE 5).
    private GameObject panel;
    private Button tombolSuara;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // tombol Pengaturan ditekan.
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelPengaturan", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-700)/2=610, (1080-620)/2=230.
        // FASE 7: panel diperbesar 520 -> 620 (tombol Akun baru).
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 610, 230, 700, 620);
        Text judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulPengaturan", "Pengaturan", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 700, 70);

        Text labelSuara = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "LabelSuara", "Suara", 40, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(labelSuara.GetComponent<RectTransform>(), 40, 120, 300, 80);
        tombolSuara = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolSuara", "Nyala", "#4E7A51", "#FFFFFF",
            delegate { ToggleSuara(); }, 36);
        UiFactory.AturPosisi(tombolSuara.GetComponent<RectTransform>(), 380, 120, 280, 90);

        Text teksVersi = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "TeksVersi", "Versi game: " + GameBalance.VersiGame + " (Fase 5)",
            32, "#5A5A5A", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksVersi.GetComponent<RectTransform>(), 40, 240, 620, 60);

        // FASE 5: ulangi tutorial pemain baru dari awal.
        Button tombolTutorial = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolUlangiTutorial", "Ulangi Tutorial", "#4E7A51", "#FFFFFF",
            delegate
            {
                Sembunyikan();
                UIManager ui = Object.FindObjectOfType<UIManager>();
                if (ui != null)
                {
                    ui.TampilkanTutorialUlang();
                }
            }, 36);
        UiFactory.AturPosisi(tombolTutorial.GetComponent<RectTransform>(), 150, 310, 400, 90);

        // FASE 7: akun Dunia Bersama (daftar/masuk/keluar).
        Button tombolAkun = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolAkunOnline", "Akun / Dunia Bersama", "#4E7A51", "#FFFFFF",
            delegate
            {
                Sembunyikan();
                PanelAkun.Tampilkan();
            }, 36);
        UiFactory.AturPosisi(tombolAkun.GetComponent<RectTransform>(), 150, 420, 400, 90);

        Button tombolTutup = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolTutupPengaturan", "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { Sembunyikan(); });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 150, 530, 400, 90);

        TerapkanSuara();
        panel.SetActive(false);
    }

    public void Tampilkan()
    {
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private static bool SuaraNyala()
    {
        return AudioManager.SuaraNyala();
    }

    private void ToggleSuara()
    {
        bool nyala = !SuaraNyala();
        PlayerPrefs.SetInt(AudioManager.KunciSuara, nyala ? 1 : 0);
        PlayerPrefs.Save();
        TerapkanSuara();
    }

    // Terapkan preferensi ke audio game + tulisan tombol.
    // FASE 5: musik latar ikut nyala/mati bersama saklar ini.
    private void TerapkanSuara()
    {
        bool nyala = SuaraNyala();
        AudioManager.TerapkanSuara();
        if (AudioManager.Instance != null)
        {
            if (nyala)
            {
                AudioManager.Instance.MulaiMusik();
            }
            else
            {
                AudioManager.Instance.HentikanMusik();
            }
        }
        if (tombolSuara != null)
        {
            Text label = UiFactory.LabelTombol(tombolSuara);
            if (label != null)
            {
                label.text = nyala ? "Nyala" : "Mati";
            }
        }
    }
}
