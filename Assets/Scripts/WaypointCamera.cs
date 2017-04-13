using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class WaypointCamera : MonoBehaviour
{
    Camera thisCamera;

    void Start()
    {
        thisCamera = gameObject.GetComponent<Camera>();

        // Turn off the waypoint cameras when not in editor - the cameras are only used to preview the waypoint view
        if (!Application.isEditor)
        {
            thisCamera.enabled = false;
        }

#if UNITY_EDITOR
		Transform parent = transform.parent;
		if (parent != null)
		{
			Waypoint waypoint = parent.GetComponent<Waypoint>();
			if (waypoint != null)
			{
				Vector3 pos = transform.position;
				pos.y = parent.position.y + waypoint.Height;
			}
		}
#endif
	}

#if UNITY_EDITOR
	void Update()
    {
        // When in editor, increase the depth of the selected waypoint camera so the game view previews it
        if (Application.isEditor)
        {
            if (Selection.activeTransform == transform)
            {
                thisCamera.depth = 10;
            }
            else
            {
                thisCamera.depth = 0;
            }

			Transform parent = transform.parent;
			if (parent != null)
			{
				Waypoint waypoint = parent.GetComponent<Waypoint>();
				if (waypoint != null)
				{
					Vector3 pos = transform.position;
					pos.y = parent.position.y + waypoint.Height;
					transform.position = pos;
				}
			}
		}

	}
#endif
}
