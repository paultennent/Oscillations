using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlecockCityCamMover : AbstractGameEffects {

	private CityBuilder cb;
	private bool madeStart = false;

	List<Vector3> currentTrajectory=null;
	float trajectoryIndex=0;

	public Transform currentPos;
	public float gravity=9.81f;
    public float dt = 0.01f;
    public bool showCast=false;
    public bool showPointObjects=false;
    
    const int MAP_POINTS=100;
    float []heightMap;
    float []trajectoryLens;
    float []gravityMults;
    float []upVelocities;
    
    private GameObject endMarker;
    private GameObject roadMarker;
    private GameObject castParent;

    public Vector3 roadCentre=new Vector3(0,0,0);
    public Vector3 roadDirection=new Vector3(0,0,1);
    public float jumpAngle=15f;
    public float roadWidth=5f;
    public float targetDistance=40f;
    public float targetTime=1f;

    private float targetEndTime=-1;
    
    private int lastQuadrant=-1;
    
	// Use this for initialization
	void Start () {
        base.Start();
        endMarker=new GameObject();
        endMarker.name="endmarker";
        roadMarker=new GameObject();
        roadMarker.name="roadmarker";
        castParent=new GameObject();
        castParent.name="castLine";
		cb = GetComponent<CityBuilder> ();
		currentTrajectory = new List<Vector3> ();
        heightMap=new float[MAP_POINTS];
        trajectoryLens=new float[MAP_POINTS];
        gravityMults=new float[MAP_POINTS];
        upVelocities=new float[MAP_POINTS];
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();
		while (!cb.buildingsExist) {
			return;
		}
        if(swingQuadrant!=lastQuadrant && (swingQuadrant==0 || swingQuadrant==2))
        {
            // this is a point where we should launch 
            // set target end time to be this time + swingcycletime/2
            targetEndTime=Time.time + swingCycleTime/2f;
            print("swing");
        }
        lastQuadrant=swingQuadrant;
        
		if (!madeStart) {
            Vector3 b = FindFirstBuilding (out madeStart);
			if (madeStart) {
                if(showPointObjects)
                {
                    GameObject startPoint = new GameObject ();
                    startPoint.transform.position = b;
                    startPoint.name = "Start Point";
                }
				currentPos.position = b;
			}
		} else {
			// if we've got trajectory points left, replay them
			if (currentTrajectory != null && currentTrajectory.Count > (int)trajectoryIndex) {
				currentPos.position = currentTrajectory [(int)trajectoryIndex];
				trajectoryIndex+=Time.deltaTime/dt;
			}else
			{
                if(currentTrajectory.Count>0)
                {
                    // make sure we're calculating from end of last trajectory 
                    currentPos.position= currentTrajectory[currentTrajectory.Count-1];
                }
                // set target time if 
                if(targetEndTime>0 && targetEndTime>Time.time)
                {
                    targetTime = targetEndTime- Time.time;
                }
                for(float angleOffset=0;angleOffset<45;angleOffset+=1f)
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
                    float minLaunchDistance=1.1f* roadDistance;
                    float mapDistance=Mathf.Max(targetDistance,minLaunchDistance);

                    // are we on left or right side of road?
                    if(showPointObjects)
                    {
                        roadMarker.transform.position=currentPos.position+launchDir*roadDistance;
                    }

                    CreateHeightMapLine(currentPos.position,launchDir,mapDistance,heightMap);
                    CalculateHeightMapTrajectories(heightMap,roadDistance,mapDistance,targetTime,trajectoryLens,gravityMults,upVelocities);
                    float bestFitDiff=99999f;
                    int bestFitPos=-1;
                    float bestFitHeight=-1f;
                    for(int c=0;c<trajectoryLens.Length;c++)
                    {
                        if(trajectoryLens[c]>0)
                        {
                            float diff=targetDistance-trajectoryLens[c];
                            if(Mathf.Abs(diff)<bestFitDiff)
                            {
                                bestFitDiff=Mathf.Abs(diff);
                                bestFitPos=c;
                                bestFitHeight=heightMap[c];
                            }
                        }
                    }
//                    print("RD:"+roadDistance+" MD:"+mapDistance+" LD:"+launchDir+ "BFD:"+bestFitDiff);
                    if(bestFitPos!=-1)
                    {
                        Vector3 targetPos=currentPos.position+((float)bestFitPos)*mapDistance*launchDir*(1.0f/(float)trajectoryLens.Length);
                        targetPos.y=bestFitHeight;
                        //print( bestFitPos+":"+targetPos+":"+bestFitHeight);
                        if(showPointObjects)
                        {
                            GameObject toPoint = new GameObject ();
                            toPoint.transform.position=targetPos;
                            toPoint.name = "To Point";
                        }
                        currentTrajectory.Clear();
                        float mult = 1f/(float)99f;
                        CreateTrajectoryFromDescription(currentPos.position,targetPos,upVelocities[bestFitPos],gravityMults[bestFitPos],targetTime);    
                        
//                        for(int c=0;c<100;c++)
 //                       {
  //                          currentTrajectory.Add(Vector3.Lerp(currentPos.position,targetPos,mult*(float)c));
   //                     }
                        trajectoryIndex=0;
                        break;
                    }else
                    {
                        //print("Can't find new target");
                    }
                }
//				CreateLaunchTrajectory(currentPos.position,launchDir,new Vector3(0,0,1),-2f*leftRight*(currentPos.position.x/Mathf.Cos(15f*Mathf.Deg2Rad)),1f);
			}
		}
	}

    // first we create a height map line for all directions we might fly in
    private void CreateHeightMapLine(Vector3 startPos,Vector3 direction,float distance,float[] heights)
    {
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
        for(int c=0;c<numPoints;c++)
        {
            float ratio=((float)c)/((float)numPoints);
            Vector3 thisPos=Vector3.Lerp(startPos,toPos,ratio);
            if (Physics.Raycast (new Vector3(thisPos.x,1000,thisPos.z), Vector3.down,  out hit,100001f)) {
                heights[c]=hit.point.y;
                goodPoints++;
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
                    GameObject castObj=new GameObject();
                    castObj.name="cast"+c;
                    castObj.transform.position=thisPos;
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
    
    // calculate how close to gravity each trajectory is (and what distance it is)
    private void CalculateHeightMapTrajectories(float[] heights, float roadDistance,float distance, float targetTime, float[] trajectoryLengths,float []gravityMultipliers,float[]upVelocities)
    {
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
        // calculate closest to gravity trajectory to this point
        for(int c=startPoint;c<numPoints;c++)
        {
            if(heights[c]<=0)
            {
                // doesn't hit a building
                trajectoryLengths[c]=-1;
                gravityMultipliers[c]=-1;
                continue;
            }
            float ratio=((float)c)/((float)numPoints);
            float heightEnd=heights[c];
            float distanceUp=heights[c]-heights[0];
            // first calculate pure gravity trajectory
            // then check if it needs scaling
            float initialUpVelocity = (distanceUp + .5f*gravity*targetTime*targetTime)/targetTime;        
            float upVelocity = initialUpVelocity;
            float y = heightZero;
            float stepMove=(ratio*distance)/(float)c;
            float stepTime=targetTime/(float)c;
            float baselineStep=distanceUp/(float)c;
            float baselineY=heightZero;
            float finalScale=1f;
            float scaledY=y;
            upVelocities[c]=initialUpVelocity;
            // calculate closest to natural gravity trajectory to this point
            // scaling vertically if it will hit a building
            for(int d=0;d<c;d++)
            {
                y+=upVelocity*stepTime;
                upVelocity-=stepTime*gravity;
                baselineY+=baselineStep;
                scaledY=(y-baselineY)*finalScale+baselineY;
                if(y<heights[d] && y>baselineY)
                {
                    // need to scale us bigger or else we'll hit a building -i.e. breaks gravity
                    float scaleNeeded=(heights[d]-baselineY)/(y-baselineY);
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
                upVelocity-=stepTime*gravity;
                baselineY+=baselineStep;
                scaledY=(y-baselineY)*finalScale+baselineY;
                trajectoryLength+=Mathf.Sqrt((ox-x)*(ox-x)+(oy-scaledY)*(oy-scaledY));
            }
//            print (trajectoryLengths[c]+":"+finalScale+"sm:"+stepMove+":"+initialUpVelocity+" DI:"+distanceUp);
            trajectoryLengths[c]=trajectoryLength;
            gravityMultipliers[c]=finalScale;
        }
    }

    private void CreateTrajectoryFromDescription(Vector3 startPos,Vector3 endPos,float upVelocity,float finalScale,float targetTime)    
    {
        currentTrajectory.Clear();
        int numPoints=(int)(targetTime/dt);        
        float y = startPos.y;
        float scaledY=y;
        float stepTime=targetTime/(float)numPoints;
        float distanceUp=endPos.y-startPos.y;
        float baselineStep=distanceUp/(float)numPoints;
        float baselineY=y;
        for(int c=0;c<numPoints;c++)
        {
            y+=upVelocity*stepTime;
            upVelocity-=stepTime*gravity;
            baselineY+=baselineStep;
            scaledY=(y-baselineY)*finalScale+baselineY;
            float ratio=((float)c)/((float)numPoints);
            Vector3 outPos=Vector3.Lerp(startPos,endPos,ratio);
            outPos.y=scaledY;
            currentTrajectory.Add(outPos);
        }
        currentTrajectory.Add(endPos);
    }

    
    // create a trajectory between two known points, we use this once we have foud good points (above)
	private void CreateLaunchTrajectory(Vector3 startPos,Vector3 direction,Vector3 roadDirection,float distance,float targetTime)
	{
		// create a parabola 60 points per second that gets us to the end position

		// first create it using correct gravity 

		// i = initial up vel
		// g = gravity
		// t = time

		// v = i - gt
		// p = it - .5g *(t^2)
		// p[0] = 
		// i = .5gt
		Vector3 toPos=startPos+direction.normalized*distance;
/*		RaycastHit hit;
		if (Physics.Raycast (new Vector3(toPos.x,1000,toPos.z), Vector3.down,  out hit,1000f)) {
			toPos = hit.point;

		}else
        {
//            print("missed");
            // find next building in this direction
            if(Physics.Raycast (new Vector3(toPos.x,1,toPos.z), roadDirection,out hit,1000f))
            {
 //               print("found next building");
                // move onto the building by 10% then raycast down
                toPos=hit.point + direction.normalized*0.1f*gameObject.transform.localScale.x;
                if (Physics.Raycast (toPos + new Vector3 (0, 1000, 0), Vector3.down,  out hit,1000f))
                {
                    toPos=hit.point;
  //                  print("found second chance");
                }
            }
        }*/
        GameObject toPoint = new GameObject ();
        toPoint.transform.position=toPos;
        toPoint.name = "To Point";

		Vector3 displacement=toPos-startPos;
        
		float distanceFlat=Mathf.Sqrt(displacement.x*displacement.x + displacement.z*displacement.z);
		float flatVelocity= distanceFlat/targetTime;
        
        
        float distanceUp = displacement.y;
        
        
        // dy = v1 - g*t
        // y(t) = v1*t - .5* g* t*t
        //
        // v1 = (y(t) + 0.5 * g * t*t)/t 
        
        //(y+ .5* g*t*t)/t = v1
        //print(distanceUp);
        
        float upAmount = (distanceUp + .5f*gravity*targetTime*targetTime)/targetTime;        
		Vector3 velocity=new Vector3(flatVelocity*displacement.x/distanceFlat,upAmount,flatVelocity*displacement.z/distanceFlat);

        //print(velocity);
        
		// calculate the initial trajectory
		Vector3 gravityForce=new Vector3(0,-gravity,0);
		Vector3 posNow=startPos;
		currentTrajectory.Clear ();
		for (float t = 0; t < targetTime; t += dt) {
			currentTrajectory.Add (posNow);
			posNow += velocity*dt;
			velocity += gravityForce * dt;
		}
        
        
		//print (startPos+":"+direction+":"+posNow+":"+toPos);
        
        // check if we hit any buildings on this trajectory
        float scaling=1f;
        float mult = 1f/(float)currentTrajectory.Count;
        for( int t=1;t<currentTrajectory.Count-1;t++)            
        {
            float ratio=mult*(float)t;
            Vector3 thisPoint=currentTrajectory[t];
            Vector3 scaleBasePoint = Vector3.Lerp(startPos,toPos,ratio);            
            Vector3 testPoint= thisPoint+Vector3.up*1000f;
            RaycastHit hit;
            if (Physics.Raycast (testPoint , Vector3.down,  out hit,1000f))
            {
                float amountUnder=1000f- hit.distance;
                float distUp = thisPoint.y-scaleBasePoint.y;
                float distUpNeeded = distUp+amountUnder;
                //print(distUpNeeded+":"+(distUpNeeded/distUp)+":"+scaling);
                scaling=Mathf.Max(scaling,(distUpNeeded/distUp));
            }            
        }
        if(scaling>1f)
        {
//            print("Avoiding hit:"+scaling);
            for( int t=1;t<currentTrajectory.Count-1;t++)            
            {
                Vector3 thisPoint=currentTrajectory[t];
                float ratio=mult*(float)t;
                Vector3 scaleBasePoint = Vector3.Lerp(startPos,toPos,ratio);            
                currentTrajectory[t]=new Vector3(thisPoint.x,scaling * (thisPoint.y-scaleBasePoint.y) + scaleBasePoint.y,thisPoint.z);
            }
        }
		trajectoryIndex = 0;
	}

	private Vector3 FindFirstBuilding(out bool found){
        found=false;
		RaycastHit hit;
        Vector3 hitTest=new Vector3();
        for(float pos=0;pos<500f;pos+=0.2f)
        {
            hitTest.x=transform.position.x-pos;
            hitTest.z=transform.position.z;
            hitTest.y=1000;
            if (Physics.Raycast (hitTest, Vector3.down, out hit,1000f)) 
            {
                found=true;
                print ("Found an object - height: " + hit.distance);
                return hit.point;
            }
		}
        print("Couldn't find start object");
        return transform.position;
	}
}
