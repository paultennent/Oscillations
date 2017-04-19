using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorScript : MonoBehaviour {

	// Use this for initialization
	Transform player;
    float speed;
    float xOffset;

    float x;
    float z;

    void Start () {

		speed = 200f;

        xOffset = Random.Range(50f, 70f);

        if(Random.Range(0f,1f) > 0.5f)
        {
            xOffset = -xOffset;
        }

		player = GameObject.Find ("CameraMover").transform;
        x = player.position.x + xOffset;
        z = player.position.z + 25f;

        transform.position = new Vector3 (x, transform.position.y, z);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (x, transform.position.y-(speed*Time.deltaTime), z);
	}
}
