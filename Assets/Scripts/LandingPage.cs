using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LandingPage : MonoBehaviour 
{

	void Start () 
    {
	    
	}

    void Update()
    {
        if (GameController.instance.isClick)
        {
            GameController.instance.LoadLevel(GameController.GameMode.Flat4);
        }
    }
}
