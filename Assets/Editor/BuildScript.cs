using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Evetero.Editor
{
    public static class BuildScript
    {
        [MenuItem("Evetero/Build/Dev Build (Android)")]
        public static void BuildAndroid()
        {
            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/Android"));
            Directory.CreateDirectory(outputDir);

            string outputPath = Path.Combine(outputDir, "Evetero.apk");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogResult(report, "Android");
        }

        [MenuItem("Evetero/Build/Dev Build (iOS)")]
        public static void BuildIOS()
        {
            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/iOS"));
            Directory.CreateDirectory(outputDir);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = outputDir,
                target = BuildTarget.iOS,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            LogResult(report, "iOS");
        }

        private static string[] GetScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static void LogResult(BuildReport report, string platform)
        {
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[Evetero] {platform} build succeeded: {report.summary.outputPath} ({report.summary.totalSize / 1024} KB)");
            else
                Debug.LogError($"[Evetero] {platform} build failed with {report.summary.totalErrors} error(s).");
        }
    }
}
