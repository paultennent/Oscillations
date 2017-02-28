using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitBarcode : MonoBehaviour {

    Transform originalParent;
    AndroidCameraHandler barcodeReader=null;
    bool active=true;
    
	// Use this for initialization
	void Start () 
    {
        originalParent=transform.parent;
        WaitForBarcode();
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
        }
        if(barcodeReader!=null && active)
        {
            string code=barcodeReader.getDetectedCode();
            if(Input.GetKeyDown("x"))
            {
                code="912030";
            }else if(Input.GetKeyDown("c"))
            {
                code="31232";
            }else if(Input.GetKeyDown("v"))
            {
                code="1234";
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
                    if(IsUserBarcode(code))
                    {
                        r.OnNewUser(code);
                        // found a user, hide us so the rest of the UI is functional
                        barcodeReader.stopCodeCapture();
                        transform.parent=null;
                        active=false;
                        print("Found user:"+code);
                    }else if(IsSwingBarcode(code))
                    {
                        // found a swing, need to pair to this swing
                        // but stick in the same UI
                        print("Found swing:"+code);
                        r.OnNewSwing(code);
//                        PairWithSwing(code);
                    }
                }                                
            }
        }
	}
    
    bool IsUserBarcode(string code)
    {
        if(code[0]=='9')return true;
        return false;
    }
    bool IsSwingBarcode(string code)
    {
        if(code[0]=='3')return true;
        return false;
    }
    
    public void WaitForBarcode()
    {
        transform.parent=originalParent;
        active=true;
    }

}
