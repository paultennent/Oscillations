using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SwitcherOffer : MonoBehaviour {

	private GameObject[] allObjects;

	public bool On = true;
	private bool allOn = true;

	private Material skyMat;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonUp ("Tap")) {
			On = !On;
		}
		if(!On && allOn){
			turnOff();
			allOn = false;
		}

		if(On && !allOn){
			turnOn();
			allOn = true;
		}
	}

	public void turnOff(){
		allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach(GameObject obj in allObjects){
			if(obj.activeInHierarchy){
				Renderer ren = obj.GetComponent<Renderer>();
				if(ren != null){
					ren.enabled = false;
				}
			}
		}
		skyMat = RenderSettings.skybox;
		RenderSettings.skybox = null;

		GameObject cam = GameObject.Find("Main Camera");
		cam.GetComponent<EdgeDetection>().enabled = false;
		cam.GetComponent<SepiaTone>().enabled = false;

		GameObject track = GameObject.Find ("TrackTest");
		if (track) {
			TrackGenerator gen=track.GetComponent<TrackGenerator> ();
			gen.startRampRadius = gen.radiusLoop;
			gen.lengthTransition = 0;
			gen.newTrackAngle = 0;
		}

		GameObject.Find ("Beams").GetComponent<DrawBeams> ().DrawAllBeams ();

	}

	public void turnOn(){
		allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
		if(allObjects != null){
			foreach(GameObject obj in allObjects){
			if(obj.activeInHierarchy){
				Renderer ren = obj.GetComponent<Renderer>();
				if(ren != null){
					ren.enabled = true;
				}
			}
		}
		RenderSettings.skybox = skyMat;
		GameObject cam = GameObject.Find("Main Camera");
		cam.GetComponent<EdgeDetection>().enabled = true;
		cam.GetComponent<SepiaTone>().enabled = true;
			GameObject.Find ("Beams").GetComponent<DrawBeams> ().DestroyChildren (GameObject.Find ("Beams").GetComponent<DrawBeams> ().lineparent);
	}


}
}
