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
	public AudioMixerGroup[] OtherEnvMixers;

	public AudioClip[] MixClips;
	public AudioClip[] SwingClips;
	public AudioClip[] BuildingsClips;
	public AudioClip[] OtherEnvClips;

	private float[] MixStartLevels;
	private float[] SwingStartLevels;
	private float[] BuildingsStartLevels;
	private float[] OtherEnvStartLevels;

	private List<AudioSource> MixSources;
	private List<AudioSource> SwingSources;
	private List<AudioSource> BuildingSources;
	private List<AudioSource> OtherEnvSources;

	private float maxAngle = 45f;
	private float dbsilence = -80f;
	private int curJumpSound = 0;

	private float leftDist;
	private float rightDist;

	private float maxBuildingDistanceForPan = 25f;


	// Use this for initialization
	void Start () {
		MixSources = new List<AudioSource>();
		SwingSources = new List<AudioSource>();
		BuildingSources = new List<AudioSource>();
		OtherEnvSources = new List<AudioSource>();

		setupAudioSources(MixClips,MixMixers, MixSources);
		setupAudioSources(SwingClips,SwingMixers, SwingSources);
		setupAudioSources(BuildingsClips,BuildingsMixers, BuildingSources);
		setupAudioSources(OtherEnvClips,OtherEnvMixers, OtherEnvSources);

		MixStartLevels = captureStartVals(MixMixers);
		SwingStartLevels = captureStartVals(SwingMixers);
		BuildingsStartLevels = captureStartVals(BuildingsMixers);
		OtherEnvStartLevels = captureStartVals(OtherEnvMixers);

		zeroMixers(MixMixers, dbsilence);
		zeroMixers(SwingMixers, dbsilence);
		zeroMixers(BuildingsMixers, dbsilence);
		zeroMixers(OtherEnvMixers, dbsilence);

		startSources(MixSources);
		startSources(SwingSources);
		startSources(BuildingSources);
		startSources(OtherEnvSources);
	}
	
	// Update is called once per frame
	void Update () {
		float swingAngle = swingBase.getSwingAngle();
		int swingQuadrant = camMover.getSwingQuadrant();
		BlockLayout.LayoutPos curTilePos = BlockLayout.GetBlockLayout ().GetBlockAt (cam.transform.position.z);

		doRaycasting();

		updateMixMixers(MixMixers, MixStartLevels, curTilePos, 5f);
		updateSwingSounds(swingQuadrant, SwingMixers, SwingStartLevels, 5f, 0.1f);
		updateBuildingMixers(BuildingsMixers, BuildingsStartLevels, curTilePos, BuildingSources, 1f);
        updateOtherEnvMixers(OtherEnvMixers, OtherEnvStartLevels, curTilePos, 5f);


	}

	private void doRaycasting()
	{
		RaycastHit hit;
		if (Physics.Raycast(cam.position, Vector3.left, out hit))
		{
			leftDist = hit.distance;
		}
		else { leftDist = -1; }
		if (Physics.Raycast(cam.position, Vector3.right, out hit))
		{
			rightDist = hit.distance;
		}
		else { rightDist = -1; }
	}

	private void setupAudioSources(AudioClip[] clips, AudioMixerGroup[] mixers, List<AudioSource> sources)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.volume = 1.0f;
			source.dopplerLevel = 0f;
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

	private void updateSwingSounds(int swingQuadrant, AudioMixerGroup[] mixers, float[] startVals, float mixRate, float mixRateSlow)
	{
		//this one is a bit odd as it depends on where we do the impulse - assuming forward motion for now.
		//will have to change the quadrant values if we switch back to backwards for the impelling
		float[] current = new float[3];
		mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
		mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);

		float[] targets = new float[2];

		//now forward (this is where we change the qaadrant vals)
		if (swingQuadrant == 1 || swingQuadrant == 2)
		{
			targets[0] = startVals[0];
			targets[1] = startVals[1];
		}else{
			targets[0] = dbsilence;
			targets[1] = dbsilence;
		}

		if (!camMover.isMoving())
		{
			targets[0] = dbsilence;
			targets[1] = dbsilence;
		}

		mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0], targets[0], mixRate * Time.deltaTime));
		if (swingQuadrant == 0 || swingQuadrant == 3) {
			mixers [1].audioMixer.SetFloat (mixers [1].name, Mathf.Lerp (current [1], targets [1], mixRateSlow * Time.deltaTime));
		} else {
			mixers [1].audioMixer.SetFloat (mixers [1].name, Mathf.Lerp (current [1], targets [1], mixRate * Time.deltaTime));
		}


	}

	private void updateMixMixers(AudioMixerGroup[] mixers, float[] startVals, BlockLayout.LayoutPos pos, float mixRate)
	{
		//lerps the mixers values to the next block states

		float[] current = new float[4];
		mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
		mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);
		mixers[2].audioMixer.GetFloat(mixers[2].name, out current[2]);
		mixers[3].audioMixer.GetFloat(mixers[3].name, out current[3]);

		float[] targets = new float[4];

		if (pos == BlockLayout.LayoutPos.START || pos == BlockLayout.LayoutPos.END)
		{
			targets = new float[] { startVals[0], dbsilence, dbsilence, dbsilence };
		}
		if (pos == BlockLayout.LayoutPos.PARKSTART || pos == BlockLayout.LayoutPos.PARKREST || pos == BlockLayout.LayoutPos.PARKEND || pos == BlockLayout.LayoutPos.LOW1 || pos == BlockLayout.LayoutPos.LOW2)
		{
			targets = new float[] { startVals[0], startVals[1], dbsilence, dbsilence };
		}
		else if (pos == BlockLayout.LayoutPos.MID1 || pos == BlockLayout.LayoutPos.MID2)
		{
			targets = new float[] { startVals[0], startVals[1], startVals[2], dbsilence };
		}
		else if (pos == BlockLayout.LayoutPos.HIGH1 || pos == BlockLayout.LayoutPos.HIGH2)
		{
			targets = new float[] { startVals[0], startVals[1], startVals[2], startVals[3] };
		}
		else if (pos == BlockLayout.LayoutPos.FINISHED)
		{
			targets = new float[] { dbsilence, dbsilence, dbsilence, dbsilence };
		}

		mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0],targets[0],mixRate * Time.deltaTime));
		mixers[1].audioMixer.SetFloat(mixers[1].name, Mathf.Lerp(current[1], targets[1], mixRate * Time.deltaTime));
		mixers[2].audioMixer.SetFloat(mixers[2].name, Mathf.Lerp(current[2], targets[2], mixRate * Time.deltaTime));
		mixers[3].audioMixer.SetFloat(mixers[3].name, Mathf.Lerp(current[3], targets[3], mixRate* Time.deltaTime));
	}

	private void updateOtherEnvMixers(AudioMixerGroup[] mixers, float[] startVals, BlockLayout.LayoutPos pos, float mixRate)
	{
		float[] current = new float[2];
		mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
		mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);

		float[] targets = new float[2];

		if (pos == BlockLayout.LayoutPos.PARKSTART || pos == BlockLayout.LayoutPos.PARKREST || pos == BlockLayout.LayoutPos.PARKEND)
		{
			targets = new float[] { dbsilence, startVals[1] };
		}
		else if (pos == BlockLayout.LayoutPos.BEACH1)
		{
			targets = new float[] { startVals[0], dbsilence };
		}
		else
		{
			targets = new float[] { dbsilence, dbsilence };
		}

		mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0],targets[0],mixRate* Time.deltaTime));
		mixers[1].audioMixer.SetFloat(mixers[1].name, Mathf.Lerp(current[1], targets[1], mixRate* Time.deltaTime));
	}

	private void updateBuildingMixers(AudioMixerGroup[] mixers, float[] startVals, BlockLayout.LayoutPos pos, List<AudioSource> sources, float mixRate)
	{
		//lerps the mixers values to the next block states

		float[] current = new float[2];
		mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
		mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);

		float[] targets = new float[2];

		if (leftDist < 0 && rightDist < 0)
		{
			targets = new float[] { dbsilence, dbsilence };
		}
		else
		{
			if (pos == BlockLayout.LayoutPos.START || pos == BlockLayout.LayoutPos.LOW1 || pos == BlockLayout.LayoutPos.LOW2 || pos == BlockLayout.LayoutPos.END)
			{
				targets = new float[] { dbsilence, startVals[1] };
			}
			else if (pos == BlockLayout.LayoutPos.MID1 || pos == BlockLayout.LayoutPos.MID2)
			{
				targets = new float[] { startVals[0], startVals[1] };
			}
			else if (pos == BlockLayout.LayoutPos.HIGH1 || pos == BlockLayout.LayoutPos.HIGH2 || pos == BlockLayout.LayoutPos.HIGH3 || pos == BlockLayout.LayoutPos.BEACH1)
			{
				targets = new float[] { startVals[0], dbsilence };
			}
			else if (pos == BlockLayout.LayoutPos.FINISHED || pos == BlockLayout.LayoutPos.PARKSTART || pos == BlockLayout.LayoutPos.PARKREST || pos == BlockLayout.LayoutPos.PARKEND)
			{
				targets = new float[] { dbsilence, dbsilence, dbsilence };
			}
		}

		setPanning(sources,mixRate);

		mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0], targets[0], mixRate * Time.deltaTime));
		mixers[1].audioMixer.SetFloat(mixers[1].name, Mathf.Lerp(current[1], targets[1], mixRate * Time.deltaTime));
	}

	private void setPanning(List<AudioSource> sources, float mixRate)
	{
		float panVal = 0f;
		if (leftDist < 0 && rightDist < 0)
		{
			//nothing at all - reset pan to even
			panVal = 0f;
		}
		else if (leftDist >= 0 && rightDist < 0)
		{
			//nothing on the right - pan hard left
			panVal = -1.0f;
		}
		else if (leftDist < 0 && rightDist >= 0)
		{
			panVal = 1.0f;
		}
		else
		{
			//we've got a mix
			float scaledLeft = Remap(Mathf.Clamp(leftDist, 0, maxBuildingDistanceForPan), 0, maxBuildingDistanceForPan, 1f, 0f);
			float scaledright = Remap(Mathf.Clamp(rightDist, 0, maxBuildingDistanceForPan), 0, maxBuildingDistanceForPan, 1f, 0f);
			panVal = -scaledLeft + scaledright;
		}
		//now do the panning
		foreach (AudioSource s in sources)
		{
			s.panStereo = Mathf.Lerp(s.panStereo, panVal, mixRate);
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
