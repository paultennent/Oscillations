using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour {

	public int fishCount = 10;
	public GameObject[] fish;

	public GameObject[] myFish;
	public Transform player;

	public float ceiling = 1000f;
	public float floor = -1000f;

	// Use this for initialization
	void Start () {
		myFish = new GameObject[fishCount];
		for (int i = 0; i < fishCount; i++) {
			myFish[i] = Instantiate(fish[Random.Range(0,fish.Length)]);
			myFish [i].GetComponent<FishAI> ().setCeiling (ceiling);
			myFish [i].GetComponent<FishAI> ().setFloor (floor);
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < myFish.Length; i++) {
			if (Vector3.Distance (player.position, myFish [i].transform.position) > 400) {
				Destroy (myFish [i],0f);
				myFish[i] = Instantiate(fish[Random.Range(0,fish.Length)]);
				myFish [i].GetComponent<FishAI> ().setCeiling (ceiling);
				myFish [i].GetComponent<FishAI> ().setFloor (floor);
			}
		}
	}
}
