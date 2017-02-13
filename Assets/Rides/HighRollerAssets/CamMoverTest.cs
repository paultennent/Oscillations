using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMoverTest : AbstractGameEffects {

	public GameObject pivot;
	public float speed;

    public bool manual = false;
    public Transform wheel;

	private float angVelscaler = 3f;
	private float dragConstant = 0.3f;

	// Use this for initialization
	void Start () {
        base.Start();
	}

    // Update is called once per frame
    void Update()
    {
        base.Update();

		//angVelscaler = angVelscaler * (1 + climaxRatio);

        if (!inSession) {
            wheel.GetComponent<Renderer>().enabled = false;
            return; }

        wheel.GetComponent<Renderer>().enabled = true;

		speed = speed + (getAccelerationNow () * Time.deltaTime);

        pivot.transform.Translate(Vector3.forward * Time.deltaTime * speed);
        wheel.RotateAround(pivot.transform.position, Vector3.left, Time.deltaTime * speed);
        
    }

	private float getAccelerationNow(){
		float totalAcc = 0;
		if (swingQuadrant == 3) {
			//print ("impelling");
			totalAcc = -swingAngVel * angVelscaler;
		}
		totalAcc -= (speed * speed) * dragConstant;
		return totalAcc;
	}
}
