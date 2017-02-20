using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMoverTest : AbstractGameEffects {

	public GameObject pivot;
	public float speed;

    public bool manual = false;

	private float angVelscaler = 3f;
	private float dragConstant = 0.1f;

	public GameObject[] wheels;
	public GameObject[] wheelpivots;
	private float wheelrotationconstant = 100f;

	// Use this for initialization
	void Start () {
        base.Start();
	}

    // Update is called once per frame
    void Update()
    {
        base.Update();

		//angVelscaler = angVelscaler * (1 + climaxRatio);


		speed = speed + (getAccelerationNow () * Time.deltaTime);

        pivot.transform.Translate(Vector3.forward * Time.deltaTime * speed);
		for (int i = 0; i < wheels.Length; i++) {
			wheels[i].transform.RotateAround (wheelpivots[i].transform.position, Vector3.right, Time.deltaTime * speed * wheelrotationconstant);
		}
        
    }

	private float getAccelerationNow(){
		float totalAcc = 0;
		if (swingQuadrant == 1) {
			print ("impelling:"+swingAngVel+":"+speed);
			totalAcc = -swingAngVel * angVelscaler;
		}
        if(speed<0)
        {
            totalAcc += (speed * speed) * dragConstant;
        }else
        {
            totalAcc -= (speed * speed) * dragConstant;
        }
		return totalAcc;
	}
}
