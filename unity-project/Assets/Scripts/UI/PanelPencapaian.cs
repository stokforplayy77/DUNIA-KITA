// Dunia Kita — PanelPencapaian
// Panel daftar pencapaian / achievement Fase 4 (GDD §8).
// Pencapaian dinilai LANGSUNG dari keadaan dunia nyata
// (SistemPencapaian + GameManager.KeadaanPencapaianSaatIni):
//
//   1. Juragan Kost    — punya 5 Kost          -> 3 Permata
//   2. Kota Pelajar    — Sekolah + Kampus      -> 3 Permata
//   3. Kota Hijau      — punya 20 Taman        -> 2 Permata
//   4. Metropolis      — capai Level 6         -> 10 Permata
//   5. Juragan Pasar   — 3 bangunan komersial  -> 3 Permata
//   6. Kota Sejahtera  — kebahagiaan 80%       -> 5 Permata
//
// Hadiah diklaim manual lewat tombol "Klaim" pada pencapaian yang
// sudah tercapai; tiap pencapaian hanya bisa diklaim SEKALI
// (dicatat di SaveData lewat GameManager.PencapaianDiklaim —
// BUKAN PlayerPrefs, supaya klaim ikut tersimpan bersama dunia).
// 6 baris muat tanpa gulir (panel 760x900).

using UnityEngine;
using UnityEngine.UI;

public class PanelPencapaian
{
    private GameObject panel;
    private RectTransform isi;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // tombol Pencapaian ditekan.
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelPencapaian", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-900)/2=90.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 90, 760, 900);
        Text judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulPencapaian", "Pencapaian", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 70);
        GameObject wadah = new GameObject("IsiPencapaian");
        isi = UiFactory.BuatRect(wadah, panel.GetComponent<RectTransform>(),
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
        Button tombolTutup = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolTutupPencapaian", "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { Sembunyikan(); });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 180, 780, 400, 90);

        panel.SetActive(false);
    }

    public void Tampilkan()
    {
        PerbaruiIsi();
        panel.SetActive(true);
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // ---- Gambar ulang daftar (dipanggil tiap kali panel dibuka) ----
    private void PerbaruiIsi()
    {
        foreach (Transform anak in isi)
        {
            Object.Destroy(anak.gameObject);
        }
        GameManager gm = GameManager.Instance;
        SistemPencapaian.KeadaanPencapaian keadaan =
            gm != null
                ? gm.KeadaanPencapaianSaatIni()
                : new SistemPencapaian.KeadaanPencapaian();
        float atas = 100f;
        foreach (string id in SistemPencapaian.SemuaId())
        {
            string salinanId = id; // Untuk closure tombol klaim.
            bool tercapai = SistemPencapaian.ApakahTercapai(salinanId, keadaan);
            bool diklaim = gm != null && gm.PencapaianDiklaim != null
                && gm.PencapaianDiklaim.Contains(salinanId);
            int hadiah = SistemPencapaian.HadiahPermata(salinanId);

            Text nama = UiFactory.BuatTeks(isi,
                "Nama_" + salinanId, SistemPencapaian.Judul(salinanId),
                36, "#2F3B2C", TextAnchor.MiddleLeft);
            UiFactory.AturPosisi(nama.GetComponent<RectTransform>(), 40, atas, 440, 46);
            Text deskripsi = UiFactory.BuatTeks(isi,
                "Deskripsi_" + salinanId, SistemPencapaian.Deskripsi(salinanId),
                28, "#5A5A5A", TextAnchor.UpperLeft);
            UiFactory.AturPosisi(deskripsi.GetComponent<RectTransform>(), 40, atas + 50, 440, 56);

            if (tercapai && !diklaim)
            {
                Button tombolKlaim = UiFactory.BuatTombol(isi,
                    "Klaim_" + salinanId, "Klaim " + hadiah + " Permata",
                    "#4E7A51", "#FFFFFF",
                    delegate
                    {
                        if (GameManager.Instance != null
                            && GameManager.Instance.KlaimPencapaian(salinanId))
                        {
                            UIManager ui = Object.FindObjectOfType<UIManager>();
                            if (ui != null)
                            {
                                ui.TampilkanPesan("Pencapaian diraih: "
                                    + SistemPencapaian.Judul(salinanId)
                                    + " (+" + hadiah + " Permata).");
                            }
                        }
                        PerbaruiIsi();
                    }, 30);
                UiFactory.AturPosisi(tombolKlaim.GetComponent<RectTransform>(),
                    500, atas + 10, 220, 90);
            }
            else
            {
                string status = !tercapai
                    ? "Belum tercapai"
                    : "Selesai — hadiah diterima";
                string warna = tercapai ? "#2E7D32" : "#8A8A8A";
                Text teksStatus = UiFactory.BuatTeks(isi,
                    "Status_" + salinanId, status, 30, warna, TextAnchor.MiddleCenter);
                UiFactory.AturPosisi(teksStatus.GetComponent<RectTransform>(),
                    500, atas + 10, 220, 90);
            }
            atas += 110f;
        }
    }
}
