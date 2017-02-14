using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlecockCamMover : AbstractGameEffects {

	public Transform point1;
	public Transform point2;

	public Transform pivot;

	private bool forward;

	// Use this for initialization
	void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();

		float dist = Vector3.Distance (point1.transform.position, point2.transform.position);

		if(swingPhase <= 2){
			forward = true;
			float ratio = swingPhase / 2f;
			print("Forward:"+ratio);
			Vector3 p = Vector3.Lerp (point1.position, point2.position, ratio);
			float yheight = 8 * -(ratio * ratio);
			pivot.position = new Vector3 (p.x, p.y + yheight, p.z);

		}else if(swingPhase > 2){
			forward = false;
			float ratio = (swingPhase - 2) / 2f;
			print("Back:"+ratio);
			Vector3 p = Vector3.Lerp (point2.position, point1.position, ratio);
			float yheight = 8 * -(ratio * ratio);
			pivot.position = new Vector3 (p.x, p.y + yheight, p.z);
		}

	}
		
}
