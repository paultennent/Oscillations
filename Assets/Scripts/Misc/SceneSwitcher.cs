using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class SceneSwitcher : MonoBehaviour {

	private string[] scenes;
	private float lastTapTime = 0;

	public int sceneID = 0;


	private GameObject[] allObjects;

	public bool On = true;
	private bool allOn = true;

	private Material skyMat;
	private SkySwitcher switcher;
	private float savedTransition;
	private float savedTrackAngle;
	private float savedRadiusLoop;


	// Use this for initialization
	void Start () {
		scenes = new string[]{"Float","Pachinko","HighRoller","Jellyfish","Shuttlecock","Walker","Oscillate"};
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonUp("Tap"))
		{
			if ((Time.time - lastTapTime) < 1.0f) {
				On=!On;
			}
			lastTapTime = Time.time;
		}
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			SceneManager.LoadScene ("Menu");
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

	private void switchScene(){
		int newscene = sceneID + 1;
		if (newscene >= scenes.Length) {
			newscene = 0;
		}
		SceneManager.LoadScene (scenes [newscene]);
	}


	public void turnOff(){
		allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach(GameObject obj in allObjects){
			if(obj.activeInHierarchy){
				Renderer ren = obj.GetComponent<Renderer>();
				if(ren != null){
					ren.enabled = false;
				}
				Terrain ter = obj.GetComponent<Terrain> ();
				if (ter != null) {
					ter.enabled = false;
				}
				TerrainCollider terCol = obj.GetComponent<TerrainCollider> ();
				if (terCol != null) {
					terCol.enabled = false;
				}
				FishSpawner fish = obj.GetComponent<FishSpawner> ();
				if (fish != null) {
					fish.enabled = false;
				}
				JellySpawner jelly = obj.GetComponent<JellySpawner> ();
				if (jelly != null) {
					jelly.enabled = false;
				}
			}
		}
		skyMat = RenderSettings.skybox;
		RenderSettings.skybox = null;
		RenderSettings.ambientLight = Color.white;

		GameObject cam = GameObject.Find("Main Camera");
		EdgeDetection ed = cam.GetComponent<EdgeDetection> ();
		if (ed != null) {
			ed.enabled = false;
		}
		SepiaTone st = cam.GetComponent<SepiaTone> ();
		if (st != null) {
			st.enabled = false;
		}

		GameObject controller = GameObject.Find ("Controller");

		GameObject track = GameObject.Find ("TrackTest");
		if (track) {
			TrackGenerator gen=track.GetComponent<TrackGenerator> ();
			savedRadiusLoop=gen.startRampRadius;
			savedTransition=gen.lengthTransition;
			savedTrackAngle=gen.newTrackAngle;
			gen.startRampRadius = gen.radiusLoop;
			gen.lengthTransition = 0;
			gen.newTrackAngle = 0;
			controller.GetComponent<PachinkoCamMover> ().trackGen = gen;
			GameObject[] tracks = GameObject.FindGameObjectsWithTag ("Generated");
			foreach (GameObject o in tracks) {
				GameObject.Destroy (o);
			}


		}
		SkySwitcher switcher = controller.GetComponent<SkySwitcher> ();
		if (switcher != null) {
			switcher.enabled = false;
		}



		GameObject beams = new GameObject ("Beams");
		DrawBeams db = beams.AddComponent<DrawBeams> ();
		if (scenes [sceneID] == "Shuttlecock") {
			beams.transform.localScale = new Vector3 (10, 10, 10);
			db.lineWidth = 0.1f;
		} else {
			beams.transform.localScale = new Vector3 (100, 100, 100);
		}
		if (scenes [sceneID] == "Walker") {
			GameObject.Find ("Centre").transform.position = Vector3.zero;
		}
		db.DrawAllBeams();

	}

	public void turnOn(){
		if (switcher != null) {
			switcher.enabled = true;
		}
		GameObject controller = GameObject.Find ("Controller");
		GameObject track = GameObject.Find ("TrackTest");
		if (track) {
			TrackGenerator gen=track.GetComponent<TrackGenerator> ();
			gen.startRampRadius = savedRadiusLoop;
			gen.lengthTransition = savedTransition;
			gen.newTrackAngle = savedTrackAngle;
			controller.GetComponent<PachinkoCamMover> ().trackGen = gen;
			GameObject[] tracks = GameObject.FindGameObjectsWithTag ("Generated");
			foreach (GameObject o in tracks) {
				GameObject.Destroy (o);
			}
		}

		allObjects = UnityEngine.Object.FindObjectsOfType<GameObject> ();
		if (allObjects != null) {
			foreach (GameObject obj in allObjects) {
				if (obj.activeInHierarchy) {
					Renderer ren = obj.GetComponent<Renderer> ();
					if (ren != null) {
						ren.enabled = true;
					}
				}
				Terrain ter = obj.GetComponent<Terrain> ();
				if (ter != null) {
					ter.enabled = true;
				}
				TerrainCollider terCol = obj.GetComponent<TerrainCollider> ();
				if (terCol != null) {
					terCol.enabled = true;
				}
				FishSpawner fish = obj.GetComponent<FishSpawner> ();
				if (fish != null) {
					fish.enabled = true;
				}
				JellySpawner jelly = obj.GetComponent<JellySpawner> ();
				if (jelly != null) {
					jelly.enabled = true;
				}

			}
			RenderSettings.skybox = skyMat;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

			GameObject cam = GameObject.Find ("Main Camera");
			EdgeDetection ed = cam.GetComponent<EdgeDetection> ();
			if (ed != null) {
				ed.enabled = true;
			}
			SepiaTone st = cam.GetComponent<SepiaTone> ();
			if (st != null) {
				st.enabled = true;
			}
			GameObject.Find ("Beams").GetComponent<DrawBeams> ().DestroyChildren (GameObject.Find ("Beams").GetComponent<DrawBeams> ().lineparent);
			Destroy (GameObject.Find ("Beams"), 0);
			if (scenes [sceneID] == "Walker") {
				GameObject.Find ("Centre").transform.position = Vector3.zero;
			}
		}
			
	}
}
