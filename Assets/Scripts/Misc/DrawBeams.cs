using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBeams : AbstractGameEffects
{

    public Material lineMaterial;
    public float gapDistance = 20f;
    public float lineCount = 5f;
    public float lineWidth = 0.5f;

    private List<Vector3> startPoints;
    private List<Vector3> endPoints;

    public GameObject lineparent;

    // Use this for initialization
    void Start()
    {

        base.Start();
        
    }

	public void DrawAllBeams(){
		print ("Draw Beams");
		startPoints = new List<Vector3>();
		endPoints = new List<Vector3>();

		float i1 = -lineCount / 2;
		float i2 = lineCount / 2;
		print (i1 + ":" + i2);

		for (float i = i1; i <= i2; i += 1)
		{
			for (float j = i1; j <= i2; j += 1)
			{
				startPoints.Add(new Vector3(i1, i, j));
				endPoints.Add(new Vector3(i2, i, j));
			}
		}

		for (float i = i1; i <= i2; i += 1)
		{
			for (float j = i1; j <= i2; j += 1)
			{
				startPoints.Add(new Vector3(i, i1, j));
				endPoints.Add(new Vector3(i, i2, j));
			}
		}

		for (float i = i1; i <= i2; i += 1)
		{
			for (float j = i1; j <= i2; j += 1)
			{
				startPoints.Add(new Vector3(i, j, i1));
				endPoints.Add(new Vector3(i, j, i2));
			}
		}

		print (startPoints.Count);
		for (int i = 0; i < startPoints.Count; i++)
		{
			DrawLine(startPoints[i], endPoints[i]);
		}
	}

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    void DrawLine(Vector3 startingPoint, Vector3 endPoint)
    {
		Vector3 scale = transform.localScale;
//		scale /= lineCount;

        GameObject clone = new GameObject();
		if (lineparent == null) {
			clone.transform.parent = transform;
		} else {
			clone.transform.parent = lineparent.transform;
		}
        LineRenderer line = clone.AddComponent<LineRenderer>();
		line.useWorldSpace = false;
        line.sortingLayerName = "OnTop";
        line.sortingOrder = 5;
        line.SetVertexCount(2);
		line.SetPosition(0, new Vector3(startingPoint.x*scale.x,startingPoint.y*scale.y,startingPoint.z*scale.z));
		line.SetPosition(1, new Vector3(endPoint.x*scale.x,endPoint.y*scale.y,endPoint.z*scale.z));
        line.SetWidth(lineWidth, lineWidth);

		Material material = new Material(Shader.Find("Standard"));
		material.color = Color.cyan;

		line.material = material;
    }

    public void DestroyChildren(GameObject go)
    {
		if (go == null) {
			go = gameObject;
		}
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        children.ForEach(child => GameObject.Destroy(child));
    }

}