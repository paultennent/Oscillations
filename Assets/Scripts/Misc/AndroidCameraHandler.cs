using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidCameraHandler: MonoBehaviour 
{

    static AndroidCameraHandler sSingleton;
    public static AndroidCameraHandler GetInstance()
    {
        return sSingleton;
    }

    public string FLASH_RUNNING="0000";
    public string FLASH_DISCONNECT_ONE="110011001100";
    public string FLASH_DISCONNECT_ALL="1";
    public string FLASH_FINISHING="1000000000";
    public string FLASH_FINISHED= "1010100000";

    bool isScanningCode=false;
    bool flashOn=false;
    bool lastFlash=false;
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
        if(pauseStatus==true)
        {
            flashOn=false;
            flashPattern=null;
            setCamera();
        }
    }
    
    void setCamera()
    {
    #if UNITY_ANDROID  && !UNITY_EDITOR
        if(isScanningCode==false)
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
    CodeProcessor mProcessor=null;
    AndroidJavaObject mCodeCam=null;
    AndroidJavaObject mCodeDetector=null;
    
    public void initCodeCapture()
    {
        isScanningCode=true;
        detectedCode="";
//CameraSource.Builder builder = new CameraSource.Builder(getApplicationContext(), barCodeDetector)
//                .setFacing(CameraSource.CAMERA_FACING_BACK)
//                .setRequestedPreviewSize(1600, 1024)
//                .setRequestedFps(15.0f);
        #if UNITY_ANDROID && !UNITY_EDITOR
        print("barcode: make processor");
        mProcessor=new CodeProcessor(this);
        print("barcode: get activity");
//        AndroidJavaObject context=new AndroidJavaClass("android.content.Context").CallStatic<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        print("barcode:begin");
//        BarcodeDetector barCodeDetector = new BarcodeDetector.Builder(context).build();
        AndroidJavaObject codeBuilder=new AndroidJavaObject("com.google.android.gms.vision.barcode.BarcodeDetector$Builder",context);
        print("barcode:builder "+codeBuilder);
        // only look for EAN13 barcodes (and ISBN for now so we can test on books)
//        codeBuilder.Call<AndroidJavaObject>("setBarcodeFormats",32|3);
        codeBuilder.Call<AndroidJavaObject>("setBarcodeFormats",(int)acceptBarcodeTypes);
        mCodeDetector=codeBuilder.Call<AndroidJavaObject>("build");
        print("barcode:detector "+mCodeDetector);
        //mCodeDetector.setProcessor(something);
        mCodeDetector.Call("setProcessor",mProcessor);
//        AndroidJavaObject codeDectector
        
        // our custom camera source which supports flash on
        AndroidJavaObject camBuilder=new AndroidJavaObject("com.mrl.flashcamerasource.CameraSource$Builder",context,mCodeDetector);
        camBuilder.Call<AndroidJavaObject>("setFacing",0);
        camBuilder.Call<AndroidJavaObject>("setRequestedFps",15.0f);
        camBuilder.Call<AndroidJavaObject>("setFocusMode","continuous-picture");
        camBuilder.Call<AndroidJavaObject>("setFlashMode","torch");
        print("barcode:cambuilder"+camBuilder);
        mCodeCam = camBuilder.Call<AndroidJavaObject>("build");
        print("barcode:mCodeCam"+mCodeCam);
        mCodeCam.Call<AndroidJavaObject>("start");
        #endif
    }

    public void stopCodeCapture()
    {
        if(isScanningCode)
        {
            
            isScanningCode=false;
            if(mCodeCam!=null)
            {
                mCodeCam.Call("release");                
                mCodeCam=null;
            }
        }
    }
    
    public string getDetectedCode()
    {
        return detectedCode;
    }
    
    public void clearDetectedCode()
    {
        detectedCode="";
    }

    public bool connectToSwing(string code)
    {
        currentSwing=code;
        AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject wifiSelector=new AndroidJavaObject("com.mrl.flashcamerasource.ClientWifiSelector");
        return wifiSelector.Call<bool>("SelectNetworkForBarcode",context,code);
    }
	
	// Update is called once per frame
	void Update () 
    {
        string str=getDetectedCode();
        if(str!=null && str.Length>0)
        {
            stopCodeCapture();
        }
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
