using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public float climaxRatio = 0f;
	protected float offsetTime;

	protected float highAngle;
	protected float lowAngle;


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

		if (inSession) {
			int gameNumber=(int)(sessionTime/climaxTime);
			offsetTime=sessionTime-(gameNumber*climaxTime);
			if((gameNumber&1)==1)
			{
				offsetTime=climaxTime-offsetTime;
			}
			climaxRatio=offsetTime/climaxTime;

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

		}
	}


}
