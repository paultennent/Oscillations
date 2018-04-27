using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public GameObject buildingBlock;
    public GameObject capPrefab;

    public int numFloorsInEachDirection = 5; // specify how many floors above and how many below;
    public float wallGap = .0025f; //should probably be numfloors/10
    private int groundFloor;

    private Floor[] floors;
    private Floor[] caps;

    private FloorPosition[] floorPositions;

    public float lightIntensity = 1f;
    public float spotAngle = 179f;
    private float skylightRadius = .25f;
    public float skylightOffset = 0.05f;

    public float scaling = 5f;

    private bool extended = false;
    private bool rotated = false;

    public Color lightColour = Color.white;

	// Use this for initialization
	void Start () {
        groundFloor = numFloorsInEachDirection + 1;
        initialise();
    }

    public int getFloorLenth()
    {
        return floors.Length;
    }

    public void LerpExtendUp(float t, bool extend)
    {
        //smoothly move upper floors to maximum/minimum extension over a defined time
        for (int i = groundFloor+1; i < floors.Length; i++)
        {
            if (extend)
            {
                floors[i].lerpYPosition(floorPositions[i].maxY, t);
            }
            else
            {
                floors[i].lerpYPosition(floorPositions[i].minY, t);
            }
        }
    }

    public void LerpExtendDown(float t, bool extend)
    {
        //smoothly move lower floors to maximum/minimum extension over a defined time
        for (int i = groundFloor-1; i >= 0; i--)
        {
            if (extend)
            {
                floors[i].lerpYPosition(floorPositions[i].maxY, t);
            }
            else
            {
                floors[i].lerpYPosition(floorPositions[i].minY, t);
            }
        }
    }

    public void SetExtendUp(float val)
    {
        //val is 0-1 mapped to rnage of extension. Should work with swing angle. gets clamped to 0-1 just in case
        val = Mathf.Clamp(val, 0f, 1f);
        for (int i = groundFloor + 1; i < floors.Length; i++)
        {
            floors[i].setYPosition(floorPositions[i].maxY * val);
        }
    }

    public void SetExtendDown(float val)
    {
        //val is 0-1 mapped to rnage of extension. Should work with swing angle. gets clamped to 0-1 just in case
        val = Mathf.Clamp(val, 0f, 1f);
        for (int i = groundFloor - 1; i >= 0; i--)
        {
            floors[i].setYPosition(floorPositions[i].maxY * val);
        }
    }

    public void SetWallRotations(float north, float south, float east, float west)
    {
        //set the rotations of all walls (degrees)
        foreach(Floor f in floors)
        {
            f.setNorthRotation(north);
            f.setSouthRotation(south);
            f.setEastRotation(east);
            f.setWestRotation(west);
        }
    }

    public void LerpWallRotations(float north, float south, float east, float west, float time)
    {
        //smoothly set the wall rotations over a defined time
        foreach (Floor f in floors)
        {
            f.lerpNorthRot(north, time);
            f.lerpSouthRot(south, time);
            f.lerpEastRot(east, time);
            f.lerpWestRot(west, time);
        }
    }

    public void SetLightIntensity(float val)
    {
        foreach(Floor f in floors)
        {
            f.setLightIntensity(val, val, val, val, lightColour);
        }
    }

    public void LerpLightIntensity(float val, float time)
    {
        foreach (Floor f in floors)
        {
            f.lerpLightIntensity(val, val, val, val, time, lightColour);
        }
    }

    public void setBuildingRotation(Vector3 angles)
    {
        transform.localEulerAngles = angles;
    }

    public void LerpBuildingRotation(Vector3 angles, float time)
    {
        StartCoroutine(MoveToRotation(transform, angles, time));
    }

    public IEnumerator MoveToRotation(Transform transform, Vector3 rotation, float timeToMove)
    {
        var currentPos = transform.localEulerAngles;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.localEulerAngles = Vector3.Lerp(currentPos, rotation, t);
            yield return null;
        }
    }

    public void initialise()
    {
        floors = new Floor[(numFloorsInEachDirection * 2)+1];
        floorPositions = new FloorPosition[floors.Length];
        int fc = 0;

        //now build from the bottom up
        for (int i = 0; i < numFloorsInEachDirection; i++)
        {
            GameObject g = new GameObject("Floor " + i);
            g.transform.parent = transform;
            floors[fc] = g.AddComponent<Floor>();
            floors[fc].buildingBlock = buildingBlock;

            floorPositions[fc] = new FloorPosition(0, -(numFloorsInEachDirection - i) * buildingBlock.transform.localScale.y);

            float myFloorPos = floorPositions[fc].minY;

            floors[fc].initialise(1f - (wallGap * i), myFloorPos, lightIntensity, spotAngle, lightColour);
            floors[fc].transform.localEulerAngles = new Vector3(floors[fc].transform.localEulerAngles.x + 180, floors[fc].transform.localEulerAngles.y, floors[fc].transform.localEulerAngles.z);
            float colorval = 1f - ((1f / (float) numFloorsInEachDirection) * i);
            Color c = new Color(colorval, colorval, colorval);
            floors[fc].SetWallColours(c);
            fc++;
        }

        //now build the main floor
        GameObject go = new GameObject("GroundFloor");
        go.transform.parent = transform;
        floors[fc] = go.AddComponent<Floor>();
        floors[fc].buildingBlock = buildingBlock;
        floors[fc].initialise(1f - (wallGap * fc), lightIntensity, spotAngle, lightColour);
        floorPositions[fc] = new FloorPosition(0, 0);
        Color co = new Color(0, 0, 0);
        floors[fc].SetWallColours(co);
        fc++;

        //now build to the top
        for (int i = 0; i < numFloorsInEachDirection; i++)
        {
            GameObject g = new GameObject("Floor " + fc);
            g.transform.parent = transform;
            floors[fc] = g.AddComponent<Floor>();
            floors[fc].buildingBlock = buildingBlock;

            floorPositions[fc] = new FloorPosition(0, (i) * buildingBlock.transform.localScale.y);

            float myFloorPos = floorPositions[fc].minY;

            floors[fc].initialise(1f - (wallGap * (numFloorsInEachDirection - i)), myFloorPos, lightIntensity, spotAngle, lightColour);
            float colorval = ((1f / (float)numFloorsInEachDirection) * i);
            Color c = new Color(colorval, colorval, colorval);
            floors[fc].SetWallColours(c);

            fc++;
        }


        //add the caps
        caps = new Floor[2];
        GameObject cap = new GameObject("Base");
        cap.transform.parent = floors[0].transform;
        caps[0] = cap.AddComponent<Floor>();
        caps[0].buildingBlock = capPrefab;
        caps[0].initialise(.5f + skylightRadius, (floors[0].transform.position.y - floors[0].transform.localScale.y/4f) - skylightOffset);

        cap = new GameObject("Roof");
        cap.transform.parent = floors[floors.Length-1].transform;
        caps[1] = cap.AddComponent<Floor>();
        caps[1].buildingBlock = capPrefab;
        caps[1].initialise(.5f + skylightRadius, (floors[floors.Length-1].transform.position.y + floors[floors.Length - 1].transform.localScale.y / 4f)+skylightOffset);
        caps[1].transform.localEulerAngles = new Vector3(180, 0, 0);

        transform.localScale = transform.localScale * scaling;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            LerpExtendUp(5f,!extended);
            LerpExtendDown(5f,!extended);
            extended = !extended;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            if (!rotated)
            {
                LerpBuildingRotation(new Vector3(90f, 0f, 0f), 5);
            }
            else
            {
                LerpBuildingRotation(new Vector3(0f, 0f, 0f), 5);
            }
            rotated = !rotated;
        }
    }

    private class FloorPosition
    {
        public float minY;
        public float maxY;
        
        public FloorPosition(float min, float max)
        {
            minY = min;
            maxY = max;
        }
    }

}
