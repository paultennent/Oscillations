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
    private float visibleTime=0f;

    private bool fadeEnabled=false;
    
    private float downY=0f;
    
	// Use this for initialization
	void Start () {
		mr = GameObject.FindGameObjectWithTag ("Controller").GetComponent<MagicReader> ();
        viewCanvas.worldCamera=Camera.main;
        viewCanvas.planeDistance=0.5f;
	}
	
	// Update is called once per frame
	void Update () {
        if(visibleTime>0f)
        {
            if(!visible)
            {
                fadeEnabled=FadeSphereScript.isEnabled();
                FadeSphereScript.enableFader(false);
            }
			visible = true;
			viewCanvas.enabled = true;
            visibleTime-=Time.deltaTime;
            if(visibleTime<=0)
            {
                visible = false;
                viewCanvas.enabled = false;
                if(fadeEnabled)
                {
                    FadeSphereScript.enableFader(fadeEnabled);
                }
            }
        }

		if(Input.GetButtonDown("Tap"))
		{
            downY=Input.mousePosition.y;
            print("Down:"+downY);
		}
        if(Input.GetButtonUp("Tap"))
        {
            float upY=Input.mousePosition.y;
            print("Up:"+upY);
            float diffY=upY-downY;
            if(diffY>200f)
            {
                // turn on for 2 seconds with up swipe
                visibleTime=2f;
            }
            if(diffY<-200f && visibleTime>0)
            {
                // turn off with down swipe
                visibleTime=0.01f;
            }
            print("Swipe:"+diffY);
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
