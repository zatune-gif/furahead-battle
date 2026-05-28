using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    const string BuildOutput = "Build/WebGL";
    const string ZipOutput   = "Build/PeraperaBattle_WebGL.zip";

    public static void BuildWebGL()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        var scenes = new[]
        {
            "Assets/Scenes/TitleScene.unity",
            "Assets/Scenes/CharacterSelectScene.unity",
            "Assets/Scenes/BattleScene.unity",
        };

        var options = new BuildPlayerOptions
        {
            scenes           = scenes,
            locationPathName = BuildOutput,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("Build FAILED");
            return;
        }

        CreateZip();
        Debug.Log("Build + ZIP complete: " + ZipOutput);
    }

    static void CreateZip()
    {
        if (File.Exists(ZipOutput)) File.Delete(ZipOutput);

        using var archive = ZipFile.Open(ZipOutput, ZipArchiveMode.Create);

        var baseDir = Path.GetFullPath(BuildOutput);
        foreach (var file in Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories))
        {
            // フォワードスラッシュで統一（Linux/itch.io 対応）
            var entryName = Path.GetRelativePath(baseDir, file).Replace('\\', '/');
            archive.CreateEntryFromFile(file, entryName, System.IO.Compression.CompressionLevel.Optimal);
        }
    }
}
