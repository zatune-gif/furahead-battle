using UnityEditor;
using UnityEngine;

public static class WebGLBuilder
{
    public static void Build()
    {
        var scenes = new[]
        {
            "Assets/Scenes/TitleScene.unity",
            "Assets/Scenes/CharacterSelectScene.unity",
            "Assets/Scenes/BattleScene.unity",
        };

        var options = new BuildPlayerOptions
        {
            scenes      = scenes,
            locationPathName = "Build/WebGL",
            target      = BuildTarget.WebGL,
            options     = BuildOptions.None,
        };

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.defaultWebScreenWidth  = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;
        PlayerSettings.runInBackground = true;

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"WebGL Build succeeded: {report.summary.totalSize / 1024 / 1024} MB");
        else
            Debug.LogError($"WebGL Build FAILED: {report.summary.result}");
    }
}
