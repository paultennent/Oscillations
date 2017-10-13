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
    Transform worldRotation;
    
	// Use this for initialization
	void Start () {               
        base.Start();
        posOfCamera=GameObject.Find("ViewPoint").transform;
        worldRotation=GameObject.Find("RoomRotationAroundCamera").transform;
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
        
        // angle around an ellipse this W and H
        // note: we rotate the world so that this is always axis oriented
        float z=Mathf.Sin(ellipseAngleMult*swingAngle*Mathf.Deg2Rad)*ellipseW;
        float y=(ellipseH-Mathf.Cos(ellipseAngleMult*swingAngle*Mathf.Deg2Rad)*ellipseH)-centralCircleRadius;
        print(z+":"+y);

        posOfCamera.localPosition=new Vector3(posOfCamera.localPosition.x,y,z);
        
    }

}
