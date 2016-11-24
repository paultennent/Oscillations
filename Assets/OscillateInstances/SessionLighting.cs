using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionLighting : AbstractGameEffects {

	public Light[] spots;
	public Light[] lights;
	public bool lightsOn = false;
	public float targetIntensity = 3f;
	public float spotTargetIntensity = 50f;

	public float duration = 3f;

	public ReflectionProbe rp;

	bool debug = false;

	// Use this for initialization
	void Start () {
		base.Start ();
		foreach (Light l in lights) {
			l.intensity = 0f;
		}
		foreach (Light l in spots) {
			l.intensity = 0f;
		}
		//rp.RenderProbe ();

		#if UNITY_EDITOR
		debug = true;
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();

		if (!debug) {
			if (!lightsOn) {
				if (inSession) {
					StartCoroutine (letThereBeLight ());
					lightsOn = true;
				}
			}

			if (lightsOn) {
				if (!inSession) {
					StartCoroutine (letThereBeNoLight ());
					lightsOn = false;
				}
			}
		} else {
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (!lightsOn) {
					StartCoroutine (letThereBeLight ());
					lightsOn = true;
				} else {
					StartCoroutine (letThereBeNoLight ());
					lightsOn = false;
				}
			}

		}

	}

	private IEnumerator letThereBeLight(){
		//rp.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
		float intensity = 0f;
		float spotIntensity = 0f;
		while (intensity < targetIntensity) {
			intensity = Mathf.Min(intensity + (Time.deltaTime * (targetIntensity/duration)),targetIntensity);
			foreach (Light l in lights) {
				l.intensity = intensity;
			}
			spotIntensity = Mathf.Min(spotIntensity + (Time.deltaTime * (spotTargetIntensity/duration)),spotTargetIntensity);
			foreach (Light l in spots) {
				l.intensity = spotIntensity;
			}
			yield return null;
		}
		//rp.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
		//rp.RenderProbe();
	}

	private IEnumerator letThereBeNoLight(){
		//rp.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
		float intensity = targetIntensity;
		float spotIntensity = spotTargetIntensity;
		while (intensity > 0f) {
			intensity = Mathf.Max(intensity - (Time.deltaTime * (targetIntensity/duration)),0f);
			foreach (Light l in lights) {
				l.intensity = intensity;
			}

			spotIntensity = Mathf.Max(spotIntensity - (Time.deltaTime * (spotTargetIntensity/duration)),0f);
			foreach (Light l in spots) {
				l.intensity = spotIntensity;
			}
			yield return null;
		}
		//rp.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
		//rp.RenderProbe();
	}
}
