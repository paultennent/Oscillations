﻿using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.VR;

public class SwingBase : MonoBehaviour {

	//add inverse trasform

	public bool applySwingTransform = true;
	public bool applyInverseSwingTransform = false;
	public bool debug = true;

	private AbstractDataReader swingData;
	private SessionManager sessionManager;

	private Transform swingPivot;
	private Transform swingSeat;
	private Transform viewPoint;

	private Transform headset;

	private float lastSwingAngle;
	private float swingAngle;

	private float altAngle = float.PositiveInfinity;

	public bool sineWave = true;
    public bool forceSineOnMobile=false;
    public float sineAmplitude= 45f;

	private GyroAccelFilter errorFilter=new GyroAccelFilter();
	public UnityEvent zeroCrossingEvent = new UnityEvent();
    
    private float randomPhaseOffset=0f;

	private bool replay = false;
	private ReplayCamMover rcm;

	public Transform getSwingSeat(){
		return swingSeat;
	}

	public Transform getSwingPivot(){
		return swingPivot;
	}

	public float getSwingAngle(){
		return swingAngle;
	}

	public float getAltAngle(){
		return altAngle;
	}

	public void setAltAngle(float a){
		altAngle = a;
	}

	// Use this for initialization
	void Start () {
		swingData = GameObject.FindGameObjectWithTag ("Controller").GetComponent<AbstractDataReader> ();
		sessionManager = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SessionManager> ();
		swingSeat = GameObject.FindGameObjectWithTag ("Swing").transform;
		swingPivot = GameObject.FindGameObjectWithTag ("SwingPivot").transform;
		headset = GameObject.Find("camPositioner").transform;
		viewPoint = GameObject.FindGameObjectWithTag ("ViewPoint").transform;

		#if !UNITY_EDITOR
			sineWave = forceSineOnMobile;
		#endif

		rcm = gameObject.GetComponent<ReplayCamMover> ();
		if (rcm) {
			replay = true;
		}

	}
	
	// Update is called once per frame
	void Update () {

		double time;

		if (!replay) {
			//swingData.dumpVals ();

//		Quaternion q=InputTracking.GetLocalRotation (VRNode.CenterEye); 
			Vector3 p = UnityEngine.XR.InputTracking.GetLocalPosition (UnityEngine.XR.XRNode.CenterEye);
			headset.localPosition = -p;
//		headset.localRotation = q;

			float[] Gxyz = swingData.getHeadingsNow ();
			//float [] Gaccel = swingData.getAccNow();
			time = swingData.getTimeNow ();

			if (!sineWave) {
				swingAngle = Gxyz [1];
				randomPhaseOffset -= Time.time * 2;
			} else {
				if (debug) {
					swingAngle = Mathf.Sin (Time.time * 2.2f + randomPhaseOffset) * sineAmplitude;
				} else {
					swingAngle = Mathf.Sin (Time.time * 2f ) * sineAmplitude;
				}
//			swingAngle = (Mathf.Sin(Time.time * 2+randomPhaseOffset+Random.Range(-.1f,.1f))) * sineAmplitude ;
				//swingAngle = (Mathf.Sin(Time.time * 2)+Random.Range(-.1f,.1f)) * sineAmplitude ;
				//randomPhaseOffset += Random.Range (-Time.deltaTime * 5f, Time.deltaTime * 5f);
			}
		
//		swingAngle=errorFilter.addValue (time, Gxyz [1], Gaccel [2]);
//		if (debug) {
//			if (errorFilter.debugMessage.Length != 0) {
//				print (errorFilter.debugMessage);
//			}
//		}
		} else {
			ResearchReplay.FrameData data = rcm.GetCurrentData();
			if (data != null) {
				time = data.time;
				swingAngle = data.swingAngle;
			} else {
				time = 0f;
				swingAngle = 0f;
			}
		}

		if (applySwingTransform) {
            // This previously rotated by 90 degrees for no reason
			swingPivot.localEulerAngles = new Vector3 (swingAngle, 0, 0);
			viewPoint.localEulerAngles = new Vector3 (-swingAngle, 0, 0);
		}

		if (applyInverseSwingTransform) {
			viewPoint.localEulerAngles = new Vector3 (-swingAngle, 0, 0);
		}

		//sessionManager.onAngle (time, swingAngle, errorFilter, Gxyz [0]);
		sessionManager.onAngle (time, swingAngle);

		checkZeroCrossing ();

		lastSwingAngle = swingAngle;

	}

	private void checkZeroCrossing(){

		if ((lastSwingAngle >= 0 && swingAngle <= 0) ||  (lastSwingAngle <= 0 && swingAngle >= 0)){
			zeroCrossingEvent.Invoke ();
		}
	}
}
