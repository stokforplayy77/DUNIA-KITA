// Dunia Kita — Managers/ServerManager
// Klien REST Dunia Bersama (Fase 7, GDD §17): akun, izin tanah lewat
// wali kota, dan sinkronisasi dunia bersama dengan server Python
// (~/workspace/dunia-kita/server/). Satu-satunya penghubung Unity
// dengan server — semua logika keputusan (harga, izin, kepemilikan)
// tetap di server; klien hanya menampilkan & mengirim aksi.
//
// ALUR:
//   - Daftar/Masuk (PanelAkun) -> token disimpan di PlayerPrefs
//     ("DuniaKita_Token"; kata sandi TIDAK PERNAH disimpan).
//   - Masuk online: simpanan lokal disimpan dulu sebagai cadangan
//     (SaveManager.Simpan), grid diperluas ke 24x24, bangunan lokal
//     dibersihkan, dunia server dimuat, koin + reputasi dicerminkan
//     dari server, pemasukan offline diterapkan server saat masuk.
//   - Sinkronisasi tiap 10 detik + sesudah tiap aksi: GET /api/dunia
//     (diff bangunan via AturanDuniaBersama) + POST /api/sinkron
//     (koin + reputasi sesi; server membatasi perubahan per sinkron
//     maks Rp6.000.000 — lihat server/db.py).
//   - Bila server tidak terjangkau: game JALAN NORMAL dalam mode
//     single player lokal (simpanan 5.1/6.0 tetap dipakai, tanpa
//     error, tanpa macet). Label status di HUD: "Online" /
//     "Offline — main sendiri dulu".
//
// KELUAR (PanelAkun): token dihapus, mode online mati. Dunia sendiri
// TIDAK dipulihkan di sesi yang sama — simpanan lokal tetap utuh
// (SaveManager tidak menimpa berkas lokal selama online), jadi
// dunia sendiri kembali setelah game dimuat ulang. Ini sengaja:
// GameManager tidak punya metode muat-ulang publik (600 baris,
// batas), dan memulihkan separuh jalan berisiko merusak simpanan.
//   - Koin sesi online TIDAK hilang saat keluar: sinkron terakhir
//     sudah mendorong koin lokal ke server (batas Rp6jt/sinkron).
//
// CATATAN JUJUR (lihat README Fase 7): sinkronisasi 10 detik, bukan
// real-time; obrolan / papan peringkat / wali kota 3D belum ada.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public partial class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }

    public const string KunciToken = "DuniaKita_Token";
    public const string KunciNamaPemain = "DuniaKita_NamaPemain";
    public const string KunciAlamatServer = "DuniaKita_Server";

    public bool ModeOnline { get; private set; }
    public string NamaPemain { get; private set; }
    public string AlamatServer { get; private set; }
    public int DistrikAktifId { get; private set; }
    public string NamaDistrikAktif { get; private set; }

    // True selama coroutine bangun/jual/bayar-izin berjalan — sinkron
    // koin ditahan supaya tidak menimpa potongan server (server
    // menolak bila klien mendorong koin sebelum respons aksi diproses).
    public bool SedangMemprosesAksi { get; private set; }

    private string token;
    private float penghitungSinkron;
    private bool sudahPeringatkanOffline;
    private Text labelStatus;
    private Dictionary<string, string> pemilikPetak;
    private Dictionary<string, string> pemilikBangunan;

    private GameManager gm;
    private BuildManager bm;
    private GridManager grid;
    private UIManager ui;
    private SaveManager sm;

    private void Awake()
    {
        Instance = this;
        pemilikPetak = new Dictionary<string, string>();
        pemilikBangunan = new Dictionary<string, string>();
        token = PlayerPrefs.GetString(KunciToken, "");
        NamaPemain = PlayerPrefs.GetString(KunciNamaPemain, "");
        AlamatServer = PlayerPrefs.GetString(KunciAlamatServer,
            GameBalance.AlamatServerDefault);
        gm = FindObjectOfType<GameManager>();
        bm = FindObjectOfType<BuildManager>();
        grid = FindObjectOfType<GridManager>();
        ui = FindObjectOfType<UIManager>();
        sm = FindObjectOfType<SaveManager>();
    }

    private void Start()
    {
        BuatLabelStatus();
        PerbaruiLabelStatus();
        // Token tersimpan dari sesi sebelumnya -> coba sambung lagi
        // diam-diam; gagal = tetap offline, game jalan seperti biasa.
        if (!string.IsNullOrEmpty(token))
        {
            StartCoroutine(CekTokenLaluMasuk());
        }
    }

    private void Update()
    {
        if (!ModeOnline)
        {
            return;
        }
        penghitungSinkron += Time.deltaTime;
        if (penghitungSinkron >= GameBalance.IntervalSinkronisasiDetik)
        {
            penghitungSinkron = 0f;
            StartCoroutine(Sinkronisasi());
        }
    }

    // ---- Kepemilikan (cache dari /api/dunia terakhir) ----
    // "" = tanah kota (belum dimiliki siapa pun).
    public string PemilikPetak(int x, int y)
    {
        string pemilik;
        if (pemilikPetak.TryGetValue(x + "," + y, out pemilik))
        {
            return pemilik;
        }
        return "";
    }

    public bool ApakahMilikSaya(int x, int y)
    {
        return !string.IsNullOrEmpty(NamaPemain)
            && PemilikPetak(x, y) == NamaPemain;
    }

    public bool ApakahMilikSaya(BangunanTerpasang bangunan)
    {
        if (bangunan == null)
        {
            return false;
        }
        string kunci = bangunan.PosisiGrid.x + "," + bangunan.PosisiGrid.y;
        string pemilik;
        if (pemilikBangunan.TryGetValue(kunci, out pemilik))
        {
            return pemilik == NamaPemain;
        }
        // Cache belum ada (belum pernah sinkron): anggap milik sendiri
        // hanya bila sedang offline — saat online cache selalu diisi
        // dulu oleh MasukOnline sebelum bangunan dimuat.
        return !ModeOnline;
    }

    // ---- Validasi nama + kata sandi (dipakai PanelAkun) ----
    public static bool NamaValid(string nama, out string alasan)
    {
        alasan = "";
        if (string.IsNullOrEmpty(nama) || nama.Trim().Length < 3)
        {
            alasan = "Nama pemain minimal 3 huruf.";
            return false;
        }
        if (nama.Trim().Length > 20)
        {
            alasan = "Nama pemain maksimal 20 huruf.";
            return false;
        }
        foreach (char c in nama.Trim())
        {
            bool boleh = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                || (c >= '0' && c <= '9') || c == ' ' || c == '_' || c == '-';
            if (!boleh)
            {
                alasan = "Nama hanya boleh huruf, angka, spasi, _ dan -.";
                return false;
            }
        }
        return true;
    }

    public static bool SandiValid(string sandi, out string alasan)
    {
        alasan = "";
        if (string.IsNullOrEmpty(sandi) || sandi.Length < 4)
        {
            alasan = "Kata sandi minimal 4 karakter.";
            return false;
        }
        if (sandi.Length > 64)
        {
            alasan = "Kata sandi maksimal 64 karakter.";
            return false;
        }
        return true;
    }

    // ---- Akun ----
    public void Daftar(string nama, string sandi, string alamat,
        Action<bool, string> selesai)
    {
        AlamatServer = HttpDuniaBersama.NormalisasiAlamat(alamat);
        StartCoroutine(AkunCoroutine("/api/daftar", nama, sandi, selesai));
    }

    public void Masuk(string nama, string sandi, string alamat,
        Action<bool, string> selesai)
    {
        AlamatServer = HttpDuniaBersama.NormalisasiAlamat(alamat);
        StartCoroutine(AkunCoroutine("/api/masuk", nama, sandi, selesai));
    }

    private IEnumerator AkunCoroutine(string jalur, string nama, string sandi,
        Action<bool, string> selesai)
    {
        string badan = "{\"nama\":\"" + HttpDuniaBersama.EscapeJson(nama.Trim())
            + "\",\"sandi\":\"" + HttpDuniaBersama.EscapeJson(sandi) + "\"}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, jalur, badan, delegate (string hasil)
        {
            json = hasil;
        });
        if (json == null)
        {
            PerbaruiLabelStatus();
            if (selesai != null)
            {
                selesai(false, "Server tidak terjangkau — periksa alamat server.");
            }
            yield break;
        }
        ResponMasuk jawab = JsonUtility.FromJson<ResponMasuk>(json);
        if (jawab == null || !jawab.ok || string.IsNullOrEmpty(jawab.token))
        {
            if (selesai != null)
            {
                selesai(false, jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                    ? jawab.kesalahan : "Masuk gagal — coba lagi.");
            }
            yield break;
        }
        token = jawab.token;
        NamaPemain = jawab.nama;
        PlayerPrefs.SetString(KunciToken, token);
        PlayerPrefs.SetString(KunciNamaPemain, NamaPemain);
        PlayerPrefs.SetString(KunciAlamatServer, AlamatServer);
        PlayerPrefs.Save();
        yield return MasukOnline(jawab);
        if (selesai != null)
        {
            selesai(true, "");
        }
    }

    // Cek token lama: GET /api/pemain; bila sah, masuk online dengan
    // profil yang dikembalikan (tanpa pemasukan offline — itu hanya
    // dihitung server saat POST /api/masuk).
    private IEnumerator CekTokenLaluMasuk()
    {
        string json = null;
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token, "/api/pemain", delegate (string hasil)
        {
            json = hasil;
        });
        if (json == null)
        {
            PerbaruiLabelStatus();
            yield break;
        }
        ResponPemain jawab = JsonUtility.FromJson<ResponPemain>(json);
        if (jawab == null || !jawab.ok || jawab.pemain == null)
        {
            // Token kedaluwarsa / tidak sah -> hapus, tetap offline.
            token = "";
            PlayerPrefs.DeleteKey(KunciToken);
            PlayerPrefs.Save();
            PerbaruiLabelStatus();
            yield break;
        }
        NamaPemain = jawab.pemain.nama;
        yield return MasukOnlineDariProfil(jawab.pemain, 0, 0);
    }

    // Keluar dari mode online. Dunia sendiri kembali setelah game
    // dimuat ulang (simpanan lokal utuh — lihat kepala berkas).
    public void Keluar()
    {
        token = "";
        NamaPemain = "";
        ModeOnline = false;
        pemilikPetak.Clear();
        pemilikBangunan.Clear();
        PlayerPrefs.DeleteKey(KunciToken);
        PlayerPrefs.DeleteKey(KunciNamaPemain);
        PlayerPrefs.Save();
        PerbaruiLabelStatus();
        if (ui != null)
        {
            ui.TampilkanPesan("Keluar dari Dunia Bersama. "
                + "Muat ulang game untuk kembali ke dunia sendiri.");
        }
    }

    // ---- Masuk online: cadangan lokal -> perluas grid -> muat dunia ----
    private IEnumerator MasukOnline(ResponMasuk jawab)
    {
        // Cadangan: simpan keadaan lokal SEKARANG ke berkas, sebelum
        // mode online mengambil alih (SaveManager tidak menimpa
        // berkas lokal selama ModeOnline).
        if (sm != null)
        {
            sm.Simpan();
        }
        yield return MasukOnlineDariProfil(
            ProfilDariMasuk(jawab), jawab.pemasukan_offline, jawab.hari_offline);
    }

    private static PemainDto ProfilDariMasuk(ResponMasuk jawab)
    {
        return new PemainDto
        {
            id = jawab.id,
            nama = jawab.nama,
            koin = jawab.koin,
            permata = jawab.permata,
            reputasi = jawab.reputasi,
            level = jawab.level,
            tunggakan = jawab.tunggakan,
            tanah = jawab.tanah
        };
    }

    // Cerminkan koin + reputasi server ke game lokal (delta).
    private void CerminkanPemain(PemainDto profil)
    {
        if (profil == null || gm == null)
        {
            return;
        }
        long deltaKoin = profil.koin - gm.Koin;
        if (deltaKoin != 0)
        {
            gm.UbahKoin(deltaKoin);
        }
        int deltaRep = profil.reputasi - gm.Reputasi;
        if (deltaRep != 0)
        {
            gm.UbahReputasi(deltaRep);
        }
    }

    // ---- Sinkronisasi berkala ----
    private IEnumerator Sinkronisasi()
    {
        string json = null;
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token, "/api/dunia", delegate (string hasil)
        {
            json = hasil;
        });
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponDunia dunia = JsonUtility.FromJson<ResponDunia>(json);
        TerapkanDunia(dunia);
        // Dorong koin + reputasi sesi ke server (batas Rp6jt/sinkron
        // di server; ditahan selama aksi bangun/jual/bayar berjalan).
        if (!SedangMemprosesAksi && gm != null)
        {
            string badan = "{\"koin\":" + gm.Koin
                + ",\"reputasi\":" + gm.Reputasi + "}";
            yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/sinkron", badan, delegate (string hasil)
            {
                // Respons sinkron tidak perlu diproses — server sudah
                // menyimpan; bila ditolak (batas), koin lokal tetap
                // dipakai dan sinkron berikutnya mencoba lagi.
            });
        }
    }

    public void SinkronisasiSekarang()
    {
        if (ModeOnline)
        {
            penghitungSinkron = 0f;
            StartCoroutine(Sinkronisasi());
        }
    }

    private void TandaiOffline(string pesan)
    {
        ModeOnline = false;
        PerbaruiLabelStatus();
        if (!sudahPeringatkanOffline)
        {
            sudahPeringatkanOffline = true;
            if (ui != null)
            {
                ui.TampilkanPesan(pesan);
            }
        }
    }

    // Terapkan state dunia server: cache pemilik + diff bangunan.
    private void TerapkanDunia(ResponDunia dunia)
    {
        if (dunia == null || bm == null)
        {
            return;
        }
        if (dunia.distrik_id > 0)
        {
            DistrikAktifId = dunia.distrik_id;
            NamaDistrikAktif = dunia.nama_distrik;
            PerbaruiLabelStatus();
        }
        pemilikPetak.Clear();
        if (dunia.tanah != null)
        {
            foreach (PetakDto p in dunia.tanah)
            {
                pemilikPetak[p.x + "," + p.y] = p.pemilik ?? "";
            }
        }
        List<AturanDuniaBersama.BangunanBersama> lokal =
            new List<AturanDuniaBersama.BangunanBersama>();
        foreach (BangunanTerpasang b in bm.DaftarTerpasang)
        {
            // Pemilik dari cache sinkron sebelumnya (bangunan pemain
            // lain yang sudah dimuat) — JANGAN pakai NamaPemain untuk
            // semuanya, kalau tidak diff mengira pemiliknya berubah
            // tiap sinkron dan bangunan dilepas-dipasang terus.
            string kunci = b.PosisiGrid.x + "," + b.PosisiGrid.y;
            string pemilik;
            if (!pemilikBangunan.TryGetValue(kunci, out pemilik))
            {
                pemilik = NamaPemain;
            }
            lokal.Add(new AturanDuniaBersama.BangunanBersama
            {
                X = b.PosisiGrid.x,
                Y = b.PosisiGrid.y,
                Id = b.Data.id,
                Pemilik = pemilik
            });
        }
        List<AturanDuniaBersama.BangunanBersama> server =
            new List<AturanDuniaBersama.BangunanBersama>();
        Dictionary<string, int> levelServer = new Dictionary<string, int>();
        if (dunia.bangunan != null)
        {
            foreach (BangunanDto b in dunia.bangunan)
            {
                server.Add(new AturanDuniaBersama.BangunanBersama
                {
                    X = b.x,
                    Y = b.y,
                    Id = b.id,
                    Pemilik = b.pemilik
                });
                levelServer[b.x + "," + b.y] = b.level > 0 ? b.level : 1;
            }
        }
        AturanDuniaBersama.HasilDiff diff =
            AturanDuniaBersama.DiffDunia(lokal, server);
        foreach (AturanDuniaBersama.BangunanBersama lepas in diff.Lepas)
        {
            BangunanTerpasang b = bm.CariBangunanDi(
                new Vector2Int(lepas.X, lepas.Y));
            if (b != null)
            {
                bm.LepasBangunan(b);
            }
            // Hapus cache pemilik supaya bangunan baru di petak yang
            // sama tidak mewarisi pemilik lama.
            pemilikBangunan.Remove(lepas.X + "," + lepas.Y);
        }
        foreach (AturanDuniaBersama.BangunanBersama tambah in diff.Tambah)
        {
            BuildingData data = bm.CariData(tambah.Id);
            if (data == null)
            {
                continue;
            }
            int lv;
            if (!levelServer.TryGetValue(tambah.X + "," + tambah.Y, out lv)) lv = 1;
            bm.BangunDariServer(data, new Vector2Int(tambah.X, tambah.Y), lv, false);
            pemilikBangunan[tambah.X + "," + tambah.Y] = tambah.Pemilik;
        }
        // Perbarui cache pemilik & sinkronkan level bangunan dari state server.
        foreach (AturanDuniaBersama.BangunanBersama s in server)
        {
            pemilikBangunan[s.X + "," + s.Y] = s.Pemilik;
            int lv;
            if (levelServer.TryGetValue(s.X + "," + s.Y, out lv))
            {
                BangunanTerpasang ada = bm.CariBangunanDi(new Vector2Int(s.X, s.Y));
                if (ada != null && ada.Level != lv)
                {
                    ada.Level = SistemUpgradeBangunan.NormalisasiLevel(lv);
                    BuildingView view = ada.Objek != null
                        ? ada.Objek.GetComponent<BuildingView>() : null;
                    if (view != null) view.PerbaruiVisualLevel(ada.Level);
                }
            }
        }
    }

    // ---- Label status HUD ----
    private void BuatLabelStatus()
    {
        Canvas canvas = UiFactory.CariCanvasUI();
        if (canvas == null)
        {
            return;
        }
        RectTransform akar = canvas.GetComponent<RectTransform>();
        // Kanan atas, di bawah bar atas (bar atas ~110 unit):
        // kiri 1440, atas 120, lebar 440, tinggi 50.
        labelStatus = UiFactory.BuatTeks(akar, "LabelStatusOnline",
            "Offline — main sendiri dulu", 28, "#5A5A5A", TextAnchor.MiddleRight);
        UiFactory.AturPosisi(labelStatus.GetComponent<RectTransform>(),
            1440, 120, 440, 50);
    }

    private void PerbaruiLabelStatus()
    {
        if (labelStatus == null)
        {
            return;
        }
        if (ModeOnline)
        {
            string distrikInfo = string.IsNullOrEmpty(NamaDistrikAktif)
                ? "" : " • " + NamaDistrikAktif;
            labelStatus.text = "Online • " + NamaPemain + distrikInfo;
            labelStatus.color = UiFactory.Warna("#2E7D32");
        }
        else if (!string.IsNullOrEmpty(token))
        {
            labelStatus.text = "Menghubungkan…";
            labelStatus.color = UiFactory.Warna("#8A6D00");
        }
        else
        {
            labelStatus.text = "Offline — main sendiri dulu";
            labelStatus.color = UiFactory.Warna("#5A5A5A");
        }
    }

}