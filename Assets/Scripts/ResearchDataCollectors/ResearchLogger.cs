using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.SceneManagement;

public class ResearchLogger : MonoBehaviour {

    static ResearchLogger sSingleton;
    public static ResearchLogger GetInstance()
    {
        return sSingleton;
    }    
    
    private bool recording=false;    

    private string currentUser="";
    private string currentSwing="";
    private string currentScene="";
    private LocationInfo swingLocation;
    private bool hasLocation=false;
    private float startTime=0f;
    private string deviceID="";

    private StreamWriter summaryFile;
    private BinaryWriter perFrameFile;
    
    private MagicReader mr;

    
    SwingNetwork mNet;
    
	// Use this for initialization
	void Start () {
        if(sSingleton==null)
        {
            DontDestroyOnLoad(transform.gameObject);
            sSingleton=this;
            #if UNITY_ANDROID && !UNITY_EDITOR
            deviceID=File.ReadAllText("/sdcard/deviceid.txt");
            deviceID=deviceID.Trim(new char[]{'\n'});
            #endif
            mNet=GetComponent<SwingNetwork>();
            mr = GameObject.FindGameObjectWithTag ("Controller").GetComponent<MagicReader> ();
            
        }else
        {
            // only allow one of us to exist
            Destroy(gameObject);
        }		
	}
    

    float[] frameData=new float[16];

	
	// Update is called once per frame 
	void Update () {
        AbstractGameEffects gameEffects=AbstractGameEffects.GetSingleton();
                
        SwingNetwork.SwingInfo info=mNet.GetSwingInfoObject();
        info.swingID=currentSwing;
        info.riderID=currentUser;
        if(gameEffects!=null)
        {
            info.inSession=gameEffects.inSession;
            info.rideTime=gameEffects.offsetTime;
        }else
        {
            info.inSession=false;
            info.rideTime=0f;
            info.swingAngle=0f;
        }
        if(mr!=null)
        {
            info.headsetBattery=mr.getLocalBatteryLevel ()*100f;
            info.swingBattery=mr.getRemoteBatteryLevel () *100f;
            info.connectionState=mr.getConnectionState();
            info.swingAngle=mr.getAngle();
        }
        
        
#if UNITY_ANDROID && !UNITY_EDITOR
        if(SceneManager.GetActiveScene().name!=currentScene)
        {
            OnNewScene(SceneManager.GetActiveScene().name);
        }

        if(Input.location.status==LocationServiceStatus.Running)
        {
            // got a location
            swingLocation=Input.location.lastData;
            Input.location.Stop();
            hasLocation=true;
        }
		if(gameEffects!=null && gameEffects.inSession)
        {
            if(!recording)
            {
                recording=true;
                OnStartRecord();
            }
            // swing and game parameters
            frameData[0]=Time.time;
            frameData[1]=gameEffects.swingAngle;
            frameData[2]=gameEffects.offsetTime;
            frameData[3]=gameEffects.climaxRatio;
            // camera position
            Transform camPos=Camera.main.transform;
            frameData[4]=camPos.position.x;
            frameData[5]=camPos.position.y;
            frameData[6]=camPos.position.z;
            // camera rotation
            frameData[7]=camPos.rotation.w;
            frameData[8]=camPos.rotation.x;
            frameData[9]=camPos.rotation.y;
            frameData[10]=camPos.rotation.z;
            
            
            // head look angle
            Quaternion headLook=InputTracking.GetLocalRotation(VRNode.Head);
            frameData[11] = headLook.w;
            frameData[12] = headLook.x;
            frameData[13] = headLook.y;
            frameData[14] = headLook.z;
            frameData[15]= -9999999f;// spare - put this in just in case files get broken, allow syncing (and make it 16x4 bytes per write)
            foreach(float d in frameData)
            {
                perFrameFile.Write(d);
            }
        }else
        {
            if(recording)
            {
                recording=false;
                OnStopRecord();
            }        
        }            
#endif
	}
    
    public void OnStartRecord()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        print("Open logs");
        startTime=Time.time;
        string saveFolder= "/sdcard/vrplayground-logs/";
        
        string curTime=DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string saveTag=curTime+"-"+currentSwing+"-"+deviceID;
        // open per frame data log (raw binary)
        // write per frame data log header (just a fixed file magic code)
        perFrameFile = new BinaryWriter(File.Open(saveFolder+saveTag+".bin", FileMode.Create));
        byte[] logHeader={79,83,67,73,76,79,71,49};
        perFrameFile.Write(logHeader);
        // open game summary log (text file)
        summaryFile = new System.IO.StreamWriter(saveFolder+saveTag+".txt");        
        // write location
        // write user code
        summaryFile.WriteLine("user,"+currentUser);
        // write swing code
        summaryFile.WriteLine("swing,"+currentSwing);
        // write start time
        summaryFile.WriteLine("startTime,"+curTime);
        // write current scene 
        summaryFile.WriteLine("scene,"+currentScene);                        
        // write phone ID
        summaryFile.WriteLine("device,"+deviceID);                        
#endif
    }
    
    public void OnStopRecord()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if(summaryFile!=null)
        {
            // write location to game log
            if(hasLocation)
            {
                summaryFile.WriteLine("latitude,"+swingLocation.latitude);                
                summaryFile.WriteLine("longitude,"+swingLocation.longitude);               
            }
            
            string curTime=DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            // write end timestamp to game log
            summaryFile.WriteLine("endTime,"+curTime);
            // write length of game to game log
            summaryFile.WriteLine("gameTime,"+(Time.time-startTime));
            // trigger auto-upload to box service
            summaryFile.Close();
            summaryFile=null;
            print("Write summary file");
        }
        if(perFrameFile!=null)
        {
            perFrameFile.Close();
            print("Stop frame log");
        }
        
        // launch the box uploaderservice
        AndroidJavaClass activityClass;
        AndroidJavaObject activity, intent;

        activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        intent = new AndroidJavaObject("android.content.Intent");
        intent.Call<AndroidJavaObject>("setClassName","com.mrl.simplegyroclient","com.mrl.boxupload.BoxUploadService");
        activity.Call<AndroidJavaObject>("startService",intent);
        print("Doing box upload");
        
#endif        
    }        

    public void OnNewScene(string scene)
    {
        currentScene=scene;
        mr=null;
        mr = GameObject.FindGameObjectWithTag ("Controller").GetComponent<MagicReader> ();                
    }
    
    public void OnNewUser(string userCode)
    {
        currentUser=userCode;
    }
    
    public void OnNewSwing(string swingCode)
    {        
        hasLocation=false;
        currentSwing=swingCode;
        // capture swing location
        Input.location.Start(10,500);
    }
    
}
