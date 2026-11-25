#!/usr/bin/env python3
"""Dunia Kita — server Dunia Bersama (Fase 7).

HTTP server Python stdlib SAJA (http.server + sqlite3) — tanpa
dependensi luar. Menyimpan dunia bersama: akun, kepemilikan tanah,
bangunan, kas kota. Logika data hidup di db.py.

Jalankan:
    python3 server.py [--port 8471] [--db dunia-kita.db] [--host 0.0.0.0]

Endpoint (semua JSON; kecuali /api/daftar dan /api/masuk, semua
butuh token — header "Authorization: Bearer <token>" atau parameter
"?token=<token>"):
    POST /api/daftar      {nama, sandi} -> {ok, token, nama}
    POST /api/masuk       {nama, sandi} -> {ok, token, pemain,
                            pemasukan_offline, hari_offline}
    GET  /api/dunia       -> {ok, ukuran, bangunan, tanah, kas_kota,
                            jumlah_pemain}
    POST /api/ajukan-izin {x, y, bangunan} -> {ok, keputusan, izin_id}
    POST /api/bayar-izin  {izin_id} -> {ok, x, y, koin, total}
    POST /api/sinkron     {koin, reputasi} -> {ok, koin, reputasi}
    POST /api/impor-tanah  {bangunan: [{x, y, id}]} -> {ok, diimpor, dilewati}
    POST /api/bangun      {x, y, bangunan} -> {ok, x, y, bangunan, koin}
    POST /api/jual        {x, y} -> {ok, x, y, bangunan, harga_jual, koin}
    GET  /api/pemain      -> {ok, pemain}
    GET  /api/kas-kota    -> {ok, saldo, riwayat}

Batas laju: maks 60 request per menit per IP (angka sama dengan
GameBalance.BatasLajuPerMenit). Melebihi -> HTTP 429.

Keamanan: ini server game kecil untuk uji + main bersama teman,
BUKAN server produksi — jangan jalankan dengan data sensitif, dan
jangan expose langsung ke internet tanpa reverse proxy + HTTPS.
Kata sandi di-hash (db.py); token acak per sesi.
"""

import argparse
import json
import sys
import threading
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import parse_qs, urlparse

sys.path.insert(0, __file__.rsplit("/", 1)[0])
from db import DuniaDB

BATAS_LAJU_PER_MENIT = 60
BATAS_LAJU_JENDELA_DETIK = 60


class PembatasLaju:
    """Hitung request per IP dalam jendela 60 detik (di memori)."""

    def __init__(self):
        self.kunci = threading.Lock()
        self.riwayat = {}

    def boleh(self, ip):
        now = time.time()
        with self.kunci:
            daftar = [t for t in self.riwayat.get(ip, [])
                      if now - t < BATAS_LAJU_JENDELA_DETIK]
            if len(daftar) >= BATAS_LAJU_PER_MENIT:
                self.riwayat[ip] = daftar
                return False
            daftar.append(now)
            self.riwayat[ip] = daftar
            return True


class Penangan(BaseHTTPRequestHandler):
    db = None
    pembatas = PembatasLaju()
    protokol = "HTTP/1.1"

    # -- util ---------------------------------------------------------
    def log_message(self, fmt, *args):
        sys.stderr.write("[server] " + fmt % args + "\n")

    def _kirim(self, kode, isi):
        badan = json.dumps(isi, ensure_ascii=False).encode("utf-8")
        self.send_response(kode)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(badan)))
        self.end_headers()
        self.wfile.write(badan)

    def _baca_json(self):
        panjang = int(self.headers.get("Content-Length") or 0)
        if panjang <= 0:
            return {}
        try:
            return json.loads(self.rfile.read(panjang).decode("utf-8"))
        except (ValueError, UnicodeDecodeError):
            return None

    def _token(self, query):
        auth = self.headers.get("Authorization") or ""
        if auth.startswith("Bearer "):
            return auth[7:].strip()
        nilai = query.get("token")
        return nilai[0] if nilai else None

    def _butuh_pemain(self, query):
        pemain = self.db.pemain_dari_token(self._token(query))
        if pemain is None:
            self._kirim(401, {"ok": False,
                              "kesalahan": "Butuh token masuk yang sah."})
            return None
        return pemain

    # -- rute ---------------------------------------------------------
    def do_GET(self):
        self._layani("GET")

    def do_POST(self):
        self._layani("POST")

    def _layani(self, metode):
        ip = self.client_address[0]
        if not self.pembatas.boleh(ip):
            self._kirim(429, {"ok": False,
                              "kesalahan": "Terlalu banyak permintaan — "
                              "coba lagi sebentar lagi."})
            return
        parsed = urlparse(self.path)
        jalur = parsed.path
        query = parse_qs(parsed.query)
        badan = {}
        if metode == "POST":
            badan = self._baca_json()
            if badan is None:
                self._kirim(400, {"ok": False,
                                  "kesalahan": "Badan JSON tidak sah."})
                return
        try:
            if jalur == "/api/daftar" and metode == "POST":
                hasil, salah = self.db.daftar(badan.get("nama"),
                                              badan.get("sandi"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/masuk" and metode == "POST":
                hasil, salah = self.db.masuk(badan.get("nama"),
                                             badan.get("sandi"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/dunia" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True, **self.db.dunia(pemain["distrik_aktif"])})
            elif jalur == "/api/distrik" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True, **self.db.daftar_distrik(pemain)})
            elif jalur == "/api/pindah-distrik" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.pindah_distrik(
                    pemain, int(badan.get("distrik_id", -1)))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/ajukan-izin" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.ajukan_izin(
                    pemain, int(badan.get("x", -1)), int(badan.get("y", -1)),
                    badan.get("bangunan"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/bayar-izin" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.bayar_izin(
                    pemain, int(badan.get("izin_id", -1)))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/sinkron" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.sinkron(
                    pemain, badan.get("koin"), badan.get("reputasi"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, "koin": hasil["koin"],
                                      "reputasi": hasil["reputasi"]})
            elif jalur == "/api/impor-tanah" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.impor_tanah(
                    pemain, badan.get("bangunan"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/bangun" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.bangun(
                    pemain, int(badan.get("x", -1)), int(badan.get("y", -1)),
                    badan.get("bangunan"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/jual" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.jual(
                    pemain, int(badan.get("x", -1)), int(badan.get("y", -1)))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/upgrade" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.upgrade_bangunan(
                    pemain, int(badan.get("x", -1)), int(badan.get("y", -1)))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/pindah" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.pindah_bangunan(
                    pemain, int(badan.get("x_lama", -1)), int(badan.get("y_lama", -1)),
                    int(badan.get("x", -1)), int(badan.get("y", -1)))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/pemain" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True,
                                  "pemain": self.db.profil(pemain["id"])})
            elif jalur == "/api/kas-kota" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True, **self.db.kas_kota()})
            elif jalur == "/api/serobot-tanah" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.serobot_tanah(
                    pemain, int(badan.get("x", -1)), int(badan.get("y", -1)))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/peristiwa" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True, **self.db.daftar_peristiwa(pemain["distrik_aktif"])})
            elif jalur == "/api/tindak-peristiwa" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.tindak_peristiwa(
                    pemain, int(badan.get("id", -1)), badan.get("tujuan"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/chat" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True, **self.db.daftar_chat(pemain["distrik_aktif"])})
            elif jalur == "/api/chat" and metode == "POST":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                hasil, salah = self.db.kirim_chat(pemain, badan.get("isi"))
                if salah:
                    self._kirim(400, {"ok": False, "kesalahan": salah})
                else:
                    self._kirim(200, {"ok": True, **hasil})
            elif jalur == "/api/leaderboard" and metode == "GET":
                pemain = self._butuh_pemain(query)
                if pemain is None:
                    return
                self._kirim(200, {"ok": True, **self.db.leaderboard()})
            elif jalur == "/":
                self._kirim(200, {"ok": True,
                                  "layanan": "Dunia Kita — Dunia Bersama",
                                  "versi": "7.0"})
            else:
                self._kirim(404, {"ok": False,
                                  "kesalahan": "Endpoint tidak dikenal."})
        except (ValueError, TypeError):
            self._kirim(400, {"ok": False,
                              "kesalahan": "Parameter tidak sah."})
        except Exception as e:  # noqa: BLE001 — jangan matikan server
            self.log_message("GALAT %s %s: %s", metode, jalur, e)
            self._kirim(500, {"ok": False,
                              "kesalahan": "Galat server internal."})


def main():
    parser = argparse.ArgumentParser(
        description="Server Dunia Bersama — Dunia Kita (Fase 7)")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=8471)
    parser.add_argument("--db", default="dunia-kita.db")
    args = parser.parse_args()
    Penangan.db = DuniaDB(args.db)
    server = ThreadingHTTPServer((args.host, args.port), Penangan)
    print("DUNIAKITA_SIAP port=%d db=%s" % (args.port, args.db), flush=True)
    print("Server Dunia Bersama jalan di http://%s:%d (Ctrl+C berhenti)"
          % (args.host, args.port), flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.shutdown()
        Penangan.db.tutup()


if __name__ == "__main__":
    main()
