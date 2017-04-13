using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlimpMoveScript : MonoBehaviour {

	float speed = 2f;
	public float minSpeed = 1f;
	public float maxSpeed = 5f;

	public bool useAltAxis = false;
	public bool invert = false;


	// Use this for initialization
	void Start () {
		speed = Random.Range (minSpeed, maxSpeed);
	}
	
	// Update is called once per frame
	void Update () {
		if (useAltAxis) {
			if (invert) {
				transform.Translate (-Vector3.right * Time.deltaTime * speed);
			} else {
				transform.Translate (Vector3.right * Time.deltaTime * speed);
			}
		} else {
			transform.Translate (Vector3.down * Time.deltaTime * speed);
		}
	}
}
