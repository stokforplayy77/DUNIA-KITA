// Dunia Kita — AturanDuniaBersama
// Aturan MURNI dunia bersama (tanpa dependensi UnityEngine) supaya
// bisa dikompilasi dan diuji di luar Unity (lihat
// tools/test-fase7.cs). Aturan di sini adalah CERMIN aturan server
// Python (server/db.py) — server adalah otoritas akhir, kelas ini
// dipakai klien untuk pra-validasi (supaya tombol yang jelas-jelas
// tidak bisa dilakukan langsung ditolak dengan pesan yang jelas)
// dan dipakai tes logika.
//
// PEMBAGIAN KEWENANGAN (jujur, Fase 7):
//   - Server yang memutuskan: kepemilikan tanah, koin, keputusan
//     izin (SistemWaliKota), syarat jalan, pemasukan saat offline.
//   - Klien yang memutuskan: syarat level bangunan (menu bangun),
//     level/hari/permata (dihitung lokal dari dunia), tampilan.
//   - Koin + reputasi dicerminkan dari server ke klien (delta);
//     level server di v1 selalu 1 dan tidak dipakai menegakkan
//     apa pun (lihat server/README.md).

using System.Collections.Generic;

public static class AturanDuniaBersama
{
    // Kunci petak "x,y" — dipakai peta kepemilikan + peta bangunan.
    public static string KunciPetak(int x, int y)
    {
        return x + "," + y;
    }

    // Hasil validasi bangun di dunia bersama (pesan Bahasa Indonesia).
    public class HasilValidasi
    {
        public bool Boleh;
        public string Alasan;
    }

    // Validasi bangun di dunia bersama — CERMIN server/db.py
    // (fungsi validasi_bangun). Semua petak dalam tapak bangunan
    // harus milik pemain yang sama dan kosong; koin harus cukup;
    // syarat izin (jalan bersebelahan) harus terpenuhi.
    //   tapak: petak pojok + ukuran bangunan (1x1 / 2x2 / 3x3).
    //   pemilikPetak: peta "x,y" -> nama pemilik (tanpa entri =
    //     tanah kota / bukan milik siapa pun).
    //   terisi: peta "x,y" -> true bila ada bangunan di petak itu.
    //   namaSaya: nama pemain yang meminta (bandingkan persis).
    public static HasilValidasi ValidasiBangun(int x, int y,
        int lebarTapak, int tinggiTapak, long biayaBangun, long koinSaya,
        string namaSaya, Dictionary<string, string> pemilikPetak,
        Dictionary<string, bool> terisi, SyaratIzin syarat,
        bool adaJalanBersebelahan)
    {
        HasilValidasi hasil = new HasilValidasi();
        for (int px = x; px < x + lebarTapak; px++)
        {
            for (int py = y; py < y + tinggiTapak; py++)
            {
                string kunci = KunciPetak(px, py);
                string pemilik;
                if (pemilikPetak == null
                    || !pemilikPetak.TryGetValue(kunci, out pemilik)
                    || pemilik != namaSaya)
                {
                    hasil.Boleh = false;
                    hasil.Alasan = "Petak (" + px + ", " + py + ") bukan "
                        + "tanah milikmu — beli dulu lewat Wali Kota.";
                    return hasil;
                }
                bool isi;
                if (terisi != null && terisi.TryGetValue(kunci, out isi) && isi)
                {
                    hasil.Boleh = false;
                    hasil.Alasan = "Petak (" + px + ", " + py + ") sudah "
                        + "terisi bangunan.";
                    return hasil;
                }
            }
        }
        if (!SistemWaliKota.SyaratTerpenuhi(syarat, adaJalanBersebelahan))
        {
            hasil.Boleh = false;
            hasil.Alasan = "Izinmu bersyarat: wajib ada Jalan di petak "
                + "bersebelahan sebelum bangunan dipasang.";
            return hasil;
        }
        if (koinSaya < biayaBangun)
        {
            hasil.Boleh = false;
            hasil.Alasan = "Koin tidak cukup untuk membangun (butuh "
                + FormatUtil.Rupiah(biayaBangun) + ").";
            return hasil;
        }
        hasil.Boleh = true;
        hasil.Alasan = "";
        return hasil;
    }

    // Hasil pemasukan saat offline (dihitung server saat masuk lagi
    // — CERMIN server/db.py pemasukan_offline).
    public struct HasilOffline
    {
        public long Kotor;    // Total pemasukan kotor per hari x hari.
        public long Pajak;    // Pajak kota 2,5% dari pemasukan kotor.
        public long Upkeep;   // Total perawatan per hari x hari.
        public long Bersih;   // Kotor - Pajak - Upkeep (boleh negatif).
        public int Hari;      // Hari game yang dihitung (dibatasi).
    }

    // Pemasukan offline: pemasukan dasar bangunan (TANPA pengali
    // warung/kebahagiaan/hujan/jalan — penyederhanaan yang
    // didokumentasikan: efek-efek itu hanya berlaku saat bermain
    // langsung) dikurangi pajak 2,5% dan perawatan. Hari dibatasi
    // MaksHariOffline (24 hari game).
    public static HasilOffline HitungPemasukanOffline(
        IList<EconomyCalculator.CatatanBangunan> bangunan, int hariBerlalu)
    {
        HasilOffline hasil = new HasilOffline();
        int hari = hariBerlalu;
        if (hari < 0)
        {
            hari = 0;
        }
        if (hari > GameBalance.MaksHariOffline)
        {
            hari = GameBalance.MaksHariOffline;
        }
        hasil.Hari = hari;
        long kotorPerHari = 0;
        long upkeepPerHari = 0;
        if (bangunan != null)
        {
            foreach (EconomyCalculator.CatatanBangunan b in bangunan)
            {
                kotorPerHari += b.PemasukanHarian;
                upkeepPerHari += b.BiayaPerawatanHarian;
            }
        }
        hasil.Kotor = kotorPerHari * hari;
        hasil.Pajak = hasil.Kotor * GameBalance.PajakKotaPerMil / 1000;
        hasil.Upkeep = upkeepPerHari * hari;
        hasil.Bersih = hasil.Kotor - hasil.Pajak - hasil.Upkeep;
        return hasil;
    }

    // Satu bangunan di dunia bersama (dipakai diff sinkronisasi).
    public struct BangunanBersama
    {
        public int X;
        public int Y;
        public string Id;
        public string Pemilik;
    }

    // Hasil diff dunia: bangunan yang harus ditambah + yang harus
    // dilepas supaya dunia lokal sama dengan dunia server.
    public class HasilDiff
    {
        public List<BangunanBersama> Tambah = new List<BangunanBersama>();
        public List<BangunanBersama> Lepas = new List<BangunanBersama>();
    }

    // Bandingkan dunia lokal dengan dunia server (per petak + id +
    // pemilik; bangunan yang sama persis tidak disentuh supaya
    // sprite/warga tidak berkedip tiap sinkronisasi).
    public static HasilDiff DiffDunia(IList<BangunanBersama> lokal,
        IList<BangunanBersama> server)
    {
        HasilDiff hasil = new HasilDiff();
        Dictionary<string, BangunanBersama> petaLokal =
            new Dictionary<string, BangunanBersama>();
        Dictionary<string, BangunanBersama> petaServer =
            new Dictionary<string, BangunanBersama>();
        if (lokal != null)
        {
            foreach (BangunanBersama b in lokal)
            {
                petaLokal[KunciPetak(b.X, b.Y)] = b;
            }
        }
        if (server != null)
        {
            foreach (BangunanBersama b in server)
            {
                petaServer[KunciPetak(b.X, b.Y)] = b;
            }
        }
        foreach (KeyValuePair<string, BangunanBersama> kv in petaServer)
        {
            BangunanBersama lama;
            if (!petaLokal.TryGetValue(kv.Key, out lama)
                || lama.Id != kv.Value.Id
                || lama.Pemilik != kv.Value.Pemilik)
            {
                hasil.Tambah.Add(kv.Value);
            }
        }
        foreach (KeyValuePair<string, BangunanBersama> kv in petaLokal)
        {
            BangunanBersama baru;
            if (!petaServer.TryGetValue(kv.Key, out baru)
                || baru.Id != kv.Value.Id
                || baru.Pemilik != kv.Value.Pemilik)
            {
                hasil.Lepas.Add(kv.Value);
            }
        }
        return hasil;
    }
}
