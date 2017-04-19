using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleScript : MonoBehaviour {

    public bool ignoreCollision = false;
    public bool trigger = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (!trigger)
        {
            return;
        }

		float speed = 15f + (5f * transform.localScale.x);
		transform.Translate(Vector3.up * Time.deltaTime * speed);
		if (transform.position.y > 2000) {
			Destroy (gameObject,0);
		}
        if (!ignoreCollision)
        {
            checkForTraps();
        }
	}

	//void OnCollisionEnter(Collision collision){
	//	print ("Bubble Collision");
	//	Destroy (gameObject,0);
	//}

    private void checkForTraps()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, 0.1f))
        {
            //print("Bubble collision");
            Destroy(gameObject, 0f);
            return;
        }
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f))
        {
            //print("Bubble collision");
            Destroy(gameObject, 0f);
            return;
        }
        if (Physics.Raycast(transform.position, Vector3.left, out hit, 0.1f))
        {
            //print("Bubble collision");
            Destroy(gameObject, 0f);
            return;
        }
        if (Physics.Raycast(transform.position, Vector3.right, out hit, 0.1f))
        {
            //print("Bubble collision");
            Destroy(gameObject, 0f);
            return;
        }
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 0.1f))
        {
            //print("Bubble collision");
            Destroy(gameObject, 0f);
            return;
        }
        if (Physics.Raycast(transform.position, Vector3.back, out hit, 0.1f))
        {
            //print("Bubble collision");
            Destroy(gameObject, 0f);
            return;
        }
    }
}
