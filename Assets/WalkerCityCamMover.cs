using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerCityCamMover : AbstractGameEffects {

    public Transform[] path;

    float legDuration;
    float turnDuratiuon;
    int curTargetWaypoint;
    bool turning = false;
    public Transform vp;
    private float growthfactor = 30f;

    public Transform cam;

    public GameObject waterBody;

    public Color underwaterFog;
    public float underwateFogEnd = 100f;

    public Color overwaterFog;
    public float overwateFogEnd = 400f;

    private SwingBase theSwingBase;

    private bool readyToGo = false;
    private bool readyToTurn = false;
    private bool turnListening = false;
    private bool goListening = false;


    // Use this for initialization
    void Start () {
        base.Start();
        theSwingBase = GetComponent<SwingBase>();
        legDuration = sessionLength / path.Length+1;
        turnDuratiuon = legDuration / path.Length;
        curTargetWaypoint = 1;


        //set waypoint heights
        float height = 0;
        for (int i = 0; i < path.Length; i++)
        {
            if (i < 6)
            {
                height = height + growthfactor;
            }else if(i == 6)
            {
                height = 0f - growthfactor;
            }
            else if (i == 7)
            {
                height = 5f;
            }
            else if (i == 8)
            {
                height = 5f + growthfactor;
            }
            else
            {
                height = 1.5f;
            }
            path[i].position = new Vector3(path[i].position.x, height, path[i].position.z);
        }

        //look at the first position
        vp.LookAt(path[curTargetWaypoint].position);
        swingBase.zeroCrossingEvent.AddListener(zeroCrossing);
		FadeSphereScript.doFadeIn (5f, Color.black);

    }
    void zeroCrossing()
    {
        if (turnListening)
        {
            readyToTurn = true;
        }
        else if(goListening){
            readyToGo = true;
        }
    }

    public bool isTurning()
    {
        return turning;
    }
	
	// Update is called once per frame
	void Update () {
        base.Update();

        if(cam.position.y >= 0)
        {
            waterBody.GetComponent<Renderer>().enabled = false;
            RenderSettings.fogColor = overwaterFog;
            RenderSettings.fogEndDistance = overwateFogEnd;
        }
        else
        {
            waterBody.GetComponent<Renderer>().enabled = true;
            RenderSettings.fogColor = underwaterFog;
            RenderSettings.fogEndDistance = underwateFogEnd;
        }

        if(curTargetWaypoint >= path.Length)
        {
            return;
        }

        if (readyToTurn)
        {
            print("Ready to turn");
            turning = true;
            curTargetWaypoint++;
            readyToTurn = false;
            turnListening = false;
			if (curTargetWaypoint >= path.Length) {
				if(!FadeSphereScript.isFading()){
					FadeSphereScript.doFadeOut (5f, Color.black);
				}
			}

        }
        else
        {
            if (Vector3.Distance(vp.position, path[curTargetWaypoint].position) < 1f)
            {
                readyToTurn = false;
                turnListening = true;
            }
        }

        if (turning)
        {
            //print("Turning");
			if (curTargetWaypoint >= path.Length) {
				return;
			}

            Quaternion targetRotation = Quaternion.LookRotation(path[curTargetWaypoint].position - vp.position);
            vp.rotation = Quaternion.Slerp(vp.rotation, targetRotation, turnDuratiuon * Time.deltaTime * 1.5f);
            float angle = Vector3.Angle((path[curTargetWaypoint].position - vp.position), vp.forward);
            if (angle < 0.1f)
            {
                vp.LookAt(path[curTargetWaypoint].position);
                if (readyToGo)
                {
                    turning = false;
                }
                else
                {
                    readyToGo = false;
                    goListening = true;
                }
            }
        }
        else
        {
            //print("Moving towards " + path[curTargetWaypoint].name);
            //vp.transform.LookAt(path[curTargetWaypoint].position);
            vp.localEulerAngles = new Vector3(vp.localEulerAngles.x, vp.localEulerAngles.y, swingAngle / 2f);
            vp.Translate(Vector3.forward * Time.deltaTime * (legDuration * 1.5f));

            float yPos = vp.position.y - Remap(Mathf.Abs(swingAngle), 0, 45, 0, vp.position.y/2f);

            cam.position = new Vector3(vp.position.x, yPos, vp.position.z);
        }
	}

    private float Remap(float val, float OldMin, float OldMax, float NewMin, float NewMax)
    {
        return (((val - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
    }


}
