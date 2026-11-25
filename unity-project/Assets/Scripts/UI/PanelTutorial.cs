// Dunia Kita — PanelTutorial
// Panel tutorial pemain baru (Fase 5, GDD §9: misi tutorial memandu
// pemain, bukan memaksa). Panel kecil di bawah layar (di atas bar
// bawah) dengan judul + satu kalimat + tombol "Lanjut" / "Lewati".
//
// Pola sama seperti panel lain: dibuat sekali saat UIManager.Awake
// dan disembunyikan sampai dibutuhkan. Logika langkah (urutan,
// lewati otomatis, selesai sekali) hidup di Utils/SistemTutorial.cs
// (logika murni, bisa dites tanpa Editor); kelas ini hanya menampilkan.
//
// Aturan tampil:
//   - Otomatis sekali saat game pertama kali dibuka (GameManager.
//     Start memanggil CobaTampilkanAwal) — hanya bila tutorial
//     belum pernah selesai (PlayerPrefs).
//   - Bisa diulang dari Pengaturan ("Ulangi Tutorial") lewat
//     TampilkanUlang() — flag selesai dihapus dulu.
//   - Langkah "bangun-rumah" dilewati otomatis bila pemain sudah
//     membangun Rumah Sederhana (cek BuildManager nyata).

using UnityEngine;
using UnityEngine.UI;

public class PanelTutorial
{
    private GameObject panel;
    private Text judul;
    private Text isi;
    private int indeksSaatIni;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // tutorial perlu ditampilkan.
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelTutorial", "#FFF8E7", 0.98f);
        // Bawah layar, di atas bar bawah (bar bawah y=940):
        // (1920-1200)/2=360, y=600, ukuran 1200x300.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 360, 600, 1200, 300);
        judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulTutorial", "Panduan", 40, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 10, 1200, 60);
        isi = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "IsiTutorial", "", 34, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(isi.GetComponent<RectTransform>(), 40, 80, 1120, 110);
        Button tombolLanjut = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolLanjutTutorial", "Lanjut", "#4E7A51", "#FFFFFF",
            delegate { Lanjut(); });
        UiFactory.AturPosisi(tombolLanjut.GetComponent<RectTransform>(), 380, 200, 400, 90);
        Button tombolLewati = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolLewatiTutorial", "Lewati", "#8A8A8A", "#FFFFFF",
            delegate { Lewati(); });
        UiFactory.AturPosisi(tombolLewati.GetComponent<RectTransform>(), 800, 200, 300, 90);

        panel.SetActive(false);
    }

    // Tampilkan tutorial dari langkah pertama yang belum terpenuhi
    // (dipanggil UIManager saat game baru dibuka / diulang).
    public void Tampilkan()
    {
        TampilkanLangkah(SistemTutorial.LangkahPertama(SudahBangunRumah()));
    }

    // Ulangi dari Pengaturan: hapus flag selesai, tampil dari awal.
    public void TampilkanUlang()
    {
        SistemTutorial.ResetUntukUlang();
        Tampilkan();
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Dipanggil GameManager.Start: tampilkan hanya bila tutorial
    // belum pernah selesai (pemain baru).
    public static void CobaTampilkanAwal()
    {
        if (!SistemTutorial.PerluTampilkan())
        {
            return;
        }
        UIManager ui = Object.FindObjectOfType<UIManager>();
        if (ui != null)
        {
            ui.TampilkanTutorial();
        }
    }

    private void TampilkanLangkah(int indeks)
    {
        if (indeks >= SistemTutorial.JumlahLangkah)
        {
            SistemTutorial.TandaiSelesai();
            Sembunyikan();
            return;
        }
        indeksSaatIni = indeks;
        SistemTutorial.LangkahTutorial langkah = SistemTutorial.LangkahSemua[indeks];
        judul.text = langkah.Judul + " (" + (indeks + 1) + "/"
            + SistemTutorial.JumlahLangkah + ")";
        isi.text = langkah.Teks;
        panel.SetActive(true);
    }

    private void Lanjut()
    {
        TampilkanLangkah(
            SistemTutorial.LangkahBerikut(indeksSaatIni, SudahBangunRumah()));
    }

    private void Lewati()
    {
        SistemTutorial.TandaiSelesai();
        Sembunyikan();
    }

    private bool SudahBangunRumah()
    {
        BuildManager build = Object.FindObjectOfType<BuildManager>();
        return build != null && build.HitungBangunan("rumah") >= 1;
    }
}
