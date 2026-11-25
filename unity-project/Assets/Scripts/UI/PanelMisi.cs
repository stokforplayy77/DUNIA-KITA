// Dunia Kita — PanelMisi
// Panel daftar misi tutorial Fase 1 + Fase 2 + Fase 3 + Fase 4B
// (GDD §8) dan misi harian Fase 4 (GDD §9). Misi dinilai LANGSUNG
// dari state game nyata (jumlah bangunan yang terpasang, populasi,
// hari, & kebahagiaan), bukan dari angka karangan:
//
//   Tutorial:
//   1. Bangun 1 Rumah      -> hadiah Rp25.000
//   2. Bangun 1 Warung     -> hadiah Rp50.000
//   3. Capai 4 Warga       -> hadiah Rp50.000 (butuh 2 Rumah Sederhana)
//   4. Bertahan 3 Hari    -> hadiah Rp25.000 (Hari mencapai 4;
//                              hari berganti otomatis tiap 5 menit)
//   5. Capai kebahagiaan 60% -> hadiah Rp50.000
//   6. Naik ke Level 2     -> hadiah Rp50.000
//   7. Bangun 1 Kost       -> hadiah Rp75.000
//   8. Capai 10 Warga      -> hadiah Rp50.000
//   9. Beli 1 Perabot      -> hadiah Rp25.000 (FASE 4B: ketuk hunian,
//                              "Lihat Interior", beli perabot apa saja)
//   10. Lengkapi 1 Kost    -> hadiah Rp75.000 (FASE 4B: isi semua
//                              10 slot perabot satu Kost)
//
//   Harian (Fase 4): 3 misi dipilih deterministik dari nomor hari
//   (SistemMisiHarian.PilihUntukHari) — misi yang tidak selesai
//   hangus saat hari berganti. Hadiah koin/permata kecil.
//
// Hadiah diklaim manual lewat tombol "Klaim" pada misi yang sudah
// selesai; tiap misi hanya bisa diklaim SEKALI (dicatat di PlayerPrefs
// supaya tidak bisa diklaim ulang dengan memuat ulang game; misi
// harian memakai kunci yang memuat nomor hari supaya bisa diklaim
// lagi di hari lain).
// Aturan hadiah ada di GameBalance (HadiahMisi* / HadiahHarian*).
//
// FASE 4: isi panel digulir (ScrollRect) — 10 tutorial + judul
// "Misi Harian" + 3 harian = 14 baris tidak muat dalam panel
// 760x1000 tanpa gulir.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMisi
{
    private BuildManager buildManager;
    private CitizenManager citizenManager;
    private EconomyManager economyManager;
    private GameManager gameManager;
    private GameObject panel;
    private RectTransform isi;

    private class Misi
    {
        public string id;
        public string judul;
        public string deskripsi;
        public long hadiah;
        public System.Func<bool> selesai;
    }

    // Satu baris di daftar (tutorial / judul bagian / harian).
    private class BarisMisi
    {
        public bool adalahJudul;
        public string judul;
        public string deskripsi;
        public string teksHadiah;
        public System.Func<bool> selesai;
        public System.Func<bool> diklaim;
        public System.Action klaim;
    }

    private List<Misi> daftar;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // tombol Misi ditekan.
    public void Bangun(RectTransform akar)
    {
        buildManager = Object.FindObjectOfType<BuildManager>();
        citizenManager = Object.FindObjectOfType<CitizenManager>();
        economyManager = Object.FindObjectOfType<EconomyManager>();
        gameManager = Object.FindObjectOfType<GameManager>();

        panel = UiFactory.BuatPanel(akar, "PanelMisi", "#FFF8E7", 0.98f);
        // Tengah layar: (1920-760)/2=580, (1080-1000)/2=40.
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 580, 40, 760, 1000);
        Text judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulMisi", "Misi", 48, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 20, 760, 70);
        // Area gulir: di bawah judul (y=100) sampai atas tombol
        // Tutup (y=880); lebar 720 (x=20..740 di dalam panel).
        isi = UiFactory.BuatScroll(panel.GetComponent<RectTransform>(),
            "ScrollMisi", 20, 100, 720, 780);
        Button tombolTutup = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolTutupMisi", "Tutup", "#8A8A8A", "#FFFFFF",
            delegate { Sembunyikan(); });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 180, 890, 400, 90);

        daftar = new List<Misi>();
        daftar.Add(new Misi
        {
            id = "misi_rumah",
            judul = "Bangun 1 Rumah",
            deskripsi = "Bangun Rumah Sederhana di petak kosong.",
            hadiah = GameBalance.HadiahMisiBangunRumah,
            selesai = delegate { return HitungBangunan("rumah") >= 1; }
        });
        daftar.Add(new Misi
        {
            id = "misi_warung",
            judul = "Bangun 1 Warung",
            deskripsi = "Bangun Warung untuk pemasukan harian.",
            hadiah = GameBalance.HadiahMisiBangunWarung,
            selesai = delegate { return HitungBangunan("warung") >= 1; }
        });
        daftar.Add(new Misi
        {
            id = "misi_warga",
            judul = "Capai 4 Warga",
            deskripsi = "Bangun 2 Rumah Sederhana supaya 4 warga pindah.",
            hadiah = GameBalance.HadiahMisiCapai4Warga,
            selesai = delegate { return Populasi() >= 4; }
        });
        daftar.Add(new Misi
        {
            id = "misi_3hari",
            judul = "Bertahan 3 Hari",
            deskripsi = "Tetap bermain sampai Hari ke-4 (hari berganti otomatis tiap 5 menit).",
            hadiah = GameBalance.HadiahMisiLewati3Hari,
            selesai = delegate { return Hari() >= 4; }
        });
        daftar.Add(new Misi
        {
            id = "misi_bahagia60",
            judul = "Capai kebahagiaan 60%",
            deskripsi = "Bangun taman dan jaga upkeep supaya warga senang.",
            hadiah = GameBalance.HadiahMisiBahagia60,
            selesai = delegate { return Kebahagiaan() >= 60; }
        });
        daftar.Add(new Misi
        {
            id = "misi_level2",
            judul = "Naik ke Level 2",
            deskripsi = "Capai 10 warga supaya duniamu jadi Dusun.",
            hadiah = GameBalance.HadiahMisiNaikLevel2,
            selesai = delegate { return LevelDunia() >= 2; }
        });
        daftar.Add(new Misi
        {
            id = "misi_kost",
            judul = "Bangun 1 Kost",
            deskripsi = "Bangun Kost (terbuka di Level 2) untuk 8 warga.",
            hadiah = GameBalance.HadiahMisiBangunKost,
            selesai = delegate { return HitungBangunan("kost") >= 1; }
        });
        daftar.Add(new Misi
        {
            id = "misi_warga10",
            judul = "Capai 10 Warga",
            deskripsi = "Bangun rumah dan kost supaya 10 warga pindah.",
            hadiah = GameBalance.HadiahMisiCapai10Warga,
            selesai = delegate { return Populasi() >= 10; }
        });
        // FASE 4B: misi interior (GDD §5.6).
        daftar.Add(new Misi
        {
            id = "misi_perabot",
            judul = "Beli 1 Perabot",
            deskripsi = "Ketuk hunian, buka Interior, dan beli satu perabot.",
            hadiah = GameBalance.HadiahMisiBeliPerabot,
            selesai = delegate { return TotalPerabot() >= 1; }
        });
        daftar.Add(new Misi
        {
            id = "misi_kost_lengkap",
            judul = "Lengkapi 1 Kost",
            deskripsi = "Isi semua 10 slot perabot satu Kost (sewa naik +25%).",
            hadiah = GameBalance.HadiahMisiLengkapiKost,
            selesai = delegate { return AdaKostLengkap(); }
        });

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

    // ---- Penilaian misi (dari state game nyata) ----
    private int HitungBangunan(string id)
    {
        if (buildManager == null || buildManager.DaftarTerpasang == null)
        {
            return 0;
        }
        int jumlah = 0;
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            if (b.Data != null && b.Data.id == id)
            {
                jumlah++;
            }
        }
        return jumlah;
    }

    private int Populasi()
    {
        if (citizenManager == null)
        {
            return 0;
        }
        return citizenManager.Populasi;
    }

    private int Hari()
    {
        if (gameManager == null)
        {
            return 0;
        }
        return gameManager.Hari;
    }

    private int Kebahagiaan()
    {
        if (citizenManager == null)
        {
            return 0;
        }
        return citizenManager.Kebahagiaan;
    }

    private int LevelDunia()
    {
        if (gameManager == null)
        {
            return 0;
        }
        return gameManager.LevelDunia;
    }

    // FASE 4B: keadaan interior (delegasi ke GameManager,
    // null-safe bila InteriorManager belum ada).
    private int TotalPerabot()
    {
        if (gameManager == null)
        {
            return 0;
        }
        return gameManager.TotalPerabotTerpasang();
    }

    private bool AdaKostLengkap()
    {
        if (gameManager == null)
        {
            return false;
        }
        return gameManager.ApakahAdaKostLengkap();
    }

    private static string KunciKlaim(string idMisi)
    {
        return "DK_Misi_" + idMisi;
    }

    private static bool SudahDiklaim(string idMisi)
    {
        return PlayerPrefs.GetInt(KunciKlaim(idMisi), 0) == 1;
    }

    // Keadaan misi harian hari ini (dari state game nyata).
    private SistemMisiHarian.KeadaanHarian KeadaanHarian()
    {
        SistemMisiHarian.KeadaanHarian keadaan =
            new SistemMisiHarian.KeadaanHarian();
        keadaan.DibangunHariIni = gameManager != null ? gameManager.DibangunHariIni : 0;
        keadaan.TamanHariIni = gameManager != null ? gameManager.TamanHariIni : 0;
        keadaan.Kebahagiaan = Kebahagiaan();
        keadaan.PerkiraanPemasukanHarian = economyManager != null
            ? economyManager.PerkiraanHarian().Pemasukan
            : 0;
        keadaan.Populasi = Populasi();
        keadaan.JumlahKomersial = buildManager != null ? buildManager.HitungKomersial() : 0;
        keadaan.JumlahTaman = HitungBangunan("taman");
        return keadaan;
    }

    // ---- Gambar ulang daftar (dipanggil tiap kali panel dibuka) ----
    // Satu loop untuk semua baris (10 tutorial + 1 judul bagian +
    // 3 harian = 14 baris) supaya geometri tetap bisa dicek otomatis.
    private void PerbaruiIsi()
    {
        foreach (Transform anak in isi)
        {
            Object.Destroy(anak.gameObject);
        }
        List<BarisMisi> baris = new List<BarisMisi>();
        foreach (Misi m in daftar)
        {
            Misi salinan = m; // Untuk closure tombol klaim.
            baris.Add(new BarisMisi
            {
                adalahJudul = false,
                judul = salinan.judul,
                deskripsi = salinan.deskripsi,
                teksHadiah = FormatUtil.Rupiah(salinan.hadiah),
                selesai = salinan.selesai,
                diklaim = delegate { return SudahDiklaim(salinan.id); },
                klaim = delegate
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.UbahKoin(salinan.hadiah);
                    }
                    AudioManager.MainkanKlaim();
                    PlayerPrefs.SetInt(KunciKlaim(salinan.id), 1);
                    PlayerPrefs.Save();
                }
            });
        }
        // Judul bagian misi harian (teks saja, tanpa tombol).
        int hari = Hari();
        baris.Add(new BarisMisi
        {
            adalahJudul = true,
            judul = "Misi Harian — Hari " + hari,
            deskripsi = "",
            teksHadiah = "",
            selesai = delegate { return false; },
            diklaim = delegate { return false; },
            klaim = delegate { }
        });
        // 3 misi harian hari ini (deterministik dari nomor hari).
        SistemMisiHarian.KeadaanHarian keadaan = KeadaanHarian();
        foreach (string id in SistemMisiHarian.PilihUntukHari(hari))
        {
            string salinanId = id; // Untuk closure tombol klaim.
            baris.Add(new BarisMisi
            {
                adalahJudul = false,
                judul = SistemMisiHarian.Judul(salinanId),
                deskripsi = SistemMisiHarian.Deskripsi(salinanId)
                    + " (hangus saat hari berganti)",
                teksHadiah = SistemMisiHarian.TeksHadiah(salinanId),
                selesai = delegate
                {
                    return SistemMisiHarian.ApakahSelesai(salinanId, keadaan);
                },
                diklaim = delegate
                {
                    return PlayerPrefs.GetInt(
                        SistemMisiHarian.KunciKlaim(hari, salinanId), 0) == 1;
                },
                klaim = delegate
                {
                    if (GameManager.Instance != null)
                    {
                        long koin = SistemMisiHarian.HadiahKoin(salinanId);
                        int permata = SistemMisiHarian.HadiahPermata(salinanId);
                        if (koin > 0)
                        {
                            GameManager.Instance.UbahKoin(koin);
                        }
                        if (permata > 0)
                        {
                            GameManager.Instance.TambahPermata(permata);
                        }
                    }
                    AudioManager.MainkanKlaim();
                    PlayerPrefs.SetInt(
                        SistemMisiHarian.KunciKlaim(hari, salinanId), 1);
                    PlayerPrefs.Save();
                }
            });
        }

        float atas = 100f;
        foreach (BarisMisi b in baris)
        {
            BarisMisi salinan = b; // Untuk closure tombol klaim.
            if (salinan.adalahJudul)
            {
                Text judulBagian = UiFactory.BuatTeks(isi,
                    "JudulHarian", salinan.judul, 36, "#2F3B2C", TextAnchor.MiddleLeft);
                UiFactory.AturPosisi(judulBagian.GetComponent<RectTransform>(),
                    40, atas, 680, 46);
                atas += 110f;
                continue;
            }
            bool selesai = salinan.selesai();
            bool diklaim = salinan.diklaim();

            Text nama = UiFactory.BuatTeks(isi,
                "Nama_" + salinan.judul, salinan.judul, 36, "#2F3B2C", TextAnchor.MiddleLeft);
            UiFactory.AturPosisi(nama.GetComponent<RectTransform>(), 40, atas, 440, 46);
            Text deskripsi = UiFactory.BuatTeks(isi,
                "Deskripsi_" + salinan.judul, salinan.deskripsi, 28, "#5A5A5A", TextAnchor.UpperLeft);
            UiFactory.AturPosisi(deskripsi.GetComponent<RectTransform>(), 40, atas + 50, 440, 56);

            if (selesai && !diklaim)
            {
                Button tombolKlaim = UiFactory.BuatTombol(isi,
                    "Klaim_" + salinan.judul, "Klaim " + salinan.teksHadiah,
                    "#4E7A51", "#FFFFFF",
                    delegate
                    {
                        salinan.klaim();
                        UIManager ui = Object.FindObjectOfType<UIManager>();
                        if (ui != null)
                        {
                            ui.TampilkanPesan("Hadiah misi diterima: "
                                + salinan.teksHadiah + ".");
                        }
                        PerbaruiIsi();
                    }, 30);
                UiFactory.AturPosisi(tombolKlaim.GetComponent<RectTransform>(),
                    500, atas + 10, 220, 90);
            }
            else
            {
                string status = !selesai
                    ? "Belum selesai"
                    : "Selesai — hadiah diterima";
                string warna = selesai ? "#2E7D32" : "#8A8A8A";
                Text teksStatus = UiFactory.BuatTeks(isi,
                    "Status_" + salinan.judul, status, 30, warna, TextAnchor.MiddleCenter);
                UiFactory.AturPosisi(teksStatus.GetComponent<RectTransform>(),
                    500, atas + 10, 220, 90);
            }
            atas += 110f;
        }
        // Tinggi konten = posisi akhir + ruang bawah; lebar konten 720
        // (sama dengan lebar area gulir di Bangun()).
        isi.sizeDelta = new Vector2(720f, atas + 20f);
    }
}
