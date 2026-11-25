// Dunia Kita — UI/PanelIzinTanah
// Panel izin tanah Dunia Bersama (Fase 7, GDD §17): ketuk petak
// kosong milik kota -> pilih jenis bangunan -> ajukan izin ke wali
// kota -> panel keputusan (disetujui / ditolak / bersyarat + alasan
// + harga) -> bayar -> petak jadi milik -> baru boleh bangun.
// Panel ini juga dipakai untuk info petak milik pemain lain dan
// untuk melihat kas kota.
//
// Pola seperti panel lain (dibuat sekali, disembunyikan sampai
// dibutuhkan), tapi dibuat LAZY oleh ServerManager — bukan oleh
// UIManager — supaya UIManager tetap < 600 baris. ServerManager
// memanggil PanelIzinTanah.PastikanAda() sebelum memakai Instance.

using UnityEngine;
using UnityEngine.UI;

public class PanelIzinTanah
{
    public static PanelIzinTanah Instance { get; private set; }

    private GameObject panel;
    private Text judul;
    private Text isiTeks;
    private RectTransform isiDaftar;
    private GameObject wadahDaftar;
    private Button tombolBayar;
    private Text labelBayar;
    private Button tombolSerobot;

    private int petakX;
    private int petakY;
    private int izinIdAktif;

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
        Instance = new PanelIzinTanah();
        Instance.Bangun(canvas.GetComponent<RectTransform>());
    }

    public static void TampilkanTanahKota(int x, int y)
    {
        PastikanAda();
        if (Instance != null)
        {
            Instance.TampilkanPilihanBangunan(x, y);
        }
    }

    public static void TampilkanInfoPemilik(Vector2Int petak, string pemilik,
        string namaBangunan)
    {
        PastikanAda();
        if (Instance != null)
        {
            Instance.TampilkanInfo(pemilik, namaBangunan, petak.x, petak.y);
        }
    }

    public static void SembunyikanJikaAda()
    {
        if (Instance != null)
        {
            Instance.Sembunyikan();
        }
    }

    // Panel dibuat sekali; disembunyikan sampai dibutuhkan.
    public void Bangun(RectTransform akar)
    {
        panel = UiFactory.BuatPanel(akar, "PanelIzinTanah", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, y=170, ukuran 760x740.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 170, 760, 740);
        RectTransform isi = panel.GetComponent<RectTransform>();
        judul = UiFactory.BuatTeks(isi, "JudulIzinTanah",
            "Izin Tanah", 44, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 60);
        isiTeks = UiFactory.BuatTeks(isi, "IsiIzinTanah", "", 32, "#2F3B2C",
            TextAnchor.UpperLeft);
        UiFactory.AturPosisi(isiTeks.GetComponent<RectTransform>(), 40, 100, 680, 420);
        // Daftar pilihan bangunan (scroll, seperti menu bangun) —
        // mulai di bawah 2 baris catatan (y=210), setinggi 310.
        isiDaftar = UiFactory.BuatScroll(isi, "DaftarIzinBangunan",
            40, 210, 680, 310);
        wadahDaftar = isiDaftar.gameObject;

        tombolBayar = UiFactory.BuatTombol(isi, "TombolBayarIzin",
            "Bayar", "#4E7A51", "#FFFFFF",
            delegate { Bayar(); }, 36);
        UiFactory.AturPosisi(tombolBayar.GetComponent<RectTransform>(),
            40, 540, 320, 90);
        labelBayar = UiFactory.LabelTombol(tombolBayar);
        Button tombolKas = UiFactory.BuatTombol(isi, "TombolKasKota",
            "Kas Kota", "#4E7A51", "#FFFFFF",
            delegate
            {
                if (ServerManager.Instance != null)
                {
                    ServerManager.Instance.MintaKasKota();
                }
            }, 36);
        UiFactory.AturPosisi(tombolKas.GetComponent<RectTransform>(),
            400, 540, 320, 90);
        Button tombolTutup = UiFactory.BuatTombol(isi, "TombolTutupIzin",
            "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { Sembunyikan(); }, 36);
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(),
            40, 640, 320, 90);
        tombolSerobot = UiFactory.BuatTombol(isi, "TombolSerobotTanah",
            "Klaim Paksa (Rp500rb)", "#C0392B", "#FFFFFF",
            delegate
            {
                if (ServerManager.Instance != null)
                {
                    ServerManager.Instance.MintaSerobotTanah(petakX, petakY);
                }
            }, 28);
        UiFactory.AturPosisi(tombolSerobot.GetComponent<RectTransform>(),
            400, 640, 320, 90);
        tombolSerobot.gameObject.SetActive(false);
        panel.SetActive(false);
    }

    public void Tampilkan()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void Sembunyikan()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void TampilkanPesan(string pesan)
    {
        PastikanTampil();
        isiTeks.text = pesan;
        isiTeks.gameObject.SetActive(true);
        wadahDaftar.SetActive(false);
        tombolBayar.gameObject.SetActive(false);
        if (tombolSerobot != null) tombolSerobot.gameObject.SetActive(false);
    }

    // Mode 1: pilih jenis bangunan untuk diajukan izinnya.
    public void TampilkanPilihanBangunan(int x, int y)
    {
        petakX = x;
        petakY = y;
        izinIdAktif = -1;
        judul.text = "Tanah Kota (" + x + ", " + y + ")";
        BuildManager bm = Object.FindObjectOfType<BuildManager>();
        GameManager gm = GameManager.Instance;
        int levelDunia = gm != null ? gm.LevelDunia : 1;
        string catatan = "Pilih bangunan yang mau diajukan ke wali kota.\n";
        if (bm != null && bm.HitungBangunan("balaikota") <= 0)
        {
            catatan += "Wali kota bekerja dari kantor sementara (belum ada "
                + "Balai Kota) — izin tetap bisa diajukan.";
        }
        else
        {
            catatan += "Balai Kota sudah berdiri — biaya izin sedikit "
                + "lebih murah.";
        }
        isiTeks.text = catatan;
        isiTeks.gameObject.SetActive(true);
        wadahDaftar.SetActive(true);
        tombolBayar.gameObject.SetActive(false);
        if (tombolSerobot != null) tombolSerobot.gameObject.SetActive(false);
        // Hapus tombol lama, buat ulang sesuai daftar bangunan.
        foreach (Transform anak in isiDaftar)
        {
            Object.Destroy(anak.gameObject);
        }
        float atas = 0f;
        if (bm != null && bm.daftarBangunan != null)
        {
            foreach (BuildingData data in bm.daftarBangunan)
            {
                if (data == null) continue;
                BuildingData salinan = data;
                bool terbuka = ProgresiDunia.ApakahTerbuka(data.levelBuka, levelDunia);
                string label = terbuka
                    ? data.nama + " — ajukan izin"
                    : data.nama + " — Terkunci (Level " + data.levelBuka + ")";
                string warna = terbuka ? "#4E7A51" : "#8A8A8A";
                Button tombol = UiFactory.BuatTombol(isiDaftar,
                    "TombolIzin_" + data.id, label, warna, "#FFFFFF",
                    delegate
                    {
                        if (!ProgresiDunia.ApakahTerbuka(
                            salinan.levelBuka, levelDunia))
                        {
                            TampilkanPesan(salinan.nama + " terbuka di Level "
                                + salinan.levelBuka + ".");
                            return;
                        }
                        Ajukan(salinan);
                    }, 34);
                UiFactory.AturPosisi(tombol.GetComponent<RectTransform>(),
                    0, atas, 640, 90);
                atas += 110f;
            }
        }
        isiDaftar.sizeDelta = new Vector2(680f, atas + 20f);
        Tampilkan();
    }

    private void Ajukan(BuildingData data)
    {
        isiTeks.text = "Mengajukan " + data.nama + " ke wali kota…";
        isiTeks.gameObject.SetActive(true);
        wadahDaftar.SetActive(false);
        if (ServerManager.Instance != null)
        {
            ServerManager.Instance.AjukanIzin(petakX, petakY, data.id);
        }
    }

    // Mode 2: keputusan wali kota (dipanggil ServerManager).
    public void TampilkanKeputusan(int x, int y, string idBangunan,
        int izinId, string hasil, string alasan,
        int hargaTanah, int biayaIzin, int total, string syarat)
    {
        petakX = x;
        petakY = y;
        izinIdAktif = izinId;
        BuildManager bm = Object.FindObjectOfType<BuildManager>();
        string nama = idBangunan;
        if (bm != null && bm.CariData(idBangunan) != null)
        {
            nama = bm.CariData(idBangunan).nama;
        }
        judul.text = "Keputusan Wali Kota";
        string teks = "Bangunan: " + nama + " di petak (" + x + ", " + y + ")\n\n"
            + alasan + "\n\n"
            + "Harga tanah: " + FormatUtil.Rupiah(hargaTanah) + "\n"
            + "Biaya izin: " + FormatUtil.Rupiah(biayaIzin) + "\n"
            + "Total bayar: " + FormatUtil.Rupiah(total);
        if (syarat == "jalan_bersebelahan")
        {
            teks += "\n\nSyarat: wajib ada Jalan di petak bersebelahan "
                + "sebelum bangunan dipasang.";
        }
        isiTeks.text = teks;
        isiTeks.gameObject.SetActive(true);
        wadahDaftar.SetActive(false);
        if (hasil == "Disetujui" || hasil == "Bersyarat")
        {
            labelBayar.text = "Bayar " + FormatUtil.Rupiah(total);
            tombolBayar.gameObject.SetActive(true);
        }
        else
        {
            tombolBayar.gameObject.SetActive(false);
        }
        Tampilkan();
    }

    private void Bayar()
    {
        if (izinIdAktif < 0 || ServerManager.Instance == null)
        {
            return;
        }
        tombolBayar.gameObject.SetActive(false);
        isiTeks.text = "Membayar ke kas kota…";
        ServerManager.Instance.BayarIzin(izinIdAktif, petakX, petakY);
    }

    // Mode 3: info petak milik pemain lain.
    public void TampilkanInfo(string pemilik, string namaBangunan, int x, int y)
    {
        petakX = x;
        petakY = y;
        judul.text = "Tanah Milik " + pemilik;
        string teks = "Petak (" + x + ", " + y + ") milik " + pemilik + ".\n\n";
        if (!string.IsNullOrEmpty(namaBangunan))
        {
            teks += "Bangunan di sini: " + namaBangunan + ".\n\n";
        }
        else
        {
            teks += "Belum ada bangunan di sini.\n\n";
        }
        teks += pemilik == "Kota"
            ? "Ini adalah fasilitas umum milik Kota."
            : "Kamu dapat mengajukan klaim paksa tanah ini (berisiko diadukan ke Polisi / Wali Kota).";
        isiTeks.text = teks;
        isiTeks.gameObject.SetActive(true);
        wadahDaftar.SetActive(false);
        tombolBayar.gameObject.SetActive(false);
        if (tombolSerobot != null)
        {
            tombolSerobot.gameObject.SetActive(pemilik != "Kota");
        }
        Tampilkan();
    }

    // Mode 4: kas kota (saldo + riwayat sudah diformat ServerManager).
    public void TampilkanKasKota(long saldo, string teksRiwayat)
    {
        judul.text = "Kas Kota";
        isiTeks.text = "Saldo kas kota: " + FormatUtil.Rupiah(saldo) + "\n\n"
            + "Uang izin + harga tanah masuk ke sini untuk fasilitas umum.\n\n"
            + teksRiwayat;
        isiTeks.gameObject.SetActive(true);
        wadahDaftar.SetActive(false);
        tombolBayar.gameObject.SetActive(false);
        if (tombolSerobot != null) tombolSerobot.gameObject.SetActive(false);
        Tampilkan();
    }

    private void PastikanTampil()
    {
        if (panel != null && !panel.activeSelf)
        {
            panel.SetActive(true);
        }
    }
}
