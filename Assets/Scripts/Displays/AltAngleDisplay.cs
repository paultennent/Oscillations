using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltAngleDisplay : MonoBehaviour {

	SwingBase swingBase;
	Renderer render;

	// Use this for initialization
	void Start () {
		swingBase = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SwingBase> ();
		render = GetComponentInChildren<Renderer> ();
	}

	// Update is called once per frame
	void Update () {
		float angle = swingBase.getAltAngle ();
		if (angle != float.PositiveInfinity) {
			transform.localEulerAngles = new Vector3 (0, 0, angle);
			render.enabled = true;
		} else {
			render.enabled = false;
		}

	}
}
