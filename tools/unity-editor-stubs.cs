// STUB API UnityEditor — HANYA untuk compile-check BuildAndroid.cs
// di luar Unity (mcs). BUKAN bagian dari project game. Berisi
// signature minimal dari API UnityEditor yang dipakai
// Assets/Editor/BuildAndroid.cs, dengan nama & tipe yang sama seperti
// Unity 2022 LTS. Dipakai BERSAMA tools/unity-stubs.cs.
// Bila BuildAndroid.cs lolos kompilasi terhadap kedua stub ini,
// sintaks & pemakaian API-nya konsisten; perilaku build nyata tetap
// harus dites dari build-android.bat di laptop (lihat
// CATATAN-VERIFIKASI.md § Fase 6).

namespace UnityEditor
{
    public enum BuildTarget
    {
        Android = 13
    }

    public enum BuildTargetGroup
    {
        Android = 7
    }

    public enum BuildOptions
    {
        None = 0
    }

    public enum BuildResult
    {
        Unknown = 0,
        Succeeded = 1,
        Failed = 2,
        Cancelled = 3
    }

    public struct BuildPlayerOptions
    {
        public string[] scenes;
        public string locationPathName;
        public BuildTarget target;
        public BuildOptions options;
    }

    public class BuildSummary
    {
        public BuildResult result;
        public ulong totalSize;
        public int totalErrors;
    }

    public class BuildReport
    {
        public BuildSummary summary;
    }

    public static class BuildPipeline
    {
        public static BuildReport BuildPlayer(BuildPlayerOptions opsi)
        {
            return new BuildReport { summary = new BuildSummary() };
        }
    }

    public static class EditorUserBuildSettings
    {
        public static bool buildAppBundle { get; set; }
    }

    public static class PlayerSettings
    {
        public static string bundleVersion { get; set; }

        public static class Android
        {
            public static int bundleVersionCode { get; set; }
        }
    }
}
