using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour
{

    public float Height = 2.0f;

    public enum WaypointType { Start, Normal };
    public WaypointType Type = WaypointType.Normal;

    private Camera[] favourableCameras;
    private int favourableCameraIndex = -1;

    private bool cameraCaptured;

    public static Waypoint previousWaypoint = null;

    void Start()
    {
		favourableCameras = transform.GetComponentsInChildren<Camera>();
		foreach (Camera cam in favourableCameras)
		{
			cam.enabled = false;
		}

		if (Type == WaypointType.Start)
        {
            Vector3 pos = transform.position;
            pos.y += Height;
            Camera.main.transform.position = pos;

            CaptureCamera();
			NextCamera();
		}
    }

    void Update()
    {
        if (cameraCaptured)
        {
            if (GameController.instance.InputManager.GetVirtualKeyDown("c"))
            {
                NextCamera();
            }
        }
    }

    public void CaptureCamera()
    {
        if (previousWaypoint != null)
        {
            previousWaypoint.ReleaseCamera();
        }

        if (favourableCameraIndex != -1)
        {
            CameraRotation rot = Camera.main.GetComponent<CameraRotation>();
            if (rot != null)
            {
                rot.RotationTarget = favourableCameras[favourableCameraIndex].transform.rotation;
            }
        }

        CameraTranslation trans = Camera.main.GetComponent<CameraTranslation>();
        if (trans != null)
        {
            Vector3 pos = transform.position;
            pos.y += Height;
            trans.TranslationTarget = pos;
        }
		
		cameraCaptured = true;
        previousWaypoint = this;
    }

    public void TeleportCamera()
    {
        if (previousWaypoint != null)
        {
            previousWaypoint.ReleaseCamera();
        }

        CameraTranslation trans = Camera.main.GetComponent<CameraTranslation>();
        if (trans != null)
        {
            Vector3 pos = transform.position;
            pos.y += Height;
            trans.TranslationTarget = pos;
            trans.transform.position = pos;
        }

        if (favourableCameras.Length > 0)
        {
            CameraRotation rot = Camera.main.GetComponent<CameraRotation>();
            if (rot != null)
            {
                rot.RotationTarget = favourableCameras[0].transform.rotation;
                rot.transform.rotation = favourableCameras[0].transform.rotation;
                favourableCameraIndex = 0;
            }
        }

        cameraCaptured = true;
        previousWaypoint = this;
    }

    public void ReleaseCamera()
    {
        favourableCameraIndex = -1;
        cameraCaptured = false;
    }

    public void NextCamera()
    {
        // Early out if no favourable cameras configured
        if (favourableCameras.Length == 0)
        {
            return;
        }

        int newIndex = (favourableCameraIndex + 1) % favourableCameras.Length;
        if (0 <= newIndex && newIndex < favourableCameras.Length)
        {
            favourableCameraIndex = newIndex;

            CameraRotation rot = Camera.main.GetComponent<CameraRotation>();
            if (rot != null)
            {
                rot.RotationTarget = favourableCameras[favourableCameraIndex].transform.rotation;
            }
        }
    }
}
