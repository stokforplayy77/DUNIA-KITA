// Dunia Kita — Managers/ServerManagerAksi
// Lanjutan ServerManager (Fase 7, partial class): alur masuk online
// (MasukOnlineDariProfil + impor tanah single player) dan aksi online
// (izin tanah, bayar izin, bangun, jual, kas kota). Dipisah dari
// ServerManager.cs supaya tiap berkas < 600 baris.

using System;
using System.Collections;
using System.Text;
using UnityEngine;

public partial class ServerManager : MonoBehaviour
{
    // ---- Izin tanah (PanelIzinTanah) ----
    public void AjukanIzin(int x, int y, string idBangunan)
    {
        StartCoroutine(AjukanIzinCoroutine(x, y, idBangunan));
    }

    private IEnumerator AjukanIzinCoroutine(int x, int y, string idBangunan)
    {
        SedangMemprosesAksi = true;
        string badan = "{\"x\":" + x + ",\"y\":" + y
            + ",\"bangunan\":\"" + HttpDuniaBersama.EscapeJson(idBangunan) + "\"}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/ajukan-izin", badan, delegate (string hasil)
        {
            json = hasil;
        });
        SedangMemprosesAksi = false;
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponKeputusan jawab = JsonUtility.FromJson<ResponKeputusan>(json);
        PanelIzinTanah.PastikanAda();
        if (PanelIzinTanah.Instance != null)
        {
            if (jawab != null && jawab.ok && jawab.keputusan != null)
            {
                PanelIzinTanah.Instance.TampilkanKeputusan(
                    x, y, idBangunan, jawab.izin_id, jawab.keputusan.hasil,
                    jawab.keputusan.alasan, jawab.keputusan.harga_tanah,
                    jawab.keputusan.biaya_izin, jawab.keputusan.total,
                    jawab.keputusan.syarat);
            }
            else
            {
                PanelIzinTanah.Instance.TampilkanPesan(
                    jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                        ? jawab.kesalahan : "Pengajuan izin gagal.");
            }
        }
    }

    public void BayarIzin(int izinId, int x, int y)
    {
        StartCoroutine(BayarIzinCoroutine(izinId, x, y));
    }

    private IEnumerator BayarIzinCoroutine(int izinId, int x, int y)
    {
        SedangMemprosesAksi = true;
        string badan = "{\"izin_id\":" + izinId + "}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/bayar-izin", badan, delegate (string hasil)
        {
            json = hasil;
        });
        SedangMemprosesAksi = false;
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponAksi jawab = JsonUtility.FromJson<ResponAksi>(json);
        if (jawab != null && jawab.ok)
        {
            // Tandai petak jadi milik sendiri + cerminkan koin server.
            pemilikPetak[x + "," + y] = NamaPemain;
            if (gm != null)
            {
                long delta = jawab.koin - gm.Koin;
                if (delta != 0)
                {
                    gm.UbahKoin(delta);
                }
            }
            if (PanelIzinTanah.Instance != null)
            {
                PanelIzinTanah.Instance.Sembunyikan();
            }
            if (ui != null)
            {
                ui.TampilkanPesan("Tanah (" + x + ", " + y + ") kini milikmu — "
                    + "silakan bangun!");
                ui.PerbaruiHUD();
            }
        }
        else
        {
            PanelIzinTanah.PastikanAda();
            if (PanelIzinTanah.Instance != null)
            {
                PanelIzinTanah.Instance.TampilkanPesan(
                    jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                        ? jawab.kesalahan : "Pembayaran izin gagal.");
            }
        }
        SinkronisasiSekarang();
    }

    // ---- Bangun / jual online (dipanggil BuildManager) ----
    // Mengembalikan false supaya menu bangun tetap terbuka sampai
    // server mengonfirmasi; panel ditutup + pesan tampil dari sini.
    public bool MintaBangun(BuildingData data, Vector2Int petak)
    {
        StartCoroutine(BangunCoroutine(data, petak));
        return false;
    }

    private IEnumerator BangunCoroutine(BuildingData data, Vector2Int petak)
    {
        SedangMemprosesAksi = true;
        string badan = "{\"x\":" + petak.x + ",\"y\":" + petak.y
            + ",\"bangunan\":\"" + HttpDuniaBersama.EscapeJson(data.id) + "\"}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/bangun", badan, delegate (string hasil)
        {
            json = hasil;
        });
        SedangMemprosesAksi = false;
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponAksi jawab = JsonUtility.FromJson<ResponAksi>(json);
        if (jawab != null && jawab.ok)
        {
            if (bm != null)
            {
                bm.BangunDariServer(data, petak);
            }
            pemilikBangunan[petak.x + "," + petak.y] = NamaPemain;
            if (gm != null)
            {
                long delta = jawab.koin - gm.Koin;
                if (delta != 0)
                {
                    gm.UbahKoin(delta);
                }
            }
            if (ui != null)
            {
                ui.SembunyikanPanel();
                ui.TampilkanPesan(data.nama + " berhasil dibangun!");
                ui.PerbaruiHUD();
            }
            AudioManager.MainkanBangun();
        }
        else if (ui != null)
        {
            ui.TampilkanPesan(jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                ? jawab.kesalahan : "Membangun gagal.");
        }
        SinkronisasiSekarang();
    }

    public void MintaJual(BangunanTerpasang bangunan)
    {
        StartCoroutine(JualCoroutine(bangunan));
    }

    private IEnumerator JualCoroutine(BangunanTerpasang bangunan)
    {
        SedangMemprosesAksi = true;
        Vector2Int petak = bangunan.PosisiGrid;
        string badan = "{\"x\":" + petak.x + ",\"y\":" + petak.y + "}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/jual", badan, delegate (string hasil)
        {
            json = hasil;
        });
        SedangMemprosesAksi = false;
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponAksi jawab = JsonUtility.FromJson<ResponAksi>(json);
        if (jawab != null && jawab.ok)
        {
            if (bm != null)
            {
                bm.LepasBangunan(bangunan);
            }
            pemilikBangunan.Remove(petak.x + "," + petak.y);
            if (gm != null)
            {
                long delta = jawab.koin - gm.Koin;
                if (delta != 0)
                {
                    gm.UbahKoin(delta);
                }
            }
            if (ui != null)
            {
                ui.SembunyikanPanel();
                ui.TampilkanPesan(bangunan.Data.nama + " dijual ("
                    + FormatUtil.Rupiah(jawab.harga_jual) + ").");
                ui.PerbaruiHUD();
            }
        }
        else if (ui != null)
        {
            ui.TampilkanPesan(jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                ? jawab.kesalahan : "Menjual gagal.");
        }
        SinkronisasiSekarang();
    }

    // ---- Kas kota ----
    public void MintaKasKota()
    {
        StartCoroutine(KasKotaCoroutine());
    }

    private IEnumerator KasKotaCoroutine()
    {
        string json = null;
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token, "/api/kas-kota", delegate (string hasil)
        {
            json = hasil;
        });
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponKas jawab = JsonUtility.FromJson<ResponKas>(json);
        PanelIzinTanah.PastikanAda();
        if (PanelIzinTanah.Instance != null)
        {
            if (jawab != null && jawab.ok)
            {
                string teksRiwayat = "Riwayat pemasukan:";
                if (jawab.riwayat != null)
                {
                    int tampil = 0;
                    foreach (RiwayatDto r in jawab.riwayat)
                    {
                        if (tampil >= 5)
                        {
                            break;
                        }
                        string tanda = r.jumlah >= 0 ? "+" : "";
                        teksRiwayat += "\n" + tanda + FormatUtil.Rupiah(r.jumlah)
                            + " — " + r.keterangan;
                        tampil++;
                    }
                    if (tampil == 0)
                    {
                        teksRiwayat += "\nBelum ada pemasukan.";
                    }
                }
                PanelIzinTanah.Instance.TampilkanKasKota(jawab.saldo, teksRiwayat);
            }
            else
            {
                PanelIzinTanah.Instance.TampilkanPesan(
                    "Kas kota tidak bisa dimuat.");
            }
        }
    }


    // Kirim daftar bangunan lokal ke server sebagai tanah milik
    // (impor sekali, tanpa bayar). Gagal = lanjut saja (dunia server
    // tetap dimuat; tanah lama harus diajukan izin lagi).
    private IEnumerator ImporTanahLokal()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"bangunan\":[");
        bool pertama = true;
        foreach (BangunanTerpasang b in bm.DaftarTerpasang)
        {
            if (b == null || b.Data == null)
            {
                continue;
            }
            if (!pertama)
            {
                sb.Append(",");
            }
            pertama = false;
            sb.Append("{\"x\":").Append(b.PosisiGrid.x)
                .Append(",\"y\":").Append(b.PosisiGrid.y)
                .Append(",\"id\":\"")
                .Append(HttpDuniaBersama.EscapeJson(b.Data.id))
                .Append("\"}");
        }
        sb.Append("]}");
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token,
            "/api/impor-tanah", sb.ToString(), delegate (string hasil)
            {
                if (hasil == null)
                {
                    return;
                }
                ResponImpor jawab = JsonUtility.FromJson<ResponImpor>(hasil);
                if (jawab != null && jawab.ok && jawab.diimpor > 0)
                {
                    Debug.Log("[Server] Impor tanah single player: "
                        + jawab.diimpor + " bangunan diakui server.");
                }
            });
    }

    private IEnumerator MasukOnlineDariProfil(PemainDto profil,
        long pemasukanOffline, int hariOffline)
    {
        // Tunggu satu frame supaya Start semua manager (termasuk
        // GameManager yang memuat bangunan lokal) sudah jalan sebelum
        // daftar bangunan lokal dibaca untuk impor.
        yield return null;
        // FASE 7 — impor tanah single player (sekali): bila server
        // belum mencatat tanah apa pun untuk pemain ini dan dunia
        // lokal punya bangunan, daftarkan tanahnya ke server TANPA
        // bayar (tanah single player diakui sebagai milik).
        // Interior perabot tidak ikut (hanya di save lokal).
        if (bm != null && (profil.tanah == null || profil.tanah.Length == 0)
            && bm.DaftarTerpasang != null && bm.DaftarTerpasang.Count > 0)
        {
            yield return ImporTanahLokal();
        }
        // Perluas grid ke 24x24 (GridManager.Perluas hanya membesar).
        if (grid != null && grid.UkuranGrid < GameBalance.UkuranGridDuniaBersama)
        {
            grid.Perluas(GameBalance.UkuranGridDuniaBersama);
        }
        // Bersihkan bangunan lokal (dunia sendiri ada di cadangan).
        if (bm != null)
        {
            bm.BersihkanSemua();
        }
        // Ambil dunia server + cerminkan koin/reputasi.
        string json = null;
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token, "/api/dunia", delegate (string hasil)
        {
            json = hasil;
        });
        if (json == null)
        {
            // Server hilang di tengah jalan: tetap offline, dunia
            // lokal sudah dicadangkan tapi bangunan sudah dibersihkan —
            // pulihkan dengan memuat ulang game (pesan jelas ke pemain).
            ModeOnline = false;
            PerbaruiLabelStatus();
            if (ui != null)
            {
                ui.TampilkanPesan("Server terputus saat memuat dunia. "
                    + "Muat ulang game untuk kembali ke dunia sendiri.");
            }
            yield break;
        }
        ResponDunia dunia = JsonUtility.FromJson<ResponDunia>(json);
        TerapkanDunia(dunia);
        CerminkanPemain(profil);
        CameraController camCtrl = FindObjectOfType<CameraController>();
        if (camCtrl != null)
        {
            camCtrl.PusatkanKe(Vector3.zero);
        }
        ModeOnline = true;
        sudahPeringatkanOffline = false;
        penghitungSinkron = 0f;
        PerbaruiLabelStatus();
        if (ui != null)
        {
            if (pemasukanOffline > 0)
            {
                ui.TampilkanPesan("Selamat datang kembali, " + NamaPemain
                    + "! Selama kamu offline: +" + FormatUtil.Rupiah(pemasukanOffline)
                    + " (" + hariOffline + " hari game).");
            }
            else
            {
                ui.TampilkanPesan("Masuk Dunia Bersama sebagai " + NamaPemain + ".");
            }
            ui.PerbaruiHUD();
        }
    }

    // ---- Sengketa tanah & Peristiwa Dunia Bersama ----
    public void MintaSerobotTanah(int x, int y)
    {
        StartCoroutine(KirimSerobotTanah(x, y));
    }

    private IEnumerator KirimSerobotTanah(int x, int y)
    {
        SedangMemprosesAksi = true;
        string badan = "{\"x\":" + x + ",\"y\":" + y + "}";
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token,
            "/api/serobot-tanah", badan, delegate (string hasil)
            {
                SedangMemprosesAksi = false;
                if (hasil == null)
                {
                    if (ui != null) ui.TampilkanPesan("Server tidak terjangkau.");
                    return;
                }
                ResponSerobot jawab = JsonUtility.FromJson<ResponSerobot>(hasil);
                if (jawab == null || !jawab.ok)
                {
                    AudioManager.MainkanTolak();
                    if (ui != null) ui.TampilkanPesan(jawab != null ? jawab.kesalahan : "Gagal.");
                    return;
                }
                pemilikPetak[x + "," + y] = NamaPemain;
                if (gm != null)
                {
                    gm.UbahKoin(jawab.koin - gm.Koin);
                    gm.UbahReputasi(jawab.reputasi - gm.Reputasi);
                }
                AudioManager.MainkanBangun();
                PanelIzinTanah.SembunyikanJikaAda();
                if (ui != null)
                {
                    ui.PerbaruiHUD();
                    ui.TampilkanPesan("Petak (" + x + "," + y + ") diklaim paksa! Hati-hati aduan Polisi/Wali Kota.");
                }
                StartCoroutine(Sinkronisasi());
            });
    }

    public void MintaDaftarPeristiwa()
    {
        StartCoroutine(AmbilDaftarPeristiwa());
    }

    private IEnumerator AmbilDaftarPeristiwa()
    {
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token,
            "/api/peristiwa", delegate (string hasil)
            {
                if (hasil == null)
                {
                    if (ui != null) ui.TampilkanPesan("Gagal mengambil laporan peristiwa.");
                    return;
                }
                ResponPeristiwa jawab = JsonUtility.FromJson<ResponPeristiwa>(hasil);
                if (jawab != null && jawab.ok)
                {
                    PanelPeristiwaMultiplayer.PerbaruiDaftar(jawab.peristiwa);
                }
            });
    }

    public void MintaTindakPeristiwa(int id, string tujuan)
    {
        StartCoroutine(KirimTindakPeristiwa(id, tujuan));
    }

    private IEnumerator KirimTindakPeristiwa(int id, string tujuan)
    {
        SedangMemprosesAksi = true;
        string badan = "{\"id\":" + id + ",\"tujuan\":\""
            + HttpDuniaBersama.EscapeJson(tujuan) + "\"}";
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token,
            "/api/tindak-peristiwa", badan, delegate (string hasil)
            {
                SedangMemprosesAksi = false;
                if (hasil == null)
                {
                    if (ui != null) ui.TampilkanPesan("Server tidak terjangkau.");
                    return;
                }
                ResponTindakPeristiwa jawab =
                    JsonUtility.FromJson<ResponTindakPeristiwa>(hasil);
                if (jawab == null || !jawab.ok)
                {
                    AudioManager.MainkanTolak();
                    if (ui != null) ui.TampilkanPesan(jawab != null ? jawab.kesalahan : "Gagal.");
                    return;
                }
                if (gm != null)
                {
                    gm.UbahKoin(jawab.koin - gm.Koin);
                    gm.UbahReputasi(jawab.reputasi - gm.Reputasi);
                }
                AudioManager.MainkanKlaim();
                if (ui != null)
                {
                    ui.PerbaruiHUD();
                    ui.TampilkanPesan(jawab.pesan);
                }
                StartCoroutine(AmbilDaftarPeristiwa());
                StartCoroutine(Sinkronisasi());
            });
    }

    // ---- Multi-Distrik ----
    public void MintaDaftarDistrik()
    {
        StartCoroutine(AmbilDaftarDistrik());
    }

    private IEnumerator AmbilDaftarDistrik()
    {
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token,
            "/api/distrik", delegate (string hasil)
            {
                if (hasil == null)
                {
                    if (ui != null) ui.TampilkanPesan("Gagal mengambil daftar distrik.");
                    return;
                }
                ResponDaftarDistrik jawab =
                    JsonUtility.FromJson<ResponDaftarDistrik>(hasil);
                if (jawab != null && jawab.ok)
                {
                    DistrikAktifId = jawab.distrik_aktif;
                    PanelDistrik.PerbaruiDaftar(jawab.distrik, jawab.distrik_aktif);
                }
            });
    }

    public void MintaPindahDistrik(int distrikId)
    {
        StartCoroutine(KirimPindahDistrik(distrikId));
    }

    private IEnumerator KirimPindahDistrik(int distrikId)
    {
        SedangMemprosesAksi = true;
        string badan = "{\"distrik_id\":" + distrikId + "}";
        string jsonPindah = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token,
            "/api/pindah-distrik", badan, delegate (string hasil)
            {
                jsonPindah = hasil;
            });
        if (jsonPindah == null)
        {
            SedangMemprosesAksi = false;
            if (ui != null) ui.TampilkanPesan("Server tidak terjangkau.");
            yield break;
        }
        ResponPindahDistrik jawab =
            JsonUtility.FromJson<ResponPindahDistrik>(jsonPindah);
        if (jawab == null || !jawab.ok)
        {
            SedangMemprosesAksi = false;
            AudioManager.MainkanTolak();
            if (ui != null) ui.TampilkanPesan(jawab != null ? jawab.kesalahan : "Gagal pindah distrik.");
            yield break;
        }
        DistrikAktifId = jawab.distrik_id;
        NamaDistrikAktif = jawab.nama;
        if (bm != null)
        {
            bm.BersihkanSemua();
        }
        pemilikPetak.Clear();
        pemilikBangunan.Clear();
        yield return Sinkronisasi();
        SedangMemprosesAksi = false;
        CameraController camCtrl = FindObjectOfType<CameraController>();
        if (camCtrl != null)
        {
            camCtrl.PusatkanKe(Vector3.zero);
        }
        PerbaruiLabelStatus();
        PanelDistrik.SembunyikanJikaAda();
        AudioManager.MainkanNaikLevel();
        if (ui != null)
        {
            ui.PerbaruiHUD();
            ui.TampilkanPesan("Berpindah ke " + jawab.nama + ".");
        }
    }

    // ---- Chat Distrik & Leaderboard ----
    public void MintaDaftarChat()
    {
        StartCoroutine(AmbilChat());
    }

    private IEnumerator AmbilChat()
    {
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token,
            "/api/chat", delegate (string hasil)
            {
                if (hasil == null) return;
                ResponChat jawab = JsonUtility.FromJson<ResponChat>(hasil);
                if (jawab != null && jawab.ok)
                {
                    PanelSosialMultiplayer.PerbaruiChat(jawab.pesan);
                }
            });
    }

    public void MintaKirimChat(string isi)
    {
        StartCoroutine(KirimChatCoroutine(isi));
    }

    private IEnumerator KirimChatCoroutine(string isi)
    {
        string badan = "{\"isi\":\"" + HttpDuniaBersama.EscapeJson(isi) + "\"}";
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token,
            "/api/chat", badan, delegate (string hasil)
            {
                if (hasil == null)
                {
                    if (ui != null) ui.TampilkanPesan("Gagal mengirim pesan.");
                    return;
                }
                ResponChat jawab = JsonUtility.FromJson<ResponChat>(hasil);
                if (jawab != null && jawab.ok)
                {
                    PanelSosialMultiplayer.PerbaruiChat(jawab.pesan);
                }
                else if (ui != null && jawab != null)
                {
                    ui.TampilkanPesan(jawab.kesalahan);
                }
            });
    }

    public void MintaLeaderboard()
    {
        StartCoroutine(AmbilLeaderboard());
    }

    private IEnumerator AmbilLeaderboard()
    {
        yield return HttpDuniaBersama.KirimGet(AlamatServer, token,
            "/api/leaderboard", delegate (string hasil)
            {
                if (hasil == null) return;
                ResponLeaderboard jawab = JsonUtility.FromJson<ResponLeaderboard>(hasil);
                if (jawab != null && jawab.ok)
                {
                    PanelSosialMultiplayer.PerbaruiLeaderboard(jawab.peringkat);
                }
            });
    }

    public void MintaUpgrade(BangunanTerpasang bangunan)
    {
        StartCoroutine(UpgradeCoroutine(bangunan));
    }

    private IEnumerator UpgradeCoroutine(BangunanTerpasang bangunan)
    {
        SedangMemprosesAksi = true;
        Vector2Int petak = bangunan.PosisiGrid;
        string badan = "{\"x\":" + petak.x + ",\"y\":" + petak.y + "}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/upgrade", badan, delegate (string hasil)
        {
            json = hasil;
        });
        SedangMemprosesAksi = false;
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponUpgrade jawab = JsonUtility.FromJson<ResponUpgrade>(json);
        if (jawab != null && jawab.ok)
        {
            if (gm != null)
            {
                long delta = jawab.koin - gm.Koin;
                if (delta != 0) gm.UbahKoin(delta);
            }
            if (bm != null) bm.TerapkanUpgradeDariServer(bangunan, jawab.level);
            if (ui != null) ui.SembunyikanPanel();
        }
        else if (ui != null)
        {
            ui.TampilkanPesan(jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                ? jawab.kesalahan : "Upgrade gagal.");
        }
        SinkronisasiSekarang();
    }

    public void MintaPindah(BangunanTerpasang bangunan, Vector2Int petakBaru)
    {
        StartCoroutine(PindahCoroutine(bangunan, petakBaru));
    }

    private IEnumerator PindahCoroutine(BangunanTerpasang bangunan, Vector2Int petakBaru)
    {
        SedangMemprosesAksi = true;
        Vector2Int petakLama = bangunan.PosisiGrid;
        string badan = "{\"x_lama\":" + petakLama.x + ",\"y_lama\":" + petakLama.y
            + ",\"x\":" + petakBaru.x + ",\"y\":" + petakBaru.y + "}";
        string json = null;
        yield return HttpDuniaBersama.KirimPost(AlamatServer, token, "/api/pindah", badan, delegate (string hasil)
        {
            json = hasil;
        });
        SedangMemprosesAksi = false;
        if (json == null)
        {
            TandaiOffline("Server tidak terjangkau — main sendiri dulu.");
            yield break;
        }
        ResponPindah jawab = JsonUtility.FromJson<ResponPindah>(json);
        if (jawab != null && jawab.ok)
        {
            pemilikBangunan.Remove(petakLama.x + "," + petakLama.y);
            pemilikBangunan[petakBaru.x + "," + petakBaru.y] = NamaPemain;
            if (bm != null) bm.TerapkanPindahDariServer(bangunan, petakBaru);
        }
        else if (ui != null)
        {
            ui.TampilkanPesan(jawab != null && !string.IsNullOrEmpty(jawab.kesalahan)
                ? jawab.kesalahan : "Memindahkan bangunan gagal.");
        }
        SinkronisasiSekarang();
    }
}
