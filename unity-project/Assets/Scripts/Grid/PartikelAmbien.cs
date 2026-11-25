// Dunia Kita — PartikelAmbien
// Efek partikel ambien melayang di dunia supaya terasa hidup seperti
// game profesional (daun jatuh, kupu-kupu, debu berkilau, burung kecil).
// Dibuat EfekDuniaManager dan mengikuti kamera; partikel muncul/hilang
// otomatis sesuai jarak terhadap viewport kamera.

using UnityEngine;

public class PartikelAmbien : MonoBehaviour
{
    public enum JenisPartikel { Daun, KupuKupu, DebuKilau, Burung }
    public JenisPartikel Jenis { get; private set; }

    private float umur;
    private float durasiHidup;
    private SpriteRenderer sr;
    private float arahX;
    private float arahY;
    private float faseSway;
    private float amplitudoSway;
    private bool arahFlip;

    // Sprite cache (satu per jenis, dibagi semua partikel)
    private static Sprite spriteDaun;
    private static Sprite spriteKupu;
    private static Sprite spriteDebu;
    private static Sprite spriteBurung;

    public void Atur(JenisPartikel jenis, Vector3 posisi)
    {
        Jenis = jenis;
        transform.position = posisi;
        umur = 0f;
        faseSway = Random.Range(0f, 6.28f);

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 85;

        switch (jenis)
        {
            case JenisPartikel.Daun:
                sr.sprite = BuatSpriteDaun();
                sr.color = new Color(0.35f, 0.80f, 0.30f, 0.75f);
                durasiHidup = Random.Range(4f, 7f);
                arahX = Random.Range(-0.4f, 0.4f);
                arahY = Random.Range(-0.3f, -0.12f);
                amplitudoSway = Random.Range(0.3f, 0.7f);
                float skalaDaun = Random.Range(0.6f, 1.1f);
                transform.localScale = new Vector3(skalaDaun, skalaDaun, 1f);
                break;

            case JenisPartikel.KupuKupu:
                sr.sprite = BuatSpriteKupu();
                float h = Random.Range(0f, 1f);
                sr.color = Color.HSVToRGB(h, 0.55f, 0.95f);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.85f);
                durasiHidup = Random.Range(6f, 10f);
                arahX = Random.Range(-0.25f, 0.25f);
                arahY = Random.Range(-0.08f, 0.15f);
                amplitudoSway = Random.Range(0.5f, 1.2f);
                float skalaKupu = Random.Range(0.4f, 0.7f);
                transform.localScale = new Vector3(skalaKupu, skalaKupu, 1f);
                break;

            case JenisPartikel.DebuKilau:
                sr.sprite = BuatSpriteDebu();
                sr.color = new Color(1f, 0.95f, 0.7f, 0.6f);
                durasiHidup = Random.Range(2.5f, 4.5f);
                arahX = Random.Range(-0.1f, 0.1f);
                arahY = Random.Range(0.08f, 0.22f);
                amplitudoSway = Random.Range(0.1f, 0.3f);
                float skalaDebu = Random.Range(0.3f, 0.6f);
                transform.localScale = new Vector3(skalaDebu, skalaDebu, 1f);
                break;

            case JenisPartikel.Burung:
                sr.sprite = BuatSpriteBurung();
                sr.color = new Color(0.25f, 0.25f, 0.30f, 0.65f);
                durasiHidup = Random.Range(5f, 9f);
                arahX = Random.value > 0.5f ? Random.Range(0.8f, 1.6f)
                    : Random.Range(-1.6f, -0.8f);
                arahY = Random.Range(0.05f, 0.25f);
                amplitudoSway = Random.Range(0.15f, 0.35f);
                float skalaBurung = Random.Range(0.35f, 0.55f);
                transform.localScale = new Vector3(skalaBurung, skalaBurung, 1f);
                arahFlip = arahX < 0f;
                sr.flipX = arahFlip;
                break;
        }
    }

    private void Update()
    {
        umur += Time.deltaTime;
        float t = Mathf.Clamp01(umur / durasiHidup);

        // Fade in/out
        float alpha = t < 0.15f ? t / 0.15f
            : (t > 0.8f ? (1f - t) / 0.2f : 1f);

        Color c = sr.color;
        c.a = alpha * (Jenis == JenisPartikel.DebuKilau ? 0.6f : 0.8f);
        sr.color = c;

        // Gerakan
        float sway = Mathf.Sin((umur + faseSway) * 2.2f) * amplitudoSway;
        Vector3 gerak = new Vector3(
            arahX + sway * 0.3f * Time.deltaTime,
            arahY * Time.deltaTime,
            0f);

        if (Jenis == JenisPartikel.Daun)
        {
            // Daun berputar sambil jatuh
            float rotasi = Mathf.Sin(umur * 3f + faseSway) * 35f;
            transform.rotation = Quaternion.Euler(0f, 0f, rotasi);
            gerak.x += Mathf.Cos(umur * 1.8f) * 0.4f * Time.deltaTime;
        }
        else if (Jenis == JenisPartikel.KupuKupu)
        {
            // Kupu-kupu mengepak (skala Y berubah)
            float kepak = Mathf.Abs(Mathf.Sin(umur * 6f));
            Vector3 sk = transform.localScale;
            float skDasar = Mathf.Abs(sk.x);
            transform.localScale = new Vector3(skDasar, skDasar * (0.4f + kepak * 0.6f), 1f);
            gerak.y += Mathf.Sin(umur * 1.5f) * 0.3f * Time.deltaTime;
        }
        else if (Jenis == JenisPartikel.DebuKilau)
        {
            // Debu berkilau kedap-kedip
            float kilau = 0.4f + Mathf.Abs(Mathf.Sin(umur * 5f + faseSway)) * 0.6f;
            c.a *= kilau;
            sr.color = c;
        }
        else if (Jenis == JenisPartikel.Burung)
        {
            // Burung mengepak sayap
            float kepak = Mathf.Abs(Mathf.Sin(umur * 8f));
            Vector3 sk = transform.localScale;
            float skDasar = Mathf.Abs(sk.x);
            transform.localScale = new Vector3(skDasar, skDasar * (0.7f + kepak * 0.3f), 1f);
        }

        transform.position += gerak;

        if (umur >= durasiHidup)
        {
            Destroy(gameObject);
        }
    }

    // ---- Pembuatan sprite prosedural (cached) ----

    private static Sprite BuatSpriteDaun()
    {
        if (spriteDaun != null) return spriteDaun;
        Texture2D tex = new Texture2D(16, 12);
        Color[] px = new Color[16 * 12];
        for (int y = 0; y < 12; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float nx = (x - 8f) / 8f;
                float ny = (y - 6f) / 6f;
                float bentuk = 1f - (nx * nx * 1.5f + ny * ny * 2.5f);
                float daun = Mathf.Clamp01(bentuk + nx * ny * 0.4f);
                px[y * 16 + x] = new Color(0.3f, 0.7f, 0.2f, daun > 0.1f ? daun : 0f);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteDaun = Sprite.Create(tex, new Rect(0, 0, 16, 12),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteDaun;
    }

    private static Sprite BuatSpriteKupu()
    {
        if (spriteKupu != null) return spriteKupu;
        Texture2D tex = new Texture2D(20, 14);
        Color[] px = new Color[20 * 14];
        for (int y = 0; y < 14; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                float nx = (x - 10f) / 10f;
                float ny = (y - 7f) / 7f;
                // Dua sayap (elips kiri dan kanan) + badan tengah
                float sayapKiri = 1f - ((nx + 0.45f) * (nx + 0.45f) * 4f + ny * ny * 3f);
                float sayapKanan = 1f - ((nx - 0.45f) * (nx - 0.45f) * 4f + ny * ny * 3f);
                float badan = Mathf.Abs(nx) < 0.08f && Mathf.Abs(ny) < 0.5f ? 0.9f : 0f;
                float a = Mathf.Max(Mathf.Max(sayapKiri, sayapKanan), badan);
                px[y * 20 + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteKupu = Sprite.Create(tex, new Rect(0, 0, 20, 14),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteKupu;
    }

    private static Sprite BuatSpriteDebu()
    {
        if (spriteDebu != null) return spriteDebu;
        Texture2D tex = new Texture2D(8, 8);
        Color[] px = new Color[64];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                float d = ((x - 3.5f) * (x - 3.5f) + (y - 3.5f) * (y - 3.5f));
                float a = Mathf.Clamp01(1f - d / 12f);
                px[y * 8 + x] = new Color(1f, 0.98f, 0.85f, a);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteDebu = Sprite.Create(tex, new Rect(0, 0, 8, 8),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteDebu;
    }

    private static Sprite BuatSpriteBurung()
    {
        if (spriteBurung != null) return spriteBurung;
        Texture2D tex = new Texture2D(16, 10);
        Color[] px = new Color[16 * 10];
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float nx = (x - 8f) / 8f;
                float ny = (y - 5f) / 5f;
                // Bentuk V terbalik (sayap terbentang)
                float bentuk = 1f - Mathf.Abs(ny - Mathf.Abs(nx) * 0.6f) * 4f
                    - Mathf.Abs(nx) * 0.5f;
                float a = Mathf.Clamp01(bentuk);
                px[y * 16 + x] = new Color(0.2f, 0.2f, 0.25f, a);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteBurung = Sprite.Create(tex, new Rect(0, 0, 16, 10),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteBurung;
    }
}
