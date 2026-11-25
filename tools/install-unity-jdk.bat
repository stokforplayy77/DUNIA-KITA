@echo off
set LOG=C:\Users\ricar\unity-install-jdk.log
echo [%DATE% %TIME%] Mulai install modul OpenJDK (id lengkap) >> "%LOG%"
"C:\Program Files\Unity Hub\Unity Hub.exe" -- --headless install-modules --version 2022.3.20f1 -m android-open-jdk-11.0.14.1+1 >> "%LOG%" 2>&1
set EC=%ERRORLEVEL%
echo [%DATE% %TIME%] Selesai, exit code %EC% >> "%LOG%"
