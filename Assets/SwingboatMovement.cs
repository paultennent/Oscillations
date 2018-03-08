using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingboatMovement : AbstractGameEffects {

    bool fadedIn=false;

	void Start () {
        base.Start();
	}
	
	void Update () {
        base.Update();
        
		if (!inSession) {
            fadedIn=false;
			return;
		}
		if (!fadedIn)
		{
			FadeSphereScript.doFadeIn(5f, Color.black);
			fadedIn = true;
		}

		
	}
}
