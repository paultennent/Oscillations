using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFloor : AbstractGameEffects {

	private Vector3 basePos;
	private Transform floor;

	public float roomMaxMove = 2f;
    public bool vertigoEffect=true;

	private float G;
	private float roomPosY;
    
    private float lastMax=0f;
    private float curMax=0f;
    private bool lastLessZero=false;

	// Use this for initialization
	void Start () {
		base.Start ();
		floor = GameObject.FindGameObjectWithTag("Floor").transform;
		G = climaxTime / Mathf.Sqrt(roomMaxMove);
		basePos = floor.position;
	}
	
	// Update is called once per frame
	void Update () {
		base.Update ();
        float offset = -Mathf.Min((offsetTime/G)*(offsetTime/G),roomMaxMove);
        if(vertigoEffect)
        {
            bool lessZero = (swingAngle<0);
            if(lastLessZero!=lessZero && curMax>5)
            {
                lastMax=curMax;
                curMax=0;
            }
            lastLessZero=lessZero;
            curMax=Mathf.Max(Mathf.Abs(swingAngle),curMax);
            if(lastMax>5)
            {
                float mult=Mathf.Abs(swingAngle)/lastMax;
                offset*=mult;
            }else
            {
                offset=0;
            }
        }
		roomPosY= basePos.y-offset; 
		floor.position = new Vector3 (basePos.x, roomPosY, basePos.z);
	}
}
