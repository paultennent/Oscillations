using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBlockGen : MonoBehaviour {

	public GameObject[] buildings;

	public float innerBlockCount = 5f;
	private float buildingHeightScaleVariationMin = 0.2f;
	private float buildingHeightScaleVariationMax = 10f;

	// Use this for initialization
	void Start () {
		initBuild();
	}
	
	private void initBuild(){
		float rows = innerBlockCount;
		while (rows > 0){
			float xwidth = Random.Range(1,rows+1);
			float cols = innerBlockCount;
			while (cols > 0){
				float zwidth = Random.Range(1,cols+1);
				build(innerBlockCount-rows,innerBlockCount-cols,xwidth,zwidth);
				cols -= zwidth;
			}
			rows -=xwidth;
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
