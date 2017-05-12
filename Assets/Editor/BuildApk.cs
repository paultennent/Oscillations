using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
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
        AssetDatabase.Refresh();
        string targetAPK=GetArg("-targetPath");
        
        Debug.Log("Building Oscillate to path: "+targetAPK+".\nScenes:");
        List<string> scenes=new List<string>();
        for(int c=0;c<20;c++)
        {
            string name = SceneUtility.GetScenePathByBuildIndex(c);                           
            if(name==null || name.Length==0)break;
            scenes.Add(name);
            Debug.Log(name);
        }
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes=scenes.ToArray();
        options.locationPathName=targetAPK;
        options.target=BuildTarget.Android;
        options.options=BuildOptions.None;
        options.targetGroup=BuildTargetGroup.Android;
        BuildPipeline.BuildPlayer(options);
     }
}