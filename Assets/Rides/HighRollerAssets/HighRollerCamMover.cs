using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighRollerCamMover : AbstractGameEffects {

	public GameObject viewpoint;
	public float speed;
    public float seatDrop=1.5f;
    
    public bool manual = false;

	public float angVelscaler = 10f;
	public float dragConstant = 0.01f;
    
    public int impelQuadrant=1;

	public GameObject[] wheels;
	public GameObject[] wheelviewpoints;
	private float wheelrotationconstant = 100f;

    
    private Vector3 initialViewpointPos;
    private bool foundInitialPos=false;
    private bool launched=false;
	private bool inCooldown = false;

    private float introTime=5f;
    private float outtroTime=10f;
    private float outtroSwingTime=5f;
    
    private float forceOutroTime=-1;

    private float accelVal;
    private bool fadedIn = false;

	public HighRollerAudioController audioController;
    
	// Use this for initialization
	void Start () {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

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

        if (!foundInitialPos)
        {
            foundInitialPos=true;
            initialViewpointPos=viewpoint.transform.position;
        }
        
        if(!inSession)return;

        
        if(countUp && (offsetTime<introTime || (offsetTime<introTime*2f && !launched)) )
        {
            // intro period - slowly reduce the amount of up and down movement
            // and reduce it to just fwd/back movement
            
            Vector3 topPoint=initialViewpointPos+Vector3.up*seatDrop;
            Quaternion rotation=Quaternion.Euler(-swingAngle,0,0);
            Vector3 rotationOffset=rotation*Vector3.up*-seatDrop;
            Vector3 seatPoint=topPoint+rotationOffset;
            Vector3 onlyFwdBackPoint=new Vector3(seatPoint.x,topPoint.y,seatPoint.z);
            
//            viewpoint.transform.position=Vector3.Lerp(seatPoint,onlyFwdBackPoint,1f);
            viewpoint.transform.position=Vector3.Lerp(seatPoint,onlyFwdBackPoint,offsetTime/10f);
            
            speed = speed + (getAccelerationNow () * Time.deltaTime);
            
            if(offsetTime>introTime && swingQuadrant==0)
            {
                launched=true;
            }            
        }else if(forceOutroTime>=0)
        {
            BlockLayout bl = BlockLayout.GetBlockLayout();
            Vector3 endPos=bl.currentTarget.position;
            Vector3 topPoint=endPos+Vector3.up*seatDrop;
            Quaternion rotation=Quaternion.Euler(-swingAngle,0,0);
            Vector3 rotationOffset=rotation*Vector3.up*-seatDrop;
            Vector3 targetPoint=rotationOffset+topPoint;
            // make it gradually start swinging again
            viewpoint.transform.position=Vector3.Lerp(targetPoint,viewpoint.transform.position,forceOutroTime/10);
            // make it fade
            
            if (forceOutroTime<5 && !FadeSphereScript.isFading())
            {
                FadeSphereScript.doFadeOut(5f, Color.black);
                inCooldown = true;
            }
            forceOutroTime=Mathf.Max(forceOutroTime-Time.deltaTime,0);
        }else if(offsetTime<outtroTime && !countUp)
        {
            // last 20 seconds - outtro - zoom to final point in first 10 seconds then return to swinging for 10 seconds and fade
            BlockLayout bl = BlockLayout.GetBlockLayout();
            bl.EnsureEndBlock();
            Vector3 endPos=bl.currentTarget.position;
            Vector3 topPoint=endPos+Vector3.up*seatDrop;
            Quaternion rotation=Quaternion.Euler(-swingAngle,0,0);
            Vector3 rotationOffset=rotation*Vector3.up*-seatDrop;
            Vector3 targetPoint=rotationOffset+topPoint;
            if(offsetTime>outtroSwingTime)
            {
                // zoom it to the point
                viewpoint.transform.position=Vector3.Lerp(viewpoint.transform.position,targetPoint,1 - (offsetTime-outtroSwingTime)/(outtroTime-outtroSwingTime));                
            }else
            {
                if (!FadeSphereScript.isFading())
                {
                    FadeSphereScript.doFadeOut(5f, Color.black);
                }
                inCooldown = true;
                viewpoint.transform.position=targetPoint;
            }
        }        
        else
        {
            //angVelscaler = angVelscaler * (1 + climaxRatio);

            speed = speed + (getAccelerationNow () * Time.deltaTime);
            
            float mz = BlockLayout.GetBlockLayout().GetMaxZ();
            float newZ=viewpoint.transform.position.z + Time.deltaTime * speed;
            if(mz!=0f && newZ>mz)
            {
                // needs to drop into swing outtro here if we hit it early
                newZ=mz;
                forceOutroTime=10f;
            }
            viewpoint.transform.position=new Vector3(viewpoint.transform.position.x,viewpoint.transform.position.y,newZ);
        }
        
    }

	private float getAccelerationNow(){
        float totalAcc = 0f;
        if (swingQuadrant == impelQuadrant)
        {
            if (sessionTime < climaxTime)
            {
                //first half
                //accelVal = Remap(climaxRatio, 0f, 1f, 0.001f, 100f);
                accelVal = (climaxRatio * climaxRatio * climaxRatio) ;
            }
            else
            {
                //second half
                //accelVal = Remap(climaxRatio, 0f, 1f, 100f, 0.001f);
                accelVal = (climaxRatio * climaxRatio);
            }
            if(swingAngVel<0)
            {
                totalAcc = -swingAngVel * angVelscaler * accelVal;
            }
        }

        if(Application.identifier=="com.mrl.swingdiffgear")
        {
            // diffusion version tuned to have less drag
            if (speed<0)
            {
                totalAcc += (speed * speed) * dragConstant*0.5f;
            }else
            {
                totalAcc -= (speed * speed) * dragConstant*0.5f;
            }
            
        }else
        {
            if (speed<0)
            {
                totalAcc += (speed * speed) * dragConstant;
            }else
            {
                totalAcc -= (speed * speed) * dragConstant;
            }
        }
		return totalAcc;
	}

	public bool isMoving()
	{
		return launched && !inCooldown;
	}

    private float Remap(float val, float OldMin, float OldMax, float NewMin, float NewMax)
    {
        return (((val - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
    }
}
