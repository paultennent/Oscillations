using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class FlatController : MonoBehaviour 
{
    public Button backButton;

    [HideInInspector]
    public List<RaycastResult> results = new List<RaycastResult>();

    GraphicRaycaster graphicsRaycaster;

    static public FlatController instance;

    private rcInputManager inputManager;
    public Waypoint waypoint1;
    public Waypoint waypoint2;
    public Waypoint waypoint3;
    public Waypoint waypoint4;
    public Waypoint waypoint5;
    public Waypoint waypoint6;
    public Waypoint waypoint7;

    List<Waypoint> allWaypoints = new List<Waypoint>();
    float timeSinceLastInput;

    private int[] permutation;
    private int permutationIndex;

    void Awake()
    {
        instance = this;

        allWaypoints.Add(waypoint1);
        allWaypoints.Add(waypoint2);
        allWaypoints.Add(waypoint3);
        allWaypoints.Add(waypoint4);
        allWaypoints.Add(waypoint5);
        allWaypoints.Add(waypoint6);
        allWaypoints.Add(waypoint7);

        SetupPermutation(allWaypoints.Count);
    }

	void Start () 
    {
        //backButton.onClick.AddListener(BackToLandingPage);
        graphicsRaycaster = gameObject.GetComponentInChildren<GraphicRaycaster>();
        if (GameController.instance != null)
        {
            inputManager = GameController.instance.InputManager;
        }
        else
        {
            inputManager = gameObject.AddComponent<rcInputManager>();
        }
    }
	
	void Update () 
    {
        results.Clear();

        if (GameController.instance != null && GameController.instance.isClick)
        {
            PointerEventData ped = new PointerEventData(null);
            ped.position = GameController.instance.mousePos;

            graphicsRaycaster.Raycast(ped, results);

            // Check if user clicked on back button
            foreach (var hit in FlatController.instance.results)
            {
                if(hit.gameObject == backButton.gameObject)
                {
                    BackToLandingPage();
                }
            }
        }

        if (inputManager.GetVirtualKeyDown("1") && waypoint1 != null)
        {
            StartCoroutine(TeleportCamera(waypoint1));
            Debug.Log("Goto: 1");
        } else if (inputManager.GetVirtualKeyDown("2") && waypoint2 != null)
        {
            StartCoroutine(TeleportCamera(waypoint2));
            Debug.Log("Goto: 2");
        } else if (inputManager.GetVirtualKeyDown("3") && waypoint3 != null)
        {
            StartCoroutine(TeleportCamera(waypoint3));
            Debug.Log("Goto: 3");
        } else if (inputManager.GetVirtualKeyDown("4") && waypoint4 != null)
        {
            StartCoroutine(TeleportCamera(waypoint4));
            Debug.Log("Goto: 4");
        } else if (inputManager.GetVirtualKeyDown("5") && waypoint5 != null)
        {
            StartCoroutine(TeleportCamera(waypoint5));
            Debug.Log("Goto: 5");
        } else if (inputManager.GetVirtualKeyDown("6") && waypoint6 != null)
        {
            StartCoroutine(TeleportCamera(waypoint6));
            Debug.Log("Goto: 6");
        } else if (inputManager.GetVirtualKeyDown("7") && waypoint6 != null)
        {
            StartCoroutine(TeleportCamera(waypoint7));
            Debug.Log("Goto: 7");
        }
        if (inputManager.GetVirtualKeyDown("b"))
        {
            BackToLandingPage();
        }

        bool mouseDown = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || inputManager.virtualMouseDown;
        
        bool gyroZero = Mathf.Approximately(inputManager.virtualGyro.x, 0.0f) && Mathf.Approximately(inputManager.virtualGyro.y, 0.0f);
        if (mouseDown || inputManager.virtualKeyDown || !gyroZero)
        {
            timeSinceLastInput = 0.0f;
        }
        else
        {
            timeSinceLastInput += Time.deltaTime;
            if(timeSinceLastInput > 10.0f)
            {
                var randomWaypoint = allWaypoints[permutation[permutationIndex++]];

                // Wrap permutation index around back to zero
                if (permutationIndex >= permutation.Length)
                    permutationIndex = 0;

                StartCoroutine(TeleportCamera(randomWaypoint));

                timeSinceLastInput = 0.0f;
            }
        }

        // TODO: swallow mouse clicks on map so they don't trigger 3D waypoints
	}

    public IEnumerator TeleportCamera(Waypoint waypoint)
    {
        if (waypoint != null)
        {
            yield return StartCoroutine(GameController.instance.fade.LowerCurtains());
            waypoint.TeleportCamera();
            yield return StartCoroutine(GameController.instance.fade.RaiseCurtains());
        }
    }

    public void BackToLandingPage()
    {
        GameController.instance.LoadLevel(GameController.GameMode.LandingPage);
    }

    private void SetupPermutation(int numItems)
    {
        permutation = new int[numItems];
        for (int i = 0; i < numItems; i++)
            permutation[i] = i;

        for (int i = 0; i < numItems - 2; i++)
        {
            int j = UnityEngine.Random.Range(0, numItems - i);

            var item1 = permutation[i];
            var item2 = permutation[i + j];
            permutation[i + j] = item1;
            permutation[i] = item2;
        }

        for (int i = 0; i < numItems; i++)
        {
            Debug.Log(permutation[i]);
        }
    }
}
