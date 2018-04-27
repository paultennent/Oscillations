using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBlock : MonoBehaviour {

    //light intensity
    public float lightIntensity = 1f;
    public float spotAngle = 179f;
    private float lastLightIntensity;
    private float lastSpotAngle;
    private Color lastColor;

    public Color default_color = Color.white;

	// Use this for initialization
	void Start () {
        lastLightIntensity = lightIntensity;
        lastSpotAngle = spotAngle;
        lastColor = default_color;
        recursiveSetLightIntensity(transform, lightIntensity, spotAngle, default_color);
	}
	
	// Update is called once per frame
	void Update () {
        checkIntensity();
	}

    public void checkIntensity()
    {
        if (lastLightIntensity != lightIntensity || lastSpotAngle != spotAngle || lastColor != default_color)
        {
            lastLightIntensity = lightIntensity;
            lastSpotAngle = spotAngle;
            lastColor = default_color;
            recursiveSetLightIntensity(transform, lightIntensity, spotAngle, default_color);
        }
    }

    public void recursiveSetLightIntensity(Transform t, float intensity, float spotAngle, Color color)
    {
        foreach(Transform tr in t)
        {
            Light l = tr.GetComponent<Light>();
            if(l != null)
            {
                l.intensity = intensity;
                l.spotAngle = spotAngle;
                l.color = color;
            }
            recursiveSetLightIntensity(tr,intensity, spotAngle, color);
        }
    }

    public void setMaterialColour(Color c)
    {
        transform.Find("Wall").gameObject.GetComponent<Renderer>().material.color = c;
    }
    
}
