﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmplifySwing : AbstractGameEffects {


	public float swingMultiplyFactor = 2.0f;

	private float G;

	public float maxAngle = 175f;
	private float swingMultiply = 0f;

	// Use this for initialization
	void Start () {
		base.Start ();
		G = climaxTime / Mathf.Sqrt(swingMultiplyFactor);
	}

	// Update is called once per frame
	void Update () {
		base.Update ();
		if (inSession) {
			swingMultiply = Mathf.Min (1 + (offsetTime / G) * (offsetTime / G), 1 + swingMultiplyFactor);

		} else {
			swingMultiply = 1f;
		}
		applySwingAngle ();
	}

	private void applySwingAngle(){
		float newAngle = swingAngle * swingMultiply;
		newAngle = Mathf.Clamp (newAngle, -maxAngle, maxAngle);
		swingPivot.localEulerAngles = new Vector3 (newAngle, 0, 0);
		viewPoint.localEulerAngles = new Vector3 (-swingAngle, 0, 0);
		swingBase.setAltAngle (-newAngle);
	}
}
