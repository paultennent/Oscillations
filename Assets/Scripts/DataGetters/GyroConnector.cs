#if UNITY_EDITOR 
#define REMOTE_SERVER
#endif
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;
using UnityEngine.VR;



public class GyroConnector  
{
    const int MIN_PACKET_SIZE=24;
#if REMOTE_SERVER
    const int MAX_PACKET_SIZE=24;
    float timeLastPoll=0;
    IPEndPoint serverEndPoint=new IPEndPoint(IPAddress.Parse("10.154.163.192"),2323);
#else
    const int MAX_PACKET_SIZE=32;
#endif

    public Socket receiver;
    byte[] receiveBytes=new byte[MAX_PACKET_SIZE];
    
    public float mAngle=0;
    public float mAngularVelocity=0;
    public float mMagDirection=0;
    public float mRemoteBatteryLevel=0;
    public float mLocalBatteryLevel=0;
    public long mTimestamp=0L;
    public int mConnectionState=0;
    
    public string dbgTxt="";
    
    public Quaternion mForwardsDirection=Quaternion.identity;
    
    public SwingTracker mTracker=new SwingTracker();

	public void init () 
    {

		doConnection ();
	}

	public void doConnection()
	{
		if (receiver == null) {
			receiver = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			receiver.Bind (new IPEndPoint (IPAddress.Any, 2424));

			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass activityClass;
			AndroidJavaObject activity, intent;

			activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
			Debug.Log("woo:1"+activity);
			intent = new AndroidJavaObject("android.content.Intent");
			Debug.Log("woo:2"+intent);
			intent.Call<AndroidJavaObject>("setClassName","com.mrl.simplegyroclient","com.mrl.simplegyroclient.GyroClientService");
			Debug.Log("woo:3"+intent);
			activity.Call<AndroidJavaObject>("startService",intent);
			#else
			timeLastPoll = Time.time;
			#endif
		}
	}


	public void pause(bool bPause)
	{
		if (bPause) {
			stop ();
		} else {
			doConnection ();
		}
	}    
    
    public void stop()
    {
		if (receiver != null) {
			receiver.Close ();
			receiver = null;
		}
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass activityClass;
        AndroidJavaObject activity, intent;

        activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log("woo:1"+activity);
        intent = new AndroidJavaObject("android.content.Intent");
        Debug.Log("woo:2"+intent);
        intent.Call<AndroidJavaObject>("setClassName","com.mrl.simplegyroclient","com.mrl.simplegyroclient.GyroClientService");
        Debug.Log("woo:3"+intent);
        activity.Call<AndroidJavaObject>("stopService",intent);
#endif
    }	
    // java is big endian, these are some conversion helpers
    
    byte[] conversionBuf4={0,0,0,0};
    byte[] conversionBuf8={0,0,0,0,0,0,0,0};
    float getBigEndianFloat(byte[]input,int offset)
    {
        if(BitConverter.IsLittleEndian)
        {
            Array.Copy(input,offset,conversionBuf4,0,4);
            Array.Reverse(conversionBuf4);
            return BitConverter.ToSingle(conversionBuf4,0);
        }else
        {
            return BitConverter.ToSingle(input,offset);
        }
    }
	int getBigEndianInt32(byte[]input,int offset)
    {
        if(BitConverter.IsLittleEndian)
        {
            Array.Copy(input,offset,conversionBuf4,0,4);
            Array.Reverse(conversionBuf4);
            return BitConverter.ToInt32(conversionBuf4,0);
        }else
        {
            return BitConverter.ToInt32(input,offset);
        }
        
    }
	long getBigEndianInt64(byte[]input,int offset)
    {
        if(BitConverter.IsLittleEndian)
        {
            Array.Copy(input,offset,conversionBuf8,0,8);
            Array.Reverse(conversionBuf8);
            return BitConverter.ToInt64(conversionBuf8,0);
        }else
        {
            return BitConverter.ToInt64(input,offset);
        }
    }
    
    EndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    float[] mSpareAngles=new float[10];
    int mSpareAngleCount=0;
    int mSpareAngleReadPos=0;

    
    class AngleTime
    {
        public float angle;
        public float time;
    };
    
    AngleTime []mBuffer=new AngleTime[10];
    int mBufferCount=0;
    
    bool firstTime=true;
    long firstTimestamp=0;
    float firstUnityTime=0;
    
    float unityDelay=0.0f;
    float accelHistoryTime=0f;

    private Quaternion getCurrentDirection()
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
        if(mForwardsDirection==Quaternion.identity)
        {
                mForwardsDirection=Quaternion.Euler(0,0,360-Input.compass.magneticHeading);
/*                Quaternion attitude=Input.gyro.attitude;                
            if(attitude.x!=0 || attitude.y!=0 || attitude.z!=0 || attitude.w!=0)
            {
                // get it from gyro once gyro has something worth having
                // don't care about the up/down-ness of it
                mForwardsDirection=Quaternion.Euler(0,0,attitude.eulerAngles.z);
            }*/
        }

        
        Quaternion currentDirection=Quaternion.identity;
        Quaternion currentTilt=Quaternion.identity;
        // get current head direction (i.e. z direction of phone)
        Quaternion attitude=Input.gyro.attitude;
        currentTilt=attitude;
        currentDirection=Quaternion.Euler(0,0,(360-Input.compass.magneticHeading)-attitude.eulerAngles.z);
        return currentTilt*currentDirection*mForwardsDirection;

    
    }
    
	public void readData() 
    {
    #if REMOTE_SERVER
    //   if we're running in editor, need to poll server to get messages
        if(Time.time-timeLastPoll>0.5)
        {
            byte[] launchPacket={1,2,3,4};
            receiver.SendTo(launchPacket,serverEndPoint);
            Debug.Log("Polling gyro");
            timeLastPoll=Time.time;
        }
    #endif


        int count=0;
        // receive everything 
        // send it out as fast as possible with zero missing frames
        
    	while(receiver.Available>=MIN_PACKET_SIZE)
        {
            int len=receiver.ReceiveFrom(receiveBytes,ref remoteIpEndPoint);
            if(len>=MIN_PACKET_SIZE)
            {
                float angle=getBigEndianFloat(receiveBytes,0);
                long timestamp=getBigEndianInt64(receiveBytes,16);
                if(firstTime)
                {
                    firstTime=false;
                    firstTimestamp=getBigEndianInt64(receiveBytes,16);
                    firstUnityTime=Time.time;
                    mAngle=angle;
                    mTimestamp=timestamp;
                    for(int c=0;c<mBuffer.Length;c++)
                    {
                        mBuffer[c]=new AngleTime();
                    }
                }
                float timestampInUnity=firstUnityTime+0.000000001f*(float)(timestamp-firstTimestamp);
    //            Debug.Log(timestampInUnity+"!!!");
                if(mBufferCount<mBuffer.Length)
                {
                    mBuffer[mBufferCount].angle=angle;
                    mBuffer[mBufferCount].time=timestampInUnity;
                    mBufferCount++;
                }
                mAngularVelocity=getBigEndianFloat(receiveBytes,4);
                mMagDirection=getBigEndianFloat(receiveBytes,8);
                mRemoteBatteryLevel=getBigEndianFloat(receiveBytes,12);
                if(len>=28)
                {
                    mLocalBatteryLevel=getBigEndianFloat(receiveBytes,24);
                }
                if(len>=32)
                {
                    mConnectionState=getBigEndianInt32(receiveBytes,28);
                }
            }            
        }
        
        bool hasAngle=true;
        
        if(mBufferCount>0)
        {
            float unityTimeNow=Time.time-firstUnityTime - unityDelay;
  //          Debug.Log(unityTimeNow+":"+mBuffer[0].time+"!"+mBufferCount+"["+mBuffer[0].angle);
            int bufPos=0;
            while(bufPos<mBuffer.Length && bufPos<mBufferCount && mBuffer[bufPos].time<unityTimeNow)
            {
                mAngle=mBuffer[bufPos].angle;
                bufPos++;
            }
            // shift used points in buffer off
            int d=0;
            for(int c=bufPos;c<mBufferCount;c++)
            {
                mBuffer[d].angle=mBuffer[c].angle;
                mBuffer[d].time=mBuffer[c].time;
                d++;
            }
            mBufferCount-=bufPos;
            if(mBufferCount>3)
            {
                // big buffer, drop latency
                unityDelay-=0.01f;
//              Debug.Log(unityTimeNow+":"+mBuffer[0].time+"!"+mBufferCount+"["+mBuffer[0].angle);

            }
            if(mBufferCount<2)
            {
                // risk of dropping frames, up latency slightly
                unityDelay+=0.001f;
            }
            if(mBufferCount==10)
            {
                // set delay to middle of buffer
                unityDelay=Time.time-firstUnityTime - mBuffer[4].time;
            }
        }else
        {
            hasAngle=false;
            // actual drop frame, up latency slightly
            unityDelay+=0.001f;
            //Debug.Log("Dropped frame");
        }
        
        float outAngle=mAngle;

        // force to use accel code (ignore gyro)
        //hasAngle=false;
        
        Quaternion directionCorrection=getCurrentDirection();
        Vector3 rotatedAccel=Vector3.zero;
        Vector3 origAccel=Vector3.zero;
        foreach (AccelerationEvent accEvent in Input.accelerationEvents) 
        {
            float mag=Mathf.Sqrt(accEvent.acceleration.x*accEvent.acceleration.x+accEvent.acceleration.y*accEvent.acceleration.y+accEvent.acceleration.z*accEvent.acceleration.z);
            origAccel=new Vector3(accEvent.acceleration.x,accEvent.acceleration.y,-accEvent.acceleration.z);
            rotatedAccel=directionCorrection*origAccel;
            accelHistoryTime+=accEvent.deltaTime;
            outAngle=mTracker.OnAccelerometerMagnitude(mag,accelHistoryTime,hasAngle,mAngle,rotatedAccel.z);
        }
        dbgTxt=mTracker.dbgTxt;
        if(!hasAngle)
        {
            mAngle=outAngle;
        }
	}




}


