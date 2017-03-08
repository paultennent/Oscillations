using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCityBlockGen : MonoBehaviour {

	public GameObject[] buildings;

	public float innerBlockCount = 5f;
	public float totalSize = 5f;
	public float buildingHeightScaleVariationMin = 0.2f;
	public float buildingHeightScaleVariationMax = 10f;
	public float innerGap = 1f;


	// Use this for initialization
	void Start () {
		initBuild();
	}

	// Update is called once per frame
	private void initBuild () {
        float offset=totalSize*0.5f-(totalSize/innerBlockCount)*0.5f;
		for (float i = 0; i < innerBlockCount; i++) {
			for (float j = 0; j < innerBlockCount; j++) {
				build(i*(totalSize/innerBlockCount)-offset,j*(totalSize/innerBlockCount)-offset,(totalSize/innerBlockCount)-innerGap,(totalSize/innerBlockCount)-innerGap);
			}
		}
	}

	private void build(float xpos,float zpos,float xwidth,float zwidth){
		//get a random building
		GameObject building = Instantiate(buildings[Random.Range(0,buildings.Length)]);
		float height = 1f * Random.Range (buildingHeightScaleVariationMin, buildingHeightScaleVariationMax);
		building.transform.parent = transform;
		building.transform.localPosition = new Vector3(xpos, height/2f , zpos);
		building.transform.localScale = new Vector3(xwidth,height,zwidth);

	}
}
