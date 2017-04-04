using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutCubeColourScript : MonoBehaviour {

	public Material[] mats;
	public Transform outerCube;

	private int matChoice;

	// Use this for initialization
	void Start () {
		matChoice = Random.Range (0, mats.Length);
		cubecolourer (outerCube);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void cubecolourer(Transform cube){
		foreach (Transform t in cube) {
			MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer> ();
			if (mr != null) {
				mr.material = mats [matChoice];
			} else {
				matChoice = Random.Range (0, mats.Length);
				cubecolourer (t);
			}
		}
	}
}
