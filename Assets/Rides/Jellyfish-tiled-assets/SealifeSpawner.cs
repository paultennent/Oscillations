using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealifeSpawner : MonoBehaviour {

	public GameObject[] lifeprefabs;
	Transform mySpawner;

	public float range = 100f;
	public float rateDivisor = 100f;
	public Transform bubbleparent;

	private GameObject[] weightedPrefabs;

	// Use this for initialization
	void Start () {
		mySpawner = new GameObject ().transform;
		bubbleparent = GameObject.Find ("FishParent").transform;
		int count = 0;
		for (int i = lifeprefabs.Length; i > 0; i--) {
			count += i;
		}
		weightedPrefabs = new GameObject[count];
		int counter = 0;
		for(int j = 0;j< lifeprefabs.Length;j++){
			for (int i = 0; i <= j; i++) {
				weightedPrefabs [counter] = lifeprefabs [j];
				counter++;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		float rate = (transform.position.y / rateDivisor / 1000f);
		if (Random.Range (0f, 1f) <= rate) {
			spawnBubble ();
		}
	}

	private void spawnBubble(){
		mySpawner.position = new Vector3(transform.position.x,transform.position.y,transform.position.z);
		float ang = Random.Range (0f, 360f);
		float xAng = Random.Range (-20f, 20f);
		float dist = Random.Range (2f, range);
		mySpawner.localEulerAngles = new Vector3(0,ang,0);
		mySpawner.Translate (Vector3.forward * range);

		GameObject bubble = Instantiate (weightedPrefabs[Random.Range(0,weightedPrefabs.Length)],mySpawner.transform.position,Quaternion.identity);
		bubble.transform.localEulerAngles = new Vector3(Random.Range (-90f, 90f),Random.Range (0f, 360f),Random.Range (-5f, 5f));


		bubble.transform.parent = bubbleparent;
	}


}