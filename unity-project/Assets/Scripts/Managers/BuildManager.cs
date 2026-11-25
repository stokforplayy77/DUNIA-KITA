// Dunia Kita — BuildManager
// Menangani tap pemain dan penempatan bangunan (GDD §5 & §10).
//
// Alur tap (mouse di Editor / sentuhan di HP):
//   tap tanah kosong  -> menu bangun -> pilih bangunan -> terpasang
//   tap bangunan      -> panel detail (info + tombol jual)
//   drag / pinch      -> ditangani CameraController, BUKAN tap
//
// Tap dibedakan dari drag dengan ambang geser (12 piksel) dan durasi
// (0,5 detik). Tap di atas tombol UI diabaikan supaya tidak tembus
// ke dunia (cek EventSystem.IsPointerOverGameObject).

using System.Collections.Generic;
using UnityEngine;

// Satu bangunan yang sudah terpasang di dunia.
public class BangunanTerpasang
{
    public BuildingData Data;
    public Vector2Int PosisiGrid;
    public GameObject Objek;
    public int Level = 1;
}

public class BuildManager : MonoBehaviour
{
    [Header("Data (diisi dari Scene)")]
    public BuildingData[] daftarBangunan;   // Rumah, Warung, Taman (Fase 1)
    public GameObject prefabBangunan;       // Prefab generik "Bangunan"

    public List<BangunanTerpasang> DaftarTerpasang { get; private set; }
    public BangunanTerpasang BangunanSedangDipindah { get; private set; }

    private GridManager gridManager;
    private Camera kameraUtama;
    private UIManager uiManager;
    private SaveManager saveManager;
    private CitizenManager citizenManager;
    private InteriorManager interiorManager;

    // Status tap (untuk membedakan tap vs drag).
    private Vector2 posisiTekan;
    private float waktuTekan;
    private bool sedangTekan;
    private const float AmbangGeserPiksel = 24f;
    private const float AmbangDurasiDetik = 0.7f;

    private void Awake()
    {
        DaftarTerpasang = new List<BangunanTerpasang>();
        gridManager = FindObjectOfType<GridManager>();
        uiManager = FindObjectOfType<UIManager>();
        saveManager = FindObjectOfType<SaveManager>();
        citizenManager = FindObjectOfType<CitizenManager>();
        interiorManager = FindObjectOfType<InteriorManager>();
    }

    private void Start()
    {
        kameraUtama = Camera.main;
    }

    private void Update()
    {
        if (kameraUtama == null)
        {
            kameraUtama = Camera.main;
            if (kameraUtama == null) return;
        }
        if (Input.touchCount > 0)
        {
            TanganiSentuhan();
        }
        else
        {
            TanganiMouse();
        }
    }

    // ---- Input: mouse (Editor & PC) ----
    private void TanganiMouse()
    {
        if (Input.touchCount > 0)
        {
            return; // Perangkat sentuh: pakai jalur sentuhan saja.
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (ApakahDiAtasUI())
            {
                sedangTekan = false;
                return;
            }
            sedangTekan = true;
            posisiTekan = Input.mousePosition;
            waktuTekan = Time.time;
        }
        else if (Input.GetMouseButtonUp(0) && sedangTekan)
        {
            sedangTekan = false;
            if (AdalahTap(Input.mousePosition))
            {
                TapLayar(Input.mousePosition);
            }
        }
    }

    // ---- Input: sentuhan (HP Android / iOS) ----
    private void TanganiSentuhan()
    {
        if (Input.touchCount != 1)
        {
            sedangTekan = false;
            return;
        }
        Touch sentuhan = Input.GetTouch(0);
        if (sentuhan.phase == TouchPhase.Began)
        {
            if (ApakahDiAtasUI(sentuhan.fingerId))
            {
                sedangTekan = false;
                return;
            }
            sedangTekan = true;
            posisiTekan = sentuhan.position;
            waktuTekan = Time.time;
        }
        else if (sentuhan.phase == TouchPhase.Ended && sedangTekan)
        {
            sedangTekan = false;
            if (AdalahTap(sentuhan.position))
            {
                TapLayar(sentuhan.position);
            }
        }
    }

    private bool AdalahTap(Vector2 posisiLepas)
    {
        float geser = Vector2.Distance(posisiTekan, posisiLepas);
        float durasi = Time.time - waktuTekan;
        return geser <= AmbangGeserPiksel && durasi <= AmbangDurasiDetik;
    }

    private bool ApakahDiAtasUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private bool ApakahDiAtasUI(int fingerId)
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    // ---- Aksi tap ----
    private void TapLayar(Vector2 posisiLayar)
    {
        Vector3 posisiDunia;
        if (gridManager != null && gridManager.Grid != null && gridManager.Grid.mode3D)
        {
            Ray ray = kameraUtama.ScreenPointToRay(posisiLayar);
            Plane bidangTanah = new Plane(Vector3.up, Vector3.zero);
            float jarakHit;
            if (!bidangTanah.Raycast(ray, out jarakHit))
            {
                return;
            }
            posisiDunia = ray.GetPoint(jarakHit);
        }
        else
        {
            posisiDunia = kameraUtama.ScreenToWorldPoint(
                new Vector3(posisiLayar.x, posisiLayar.y, -kameraUtama.transform.position.z));
        }
        Vector2Int petak = gridManager.Grid.DuniaKeGrid(posisiDunia);
        if (!gridManager.Grid.DiDalamGrid(petak))
        {
            BatalPindah();
            uiManager.SembunyikanPanel();
            gridManager.SembunyikanSorotan();
            return;
        }
        if (BangunanSedangDipindah != null)
        {
            PindahkanBangunan(BangunanSedangDipindah, petak);
            return;
        }
        BangunanTerpasang bangunan = CariBangunanDi(petak);
        // FASE 7: mode online — kepemilikan diputuskan server.
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            TapLayarOnline(petak, bangunan);
            return;
        }
        if (bangunan != null)
        {
            gridManager.TampilkanSorotan(petak);
            BuildingView view = bangunan.Objek != null
                ? bangunan.Objek.GetComponent<BuildingView>() : null;
            if (view != null) view.MulaiAnimasiTap();
            int radius = SistemRadiusEfek.RadiusUntuk(bangunan.Data.id);
            if (radius > 0)
            {
                gridManager.TampilkanSorotanRadius(
                    SistemRadiusEfek.PetakDalamRadius(
                        bangunan.PosisiGrid.x, bangunan.PosisiGrid.y,
                        bangunan.Data.ukuran.x, bangunan.Data.ukuran.y,
                        radius, gridManager.UkuranGrid));
            }
            else
            {
                gridManager.SembunyikanSorotanRadius();
            }
            uiManager.TampilkanDetail(bangunan);
        }
        else
        {
            gridManager.TampilkanSorotan(petak);
            gridManager.SembunyikanSorotanRadius();
            uiManager.TampilkanMenuBangun(petak, daftarBangunan);
        }
    }

    // FASE 7 — tap dalam mode online: petak kota -> panel izin;
    // petak milik sendiri kosong -> menu bangun; petak/bangunan
    // milik orang lain -> info pemilik (tanpa tombol bangun/jual).
    private void TapLayarOnline(Vector2Int petak, BangunanTerpasang bangunan)
    {
        ServerManager server = ServerManager.Instance;
        PanelIzinTanah.SembunyikanJikaAda();
        if (bangunan != null)
        {
            gridManager.TampilkanSorotan(petak);
            BuildingView view = bangunan.Objek != null
                ? bangunan.Objek.GetComponent<BuildingView>() : null;
            if (view != null) view.MulaiAnimasiTap();
            string pemilik = server.PemilikPetak(petak.x, petak.y);
            if (server.ApakahMilikSaya(bangunan) || pemilik == "Kota")
            {
                uiManager.TampilkanDetail(bangunan);
            }
            else
            {
                PanelIzinTanah.TampilkanInfoPemilik(petak, pemilik,
                    bangunan.Data.nama);
            }
            return;
        }
        gridManager.TampilkanSorotan(petak);
        if (server.ApakahMilikSaya(petak.x, petak.y))
        {
            uiManager.TampilkanMenuBangun(petak, daftarBangunan);
        }
        else if (string.IsNullOrEmpty(server.PemilikPetak(petak.x, petak.y)))
        {
            PanelIzinTanah.TampilkanTanahKota(petak.x, petak.y);
        }
        else
        {
            PanelIzinTanah.TampilkanInfoPemilik(petak,
                server.PemilikPetak(petak.x, petak.y), "");
        }
    }

    // ---- Membangun ----
    public bool CobaBangun(BuildingData data, Vector2Int petak)
    {
        if (data == null)
        {
            return false;
        }
        // Balai Kota: hanya 1 per dunia (batas ditegakkan di sini
        // supaya tidak bisa dilewati lewat jalur selain menu bangun).
        if (data.id == SistemBalaiKota.IdBalaiKota
            && !SistemBalaiKota.BolehBangun(HitungBangunan("balaikota")))
        {
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan("Balai Kota sudah dibangun — hanya 1 per dunia.");
            return false;
        }
        if (!gridManager.ApakahKosong(petak, data.ukuran))
        {
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan("Petak sudah terisi atau di luar dunia.");
            return false;
        }
        // FASE 7: mode online — koin, izin & syarat jalan divalidasi
        // server; menu tetap terbuka sampai server mengonfirmasi.
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            return CobaBangunOnline(data, petak);
        }
        if (!GameManager.Instance.ApakahKoinCukup(data.biayaBangun))
        {
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan("Koin tidak cukup untuk membangun " + data.nama + ".");
            return false;
        }
        GameManager.Instance.UbahKoin(-data.biayaBangun);
        PasangBangunan(data, petak, 1, true);
        if (EfekDuniaManager.Instance != null)
        {
            Vector3 pos = gridManager.Grid.GridKeDuniaTengah(petak, data.ukuran);
            EfekDuniaManager.Instance.TampilkanTeksMelayang(
                pos + Vector3.up * 0.6f,
                "-" + FormatUtil.Rupiah(data.biayaBangun),
                new Color(1f, 0.85f, 0.35f, 1f));
        }
        citizenManager.HitungUlang();
        // FASE 4: penghitung misi harian (dibangun hari ini) + cek
        // pencapaian yang mungkin baru terpenuhi karena bangunan ini.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CatatBangunanDibangun(data.id);
            GameManager.Instance.PeriksaPencapaian();
        }
        uiManager.PerbaruiHUD();
        uiManager.TampilkanPesan(data.nama + " berhasil dibangun!");
        AudioManager.MainkanBangun();
        saveManager.Simpan();
        return true;
    }

    // FASE 7 — bangun dalam mode online: tapak harus milik sendiri
    // (cek lokal instan); koin + syarat jalan divalidasi server.
    private bool CobaBangunOnline(BuildingData data, Vector2Int petak)
    {
        if (!TapakMilikSendiri(petak, data.ukuran))
        {
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan(
                "Petak ini bukan tanah milikmu — ajukan izin dulu.");
            return false;
        }
        return ServerManager.Instance.MintaBangun(data, petak);
    }

    private bool TapakMilikSendiri(Vector2Int petak, Vector2Int ukuran)
    {
        ServerManager server = ServerManager.Instance;
        if (server == null)
        {
            return false;
        }
        for (int x = petak.x; x < petak.x + ukuran.x; x++)
        {
            for (int y = petak.y; y < petak.y + ukuran.y; y++)
            {
                if (!server.ApakahMilikSaya(x, y))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // FASE 7 — pasang bangunan dari state server (tanpa biaya, tanpa
    // simpan lokal; koin sudah dipotong server).
    public void BangunDariServer(BuildingData data, Vector2Int petak,
        int level = 1, bool efekBiaya = true)
    {
        if (data == null || !gridManager.ApakahKosong(petak, data.ukuran))
        {
            return;
        }
        PasangBangunan(data, petak, level, true);
        if (efekBiaya && EfekDuniaManager.Instance != null)
        {
            Vector3 pos = gridManager.Grid.GridKeDuniaTengah(petak, data.ukuran);
            EfekDuniaManager.Instance.TampilkanTeksMelayang(
                pos + Vector3.up * 0.6f,
                "-" + FormatUtil.Rupiah(data.biayaBangun),
                new Color(1f, 0.85f, 0.35f, 1f));
        }
        if (citizenManager != null)
        {
            citizenManager.HitungUlang();
        }
        if (uiManager != null)
        {
            uiManager.PerbaruiHUD();
        }
    }

    public void TerapkanUpgradeDariServer(BangunanTerpasang bangunan, int levelBaru)
    {
        if (bangunan == null || bangunan.Data == null) return;
        bangunan.Level = SistemUpgradeBangunan.NormalisasiLevel(levelBaru);
        BuildingView view = bangunan.Objek != null
            ? bangunan.Objek.GetComponent<BuildingView>() : null;
        if (view != null)
        {
            view.PerbaruiVisualLevel(bangunan.Level);
            view.MulaiAnimasiBangun();
        }
        if (EfekDuniaManager.Instance != null && bangunan.Objek != null)
        {
            EfekDuniaManager.Instance.TampilkanTeksMelayang(
                bangunan.Objek.transform.position + Vector3.up * 0.6f,
                "Level " + bangunan.Level + "!",
                new Color(0.4f, 0.95f, 0.55f, 1f));
        }
        if (citizenManager != null) citizenManager.HitungUlang();
        if (uiManager != null)
        {
            uiManager.PerbaruiHUD();
            uiManager.TampilkanPesan(bangunan.Data.nama + " naik ke Level " + bangunan.Level + "!");
        }
        AudioManager.MainkanNaikLevel();
    }

    public void TerapkanPindahDariServer(BangunanTerpasang bangunan, Vector2Int petakBaru)
    {
        if (bangunan == null || bangunan.Data == null) return;
        gridManager.TandaiKosong(bangunan.PosisiGrid, bangunan.Data.ukuran);
        bangunan.PosisiGrid = petakBaru;
        gridManager.TandaiTerisi(petakBaru, bangunan.Data.ukuran);
        BuildingView view = bangunan.Objek != null
            ? bangunan.Objek.GetComponent<BuildingView>() : null;
        if (view != null)
        {
            view.Atur(bangunan.Data, petakBaru, gridManager.Grid);
            view.PerbaruiVisualLevel(bangunan.Level);
            view.MulaiAnimasiBangun();
        }
        if (bangunan.Data.id == "jalan") PerbaruiSemuaKoneksiJalan();
        gridManager.SembunyikanSorotan();
        if (citizenManager != null) citizenManager.HitungUlang();
        if (uiManager != null)
        {
            uiManager.PerbaruiHUD();
            uiManager.TampilkanPesan(bangunan.Data.nama + " dipindahkan!");
        }
        AudioManager.MainkanBangun();
    }

    // FASE 7 — lepas bangunan karena dijual/dihapus di server (tanpa
    // pengembalian koin; koin sudah diurus server).
    public void LepasBangunan(BangunanTerpasang bangunan)
    {
        if (bangunan == null)
        {
            return;
        }
        if (interiorManager != null)
        {
            interiorManager.HapusInterior(bangunan);
        }
        gridManager.TandaiKosong(bangunan.PosisiGrid, bangunan.Data.ukuran);
        DaftarTerpasang.Remove(bangunan);
        Destroy(bangunan.Objek);
        if (bangunan.Data != null && bangunan.Data.id == "jalan")
        {
            PerbaruiSemuaKoneksiJalan();
        }
        if (citizenManager != null)
        {
            citizenManager.HitungUlang();
        }
        if (uiManager != null)
        {
            uiManager.PerbaruiHUD();
        }
    }

    // FASE 7 — bersihkan semua bangunan lokal (masuk mode online:
    // dunia sendiri ada di cadangan simpanan lokal).
    public void BersihkanSemua()
    {
        // Salin daftar dulu: LepasBangunan mengubah DaftarTerpasang.
        List<BangunanTerpasang> salinan =
            new List<BangunanTerpasang>(DaftarTerpasang);
        foreach (BangunanTerpasang b in salinan)
        {
            LepasBangunan(b);
        }
    }

    private void PasangBangunan(BuildingData data, Vector2Int petak,
        int level = 1, bool animasi = true)
    {
        GameObject objek = Instantiate(prefabBangunan);
        BuildingView tampilan = objek.GetComponent<BuildingView>();
        tampilan.Atur(data, petak, gridManager.Grid);
        int lv = SistemUpgradeBangunan.NormalisasiLevel(level);
        tampilan.PerbaruiVisualLevel(lv);
        if (animasi)
        {
            tampilan.MulaiAnimasiBangun();
        }
        gridManager.TandaiTerisi(petak, data.ukuran);
        DaftarTerpasang.Add(new BangunanTerpasang
        {
            Data = data,
            PosisiGrid = petak,
            Objek = objek,
            Level = lv
        });
        if (data.id == "jalan")
        {
            PerbaruiSemuaKoneksiJalan();
        }
    }

    public void PerbaruiSemuaKoneksiJalan()
    {
        if (gridManager == null || gridManager.Grid == null || gridManager.Grid.mode3D)
        {
            return;
        }
        HashSet<Vector2Int> posisiJalan = new HashSet<Vector2Int>();
        for (int i = 0; i < DaftarTerpasang.Count; i++)
        {
            BangunanTerpasang b = DaftarTerpasang[i];
            if (b != null && b.Data != null && b.Data.id == "jalan")
            {
                posisiJalan.Add(b.PosisiGrid);
            }
        }
        for (int i = 0; i < DaftarTerpasang.Count; i++)
        {
            BangunanTerpasang b = DaftarTerpasang[i];
            if (b == null || b.Data == null || b.Data.id != "jalan" || b.Objek == null)
            {
                continue;
            }
            BuildingView view = b.Objek.GetComponent<BuildingView>();
            if (view == null) continue;
            Vector2Int p = b.PosisiGrid;
            view.PerbaruiKoneksiJalan(
                posisiJalan.Contains(new Vector2Int(p.x + 1, p.y)),
                posisiJalan.Contains(new Vector2Int(p.x - 1, p.y)),
                posisiJalan.Contains(new Vector2Int(p.x, p.y + 1)),
                posisiJalan.Contains(new Vector2Int(p.x, p.y - 1)),
                gridManager.Grid);
        }
    }

    // Upgrade level bangunan (Lv 1 -> 2 -> 3).
    public bool UpgradeBangunan(BangunanTerpasang bangunan)
    {
        if (bangunan == null || bangunan.Data == null) return false;
        if (!SistemUpgradeBangunan.BisaUpgrade(bangunan.Data.id, bangunan.Level))
        {
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan("Bangunan ini sudah mencapai level maksimal.");
            return false;
        }
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            ServerManager.Instance.MintaUpgrade(bangunan);
            return true;
        }
        long biaya = SistemUpgradeBangunan.BiayaUpgrade(
            bangunan.Data.biayaBangun, bangunan.Level);
        if (!GameManager.Instance.ApakahKoinCukup(biaya))
        {
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan("Koin tidak cukup untuk upgrade ("
                + FormatUtil.Rupiah(biaya) + ").");
            return false;
        }
        GameManager.Instance.UbahKoin(-biaya);
        bangunan.Level++;
        BuildingView view = bangunan.Objek != null
            ? bangunan.Objek.GetComponent<BuildingView>() : null;
        if (view != null)
        {
            view.PerbaruiVisualLevel(bangunan.Level);
            view.MulaiAnimasiBangun();
        }
        if (EfekDuniaManager.Instance != null && bangunan.Objek != null)
        {
            EfekDuniaManager.Instance.TampilkanTeksMelayang(
                bangunan.Objek.transform.position + Vector3.up * 0.6f,
                "Level " + bangunan.Level + "!",
                new Color(0.4f, 0.95f, 0.55f, 1f));
        }
        citizenManager.HitungUlang();
        uiManager.PerbaruiHUD();
        uiManager.TampilkanPesan(bangunan.Data.nama + " naik ke Level " + bangunan.Level + "!");
        AudioManager.MainkanNaikLevel();
        saveManager.Simpan();
        return true;
    }

    public void MulaiPindahBangunan(BangunanTerpasang bangunan)
    {
        if (bangunan == null) return;
        BangunanSedangDipindah = bangunan;
        uiManager.SembunyikanPanel();
        uiManager.TampilkanPesan("Tap petak tujuan untuk memindahkan "
            + bangunan.Data.nama + ".");
    }

    public void BatalPindah()
    {
        BangunanSedangDipindah = null;
    }

    public bool PindahkanBangunan(BangunanTerpasang bangunan, Vector2Int petakBaru)
    {
        BangunanSedangDipindah = null;
        if (bangunan == null || bangunan.Data == null) return false;
        if (petakBaru == bangunan.PosisiGrid)
        {
            gridManager.SembunyikanSorotan();
            return false;
        }
        gridManager.TandaiKosong(bangunan.PosisiGrid, bangunan.Data.ukuran);
        bool tujuanKosong = gridManager.ApakahKosong(petakBaru, bangunan.Data.ukuran);
        if (!tujuanKosong)
        {
            gridManager.TandaiTerisi(bangunan.PosisiGrid, bangunan.Data.ukuran);
            AudioManager.MainkanTolak();
            uiManager.TampilkanPesan("Petak tujuan tidak kosong atau di luar dunia.");
            return false;
        }
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            gridManager.TandaiTerisi(bangunan.PosisiGrid, bangunan.Data.ukuran);
            ServerManager.Instance.MintaPindah(bangunan, petakBaru);
            return true;
        }
        bangunan.PosisiGrid = petakBaru;
        gridManager.TandaiTerisi(petakBaru, bangunan.Data.ukuran);
        BuildingView view = bangunan.Objek != null
            ? bangunan.Objek.GetComponent<BuildingView>() : null;
        if (view != null)
        {
            view.Atur(bangunan.Data, petakBaru, gridManager.Grid);
            view.PerbaruiVisualLevel(bangunan.Level);
            view.MulaiAnimasiBangun();
        }
        if (bangunan.Data.id == "jalan") PerbaruiSemuaKoneksiJalan();
        gridManager.SembunyikanSorotan();
        citizenManager.HitungUlang();
        uiManager.PerbaruiHUD();
        uiManager.TampilkanPesan(bangunan.Data.nama + " dipindahkan!");
        AudioManager.MainkanBangun();
        saveManager.Simpan();
        return true;
    }

    public List<SistemRadiusEfek.InfoPosisiBangunan> BuatInfoPosisi(bool[] tanpaJalan)
    {
        List<SistemRadiusEfek.InfoPosisiBangunan> hasil =
            new List<SistemRadiusEfek.InfoPosisiBangunan>();
        for (int i = 0; i < DaftarTerpasang.Count; i++)
        {
            BangunanTerpasang b = DaftarTerpasang[i];
            if (b == null || b.Data == null) continue;
            hasil.Add(new SistemRadiusEfek.InfoPosisiBangunan
            {
                Id = b.Data.id,
                X = b.PosisiGrid.x,
                Y = b.PosisiGrid.y,
                Lebar = b.Data.ukuran.x,
                Tinggi = b.Data.ukuran.y,
                TanpaJalan = tanpaJalan != null && i < tanpaJalan.Length && tanpaJalan[i]
            });
        }
        return hasil;
    }

    // ---- Menjual ----
    public void JualBangunan(BangunanTerpasang bangunan)
    {
        if (bangunan == null)
        {
            return;
        }
        // FASE 7: mode online — bangunan orang lain tidak bisa dijual;
        // bangunan sendiri dijual lewat server (koin dari server).
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            if (!ServerManager.Instance.ApakahMilikSaya(bangunan))
            {
                AudioManager.MainkanTolak();
                uiManager.TampilkanPesan(
                    "Bangunan ini milik pemain lain — tidak bisa dijual.");
                return;
            }
            ServerManager.Instance.MintaJual(bangunan);
            return;
        }
        // FASE 4B: bangunan yang dijual kehilangan interiornya
        // (dan keluar dari mode interior bila sedang dibuka).
        if (interiorManager != null)
        {
            interiorManager.HapusInterior(bangunan);
        }
        long hargaJual = EconomyCalculator.HitungHargaJual(
            bangunan.Data.biayaBangun, GameBalance.RasioJualKembali);
        GameManager.Instance.UbahKoin(hargaJual);
        gridManager.TandaiKosong(bangunan.PosisiGrid, bangunan.Data.ukuran);
        DaftarTerpasang.Remove(bangunan);
        Destroy(bangunan.Objek);
        if (bangunan.Data != null && bangunan.Data.id == "jalan")
        {
            PerbaruiSemuaKoneksiJalan();
        }
        citizenManager.HitungUlang();
        uiManager.PerbaruiHUD();
        uiManager.TampilkanPesan(bangunan.Data.nama + " dijual (" +
            FormatUtil.Rupiah(hargaJual) + ").");
        saveManager.Simpan();
    }

    // ---- Muat dari simpanan (tanpa biaya) ----
    public void MuatBangunan(List<DataBangunanTersimpan> simpanan)
    {
        if (simpanan == null)
        {
            return;
        }
        foreach (DataBangunanTersimpan s in simpanan)
        {
            BuildingData data = CariData(s.id);
            if (data == null)
            {
                Debug.LogWarning("[Build] Data bangunan tidak dikenal: " + s.id);
                continue;
            }
            Vector2Int petak = new Vector2Int(s.x, s.y);
            if (!gridManager.ApakahKosong(petak, data.ukuran))
            {
                Debug.LogWarning("[Build] Petak simpanan sudah terisi: " + s.id);
                continue;
            }
            PasangBangunan(data, petak,
                SistemUpgradeBangunan.NormalisasiLevel(s.level), false);
        }
    }

    // Cari bangunan yang menempati petak ini — termasuk bangunan
    // multi-petak (Fase 3: Kost 2x2): petak manapun di dalam area
    // bangunan dikenali, bukan hanya petak pojoknya.
    public BangunanTerpasang CariBangunanDi(Vector2Int petak)
    {
        foreach (BangunanTerpasang b in DaftarTerpasang)
        {
            if (petak.x >= b.PosisiGrid.x
                && petak.x < b.PosisiGrid.x + b.Data.ukuran.x
                && petak.y >= b.PosisiGrid.y
                && petak.y < b.PosisiGrid.y + b.Data.ukuran.y)
            {
                return b;
            }
        }
        return null;
    }

    public BuildingData CariData(string id)
    {
        foreach (BuildingData d in daftarBangunan)
        {
            if (d.id == id)
            {
                return d;
            }
        }
        return null;
    }

    // FASE 4 — aturan Jalan (GDD §5.5): satu bool per bangunan
    // terpasang (seurutan DaftarTerpasang): true = tidak ada Jalan
    // di sebelahnya. Logika murni di EconomyCalculator.
    public bool[] HitungTanpaJalan()
    {
        System.Collections.Generic.List<EconomyCalculator.PetakBangunan> daftar =
            new System.Collections.Generic.List<EconomyCalculator.PetakBangunan>();
        foreach (BangunanTerpasang b in DaftarTerpasang)
        {
            daftar.Add(new EconomyCalculator.PetakBangunan
            {
                Id = b.Data.id,
                X = b.PosisiGrid.x,
                Y = b.PosisiGrid.y,
                Lebar = b.Data.ukuran.x,
                Tinggi = b.Data.ukuran.y
            });
        }
        return EconomyCalculator.HitungTanpaJalan(daftar);
    }

    // FASE 4 — penghitung untuk pencapaian & misi harian.
    public int HitungBangunan(string id)
    {
        int jumlah = 0;
        foreach (BangunanTerpasang b in DaftarTerpasang)
        {
            if (b.Data != null && b.Data.id == id)
            {
                jumlah++;
            }
        }
        return jumlah;
    }

    // Jumlah bangunan komersial (Warung, Toko Kelontong, Pasar, Kafe)
    // — dipakai pencapaian "Juragan Pasar" & misi harian.
    public int HitungKomersial()
    {
        int jumlah = 0;
        foreach (BangunanTerpasang b in DaftarTerpasang)
        {
            if (b.Data == null)
            {
                continue;
            }
            string id = b.Data.id;
            if (id == "warung" || id == "toko_kelontong"
                || id == "pasar" || id == "kafe")
            {
                jumlah++;
            }
        }
        return jumlah;
    }

    public int HitungKategori(KategoriBangunan kategori)
    {
        int jumlah = 0;
        foreach (BangunanTerpasang b in DaftarTerpasang)
        {
            if (b.Data != null && b.Data.kategori == kategori)
            {
                jumlah++;
            }
        }
        return jumlah;
    }
}
