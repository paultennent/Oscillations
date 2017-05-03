using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCam : MonoBehaviour {

	// Use this for initialization

	Vector3 pos;

	void Start () {
		pos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton ("Tap")) {
			transform.Translate (Vector3.up * Time.deltaTime);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			transform.position = pos;
		}
	}
}
