using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellySpawner : MonoBehaviour {

	public GameObject jellyPrefab;
	public int count;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < count; i++) {
			Instantiate (jellyPrefab);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
