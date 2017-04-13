using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class JellyfishAudioController : MonoBehaviour {

	public SwingBase swingBase;
	public JellyfishTileCamMover camMover;
	public Transform cam;

	public AudioMixerGroup[] MixMixers;
	public AudioMixerGroup[] SwingMixers;
	public AudioMixerGroup[] VoidMixers;

	public AudioClip[] MixClips;
	public AudioClip[] SwingClips;
	public AudioClip[] VoidClips;

	private float[] MixStartLevels;
	private float[] SwingStartLevels;
	private float[] VoidStartLevels;

	private List<AudioSource> MixSources;
	private List<AudioSource> SwingSources;
	private List<AudioSource> VoidSources;

	private float maxAngle = 45f;
	private float dbsilence = -80f;

	private float raycastMaxDistance = 100f;

	// Use this for initialization
	void Start () {
		MixSources = new List<AudioSource>();
		SwingSources = new List<AudioSource>();
		VoidSources = new List<AudioSource>();

		setupAudioSources(MixClips,MixMixers, MixSources, false);
		setupAudioSources(SwingClips,SwingMixers, SwingSources, true);
		setupAudioSources(VoidClips,VoidMixers, VoidSources, false);

		MixStartLevels = captureStartVals(MixMixers);
		SwingStartLevels = captureStartVals(SwingMixers);
		VoidStartLevels = captureStartVals(VoidMixers);

		zeroMixers(MixMixers, dbsilence);
		zeroMixers(SwingMixers, dbsilence);
		zeroMixers(VoidMixers, dbsilence);

		startSources(MixSources);
		startSources(SwingSources);
		startSources(VoidSources);
	}
	
	// Update is called once per frame
	void Update () {
		updateSwingSounds (camMover.getSwingQuadrant (),SwingMixers,SwingStartLevels,10f);
		LayerLayout.LayoutPos curTilePos = LayerLayout.GetLayerLayout ().GetBlockAt (cam.transform.position.y);
		updateMixMixers (MixMixers,MixStartLevels,curTilePos,1f);
		updateVoidMixers (VoidMixers,VoidStartLevels,5f);
	}

	private void startSources(List<AudioSource> mysources){
		foreach(AudioSource source in mysources){
			source.Play();
		}
	}

	private float Remap(float val, float OldMin, float OldMax, float NewMin, float NewMax)
	{
		return (((val - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
	}

	private void zeroMixers(AudioMixerGroup[] mixers, float value)
	{
		for (int i = 0; i < mixers.Length; i++)
		{
			mixers[i].audioMixer.SetFloat(mixers[i].name, value);
		}
	}

	private void setupAudioSources(AudioClip[] clips, AudioMixerGroup[] mixers, List<AudioSource> sources, bool alternatePans)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.volume = 1.0f;
			source.dopplerLevel = 0f;
			source.outputAudioMixerGroup = mixers[i];
			source.clip = clips[i];
			if (alternatePans) {
				if (i % 2 == 0) {
					source.panStereo = -1f;
				} else {
					source.panStereo = 1f;
				}
			}
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

	private void updateSwingSounds(int swingQuadrant, AudioMixerGroup[] mixers, float[] startVals, float mixRate)
	{
		//this one is a bit odd as it depends on where we do the impulse - assuming forward motion for now.
		//will have to change the quadrant values if we switch back to backwards for the impelling
		float[] current = new float[2];
		mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
		mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);
		float[] targets = new float[2];

		//now forward (this is where we change the qaadrant vals)
		if (swingQuadrant == 0 || swingQuadrant == 3)
		{
			targets[0] = startVals[0];
			targets[1] = dbsilence;
		}else{
			targets[0] = dbsilence;
			targets[1] = startVals[1];
		}

		//if (!camMover.isMoving())
		//{
		//	targets[0] = dbsilence;
		//	targets[1] = dbsilence;
		//}

		mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0], targets[0], mixRate * Time.deltaTime));
		mixers[1].audioMixer.SetFloat(mixers[1].name, Mathf.Lerp(current[1], targets[1], mixRate * Time.deltaTime));
	}
		
//		START,
//		CAVE_RPT,
//		CAVE_TOP,
//		MID_BASE,
//		MID_RPT,
//		MID_TOP,
//		CORAL,
//		END,
//		FINISHED,
	private void updateMixMixers(AudioMixerGroup[] mixers, float[] startVals, LayerLayout.LayoutPos pos, float mixRate)
		{
			//lerps the mixers values to the next block states

			float[] current = new float[3];
			mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
			mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);
			mixers[2].audioMixer.GetFloat(mixers[2].name, out current[2]);

			float[] targets = new float[4];

		if (pos == LayerLayout.LayoutPos.START || pos == LayerLayout.LayoutPos.CAVE_RPT || pos == LayerLayout.LayoutPos.CAVE_TOP)
			{
				targets = new float[] { startVals[0], dbsilence, dbsilence};
			}
		else if (pos == LayerLayout.LayoutPos.MID_BASE || pos == LayerLayout.LayoutPos.MID_RPT || pos == LayerLayout.LayoutPos.MID_TOP)
			{
			targets = new float[] { dbsilence, startVals[1], dbsilence };
			}
		else if (pos == LayerLayout.LayoutPos.CORAL || pos == LayerLayout.LayoutPos.END)
			{
			targets = new float[] { dbsilence, dbsilence, startVals[2]};
			}
		else if (pos == LayerLayout.LayoutPos.FINISHED)
		{
			targets = new float[] { dbsilence, dbsilence, dbsilence};
		}


			mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0],targets[0],mixRate * Time.deltaTime));
			mixers[1].audioMixer.SetFloat(mixers[1].name, Mathf.Lerp(current[1], targets[1], mixRate * Time.deltaTime));
			mixers[2].audioMixer.SetFloat(mixers[2].name, Mathf.Lerp(current[2], targets[2], mixRate * Time.deltaTime));
		}

	private void updateVoidMixers(AudioMixerGroup[] mixers, float[] startVals, float mixRate){
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
		}
		float[] targets = new float[mixers.Length];

		RaycastHit hit;
		float dist = 0;

		if (Physics.Raycast (cam.position, Vector3.left, out hit)) {
			dist += Vector3.Distance (hit.collider.transform.position, cam.position);
		} else {
			dist += raycastMaxDistance;
		}
		if (Physics.Raycast (cam.position, Vector3.right, out hit)) {
			dist += Vector3.Distance (hit.collider.transform.position, cam.position);
		} else {
			dist += raycastMaxDistance;
		}
		if (Physics.Raycast(cam.position, Vector3.forward, out hit))
		{
			dist += Vector3.Distance (hit.collider.transform.position, cam.position);
		}else {
			dist += raycastMaxDistance;
		}
		if (Physics.Raycast(cam.position, Vector3.back, out hit))
		{
			dist += Vector3.Distance (hit.collider.transform.position, cam.position);
		}else {
			dist += raycastMaxDistance;
		}


		for (int i = 0; i < targets.Length; i++) {
			targets[i] = Remap (dist, 0f, raycastMaxDistance*4f, dbsilence, startVals [i]);
			targets [i] = Mathf.Clamp (targets [i], dbsilence, startVals [i]);
			mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], targets[i], mixRate * Time.deltaTime));
		}

	}


}
