using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyTracker : MonoBehaviour {

    public Transform center;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(center);
        if(center.transform.position.y > 50f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 3 * Time.deltaTime, transform.position.z);
        }
	}
}
