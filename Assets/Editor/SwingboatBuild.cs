using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;

public class ScriptBatch 
{
    [MenuItem("Oscillations/Swingboat Build")]
    public static void BuildSwingboat()
    {
        SwingboatBuild(false);
    }

    [MenuItem("Oscillations/Swingboat Build and Run")]
    public static void BuildSwingboatAndRun()
    {
        SwingboatBuild(true);
    }
    
    static void SwingboatBuild(bool run)
    {
        // Get filename.
        string[] levels = new string[] {"Assets/SwingBoat.unity"};

        // List<string> scenes=new List<string>();
        // for(int c=0;c<20;c++)
        // {
            // string name = SceneUtility.GetScenePathByBuildIndex(c);                           
            // if(name==null || name.Length==0)break;
            // scenes.Add(name);
            // Debug.Log(name);
        // }
        // levels=scenes.ToArray();
        
        
        string path = EditorUtility.SaveFilePanel("Choose Location of Built Game", "","swingboat.apk", "apk");
        
        string productName=PlayerSettings.productName+"";
        PlayerSettings.productName="Swingboat";
        string androidProductID=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);        
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.mrl.swingboat");
        
        // Build player.
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes=levels;
        options.locationPathName=path;
        options.target=BuildTarget.Android;
        if(run)
        {
            options.options=BuildOptions.AutoRunPlayer;
        }else
        {
            options.options=BuildOptions.None;
        }
        options.targetGroup=BuildTargetGroup.Android;
        
        BuildPipeline.BuildPlayer(options);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,androidProductID);
        PlayerSettings.productName=productName;
    }
    
    [MenuItem("Oscillations/List scenes")]
    public static void ListScenes()
    {
        
        for(int c=0;c<20;c++)
        {
            string name = SceneUtility.GetScenePathByBuildIndex(c);                           
            if(name==null || name.Length==0)break;
            Debug.Log(name);
        }
        
    }
    
    
    

}
