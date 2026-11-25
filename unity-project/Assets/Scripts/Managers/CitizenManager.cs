// Dunia Kita — CitizenManager
// Menghitung populasi & kebahagiaan, dan menampilkan warga yang
// berjalan di dunia (GDD §7, Fase 2).
//
// FASE 2:
// - Populasi = total kapasitas hunian yang dibangun. Tiap Rumah
//   menambah warga sampai kapasitasnya (Rumah Sederhana = 2 warga).
// - Warga MUNCUL sebagai sprite kecil yang berjalan antar petak
//   kosong (komponen PejalanKaki). Demi performa HP, warga yang
//   tampil di layar dibatasi (GameBalance.MaksWargaTampil = 12);
//   sisanya hanya dihitung sebagai angka populasi.
// - Kebahagiaan dihitung EconomyCalculator.HitungKebahagiaan:
//   mulai 50, +2/taman (maks +20), +5 bila semua warga punya rumah,
//   -3 bila warga melebihi kapasitas, -5 bila upkeep menunggak
//   2 hari berturut-turut.
// - Posisi warga TIDAK disimpan (dibangkitkan ulang acak saat muat).

using System.Collections.Generic;
using UnityEngine;

public class CitizenManager : MonoBehaviour
{
    [Header("Sprite warga (diisi dari Scene)")]
    public Sprite[] spriteWarga;   // 3 varian warna (warga1/2/3)
    // Frame jalan warga (diisi dari Scene): 6 sprite, urutan
    // [1a, 1b, 2a, 2b, 3a, 3b] — pasangan A/B per varian warna,
    // dipakai PejalanKaki saat berjalan (diam = spriteWarga).
    public Sprite[] spriteWargaJalan;

    public int Populasi { get; private set; }
    public int Kebahagiaan { get; private set; }
    // FASE 3: bonus kebahagiaan sementara dari festival desa
    // (SistemKejadian) — berlaku untuk satu hari (satu HitungUlang),
    // lalu kembali 0. Diatur oleh GameManager sebelum HitungUlang().
    public int BonusSementara = 0;
    // FASE 4B: kapasitas hunian yang hilang hari ini karena kamar
    // kosong peristiwa penghuni (GDD §9.1: usir / lapor membuat
    // kamar kosong 1 hari). Diatur GameManager tiap pergantian
    // hari (0 bila tidak ada peristiwa) dan dipakai di HitungUlang;
    // TIDAK direset di sini supaya bertahan sepanjang hari meski
    // HitungUlang dipanggil berkali-kali (mis. sesudah membangun).

    public int KapasitasHilangHariIni = 0;

    // FASE 7 (tycoon): ringkasan warga dari server — jumlah warga NPC
    // di distrik dan berapa yang menyewa kamar milik pemain.
    public int WargaServer { get; private set; }
    public int PenyewaServer { get; private set; }

    // Dipanggil ServerManager tiap sinkron (10 detik).
    public void AturDariServer(int jumlah, int penyewa)
    {
        WargaServer = jumlah;
        PenyewaServer = penyewa;
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            Populasi = penyewa;
            SesuaikanWargaTampil();
        }
    }

    private BuildManager buildManager;
    private EconomyManager economyManager;
    private GridManager gridManager;
    private IsometricGrid grid;
    private InteriorManager interiorManager;

    private List<PejalanKaki> pejalan;
    private GameObject wadahWarga;
    private System.Random acak;

    private void Awake()
    {
        buildManager = FindObjectOfType<BuildManager>();
        economyManager = FindObjectOfType<EconomyManager>();
        gridManager = FindObjectOfType<GridManager>();
        interiorManager = FindObjectOfType<InteriorManager>();
        if (gridManager != null)
        {
            grid = gridManager.Grid;
        }
        pejalan = new List<PejalanKaki>();
        acak = new System.Random(System.Environment.TickCount);
        Populasi = 0;
        Kebahagiaan = GameBalance.KebahagiaanAwal;
    }

    // Hitung ulang dari daftar bangunan yang terpasang.
    // Dipanggil setiap kali bangunan berubah atau hari berganti.
    public void HitungUlang()
    {
        List<EconomyCalculator.CatatanBangunan> catatan =
            new List<EconomyCalculator.CatatanBangunan>();
        int jumlahTaman = 0;
        int kapasitas = 0;
        // FASE 4: aturan Jalan (GDD §5.5) — seurutan DaftarTerpasang.
        bool[] tanpaJalan = buildManager != null
            ? buildManager.HitungTanpaJalan()
            : new bool[0];
        int i = 0;
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            int kapEfektif = SistemUpgradeBangunan.KapasitasEfektif(
                b.Data.kapasitasWarga, b.Level);
            catatan.Add(new EconomyCalculator.CatatanBangunan
            {
                Id = b.Data.id,
                KapasitasWarga = kapEfektif,
                AdalahTaman = b.Data.id == "taman",
                BonusBahagia = b.Data.bonusBahagia,
                TanpaJalan = i < tanpaJalan.Length && tanpaJalan[i]
            });
            if (b.Data.id == "taman")
            {
                jumlahTaman++;
            }
            kapasitas += kapEfektif;
            i++;
        }
        Populasi = EconomyCalculator.HitungPopulasi(catatan);
        int hariMenunggak = economyManager != null
            ? economyManager.HariMenunggakBerturut
            : 0;
        // FASE 4B: kamar kosong peristiwa penghuni mengurangi
        // kapasitas (dan populasi) hari ini.
        int kapasitasEfektif = kapasitas - KapasitasHilangHariIni;
        if (kapasitasEfektif < 0)
        {
            kapasitasEfektif = 0;
        }
        if (Populasi > kapasitasEfektif)
        {
            Populasi = kapasitasEfektif;
        }
        // FASE 7 (tycoon): di mode online populasi = warga NPC yang
        // benar-benar menyewa kamar milik pemain (dikirim server).
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            Populasi = PenyewaServer;
        }
        // FASE 4: bonus kebahagiaan fasilitas (perpustakaan, sekolah,
        // klinik, dsb. — dihitung murni dari catatan yang sama).
        int bonusFasilitas = EconomyCalculator.HitungBonusFasilitas(catatan);
        // FASE 4B: bonus kebahagiaan interior (perabot +5/-5 per
        // hunian kosong, dibatasi -20..+20) masuk ke perhitungan
        // kebahagiaan dunia yang sama (GDD §5.6 + §7).
        if (interiorManager != null)
        {
            bonusFasilitas += interiorManager.BonusInteriorTotal();
        }
        // Sinergi radius fasilitas kota (taman/sekolah/klinik di dekat hunian).
        if (buildManager != null)
        {
            bonusFasilitas += SistemRadiusEfek.HitungBonusKebahagiaanRadius(
                buildManager.BuatInfoPosisi(tanpaJalan));
        }
        Kebahagiaan = EconomyCalculator.HitungKebahagiaan(
            jumlahTaman, Populasi, kapasitasEfektif, hariMenunggak, BonusSementara,
            bonusFasilitas);
        BonusSementara = 0;
        SesuaikanWargaTampil();
    }

    // Jumlah warga yang tampil = populasi, dibatasi maks tampil.
    private void SesuaikanWargaTampil()
    {
        int target = Populasi;
        if (target > GameBalance.MaksWargaTampil)
        {
            target = GameBalance.MaksWargaTampil;
        }
        while (pejalan.Count < target)
        {
            TambahWarga();
        }
        while (pejalan.Count > target)
        {
            PejalanKaki terakhir = pejalan[pejalan.Count - 1];
            pejalan.RemoveAt(pejalan.Count - 1);
            if (terakhir != null)
            {
                Destroy(terakhir.gameObject);
            }
        }
    }

    private void TambahWarga()
    {
        if (wadahWarga == null)
        {
            wadahWarga = new GameObject("Warga");
            wadahWarga.transform.SetParent(transform, false);
        }
        GameObject objek = new GameObject("Warga_" + pejalan.Count);
        objek.transform.SetParent(wadahWarga.transform, false);
        PejalanKaki kaki = objek.AddComponent<PejalanKaki>();
        int varian = PilihVarianAcak();
        Sprite sprite = PilihSpriteVarian(varian);
        Vector3 posisiAwal = CariPosisiAwalAcak();
        kaki.Atur(sprite, PilihFrameJalan(varian), posisiAwal, grid,
            gridManager, acak, GameBalance.KecepatanJalanWarga);
        pejalan.Add(kaki);
    }

    private int PilihVarianAcak()
    {
        if (spriteWarga == null || spriteWarga.Length == 0)
        {
            return 0;
        }
        return acak.Next(0, spriteWarga.Length);
    }

    private Sprite PilihSpriteVarian(int varian)
    {
        if (spriteWarga == null || spriteWarga.Length == 0)
        {
            return null;
        }
        if (varian < 0 || varian >= spriteWarga.Length)
        {
            varian = 0;
        }
        return spriteWarga[varian];
    }

    // Pasangan frame jalan [A, B] untuk satu varian warna; null bila
    // spriteWargaJalan belum diisi di Scene (warga tetap jalan
    // dengan sprite diam — ayunan langkah saja).
    private Sprite[] PilihFrameJalan(int varian)
    {
        if (spriteWargaJalan == null || spriteWargaJalan.Length < 2 * varian + 2)
        {
            return null;
        }
        return new Sprite[]
        {
            spriteWargaJalan[2 * varian],
            spriteWargaJalan[2 * varian + 1]
        };
    }

    // Posisi awal: petak kosong acak (20 kali coba, lalu petak mana saja).
    private Vector3 CariPosisiAwalAcak()
    {
        if (grid == null || gridManager == null)
        {
            return Vector3.zero;
        }
        for (int i = 0; i < 20; i++)
        {
            Vector2Int petak = new Vector2Int(
                acak.Next(0, grid.lebar), acak.Next(0, grid.tinggi));
            if (gridManager.ApakahKosong(petak, Vector2Int.one))
            {
                return grid.GridKeDunia(petak);
            }
        }
        return grid.GridKeDunia(new Vector2Int(
            acak.Next(0, grid.lebar), acak.Next(0, grid.tinggi)));
    }
}
