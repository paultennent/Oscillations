using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamMover : AbstractGameEffects {

    public float centralCircleRadius=10f;
    public float parabolaConstant=2.5f;
    public float swingMaxDistance=100f;
    
    public float []offsetY=new float[900];
    public float []angleY=new float[900];
    
    public bool fadedIn=false;
    

	public bool doMadRotate = false;
    
    float lastConstant=0f;
    float lastMaxDistance=0f;
    Transform posOfCamera;
    
	// Use this for initialization
	void Start () {               
        base.Start();
        posOfCamera=GameObject.Find("ViewPoint").transform;
        CalculateOffsetArray();
    }
    
    void CalculateOffsetArray()
    {
        lastConstant=parabolaConstant;
        lastMaxDistance=swingMaxDistance;
        // calculate 1000 points on this parabola that are uniform distance apart
        offsetY[0]=0;
        float ptDistance=swingMaxDistance*0.001f;
        float curX=0;
        float lastY=0;
        float lastX=0;
        for(int c=1;c<900;c++)
        {
            float dist=0;
            float curY=lastY;
            while(dist<ptDistance*ptDistance)
            {
                curX+=0.01f;
                curY=(curX*curX)/(4f*parabolaConstant);
                dist=(curY-lastY)*(curY-lastY)+(curX-lastX)*(curX-lastX);
            }
            angleY[c]=Mathf.Atan2(curX-lastX,curY-lastY)*Mathf.Rad2Deg-90;
            offsetY[c]=curY;
            lastY=curY;
            lastX=curX;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(lastConstant!=parabolaConstant || lastMaxDistance!=swingMaxDistance)
        {
            CalculateOffsetArray();
        }

        base.Update();
		if (!inSession) {
			return;
		}

        if (!fadedIn)
        {
            FadeSphereScript.doFadeIn(5f, Color.black);
            fadedIn = true;
        }

        // map from swing angle to point on parabola x^2 = 4ay (where a = parabolaConstant parameter)
        int parabolaYIndex=(int)(Mathf.Abs(swingAngle)*10);
        if(parabolaYIndex>899)
        {
            parabolaYIndex=899;
        }
        float angle=angleY[parabolaYIndex];
        float parabolaY=offsetY[parabolaYIndex];        
        float parabolaZ=Mathf.Sqrt(4f*parabolaConstant*parabolaY);
        if(swingAngle<0)
        {
            parabolaZ=-parabolaZ;
            angle=-angle;
        }
        parabolaY-=centralCircleRadius;
        print(parabolaY+":"+parabolaZ+":"+swingAngle);
        posOfCamera.localPosition=new Vector3(0,parabolaY,parabolaZ);
        posOfCamera.localRotation=Quaternion.Euler(angle,0f,0f);
        
    }

}
