// Dunia Kita — SaveManager
// Simpan/muat game ke JSON di file lokal (GDD §13).
// Lokasi: Application.persistentDataPath — aman di Android & iOS,
// tidak butuh izin penyimpanan khusus.

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private BuildManager buildManager;
    private EconomyManager economyManager;
    private CitizenManager citizenManager;
    private InteriorManager interiorManager;

    private void Awake()
    {
        buildManager = FindObjectOfType<BuildManager>();
        economyManager = FindObjectOfType<EconomyManager>();
        citizenManager = FindObjectOfType<CitizenManager>();
        interiorManager = FindObjectOfType<InteriorManager>();
    }

    private string JalurFile()
    {
        return Path.Combine(Application.persistentDataPath, GameBalance.NamaFileSimpan);
    }

    // Kumpulkan state dari semua manager lalu tulis sebagai JSON.
    public void Simpan()
    {
        // FASE 7: selama mode online, simpanan lokal TIDAK ditimpa —
        // berkas lokal adalah cadangan dunia sendiri (disimpan sekali
        // oleh ServerManager sebelum masuk online). Koin/bangunan
        // online hidup di server; menimpa berkas akan menghapus
        // dunia sendiri pemain.
        if (ServerManager.Instance != null && ServerManager.Instance.ModeOnline)
        {
            return;
        }
        GameManager gm = GameManager.Instance;
        if (gm == null || buildManager == null)
        {
            return;
        }
        SaveData data = new SaveData();
        data.koin = gm.Koin;
        data.permata = gm.Permata;
        data.hari = gm.Hari;
        data.levelDunia = gm.LevelDunia;
        data.populasi = citizenManager != null ? citizenManager.Populasi : 0;
        data.kebahagiaan = citizenManager != null ? citizenManager.Kebahagiaan : 0;
        data.tunggakan = economyManager != null ? economyManager.Tunggakan : 0;
        data.hariMenunggakBerturut = economyManager != null
            ? economyManager.HariMenunggakBerturut : 0;
        // FASE 3: keadaan kejadian acak untuk hari berikutnya.
        data.bonusBahagiaBesok = gm.KeadaanKejadian.BonusBahagiaBesok;
        data.pengaliHujanBesok = gm.KeadaanKejadian.PengaliHujanBesok;
        // FASE 4: ukuran grid, pencapaian yang sudah diklaim, dan
        // penghitung misi harian untuk hari yang sedang berjalan.
        data.ukuranGrid = gm.UkuranGridSaatIni;
        data.pencapaianDiklaim = gm.PencapaianDiklaim != null
            ? new List<string>(gm.PencapaianDiklaim)
            : new List<string>();
        data.hariMisiTerakhir = gm.Hari;
        data.dibangunHariIni = gm.DibangunHariIni;
        data.tamanHariIni = gm.TamanHariIni;
        // FASE 4B: reputasi, interior per bangunan, dan efek
        // "besok" peristiwa penghuni yang belum dipakai.
        data.reputasi = gm.Reputasi;
        data.interior = interiorManager != null
            ? interiorManager.SimpanInterior()
            : new List<DataInteriorTersimpan>();
        data.sewaKostHilangBesok = gm.KeadaanPeristiwa.SewaKostHilangBesok;
        data.kapasitasHilangBesok = gm.KeadaanPeristiwa.KapasitasHilangBesok;
        data.tambahanKoinBesok = gm.KeadaanPeristiwa.TambahanKoinBesok;
        data.bonusBahagiaPeristiwaBesok = gm.KeadaanPeristiwa.BonusBahagiaBesok;
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            data.bangunan.Add(new DataBangunanTersimpan
            {
                id = b.Data.id,
                x = b.PosisiGrid.x,
                y = b.PosisiGrid.y,
                level = b.Level
            });
        }
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(JalurFile(), json);
        Debug.Log("[Simpan] Game disimpan (" + data.bangunan.Count + " bangunan).");
    }

    // Muat simpanan bila ada; kembalikan null bila belum pernah simpan
    // atau file rusak (game mulai baru).
    public SaveData Muat()
    {
        string jalur = JalurFile();
        if (!File.Exists(jalur))
        {
            return null;
        }
        try
        {
            string json = File.ReadAllText(jalur);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            // Migrasi aman: versi "1.0" (Fase 1), "2.0" (Fase 2),
            // "3.0" (Fase 3) dan "4.0" (Fase 4) tetap dimuat —
            // field Fase 4B yang tidak ada di JSON lama terisi
            // default (reputasi 0 -> 50 saat muat, interior kosong,
            // efek peristiwa tidak ada).
            // Versi lain yang tidak dikenal -> mulai baru.
            if (data == null || !SaveData.VersiDidukung(data.versi))
            {
                Debug.LogWarning("[Simpan] Versi simpanan tidak cocok, mulai baru.");
                return null;
            }
            if (data.versi == GameBalance.VersiSimpanLama)
            {
                Debug.Log("[Simpan] Simpanan Fase 1 dimuat (migrasi ke Fase 4B).");
            }
            else if (data.versi == GameBalance.VersiSimpanFase2)
            {
                Debug.Log("[Simpan] Simpanan Fase 2 dimuat (migrasi ke Fase 4B).");
            }
            else if (data.versi == GameBalance.VersiSimpanFase3)
            {
                Debug.Log("[Simpan] Simpanan Fase 3 dimuat (migrasi ke Fase 4B).");
            }
            else if (data.versi == GameBalance.VersiSimpanFase4)
            {
                Debug.Log("[Simpan] Simpanan Fase 4 dimuat (migrasi ke Fase 4B).");
            }
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Simpan] Gagal membaca simpanan: " + e.Message);
            return null;
        }
    }
}
