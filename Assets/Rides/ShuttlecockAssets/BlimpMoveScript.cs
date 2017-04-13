using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlimpMoveScript : MonoBehaviour {

	float speed = 2f;
	public float minSpeed = 1f;
	public float maxSpeed = 5f;


	// Use this for initialization
	void Start () {
		speed = Random.Range (minSpeed, maxSpeed);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(Vector3.down * Time.deltaTime * speed);
	}
}
