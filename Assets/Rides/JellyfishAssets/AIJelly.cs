using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIJelly : MonoBehaviour  {

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

	private float swingAngle = 0f;
	private float swingAngVel;
	private float lastAngle = 0f;

	private float speed = 2f;

	private float spawnRange = 200f;


	// Use this for initialization
	void Start () {
		speed = speed * Random.Range(0.5f,1.5f);
		transform.localScale = transform.localScale * Random.Range(0.5f,1.5f);
		swingAngle = Random.Range(0f,360f);
		initialPosition = new Vector3 (Random.Range (transform.position.x-spawnRange, transform.position.x+spawnRange), Random.Range (transform.position.y-spawnRange, transform.position.y+spawnRange), Random.Range (transform.position.z-spawnRange, transform.position.z+spawnRange));

		skirtObj=skirt.GetComponent<Skirt>();
		transform.parent = GameObject.Find ("JellySpawner").transform;
	}

	// Update is called once per frame
	void Update () {

		swingAngle = Mathf.Sin(Time.time * speed) * 45;
		swingAngVel=(swingAngle-lastAngle)/Time.deltaTime;
		lastAngle = swingAngle;

		float upforce=calculateUpforce();

		yVelocity+=upforce*Time.deltaTime;
		curHeight=curHeight+yVelocity*Time.deltaTime;
		if(curHeight<0 & !infiniteFall)
		{
			curHeight=0;
			yVelocity=0;
		}
		pivot.transform.position=initialPosition+new Vector3(0,curHeight,0);
		float skirtAngle = Mathf.Abs(Mathf.Min(swingAngle,45f)/45f)*0.5f;
		skirtObj.radius1=10*Mathf.Sin(skirtAngle)+5*Mathf.Cos(skirtAngle);
		skirtObj.height=-1 * Mathf.Sin(skirtAngle)-5*Mathf.Cos(skirtAngle);
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
