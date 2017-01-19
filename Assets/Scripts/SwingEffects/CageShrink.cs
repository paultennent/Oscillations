using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageShrink : AbstractGameEffects {

	public GameObject cage;
	public Vector3 targetScale;

	private Vector3 scaleMod;
	private Vector3 currentScale;
	private Vector3 startScale;

	// Use this for initialization
	void Start () {
		base.Start ();
		startScale = cage.transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		if (inSession) {
			Vector3 scaleDiff = new Vector3 (startScale.x - targetScale.x, startScale.y - targetScale.y, startScale.z - targetScale.z);
			scaleMod = scaleDiff * climaxRatio;
			currentScale = startScale - scaleMod;
			cage.transform.localScale = startScale - scaleMod;
		} else {
			cage.transform.localScale = startScale;
		}
	}
}
