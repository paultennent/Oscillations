using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatchetSwing : AbstractGameEffects {

	public float ratchetRate = 5f;
	public float rachetValue = 0f;

	// Use this for initialization
	void Start () {
		base.Start ();
		swingBase.zeroCrossingEvent.AddListener (zeroCrossing);
	}

	// Update is called once per frame
	void Update () {
		base.Update ();
		applySwingAngle ();
	}

	private void applySwingAngle(){
		float newAngle = swingAngle + rachetValue;
		swingPivot.localEulerAngles = new Vector3 (newAngle, 0, 0);
		swingBase.setAltAngle (-newAngle);
	}

	private void zeroCrossing(){
		if (inSession) {
			if (sessionTime % sessionLength <= climaxTime) {
				rachetValue -= ratchetRate;
			} else {
				rachetValue += ratchetRate;
			}
		} else {
			rachetValue = 0;
		}
	}
}
