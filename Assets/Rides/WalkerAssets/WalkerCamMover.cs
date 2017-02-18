using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// walk in 4 phases:
// 1 Left leg bend up (does right leg bend so that body stays straight here?) // swing angle 0 -> 1
// 2 Right leg bends until left leg touches down                              // swing angle 1 -> 0
// 3 Right leg bends up                                                       // swing angle 0 -> -1
// 4 Left leg bends until right leg touches down                              // swing angle -1 -> 0

// on terrain, raycast down from each leg
// if it hits terrain, then lift it up
// lift up to minimum that gets lowest leg out
// then rotate until highest leg out

public class WalkerCamMover : AbstractGameEffects  
{
    
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform leftHip;
    public Transform rightHip;
    
    public Transform body;
    
    private Transform bodyParent;
    
    public float rockMultiplier=0.25f;
    public float twistMultiplier=0.25f;
    public float strideMultiplier=0.25f;
    
    public float leftAngle=0f;
    public float rightAngle=0f;
    
	// Use this for initialization
	void Start () 
    {
        base.Start();
        bodyParent=body.parent;
	}

    bool seenZeroCrossing=false;
    bool firstStep=true;
    
    float prevAngle=0;
    float maxAngle=0;
	// Update is called once per frame
	void Update () 
    {
        
        base.Update();
        // are we going left-> right or right->left
        if((prevAngle<0 && swingAngle>=0) || (prevAngle>=0 && swingAngle<0))
        {
            if(!seenZeroCrossing)
            {
                seenZeroCrossing=true;
                firstStep=true;
            }else
            {
                firstStep=false;
            }
        }                
        if(seenZeroCrossing)
        {
            float legAngle=swingAngle;
            Transform lockFoot=rightFoot;
            Transform otherFoot=leftFoot;
            if(swingQuadrant==1 || swingQuadrant==2)
            {
                lockFoot=leftFoot;
                otherFoot=rightFoot;
            }
            Vector3 footPos;
            Vector3 offset;
            
/*            footPos=lockFoot.position;
            // set hip angles then fix foot            
            leftHip.localEulerAngles=new Vector3(0,swingAngle*strideMultiplier,0);
            rightHip.localEulerAngles=new Vector3(0,-swingAngle*strideMultiplier,0);
            offset = lockFoot.position-footPos;
            body.position+=offset;*/
            footPos=lockFoot.position;
//            float rockAngle=-swingAngle*rockMultiplier;
            float rockAngle=-Mathf.Cos(Mathf.Deg2Rad*swingPhase*90.0f)*rockMultiplier*30.0f;
            body.eulerAngles=new Vector3(0,0,rockAngle);
            leftHip.localEulerAngles=new Vector3(0,swingAngle*strideMultiplier,rockAngle);
            rightHip.localEulerAngles=new Vector3(0,-swingAngle*strideMultiplier,rockAngle);
            offset = lockFoot.position-footPos;
            body.position-=offset;
            
            RaycastHit hitThis,hitOther;
            Physics.Raycast(otherFoot.position+new Vector3(0,100,0),-Vector3.up,out hitOther);
            if(hitOther.distance<1000)
            {
                // add rotation
            }
/*            RaycastHit hitThis,hitOther;
            // now raycast down from high above each foot (robot is in ignore raycast layer)
            Physics.Raycast(lockFoot.position+new Vector3(0,100,0),-Vector3.up,out hitThis);
            
            if(hitThis.distance<1000 )
            {
                Vector3 thisPt=hitThis.point;
                float distThis=lockFoot.position.y-thisPt.y;
                body.Translate(new Vector3(0,-distThis,0),Space.World);

                Physics.Raycast(otherFoot.position+new Vector3(0,100,0),-Vector3.up,out hitOther);
                Vector3 otherPt=hitOther.point;
                float distOther=otherFoot.position.y-otherPt.y;
                if(hitOther.distance<1000)
                {
                    // rotate body until foot  is directly on the ground
                    Vector3 feetDiff=(otherFoot.position-lockFoot.position).normalized;
                    Vector3 bodyDiff=(body.position - lockFoot.position).normalized;
                    Vector3 axis=Vector3.Cross(feetDiff,bodyDiff);
                    axis=Vector3.fwd;
                    float rotationRads = Mathf.Asin(distOther)/Vector3.Distance(otherFoot.position,lockFoot.position);
                    if(lockFoot==rightFoot){
                        rotationRads=-rotationRads;
                    }
                    print (axis+","+lockFoot.position+":"+Mathf.Rad2Deg*rotationRads);
                    body.RotateAround(lockFoot.position,axis,-Mathf.Rad2Deg*rotationRads);
                }
            }            */
            
            
            
            
        }
        prevAngle=swingAngle;
	}    
    
    void pivotBody(float rockAngle,float twistAdd,Transform pivotFoot,Transform otherFoot)
    {
        if(body.parent!=pivotFoot)
        {
            if(body.parent==otherFoot)
            {
                body.parent=null;
                otherFoot.parent=body;
            }
            pivotFoot.parent=null;
            body.parent=pivotFoot;
        }
        pivotFoot.RotateAround(pivotFoot.position,Vector3.up,twistAdd);
        pivotFoot.eulerAngles=new Vector3(rockAngle,pivotFoot.eulerAngles.y,pivotFoot.eulerAngles.z);        
    }
}
