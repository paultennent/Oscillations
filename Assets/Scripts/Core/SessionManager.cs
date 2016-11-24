using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour {

	public float triggerAngle=10f;

	private bool inGame=false;

	private double gameStartTime=0;

	private double lastForwardSwing=0;
	private double lastBackwardSwing=0;
	private float gameTime=0;

	private double nextTime = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public float getSessionTime(){
		return gameTime;
	}

	public bool isInSession(){
		return inGame;
	}

	private bool isResetTime(){
		if (Time.time > nextTime) {
			nextTime = Time.time + 5.0f;
			return true;
		}
		return false;
	}

	public void reset(){
		//print ("session reset");
		inGame=false;
		gameTime=0;
	}

	//public void onAngle(double time,float angle, GyroAccelFilter gf, float rawAngle)
	public void onAngle(double time,float angle)
	{
		if(angle>triggerAngle)
		{
			lastForwardSwing=time;
			if(!inGame && lastBackwardSwing-time<3)
			{
				// two swings in last 3 seconds, start the game clock
				inGame=true	;
				gameStartTime=time;
				//print ("Start Session");
			}
		}
		if(angle<-triggerAngle)
		{
			lastBackwardSwing=time;
		}
		if ((time - lastBackwardSwing > 5 && time - lastForwardSwing > 5)
			||  (time - lastBackwardSwing > 15 || time - lastForwardSwing > 15))
		{
			// no swings seen for ages, person has got off
			inGame=false;
			gameTime=0;
			//print ("session reset");
			//if(isResetTime()){
				//gf.reset(rawAngle);
				//OVRManager.display.RecenterPose();
			//}
		}

		if (inGame) 
		{
			gameTime=(float)(time-gameStartTime);
		}

	}
}
