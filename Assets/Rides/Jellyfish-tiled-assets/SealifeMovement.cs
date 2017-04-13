using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealifeMovement : MonoBehaviour {

	public float minSpeed = 1f;
	public float maxSpeed = 5f;

	float speed;

	// Use this for initialization
	void Start () {
		speed = Random.Range (minSpeed, maxSpeed);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * Time.deltaTime * speed);	
	}
}
