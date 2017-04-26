using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReplayCamMover : ResearchReplay
{
    public void Start()
    {
        base.Start();
        Open("D:\\jqm\\Box Sync\\VRPlayground-Data\\20170426124624-10010037-ce12160c4a6c7b0a05");
        Play();
    }
    
    public void Update()
    {
        base.Update();
        FrameData data=GetCurrentData();
        print(data);
        if(data!=null)
        {
            print(data.camPos);
            Camera.main.transform.position=data.camPos;
            Camera.main.transform.rotation=data.camRotation;
        }
    }
    
}