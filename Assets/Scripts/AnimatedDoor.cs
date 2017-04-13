using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimatedDoor
{
    public Transform door;
    public List<Waypoint> waypoints;
    public bool isOpen;

    public bool NeedsOpening(Waypoint waypoint)
    {
        return !isOpen && waypoints.Contains(waypoint);
    }

    public bool NeedsClosing(Waypoint waypoint)
    {
        return isOpen && !waypoints.Contains(waypoint);
    }

    public void OpenDoor(MonoBehaviour obj)
    {
        obj.StartCoroutine(OpenDoor(door, true, 100.0f, 100.0f));
    }

    public void CloseDoor(MonoBehaviour obj)
    {
        obj.StartCoroutine(OpenDoor(door, false, -100.0f, 100.0f));
    }

    IEnumerator OpenDoor(Transform door, bool open, float angle, float speed)
    {
        var targetRot = door.rotation * Quaternion.Euler(0.0f, angle, 0.0f);

        bool done = false;
        while (!done)
        {
            float step = speed * Time.deltaTime;
            door.rotation = Quaternion.RotateTowards(door.rotation, targetRot, step);
            done = Quaternion.Angle(door.rotation, targetRot) < 0.01f;
            yield return null;
        }

        isOpen = open;
    }
}
