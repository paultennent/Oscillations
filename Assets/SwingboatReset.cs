using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingboatReset : MonoBehaviour {

    // it also sends a reset to the swing sensor if back button is pressed
    // this does a reset if it detects that the swing sensor has been reset
    MagicReader reader;
    
    long lastTimestamp=0L;

	// Use this for initialization
	void Start () {
		reader=GetComponent<MagicReader>();		
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            // restart the clock
//            SessionManager.getInstance().reset();
            reader.sendSensorMessage(1);
        }
        
        long curTimestamp=reader.getRemoteTimestamp();
        // if we have been sent a force reset, then 
        if(curTimestamp<lastTimestamp)
        {
            print("Reset from server");
            SessionManager.getInstance().reset();
        }
        lastTimestamp=curTimestamp;
	}
}
