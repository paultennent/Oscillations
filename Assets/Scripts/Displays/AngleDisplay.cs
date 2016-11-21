using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleDisplay : MonoBehaviour {

	SwingBase swingBase;

	// Use this for initialization
	void Start () {
		swingBase = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SwingBase> ();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localEulerAngles = new Vector3 (0,0,-swingBase.getSwingAngle ());
	}
}
