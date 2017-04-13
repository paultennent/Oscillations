using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float speed = 15f + (5f * transform.localScale.x);
		transform.Translate(Vector3.up * Time.deltaTime * speed);
		if (transform.position.y > 2000) {
			Destroy (gameObject,0);
		}
	}

	void OnTriggerEnter(Collider c){
		print ("Collision");
		Destroy (gameObject,0);
	}
}
