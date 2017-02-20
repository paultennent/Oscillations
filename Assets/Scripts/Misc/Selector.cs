using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.VR;

public class Selector : MonoBehaviour {


	public void Update()
	{
		PointerEventData pointer = new PointerEventData (EventSystem.current);
		pointer.position = new Vector2 (Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2);
		pointer.button = PointerEventData.InputButton.Left;

		List<RaycastResult> raycastResults = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (pointer, raycastResults);

		if (raycastResults.Count > 0) {
			if(Input.GetButtonUp("Tap")){
				SceneManager.LoadScene (raycastResults [0].gameObject.name);
			}
		}

	}

	public void openHighRoller(){
		SceneManager.LoadScene ("HighRoller");
	}

	public void openFloat(){
		SceneManager.LoadScene ("Float");
	}

	public void openWalker(){
		SceneManager.LoadScene ("Walker");
	}

	public void openJellyfish(){
		SceneManager.LoadScene ("Jellyfish");
	}

	public void openPachinko(){
		SceneManager.LoadScene ("Pachinko");
	}

	public void openShuttlecock(){
		SceneManager.LoadScene ("Shuttlecock");
	}
}
