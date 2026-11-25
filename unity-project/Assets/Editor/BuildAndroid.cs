// Dunia Kita — Assets/Editor/BuildAndroid.cs
// Skrip build EDITOR (bukan runtime): file di Assets/Editor/ hanya
// dikompilasi Unity untuk Editor, tidak ikut ke game di HP.
// Dipanggil dari build-android.bat di root project (atau dari menu
// File > Build Settings bila mau manual). Dua metode statis:
//   BuildAndroid.BuildAPK — APK debug untuk uji di HP.
//   BuildAndroid.BuildAAB — AAB release untuk Play Store.
// Keduanya memakai BuildPipeline.BuildPlayer, hanya scene MainScene,
// keluaran di C:\Dunia Kita\build\. Versi diambil dari
// GameBalance.VersiGame supaya .bat & Player Settings selaras.
// CATATAN: sebelum BuildAAB pertama, buat keystore dulu di
// Player Settings > Publishing Settings > Keystore Manager
// (tanpa keystore, build AAB gagal). Naikkan Bundle Version Code
// tiap rilis (Player Settings > Android > Other Settings).
// Verifikasi kompilasi tanpa Editor: tools/unity-editor-stubs.cs
// (mcs) — lihat CATATAN-VERIFIKASI.md § Fase 6.

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAndroid
{
    private const string FolderBuild = @"C:\Dunia Kita\build";
    private const string NamaApk = "DuniaKita-v1.0.apk";
    private const string NamaAab = "DuniaKita-v1.0.aab";

    private static string[] SceneBuild
    {
        get { return new string[] { "Assets/Scenes/MainScene.unity" }; }
    }

    private static BuildPlayerOptions OpsiDasar(string keluaran)
    {
        return new BuildPlayerOptions
        {
            scenes = SceneBuild,
            locationPathName = Path.Combine(FolderBuild, keluaran),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
    }

    private static void Bangun(bool appBundle, string keluaran)
    {
        PlayerSettings.bundleVersion = GameBalance.VersiGame;
        EditorUserBuildSettings.buildAppBundle = appBundle;
        Directory.CreateDirectory(FolderBuild);
        BuildReport laporan = BuildPipeline.BuildPlayer(OpsiDasar(keluaran));
        BuildSummary ringkasan = laporan.summary;
        Debug.Log("[Build] " + keluaran + " -> " + ringkasan.result
            + ", ukuran " + ringkasan.totalSize + " byte, "
            + ringkasan.totalErrors + " error.");
        if (ringkasan.result != BuildResult.Succeeded)
        {
            throw new Exception("Build gagal: " + ringkasan.result);
        }
    }

    public static void BuildAPK()
    {
        Bangun(false, NamaApk);
    }

    public static void BuildAAB()
    {
        Bangun(true, NamaAab);
    }
}
