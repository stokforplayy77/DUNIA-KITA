// Dunia Kita — BayanganBangunan
// Bayangan lembut di bawah setiap bangunan supaya terlihat
// "menyatu" dengan tanah, mirip game city-builder profesional.
// Bayangan = sprite elips gelap transparan, posisi sedikit di bawah
// dan kanan bangunan (arah cahaya matahari isometrik).

using UnityEngine;

public class BayanganBangunan : MonoBehaviour
{
    private SpriteRenderer sr;
    private static Sprite spriteBayangan;
    private float offsetX;
    private float offsetY;

    public static void TambahBayangan(GameObject bangunan, Vector2Int ukuran)
    {
        if (bangunan == null) return;
        // Jangan duplikat
        if (bangunan.transform.Find("Bayangan") != null) return;

        GameObject bayang = new GameObject("Bayangan");
        bayang.transform.SetParent(bangunan.transform, false);

        BayanganBangunan komponen = bayang.AddComponent<BayanganBangunan>();
        komponen.Atur(ukuran);
    }

    private void Atur(Vector2Int ukuran)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = BuatSpriteBayangan();
        sr.color = new Color(0f, 0f, 0f, 0.18f);
        sr.sortingOrder = 1; // Di atas tanah, di bawah bangunan

        // Skala bayangan sesuai ukuran bangunan
        float skalaX = 1.3f + (ukuran.x - 1) * 0.5f;
        float skalaY = 0.5f + (ukuran.y - 1) * 0.2f;
        transform.localScale = new Vector3(skalaX, skalaY, 1f);

        // Offset ke bawah-kanan (arah bayangan isometrik)
        offsetX = 0.08f;
        offsetY = -0.28f;
        transform.localPosition = new Vector3(offsetX, offsetY, 0f);
    }

    private static Sprite BuatSpriteBayangan()
    {
        if (spriteBayangan != null) return spriteBayangan;

        const int lebar = 32;
        const int tinggi = 16;
        Texture2D tex = new Texture2D(lebar, tinggi);
        Color[] px = new Color[lebar * tinggi];
        for (int y = 0; y < tinggi; y++)
        {
            for (int x = 0; x < lebar; x++)
            {
                float nx = (x - lebar * 0.5f) / (lebar * 0.5f);
                float ny = (y - tinggi * 0.5f) / (tinggi * 0.5f);
                float d = nx * nx + ny * ny;
                float a = Mathf.Clamp01(1f - d);
                // Gaussian soft edge
                a = a * a;
                px[y * lebar + x] = new Color(0f, 0f, 0f, a);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteBayangan = Sprite.Create(tex, new Rect(0, 0, lebar, tinggi),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteBayangan;
    }
}
