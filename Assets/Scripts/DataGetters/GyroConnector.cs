using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;

public class GyroConnector  
{
    public UdpClient receiver;
    
    public float mAngle=0;
    public float mAngularVelocity=0;
    public float mMagDirection=0;
    public float mRemoteBatteryLevel=0;
    public float mLocalBatteryLevel=0;
    public long mTimestamp=0L;

	public void init () 
    {
        // make sure gyro service is running
#if UNITY_ANDROID && !UNITY_EDITOR
        receiver=new UdpClient(2424);

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



		receiver.BeginReceive(new AsyncCallback(ReceiveCallback),receiver);


#endif
	}
    
    public void stop()
    {
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
	
    // java is big endian, sorry
    static byte[] conversionBuf4={0,0,0,0};
    static byte[] conversionBuf8={0,0,0,0,0,0,0,0};
    static float getBigEndianFloat(byte[]input,int offset)
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
	static int getBigEndianInt32(byte[]input,int offset)
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
	static long getBigEndianInt64(byte[]input,int offset)
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
    
	static IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);


	public static void ReceiveCallback(IAsyncResult ar)
	{
		GyroConnector pThis = (GyroConnector)ar.AsyncState;
		UdpClient u = ((GyroConnector)(ar.AsyncState)).receiver;

		Byte[] receiveBytes = u.EndReceive(ar, ref RemoteIpEndPoint);

		if(receiveBytes.Length>=28)
		{
			pThis.mAngle=getBigEndianFloat(receiveBytes,0);
			pThis.mAngularVelocity=getBigEndianFloat(receiveBytes,4);
			pThis.mMagDirection=getBigEndianFloat(receiveBytes,8);
			pThis.mRemoteBatteryLevel=getBigEndianFloat(receiveBytes,12);
			pThis.mTimestamp=getBigEndianInt64(receiveBytes,16);               
			pThis.mLocalBatteryLevel=getBigEndianFloat(receiveBytes,24);
		}            


		u.BeginReceive (new AsyncCallback (ReceiveCallback), ar.AsyncState);
	}

	public void readData() 
    {
		receiver.BeginReceive (new AsyncCallback(ReceiveCallback),this);
/*		const int FRAME_DELAY = 2;
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
		while(receiver.Available>=28)
//		for(int c=0;c<5 && receiver!=null && receiver.Available>=28;c++)
        {
            Byte[] receiveBytes = receiver.Receive(ref RemoteIpEndPoint);
            if(receiveBytes.Length>=28)
            {
                mAngle=getBigEndianFloat(receiveBytes,0);
                mAngularVelocity=getBigEndianFloat(receiveBytes,4);
                mMagDirection=getBigEndianFloat(receiveBytes,8);
                mRemoteBatteryLevel=getBigEndianFloat(receiveBytes,12);
                mTimestamp=getBigEndianInt64(receiveBytes,16);               
                mLocalBatteryLevel=getBigEndianFloat(receiveBytes,24);
            }            
        }*/
        // get angle and other things
        // print it here
        // TODO: check mag direction code
        // TODO: get battery level
        // TODO: put this into Paul's code
        // TODO: bluetooth fallback
	}
}
