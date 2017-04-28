using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// only land on floor 1

public class ShuttlecockCityCamMover : AbstractGameEffects {

	public ShuttlecockAudioScript audioController;
	private CityBuilder cb;
	private bool madeStart = false;

	List<Vector3> currentTrajectory=null;
	float trajectoryIndex=0;

    public GameObject cage;
	public Transform currentPos;
	public float jumpGravity=9.81f;
    public float dt = 0.01f;
    public bool showCast=false;
    public bool showPointObjects=false;
    public GameObject travelPath;
    public float minJumpUp=10f;

    private Transform seat;
    private float seatDistance=-1.5f;

    
    
    // for jumping, we create a height map in a line in the direction we want to jump
    // by using downwards raycasts
    // then we calculate which jumpGravity based trajectory to a point on the other side of the 'road' 
    // will a)hit something, b)get us there in closest to the target total distance
    const int MAP_POINTS=100;
    class HeightMapDescriptor
    {
        public HeightMapDescriptor()
        {
            heightMap=new float[MAP_POINTS];
            trajectoryLens=new float[MAP_POINTS];
            gravityMults=new float[MAP_POINTS];
            upVelocities=new float[MAP_POINTS];
            isTopFloor=new bool[MAP_POINTS];
            numPoints=MAP_POINTS;
        }
        
        public float[] heightMap;
        public float[] trajectoryLens;
        public float[] gravityMults;
        public float[] upVelocities;
        public bool []isTopFloor;
        public int numPoints;
    }

    private HeightMapDescriptor mHeights=new HeightMapDescriptor();
    
    private GameObject endMarker;
    private GameObject roadMarker;
    private GameObject castParent;
    private float stretchMultiplier=1f;
    private bool inIntro=false;
    private bool inOutro=false;
    public bool outroNext=false;
    private float outroStartTime=0f;

    public Vector3 roadCentre=new Vector3(0,0,0);
    public Vector3 roadDirection=new Vector3(0,0,1);
    public float jumpAngle=15f;
    public float roadWidth=5f;
    private float targetDistance=0f;
    public float targetTime=1f;
    public float targetDistanceMultiplier=1f;

    private float targetEndTime=-1;
    
    private int lastQuadrant=-1;
    
    private Transform[] travelPathPoints;
    private int travelPathSegment=0;

    private bool fadedIn = false;

	public bool isInIntro(){
		return inIntro;
	}

	public bool isInOuttro()
	{
		return inOutro;
	}

	public bool isInTraining()
	{
		return (travelPathSegment==0);
	}
    
	// Use this for initialization
	void Start () {
        base.Start();

        usePLLPhaseEstimation=true;
        endMarker=new GameObject();
        endMarker.name="endmarker";
        roadMarker=new GameObject();
        roadMarker.name="roadmarker";
        castParent=new GameObject();
        castParent.name="castLine";
		cb = GetComponent<CityBuilder> ();
		currentTrajectory = new List<Vector3> ();
        
        travelPathPoints=new Transform[travelPath.transform.childCount];        
        for(int c=0;c<travelPath.transform.childCount;c++)
        {
            travelPathPoints[c]=travelPath.transform.GetChild(c);
        }
        if(swingPivot!=null)
        {
            seat=swingPivot.GetChild(0);
            seatDistance=seat.localPosition.y;
            print(seatDistance);
        }
	}
	
	// Update is called once per frame
	void Update () {

        

        base.Update();
        bool newQuadrant=false;
        if(swingQuadrant!=lastQuadrant)
        {
            newQuadrant=true;
        }
        if(newQuadrant && (swingQuadrant==0 || swingQuadrant==2))
        {
            // this is a point where we should launch 
            // set target end time to be this time + swingcycletime/2
            targetEndTime=Time.time + swingCycleTime/2f;
        }
        lastQuadrant=swingQuadrant;
        if(!inSession)return;

		audioController.begin ();

		if (!fadedIn)
		{
			FadeSphereScript.doFadeIn(5f, Color.black);
			fadedIn = true;
		}
			
        if (!madeStart) {
            currentPos.position=travelPathPoints[0].position;
            updateTravelPath(false);
            Vector3 b = FindFirstBuilding (out madeStart);
			if (madeStart) {
                if(showPointObjects)
                {
                    GameObject startPoint = new GameObject ();
                    startPoint.transform.position = b;
                    startPoint.name = "Start Point";
                }
				currentPos.position = b;
                cage.transform.position=currentPos.position;
			}
		} else {
            if(Input.GetKeyDown("z"))
            {
                outroNext=true;
            }
            // first 10 seconds, transitioning between swing and not swing and fading cube
            if(offsetTime<10f && countUp)
            {
                if(Input.GetKeyDown("s"))
                {
                    debugTimeOffset+=10f;
                    inIntro=false;
                    return;
                }
					
                inOutro=false;
                outroStartTime=0f;
                inIntro=true;
                outroNext=false;
                // put in an up and down oscillation from the swing only
                float ratio=1f-.1f*offsetTime;
                // now manually fix seat transform position and rotation
                float realSwingY=Mathf.Cos(swingAngle*Mathf.Deg2Rad) * seatDistance;
                float transformY=(2*Mathf.Pow(Mathf.Cos(.5f*swingAngle*Mathf.Deg2Rad),4) -1) * seatDistance;
                float realSwingZ=Mathf.Sin(swingAngle*Mathf.Deg2Rad)*seatDistance;
                
                seat.transform.localPosition=new Vector3(0,realSwingY*ratio + (1.0f-ratio)*transformY,ratio*realSwingZ);
//                seat.transform.rotation=Quaternion.Euler(0,180,swingAngle);
                return;
            }else if(inIntro)
            {
                // wait until we reach zero degrees before starting properly
                if(newQuadrant && (swingQuadrant==0 || swingQuadrant==2))
                {
                    inIntro=false;
                }else
                {
                    float transformY=(2*Mathf.Pow(Mathf.Cos(.5f*swingAngle*Mathf.Deg2Rad),4) -1) * seatDistance;
                    // bounce on spot
                    seat.transform.localPosition=new Vector3(0,transformY);                    
                    return;
                }
            }
            if(inOutro)
            {
                // if we've got trajectory points left, replay them
                if (currentTrajectory != null && currentTrajectory.Count > (int)trajectoryIndex) {
                    currentPos.position = currentTrajectory [(int)trajectoryIndex];
                    trajectoryIndex+=Time.deltaTime/dt;
                }else
                {
                    if(currentTrajectory.Count>0)
                    {
                        currentPos.position = currentTrajectory[currentTrajectory.Count-1];
                    }
                    if(offsetTime>10f && outroStartTime==0)outroStartTime=Time.time;
                }
                if(offsetTime<10f || outroStartTime!=0)
                {
                    float timeLeft=offsetTime;
                    if(outroStartTime!=0)
                    {
                        timeLeft=Mathf.Max(10f-(Time.time-outroStartTime),0);
                    }
                    float ratio=1f-.1f*timeLeft;
                    if(!FadeSphereScript.isFading())
                    {
						FadeSphereScript.doFadeOut(5f,Color.black);
                    }
                    // now manually fix seat transform position and rotation
                    float realSwingY=Mathf.Cos(swingAngle*Mathf.Deg2Rad) * seatDistance;
                    float transformY=(2*Mathf.Pow(Mathf.Cos(.5f*swingAngle*Mathf.Deg2Rad),4) -1) * seatDistance;
                    float realSwingZ=Mathf.Sin(swingAngle*Mathf.Deg2Rad)*seatDistance;
                    
                    seat.transform.localPosition=new Vector3(0,realSwingY*ratio + (1.0f-ratio)*transformY,ratio*realSwingZ);
                }
                return;
            }
            
            updateTravelPath(Input.GetKeyDown("n"));
            if(swingAmplitude!=0)
            {
                targetDistance=swingAmplitude*targetDistanceMultiplier;
            }        
			// if we've got trajectory points left, replay them
			if (currentTrajectory != null && currentTrajectory.Count > (int)trajectoryIndex) {
				currentPos.position = currentTrajectory [(int)trajectoryIndex];
				trajectoryIndex+=Time.deltaTime/dt;
                if(Input.GetKeyDown("s"))
                {
                    trajectoryIndex=currentTrajectory.Count;
                }
			}else
			{
                if(currentTrajectory.Count>0)
                {
                    // make sure we're calculating from end of last trajectory 
                    currentPos.position= currentTrajectory[currentTrajectory.Count-1];
                }
                float timeToPhase4=(4f-swingPhase)*(swingCycleTime/4f);
                targetTime=timeToPhase4;
                if(targetTime<swingCycleTime/4f)
                {
                    targetTime+=swingCycleTime/2f;
                }else if(targetTime>(3f* swingCycleTime/4f))
                {
                    targetTime-=swingCycleTime/2f;
                }
                
//                targetTime=swingCycleTime/2f;
                print(targetTime);
                // if we're 
                
                // set target time 
/*                if(targetEndTime>0 && targetEndTime>Time.time)
                {
                    targetTime = targetEndTime- Time.time;
                }
                // maybe we landed just slightly earlier in swing cycle than predicted, avoid
                // a double jump
                if(targetTime<0.5f)
                {
					targetTime += swingCycleTime / 2f;
                }*/
                // if we're coming to the end, zoom to the final point instead
                if((!countUp && offsetTime-targetTime<10f) || outroNext)
                {
                    CreateFinalTrajectory(currentPos.position,travelPathPoints[travelPathPoints.Length-1].position,targetTime,mHeights);
                    inOutro=true;
                    return;
                }

                for(int retries=5;retries>0;retries--)
                {
                    float offsetFrom=0;
                    float offsetTo=40;
                    float offsetStep=1f;
                    if(jumpAngle>45)
                    {
                        offsetTo=-40;
                        offsetStep=-1f;
                    }
                    for(float angleOffset=offsetFrom;(offsetStep<0 && angleOffset>offsetTo)|| (offsetStep>0 && angleOffset<offsetTo);angleOffset+=offsetStep)
                    {
                        
                        // distance to other side of road is the minimum we need to launch
                        // make sure that our array goes to 10% more than that at least
                        
                        Quaternion roadRotation = Quaternion.AngleAxis(90,Vector3.up);
                        Vector3 perpendicular=roadRotation*roadDirection;
                        // distance to centre of road
                        float roadDistancePerpendicular = Vector3.Dot((currentPos.position - roadCentre),perpendicular);

                        bool leftSide = (roadDistancePerpendicular < 0);
                        float leftRight=leftSide?1f:-1f;

                        Quaternion jumpRotation;
                        jumpRotation = Quaternion.AngleAxis(leftRight*(90-(jumpAngle+angleOffset)),Vector3.up);
                        Vector3 launchDir = jumpRotation*roadDirection ;
                        
                        float roadDistance = -(roadDistancePerpendicular-leftRight*roadWidth*.5f)/Vector3.Dot(launchDir,perpendicular);
                        // closest point on line = 
                        float minLaunchDistance=1.1f* roadDistance*stretchMultiplier;
                        float mapDistance=Mathf.Max(targetDistance,minLaunchDistance);

                        // are we on left or right side of road?
                        if(showPointObjects)
                        {
                            roadMarker.transform.position=currentPos.position+launchDir*roadDistance;
                        }

                        CreateHeightMapLine(currentPos.position,launchDir,mapDistance,mHeights);
                        CalculateHeightMapTrajectories(mHeights,roadDistance,mapDistance,targetTime);
                        float bestFitDiff=99999f;
                        int bestFitPos=-1;
                        float bestFitHeight=-1f;
                        for(int c=0;c<mHeights.trajectoryLens.Length;c++)
                        {
                            if(mHeights.trajectoryLens[c]>0)
                            {
                                float diff=targetDistance-mHeights.trajectoryLens[c];
                                if(Mathf.Abs(diff)<bestFitDiff)
                                {
                                    bestFitDiff=Mathf.Abs(diff);
                                    bestFitPos=c;
                                    bestFitHeight=mHeights.heightMap[c];
                                }
                            }
                        }
    //                    print("RD:"+roadDistance+" MD:"+mapDistance+" LD:"+launchDir+ "BFD:"+bestFitDiff);
                        if(bestFitPos!=-1)
                        {
                            Vector3 targetPos=currentPos.position+((float)bestFitPos)*mapDistance*launchDir*(1.0f/(float)mHeights.numPoints);
                            targetPos.y=bestFitHeight;
                            //print( bestFitPos+":"+targetPos+":"+bestFitHeight);
                            if(showPointObjects)
                            {
                                GameObject toPoint = new GameObject ();
                                toPoint.transform.position=targetPos;
                                toPoint.name = "To Point:"+travelPathPoints[travelPathSegment].gameObject.name+":"+mHeights.gravityMults[bestFitPos];
                            }
//                            print("GM:"+gravityMults[bestFitPos]);
                            currentTrajectory.Clear();
                            float mult = 1f/(float)99f;
                            CreateTrajectoryFromDescription(currentPos.position,targetPos,mHeights.upVelocities[bestFitPos],mHeights.gravityMults[bestFitPos],targetTime);    
                            
                            trajectoryIndex=0;
                            stretchMultiplier=1f;
                            retries=0;
                            break;
                        }else
                        {
                            // try slightly further next frame if we can't hit anything
                            stretchMultiplier+=.5f;
                            //print("Can't find new target"+currentPos.position);
                        }
                    }
                }
			}
		}
	}
    
    private void updateTravelPath(bool skip)
    {
        // get current travel path
        Transform tFrom=travelPathPoints[travelPathSegment];
        Transform tTo=travelPathPoints[travelPathSegment+1];
        targetDistanceMultiplier=tFrom.localScale.y;

        roadDirection=(tTo.position-tFrom.position).normalized;
        roadCentre=tFrom.position;
        jumpAngle=tFrom.localRotation.eulerAngles.y;
        roadWidth=tFrom.localScale.x;
        jumpGravity=9.8f*tFrom.localScale.z;
        // distance along road of tTo
        float distanceSegment = (tTo.position-tFrom.position).magnitude;
        // distance along road of currentPos
        float distanceCurrent = Vector3.Dot((currentPos.position-tFrom.position),roadDirection);
        if(skip)
        {
            print("Skip segment");
            currentPos.position=tTo.position;
            distanceCurrent=distanceSegment+1f;
        }
        if(tFrom.localScale.z==0 || distanceCurrent>distanceSegment    )
        {
            if(travelPathSegment<travelPathPoints.Length-3)
            {
                print ("next segment:"+tTo.gameObject.name);
                // go to next point
                travelPathSegment+=1;
                updateTravelPath(false);
            }else
            {
                // final point is just a zoom to end
                outroNext=true;
            }
        }                
        // if we are past the end, then we need to rotate onto next travel path and update
        // road centre and direction accordingly
    }

    // first we create a height map line for all directions we might fly in
    private void CreateHeightMapLine(Vector3 startPos,Vector3 direction,float distance,HeightMapDescriptor mapDesc)
    {
        float[] heights=mapDesc.heightMap;
        bool[] isTopFloor=mapDesc.isTopFloor;
        if(showCast)
        {
            foreach(Transform child in castParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        int numPoints=heights.Length;
		Vector3 toPos=startPos+direction.normalized*distance;
		RaycastHit hit;
        int goodPoints=0;
        Vector3 fromPoint=new Vector3(startPos.x,1000.0f,startPos.y);
        for(int c=0;c<numPoints;c++)
        {
            isTopFloor[c]=false;
            float ratio=((float)c)/((float)numPoints);
            Vector3 thisPos=Vector3.Lerp(startPos,toPos,ratio);
            fromPoint.x=thisPos.x;
            fromPoint.z=thisPos.z;
            if (Physics.Raycast (fromPoint, Vector3.down,  out hit,100001f)) {
                heights[c]=hit.point.y;
                goodPoints++;
                if(hit.transform.gameObject.name.IndexOf("Floor1")!=-1)
                {
                    isTopFloor[c]=true;
                }
                if(showCast)
                {
                    GameObject castObj=new GameObject();
                    castObj.name="cast"+c;
                    castObj.transform.position=hit.point;
                    castObj.transform.parent=castParent.transform;
                }
            }else
            {
                heights[c]=-1;
                if(showCast)
                {
                    Vector3 tempPos=fromPoint;
                    tempPos.y=-1;
                    GameObject castObj=new GameObject();
                    castObj.name="cast"+c;
                    castObj.transform.position=tempPos;
                    castObj.transform.parent=castParent.transform;
                }
            }
        }
        if(showPointObjects)
        {
            endMarker.transform.position=toPos;        
        }
        //print("GP:"+goodPoints+":"+startPos+":"+toPos+"["+numPoints+"]");
    }
    
    private void CreateFinalTrajectory(Vector3 startPos,Vector3 endPos,float targetTime,HeightMapDescriptor mapDesc)
    {
        Vector3 diff = endPos-startPos;
        float distance=diff.magnitude;
        // check if there's anything in the way
        CreateHeightMapLine(startPos,diff.normalized,distance,mapDesc);
        for(int c=0;c<mapDesc.numPoints;c++)
        {
            mapDesc.isTopFloor[c]=false;
        }
        mapDesc.heightMap[0]=startPos.y;
        mapDesc.heightMap[mapDesc.numPoints-1]=endPos.y;
        //print(endPos.y+":"+startPos.y);
        mapDesc.isTopFloor[mapDesc.numPoints-1]=true;
        CalculateHeightMapTrajectories(mapDesc,0,distance,targetTime);
        float finalScale=mapDesc.gravityMults[mapDesc.numPoints-1];
        float initialUpVelocity=mapDesc.upVelocities[mapDesc.numPoints-1];
        CreateTrajectoryFromDescription(startPos,endPos,initialUpVelocity,finalScale,targetTime);
        trajectoryIndex=0;
    }
    
    // calculate how close to jumpGravity each trajectory is (and what distance it is)
    private void CalculateHeightMapTrajectories(HeightMapDescriptor mapDesc, float roadDistance,float distance, float targetTime)
    {
        float[] heights=mapDesc.heightMap;
        float[] trajectoryLengths=mapDesc.trajectoryLens;
        float []gravityMultipliers=mapDesc.gravityMults;
        float[]upVelocities=mapDesc.upVelocities;
        bool[] isTopFloor=mapDesc.isTopFloor;
        float startRatio=roadDistance/distance;
        float heightZero=heights[0];
        int numPoints=heights.Length;
        if(heightZero==-1)
        {
            for(int c=0;c<numPoints;c++)
            {
                trajectoryLengths[c]=-1;
            }
            return;
        }

        int startPoint=(int)((float)numPoints * startRatio);
        for(int c=0;c<startPoint;c++)
        {
            trajectoryLengths[c]=-1;
        }

        // calculate closest to jumpGravity trajectory to this point
        // with the exception that we must go 10m(minJumpUp) higher than the final point, as
        // otherwise it looks bad that you don't drop in on it
        for(int c=startPoint;c<numPoints;c++)
        {
            // only land on top floor points
            if(heights[c]<0 || isTopFloor[c]==false)
            {
                // doesn't hit a building
                trajectoryLengths[c]=-1;
                gravityMultipliers[c]=-1;
                continue;
            }
            
            float ratio=((float)c)/((float)numPoints);
            float heightStart=heights[0];
            float heightEnd=heights[c];
            float distanceUp=heights[c]-heights[0];

            // dy = iV - g t 
            // y = iY + iV t - .5g t*t
            
            // t(yMax) = (iV/g = t ) 
            // yMax = iY + (iV*iV/g) - .5*g*(iV/g)*(iV/g) 
            // 
            
            // first calculate for correct jumpGravity trajectory
            // then check if it (a) goes high enough, (b) hits any points
            float initialUpVelocity = 0f;
            float topTime= 0f;
            float topPoint=heightStart;
            float finalScale=.9f;
            // shift the launch angle (and up the (fake)time until it jumps up enough
            if(travelPathSegment!=0)
            {
                while((topPoint<heightEnd+minJumpUp || topPoint<heightStart+minJumpUp)&& finalScale<500f)
                {
                    finalScale+=0.1f;
                    float scaledTime=targetTime*finalScale;
                    initialUpVelocity = (distanceUp + .5f*jumpGravity*scaledTime*scaledTime)/scaledTime;
                    topTime = initialUpVelocity/jumpGravity;
                    topPoint=heightStart + (initialUpVelocity*topTime) - .5f*jumpGravity * (topTime*topTime);
                }
            }
            // make sure it goes over buildings
            {
                float scaledTime=targetTime*finalScale;
                float stepTime=scaledTime/(float)c;
                for(int d=0;d<c;d++)
                {
                    bool checkHit=true;
                    while(checkHit)
                    {
                        float thisT=stepTime*(float)d;
                        float thisY=heightStart+ initialUpVelocity*thisT - .5f*jumpGravity*(thisT*thisT);
                        if(d!=0 && heights[d]>thisY && finalScale<500f) 
                        {
                            // make the scale such that this doesn't hit here
                            finalScale+=.1f;
                            scaledTime=targetTime*finalScale;
                            stepTime=scaledTime/(float)c;
                            initialUpVelocity = (distanceUp + .5f*jumpGravity*scaledTime*scaledTime)/scaledTime;
                        }else
                        {
                            checkHit=false;
                        }
                    }
                }
            }
            
            
            float trajectoryLength=0;
            {
                float scaledTime=targetTime*finalScale;
                float x=0;
                float y=heightStart;
                float dy=initialUpVelocity;            
                float stepMove=(ratio*distance)/(float)c;
                float stepTime=scaledTime/(float)c;
                // calculate distance of this trajectory            
                float oy=y;
                float ox=x;
                for(int d=0;d<c;d++)
                {
                    ox=x;
                    oy=y;
                    x+=stepMove;
                    dy-=stepTime*jumpGravity;
                    y+=stepTime*dy;
                    trajectoryLength+=Mathf.Sqrt((ox-x)*(ox-x)+(oy-y)*(oy-y));                                                                
                }            
            }
            
            trajectoryLengths[c]=trajectoryLength;
            gravityMultipliers[c]=finalScale;
            upVelocities[c]=initialUpVelocity;
/*            
            float upVelocity = initialUpVelocity;
            float y = heightZero;
            float stepMove=(ratio*distance)/(float)c;
            float stepTime=targetTime/(float)c;
            float baselineStep=distanceUp/(float)c;
            float baselineY=heightZero;
            float finalScale=1f;
            float scaledY=y;
            
            upVelocities[c]=initialUpVelocity;
            // calculate closest to natural jumpGravity trajectory to this point
            // scaling vertically if it will hit a building
            // also mess with the scale at the top point if it isn't high enough
            
            float lastVel=upVelocity;
            
            float maxY=y;
            float maxBaseline=0f;
            
            for(int d=0;d<c;d++)
            {
                y+=upVelocity*stepTime;
                upVelocity-=stepTime*jumpGravity;
                baselineY+=baselineStep;
                scaledY=(y-baselineY)*finalScale+baselineY;
                float compareHeight=heights[d];
                if(y>maxY)
                {
                    maxY=y;
                    maxBaseline=baselineY;
                }
                if(compareHeight!=-1 && y<compareHeight && y>baselineY)
                {
                    // need to scale us bigger or else we'll hit a building -i.e. breaks jumpGravity
                    float scaleNeeded=(compareHeight-baselineY)/(y-baselineY);
                    finalScale=Mathf.Max(scaleNeeded,finalScale);
                }
                lastVel=upVelocity;
            }
            // make path look nice (except for intro section, where we
            // don't want to mess with jumpGravity)
            if(travelPathSegment!=0)
            {
                // we want to drop down to final point always
                if(maxY< heightEnd+minJumpUp)
                {
                    float scaleNeeded=((minJumpUp+heightEnd)-maxBaseline)/(maxY-maxBaseline);
                    finalScale=Mathf.Max(scaleNeeded,finalScale);
                }
                // we want to go up from start always
                if(maxY< heightStart+minJumpUp)
                {
                    float scaleNeeded=((minJumpUp+heightStart)-maxBaseline)/(maxY-maxBaseline);
                    finalScale=Mathf.Max(scaleNeeded,finalScale);
                }
            }
            float x = 0;
            upVelocity = initialUpVelocity;
            y=heightZero;
            float trajectoryLength=0;
            scaledY=y;
            // measure distance of this trajectory
            for(int d=0;d<c;d++)
            {
                float ox=x;
                float oy=scaledY;
                x+=stepMove;
                y+=upVelocity*stepTime;
                upVelocity-=stepTime*jumpGravity;
                baselineY+=baselineStep;
                scaledY=(y-baselineY)*finalScale+baselineY;
                trajectoryLength+=Mathf.Sqrt((ox-x)*(ox-x)+(oy-scaledY)*(oy-scaledY));
            }
//            print (trajectoryLengths[c]+":"+finalScale+"sm:"+stepMove+":"+initialUpVelocity+" DI:"+distanceUp);
            */
        }
    }

    private void CreateTrajectoryFromDescription(Vector3 startPos,Vector3 endPos,float upVelocity,float finalScale,float targetTime)    
    {
        currentTrajectory.Clear();
        int numPoints=(int)(targetTime/dt);        
                
        float scaledTime=targetTime*finalScale;
        float stepRatio=1/(float)numPoints;
        float ratio=0f;
        float y = startPos.y;
        float maxY=y;

        float dy=upVelocity;
        
        float stepTime=scaledTime/(float)numPoints;
        Vector3 outPos=Vector3.zero;
        for(int c=0;c<numPoints;c++)
        {

            outPos=Vector3.Lerp(startPos,endPos,ratio);

            // sanity check building hits (and shuffle up in case of maths error
            Vector3 fromHitPoint=outPos;
            fromHitPoint.y=1000f;
            RaycastHit  hit;
            if (Physics.Raycast (fromHitPoint, Vector3.down,  out hit,100001f)) 
            {
                float buildingY=hit.point.y;
                
                if(buildingY-2f>y)
                {
                    // bad error - more than just a rounding error
                    print("Bad trajectory:"+hit.point+":"+y);
                }
                if(buildingY+0.01f>y)
                {
                    outPos.y=buildingY+0.01f;
                }
            }
            outPos.y=y;


            currentTrajectory.Add(outPos);
            ratio+=stepRatio;
            // calculate distance of this trajectory            
            dy-=stepTime*jumpGravity;
            y+=stepTime*dy;
            maxY=Mathf.Max(y,maxY);

            
        }
        currentTrajectory.Add(endPos);
    }


	private Vector3 FindFirstBuilding(out bool found){
        found=false;
		RaycastHit hit;
        Vector3 hitTest=new Vector3();
        for(float pos=0;pos<500f;pos+=0.2f)
        {
            hitTest.x=currentPos.position.x-pos*roadDirection.z;
            hitTest.z=currentPos.position.z+pos*roadDirection.x;
            hitTest.y=1000;
            if (Physics.Raycast (hitTest, Vector3.down, out hit,1000f)) 
            {
                found=true;
                //print ("Found an object - height: " + hit.distance);
                return hit.point;
            }
		}
        print("Couldn't find start object");
        return currentPos.position;
	}
}
