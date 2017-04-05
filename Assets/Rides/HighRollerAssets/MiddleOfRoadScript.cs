using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiddleOfRoadScript : MonoBehaviour {

	public Transform track;
	public char axis = 'z';

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(axis == 'z'){
			transform.position = new Vector3(transform.position.x,transform.position.y,track.position.z);
		}else if( axis == 'x'){
			transform.position = new Vector3(track.position.x,transform.position.y,transform.position.z);
		}
	}
}
