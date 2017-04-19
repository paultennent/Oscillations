using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSphereScript : MonoBehaviour {

    static FadeSphereScript globalAccess;

    public bool fading = false;

	// Use this for initialization
	void Start () {
        globalAccess = this;
    }
	
	// Update is called once per frame
	void Update () {

	}

    public static bool isFading() {
        return globalAccess.fading;
    }

    static IEnumerator fadeOut(float t, Color fadeColour)
    {
        Material mat = globalAccess.GetComponent<Renderer>().material;
        if (!globalAccess.fading)
        {
            globalAccess.fading = true;
            float fadeStartTime = Time.time;
            while (Time.time < fadeStartTime + t)
            {
                mat.color = new Color(fadeColour.r, fadeColour.g, fadeColour.b, mat.color.a + (1f / t) * Time.deltaTime);
                yield return null;
            }
            globalAccess.fading = false;
        }
        yield break;
    }

    static IEnumerator fadeIn(float t, Color fadeColour)
    {
        Material mat = globalAccess.GetComponent<Renderer>().material;
        mat.color = fadeColour;
        if (!globalAccess.fading)
        {
            globalAccess.fading = true;
            float fadeStartTime = Time.time;
            while (Time.time < fadeStartTime + t)
            {
                if(Time.time > fadeStartTime + (t / 5f))
                {
                    mat.color = new Color(fadeColour.r, fadeColour.g, fadeColour.b, mat.color.a - (1f / ((4f / 5f) * t)) * Time.deltaTime);
                } 
                yield return null;
            }
            globalAccess.fading = false;
        }
        yield break;
    }

    public static void doFadeIn(float t, Color fadeColour)
    {
        globalAccess.StartCoroutine(fadeIn(t, fadeColour));
    }

    public static void doFadeOut(float t, Color fadeColour)
    {
        globalAccess.StartCoroutine(fadeOut(t, fadeColour));
    }
}
