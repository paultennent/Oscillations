using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using VRCapture;

public class VideoSaveEditorScript : MonoBehaviour 
{
    
    public static bool recording=false;
    public static bool started=false;
    public static float recordTime=0f;
    public static bool firstTime=true;
    
    [MenuItem("Joe/MakeVideo")]
     public static void MakeVideo ()
     {
        GameObject go=new GameObject("_VideoStartMarker");
        go.AddComponent<VideoSaver>();
     }

     public static void MakeVideoAndQuit ()
     {
        GameObject go=new GameObject("_VideoStartMarker");
        VideoSaver vs=go.AddComponent<VideoSaver>();
        vs.exitOnFinish=true;
     }

     
 }
