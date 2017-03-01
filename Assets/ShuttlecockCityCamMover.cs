using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlecockCityCamMover : MonoBehaviour {

	private CityBuilder cb;
	private bool madeStart = false;

	List<Vector3> currentTrajectory=null;
	int trajectoryIndex=0;

	public Transform currentPos;
	public float gravity=9.81f;

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
			GameObject startPoint = new GameObject ();
			Transform b = FindFirstBuilding ();
			if (b != null) {
				startPoint.transform.position = new Vector3 (b.position.x, b.position.y + b.localScale.y / 2f, b.position.z);
				startPoint.name = "Start Point";
				madeStart = true;
				currentPos.position = startPoint.transform.position;
			}
		} else {
			// if we've got trajectory points left, replay them
			if (currentTrajectory != null && currentTrajectory.Count > trajectoryIndex) {
				currentPos.position = currentTrajectory [trajectoryIndex];
				trajectoryIndex++;
			}else
			{
				bool leftSide = (currentPos.position.x < 0);
				Vector3 launchDir = new Vector3 (0, (leftSide?1:-1)*Mathf.Cos (15f * Mathf.Deg2Rad), Mathf.Sin (15f * Mathf.Deg2Rad));
  				// otherwise launch again
				CreateLaunchTrajectory(currentPos.position,launchDir,2f*(leftSide?1:-1)*currentPos.position.x*Mathf.Cos(15f*Mathf.Deg2Rad),1f);
			}
		}


	}

	private void CreateLaunchTrajectory(Vector3 startPos,Vector3 direction,float distance,float targetTime)
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
		if (Physics.Raycast (toPos + new Vector3 (0, 1000, 0), Vector3.down, out hit)) {
			toPos = hit.point;

		}// fixme - if this misses, we're in a gap, needs fixing

		Vector3 displacement=toPos-startPos;
		float distanceNeeded=displacement.magnitude;           
		float hitVelocity= distanceNeeded/targetTime;
		//print(hitVelocity);
		Vector3 velocity=displacement.normalized*hitVelocity;
		float timeNeeded = (distanceNeeded/hitVelocity);
		float upAmount = timeNeeded*(.5f*gravity);
		velocity=new Vector3(velocity.x,upAmount,velocity.z);

		// velocity = direction to shoot off in with normal gravity

		// calculate the initial trajectory
		Vector3 gravityForce=new Vector3(0,-gravity,0);
		Vector3 posNow=startPos;
		currentTrajectory.Clear ();
		currentTrajectory.Add (posNow);
		float dt = 0.01f;
		for (float t = 0; t < targetTime; t += dt) {
			posNow += velocity*dt;
			velocity += gravityForce * dt;
			currentTrajectory.Add (posNow);
		}
		print (posNow+":"+toPos);
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
