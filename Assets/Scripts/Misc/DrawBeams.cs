using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBeams : AbstractGameEffects
{

    public Material lineMaterial;
    public float gapDistance = 1f;
    public float lineCount = 10f;
    public float lineWidth = 0.01f;

    private List<Vector3> startPoints;
    private List<Vector3> endPoints;

    public GameObject lineparent;

    // Use this for initialization
    void Start()
    {

        base.Start();
        startPoints = new List<Vector3>();
        endPoints = new List<Vector3>();

        for (float i = -lineCount; i <= lineCount; i += gapDistance)
        {
            for (float j = -lineCount; j <= lineCount; j += gapDistance)
            {
                startPoints.Add(new Vector3(-lineCount, i, j));
                endPoints.Add(new Vector3(lineCount, i, j));
            }
        }

        for (float i = -lineCount; i <= lineCount; i += gapDistance)
        {
            for (float j = -lineCount; j <= lineCount; j += gapDistance)
            {
                startPoints.Add(new Vector3(i, -lineCount, j));
                endPoints.Add(new Vector3(i, lineCount, j));
            }
        }

        for (float i = -lineCount; i <= lineCount; i += gapDistance)
        {
            for (float j = -lineCount; j <= lineCount; j += gapDistance)
            {
                startPoints.Add(new Vector3(i, j, -lineCount));
                endPoints.Add(new Vector3(i, j, lineCount));
            }
        }

        for (int i = 0; i < startPoints.Count; i++)
        {
            DrawLine(startPoints[i], endPoints[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (inSession)
        {
            DestroyChildren(lineparent);
        }
    }

    void DrawLine(Vector3 startingPoint, Vector3 endPoint)
    {
        GameObject clone = new GameObject();
        clone.transform.parent = transform;
        LineRenderer line = clone.AddComponent<LineRenderer>();
        line.sortingLayerName = "OnTop";
        line.sortingOrder = 5;
        line.SetVertexCount(2);
        line.SetPosition(0, startingPoint);
        line.SetPosition(1, endPoint);
        line.SetWidth(lineWidth, lineWidth);
        line.useWorldSpace = true;
        line.material = lineMaterial;
        line.transform.parent = lineparent.transform;
    }

    public void DestroyChildren(GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        children.ForEach(child => GameObject.Destroy(child));
    }

}