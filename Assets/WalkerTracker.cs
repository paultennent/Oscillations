using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerTracker : MonoBehaviour {

    public GameObject MainWalker;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(MainWalker.transform);
        if (Vector3.Distance(transform.position,MainWalker.transform.position) > 30f)
        {       
            transform.Translate(Vector3.forward * Time.deltaTime);
        }
	}
}
