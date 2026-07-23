using UnityEditor;
using System.IO;

public class BuildAndroid
{
    public static void Build()
    {
        string[] scenes = new string[] {
            "Assets/Scenes/LoadingScene.unity",
            "Assets/Scenes/LoginScene.unity", 
            "Assets/Scenes/MainMenuScene.unity",
            "Assets/Scenes/GameScene.unity"
        };
        
        string buildPath = "/workspace/PixelSurvival/Build/LostBlueSea.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
        
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
        UnityEngine.Debug.Log("Build completed: " + buildPath);
    }
}
