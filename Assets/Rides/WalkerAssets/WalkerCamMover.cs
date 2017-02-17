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
            if(swingQuadrant==1 || swingQuadrant==2)
            {
                lockFoot=leftFoot;
            }
            Vector3 footPos=lockFoot.position;
            // set hip angles
            
            
            leftHip.localEulerAngles=new Vector3(0,swingAngle*twistMultiplier,0);
            rightHip.localEulerAngles=new Vector3(0,-swingAngle*twistMultiplier,0);
            Vector3 offset = lockFoot.position-footPos;
            body.position+=offset;
            
/*            float twistAmount = Mathf.Abs(swingAngVel*twistMultiplier*Time.deltaTime);
            if(firstStep)twistAmount*=0.5f;
            if(swingAngle<0)
            {
                if(swingAngVel>0)twistAmount=0;
                pivotBody(swingAngle*rockMultiplier,-twistAmount,leftFoot,rightFoot);
            }else
            {
                if(swingAngVel<0)twistAmount=0;
                pivotBody(swingAngle*rockMultiplier,twistAmount,rightFoot,leftFoot);            
            }*/
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
