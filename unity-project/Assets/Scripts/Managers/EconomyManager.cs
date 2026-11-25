// Dunia Kita — EconomyManager
// Menerapkan ekonomi harian (GDD §6, Fase 2). Perhitungan murninya ada
// di EconomyCalculator supaya bisa diuji di luar Unity.
//
// FASE 2: pemasukan warung dipengaruhi kebahagiaan hari sebelumnya,
// pajak kota 2,5% dari pemasukan kotor, dan upkeep yang tidak bisa
// dibayar menjadi TUNGGAKAN (bukan bangkrut langsung): koin jadi 0,
// kekurangan dicatat, dan kebahagiaan turun bila menunggak 2 hari
// berturut-turut (aturan di EconomyCalculator.HitungKebahagiaan).

using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    // Tunggakan upkeep yang belum terbayar (rupiah).
    public long Tunggakan { get; private set; }
    // Jumlah hari menunggak berturut-turut (0 = tidak menunggak).
    public int HariMenunggakBerturut { get; private set; }
    // FASE 3: pengali pemasukan warung dari cuaca (SistemKejadian) —
    // 100 normal, 80 hujan tanpa payung, 90 hujan dengan payung.
    // Diatur oleh GameManager sebelum TerapkanHari(), dipakai sekali
    // lalu kembali 100.
    public int PengaliHujanBesok = 100;
    // FASE 4B: sewa Kost hari ini hilang karena keringanan peristiwa
    // penghuni (GDD §9.1). Diatur GameManager sebelum TerapkanHari(),
    // dipakai sekali lalu kembali false.
    public bool SewaKostHilangHariIni = false;

    private BuildManager buildManager;
    private CitizenManager citizenManager;
    private InteriorManager interiorManager;

    private void Awake()
    {
        buildManager = FindObjectOfType<BuildManager>();
        citizenManager = FindObjectOfType<CitizenManager>();
        interiorManager = FindObjectOfType<InteriorManager>();
        Tunggakan = 0;
        HariMenunggakBerturut = 0;
    }

    // Dipanggil SaveManager/GameManager saat memuat simpanan Fase 2
    // (simpanan Fase 1 tidak punya field ini -> dipanggil dengan 0, 0).
    public void MuatKeadaan(long tunggakan, int hariMenunggakBerturut)
    {
        Tunggakan = tunggakan < 0 ? 0 : tunggakan;
        HariMenunggakBerturut = hariMenunggakBerturut < 0 ? 0 : hariMenunggakBerturut;
    }

    // Terapkan satu hari game: hitung pemasukan/pengeluaran dari semua
    // bangunan yang terpasang, bayar dari koin (atau catat tunggakan),
    // kembalikan ringkasannya untuk ditampilkan sebagai laporan hari.
    public EconomyCalculator.RingkasanHarian TerapkanHari()
    {
        // FASE 7 (tycoon): di mode online pemasukan & perawatan dihitung
        // server dari warga NPC yang menyewa dan berbelanja (koin server
        // diterapkan lewat sinkron). Ekonomi lokal tidak dijalankan
        // supaya uang tidak dihitung dua kali.
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            PengaliHujanBesok = 100;
            SewaKostHilangHariIni = false;
            return new EconomyCalculator.RingkasanHarian();
        }
        List<EconomyCalculator.CatatanBangunan> catatan = BuatCatatan();
        int kebahagiaan = citizenManager != null
            ? citizenManager.Kebahagiaan
            : GameBalance.KebahagiaanAwal;
        EconomyCalculator.RingkasanHarian ringkasan =
            EconomyCalculator.HitungHarian(catatan, kebahagiaan, PengaliHujanBesok);
        PengaliHujanBesok = 100;
        SewaKostHilangHariIni = false;

        // Bayar bersih dari koin; bila tidak cukup, menunggak.
        long koinLama = GameManager.Instance.Koin;
        EconomyCalculator.HasilPembayaran bayar =
            EconomyCalculator.HitungPembayaran(koinLama, ringkasan.Bersih, Tunggakan);
        long selisih = bayar.KoinBaru - koinLama;
        if (selisih != 0)
        {
            // Selisih selalu >= -koinLama, jadi UbahKoin pasti berhasil.
            GameManager.Instance.UbahKoin(selisih);
        }
        Tunggakan = bayar.TunggakanBaru;
        if (bayar.Menunggak)
        {
            HariMenunggakBerturut++;
        }
        else
        {
            HariMenunggakBerturut = 0;
        }
        ringkasan.Menunggak = bayar.Menunggak;
        ringkasan.Tunggakan = Tunggakan;
        ringkasan.HariMenunggakBerturut = HariMenunggakBerturut;

        Debug.Log("[Ekonomi] Hari selesai. Masuk " + FormatUtil.Rupiah(ringkasan.Pemasukan)
            + " (warung " + FormatUtil.Rupiah(ringkasan.PemasukanWarung)
            + ", lain " + FormatUtil.Rupiah(ringkasan.PemasukanLain)
            + "), pajak " + FormatUtil.Rupiah(ringkasan.Pajak)
            + ", perawatan " + FormatUtil.Rupiah(ringkasan.Upkeep)
            + ", bersih " + FormatUtil.Rupiah(ringkasan.Bersih)
            + (ringkasan.Menunggak
                ? ", MENUNGGAK " + FormatUtil.Rupiah(ringkasan.Tunggakan)
                    + " (hari ke-" + ringkasan.HariMenunggakBerturut + ")"
                : ""));
        return ringkasan;
    }

    // Ringkasan hari ini tanpa mengubah koin (untuk panel info).
    public EconomyCalculator.RingkasanHarian PerkiraanHarian()
    {
        int kebahagiaan = citizenManager != null
            ? citizenManager.Kebahagiaan
            : GameBalance.KebahagiaanAwal;
        return EconomyCalculator.HitungHarian(BuatCatatan(), kebahagiaan);
    }

    private List<EconomyCalculator.CatatanBangunan> BuatCatatan()
    {
        List<EconomyCalculator.CatatanBangunan> hasil =
            new List<EconomyCalculator.CatatanBangunan>();
        // FASE 4: aturan Jalan (GDD §5.5) — dihitung sekali per hari
        // dari posisi bangunan (BuildManager), seurutan DaftarTerpasang.
        bool[] tanpaJalan = buildManager != null
            ? buildManager.HitungTanpaJalan()
            : new bool[0];
        List<SistemRadiusEfek.InfoPosisiBangunan> infoPosisi = buildManager != null
            ? buildManager.BuatInfoPosisi(tanpaJalan)
            : new List<SistemRadiusEfek.InfoPosisiBangunan>();
        int i = 0;
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            // Komersial = pemasukan dipengaruhi kebahagiaan + hujan:
            // Warung, Toko Kelontong (Fase 3), Pasar, Kafe (Fase 4).
            bool komersial = b.Data.id == "warung"
                || b.Data.id == "toko_kelontong"
                || b.Data.id == "pasar"
                || b.Data.id == "kafe";
            // FASE 4B: sewa Kost hilang hari ini (keringanan peristiwa)
            // dan pengali sewa interior (Kost lengkap = +25%).
            long pemasukan = b.Data.pemasukanHarian;
            pemasukan = (pemasukan * SistemUpgradeBangunan.PengaliPemasukanPersen(b.Level)) / 100;
            if (komersial && SistemRadiusEfek.ApakahTerjangkauListrik(i, infoPosisi))
            {
                pemasukan = (pemasukan * 110) / 100;
            }
            if (b.Data.id == "kost" && SewaKostHilangHariIni)
            {
                pemasukan = 0;
            }
            int pengaliSewa = interiorManager != null
                ? interiorManager.PengaliSewa(b)
                : 100;
            hasil.Add(new EconomyCalculator.CatatanBangunan
            {
                Id = b.Data.id,
                PemasukanHarian = pemasukan,
                BiayaPerawatanHarian = b.Data.biayaPerawatanHarian,
                KapasitasWarga = b.Data.kapasitasWarga,
                AdalahTaman = b.Data.id == "taman",
                AdalahWarung = komersial,
                BonusBahagia = b.Data.bonusBahagia,
                TanpaJalan = i < tanpaJalan.Length && tanpaJalan[i],
                PengaliSewaPersen = pengaliSewa
            });
            i++;
        }
        return hasil;
    }
}
