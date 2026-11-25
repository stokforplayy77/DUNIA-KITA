// Dunia Kita — PanelKejadian
// Panel pilihan kejadian acak Fase 3 (GDD §9): Festival Desa, Hujan
// Deras, Pedagang Keliling. Dibuka GameManager sesudah roll kejadian
// di akhir hari; tiap kejadian punya 2 pilihan dan TIDAK ada pilihan
// yang selalu benar (lihat SistemKejadian).
//
// Pola sama seperti PanelMisi: panel dibuat sekali saat UIManager.Awake
// dan disembunyikan sampai dibutuhkan. Pilihan diteruskan ke
// GameManager.TerapkanPilihanKejadian, yang membayar biaya (bila ada),
// menerapkan efek, lalu melanjutkan ke laporan harian.

using UnityEngine;
using UnityEngine.UI;

public class PanelKejadian
{
    private GameObject panel;
    private Text judul;
    private Text isi;
    private Button tombolPilihan1;
    private Button tombolPilihan2;

    // Kejadian yang sedang ditampilkan ("festival" / "hujan" /
    // "pedagang" — lihat SistemKejadian).
    private string idKejadian;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // GameManager memanggil Tampilkan().
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelKejadian", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-640)/2=220.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 220, 760, 640);
        judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulKejadian", "Kejadian", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 70);
        isi = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "IsiKejadian", "", 36, "#2F3B2C", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(isi.GetComponent<RectTransform>(), 40, 110, 680, 220);
        tombolPilihan1 = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolKejadian1", "Pilihan 1", "#4E7A51", "#FFFFFF",
            delegate { Pilih(0); }, 32);
        UiFactory.AturPosisi(tombolPilihan1.GetComponent<RectTransform>(), 40, 360, 330, 110);
        tombolPilihan2 = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolKejadian2", "Pilihan 2", "#5A6B7A", "#FFFFFF",
            delegate { Pilih(1); }, 32);
        UiFactory.AturPosisi(tombolPilihan2.GetComponent<RectTransform>(), 390, 360, 330, 110);
        Text catatan = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "CatatanKejadian", "Pilih dengan bijak — tiap pilihan ada untung dan ruginya.",
            28, "#5A5A5A", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(catatan.GetComponent<RectTransform>(), 40, 500, 680, 60);
        panel.SetActive(false);
    }

    public void Tampilkan(string id)
    {
        idKejadian = id;
        judul.text = SistemKejadian.Judul(id);
        isi.text = SistemKejadian.Deskripsi(id);
        AturLabelTombol(tombolPilihan1, SistemKejadian.LabelPilihan1(id));
        AturLabelTombol(tombolPilihan2, SistemKejadian.LabelPilihan2(id));
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Pilihan 0 = pilihan berbayar (festival diadakan / payung
    // disiapkan / dagangan dibeli), pilihan 1 = tolak/biarkan.
    private void Pilih(int pilihan)
    {
        string id = idKejadian;
        Sembunyikan();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TerapkanPilihanKejadian(id, pilihan);
        }
    }

    private static void AturLabelTombol(Button tombol, string label)
    {
        if (tombol == null)
        {
            return;
        }
        Text teks = tombol.GetComponentInChildren<Text>();
        if (teks != null)
        {
            teks.text = label;
        }
    }
}
