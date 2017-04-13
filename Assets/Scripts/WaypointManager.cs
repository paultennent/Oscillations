using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour 
{
    public AnimatedDoor[] animatedDoors;

    [System.Serializable]
    public class AdjacentWaypoints
    {
        public Waypoint waypoint;
        public List<Waypoint> adjacent;
    }

    // list of waypoints that are only enabled when user is on adjacent waypoint
    public List<AdjacentWaypoints> adjacentWaypoints;

    void Awake()
    {
        var camRotation = FindObjectOfType<CameraRotation>();
        camRotation.OnWaypointClicked += OnWaypointClicked;
    }

	void Start () 
    {
        
	}
	
	void Update () 
    {
	
	}

    void OnWaypointClicked(Waypoint waypoint)
    {
        foreach (var door in animatedDoors)
        {
            if (door.NeedsOpening(waypoint))
            {
                door.OpenDoor(this);
            }

            if (door.NeedsClosing(waypoint))
            {
                door.CloseDoor(this);
            }
        }
    
        foreach(var wp in adjacentWaypoints)
        {
            bool active = wp.waypoint == waypoint || wp.adjacent.Contains(waypoint);
            wp.waypoint.gameObject.SetActive(active);
        }
    }

}
