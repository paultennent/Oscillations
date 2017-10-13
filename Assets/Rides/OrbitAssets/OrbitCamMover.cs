using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamMover : AbstractGameEffects {

    public float centralCircleRadius=10f;

    float ellipseW=20f;
    float ellipseH=40f;
    
    Transform posOfCamera;
    Transform worldRotation;
    
	// Use this for initialization
	void Start () {               
        base.Start();
        posOfCamera=GameObject.Find("CameraMover").transform;
        worldRotation=GameObject.Find("WorldRotator").transform;
    }

    // Update is called once per frame
    void Update()
    {

        base.Update();

		if (!inSession) {
			return;
		}

        // if (!fadedIn)
        // {
            // FadeSphereScript.doFadeIn(5f, Color.black);
            // fadedIn = true;
        // }

        // if (!foundInitialPos)
        // {
            // foundInitialPos=true;
            // initialViewpointPos=viewpoint.transform.position;
        // }
        
        // angle around an ellipse this W and H
        // note: we rotate the world so that this is always axis oriented
       
        // float x=Mathf.Sin(swingAngle*Mathf.Deg2Rad)*ellipseH;
        // float y=(ellipseH-Mathf.Cos(swingAngle*Mathf.Deg2Rad)*ellipseH)-centralCircleRadius;
        // posOfCamera.localPosition.x=x;
        // posOfCamera.localPosition.y=y;
        
        
        
    }

}
