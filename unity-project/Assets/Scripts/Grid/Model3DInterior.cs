// Dunia Kita — Model3DInterior
// Membangun sekat 4 ruangan (Kamar Tidur, Dapur, Kamar Mandi, Ruang Tamu),
// perabot 3D Low-Poly (10 slot), dan warga penghuni 3D di dalam hunian
// saat mode interior tembus pandang dibuka.

using System.Collections.Generic;
using UnityEngine;

public static class Model3DInterior
{
    private static GameObject Kotak(Transform induk, string nama, Vector3 pos,
        Vector3 skala, string hex)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nama;
        go.transform.SetParent(induk, false);
        go.transform.localPosition = pos;
        go.transform.localScale = skala;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = Model3DBangunan.Mat(hex);
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
        if (r != null) r.sharedMaterial = Model3DBangunan.Mat(hex);
        return go;
    }

    // Sekat 4 ruangan + lantai kayu hangat di dalam bangunan.
    public static void BangunRuangan3D(BuildingData data, Transform induk)
    {
        if (data == null || induk == null) return;
        float w = data.ukuran.x * GameBalance.LebarTile * 0.72f;
        float d = data.ukuran.y * GameBalance.LebarTile * 0.68f;

        GameObject wadah = new GameObject("Ruangan3D");
        wadah.transform.SetParent(induk, false);

        // Lantai kayu
        Kotak(wadah.transform, "Lantai", new Vector3(0f, 0.12f, 0f),
            new Vector3(w, 0.04f, d), "#E6CCB2");
        // Dinding luar rendah (potongan x-ray)
        Kotak(wadah.transform, "DindingBelakang", new Vector3(0f, 0.34f, d * 0.48f),
            new Vector3(w, 0.4f, 0.06f), "#F5EBE0");
        Kotak(wadah.transform, "DindingKiri", new Vector3(-w * 0.48f, 0.34f, 0f),
            new Vector3(0.06f, 0.4f, d), "#EDE0D4");
        // Sekat silang 4 ruangan
        Kotak(wadah.transform, "SekatX", new Vector3(0f, 0.3f, 0f),
            new Vector3(w * 0.94f, 0.32f, 0.05f), "#DDB892");
        Kotak(wadah.transform, "SekatZ", new Vector3(0f, 0.3f, 0f),
            new Vector3(0.05f, 0.32f, d * 0.94f), "#DDB892");
    }

    public static Vector3 PosisiSlot3D(BuildingData data, int slot)
    {
        float w = data.ukuran.x * GameBalance.LebarTile * 0.72f;
        float d = data.ukuran.y * GameBalance.LebarTile * 0.68f;
        float qx = w * 0.24f;
        float qz = d * 0.24f;
        switch (slot)
        {
            case 0: return new Vector3(-qx - 0.15f, 0.14f, qz);       // Kasur (Kamar Tidur)
            case 1: return new Vector3(-qx + 0.22f, 0.14f, qz + 0.1f);// Lemari
            case 2: return new Vector3(qx - 0.18f, 0.14f, qz + 0.1f); // Kompor (Dapur)
            case 3: return new Vector3(qx + 0.12f, 0.14f, qz - 0.05f);// Meja
            case 4: return new Vector3(qx + 0.12f, 0.14f, qz - 0.28f);// Kursi
            case 5: return new Vector3(-qx - 0.12f, 0.14f, -qz);      // Bak Mandi (Kamar Mandi)
            case 6: return new Vector3(-qx + 0.22f, 0.14f, -qz + 0.1f);// Wastafel
            case 7: return new Vector3(qx + 0.15f, 0.14f, -qz + 0.12f);// TV (Ruang Tamu)
            case 8: return new Vector3(qx - 0.12f, 0.14f, -qz - 0.12f);// Sofa
            default: return new Vector3(qx + 0.25f, 0.14f, -qz - 0.15f);// Lampu
        }
    }

    public static void TambahPerabot3D(BuildingData data, Transform induk, int slot)
    {
        if (data == null || induk == null) return;
        float skala = data.ukuran.x >= 2 ? 1.25f : 0.85f;
        Vector3 pos = PosisiSlot3D(data, slot);

        GameObject go = new GameObject("InteriorAnak3D_" + slot);
        go.transform.SetParent(induk, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(skala, skala, skala);

        switch (slot)
        {
            case 0: // Kasur
                Kotak(go.transform, "Rangka", new Vector3(0f, 0.06f, 0f), new Vector3(0.45f, 0.12f, 0.3f), "#9C6644");
                Kotak(go.transform, "Seprai", new Vector3(0.04f, 0.14f, 0f), new Vector3(0.36f, 0.06f, 0.26f), "#63B3ED");
                Kotak(go.transform, "Bantal", new Vector3(-0.14f, 0.15f, 0f), new Vector3(0.1f, 0.06f, 0.2f), "#FFFFFF");
                break;
            case 1: // Lemari
                Kotak(go.transform, "Lemari", new Vector3(0f, 0.22f, 0f), new Vector3(0.24f, 0.44f, 0.18f), "#7F5539");
                break;
            case 2: // Kompor
                Kotak(go.transform, "Kompor", new Vector3(0f, 0.12f, 0f), new Vector3(0.24f, 0.24f, 0.22f), "#CBD5E0");
                Kotak(go.transform, "Tungku", new Vector3(0f, 0.25f, 0f), new Vector3(0.18f, 0.03f, 0.16f), "#2D3748");
                break;
            case 3: // Meja
                Kotak(go.transform, "PapanMeja", new Vector3(0f, 0.16f, 0f), new Vector3(0.32f, 0.05f, 0.24f), "#B08968");
                Kotak(go.transform, "KakiMeja", new Vector3(0f, 0.08f, 0f), new Vector3(0.12f, 0.14f, 0.12f), "#7F5539");
                break;
            case 4: // Kursi
                Kotak(go.transform, "Dudukan", new Vector3(0f, 0.09f, 0f), new Vector3(0.16f, 0.16f, 0.16f), "#B08968");
                break;
            case 5: // Bak Mandi
                Kotak(go.transform, "Bak", new Vector3(0f, 0.1f, 0f), new Vector3(0.38f, 0.2f, 0.22f), "#FFFFFF");
                Kotak(go.transform, "Air", new Vector3(0f, 0.18f, 0f), new Vector3(0.3f, 0.06f, 0.15f), "#90CDF4");
                break;
            case 6: // Wastafel
                Kotak(go.transform, "Wastafel", new Vector3(0f, 0.16f, 0f), new Vector3(0.18f, 0.26f, 0.16f), "#EDF2F7");
                break;
            case 7: // TV
                Kotak(go.transform, "MejaTV", new Vector3(0f, 0.08f, 0f), new Vector3(0.32f, 0.14f, 0.14f), "#7F5539");
                Kotak(go.transform, "LayarTV", new Vector3(0f, 0.24f, 0f), new Vector3(0.28f, 0.18f, 0.04f), "#1A202C");
                break;
            case 8: // Sofa
                Kotak(go.transform, "Sofa", new Vector3(0f, 0.1f, 0f), new Vector3(0.38f, 0.18f, 0.2f), "#E53E3E");
                Kotak(go.transform, "Sandaran", new Vector3(0f, 0.2f, 0.07f), new Vector3(0.38f, 0.16f, 0.06f), "#C53030");
                break;
            default: // Lampu
                Silinder(go.transform, "Tiang", new Vector3(0f, 0.16f, 0f), new Vector3(0.05f, 0.16f, 0.05f), "#4A5568");
                Silinder(go.transform, "Kap", new Vector3(0f, 0.34f, 0f), new Vector3(0.15f, 0.08f, 0.15f), "#ECC94B");
                break;
        }
    }

    public static GameObject TambahWarga3D(Transform induk, Vector3 posAwal, int indeks)
    {
        if (induk == null) return null;
        string[] warnaBaju = { "#3182CE", "#E53E3E", "#38A169", "#D69E2E" };
        GameObject go = new GameObject("InteriorWarga3D_" + indeks);
        go.transform.SetParent(induk, false);
        go.transform.localPosition = posAwal;

        GameObject badan = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        badan.name = "Badan";
        badan.transform.SetParent(go.transform, false);
        badan.transform.localPosition = new Vector3(0f, 0.16f, 0f);
        badan.transform.localScale = new Vector3(0.16f, 0.14f, 0.16f);
        Renderer rb = badan.GetComponent<Renderer>();
        if (rb != null) rb.sharedMaterial = Model3DBangunan.Mat(warnaBaju[indeks % warnaBaju.Length]);

        GameObject kepala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        kepala.name = "Kepala";
        kepala.transform.SetParent(go.transform, false);
        kepala.transform.localPosition = new Vector3(0f, 0.34f, 0f);
        kepala.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Renderer rk = kepala.GetComponent<Renderer>();
        if (rk != null) rk.sharedMaterial = Model3DBangunan.Mat("#FBD38D");
        return go;
    }

    public static void BersihkanInterior3D(Transform induk)
    {
        if (induk == null) return;
        for (int i = induk.childCount - 1; i >= 0; i--)
        {
            Transform anak = induk.GetChild(i);
            if (anak != null && (anak.name == "Ruangan3D"
                || anak.name.StartsWith("InteriorAnak3D_")
                || anak.name.StartsWith("InteriorWarga3D_")))
            {
                Object.Destroy(anak.gameObject);
            }
        }
    }
}
