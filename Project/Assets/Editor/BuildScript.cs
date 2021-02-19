using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;


namespace Editor
{
    public static class BuildScript
    {
        static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        static IEnumerable<string> GetAllScenePaths()
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    yield return scene.path;
                }
            }
        }
        static bool isToolBar = false;
        public static void Build()
        {
            var outputPath = "../out/WebGL";
            Console.WriteLine($"{nameof(outputPath)}: {Path.GetFullPath(outputPath)}");
            CreateFolder(outputPath);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                options = BuildOptions.None,
                scenes = GetAllScenePaths().ToArray(),
                target = BuildTarget.WebGL,
                locationPathName = outputPath,
            });

            if (isToolBar) { return; }

            EditorApplication.Exit(
                report.summary.result == BuildResult.Succeeded ? 0 : 1
            );
        }
        [MenuItem("ビルド/アプリをビルド")]
        static void BuildApp() {
            isToolBar = true;
            Build();
        }
    }
}
