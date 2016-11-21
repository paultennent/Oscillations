using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFloor : AbstractGameEffects {

	private Vector3 basePos;
	private Transform floor;

	public float roomMaxMove = 2f;

	private float G;
	private float roomPosY;

	// Use this for initialization
	void Start () {
		base.Start ();
		floor = GameObject.FindGameObjectWithTag("Floor").transform;
		G = climaxTime / Mathf.Sqrt(roomMaxMove);
		basePos = floor.position;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		roomPosY= basePos.y - Mathf.Min((offsetTime/G)*(offsetTime/G),roomMaxMove);
		floor.position = new Vector3 (basePos.x, roomPosY, basePos.z);
	}
}
