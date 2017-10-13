using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.SceneManagement;

public class OVRPower: MonoBehaviour {

public void Start()
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    OVRPlugin.cpuLevel=0;
    OVRPlugin.gpuLevel=0;
    #endif
}

}
