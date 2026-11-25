# Server Dunia Bersama — Dunia Kita (Fase 7)

Server dunia bersama: satu peta 24×24 dipakai banyak pemain. Python
stdlib SAJA (`http.server` + `sqlite3`) — tanpa dependensi, tanpa
`pip install`. Logika data: `db.py`. Lapisan HTTP + batas laju:
`server.py`.

## Menjalankan

```bash
cd server
python3 server.py                 # http://127.0.0.1:8471, DB dunia-kita.db
python3 server.py --port 9000 --db /tmp/uji.db   # port + DB lain
```

Server mencetak `DUNIAKITA_SIAP port=...` saat sudah menerima
request. Berhenti dengan Ctrl+C. Database dibuat otomatis saat
pertama jalan (file `dunia-kita.db` di folder yang sama).

**Di HP:** klien Unity memakai alamat server dari kolom "Alamat
server" di panel Akun (disimpan di PlayerPrefs `DuniaKita_Server`).
Default `http://127.0.0.1:8471` hanya jalan bila server ada di
perangkat yang sama — di HP isi dengan IP LAN laptop yang
menjalankan server (mis. `http://192.168.1.5:8471`) dan jalankan
server dengan `--host 0.0.0.0`.

## Endpoint (semua JSON)

Kecuali `/api/daftar` dan `/api/masuk`, semua butuh token: header
`Authorization: Bearer <token>` atau `?token=<token>`.

| Endpoint | Isi |
|---|---|
| POST `/api/daftar` | `{nama, sandi}` → `{ok, token, nama}` |
| POST `/api/masuk` | `{nama, sandi}` → `{ok, token, pemain, pemasukan_offline, hari_offline}` |
| GET `/api/dunia` | `{ok, ukuran, bangunan, tanah, kas_kota, jumlah_pemain}` |
| POST `/api/ajukan-izin` | `{x, y, bangunan}` → `{ok, keputusan, izin_id}` |
| POST `/api/bayar-izin` | `{izin_id}` → `{ok, x, y, koin, total}` |
| POST `/api/bangun` | `{x, y, bangunan}` → `{ok, x, y, bangunan, koin}` |
| POST `/api/jual` | `{x, y}` → `{ok, x, y, bangunan, harga_jual, koin}` |
| POST `/api/sinkron` | `{koin, reputasi}` → `{ok, koin, reputasi}` |
| GET `/api/pemain` | `{ok, pemain}` |
| GET `/api/kas-kota` | `{ok, saldo, riwayat}` |

Gagal: `{ok: false, kesalahan: "..."}` (HTTP 400/401/404/429/500).

## Aturan dunia bersama

- **Peta 24×24 (576 petak).** Tanah tanpa pemilik = tanah kota.
- **Akun:** nama 1–20 karakter (unik), kata sandi minimal 4 karakter.
  Kata sandi di-hash PBKDF2-HMAC-SHA256 (100.000 iterasi, salt per
  akun) — tidak pernah disimpan polos. Token acak 48 heksadesimal;
  masuk baru mengganti token lama.
- **Beli tanah (alur GDD §15):** ajukan izin → wali kota memutuskan
  → bayar harga tanah + biaya izin → tanah jadi milik → baru boleh
  bangun. Uang masuk **kas kota** (lihat `/api/kas-kota`).
- **Wali kota (NPC):** harga tanah dari jarak ke pusat (pinggir
  Rp200.000, pusat Rp1.400.000); biaya izin 10% harga tanah,
  diubah reputasi (≥70: −30%, <30: +50%), kas kota (<Rp1jt: +50%,
  >Rp20jt: −20%), dan Balai Kota (−10%); paling murah Rp10.000.
  Skor 50 awal (hunian +10, fasilitas +15, sejenis di kawasan
  mengurangi); ≥40 disetujui, 25–39 bersyarat (biaya izin ×1,5 +
  wajib Jalan bersebelahan — kecuali bangunan Jalan sendiri),
  <25 ditolak; ditolak otomatis bila reputasi <15 atau kawasan
  sudah punya 4+ bangunan sejenis. Rumus ini CERMIN
  `SistemWaliKota.cs` — ubah di kedua tempat bila berubah.
- **Bangun:** semua petak dalam tapak bangunan (1×1/2×2/3×3) harus
  milik pemain yang sama dan kosong; koin harus cukup (dipotong di
  server); syarat jalan dicek saat membangun. **Syarat level
  bangunan TIDAK ditegakkan server di v1** — ditegakkan di klien
  (menu bangun); level server selalu 1 dan hanya informatif.
- **Jual:** kembali 50% biaya bangun; tanah tetap milik pemain
  (hanya bangunannya hilang).
- **Offline:** saat masuk lagi, server menghitung pemasukan bangunan
  selama pemain pergi: (pemasukan − perawatan) per hari game,
  dikurangi pajak 2,5%, maks **24 hari game** (= 2 jam waktu nyata;
  1 hari game = 5 menit). Tanpa pengali warung/kebahagiaan/hujan/
  jalan (penyederhanaan yang didokumentasikan — efek itu hanya
  berlaku saat bermain langsung). Bila bersih negatif dan koin
  tidak cukup, koin jadi 0 dan sisanya jadi tunggakan.
- **Sinkron sesi (`/api/sinkron`):** klien mendorong koin + reputasi
  sesinya tiap 10 detik supaya koin server mengikuti ekonomi lokal
  yang berjalan di klien (hari, kejadian, misi dihitung lokal).
  Server TIDAK percaya begitu saja: koin ≥ 0, reputasi 0–100, dan
  perubahan per sinkron maks **Rp6.000.000** (penjualan termahal
  kampus Rp10jt → jual 50% = Rp5jt + hadiah naik level Rp2jt).
  Pembelian bangunan TETAP divalidasi penuh di `/api/bangun` dari
  koin server; klien menahan sinkron selama aksi bangun/jual/bayar
  berjalan supaya tidak menimpa potongan server. Batas ini BUKAN
  anti-curang penuh (klien masih bisa mendorong +Rp6jt tiap 10
  detik) — validasi anti-curang penuh = pengembangan lanjutan.
- **Migrasi single player → online:** simpanan lokal "5.1" tetap
  dimuat klien sebagai "6.0"; saat masuk online pertama kali, klien
  mengirim daftar bangunan lokal ke `POST /api/impor-tanah` — bila
  pemain BELUM punya tanah apa pun di server, tanahnya diakui
  sebagai milik TANPA bayar (tanah single player memang gratis).
  Impor hanya sekali; bangunan yang tapaknya sudah dimiliki pemain
  lain dilewati. Interior perabot TIDAK ikut ke server (hanya di
  save lokal; dunia sendiri di cadangan tetap punya interiornya).
- **Batas laju:** maks 60 request/menit per IP (HTTP 429 bila lebih).

## Deploy (belum diputuskan — jujur)

Server baru diuji di **localhost**. Belum di-deploy ke mana pun.
**JANGAN deploy ke VPS utama user (Sumopod/Tencent)** — ToS-nya
melarang game server. Opsi yang masuk akal: Node VPS lain yang
tidak melarang game server, atau laptop user sebagai host sementara
saat main bersama teman di satu Wi-Fi (server `--host 0.0.0.0` +
IP LAN). Keputusan menunggu user. Server ini BUKAN server produksi:
tanpa HTTPS, tanpa backup otomatis — backup DB dengan menyalin
file `dunia-kita.db` secara berkala.

## Yang BELUM ada (jujur, Fase 7)

- Obrolan antar pemain, papan peringkat online.
- Wali kota tampil sebagai karakter di peta (keputusannya ada,
  wujudnya belum).
- Pembangunan fasilitas kota otomatis dari kas kota (kas hanya
  dicatat + ditampilkan).
- Sinkronisasi di bawah 1 detik (tarik state tiap 10 detik +
  sesudah tiap aksi — cukup untuk city builder).
- Penegakan syarat level bangunan di server (baru di klien).
