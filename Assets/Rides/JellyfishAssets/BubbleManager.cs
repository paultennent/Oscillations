using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleManager : MonoBehaviour {

	private ParticleSystem ps;
	private JellyfishCamMover jcm;
	private float spawnRange = 100f;


	// Use this for initialization
	void Start () {
		ps = GetComponent<ParticleSystem>();
		jcm = GameObject.Find ("Controller").GetComponent<JellyfishCamMover> ();

		// Every 1 secs we will emit.
		InvokeRepeating("DoEmit", 0.5f, 0.5f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void DoEmit()
	{
		// Any parameters we assign in emitParams will override the current system's when we call Emit.
		// Here we will override the start color and size.
		var emitParams = new ParticleSystem.EmitParams();
		emitParams.position = new Vector3 (Random.Range (transform.position.x-spawnRange, transform.position.x+spawnRange), Random.Range (transform.position.y-spawnRange, transform.position.y+spawnRange), Random.Range (transform.position.z-spawnRange, transform.position.z+spawnRange));
		ps.Emit(emitParams, 1);
	}
}
