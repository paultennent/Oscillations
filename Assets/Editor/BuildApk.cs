using UnityEditor;
using UnityEngine;
class BuildApk
{
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
    
     static void PerformBuild ()
     {
        Debug.Log("woooo!");
        string targetAPK=GetArg("-targetPath");
        string[] scenes = { "Assets/Rides/Menu.unity", "Assets/Rides/HighRoller-City-tiled.unity","Assets/Rides/Shuttlecock-City-Static.unity"};
        Debug.Log("Building Oscillate to path: "+targetAPK+".\nScenes:"+scenes);
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes=scenes;
        options.locationPathName=targetAPK;
        options.target=BuildTarget.Android;
        options.options=BuildOptions.None;
        BuildPipeline.BuildPlayer(options);
     }
}