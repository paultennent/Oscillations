using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatTracker : MonoBehaviour {

    public Transform floater;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(transform.position.x, floater.position.y * 2, transform.position.z);
        transform.LookAt(floater);
	}
}
