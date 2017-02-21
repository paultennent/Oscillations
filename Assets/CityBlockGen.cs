using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBlockGen : MonoBehaviour {

	private float blockSize = 10f;
	private float innerBlockCount = 5f;
	private float buildingHeightScaleVariationMin = 0.2f;
	private float buildingHeightScaleVariationMax = 2f;

	// Use this for initialization
	void Start () {
		initBuild();
	}
	
	public void initBuild(){
		int width = 5;
		while (width > 0){
			xwidth = Random.Range(1,inner+1);


			inner + xwidth;
		}
	}
}
