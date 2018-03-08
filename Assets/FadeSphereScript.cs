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
    
    public float fadeAlpha=1f;
    public float targetAlpha=1f;
    public float alphaStep=0f;
    public Color fadeColour;

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
        
        if(alphaStep>0.01f)
        {
            fading=true;
            if(targetAlpha>fadeAlpha)
            {                
                fadeAlpha+=alphaStep*Time.deltaTime;
                if(fadeAlpha>targetAlpha)
                {
                    // stop fade
                    alphaStep=0f;
                    fadeAlpha=targetAlpha;
                    fading=false;
                }
            }else if(targetAlpha<fadeAlpha)
            {
                fadeAlpha-=alphaStep*Time.deltaTime;
                if(fadeAlpha<targetAlpha)
                {
                    // stop fade
                    alphaStep=0f;
                    fadeAlpha=targetAlpha;
                    fading=false;
                }
            }
            Material mat = GetComponent<Renderer>().material;
            mat.color = new Color(fadeColour.r, fadeColour.g, fadeColour.b, fadeAlpha);
        }
        
                
        if(SessionManager.getInstance()!=null && SessionManager.getInstance().isInSession())
        {
            seenGame=true;
        }else
        {
            if(seenGame && (targetAlpha!=1f || (fadeAlpha!=1 && alphaStep==0)) )
            {
                fadeTo(.2f,1f,Color.black);
            }
        }

	}

    public void fadeTo(float step,float target,Color fadeColour)
    {
        this.fadeColour=fadeColour;
        this.alphaStep=step;
        this.targetAlpha=target;
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

    public static void doFadeIn(float t, Color fadeColour)
    {
        if(globalAccess!=null)
        {
            globalAccess.fadeTo(1.0f/t,0,fadeColour);
        }
    }

    public static void doFadeOut(float t, Color fadeColour)
    {
        if(globalAccess!=null)
        {
            globalAccess.fadeTo(1.0f/t,1,fadeColour);
        }
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
