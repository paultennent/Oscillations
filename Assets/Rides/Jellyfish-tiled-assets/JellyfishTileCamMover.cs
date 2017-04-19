using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyfishTileCamMover : AbstractGameEffects  {

    private float curHeight=0f;
    private float yVelocity=0f;
    
    public float velocityDragConstant=.00003f;
    public  float upforceConstant=.001f;
    public  float gravityConstant=9.8f;

	public float fadeTime = 5f;
    
    private Vector3 initialPosition;

	public GameObject pivot;
    
    public bool launch=false;

	public bool infiniteFall = true;

    private Skirt skirtObj;

	private bool launched=false;
	private float introTime=5f;

	public float seatDrop=1.5f;

    float minCurheight = 0f;
    
	// Use this for initialization
	void Start () {
        base.Start();
        initialPosition=pivot.transform.position;
        minCurheight = pivot.transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();

		if(sessionTime<introTime || !launched)
		{
			// intro period - slowly reduce the amount of up and down movement
			// and reduce it to just fwd/back movement
			//need to change this to up/down

			Vector3 topPoint=initialPosition+Vector3.up*seatDrop;
			Quaternion rotation=Quaternion.Euler(-swingAngle,0,0);
			Vector3 rotationOffset=rotation*Vector3.up*-seatDrop;
			Vector3 seatPoint=topPoint+rotationOffset;
			Vector3 onlyFwdBackPoint=new Vector3(seatPoint.x,topPoint.y,seatPoint.z);

			pivot.transform.position=Vector3.Lerp(seatPoint,onlyFwdBackPoint,offsetTime/10f);

			if(offsetTime>introTime && swingQuadrant==0)
			{
				launched=true;
			}            
		}

		else{


			//for the rest of the motion as brendan wants it:
			//need to apply inverse swing transform
			//then set angle based on the "force angle" - that's in the equations
			//then after we emerge from the cave make a forwards/backwards pingpong of out transform based on swingangle/59 with the maximum ever increasing (need to tune this value probably)


        float upforce=calculateUpforce();

        yVelocity+=upforce*Time.deltaTime;
        curHeight=curHeight+yVelocity*Time.deltaTime;
		if(curHeight<0 & !infiniteFall)
        {
            curHeight= minCurheight;
            yVelocity=0;
        }
        pivot.transform.position=initialPosition+new Vector3(0,curHeight,0);

		//print (sessionTime + ":" + sessionLength);
		if (sessionTime > (sessionLength - fadeTime)) {
			//print ("Should be fading");
			if(!Fader.IsFading())
			{
				Fader.DoFade(Time.time+(sessionLength-sessionTime));
			}
		}

		}
	}
    
    float calculateUpforce()
    {
        float totalForce=swingAngVel*swingAngVel*upforceConstant;
        if(launch==true)
        {
            totalForce=20f;
        }
        
        totalForce-=gravityConstant;
        if(yVelocity>0)
        {
            totalForce-=(yVelocity*yVelocity)*velocityDragConstant;
        }else
        {
            totalForce+=(yVelocity*yVelocity)*velocityDragConstant;
        }
        return totalForce;
    }
}
