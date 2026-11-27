using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SectorX13.Editor
{
    /// <summary>
    /// Configures Android PlayerSettings and builds the Sector_X13.apk via Unity CLI.
    /// </summary>
    public static class X13AndroidBuild
    {
        [MenuItem("SECTOR X-13/Phase 8/Build Android APK")]
        public static void BuildAPK()
        {
            string tempSdk = Path.GetFullPath(Path.Combine("Builds", "TempAndroidSDK"));
            if (Directory.Exists(Path.Combine(tempSdk, "cmdline-tools", "latest")))
            {
                EditorPrefs.SetBool("SdkUseEmbedded", false);
                EditorPrefs.SetString("AndroidSdkRoot", tempSdk);
            }

            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.sectorx13.arlab");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            string buildDir = "Builds";
            Directory.CreateDirectory(buildDir);
            string apkPath = Path.Combine(buildDir, "Sector_X13.apk");

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[X13 Build] Android APK succeeded: {apkPath} ({summary.totalSize} bytes)");
            }
            else
            {
                Debug.LogError($"[X13 Build] Android APK failed: {summary.result}, errors: {summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }
    }
}
