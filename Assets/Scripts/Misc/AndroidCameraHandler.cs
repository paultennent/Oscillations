using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidCameraHandler: MonoBehaviour 
{
    const bool useCamera2=false;

    static AndroidCameraHandler sSingleton;
    public static AndroidCameraHandler GetInstance()
    {
        return sSingleton;
    }

    public string FLASH_RUNNING="0000";
	public string FLASH_DISCONNECT_ONE="0000";
	public string FLASH_DISCONNECT_ALL="1";
	public string FLASH_FINISHING="0000";
	public string FLASH_FINISHED= "1010100000";

    bool isScanningCode=false;
    bool flashOn=false;
    bool lastFlash=false;
    bool paused=false;
    string currentSwing=null;
    
    
    const float FLASH_STEP_TIME=0.1f;
    float currentTime=0;
    string flashPattern=null;

    AndroidJavaObject mCamera=null;
    
    public enum CodeType 
    {
        CODETYPE_ALL=0,
        CODETYPE_EAN13=32,
        CODETYPE_EAN8 = 64,
        CODETYPE_QR_CODE=265,
        CODETYPE_ISBN=3
    };
    
    public CodeType acceptBarcodeTypes=CodeType.CODETYPE_EAN8;
    string detectedCode="";
    
    public string getCurrentSwing(){return currentSwing;}
    
	// Use this for initialization
	void Start () {
        if(sSingleton==null)
        {
            DontDestroyOnLoad(transform.gameObject);
            sSingleton=this;
    //        Application.stackTraceLogType = StackTraceLogType.None;
            //setFlashPattern("1100110000");
            //initCodeCapture();
        }else
        {
            // only allow one of us to exist
            Destroy(gameObject);
        }
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
        paused=pauseStatus;
        if(pauseStatus==true)
        {
            flashOn=false;
            if(!useCamera2)
            {
                if(mCamera!=null)
                {
                    mCamera.Call("release");
                    mCamera=null;
                }
            }
        }
    }
    
    void setCamera()
    {
    #if UNITY_ANDROID  && !UNITY_EDITOR
        if(isScanningCode==false && paused==false)
        {
            if(flashOn!=lastFlash)
            {
                if(mCamera==null)
                {
                    if(useCamera2)
                    {
                        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                        mCamera=activity.Call<AndroidJavaObject>("getSystemService","camera");
                    }else
                    {
                        AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
                        mCamera = cameraClass.CallStatic<AndroidJavaObject>("open", 0);
                        mCamera.Call("startPreview");
                    }
                }
                if(mCamera!=null)
                {
                    if(flashOn)
                    {
                        AndroidJavaObject cameraParameters = mCamera.Call<AndroidJavaObject>("getParameters");
                        cameraParameters.Call("setFlashMode", "torch");
                        mCamera.Call("setParameters", cameraParameters);                                                
                    }else
                    {
                        AndroidJavaObject cameraParameters = mCamera.Call<AndroidJavaObject>("getParameters");
                        cameraParameters.Call("setFlashMode", "off");
                        mCamera.Call("setParameters", cameraParameters);                                                
                    }
                }
                lastFlash=flashOn;
            }
        }else
        {
            flashOn=false;
            // release the camera so barcode scanning works
            if(!useCamera2)
            {
                if(mCamera!=null)
                {
                    mCamera.Call("release");
                    mCamera=null;
                }
            }
        }
        return;
    #endif
    }
    
    class CodeProcessor:AndroidJavaProxy
    {
        AndroidCameraHandler pThis;
        
        public CodeProcessor(AndroidCameraHandler owner) : base("com.google.android.gms.vision.Detector$Processor") 
        {
            pThis=owner;
        }
        
        public void	receiveDetections(AndroidJavaObject detections)
        {
            AndroidJavaObject items = detections.Call<AndroidJavaObject>("getDetectedItems");
            bool active=detections.Call<bool>("detectorIsOperational");
            int numCodes=items.Call<int>("size");
            for(int i=0;i<numCodes;i++)
            {
                AndroidJavaObject code = items.Call<AndroidJavaObject>("valueAt",i);
                int format=code.Get<int>("format");
                string value=code.Get<string>("displayValue");
                print("barcode: detect["+format+"]:"+value);
                pThis.detectedCode=value;
            }            
        }

        public void	release()
        {
            print("barcode: release");
        }
    };
    
    // barcode scanning objects    
    AndroidJavaObject mBarcodeReader;
    
    public void initCodeCapture()
    {
        if(!isScanningCode)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            isScanningCode=true;
            // clear any flash so we can use the camera
            setCamera();
            detectedCode="";
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            mBarcodeReader=new AndroidJavaObject("com.mrl.flashcamerasource.BarcodeReader");
            bool readingOk=mBarcodeReader.Call<bool>("startReading",context);
            print("Barcode start:"+readingOk);
            #endif
        }
        
    }

    public void stopCodeCapture()
    {
        if(isScanningCode)
        {
            isScanningCode=false;
            if(mBarcodeReader!=null)
            {
                print("Stop reading barcode");
                mBarcodeReader.Call("stopReading");                
                mBarcodeReader=null;
            }
        }
    }
    
    public string getDetectedCode()
    {
        if(mBarcodeReader==null)
        {
            initCodeCapture();
            return null;
        }
        return mBarcodeReader.Call<string>("getDetectedCode");
    }
    
    public void clearDetectedCode()
    {
        print("Clear barcode");
        string code=getDetectedCode();
        if(code!=null && code.Length>0)
        {
            print("Clearing code:"+code);
            stopCodeCapture();
            initCodeCapture();
        }
    }

    public bool connectToSwing(string code)
    {
        currentSwing=code;
        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject wifiSelector=new AndroidJavaObject("com.mrl.flashcamerasource.ClientWifiSelector");
        return wifiSelector.Call<bool>("SelectNetworkForBarcode",context,code);
        #else
        return true;
        #endif
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
