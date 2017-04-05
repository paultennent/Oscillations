using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VR;

public class ShuttlecockAudioScript : MonoBehaviour {

	public SwingBase swingBase;
	public ShuttlecockCityCamMover scccm;
	public Transform cam;

	public AudioMixerGroup[] swingSoundsMixer;
	public AudioMixerGroup[] jumpSoundsMixer;
	public AudioMixerGroup[] directionalMixer;
	public AudioMixerGroup[] mixMixer;

	public float[] swingSoundsMixerStartVals;
	public float[] jumpSoundsMixerStartVals;
	public float[] directionalMixerStartVals;
	public float[] mixMixerStartVals;

	public AudioClip[] swingSounds;
	public AudioClip[] jumpSounds;
	public AudioClip[] directional;
	public AudioClip[] mix;

	private float maxAngle = 45f;
	private float dbsilence = -80f;
	private float minValueForDirectionalSounds = -20f;
	private int curJumpSound = 0;

	private List<AudioSource> swingSources;
	private List<AudioSource> jumpSources;
	private List<AudioSource> directionalSources;
	private List<AudioSource> mixSources;

	// Use this for initialization
	void Start () {

		swingBase.zeroCrossingEvent.AddListener(OnZeroCross);

		swingSources = new List<AudioSource>();
		jumpSources = new List<AudioSource>();
		directionalSources = new List<AudioSource>();
		mixSources = new List<AudioSource>();

		setupAudioSources(swingSounds,swingSoundsMixer, swingSources);
		setupAudioSources(jumpSounds,jumpSoundsMixer, jumpSources);
		setupAudioSources(directional,directionalMixer, directionalSources);
		setupAudioSources(mix,mixMixer, mixSources);

		swingSoundsMixerStartVals = captureStartVals(swingSoundsMixer);
		jumpSoundsMixerStartVals = captureStartVals(jumpSoundsMixer);
		directionalMixerStartVals = captureStartVals(directionalMixer);
		mixMixerStartVals = captureStartVals(mixMixer);

		zeroMixers(swingSoundsMixer, dbsilence);
		zeroMixers(jumpSoundsMixer, dbsilence);
		zeroMixers(directionalMixer, minValueForDirectionalSounds);
		zeroMixers(mixMixer, dbsilence);

		startSources(swingSources);
		startSources(jumpSources);
		startSources(directionalSources);
		startSources(mixSources);

	}

	private void OnZeroCross()
	{
		curJumpSound = getNextJumpSound();
	}
	
	// Update is called once per frame
	void Update () {
		float swingAngle = swingBase.getSwingAngle ();
		//clamp the bugger so we don't have issues
		swingAngle = Mathf.Clamp(swingAngle, -maxAngle, maxAngle);
		updateSwingSounds(swingAngle, mixMixer, mixMixerStartVals, true);
		updateSwingSounds(swingAngle, swingSoundsMixer, swingSoundsMixerStartVals, false);
		updateJumpSoundMixer(swingAngle, jumpSoundsMixer, jumpSoundsMixerStartVals, curJumpSound);
#if UNITY_EDITOR
		updateDirectionalSounds(cam.localEulerAngles.y, directionalMixer, directionalMixerStartVals, minValueForDirectionalSounds);
#else
		updateDirectionalSounds(InputTracking.GetLocalRotation(VRNode.Head).eulerAngles.y, directionalMixer, directionalMixerStartVals, minValueForDirectionalSounds);    
#endif    
	}
		

	private int getNextJumpSound(){
		int myVal = 0;
		if (scccm.isInTraining ()) {
			myVal = Random.Range(0,2);
		} else {
			myVal = Random.Range (2, 5);
		}
		return myVal;
	}

	private void setupAudioSources(AudioClip[] clips, AudioMixerGroup[] mixers, List<AudioSource> sources)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.volume = 1.0f;
			source.outputAudioMixerGroup = mixers[i];
			source.clip = clips[i];
			sources.Add(source);
		}
	}

	private float[] captureStartVals(AudioMixerGroup[] mixers)
	{
		float[] startVals = new float[mixers.Length];
		for (int i = 0; i < mixers.Length; i++)
		{
			 mixers[i].audioMixer.GetFloat(mixers[i].name, out startVals[i]);
			//print("StartVals:" + mixers[i].name + ":" + startVals[i]);
		}
		return startVals;
	}

	private void zeroMixers(AudioMixerGroup[] mixers, float value)
	{
		for (int i = 0; i < mixers.Length; i++)
		{
			mixers[i].audioMixer.SetFloat(mixers[i].name, value);
		}
	}

	private void updateSwingSounds(float swingAngle, AudioMixerGroup[] mixers, float[] startVals, bool byPassMixFoirMain)
	{
		//assumes we have three sounds one for the centre, on for forward one for back
		float val = 0;
		float fval = 0;
		float bval = 0;

		//handle the centreChanel (louder as we approach zero) // this should be the 0th element
		val = Remap(Mathf.Abs(swingAngle), 0, maxAngle, startVals[0], dbsilence);
		if (byPassMixFoirMain)
		{
			val = startVals[0];
		}
		mixers[0].audioMixer.SetFloat(mixers[0].name, val);

		//now forward
		if (swingAngle > 0)
		{
			fval = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[1]);
			mixers[1].audioMixer.SetFloat(mixers[1].name, fval);
			mixers[2].audioMixer.SetFloat(mixers[2].name, dbsilence);
		}

		if (swingAngle < 0)
		{
			bval = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[2]);
			mixers[2].audioMixer.SetFloat(mixers[2].name, bval);
			mixers[1].audioMixer.SetFloat(mixers[1].name, dbsilence);
		}

		//print(string.Format("Angle: {0} , CenterMix: {1}, ForawrdMix: {2}, BackMix: {3}", swingAngle, val, fval, bval));

	}

	private void updateJumpSoundMixer(float swingAngle, AudioMixerGroup[] mixers, float[] startVals, int chosenJump)
	{
		float val = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[chosenJump]);
		if (scccm.isInOuttro() || scccm.isInIntro())
		{
			val = dbsilence;
		}
		mixers[1].audioMixer.SetFloat(mixers[chosenJump].name, val);
	}

	private void updateDirectionalSounds(float camAngle, AudioMixerGroup[] mixers, float[] startVals, float minValue)
	{
		if (camAngle >= 180)
		{
			float val = Remap(camAngle, 270, 360, minValue, startVals[0]);
			val = Mathf.Clamp(val, minValue, startVals[0]);
			mixers[0].audioMixer.SetFloat(mixers[0].name, val);


			val = Remap(camAngle, 270, 180, minValue, startVals[1]);
			val = Mathf.Clamp(val, minValue, startVals[1]);
			mixers[1].audioMixer.SetFloat(mixers[1].name, val);

			if (camAngle > 270)
			{
				val = Remap(camAngle, 360, 270, minValue, startVals[2]);
				val = Mathf.Clamp(val, minValue, startVals[2]);
				mixers[2].audioMixer.SetFloat(mixers[2].name, val);
			}
			else
			{
				val = Remap(camAngle, 180, 270, minValue, startVals[2]);
				val = Mathf.Clamp(val, minValue, startVals[2]);
				mixers[2].audioMixer.SetFloat(mixers[2].name, val);
			}
		}
		else{
			float val = Remap(camAngle, 90, 0, minValue, startVals[0]);
			val = Mathf.Clamp(val, minValue, startVals[0]);
			mixers[0].audioMixer.SetFloat(mixers[0].name, val);

			val = Remap(camAngle, 90, 180, minValue, startVals[1]);
			val = Mathf.Clamp(val, minValue, startVals[1]);
			mixers[1].audioMixer.SetFloat(mixers[1].name, val);

			if (camAngle < 90)
			{
				val = Remap(camAngle, 0, 90, minValue, startVals[3]);
				val = Mathf.Clamp(val, minValue, startVals[3]);
				mixers[3].audioMixer.SetFloat(mixers[3].name, val);
			}
			else
			{
				val = Remap(camAngle, 180, 90, minValue, startVals[3]);
				val = Mathf.Clamp(val, minValue, startVals[3]);
				mixers[3].audioMixer.SetFloat(mixers[3].name, val);
			}
		}

	}

	private float Remap(float val, float OldMin, float OldMax, float NewMin, float NewMax)
	{
		return (((val - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
	}

	private void startSources(List<AudioSource> mysources){
		foreach(AudioSource source in mysources){
			source.Play();
		}
	}
}
