// Dunia Kita — SaveData
// Model data simpanan game (GDD §13: JSON ke file lokal).
// Hanya memakai field publik + List<T> karena JsonUtility Unity
// tidak bisa menserialisasi property, Dictionary, atau field privat
// tanpa [SerializeField].
//
// FASE 3 (versi "3.0"): tambahan level dunia (field levelDunia
// sudah ada sejak Fase 1), bonus kebahagiaan besok dan pengali
// hujan besok dari kejadian acak.
// FASE 2 (versi "2.0"): tambahan populasi, kebahagiaan, tunggakan,
// dan hari menunggak berturut-turut. Populasi/kebahagiaan sebenarnya
// dihitung ulang dari bangunan saat memuat (jadi nilainya selalu
// konsisten); field ini disimpan sebagai catatan keadaan saat simpan.
// Simpanan Fase 1 (versi "1.0") tidak punya field baru ini —
// JsonUtility mengisinya dengan 0 saat dimuat (migrasi aman,
// lihat SaveManager.Muat dan VersiDidukung).

using System.Collections.Generic;

[System.Serializable]
public class DataBangunanTersimpan
{
    public string id;   // id BuildingData, contoh: "rumah"
    public int x;       // Kolom grid
    public int y;       // Baris grid
    public int level;   // Level bangunan (0 di simpanan lama = level 1)
}

[System.Serializable]
public class DataInteriorTersimpan
{
    public int x;       // Kolom grid bangunan
    public int y;       // Baris grid bangunan
    // Isi slot interior: 10 id perabot ("" = slot kosong),
    // seurutan SistemInterior.PerabotPerSlot.
    public List<string> perabot = new List<string>();
}

[System.Serializable]
public class SaveData
{
    public string versi = GameBalance.VersiSimpan;
    public long koin;
    public int permata;
    public int hari;
    public int levelDunia;
    public List<DataBangunanTersimpan> bangunan = new List<DataBangunanTersimpan>();

    // FASE 2 (baru di versi "2.0"; simpanan "1.0" terisi 0 = default aman).
    public int populasi;
    public int kebahagiaan;
    public long tunggakan;            // Upkeep yang belum terbayar (rupiah)
    public int hariMenunggakBerturut; // 0 = tidak menunggak

    // FASE 3 (baru di versi "3.0"; simpanan "2.0"/"1.0" terisi 0 =
    // default aman: tidak ada bonus kebahagiaan, hujan normal 100).
    public int bonusBahagiaBesok;
    public int pengaliHujanBesok;

    // FASE 4 (baru di versi "4.0"; simpanan lama terisi default aman):
    // ukuran grid saat ini (12 = awal; 16 = sesudah perluasan lahan
    // Level 5 — 0 di simpanan lama dinormalkan menjadi 12),
    // pencapaian yang sudah diklaim hadiahnya, dan penghitung misi
    // harian untuk hari yang sedang berjalan (hariMisiTerakhir =
    // nomor hari saat penghitung ini dicatat; bila hari sudah
    // berganti, penghitung diabaikan dan direset 0 saat muat).
    public int ukuranGrid;
    public List<string> pencapaianDiklaim = new List<string>();
    public int hariMisiTerakhir;
    public int dibangunHariIni;
    public int tamanHariIni;

    // FASE 4B (baru di versi "5.0"; simpanan lama terisi default aman):
    // reputasi pemain (simpanan lama: 0 -> dinormalkan jadi 50 saat
    // muat, karena 0 hanya berarti "tidak ada data"), interior per
    // bangunan (slot -> perabot), dan keadaan efek "besok" peristiwa
    // penghuni yang belum dipakai (simpanan lama: semua 0/false =
    // tidak ada efek tertunda).
    public int reputasi;
    public List<DataInteriorTersimpan> interior = new List<DataInteriorTersimpan>();
    public bool sewaKostHilangBesok;
    public int kapasitasHilangBesok;
    public long tambahanKoinBesok;
    public int bonusBahagiaPeristiwaBesok;

    // Ukuran grid efektif: simpanan lama (0) atau rusak (negatif)
    // berarti grid awal 12x12.
    public static int UkuranGridEfektif(int ukuranGridSimpanan)
    {
        return ukuranGridSimpanan <= 0
            ? GameBalance.UkuranGrid
            : ukuranGridSimpanan;
    }

    // Versi simpanan yang masih bisa dimuat (migrasi aman):
    // "6.0" (Fase 7, saat ini), "5.1" (Balai Kota), "5.0" (Fase 4B),
    // "4.0" (Fase 4), "3.0" (Fase 3), "2.0" (Fase 2) dan "1.0"
    // (Fase 1 — field baru default 0). Logika murni supaya bisa
    // diuji di luar Unity (test-fase2.cs, test-fase3.cs,
    // test-fase4.cs, test-fase4b.cs, test-balaikota.cs,
    // test-fase6.cs, test-fase7.cs).
    public static bool VersiDidukung(string versi)
    {
        return versi == GameBalance.VersiSimpan
            || versi == GameBalance.VersiSimpanFase5_1
            || versi == GameBalance.VersiSimpanFase4B
            || versi == GameBalance.VersiSimpanFase4
            || versi == GameBalance.VersiSimpanFase3
            || versi == GameBalance.VersiSimpanFase2
            || versi == GameBalance.VersiSimpanLama;
    }
}
