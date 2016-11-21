using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertSwing : AbstractGameEffects {

	// Use this for initialization
	void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		swingPivot.localEulerAngles = new Vector3 (-swingAngle, 0, 0);
		swingBase.setAltAngle (swingAngle);
	}
}
