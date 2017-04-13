using System;
using UnityEngine;
using System.Collections;

public class CameraRotation : MonoBehaviour
{

    public float rotationSpeed = 10;
    public float dampingTime = 0.2f;
    public Vector2 rotationRange = new Vector3(70, float.MaxValue);

    private bool blendToRotationTarget = false;
    private Quaternion rotationTarget;
    public Quaternion RotationTarget { set { rotationTarget = value; blendToRotationTarget = true; } }


    private rcInputManager inputManager;
    private rcInputManager.Stick stick;

    private Vector2 prevPos = Vector2.zero;

    private Vector3 targetAngles;
    private Vector3 followAngles;
    private Vector3 followVelocity;
    //private Quaternion m_OriginalRotation;

    private float targetXAngleVelocity = 0.0f;
    private float targetYAngleVelocity = 0.0f;
    private float targetZoomVelocity = 0.0f;
    //float targetZAngleVelocity = 0.0f;

    public static CameraRotation instance;

    public delegate void WaypointClicked(Waypoint waypoint);
    public WaypointClicked OnWaypointClicked;

    public float defaultFOV = 55.0f;

    private bool inited;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        if (GameController.instance != null)
        {
            inputManager = GameController.instance.InputManager;
            stick = GameController.instance.Stick;
        }
        else
        {
            inputManager = gameObject.AddComponent<rcInputManager>();
            stick = inputManager.RegStick();
            stick.EnableTouch();
        }

        targetAngles = transform.eulerAngles;
        followAngles = targetAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if(!inited)
        {
            // Lazily notify any registered delegates in Update to avoid Awake/Start order dependencies
            if (OnWaypointClicked != null)
                OnWaypointClicked(Waypoint.previousWaypoint);

            inited = true;
        }

        if (GameController.instance == null)
        {
            float dt = Time.deltaTime;
            inputManager.UpdateManual(dt, dt);
        }

        float touchH = 0.0f;
        float touchV = 0.0f;

        rcInputManager.RawTouchStream touches = inputManager.RawTouches;
        if (touches.touches_.Count > 0)
        {
            rcInputManager.RawTouchStream.Touch touch = touches.touches_[0];

            if (prevPos == Vector2.zero)
            {
                prevPos = touch.normPosOnDown;
            }

            Vector2 currentPos = touch.normPos;
            Vector2 delta = currentPos - prevPos;

            touchH = delta.x;
            touchV = delta.y;

            prevPos = currentPos;

            if (touch.state == rcInputManager.RawTouchStream.Touch.eState.Up)
            {
                if (Time.realtimeSinceStartup - touch.timeOnDown < 0.2f)
                {
                    // Do a cheeky raycast
                    Vector3 screenPoint = new Vector3(currentPos.x, currentPos.y, 0.0f);
                    Ray ray = Camera.main.ViewportPointToRay(screenPoint);

                    LayerMask maskWaypoint = LayerMask.NameToLayer("Waypoints");
					LayerMask maskDefault = LayerMask.NameToLayer("Default");

					RaycastHit hitInfo;
                    bool hit = Physics.Raycast(ray, out hitInfo, 100.0f, 1 << maskWaypoint.value | 1 << maskDefault, QueryTriggerInteraction.UseGlobal);
                    if (hit)
                    {
                        Waypoint wp = hitInfo.transform.gameObject.GetComponent<Waypoint>();
                        if (wp != null)
                        {
                            wp.CaptureCamera();

                            if (OnWaypointClicked != null)
                                OnWaypointClicked(wp);
                        }
                    }
                }
            }
        }
        else
        {
            prevPos = Vector3.zero;
        }

        // Read input from mouse
        float inputH = -touchH;
        float inputV = touchV;

        // Wrap values to avoid springing quickly the wrong way from positive to negative
        if (targetAngles.y > 180)
        {
            targetAngles.y -= 360;
            followAngles.y -= 360;
        }
        if (targetAngles.x > 180)
        {
            targetAngles.x -= 360;
            followAngles.x -= 360;
        }
        if (targetAngles.y < -180)
        {
            targetAngles.y += 360;
            followAngles.y += 360;
        }
        if (targetAngles.x < -180)
        {
            targetAngles.x += 360;
            followAngles.x += 360;
        }

        // Use favourable angle or angle from input
        if (blendToRotationTarget)
        {
            targetAngles.x = rotationTarget.eulerAngles.x;
            targetAngles.y = rotationTarget.eulerAngles.y;
        }
        else
        {
            targetAngles.x += (inputV * rotationSpeed * 0.2f) + inputManager.virtualGyro.x;
            targetAngles.y += (inputH * rotationSpeed) + inputManager.virtualGyro.y;
        }

        // Clamp values to allowed range
        targetAngles.x = Mathf.Clamp(targetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);
        targetAngles.y = Mathf.Clamp(targetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f);

        followAngles.x = Mathf.SmoothDampAngle(transform.eulerAngles.x, targetAngles.x, ref targetXAngleVelocity, dampingTime);
        followAngles.y = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngles.y, ref targetYAngleVelocity, dampingTime);

        transform.rotation = Quaternion.Euler(followAngles.x, followAngles.y, 0);

        // Blend to favourable angle
        if (blendToRotationTarget)
        {
            Vector3 targetEuler = rotationTarget.eulerAngles;
            Vector3 currentEuler = transform.eulerAngles;

            if ((currentEuler.x - 1.0f < targetEuler.x && targetEuler.x < currentEuler.x + 1.0f) &&
                (currentEuler.y - 1.0f < targetEuler.y && targetEuler.y < currentEuler.y + 1.0f) &&
                (currentEuler.z - 1.0f < targetEuler.z && targetEuler.z < currentEuler.z + 1.0f))
            {
                blendToRotationTarget = false;
            }
        }

        //set field of view on pinch to zoom or mouse wheel
        if (inputManager.virtualMouseWheelActive)
        {
            //chaage Field of view or something else here
            var newFov = Mathf.Clamp(GetComponent<Camera>().fieldOfView - inputManager.virtualMouseWheel, 30f, defaultFOV);
            GetComponent<Camera>().fieldOfView = newFov;
        }
        else
        {
            GetComponent<Camera>().fieldOfView = Mathf.SmoothDamp(GetComponent<Camera>().fieldOfView, defaultFOV, ref targetZoomVelocity, dampingTime);
        }
    }

    public static Quaternion FromToRotation(Quaternion start, Quaternion end)
    {
        return Quaternion.Inverse(start) * end;
    }

    void LateUpdate()
    {
        inputManager.virtualKeyDown = false;
        inputManager.virtualGyro = new Vector3(0,0,0);
        inputManager.virtualMouseWheel = 0;
    }
}