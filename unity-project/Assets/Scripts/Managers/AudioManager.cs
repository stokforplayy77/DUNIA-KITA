// Dunia Kita — AudioManager
// Pusat audio game (Fase 5, GDD §12): efek suara (SFX) + musik
// latar ambient. Semua klip disintesis dari kode (tools/
// generate-audio.py) — tanpa unduhan, tanpa rekaman.
//
// Cara pakai: panggil metode statis MainkanTap() / MainkanBangun() /
// MainkanKoin() / MainkanNaikLevel() / MainkanKlaim() /
// MainkanTolak() dari mana saja. Semua metode statis aman dipanggil
// sebelum AudioManager siap (Instance null -> tidak berbuat apa-apa)
// dan menghormati toggle Suara di PanelPengaturan (PlayerPrefs
// "DuniaKita_Suara" — kunci yang sama, lihat KunciSuara).
//
// Musik latar diputar loop otomatis saat game mulai (bila suara
// nyala); volumenya terpisah dari SFX (GameBalance.VolumeMusik)
// supaya tidak menenggelamkan efek suara. AudioListener.volume
// tetap dipakai sebagai saklar utama (0 = bisu total) seperti
// PanelPengaturan sejak Fase 1.

using UnityEngine;

public enum MomenAudio
{
    Tap,       // Ketuk tombol apa pun.
    Bangun,    // Bangunan / perabot berhasil dipasang.
    Koin,      // Pemasukan harian masuk (laporan hari, bersih > 0).
    NaikLevel, // Dunia naik level.
    Klaim,     // Hadiah misi / pencapaian diklaim.
    Tolak      // Aksi ditolak (koin tidak cukup, petak terisi).
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Kunci PlayerPrefs yang SAMA dengan PanelPengaturan (1 = nyala).
    public const string KunciSuara = "DuniaKita_Suara";

    [Header("SFX (diisi dari Scene)")]
    public AudioClip sfxTap;
    public AudioClip sfxBangun;
    public AudioClip sfxKoin;
    public AudioClip sfxNaikLevel;
    public AudioClip sfxKlaim;
    public AudioClip sfxTolak;

    [Header("Musik (diisi dari Scene)")]
    public AudioClip musikLatar;

    // Nama momen terakhir yang dimainkan (untuk tes logika Fase 5).
    public string TerakhirDimainkan { get; private set; }

    // True bila musik latar sedang diputar (untuk tes logika Fase 5).
    public bool MusikBerjalan
    {
        get { return sumberMusik != null && sumberMusik.isPlaying; }
    }

    private AudioSource sumberSfx;
    private AudioSource sumberMusik;

    private void Awake()
    {
        Siapkan();
    }

    // Dipanggil Awake; dipisah supaya tes logika bisa memanggilnya
    // langsung (new AudioManager().Siapkan()) tanpa Unity Editor.
    public void Siapkan()
    {
        Instance = this;
        TerapkanSuara();
        BuatSumber();
        MulaiMusik();
    }

    private void BuatSumber()
    {
        if (sumberSfx == null)
        {
            GameObject goSfx = new GameObject("AudioSFX");
            goSfx.transform.SetParent(transform, false);
            sumberSfx = goSfx.AddComponent<AudioSource>();
        }
        if (sumberMusik == null)
        {
            GameObject goMusik = new GameObject("AudioMusik");
            goMusik.transform.SetParent(transform, false);
            sumberMusik = goMusik.AddComponent<AudioSource>();
            sumberMusik.clip = musikLatar;
            sumberMusik.loop = true;
            sumberMusik.volume = GameBalance.VolumeMusik;
        }
    }

    // Saklar suara utama (dipakai juga PanelPengaturan): mati ->
    // AudioListener.volume 0 (semua suara bisu, termasuk musik).
    public static void TerapkanSuara()
    {
        AudioListener.volume = SuaraNyala() ? 1f : 0f;
    }

    public static bool SuaraNyala()
    {
        return PlayerPrefs.GetInt(KunciSuara, 1) == 1;
    }

    // Mulai musik latar (loop). Dipanggil otomatis di Siapkan();
    // aman dipanggil ulang (tidak memulai dua kali).
    public void MulaiMusik()
    {
        if (!SuaraNyala() || musikLatar == null)
        {
            return;
        }
        if (sumberMusik == null)
        {
            BuatSumber();
        }
        if (!sumberMusik.isPlaying)
        {
            sumberMusik.Play();
        }
    }

    public void HentikanMusik()
    {
        if (sumberMusik != null && sumberMusik.isPlaying)
        {
            sumberMusik.Stop();
        }
    }

    // Sesuaikan nuansa musik latar saat malam/hujan agar lebih tenang.
    public void AturSuasanaMalam(bool malam)
    {
        if (sumberMusik == null) return;
        sumberMusik.volume = malam
            ? GameBalance.VolumeMusik * 0.72f
            : GameBalance.VolumeMusik;
        sumberMusik.pitch = malam ? 0.94f : 1.0f;
    }

    // Klip untuk satu momen (null bila klip belum diisi di Scene).
    public AudioClip KlipUntuk(MomenAudio momen)
    {
        switch (momen)
        {
            case MomenAudio.Tap: return sfxTap;
            case MomenAudio.Bangun: return sfxBangun;
            case MomenAudio.Koin: return sfxKoin;
            case MomenAudio.NaikLevel: return sfxNaikLevel;
            case MomenAudio.Klaim: return sfxKlaim;
            case MomenAudio.Tolak: return sfxTolak;
            default: return null;
        }
    }

    // Mainkan satu SFX. Aman bila suara mati / klip kosong /
    // sumber belum dibuat (tidak berbuat apa-apa, tidak error).
    public void Mainkan(MomenAudio momen)
    {
        if (!SuaraNyala())
        {
            return;
        }
        AudioClip klip = KlipUntuk(momen);
        if (klip == null)
        {
            return;
        }
        if (sumberSfx == null)
        {
            BuatSumber();
        }
        sumberSfx.PlayOneShot(klip);
        TerakhirDimainkan = momen.ToString();
    }

    // ---- Pintasan statis (dipanggil manager/panel lain) ----
    public static void MainkanTap() { MainkanStatis(MomenAudio.Tap); }
    public static void MainkanBangun() { MainkanStatis(MomenAudio.Bangun); }
    public static void MainkanKoin() { MainkanStatis(MomenAudio.Koin); }
    public static void MainkanNaikLevel() { MainkanStatis(MomenAudio.NaikLevel); }
    public static void MainkanKlaim() { MainkanStatis(MomenAudio.Klaim); }
    public static void MainkanTolak() { MainkanStatis(MomenAudio.Tolak); }

    private static void MainkanStatis(MomenAudio momen)
    {
        if (Instance != null)
        {
            Instance.Mainkan(momen);
        }
    }
}
