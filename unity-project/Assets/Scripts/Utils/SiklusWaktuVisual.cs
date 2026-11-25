// Dunia Kita — SiklusWaktuVisual
// Logika MURNI siklus siang-malam & cuaca hujan (GDD §4 & §9).
// Menghitung fase waktu (Pagi/Siang/Sore/Malam), warna langit,
// dan intensitas malam dari sisa detik PewaktuHari — tanpa
// dependensi UnityEngine supaya bisa diuji di luar Unity.

public static class SiklusWaktuVisual
{
    public enum FaseWaktu
    {
        Pagi = 0,
        Siang = 1,
        Sore = 2,
        Malam = 3
    }

    // Progres hari 0..1 (0 = awal hari pagi, 1 = akhir malam).
    public static float HitungProgresHari(float sisaDetik, float durasiHari)
    {
        if (durasiHari <= 0f)
        {
            return 0f;
        }
        float p = 1f - (sisaDetik / durasiHari);
        if (p < 0f) return 0f;
        if (p > 1f) return 1f;
        return p;
    }

    public static FaseWaktu TentukanFase(float progres)
    {
        if (progres < 0.20f) return FaseWaktu.Pagi;
        if (progres < 0.65f) return FaseWaktu.Siang;
        if (progres < 0.80f) return FaseWaktu.Sore;
        return FaseWaktu.Malam;
    }

    public static string NamaFase(FaseWaktu fase)
    {
        switch (fase)
        {
            case FaseWaktu.Pagi: return "Pagi";
            case FaseWaktu.Siang: return "Siang";
            case FaseWaktu.Sore: return "Sore";
            default: return "Malam";
        }
    }

    // Intensitas gelap malam 0..1 (0 = terang siang, 1 = malam penuh).
    public static float HitungIntensitasMalam(float progres)
    {
        if (progres < 0.65f) return 0f;
        if (progres < 0.80f)
        {
            return (progres - 0.65f) / 0.15f;
        }
        if (progres < 0.95f) return 1f;
        return 1f - ((progres - 0.95f) / 0.05f) * 0.5f;
    }

    // Warna latar langit (0..1 RGB) sesuai progres hari & status hujan.
    public static void HitungWarnaLangit(float progres, bool sedangHujan,
        out float r, out float g, out float b)
    {
        // Warna acuan: Pagi (biru cerah hangat), Siang (biru langit),
        // Sore (oranye senja), Malam (biru tua malam).
        if (progres < 0.20f)
        {
            float t = progres / 0.20f;
            r = Lerp(0.55f, 0.45f, t);
            g = Lerp(0.75f, 0.78f, t);
            b = Lerp(0.88f, 0.92f, t);
        }
        else if (progres < 0.65f)
        {
            r = 0.45f;
            g = 0.78f;
            b = 0.92f;
        }
        else if (progres < 0.80f)
        {
            float t = (progres - 0.65f) / 0.15f;
            r = Lerp(0.45f, 0.78f, t);
            g = Lerp(0.78f, 0.48f, t);
            b = Lerp(0.92f, 0.42f, t);
        }
        else
        {
            float t = (progres - 0.80f) / 0.20f;
            r = Lerp(0.78f, 0.13f, t);
            g = Lerp(0.48f, 0.17f, t);
            b = Lerp(0.42f, 0.30f, t);
        }

        if (sedangHujan)
        {
            // Mendung: kurangi kecerahan dan geser ke abu-abu biru.
            r = r * 0.68f + 0.12f;
            g = g * 0.70f + 0.14f;
            b = b * 0.78f + 0.16f;
        }
    }

    // Warna tint bangunan/tanah (putih saat siang, hangat saat sore,
    // biru lembut saat malam).
    public static void HitungTintDunia(float progres, bool sedangHujan,
        out float r, out float g, out float b)
    {
        float malam = HitungIntensitasMalam(progres);
        r = Lerp(1f, 0.62f, malam);
        g = Lerp(1f, 0.66f, malam);
        b = Lerp(1f, 0.84f, malam);
        if (progres >= 0.65f && progres < 0.80f)
        {
            // Sentuhan hangat senja.
            float senja = 1f - System.Math.Abs(progres - 0.725f) / 0.075f;
            if (senja > 0f)
            {
                r = Lerp(r, 1f, senja * 0.4f);
                g = Lerp(g, 0.84f, senja * 0.3f);
            }
        }
        if (sedangHujan)
        {
            r *= 0.86f;
            g *= 0.89f;
            b *= 0.96f;
        }
    }

    private static float Lerp(float a, float b, float t)
    {
        if (t < 0f) t = 0f;
        if (t > 1f) t = 1f;
        return a + (b - a) * t;
    }
}
