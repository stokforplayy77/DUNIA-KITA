// Dunia Kita — CameraController (3D Orbit + Pan + Zoom)
// Kamera 3D Low-Poly:
//   - Drag 1 jari / Mouse kiri: geser peta (pan) di bidang tanah XZ
//   - Drag Mouse kanan / Q & E / Geser 2 jari: putar kamera 360 derajat
//   - Cubit 2 jari / Roda mouse: zoom masuk/keluar
// Properti SedangDrag dipakai BuildManager untuk membedakan tap dari drag.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool SedangDrag { get; private set; }

    private Camera kamera;
    private IsometricGrid grid;
    private float jarakPinchSebelumnya;
    private Vector2 posisiDragSebelumnya;
    private bool sedangDragMouse;
    private bool sedangPutarMouse;
    private const float AmbangDragPiksel = 12f;

    // Parameter orbit 3D
    private Vector3 fokus = Vector3.zero;
    private float yaw = 45f;
    private float pitch = 44f;
    private float jarak = 24f;
    private const float JarakMin = 8f;
    private const float JarakMaks = 70f;

    private void Awake()
    {
        kamera = GetComponent<Camera>();
        if (kamera == null)
        {
            kamera = Camera.main;
        }
        if (GetComponent<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
    }

    private void Start()
    {
        grid = FindObjectOfType<IsometricGrid>();
        if (grid != null && grid.mode3D)
        {
            kamera.orthographic = false;
            kamera.fieldOfView = 45f;
        }
        else
        {
            kamera.orthographic = true;
            kamera.orthographicSize = GameBalance.UkuranKameraAwal;
        }
        PusatkanKe(Vector3.zero);
    }

    public void PusatkanKe(Vector3 titik)
    {
        fokus = new Vector3(titik.x, 0f, titik.z);
        TerapkanTransformasi();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            TanganiSentuhan();
        }
        else
        {
            TanganiMouse();
        }
        // Putar kamera dengan tombol Q / E di PC/Editor
        if (grid != null && grid.mode3D)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                yaw -= 60f * Time.deltaTime;
                TerapkanTransformasi();
            }
            if (Input.GetKey(KeyCode.E))
            {
                yaw += 60f * Time.deltaTime;
                TerapkanTransformasi();
            }
        }
    }

    private void TanganiSentuhan()
    {
        if (Input.touchCount == 1)
        {
            Touch sentuhan = Input.GetTouch(0);
            if (sentuhan.phase == TouchPhase.Began)
            {
                posisiDragSebelumnya = sentuhan.position;
                SedangDrag = false;
            }
            else if (sentuhan.phase == TouchPhase.Moved)
            {
                Vector2 geser = sentuhan.position - posisiDragSebelumnya;
                if (!SedangDrag && geser.magnitude > AmbangDragPiksel)
                {
                    SedangDrag = true;
                }
                if (SedangDrag)
                {
                    GeserKamera(geser);
                }
                posisiDragSebelumnya = sentuhan.position;
            }
            else if (sentuhan.phase == TouchPhase.Ended
                || sentuhan.phase == TouchPhase.Canceled)
            {
                SedangDrag = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            SedangDrag = true;
            Touch jari0 = Input.GetTouch(0);
            Touch jari1 = Input.GetTouch(1);
            float jarakSekarang = Vector2.Distance(jari0.position, jari1.position);
            if (jari0.phase == TouchPhase.Began || jari1.phase == TouchPhase.Began)
            {
                jarakPinchSebelumnya = jarakSekarang;
            }
            else if (jarakPinchSebelumnya > 0f)
            {
                float selisih = jarakSekarang - jarakPinchSebelumnya;
                ZoomKamera(-selisih * 0.03f);
                jarakPinchSebelumnya = jarakSekarang;
            }
            if (jari0.phase == TouchPhase.Ended || jari1.phase == TouchPhase.Ended)
            {
                SedangDrag = false;
            }
        }
    }

    private void TanganiMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (ApakahDiAtasUI())
            {
                sedangDragMouse = false;
                return;
            }
            sedangDragMouse = true;
            posisiDragSebelumnya = Input.mousePosition;
            SedangDrag = false;
        }
        else if (Input.GetMouseButton(0) && sedangDragMouse)
        {
            Vector2 posisi = Input.mousePosition;
            Vector2 geser = posisi - posisiDragSebelumnya;
            if (!SedangDrag && geser.magnitude > AmbangDragPiksel)
            {
                SedangDrag = true;
            }
            if (SedangDrag)
            {
                GeserKamera(geser);
            }
            posisiDragSebelumnya = posisi;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            sedangDragMouse = false;
            SedangDrag = false;
        }

        // Klik kanan drag = putar kamera 360 derajat
        if (Input.GetMouseButtonDown(1))
        {
            sedangPutarMouse = true;
            posisiDragSebelumnya = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1) && sedangPutarMouse)
        {
            Vector2 posisi = Input.mousePosition;
            Vector2 geser = posisi - posisiDragSebelumnya;
            yaw += geser.x * 0.35f;
            pitch = Mathf.Clamp(pitch - geser.y * 0.25f, 22f, 75f);
            posisiDragSebelumnya = posisi;
            TerapkanTransformasi();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            sedangPutarMouse = false;
        }

        float roda = Input.GetAxis("Mouse ScrollWheel");
        if (roda != 0f)
        {
            ZoomKamera(-roda * 8f);
        }
    }

    private bool ApakahDiAtasUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void GeserKamera(Vector2 geserPiksel)
    {
        if (grid != null && grid.mode3D)
        {
            float skala = (jarak * 0.0014f);
            float rad = yaw * Mathf.Deg2Rad;
            Vector3 kanan = new Vector3(Mathf.Cos(rad), 0f, -Mathf.Sin(rad));
            Vector3 maju = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            fokus -= kanan * (geserPiksel.x * skala);
            fokus -= maju * (geserPiksel.y * skala);
            fokus = BatasiFokus(fokus);
            TerapkanTransformasi();
            return;
        }
        float skala2D = kamera.orthographicSize * 2f / Screen.height;
        Vector3 posisi = transform.position;
        posisi.x -= geserPiksel.x * skala2D;
        posisi.y -= geserPiksel.y * skala2D;
        transform.position = posisi;
    }

    private void ZoomKamera(float delta)
    {
        if (grid != null && grid.mode3D)
        {
            jarak = Mathf.Clamp(jarak + delta, JarakMin, JarakMaks);
            TerapkanTransformasi();
            return;
        }
        kamera.orthographicSize = Mathf.Clamp(
            kamera.orthographicSize + delta * 0.25f,
            GameBalance.ZoomMin, GameBalance.ZoomMaks);
    }

    private Vector3 BatasiFokus(Vector3 pos)
    {
        if (grid == null) return pos;
        float minX, maxX, minZ, maxZ;
        grid.BatasDunia(out minX, out maxX, out minZ, out maxZ);
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        return pos;
    }

    private void TerapkanTransformasi()
    {
        if (kamera == null) return;
        if (grid != null && !grid.mode3D)
        {
            transform.position = new Vector3(fokus.x, fokus.z, -10f);
            transform.rotation = Quaternion.identity;
            return;
        }
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 arahBelakang = rot * new Vector3(0f, 0f, -jarak);
        transform.position = fokus + arahBelakang;
        transform.rotation = rot;
    }
}
