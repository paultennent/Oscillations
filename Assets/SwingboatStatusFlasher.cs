using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingboatStatusFlasher : MonoBehaviour {

    // it also sends a reset to the swing sensor if back button is pressed
    // this does a reset if it detects that the swing sensor has been reset
    MagicReader reader;
    AndroidCameraHandler ah;
    string flashPattern="";
    
    long lastTimestamp=0L;
    
    float timeDisconnected=0f;

	// Use this for initialization
	void Start () {
		reader=GetComponent<MagicReader>();
        ah=AndroidCameraHandler.GetInstance();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(ah!=null)
        {
            //print(reader.getLocalBatteryLevel()+":"+reader.getRemoteBatteryLevel());
            string newPattern=flashPattern;
            bool batteryLow=false;
            bool disconnected=false;
            // if battery is low on this phone or on the swing phone flash every 2 seconds
            if(reader.getLocalBatteryLevel()>=-1 && reader.getLocalBatteryLevel()<0.3f && reader.getRemoteBatteryLevel()<0.3f && reader.getRemoteBatteryLevel()>-1)
            {
                // low battery
                newPattern="10000000000";
                batteryLow=true;
            }            
            if(reader.getConnectionState()==0)
            {
                timeDisconnected+=Time.deltaTime;
                if(timeDisconnected>10f)
                {
                    // flash 3 times quickly then pause then 3 times...
                    newPattern="10101000000";
                    disconnected=true;
                }
            }else
            {
                timeDisconnected=0;
            }
            if(!batteryLow && !disconnected)
            {
                newPattern="0";
            }
            if(flashPattern!=newPattern)
            {
                flashPattern=newPattern;
                ah.setFlashPattern(flashPattern);
            }
        }
	}
}
