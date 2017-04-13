using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TimeOfDayManager : MonoBehaviour 
{
    [Range(0.0f, 24.0f)]
    public float timeOfDay;

    public Light sun;
    public GameObject nightLightRoot;

    public float sunIntensity;

	// Use this for initialization
	void Start () 
    {
        sunIntensity = sun.intensity;
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if (sun != null)
        {
            var eulerAngles = sun.transform.localEulerAngles;
            eulerAngles.y = 100.0f;
            eulerAngles.z = 0.0f;
            
            // Daytime
            if (8.0f <= timeOfDay && timeOfDay <= 20.0f)
            {
                var w = (timeOfDay - 8.0f) / 12.0f;
                eulerAngles.x = Mathf.SmoothStep(0.0f, 180.0f, w);
            }
            else
            {
                var w = 0.0f;
                if (20.0f < timeOfDay)
                {
                    w = 0.33f * ((timeOfDay - 20.0f) / 4.0f);
                }
                else
                {
                    w = 0.33f + 0.66f * timeOfDay / 8.0f;
                }

                eulerAngles.x = Mathf.SmoothStep(180.0f, 360.0f, w);
            }

            sun.transform.localEulerAngles = eulerAngles;

            sun.enabled = 6.0f <= timeOfDay && timeOfDay <= 22.0f;

            if (6.0f <= timeOfDay && timeOfDay <= 8.0f)
            {
                var w = (timeOfDay - 6.0f) / 2.0f;
                sun.intensity = Mathf.SmoothStep(0.0f, sunIntensity, w);
            }
            else if (20.0f <= timeOfDay && timeOfDay <= 22.0f)
            {
                var w = (timeOfDay - 20.0f) / 2.0f;
                sun.intensity = Mathf.SmoothStep(sunIntensity, 0.0f, w);
            }
            else
            {
                sun.intensity = sunIntensity;
            }

            if(21.0f <= timeOfDay && timeOfDay <= 24.0f)
            {
                nightLightRoot.SetActive(true);
            }
            else
            {
                nightLightRoot.SetActive(false);
            }

            //Debug.Log(eulerAngles.x + " " + sun.transform.localEulerAngles.x);

            //UnityEditor.SceneView.RepaintAll();

            //UnityEditor.EditorUtility.SetDirty(sun.transform);
        }
	}
}
