using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour {

    static Fader globalAccess;

    float fadeStartTime=0f;
    float fadeEndTime=0f;

    Image img;
	public Color fadeColour = Color.black;
    
	// Use this for initialization
	void Start () {
        globalAccess=this;
        GetComponent<Canvas>().enabled=false;
        img=transform.GetChild(0).gameObject.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		if(fadeEndTime!=0f)
        {
            float alpha=1f;
            if(Time.time>fadeEndTime)
            {
                fadeEndTime=0f;
            }else
            {
                float divisor=fadeEndTime-fadeStartTime;
                alpha=(Time.time-fadeStartTime)/divisor;
            }
			img.color=new Color(fadeColour.r,fadeColour.g,fadeColour.b,alpha);
        }
	}
    
    public static void DoFade(float endTime)
    {        
        if(globalAccess!=null)
        {
			globalAccess.img.color=new Color(0f,0f,0f,0f);
            globalAccess.GetComponent<Canvas>().enabled=true;
            if(endTime!=Time.time)
            {
                globalAccess.fadeStartTime=Time.time;
                globalAccess.fadeEndTime=endTime;
            }else
            {
                globalAccess.fadeStartTime=Time.time-1f;
                globalAccess.fadeEndTime=endTime;                
            }
        }
    }
    
    public static void EndFade()
    {
        if(globalAccess!=null)
        {
            globalAccess.GetComponent<Canvas>().enabled=false;
        }        
    }
    
    public static bool IsFading()
    {
        if(globalAccess!=null)
        {
            return globalAccess.GetComponent<Canvas>().enabled;
        }
        return false;
    }
}
