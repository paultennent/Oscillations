using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBuilder : AbstractGameEffects {

	public GameObject wallprefab;
	private GameObject focusPoint;
	private GameObject rearfocusPoint;
	private GameObject leftfocusPoint;
	private GameObject rightfocusPoint;

	private GameObject drawPoint;

	public Transform wallparent;
	public GameObject pivot;
	public GameObject floorPrefab;
	public float floorDepth = 1f;

	public float ZFocusDistance = 500f;
	public float XFocusDistance = 500f;

	private Vector3 lastPivotPos;

	private LinkedList<List<GameObject>> walls;

	private float timeCounter = 0;
    private bool wallsExist = false;
	public float blockSize;

	public float gapWidth = 10f;
	public float roadWidth = 5f;

	private float blockOffset = 2.25f;

	public bool buildingsExist = false;

	// Use this for initialization
	void Start () {
        base.Start();

		initFocusPoints ();

		drawPoint = new GameObject ("Drawer");
		drawPoint.transform.position = pivot.transform.position;

		blockSize = wallprefab.GetComponent<SimpleCityBlockGen> ().totalSize;
		blockOffset = blockSize+gapWidth;
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();

        if (true || inSession)
        {
            if (!wallsExist)
            {
                initWalls();
				buildingsExist = true;
            }
        }
        else { return; }


		timeCounter += Time.deltaTime;
		if (timeCounter > 60f) {
			timeCounter = 0f;
		}

		if (lastPivotPos.z < pivot.transform.position.z) {
			//we're going forward
			if (focusPoint.transform.position.z > (walls.Last.Value) [0].transform.position.z + blockOffset) {
				//it's more than a wall since we drew one
				addMoreWalls (true);
			}


		} else if (lastPivotPos.z > pivot.transform.position.z) {
			//we're going backwards
			if (rearfocusPoint.transform.position.z < (walls.First.Value) [0].transform.position.z - blockOffset) {
				//it's more than a wall since we drew one
				addMoreWalls (false);
			}
		}

		lastPivotPos = pivot.transform.position;
	}

	private void AddWallPair(bool forward){

		List<GameObject> innerWalls = new List<GameObject> ();

		//left walls
		float xpos = 0;
		float ypos = (drawPoint.transform.position.y) - floorDepth;
		float zpos = drawPoint.transform.position.z;
		if(!forward){
			zpos = drawPoint.transform.position.z - blockSize / (blockSize / 2f);
		}

		//left blocks
		for (float i = 0; i <= XFocusDistance; i += (blockOffset)) {
			xpos = i + (roadWidth *.5f) + blockSize*.5f;
			GameObject wall1 = GameObject.Instantiate (wallprefab, new Vector3 (-xpos, ypos, zpos), Quaternion.identity) as GameObject;
			wall1.name = "w1 " + zpos;
			wall1.transform.parent = wallparent.transform;
			innerWalls.Add (wall1);
		}

		//right blocks
		for (float i = 0; i <= XFocusDistance; i += (blockOffset)) {
			xpos = i  + blockSize*.5f + roadWidth*.5f;
			GameObject wall2 = GameObject.Instantiate (wallprefab, new Vector3 (xpos, ypos, zpos), Quaternion.identity) as GameObject;
			wall2.name = "w2 " + zpos;
			wall2.transform.parent = wallparent.transform;
			innerWalls.Add (wall2);
		}

		if (forward) {
			walls.AddLast(innerWalls);
		} else {
			walls.AddFirst(innerWalls);
		}
	}

	private void addMoreWalls(bool forward){
		if (forward) {
			//add in the necessary amount of walls forward
			float start = (walls.Last.Value)[0].transform.position.z + (blockOffset);
			for (float i = start; i <= focusPoint.transform.position.z; i += blockOffset) {
				drawPoint.transform.position = new Vector3 (0, 0, i);
				AddWallPair (true);
				removeWalls (true);
			}

		} else {
			//add in the necessarty amount of walls backward
			float start = (walls.First.Value)[0].transform.position.z- (blockOffset);

			for (float i = start; i >= rearfocusPoint.transform.position.z; i -= blockOffset) {
				drawPoint.transform.position = new Vector3 (0, 0, i);
				AddWallPair (false);
				removeWalls (false);
			}
		}

	}

	private void initWalls(){
		walls = new LinkedList<List<GameObject>> ();
		//wallinfos = new LinkedList<float[]> ();


		for (float i = -ZFocusDistance; i < ZFocusDistance; i += blockOffset) {
			drawPoint.transform.position = pivot.transform.position + new Vector3 (0, 0, i);
			AddWallPair (true);
		}
        wallsExist = true;
	}

	private void removeWalls(bool first){
		if (first) {
			List<GameObject> kilList = walls.First.Value;
			foreach (GameObject g in kilList) {
				Destroy (g);
			}
			walls.RemoveFirst ();
		} else {
			List<GameObject> kilList = walls.Last.Value;
			foreach (GameObject g in kilList) {
					Destroy (g);
				}
			walls.RemoveLast ();
		}
	} 

	private void initFocusPoints(){
		focusPoint = new GameObject ("FrontFocus");
		focusPoint.transform.position = pivot.transform.position;
		focusPoint.transform.parent = pivot.transform;
		focusPoint.transform.localPosition = new Vector3 (0, 0, ZFocusDistance);
		rearfocusPoint = new GameObject ("RearFocus");
		rearfocusPoint.transform.position = pivot.transform.position;
		rearfocusPoint.transform.parent = pivot.transform;
		rearfocusPoint.transform.localPosition = new Vector3 (0, 0, -ZFocusDistance);
		leftfocusPoint = new GameObject ("LeftFocus");
		leftfocusPoint.transform.position = pivot.transform.position;
		leftfocusPoint.transform.parent = pivot.transform;
		leftfocusPoint.transform.localPosition = new Vector3 (-XFocusDistance, 0, 0);
		rightfocusPoint = new GameObject ("RightFocus");
		rightfocusPoint.transform.position = pivot.transform.position;
		rightfocusPoint.transform.parent = pivot.transform;
		rightfocusPoint.transform.localPosition = new Vector3 (XFocusDistance, 0, 0);
	}
}
