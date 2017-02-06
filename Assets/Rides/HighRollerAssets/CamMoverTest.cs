using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMoverTest : MonoBehaviour {

	public GameObject pivot;
	public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.UpArrow)){
			speed = speed + (1 * Time.deltaTime);
			pivot.transform.Translate (Vector3.forward * Time.deltaTime * speed);
		}

		if(Input.GetKey(KeyCode.DownArrow)){
			speed = speed + (1 * Time.deltaTime);
			pivot.transform.Translate (Vector3.back * Time.deltaTime * speed);
		}

		if(Input.GetKeyUp(KeyCode.UpArrow)){
			speed = 1;
			pivot.transform.Translate (Vector3.forward * Time.deltaTime * speed);
		}

		if(Input.GetKeyUp(KeyCode.DownArrow)){
			speed = 1;
			pivot.transform.Translate (Vector3.back * Time.deltaTime * speed);
		}
	}
}
