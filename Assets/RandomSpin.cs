using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpin : MonoBehaviour {

	float xRot;
	float yRot;
	float zRot;

	float speed;

	// Use this for initialization
	void Start () {
		xRot = Random.Range (0f, 1f);
		yRot = Random.Range (0f, 1f);
		zRot = Random.Range (0f, 1f);
		speed = Random.Range (0f, 20f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3 (xRot, yRot, zRot), speed * Time.deltaTime);
	}
}
