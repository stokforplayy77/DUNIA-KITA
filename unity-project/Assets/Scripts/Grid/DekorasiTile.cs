// Dunia Kita — DekorasiTile
// Dekorasi kecil acak di petak rumput (bunga kecil, batu, semak)
// supaya tanah tidak terlihat monoton seperti game amatir.
// Ditempel GridManager saat GambarTanah(); satu petak mungkin punya
// 0-2 dekorasi kecil (peluang 25% per petak).

using UnityEngine;

public static class DekorasiTile
{
    private static Sprite spriteBunga;
    private static Sprite spriteBatu;
    private static Sprite spriteSemak;

    // Tambah dekorasi acak di petak tertentu.
    public static void TambahDekorasiAcak(Transform induk, Vector3 posisiPetak,
        int x, int y)
    {
        // Hash deterministik dari posisi supaya dekorasi tetap sama tiap load
        int hash = x * 7919 + y * 6271;
        float peluang = ((hash & 0x7FFFFFFF) % 100) / 100f;
        if (peluang > 0.30f) return; // 30% petak punya dekorasi

        int jenis = (hash >> 3) % 3;
        Sprite sp;
        Color warna;
        float skala;
        switch (jenis)
        {
            case 0: // Bunga kecil
                sp = BuatSpriteBunga();
                // Warna bunga bervariasi
                float hue = ((hash >> 5) % 100) / 100f;
                warna = Color.HSVToRGB(hue, 0.6f, 0.9f);
                warna.a = 0.75f;
                skala = 0.2f + ((hash >> 7) % 20) / 100f;
                break;
            case 1: // Batu kecil
                sp = BuatSpriteBatu();
                float kecerahan = 0.5f + ((hash >> 9) % 20) / 100f;
                warna = new Color(kecerahan, kecerahan * 0.95f, kecerahan * 0.85f, 0.5f);
                skala = 0.15f + ((hash >> 11) % 15) / 100f;
                break;
            default: // Semak kecil
                sp = BuatSpriteSemak();
                warna = new Color(0.3f, 0.65f + ((hash >> 13) % 15) / 100f, 0.25f, 0.6f);
                skala = 0.2f + ((hash >> 15) % 20) / 100f;
                break;
        }

        GameObject deko = new GameObject("Deko_" + x + "_" + y);
        deko.transform.SetParent(induk, false);
        // Posisi sedikit offset dari pusat petak (acak tapi deterministik)
        float ox = ((hash >> 4) % 40 - 20) / 100f;
        float oy = ((hash >> 6) % 20 - 10) / 100f;
        deko.transform.position = posisiPetak + new Vector3(ox, oy, 0f);
        deko.transform.localScale = new Vector3(skala, skala, 1f);

        SpriteRenderer sr = deko.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.color = warna;
        sr.sortingOrder = 1;
    }

    private static Sprite BuatSpriteBunga()
    {
        if (spriteBunga != null) return spriteBunga;
        const int uk = 10;
        Texture2D tex = new Texture2D(uk, uk);
        Color[] px = new Color[uk * uk];
        for (int y = 0; y < uk; y++)
        {
            for (int x = 0; x < uk; x++)
            {
                float nx = (x - uk * 0.5f) / (uk * 0.5f);
                float ny = (y - uk * 0.5f) / (uk * 0.5f);
                float d = nx * nx + ny * ny;
                // Kelopak (4 titik + pusat)
                float kelopak = Mathf.Max(
                    Mathf.Clamp01(1f - ((nx - 0.3f) * (nx - 0.3f) + ny * ny) * 8f),
                    Mathf.Max(
                        Mathf.Clamp01(1f - ((nx + 0.3f) * (nx + 0.3f) + ny * ny) * 8f),
                        Mathf.Max(
                            Mathf.Clamp01(1f - (nx * nx + (ny - 0.3f) * (ny - 0.3f)) * 8f),
                            Mathf.Clamp01(1f - (nx * nx + (ny + 0.3f) * (ny + 0.3f)) * 8f))));
                float pusat = Mathf.Clamp01(1f - d * 5f);
                float a = Mathf.Max(kelopak, pusat);
                px[y * uk + x] = a > 0.1f
                    ? new Color(1f, 1f, pusat > 0.1f ? 0.4f : 1f, a)
                    : Color.clear;
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteBunga = Sprite.Create(tex, new Rect(0, 0, uk, uk),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteBunga;
    }

    private static Sprite BuatSpriteBatu()
    {
        if (spriteBatu != null) return spriteBatu;
        const int uk = 8;
        Texture2D tex = new Texture2D(uk, uk);
        Color[] px = new Color[uk * uk];
        for (int y = 0; y < uk; y++)
        {
            for (int x = 0; x < uk; x++)
            {
                float nx = (x - uk * 0.5f) / (uk * 0.5f);
                float ny = (y - uk * 0.5f) / (uk * 0.5f);
                float d = nx * nx * 1.3f + ny * ny * 2f;
                float a = Mathf.Clamp01(1f - d);
                px[y * uk + x] = new Color(0.6f, 0.58f, 0.52f, a > 0.2f ? a : 0f);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteBatu = Sprite.Create(tex, new Rect(0, 0, uk, uk),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteBatu;
    }

    private static Sprite BuatSpriteSemak()
    {
        if (spriteSemak != null) return spriteSemak;
        const int uk = 12;
        Texture2D tex = new Texture2D(uk, uk);
        Color[] px = new Color[uk * uk];
        for (int y = 0; y < uk; y++)
        {
            for (int x = 0; x < uk; x++)
            {
                float nx = (x - uk * 0.5f) / (uk * 0.5f);
                float ny = (y - uk * 0.5f + 1f) / (uk * 0.5f);
                float bentuk = 1f - (nx * nx * 1.8f + ny * ny * 3f);
                float a = Mathf.Clamp01(bentuk);
                px[y * uk + x] = new Color(0.25f, 0.6f, 0.2f, a > 0.15f ? a : 0f);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteSemak = Sprite.Create(tex, new Rect(0, 0, uk, uk),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteSemak;
    }
}
