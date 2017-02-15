using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatCamMover : AbstractGameEffects {

	private float curHeight=0f;
	private float yVelocity=0f;

	public float velocityDragConstant=.00003f;
	public  float upforceConstant=.001f;
	public  float gravityConstant=9.8f;

	private Vector3 initialPosition;

	public GameObject pivot;

	public bool launch=false;

	public bool infiniteFall = true;

	private Skirt skirtObj;

	// Use this for initialization
	void Start () {
		base.Start();
		initialPosition=pivot.transform.position;
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
