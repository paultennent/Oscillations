using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MagicReader : AbstractDataReader {

	private static GyroConnector gc;

	private float mAngle = 0f;
	private float mAngularVelocity=0f;
	private float mMagDirection=0f;
	private float mRemoteBatteryLevel=0f;
	private float mLocalBatteryLevel=0f;
	private long mTimestamp=0L;

	private int sameDataCount = 0;
    private int resetCount=0;
	private float lastAng = 0;
    
    private int restartCount=0;

    public bool useAccelerometer=false;
    
    public float getConnectionState()
    {
        return gc.mConnectionState;
    }
    
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
        // only one gyroconnector ever - won't get deleted during the scene switch
        if(gc==null)
        {
            gc = new GyroConnector();
            gc.init ();
        }else
        {
            if(SceneManager.GetActiveScene().name.IndexOf("Menu")!=-1)
            {
                // if we've gone back to the menu scene then reset the reader connection
                // just in case anything has gone bad (or we've got latency from somewhere)
                gc.stop();
                resetCount=500;
                sameDataCount=0;
            }

        }
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
			sameDataCount = 0;
		}
        // same data for 1 second - restart the connection
        if(resetCount==0 && sameDataCount>480)
        {
            gc.stop();
            resetCount=500;
            sameDataCount=0;
        }
        if(resetCount>0)
        {
            resetCount-=1;
            sameDataCount=0;
        }
        if(resetCount==450)
        {
            gc.doConnection();
        }
	}
}
