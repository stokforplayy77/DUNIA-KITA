// STUB API Unity — HANYA untuk compile-check di luar Unity (mcs).
// BUKAN bagian dari project game. Berisi signature minimal dari API
// Unity yang dipakai script Dunia Kita, dengan nama & tipe yang sama
// seperti Unity 2022 LTS. Bila script lolos kompilasi terhadap stub
// ini, sintaks & pemakaian API-nya konsisten; perilaku runtime tetap
// harus dites di Unity Editor (lihat CATATAN-VERIFIKASI.md).

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static T Instantiate<T>(T original) where T : Object { return original; }
        public static void Destroy(Object obj) { }
        public static T FindObjectOfType<T>() where T : Object { return null; }
    }

    public class Component : Object
    {
        public GameObject gameObject;
        // Di Unity asli, transform tidak pernah null. Stub memakai
        // properti lazy supaya new AudioManager().Siapkan() (yang
        // memanggil transform.SetParent) bisa dites tanpa Editor.
        // (Lazy — bukan inisialisasi di konstruktor — supaya tidak
        // rekursi: Transform sendiri turunan Component.)
        private Transform _transform;
        public Transform transform
        {
            get
            {
                if (_transform == null)
                {
                    _transform = new Transform();
                }
                return _transform;
            }
            set { _transform = value; }
        }
        public T GetComponent<T>() where T : Component { return null; }
        public T GetComponentInChildren<T>() where T : Component { return null; }
        public T AddComponent<T>() where T : Component, new() { return new T(); }
    }

    public class Behaviour : Component
    {
        public bool enabled;
    }

    public class MonoBehaviour : Behaviour
    {
        // FASE 7: coroutine (ServerManager memakai UnityWebRequest
        // lewat coroutine; di stub tidak ada eksekusi nyata).
        public Coroutine StartCoroutine(IEnumerator routine) { return null; }
        public void StopAllCoroutines() { }
        public void StopCoroutine(Coroutine routine) { }
    }

    public class Coroutine : Object { }

    public class ScriptableObject : Object { }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string nama) { name = nama; }
        public static GameObject Find(string nama) { return null; }
        public static GameObject CreatePrimitive(PrimitiveType jenis) { return new GameObject(); }
        private Transform _transform;
        public Transform transform
        {
            get
            {
                if (_transform == null)
                {
                    _transform = new Transform();
                }
                return _transform;
            }
            set { _transform = value; }
        }
        public T AddComponent<T>() where T : Component, new() { return new T(); }
        public T GetComponent<T>() where T : Component { return null; }
        public void SetActive(bool nilai) { }
        // FASE 7: activeSelf (Unity asli punya; dipakai PanelIzinTanah).
        public bool activeSelf { get { return true; } }
    }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position;
        public Vector3 localPosition;
        public Vector3 localScale = Vector3.one;
        public Quaternion rotation;
        public Quaternion localRotation;
        public int childCount { get { return 0; } }
        public Transform GetChild(int i) { return null; }
        public void SetParent(Transform induk) { }
        public void SetParent(Transform induk, bool tetapPosisiDunia) { }
        public Transform Find(string nama) { return null; }
        public IEnumerator GetEnumerator() { yield break; }
    }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } }
        public static float Distance(Vector2 a, Vector2 b)
        { return (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y)); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator *(Vector2 a, float d) { return new Vector2(a.x * d, a.y * d); }
    }

    public struct Vector2Int
    {
        public int x, y;
        public Vector2Int(int x, int y) { this.x = x; this.y = y; }
        public static Vector2Int one { get { return new Vector2Int(1, 1); } }
        public static Vector2Int zero { get { return new Vector2Int(0, 0); } }
        public static bool operator ==(Vector2Int a, Vector2Int b) { return a.x == b.x && a.y == b.y; }
        public static bool operator !=(Vector2Int a, Vector2Int b) { return !(a == b); }
        public override bool Equals(object o) { return o is Vector2Int v && this == v; }
        public override int GetHashCode() { return x * 31 + y; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
        public static Vector3 up { get { return new Vector3(0, 1, 0); } }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator *(Vector3 a, float d) { return new Vector3(a.x * d, a.y * d, a.z * d); }
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public class Renderer : Component
    {
        public Material sharedMaterial;
        public bool enabled;
    }

    public class Shader : Object
    {
        public static Shader Find(string nama) { return new Shader(); }
    }

    public class Material : Object
    {
        public Color color;
        public bool enableInstancing;
        public Material(Shader s) { }
        public bool HasProperty(string nama) { return false; }
        public void SetFloat(string nama, float v) { }
    }

    public enum LightType { Spot, Directional, Point, Area, Rectangle, Disc }

    public class Light : Behaviour
    {
        public LightType type;
        public float intensity;
        public Color color;
    }

    public struct Ray
    {
        public Vector3 origin, direction;
        public Vector3 GetPoint(float d) { return origin + direction * d; }
    }

    public struct Plane
    {
        public Plane(Vector3 n, Vector3 p) { }
        public bool Raycast(Ray r, out float enter) { enter = 0f; return true; }
    }

    public enum KeyCode { Q, E }

    public struct Quaternion
    {
        public float x, y, z, w;
        public static Quaternion identity { get { return new Quaternion(); } }
        public static Quaternion Euler(float x, float y, float z) { return new Quaternion(); }
        public static Vector3 operator *(Quaternion q, Vector3 v) { return v; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1f; }
        public static Color white { get { return new Color(1, 1, 1, 1); } }
        public static Color clear { get { return new Color(0, 0, 0, 0); } }
        public static Color HSVToRGB(float H, float S, float V) { return new Color(V, V, V, 1f); }
        public static Color operator *(Color a, Color b) { return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a); }
    }

    public static class ColorUtility
    {
        public static bool TryParseHtmlString(string html, out Color warna)
        { warna = Color.white; return true; }
    }

    public class Camera : Behaviour
    {
        public static Camera main { get { return null; } }
        public bool orthographic;
        public float orthographicSize;
        public float fieldOfView;
        public Color backgroundColor;
        public float aspect = 0.5625f;
        public new Transform transform;
        public Vector3 ScreenToWorldPoint(Vector3 posisi) { return posisi; }
        public Vector3 WorldToScreenPoint(Vector3 posisi) { return posisi; }
        public Ray ScreenPointToRay(Vector2 posisi) { return new Ray(); }
    }

    public struct Vector4
    {
        public float x, y, z, w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public class Sprite : Object
    {
        public static Sprite Create(Texture2D tex, Rect rect, Vector2 pivot, float ppu) { return new Sprite(); }
        public static Sprite Create(Texture2D tex, Rect rect, Vector2 pivot, float ppu, uint extrude, SpriteMeshType meshType, Vector4 border) { return new Sprite(); }
    }

    public enum SpriteMeshType { FullRect, Tight }

    public class SpriteRenderer : Component
    {
        public Sprite sprite;
        public Color color;
        public int sortingOrder;
        public bool flipX;
        public bool enabled;
    }

    public enum FilterMode { Point, Bilinear, Trilinear }

    public class Texture2D : Object
    {
        public FilterMode filterMode;
        public Texture2D(int w, int h) { }
        public void SetPixels(Color[] colors) { }
        public void Apply() { }
    }

    public enum TextAnchor
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight
    }

    public class TextMesh : Component
    {
        public string text;
        public int fontSize;
        public float characterSize;
        public TextAnchor anchor;
        public Color color;
    }

    public class MeshRenderer : Component
    {
        public int sortingOrder;
    }

    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
    }

    public class Font : Object { }

    public static class Resources
    {
        public static T GetBuiltinResource<T>(string path) where T : Object { return null; }
        public static T Load<T>(string path) where T : Object { return null; }
    }

    public static class JsonUtility
    {
        public static string ToJson(object obj, bool pretty) { return "{}"; }
        public static T FromJson<T>(string json) { return default(T); }
    }

    public static class Application
    {
        public static string persistentDataPath { get { return "/tmp"; } }
        // Target frame rate (Fase 5: 60 fps, vSync mati di QualitySettings).
        public static int targetFrameRate { get; set; }
    }

    // Preferensi kecil (pengaturan suara, hadiah misi yang sudah diklaim).
    // FASE 5: stub kini benar-benar menyimpan nilai (kamus di memori)
    // supaya tes tutorial & audio bisa menguji putaran
    // SetInt -> GetInt seperti PlayerPrefs asli.
    // FASE 7: ditambah GetString/SetString (token + nama pemain
    // online + alamat server) dengan kamus yang sama polanya.
    public static class PlayerPrefs
    {
        private static Dictionary<string, int> simpanInt =
            new Dictionary<string, int>();
        private static Dictionary<string, string> simpanString =
            new Dictionary<string, string>();
        public static int GetInt(string kunci, int nilaiAwal)
        {
            int nilai;
            if (simpanInt.TryGetValue(kunci, out nilai))
            {
                return nilai;
            }
            return nilaiAwal;
        }
        public static void SetInt(string kunci, int nilai)
        {
            simpanInt[kunci] = nilai;
        }
        public static string GetString(string kunci, string nilaiAwal)
        {
            string nilai;
            if (simpanString.TryGetValue(kunci, out nilai))
            {
                return nilai;
            }
            return nilaiAwal;
        }
        public static string GetString(string kunci)
        {
            return GetString(kunci, "");
        }
        public static void SetString(string kunci, string nilai)
        {
            simpanString[kunci] = nilai;
        }
        public static void DeleteKey(string kunci)
        {
            simpanInt.Remove(kunci);
            simpanString.Remove(kunci);
        }
        public static bool HasKey(string kunci)
        {
            return simpanInt.ContainsKey(kunci)
                || simpanString.ContainsKey(kunci);
        }
        public static void Save() { }
        // Untuk tes: kosongkan semua preferensi (awal bersih).
        public static void HapusSemuaUntukTes()
        {
            simpanInt.Clear();
            simpanString.Clear();
        }
    }

    public class AudioListener : Behaviour
    {
        public static float volume { get; set; }
    }

    public class AudioClip : Object { }

    public class AudioSource : Behaviour
    {
        public AudioClip clip;
        public bool loop;
        public float volume;
        public float pitch = 1f;
        public bool isPlaying { get; private set; }
        public void Play() { isPlaying = true; }
        public void Stop() { isPlaying = false; }
        public void PlayOneShot(AudioClip klip) { }
    }

    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }

    public struct Touch
    {
        public int fingerId;
        public Vector2 position;
        public Vector2 deltaPosition;
        public TouchPhase phase;
    }

    public static class Input
    {
        public static int touchCount { get { return 0; } }
        public static Touch GetTouch(int i) { return new Touch(); }
        public static bool GetMouseButtonDown(int b) { return false; }
        public static bool GetMouseButton(int b) { return false; }
        public static bool GetMouseButtonUp(int b) { return false; }
        public static Vector2 mousePosition { get { return Vector2.zero; } }
        public static float GetAxis(string nama) { return 0f; }
        public static bool GetKey(KeyCode k) { return false; }
    }

    public static class Screen
    {
        public static int height { get { return 1920; } }
        public static int width { get { return 1080; } }
        public static Rect safeArea { get { return new Rect(0, 0, width, height); } }
    }

    public static class Time
    {
        public static float deltaTime { get { return 0.016f; } }
        public static float time { get { return 0f; } }
    }

    public static class Random
    {
        // Nilai acak [0, 1) — di stub selalu 0,5 (deterministik);
        // di Unity asli memakai generator acak Unity.
        public static float value { get { return 0.5f; } }
        public static int Range(int min, int maks) { return min; }
        public static float Range(float min, float maks) { return min; }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265f;
        public const float Deg2Rad = 0.017453292f;
        public static float Cos(float v) { return (float)Math.Cos(v); }
        public static float Clamp(float v, float min, float maks)
        { return v < min ? min : (v > maks ? maks : v); }
        public static int Clamp(int v, int min, int maks)
        { return v < min ? min : (v > maks ? maks : v); }
        public static float Clamp01(float v) { return Clamp(v, 0f, 1f); }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
        public static int RoundToInt(float v) { return (int)Math.Round(v); }
        public static float Sin(float v) { return (float)Math.Sin(v); }
        public static float Abs(float v) { return v < 0 ? -v : v; }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Abs(target - current) <= maxDelta) return target;
            return current + (target > current ? maxDelta : -maxDelta);
        }
    }

    public static class Debug
    {
        public static void Log(object pesan) { }
        public static void LogWarning(object pesan) { }
    }

    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string teks) { }
    }

    public class TextAreaAttribute : Attribute { }

    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
    }

    public class SerializeFieldAttribute : Attribute { }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public class UnityEvent
    {
        public void AddListener(UnityAction call) { }
        public void Invoke() { }
    }
}

namespace UnityEngine.UI
{
    using UnityEngine;
    using UnityEngine.Events;

    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }

    public class Canvas : Behaviour
    {
        public RenderMode renderMode;
        public int sortingOrder;
    }

    public class CanvasScaler : Behaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public ScaleMode uiScaleMode;
        public Vector2 referenceResolution;
        public float matchWidthOrHeight;
    }

    public class GraphicRaycaster : Behaviour { }

    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }

    public class Text : Behaviour
    {
        public Font font;
        public string text;
        public int fontSize;
        public Color color;
        public TextAnchor alignment;
        public HorizontalWrapMode horizontalOverflow;
        public VerticalWrapMode verticalOverflow;
        public FontStyle fontStyle;
        public bool raycastTarget;
    }

    public class Shadow : Behaviour
    {
        public Color effectColor;
        public Vector2 effectDistance;
    }

    public class Outline : Behaviour
    {
        public Color effectColor;
        public Vector2 effectDistance;
    }

    public class Image : Behaviour
    {
        public enum Type { Simple, Sliced, Tiled, Filled }
        public Color color;
        public Sprite sprite;
        public bool preserveAspect;
        public Type type;
    }

    public struct ColorBlock
    {
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public Color disabledColor;
        public float colorMultiplier;
        public float fadeDuration;
        public static ColorBlock defaultColorBlock { get { return new ColorBlock(); } }
    }

    public class Button : Behaviour
    {
        public class ButtonClickedEvent : UnityEvent { }
        public ButtonClickedEvent onClick = new ButtonClickedEvent();
        public ColorBlock colors;
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 offsetMin;
        public Vector2 offsetMax;
    }

    public class RectMask2D : Behaviour { }

    public class ScrollRect : Behaviour
    {
        public RectTransform content;
        public RectTransform viewport;
        public bool horizontal;
        public bool vertical;
    }

    // FASE 7: kolom teks (PanelAkun: nama pemain, kata sandi, alamat
    // server). Minimal: isi teks, jenis konten (kata sandi berbintang),
    // dan teks penanda (placeholder). Di Editor, komponen ini butuh
    // Text penanda + Text isi yang dihubungkan di Inspector; UiFactory
    // (lihat BuatInput) membuat keduanya bersamaan.
    public class InputField : Behaviour
    {
        public enum ContentType
        {
            Standard, Autocorrected, IntegerNumber, DecimalNumber,
            Alphanumeric, Name, EmailAddress, Password, Pin, Custom
        }
        public string text;
        public ContentType contentType;
        public Text placeholder;
        public Text textComponent;
    }
}

namespace UnityEngine.EventSystems
{
    using UnityEngine;

    public class EventSystem : MonoBehaviour
    {
        public static EventSystem current;
        public bool IsPointerOverGameObject() { return false; }
        public bool IsPointerOverGameObject(int fingerId) { return false; }
    }

    public class StandaloneInputModule : MonoBehaviour { }
}

// FASE 7: stub minimal UnityWebRequest (ServerManager). Di Unity asli
// namespace ini memakai coroutine + objek native; stub ini hanya
// memberi signature supaya kode klien bisa dikompilasi & dites
// logikanya (JSON & alur) di luar Editor. Request nyata hanya jalan
// di Unity Editor / build Android.
namespace UnityEngine.Networking
{
    using System.Collections;

    public class DownloadHandler
    {
        public string text;
        public byte[] data;
    }

    public class DownloadHandlerBuffer : DownloadHandler { }

    public class UploadHandlerRaw : System.IDisposable
    {
        public UploadHandlerRaw(byte[] data) { }
        public void Dispose() { }
    }

    public class UnityWebRequest : System.IDisposable
    {
        public enum Result { InProgress, Success, ConnectionError, ProtocolError, DataProcessingError }

        public DownloadHandler downloadHandler;
        public UploadHandlerRaw uploadHandler;
        public int timeout;
        public bool isDone;
        public Result result;
        public string error;
        public long responseCode;

        private UnityWebRequest(string url) { }

        // Konstruktor POST JSON (cara asli Unity: new UnityWebRequest
        // (url, "POST") + UploadHandlerRaw + DownloadHandlerBuffer).
        public UnityWebRequest(string url, string metode) { }

        public static UnityWebRequest Get(string url)
        {
            return new UnityWebRequest(url);
        }

        public void SetRequestHeader(string nama, string nilai) { }
        public IEnumerator SendWebRequest() { yield return null; }
        public void Dispose() { }
    }
}
