// Dunia Kita — PartikelAsap
// Animasi kepulan asap/uap ringan yang melayang naik dan memudar
// di atas rumah, kost, warung, kafe, dan pembangkit listrik.

using UnityEngine;

public class PartikelAsap : MonoBehaviour
{
    private float umur;
    private const float Durasi = 2.0f;
    private SpriteRenderer sr;
    private float arahX;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        arahX = Random.Range(-0.12f, 0.18f);
    }

    private void Update()
    {
        umur += Time.deltaTime;
        float t = Mathf.Clamp01(umur / Durasi);
        transform.localPosition += new Vector3(arahX, 0.55f, 0f) * Time.deltaTime;
        float skala = Mathf.Lerp(0.45f, 1.35f, t);
        transform.localScale = new Vector3(skala, skala, 1f);
        if (sr != null)
        {
            sr.color = new Color(1f, 1f, 1f, (1f - t) * 0.75f);
        }
        if (umur >= Durasi)
        {
            Destroy(gameObject);
        }
    }
}
