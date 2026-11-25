// Dunia Kita — CahayaMalamBangunan
// Efek jendela menyala saat malam (glow kuning hangat) pada bangunan
// yang berpenghuni. Menambah kesan "hidup" seperti game Happy Cityzens.
// Dibuat otomatis oleh BuildingView saat Atur().

using UnityEngine;

public class CahayaMalamBangunan : MonoBehaviour
{
    private SpriteRenderer[] srJendela;
    private float intensitasSaatIni;
    private float intensitasTarget;
    private bool sedangMalam;
    private static Sprite spriteJendela;
    private string idBangunan;
    private float kedipTimer;
    private float kedipInterval;

    public void Atur(string id, Vector2Int ukuran)
    {
        idBangunan = id;
        kedipInterval = Random.Range(3f, 6f);
        kedipTimer = Random.Range(0f, kedipInterval);
        intensitasSaatIni = 0f;
        intensitasTarget = 0f;

        // Jendela: 1-3 titik cahaya tergantung ukuran bangunan
        int jumlahJendela = JumlahJendela(id, ukuran);
        srJendela = new SpriteRenderer[jumlahJendela];

        Sprite sp = BuatSpriteJendela();
        for (int i = 0; i < jumlahJendela; i++)
        {
            GameObject jendelaObj = new GameObject("CahayaJendela_" + i);
            jendelaObj.transform.SetParent(transform, false);
            Vector2 offset = PosisiJendela(i, jumlahJendela, ukuran);
            jendelaObj.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            float skala = Random.Range(0.3f, 0.5f);
            jendelaObj.transform.localScale = new Vector3(skala, skala, 1f);
            SpriteRenderer sr = jendelaObj.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.color = new Color(1f, 0.88f, 0.45f, 0f);
            sr.sortingOrder = 500; // Di depan bangunan
            srJendela[i] = sr;
        }
    }

    private void Update()
    {
        // Transisi halus nyala/mati
        intensitasSaatIni = Mathf.MoveTowards(
            intensitasSaatIni, intensitasTarget, Time.deltaTime * 0.8f);

        if (intensitasSaatIni < 0.01f && !sedangMalam)
        {
            return; // Hemat performa siang hari
        }

        // Kedap-kedip realistis saat malam
        kedipTimer += Time.deltaTime;
        for (int i = 0; i < srJendela.Length; i++)
        {
            if (srJendela[i] == null) continue;
            float kedip = 1f;
            if (sedangMalam)
            {
                kedip = 0.85f + Mathf.Sin(kedipTimer * 1.5f + i * 2.1f) * 0.15f;
            }
            Color c = srJendela[i].color;
            c.a = intensitasSaatIni * kedip;
            srJendela[i].color = c;
        }
    }

    // Dipanggil EfekDuniaManager sesuai siklus waktu
    public void AturMalam(bool malam)
    {
        sedangMalam = malam;
        // Jalan tidak punya lampu jendela
        if (idBangunan == "jalan" || idBangunan == "taman"
            || idBangunan == "alun_alun")
        {
            intensitasTarget = 0f;
            return;
        }
        intensitasTarget = malam ? 0.65f : 0f;
    }

    private int JumlahJendela(string id, Vector2Int ukuran)
    {
        if (id == "jalan" || id == "taman" || id == "alun_alun") return 0;
        if (ukuran.x >= 2 || ukuran.y >= 2) return 3;
        if (id == "rumah") return 2;
        return 2;
    }

    private Vector2 PosisiJendela(int indeks, int total, Vector2Int ukuran)
    {
        float basis = 0.25f + (ukuran.x - 1) * 0.15f;
        switch (total)
        {
            case 1: return new Vector2(0f, basis);
            case 2:
                return indeks == 0
                    ? new Vector2(-0.18f, basis)
                    : new Vector2(0.18f, basis + 0.12f);
            case 3:
                if (indeks == 0) return new Vector2(-0.22f, basis - 0.05f);
                if (indeks == 1) return new Vector2(0.05f, basis + 0.15f);
                return new Vector2(0.22f, basis + 0.05f);
            default: return Vector2.zero;
        }
    }

    private static Sprite BuatSpriteJendela()
    {
        if (spriteJendela != null) return spriteJendela;
        const int uk = 12;
        Texture2D tex = new Texture2D(uk, uk);
        Color[] px = new Color[uk * uk];
        for (int y = 0; y < uk; y++)
        {
            for (int x = 0; x < uk; x++)
            {
                float nx = (x - uk * 0.5f) / (uk * 0.5f);
                float ny = (y - uk * 0.5f) / (uk * 0.5f);
                float d = nx * nx + ny * ny;
                float a = Mathf.Clamp01(1f - d * 0.8f);
                a = a * a; // Soft glow
                px[y * uk + x] = new Color(1f, 0.9f, 0.5f, a);
            }
        }
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        spriteJendela = Sprite.Create(tex, new Rect(0, 0, uk, uk),
            new Vector2(0.5f, 0.5f), 32f);
        return spriteJendela;
    }
}
