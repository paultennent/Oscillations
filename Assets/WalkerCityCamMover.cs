using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerCityCamMover : AbstractGameEffects
{

    public Transform[] path;

    float legDuration;
    float turnDuratiuon;
    int curTargetWaypoint;
    bool turning = false;
    public Transform vp;

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

    private bool launched = false;
    private float introTime = 8f;
    private Vector3 initialPosition;

    public float seatDrop = 1.5f;
    private float swingStartTime;

    private float yDrop = 0f;

    private float targetTiltAngle = 0f;
    private bool leftStep = true;

    private float growthFactor = 1.25f;

	private float h2 = 1f;
	private float gf2 = 10f;

    private float stepRateMultiplier = 1.5f;

    private float maxTurnPerSwing = 15f;
    public float closeEnough = 15f;

    private float myheight = 1f;
    private bool completedTurn = true;

    private bool reachedWaypoint = false;

    public Material underWaterSky;
    public Material overwaterSky;

    private bool envSwitch = false;
    private bool inOutro = false;

    private float outroStartTime = 0f;

    public WakerAudioController audioController;
    private bool growSwitch = false;

    private bool fadedIn = false;


    // Use this for initialization
    void Start()
    {
        base.Start();
        theSwingBase = GetComponent<SwingBase>();
        legDuration = sessionLength / path.Length + 1;
        turnDuratiuon = legDuration / path.Length;
        curTargetWaypoint = 1;

        initialPosition = vp.position;

        //look at the first position
        vp.LookAt(path[curTargetWaypoint].position);
        

    }

    public bool isTurning()
    {
        return turning;
    }

    private float distToNextWP()
    {
        float dist = Vector2.Distance(new Vector2(vp.position.x, vp.position.z), new Vector2(path[curTargetWaypoint].position.x, path[curTargetWaypoint].position.z));
        return dist;
    }

    // Update is called once per frame
    void Update()
    {

        if (!fadedIn)
        {
            FadeSphereScript.doFadeIn(5f, Color.black);
            fadedIn = true;
        }
        base.Update();

        if (curTargetWaypoint >= path.Length)
        {
            //we're at the end.
            //lerp to the correct position and rotation
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

        //intro - still gather the angles as if we were stepping
        if (sessionTime < introTime || !launched)
        {
            if (swingQuadrant == 0 || swingQuadrant == 3)
            {
                if (swingStartTime == -1f)
                {
                    swingStartTime = Time.time;
                    yDrop = 0f;
					stepRateMultiplier = 1.6f;//myheight / 10f;

					print ("Height:" + myheight + ", Multiplier:" + stepRateMultiplier); 

                    if (leftStep)
                    {
                        targetTiltAngle = swingAngle / 2f;
                    }
                    else
                    {
                        targetTiltAngle = -swingAngle / 2f;
                    }
                    leftStep = !leftStep;
                }
            }
            else
            {
                swingStartTime = -1f;
            }
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
            if (!turning)
            {
				//closeEnough = 5f + yDrop;

                if (distToNextWP() < closeEnough)
                {
                    reachedWaypoint = true;
                }
                //we're moving forward
				if (swingQuadrant == 0 || swingQuadrant == 3) {
					if (swingStartTime == -1f) {
						//this is stuff we do once at the beginning of each cycle (starting from the back)
						completedTurn = true;
						growSwitch = false;
						if (curTargetWaypoint < 5) {
							myheight = myheight * growthFactor;
							h2 = h2 * gf2;
						} else {
							myheight = myheight / (growthFactor * 4);
							h2 = h2 * gf2;
						}
						swingStartTime = Time.time;
						yDrop = 0f;
						stepRateMultiplier = 1.6f;//myheight / 10f;

						print ("Height:" + myheight + ", Multiplier:" + stepRateMultiplier); 

						if (leftStep) {
							targetTiltAngle = swingAngle / 4f;
						} else {
							targetTiltAngle = -swingAngle / 4f;
						}
						leftStep = !leftStep;
						if (reachedWaypoint) {
							//we're ready to tun here
							turning = true;
							curTargetWaypoint++;
							reachedWaypoint = false;
							return;
						}
					}
					if (swingQuadrant == 3) {
						//in the first forward quadrant we're arcing down
						float curYDrop = ((myheight/2f) * Mathf.Sin (swingAngle * Mathf.Deg2Rad));
						yDrop += curYDrop * Time.deltaTime;
						if (!reachedWaypoint) {
							vp.Translate (new Vector3 (0f, curYDrop, swingAngVel * stepRateMultiplier) * Time.deltaTime);
						} else {
							vp.Translate (new Vector3 (0f, curYDrop, 0f) * Time.deltaTime);
						}
					} else {
						//second forward quadrant is pretty straight
						if (!reachedWaypoint) {
							vp.Translate (new Vector3 (0f, 0f, swingAngVel * stepRateMultiplier) * Time.deltaTime);
						} else {
							vp.Translate (new Vector3 (0f, 0f, 0f) * Time.deltaTime);
						}
					}
					vp.Rotate(new Vector3(0f, 0f, targetTiltAngle / (swingCycleTime / 2f)) * Time.deltaTime);
					//float myang = Mathf.Sin ((swingPhase * Mathf.Deg2Rad + 45)) * targetTiltAngle;
					//vp.localEulerAngles = new Vector3 (vp.localEulerAngles.x, vp.localEulerAngles.y, myang);
				}
                else
                {
                    //now we're going backwards so we want to slowly grow back up to height
                    if (completedTurn)
                    {
                        if (!growSwitch)
                        {
                            audioController.grow();
                            growSwitch = true;
                        }
                        swingStartTime = -1f;
						if (!reachedWaypoint) {
							vp.Translate (new Vector3 (0f, (-yDrop / (swingCycleTime / 2f)) * growthFactor, -(swingAngVel * stepRateMultiplier) / 3f) * Time.deltaTime);
						} else {
							vp.Translate (new Vector3 (0f, (-yDrop / (swingCycleTime / 2f)) * growthFactor, 0f) * Time.deltaTime);
						}
                        vp.Rotate(new Vector3(0f, 0f, -targetTiltAngle / (swingCycleTime / 2f)) * Time.deltaTime);
                    }
                }
            }
            else
            {
                //only turn on the back swings
                if (swingQuadrant == 3 || swingQuadrant == 1 || swingQuadrant == 2)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(path[curTargetWaypoint].position - vp.position);

                    float rotationSpeed = maxTurnPerSwing / (swingCycleTime / 2f);
                    float angle = Quaternion.Angle(vp.rotation, targetRotation);
                    float timeToComplete = angle / rotationSpeed;
                    float donePercentage = Mathf.Min(1F, Time.deltaTime / timeToComplete);
                    vp.rotation = Quaternion.Slerp(vp.rotation, targetRotation, donePercentage);
                    float curAngle = Vector3.Angle((path[curTargetWaypoint].position - vp.position), vp.forward);
                    if (curAngle < 0.1f)
                    {
                        vp.rotation = targetRotation;
                        turning = false;
                        completedTurn = false;
                        swingStartTime = -1f;
                    }

                }
            }
        }
    }

    private float Remap(float val, float OldMin, float OldMax, float NewMin, float NewMax)
    {
        return (((val - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
    }


}
