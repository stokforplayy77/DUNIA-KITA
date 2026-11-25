// Dunia Kita — UIManager
// Membangun seluruh UI saat runtime (GDD §11 & §17: LANDSCAPE 1920x1080):
// bar atas (koin, permata, hari + hitung mundur, warga, kebahagiaan,
// reputasi, level + tombol Misi, Pencapaian & Pengaturan), bar bawah
// (tombol Bangun IKON saja), menu bangun, panel detail bangunan
// (+ "Lihat Interior" untuk hunian), panel misi, panel pencapaian,
// panel pengaturan, panel interior, panel peristiwa penghuni,
// laporan harian, dan pesan toast. Semua teks Bahasa Indonesia;
// semua dialog dibuat di dalam game (bukan dialog bawaan sistem). Tanpa emoji.
//
// FASE 3 revisi (24 Nov 2026): tombol "Lewati Hari" & "Simpan"
// DIHAPUS — hari otomatis tiap 5 menit, simpan otomatis.

using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Sprite ikon tombol Bangun (palu + rumah kecil) — diisi dari Scene.
    public Sprite ikonBangun;

    private BuildManager buildManager;
    private CitizenManager citizenManager;
    private InteriorManager interiorManager;

    private Canvas canvas;
    private Text teksKoin;
    private Text teksPermata;
    private Text teksHari;
    private Text teksPopulasi;
    private Text teksBahagia;
    private Text teksReputasi; // FASE 4B: reputasi (GDD §9.1).
    private Text teksLevel;

    private GameObject panelBangun;
    private Text judulBangun;
    private RectTransform isiMenuBangun;

    private GameObject panelDetail;
    private Text judulDetail;
    private Text isiDetail;
    private Button tombolLihatInterior; // FASE 4B: hanya hunian.
    private Button tombolUpgrade;
    private Text teksTombolUpgrade;
    private Button tombolPindah;
    private Button tombolJual;

    private GameObject panelLaporan;
    private Text isiLaporan;

    // FASE 5: panel perayaan & tutorial hidup di kelas sendiri
    // (PanelPerayaan.cs, PanelTutorial.cs) supaya berkas ini < 600 baris.
    private PanelPerayaan panelPerayaan;
    private PanelTutorial panelTutorial;

    private PanelMisi panelMisi;
    private PanelPengaturan panelPengaturan;
    private PanelKejadian panelKejadian;
    private PanelPencapaian panelPencapaian;
    // FASE 4B: panel interior + panel peristiwa penghuni.
    private PanelInterior panelInterior;
    private PanelPeristiwaPenghuni panelPeristiwa;
    private PanelDiagnosisKota panelDiagnosis;

    private Text teksToast;
    private GameObject panelToast;
    private float sisaWaktuToast;

    private Vector2Int petakDipilih;
    private bool adaPetakDipilih;
    private BangunanTerpasang bangunanDipilih;

    private void Awake()
    {
        buildManager = FindObjectOfType<BuildManager>();
        citizenManager = FindObjectOfType<CitizenManager>();
        interiorManager = FindObjectOfType<InteriorManager>();
        BuatEventSystemJikaBelumAda();
        BangunUI();
    }

    private void Update()
    {
        if (sisaWaktuToast > 0f)
        {
            sisaWaktuToast -= Time.deltaTime;
            if (sisaWaktuToast <= 0f && panelToast != null)
            {
                panelToast.SetActive(false);
            }
        }
    }

    private void BuatEventSystemJikaBelumAda()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // ---- Pembangunan UI (acuan layar 1920x1080, koordinat kiri-atas) ----
    private void BangunUI()
    {
        canvas = UiFactory.BuatCanvas("CanvasUI");
        RectTransform akar = canvas.GetComponent<RectTransform>();
        BangunBarAtas(akar);
        BangunBarBawah(akar);
        BangunPanelBangun(akar);
        BangunPanelDetail(akar);
        BangunPanelLaporan(akar);
        panelPerayaan = new PanelPerayaan();
        panelPerayaan.Bangun(akar);
        BangunToast(akar);
        panelMisi = new PanelMisi();
        panelMisi.Bangun(akar);
        panelPengaturan = new PanelPengaturan();
        panelPengaturan.Bangun(akar);
        panelKejadian = new PanelKejadian();
        panelKejadian.Bangun(akar);
        panelPencapaian = new PanelPencapaian();
        panelPencapaian.Bangun(akar);
        panelInterior = new PanelInterior();
        panelInterior.Bangun(akar);
        panelPeristiwa = new PanelPeristiwaPenghuni();
        panelPeristiwa.Bangun(akar);
        panelTutorial = new PanelTutorial();
        panelTutorial.Bangun(akar);
        panelDiagnosis = new PanelDiagnosisKota();
        panelDiagnosis.Bangun(akar);
    }

    // Bar atas: 7 angka status + tombol Misi, Pencapaian & Pengaturan.
    private void BangunBarAtas(RectTransform akar)
    {
        GameObject bar = UiFactory.BuatPanel(akar, "BarAtas", "#2F3B2C", 0.92f);
        UiFactory.AturPosisi(bar.GetComponent<RectTransform>(), 0, 0, 1920, 110);
        teksKoin = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksKoin", "Rp0", 40, "#FFD166", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksKoin.GetComponent<RectTransform>(), 30, 12, 430, 48);
        teksPermata = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksPermata", "Permata: 0", 40, "#7FD4FF", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksPermata.GetComponent<RectTransform>(), 480, 12, 300, 48);
        teksHari = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksHari", "Hari 1", 40, "#FFFFFF", TextAnchor.MiddleRight);
        UiFactory.AturPosisi(teksHari.GetComponent<RectTransform>(), 800, 12, 540, 48);
        teksPopulasi = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksPopulasi", "Warga: 0", 30, "#FFFFFF", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksPopulasi.GetComponent<RectTransform>(), 30, 58, 300, 44);
        teksBahagia = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksBahagia", "Bahagia: 50%", 30, "#FFFFFF", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksBahagia.GetComponent<RectTransform>(), 340, 58, 260, 44);
        // FASE 4B: reputasi pemain (GDD §9.1) — baris 2 bar atas.
        teksReputasi = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksReputasi", "Reputasi: 50", 30, "#FFFFFF", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksReputasi.GetComponent<RectTransform>(), 610, 58, 230, 44);
        teksLevel = UiFactory.BuatTeks(bar.GetComponent<RectTransform>(),
            "TeksLevel", "Level 1 - Tanah Kosong", 30, "#FFFFFF", TextAnchor.MiddleRight);
        UiFactory.AturPosisi(teksLevel.GetComponent<RectTransform>(), 850, 58, 490, 44);
        Button tombolMisi = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolMisi", "Misi", "#E07840", "#FFFFFF",
            delegate { TampilkanMisi(); }, 28);
        UiFactory.AturPosisi(tombolMisi.GetComponent<RectTransform>(), 1350, 10, 170, 90);
        Button tombolPencapaian = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolPencapaian", "Pencapaian", "#C99A2B", "#FFFFFF",
            delegate { TampilkanPencapaian(); }, 28);
        UiFactory.AturPosisi(tombolPencapaian.GetComponent<RectTransform>(), 1530, 10, 170, 90);
        Button tombolPengaturan = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolPengaturan", "Pengaturan", "#5A6B7A", "#FFFFFF",
            delegate { TampilkanPengaturan(); }, 28);
        UiFactory.AturPosisi(tombolPengaturan.GetComponent<RectTransform>(), 1710, 10, 170, 90);
    }

    // Bar bawah: tombol Bangun IKON + tombol Diagnosis Kota.
    private void BangunBarBawah(RectTransform akar)
    {
        GameObject bar = UiFactory.BuatPanel(akar, "BarBawah", "#2F3B2C", 0.92f);
        UiFactory.AturPosisi(bar.GetComponent<RectTransform>(), 0, 940, 1920, 140);
        Button tombolSosial = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolSosial", "Sosial", "#0284C7", "#FFFFFF",
            delegate { SembunyikanPanel(); PanelSosialMultiplayer.Tampilkan(); }, 26);
        UiFactory.AturPosisi(tombolSosial.GetComponent<RectTransform>(), 20, 20, 200, 100);
        Button tombolLaporan = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolLaporanKota", "Laporan Kota", "#D97706", "#FFFFFF",
            delegate { SembunyikanPanel(); PanelPeristiwaMultiplayer.Tampilkan(); }, 26);
        UiFactory.AturPosisi(tombolLaporan.GetComponent<RectTransform>(), 240, 20, 200, 100);
        Button tombolOnline = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolDuniaBersama", "Dunia Bersama", "#6C5CE7", "#FFFFFF",
            delegate { SembunyikanPanel(); PanelAkun.Tampilkan(); }, 26);
        UiFactory.AturPosisi(tombolOnline.GetComponent<RectTransform>(), 460, 20, 210, 100);
        Button tombolBangun = UiFactory.BuatTombolIkon(bar.GetComponent<RectTransform>(),
            "TombolBangun", ikonBangun, "#4E7A51",
            delegate { TampilkanMenuBangunDariTombol(); });
        // Posisi relatif terhadap bar bawah (bar di y=940, tinggi 140).
        UiFactory.AturPosisi(tombolBangun.GetComponent<RectTransform>(), 700, 10, 520, 120);
        Button tombolInfo = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolDiagnosis", "Info Kota", "#3E6B89", "#FFFFFF",
            delegate { TampilkanDiagnosis(); }, 28);
        UiFactory.AturPosisi(tombolInfo.GetComponent<RectTransform>(), 1250, 20, 210, 100);
        Button tombolDistrik = UiFactory.BuatTombol(bar.GetComponent<RectTransform>(),
            "TombolDistrik", "Distrik", "#0D9488", "#FFFFFF",
            delegate { SembunyikanPanel(); PanelDistrik.Tampilkan(); }, 26);
        UiFactory.AturPosisi(tombolDistrik.GetComponent<RectTransform>(), 1480, 20, 210, 100);
    }

    private void BangunPanelBangun(RectTransform akar)
    {
        panelBangun = UiFactory.BuatPanel(akar, "PanelBangun", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-880)/2=100.
        UiFactory.AturPosisi(panelBangun.GetComponent<RectTransform>(), 580, 100, 760, 880);
        judulBangun = UiFactory.BuatTeks(panelBangun.GetComponent<RectTransform>(),
            "JudulBangun", "Bangun di sini", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judulBangun.GetComponent<RectTransform>(), 0, 20, 760, 70);
        isiMenuBangun = UiFactory.BuatScroll(panelBangun.GetComponent<RectTransform>(),
            "ScrollBangun", 20, 100, 720, 620);
        Button tombolTutup = UiFactory.BuatTombol(panelBangun.GetComponent<RectTransform>(),
            "TombolTutupBangun", "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { SembunyikanPanel(); });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 180, 740, 400, 110);
        panelBangun.SetActive(false);
    }

    private void BangunPanelDetail(RectTransform akar)
    {
        panelDetail = UiFactory.BuatPanel(akar, "PanelDetail", "#FFF8E7", 0.98f);
        UiFactory.AturPosisi(panelDetail.GetComponent<RectTransform>(), 540, 180, 840, 720);
        judulDetail = UiFactory.BuatTeks(panelDetail.GetComponent<RectTransform>(),
            "JudulDetail", "Bangunan", 46, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judulDetail.GetComponent<RectTransform>(), 0, 20, 840, 70);
        isiDetail = UiFactory.BuatTeks(panelDetail.GetComponent<RectTransform>(),
            "IsiDetail", "", 32, "#2F3B2C", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(isiDetail.GetComponent<RectTransform>(), 40, 100, 760, 230);
        // FASE 4B: tombol "Lihat Interior" — hanya untuk hunian.
        tombolLihatInterior = PanelInterior.BuatTombolLihatInterior(
            panelDetail.GetComponent<RectTransform>(),
            delegate
            {
                if (bangunanDipilih != null)
                {
                    TampilkanInterior(bangunanDipilih);
                }
            });
        tombolUpgrade = UiFactory.BuatTombol(panelDetail.GetComponent<RectTransform>(),
            "TombolUpgrade", "Upgrade", "#2E7D32", "#FFFFFF",
            delegate
            {
                if (bangunanDipilih != null && buildManager.UpgradeBangunan(bangunanDipilih))
                {
                    TampilkanDetail(bangunanDipilih);
                }
            }, 26);
        UiFactory.AturPosisi(tombolUpgrade.GetComponent<RectTransform>(), 40, 440, 370, 95);
        teksTombolUpgrade = tombolUpgrade.GetComponentInChildren<Text>();
        tombolPindah = UiFactory.BuatTombol(panelDetail.GetComponent<RectTransform>(),
            "TombolPindah", "Pindah Lokasi", "#3E6B89", "#FFFFFF",
            delegate
            {
                if (bangunanDipilih != null)
                {
                    buildManager.MulaiPindahBangunan(bangunanDipilih);
                }
            }, 26);
        UiFactory.AturPosisi(tombolPindah.GetComponent<RectTransform>(), 430, 440, 370, 95);
        tombolJual = UiFactory.BuatTombol(panelDetail.GetComponent<RectTransform>(),
            "TombolJual", "Jual Bangunan", "#C0392B", "#FFFFFF",
            delegate
            {
                buildManager.JualBangunan(bangunanDipilih);
                bangunanDipilih = null;
                SembunyikanPanel();
            });
        UiFactory.AturPosisi(tombolJual.GetComponent<RectTransform>(), 40, 560, 370, 105);
        Button tombolTutup = UiFactory.BuatTombol(panelDetail.GetComponent<RectTransform>(),
            "TombolTutupDetail", "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { SembunyikanPanel(); });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 430, 560, 370, 105);
        panelDetail.SetActive(false);
    }

    private void BangunPanelLaporan(RectTransform akar)
    {
        panelLaporan = UiFactory.BuatPanel(akar, "PanelLaporan", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-760)/2=160 (diperbesar Fase 3).
        UiFactory.AturPosisi(panelLaporan.GetComponent<RectTransform>(), 580, 160, 760, 760);
        Text judul = UiFactory.BuatTeks(panelLaporan.GetComponent<RectTransform>(),
            "JudulLaporan", "Laporan Harian", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 70);
        isiLaporan = UiFactory.BuatTeks(panelLaporan.GetComponent<RectTransform>(),
            "IsiLaporan", "", 34, "#2F3B2C", TextAnchor.UpperLeft);
        UiFactory.AturPosisi(isiLaporan.GetComponent<RectTransform>(), 40, 110, 680, 500);
        Button tombolTutup = UiFactory.BuatTombol(panelLaporan.GetComponent<RectTransform>(),
            "TombolTutupLaporan", "Tutup", "#4E7A51", "#FFFFFF",
            delegate { panelLaporan.SetActive(false); });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 180, 640, 400, 110);
        panelLaporan.SetActive(false);
    }

    private void BangunToast(RectTransform akar)
    {
        panelToast = UiFactory.BuatPanel(akar, "Toast", "#2F3B2C", 0.92f);
        // Di bawah bar atas, tengah horizontal: (1920-800)/2=560.
        UiFactory.AturPosisi(panelToast.GetComponent<RectTransform>(), 560, 140, 800, 90);
        teksToast = UiFactory.BuatTeks(panelToast.GetComponent<RectTransform>(),
            "TeksToast", "", 38, "#FFFFFF", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(teksToast.GetComponent<RectTransform>(), 0, 0, 800, 90);
        panelToast.SetActive(false);
    }

    // ---- API publik untuk manager lain ----
    public void PerbaruiHUD()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        teksKoin.text = FormatUtil.Rupiah(GameManager.Instance.Koin);
        teksPermata.text = "Permata: " + GameManager.Instance.Permata;
        // Hitung mundur ke pergantian hari ("Hari 2 • 03:12").
        teksHari.text = FormatUtil.Hari(GameManager.Instance.Hari)
            + " • " + GameManager.Instance.TeksSisaHari();
        teksPopulasi.text = "Warga: " + citizenManager.Populasi;
        teksBahagia.text = "Bahagia: " + citizenManager.Kebahagiaan + "%";
        teksReputasi.text = "Reputasi: " + GameManager.Instance.Reputasi;
        teksLevel.text = "Level " + GameManager.Instance.LevelDunia
            + " - " + GameManager.Instance.NamaLevelDunia();
    }

    public void TampilkanMenuBangun(Vector2Int petak, BuildingData[] daftar)
    {
        SembunyikanPanel();
        petakDipilih = petak;
        adaPetakDipilih = true;
        bool adaJalan = false;
        if (buildManager != null)
        {
            foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
            {
                if (b.Data != null && b.Data.id == "jalan")
                {
                    int dx = System.Math.Abs(b.PosisiGrid.x - petak.x);
                    int dy = System.Math.Abs(b.PosisiGrid.y - petak.y);
                    if (dx + dy == 1) { adaJalan = true; break; }
                }
            }
        }
        judulBangun.text = "Petak (" + petak.x + "," + petak.y + ") "
            + (adaJalan ? "[Terhubung Jalan]" : "[Tanpa Jalan -50%]");
        // Hapus tombol lama, buat ulang sesuai daftar bangunan.
        foreach (Transform anak in isiMenuBangun)
        {
            Destroy(anak.gameObject);
        }
        int levelDunia = GameManager.Instance != null
            ? GameManager.Instance.LevelDunia : 1;
        // FASE 4 — baris perluasan lahan (selalu ada di atas menu).
        bool bisaPerluas = GameManager.Instance != null
            && levelDunia >= 5
            && GameManager.Instance.UkuranGridSaatIni < GameBalance.UkuranGridDiperluas;
        bool sudahLuas = GameManager.Instance != null
            && GameManager.Instance.UkuranGridSaatIni >= GameBalance.UkuranGridDiperluas;
        string labelPerluas = sudahLuas
            ? "Lahan sudah 16x16"
            : (bisaPerluas
                ? "Perluas Lahan ke 16x16 — " + FormatUtil.Rupiah(GameBalance.HargaPerluasanLahan)
                : "Perluas Lahan — terbuka di Level 5");
        string warnaPerluas = bisaPerluas ? "#4E7A51" : "#8A8A8A";
        Button tombolPerluas = UiFactory.BuatTombol(isiMenuBangun,
            "TombolPerluas", labelPerluas, warnaPerluas, "#FFFFFF",
            delegate
            {
                if (GameManager.Instance != null
                    && GameManager.Instance.PerluasLahan())
                {
                    TampilkanMenuBangun(petakDipilih, buildManager.daftarBangunan);
                }
            });
        UiFactory.AturPosisi(tombolPerluas.GetComponent<RectTransform>(), 40, 110, 680, 110);
        float atas = 240f;
        foreach (BuildingData data in daftar)
        {
            BuildingData salinan = data; // Untuk closure tombol.
            bool terbuka = ProgresiDunia.ApakahTerbuka(data.levelBuka, levelDunia);
            bool sudahBalaiKota = data.id == "balaikota" && buildManager != null
                && buildManager.HitungBangunan("balaikota") >= 1;
            string label;
            string warna;
            if (sudahBalaiKota)
            {
                label = data.nama + " - Sudah dibangun";
                warna = "#8A8A8A";
            }
            else if (terbuka)
            {
                label = data.nama + " - " + FormatUtil.Rupiah(data.biayaBangun);
                warna = "#4E7A51";
            }
            else
            {
                label = data.nama + " - Terkunci (Level " + data.levelBuka + ")";
                warna = "#8A8A8A";
            }
            Button tombol = UiFactory.BuatTombol(isiMenuBangun,
                "Tombol_" + data.id, label, warna, "#FFFFFF",
                delegate
                {
                    if (sudahBalaiKota)
                    {
                        TampilkanPesan("Balai Kota sudah dibangun — hanya 1 per dunia.");
                        return;
                    }
                    if (!ProgresiDunia.ApakahTerbuka(salinan.levelBuka, levelDunia))
                    {
                        TampilkanPesan(salinan.nama + " terbuka di Level "
                            + salinan.levelBuka + " (butuh "
                            + ProgresiDunia.TeksSyarat(salinan.levelBuka) + ").");
                        return;
                    }
                    if (buildManager.CobaBangun(salinan, petakDipilih))
                    {
                        SembunyikanPanel();
                    }
                });
            UiFactory.AturPosisi(tombol.GetComponent<RectTransform>(), 40, atas, 680, 110);
            atas += 130f;
        }
        isiMenuBangun.sizeDelta = new Vector2(720f, atas + 20f);
        panelBangun.SetActive(true);
    }

    // Tombol "Bangun" di bar bawah: buka menu untuk petak terakhir diketuk.
    public void TampilkanMenuBangunDariTombol()
    {
        if (!adaPetakDipilih || buildManager == null)
        {
            TampilkanPesan("Ketuk petak kosong di peta untuk memilih lokasi membangun.");
            return;
        }
        TampilkanMenuBangun(petakDipilih, buildManager.daftarBangunan);
    }

    public void TampilkanDetail(BangunanTerpasang bangunan)
    {
        SembunyikanPanel();
        bangunanDipilih = bangunan;
        BuildingData d = bangunan.Data;
        int lv = SistemUpgradeBangunan.NormalisasiLevel(bangunan.Level);
        long masukEfektif = (d.pemasukanHarian
            * SistemUpgradeBangunan.PengaliPemasukanPersen(lv)) / 100;
        int kapEfektif = SistemUpgradeBangunan.KapasitasEfektif(d.kapasitasWarga, lv);
        judulDetail.text = d.nama + " (Lv " + lv + ")";
        string teks = d.deskripsi + "\n\n"
            + "Pemasukan/hari: " + FormatUtil.Rupiah(masukEfektif) + "\n"
            + "Perawatan/hari: " + FormatUtil.Rupiah(d.biayaPerawatanHarian) + "\n"
            + "Kapasitas warga: " + kapEfektif + "\n"
            + "Harga jual: " + FormatUtil.Rupiah(
                EconomyCalculator.HitungHargaJual(d.biayaBangun,
                    GameBalance.RasioJualKembali));
        bool milikSendiri = ServerManager.Instance == null
            || !ServerManager.Instance.ModeOnline
            || ServerManager.Instance.ApakahMilikSaya(bangunan);
        if (!milikSendiri)
        {
            teks += "\nStatus: Fasilitas Umum Kota";
        }
        if (tombolUpgrade != null)
        {
            bool bisaUp = milikSendiri && SistemUpgradeBangunan.BisaUpgrade(d.id, lv);
            tombolUpgrade.gameObject.SetActive(bisaUp);
            if (bisaUp && teksTombolUpgrade != null)
            {
                long biayaUp = SistemUpgradeBangunan.BiayaUpgrade(d.biayaBangun, lv);
                teksTombolUpgrade.text = "Upgrade Lv " + (lv + 1)
                    + " (" + FormatUtil.Rupiah(biayaUp) + ")";
            }
        }
        if (tombolPindah != null) tombolPindah.gameObject.SetActive(milikSendiri);
        if (tombolJual != null) tombolJual.gameObject.SetActive(milikSendiri);
        if (tombolLihatInterior != null)
        {
            tombolLihatInterior.gameObject.SetActive(
                InteriorManager.ApakahPunyaInterior(d));
        }
        if (interiorManager != null)
        {
            teks += interiorManager.TeksRingkasanDetail(bangunan);
        }
        isiDetail.text = teks;
        panelDetail.SetActive(true);
    }

    public void TampilkanDiagnosis()
    {
        SembunyikanPanel();
        if (panelDiagnosis == null || buildManager == null) return;
        EconomyManager econ = FindObjectOfType<EconomyManager>();
        bool[] tanpaJalan = buildManager.HitungTanpaJalan();
        int jmlTanpaJalan = 0;
        for (int i = 0; i < tanpaJalan.Length; i++)
        {
            if (tanpaJalan[i]) jmlTanpaJalan++;
        }
        int kap = 0;
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            kap += SistemUpgradeBangunan.KapasitasEfektif(b.Data.kapasitasWarga, b.Level);
        }
        long bersih = econ != null ? econ.PerkiraanHarian().Bersih : 0;
        panelDiagnosis.Tampilkan(new SistemDiagnosisKota.InputDiagnosis
        {
            Populasi = citizenManager != null ? citizenManager.Populasi : 0,
            KapasitasHunian = kap,
            Kebahagiaan = citizenManager != null ? citizenManager.Kebahagiaan : 50,
            JumlahTaman = buildManager.HitungBangunan("taman"),
            JumlahBangunanTanpaJalan = jmlTanpaJalan,
            HariMenunggak = econ != null ? econ.HariMenunggakBerturut : 0,
            JumlahKomersial = buildManager.HitungKomersial(),
            JumlahHunian = buildManager.HitungKategori(KategoriBangunan.Hunian),
            PemasukanBersihHarian = bersih,
            LevelDunia = GameManager.Instance != null ? GameManager.Instance.LevelDunia : 1
        });
    }

    public void TampilkanMisi()
    {
        SembunyikanPanel();
        if (panelMisi != null)
        {
            panelMisi.Tampilkan();
        }
    }

    // FASE 4: panel pencapaian (hadiah Permata).
    public void TampilkanPencapaian()
    {
        SembunyikanPanel();
        if (panelPencapaian != null)
        {
            panelPencapaian.Tampilkan();
        }
    }

    public void TampilkanPengaturan()
    {
        SembunyikanPanel();
        if (panelPengaturan != null)
        {
            panelPengaturan.Tampilkan();
        }
    }

    // FASE 3: panel perayaan naik level (dipanggil GameManager).
    // FASE 5: isi panel hidup di PanelPerayaan.cs.
    public void TampilkanPerayaanNaikLevel(int levelBaru, long hadiah)
    {
        SembunyikanPanel();
        if (panelPerayaan != null)
        {
            panelPerayaan.Tampilkan(levelBaru, hadiah,
                buildManager != null ? buildManager.daftarBangunan : null);
        }
    }

    // FASE 5: tampilkan tutorial (dipanggil GameManager saat game
    // baru dibuka — PanelTutorial.CobaTampilkanAwal).
    public void TampilkanTutorial()
    {
        if (panelTutorial != null)
        {
            panelTutorial.Tampilkan();
        }
    }

    // FASE 5: ulangi tutorial dari Pengaturan.
    public void TampilkanTutorialUlang()
    {
        if (panelTutorial != null)
        {
            panelTutorial.TampilkanUlang();
        }
    }

    // FASE 3: panel pilihan kejadian acak (dipanggil GameManager).
    public void TampilkanKejadian(string idKejadian)
    {
        SembunyikanPanel();
        if (panelKejadian != null)
        {
            panelKejadian.Tampilkan(idKejadian);
        }
    }

    // FASE 4B: buka mode interior tembus pandang untuk satu hunian
    // (GDD §5.6) — panel detail ditutup, bangunan beralih ke sprite
    // denah di tempatnya, panel perabot dibuka.
    public void TampilkanInterior(BangunanTerpasang bangunan)
    {
        SembunyikanPanel();
        if (interiorManager != null)
        {
            interiorManager.MasukModeInterior(bangunan);
        }
        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null && bangunan != null && bangunan.Objek != null)
        {
            Vector3 p = bangunan.Objek.transform.position;
            cam.PusatkanKe(new Vector3(p.x + 1.4f, 0f, p.y));
        }
        if (panelInterior != null)
        {
            panelInterior.Tampilkan(bangunan);
        }
    }

    // FASE 4B: panel pilihan peristiwa penghuni (GDD §9.1) —
    // dipanggil GameManager sesudah roll peristiwa di akhir hari.
    public void TampilkanPeristiwaPenghuni(string idPeristiwa)
    {
        SembunyikanPanel();
        if (panelPeristiwa != null)
        {
            panelPeristiwa.Tampilkan(idPeristiwa);
        }
    }

    public void TampilkanLaporanHari(EconomyCalculator.RingkasanHarian ringkasan, int hari,
        string teksKejadian)
    {
        int bahagia = citizenManager != null ? citizenManager.Kebahagiaan : 0;
        int pengali = EconomyCalculator.PengaliWarung(bahagia);
        string efekWarung = pengali == 80 ? "komersial -20%"
            : (pengali == 110 ? "komersial +10%" : "komersial normal");
        string teks = "Hari " + hari + " selesai.\n\n"
            + "Pemasukan komersial: " + FormatUtil.Rupiah(ringkasan.PemasukanWarung) + "\n"
            + "Pemasukan lain: " + FormatUtil.Rupiah(ringkasan.PemasukanLain) + "\n"
            + "Pajak kota (2,5%): -" + FormatUtil.Rupiah(ringkasan.Pajak) + "\n"
            + "Perawatan bangunan: -" + FormatUtil.Rupiah(ringkasan.Upkeep) + "\n"
            + "Bersih: " + FormatUtil.Rupiah(ringkasan.Bersih) + "\n"
            + "Kebahagiaan: " + bahagia + "% (" + efekWarung + ")";
        if (!string.IsNullOrEmpty(teksKejadian))
        {
            teks += "\n\nKejadian: " + teksKejadian;
        }
        if (ringkasan.Menunggak)
        {
            teks += "\n\nPeringatan: koin tidak cukup! Upkeep menunggak "
                + FormatUtil.Rupiah(ringkasan.Tunggakan)
                + " (hari ke-" + ringkasan.HariMenunggakBerturut
                + " berturut-turut). Kebahagiaan turun bila menunggak 2 hari.";
        }
        isiLaporan.text = teks;
        panelLaporan.SetActive(true);
    }

    public void TampilkanPesan(string pesan)
    {
        if (teksToast == null || panelToast == null)
        {
            return;
        }
        teksToast.text = pesan;
        panelToast.SetActive(true);
        sisaWaktuToast = 2.5f;
    }

    public void SembunyikanPanel()
    {
        adaPetakDipilih = false;
        panelBangun.SetActive(false);
        panelDetail.SetActive(false);
        if (panelPerayaan != null)
        {
            panelPerayaan.Sembunyikan();
        }
        if (panelTutorial != null)
        {
            panelTutorial.Sembunyikan();
        }
        if (panelMisi != null)
        {
            panelMisi.Sembunyikan();
        }
        if (panelPengaturan != null)
        {
            panelPengaturan.Sembunyikan();
        }
        if (panelKejadian != null)
        {
            panelKejadian.Sembunyikan();
        }
        if (panelPencapaian != null)
        {
            panelPencapaian.Sembunyikan();
        }
        // FASE 4B: panel interior + panel peristiwa ikut ditutup,
        // dan bangunan keluar dari mode interior (sprite luar kembali).
        if (panelInterior != null)
        {
            panelInterior.Sembunyikan();
        }
        if (panelPeristiwa != null)
        {
            panelPeristiwa.Sembunyikan();
        }
        if (panelDiagnosis != null)
        {
            panelDiagnosis.Sembunyikan();
        }
        if (interiorManager != null)
        {
            interiorManager.KeluarModeInterior();
        }
    }
}
