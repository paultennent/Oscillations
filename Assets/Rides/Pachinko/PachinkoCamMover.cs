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
    public TrackGenerator trackGen;
    public float friction=0.001f;
    public float gravity=9.8f;
    public float launchMult=0.1f;
    public float brakeMult=0.1f;
    public bool reset=false;
    public Slider chargeDisplay;
        
    float charge=0;
    int chargeSwings=1;
    int lastQuadrant=-10;
    float rotateFadePercent=0;
    Quaternion lastRotation=Quaternion.identity;
    
	// Use this for initialization
	void Start () {
        base.Start();
	}

    float trackVelocity=0f;
    float trackDistance=0f;
    void MoveOnTrack(float amount)
    {
        float inletLen=trackGen.lengthInlet;
        float radiusLoop=trackGen.radiusLoop;
        trackDistance+=amount;
        if(trackDistance<inletLen)
        {
            // along track
            Vector3 trackStart=trackGen.transform.TransformPoint(new Vector3(0,0,0));
            Vector3 trackEnd=trackGen.transform.TransformPoint(new Vector3(0,0,inletLen));
            pivot.transform.position=Vector3.Lerp(trackStart,trackEnd,trackDistance/inletLen);
//            pivot.transform.eulerAngles=new Vector3(0,0,0);
            if(rotateFadePercent<1)
            {
                rotateFadePercent+=Time.deltaTime*2.0f;
                pivot.transform.rotation=Quaternion.Slerp(lastRotation,trackGen.transform.rotation,rotateFadePercent);
            }else
            {
                pivot.transform.rotation=trackGen.transform.rotation;
            }
        }else
        {
//            float circumference=(2.0*radiusLoop*Mathf.PI);            
//            float circleAngleRad=((trackDistance-inletLen)/circumference)*2.0*Mathf.PI;
            // angle round the loop
            float circleAngleRad=((trackDistance-inletLen)/radiusLoop);
            float posY=radiusLoop-Mathf.Cos(circleAngleRad)*radiusLoop;
            float posZ=inletLen+Mathf.Sin(circleAngleRad)*radiusLoop;
            pivot.transform.position=trackGen.transform.TransformPoint(new Vector3(0,posY,posZ));
            if(rotateFadePercent<1)
            {
                rotateFadePercent+=Time.deltaTime*2.0f;
                pivot.transform.rotation=Quaternion.Slerp(lastRotation,trackGen.transform.rotation,rotateFadePercent);
            }else
            {
                pivot.transform.rotation=trackGen.transform.rotation;
            }
            pivot.transform.Rotate(new Vector3(-circleAngleRad*Mathf.Rad2Deg,0,0));
        }
    }
    
    // get force in direction of track (only)
    float GetTrackForces()
    {
        float inletLen=trackGen.lengthInlet;
        float radiusLoop=trackGen.radiusLoop;
        float gravityForce=0;
        if(trackDistance<inletLen)
        {
            gravityForce=0;
        }else
        {
            float circleAngleRad=((trackDistance-inletLen)/radiusLoop);
            gravityForce=-gravity*Mathf.Sin(circleAngleRad);
        }
        float dragForce=trackVelocity*trackVelocity*friction;
        if(trackVelocity<0)dragForce=-dragForce;
        return gravityForce-dragForce;
    }
    
    void NewTrack()
    {
        float inletLen=trackGen.lengthInlet;
        float radiusLoop=trackGen.radiusLoop;
        float circleAngleRad=0;
        if(trackDistance>=inletLen)
        {
            circleAngleRad=Mathf.Repeat((trackDistance-inletLen)/radiusLoop,Mathf.PI*2.0f);            
        }
        lastRotation=pivot.transform.rotation;
        trackGen=trackGen.CreateNewSegment(circleAngleRad*Mathf.Rad2Deg,40).GetComponent<TrackGenerator>();
        trackDistance=0;
        trackVelocity=0;
        rotateFadePercent=0;
    }
	
	// Update is called once per frame
	void Update () {
        base.Update();
        trackVelocity+=GetTrackForces()*Time.deltaTime;
        if(state==State.LAUNCH || state==State.BRAKE)
        {
            MoveOnTrack(trackVelocity*Time.deltaTime);
        }else
        {
            MoveOnTrack(0);
        }
        if(trackDistance<0 || reset)
        {
            trackDistance=0;
            trackVelocity=0;
            reset=false;
        }
        
        switch(state)
        {
            case State.CHARGING:
                if(swingQuadrant==3 || swingQuadrant==0)
                {
                    charge+=swingAngVel*swingAngVel*Time.deltaTime*chargeMultiplier;
                }
                if(charge>1)charge=1;
                if(chargeDisplay!=null)
                {
                    chargeDisplay.value=charge;
                }
                if(swingQuadrant!=lastQuadrant && swingQuadrant==1)
                {
                    chargeSwings+=1;
                    print("Charge:"+chargeSwings);
                    if(chargeSwings==3)
                    {
                        state=State.LAUNCH;
                        print("Launching: charge");
                    }
                }
                break;
            case State.LAUNCH:
                if(swingQuadrant==1 || swingQuadrant==2)
                {
                    trackVelocity+=swingAngVel*swingAngVel*Time.deltaTime*launchMult*charge;
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
