// in editor run from previously recorded csv file
#if UNITY_EDITOR 
#define ACCEL_LOGFILE 
#endif
using UnityEngine;
using UnityEngine.VR;

using System;


public class AccelerometerGetter
{
    private bool firstTime=true;
    private int accelPos=0;
    private Quaternion directionCorrection;

    public TextAsset replayCSV;

#if ACCEL_LOGFILE
    float replayTime=0;
	int replayPos=0;
    float [] accelReplayData;
    float [] accelReplayTimes;
#else
    float accelHistoryTime=0f;

#endif
    
    public Quaternion getCurrentDirection()
    {
        if(VRDevice.isPresent)
        {
            return InputTracking.GetLocalRotation(VRNode.Head);
//            return Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.Head));
        }
        // for cardboard VR
/*        if(GvrViewer.Instance!=null)
        {
            return GvrViewer.Instance.HeadPose.Orientation;
//            return Quaternion.Inverse(GvrViewer.Instance.HeadPose.Orientation);
        }*/
        return Quaternion.identity;
/*        if(mForwardsDirection==Quaternion.identity)
        {
                mForwardsDirection=Quaternion.Euler(0,0,360-Input.compass.magneticHeading);
        }

        
        Quaternion currentDirection=Quaternion.identity;
        Quaternion currentTilt=Quaternion.identity;
        // get current head direction (i.e. z direction of phone)
        Quaternion attitude=Input.gyro.attitude;
        currentTilt=attitude;
        currentDirection=Quaternion.Euler(0,0,(360-Input.compass.magneticHeading)-attitude.eulerAngles.z);
        return currentTilt*currentDirection*mForwardsDirection;*/
    
    }

    // call this every frame, so we know the unity time (and can reset our position in Input.accelerationEvents
    public void onFrame(float unityTime)
    {
        if(firstTime)
        {
            // any initial startup (e.g. we need to grab a head direction or a dt or something)
            firstTime=false;
#if ACCEL_LOGFILE
                replayCSV=Resources.Load("ReplayData/bigswingers2") as TextAsset;
            Debug.Log(replayCSV);
            if(replayCSV!=null)
            {
                string csvText=replayCSV.text;
                string []lines=csvText.Split('\n');
                Debug.Log(lines[0]);
                accelReplayTimes=new float[lines.Length];
                accelReplayData=new float[lines.Length];
                float curTime=0;
                float curMag=0;
                for(int c=0;c<lines.Length;c++)
                {
                    string[] values=lines[c].Split(',');
                    if(values.Length>=2)
                    {
                        curTime=float.Parse(values[0]);
                        curMag=float.Parse(values[1]);
                    }
                    accelReplayTimes[c]=curTime;
                    accelReplayData[c]=curMag;
                }
                replayTime=(float)accelReplayTimes[0];
            }
#endif            
        }
        accelPos=0;      
#if ACCEL_LOGFILE
    // get events for this frame
        replayTime+=Time.deltaTime;
#else
        directionCorrection=getCurrentDirection();        
#endif
    }
    // returns true if there are any acceleration events left
    public bool getAcceleration(out float mag,out float forwardAccel, out float timestamp)
    {
#if ACCEL_LOGFILE
        if(replayPos<(accelReplayTimes.Length-1) && accelReplayTimes[replayPos]<replayTime )
        {
            replayPos++;
            if(replayPos<accelReplayData.Length && replayPos<accelReplayTimes.Length)
            {
                mag=(float)accelReplayData[replayPos];
                timestamp=(float)accelReplayTimes[replayPos];
                forwardAccel=0;
                return true;
            }
        }
        mag=0;
        forwardAccel=0;
        timestamp=0;
        return false;
#else
    if(accelPos<Input.accelerationEvents.Length)
        {
            AccelerationEvent accEvent=Input.accelerationEvents[accelPos];
            origAccel=new Vector3(accEvent.acceleration.x,accEvent.acceleration.y,-accEvent.acceleration.z);
            rotatedAccel=directionCorrection*origAccel;            
            accelHistoryTime+=accEvent.deltaTime;

            mag=Mathf.Sqrt(accEvent.acceleration.x*accEvent.acceleration.x+accEvent.acceleration.y*accEvent.acceleration.y+accEvent.acceleration.z*accEvent.acceleration.z);
            timestamp=accelHistoryTime;
            forwardAccel=rotatedAccel.z;
            
            accelPos++;
            return true;
        }else
        {
            mag=0;
            forwardAccel=0;
            timestamp=0;
            return false;
        }                
#endif        
    }
    
    public void resetForward()
    {
        // do the reset of forward thing here        
     //   VRDevice.
        throw(new Exception());
    }
    
    
};