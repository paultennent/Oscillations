using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealifeMovement : MonoBehaviour {

	public float minSpeed = 1f;
	public float maxSpeed = 5f;
	public float sizeMin = 0.5f;
	public float sizeMax = 1.5f;

	float speed;

	// Use this for initialization
	void Start () {
		speed = Random.Range (minSpeed, maxSpeed);
		float scale = Random.Range (sizeMin, sizeMax);
		transform.localScale = new Vector3 (scale, scale, scale);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * Time.deltaTime * speed);	
	}

	void OnCollisionEnter(Collision collision){
		print ("Sealife Collision");
	}
}
