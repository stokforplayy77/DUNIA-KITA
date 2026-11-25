// Dunia Kita — PenghuniInterior
// Menggerakkan warga penghuni di dalam mode interior tembus pandang
// antar perabot yang terpasang di 4 ruangan.

using System.Collections.Generic;
using UnityEngine;

public class PenghuniInterior : MonoBehaviour
{
    private List<Vector2> titikTujuan;
    private Vector2 tujuanSekarang;
    private List<Vector3> titikTujuan3D;
    private Vector3 tujuanSekarang3D;
    private bool mode3D;
    private float sisaDiam;
    private float waktuJalan;
    private SpriteRenderer sr;
    private System.Random acak;
    private List<string> daftarAktivitas;
    private TextMesh gelembungAktivitas;

    public void Atur(List<Vector2> titik, System.Random acakBersama, List<string> aktivitas = null)
    {
        mode3D = false;
        titikTujuan = titik;
        daftarAktivitas = aktivitas;
        acak = acakBersama != null ? acakBersama : new System.Random();
        sr = GetComponent<SpriteRenderer>();
        SiapkanGelembung();
        PilihTujuanBerikutnya();
    }

    private void SiapkanGelembung()
    {
        if (gelembungAktivitas != null) return;
        GameObject go = new GameObject("AktivitasWarga");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        gelembungAktivitas = go.AddComponent<TextMesh>();
        gelembungAktivitas.fontSize = 22;
        gelembungAktivitas.characterSize = 0.06f;
        gelembungAktivitas.anchor = TextAnchor.MiddleCenter;
        gelembungAktivitas.color = new Color(1f, 0.98f, 0.8f, 1f);
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null && sr != null) mr.sortingOrder = sr.sortingOrder + 3;
    }

    public void Atur3D(List<Vector3> titik, System.Random acakBersama)
    {
        mode3D = true;
        titikTujuan3D = titik;
        acak = acakBersama != null ? acakBersama : new System.Random();
        PilihTujuanBerikutnya();
    }

    private void Update()
    {
        if (mode3D)
        {
            Update3D();
            return;
        }
        if (titikTujuan == null || titikTujuan.Count == 0)
        {
            return;
        }
        if (sisaDiam > 0f)
        {
            sisaDiam -= Time.deltaTime;
            if (sisaDiam <= 0f)
            {
                PilihTujuanBerikutnya();
            }
            return;
        }
        Vector2 pos = new Vector2(transform.localPosition.x, transform.localPosition.y);
        Vector2 selisih = tujuanSekarang - pos;
        float jarak = selisih.magnitude;
        float langkah = 0.35f * Time.deltaTime;
        if (jarak <= langkah)
        {
            transform.localPosition = new Vector3(tujuanSekarang.x, tujuanSekarang.y, 0f);
            sisaDiam = 1.6f + (float)acak.NextDouble() * 2.2f;
            if (gelembungAktivitas != null && daftarAktivitas != null && daftarAktivitas.Count > 0)
            {
                gelembungAktivitas.text = daftarAktivitas[acak.Next(0, daftarAktivitas.Count)];
            }
        }
        else
        {
            Vector2 arah = selisih * (1f / jarak);
            if (sr != null && Mathf.Abs(arah.x) > 0.01f)
            {
                sr.flipX = arah.x < 0f;
            }
            waktuJalan += Time.deltaTime;
            float ayun = Mathf.Sin(waktuJalan * 10f) * 0.015f;
            Vector2 baru = pos + arah * langkah;
            transform.localPosition = new Vector3(baru.x, baru.y + ayun, 0f);
        }
    }

    private void Update3D()
    {
        if (titikTujuan3D == null || titikTujuan3D.Count == 0) return;
        if (sisaDiam > 0f)
        {
            sisaDiam -= Time.deltaTime;
            if (sisaDiam <= 0f) PilihTujuanBerikutnya();
            return;
        }
        Vector3 pos = transform.localPosition;
        Vector3 selisih = new Vector3(tujuanSekarang3D.x - pos.x, 0f, tujuanSekarang3D.z - pos.z);
        float jarak = (float)System.Math.Sqrt(selisih.x * selisih.x + selisih.z * selisih.z);
        float langkah = 0.45f * Time.deltaTime;
        if (jarak <= langkah)
        {
            transform.localPosition = tujuanSekarang3D;
            sisaDiam = 1.5f + (float)acak.NextDouble() * 2.5f;
        }
        else
        {
            Vector3 arah = selisih * (1f / jarak);
            waktuJalan += Time.deltaTime;
            float ayun = Mathf.Sin(waktuJalan * 10f) * 0.02f;
            transform.localPosition = new Vector3(
                pos.x + arah.x * langkah,
                tujuanSekarang3D.y + ayun,
                pos.z + arah.z * langkah);
        }
    }

    private void PilihTujuanBerikutnya()
    {
        if (mode3D)
        {
            if (titikTujuan3D == null || titikTujuan3D.Count == 0) return;
            tujuanSekarang3D = titikTujuan3D[acak.Next(0, titikTujuan3D.Count)];
            return;
        }
        if (titikTujuan == null || titikTujuan.Count == 0) return;
        tujuanSekarang = titikTujuan[acak.Next(0, titikTujuan.Count)];
    }
}
