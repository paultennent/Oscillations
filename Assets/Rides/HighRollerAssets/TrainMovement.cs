using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMovement : MonoBehaviour {

	float speed = 1f;
	bool moving = false;
	GameObject player;
	public bool autoStart = false;
	private float chanceOfExistence = 0.05f;

	// Use this for initialization
	void Start () {
		player = GameObject.Find("Centre");
		speed = Random.Range(150f, 250f);
		if (autoStart) {
			speed = 50f;
		}
		float exist = Random.Range(0f, 1f);
		if (chanceOfExistence < exist & !autoStart)
		{
			Destroy(gameObject, 0f);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (transform.position.z < 0) {
			speed = -Mathf.Abs (speed);
		}

		if (Vector3.Distance(transform.position,player.transform.position) < 1000  || autoStart)
		{
			moving = true;
		}
		if (moving)
		{
			transform.Translate(Vector3.down * Time.deltaTime * speed);
		}
	}
}
