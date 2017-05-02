using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryLevels : MonoBehaviour {

	private MagicReader mr;
	public Canvas viewCanvas;
	public Text headsetDisplay;
	public Text swingDisplay;
	public Text fps;
	public Text swingAngle;
	public Text connectionState;

	private bool visible = false;

	// Use this for initialization
	void Start () {
		mr = GameObject.FindGameObjectWithTag ("Controller").GetComponent<MagicReader> ();
        viewCanvas.worldCamera=Camera.main;
        viewCanvas.planeDistance=0.5f;
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetButton("Tap"))
		{
			visible = true;
			viewCanvas.enabled = true;
            FadeSphereScript.enableFader(false);
		}

		if(Input.GetButtonUp("Tap"))
		{
			visible = false;
			viewCanvas.enabled = false;
            FadeSphereScript.enableFader(true);
		}

		if (visible) {
			headsetDisplay.text = "Headset Battery: " + (int) (mr.getLocalBatteryLevel () * 100f) + "%";
			swingDisplay.text = "Swing Battery: " + (int) (mr.getRemoteBatteryLevel () * 100f) + "%";
			fps.text = "Framerate:" + ((int)(1.0f / Time.deltaTime));
            swingAngle.text= "Swing angle: "+mr.getAngle();
            connectionState.text="Connection state: "+mr.getConnectionState();
		}
	}
}
