using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour {

	private string[] scenes;
	private float lastTapTime = 0;

	public int sceneID = 0;

	// Use this for initialization
	void Start () {
		scenes = new string[]{"Float","Pachinko","HighRoller","Jellyfish","Shuttlecock","Walker"};
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonUp("Tap"))
		{
			if ((Time.time - lastTapTime) < 1.0f) {
				switchScene ();
			}
			lastTapTime = Time.time;
		}
	}

	private void switchScene(){
		int newscene = sceneID + 1;
		if (newscene >= scenes.Length) {
			newscene = 0;
		}
		SceneManager.LoadScene (scenes [newscene]);
	}
}
