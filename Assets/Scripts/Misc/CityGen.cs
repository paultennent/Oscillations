using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGen : MonoBehaviour {

	public int xBlocks = 1;
	public int zBlocks = 1;

	public float spacer = 1f;

	public GameObject block;


	// Use this for initialization
	void Start () {

		float blocksize = block.GetComponent<CityBlockGen> ().innerBlockCount;

		for(int i=0;i<xBlocks;i++){
			for (int j = 0; j < zBlocks; j++) {
				float xpos = i * blocksize + i * spacer;
				float zpos = j * blocksize + j * spacer;
				GameObject newblock = Instantiate (block);
				newblock.transform.position = new Vector3 (xpos, 0, zpos);
				newblock.transform.parent = transform;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
