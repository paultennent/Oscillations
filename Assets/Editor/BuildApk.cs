using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

class BuildApk
{
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log(args[i]);
            if (args[i].Equals(name) && args.Length > i + 1)
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

     static void PerformWebBuilds()
     {
        AssetDatabase.Refresh();
        string targetFolder=GetArg("-targetPath");
        
        Debug.Log("Building Oscillate to path: "+targetFolder+"\nScenes:");
        List<string> allScenes=new List<string>();
        string starterScene="";
        for(int c=0;c<20;c++)
        {
            string name = SceneUtility.GetScenePathByBuildIndex(c);                           
            if(name==null || name.Length==0)break;
            if(name.IndexOf("ReplayStarter")==-1)
            {
                allScenes.Add(name);
            }else
            {
                starterScene=name;
            }
            Debug.Log(name);
        }
        for(int c=0;c<allScenes.Count;c++)
        {
            string shortName=Path.GetFileNameWithoutExtension(allScenes[c]);
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes=new string[]{starterScene,allScenes[c]};
            options.locationPathName=targetFolder+Path.DirectorySeparatorChar+shortName;            
            options.target=BuildTarget.WebGL;
            options.options=BuildOptions.None;

            Debug.Log("Building Oscillate scene:  to path: "+options.locationPathName+".\nScenes:"+options.scenes[0]+","+options.scenes[1]);
            BuildPipeline.BuildPlayer(options);
        }
     }

    [MenuItem("Oscillations/Cardboard Build")]
    public static void BuildCardboard()
    {
        // Get filename.
        string path = EditorUtility.SaveFilePanel("Choose Location of Built Game", "","oscillations-cardboard.apk", "apk");
        AssetDatabase.Refresh();
        string targetAPK=GetArg("-targetPath");

        string productName=PlayerSettings.productName;
        PlayerSettings.productName="Oscillate-Diffusion";
        string androidProductID=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);        
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.mrl.swingdiff");

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
        options.locationPathName=path;
        options.target=BuildTarget.Android;
        options.options=BuildOptions.None;
        options.targetGroup=BuildTargetGroup.Android;

        string[] vrSDKs=PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.Android);
        PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android,new string[]{"Cardboard"});
        BuildPipeline.BuildPlayer(options);
     
        PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android,vrSDKs);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,androidProductID);
        PlayerSettings.productName=productName;

    }
     
}