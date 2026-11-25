// Dunia Kita — PanelInterior
// Panel pengaturan perabot di mode interior tembus pandang
// (GDD §5.6, Fase 4B): daftar 10 slot perabot hunian, dikelompokkan
// per ruangan (Kamar Tidur, Dapur, Kamar Mandi, Ruang Tamu).
//
// Pola sama seperti PanelMisi: panel dibuat sekali saat
// UIManager.Awake dan disembunyikan sampai dibutuhkan; isi digulir
// (ScrollRect) karena 4 judul ruangan + 10 slot tidak muat dalam
// satu layar. Tiap baris slot: nama perabot + harga (bila kosong)
// atau "Terpasang" + tombol Beli / Hapus (uang kembali 50%).
// Beli/hapus diteruskan ke InteriorManager; sesudah itu daftar
// digambar ulang supaya status dan koin terbaru terlihat.

using UnityEngine;
using UnityEngine.UI;

public class PanelInterior
{
    private InteriorManager interiorManager;
    private GameObject panel;
    private RectTransform isi;
    private Text judul;
    private BangunanTerpasang bangunan;

    // Panel dibuat sekali saat UIManager.Awake; disembunyikan sampai
    // pemain menekan "Lihat Interior" di panel detail hunian.
    public void Bangun(RectTransform akar)
    {
        interiorManager = Object.FindObjectOfType<InteriorManager>();

        panel = UiFactory.BuatPanel(akar, "PanelInterior", "#FFF8E7", 0.96f);
        // Sisi kanan layar agar bangunan tembus pandang di tengah tetap terlihat jelas!
        UiFactory.AturPosisi(panel.GetComponent<RectTransform>(), 1220, 120, 660, 800);
        judul = UiFactory.BuatTeks(panel.GetComponent<RectTransform>(),
            "JudulInterior", "Interior", 40, "#2F3B2C", TextAnchor.MiddleCenter);
        UiFactory.AturPosisi(judul.GetComponent<RectTransform>(), 0, 16, 660, 64);
        isi = UiFactory.BuatScroll(panel.GetComponent<RectTransform>(),
            "ScrollInterior", 20, 90, 620, 590);
        Button tombolTutup = UiFactory.BuatTombol(panel.GetComponent<RectTransform>(),
            "TombolTutupInterior", "Tutup Interior", "#8A8A8A", "#FFFFFF",
            delegate
            {
                Sembunyikan();
                if (interiorManager != null)
                {
                    interiorManager.KeluarModeInterior();
                }
            });
        UiFactory.AturPosisi(tombolTutup.GetComponent<RectTransform>(), 130, 695, 400, 90);

        panel.SetActive(false);
    }

    // Tombol "Lihat Interior" di panel detail hunian (dibuat
    // UIManager saat BangunPanelDetail; disembunyikan untuk
    // non-hunian di TampilkanDetail).
    public static Button BuatTombolLihatInterior(RectTransform induk,
        UnityEngine.Events.UnityAction klik)
    {
        Button tombol = UiFactory.BuatTombol(induk,
            "TombolLihatInterior", "Lihat Interior", "#4E7A51", "#FFFFFF",
            delegate { klik(); }, 30);
        UiFactory.AturPosisi(tombol.GetComponent<RectTransform>(), 40, 340, 760, 90);
        return tombol;
    }

    // Buka panel untuk satu hunian (mode interior sudah diaktifkan
    // UIManager sebelum memanggil ini).
    public void Tampilkan(BangunanTerpasang b)
    {
        bangunan = b;
        if (b != null && b.Data != null)
        {
            judul.text = "Interior — " + b.Data.nama;
        }
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

    // ---- Gambar ulang daftar slot (dipanggil tiap kali panel
    // dibuka dan sesudah beli/hapus perabot) ----
    // Satu loop untuk semua baris (4 judul ruangan + 10 slot =
    // 14 baris) supaya geometri tetap bisa dicek otomatis.
    private void PerbaruiIsi()
    {
        foreach (Transform anak in isi)
        {
            Object.Destroy(anak.gameObject);
        }
        if (bangunan == null || interiorManager == null)
        {
            return;
        }
        if (bangunan.Data != null && bangunan.Data.kategori != KategoriBangunan.Hunian)
        {
            TampilkanInteriorPublik(bangunan.Data);
            return;
        }
        System.Collections.Generic.List<string> isiInterior =
            interiorManager.AmbilInterior(bangunan);
        int jumlahTerpasang = SistemInterior.HitungPerabot(isiInterior);
        int bonus = SistemInterior.BonusBahagia(isiInterior);
        string ringkasan = "Perabot terpasang: " + jumlahTerpasang + "/"
            + SistemInterior.JumlahSlot + "  •  Bonus kebahagiaan: "
            + (bonus >= 0 ? "+" : "") + bonus;
        if (bangunan.Data != null && bangunan.Data.id == "kost"
            && SistemInterior.ApakahLengkap(isiInterior))
        {
            ringkasan += "  •  Sewa +25%";
        }
        Text teksRingkasan = UiFactory.BuatTeks(isi,
            "RingkasanInterior", ringkasan, 30, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(teksRingkasan.GetComponent<RectTransform>(),
            20, 20, 580, 46);

        float atas = 80f;
        string ruanganTerakhir = "";
        for (int slot = 0; slot < SistemInterior.JumlahSlot; slot++)
        {
            int salinanSlot = slot; // Untuk closure tombol.
            string ruangan = SistemInterior.NamaRuangan(slot);
            if (ruangan != ruanganTerakhir)
            {
                ruanganTerakhir = ruangan;
                Text judulRuangan = UiFactory.BuatTeks(isi,
                    "Ruangan_" + ruangan, ruangan, 34, "#2F3B2C", TextAnchor.MiddleLeft);
                UiFactory.AturPosisi(judulRuangan.GetComponent<RectTransform>(),
                    20, atas, 580, 46);
                atas += 110f;
            }
            string idPerabot = SistemInterior.IdPerabotSlot(slot);
            string namaPerabot = SistemInterior.NamaPerabot(idPerabot);
            string terpasang = isiInterior[slot];
            bool adaIsi = terpasang != null && terpasang.Length > 0;
            long harga = SistemInterior.HargaPerabot(idPerabot);
            int bonusPerabot = SistemInterior.BonusBahagiaPerabot(idPerabot);

            Text nama = UiFactory.BuatTeks(isi,
                "Nama_" + idPerabot, namaPerabot, 32, "#2F3B2C", TextAnchor.MiddleLeft);
            UiFactory.AturPosisi(nama.GetComponent<RectTransform>(), 20, atas, 340, 44);
            string deskripsi = adaIsi
                ? "Terpasang (bahagia +" + bonusPerabot + ")"
                : "Belum ada (bahagia +" + bonusPerabot + " bila dipasang)";
            Text teksDeskripsi = UiFactory.BuatTeks(isi,
                "Deskripsi_" + idPerabot, deskripsi, 26, "#5A5A5A", TextAnchor.UpperLeft);
            UiFactory.AturPosisi(teksDeskripsi.GetComponent<RectTransform>(),
                20, atas + 46, 340, 56);

            if (!adaIsi)
            {
                // Nama variabel sama dengan tombol Hapus di bawah
                // (tombolAksi) supaya alat cek-ui-bounds melihat
                // keduanya sebagai satu elemen: satu slot hanya
                // menampilkan satu tombol dalam satu waktu.
                Button tombolAksi = UiFactory.BuatTombol(isi,
                    "Beli_" + idPerabot, "Beli " + FormatUtil.Rupiah(harga),
                    "#4E7A51", "#FFFFFF",
                    delegate
                    {
                        if (interiorManager.BeliPerabot(bangunan, salinanSlot))
                        {
                            AudioManager.MainkanBangun();
                            UIManager ui = Object.FindObjectOfType<UIManager>();
                            if (ui != null)
                            {
                                ui.TampilkanPesan(namaPerabot + " dibeli ("
                                    + FormatUtil.Rupiah(harga) + ").");
                            }
                        }
                        else
                        {
                            AudioManager.MainkanTolak();
                            UIManager ui = Object.FindObjectOfType<UIManager>();
                            if (ui != null)
                            {
                                ui.TampilkanPesan("Koin tidak cukup untuk "
                                    + namaPerabot + ".");
                            }
                        }
                        PerbaruiIsi();
                    }, 30);
                UiFactory.AturPosisi(tombolAksi.GetComponent<RectTransform>(),
                    380, atas + 10, 210, 90);
            }
            else
            {
                Button tombolAksi = UiFactory.BuatTombol(isi,
                    "Hapus_" + idPerabot,
                    "Hapus (+" + FormatUtil.Rupiah(SistemInterior.HargaJualPerabot(idPerabot)) + ")",
                    "#C0392B", "#FFFFFF",
                    delegate
                    {
                        interiorManager.HapusPerabot(bangunan, salinanSlot);
                        PerbaruiIsi();
                    }, 30);
                UiFactory.AturPosisi(tombolAksi.GetComponent<RectTransform>(),
                    380, atas + 10, 210, 90);
            }
            atas += 110f;
        }
        isi.sizeDelta = new Vector2(600f, atas + 20f);
    }

    private void TampilkanInteriorPublik(BuildingData d)
    {
        string[] ruangan;
        if (d.id == "balaikota")
        {
            ruangan = new[]
            {
                "Ruang Wali Kota — Pengesahan izin & kebijakan distrik",
                "Lobi Pelayanan Izin — Antrean warga mengurus IMB & tanah",
                "Ruang Arsip Kota — Catatan kas kota & peta kepemilikan",
                "Ruang Rapat — Koordinasi pembangunan antar distrik"
            };
        }
        else if (d.id == "klinik")
        {
            ruangan = new[]
            {
                "Unit Gawat Darurat (IGD) — Penanganan medis cepat",
                "Ruang Periksa Dokter — Pemeriksaan kesehatan warga",
                "Apotek & Obat — Penyediaan obat harian gratis/murah",
                "Ruang Tunggu — Tempat istirahat pasien & keluarga"
            };
        }
        else if (d.id == "pos_polisi")
        {
            ruangan = new[]
            {
                "Meja Laporan Warga — Penerimaan aduan sengketa & keamanan",
                "Ruang Jaga & Patroli — Petugas piket 24 jam",
                "Ruang Arsip Kasus — Catatan peristiwa keamanan kota",
                "Ruang Tahanan Sementara — Pengamanan pelanggar hukum"
            };
        }
        else
        {
            ruangan = new[]
            {
                "Lobi Utama — Area penerimaan pengunjung & warga",
                "Ruang Layanan — Aktivitas operasional harian",
                "Ruang Staf — Koordinasi petugas bangunan",
                "Area Istirahat — Fasilitas pendukung kegiatan"
            };
        }
        Text info = UiFactory.BuatTeks(isi, "InfoPublik",
            "Aktivitas di dalam " + d.nama + " (lihat peta kiri):",
            28, "#2F3B2C", TextAnchor.MiddleLeft);
        UiFactory.AturPosisi(info.GetComponent<RectTransform>(), 20, 20, 580, 50);
        float atas = 90f;
        for (int i = 0; i < ruangan.Length; i++)
        {
            Text baris = UiFactory.BuatTeks(isi, "RuangPublik_" + i,
                "• " + ruangan[i], 28, "#2F3B2C", TextAnchor.UpperLeft);
            UiFactory.AturPosisi(baris.GetComponent<RectTransform>(), 20, atas, 580, 90);
            atas += 105f;
        }
        isi.sizeDelta = new Vector2(600f, atas + 20f);
    }
}
