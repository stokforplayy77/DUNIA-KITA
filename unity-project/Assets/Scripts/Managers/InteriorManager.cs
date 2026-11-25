// Dunia Kita — InteriorManager
// Mengelola interior hunian (GDD §5.6, Fase 4B): data perabot per
// bangunan, mode tembus pandang (x-ray), efek ke kebahagiaan &
// sewa Kost, dan simpan/muat interior.
//
// Cara kerja mode interior: BuildingView menukar sprite luar
// dengan sprite denah (4 ruangan, tanpa dinding/atap) DI TEMPAT
// yang sama di grid; perabot terpasang dan 1-2 warga penghuni
// ditambahkan sebagai anak (child) di atas denah. Hanya satu
// bangunan dalam mode interior dalam satu waktu; membuka bangunan
// lain (atau menutup panel) menutup yang sebelumnya.
//
// Data interior disimpan per posisi grid ("x,y") supaya tetap
// cocok sesudah muat ulang (posisi bangunan tidak berubah).
// Bangunan yang dijual kehilangan interiornya.

using System.Collections.Generic;
using UnityEngine;

public class InteriorManager : MonoBehaviour
{
    [Header("Sprite interior (diisi dari Scene, urutan: rumah, kost, apartemen, perumahan)")]
    public Sprite[] spriteInterior;

    [Header("Sprite perabot (diisi dari Scene, urutan = indeks slot SistemInterior)")]
    public Sprite[] spritePerabot;

    [Header("Sprite warga penghuni di dalam ruangan (diisi dari Scene)")]
    public Sprite[] spriteWarga;

    // Bangunan yang sedang dalam mode interior (null = tidak ada).
    public BangunanTerpasang BangunanDalamInterior { get; private set; }

    private BuildManager buildManager;
    private CitizenManager citizenManager;
    private Dictionary<string, List<string>> interiorPerBangunan;
    private System.Random acak;

    private void Awake()
    {
        buildManager = FindObjectOfType<BuildManager>();
        citizenManager = FindObjectOfType<CitizenManager>();
        interiorPerBangunan = new Dictionary<string, List<string>>();
        acak = new System.Random(System.Environment.TickCount);
        BangunanDalamInterior = null;
    }

    // ---- Data interior ----
    private static string Kunci(Vector2Int posisi)
    {
        return posisi.x + "," + posisi.y;
    }

    // Daftar perabot satu bangunan (dibuat kosong bila belum ada).
    public List<string> AmbilInterior(BangunanTerpasang b)
    {
        if (b == null)
        {
            return SistemInterior.InteriorKosong();
        }
        string kunci = Kunci(b.PosisiGrid);
        List<string> isi;
        if (!interiorPerBangunan.TryGetValue(kunci, out isi))
        {
            isi = SistemInterior.InteriorKosong();
            interiorPerBangunan[kunci] = isi;
        }
        return isi;
    }

    // Total perabot terpasang di semua hunian (dipakai misi).
    public int TotalPerabotTerpasang()
    {
        int total = 0;
        foreach (List<string> isi in interiorPerBangunan.Values)
        {
            total += SistemInterior.HitungPerabot(isi);
        }
        return total;
    }

    // Apakah ada satu Kost yang semua 10 slotnya terisi (misi).
    public bool ApakahAdaKostLengkap()
    {
        if (buildManager == null)
        {
            return false;
        }
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            if (b.Data != null && b.Data.id == "kost"
                && SistemInterior.ApakahLengkap(AmbilInterior(b)))
            {
                return true;
            }
        }
        return false;
    }

    // Total bonus kebahagiaan interior se-dunia (jumlah bonus
    // perabot semua hunian, termasuk -5 per hunian kosong),
    // dibatasi -20..+20 (GameBalance.BatasBonusInterior).
    public int BonusInteriorTotal()
    {
        if (buildManager == null)
        {
            return 0;
        }
        int total = 0;
        foreach (BangunanTerpasang b in buildManager.DaftarTerpasang)
        {
            if (b.Data == null || b.Data.kategori != KategoriBangunan.Hunian)
            {
                continue;
            }
            total += SistemInterior.BonusBahagia(AmbilInterior(b));
        }
        return SistemInterior.BatasiBonusTotal(total);
    }

    // Pengali sewa satu bangunan (persen): Kost lengkap = 125,
    // sisanya 100. Dipakai EconomyManager saat menghitung pemasukan.
    public int PengaliSewa(BangunanTerpasang b)
    {
        if (b == null || b.Data == null || b.Data.id != "kost")
        {
            return 100;
        }
        if (SistemInterior.ApakahLengkap(AmbilInterior(b)))
        {
            return GameBalance.PengaliSewaKostLengkap;
        }
        return 100;
    }

    // ---- Beli / hapus perabot ----
    // Mengembalikan false bila bukan hunian, slot tidak valid,
    // sudah terisi, atau koin tidak cukup.
    public bool BeliPerabot(BangunanTerpasang b, int slot)
    {
        if (b == null || b.Data == null
            || b.Data.kategori != KategoriBangunan.Hunian)
        {
            return false;
        }
        if (GameManager.Instance == null)
        {
            return false;
        }
        List<string> isi = AmbilInterior(b);
        long koinBaru;
        if (!SistemInterior.Beli(isi, slot, GameManager.Instance.Koin, out koinBaru))
        {
            return false;
        }
        long harga = SistemInterior.HargaPerabot(SistemInterior.IdPerabotSlot(slot));
        GameManager.Instance.UbahKoin(-harga);
        SegarkanTampilanInterior(b);
        if (citizenManager != null)
        {
            citizenManager.HitungUlang();
        }
        return true;
    }

    // Hapus perabot (uang kembali 50%). False bila slot kosong.
    public bool HapusPerabot(BangunanTerpasang b, int slot)
    {
        if (b == null || b.Data == null
            || b.Data.kategori != KategoriBangunan.Hunian)
        {
            return false;
        }
        if (GameManager.Instance == null)
        {
            return false;
        }
        List<string> isi = AmbilInterior(b);
        long koinBaru;
        if (!SistemInterior.Hapus(isi, slot, GameManager.Instance.Koin, out koinBaru))
        {
            return false;
        }
        long kembali = koinBaru - GameManager.Instance.Koin;
        if (kembali > 0)
        {
            GameManager.Instance.UbahKoin(kembali);
        }
        SegarkanTampilanInterior(b);
        if (citizenManager != null)
        {
            citizenManager.HitungUlang();
        }
        return true;
    }

    // ---- Mode interior (tembus pandang) ----
    // Bangunan yang memiliki ruangan interior (hunian + fasilitas publik/komersial).
    public static bool ApakahPunyaInterior(BuildingData d)
    {
        if (d == null) return false;
        if (d.kategori == KategoriBangunan.Hunian) return true;
        return d.id == "balaikota" || d.id == "klinik" || d.id == "pos_polisi"
            || d.id == "sekolah" || d.id == "pasar" || d.id == "perpustakaan"
            || d.id == "kampus" || d.id == "kafe" || d.id == "warung"
            || d.id == "minimarket";
    }

    // Sprite denah untuk satu bangunan (hunian maupun fasilitas umum).
    public Sprite SpriteInteriorUntuk(string idBangunan)
    {
        if (spriteInterior == null || spriteInterior.Length == 0)
        {
            return null;
        }
        int indeks = -1;
        if (idBangunan == "rumah")
        {
            indeks = 0;
        }
        else if (idBangunan == "kost")
        {
            indeks = 1;
        }
        else if (idBangunan == "apartemen")
        {
            indeks = 2;
        }
        else if (idBangunan == "perumahan")
        {
            indeks = 3;
        }
        else if (idBangunan == "balaikota" || idBangunan == "klinik"
            || idBangunan == "pos_polisi" || idBangunan == "sekolah"
            || idBangunan == "pasar" || idBangunan == "perpustakaan"
            || idBangunan == "kampus" || idBangunan == "kafe"
            || idBangunan == "warung" || idBangunan == "minimarket")
        {
            indeks = spriteInterior.Length > 2 ? 2 : 0;
        }
        if (indeks < 0 || indeks >= spriteInterior.Length)
        {
            return null;
        }
        return spriteInterior[indeks];
    }

    // Masuk mode interior untuk hunian / fasilitas publik: tukar sprite ke denah
    // + gambar perabot terpasang + warga/petugas di ruangan.
    public void MasukModeInterior(BangunanTerpasang b)
    {
        if (b == null || b.Data == null || !ApakahPunyaInterior(b.Data))
        {
            return;
        }
        if (BangunanDalamInterior != null && BangunanDalamInterior != b)
        {
            KeluarModeInterior();
        }
        BuildingView tampilan = b.Objek != null
            ? b.Objek.GetComponent<BuildingView>()
            : null;
        if (tampilan == null)
        {
            return;
        }
        Sprite denah = SpriteInteriorUntuk(b.Data.id);
        IsometricGrid grid = FindObjectOfType<IsometricGrid>();
        bool mode3D = grid != null && grid.mode3D;
        if (!mode3D && denah == null)
        {
            return;
        }
        BangunanDalamInterior = b;
        tampilan.TampilkanInterior(denah);
        List<string> isi = AmbilInterior(b);

        if (mode3D)
        {
            List<Vector3> titik3D = new List<Vector3>();
            for (int slot = 0; slot < SistemInterior.JumlahSlot; slot++)
            {
                if (!string.IsNullOrEmpty(isi[slot]))
                {
                    Model3DInterior.TambahPerabot3D(b.Data, tampilan.transform, slot);
                    titik3D.Add(Model3DInterior.PosisiSlot3D(b.Data, slot));
                }
            }
            if (titik3D.Count == 0)
            {
                titik3D.Add(new Vector3(-0.3f, 0.14f, 0.3f));
                titik3D.Add(new Vector3(0.3f, 0.14f, -0.3f));
            }
            int jmlWarga = b.Data.kapasitasWarga >= 8 ? 2 : 1;
            for (int i = 0; i < jmlWarga; i++)
            {
                Vector3 posW = titik3D[i % titik3D.Count];
                GameObject w3d = Model3DInterior.TambahWarga3D(tampilan.transform, posW, i);
                if (w3d != null)
                {
                    PenghuniInterior gerak = w3d.AddComponent<PenghuniInterior>();
                    gerak.Atur3D(titik3D, acak);
                }
            }
            return;
        }
        float skalaPerabot = 0.35f + 0.05f * b.Data.ukuran.x;
        int urutan = tampilan.UrutanGambar;
        bool adalahHunian = b.Data.kategori == KategoriBangunan.Hunian;
        List<Vector2> titikAktivitas = new List<Vector2>();
        List<string> labelAktivitas = new List<string>();
        if (adalahHunian)
        {
            for (int slot = 0; slot < SistemInterior.JumlahSlot; slot++)
            {
                if (isi[slot] == null || isi[slot].Length == 0) continue;
                Sprite sprite = SpritePerabotUntuk(slot);
                if (sprite == null) continue;
                Vector2 posisi = PosisiSlot(b, slot);
                tampilan.TambahAnakInterior(sprite, posisi, skalaPerabot, urutan + 1);
                titikAktivitas.Add(posisi);
                labelAktivitas.Add("Menggunakan " + SistemInterior.NamaPerabot(SistemInterior.IdPerabotSlot(slot)));
            }
        }
        else
        {
            int[] slotPublik = SlotDefaultPublik(b.Data.id);
            for (int i = 0; i < slotPublik.Length; i++)
            {
                int slot = slotPublik[i];
                Sprite sprite = SpritePerabotUntuk(slot);
                if (sprite == null) continue;
                Vector2 posisi = PosisiSlot(b, slot);
                tampilan.TambahAnakInterior(sprite, posisi, skalaPerabot, urutan + 1);
                titikAktivitas.Add(posisi);
                labelAktivitas.Add(AktivitasPublikUntuk(b.Data.id, i));
            }
        }
        // Warga/petugas di dalam ruangan (aktif berjalan & berinteraksi antar barang):
        int jumlahWarga = adalahHunian ? (b.Data.kapasitasWarga >= 8 ? 2 : 1) : 3;
        Vector2[] posisiWarga = PosisiWarga(b);
        foreach (Vector2 pw in posisiWarga)
        {
            titikAktivitas.Add(pw);
        }
        for (int i = 0; i < jumlahWarga; i++)
        {
            Sprite sprite = SpriteWargaAcak();
            if (sprite == null) continue;
            Vector2 posAwal = posisiWarga[i % posisiWarga.Length];
            GameObject anakWarga = tampilan.TambahAnakInterior(
                sprite, posAwal, 0.35f, urutan + 2);
            if (anakWarga != null)
            {
                PenghuniInterior gerak = anakWarga.AddComponent<PenghuniInterior>();
                gerak.Atur(titikAktivitas, acak, labelAktivitas);
            }
        }
    }

    private static int[] SlotDefaultPublik(string id)
    {
        if (id == "balaikota") return new[] { 1, 3, 4, 7, 8, 9 };
        if (id == "klinik") return new[] { 0, 1, 3, 4, 6, 9 };
        if (id == "pos_polisi") return new[] { 1, 3, 4, 7, 9 };
        if (id == "kafe" || id == "warung" || id == "pasar" || id == "minimarket")
            return new[] { 2, 3, 4, 7, 9 };
        return new[] { 1, 3, 4, 6, 9 };
    }

    private static string AktivitasPublikUntuk(string id, int indeks)
    {
        if (id == "balaikota")
        {
            string[] teks = { "Merapikan arsip...", "Menandatangani izin...", "Melayani warga...", "Memantau distrik...", "Rapat koordinasi..." };
            return teks[indeks % teks.Length];
        }
        if (id == "klinik")
        {
            string[] teks = { "Merawat pasien...", "Menyiapkan obat...", "Memeriksa resep...", "Konsultasi dokter...", "Cuci tangan..." };
            return teks[indeks % teks.Length];
        }
        if (id == "pos_polisi")
        {
            string[] teks = { "Mengecek berkas...", "Menerima laporan...", "Siaga patroli...", "Memantau CCTV...", "Patroli pos..." };
            return teks[indeks % teks.Length];
        }
        string[] umum = { "Menata barang...", "Melayani pengunjung...", "Mencatat stok...", "Membersihkan meja..." };
        return umum[indeks % umum.Length];
    }

    // Keluar mode interior: sprite luar kembali, anak-anak dihapus.
    public void KeluarModeInterior()
    {
        if (BangunanDalamInterior == null)
        {
            return;
        }
        BuildingView tampilan = BangunanDalamInterior.Objek != null
            ? BangunanDalamInterior.Objek.GetComponent<BuildingView>()
            : null;
        if (tampilan != null)
        {
            tampilan.BersihkanAnakInterior();
            tampilan.TampilkanLuar();
        }
        BangunanDalamInterior = null;
    }

    // Gambar ulang anak interior sesudah beli/hapus perabot
    // (hanya bila bangunan ini sedang dalam mode interior).
    private void SegarkanTampilanInterior(BangunanTerpasang b)
    {
        if (BangunanDalamInterior != b)
        {
            return;
        }
        // Keluar lalu masuk lagi: anak lama dihapus, anak baru
        // digambar dari data terbaru.
        BangunanDalamInterior = null;
        MasukModeInterior(b);
    }

    private Sprite SpritePerabotUntuk(int slot)
    {
        if (spritePerabot == null || slot < 0 || slot >= spritePerabot.Length)
        {
            return null;
        }
        return spritePerabot[slot];
    }

    private Sprite SpriteWargaAcak()
    {
        if (spriteWarga == null || spriteWarga.Length == 0 || acak == null)
        {
            return null;
        }
        return spriteWarga[acak.Next(0, spriteWarga.Length)];
    }

    // Posisi satu slot perabot (offset lokal dari tengah bangunan,
    // dalam unit dunia). Denah dibagi 4 segitiga (ruangan):
    // atas = Kamar Tidur, kanan = Dapur, bawah = Ruang Tamu,
    // kiri = Kamar Mandi. Offset y dikurangi offsetVisualY karena
    // titik tengah sprite berada 0,4 unit di atas tengah petak.
    private Vector2 PosisiSlot(BangunanTerpasang b, int slot)
    {
        Vector2 jangkar;
        Vector2 goyang;
        string ruangan = SistemInterior.NamaRuangan(slot);
        if (ruangan == "Kamar Tidur")
        {
            jangkar = new Vector2(0f, 0.25f);
            goyang = slot == 0 ? new Vector2(-0.18f, 0.06f) : new Vector2(0.18f, -0.02f);
        }
        else if (ruangan == "Dapur")
        {
            jangkar = new Vector2(0.5f, 0f);
            if (slot == 2)
            {
                goyang = new Vector2(-0.2f, 0.05f);
            }
            else if (slot == 3)
            {
                goyang = new Vector2(0.05f, 0.08f);
            }
            else
            {
                goyang = new Vector2(0.22f, -0.06f);
            }
        }
        else if (ruangan == "Kamar Mandi")
        {
            jangkar = new Vector2(-0.5f, 0f);
            goyang = slot == 5 ? new Vector2(-0.18f, 0.05f) : new Vector2(0.18f, -0.05f);
        }
        else
        {
            jangkar = new Vector2(0f, -0.25f);
            if (slot == 7)
            {
                goyang = new Vector2(-0.2f, 0.06f);
            }
            else if (slot == 8)
            {
                goyang = new Vector2(0.05f, -0.02f);
            }
            else
            {
                goyang = new Vector2(0.22f, 0.06f);
            }
        }
        float x = (jangkar.x + goyang.x) * b.Data.ukuran.x;
        float y = (jangkar.y + goyang.y) * b.Data.ukuran.y - b.Data.offsetVisualY;
        return new Vector2(x, y);
    }

    // Posisi warga penghuni: Ruang Tamu dan Kamar Tidur.
    private Vector2[] PosisiWarga(BangunanTerpasang b)
    {
        float ox = b.Data.offsetVisualY;
        return new Vector2[]
        {
            new Vector2(0.15f * b.Data.ukuran.x, -0.25f * b.Data.ukuran.y - ox),
            new Vector2(-0.15f * b.Data.ukuran.x, 0.25f * b.Data.ukuran.y - ox)
        };
    }

    // Ringkasan interior untuk panel detail hunian (UIManager):
    // "Perabot: N/10 • Bonus bahagia: +N" (+ "Sewa +25%" bila Kost
    // lengkap). String kosong bila bukan hunian.
    public string TeksRingkasanDetail(BangunanTerpasang b)
    {
        if (b == null || b.Data == null
            || b.Data.kategori != KategoriBangunan.Hunian)
        {
            return "";
        }
        List<string> isi = AmbilInterior(b);
        int jumlah = SistemInterior.HitungPerabot(isi);
        int bonus = SistemInterior.BonusBahagia(isi);
        string teks = "\n\nPerabot: " + jumlah + "/"
            + SistemInterior.JumlahSlot
            + " • Bonus bahagia: " + (bonus >= 0 ? "+" : "") + bonus;
        if (b.Data.id == "kost" && SistemInterior.ApakahLengkap(isi))
        {
            teks += " • Sewa +25%";
        }
        return teks;
    }

    // ---- Simpan / muat ----
    public List<DataInteriorTersimpan> SimpanInterior()
    {
        List<DataInteriorTersimpan> hasil = new List<DataInteriorTersimpan>();
        foreach (KeyValuePair<string, List<string>> entri in interiorPerBangunan)
        {
            string[] bagian = entri.Key.Split(',');
            if (bagian.Length != 2)
            {
                continue;
            }
            int x;
            int y;
            if (!int.TryParse(bagian[0], out x) || !int.TryParse(bagian[1], out y))
            {
                continue;
            }
            // Hanya simpan bangunan yang masih ada di dunia
            // (interior bangunan yang sudah dijual sudah dihapus).
            hasil.Add(new DataInteriorTersimpan
            {
                x = x,
                y = y,
                perabot = new List<string>(SistemInterior.Normalisasi(entri.Value))
            });
        }
        return hasil;
    }

    public void MuatInterior(List<DataInteriorTersimpan> simpanan)
    {
        interiorPerBangunan.Clear();
        if (simpanan == null)
        {
            return;
        }
        foreach (DataInteriorTersimpan s in simpanan)
        {
            if (s == null)
            {
                continue;
            }
            interiorPerBangunan[Kunci(new Vector2Int(s.x, s.y))] =
                SistemInterior.Normalisasi(s.perabot);
        }
    }

    // Hapus interior satu bangunan (dipanggil saat bangunan dijual).
    public void HapusInterior(BangunanTerpasang b)
    {
        if (b == null)
        {
            return;
        }
        if (BangunanDalamInterior == b)
        {
            KeluarModeInterior();
        }
        interiorPerBangunan.Remove(Kunci(b.PosisiGrid));
    }
}
