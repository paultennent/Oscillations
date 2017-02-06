using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidFlasher : MonoBehaviour 
{

    public string FLASH_RUNNING="0000";
    public string FLASH_DISCONNECT_ONE="110011001100";
    public string FLASH_DISCONNECT_ALL="1";
    public string FLASH_FINISHING="1000000000";
    public string FLASH_FINISHED= "1010100000";

    bool flashOn=false;
    bool lastFlash=false;
    
    const float FLASH_STEP_TIME=0.1f;
    float currentTime=0;
    string flashPattern=null;

    AndroidJavaObject mCamera=null;
    
	// Use this for initialization
	void Start () {
	}
    
    public void setFlashPattern(string pat)
    {
        if(pat!=flashPattern || pat==null)
        {
            if(pat!=null)
            {
                print("new pattern:"+pat+":"+pat.Length);
            }else
            {
                print("no flash pattern set");
            }
            currentTime=0f;
        }
        flashPattern=pat;
    }

    void OnApplicationPause( bool pauseStatus )
    {
        if(pauseStatus==true)
        {
            flashOn=false;
            flashPattern=null;
            setCamera();
        }
    }
    
    void setCamera()
    {
        if(flashOn!=lastFlash)
        {
            if(mCamera==null)
            {
                AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                mCamera=activity.Call<AndroidJavaObject>("getSystemService","camera");
            }
            if(mCamera!=null)
            {
                mCamera.Call("setTorchMode","0",flashOn);
            }
            lastFlash=flashOn;
        }
    }
	
	// Update is called once per frame
	void Update () 
    {

        if(flashPattern!=null && flashPattern.Length>0)
        {
            currentTime+=Time.deltaTime;        
            int stepPos=((int) (currentTime/FLASH_STEP_TIME)) % flashPattern.Length;
            flashOn=(flashPattern[stepPos]=='1');
        }else
        {
            flashOn=false;
        }
        setCamera();
	}
}
