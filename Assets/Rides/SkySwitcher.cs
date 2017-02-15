using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySwitcher : MonoBehaviour {

	public Transform pivot;
	public Material upsky;
	public Material downsky;

	private bool above = true;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (pivot.position.y >= 0) {
			if (!above) {
				RenderSettings.skybox = upsky;
				above = true;
			}
		} else {
			if (above) {
				RenderSettings.skybox = downsky;
				above = false;
			}
		}
	}
}
