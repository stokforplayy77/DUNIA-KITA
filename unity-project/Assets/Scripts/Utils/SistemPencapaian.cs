// Dunia Kita — SistemPencapaian
// Logika MURNI pencapaian / achievement (GDD §8), tanpa dependensi
// UnityEngine, supaya bisa diuji di luar Unity (tools/test-fase4.cs).
//
// FASE 4: 6 pencapaian pertama. Hadiah berupa Permata (GDD §6:
// Permata langka, dari pencapaian) — angka hadiah di GameBalance.
// Pencapaian dinilai dari keadaan dunia nyata (jumlah bangunan,
// level, kebahagiaan); status klaim disimpan di SaveData
// (GameManager.PencapaianDiklaim), BUKAN di sini — class ini hanya
// menjawab "apakah syaratnya terpenuhi sekarang?".

public static class SistemPencapaian
{
    public const string JuraganKost = "juragan_kost";
    public const string KotaPelajar = "kota_pelajar";
    public const string KotaHijau = "kota_hijau";
    public const string Metropolis = "metropolis";
    public const string JuraganPasar = "juragan_pasar";
    public const string KotaSejahtera = "kota_sejahtera";

    // Keadaan dunia yang dibutuhkan untuk menilai semua pencapaian.
    public struct KeadaanPencapaian
    {
        public int JumlahKost;
        public int JumlahTaman;
        public int JumlahKomersial; // Warung + Toko + Pasar + Kafe
        public bool PunyaSekolah;
        public bool PunyaKampus;
        public int LevelDunia;
        public int Kebahagiaan;
    }

    // Semua id pencapaian, sesuai urutan tampil di panel.
    public static string[] SemuaId()
    {
        return new string[]
        {
            JuraganKost, KotaPelajar, KotaHijau,
            Metropolis, JuraganPasar, KotaSejahtera
        };
    }

    public static string Judul(string id)
    {
        switch (id)
        {
            case JuraganKost: return "Juragan Kost";
            case KotaPelajar: return "Kota Pelajar";
            case KotaHijau: return "Kota Hijau";
            case Metropolis: return "Metropolis";
            case JuraganPasar: return "Juragan Pasar";
            case KotaSejahtera: return "Kota Sejahtera";
            default: return "";
        }
    }

    public static string Deskripsi(string id)
    {
        switch (id)
        {
            case JuraganKost: return "Punya 5 Kost.";
            case KotaPelajar: return "Punya Sekolah Dasar dan Kampus Kecil.";
            case KotaHijau: return "Punya 20 Taman.";
            case Metropolis: return "Capai Level 6 — Metropolis.";
            case JuraganPasar: return "Punya 3 bangunan komersial (Warung, Toko, Pasar, atau Kafe).";
            case KotaSejahtera: return "Kebahagiaan warga mencapai 80%.";
            default: return "";
        }
    }

    // Hadiah Permata sekali klaim (angka di GameBalance).
    public static int HadiahPermata(string id)
    {
        switch (id)
        {
            case JuraganKost: return GameBalance.PermataJuraganKost;
            case KotaPelajar: return GameBalance.PermataKotaPelajar;
            case KotaHijau: return GameBalance.PermataKotaHijau;
            case Metropolis: return GameBalance.PermataMetropolis;
            case JuraganPasar: return GameBalance.PermataJuraganPasar;
            case KotaSejahtera: return GameBalance.PermataKotaSejahtera;
            default: return 0;
        }
    }

    // Apakah syarat pencapaian ini terpenuhi dengan keadaan sekarang?
    public static bool ApakahTercapai(string id, KeadaanPencapaian keadaan)
    {
        switch (id)
        {
            case JuraganKost: return keadaan.JumlahKost >= 5;
            case KotaPelajar: return keadaan.PunyaSekolah && keadaan.PunyaKampus;
            case KotaHijau: return keadaan.JumlahTaman >= 20;
            case Metropolis: return keadaan.LevelDunia >= 6;
            case JuraganPasar: return keadaan.JumlahKomersial >= 3;
            case KotaSejahtera: return keadaan.Kebahagiaan >= 80;
            default: return false;
        }
    }
}
