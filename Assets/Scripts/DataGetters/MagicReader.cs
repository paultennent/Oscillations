using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicReader : AbstractDataReader {

	private GyroConnector gc;

	private float mAngle = 0f;
	private float mAngularVelocity=0f;
	private float mMagDirection=0f;
	private float mRemoteBatteryLevel=0f;
	private float mLocalBatteryLevel=0f;
	private long mTimestamp=0L;

	private int sameDataCount = 0;
	private float lastAng = 0;

    public bool useAccelerometer=false;
    
	public float getAngle(){
		return mAngle;
	}

	public float getAngularVelocity(){
		return mAngularVelocity;
	}

	public float getMagDirection(){
		return mMagDirection;
	}

	public float getLocalBatteryLevel(){
		return mLocalBatteryLevel;
	}

	public float getRemoteBatteryLevel(){
		return mRemoteBatteryLevel;
	}

	public long getRemoteTimestamp(){
		return mTimestamp;
	}

	public int getSameDataCount(){
		return sameDataCount;
	}

	void OnApplicationPause(bool paused){
        if(gc!=null)
        {
            gc.pause (paused);
        }
	}

	// Use this for initialization
	void Start () {
		gc = new GyroConnector();
		gc.init ();
	}
	
	// Update is called once per frame
	void Update () {
        gc.useAccelerometer=useAccelerometer;
		lastAng = mAngle;
		gc.readData ();

		headingNow = lowpass(new double[]{ 0, (double) gc.mAngle, 0 },headingNow);

		mAngle = gc.mAngle;
		mAngularVelocity = gc.mAngularVelocity;
		mMagDirection = gc.mMagDirection;
		mLocalBatteryLevel = gc.mLocalBatteryLevel;
		mRemoteBatteryLevel = gc.mRemoteBatteryLevel;
		mTimestamp = gc.mTimestamp;

        switch(gc.mConnectionState&3)
        {
            case 3:
                connectionState=CONNECTION_FULL;
                break;
            case 0:
                connectionState=CONNECTION_NONE;
                break;
            default:
                connectionState=CONNECTION_PARTIAL;
                break;
        };
        
		if (lastAng == mAngle) {
			sameDataCount += 1;
		} else {
			//sameDataCount = 0;
		}
	}
}
