using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageSwing : AbstractGameEffects {

	public Transform cage;
	public Transform cagePivot;
	private Vector3 startAngles;

	// Use this for initialization
	void Start () {
		base.Start ();
		startAngles = cagePivot.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();

		if (inSession) {
			float cageAngle = swingAngle * climaxRatio;
			cagePivot.localEulerAngles = new Vector3 (cageAngle, 0, 0);
		}else{
			cagePivot.localEulerAngles = startAngles;
		}
	}
}
