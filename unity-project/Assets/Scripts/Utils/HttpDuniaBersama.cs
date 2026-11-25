// Dunia Kita — Utils/HttpDuniaBersama
// Pembantu HTTP JSON untuk ServerManager (Fase 7): GET/POST
// UnityWebRequest lewat coroutine + normalisasi alamat + escape
// JSON. Dipisah dari ServerManager supaya berkas inti < 600 baris.
// Di Editor memakai API Unity asli; di luar Editor memakai stub
// (tools/unity-stubs.cs). Timeout tiap request 10 detik.

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class HttpDuniaBersama
{
    public static IEnumerator KirimGet(string alamatServer, string token, string jalur, Action<string> selesai)
    {
        string url = alamatServer + jalur;
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 10;
            req.SetRequestHeader("Authorization", "Bearer " + token);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                selesai(req.downloadHandler.text);
            }
            else
            {
                selesai(null);
            }
        }
    }

    public static IEnumerator KirimPost(string alamatServer, string token, string jalur, string badanJson,
        Action<string> selesai)
    {
        string url = alamatServer + jalur;
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.timeout = 10;
            req.uploadHandler = new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(badanJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + token);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success
                || req.responseCode == 400 || req.responseCode == 401)
            {
                // 400/401 membawa JSON {ok:false, kesalahan:...} —
                // tetap diteruskan supaya pesan server tampil.
                selesai(req.downloadHandler.text);
            }
            else
            {
                selesai(null);
            }
        }
    }

    public static string NormalisasiAlamat(string alamat)
    {
        if (string.IsNullOrEmpty(alamat))
        {
            return GameBalance.AlamatServerDefault;
        }
        string hasil = alamat.Trim();
        while (hasil.EndsWith("/"))
        {
            hasil = hasil.Substring(0, hasil.Length - 1);
        }
        if (hasil.Length == 0)
        {
            return GameBalance.AlamatServerDefault;
        }
        return hasil;
    }

    public static string EscapeJson(string s)
    {
        if (s == null)
        {
            return "";
        }
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

}