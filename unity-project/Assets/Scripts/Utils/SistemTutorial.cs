// Dunia Kita — SistemTutorial
// Logika murni tutorial pemain baru (Fase 5, GDD §9: misi tutorial
// memandu pemain, bukan memaksa). TIDAK memakai API Unity selain
// PlayerPrefs (untuk mengingat "sudah selesai") — jadi bisa dites
// penuh tanpa Editor.
//
// Aturan:
//   - 6 langkah singkat, berurutan, masing-masing satu kalimat.
//   - Langkah "bangun-rumah" dilewati OTOMATIS bila pemain sudah
//     membangun Rumah Sederhana sebelum tutorial selesai (cek state
//     nyata lewat parameter sudahBangunRumah — jangan paksa urutan
//     kaku pada pemain yang sudah lebih dulu membangun).
//   - Tutorial muncul SEKALI (PlayerPrefs "DuniaKita_TutorialSelesai").
//   - Pemain bisa menekan "Lewati" di langkah mana pun (langsung
//     selesai) atau "Lanjut" sampai langkah terakhir (juga selesai).
//   - Bisa diulang dari Pengaturan ("Ulangi Tutorial") — flag
//     selesai dihapus dulu (ResetUntukUlang), lalu tampil lagi.

using UnityEngine;

public static class SistemTutorial
{
    // Kunci PlayerPrefs: 1 = tutorial sudah selesai (jangan tampilkan).
    public const string KunciSelesai = "DuniaKita_TutorialSelesai";

    public struct LangkahTutorial
    {
        public string Id;
        public string Judul;
        public string Teks;
    }

    // Urutan langkah JANGAN diubah sembarangan: PanelTutorial
    // menampilkan judul + teks langkah ke-i; tes Fase 5 mengunci
    // jumlah dan isi langkah ini.
    public static readonly LangkahTutorial[] LangkahSemua =
    {
        new LangkahTutorial
        {
            Id = "selamat-datang",
            Judul = "Selamat datang di Dunia Kita!",
            Teks = "Kamu punya sebidang tanah kosong dan modal Rp5.000.000. "
                + "Ketuk petak kosong untuk memilih tempat membangun.",
        },
        new LangkahTutorial
        {
            Id = "bangun-rumah",
            Judul = "Bangun Rumah Sederhana",
            Teks = "Bangun 1 Rumah Sederhana supaya warga pertama bisa "
                + "pindah. Satu rumah menampung 2 warga.",
        },
        new LangkahTutorial
        {
            Id = "tombol-bangun",
            Judul = "Tombol Bangun",
            Teks = "Tombol palu+rumah di bawah membuka menu bangun "
                + "untuk petak yang terakhir kamu ketuk.",
        },
        new LangkahTutorial
        {
            Id = "hari-otomatis",
            Judul = "Hari berganti sendiri",
            Teks = "Hari game berganti otomatis tiap 5 menit — lihat "
                + "hitung mundur di bar atas. Tiap pergantian hari ada "
                + "laporan dan pemasukan masuk.",
        },
        new LangkahTutorial
        {
            Id = "detail-interior",
            Judul = "Ketuk bangunan untuk detail",
            Teks = "Ketuk bangunan apa pun untuk melihat detailnya. "
                + "Hunian bisa dibuka interiornya: ketuk 'Lihat Interior' "
                + "untuk mengatur perabot.",
        },
        new LangkahTutorial
        {
            Id = "misi-pencapaian",
            Judul = "Misi & Pencapaian",
            Teks = "Tombol Misi dan Pencapaian di bar atas memberi "
                + "hadiah koin dan Permata. Selamat membangun duniamu!",
        },
    };

    public static int JumlahLangkah
    {
        get { return LangkahSemua.Length; }
    }

    // Apakah tutorial perlu ditampilkan (belum pernah selesai)?
    public static bool PerluTampilkan()
    {
        return PlayerPrefs.GetInt(KunciSelesai, 0) == 0;
    }

    public static void TandaiSelesai()
    {
        PlayerPrefs.SetInt(KunciSelesai, 1);
        PlayerPrefs.Save();
    }

    // Untuk "Ulangi Tutorial" di Pengaturan: hapus flag selesai.
    public static void ResetUntukUlang()
    {
        PlayerPrefs.SetInt(KunciSelesai, 0);
        PlayerPrefs.Save();
    }

    // Apakah langkah ini terpenuhi otomatis dari keadaan dunia?
    // Hanya "bangun-rumah" yang punya syarat otomatis (sudah ada
    // Rumah Sederhana); langkah lain selalu harus dibaca pemain.
    public static bool TerpenuhiOtomatis(int indeks, bool sudahBangunRumah)
    {
        if (indeks < 0 || indeks >= LangkahSemua.Length)
        {
            return false;
        }
        return LangkahSemua[indeks].Id == "bangun-rumah" && sudahBangunRumah;
    }

    // Indeks langkah berikut yang harus ditampilkan, mulai dari
    // indeksSaatIni (langkah yang baru selesai dibaca). Melewati
    // langkah yang terpenuhi otomatis. Mengembalikan JumlahLangkah
    // bila tidak ada lagi langkah (tutorial selesai).
    public static int LangkahBerikut(int indeksSaatIni, bool sudahBangunRumah)
    {
        int i = indeksSaatIni + 1;
        while (i < LangkahSemua.Length
            && TerpenuhiOtomatis(i, sudahBangunRumah))
        {
            i++;
        }
        return i;
    }

    // Indeks langkah pertama yang harus ditampilkan (dipakai saat
    // tutorial baru dibuka — langkah awal yang sudah terpenuhi
    // otomatis juga dilewati).
    public static int LangkahPertama(bool sudahBangunRumah)
    {
        int i = 0;
        while (i < LangkahSemua.Length
            && TerpenuhiOtomatis(i, sudahBangunRumah))
        {
            i++;
        }
        return i;
    }
}
