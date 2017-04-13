using System;
using UnityEngine;
using System.Collections;

public class TopDownCameraNavigator : MonoBehaviour 
{
    public Camera cam;
    public Transform src;

    public GameObject[] hideInAir;

    Collider[] targets;

    Vector3 initPos;
    Quaternion initRot;
    Quaternion targetRot;
    
    Transform activeTarget = null;
    float camInterp = 0.0f;
    public float camSpeed = 0.01f;
    float camStep = 0.0f;

    Vector3 m_Euler;
    public float m_Dist = 1.0f;

    bool mouseDown;
    Vector3 startMousePos;

    private rcInputManager inputManager;
    private rcInputManager.Stick stick;

    enum CameraState
    {
        InAir,
        OnFloor
    }
    CameraState camState = CameraState.InAir;

    private GameObject waypoints;

    void Awake()
    {
        waypoints = GameObject.Find("Waypoints");
        DisableInteriorCamera();
    }

    void OnDisable()
    {

    }

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

        targets = GetComponentsInChildren<Collider>();
        foreach(var target in targets)
        {
            var rnd = target.GetComponent<MeshRenderer>();
            if (rnd != null)
                rnd.enabled = false;
        }

        initPos = src.transform.position;
        initRot = src.transform.rotation;
        targetRot = initRot;

        cam.transform.position = initPos;
        cam.transform.rotation = initRot;

        m_Euler = initRot.eulerAngles;

        HideInAir(true);
	}

    public void DisableInteriorCamera()
    {
        if(waypoints != null)
        {
            waypoints.SetActive(false);
        }

        var camTrans = cam.GetComponent<CameraTranslation>();
        if (camTrans != null)
            camTrans.enabled = false;

        var camRot = cam.GetComponent<CameraRotation>();
        if (camRot != null)
            camRot.enabled = false;
    }
    
    void HideInAir(bool hide)
    {
        foreach (var obj in hideInAir)
        {
            obj.SetActive(!hide);
        }
    }

    private Quaternion rotationGyro = Quaternion.Euler(0, 0, 0);
    private Quaternion camPos = Quaternion.Euler(0,0,0);
    void Update()
    {
        float dt = Time.deltaTime;
        if (GameController.instance == null)
        {
            inputManager.UpdateManual(dt, dt);
        }
        
        rcInputManager.RawTouchStream touches = inputManager.RawTouches;

        var mousePos = Vector3.zero;
        bool isMouseDown = touches.touches_.Count > 0;
        bool isClick = false;
        if (isMouseDown)
        {
            var touch = touches.touches_[0];
            var touchPos = touch.pixelPos;
            mousePos.x = touchPos.x;
            mousePos.y = touchPos.y;

            isClick = (touch.state == rcInputManager.RawTouchStream.Touch.eState.Up) && (Time.realtimeSinceStartup - touch.timeOnDown < 0.2f);
        }
        else
        {
            mouseDown = false;
        }

        if (isClick)
        {
            Ray ray = cam.ScreenPointToRay(mousePos);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                // Is the hit object a child of ours (ie. a valid hit target?)
                if (hit.collider.gameObject.transform.IsChildOf(gameObject.transform) && camState == CameraState.InAir)
                {
                    activeTarget = hit.collider.gameObject.transform;
                    camInterp = 0.0f;
                    camStep = camSpeed;

                    initRot = cam.transform.rotation;

                    targetRot = Quaternion.AngleAxis(-90.0f, cam.transform.right) * cam.transform.rotation;
                }
            }
            else if (camState == CameraState.OnFloor)
            {
                //camStep = -camSpeed;
                //HideInAir(true);
            }
    	}

        if (activeTarget != null && camStep != 0.0f)
        {
            camInterp += camStep;

            if(camInterp <= 0.0f)
            {
                camState = CameraState.InAir;
                camStep = 0.0f;
                camInterp = 0.0f;
            }
            else if (camInterp >= 1.0f)
            {
                camState = CameraState.OnFloor;
                rotationGyro = Quaternion.Euler(0,0,0);
                camStep = 0.0f;
                camInterp = 1.0f;

                HideInAir(false);
            }

            camInterp = Mathf.Clamp01(camInterp);

            float smoothedInterp = Mathf.SmoothStep(0.0f, 1.0f, camInterp);

            var pos = Vector3.Lerp(initPos, activeTarget.transform.position, smoothedInterp);
            var rot = Quaternion.Slerp(initRot, targetRot, smoothedInterp);

            cam.transform.position = pos;
            cam.transform.rotation = rot;

            camPos = rot;

            m_Euler = rot.eulerAngles;
        }
        else if (camState == CameraState.InAir)
        {
            var axisX = stick.axisX;
#pragma warning disable 168
            var axisY = stick.axisY;
#pragma warning restore 168

            if (isMouseDown)
            {
                if (!mouseDown)
                {
                    startMousePos = mousePos;
                    mouseDown = true;
                }
                else
                {
                    var tmp = mousePos - startMousePos;

                    axisX = Mathf.Clamp(tmp.x / Screen.width, -1.0f, 1.0f);
                    axisY = Mathf.Clamp(tmp.y / Screen.height, -1.0f, 1.0f);

                    m_Euler.y += axisX * dt * 200.0f;

                    cam.transform.rotation = Quaternion.Euler(m_Euler);
                }
            }
        }
        else if (camState == CameraState.OnFloor)
        {
            var axisX = stick.axisX;
            var axisY = stick.axisY;

            if (isMouseDown)
            {
                if (isClick)
                {
                     if (camStep == 0.0f)
                     {
                         camStep = -camSpeed;
                         targetRot = cam.transform.rotation;
                         initRot = Quaternion.AngleAxis(90.0f, cam.transform.right) * cam.transform.rotation;

                         var eulerAngles = initRot.eulerAngles;
                         eulerAngles.x = 90.0f;
                         initRot.eulerAngles = eulerAngles;

                         HideInAir(true);
                     }
                }
                else
                {
                    if (!mouseDown)
                    {
                        startMousePos = mousePos;
                        mouseDown = true;
                    }
                    else
                    {
                        var tmp = mousePos - startMousePos;
                        axisX = -Mathf.Clamp(tmp.x / Screen.width, -1.0f, 1.0f);
                        axisY = Mathf.Clamp(tmp.y / Screen.height, -1.0f, 1.0f);

                        m_Euler.y += axisX * dt * 200.0f;
                        if (m_Euler.x > 180)
                            m_Euler.x = 360.0f - m_Euler.x;
                        else if(m_Euler.x < -180)
                            m_Euler.x = 360.0f + m_Euler.x;

                        m_Euler.x = Mathf.Clamp(m_Euler.x + axisY * dt * 200.0f, -30.0f, 30.0f);

                        camPos = Quaternion.Euler(m_Euler);

                        cam.transform.rotation = Quaternion.Euler(m_Euler);
                    }
                }
            }
        }

        if (camState == CameraState.OnFloor)
        {
            Debug.Log("could do gyro shit now");
            Quaternion rotChange = Quaternion.Euler(inputManager.virtualGyro.x, inputManager.virtualGyro.y,
                inputManager.virtualGyro.z);
            rotationGyro *= rotChange;

            cam.transform.rotation = camPos * rotationGyro;

            //set field of view on pinch to zoom or mouse wheel
            /*if (inputManager.virtualMouseWheel != 0f)
            {
                //chnage Field of view or something else here
                GetComponent<Camera>().fieldOfView -= inputManager.virtualMouseWheel;
            }*/
        }

        inputManager.virtualKeyDown = false;
        inputManager.virtualGyro = Vector3.zero;
        inputManager.virtualMouseWheel = 0;
    }
}
