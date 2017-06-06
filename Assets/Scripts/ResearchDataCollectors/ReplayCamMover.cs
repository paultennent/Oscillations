using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReplayCamMover : ResearchReplay
{

	byte[] rawData;
	float lerpSpeed = 1f;

    public void Start()
    {
        base.Start();
		rawData = GameObject.Find ("ReplayController").GetComponent<ReplaySceneSelector> ().getData ();
		Open(null,true,rawData);
        Play();
    }
    
    public void Update()
    {
        base.Update();
        FrameData data=GetCurrentData();
        
        if(data!=null)
        {
			//print(data);
            //print(data.camPos);
            //Camera.main.transform.position=data.camPos;
			if (!Input.GetMouseButton(0)) {
				Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation,data.camRotation,Time.deltaTime*lerpSpeed);
			}
        }
    }
    
}