using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RotatableReflectionProbe : MonoBehaviour 
{
    //ReflectionProbe[] reflectionProbes;
    //MeshRenderer rnd;
    public SavedProbeInfo[] savedProbeInfo;

    [System.Serializable]
    public class SavedProbeInfo
    {
        public Vector3 size;
        public Vector3 origin;
        public ReflectionProbe probe;
    }

    public enum yRotation
    {
        angle0,
        angle90,
        angle180,
        angle270
    }

    public yRotation yAngle;

    bool inited;
    yRotation yLastRotation;

	// Use this for initialization
	void Start () 
    {
        var reflectionProbes = GetComponentsInChildren<ReflectionProbe>();

        var savedProbes = new List<SavedProbeInfo>();

        foreach(var probe in reflectionProbes)
        {
            var probeInfo = new SavedProbeInfo();
            probeInfo.size = probe.size;
            probeInfo.origin = probe.center;
            probeInfo.probe = probe;

            savedProbes.Add(probeInfo);
        }

        savedProbeInfo = savedProbes.ToArray();
        
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(yAngle != yLastRotation || !inited)
        {
            bool rotate = false;
            if (rotate)
            {
                var eulerAngles = transform.localEulerAngles;
                eulerAngles.y = 90 * (int)yAngle;
                transform.localEulerAngles = eulerAngles;
            }

            foreach(var probeInfo in savedProbeInfo)
            {
                if(yAngle == yRotation.angle0 || yAngle == yRotation.angle180)
                {
                    probeInfo.probe.size = probeInfo.size;
                }
                else
                {
                    probeInfo.probe.size = new Vector3(probeInfo.size.z, probeInfo.size.y, probeInfo.size.x);
                }

                if (yAngle == yRotation.angle0)
                {
                    probeInfo.probe.center = probeInfo.origin;
                }
                else if (yAngle == yRotation.angle90)
                {
                    probeInfo.probe.center = new Vector3(probeInfo.origin.z, probeInfo.origin.y, -probeInfo.origin.x);
                }
                else if (yAngle == yRotation.angle180)
                {
                    probeInfo.probe.center = new Vector3(-probeInfo.origin.x, probeInfo.origin.y, -probeInfo.origin.z);
                }
                else
                {
                    probeInfo.probe.center = new Vector3(-probeInfo.origin.z, probeInfo.origin.y, probeInfo.origin.x);
                }
            }

            yLastRotation = yAngle;
            inited = true;
        }

        /*
        if (transform.Find("Rotatable") == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Rotatable";
            cube.transform.parent = transform;

            if (reflectionProbe != null)
            {
                cube.transform.localScale = reflectionProbe.size;
                cube.transform.localPosition = reflectionProbe.center;
            }

            rnd = cube.GetComponent<MeshRenderer>();
        }

        if (rnd != null && reflectionProbe != null)
        {
            reflectionProbe.size = rnd.bounds.size;
        }*/
	}
}
