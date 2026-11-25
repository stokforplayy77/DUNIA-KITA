// Dunia Kita — PanelDiagnosisKota
// Panel UI ringkas untuk menampilkan diagnosis & saran kota
// (dibangun dengan UiFactory, mengikuti gaya landscape 1920x1080).

using UnityEngine;
using UnityEngine.UI;

public class PanelDiagnosisKota
{
    private GameObject panel;
    private Text isiTeks;

    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelDiagnosisKota", "#FAF6EE", 0.97f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        UiFactory.AturPosisi(rt, 460, 170, 1000, 740);

        Text judul = UiFactory.BuatTeks(rt, "JudulDiagnosis",
            "Diagnosis & Saran Kota", 44, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 40, 24, 700, 60);

        Button tutup = UiFactory.BuatTombol(rt, "TombolTutupDiagnosis",
            "Tutup", "#C44D3D", "#FFFFFF", delegate { Sembunyikan(); }, 28);
        UiFactory.AturPosisi(tutup.GetComponent<RectTransform>(), 800, 24, 160, 60);

        isiTeks = UiFactory.BuatTeks(rt, "IsiDiagnosis",
            "", 30, "#2F3B2C", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(isiTeks.GetComponent<RectTransform>(), 40, 110, 920, 590);

        panel.SetActive(false);
    }

    public void Tampilkan(SistemDiagnosisKota.InputDiagnosis input)
    {
        if (panel == null) return;
        isiTeks.text = SistemDiagnosisKota.FormatLaporan(input);
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null) panel.SetActive(false);
    }
}
