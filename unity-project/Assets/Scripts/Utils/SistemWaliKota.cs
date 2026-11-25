// Dunia Kita — SistemWaliKota
// Logika MURNI Wali Kota NPC (tanpa dependensi UnityEngine) supaya
// bisa dikompilasi dan diuji di luar Unity (lihat
// tools/test-fase7.cs). Wali Kota HANYA ada di Fase 7 (Dunia
// Bersama, GDD §15 + §17): tanah harus dibeli lewat dia — ajukan
// -> urus izin -> bayar harga tanah + biaya izin -> disetujui ->
// tanah jadi milik -> baru boleh bangun. Uang masuk kas kota.
//
// Cara berpikir wali kota "macam-macam" (keputusan user 24 Nov
// 2026): keputusan menimbang lokasi tanah, reputasi pemain, jenis
// bangunan yang direncanakan, kondisi kas kota, dan kepadatan
// kawasan — jadi ada penolakan, persetujuan bersyarat, dan harga
// yang berubah-ubah. Semua angka di GameBalance (blok Fase 7).
//
// DETERMINISTIK: input yang sama -> keputusan yang sama. Variasi
// kecil (-5..+5) dihitung dari hash sederhana id pemain + koordinat
// petak (BUKAN Random dan BUKAN string.GetHashCode yang berubah
// antar sesi) supaya keputusan stabil bila diajukan ulang.
//
// Server Python (server/db.py) memakai rumus YANG SAMA — bila
// mengubah rumus di sini, ubah juga di sana (dan sebaliknya).
// Server adalah otoritas akhir; kelas ini dipakai klien untuk
// menampilkan perkiraan + dipakai tes logika.

public enum HasilIzin
{
    Disetujui = 0,   // Boleh bayar harga tanah + biaya izin.
    Ditolak = 1,     // Tidak boleh — lihat Alasan.
    Bersyarat = 2    // Boleh bayar, tapi dengan syarat/harga berbeda.
}

public enum SyaratIzin
{
    TanpaSyarat = 0,        // Bayar, tanah langsung milikmu.
    JalanBersebelahan = 1,  // Wajib ada Jalan di petak bersebelahan
                            // (4 arah) sebelum bangunan boleh dipasang.
    BiayaLebihMahal = 2     // Biaya izin naik 1,5x (untuk bangunan
                            // Jalan — syarat jalan tidak masuk akal).
}

public class KeputusanIzin
{
    public HasilIzin Hasil;
    public long HargaTanah;
    public long BiayaIzin;
    public long HargaTotal;   // HargaTanah + BiayaIzin (0 bila ditolak).
    public string Alasan;     // Bahasa Indonesia, jelas.
    public SyaratIzin Syarat;
}

public static class SistemWaliKota
{
    // Harga tanah per petak dari jarak Chebyshev ke pusat peta
    // bersama (11,11): pinggir Rp200.000, tiap langkah mendekati
    // pusat +Rp100.000 (pusat Rp1.400.000).
    public static long HargaTanah(int x, int y)
    {
        int dx = x - GameBalance.PusatPetaBersama;
        if (dx < 0)
        {
            dx = -dx;
        }
        int dy = y - GameBalance.PusatPetaBersama;
        if (dy < 0)
        {
            dy = -dy;
        }
        int jarak = dx > dy ? dx : dy;
        int langkah = 12 - jarak;
        if (langkah < 0)
        {
            langkah = 0;
        }
        return GameBalance.HargaTanahPinggir
            + (long)langkah * GameBalance.HargaTanahPerLangkahPusat;
    }

    // Biaya izin = 10% harga tanah, diubah oleh reputasi, kas kota,
    // dan Balai Kota. Paling murah Rp10.000 (tidak pernah gratis).
    public static long BiayaIzin(long hargaTanah, int reputasi,
        long kasKota, bool adaBalaiKota)
    {
        long biaya = hargaTanah * GameBalance.BiayaIzinPerMil / 1000;
        if (reputasi >= GameBalance.ReputasiDiskonIzin)
        {
            biaya = biaya * (1000 - GameBalance.DiskonIzinReputasiBagusPerMil) / 1000;
        }
        else if (reputasi < GameBalance.ReputasiIzinMahal)
        {
            biaya = biaya * (1000 + GameBalance.KenaikanIzinReputasiBurukPerMil) / 1000;
        }
        if (kasKota < GameBalance.KasKotaMenipis)
        {
            biaya = biaya * (1000 + GameBalance.KenaikanIzinKasMenipisPerMil) / 1000;
        }
        else if (kasKota > GameBalance.KasKotaPenuh)
        {
            biaya = biaya * (1000 - GameBalance.DiskonIzinKasPenuhPerMil) / 1000;
        }
        if (adaBalaiKota)
        {
            biaya = biaya * (1000 - GameBalance.DiskonIzinBalaiKotaPerMil) / 1000;
        }
        if (biaya < GameBalance.BiayaIzinMinimum)
        {
            biaya = GameBalance.BiayaIzinMinimum;
        }
        return biaya;
    }

    // Hash sederhana yang stabil antar sesi (jumlah karakter x 31):
    // dipakai untuk variasi skor -5..+5.
    public static int HashStabil(string teks)
    {
        if (teks == null)
        {
            teks = "";
        }
        int h = 0;
        for (int i = 0; i < teks.Length; i++)
        {
            h = h * 31 + teks[i];
        }
        if (h < 0)
        {
            h = -h;
        }
        return h;
    }

    // Variasi skor deterministik -5..+5 dari id pemain + petak.
    public static int VariasiSkor(string idPemain, int x, int y)
    {
        int h = HashStabil(idPemain) + x * 131 + y * 17;
        int rentang = GameBalance.VariasiSkorIzin * 2 + 1; // 11
        return h % rentang - GameBalance.VariasiSkorIzin;
    }

    // Skor mentah keputusan (sebelum ambang + penolakan otomatis):
    // 50 awal; reputasi bagus +15 / sedang +5 / buruk -10;
    // hunian +10, fasilitas +15, komersial +0; tiap bangunan
    // sejenis di kawasan di atas 2 mengurangi 15.
    public static int SkorKeputusan(int reputasi, KategoriBangunan kategori,
        int jumlahSejenisKawasan, string idPemain, int x, int y)
    {
        int skor = GameBalance.SkorIzinAwal;
        if (reputasi >= GameBalance.ReputasiDiskonIzin)
        {
            skor += 15;
        }
        else if (reputasi >= 50)
        {
            skor += 5;
        }
        else if (reputasi < GameBalance.ReputasiIzinMahal)
        {
            skor -= 10;
        }
        if (kategori == KategoriBangunan.Hunian)
        {
            skor += 10;
        }
        else if (kategori == KategoriBangunan.Fasilitas)
        {
            skor += 15;
        }
        int lebih = jumlahSejenisKawasan - 2;
        if (lebih > 0)
        {
            skor -= lebih * 15;
        }
        skor += VariasiSkor(idPemain, x, y);
        return skor;
    }

    // Keputusan lengkap wali kota untuk satu pengajuan izin.
    // jumlahSejenisKawasan = jumlah bangunan dengan kategori yang
    // sama dalam area 5x5 (jari-jari 2 petak) di sekitar petak.
    public static KeputusanIzin Putuskan(int x, int y, string idBangunan,
        KategoriBangunan kategori, int reputasi, long kasKota,
        int jumlahSejenisKawasan, bool adaBalaiKota, string idPemain)
    {
        KeputusanIzin hasil = new KeputusanIzin();
        hasil.HargaTanah = HargaTanah(x, y);
        hasil.BiayaIzin = BiayaIzin(hasil.HargaTanah, reputasi, kasKota, adaBalaiKota);
        hasil.Syarat = SyaratIzin.TanpaSyarat;

        // Penolakan otomatis (tanpa melihat skor).
        if (reputasi < GameBalance.ReputasiTolakIzin)
        {
            hasil.Hasil = HasilIzin.Ditolak;
            hasil.HargaTotal = 0;
            hasil.Alasan = "Wali kota menolak: reputasimu terlalu rendah ("
                + reputasi + "). Bangun reputasi dulu — layani penghunimu "
                + "dengan baik, lalu ajukan lagi.";
            return hasil;
        }
        if (jumlahSejenisKawasan >= GameBalance.KepadatanMaksKawasan)
        {
            hasil.Hasil = HasilIzin.Ditolak;
            hasil.HargaTotal = 0;
            hasil.Alasan = "Wali kota menolak: kawasan ini sudah padat "
                + "bangunan sejenis (" + jumlahSejenisKawasan + " dalam "
                + "area sekitar). Coba petak di kawasan lain.";
            return hasil;
        }

        int skor = SkorKeputusan(reputasi, kategori, jumlahSejenisKawasan,
            idPemain, x, y);
        if (skor >= GameBalance.SkorIzinDisetujui)
        {
            hasil.Hasil = HasilIzin.Disetujui;
            hasil.HargaTotal = hasil.HargaTanah + hasil.BiayaIzin;
            hasil.Alasan = "Wali kota menyetujui. Lokasi bagus, "
                + "reputasimu baik — bayar harga tanah + biaya izin, "
                + "tanah ini jadi milikmu.";
            return hasil;
        }
        if (skor >= GameBalance.SkorIzinBersyarat)
        {
            hasil.Hasil = HasilIzin.Bersyarat;
            hasil.BiayaIzin = hasil.BiayaIzin
                * GameBalance.PengaliIzinBersyaratPerMil / 1000;
            hasil.HargaTotal = hasil.HargaTanah + hasil.BiayaIzin;
            if (idBangunan == "jalan")
            {
                hasil.Syarat = SyaratIzin.BiayaLebihMahal;
                hasil.Alasan = "Wali kota menyetujui dengan syarat: "
                    + "biaya izin naik 1,5x karena lokasimu kurang "
                    + "strategis. Bayar harga di atas bila setuju.";
            }
            else
            {
                hasil.Syarat = SyaratIzin.JalanBersebelahan;
                hasil.Alasan = "Wali kota menyetujui dengan syarat: "
                    + "biaya izin naik 1,5x DAN wajib ada Jalan di petak "
                    + "bersebelahan sebelum bangunan dipasang. Bayar "
                    + "harga di atas bila setuju.";
            }
            return hasil;
        }
        hasil.Hasil = HasilIzin.Ditolak;
        hasil.HargaTotal = 0;
        hasil.Alasan = "Wali kota menolak: lokasimu kurang cocok untuk "
            + "bangunan ini (skor " + skor + "). Coba petak lain, "
            + "naikkan reputasi, atau pilih jenis bangunan lain.";
        return hasil;
    }

    // Apakah syarat izin terpenuhi saat bangunan mau dipasang?
    // (Dipakai server + klien sebelum membangun di tanah bersyarat.)
    public static bool SyaratTerpenuhi(SyaratIzin syarat,
        bool adaJalanBersebelahan)
    {
        if (syarat == SyaratIzin.JalanBersebelahan)
        {
            return adaJalanBersebelahan;
        }
        return true;
    }
}
