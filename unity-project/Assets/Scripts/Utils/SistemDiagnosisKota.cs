// Dunia Kita — SistemDiagnosisKota
// Logika MURNI untuk menganalisis kondisi kota & memberi saran
// konkret ke pemain (tanpa dependensi UnityEngine, bisa diuji
// di luar Unity).

using System.Collections.Generic;
using System.Text;

public static class SistemDiagnosisKota
{
    public struct InputDiagnosis
    {
        public int Populasi;
        public int KapasitasHunian;
        public int Kebahagiaan;
        public int JumlahTaman;
        public int JumlahBangunanTanpaJalan;
        public int HariMenunggak;
        public int JumlahKomersial;
        public int JumlahHunian;
        public long PemasukanBersihHarian;
        public int LevelDunia;
    }

    public static List<string> BuatSaran(InputDiagnosis d)
    {
        List<string> saran = new List<string>();
        if (d.JumlahBangunanTanpaJalan > 0)
        {
            saran.Add("Ada " + d.JumlahBangunanTanpaJalan
                + " bangunan belum menempel ke Jalan (efektivitas turun 50%).");
        }
        if (d.HariMenunggak > 0)
        {
            saran.Add("Kas kota menunggak " + d.HariMenunggak
                + " hari! Tambah bangunan komersial atau kurangi biaya perawatan.");
        }
        if (d.Populasi > d.KapasitasHunian)
        {
            saran.Add("Ada warga belum punya rumah! Bangun atau upgrade hunian.");
        }
        if (d.JumlahHunian > 0 && d.JumlahKomersial == 0)
        {
            saran.Add("Belum ada bangunan komersial (Warung/Toko). Bangun agar kota berpenghasilan.");
        }
        if (d.JumlahTaman == 0 && d.JumlahHunian > 0)
        {
            saran.Add("Belum ada Taman. Taman menaikkan kebahagiaan warga sekitar.");
        }
        if (d.PemasukanBersihHarian < 0)
        {
            saran.Add("Pemasukan harian minus (" + FormatUtil.Rupiah(d.PemasukanBersihHarian)
                + "). Bangun atau upgrade usaha.");
        }
        if (saran.Count == 0)
        {
            saran.Add("Kota dalam kondisi sehat! Terus kembangkan fasilitas & interior hunian.");
        }
        return saran;
    }

    public static string FormatLaporan(InputDiagnosis d)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Ringkasan Kondisi Kota:");
        sb.AppendLine("• Warga: " + d.Populasi + " / " + d.KapasitasHunian + " kapasitas");
        sb.AppendLine("• Kebahagiaan: " + d.Kebahagiaan + "%");
        sb.AppendLine("• Perkiraan bersih/hari: " + FormatUtil.Rupiah(d.PemasukanBersihHarian));
        sb.AppendLine("• Bangunan tanpa jalan: " + d.JumlahBangunanTanpaJalan);
        sb.AppendLine();
        sb.AppendLine("Saran Prioritas:");
        foreach (string s in BuatSaran(d))
        {
            sb.AppendLine("→ " + s);
        }
        return sb.ToString();
    }
}
