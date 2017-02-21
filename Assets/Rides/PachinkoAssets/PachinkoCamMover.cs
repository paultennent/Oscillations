using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PachinkoCamMover : AbstractGameEffects  
{
    
    enum State
    {
        CHARGING,
        LAUNCH,
        BRAKE,
    };
    State state=State.CHARGING;

    public GameObject pivot;
    public float chargeMultiplier=1f;
	public float unchargeMultiplier = 0.25f;
    public TrackGenerator trackGen;
    public float friction=0.001f;
    public float gravity=9.8f;
    public float launchMult=0.1f;
    public float brakeMult=0.1f;
	public float dischargeMult=0.1f;
    public bool reset=false;
    public Slider chargeDisplay;
        
    float charge=0;
    float launchCharge=0;
    int chargeSwings=1;
    int lastQuadrant=-10;
    float rotateFadePercent=0;
    Quaternion lastRotation=Quaternion.identity;
    
	// Use this for initialization
	void Start () {
        base.Start();
        trackDistance=trackGen.GetInitialDistance();
	}

    float trackVelocity=0f;
    float trackDistance=0f;
    void MoveOnTrack(float amount)
    {
        trackDistance+=amount;
        Vector3 pos=trackGen.GetTrackPosition(trackDistance);
        pos=trackGen.transform.TransformPoint(pos);
        pivot.transform.position=pos;
        if(rotateFadePercent<1)
        {
            rotateFadePercent+=Time.deltaTime*.5f;
            pivot.transform.rotation=Quaternion.Slerp(lastRotation,trackGen.transform.rotation,rotateFadePercent);
        }else
        {
            pivot.transform.rotation=trackGen.transform.rotation;
        }
        pivot.transform.Rotate(new Vector3(-trackGen.GetTrackSlopeAngle(trackDistance)*Mathf.Rad2Deg,0,0));
    }
    
    // get force in direction of track (only)
    float GetTrackForces()
    {
        float gravityForce=0;
        gravityForce=Mathf.Sin(trackGen.GetTrackSlopeAngle(trackDistance))*-gravity;
        
        float dragForce=trackVelocity*trackVelocity*friction;
        if(trackVelocity<0)dragForce=-dragForce;
        return gravityForce-dragForce;
    }
    
    void NewTrack()
    {
        float circleAngleRad=trackGen.GetTrackSlopeAngle(trackDistance);
        lastRotation=pivot.transform.rotation;
        trackGen=trackGen.CreateNewSegment(circleAngleRad*Mathf.Rad2Deg).GetComponent<TrackGenerator>();
        trackDistance=trackGen.GetInitialDistance();
        trackVelocity=0;
        rotateFadePercent=0;
    }
	
	// Update is called once per frame
	void Update () {
        base.Update();

		if (!inSession) {
			return;
		}

//        MoveOnTrack(-1f*Time.deltaTime);
//        return;
        trackVelocity+=GetTrackForces()*Time.deltaTime;
        if(state==State.LAUNCH || state==State.BRAKE)
        {
            MoveOnTrack(trackVelocity*Time.deltaTime);
        }else
        {
            MoveOnTrack(0);
        }
        if(trackDistance<0)
        {
            trackDistance=0;
            trackVelocity=0;
        }
        if(reset)
        {
            trackDistance=trackGen.GetInitialDistance();
            trackVelocity=0;
            reset=false;
        }
        
        switch(state)
        {
		case State.CHARGING:
			if (swingQuadrant == 3 || swingQuadrant == 0) {
				charge += swingAngVel * swingAngVel * Time.deltaTime * chargeMultiplier * chargeSwings;
			} else {
				charge -= swingAngVel * swingAngVel * Time.deltaTime * unchargeMultiplier;
			}
			if (charge < 0) {
				charge = 0;
			}
                if(charge>1)charge=1;
                trackDistance=(1f-charge)*trackGen.GetInitialDistance();
                MoveOnTrack(0);
                if(chargeDisplay!=null)
                {
                    chargeDisplay.value=charge;
                }
                if(swingQuadrant!=lastQuadrant && swingQuadrant==1)
                {
                    chargeSwings+=1;
                    //print("Charge:"+chargeSwings);
                    if(chargeSwings==4)
                    {
                        state=State.LAUNCH;
                        //print("Launching: charge");
                    }
                }
                break;
            case State.LAUNCH:
                if(swingQuadrant==1 || swingQuadrant==2)
                {
                    trackVelocity+=swingAngVel*swingAngVel*Time.deltaTime*launchMult;
					trackVelocity += charge * Time.deltaTime * dischargeMult;
					charge = Mathf.Max(0, charge-Time.deltaTime);
                }
                if(swingQuadrant==3)
                {
                    state=State.BRAKE;
                }
                break;
            case State.BRAKE:
                if(swingQuadrant==3 || swingQuadrant==0)
                {
                    trackVelocity-=swingAngVel*swingAngVel*Time.deltaTime*brakeMult;
                    if(trackVelocity<0)trackVelocity=0;
                }
                if(swingQuadrant==1)
                {
                    state=State.CHARGING;
                    chargeSwings=1;
                    charge=0;
                    NewTrack();
                }
//                print(trackVelocity+":"+trackDistance);
                break;
        }

        lastQuadrant=swingQuadrant;
	}    
}
