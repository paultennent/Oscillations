using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandRoom : AbstractGameEffects {

	Transform room; 
	public float roomMultiplyFactor = 1.5f;

	private float G;
	private float roomMultiply = 0f;

	Vector3 baseScale;

	// Use this for initialization
	void Start () {
		base.Start ();
		room = GameObject.FindGameObjectWithTag("Room").transform;
		G = climaxTime / Mathf.Sqrt(roomMultiplyFactor);
		baseScale = room.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		roomMultiply=Mathf.Min(1+(offsetTime/G)*(offsetTime/G),1+roomMultiplyFactor);

		Vector3 maxScale = baseScale * roomMultiply;

		//need to edit this...
		room.localScale = maxScale;

	}
}
