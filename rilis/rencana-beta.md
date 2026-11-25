# Rencana Beta Tertutup — Dunia Kita (Fase 6)

Beta tertutup = APK dibagikan langsung ke 3–5 orang (bukan lewat
Play Store) sebelum AAB diunggah. Tujuannya menemukan crash dan
hal yang membingungkan, bukan menambah fitur.

## Langkah

1. **Build APK debug** — jalankan `build-android.bat` di laptop,
   pilih 1. Hasil: `C:\Dunia Kita\build\DuniaKita-v1.0.apk`.
2. **Pasang di HP sendiri dulu** — mainkan 1 hari game penuh
   (5 menit/hari, beberapa hari game): bangun 3 bangunan pertama,
   klaim 1 misi, tutup lalu buka lagi (progres harus tetap).
3. **Bagikan ke 3–5 teman** — kirim APK lewat WhatsApp/Drive.
   Minta mereka mengizinkan "instal dari sumber tidak dikenal"
   sekali saja. Sertakan pesan singkat:
   > "Tolong coba game buatanku, Dunia Kita. Mainkan 10–15 menit,
   > lalu jawab 5 pertanyaan di bawah. Tidak perlu install
   > apa-apa lagi."
4. **Kumpulkan masukan** — 5 pertanyaan untuk tiap penguji:
   - Apakah game pernah menutup sendiri (crash)? Kapan?
   - Apakah tutorialnya jelas? Bagian mana yang membingungkan?
   - Apakah tombol dan teksnya mudah dibaca di HP kamu?
   - Apakah suara/musiknya enak atau mengganggu?
   - Apa yang paling seru, dan apa yang paling membosankan?
5. **Perbaiki yang ditemukan** — crash dan kebingungan tutorial
   didahulukan; permintaan fitur baru dicatat untuk Fase 7,
   bukan dikerjakan sekarang.
6. **Build ulang + uji lagi** — naikkan Bundle Version Code tiap
   build ulang (Player Settings > Android > Other Settings),
   lalu bagikan APK baru ke penguji yang sama.
7. **Bila 3 hari main tanpa crash dan tutorial dipahami** — lanjut
   ke AAB: buat keystore, build AAB lewat `build-android.bat`
   (pilih 2), lalu unggah ke Play Console (jalur rilis tertutup
   dulu, baru produksi bila stabil).

## Kriteria lanjut ke produksi

- 0 crash dalam 3 hari pemakaian tiap penguji.
- Semua penguji bisa menyelesaikan tutorial tanpa bertanya.
- Tidak ada laporan teks/tombol yang tidak terbaca.
- Performa terasa mulus di HP penguji (tidak panas berlebihan).

## Yang BUKAN bagian beta ini

- Akun developer Play Store ($25, sekali bayar) — dibuat user
  saat siap mengunggah AAB; bukan urusan penguji beta.
- Mode multipemain — Fase 7, sesudah rilis single player stabil.
