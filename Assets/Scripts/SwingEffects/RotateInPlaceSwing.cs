using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInPlaceSwing : AbstractGameEffects {

	public int axis = 2;

	// Use this for initialization
	void Start () {
		base.Start ();
		viewPoint.parent = null;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		if(axis == 0){
			viewPoint.localEulerAngles = new Vector3 (swingAngle, viewPoint.localEulerAngles.y, viewPoint.localEulerAngles.z);
		}else if(axis == 1){
			viewPoint.localEulerAngles = new Vector3 (viewPoint.localEulerAngles.x, swingAngle, viewPoint.localEulerAngles.z);
		}else if(axis == 2){
			viewPoint.localEulerAngles = new Vector3 (viewPoint.localEulerAngles.x, viewPoint.localEulerAngles.y, swingAngle);
		}
	}
}
