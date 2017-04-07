using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSpawner : MonoBehaviour {

	public GameObject planePrefab;
	public Transform viewPoint;
	public Transform planeParent;

	public float frequency = 0.001f;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Random.Range (0f, 1f) < frequency) {
			spawnPlane ();
		}
	}

	private void spawnPlane(){
		float yPos = Random.Range (100f, 500f);
		float xPos = Random.Range (-1000f, 1000f);
		float zPos = Random.Range (viewPoint.position.z - 500f, viewPoint.position.z + 500f);
		float zRot = Random.Range (0, 360f);

		GameObject plane = GameObject.Instantiate (planePrefab);
		plane.transform.position = new Vector3 (xPos, yPos, zPos);
		plane.transform.localEulerAngles = new Vector3 (plane.transform.localEulerAngles.x, plane.transform.localEulerAngles.y, zRot);
		plane.transform.parent = planeParent;
	}
}
