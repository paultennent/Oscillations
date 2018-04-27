using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

    //north is relative to x-axis
    //distance refers to distance of each wall from the centre
    //for rotation positive change = clockwise (degrees) / negative change = counter-clockwise (degrees)

    public GameObject buildingBlock; // prefab for each buildingblock

    private GameObject[] buildingBlocks;

    private float northDist = .75f;
    private float southDist = .75f;
    private float westDist = .75f;
    private float eastDist = .75f;
    private float yPosition = 0f;

    private float northRot = 0f;
    private float southRot = 0f;
    private float eastRot = 0f;
    private float westRot = 0f;
    private float yRotation = 0f;

    private static float DEFAULT_INTENSSITY = 1f;
    private static float DEFAULT_SPOT_ANGLE = 179f;
    private static Color DEFAULT_COLOUR = Color.white;

    public Color debugCol = Color.white;

    public void initialise(float radius, float yPos)
    {
        northDist = radius;
        southDist = radius;
        westDist = radius;
        eastDist = radius;
        setYPosition(yPos);
        createBuildingBlocks(DEFAULT_INTENSSITY,DEFAULT_SPOT_ANGLE, DEFAULT_COLOUR);
    }

    public void initialise(float north, float south, float east, float west, float lightIntensity, float spotAngle, Color color)
    {
        northDist = north;
        southDist = south;
        westDist = west;
        eastDist = east;
        createBuildingBlocks(lightIntensity, spotAngle, color);
    }

    public void initialise(float radius, float lightIntensity, float spotAngle, Color color)
    {
        northDist = radius;
        southDist = radius;
        westDist = radius;
        eastDist = radius;
        createBuildingBlocks(lightIntensity, spotAngle, color);
    }

    public void initialise(float radius, float yPos, float lightIntensity, float spotAngle, Color color)
    {
        northDist = radius;
        southDist = radius;
        westDist = radius;
        eastDist = radius;
        setYPosition(yPos);
        createBuildingBlocks(lightIntensity, spotAngle, color);
    }

    //////////////////handle translations///////////////////////

    public void setSouthDistance(float south)
    {
        southDist = south;
        lerpSouthRot(south, 0f);
    }

    public void lerpSouthDistance(float newSouthDist, float time)
    {
        StartCoroutine(MoveToPosition(buildingBlocks[0].transform, new Vector3(-newSouthDist, buildingBlocks[0].transform.localPosition.y, buildingBlocks[0].transform.localPosition.z), time));
    }

    public void setNorthDistance(float north)
    {
        northDist = north;
        lerpNorthDistance(north, 0f);
    }

    public void lerpNorthDistance(float newNorthDist, float time)
    {
        StartCoroutine(MoveToPosition(buildingBlocks[1].transform, new Vector3(newNorthDist, buildingBlocks[1].transform.localPosition.y, buildingBlocks[1].transform.localPosition.z), time));
    }

    public void setWestDistance(float west)
    {
        westDist = west;
        lerpWestDistance(west, 0f);
    }

    public void lerpWestDistance(float newWestDist, float time)
    {
        StartCoroutine(MoveToPosition(buildingBlocks[2].transform, new Vector3(buildingBlocks[2].transform.localPosition.x, buildingBlocks[2].transform.localPosition.y, newWestDist), time));
    }

    public void setEastDistance(float east)
    {
        eastDist = east;
        lerpEastDistance(east, 0f);
    }

    public void lerpEastDistance(float newEastDist, float time)
    {
        StartCoroutine(MoveToPosition(buildingBlocks[3].transform, new Vector3(buildingBlocks[3].transform.localPosition.x, buildingBlocks[3].transform.localPosition.y, -newEastDist), time));
    }

    public void setYPosition(float yPos)
    {
        yPosition = yPos;
        lerpYPosition(yPosition, 0f);
    }

    public void lerpYPosition(float newYPos, float time)
    {
        StartCoroutine(MoveToPosition(transform, new Vector3(transform.localPosition.x, newYPos, transform.localPosition.z), time));
    }

    public IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToMove)
    {
        var currentPos = transform.localPosition;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.localPosition = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }
    }

    ///////////////////////handle rotations/////////////////////////////////
    public void setSouthRotation(float south)
    {
        southRot = south;
        lerpSouthRot(south,0f);
    }

    public void lerpSouthRot(float south, float time)
    {
        float offset = 0f;
        StartCoroutine(MoveToRotation(buildingBlocks[0].transform, new Vector3(0f, offset + south, 0f), time));
    }

    public void setNorthRotation(float north)
    {
        northRot = north;
        lerpNorthRot(north, 0f);
    }

    public void lerpNorthRot(float north, float time)
    {
        float offset = 180f;
        StartCoroutine(MoveToRotation(buildingBlocks[1].transform, new Vector3(0f, offset + north, 0f), time));
    }

    public void setWestRotation(float west)
    {
        westRot = west;
        lerpWestRot(west, 0f);
    }

    public void lerpWestRot(float west, float time)
    {
        float offset = 90f;
        StartCoroutine(MoveToRotation(buildingBlocks[2].transform, new Vector3(0f, offset + west, 0f), time));
    }

    public void setEastRotation(float east)
    {
        eastRot = east;
        lerpEastRot(east, 0f);
    }

    public void lerpEastRot(float east, float time)
    {
        float offset = 270f;
        StartCoroutine(MoveToRotation(buildingBlocks[3].transform, new Vector3(0f, offset + east, 0f), time));
    }

    public void setYRotation(float y)
    {
        yRotation = y;
        lerpYRot(y, 0f);
    }

    public void lerpYRot(float y, float time)
    {
        StartCoroutine(MoveToRotation(transform, new Vector3(0f, y, 0f), time));
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

    ///////////////////////handle intensities///////////////////////////////////
    public void setLightIntensity(float north, float south, float east, float west, Color color)
    {
        lerpLightIntensity(north, south, east, west , 0f, color);
    }

    public void lerpLightIntensity(float north, float south, float east, float west, float time, Color color)
    {
        StartCoroutine(ChangeToIntensity(buildingBlocks[0].GetComponent<BuildingBlock>(), south, time, color));
        StartCoroutine(ChangeToIntensity(buildingBlocks[1].GetComponent<BuildingBlock>(), north, time, color));
        StartCoroutine(ChangeToIntensity(buildingBlocks[2].GetComponent<BuildingBlock>(), west, time, color));
        StartCoroutine(ChangeToIntensity(buildingBlocks[3].GetComponent<BuildingBlock>(), east, time, color));
    }

    public IEnumerator ChangeToIntensity(BuildingBlock bb, float newIntensity, float timeToMove, Color color)
    {
        var currentPos = bb.lightIntensity;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            bb.default_color = color;
            bb.lightIntensity = Mathf.Lerp(currentPos, newIntensity, t);
            yield return null;
        }
    }

    public void SetWallColours(Color c)
    {
        foreach(GameObject b in buildingBlocks)
        {
            b.GetComponent<BuildingBlock>().setMaterialColour(c);
            debugCol = c;
        }
    }

    ///////////////////////build the room///////////////////////////////////////

    public void createBuildingBlocks (float lightIntensity, float spotAngle, Color color) {
        buildingBlocks = new GameObject[4];
        buildingBlocks[0] = createBuildingBlock(-southDist, 0, 0, 0, 0, 0, lightIntensity, spotAngle, color);
        buildingBlocks[1] = createBuildingBlock(northDist, 0, .001f, 0, 180, 0, lightIntensity, spotAngle, color);
        buildingBlocks[2] = createBuildingBlock(0, 0, westDist, .002f, 90, 0, lightIntensity, spotAngle, color);
        buildingBlocks[3] = createBuildingBlock(0, 0, -eastDist, .003f, 270, 0, lightIntensity, spotAngle, color);
    }

    public GameObject createBuildingBlock(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot, float intensity, float spotAngle, Color color)
    {
        GameObject g = Instantiate(buildingBlock);
        Transform t = g.transform;
        
        t.parent = transform;
        t.localPosition = new Vector3(xPos,yPos, zPos);
        t.localEulerAngles = new Vector3(xRot, yRot, zRot);
        if (t.gameObject.GetComponent<BuildingBlock>() != null)
        {
            t.gameObject.GetComponent<BuildingBlock>().default_color = color;
            t.gameObject.GetComponent<BuildingBlock>().lightIntensity = intensity;
            t.gameObject.GetComponent<BuildingBlock>().spotAngle = spotAngle;
        }

        return g;
    }

    

}
