using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbstractGameEffects : MonoBehaviour {

	protected AbstractDataReader swingData;
	protected SessionManager sessionManager;
	protected SwingBase swingBase;
	protected Transform swingPivot;
	protected Transform swingSeat;
	protected Transform viewPoint;

	protected float swingAngle;
	protected float sessionTime;
	protected bool inSession;

	public float sessionLength = 60f;
	public float climaxTime = 30.0f;
	public bool dontcycle = false;
	public float climaxRatio = 0f;
	protected float offsetTime;

	public int maxSessions = 1;

	protected float highAngle;
	protected float lowAngle;

	protected bool supressEffects;

	//public bool faded;
	//public bool fading = false;
	//public Canvas killer;


	// Use this for initialization
	public void Start () {
		swingBase = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SwingBase> ();
		swingData = GameObject.FindGameObjectWithTag ("Controller").GetComponent<AbstractDataReader> ();
		sessionManager = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SessionManager> ();
		swingSeat = GameObject.FindGameObjectWithTag ("Swing").transform;
		swingPivot = GameObject.FindGameObjectWithTag ("SwingPivot").transform;
		viewPoint = GameObject.FindGameObjectWithTag ("ViewPoint").transform;

	}
	
	// Update is called once per frame
	public void Update () {
		swingAngle = swingBase.getSwingAngle ();
		sessionTime = sessionManager.getSessionTime ();
		inSession = sessionManager.isInSession ();

		if (supressEffects && !inSession) {
			supressEffects = false;
		}
		//if(faded & !fading & !inSession){
		//	faded = false;
		//	killer.enabled = false;
		//}


		if (inSession) {
			int gameNumber = (int)(sessionTime / climaxTime);

			if (gameNumber > maxSessions && maxSessions > 0) {
				supressEffects = true;
			}

			offsetTime = sessionTime - (gameNumber * climaxTime);
			if ((gameNumber & 1) == 1) {
				offsetTime = climaxTime - offsetTime;
			}

			if (!dontcycle) {
				climaxRatio = offsetTime / climaxTime;
			} else {
				if (sessionTime <= climaxTime) {
					climaxRatio = offsetTime / climaxTime;
				} else {
					climaxRatio = 1f;
				}
			}
				
			if (swingAngle > highAngle) {
				highAngle = swingAngle;
			}
			if (swingAngle < lowAngle) {
				lowAngle = swingAngle;
			}
			if (swingAngle < 0) {
				highAngle = 0;
			}
			if (swingAngle > 0) {
				lowAngle = 0;
			}
				
		} else {
			if (supressEffects) {
				supressEffects = false;
			}
		}

		if (inSession && supressEffects) {
			inSession = false;
			//if(!faded){
			//	StartCoroutine(fader());
			//}
		}

	}

//	private IEnumerator fader(){
//		fading = true;
//		yield return new WaitForSeconds(2f);
//		killer.enabled = true;
//		fading = false;
//		faded = true;
//	}


}
