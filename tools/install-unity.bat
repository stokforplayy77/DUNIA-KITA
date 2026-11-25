@echo off
set LOG=C:\Users\ricar\unity-install.log
echo [%DATE% %TIME%] Mulai install Unity 2022.3.20f1 + Android >> "%LOG%"
"C:\Program Files\Unity Hub\Unity Hub.exe" -- --headless install --version 2022.3.20f1 --changeset 61c2feb0970d --module android --module android-sdk-ndk-tools --module android-open-jdk >> "%LOG%" 2>&1
set EC=%ERRORLEVEL%
echo [%DATE% %TIME%] Selesai, exit code %EC% >> "%LOG%"
