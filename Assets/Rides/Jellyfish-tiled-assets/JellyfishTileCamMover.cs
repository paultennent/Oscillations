using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class JellyfishTileCamMover : AbstractGameEffects
{

    private float curHeight = 0f;
    private float yVelocity = 0f;

    public float velocityDragConstant = .00003f;
    public float upforceConstant = .001f;
    public float gravityConstant = 9.8f;

    public float fadeTime = 5f;

    private Vector3 initialPosition;

    public GameObject pivot;

    public bool launch = false;

    public bool infiniteFall = true;

    private Skirt skirtObj;

    private bool launched = false;
    private float introTime = 15f;

    public float seatDrop = 1.5f;

    float minCurheight = 0f;

    public BubbleScript startBubble;

    public bool SwapAngle = true;

    private bool fadedIn = false;

	public JellyfishAudioController audioController;

    private float smoothedAngle=0f;
    
    // Use this for initialization
    void Start()
    {
        base.Start();
        initialPosition = pivot.transform.position;
        minCurheight = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        smoothedAngle=swingAngle*0.1f+smoothedAngle*0.9f;
        base.Update();
		if (!inSession) {
			return;
		}
			
		audioController.begin ();


        if (!fadedIn)
        {
            FadeSphereScript.doFadeIn(5f, Color.black);
            fadedIn = true;
        }

        //doDifferentRotation - I think this wants removing...
        if (SwapAngle)
        {
            float ang = -swingAngle + shopping["angle_as_offset_from_perpendicular_to_CofG_direction"];
            viewPoint.localEulerAngles = new Vector3(ang, 0, 0);
        }

        if (sessionTime < introTime || !launched)
        {
            float upforce = calculateUpforce(); // calculate and ignore up force
            // intro period - slowly reduce the amount of up and down movement
            // and reduce it to just fwd/back movement
            //need to change this to up/down

            Vector3 topPoint = initialPosition + Vector3.up * seatDrop;
            Quaternion rotation = Quaternion.Euler(-swingAngle, 0, 0);
            Vector3 rotationOffset = rotation * Vector3.up * -seatDrop;
            Vector3 seatPoint = topPoint + rotationOffset;
            Vector3 onlyFwdBackPoint = new Vector3(seatPoint.x, topPoint.y, seatPoint.z);

            float ratio=0f;
            if(sessionTime>10f)
            {
                ratio=(sessionTime-10f)/5;
            }
            
            pivot.transform.position = Vector3.Lerp(seatPoint, onlyFwdBackPoint,ratio);

            if (offsetTime > introTime && swingQuadrant == 0)
            {
                launched = true;
                print("launched!");
                startBubble.trigger = true;
            }
        }

        else
        {

            float upforce = calculateUpforce();
//            print("!"+curHeight+":"+upforce+":"+swingAngVel+":"+swingAngle);

            yVelocity += upforce * Time.deltaTime;
            curHeight = curHeight + yVelocity * Time.deltaTime;
			if (curHeight < minCurheight && !infiniteFall)
            {
                curHeight = minCurheight;
                yVelocity = 0;
            }

            float zVal = 0f;
            LayerLayout.LayoutPos curTilePos = LayerLayout.GetLayerLayout().GetBlockAt(pivot.transform.position.y);
            if (curTilePos > LayerLayout.LayoutPos.CAVE_TOP)
            {
                zVal = (smoothedAngle / 2f) * (0.5f-climaxRatio);
            }
				
            pivot.transform.position = initialPosition + new Vector3(0, curHeight, zVal);

            //print (sessionTime + ":" + sessionLength);
            if (sessionTime > (sessionLength - fadeTime))
            {
                //print ("Should be fading");
                if (!FadeSphereScript.isFading())
                {
					FadeSphereScript.doFadeOut(fadeTime, Color.black);
                }
            }

        }
    }

    
    const int ANGULAR_VELOCITY_FRAMES=5;
    float []angleHistory=new float[ANGULAR_VELOCITY_FRAMES];
    float []timeHistory=new float[ANGULAR_VELOCITY_FRAMES];
    
    float calculateUpforce()
    {
        float totalForce=0f;
/*        if(Application.identifier=="com.mrl.swingdiffgear")
        {
            // filter out glitches in diffusion version
            Array.Copy(angleHistory,0,angleHistory,1,angleHistory.Length-1);
            Array.Copy(timeHistory,0,timeHistory,1,timeHistory.Length-1);
            angleHistory[0]=swingAngVel;
            timeHistory[0]=Time.time;
            // take a median 
            List<float> sorted=new List<float>(angleHistory);
            float smoothedAngVel=sorted[ANGULAR_VELOCITY_FRAMES/2];
            totalForce = smoothedAngVel * smoothedAngVel * upforceConstant;
            totalForce=Mathf.Min(totalForce,250f); 
            
        }else
        {*/
            Array.Copy(angleHistory,0,angleHistory,1,angleHistory.Length-1);
            Array.Copy(timeHistory,0,timeHistory,1,timeHistory.Length-1);
            angleHistory[0]=swingAngle;
            timeHistory[0]=Time.time;
            
            float smoothedAngVel=(angleHistory[0]-angleHistory[angleHistory.Length-1])/(timeHistory[0]-timeHistory[timeHistory.Length-1]);
            print(smoothedAngVel+","+swingAngVel+":"+angleHistory[0]+":"+angleHistory[angleHistory.Length-1]);
            totalForce = smoothedAngVel * smoothedAngVel * upforceConstant;
    //        float totalForce = swingAngVel * swingAngVel * upforceConstant;
            // if there is an error in angular velocity it can cause silly large forces
            totalForce=Mathf.Min(totalForce,250f); 
        //}
        if (launch == true)
        {
            totalForce += 20f;
        }

        totalForce -= gravityConstant;
        if (yVelocity > 0)
        {
            totalForce -= (yVelocity * yVelocity) * velocityDragConstant;
        }
        else
        {
            totalForce += (yVelocity * yVelocity) * velocityDragConstant;
        }
        return totalForce;
    }
}
