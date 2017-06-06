using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCapture;

[ExecuteInEditMode]
public class VideoSaver : MonoBehaviour 
{
    float startTime=0f;

    public bool exitOnFinish=false;
    public bool doRecording=false;
    bool startedRecording=false;
    bool stopPlaying=false;

#if UNITY_EDITOR    
     public void Start()
     {                  
         if(!UnityEditor.EditorApplication.isPlaying)
         {
             if(doRecording)
             {
                print("Destroying record object");
                 GameObject.DestroyImmediate(gameObject);
             }else
             {
                 doRecording=true;
                EditorApplication.isPlaying=true;
             }
         }             
     }
     
    void Awake() 
    {
         if(UnityEditor.EditorApplication.isPlaying)
         {
            print("onload");
            DontDestroyOnLoad(gameObject);
         }
    }
     
     public void StartRecord()
     {
         // find main camera
         GameObject c= Camera.main.gameObject;
         // add: video saver
        // add: audio saver
         VRCapture.VRCaptureAudio ac = c.AddComponent<VRCapture.VRCaptureAudio>();
         
         VRCapture.VRCaptureVideo vidCap=c.AddComponent<VRCapture.VRCaptureVideo>();
         vidCap.frameSize=VRCapture.VRCaptureVideo.FrameSizeType._960x540;
         vidCap.encodeQuality=VRCapture.VRCaptureVideo.EncodeQualityType.Low;
         // add: vrcapture object
         VRCapture.VRCapture vc=c.AddComponent<VRCapture.VRCapture>();
         vc.CaptureAudio=ac;
         vc.CaptureVideos=new VRCaptureVideo[]{vidCap};
         
         startTime=Time.time;
         print(VRCapture.VRCapture.Instance);
         VRCapture.VRCapture.Instance.RegisterCompleteDelegate(HandleCaptureFinish);
         VRCapture.VRCapture.Instance.StartCapture();
         startedRecording=true;
        Application.runInBackground=true;
        Application.targetFrameRate=30;
     }
     
     public void Update()
     {
         
         SessionManager sm=SessionManager.getInstance();
         if(!startedRecording)
         {
             if(sm!=null && sm.isInSession() )
             {
                StartRecord();
             }
         }
         
         print("SM:"+sm+":"+sm.isInSession());
         if(startedRecording && (Time.time-startTime>300f || (sm!=null && sm.isInSession()==false) ))
         {
             startedRecording=false;
             print("Finish capture");
            VRCapture.VRCapture.Instance.StopCapture();
         }
         if(stopPlaying)
         {
             print("Stop playing");
             if(exitOnFinish)
             {
                 EditorApplication.Exit(0);
                 print("Quit");
             }
             UnityEditor.EditorApplication.isPlaying =false;              
        }
     }
     
     void HandleCaptureFinish()     
     {
         stopPlaying=true;
     }
#endif
    
}
