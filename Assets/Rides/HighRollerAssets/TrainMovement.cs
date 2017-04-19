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
            moving = true;
		}
		float exist = Random.Range(0f, 1f);
		if (chanceOfExistence < exist & !autoStart)
		{
			Destroy(gameObject, 0f);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (transform.position.z < player.transform.position.z-4500f && autoStart) {
            speed = -500f;
		}

		if ((transform.position.z - player.transform.position.z) < 750f)
		{
			moving = true;
		}
		if (moving)
		{
			transform.Translate(Vector3.down * Time.deltaTime * speed);
		}
	}
}
