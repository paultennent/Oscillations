using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour {

	Transform swing;
	Transform view;

	public Text swingx;
	public Text swingy;
	public Text swingz;
	public Text swingmag;

	public Text viewx;
	public Text viewy;
	public Text viewz;
	public Text viewmag;

	public Text fpsText;
	public Text sameCount;

	private Vector3 lastSwing;
	private Vector3 lastView;

	MagicReader mr;


	// Use this for initialization
	void Start () {
		swing = GameObject.FindGameObjectWithTag ("Swing").transform;
		view = GameObject.FindGameObjectWithTag ("ViewPoint").transform;
		mr = GameObject.FindGameObjectWithTag ("Controller").GetComponent<MagicReader> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (lastSwing == null) {
			lastSwing = swing.position;
			lastView = view.position;
		}

		Vector3 swingVelocity = (swing.position - lastSwing) / Time.deltaTime;
		Vector3 viewVelocity = (view.position - lastView) / Time.deltaTime;

		swingx.text = "X:" + swingVelocity.x;
		swingy.text = "Y:" + swingVelocity.y;
		swingz.text = "Z:" + swingVelocity.z;
		swingmag.text = "M:" + swingVelocity.magnitude;

		viewx.text = "X:" + viewVelocity.x;
		viewy.text = "Y:" + viewVelocity.y;
		viewz.text = "Z:" + viewVelocity.z;
		viewmag.text = "M:" + viewVelocity.magnitude;

		fpsText.text = "FPS:" + (1.0f / Time.deltaTime);
		sameCount.text = "SC:" + mr.getSameDataCount ();

		lastSwing = swing.position;
		lastView = view.position;
	}
}
