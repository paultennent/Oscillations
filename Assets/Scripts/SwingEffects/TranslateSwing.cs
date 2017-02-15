using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateSwing : AbstractGameEffects {

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
			swingSeat.position = new Vector3 (swingSeat.position.x, viewPoint.position.y, viewPoint.position.z);
		}else if(axis == 1){
			swingSeat.position = new Vector3 (viewPoint.position.x, swingSeat.position.y, viewPoint.position.z);
		}else if(axis == 2){
			swingSeat.position = new Vector3 (viewPoint.position.x, viewPoint.position.y, swingSeat.position.z);
		}
	}
}
