using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalkerCityCamMover : AbstractGameEffects
{

    private Transform[] path;

	public Transform pathParent;

    int curTargetWaypoint;
    bool turning = false;
    public Transform vp;
	public Transform pivot;
    public Transform cam;
    public bool debugShowPath=false;

    public GameObject waterBody;

    public Color underwaterFog;
    public float underwateFogEnd = 100f;
	public float underwaterFogDensity = 0.02f;

    public Color overwaterFog;
    public float overwateFogEnd = 400f;
	public float overwaterFogDensity = 0.005f;

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

	float speedMultiplier = 1f;
	float rotAmount = 0f;
	float distTravelled;
	float nextStepDist = 0f;

	bool growing = false;
	bool shrinking = false;


	float zeroFloor = 1.3f;
	bool hitWaypoint = false;

	bool sentGrow = false;

	float maxheight = 100f;

    
    public float turnRadius=20f;
	//route rules
	int stage = 0;
	int stepCounter = 0;
    float currentTurnAmount=0f;
    float percentageThroughTurn=0f;
    float currentFloorHeight=0;

	float lauchclimaxTime = 0f;

	float lastDistToNextWaypoint = 9999999f;

	float maxAngleForSwoop = 45f;

	float lastSeenAngle = 0f;

    // Use this for initialization
    void Start()
    {
        base.Start();
        currentFloorHeight=zeroFloor;

		path = new Transform[pathParent.childCount];
		int count = 0;
		foreach (Transform t in pathParent) {
			path [count] = t;
			count++;
		}

        if(debugShowPath)
        {
            GameObject fullPath=new GameObject("fullpath");
            for(int c=0;c<2000;c++)
            {
                followWaypointPath(2);
                GameObject newObj=GameObject.CreatePrimitive(PrimitiveType.Cube);
                newObj.transform.position=vp.position;
                newObj.transform.parent=fullPath.transform;
            }
            followWaypointPath(-10000);
        }
        
        theSwingBase = GetComponent<SwingBase>();
        curTargetWaypoint = 1;

        initialPosition = vp.position;

		swingBase.zeroCrossingEvent.AddListener (zeroCross);

	}

	public void zeroCross(){
		if (launched && !inOutro) {
			if (growthRate > 0) {
				audioController.grow ();
			} else if (growthRate < 0) {
				audioController.shrink ();
			}
		}
	}

    public bool isTurning()
    {
		return turning;
    }

    // Update is called once per frame
    void Update()
    {
		base.Update();

		if (debugText != null) {
			debugText.text = "" + swingAngle + ":" + swingQuadrant + ":" + swingPhase +":Steps:" +stepCounter;
		}

        if(Input.GetKey("]"))
        {
            followWaypointPath(Time.deltaTime*500f);
        }
        if(Input.GetKey("["))
        {
            followWaypointPath(-Time.deltaTime*500f);
        }

		if(Input.GetKey(KeyCode.N))
		{
			print (vp.position);
		}
        
		if (!inSession) {
			return;
		}

		audioController.begin ();

		//keep track of our angles and steps
		if ((lastSwingQuadrant == 2) && (swingQuadrant == 3)) {
			//we're starting a new step so we need to zero the swoop time
			maxTipAngle = -swingAngle * tipMultiplier;
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
            inOutro=true;
            {
                if (Time.time > outroStartTime + 5f)
                {
                    if (!FadeSphereScript.isFading())
                    {
                        FadeSphereScript.doFadeOut(5f, Color.black);
                    }
                }
                Vector3 endPos = vp.position;
                Vector3 topPoint = endPos + Vector3.up * seatDrop;
				Quaternion rotation = Quaternion.Euler(-swingAngle, 0f, 0f);
                Vector3 rotationOffset = rotation * Vector3.up * -seatDrop;
                Vector3 targetPoint = rotationOffset + topPoint;
				Quaternion pivRot = Quaternion.RotateTowards (pivot.rotation, Quaternion.Euler (0f, 0f, 0f), Time.deltaTime);
				pivot.rotation = pivRot;
				cam.position = targetPoint;
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
				RenderSettings.fogDensity = overwaterFogDensity;
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
				RenderSettings.fogDensity = underwaterFogDensity;
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
				lauchclimaxTime = climaxRatio;
            }

        }
        else
        {
			float distToNextWaypoint = XZDistance (vp.position, path [stage + 1].position);

            //this is the swinging action
			if (((lastSwingQuadrant == 2) && (swingQuadrant == 3)) || ((lastSwingQuadrant == 0) && (swingQuadrant == 1))) {
					
				swoopTime = 0f;
				sentGrow = false;
				stepCounter++;

			}
				

//			float halfcycle = .1f;//swingCycleTime / 2f;
//			float quartercycle = .5f;//swingCycleTime / 4f;
//			print (quartercycle);
//			swoopTime += Time.deltaTime;
//
//			float phaseFracPart = swingPhase - (float)swingQuadrant;
//			//do the steps with a half cycle
//			if (swingQuadrant == 3 || swingQuadrant == 1) {
//
//				yPos = Mathf.Cos (Mathf.PI * phaseFracPart);
//				speed = Mathf.Cos (Mathf.PI * phaseFracPart);
//			}
//			else if (swingQuadrant == 0 || swingQuadrant == 2) {
//				yPos = Mathf.Cos (Mathf.PI + Mathf.PI * phaseFracPart);
//				if (phaseFracPart < 0.333f) {
//					speed = Mathf.Cos (Mathf.PI +  Mathf.PI * phaseFracPart * 3f);
//				} else {
//					speed = 1;
//				}
//
///*				if (swoopTime <= quartercycle + (quartercycle / swoopRatio)) {
//					yPos = Mathf.Cos (Mathf.PI * swoopTime / quartercycle);
//					speed = (Mathf.Cos (Mathf.PI * swoopTime / (quartercycle / swoopRatio)));
//				} else {
//					yPos = Mathf.Cos (Mathf.PI * swoopTime / quartercycle);
//					speed = 1f;
//					//don't grow when turning
//					//if (!turning) {
//
//					//}
//				}*/
//				if (!sentGrow) {
//					if (growthRate> 2) {
//						audioController.grow ();
//					} else if (growthRate  < 0) {
//						audioController.shrink ();
//					}
//					sentGrow = true;
//				}
//			}
//			print (yPos + ":" + swingQuadrant+":"+phaseFracPart);
//
//			myHeight = 1f + (climaxRatio - lauchclimaxTime) * maxheight;//myHeight + (growthRate * Time.deltaTime);
//
//			if (swingQuadrant == 0 || swingQuadrant == 2) {	
//				if (vp.position.y < currentFloorHeight) {
//					float diff = Mathf.Abs (currentFloorHeight - vp.position.y);
//					float newPos = vp.position.y + (diff * Time.deltaTime * 2f);
//					if (newPos > currentFloorHeight) {
//						newPos = currentFloorHeight;
//					}
//					vp.position = new Vector3 (vp.position.x, newPos, vp.position.z);
//				} else if (vp.position.y > currentFloorHeight) {
//				
//					float diff = Mathf.Abs (vp.position.y - currentFloorHeight);
//					float newPos = vp.position.y - (diff * Time.deltaTime * (2f ));
//					if (newPos < currentFloorHeight) {
//						newPos = currentFloorHeight;
//					}
//					vp.position = new Vector3 (vp.position.x, newPos, vp.position.z);
//				}
//			}
//
//			//don't shrink past our original height
//			if (myHeight < 1f) {
//				myHeight = 1f;
//			}
//
//			//get yPos into the correct range to be useful
//			yPos = (yPos - 1f)/2f;
//			speed = 1f-(yPos + 1f);
//
//			if (swingQuadrant == 0 || swingQuadrant == 3) {
//				curAngle = maxTipAngle * yPos;
//			} else {
//				curAngle = -maxTipAngle * yPos;
//			}
//
//
//
//			//we only want to go down 75% of our height
//			if (yPos != 0f) {
//				yPos = yPos * myHeight * dropPercentage;
//			}
//
//            
//			speed = speed * Mathf.Max(myHeight,5f);
//
//            
//			//speed multiplier? - probably going to need this to be able to finish the route in time
//			speed = speed * speedMultiplier;
//				
//			pivot.transform.localPosition = new Vector3 (0f, myHeight, 0f);
//			cam.transform.localPosition = new Vector3 (0f, yPos, 0f);
//
//			pivot.localEulerAngles = new Vector3(0f,0f,curAngle);

			myHeight = 1f + (climaxRatio - lauchclimaxTime) * maxheight;
			//don't shrink past our original height
			if (myHeight < 1f) {
				myHeight = 1f;
			}


			yPos = Mathf.Min (Mathf.Abs (swingAngle), maxAngleForSwoop);
			yPos = Remap (yPos, 0, maxAngleForSwoop, 0, 0.75f);
			yPos = yPos * myHeight;

			curAngle = swingAngle / 4f;

			//as angle decreasing approaches max bring speed down to 0
			speed = Mathf.Min (Mathf.Abs (swingAngle), maxAngleForSwoop);
			speed = 1f - Remap (speed, 0, maxAngleForSwoop, 0, 1f);
			speed = speed * myHeight;

			if ((swingAngle > 0 && lastSeenAngle > swingAngle) || (swingAngle < 0 && lastSeenAngle < swingAngle)) {
				speed = speed * 3f;
				if (speed > myHeight) {
					speed = myHeight;
				}
			}

			speed = speed * speedMultiplier;
			pivot.transform.localPosition = new Vector3 (0f, myHeight, 0f);
			cam.transform.localPosition = new Vector3 (0f, -yPos, 0f);
			pivot.localEulerAngles = new Vector3(0f,0f,curAngle);
            followWaypointPath(speed*Time.deltaTime);

			if (vp.position.y < currentFloorHeight) {
				float diff = Mathf.Abs (currentFloorHeight - vp.position.y);
				float newPos = vp.position.y + (diff * Time.deltaTime * 2f);
				if (newPos > currentFloorHeight) {
					newPos = currentFloorHeight;
				}
				vp.position = new Vector3 (vp.position.x, newPos, vp.position.z);
			} else if (vp.position.y > currentFloorHeight) {
								
				float diff = Mathf.Abs (vp.position.y - currentFloorHeight);
				float newPos = vp.position.y - (diff * Time.deltaTime * (2f));
				if (newPos < currentFloorHeight) {
					newPos = currentFloorHeight;
				}
				vp.position = new Vector3 (vp.position.x, newPos, vp.position.z);
			}

            			//add banking
//			if (turning) {
//                float bankingAmount = currentTurnAmount;
//                // fade banking in and out
//                if(percentageThroughTurn<0.2)
//                {
//                    bankingAmount*=percentageThroughTurn*5f;
//                }else if(percentageThroughTurn>.8)
//                {
//                    bankingAmount*=(1f-percentageThroughTurn)*5f;
//                }
//                curAngle += -maxTipAngle * yPos * (currentTurnAmount*percentageThroughTurn) /30f;
//
//			}
				
			lastDistToNextWaypoint = distToNextWaypoint;
        }

		lastSeenAngle = swingAngle;
			
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
    
    float totalDistanceOnPath=0f;
    void followWaypointPath(float distanceForwards)
    {

        totalDistanceOnPath+=distanceForwards;
        if(totalDistanceOnPath<0)totalDistanceOnPath=0;
        // path line
        Vector2 lineStart=new Vector2(path[0].position.x,path[0].position.z);
        float distanceLeft=totalDistanceOnPath;
        for(int c=1;c<path.Length;c++)
        {
            growthRate=path[c-1].localScale.y;
            currentFloorHeight=path[c-1].position.y+zeroFloor;
            // first calculate the straight line bit
            Vector2  lineEnd=new Vector2(path[c].position.x,path[c].position.z);
            Vector2 offset=(lineEnd-lineStart);
            Vector2 direction=offset.normalized;
            float lineDistance=offset.magnitude;
            if(c!=path.Length-1)
            {
                // find the radius point for the rounded corner
                // this is the point which is [radius] away from both lines in a perpendicular direction
                // along the bisecting vector
                
                
                
                
                Vector2 nextEnd=new Vector2(path[c+1].position.x,path[c+1].position.z);                
                Vector2 nextDirection= (nextEnd-lineEnd).normalized;

                
                // two directions of lines exiting and leaving this point
/*                float angle1 = Mathf.Rad2Deg*Mathf.Atan2(nextDirection.y,nextDirection.x);
                float angle2 = Mathf.Rad2Deg*Mathf.Atan2(-direction.y,-direction.x);
                // bisection = angle that is half way between these two 
                float bisectionAngle=(angle1+angle2)*.5;
                Vector2 radiusDirection=Quaternion.Euler(0,0,bisectionAngle)*Vector2.right;
                if(Vector2.Dot(radiusDirection,nextDirection)<0)
                {
                    radiusDirection=-radiusDirection;
                }*/
                
                float angle=Mathf.Rad2Deg*(Mathf.Atan2(nextDirection.y,nextDirection.x)-Mathf.Atan2(direction.y,direction.x));
                if(angle<-180)angle+=360;
                if(angle>180)angle-=360;

                Vector2 radiusDirection;
                if(angle<0)
                {
                    radiusDirection=Quaternion.Euler(0,0,(angle-180f)*.5f)*direction;
                }else
                {
                    radiusDirection=Quaternion.Euler(0,0,(angle+180f)*.5f)*direction;
                }
                
                // radius point is distance of radius from line in perpendicular direction
                Vector2 perpendicularLine1=Quaternion.Euler(0,0,90)*direction;
                if(Vector2.Dot(perpendicularLine1,nextDirection)<0)
                {
                    perpendicularLine1=Quaternion.Euler(0,0,-90)*direction;
                }
                Vector2 perpendicularLine2=new Vector2(nextDirection.y,-nextDirection.x);
                Vector2 cornerCentrePoint = lineEnd + radiusDirection * (turnRadius/Vector2.Dot(radiusDirection,perpendicularLine1));
                                
                float distanceFromSecondLine = ((cornerCentrePoint-lineEnd)-nextDirection*Vector2.Dot(cornerCentrePoint-lineEnd,nextDirection)).magnitude;

                // move away from the second line in the perpendicular that is coming back on us
                cornerCentrePoint -= direction*(turnRadius-distanceFromSecondLine)*Vector2.Dot(perpendicularLine2,direction);
                
/*                GameObject newObj=GameObject.CreatePrimitive(PrimitiveType.Cube);
                newObj.transform.position=new Vector3(cornerCentrePoint.x,0,cornerCentrePoint.y);
                newObj.transform.parent=fullPath.transform;
                newObj.name="CornerPoint";*/

                
                // now move away from the second line by radius
                float distanceBeforeTurn=Vector3.Dot(cornerCentrePoint - lineStart,direction); 
                //print(distanceBeforeTurn+","+lineDistance);
                
                if( distanceLeft<distanceBeforeTurn)
                {
                    // on the straight bit of this line
                    Vector2 finalPosition=lineStart+(distanceLeft*direction);;
                    vp.transform.position = new Vector3(finalPosition.x,vp.transform.position.y,finalPosition.y);
                    curTargetWaypoint=c;
                    vp.transform.rotation= Quaternion.LookRotation(new Vector3(direction.x,0,direction.y), Vector3.up);;
                    stage=c-1;
                    turning=false;
                    return;
                }else
                {
                    // take off the straight line bit
                    distanceLeft-=distanceBeforeTurn;

                    // go round the corner
                    
                    float arcDistance = Mathf.Abs(angle/360f) * (Mathf.PI * turnRadius*2f);
                    //print("AD:"+arcDistance);
                    if(arcDistance>distanceLeft)
                    {
                        float rotation = angle * Mathf.Abs(distanceLeft / arcDistance);
                        Vector2 turnPoint= lineStart+(distanceBeforeTurn*direction);
                        Vector2 radiusOffset = (turnPoint-cornerCentrePoint);
                        radiusOffset=Quaternion.Euler(0,0,rotation)*radiusOffset;
                        Vector3 finalPosition= radiusOffset + cornerCentrePoint;
                        vp.transform.position = new Vector3(finalPosition.x,vp.transform.position.y,finalPosition.y);
                        vp.transform.rotation= Quaternion.LookRotation(new Vector3(direction.x,0,direction.y), Vector3.up)*Quaternion.Euler(0,-rotation,0);
                        stage=c-1;
                        turning=true;
                        percentageThroughTurn=distanceLeft/arcDistance;
                        currentTurnAmount=Mathf.Abs(angle);
                        return;
                    }else
                    {
                        // find the next startpoint
                        distanceLeft-=arcDistance;
                        Vector2 turnPoint= lineStart+(distanceBeforeTurn*direction);
                        Vector2 radiusOffset = (turnPoint-cornerCentrePoint);
                        radiusOffset=Quaternion.Euler(0,0,angle)*radiusOffset;
                        lineStart=radiusOffset + cornerCentrePoint;
                    }
                }
                // now calculate the 
            }else
            {
                // last line - go along it and stop at the end
                if(distanceLeft>lineDistance)
                {
                    stage=c;
                }else
                {
                    stage=c-1;
                }
                distanceLeft=Mathf.Min(distanceLeft,lineDistance);
                Vector3 finalPosition=lineStart+distanceLeft*direction;
                vp.transform.position = new Vector3(finalPosition.x,vp.transform.position.y,finalPosition.y);
                vp.transform.rotation= Quaternion.LookRotation(new Vector3(direction.x,0,direction.y), Vector3.up);
                turning=false;
                return;
            }
        }
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
