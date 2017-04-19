using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealifeMovement : MonoBehaviour {

	public float minSpeed = 1f;
	public float maxSpeed = 5f;
	public float sizeMin = 0.5f;
	public float sizeMax = 1.5f;

	float speed;
    bool turnleft = true;

	// Use this for initialization
	void Start () {
		speed = Random.Range (minSpeed, maxSpeed);
		float scale = Random.Range (sizeMin, sizeMax);
		transform.localScale = new Vector3 (scale, scale, scale);
        if (Random.value >= 0.5)
        {
            turnleft = false;
        }

    }
	
	// Update is called once per frame
	void Update () {
        if (checkForTraps())
        {
            if (turnleft)
            {
                transform.Rotate(Vector3.up, speed * Time.deltaTime * 5);
            }
            else
            {
                transform.Rotate(Vector3.down, speed * Time.deltaTime * 5);
            }
        }
        else
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
	}

    private bool checkForTraps()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, 2f))
        {
            return true;
        }
        return false;
    }
}
