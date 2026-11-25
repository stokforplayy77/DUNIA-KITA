// Dunia Kita — PewaktuHari
// Pewaktu hari game waktu nyata (Fase 3 revisi desain 24 Nov 2026):
// 1 hari game = GameBalance.DurasiHariDetik (300 detik = 5 menit).
// Logika MURNI tanpa dependensi UnityEngine, supaya bisa diuji di
// luar Unity (tools/test-fase3.cs).
//
// Cara pakai (GameManager.Update): tiap frame panggil
// Tambah(Time.deltaTime); bila kembali true, hari berganti —
// GameManager lalu memproses pergantian hari. Kelebihan waktu
// (mis. aplikasi sempat dijeda lama) dibuang: maksimal 1 hari
// berganti per panggilan, supaya tidak ada lonjakan hari.

public class PewaktuHari
{
    // Sisa detik sampai hari berikutnya berganti.
    public float SisaDetik { get; private set; }

    public PewaktuHari()
    {
        SisaDetik = GameBalance.DurasiHariDetik;
    }

    // Maju sebanyak `detik` waktu nyata. Mengembalikan true tepat
    // saat melewati batas hari (timer direset ke durasi penuh).
    public bool Tambah(float detik)
    {
        if (detik < 0f)
        {
            detik = 0f;
        }
        SisaDetik -= detik;
        if (SisaDetik > 0f)
        {
            return false;
        }
        SisaDetik = GameBalance.DurasiHariDetik;
        return true;
    }

    // Teks sisa waktu "MM:SS" untuk HUD, contoh: "03:12".
    public string TeksSisaWaktu()
    {
        int total = (int)SisaDetik;
        if (total < 0)
        {
            total = 0;
        }
        int menit = total / 60;
        int detik = total % 60;
        string teksMenit = menit < 10 ? "0" + menit : "" + menit;
        string teksDetik = detik < 10 ? "0" + detik : "" + detik;
        return teksMenit + ":" + teksDetik;
    }
}
