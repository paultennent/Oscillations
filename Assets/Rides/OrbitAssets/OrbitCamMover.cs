using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamMover : AbstractGameEffects {

    float ellipseW=20f;
    float ellipseH=20f;
    
	// Use this for initialization
	void Start () {
        base.Start();
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

        if (!foundInitialPos)
        {
            foundInitialPos=true;
            initialViewpointPos=viewpoint.transform.position;
        }
        
        swingAngle;

        
        
    }

}
