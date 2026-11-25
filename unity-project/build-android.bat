@echo off
rem Dunia Kita - build Android dari baris perintah (Fase 6).
rem Double-klik berkas ini, pilih 1 untuk APK debug (uji di HP)
rem atau 2 untuk AAB release (Play Store). Build pertama agak
rem lama (10-20 menit). Bila gagal, baca C:\Dunia Kita\build.log.
rem CATATAN: sebelum AAB pertama, buat keystore dulu di Unity:
rem Player Settings - Publishing Settings - Keystore Manager.
rem Naikkan Bundle Version Code tiap rilis.
setlocal
set PROYEK=C:\Dunia Kita\unity-project
set LOG=C:\Dunia Kita\build.log
set UNITY_EXE=C:\Program Files\Unity\Hub\Editor\2022.3.20f1\Editor\Unity.exe
if exist "%UNITY_EXE%" goto :menu
echo Unity 2022.3.20f1 tidak ditemukan di:
echo   %UNITY_EXE%
echo Periksa instalasi Unity Hub, lalu ubah baris UNITY_EXE di .bat ini.
goto :akhir
:menu
echo ===============================
echo  Build Android - Dunia Kita
echo ===============================
echo  1. APK debug (uji di HP)
echo  2. AAB release (Play Store)
echo  3. Keluar
set /p PILIH=Pilih 1, 2, atau 3: 
if "%PILIH%"=="1" goto :apk
if "%PILIH%"=="2" goto :aab
goto :akhir
:apk
set METODE=BuildAndroid.BuildAPK
set HASIL=C:\Dunia Kita\build\DuniaKita-v1.0.apk
goto :bangun
:aab
echo Pastikan keystore sudah dibuat di Player Settings - Publishing Settings.
set METODE=BuildAndroid.BuildAAB
set HASIL=C:\Dunia Kita\build\DuniaKita-v1.0.aab
goto :bangun
:bangun
echo.
echo Membangun %HASIL% ...
echo Log: %LOG%
if not exist "C:\Dunia Kita\build" mkdir "C:\Dunia Kita\build"
"%UNITY_EXE%" -quit -batchmode -nographics -projectPath "%PROYEK%" -executeMethod %METODE% -logFile "%LOG%"
if exist "%HASIL%" goto :sukses
echo.
echo BUILD GAGAL - hasil tidak ditemukan.
echo Penyebab umum: Android Build Support belum terpasang,
echo keystore belum dibuat (untuk AAB), atau ruang disk habis.
echo Baca 20 baris terakhir %LOG% untuk detailnya.
goto :akhir
:sukses
echo.
echo BUILD BERHASIL: %HASIL%
goto :akhir
:akhir
echo.
pause
endlocal
