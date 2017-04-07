using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneMovescript : MonoBehaviour {

	float speed = 100;
	Transform viewPoint;

	// Use this for initialization
	void Start () {
		viewPoint = GameObject.Find ("Centre").transform;
		speed = Random.Range (100f, 500f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(Vector3.down * Time.deltaTime * speed);
		if(Vector3.Distance(transform.position,viewPoint.transform.position) > 10000f){
			Destroy(gameObject,0);
		}
	}
}
