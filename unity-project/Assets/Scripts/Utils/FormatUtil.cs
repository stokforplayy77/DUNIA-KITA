// Dunia Kita — FormatUtil
// Pembantu format tampilan (Rupiah, hari game).

using System.Globalization;

public static class FormatUtil
{
    private static readonly CultureInfo BudayaIndonesia = new CultureInfo("id-ID");

    // Contoh: 5000000 -> "Rp5.000.000"
    public static string Rupiah(long jumlah)
    {
        return "Rp" + jumlah.ToString("N0", BudayaIndonesia);
    }

    // Contoh: 3 -> "Hari 3"
    public static string Hari(int hari)
    {
        return "Hari " + hari;
    }
}
