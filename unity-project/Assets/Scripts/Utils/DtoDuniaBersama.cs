// Dunia Kita — Utils/DtoDuniaBersama
// DTO JSON ServerManager (Fase 7): nama field = nama kunci JSON
// yang dikembalikan server (server/db.py + server/server.py).
// Diisi JsonUtility.FromJson — field public supaya tidak ada
// warning kompilasi. Dipisah dari ServerManager supaya berkas
// inti < 600 baris.

using System;

[Serializable]
public class ResponMasuk
{
    public bool ok;
    public string kesalahan;
    public string token;
    public int id;
    public string nama;
    public long koin;
    public int permata;
    public int reputasi;
    public int level;
    public int tunggakan;
    public TanahDto[] tanah;
    public long pemasukan_offline;
    public int hari_offline;
}

[Serializable]
public class ResponPemain
{
    public bool ok;
    public string kesalahan;
    public PemainDto pemain;
}

[Serializable]
public class PemainDto
{
    public int id;
    public string nama;
    public long koin;
    public int permata;
    public int reputasi;
    public int level;
    public int tunggakan;
    public TanahDto[] tanah;
}

[Serializable]
public class TanahDto
{
    public int x;
    public int y;
    public string bangunan;
}

[Serializable]
public class ResponDunia
{
    public bool ok;
    public string kesalahan;
    public int distrik_id;
    public string nama_distrik;
    public int ukuran;
    public BangunanDto[] bangunan;
    public PetakDto[] tanah;
    public long kas_kota;
    public int jumlah_pemain;
    public PemainDto profil;
    public WargaRingkasDto warga;
}

[Serializable]
public class WargaRingkasDto
{
    public int jumlah;
    public int penyewa;
}

[Serializable]
public class WargaDto
{
    public int id;
    public string nama;
    public long uang;
    public int hunian_x;
    public int hunian_y;
    public int pemilik_id;
    public int toko_x;
    public int toko_y;
    public string pemilik;
}

[Serializable]
public class ResponWarga
{
    public bool ok;
    public string kesalahan;
    public WargaDto[] warga;
}

[Serializable]
public class DistrikDto
{
    public int id;
    public string nama;
    public string deskripsi;
    public int jumlah_pemain;
    public int jumlah_bangunan;
}

[Serializable]
public class ResponDaftarDistrik
{
    public bool ok;
    public string kesalahan;
    public int distrik_aktif;
    public DistrikDto[] distrik;
}

[Serializable]
public class ResponPindahDistrik
{
    public bool ok;
    public string kesalahan;
    public int distrik_id;
    public string nama;
}

[Serializable]
public class BangunanDto
{
    public int x;
    public int y;
    public string id;
    public string pemilik;
    public int level;
}

[Serializable]
public class ResponUpgrade
{
    public bool ok;
    public string kesalahan;
    public int x;
    public int y;
    public int level;
    public long koin;
}

[Serializable]
public class ResponPindah
{
    public bool ok;
    public string kesalahan;
    public int x_lama;
    public int y_lama;
    public int x;
    public int y;
    public int level;
}

[Serializable]
public class PetakDto
{
    public int x;
    public int y;
    public string pemilik;
}

[Serializable]
public class ResponKeputusan
{
    public bool ok;
    public string kesalahan;
    public int izin_id;
    public KeputusanDto keputusan;
}

[Serializable]
public class KeputusanDto
{
    public string hasil;
    public string alasan;
    public int harga_tanah;
    public int biaya_izin;
    public int total;
    public string syarat;
}

[Serializable]
public class ResponImpor
{
    public bool ok;
    public string kesalahan;
    public int diimpor;
    public int dilewati;
}

[Serializable]
public class ResponAksi
{
    public bool ok;
    public string kesalahan;
    public long koin;
    public int x;
    public int y;
    public string bangunan;
    public long harga_jual;
    public int total;
}

[Serializable]
public class ResponKas
{
    public bool ok;
    public string kesalahan;
    public long saldo;
    public RiwayatDto[] riwayat;
}

[Serializable]
public class RiwayatDto
{
    public string jenis;
    public long jumlah;
    public string keterangan;
    public long waktu;
}

[Serializable]
public class PeristiwaDuniaDto
{
    public int id;
    public string jenis;
    public string judul;
    public string deskripsi;
    public int target_x;
    public int target_y;
    public string status;
    public long dibuat;
}

[Serializable]
public class ResponPeristiwa
{
    public bool ok;
    public string kesalahan;
    public PeristiwaDuniaDto[] peristiwa;
}

[Serializable]
public class ResponTindakPeristiwa
{
    public bool ok;
    public string kesalahan;
    public int id;
    public string tujuan;
    public string pesan;
    public long koin;
    public int reputasi;
}

[Serializable]
public class ResponSerobot
{
    public bool ok;
    public string kesalahan;
    public int x;
    public int y;
    public long koin;
    public int reputasi;
}

[Serializable]
public class PesanChatDto
{
    public int id;
    public string nama;
    public string isi;
    public long dibuat;
}

[Serializable]
public class ResponChat
{
    public bool ok;
    public string kesalahan;
    public PesanChatDto[] pesan;
}

[Serializable]
public class PeringkatDto
{
    public string nama;
    public long koin;
    public int reputasi;
    public int bangunan;
}

[Serializable]
public class ResponLeaderboard
{
    public bool ok;
    public string kesalahan;
    public PeringkatDto[] peringkat;
}
