// Dunia Kita — SistemInterior
// Logika MURNI interior hunian (GDD §5.6), tanpa dependensi
// UnityEngine, supaya bisa diuji di luar Unity (tools/test-fase4b.cs).
//
// Model: tiap hunian punya 10 SLOT perabot tetap, masing-masing
// terikat pada SATU jenis perabot (bukan penempatan bebas —
// supaya bisa dites dan disimpan sederhana):
//
//   Slot  Ruangan      Perabot
//   0-1   Kamar Tidur  Kasur, Lemari
//   2-4   Dapur        Kompor, Meja, Kursi
//   5-6   Kamar Mandi  Bak Mandi, Wastafel
//   7-9   Ruang Tamu   TV, Sofa, Lampu
//
// Isi slot disimpan sebagai List<string> berisi id perabot
// ("" = slot kosong). Harga + bonus kebahagiaan perabot ada di
// GameBalance (Fase 4B). Hapus perabot mengembalikan 50% harga
// (sama seperti rasio jual bangunan).
//
// Efek (GDD §5.6):
// - Bonus kebahagiaan = jumlah bonus semua perabot terpasang;
//   hunian TANPA perabot sama sekali = -5 (penghuni tidak betah).
// - Kost yang semua 10 slotnya terisi = sewa +25%
//   (EconomyCalculator, lewat PengaliSewaPersen).
// - Total bonus interior se-dunia dibatasi -20..+20
//   (BatasiBonusTotal) supaya furnitur tidak membuat kebahagiaan
//   langsung 100 dari rumah pertama.

using System.Collections.Generic;

public static class SistemInterior
{
    public const int JumlahSlot = 10;

    // Id perabot per slot (tetap — urutan JANGAN diubah, karena
    // simpanan menyimpan id perabot per indeks slot).
    public static readonly string[] PerabotPerSlot =
    {
        "kasur", "lemari",
        "kompor", "meja", "kursi",
        "bak_mandi", "wastafel",
        "tv", "sofa", "lampu"
    };

    // Nama ruangan tiap slot (0-1 Kamar Tidur, 2-4 Dapur,
    // 5-6 Kamar Mandi, 7-9 Ruang Tamu).
    public static string NamaRuangan(int slot)
    {
        if (slot < 0 || slot >= JumlahSlot)
        {
            return "";
        }
        if (slot <= 1)
        {
            return "Kamar Tidur";
        }
        if (slot <= 4)
        {
            return "Dapur";
        }
        if (slot <= 6)
        {
            return "Kamar Mandi";
        }
        return "Ruang Tamu";
    }

    public static string IdPerabotSlot(int slot)
    {
        if (slot < 0 || slot >= JumlahSlot)
        {
            return "";
        }
        return PerabotPerSlot[slot];
    }

    public static string NamaPerabot(string id)
    {
        switch (id)
        {
            case "kasur": return "Kasur";
            case "lemari": return "Lemari";
            case "kompor": return "Kompor";
            case "meja": return "Meja";
            case "kursi": return "Kursi";
            case "bak_mandi": return "Bak Mandi";
            case "wastafel": return "Wastafel";
            case "tv": return "TV";
            case "sofa": return "Sofa";
            case "lampu": return "Lampu";
            default: return "";
        }
    }

    public static long HargaPerabot(string id)
    {
        switch (id)
        {
            case "kasur": return GameBalance.HargaKasur;
            case "lemari": return GameBalance.HargaLemari;
            case "kompor": return GameBalance.HargaKompor;
            case "meja": return GameBalance.HargaMeja;
            case "kursi": return GameBalance.HargaKursi;
            case "bak_mandi": return GameBalance.HargaBakMandi;
            case "wastafel": return GameBalance.HargaWastafel;
            case "tv": return GameBalance.HargaTV;
            case "sofa": return GameBalance.HargaSofa;
            case "lampu": return GameBalance.HargaLampu;
            default: return 0;
        }
    }

    public static int BonusBahagiaPerabot(string id)
    {
        switch (id)
        {
            case "kasur": return GameBalance.BonusKasur;
            case "lemari": return GameBalance.BonusLemari;
            case "kompor": return GameBalance.BonusKompor;
            case "meja": return GameBalance.BonusMeja;
            case "kursi": return GameBalance.BonusKursi;
            case "bak_mandi": return GameBalance.BonusBakMandi;
            case "wastafel": return GameBalance.BonusWastafel;
            case "tv": return GameBalance.BonusTV;
            case "sofa": return GameBalance.BonusSofa;
            case "lampu": return GameBalance.BonusLampu;
            default: return 0;
        }
    }

    // Nama file sprite perabot (di Assets/Sprites/).
    public static string NamaSpritePerabot(string id)
    {
        if (id == null || id.Length == 0)
        {
            return "";
        }
        return "perabot-" + id + ".png";
    }

    // Interior kosong: 10 slot berisi "".
    public static List<string> InteriorKosong()
    {
        List<string> isi = new List<string>();
        for (int i = 0; i < JumlahSlot; i++)
        {
            isi.Add("");
        }
        return isi;
    }

    // Normalisasi daftar simpanan lama/rusak menjadi 10 slot
    // (kurang -> ditambah "", lebih -> dipotong, null -> kosong).
    public static List<string> Normalisasi(IList<string> isi)
    {
        List<string> hasil = InteriorKosong();
        if (isi == null)
        {
            return hasil;
        }
        for (int i = 0; i < JumlahSlot && i < isi.Count; i++)
        {
            string id = isi[i];
            // Hanya id perabot yang dikenal yang dipertahankan.
            hasil[i] = NamaPerabot(id).Length > 0 ? id : "";
        }
        return hasil;
    }

    public static int HitungPerabot(IList<string> isi)
    {
        if (isi == null)
        {
            return 0;
        }
        int jumlah = 0;
        for (int i = 0; i < isi.Count && i < JumlahSlot; i++)
        {
            if (isi[i] != null && isi[i].Length > 0)
            {
                jumlah++;
            }
        }
        return jumlah;
    }

    public static bool ApakahKosong(IList<string> isi)
    {
        return HitungPerabot(isi) == 0;
    }

    public static bool ApakahLengkap(IList<string> isi)
    {
        return HitungPerabot(isi) >= JumlahSlot;
    }

    // Bonus kebahagiaan satu hunian: jumlah bonus perabot
    // terpasang; bila tidak ada perabot sama sekali, -5
    // (penghuni tidak betah, GDD §5.6).
    public static int BonusBahagia(IList<string> isi)
    {
        if (ApakahKosong(isi))
        {
            return -GameBalance.HukumanRumahKosong;
        }
        int total = 0;
        for (int i = 0; i < JumlahSlot && isi != null && i < isi.Count; i++)
        {
            total += BonusBahagiaPerabot(isi[i]);
        }
        return total;
    }

    // Total bonus interior se-dunia dibatasi -20..+20.
    public static int BatasiBonusTotal(int total)
    {
        if (total > GameBalance.BatasBonusInterior)
        {
            return GameBalance.BatasBonusInterior;
        }
        if (total < -GameBalance.BatasBonusInterior)
        {
            return -GameBalance.BatasBonusInterior;
        }
        return total;
    }

    // Beli perabot untuk satu slot (slot terikat satu jenis
    // perabot). Mengembalikan false bila slot tidak valid, sudah
    // terisi, atau koin tidak cukup. koinBaru = koin sesudah beli.
    public static bool Beli(IList<string> isi, int slot, long koin,
        out long koinBaru)
    {
        koinBaru = koin;
        if (isi == null || slot < 0 || slot >= JumlahSlot)
        {
            return false;
        }
        if (isi[slot] != null && isi[slot].Length > 0)
        {
            return false;
        }
        long harga = HargaPerabot(PerabotPerSlot[slot]);
        if (harga <= 0 || koin < harga)
        {
            return false;
        }
        isi[slot] = PerabotPerSlot[slot];
        koinBaru = koin - harga;
        return true;
    }

    // Harga jual kembali perabot = 50% harga beli.
    public static long HargaJualPerabot(string id)
    {
        return HargaPerabot(id) / 2;
    }

    // Hapus perabot dari slot (uang kembali 50%). Mengembalikan
    // false bila slot tidak valid atau sudah kosong.
    public static bool Hapus(IList<string> isi, int slot, long koin,
        out long koinBaru)
    {
        koinBaru = koin;
        if (isi == null || slot < 0 || slot >= JumlahSlot)
        {
            return false;
        }
        string id = isi[slot];
        if (id == null || id.Length == 0)
        {
            return false;
        }
        koinBaru = koin + HargaJualPerabot(id);
        isi[slot] = "";
        return true;
    }
}
