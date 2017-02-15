using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAI : MonoBehaviour {

	private Transform fishParent;
	public float spawnRange = 200f;
	private float speed = 2f;

	private float rotSpeed = 2f;

	private Vector3 target;
	public Animation anim;
	private Transform root;

	private float ceiling = 1000f;
	private float floor = -1000f;

	// Use this for initialization
	void Start () {
		fishParent = GameObject.Find ("Fish").transform;
		root = GameObject.Find ("Centre").transform;
		transform.parent = fishParent;
		transform.localPosition = new Vector3 (Random.Range (root.position.x-spawnRange, root.position.x+spawnRange), Random.Range (Mathf.Max(floor,root.position.y-spawnRange), Mathf.Min(ceiling,root.position.y+spawnRange)), Random.Range (root.position.z-spawnRange, root.position.z+spawnRange));
		target = new Vector3 (Random.Range (root.position.x-spawnRange, root.position.x+spawnRange), Random.Range (Mathf.Max(floor,root.position.y-spawnRange), Mathf.Min(ceiling,root.position.y+spawnRange)), Random.Range (root.position.z-spawnRange, root.position.z+spawnRange));

		speed = Random.Range (2f, 7.5f);
		rotSpeed = Random.Range (2f, 10f);
		transform.localScale = transform.localScale * (Random.Range (0.5f, 1.5f));
	
	}
	
	// Update is called once per frame
	void Update () {
		//new target if we ever get close
		if (Vector3.Distance (transform.position, target) < 2) {
			target = new Vector3 (Random.Range (root.position.x-spawnRange, root.position.x+spawnRange), Random.Range (Mathf.Max(floor,root.position.y-spawnRange), Mathf.Min(ceiling,root.position.y+spawnRange)), Random.Range (root.position.z-spawnRange, root.position.z+spawnRange));
		}

		SmoothLookAt(target);
		transform.Translate (Vector3.left * speed * Time.deltaTime);
	}

	void SmoothLookAt(Vector3 t)
	{
		Vector3 dir = t - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation(dir);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
	}

	public void setCeiling(float i){
		ceiling = i;
	}

	public void setFloor(float i){
		floor = i;
	}
}
