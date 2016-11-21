//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

public class SwingGameLogic
{
	//const float CLIMAX_TIME = 10.0f;
	const float CLIMAX_TIME = 90.0f;
	const float SWING_MULTIPLY_FACTOR = 1.5f;
	const float ROOM_ZOOM_FACTOR = 2f;
	const float TRIGGER_ANGLE=10f;
	 float G = CLIMAX_TIME / Mathf.Sqrt(SWING_MULTIPLY_FACTOR);

	bool inGame=false;
	double gameStartTime=0;

	double lastForwardSwing=0;
	double lastBackwardSwing=0;

	public float floorDropMultiply=0;
	public float swingMultiply=1;
	public float gameTime=0;
	public float climaxRatio = 0;

	public double nextTime = 0;

	public SwingGameLogic ()
	{

	}

	bool isResetTime(){
		if (Time.time > nextTime) {
			nextTime = Time.time + 5.0f;
			return true;
		}
		return false;
	}

	public void reset(){
		inGame=false;
		gameTime=0;
		floorDropMultiply=1;
		swingMultiply=1;
		climaxRatio=0;
	}

	public void onAngle(double time,float angle, GyroAccelFilter gf, float rawAngle)
	{
		if(angle>TRIGGER_ANGLE)
		{
			lastForwardSwing=time;
			if(!inGame && lastBackwardSwing-time<3)
			{
				// two swings in last 3 seconds, start the game clock
				inGame=true	;
				gameStartTime=time;
			}
		}
		if(angle<-TRIGGER_ANGLE)
		{
			lastBackwardSwing=time;
		}
		if ((time - lastBackwardSwing > 5 && time - lastForwardSwing > 5)
		    ||  (time - lastBackwardSwing > 15 || time - lastForwardSwing > 15))
		{
			// no swings seen for ages, person has got off
			inGame=false;
			gameTime=0;
			floorDropMultiply=1;
			swingMultiply=1;
			climaxRatio=0;
			if(isResetTime()){
				gf.reset(rawAngle);
				//OVRManager.display.RecenterPose();
			}
		}

		if (inGame) 
		{
			gameTime=(float)(time-gameStartTime);
			int gameNumber=(int)(gameTime/CLIMAX_TIME);
			float ofsTime=gameTime-(gameNumber*CLIMAX_TIME);
			if((gameNumber&1)==1)
			{
				ofsTime=CLIMAX_TIME-ofsTime;
			}
			climaxRatio=ofsTime/CLIMAX_TIME;
			swingMultiply=Mathf.Min(1+(ofsTime/G)*(ofsTime/G),1+SWING_MULTIPLY_FACTOR);
			floorDropMultiply=Mathf.Min (1+ROOM_ZOOM_FACTOR*Mathf.Abs (angle*swingMultiply)/124,1+ROOM_ZOOM_FACTOR); 
		}

	}


}
