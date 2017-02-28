using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchLogger : MonoBehaviour {

    static ResearchLogger sSingleton;
    public static ResearchLogger GetInstance()
    {
        return sSingleton;
    }

	// Use this for initialization
	void Start () {
        if(sSingleton==null)
        {
            DontDestroyOnLoad(transform.gameObject);
            sSingleton=this;
        }else
        {
            // only allow one of us to exist
            Destroy(gameObject);
        }		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void OnNewUser(string userCode)
    {
    }
    
    public void OnNewSwing(string swingCode)
    {
    }
    
}
