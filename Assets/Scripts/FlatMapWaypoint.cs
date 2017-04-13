using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlatMapWaypoint : MonoBehaviour 
{
    public Waypoint waypoint3D;

    public void OnClicked()
    {
        Debug.Log("OnClicked: " + gameObject.name);

        // TODO: don't teleport if we are already at target waypoint
        // could be tricky as the user can navigate arbitrarily
        // need Waypoint.current or something like that

        StartCoroutine(TeleportCamera());
    }

    public IEnumerator TeleportCamera()
    {
        if (waypoint3D != null)
        {
            yield return StartCoroutine(GameController.instance.fade.LowerCurtains());

            waypoint3D.TeleportCamera();

            yield return StartCoroutine(GameController.instance.fade.RaiseCurtains());
        }
    }
}
