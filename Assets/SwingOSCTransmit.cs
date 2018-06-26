using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SwingOSCTransmit : MonoBehaviour {
    MagicReader reader;
    OSC osc;
    OscMessage msg;
    
    float hpCoefficient=0.98f;
    float twistFilter=0f;
    float lastTwist=-99999f;

	// Use this for initialization
	void Start () 
    {
        Input.gyro.enabled=true;                
        reader=GetComponent<MagicReader>();	
        osc=GetComponent<OSC>();
        msg=new OscMessage();

	}
	
    
    
    float angleDiff(float a,float b)
    {
        // now they are both in range 0-360
        a=(a+540f)%360;
        b=(b+540f)%360;
        float difference=a-b;
        if(difference<-180)
        {
            return 360+difference;
        }
        if(difference>180)
        {
            return -360+difference;
        }
        return difference;
    }
    
    float wrapPlusMinus180(float angle)
    {
        return (angle+180)%360 - 180;
    }
    
	// Update is called once per frame
	void Update () {
        if(Input.GetButtonDown("Tap"))
        {
            UnityEngine.XR.InputTracking.Recenter();
            
        }

        float twistAbsolute=reader.getMagDirection();


        // high pass filter on seat twist to get difference from normal
        if(lastTwist<=-1000f)
        {
            lastTwist=twistAbsolute;
            twistFilter=twistAbsolute;
        }else
        {
            // make sure twist is in range -180 to 180
            if(twistAbsolute>180f)
            {
                twistAbsolute-=360f;
            }
            
            // add or remove 360 so that twist is closest to filter values
            if(twistAbsolute<twistFilter-180f)
            {
                twistAbsolute+=360f;
            }else if(twistAbsolute>twistFilter+180f)
            {
                twistAbsolute-=360f;
            }
            // standard HP filter now
            twistFilter=hpCoefficient*(twistFilter+angleDiff(twistAbsolute,lastTwist));
            // make sure output is within -+180
            lastTwist=twistAbsolute;
        }

        msg.values.Clear();
        msg.values.Add(reader.getAngle());
        msg.values.Add(twistAbsolute);
        msg.values.Add(twistFilter);
        msg.values.Add(reader.getSwingTilt());
        msg.address="/seat";
        osc.Send(msg);
 
/*        msg.values.Clear();
        msg.values.Add(reader.getMagDirection());
        msg.address="/twist";
        osc.Send(msg);*/
        

        Quaternion headRotation=UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        Vector3 eRotation=headRotation.eulerAngles;

        msg.values.Clear();
        msg.values.Add(wrapPlusMinus180(eRotation.x));
        msg.values.Add(wrapPlusMinus180(eRotation.y));
        msg.values.Add(wrapPlusMinus180(eRotation.z));
        msg.address="/head";
        osc.Send(msg);
	}
}
