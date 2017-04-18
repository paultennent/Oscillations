using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour {

	public GameObject bubbleprefab;
	Transform mySpawner;
	public float sizeMin = 0.05f;
	public float sizeMax = 0.5f;
	public float range = 20f;
	public float rate = 0.001f;
	public float spawnYoffset = 10f;
	public Transform bubbleparent;

	// Use this for initialization
	void Start () {
		mySpawner = new GameObject ().transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (Random.Range (0f, 1f) <= rate) {
			spawnBubble ();
		}
	}

	private void spawnBubble(){
		mySpawner.position = new Vector3(transform.position.x,transform.position.y+spawnYoffset,transform.position.z);
		float ang = Random.Range (0f, 360f);
		float dist = Random.Range (5f, range);
		mySpawner.localEulerAngles = new Vector3(0,ang,0);
		mySpawner.Translate (Vector3.forward * range);

		GameObject bubble = Instantiate (bubbleprefab,mySpawner.transform.position,Quaternion.identity);
		bubble.transform.localScale = bubble.transform.localScale * Random.Range (sizeMin, sizeMax);
		bubble.transform.parent = bubbleparent;
	}
}
