using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HighRollerAudioController : MonoBehaviour {

	public SwingBase swingBase;
	public HighRollerCamMover camMover;
	public Transform cam;

	public AudioMixerGroup[] MixMixers;
	public AudioMixerGroup[] SwingMixers;
	public AudioMixerGroup[] BuildingsMixers;
	public AudioMixerGroup[] WindowsMixers;

	public AudioClip[] MixClips;
	public AudioClip[] SwingClips;
	public AudioClip[] BuildingsClips;
	public AudioClip[] WindowsClips;

	private float[] MixStartLevels;
	private float[] SwingStartLevels;
	private float[] BuildingsStartLevels;
	private float[] WindowsStartLevels;

	private float maxAngle = 45f;
	private float dbsilence = -80f;
	private int curJumpSound = 0;
	private List<AudioSource> sources;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		BlockLayout.LayoutPos curTilePos = BlockLayout.GetBlockLayout ().GetBlockAt (cam.transform.position.z);
	}

	private void setupAudioSources(AudioClip[] clips, AudioMixerGroup[] mixers)
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

	private void updateSwingSounds(float swingAngle, AudioMixerGroup[] mixers, float[] startVals)
	{
		//assumes we have three sounds one for the centre, on for forward one for back
		float fval = 0;
		float bval = 0;

		//now forward
		if (swingAngle > 0)
		{
			fval = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[1]);
			mixers[0].audioMixer.SetFloat(mixers[0].name, fval);
			mixers[1].audioMixer.SetFloat(mixers[1].name, dbsilence);
		}

		if (swingAngle < 0)
		{
			bval = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[2]);
			mixers[1].audioMixer.SetFloat(mixers[1].name, bval);
			mixers[0].audioMixer.SetFloat(mixers[0].name, dbsilence);
		}

		//print(string.Format("Angle: {0} , CenterMix: {1}, ForawrdMix: {2}, BackMix: {3}", swingAngle, val, fval, bval));

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
