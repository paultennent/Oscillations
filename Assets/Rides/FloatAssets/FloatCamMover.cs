using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatCamMover : AbstractGameEffects {

	private float curHeight=0f;
	private float yVelocity=0f;
	public  float gravityConstant=9.8f;
	public float buoyancy = 10f;

	public float buoyancyModConstant = 3f;
	public float angularVelocityCoefficient = 0.01f;

	private Vector3 initialPosition;
	public GameObject pivot;

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
		pivot.transform.position=initialPosition+new Vector3(0,curHeight,0);
	
	}

	float calculateUpforce()
	{
		float totalForce = 0f;


		totalForce-=gravityConstant;

		float curYPos = pivot.transform.position.y;
		float curbouy = buoyancy;

		if (curYPos > 0f) {
			curbouy = buoyancy - (curYPos * buoyancyModConstant); 	 
		}

		totalForce+=curbouy;

		//add impel upwards:
		if (swingQuadrant == 0) {
			totalForce += Mathf.Abs(swingAngVel) * angularVelocityCoefficient;
		}

		return totalForce;
	}
}
