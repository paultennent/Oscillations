using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlecockAudioScript : MonoBehaviour {

	public SwingBase swingBase;
	public Transform pivot;

	public GameObject frontControllable;
	public GameObject backControllable;

	private AudioSource[] frontSources;
	private AudioSource[] backSources;

	private int curFrontJumpSound = 0;
	private int curBackJumpSound = 0;

	public ShuttlecockCityCamMover scccm;

	// Use this for initialization
	void Start () {
		frontSources = frontControllable.GetComponents<AudioSource> ();
		backSources = backControllable.GetComponents<AudioSource> ();
		swingBase.zeroCrossingEvent.AddListener (onZeroCrosing);
	}
	
	// Update is called once per frame
	void Update () {
		float swingAngle = swingBase.getSwingAngle ();
		pivot.localEulerAngles = new Vector3 (swingAngle, 90, 0);
	}

	private void onZeroCrosing(){

		print ("zero crossing");

		frontSources [curFrontJumpSound].volume = 0;
		backSources [curBackJumpSound].volume = 0;

		curFrontJumpSound = getNextJumpSound ();
		curBackJumpSound = getNextJumpSound ();

		frontSources [curFrontJumpSound].volume = 100;
		backSources [curBackJumpSound].volume = 100;
	}

	private int getNextJumpSound(){
		int myVal = 0;
		if (scccm.isInIntro ()) {
			myVal = Random.Range (0, 2);
		} else {
			myVal = Random.Range (2, 5);
		}
		//print ("new soundval:" + myVal);
		return myVal;
	}
}
