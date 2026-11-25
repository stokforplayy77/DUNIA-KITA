// Dunia Kita — PejalanKaki
// Satu warga yang berjalan di dunia (GDD §7, Fase 2).
// Gerakan sederhana tanpa pathfinding: pilih petak kosong acak,
// berjalan lurus ke sana dengan kecepatan jalan kaki, berhenti
// sejenak (1-3 detik), lalu pilih petak baru. Saat berjalan,
// posisi naik-turun sedikit (ayunan langkah).
//
// Komponen ini menempel pada GameObject warga yang dibuat oleh
// CitizenManager; sprite & target awal diisi lewat Atur().
// Posisi warga TIDAK disimpan di simpanan game — saat memuat game,
// warga dibangkitkan ulang di petak acak (lihat CATATAN-VERIFIKASI.md).

using System;
using System.Collections.Generic;
using UnityEngine;

public class PejalanKaki : MonoBehaviour
{
    private IsometricGrid grid;
    private GridManager gridManager;
    private System.Random acak;
    private SpriteRenderer spriteRenderer;

    private BuildManager buildManager;
    private Vector2 target;
    private List<JalanPathfinder.TitikGrid> jalurAktif;
    private int indeksJalur;
    private Vector2Int petakSaatIni;
    private bool punyaTarget;
    private float sisaIstirahat;
    private float kecepatan;
    private float waktuJalan;
    // Frame jalan (kartun proper): sprite diam + pasangan frame A/B.
    private Sprite spriteDiam;
    private Sprite[] frameJalan;
    private int indeksFrame;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    // Dipanggil CitizenManager tepat setelah GameObject warga dibuat.
    // frameJalanWarga = pasangan [A, B] untuk varian warna warga ini
    // (null = tidak ada frame jalan, warga jalan dengan sprite diam).
    public void Atur(Sprite sprite, Sprite[] frameJalanWarga,
        Vector3 posisiAwal, IsometricGrid gridDunia,
        GridManager pengelolaGrid, System.Random acakBersama, float kecepatanJalan)
    {
        grid = gridDunia;
        gridManager = pengelolaGrid;
        buildManager = FindObjectOfType<BuildManager>();
        acak = acakBersama;
        kecepatan = kecepatanJalan;
        spriteDiam = sprite;
        frameJalan = frameJalanWarga;
        indeksFrame = 0;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 8;
        // Warga lebih kecil dari bangunan (sprite 256px @ PPU 100 = 2,56
        // unit — sama tinggi rumah 1 lantai). Skala 0,42 -> ±1 unit.
        transform.localScale = new Vector3(0.42f, 0.42f, 1f);
        transform.position = posisiAwal;
        if (grid != null && grid.mode3D)
        {
            spriteRenderer.enabled = false;
            BuatModelWarga3D();
        }
        petakSaatIni = grid != null ? grid.DuniaKeGrid(posisiAwal) : Vector2Int.zero;
        jalurAktif = new List<JalanPathfinder.TitikGrid>();
        indeksJalur = 0;
        waktuJalan = 0f;
        PilihTargetBaru();
    }

    private void BuatModelWarga3D()
    {
        if (transform.Find("Badan3D") != null) return;
        string[] warnaBaju = { "#E53E3E", "#3182CE", "#D69E2E", "#38A169", "#805AD5" };
        string hex = warnaBaju[acak != null ? acak.Next(0, warnaBaju.Length) : 0];
        GameObject badan = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        badan.name = "Badan3D";
        badan.transform.SetParent(transform, false);
        badan.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        badan.transform.localScale = new Vector3(0.24f, 0.22f, 0.24f);
        Renderer rb = badan.GetComponent<Renderer>();
        if (rb != null) rb.sharedMaterial = Model3DBangunan.Mat(hex);

        GameObject kepala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        kepala.name = "Kepala3D";
        kepala.transform.SetParent(transform, false);
        kepala.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        kepala.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        Renderer rk = kepala.GetComponent<Renderer>();
        if (rk != null) rk.sharedMaterial = Model3DBangunan.Mat("#FBD38D");
    }

    private void Update()
    {
        if (!punyaTarget || grid == null || gridManager == null)
        {
            return;
        }
        if (sisaIstirahat > 0f)
        {
            sisaIstirahat -= Time.deltaTime;
            // Istirahat: kembali ke sprite diam.
            if (spriteDiam != null && spriteRenderer.sprite != spriteDiam)
            {
                spriteRenderer.sprite = spriteDiam;
            }
            if (sisaIstirahat <= 0f)
            {
                PilihTargetBaru();
            }
            return;
        }
        bool mode3d = grid.mode3D;
        Vector2 posisi = mode3d
            ? new Vector2(transform.position.x, transform.position.z)
            : new Vector2(transform.position.x, transform.position.y);
        Vector2 selisih = target - posisi;
        float jarak = selisih.magnitude;
        float langkah = kecepatan * Time.deltaTime;
        Vector2 posisiBaru;
        if (jarak <= langkah)
        {
            posisiBaru = target;
            if (jalurAktif != null && indeksJalur + 1 < jalurAktif.Count)
            {
                indeksJalur++;
                JalanPathfinder.TitikGrid titik = jalurAktif[indeksJalur];
                petakSaatIni = new Vector2Int(titik.X, titik.Y);
                Vector3 dunia = grid.GridKeDunia(petakSaatIni);
                target = mode3d ? new Vector2(dunia.x, dunia.z) : new Vector2(dunia.x, dunia.y);
            }
            else
            {
                punyaTarget = false;
                sisaIstirahat = (float)(GameBalance.IstirahatWargaMinDetik
                    + acak.NextDouble() * (GameBalance.IstirahatWargaMaksDetik
                        - GameBalance.IstirahatWargaMinDetik));
            }
        }
        else
        {
            Vector2 arah = selisih * (1f / jarak);
            posisiBaru = posisi + arah * langkah;
            if (!mode3d && spriteRenderer != null && Mathf.Abs(arah.x) > 0.01f)
            {
                spriteRenderer.flipX = arah.x < 0f;
            }
            waktuJalan += Time.deltaTime;
            if (!mode3d && frameJalan != null && frameJalan.Length == 2)
            {
                int indeks = IndeksFrameJalan(waktuJalan);
                if (indeks != indeksFrame)
                {
                    indeksFrame = indeks;
                    spriteRenderer.sprite = frameJalan[indeks];
                }
            }
        }
        float ayunan = 0f;
        if (punyaTarget)
        {
            ayunan = Mathf.Sin(waktuJalan * 8f) * GameBalance.AmplitudoLangkahWarga;
        }
        transform.position = mode3d
            ? new Vector3(posisiBaru.x, ayunan, posisiBaru.y)
            : new Vector3(posisiBaru.x, posisiBaru.y + ayunan, 0f);
    }

    // Indeks frame jalan murni (0/1 bergantian tiap interval) —
    // dipakai Update dan diuji di luar Unity (test-balaikota.cs).
    // Epsilon 0,001 menutup galat float (0,3f tidak eksak: 0,9f/0,3f
    // = 2,9999998 sehingga tanpa epsilon frame di t=0,9 salah).
    public static int IndeksFrameJalan(float waktu)
    {
        return (int)(waktu / GameBalance.IntervalFrameJalanWarga + 0.001f) % 2;
    }

    private void PilihTargetBaru()
    {
        jalurAktif = null;
        indeksJalur = 0;

        // Jika ada jaringan Jalan, prioritaskan berjalan mengikuti Jalan.
        if (buildManager != null)
        {
            List<Vector2Int> daftarJalan = new List<Vector2Int>();
            HashSet<long> setJalan = new HashSet<long>();
            foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
            {
                if (b != null && b.Data != null && b.Data.id == "jalan")
                {
                    daftarJalan.Add(b.PosisiGrid);
                    setJalan.Add(JalanPathfinder.Kunci(b.PosisiGrid.x, b.PosisiGrid.y));
                }
            }
            if (daftarJalan.Count >= 2)
            {
                Vector2Int tujuan = daftarJalan[acak.Next(0, daftarJalan.Count)];
                long kunciSekarang = JalanPathfinder.Kunci(petakSaatIni.x, petakSaatIni.y);
                if (setJalan.Contains(kunciSekarang))
                {
                    List<JalanPathfinder.TitikGrid> jalur = JalanPathfinder.CariJalur(
                        grid.lebar, grid.tinggi, setJalan,
                        petakSaatIni.x, petakSaatIni.y, tujuan.x, tujuan.y);
                    if (jalur.Count > 0)
                    {
                        jalurAktif = jalur;
                        indeksJalur = 0;
                        petakSaatIni = new Vector2Int(jalur[0].X, jalur[0].Y);
                        Vector3 duniaJalur = grid.GridKeDunia(petakSaatIni);
                        target = grid.mode3D
                            ? new Vector2(duniaJalur.x, duniaJalur.z)
                            : new Vector2(duniaJalur.x, duniaJalur.y);
                        punyaTarget = true;
                        sisaIstirahat = 0f;
                        return;
                    }
                }
                // Belum berada di petak jalan: berjalan ke salah satu petak jalan.
                petakSaatIni = tujuan;
                Vector3 duniaJalan = grid.GridKeDunia(tujuan);
                target = grid.mode3D
                    ? new Vector2(duniaJalan.x, duniaJalan.z)
                    : new Vector2(duniaJalan.x, duniaJalan.y);
                punyaTarget = true;
                sisaIstirahat = 0f;
                return;
            }
        }

        Vector2Int petak = CariPetakKosongAcak();
        petakSaatIni = petak;
        Vector3 dunia = grid.GridKeDunia(petak);
        target = grid.mode3D ? new Vector2(dunia.x, dunia.z) : new Vector2(dunia.x, dunia.y);
        punyaTarget = true;
        sisaIstirahat = 0f;
    }

    // Petak kosong acak; bila setelah 20 kali coba tidak ketemu
    // (dunia hampir penuh bangunan), pakai petak acak mana saja.
    private Vector2Int CariPetakKosongAcak()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2Int petak = new Vector2Int(
                acak.Next(0, grid.lebar), acak.Next(0, grid.tinggi));
            if (gridManager.ApakahKosong(petak, Vector2Int.one))
            {
                return petak;
            }
        }
        return new Vector2Int(
            acak.Next(0, grid.lebar), acak.Next(0, grid.tinggi));
    }
}
