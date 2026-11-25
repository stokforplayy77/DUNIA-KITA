# Conventions

## Guards (added after a mistake repeated)
- Report a test, build or fix as passing only after running it in the same task; quote the command and its result, and write "Not verified:" otherwise.
- Never overwrite the approved HD building sprites in `unity-project/Assets/Sprites/` (apartemen, balaikota, kafe, klinik, kost, pasar, pembangkit, pos, rumah, sekolah, taman, warung). `tools/generate-sprites.py` skips them through the `SPRITE_HD` set in `simpan()`; add a name there when a building gets HD art.

## Sprites
- Building art style: detailed isometric HD illustrations, not flat PIL boxes.
- HD sprites are 1024x1024 (1x1) or 2048x1024 (2x2) with `spritePixelsToUnits: 400` in the `.meta`; generated sprites use PPU 100 (256x256, 512x256, 768x384). World size must stay the same either way.
- HD sprites are produced from illustrations with `tools/proses-aset-anime.py` (background removed, base of the building at 92% of canvas height).
- `tools/qa-fase6.py` checks sprite size against the PPU in the `.meta`.
- Before finishing, stop the servers and background commands you started; long-lived work goes in a job or the preview.
