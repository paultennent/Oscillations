using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlecockCamMover : AbstractGameEffects {

	public Transform point1;
	public Transform point2;

    int targetPoint=-1;
    
    public GameObject bat1;
    public GameObject bat2;
    
	public Transform pivot;
	public Transform shuttlecock;
    
    public float gravity=9.8f;

	private bool forward;
    int lastQuadrant=-1;

    public Vector3 velocity=new Vector3(0,0,0);
    
	// Use this for initialization
	void Start () {
		base.Start();
	}
	
    
	// Update is called once per frame
	void Update () {
		base.Update();

        // as swing phase switches, launch us towards the top
        // at a speed that will land us at T = swingCycleTime later
        
        if(swingQuadrant!=lastQuadrant && swingQuadrant==1)
        {
            launchAt(point1,point2);
            targetPoint=2;
            moveTargetPoint(point1,-3,-7);
        }else if(swingQuadrant!=lastQuadrant && swingQuadrant==3)
        {
            targetPoint=1;
            launchAt(point2,point1);
            moveTargetPoint(point2,3,7);
        }
        pivot.transform.position+=velocity*Time.deltaTime;;
        velocity=new Vector3(velocity.x,velocity.y-(gravity*Time.deltaTime),velocity.z);
        lastQuadrant=swingQuadrant;        
        if(targetPoint==1)
        {
            moveBat(bat1,point1);
        }
        if(targetPoint==2)
        {
            moveBat(bat2,point2);
        }
        pointShuttle();
	}
    
    void pointShuttle()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(new Vector3(0,-1,0),velocity);
        shuttlecock.rotation=targetRotation;
    }
    
    void moveBat(GameObject bat,Transform pt)
    {
        bat.transform.position=Vector3.MoveTowards(bat.transform.position,pt.position,Time.deltaTime*5f);
		
    }
    
    void moveTargetPoint(Transform pt,float minX,float maxX)
    {
        if( swingAmplitude!=0)
        {
            float amplitudeScaling = Mathf.Min(swingAmplitude/45f,1f);
            pt.position=new Vector3(Mathf.Lerp(minX,maxX,amplitudeScaling),pt.position.y,Random.Range(-4,4));
        }
    }
    
    void launchAt(Transform fromPoint,Transform toPoint)
    {
            // launch so that it will hit the other bat probably
            // i = initial up vel
            // g = gravity
            // t = time
            
            // v = i - gt
            // p = it - .5g *(t^2)
            // p[0] = 
            // i = .5gt
            pivot.transform.position=fromPoint.position;
            Vector3 displacement=toPoint.position-fromPoint.position;
            float distanceNeeded=displacement.magnitude;           
            float hitVelocity= distanceNeeded/(swingCycleTime/2f);
            //print(hitVelocity);
            velocity=displacement.normalized*hitVelocity;
            float timeNeeded = (distanceNeeded/hitVelocity);
            float upAmount = timeNeeded*(.5f*gravity);
            velocity=new Vector3(velocity.x,upAmount,velocity.z);
            
    }
		
}
