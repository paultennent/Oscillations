using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorScript : MonoBehaviour {

	// Use this for initialization
	Transform player;
	float speed;

	void Start () {

		speed = 200f;

		player = GameObject.Find ("CameraMover").transform;
		transform.position = new Vector3 (player.position.x+30f, transform.position.y, player.position.z+25f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (player.position.x+30f, transform.position.y-(speed*Time.deltaTime), player.position.z+25f);
	}
}
