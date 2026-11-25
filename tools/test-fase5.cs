// Dunia Kita — tools/test-fase5.cs
// Test harness logika murni Fase 5 (tanpa Unity): dikompilasi dengan
// mcs bersama AudioManager.cs, GameBalance.cs, SistemTutorial.cs dan
// unity-stubs.cs. Keluar kode 0 bila semua pemeriksaan lolos.
//
// Yang diuji:
//   A. SistemTutorial — 6 langkah (urutan + isi), tampil sekali
//      (PlayerPrefs), lewati otomatis langkah "bangun-rumah" bila
//      rumah sudah dibangun, jalan maju (Lanjut) sampai selesai,
//      ulangi dari Pengaturan (ResetUntukUlang).
//   B. AudioManager — pemetaan momen -> klip, putar SFX per momen,
//      suara mati (toggle) membisukan SFX + musik, musik latar loop
//      mulai/berhenti, aman dipanggil sebelum Instance siap dan
//      bila klip belum diisi, angka GameBalance (VolumeMusik 0,35,
//      TargetFrameRate 60).
//
// Catatan: perilaku yang TIDAK bisa diuji di sini (butuh Editor):
// bunyi SFX/musik di telinga, panel tutorial di layar, dan
// performa di HP nyata — lihat CATATAN-VERIFIKASI.md.

using System;
using UnityEngine;

public static class TestFase5
{
    private static int lolos = 0;
    private static int gagal = 0;

    private static void Periksa(bool kondisi, string nama)
    {
        if (kondisi)
        {
            lolos++;
            Console.WriteLine("  OK  " + nama);
        }
        else
        {
            gagal++;
            Console.WriteLine("  GAGAL  " + nama);
        }
    }

    public static void Main()
    {
        // Awal bersih: PlayerPrefs stub kosong (seperti instal baru).
        PlayerPrefs.HapusSemuaUntukTes();

        // ================= A. SistemTutorial =================
        Console.WriteLine("== Tes A: SistemTutorial — langkah ==");
        Periksa(SistemTutorial.JumlahLangkah == 6, "jumlah langkah 6");
        string[] idHarap = { "selamat-datang", "bangun-rumah",
            "tombol-bangun", "hari-otomatis", "detail-interior",
            "misi-pencapaian" };
        bool urutanBenar = true;
        bool isiTidakKosong = true;
        for (int i = 0; i < SistemTutorial.JumlahLangkah; i++)
        {
            if (SistemTutorial.LangkahSemua[i].Id != idHarap[i])
            {
                urutanBenar = false;
            }
            if (SistemTutorial.LangkahSemua[i].Judul.Length == 0
                || SistemTutorial.LangkahSemua[i].Teks.Length == 0)
            {
                isiTidakKosong = false;
            }
        }
        Periksa(urutanBenar, "urutan id langkah sesuai rencana");
        Periksa(isiTidakKosong, "semua judul + teks langkah terisi");
        Periksa(SistemTutorial.LangkahSemua[3].Teks.Contains("5 menit"),
            "langkah hari-otomatis menyebut 5 menit");
        Periksa(SistemTutorial.KunciSelesai == "DuniaKita_TutorialSelesai",
            "kunci PlayerPrefs selesai benar");

        Console.WriteLine("== Tes B: SistemTutorial — tampil sekali ==");
        Periksa(SistemTutorial.PerluTampilkan(),
            "pemain baru: tutorial perlu ditampilkan");
        SistemTutorial.TandaiSelesai();
        Periksa(!SistemTutorial.PerluTampilkan(),
            "sesudah selesai: tidak ditampilkan lagi");
        SistemTutorial.ResetUntukUlang();
        Periksa(SistemTutorial.PerluTampilkan(),
            "Ulangi Tutorial: ditampilkan lagi");

        Console.WriteLine("== Tes C: SistemTutorial — lewati otomatis ==");
        Periksa(!SistemTutorial.TerpenuhiOtomatis(0, true),
            "selamat-datang tidak pernah terlewati otomatis");
        Periksa(SistemTutorial.TerpenuhiOtomatis(1, true),
            "bangun-rumah terlewati bila rumah sudah dibangun");
        Periksa(!SistemTutorial.TerpenuhiOtomatis(1, false),
            "bangun-rumah TIDAK terlewati bila belum ada rumah");
        Periksa(!SistemTutorial.TerpenuhiOtomatis(2, true),
            "langkah lain tidak terlewati otomatis");
        Periksa(!SistemTutorial.TerpenuhiOtomatis(-1, true)
            && !SistemTutorial.TerpenuhiOtomatis(6, true),
            "indeks di luar jangkauan aman (false)");
        Periksa(SistemTutorial.LangkahPertama(false) == 0,
            "langkah pertama tanpa rumah = 0");
        Periksa(SistemTutorial.LangkahPertama(true) == 0,
            "langkah pertama (selamat-datang) tetap 0 bila ada rumah");
        Periksa(SistemTutorial.LangkahBerikut(0, false) == 1,
            "Lanjut 0->1 tanpa rumah");
        Periksa(SistemTutorial.LangkahBerikut(0, true) == 2,
            "Lanjut 0->2 bila rumah sudah dibangun (1 dilewati)");
        Periksa(SistemTutorial.LangkahBerikut(1, true) == 2,
            "Lanjut 1->2 bila rumah sudah dibangun");
        Periksa(SistemTutorial.LangkahBerikut(4, false) == 5,
            "Lanjut 4->5");
        Periksa(SistemTutorial.LangkahBerikut(5, false)
            == SistemTutorial.JumlahLangkah,
            "Lanjut dari langkah terakhir = selesai (6)");

        Console.WriteLine("== Tes D: SistemTutorial — jalan penuh ==");
        // Pemain baru tanpa rumah: 0,1,2,3,4,5 lalu selesai.
        int langkah = SistemTutorial.LangkahPertama(false);
        int dikunjungi = 0;
        while (langkah < SistemTutorial.JumlahLangkah)
        {
            dikunjungi++;
            langkah = SistemTutorial.LangkahBerikut(langkah, false);
        }
        Periksa(dikunjungi == 6, "jalan tanpa rumah mengunjungi 6 langkah");
        // Pemain yang sudah membangun rumah dulu: 0,2,3,4,5.
        langkah = SistemTutorial.LangkahPertama(true);
        dikunjungi = 0;
        while (langkah < SistemTutorial.JumlahLangkah)
        {
            dikunjungi++;
            langkah = SistemTutorial.LangkahBerikut(langkah, true);
        }
        Periksa(dikunjungi == 5,
            "jalan dengan rumah mengunjungi 5 langkah (1 dilewati)");
        SistemTutorial.TandaiSelesai();
        Periksa(!SistemTutorial.PerluTampilkan(),
            "jalan penuh lalu selesai: tidak ditampilkan lagi");

        // ================= B. AudioManager =================
        Console.WriteLine("== Tes E: AudioManager — angka & kunci ==");
        Periksa(GameBalance.VolumeMusik == 0.35f, "VolumeMusik 0,35");
        Periksa(GameBalance.TargetFrameRate == 60, "TargetFrameRate 60");
        Periksa(AudioManager.KunciSuara == "DuniaKita_Suara",
            "kunci suara sama dengan PanelPengaturan");

        Console.WriteLine("== Tes F: AudioManager — aman tanpa Instance ==");
        // Panggilan statis sebelum AudioManager siap: tidak error,
        // tidak berbuat apa-apa (Instance masih null).
        AudioManager.MainkanTap();
        AudioManager.MainkanBangun();
        Periksa(AudioManager.Instance == null,
            "tanpa Siapkan(), Instance null dan Mainkan aman");

        Console.WriteLine("== Tes G: AudioManager — pemetaan momen-klip ==");
        AudioManager am = new AudioManager();
        am.sfxTap = new AudioClip();
        am.sfxBangun = new AudioClip();
        am.sfxKoin = new AudioClip();
        am.sfxNaikLevel = new AudioClip();
        am.sfxKlaim = new AudioClip();
        am.sfxTolak = new AudioClip();
        am.musikLatar = new AudioClip();
        Periksa(am.KlipUntuk(MomenAudio.Tap) == am.sfxTap, "Tap -> sfxTap");
        Periksa(am.KlipUntuk(MomenAudio.Bangun) == am.sfxBangun,
            "Bangun -> sfxBangun");
        Periksa(am.KlipUntuk(MomenAudio.Koin) == am.sfxKoin,
            "Koin -> sfxKoin");
        Periksa(am.KlipUntuk(MomenAudio.NaikLevel) == am.sfxNaikLevel,
            "NaikLevel -> sfxNaikLevel");
        Periksa(am.KlipUntuk(MomenAudio.Klaim) == am.sfxKlaim,
            "Klaim -> sfxKlaim");
        Periksa(am.KlipUntuk(MomenAudio.Tolak) == am.sfxTolak,
            "Tolak -> sfxTolak");
        Periksa(am.KlipUntuk((MomenAudio)99) == null,
            "momen tak dikenal -> null");

        Console.WriteLine("== Tes H: AudioManager — putar SFX & musik ==");
        PlayerPrefs.SetInt(AudioManager.KunciSuara, 1);
        am.Siapkan();
        Periksa(AudioManager.Instance == am, "Siapkan() mendaftarkan Instance");
        Periksa(am.MusikBerjalan, "musik latar berjalan sesudah Siapkan()");
        am.MulaiMusik(); // Panggilan kedua: tidak memulai dua kali.
        Periksa(am.MusikBerjalan, "MulaiMusik() kedua tetap berjalan (aman)");
        am.Mainkan(MomenAudio.Tap);
        Periksa(am.TerakhirDimainkan == "Tap", "Mainkan Tap tercatat");
        am.Mainkan(MomenAudio.Bangun);
        Periksa(am.TerakhirDimainkan == "Bangun", "Mainkan Bangun tercatat");
        am.Mainkan(MomenAudio.Koin);
        Periksa(am.TerakhirDimainkan == "Koin", "Mainkan Koin tercatat");
        am.Mainkan(MomenAudio.NaikLevel);
        Periksa(am.TerakhirDimainkan == "NaikLevel",
            "Mainkan NaikLevel tercatat");
        am.Mainkan(MomenAudio.Klaim);
        Periksa(am.TerakhirDimainkan == "Klaim", "Mainkan Klaim tercatat");
        am.Mainkan(MomenAudio.Tolak);
        Periksa(am.TerakhirDimainkan == "Tolak", "Mainkan Tolak tercatat");
        AudioManager.MainkanKlaim(); // Pintasan statis via Instance.
        Periksa(am.TerakhirDimainkan == "Klaim",
            "pintasan statis MainkanKlaim() bekerja");
        am.HentikanMusik();
        Periksa(!am.MusikBerjalan, "HentikanMusik() menghentikan musik");
        am.MulaiMusik();
        Periksa(am.MusikBerjalan, "MulaiMusik() memulai lagi");

        Console.WriteLine("== Tes I: AudioManager — toggle Suara ==");
        PlayerPrefs.SetInt(AudioManager.KunciSuara, 0);
        AudioManager.TerapkanSuara();
        Periksa(AudioListener.volume == 0f, "suara mati -> volume 0");
        Periksa(!AudioManager.SuaraNyala(), "SuaraNyala() false bila mati");
        AudioManager am2 = new AudioManager();
        am2.sfxTap = new AudioClip();
        am2.musikLatar = new AudioClip();
        am2.Siapkan();
        Periksa(!am2.MusikBerjalan,
            "suara mati: musik tidak mulai di Siapkan()");
        am2.Mainkan(MomenAudio.Tap);
        Periksa(am2.TerakhirDimainkan == null,
            "suara mati: SFX tidak dimainkan");
        PlayerPrefs.SetInt(AudioManager.KunciSuara, 1);
        AudioManager.TerapkanSuara();
        Periksa(AudioListener.volume == 1f, "suara nyala -> volume 1");
        Periksa(AudioManager.SuaraNyala(), "SuaraNyala() true bila nyala");
        am2.MulaiMusik();
        Periksa(am2.MusikBerjalan, "suara nyala: musik bisa mulai lagi");
        am2.Mainkan(MomenAudio.Tap);
        Periksa(am2.TerakhirDimainkan == "Tap",
            "suara nyala: SFX dimainkan lagi");

        Console.WriteLine("== Tes J: AudioManager — klip kosong aman ==");
        AudioManager am3 = new AudioManager(); // Semua klip null.
        am3.Siapkan();
        Periksa(!am3.MusikBerjalan, "tanpa klip musik: musik tidak berjalan");
        am3.Mainkan(MomenAudio.Tap);
        Periksa(am3.TerakhirDimainkan == null,
            "tanpa klip SFX: Mainkan aman, tidak tercatat");

        Console.WriteLine();
        Console.WriteLine("HASIL: " + lolos + " lolos, " + gagal + " gagal");
        if (gagal > 0)
        {
            Environment.Exit(1);
        }
    }
}
