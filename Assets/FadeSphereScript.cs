using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.SceneManagement;

public class FadeSphereScript : MonoBehaviour {

    static FadeSphereScript globalAccess;

    public bool fading = false;
    public bool fadingOut = false;
    
    public bool seenGame=false;
    
    private bool enableSphere=true;
    
    public string endScene="Menu";

	// Use this for initialization
	void Start () {
        globalAccess = this;
    }
    
    void OnDestroy() {
        if(globalAccess==this)
        {
            globalAccess=null;
        }
    }
    
    public static void enableFader(bool enabled)
    {
        if(globalAccess!=null)
        {
            globalAccess.enableSphere=enabled;
        }
    }
    
    public static bool isEnabled()
    {
        if(globalAccess!=null)
        {
            return globalAccess.enableSphere;
        }
        return false;
    }        
	
	// Update is called once per frame
	void Update () {
        GetComponent<Renderer>().enabled=enableSphere;
        if(!enableSphere)
        {            
            Renderer[] renderers = getChildRenderers();
            foreach(Renderer r in renderers)
            {
                r.enabled=false;
            }
        }else
        {
            Material mat = globalAccess.GetComponent<Renderer>().material;
            Renderer[] renderers = getChildRenderers();
            foreach(Renderer r in renderers)
            {
                r.enabled = (mat.color.a==1);
            }
        }
                
        if(SessionManager.getInstance()!=null && SessionManager.getInstance().isInSession())
        {
            seenGame=true;
        }else
        {
            if(seenGame && !fadingOut && !fading)
            {
                doFadeOut(5,Color.black);
            }
        }

	}

    public static bool isFading() {
        if(globalAccess==null)return false;
        return globalAccess.fading;
    }

    public static bool isFadingOut()
    {
        if(globalAccess==null)return false;
        return globalAccess.fadingOut;
    }
    
    public Renderer[] getChildRenderers()
    {
        Renderer[] renderers = transform.GetChild(0).GetComponentsInChildren<Renderer>();
        return renderers;
    }

    static IEnumerator fadeOut(float t, Color fadeColour)
    {
        if (globalAccess!=null && !globalAccess.fading)
        {
            Material mat = globalAccess.GetComponent<Renderer>().material;
            globalAccess.fading = true;
            globalAccess.fadingOut = true;
            float fadeStartTime = Time.time;
            while (Time.time < fadeStartTime + t)
            {
                mat.color = new Color(fadeColour.r, fadeColour.g, fadeColour.b, mat.color.a + (1f / t) * Time.deltaTime);
                yield return null;
            }
            globalAccess.fading = false;
            if(globalAccess.endScene!=null && globalAccess.endScene.Length>0)
            {
                SceneManager.LoadScene(globalAccess.endScene);
            }
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
        if(globalAccess!=null)
        {
            Renderer[] renderers = globalAccess.getChildRenderers();
            foreach(Renderer r in renderers)
            {
                r.enabled=false;
            }
            globalAccess.StartCoroutine(fadeIn(t, fadeColour));
        }
    }

    public static void doFadeOut(float t, Color fadeColour)
    {
        globalAccess.StartCoroutine(fadeOut(t, fadeColour));
    }
    
    public static void changePauseColour(Color c)
    {
        if(globalAccess!=null)
        {
            Renderer[] renderers = globalAccess.getChildRenderers();
            foreach(Renderer r in renderers)
            {
                r.material.color=c;
            }
        }
    }
}
