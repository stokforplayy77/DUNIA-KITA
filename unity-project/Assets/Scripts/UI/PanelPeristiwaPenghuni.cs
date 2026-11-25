// Dunia Kita — PanelPeristiwaPenghuni
// Panel pilihan peristiwa penghuni Fase 4B (GDD §9.1): anak kost
// tidak bisa bayar, anak kost melarikan diri, penghuni mengeluh
// tetangga berisik, penghuni merusak perabot, penghuni
// merekomendasikan teman, penghuni minta pindah kamar.
//
// Pola sama seperti PanelKejadian: panel dibuat sekali saat
// UIManager.Awake dan disembunyikan sampai dibutuhkan. Bedanya:
// ada peristiwa dengan 3 pilihan (tidak bisa bayar) — tombol
// ketiga disembunyikan bila peristiwa hanya punya 2 pilihan.
// Tiap pilihan punya konsekuensi berbeda (uang vs reputasi vs
// kebahagiaan) dan TIDAK ada pilihan yang selalu benar (lihat
// SistemPeristiwaPenghuni). Pilihan diteruskan ke
// GameManager.TerapkanPilihanPeristiwa, yang menerapkan dampak
// lalu melanjutkan ke laporan harian.

using UnityEngine;
using UnityEngine.UI;

public class PanelPeristiwaPenghuni
{
    private GameObject panel;
    private Text judul;
    private Text isi;
    private Button tombolPilihan1;
    private Button tombolPilihan2;
    private Button tombolPilihan3;

    // Peristiwa yang sedang ditampilkan ("tidak_bayar" / "lari" /
    // "berisik" / "rusak" / "rekomendasi" / "pindah" — lihat
    // SistemPeristiwaPenghuni).
    private string idPeristiwa;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // GameManager memanggil Tampilkan().
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelPeristiwaPenghuni", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-700)/2=190.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 190, 760, 700);
        judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulPeristiwa", "Peristiwa Penghuni", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 70);
        isi = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "IsiPeristiwa", "", 36, "#2F3B2C", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(isi.GetComponent<RectTransform>(), 40, 110, 680, 220);
        tombolPilihan1 = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolPeristiwa1", "Pilihan 1", "#4E7A51", "#FFFFFF",
            delegate { Pilih(0); }, 32);
        UiFactory.AturPosisi(tombolPilihan1.GetComponent<RectTransform>(), 40, 360, 330, 110);
        tombolPilihan2 = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolPeristiwa2", "Pilihan 2", "#5A6B7A", "#FFFFFF",
            delegate { Pilih(1); }, 32);
        UiFactory.AturPosisi(tombolPilihan2.GetComponent<RectTransform>(), 390, 360, 330, 110);
        tombolPilihan3 = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolPeristiwa3", "Pilihan 3", "#7A5A3A", "#FFFFFF",
            delegate { Pilih(2); }, 32);
        UiFactory.AturPosisi(tombolPilihan3.GetComponent<RectTransform>(), 215, 490, 330, 110);
        panel.SetActive(false);
    }

    public void Tampilkan(string id)
    {
        idPeristiwa = id;
        judul.text = SistemPeristiwaPenghuni.Judul(id);
        isi.text = SistemPeristiwaPenghuni.Deskripsi(id);
        AturLabelTombol(tombolPilihan1,
            SistemPeristiwaPenghuni.LabelPilihan(id, 0));
        AturLabelTombol(tombolPilihan2,
            SistemPeristiwaPenghuni.LabelPilihan(id, 1));
        if (SistemPeristiwaPenghuni.JumlahPilihan(id) >= 3)
        {
            tombolPilihan3.gameObject.SetActive(true);
            AturLabelTombol(tombolPilihan3,
                SistemPeristiwaPenghuni.LabelPilihan(id, 2));
        }
        else
        {
            tombolPilihan3.gameObject.SetActive(false);
        }
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Pilih(int pilihan)
    {
        string id = idPeristiwa;
        Sembunyikan();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TerapkanPilihanPeristiwa(id, pilihan);
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
