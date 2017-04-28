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
	private float minValueForDirectionalSounds = -40f;
	private int curJumpSound = 0;

	private List<AudioSource> swingSources;
	private List<AudioSource> jumpSources;
	private List<AudioSource> directionalSources;
	private List<AudioSource> mixSources;

	private float fadeInDuration = 5f;
    private bool fadingIn = false;
    private bool fadingOut = false;

    public AudioMixerGroup masterMixer;

	public bool started = false;

    // Use this for initialization
    void Start () {

        //masterMixer.audioMixer.SetFloat(masterMixer.name, dbsilence);

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

	public void begin(){
		if (!started) {
			startSources(swingSources);
			startSources(jumpSources);
			startSources(directionalSources);
			startSources(mixSources);
			started = true;
			masterMixer.audioMixer.SetFloat ("Master", 0f);
		}

	}

	private void OnZeroCross()
	{
		curJumpSound = getNextJumpSound();
	}
	
	// Update is called once per frame
	void Update () {

		if (FadeSphereScript.isFadingOut())
        {
            if (!fadingOut)
            {
                StartCoroutine(fadeOut());
            }
        }
        
        
			float swingAngle = swingBase.getSwingAngle ();
			//clamp the bugger so we don't have issues
			swingAngle = Mathf.Clamp (swingAngle, -maxAngle, maxAngle);
			updateSwingSounds (swingAngle, mixMixer, mixMixerStartVals, true, 5f);
			updateSwingSounds (swingAngle, swingSoundsMixer, swingSoundsMixerStartVals, false, 10f);
			updateJumpSoundMixer (swingAngle, jumpSoundsMixer, jumpSoundsMixerStartVals, curJumpSound, 5f);
#if UNITY_EDITOR
			updateDirectionalSounds (cam.localEulerAngles.y, directionalMixer, directionalMixerStartVals, minValueForDirectionalSounds, 5f);
#else
		updateDirectionalSounds(InputTracking.GetLocalRotation(VRNode.Head).eulerAngles.y, directionalMixer, directionalMixerStartVals, minValueForDirectionalSounds, 5f);    
#endif    
	
	}

	private void fadeLimitMixers(AudioMixerGroup[] mixers){
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
			float cap = Remap (scccm.getSessionTime (), 0, fadeInDuration, dbsilence, current [i]);
			if (current [i] > cap) {
				mixers [i].audioMixer.SetFloat (mixers [0].name, cap);
			}
		}
	}

    private IEnumerator fadeIn()
    {
        fadingIn = true;
        float cur = 0f;
        masterMixer.audioMixer.GetFloat(masterMixer.name, out cur);
        while (cur < -0.01f)
        {
            masterMixer.audioMixer.SetFloat(masterMixer.name, Mathf.Lerp(cur, 0f, (1/fadeInDuration) * Time.deltaTime));
            masterMixer.audioMixer.GetFloat(masterMixer.name, out cur);
            yield return null;
        }
        masterMixer.audioMixer.SetFloat(masterMixer.name, 0f);
        fadingIn = false;
        yield break;
    }

    private IEnumerator fadeOut()
    {
        fadingOut = true;
        float cur = 0f;
        masterMixer.audioMixer.GetFloat(masterMixer.name, out cur);
        while (cur > dbsilence+0.01f)
        {
            masterMixer.audioMixer.SetFloat(masterMixer.name, Mathf.Lerp(cur, dbsilence, (1 / fadeInDuration) * Time.deltaTime));
            masterMixer.audioMixer.GetFloat(masterMixer.name, out cur);
            yield return null;
        }
        masterMixer.audioMixer.SetFloat(masterMixer.name, dbsilence);
        fadingOut = false;
        yield break;
    }


    private int getNextJumpSound(){
		int myVal = 0;
		if (scccm.isInTraining ()) {
			myVal = Random.Range(0,1);
		} else {
			myVal = Random.Range (1, 5);
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

	private void updateSwingSounds(float swingAngle, AudioMixerGroup[] mixers, float[] startVals, bool byPassMixFoirMain, float mixRate)
	{
		//assumes we have three sounds one for the centre, on for forward one for back
		float val = 0;
		float fval = 0;
		float bval = 0;

		float[] current = new float[3];
		mixers[0].audioMixer.GetFloat(mixers[0].name, out current[0]);
		mixers[1].audioMixer.GetFloat(mixers[1].name, out current[1]);
		mixers[2].audioMixer.GetFloat(mixers[2].name, out current[2]);

		float[] targets = new float[3];

		//handle the centreChanel (louder as we approach zero) // this should be the 0th element
		val = Remap(Mathf.Abs(swingAngle), 0, maxAngle, startVals[0], dbsilence);
		if (byPassMixFoirMain)
		{
			val = startVals[0];
		}
		targets [0] = val;

		//now forward
		if (swingAngle > 0)
		{
			fval = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[1]);
			targets[1] = fval;
			targets[2] = dbsilence;
		}

		if (swingAngle < 0)
		{
			bval = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[2]);
			targets[1] = dbsilence;
			targets[2] = bval;
		}

		mixers[0].audioMixer.SetFloat(mixers[0].name, Mathf.Lerp(current[0],targets[0],mixRate * Time.deltaTime));
		mixers[1].audioMixer.SetFloat(mixers[1].name, Mathf.Lerp(current[1], targets[1], mixRate * Time.deltaTime));
		mixers[2].audioMixer.SetFloat(mixers[2].name, Mathf.Lerp(current[2], targets[2], mixRate * Time.deltaTime));
		//print(string.Format("Angle: {0} , CenterMix: {1}, ForawrdMix: {2}, BackMix: {3}", swingAngle, val, fval, bval));

	}

	private void updateJumpSoundMixer(float swingAngle, AudioMixerGroup[] mixers, float[] startVals, int chosenJump, float mixRate)
	{
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
		}
			
		float val = Remap(Mathf.Abs(swingAngle), 0, maxAngle, dbsilence, startVals[chosenJump]);
		if (scccm.isInOuttro() || scccm.isInIntro())
		{
			val = dbsilence;
		}
		for (int i = 0; i < mixers.Length; i++) {
			if (i == chosenJump) {
				mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], val, mixRate * Time.deltaTime));
			} else {
				mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], dbsilence, mixRate * Time.deltaTime));
			}
		}
	}

	private void updateDirectionalSounds(float camAngle, AudioMixerGroup[] mixers, float[] startVals, float minValue, float mixRate)
	{
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
		}
		float[] targets = new float[]{minValueForDirectionalSounds,minValueForDirectionalSounds,minValueForDirectionalSounds,minValueForDirectionalSounds};

		if (camAngle >= 180)
		{
			float val = Remap(camAngle, 270, 360, minValue, startVals[0]);
			val = Mathf.Clamp(val, minValue, startVals[0]);
			targets[0] = val;


			val = Remap(camAngle, 270, 180, minValue, startVals[1]);
			val = Mathf.Clamp(val, minValue, startVals[1]);
			targets [1] = val;

			if (camAngle > 270)
			{
				val = Remap(camAngle, 360, 270, minValue, startVals[2]);
				val = Mathf.Clamp(val, minValue, startVals[2]);
				targets [2] = val;
			}
			else
			{
				val = Remap(camAngle, 180, 270, minValue, startVals[2]);
				val = Mathf.Clamp(val, minValue, startVals[2]);
				targets [2] = val;
			}
		}
		else{
			float val = Remap(camAngle, 90, 0, minValue, startVals[0]);
			val = Mathf.Clamp(val, minValue, startVals[0]);
			targets [0] = val;

			val = Remap(camAngle, 90, 180, minValue, startVals[1]);
			val = Mathf.Clamp(val, minValue, startVals[1]);
			targets [1] = val;

			if (camAngle < 90)
			{
				val = Remap(camAngle, 0, 90, minValue, startVals[3]);
				val = Mathf.Clamp(val, minValue, startVals[3]);
				targets [3] = val;
			}
			else
			{
				val = Remap(camAngle, 180, 90, minValue, startVals[3]);
				val = Mathf.Clamp(val, minValue, startVals[3]);
				targets [3] = val;
			}
		}

		for (int i = 0; i < mixers.Length; i++) {
			mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], targets[i], mixRate * Time.deltaTime));
		}

	}

	private void fadeZeroMixers(AudioMixerGroup[] mixers,float mixRate){
		float[] current = new float[mixers.Length];
		for(int i=0;i<mixers.Length;i++){
			mixers[i].audioMixer.GetFloat(mixers[i].name, out current[i]);
		}
		for (int i = 0; i < mixers.Length; i++) {
			mixers [i].audioMixer.SetFloat (mixers [i].name, Mathf.Lerp (current[i], dbsilence, mixRate * Time.deltaTime));
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
