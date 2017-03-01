using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlecockCityCamMover : MonoBehaviour {

	private CityBuilder cb;
	private bool madeStart = false;

	List<Vector3> currentTrajectory=null;
	float trajectoryIndex=0;

	public Transform currentPos;
	public float gravity=9.81f;
    public float dt = 0.01f;

	// Use this for initialization
	void Start () {
		cb = GetComponent<CityBuilder> ();
		currentTrajectory = new List<Vector3> ();
	}
	
	// Update is called once per frame
	void Update () {
		while (!cb.buildingsExist) {
			return;
		}
		if (!madeStart) {
            Transform b = FindFirstBuilding ();
			if (b != null) {
                GameObject startPoint = new GameObject ();
				startPoint.transform.position = new Vector3 (b.position.x, b.position.y + b.localScale.y / 2f, b.position.z);
				startPoint.name = "Start Point";
				madeStart = true;
				currentPos.position = startPoint.transform.position;
			}
		} else {
			// if we've got trajectory points left, replay them
			if (currentTrajectory != null && currentTrajectory.Count > (int)trajectoryIndex) {
				currentPos.position = currentTrajectory [(int)trajectoryIndex];
				trajectoryIndex+=Time.deltaTime/dt;
			}else
			{
  				// otherwise launch again
				bool leftSide = (currentPos.position.x < 0);
                float leftRight=leftSide?1f:-1f;
				Vector3 launchDir = new Vector3 (leftRight*Mathf.Cos (15f * Mathf.Deg2Rad), 0,Mathf.Sin (15f * Mathf.Deg2Rad));
				CreateLaunchTrajectory(currentPos.position,launchDir,new Vector3(0,0,1),-2f*leftRight*(currentPos.position.x/Mathf.Cos(15f*Mathf.Deg2Rad)),1f);
			}
		}


	}

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
		RaycastHit hit;
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
        }
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

	private Transform FindFirstBuilding(){
		RaycastHit hit;
		if (Physics.Raycast (transform.position, Vector3.left, out hit)) {
			print ("Found an object - distance: " + hit.distance);
			return hit.collider.gameObject.transform;
		} else {
			print ("didn't find an object");
			return null;
		}
	}
}
