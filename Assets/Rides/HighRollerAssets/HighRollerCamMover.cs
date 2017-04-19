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
    
	// Use this for initialization
	void Start () {
        base.Start();
	}

    // Update is called once per frame
    void Update()
    {

        base.Update();
        if(!foundInitialPos)
        {
            foundInitialPos=true;
            initialViewpointPos=viewpoint.transform.position;
            print("Initial pos:"+initialViewpointPos);
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
                if (!Fader.IsFading())
                {
                    Fader.DoFade(Time.time + 5f);
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
            if(mz!=0f )
            {
                // needs to drop into swing outtro here if we hit it early
                newZ=Mathf.Min(mz,newZ);
            }
            viewpoint.transform.position=new Vector3(viewpoint.transform.position.x,viewpoint.transform.position.y,newZ);
        }
        
    }

	private float getAccelerationNow(){
		float totalAcc = 0;
		if (swingQuadrant == impelQuadrant) {
			//print ("impelling:"+swingAngVel+":"+speed);
			if (sessionTime < climaxTime)
			{
				//cubic for first half
				totalAcc = -swingAngVel * angVelscaler * (climaxRatio * climaxRatio * climaxRatio);
			}
			else
			{
				//squared for second half
				totalAcc = -swingAngVel * angVelscaler * (climaxRatio * climaxRatio);
			}
		}
        if(speed<0)
        {
            totalAcc += (speed * speed) * dragConstant;
        }else
        {
            totalAcc -= (speed * speed) * dragConstant;
        }
		return totalAcc;
	}

	public bool isMoving()
	{
		return launched && !inCooldown;
	}
}
