using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VandARoomExtender : AbstractGameEffects {

	public GameObject[] roomBase;
	public GameObject extenders;
	public GameObject legs;

	private float ratio = 0f;
	private float rate = 0.25f;

	private float wallDropMin = 0f;
	private float wallDropMax = 20f;

	private float extendScaleMin = 0f;
	private float extendScaleMax = 2f;

	private float extenderMovementMin = -7.418f;
	private float extenderMovementMax = 7.55f;

	private float legScaleMin = 1f;
	private float legScaleMax = 11f;

	private float legMoveMin = 0f;
	private float legMoveMax = 20f;

	private bool toReset = false;
	public float resetTime = 2f;
	private bool resetting = false;

	private float scale;


	// Use this for initialization
	void Start () {
		base.Start ();
		scale = GameObject.Find ("Gallery").transform.localScale.y;

		wallDropMin *= scale;
		wallDropMax *= scale;
		extendScaleMin *= scale;
		extendScaleMax *= scale;
		extenderMovementMin *= scale;
		extenderMovementMax *= scale;
		//legScaleMin *= //scale / 2f;
		//legScaleMax *= //scale / 2f;
		legMoveMin *= scale;
		legMoveMax *= scale;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		if (inSession) {
			ratio = climaxRatio;
			toReset = true;
			setRoomVals ();
		} else {
			//zeroRoomVals ();
			if (toReset && !resetting) {
				StartCoroutine (smoothZeroRoomVals ());
			}
		}
//		if(Input.GetKey(KeyCode.UpArrow)){
//			up ();
//			setRoomVals();
//		}
//
//		if(Input.GetKey(KeyCode.DownArrow)){
//			down ();
//			setRoomVals();
//		}
	}

	private void zeroRoomVals(){
		foreach (GameObject go in roomBase) {
			go.transform.position = new Vector3 (go.transform.position.x, wallDropMin, go.transform.position.z);
		}
		extenders.transform.localScale = new Vector3 (extenders.transform.localScale.x, extendScaleMin, extenders.transform.localScale.z);
		extenders.transform.position = new Vector3 (extenders.transform.position.x, extenderMovementMin, extenders.transform.position.z);
		legs.transform.localScale = new Vector3 (legs.transform.localScale.x, legScaleMin, legs.transform.localScale.z);
		legs.transform.position = new Vector3 (legs.transform.position.x, legMoveMin, legs.transform.position.z);
	}

	private IEnumerator smoothZeroRoomVals(){
		float startRatio = ratio;
		resetting = true;
		float timer = resetTime;
		while (timer > 0f) {
			if (inSession) {
				zeroRoomVals ();
				resetting = false;
				yield break;
			}
			timer = timer - (Time.deltaTime * resetTime);
			ratio = (timer / resetTime) * startRatio;
			setRoomVals ();
			yield return null;
		}
		zeroRoomVals ();
		toReset = false;
		resetting = false;
	}

	private void setRoomVals(){
		//drop walls first
		float basePos = -mapFloat (ratio, 0f, 1f, wallDropMin, wallDropMax);
		foreach (GameObject go in roomBase) {
			go.transform.position = new Vector3 (go.transform.position.x, basePos, go.transform.position.z);
		}

		//now extend extenders
		float extrnderScale = mapFloat (ratio, 0f, 1f, extendScaleMin, extendScaleMax);
		extenders.transform.localScale = new Vector3 (extenders.transform.localScale.x, extrnderScale, extenders.transform.localScale.z);

		float extenderPos = -mapFloat (ratio, 0f, 1f, extenderMovementMin, extenderMovementMax);
		extenders.transform.position = new Vector3 (extenders.transform.position.x, extenderPos, extenders.transform.position.z);

		//now extend legs
		float legextrnderScale = mapFloat (ratio, 0f, 1f, legScaleMin, legScaleMax);
		legs.transform.localScale = new Vector3 (legs.transform.localScale.x, legextrnderScale, legs.transform.localScale.z);

		float legextenderPos = -mapFloat (ratio, 0f, 1f, legMoveMin, legMoveMax);
		legs.transform.position = new Vector3 (legs.transform.position.x, legextenderPos, legs.transform.position.z);
			
	}

	private void down (){
		ratio = ratio + (Time.deltaTime * rate);
		ratio = Mathf.Clamp (ratio, 0f, 1f);
	}

	private void up(){
		ratio = ratio - (Time.deltaTime * rate);
		ratio = Mathf.Clamp (ratio, 0f, 1f);
	}

	private float mapFloat (float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
		
}
