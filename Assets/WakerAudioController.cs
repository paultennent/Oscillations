using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class WakerAudioController : MonoBehaviour {

	public SwingBase swingBase;
	public WalkerCityCamMover scccm;
	public Transform cam;

	public AudioMixerGroup[] swingSoundsMixer;
	public AudioMixerGroup[] turnSoundsMixer;
	public AudioMixerGroup[] waterMixer;
	public AudioMixerGroup[] mixMixer;
	public AudioMixerGroup[] growthMixer;

	private float[] swingSoundsMixerStartVals;
	private float[] turnSoundsMixerStartVals;
	private float[] waterMixerStartVals;
	private float[] mixMixerStartVals;
	private float[] growthMixerStartVals;

	public AudioClip[] swingSounds;
	public AudioClip[] turnSounds;
	public AudioClip[] waterSounds;
	public AudioClip[] mix;
	public AudioClip[] growthSounds;


	private float maxAngle = 45f;
	private float dbsilence = -80f;
	private int curJumpSound = 0;

	private List<AudioSource> swingSources;
	private List<AudioSource> turnSources;
	private List<AudioSource> waterSources;
	private List<AudioSource> mixSources;
	private List<AudioSource> growthSources;

	private bool turning = false;
	private bool growing = false;
	private bool shrinking = false;

	// Use this for initialization
	void Start () {

		swingBase.zeroCrossingEvent.AddListener(OnZeroCross);

		swingSources = new List<AudioSource>();
		turnSources = new List<AudioSource>();
		waterSources = new List<AudioSource>();
		mixSources = new List<AudioSource>();
		growthSources = new List<AudioSource>();

		setupAudioSources(swingSounds,swingSoundsMixer, swingSources, true);
		setupAudioSources(turnSounds,turnSoundsMixer, turnSources, false);
		setupAudioSources(waterSounds,waterMixer, waterSources, false);
		setupAudioSources(mix,mixMixer, mixSources, false);
		setupAudioSources(growthSounds,growthMixer, growthSources, false);

		swingSoundsMixerStartVals = captureStartVals(swingSoundsMixer);
		turnSoundsMixerStartVals = captureStartVals(turnSoundsMixer);
		waterMixerStartVals = captureStartVals(waterMixer);
		mixMixerStartVals = captureStartVals(mixMixer);
		growthMixerStartVals = captureStartVals(growthMixer);

		zeroMixers(swingSoundsMixer, dbsilence);
		zeroMixers(turnSoundsMixer, dbsilence);
		zeroMixers(waterMixer, dbsilence);
		zeroMixers(mixMixer, dbsilence);
		zeroMixers(growthMixer, dbsilence);

		startSources(swingSources);
		startSources(turnSources);
		startSources(waterSources);
		startSources(mixSources);
		startSources(growthSources);

	}

	private void OnZeroCross()
	{
		curJumpSound = getNextJumpSound();
	}

	private int getNextJumpSound(){
		int myVal = 0;
		myVal = Random.Range(0,turnSources.Count);
		return myVal;
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
            source.loop = true;
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

	private void zeroMixers(AudioMixerGroup[] mixers, float value)
	{
		for (int i = 0; i < mixers.Length; i++)
		{
			mixers[i].audioMixer.SetFloat(mixers[i].name, value);
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
	
	// Update is called once per frame
	void Update () {
		float swingAngle = swingBase.getSwingAngle ();
		//clamp the bugger so we don't have issues
		swingAngle = Mathf.Clamp (scccm.getSwingQuadrant(), -maxAngle, maxAngle);
		updateMixSounds (mixMixer, mixMixerStartVals, 5f);

        turning = scccm.isTurning();

		if(!turning){
			updateSwingSounds (scccm.getSwingQuadrant(), swingSoundsMixer, swingSoundsMixerStartVals, 10f);
		}
		updateTurnSoundMixer (swingAngle, turnSoundsMixer, turnSoundsMixerStartVals, curJumpSound, 5f);
		

        updateWaterSounds(waterMixer, waterMixerStartVals, 5f);
	}

    private void updateWaterSounds(AudioMixerGroup[] mixers, float[] startVals, float mixRate)
    {
        float[] current = new float[1];
        mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);

        float[] targets = new float[1];

        if (scccm.cam.position.y < 0f)
        {
            targets[0] = startVals[0];
        }
        else
        {
            targets[0] = dbsilence;
        }

        mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0], targets[0], mixRate * Time.deltaTime));
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

	private void updateMixSounds(AudioMixerGroup[] mixers, float[] startVals, float mixRate){
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
		}
		float[] targets = new float[mixers.Length];

		for (int i = 0; i < mixers.Length; i++) {
			mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], targets[i], mixRate * Time.deltaTime));
		}
	}

    private void updateTurnSoundMixer(float swingAngle, AudioMixerGroup[] mixers, float[] startVals, int chosenJump, float mixRate)
    {

        float[] current = new float[mixers.Length];
        for (int i = 0; i < mixers.Length; i++)
        {
            mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
        }

        if (turning)
        {

            float val = Remap(Mathf.Abs(swingAngle), -maxAngle, maxAngle, dbsilence, startVals[chosenJump]);

            for (int i = 0; i < mixers.Length; i++)
            {
                if (i == chosenJump)
                {
                    mixers[i].audioMixer.SetFloat(mixers[i].name, Mathf.Lerp(current[i], val, mixRate * Time.deltaTime));
                }
                else
                {
                    mixers[i].audioMixer.SetFloat(mixers[i].name, Mathf.Lerp(current[i], dbsilence, mixRate * Time.deltaTime));
                }
            }
        }
        else
        {
            for (int i = 0; i < mixers.Length; i++)
            {
                mixers[i].audioMixer.SetFloat(mixers[i].name, Mathf.Lerp(current[i], dbsilence, mixRate * Time.deltaTime));
            }
        }
    }

	private void updateGrowthSounds(AudioMixerGroup[] mixers, float[] startVals, float mixRate){
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
		}
		float[] targets = new float[mixers.Length];
		if (growing) {
			targets [0] = startVals [0];
		} else {
			targets [0] = dbsilence;
		}

		if (shrinking) {
			targets [1] = startVals [1];
		} else {
			targets [1] = dbsilence;
		}

		for (int i = 0; i < mixers.Length; i++) {
			mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], targets[i], mixRate * Time.deltaTime));
		}
	}
}
