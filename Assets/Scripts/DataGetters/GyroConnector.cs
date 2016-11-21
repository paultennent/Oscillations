using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;

public class GyroConnector 
{
    UdpClient receiver;
    
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
    
	public void readData() 
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
		while(receiver!=null && receiver.Available>=28)
        {
            Byte[] receiveBytes = receiver.Receive(ref RemoteIpEndPoint);
            if(receiveBytes.Length==28)
            {
                mAngle=getBigEndianFloat(receiveBytes,0);
                mAngularVelocity=getBigEndianFloat(receiveBytes,4);
                mMagDirection=getBigEndianFloat(receiveBytes,8);
                mRemoteBatteryLevel=getBigEndianFloat(receiveBytes,12);
                mTimestamp=getBigEndianInt64(receiveBytes,16);               
                mLocalBatteryLevel=getBigEndianFloat(receiveBytes,24);
            }            
        }
        // get angle and other things
        // print it here
        // TODO: check mag direction code
        // TODO: get battery level
        // TODO: put this into Paul's code
        // TODO: bluetooth fallback
	}
}
