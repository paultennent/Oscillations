using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.VR;
using UnityEngine.UI;

public class Selector : MonoBehaviour {

    public GameObject crosshair;
    public Text loadText;
    Vector3[] corners=new Vector3[4];

    
	public void Update()
	{

        RectTransform rt=(RectTransform)crosshair.transform;
        rt.GetWorldCorners(corners);

        Vector3 screenPoint=Camera.allCameras[0].WorldToScreenPoint(corners[0]);
        
		PointerEventData pointer = new PointerEventData (EventSystem.current);
		pointer.position = new Vector2 (screenPoint.x, screenPoint.y);
		pointer.button = PointerEventData.InputButton.Left;

#if UNITY_EDITOR
        pointer.position=Input.mousePosition;
#endif
        
        
		List<RaycastResult> raycastResults = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (pointer, raycastResults);
		if (raycastResults.Count > 0) {
			EventSystem.current.SetSelectedGameObject (raycastResults[0].gameObject);
            if(raycastResults[0].gameObject.GetComponent<Button>()!=null)
            {
                print (raycastResults [0].gameObject.name);
                if (Input.GetButtonUp ("Tap")) {
                    string sceneName=raycastResults [0].gameObject.name;
                    showLoadingGraphic(sceneName);
                    SceneManager.LoadScene (sceneName);
                }
            }
		} else {
			EventSystem.current.SetSelectedGameObject (null);
		}

	}

    public static bool openSceneByNumber(int number)
    {
        string sceneName=null;
        switch(number)
        {
            case 0:
                sceneName="Jellyfish-tiled";
                break;
            case 2:
                sceneName="HighRoller-City-tiled";
                break;
            case 3:
                sceneName="Walker-city";
                break;
            case 4:
                sceneName="Shuttlecock-City-Static";
                break;
            default:
                return false;
        }
        print("Auto scene:"+number+":"+sceneName);
        
        
        showLoadingGraphic(sceneName);
        SceneManager.LoadScene(sceneName);
        return true;
    }
    
    public static void showLoadingGraphic(string sceneName)
    {
        GameObject.Find("LoadingScreenParent").transform.GetChild(0).gameObject.SetActive(true);
    }
    
	public void openHighRoller(){
		SceneManager.LoadScene ("HighRoller-City-tiled");
	}

	public void openFloat(){
		SceneManager.LoadScene ("Float");
	}

	public void openWalker(){
		SceneManager.LoadScene ("Walker-city");
	}

	public void openJellyfish(){
		SceneManager.LoadScene ("Jellyfish-tiled");
	}

	public void openPachinko(){
		SceneManager.LoadScene ("Pachinko");
	}

	public void openShuttlecock(){
		SceneManager.LoadScene ("Shuttlecock-City-Static");
	}
}
