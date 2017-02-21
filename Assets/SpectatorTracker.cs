using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorTracker : MonoBehaviour {

    private GameObject target;
    private Vector3 myOffset;
	private Quaternion myRotOffset;

	public PachinkoCamMover mover;
	public GameObject trackgen;

	private bool moving = false;


	// Use this for initialization
	void Start () {
        target = GameObject.Find("TrackTest");
		myOffset = transform.localPosition;
		myRotOffset = transform.localRotation;
	}

	// Update is called once per frame
	void Update () {
		if (mover.trackGen.gameObject != trackgen) {
			updateTarget (mover.trackGen.gameObject);
			trackgen = mover.trackGen.gameObject;
		}

		if (moving) {
			transform.localPosition = Vector3.Lerp (transform.localPosition, myOffset, 1f * Time.deltaTime);
			transform.localRotation = Quaternion.Lerp (transform.localRotation, myRotOffset, 1f * Time.deltaTime);
		}

	}

    public void updateTarget(GameObject newtarget)
    {
        target = newtarget;
		transform.parent = newtarget.transform;
		StartCoroutine (move ());
		//transform.localPosition = myOffset;
		//transform.localEulerAngles = myRotOffset;
    }

	public IEnumerator move(){
		moving = true;
		yield return new WaitForSeconds (5);
		transform.parent = null;
		moving = false;
	}
}
