using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopperMovementScript : MonoBehaviour {

	public float speedMin = 30;
	public float speedMax = 50f;
	float speed = 100;
	Transform viewPoint;

	// Use this for initialization
	void Start () {
		viewPoint = GameObject.Find ("TrackingPoint").transform;
		speed = Random.Range (speedMin, speedMax);
	}

	// Update is called once per frame
	void Update () {
		transform.Translate(Vector3.down * Time.deltaTime * speed);
		if(Vector3.Distance(transform.position,viewPoint.transform.position) > 10000f){
			Destroy(gameObject,0);
		}
	}
}