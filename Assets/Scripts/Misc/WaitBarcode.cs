using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitBarcode : MonoBehaviour {

    Transform originalParent;
    AndroidCameraHandler barcodeReader=null;
    bool active=true;
   
    bool hasSwingCode=false;
   
    public GameObject mTextObject;
    Text mText;
	// Use this for initialization
	void Start () 
    {
        originalParent=transform.parent;
        WaitForBarcode();
        mText=mTextObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        
        if(barcodeReader==null)
        {
            // first call - get barcode
            barcodeReader=AndroidCameraHandler.GetInstance();
            if(barcodeReader!=null)
            {
                barcodeReader.initCodeCapture();
            }
            if(barcodeReader.getCurrentSwing()==null)
            {
                FadeSphereScript.enableFader(false);
            }else
            {
                FadeSphereScript.enableFader(true);                
            }
        }
        if(barcodeReader!=null && active)
        {
            if(mText && barcodeReader.getCurrentSwing()==null)
            {
                // no swing yet, ask for that
                mText.text="SCAN \nSWING\n BARCODE";
            }else
            {
                mText.text="SCAN \nA RIDER OR SWING\nBARCODE";
                FadeSphereScript.changePauseColour(new Color(0,0,1));
            }
            string code=barcodeReader.getDetectedCode();
            if(Input.GetKeyDown("s"))
            {
                code="10010037";
            }else if(Input.GetKeyDown("r"))
            {
                code="20000987";
            }
            if(code!=null && code.Length>0)
            {
                // got a barcode
                // find the research data object
                // if the barcode is a user identifier for research 
                // then remove the barcode message
                // if the barcode is a swing identifier, then pair with the correct swing phone
                ResearchLogger r= ResearchLogger.GetInstance();
                if(r!=null)
                {
                    print("code:"+code);
                    if(IsUserBarcode(code) && barcodeReader.getCurrentSwing()!=null)
                    {
                        r.OnNewUser(code);
                        // found a user, hide us so the rest of the UI is functional
                        barcodeReader.stopCodeCapture();
                        transform.parent=null;
                        active=false;
                        print("Found user:"+code);
                        if(isAutoSceneBarcode(code))
                        {
                            // these barcodes auto start a scene
                            int sceneNum=0;
                            if(int.TryParse(code[0].ToString(),out sceneNum))
                            {
                                Selector.openSceneByNumber(sceneNum);
                                FadeSphereScript.changePauseColour(new Color(1,0,0));
                                
                            }
                        }
                    }else if(IsSwingBarcode(code))
                    {
                        barcodeReader.stopCodeCapture();
                        barcodeReader.connectToSwing(code);
                        // found a swing, need to pair to this swing
                        // but stick in the same UI for rider code capture
                        print("Found swing:"+code);
                        r.OnNewSwing(code);
                        barcodeReader.clearDetectedCode();
                        hasSwingCode=true;
                        FadeSphereScript.enableFader(true);                
                    }
                }                                
            }
        }
	}
    
    bool paused=false;
    
	void OnApplicationPause(bool paused)
    {
        if(paused)
        {
            if(active && barcodeReader!=null)
            {
                barcodeReader.stopCodeCapture();
            }
        }else
        {
            if(active && barcodeReader!=null)
            {
                barcodeReader.initCodeCapture();                
            }
        }
        
    }
    
    bool IsUserBarcode(string code)
    {
        if(code[0]!='1')return true;
        return false;
    }
    bool IsSwingBarcode(string code)
    {
        if(code[0]=='1')return true;
        return false;
    }
    bool isAutoSceneBarcode(string code)
    {
        if(code[0]!='1' && code[0]!='0')return true;
        return false;
    }    
    
    public void WaitForBarcode()
    {
        transform.parent=originalParent;
        active=true;
    }

}
