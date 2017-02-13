using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour {

	public int fishCount = 10;
	public GameObject[] fish;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < fishCount; i++) {
			Instantiate(fish[Random.Range(0,fish.Length)]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
