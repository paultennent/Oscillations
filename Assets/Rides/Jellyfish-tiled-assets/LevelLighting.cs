using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLighting : MonoBehaviour {

    Color targetFogColour;
    float targetFogDistance;
    
    public float changeSpeed=0.05f;
    public float distanceChangeSpeed=3f;

    public Color initialFogColour;
    public Color caveFogColour;
    public Color midFogColour;
    public Color outFogColour;

    
	// Use this for initialization
	void Start () {
           targetFogColour=RenderSettings.fogColor;
           targetFogDistance=RenderSettings.fogEndDistance;
	}
	
	// Update is called once per frame
	void Update () {
        Camera cam = Camera.main;
        
        LayerLayout l= LayerLayout.GetLayerLayout();
        
        switch(l.GetBlockAt(cam.transform.position.y))
        {
            case LayerLayout.LayoutPos.START:
                targetFogColour=initialFogColour;
                break;
            case LayerLayout.LayoutPos.CAVE_RPT:
                targetFogColour=caveFogColour;
                break;
            case LayerLayout.LayoutPos.CAVE_TOP:
                targetFogColour=Color.Lerp(caveFogColour,midFogColour,0.4f);
                break;
            case LayerLayout.LayoutPos.MID_BASE:
                targetFogColour=Color.Lerp(caveFogColour,midFogColour,0.7f);
                break;
            case LayerLayout.LayoutPos.MID_RPT:
            case LayerLayout.LayoutPos.MID_TOP:
                targetFogColour=midFogColour;
                targetFogDistance=500f;
                break;
            case LayerLayout.LayoutPos.CORAL:
            case LayerLayout.LayoutPos.FINISHED:
            case LayerLayout.LayoutPos.END:
                targetFogColour=outFogColour;
                targetFogColour=outFogColour;
                break;
        }
        Color fromColour=RenderSettings.fogColor;
        if(fromColour.r<targetFogColour.r)
        {
            fromColour.r=Mathf.Min(targetFogColour.r,fromColour.r+Time.deltaTime*changeSpeed);
        }else
        {
            fromColour.r=Mathf.Max(targetFogColour.r,fromColour.r-Time.deltaTime*changeSpeed);            
        }
        if(fromColour.g<targetFogColour.g)
        {
            fromColour.g=Mathf.Min(targetFogColour.g,fromColour.g+Time.deltaTime*changeSpeed);
        }else
        {
            fromColour.g=Mathf.Max(targetFogColour.g,fromColour.g-Time.deltaTime*changeSpeed);            
        }
        if(fromColour.b<targetFogColour.b)
        {
            fromColour.b=Mathf.Min(targetFogColour.b,fromColour.b+Time.deltaTime*changeSpeed);
        }else
        {
            fromColour.b=Mathf.Max(targetFogColour.b,fromColour.b-Time.deltaTime*changeSpeed);            
        }
        
        RenderSettings.fogColor = fromColour;            
        
        float distance=RenderSettings.fogEndDistance;
        if(distance<targetFogDistance)
        {
            distance=Mathf.Min(targetFogDistance,distance+Time.deltaTime*distanceChangeSpeed);
        }else if(distance>targetFogDistance)
        {
            distance=Mathf.Max(targetFogDistance,distance-Time.deltaTime*distanceChangeSpeed);
        }
        RenderSettings.fogEndDistance=distance;
        if(cam.farClipPlane<distance+10f)
        {
            cam.farClipPlane=distance+10f;
        }
        
		cam.backgroundColor=fromColour;
	}
}
