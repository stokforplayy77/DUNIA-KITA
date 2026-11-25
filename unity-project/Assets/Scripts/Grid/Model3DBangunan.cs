// Dunia Kita — Model3DBangunan
// Pabrik model 3D Low-Poly ringan untuk seluruh 18 bangunan.
// Memakai Material bersama (GPU Instancing) supaya tetap enteng
// di HP Android/iOS (60 FPS).

using System.Collections.Generic;
using UnityEngine;

public static class Model3DBangunan
{
    private static Dictionary<string, Material> cacheMat = new Dictionary<string, Material>();

    public static Material Mat(string hex)
    {
        Material m;
        if (cacheMat.TryGetValue(hex, out m) && m != null)
        {
            return m;
        }
        Color c = Color.white;
        ColorUtility.TryParseHtmlString(hex, out c);
        Shader s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Sprites/Default");
        m = new Material(s);
        m.color = c;
        m.enableInstancing = true;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        cacheMat[hex] = m;
        return m;
    }

    private static GameObject Kotak(Transform induk, string nama, Vector3 pos,
        Vector3 skala, string hex)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nama;
        go.transform.SetParent(induk, false);
        go.transform.localPosition = pos;
        go.transform.localScale = skala;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = Mat(hex);
        return go;
    }

    private static GameObject Bola(Transform induk, string nama, Vector3 pos,
        Vector3 skala, string hex)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = nama;
        go.transform.SetParent(induk, false);
        go.transform.localPosition = pos;
        go.transform.localScale = skala;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = Mat(hex);
        return go;
    }

    private static GameObject Silinder(Transform induk, string nama, Vector3 pos,
        Vector3 skala, string hex)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = nama;
        go.transform.SetParent(induk, false);
        go.transform.localPosition = pos;
        go.transform.localScale = skala;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = Mat(hex);
        return go;
    }

    public static void BangunModel3D(BuildingData data, Transform induk)
    {
        if (data == null || induk == null) return;
        float w = data.ukuran.x * GameBalance.LebarTile;
        float d = data.ukuran.y * GameBalance.LebarTile;
        string id = data.id;

        if (id == "jalan")
        {
            Kotak(induk, "Trotoar", new Vector3(0f, 0.015f, 0f), new Vector3(w * 0.98f, 0.03f, d * 0.98f), "#D5DCE2");
            Kotak(induk, "Aspal", new Vector3(0f, 0.03f, 0f), new Vector3(w * 0.78f, 0.03f, d * 0.78f), "#4A5568");
            Kotak(induk, "Marka", new Vector3(0f, 0.046f, 0f), new Vector3(0.45f, 0.01f, 0.12f), "#F6AD55");
            return;
        }

        if (id == "taman" || id == "alun_alun")
        {
            Kotak(induk, "AlasTaman", new Vector3(0f, 0.02f, 0f), new Vector3(w * 0.92f, 0.04f, d * 0.92f), "#88D45C");
            float off = id == "alun_alun" ? 1.8f : 0.45f;
            Pohon(induk, new Vector3(-off, 0f, -off), 1f);
            Pohon(induk, new Vector3(off, 0f, off), 0.9f);
            if (id == "alun_alun")
            {
                Pohon(induk, new Vector3(-off, 0f, off), 1.1f);
                Pohon(induk, new Vector3(off, 0f, -off), 1.0f);
                Silinder(induk, "AirMancur", new Vector3(0f, 0.22f, 0f), new Vector3(1.5f, 0.22f, 1.5f), "#CBD5E0");
                Silinder(induk, "Kolam", new Vector3(0f, 0.36f, 0f), new Vector3(1.2f, 0.08f, 1.2f), "#63B3ED");
                Silinder(induk, "Tugu", new Vector3(0f, 0.75f, 0f), new Vector3(0.35f, 0.45f, 0.35f), "#ECC94B");
            }
            else
            {
                Kotak(induk, "Bangku", new Vector3(0.4f, 0.16f, -0.35f), new Vector3(0.5f, 0.12f, 0.22f), "#9C6644");
            }
            return;
        }

        if (id == "balaikota")
        {
            Kotak(induk, "Plaza", new Vector3(0f, 0.08f, 0f), new Vector3(w * 0.92f, 0.16f, d * 0.92f), "#CBD5E0");
            Kotak(induk, "Badan", new Vector3(0f, 0.95f, 0f), new Vector3(w * 0.76f, 1.6f, d * 0.68f), "#FAF0CA");
            // 4 Pilar depan
            for (int i = 0; i < 4; i++)
            {
                float px = -1.1f + i * 0.73f;
                Silinder(induk, "Pilar_" + i, new Vector3(px, 0.95f, -d * 0.37f), new Vector3(0.22f, 0.8f, 0.22f), "#FFFFFF");
            }
            Kotak(induk, "AtapUtama", new Vector3(0f, 1.85f, 0f), new Vector3(w * 0.82f, 0.24f, d * 0.74f), "#3B5998");
            Bola(induk, "AtapKubah", new Vector3(0f, 2.25f, 0f), new Vector3(1.2f, 0.85f, 1.2f), "#ECC94B");
            Silinder(induk, "TiangBendera", new Vector3(0f, 2.95f, 0f), new Vector3(0.06f, 0.45f, 0.06f), "#4A5568");
            Kotak(induk, "BenderaMerah", new Vector3(0.22f, 3.28f, 0f), new Vector3(0.42f, 0.14f, 0.04f), "#E53E3E");
            Kotak(induk, "BenderaPutih", new Vector3(0.22f, 3.14f, 0f), new Vector3(0.42f, 0.14f, 0.04f), "#FFFFFF");
            return;
        }

        if (id == "klinik")
        {
            Kotak(induk, "Alas", new Vector3(0f, 0.06f, 0f), new Vector3(w * 0.92f, 0.12f, d * 0.92f), "#E2E8F0");
            Kotak(induk, "Badan", new Vector3(0f, 0.9f, 0f), new Vector3(w * 0.78f, 1.56f, d * 0.72f), "#FFFFFF");
            Kotak(induk, "Atap", new Vector3(0f, 1.78f, 0f), new Vector3(w * 0.82f, 0.2f, d * 0.76f), "#4FD1C5");
            // Palang merah 3D di atas atap & depan
            Kotak(induk, "PalangV", new Vector3(0f, 2.02f, 0f), new Vector3(0.32f, 0.28f, 0.95f), "#E53E3E");
            Kotak(induk, "PalangH", new Vector3(0f, 2.02f, 0f), new Vector3(0.95f, 0.28f, 0.32f), "#E53E3E");
            Kotak(induk, "PintuKaca", new Vector3(0f, 0.45f, -d * 0.37f), new Vector3(0.9f, 0.75f, 0.08f), "#63B3ED");
            return;
        }

        if (id == "pos")
        {
            Kotak(induk, "Alas", new Vector3(0f, 0.05f, 0f), new Vector3(w * 0.9f, 0.1f, d * 0.9f), "#CBD5E0");
            Kotak(induk, "Badan", new Vector3(0f, 0.65f, 0f), new Vector3(w * 0.75f, 1.1f, d * 0.72f), "#F7FAFC");
            Kotak(induk, "AtapBiru", new Vector3(0f, 1.28f, 0f), new Vector3(w * 0.82f, 0.2f, d * 0.78f), "#2B6CB0");
            // Lampu sirene polisi (merah + biru)
            Kotak(induk, "SireneM", new Vector3(-0.18f, 1.44f, 0f), new Vector3(0.28f, 0.14f, 0.22f), "#E53E3E");
            Kotak(induk, "SireneB", new Vector3(0.18f, 1.44f, 0f), new Vector3(0.28f, 0.14f, 0.22f), "#3182CE");
            return;
        }

        if (id == "pembangkit")
        {
            Kotak(induk, "Alas", new Vector3(0f, 0.06f, 0f), new Vector3(w * 0.92f, 0.12f, d * 0.92f), "#A0AEC0");
            Kotak(induk, "Badan", new Vector3(-0.3f, 0.85f, 0f), new Vector3(w * 0.56f, 1.45f, d * 0.7f), "#4A5568");
            Silinder(induk, "Cerobong1", new Vector3(0.95f, 1.35f, -0.5f), new Vector3(0.55f, 1.25f, 0.55f), "#718096");
            Silinder(induk, "Cerobong2", new Vector3(0.95f, 1.15f, 0.6f), new Vector3(0.55f, 1.05f, 0.55f), "#718096");
            Kotak(induk, "Listrik", new Vector3(-0.3f, 1.65f, 0f), new Vector3(0.6f, 0.22f, 0.6f), "#ECC94B");
            return;
        }

        // Bangunan hunian, komersial, dan pendidikan bergaya kartun hangat
        float tinggi = (id == "apartemen" || id == "perumahan" || id == "kampus") ? 2.2f
            : (data.ukuran.x >= 2 ? 1.45f : 1.05f);
        string warnaDinding = id == "warung" ? "#FFE5B4"
            : id == "toko_kelontong" ? "#D8F3DC"
            : id == "kafe" ? "#F3D9FA"
            : id == "apartemen" ? "#E9ECEF"
            : id == "sekolah" ? "#FFE3E3"
            : id == "kampus" ? "#F8F9FA"
            : id == "pasar" ? "#FFF3BF"
            : "#FFF8DC";
        string warnaAtap = id == "warung" ? "#FF6B35"
            : id == "toko_kelontong" ? "#40916C"
            : id == "kafe" ? "#9775FA"
            : id == "apartemen" ? "#5C7AEA"
            : id == "sekolah" ? "#FA5252"
            : id == "kampus" ? "#3B5BDB"
            : id == "pasar" ? "#FAB005"
            : "#E85D04";

        Kotak(induk, "Alas", new Vector3(0f, 0.05f, 0f), new Vector3(w * 0.92f, 0.1f, d * 0.92f), "#95D5B2");
        // Fondasi batu ringan
        Kotak(induk, "Fondasi", new Vector3(0f, 0.16f, 0f), new Vector3(w * 0.76f, 0.14f, d * 0.72f), "#CED4DA");
        Kotak(induk, "Badan", new Vector3(0f, tinggi * 0.5f + 0.14f, 0f),
            new Vector3(w * 0.72f, tinggi, d * 0.68f), warnaDinding);
        // Lis putih di bawah atap (gaya kartun)
        Kotak(induk, "AtapLis", new Vector3(0f, tinggi + 0.16f, 0f),
            new Vector3(w * 0.84f, 0.1f, d * 0.8f), "#FFFFFF");
        // Atap bertingkat kartun (terlihat seperti atap limas halus)
        Kotak(induk, "Atap", new Vector3(0f, tinggi + 0.28f, 0f),
            new Vector3(w * 0.78f, 0.18f, d * 0.74f), warnaAtap);
        Kotak(induk, "AtapPuncak", new Vector3(0f, tinggi + 0.44f, 0f),
            new Vector3(w * 0.52f, 0.16f, d * 0.48f), warnaAtap);
        Kotak(induk, "AtapKecil", new Vector3(0f, tinggi + 0.56f, 0f),
            new Vector3(w * 0.26f, 0.12f, d * 0.24f), warnaAtap);

        // Cerobong asap kartun untuk hunian
        if (id == "rumah" || id == "kost" || id == "perumahan")
        {
            Kotak(induk, "AtapCerobong", new Vector3(w * 0.2f, tinggi + 0.58f, d * 0.15f),
                new Vector3(0.24f, 0.45f, 0.24f), "#D9480F");
            Bola(induk, "AtapAsap1", new Vector3(w * 0.2f, tinggi + 0.92f, d * 0.15f),
                new Vector3(0.22f, 0.22f, 0.22f), "#FFFFFF");
            Bola(induk, "AtapAsap2", new Vector3(w * 0.24f, tinggi + 1.15f, d * 0.15f),
                new Vector3(0.16f, 0.16f, 0.16f), "#F8F9FA");
        }

        // Kanopi bergaris untuk komersial (Warung, Toko, Kafe, Pasar)
        if (id == "warung" || id == "toko_kelontong" || id == "kafe" || id == "pasar")
        {
            Kotak(induk, "Kanopi1", new Vector3(-w * 0.16f, 0.72f, -d * 0.4f),
                new Vector3(w * 0.32f, 0.1f, 0.35f), warnaAtap);
            Kotak(induk, "Kanopi2", new Vector3(w * 0.16f, 0.72f, -d * 0.4f),
                new Vector3(w * 0.32f, 0.1f, 0.35f), "#FFFFFF");
        }

        // Pintu kayu + gagang emas + jendela kuning hangat kartun
        Kotak(induk, "Pintu", new Vector3(0f, 0.42f, -d * 0.35f),
            new Vector3(0.4f, 0.56f, 0.08f), "#8D5524");
        Bola(induk, "Gagang", new Vector3(0.12f, 0.42f, -d * 0.39f),
            new Vector3(0.07f, 0.07f, 0.07f), "#FFD166");
        Kotak(induk, "Jendela", new Vector3(-w * 0.22f, tinggi * 0.58f, -d * 0.35f),
            new Vector3(0.36f, 0.32f, 0.08f), "#FFE066");
        Kotak(induk, "Jendela2", new Vector3(w * 0.22f, tinggi * 0.58f, -d * 0.35f),
            new Vector3(0.36f, 0.32f, 0.08f), "#FFE066");
    }

    private static void Pohon(Transform induk, Vector3 pos, float skala)
    {
        Silinder(induk, "Batang", pos + new Vector3(0f, 0.25f * skala, 0f),
            new Vector3(0.18f * skala, 0.25f * skala, 0.18f * skala), "#744210");
        Bola(induk, "Daun1", pos + new Vector3(0f, 0.62f * skala, 0f),
            new Vector3(0.65f * skala, 0.55f * skala, 0.65f * skala), "#38A169");
        Bola(induk, "Daun2", pos + new Vector3(0f, 0.9f * skala, 0f),
            new Vector3(0.45f * skala, 0.42f * skala, 0.45f * skala), "#48BB78");
    }
}
