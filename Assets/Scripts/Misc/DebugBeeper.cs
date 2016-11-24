using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBeeper : MonoBehaviour {

	AudioSource audio;
	private float counter = 2f;
	public GameObject sphere;

	private long lastTime;

	// Use this for initialization
	void Start () {
		audio = GetComponent<AudioSource> ();
		lastTime = System.DateTime.Now.Ticks;
		sphere.GetComponent<Renderer> ().enabled = false;
	}

	void Update(){
		if(Input.GetButton("Tap"))
		{
			nativeBeep ();
		}

		long t = System.DateTime.Now.Ticks;
		if ((t - lastTime) > 10000000) {
			beep ();
		}
		lastTime = t;
	}

	void OnApplicationPause(bool paused){
		nativeBeep ();
		sphere.GetComponent<Renderer> ().enabled = true;
	}

	// Update is called once per frame
	public void beep(){
		if (!audio.isPlaying) {
			audio.Play ();
		}
	}

	public void nativeBeep(){
		AndroidJavaObject tg=new AndroidJavaObject("android.media.ToneGenerator",5,0x64);
		//        ToneGenerator tg = new ToneGenerator(AudioManager.STREAM_NOTIFICATION, ToneGenerator.MAX_VOLUME );
		tg.Call<bool>("startTone",41);
	}


}
