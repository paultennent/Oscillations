using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalkerCamMover : AbstractGameEffects  
{
    
    public Transform leftFoot;
    public Transform rightFoot;
    
    public Transform body;
    
    private Transform bodyParent;
    
    public float rockMultiplier=0.25f;
    public float twistMultiplier=0.25f;
    
	// Use this for initialization
	void Start () 
    {
        base.Start();
        bodyParent=body.parent;
	}

    bool seenZeroCrossing=false;
    bool firstStep=true;
    
    float lastAngle=0;
    float maxAngle=0;
	// Update is called once per frame
	void Update () 
    {
        
        base.Update();
        // are we going left-> right or right->left
        if((lastAngle<0 && swingAngle>=0) || (lastAngle>=0 && swingAngle<0))
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
            float twistAmount = Mathf.Abs(swingAngVel*twistMultiplier*Time.deltaTime);
            if(firstStep)twistAmount*=0.5f;
            if(swingAngle<0)
            {
                pivotBody(swingAngle*rockMultiplier,-twistAmount,leftFoot,rightFoot);
            }else
            {
                pivotBody(swingAngle*rockMultiplier,twistAmount,rightFoot,leftFoot);            
            }
        }
        lastAngle=swingAngle;
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
