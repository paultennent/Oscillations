using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class FlatMapController : MonoBehaviour 
{
    public GameObject pin;

	void Start () 
    {
        
	}
	
	void LateUpdate () 
    {
        foreach (var hit in FlatController.instance.results)
        {
            var mapWaypoint = hit.gameObject.GetComponent<FlatMapWaypoint>();
            if (mapWaypoint != null)
            {
                mapWaypoint.OnClicked();

                if (pin != null)
                    pin.transform.position = mapWaypoint.transform.position;
            }
        }
	}
}
