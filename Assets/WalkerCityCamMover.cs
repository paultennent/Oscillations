using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerCityCamMover : AbstractGameEffects
{

    public Transform[] path;

    int curTargetWaypoint;
    bool turning = false;
    public Transform vp;
	public Transform pivot;
    public Transform cam;

    public GameObject waterBody;

    public Color underwaterFog;
    public float underwateFogEnd = 100f;

    public Color overwaterFog;
    public float overwateFogEnd = 400f;

    private SwingBase theSwingBase;


    private bool launched = false;
    private float introTime = 8f;
    private Vector3 initialPosition;

    public float seatDrop = 1.5f;

    public Material underWaterSky;
    public Material overwaterSky;

    private bool envSwitch = false;
    private bool inOutro = false;

    private float outroStartTime = 0f;

    public WakerAudioController audioController;

    private bool fadedIn = false;

	float yPos;
	float speed;

	float swingStart = 0f;
	float swoopRatio = 3f;

	float dropPercentage = 0.75f;

	float maxTipAngle = 45f;
	float tipMultiplier = 0.25f;
	bool leftStaep = false;
	float curAngle = 0f;

	float swoopTime = 0f;
	bool climbing = false;

	float growthRate = 2f;
	float myHeight = 1f;

	int lastSwingQuadrant = 0;

	float speedMultiplier = 2f;
	float rotAmount = 0f;
	float distTravelled;
	float nextStepDist = 0f;

	bool growing = false;
	bool shrinking = false;

	bool steppingUp = false;

	float zeroFloor = 1.3f;
	bool hitWaypoint = false;

	bool sentGrow = false;

	//route rules
	int stage = 0;
	int stepCounter = 0;
	int[] steps;
	float[] growthRates;
	float[] turnAmounts;
	float [] floorheights;

	float lastDistToNextWaypoint = 9999999f;

    // Use this for initialization
    void Start()
    {
        base.Start();
        theSwingBase = GetComponent<SwingBase>();
        curTargetWaypoint = 1;

        initialPosition = vp.position;

		//setup route
		//17 steps - turn left 90 degrees
		//27 steps - turn right 30 degrees - into the city
		//30 steps - step up onto the buildings
		//34 steps - turn left 90 degrees and down into the waterline
		//43 steps - turn right 60 degrees - onto the far edge
		//46 steps - turn right 90 degrees - walking to end now

		//steps = new int[]{17,27,30,34,43,48,100000};
		growthRates = new float[] {3.005f,3f,3f,3f,3f,1.5f,1.5f,1.5f,-13.5f,-0.75f};
		turnAmounts = new float[] {0f,-90f,30f,0f,0f,-75f,0f,0f,45f,90f,0f};
		floorheights = new float[] {zeroFloor,zeroFloor,zeroFloor,zeroFloor+100f,zeroFloor+30f,zeroFloor,zeroFloor-100f,zeroFloor-50,zeroFloor,zeroFloor };
    }

    public bool isTurning()
    {
		return turning;
    }

    // Update is called once per frame
    void Update()
    {
		base.Update();

		if (!inSession) {
			return;
		}

		audioController.begin ();

		//keep track of our angles and steps
		if ((lastSwingQuadrant == 2) && (swingQuadrant == 3)) {
			//we're starting a new step so we need to zero the swoop time
			maxTipAngle = swingAngle * tipMultiplier;
		}
		lastSwingQuadrant = swingQuadrant;

        if (!fadedIn)
        {
            FadeSphereScript.doFadeIn(5f, Color.black);
            fadedIn = true;
        }
        

        if (stage >= path.Length-1)
        {
            //we're at the end - do the outro
            //lerp to the correct position and rotation, just in case we're out
            if (!inOutro)
            {
                if (Vector3.Distance(vp.position, path[path.Length - 1].position) < 0.1f)
                {
                    vp.position = path[path.Length - 1].position;
                    inOutro = true;
                    outroStartTime = Time.time;
                    return;
                }
                else
                {
                    vp.position = Vector3.Lerp(vp.position, path[path.Length - 1].position, Time.deltaTime * 3f);
                    vp.rotation = Quaternion.Slerp(vp.rotation, path[path.Length - 1].rotation, Time.deltaTime * 15f);
					pivot.localPosition = Vector3.Lerp (pivot.localPosition, Vector3.zero, Time.deltaTime * 15f);
                    return;
                }
            }
            else
            {
                if (Time.time > outroStartTime + 5f)
                {
                    if (!FadeSphereScript.isFading())
                    {
                        FadeSphereScript.doFadeOut(5f, Color.black);
                    }
                }
                print("Doing outro swinging");
                Vector3 endPos = vp.position;
                Vector3 topPoint = endPos + Vector3.up * seatDrop;
                Quaternion rotation = Quaternion.Euler(-swingAngle, 0, 0);
                Vector3 rotationOffset = rotation * Vector3.up * -seatDrop;
                Vector3 targetPoint = rotationOffset + topPoint;
                return;
            }
        }

        //handle water stuff
        if (cam.position.y >= 0)
        {
            if (!envSwitch)
            {
                //waterBody.GetComponent<Renderer>().enabled = false;
                RenderSettings.fogColor = overwaterFog;
                RenderSettings.fogEndDistance = overwateFogEnd;
                RenderSettings.skybox = overwaterSky;
                audioController.goAboveWater();
                envSwitch = true;
            }
        }
        else
        {
            if (envSwitch)
            {
                //waterBody.GetComponent<Renderer>().enabled = true;
                RenderSettings.fogColor = underwaterFog;
                RenderSettings.fogEndDistance = underwateFogEnd;
                RenderSettings.skybox = underWaterSky;
                audioController.goUnderWater();
                envSwitch = false;
            }
        }

        //intro
        if (sessionTime < introTime || !launched)
        {
            Vector3 topPoint = initialPosition + Vector3.up * seatDrop;
            Quaternion rotation = Quaternion.Euler(-swingAngle, 0, 0);
            Vector3 rotationOffset = rotation * Vector3.up * -seatDrop;
            Vector3 seatPoint = topPoint + rotationOffset;
            Vector3 onlyFwdBackPoint = new Vector3(seatPoint.x, topPoint.y, seatPoint.z);

            vp.transform.position = Vector3.Lerp(seatPoint, onlyFwdBackPoint, offsetTime / 10f);

            if (offsetTime > introTime && swingQuadrant == 3)
            {
                launched = true;
            }

        }
        else
        {
			float distToNextWaypoint = XZDistance (vp.position, path [stage + 1].position);

            //this is the swinging action
			if (((lastSwingQuadrant == 2) && (swingQuadrant == 3)) || ((lastSwingQuadrant == 0) && (swingQuadrant == 1))) {
				if (turning) {
					turning = false;
				}
				if (steppingUp) {
					steppingUp = false;
				}
					
				swoopTime = 0f;
				sentGrow = false;
				stepCounter++;

				if (distToNextWaypoint > lastDistToNextWaypoint || hitWaypoint) {
					//we've reached a turning point
					stage++;

					if (stage == path.Length) {
						//we've reached the end of the line
						return;
					}

					turning = true;
					if (floorheights [stage - 1] != floorheights [stage]) {
						steppingUp = true;
					}
					rotAmount = turnAmounts [stage];
					hitWaypoint = false;
					lastDistToNextWaypoint = 9999999f;
					distToNextWaypoint = 9999999f;
				}
			}
				
			growthRate = growthRates[stage];

			float halfcycle = swingCycleTime / 2f;
			float quartercycle = swingCycleTime / 4f;
			swoopTime += Time.deltaTime;

			//do the steps with a half cycle
			if (swingQuadrant == 3 || swingQuadrant == 1) {
				yPos = Mathf.Cos (Mathf.PI * swoopTime / quartercycle);
				speed = Mathf.Cos (Mathf.PI * swoopTime / quartercycle);
			}
			else if (swingQuadrant == 0 || swingQuadrant == 2) {
				if (swoopTime <= quartercycle + (quartercycle / swoopRatio)) {
					yPos = Mathf.Cos (Mathf.PI * swoopTime / quartercycle);
					speed = (Mathf.Cos (Mathf.PI * swoopTime / (quartercycle / swoopRatio)));
				} else {
					yPos = Mathf.Cos (Mathf.PI * swoopTime / quartercycle);
					speed = 1f;
					//don't grow when turning
					if (!turning) {
						myHeight = myHeight + (growthRate * Time.deltaTime);
					}
				}
				if (!sentGrow) {
					if (growthRates [stage] > 2) {
						audioController.grow ();
					} else if (growthRates [stage] < 0) {
						audioController.shrink ();
					}
					sentGrow = true;
				}
			}

			//if we have to turn
			if (turning) {
				vp.localEulerAngles = new Vector3 (0f, vp.localEulerAngles.y + turnAmounts [stage] * Time.deltaTime * (1f/halfcycle), 0f);
			}
				
			if (vp.position.y < floorheights [stage]) {
				float diff = Mathf.Abs(floorheights [stage] - vp.position.y);
				float newPos = vp.position.y + (diff * Time.deltaTime * (1f / halfcycle));
				if (newPos > floorheights [stage]) {
					newPos = floorheights [stage];
				}
				vp.position = new Vector3 (vp.position.x, newPos , vp.position.z);
			}
			else if (vp.position.y > floorheights [stage]) {
				
				float diff = Mathf.Abs(vp.position.y - floorheights [stage]);
				float newPos = vp.position.y - (diff * Time.deltaTime * (1f / halfcycle));
				if (newPos < floorheights [stage]) {
					newPos = floorheights [stage];
				}
				vp.position = new Vector3 (vp.position.x, newPos , vp.position.z);
			}

			//don't shrink past our original height
			if (myHeight < 1f) {
				myHeight = 1f;
			}

			//get yPos into the correct range to be useful
			yPos = (yPos - 1f)/2f;
			speed = 1f-(yPos + 1f);

			if (swingQuadrant == 0 || swingQuadrant == 3) {
				curAngle = maxTipAngle * yPos;
			} else {
				curAngle = -maxTipAngle * yPos;
			}

			//add banking
			if (turning) {
				if (turnAmounts [stage] < 0) {
					curAngle += maxTipAngle * yPos * Mathf.Abs(turnAmounts [stage])/30f;
					//turning left - bank right

				} else if (turnAmounts [stage] > 0) {
					curAngle += -maxTipAngle * yPos * Mathf.Abs(turnAmounts [stage])/30f;
					//turning right - bank left
				}
			}


			//we only want to go down 75% of our height
			if (yPos != 0f) {
				yPos = yPos * myHeight * dropPercentage;
			}

			speed = speed * myHeight;

			//speed multiplier? - probably going to need this to be able to finish the route in time
			speed = speed * speedMultiplier;
				
			pivot.transform.localPosition = new Vector3 (0f, myHeight, 0f);
			cam.transform.localPosition = new Vector3 (0f, yPos, 0f);

			pivot.localEulerAngles = new Vector3(0f,0f,curAngle);


			if (distToNextWaypoint <= lastDistToNextWaypoint && !hitWaypoint) {
				//if we're in the final run, lets just tidy up our position and angle
				//if (stage == path.Length - 2) {
					//fastLookAt (vp, path [stage + 1]);
				//}
				vp.transform.Translate (Vector3.forward * speed * Time.deltaTime);
				distTravelled += speed * Time.deltaTime;


			} else {
				hitWaypoint = true;
				float lefties = 1f/(halfcycle - swoopTime);
				//vp.position = Vector3.Lerp (vp.position, path [stage+1].position, Time.deltaTime * lefties);
			}
			lastDistToNextWaypoint = distToNextWaypoint;
        }



    }

    private float Remap(float val, float OldMin, float OldMax, float NewMin, float NewMax)
    {
        return (((val - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
    }

	private void fastLookAt(Transform t, Transform target){
		//immediately look at a target in 2D
		Vector3 lookPos = target.position - t.position;
		lookPos.y = 0;
		Quaternion rotation = Quaternion.LookRotation(lookPos);
		t.rotation = rotation;
	}
		

	private bool reachedWayPointExactly(){
		//we actually stop a bit before as we need space to turn - we'll plot these manually

		float wprotAmount = path[curTargetWaypoint].localEulerAngles.y - vp.localEulerAngles.y;
		if (wprotAmount > 180) {
			wprotAmount = wprotAmount - 180;
			wprotAmount = -wprotAmount;
		}
		float prop = wprotAmount / 360f;

		Vector3 myTestPos = vp.position + (vp.forward * (nextStepDist - (prop * nextStepDist)));
		float myDist = XZDistance (myTestPos, path [curTargetWaypoint].transform.position);

		if (myDist < 2f) {
			return true;
		}
		return false;
	}

	private float XZDistance(Vector3 t, Vector3 target){
		Vector3 vectorToTarget = t - target;
		vectorToTarget.y = 0;
		return vectorToTarget.magnitude;
	}
		
}
