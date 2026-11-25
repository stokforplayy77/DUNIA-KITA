// Dunia Kita — GameManager
// Pusat state permainan (GDD §13, Manager pattern).
// Memegang koin, permata, hari game, dan level dunia; mengatur
// urutan mulai game (Awake = siapkan, Start = muat simpanan atau
// mulai baru) dan pergantian hari otomatis tiap 5 menit.
//
// FASE 3 revisi (24 Nov 2026): TIDAK ADA tombol "Lewati Hari" /
// "Simpan". Hari OTOMATIS tiap 5 menit (GameBalance.DurasiHariDetik),
// simpan OTOMATIS (tiap hari + tiap 60 detik + saat aplikasi berhenti).
// Alur pergantian hari ATOMIK (SedangProsesHari mengunci pewaktu):
// ekonomi -> hari++ -> hitung ulang -> cek naik level (panel perayaan
// + hadiah) -> roll kejadian (20%) -> laporan -> simpan. Pewaktu
// DIJEDA selama panel perayaan/kejadian terbuka.
//
// - Awake: hanya daftarkan Instance (semua manager saling menemukan
//   lewat FindObjectOfType, jadi tidak ada ketergantungan urutan Awake).
// - Start: semua Awake sudah jalan -> aman memuat simpanan, membangun
//   ulang dunia, dan menghitung ulang ekonomi/warga.

using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // State permainan (disimpan ke JSON oleh SaveManager).
    public long Koin { get; private set; }
    public int Permata { get; private set; }
    public int Hari { get; private set; }
    public int LevelDunia { get; private set; }

    // FASE 3: keadaan kejadian acak (efek "besok" — bonus kebahagiaan
    // dan pengali hujan) — ikut disimpan oleh SaveManager.
    public SistemKejadian.KeadaanKejadian KeadaanKejadian { get; private set; }

    // FASE 4: pencapaian yang sudah diklaim hadiahnya (id dari
    // SistemPencapaian) — ikut disimpan oleh SaveManager. Klaim
    // sekali per pencapaian; syarat dinilai ulang tiap kali dari
    // keadaan dunia (SistemPencapaian.ApakahTercapai).
    public List<string> PencapaianDiklaim { get; private set; }

    // FASE 4B: reputasi (GDD §9.1, 50 awal, 0-100) + efek "besok"
    // peristiwa penghuni — ikut disimpan oleh SaveManager.
    public int Reputasi { get; private set; }
    public SistemPeristiwaPenghuni.KeadaanPeristiwa KeadaanPeristiwa { get; private set; }

    // FASE 4: penghitung misi harian untuk hari yang sedang berjalan
    // (direset tiap pergantian hari; ikut disimpan supaya tidak
    // hilang bila game ditutup di tengah hari).
    public int DibangunHariIni { get; private set; }
    public int TamanHariIni { get; private set; }

    // FASE 4: ukuran grid saat ini (12 = awal, 16 = sesudah
    // perluasan lahan Level 5).
    public int UkuranGridSaatIni { get; private set; }

    // Pencapaian yang sudah diberitahukan lewat toast di sesi ini
    // (runtime saja, tidak disimpan — supaya toast tidak muncul
    // berulang tiap kali PeriksaPencapaian dipanggil).
    private HashSet<string> pencapaianDiberitahu;

    // True selama alur pergantian hari belum selesai (panel perayaan /
    // kejadian masih terbuka) — pewaktu hari dijeda selama proses ini.
    public bool SedangProsesHari { get; private set; }

    // Pewaktu hari otomatis (Fase 3 revisi) + pewaktu simpan otomatis.
    public PewaktuHari Pewaktu { get; private set; }
    private float waktuSejakSimpan;

    private BuildManager buildManager;
    private EconomyManager economyManager;
    private CitizenManager citizenManager;
    private SaveManager saveManager;
    private UIManager uiManager;
    private GridManager gridManager;
    private InteriorManager interiorManager;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = GameBalance.TargetFrameRate;
        buildManager = FindObjectOfType<BuildManager>();
        economyManager = FindObjectOfType<EconomyManager>();
        citizenManager = FindObjectOfType<CitizenManager>();
        saveManager = FindObjectOfType<SaveManager>();
        uiManager = FindObjectOfType<UIManager>();
        gridManager = FindObjectOfType<GridManager>();
        interiorManager = FindObjectOfType<InteriorManager>();
        if (FindObjectOfType<EfekDuniaManager>() == null)
        {
            new GameObject("EfekDuniaManager").AddComponent<EfekDuniaManager>();
        }
        Pewaktu = new PewaktuHari();
        waktuSejakSimpan = 0f;
        PencapaianDiklaim = new List<string>();
        pencapaianDiberitahu = new HashSet<string>();
    }

    // Tiap frame: majukan pewaktu hari (dijeda selama panel perayaan /
    // kejadian terbuka) dan simpan otomatis tiap 60 detik.
    // PerbaruiHUD dipanggil tiap frame supaya hitung mundur di HUD
    // ("Hari 2 • 03:12") bergerak tiap detik.
    private void Update()
    {
        if (!SedangProsesHari && Pewaktu.Tambah(Time.deltaTime))
        {
            ProsesPergantianHari();
        }
        waktuSejakSimpan += Time.deltaTime;
        if (waktuSejakSimpan >= GameBalance.IntervalSimpanOtomatisDetik)
        {
            waktuSejakSimpan = 0f;
            saveManager.Simpan();
        }
        uiManager.PerbaruiHUD();
    }

    private void Start()
    {
        SaveData simpanan = saveManager.Muat();
        if (simpanan != null)
        {
            MuatDariSimpanan(simpanan);
        }
        else
        {
            MulaiBaru();
        }
        citizenManager.HitungUlang();
        uiManager.PerbaruiHUD();
        PeriksaPencapaian();
        PanelTutorial.CobaTampilkanAwal(); // FASE 5: tutorial pemain baru.
        saveManager.Simpan();
    }

    private void MulaiBaru()
    {
        Koin = GameBalance.KoinAwal;
        Permata = GameBalance.PermataAwal;
        Hari = GameBalance.HariAwal;
        LevelDunia = GameBalance.LevelDuniaAwal;
        KeadaanKejadian = SistemKejadian.KeadaanAwal();
        // FASE 4B: reputasi mulai 50, tidak ada efek peristiwa tertunda.
        Reputasi = GameBalance.ReputasiAwal;
        KeadaanPeristiwa = SistemPeristiwaPenghuni.KeadaanAwal();
        PencapaianDiklaim = new List<string>();
        DibangunHariIni = 0;
        TamanHariIni = 0;
        UkuranGridSaatIni = GameBalance.UkuranGrid;
        SedangProsesHari = false;
        Debug.Log("[GameManager] Dunia baru dimulai. Koin awal: " + FormatUtil.Rupiah(Koin));
    }

    private void MuatDariSimpanan(SaveData data)
    {
        Koin = data.koin;
        Permata = data.permata;
        Hari = data.hari;
        LevelDunia = data.levelDunia;
        // Amankan level dari simpanan lama/rusak (Fase 4: maks Level 6).
        if (LevelDunia < 1)
        {
            LevelDunia = 1;
        }
        if (LevelDunia > GameBalance.LevelMaksDunia)
        {
            LevelDunia = GameBalance.LevelMaksDunia;
        }
        // FASE 4: pulihkan ukuran grid — perluasan lahan HARUS jalan
        // SEBELUM MuatBangunan supaya bangunan di petak 12-15 punya
        // grid yang cukup (simpanan lama: 0 -> 12, tidak diperluas).
        UkuranGridSaatIni = SaveData.UkuranGridEfektif(data.ukuranGrid);
        if (UkuranGridSaatIni > GameBalance.UkuranGrid && gridManager != null)
        {
            gridManager.Perluas(UkuranGridSaatIni);
        }
        // FASE 4: pulihkan pencapaian yang sudah diklaim (simpanan
        // lama: list kosong — pencapaian bisa diraih & diklaim lagi
        // dari awal, tidak ada yang hilang).
        PencapaianDiklaim = data.pencapaianDiklaim != null
            ? new List<string>(data.pencapaianDiklaim)
            : new List<string>();
        // FASE 4: penghitung misi harian hanya berlaku bila simpanan
        // masih di hari yang sama; bila hari sudah berganti (game
        // ditutup semalaman), penghitung direset 0.
        if (data.hariMisiTerakhir == data.hari)
        {
            DibangunHariIni = data.dibangunHariIni < 0 ? 0 : data.dibangunHariIni;
            TamanHariIni = data.tamanHariIni < 0 ? 0 : data.tamanHariIni;
        }
        else
        {
            DibangunHariIni = 0;
            TamanHariIni = 0;
        }
        // Pulihkan keadaan kejadian (simpanan 2.0/1.0: 0 = default aman).
        SistemKejadian.KeadaanKejadian keadaan = SistemKejadian.KeadaanAwal();
        keadaan.BonusBahagiaBesok = data.bonusBahagiaBesok < 0
            ? 0 : data.bonusBahagiaBesok;
        keadaan.PengaliHujanBesok = data.pengaliHujanBesok <= 0
            ? 100 : data.pengaliHujanBesok;
        KeadaanKejadian = keadaan;
        // FASE 4B: reputasi — simpanan lama (0 = "tidak ada data")
        // -> 50; simpanan "5.0" memakai nilainya (0 = memang 0).
        if (data.versi == GameBalance.VersiSimpan
            || data.versi == GameBalance.VersiSimpanFase5_1)
        {
            Reputasi = SistemPeristiwaPenghuni.BatasiReputasi(data.reputasi);
        }
        else
        {
            Reputasi = GameBalance.ReputasiAwal;
        }
        // FASE 4B: pulihkan efek "besok" peristiwa (simpanan lama: tidak ada).
        SistemPeristiwaPenghuni.KeadaanPeristiwa keadaanPeristiwa =
            SistemPeristiwaPenghuni.KeadaanAwal();
        keadaanPeristiwa.SewaKostHilangBesok = data.sewaKostHilangBesok;
        keadaanPeristiwa.KapasitasHilangBesok = data.kapasitasHilangBesok < 0
            ? 0 : data.kapasitasHilangBesok;
        keadaanPeristiwa.TambahanKoinBesok = data.tambahanKoinBesok < 0
            ? 0 : data.tambahanKoinBesok;
        keadaanPeristiwa.BonusBahagiaBesok = data.bonusBahagiaPeristiwaBesok;
        KeadaanPeristiwa = keadaanPeristiwa;
        SedangProsesHari = false;
        buildManager.MuatBangunan(data.bangunan);
        // FASE 4B: pulihkan interior (sesudah bangunan dimuat).
        if (interiorManager != null)
        {
            interiorManager.MuatInterior(data.interior);
        }
        // Pulihkan keadaan ekonomi Fase 2 (simpanan Fase 1: 0, 0).
        if (economyManager != null)
        {
            economyManager.MuatKeadaan(data.tunggakan, data.hariMenunggakBerturut);
        }
        Debug.Log("[GameManager] Simpanan dimuat. Hari " + Hari + ", koin: " + FormatUtil.Rupiah(Koin));
    }

    // Pergantian hari OTOMATIS (dipanggil Update saat pewaktu habis):
    // ekonomi -> hari++ -> hitung ulang -> cek naik level -> roll
    // kejadian -> laporan -> simpan.
    public void ProsesPergantianHari()
    {
        if (SedangProsesHari)
        {
            return;
        }
        SedangProsesHari = true;
        // Terapkan efek kejadian kemarin untuk hari ini, lalu kosongkan
        // supaya dipakai tepat satu hari.
        if (economyManager != null)
        {
            economyManager.PengaliHujanBesok = KeadaanKejadian.PengaliHujanBesok;
            // FASE 4B: sewa Kost hari ini hilang (keringanan peristiwa).
            economyManager.SewaKostHilangHariIni = KeadaanPeristiwa.SewaKostHilangBesok;
        }
        if (citizenManager != null)
        {
            citizenManager.BonusSementara = KeadaanKejadian.BonusBahagiaBesok
                + KeadaanPeristiwa.BonusBahagiaBesok;
            // FASE 4B: kamar kosong peristiwa mengurangi kapasitas hari ini.
            citizenManager.KapasitasHilangHariIni = KeadaanPeristiwa.KapasitasHilangBesok;
        }
        // FASE 4B: cicilan pembayaran peristiwa masuk hari ini.
        if (KeadaanPeristiwa.TambahanKoinBesok > 0)
        {
            UbahKoin(KeadaanPeristiwa.TambahanKoinBesok);
        }
        SistemKejadian.KeadaanKejadian keadaan = SistemKejadian.KeadaanAwal();
        KeadaanKejadian = keadaan;
        KeadaanPeristiwa = SistemPeristiwaPenghuni.KeadaanAwal();

        EconomyCalculator.RingkasanHarian ringkasan = economyManager.TerapkanHari();
        Hari++;
        // FASE 4: hari baru = misi harian baru; penghitung bangunan
        // hari ini direset (misi kemarin yang belum selesai hangus).
        DibangunHariIni = 0;
        TamanHariIni = 0;
        citizenManager.HitungUlang();
        // Balai Kota: sumber reputasi pasif (+2/hari bila ada).
        if (buildManager != null)
        {
            int bonusRep = SistemBalaiKota.BonusReputasiHarian(
                buildManager.HitungBangunan("balaikota"));
            if (bonusRep != 0)
            {
                UbahReputasi(bonusRep);
            }
        }
        PeriksaPencapaian();
        uiManager.PerbaruiHUD();
        ringkasanTerakhir = ringkasan;

        // Cek naik level (bisa naik 1 level per hari).
        int tujuan = LevelDunia + 1;
        if (ProgresiDunia.BisaNaikKe(tujuan, citizenManager.Populasi,
            citizenManager.Kebahagiaan))
        {
            LevelDunia = tujuan;
            long hadiah = ProgresiDunia.HadiahNaikLevel(tujuan);
            UbahKoin(hadiah);
            AudioManager.MainkanNaikLevel();
            uiManager.PerbaruiHUD();
            uiManager.TampilkanPerayaanNaikLevel(tujuan, hadiah);
            return;
        }
        LanjutkanSesudahPerayaan();
    }

    // Dipanggil UIManager saat tombol "Lanjutkan" di panel perayaan.
    public void LanjutkanSesudahPerayaan()
    {
        // Roll kejadian hari ini (20%) — acak dari Unity.
        string idKejadian = SistemKejadian.AcakKejadian(Random.value);
        if (idKejadian != SistemKejadian.TidakAda)
        {
            uiManager.TampilkanKejadian(idKejadian);
            return;
        }
        // FASE 4B: bila tidak ada kejadian biasa, roll peristiwa
        // penghuni (10%, butuh >= 1 Kost; maks 1 panel/hari).
        if (buildManager != null && buildManager.HitungBangunan("kost") >= 1)
        {
            string idPeristiwa = SistemPeristiwaPenghuni.AcakPeristiwa(Random.value);
            if (idPeristiwa != SistemPeristiwaPenghuni.TidakAda)
            {
                uiManager.TampilkanPeristiwaPenghuni(idPeristiwa);
                return;
            }
        }
        SelesaikanHari("");
    }

    // Dipanggil PanelKejadian saat pemain memilih salah satu pilihan.
    public void TerapkanPilihanKejadian(string idKejadian, int pilihan)
    {
        long biaya = SistemKejadian.Biaya(idKejadian, pilihan);
        bool berhasilBayar = true;
        if (biaya > 0)
        {
            berhasilBayar = UbahKoin(-biaya);
        }
        string teksHasil;
        if (berhasilBayar)
        {
            SistemKejadian.KeadaanKejadian keadaan = KeadaanKejadian;
            SistemKejadian.TerapkanPilihan(idKejadian, pilihan, ref keadaan);
            KeadaanKejadian = keadaan;
            if (idKejadian == SistemKejadian.Pedagang && pilihan == 0)
            {
                TambahPermata(GameBalance.PermataPedagangKeliling);
            }
        }
        teksHasil = SistemKejadian.TeksHasil(idKejadian, pilihan, berhasilBayar);
        uiManager.PerbaruiHUD();
        SelesaikanHari(teksHasil);
    }

    // FASE 4B: pilihan peristiwa (GDD §9.1).
    public void TerapkanPilihanPeristiwa(string idPeristiwa, int pilihan)
    {
        SistemPeristiwaPenghuni.DampakPeristiwa dampak =
            SistemPeristiwaPenghuni.Dampak(idPeristiwa, pilihan);
        if (dampak.Koin > 0)
        {
            UbahKoin(dampak.Koin);
        }
        else if (dampak.Koin < 0)
        {
            // Rugi dipotong sampai koin 0 (bukan utang).
            UbahKoin(-System.Math.Min(-dampak.Koin, Koin));
        }
        if (dampak.Reputasi != 0)
        {
            UbahReputasi(dampak.Reputasi);
        }
        SistemPeristiwaPenghuni.KeadaanPeristiwa keadaan = KeadaanPeristiwa;
        keadaan.SewaKostHilangBesok = dampak.SewaKostHilangBesok;
        keadaan.KapasitasHilangBesok = dampak.KapasitasHilangBesok;
        keadaan.TambahanKoinBesok = dampak.TambahanKoinBesok;
        keadaan.BonusBahagiaBesok = dampak.BonusBahagiaBesok;
        KeadaanPeristiwa = keadaan;
        string teksHasil = SistemPeristiwaPenghuni.TeksHasil(idPeristiwa, pilihan);
        uiManager.PerbaruiHUD();
        SelesaikanHari(teksHasil);
    }

    // FASE 4B: ubah reputasi (dibatasi 0-100).
    public void UbahReputasi(int delta)
    {
        Reputasi = SistemPeristiwaPenghuni.BatasiReputasi(Reputasi + delta);
        uiManager.PerbaruiHUD();
    }

    // FASE 4B: keadaan interior untuk misi tutorial.
    public int TotalPerabotTerpasang()
    {
        if (interiorManager == null)
        {
            return 0;
        }
        return interiorManager.TotalPerabotTerpasang();
    }

    public bool ApakahAdaKostLengkap()
    {
        if (interiorManager == null)
        {
            return false;
        }
        return interiorManager.ApakahAdaKostLengkap();
    }

    // Langkah terakhir alur pergantian hari: laporan + peringatan
    // tunggakan (bila ada) + simpan + buka kunci pewaktu hari.
    private void SelesaikanHari(string teksKejadian)
    {
        // FASE 5: pemasukan bersih hari ini berbunyi koin masuk.
        if (ringkasanTerakhir.Bersih > 0) AudioManager.MainkanKoin();
        uiManager.TampilkanLaporanHari(ringkasanTerakhir, Hari - 1, teksKejadian);
        if (ringkasanTerakhir.Menunggak)
        {
            uiManager.TampilkanPesan("Peringatan: koin tidak cukup! Upkeep menunggak "
                + FormatUtil.Rupiah(ringkasanTerakhir.Tunggakan) + ".");
        }
        saveManager.Simpan();
        // Simpan otomatis baru saja jalan -> reset pewaktu simpan
        // 60 detik supaya tidak simpan dua kali beruntun.
        waktuSejakSimpan = 0f;
        SedangProsesHari = false;
    }

    // Teks hitung mundur hari untuk HUD ("03:12").
    public string TeksSisaHari()
    {
        return Pewaktu != null ? Pewaktu.TeksSisaWaktu() : "05:00";
    }

    private EconomyCalculator.RingkasanHarian ringkasanTerakhir;

    public bool ApakahKoinCukup(long jumlah)
    {
        return Koin >= jumlah;
    }

    // Mengubah koin (positif = tambah, negatif = kurangi).
    // Mengembalikan false bila koin tidak cukup untuk pengurangan.
    public bool UbahKoin(long jumlah)
    {
        if (jumlah < 0 && Koin + jumlah < 0)
        {
            return false;
        }
        Koin += jumlah;
        uiManager.PerbaruiHUD();
        return true;
    }

    // Tambah permata (hadiah pedagang keliling Fase 3).
    public void TambahPermata(int jumlah)
    {
        if (jumlah <= 0)
        {
            return;
        }
        Permata += jumlah;
        uiManager.PerbaruiHUD();
    }

    // FASE 4 — misi harian: dicatat BuildManager tiap kali bangunan
    // berhasil dibangun (dipakai SistemMisiHarian).
    public void CatatBangunanDibangun(string idBangunan)
    {
        DibangunHariIni++;
        if (idBangunan == "taman")
        {
            TamanHariIni++;
        }
    }

    // FASE 4 — keadaan pencapaian dari dunia nyata saat ini.
    public SistemPencapaian.KeadaanPencapaian KeadaanPencapaianSaatIni()
    {
        SistemPencapaian.KeadaanPencapaian keadaan =
            new SistemPencapaian.KeadaanPencapaian();
        if (buildManager != null)
        {
            keadaan.JumlahKost = buildManager.HitungBangunan("kost");
            keadaan.JumlahTaman = buildManager.HitungBangunan("taman");
            keadaan.JumlahKomersial = buildManager.HitungKomersial();
            keadaan.PunyaSekolah = buildManager.HitungBangunan("sekolah") >= 1;
            keadaan.PunyaKampus = buildManager.HitungBangunan("kampus") >= 1;
        }
        keadaan.LevelDunia = LevelDunia;
        keadaan.Kebahagiaan = citizenManager != null ? citizenManager.Kebahagiaan : 0;
        return keadaan;
    }

    // FASE 4 — cek pencapaian yang baru terpenuhi; beri tahu pemain
    // SEKALI per sesi lewat toast (klaim hadiahnya di panel Pencapaian).
    // Dipanggil sesudah membangun, sesudah pergantian hari, dan saat muat.
    public void PeriksaPencapaian()
    {
        if (uiManager == null)
        {
            return;
        }
        SistemPencapaian.KeadaanPencapaian keadaan = KeadaanPencapaianSaatIni();
        foreach (string id in SistemPencapaian.SemuaId())
        {
            if (pencapaianDiberitahu.Contains(id))
            {
                continue;
            }
            if (!SistemPencapaian.ApakahTercapai(id, keadaan))
            {
                continue;
            }
            pencapaianDiberitahu.Add(id);
            uiManager.TampilkanPesan("Pencapaian baru: " + SistemPencapaian.Judul(id)
                + "! Klaim " + SistemPencapaian.HadiahPermata(id)
                + " Permata di panel Pencapaian.");
        }
    }

    // FASE 4 — klaim hadiah pencapaian (sekali per pencapaian).
    // Mengembalikan false bila syarat belum terpenuhi atau sudah diklaim.
    public bool KlaimPencapaian(string id)
    {
        if (PencapaianDiklaim == null)
        {
            PencapaianDiklaim = new List<string>();
        }
        if (PencapaianDiklaim.Contains(id))
        {
            return false;
        }
        if (!SistemPencapaian.ApakahTercapai(id, KeadaanPencapaianSaatIni()))
        {
            return false;
        }
        PencapaianDiklaim.Add(id);
        TambahPermata(SistemPencapaian.HadiahPermata(id));
        AudioManager.MainkanKlaim();
        return true;
    }

    // FASE 4 — perluasan lahan (GDD §8, Level 5): grid 12x12 -> 16x16,
    // sekali beli dengan koin. Mengembalikan false bila belum Level 5,
    // sudah diperluas, atau koin tidak cukup.
    public bool PerluasLahan()
    {
        if (LevelDunia < 5)
        {
            uiManager.TampilkanPesan("Perluasan lahan terbuka di Level 5 (Kota).");
            return false;
        }
        if (UkuranGridSaatIni >= GameBalance.UkuranGridDiperluas)
        {
            uiManager.TampilkanPesan("Lahan sudah diperluas sampai 16x16.");
            return false;
        }
        if (!UbahKoin(-GameBalance.HargaPerluasanLahan))
        {
            uiManager.TampilkanPesan("Koin tidak cukup untuk perluasan lahan ("
                + FormatUtil.Rupiah(GameBalance.HargaPerluasanLahan) + ").");
            return false;
        }
        if (gridManager == null || !gridManager.Perluas(GameBalance.UkuranGridDiperluas))
        {
            UbahKoin(GameBalance.HargaPerluasanLahan);
            uiManager.TampilkanPesan("Perluasan lahan gagal — koin dikembalikan.");
            return false;
        }
        UkuranGridSaatIni = GameBalance.UkuranGridDiperluas;
        uiManager.PerbaruiHUD();
        uiManager.TampilkanPesan("Lahan diperluas menjadi 16x16! Area baru siap dibangun.");
        if (saveManager != null)
        {
            saveManager.Simpan();
        }
        return true;
    }

    // Nama tampilan level dunia (GDD §8; Fase 4: Level 1-6).
    public string NamaLevelDunia()
    {
        return ProgresiDunia.NamaLevel(LevelDunia);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            saveManager.Simpan();
        }
    }

    private void OnApplicationQuit()
    {
        saveManager.Simpan();
    }
}
