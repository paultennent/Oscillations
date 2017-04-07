using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyfishTileCamMover : AbstractGameEffects  {

    private float curHeight=0f;
    private float yVelocity=0f;
    
    public float velocityDragConstant=.00003f;
    public  float upforceConstant=.001f;
    public  float gravityConstant=9.8f;
    
    private Vector3 initialPosition;

	public GameObject pivot;
	public GameObject skirt;
    
    public bool launch=false;

	public bool infiniteFall = true;

    private Skirt skirtObj;
    
	// Use this for initialization
	void Start () {
        base.Start();
        initialPosition=pivot.transform.position;
        skirtObj=skirt.GetComponent<Skirt>();
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();
        float upforce=calculateUpforce();

        yVelocity+=upforce*Time.deltaTime;
        curHeight=curHeight+yVelocity*Time.deltaTime;
		if(curHeight<0 & !infiniteFall)
        {
            curHeight=0;
            yVelocity=0;
        }
        pivot.transform.position=initialPosition+new Vector3(0,curHeight,0);
        if(skirtObj!=null)
        {
            float skirtAngle = Mathf.Abs(Mathf.Min(swingAngle,45f)/45f)*0.5f;
    //        float skirtAngle = (swingAmplitude
    //        float skirtAngle=(swingAngVel*swingAngVel)*0.0001f;
            //print("curheight:"+curHeight+",curVel"+yVelocity+",curUpforce:"+calculateUpforce()+",av:"+swingAngVel+",sa"+skirtAngle);
            skirtObj.radius1=10*Mathf.Sin(skirtAngle)+5*Mathf.Cos(skirtAngle);
            skirtObj.height=-1 * Mathf.Sin(skirtAngle)-5*Mathf.Cos(skirtAngle);
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
