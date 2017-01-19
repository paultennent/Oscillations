// in editor run from previously recorded csv file
#if UNITY_EDITOR 
#define ACCEL_LOGFILE 
#endif
using UnityEngine;
using UnityEngine.VR;

using System;
using System.IO;


public class AccelerometerGetter
{
    private bool firstTime=true;
    private int accelPos=0;
    private Quaternion directionCorrection;

    public TextAsset replayCSV;
    
    StreamWriter mLogWriter;
 
    class ReplayItem
    {
        public ReplayItem()
        {
            fwdAccel=0;
            magAccel=0;
            time=0;
            gyro=0;
            hasGyro=false;
        }
        
        public string getCSVLine()
        {
            return time+","+magAccel+","+fwdAccel+","+gyro+","+hasGyro+"\n";
        }
        
        public string getCSVTitle()
        {
            return "time,mag,fwd,gyro,hasGyro\n";
        }
        
        public float fwdAccel;
        public float magAccel;
        public float time;
        public float gyro;
        public bool hasGyro;
    };
    
    
#if ACCEL_LOGFILE
    float replayTime=0;
	int replayPos=0;
    ReplayItem [] mReplayItems;
#else
    float accelHistoryTime=0f;
    ReplayItem mLogItem=new ReplayItem();
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
        return Quaternion.Euler(90,0,0);
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

//            replayCSV=Resources.Load("ReplayData/swing-gyroonoff") as TextAsset;
//            replayCSV=Resources.Load("ReplayData/swing-yesgyro") as TextAsset;
            replayCSV=Resources.Load("ReplayData/gyrotest") as TextAsset;
//            replayCSV=Resources.Load("ReplayData/swing-20161205-153113") as TextAsset;
//            replayCSV=Resources.Load("ReplayData/bigswingers2") as TextAsset;
            if(replayCSV!=null)
            {
                string csvText=replayCSV.text;
                string []lines=csvText.Split('\n');
                mReplayItems=new ReplayItem[lines.Length-1];
                string []headings=lines[0].Split(',');
                
                int timeIndex=Array.IndexOf(headings,"time");
                int fwdIndex=Array.IndexOf(headings,"fwd");
                int magIndex=Array.IndexOf(headings,"mag");
                int gyroIndex=Array.IndexOf(headings,"gyro");
                int hasGyroIndex=Array.IndexOf(headings,"hasGyro");
                
                for(int c=1;c<lines.Length;c++)
                {
                    string[]values=lines[c].Split(',');
                    ReplayItem ri=new ReplayItem();
                    try
                    {
                        if(timeIndex!=-1)
                        {
                            ri.time=float.Parse(values[timeIndex]);
                        }
                        if(fwdIndex!=-1)
                        {
                            ri.fwdAccel=float.Parse(values[fwdIndex]);
                        }
                        if(magIndex!=-1)
                        {
                            ri.magAccel=float.Parse(values[magIndex]);
                        }
                        if(gyroIndex!=-1)
                        {
                            ri.gyro=float.Parse(values[gyroIndex]);
                        }
                        if(hasGyroIndex!=-1)
                        {
                            if(String.Compare(values[hasGyroIndex],"True")==0 || String.Compare(values[hasGyroIndex],"1")==0)
                            {
                                ri.hasGyro=true;
                            }
                        }
                    }catch(IndexOutOfRangeException e)
                    {
                    }catch(FormatException e)
                    {
                    }
                    
                    mReplayItems[c-1]=ri;
                }
                replayTime=mReplayItems[0].time;
            }
#endif            
        }
        accelPos=0;      
#if ACCEL_LOGFILE
    // get events for this frame
        replayTime+=Time.deltaTime;
#else
        directionCorrection=getCurrentDirection();        
        if(mLogWriter!=null)
        {
            mLogWriter.Flush();
        }    
#endif
    }
    // returns true if there are any acceleration events left
    public bool getAcceleration(out float mag,out float forwardAccel, out float timestamp)
    {
#if ACCEL_LOGFILE
        if(replayPos<(mReplayItems.Length-1) && mReplayItems[replayPos].time<replayTime )
        {
            replayPos++;
            if(replayPos<mReplayItems.Length)
            {
                mag=mReplayItems[replayPos].magAccel;
                timestamp=mReplayItems[replayPos].time;
                forwardAccel=mReplayItems[replayPos].fwdAccel;
                return true;
            }
        }
        mag=0;
        forwardAccel=0;
        timestamp=0;
        return false;
#else
        Vector3 rotatedAccel;
        Vector3 origAccel;
        if(accelPos<Input.accelerationEvents.Length)
        {
            AccelerationEvent accEvent=Input.accelerationEvents[accelPos];
            origAccel=new Vector3(accEvent.acceleration.x,accEvent.acceleration.y,-accEvent.acceleration.z);
            rotatedAccel=directionCorrection*origAccel;            
            accelHistoryTime+=accEvent.deltaTime;

            mag=Mathf.Sqrt(accEvent.acceleration.x*accEvent.acceleration.x+accEvent.acceleration.y*accEvent.acceleration.y+accEvent.acceleration.z*accEvent.acceleration.z);
            timestamp=accelHistoryTime;
            forwardAccel=rotatedAccel.z;

#if !ACCEL_LOGFILE            
            if(mLogWriter!=null)
            {
                mLogItem.fwdAccel=forwardAccel;
                mLogItem.magAccel=mag;
                mLogItem.time=accelHistoryTime;
                mLogWriter.Write(mLogItem.getCSVLine());
            }
#endif

            
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

    public bool fromLogFile()
    {
#if ACCEL_LOGFILE
        return true;
#else        
        return false;
#endif
    }
    
    public bool isWritingLogFile()
    {
#if ACCEL_LOGFILE
        return false;
#else
        if(mLogWriter!=null)
        {
            return true;
        }
        return false;
#endif
    }
    
    public void setLogExtraData(float gyro,bool hasGyro)
    {
#if !ACCEL_LOGFILE
        mLogItem.gyro=gyro;
        mLogItem.hasGyro=hasGyro;
#endif        
    }
    
    public void startLog(string name)
    {
#if !ACCEL_LOGFILE
        mLogWriter=new StreamWriter(name);
        mLogWriter.Write(mLogItem.getCSVTitle());
#endif        
    }
    
    public bool getLogExtraData(out float gyro,out bool hasGyro)
    {
#if ACCEL_LOGFILE
        
        if(replayPos<mReplayItems.Length)
        {
            gyro=mReplayItems[replayPos].gyro;
            hasGyro=mReplayItems[replayPos].hasGyro;
            return true;
        }
        gyro=0;
        hasGyro=false;
        return false;
#else        
        gyro=0;
        hasGyro=false;
        return false;
#endif
    }
    
    public void resetForward()
    {
        // do the reset of forward thing here        
     //   VRDevice.
        throw(new Exception());
    }
    
    
};

