using System;
using UnityEngine;
using System.Collections;

public class animatePng : MonoBehaviour
{
    public int StartFrame = 100;
    public int FrameCount = 19;
    public String Prefix = "ring_0";
    private int _step = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    _step++;
	    if (_step > FrameCount)
	    {
	        _step = 0;
	    }
	    Material mat = gameObject.GetComponent<Renderer>().material;
	    Texture2D tx = Resources.Load(Prefix + (StartFrame + _step)) as Texture2D;
	    mat.SetTexture("_MainTex", tx);
	}
}
