using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherStationSpawner : MonoBehaviour {

	public int minStations = 5;
	public int maxStations = 20;
	public GameObject station;
	public float minDistance = 1500;
	public float maxDistance = 10000;
	public Transform transformParent;

	// Use this for initialization
	void Start () {
		int numStations = Random.Range (minStations, maxStations + 1);
		for (int i = 0; i < numStations; i++) {

			//randomise the position
			float xPos = Random.Range (minDistance, maxDistance);
			if (Random.Range (0f, 1f) > .5f) {
				xPos = -xPos;
			}
			float yPos = Random.Range (minDistance, maxDistance);
			if (Random.Range (0f, 1f) > .5f) {
				yPos = -yPos;
			}
			float zPos = Random.Range (minDistance, maxDistance);
			if (Random.Range (0f, 1f) > .5f) {
				zPos = -zPos;
			}

			//spawn it
			GameObject go = Instantiate(station,new Vector3(xPos,yPos,zPos),Quaternion.identity,transformParent);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
