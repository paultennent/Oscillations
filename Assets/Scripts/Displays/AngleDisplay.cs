using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleDisplay : MonoBehaviour {
	private SwingBase swingBase;
    private MagicReader reader;
    
    public bool showSwingAngle = true;
    public bool showTiltAngle = false;
    public bool showTwistAngle = false;


	// Use this for initialization
	void Start () {
		swingBase = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SwingBase> ();
        reader = GameObject.FindGameObjectWithTag ("Controller").GetComponent<MagicReader> ();
	}
	
	// Update is called once per frame
	void Update () {
        if(showSwingAngle){
            transform.localEulerAngles = new Vector3 (0,0,swingBase.getSwingAngle ());
        }
        if(showTiltAngle){
            transform.localEulerAngles = new Vector3 (0,0,reader.getSwingTilt());
        }
        if(showTwistAngle){
            transform.localEulerAngles = new Vector3 (0,0,reader.getMagDirection());
        }
	}
}
