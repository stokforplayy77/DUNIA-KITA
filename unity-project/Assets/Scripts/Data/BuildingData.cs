// Dunia Kita — BuildingData
// Definisi satu jenis bangunan sebagai ScriptableObject (GDD §5 & §13).
// Data bangunan hidup di file .asset (Assets/Data/Buildings/) sehingga
// desainer bisa mengubah harga/pemasukan tanpa menyentuh kode.

using UnityEngine;

public enum KategoriBangunan
{
    Hunian = 0,
    Komersial = 1,
    Fasilitas = 2
}

[CreateAssetMenu(fileName = "BangunanBaru", menuName = "Dunia Kita/Data Bangunan")]
public class BuildingData : ScriptableObject
{
    [Header("Identitas")]
    public string id;                 // Contoh: "rumah", "warung", "taman"
    public string nama;               // Nama tampil: "Rumah Sederhana"
    [TextArea]
    public string deskripsi;
    public KategoriBangunan kategori;

    [Header("Syarat buka")]
    public int levelBuka = 1;             // Level dunia minimum supaya
                                          // bangunan ini muncul di menu
                                          // bangun (GDD §8; 1 = sejak awal)

    [Header("Ukuran (petak)")]
    public Vector2Int ukuran = Vector2Int.one;   // Fase 1: semua 1x1

    [Header("Ekonomi (per hari game)")]
    public long biayaBangun;
    public long pemasukanHarian;
    public long biayaPerawatanHarian;

    [Header("Warga")]
    public int kapasitasWarga;        // Jumlah warga yang bisa tinggal

    [Header("Kebahagiaan")]
    public int bonusBahagia;          // Bonus kebahagiaan dunia per
                                      // bangunan ini (Fase 4: fasilitas
                                      // & taman; 0 = tidak berpengaruh).
                                      // Taman dihitung khusus lewat
                                      // jumlahTaman di HitungKebahagiaan
                                      // (+2/taman, maks +20) — jadi
                                      // bonusBahagia Taman = 0 di .asset.

    [Header("Tampilan")]
    public Sprite sprite;             // Sprite isometric placeholder
    public Color warna = Color.white;
    public float offsetVisualY = 0.4f; // Geser sprite ke atas supaya alas
                                      // bangunan menempel di tengah petak
}
