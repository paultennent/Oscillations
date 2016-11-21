using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SwingBase : MonoBehaviour {

	public bool applySwingTransform = true;
	public bool debug = true;

	private AbstractDataReader swingData;
	private SessionManager sessionManager;

	private Transform swingPivot;
	private Transform swingSeat;

	private float lastSwingAngle;
	private float swingAngle;

	private float altAngle = float.PositiveInfinity;

	public bool sineWave = true;

	private GyroAccelFilter errorFilter=new GyroAccelFilter();
	public UnityEvent zeroCrossingEvent = new UnityEvent();


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
	}
	
	// Update is called once per frame
	void Update () {

		//swingData.dumpVals ();

		float[] Gxyz = swingData.getHeadingsNow ();
		//float [] Gaccel = swingData.getAccNow();
		double time = swingData.getTimeNow();

		if (!sineWave) {
			swingAngle = Gxyz [1];
		} else {
			swingAngle = Mathf.Sin(Time.time * 2) * 45;
		}
		
//		swingAngle=errorFilter.addValue (time, Gxyz [1], Gaccel [2]);
//		if (debug) {
//			if (errorFilter.debugMessage.Length != 0) {
//				print (errorFilter.debugMessage);
//			}
//		}

		if (applySwingTransform) {
			swingPivot.localEulerAngles = new Vector3 (swingAngle, 0, 0);
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
