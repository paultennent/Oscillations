using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamMover : AbstractGameEffects {

    public float centralCircleRadius=10f;

    public bool fadedIn=false;
    
    float ellipseW=20f;
    float ellipseH=40f;
    float ellipseAngleMult=2f;
    
    Transform posOfCamera;
    Transform ellipseRotation;
    
    float lastEllipseZ,lastEllipseY;
    float lastEllipseSwingAngle;
    
    bool onTangent=false;
    float tangentDistance=0f;
    Quaternion tangentUp;
    
	// Use this for initialization
	void Start () {               
        base.Start();
        posOfCamera=GameObject.Find("ViewPoint").transform;
        ellipseRotation=GameObject.Find("EllipseBasis").transform;
    }

    // Update is called once per frame
    void Update()
    {

        base.Update();

		if (!inSession) {
			return;
		}

        if (!fadedIn)
        {
            FadeSphereScript.doFadeIn(5f, Color.black);
            fadedIn = true;
        }

        // if (!foundInitialPos)
        // {
            // foundInitialPos=true;
            // initialViewpointPos=viewpoint.transform.position;
        // }
        
//        print(swingQuadrant);
        // angle around an ellipse this W and H
        // note: we rotate the world so that this is always axis oriented
        if(swingQuadrant==1 && swingAngle>0 && lastEllipseSwingAngle>10f)
        {
            if(!onTangent)
            {
                onTangent=true;
                // on the fall back quadrant, we fall in a straight line on the tangent to the central circle
                // find distance to centre of circle (hypotenuse of triangle)
                float distanceToCentreSq= lastEllipseY*lastEllipseY + lastEllipseZ*lastEllipseZ;
                float distanceToCentre = Mathf.Sqrt(distanceToCentreSq);
                // right angled triangle, adjacent side = circle radius
                // so: cos(angle)= adjacent/hypotenuse
                float adjacentAngle = Mathf.Acos(centralCircleRadius/distanceToCentre);
                // direction to centre from point
                float directionToCentre=Mathf.Atan2(lastEllipseY,lastEllipseZ);
                // add on the adjacentAngle to get the angle of the tangent in the global coord system 
                float tangentAngle = directionToCentre-adjacentAngle;
                print(Mathf.Rad2Deg*tangentAngle);
                // rotate ellipse coordinates so that our current point is (-centralCircleRadius,tangentDistance)
                Vector3 savePos=posOfCamera.position;                
                Quaternion saveRotation=posOfCamera.rotation;
                ellipseRotation.transform.localRotation*=Quaternion.Euler(270-(tangentAngle*Mathf.Rad2Deg),0,0);
                posOfCamera.position=savePos;
                posOfCamera.rotation=saveRotation;
                tangentUp=posOfCamera.localRotation;
                print(posOfCamera.localPosition);
                // right angled triangle - shorter side = circle radius, so last side length by pythagoras
                tangentDistance= Mathf.Sqrt(distanceToCentre*distanceToCentre - centralCircleRadius*centralCircleRadius);
            }
            float ratio = swingAngle / lastEllipseSwingAngle;
            Vector3 fromPos=new Vector3(posOfCamera.localPosition.x,-centralCircleRadius,tangentDistance);
            Vector3 toPos= new Vector3(posOfCamera.localPosition.x,-centralCircleRadius,0);
            posOfCamera.localPosition=Vector3.Lerp(toPos,fromPos,ratio);
            posOfCamera.localRotation=Quaternion.Slerp(Quaternion.identity,tangentUp,ratio);
   //         print("!"+posOfCamera.localPosition.z+":"+posOfCamera.localPosition.y);
            
        }else
        {
            onTangent=false;
            posOfCamera.localRotation=Quaternion.identity;
            float z=Mathf.Sin(ellipseAngleMult*swingAngle*Mathf.Deg2Rad)*ellipseW;
            float y=(ellipseH-Mathf.Cos(ellipseAngleMult*swingAngle*Mathf.Deg2Rad)*ellipseH)-centralCircleRadius;
//            float rotatedZ= Mathf.Cos(ellipseRotation)*z + Mathf.Sin(ellipseRotation)*y ;
//            float rotatedY= Mathf.Cos(ellipseRotation)*y - Mathf.Sin(ellipseRotation)*z ;
            
            lastEllipseZ=z;
            lastEllipseY=y;
            lastEllipseSwingAngle=swingAngle;
 //           print(z+":"+y);
            posOfCamera.localPosition=new Vector3(posOfCamera.localPosition.x,y,z);            
        }

        
    }

}
