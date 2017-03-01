using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighRollerCityBuilder : AbstractGameEffects {

	public GameObject wallprefab;
	private GameObject focusPoint;
	private GameObject rearfocusPoint;

	private GameObject drawPoint;

	public Transform wallparent;
	public GameObject pivot;
	public GameObject floorPrefab;
	public float floorDepth = 1f;

	public float floorwidth;
	public float focusdistance = 500f;

	private Vector3 lastPivotPos;

	public Material[] materials;
	private int curMat = 1;
    private int matSwitchcounter = 0;
    public int colourBlockSize = 1;


	private LinkedList<GameObject[]> walls;
	private LinkedList<float[]>wallinfos;

	private CamMoverTest cmt;

	private float timeCounter = 0;
    private bool wallsExist = false;
	public float blocksize;

	public float gapWidth = 10f;
	public float roadWidth = 5f;

	private float blockOffset = 2.25f;

	public bool buildingsExist = false;

	// Use this for initialization
	void Start () {
        base.Start();

		cmt = GetComponent<CamMoverTest> ();

		focusPoint = new GameObject ("Focus");
		focusPoint.transform.position = pivot.transform.position;
		focusPoint.transform.parent = pivot.transform;
		focusPoint.transform.localPosition = new Vector3 (0, 0, focusdistance);
		rearfocusPoint = new GameObject ("RearFocus");
		rearfocusPoint.transform.position = pivot.transform.position;
		rearfocusPoint.transform.parent = pivot.transform;
		rearfocusPoint.transform.localPosition = new Vector3 (0, 0, -focusdistance);

		drawPoint = new GameObject ("Drawer");
		drawPoint.transform.position = pivot.transform.position;


		blocksize = wallprefab.GetComponent<SimpleCityBlockGen> ().totalSize + gapWidth;
		blockOffset = ((wallprefab.GetComponent<SimpleCityBlockGen> ().totalSize / wallprefab.GetComponent<SimpleCityBlockGen> ().innerBlockCount) - wallprefab.GetComponent<SimpleCityBlockGen> ().innerGap)/2f ;

		floorwidth = blocksize ;//floorPrefab.transform.localScale.x * 2;
		
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
			if (focusPoint.transform.position.z > (walls.Last.Value) [0].transform.position.z + blocksize) {
				//it's more than a wall since we drew one
				addMoreWalls (true);
			}


		} else if (lastPivotPos.z > pivot.transform.position.z) {
			//we're going backwards
			if (rearfocusPoint.transform.position.z < (walls.First.Value) [0].transform.position.z - blocksize) {
				//it's more than a wall since we drew one
				addMoreWalls (false);
			}
		}

		lastPivotPos = pivot.transform.position;
	}

	private void AddWallPair(bool forward, float leftwidth, float rightwidth, int leftColour, int rightColour, int floorColour, float leftTilt, float rightTilt){

		GameObject[] wallset = new GameObject[3];
		float[] lastwallinfo = { };
		//get the info about the last set of walls before this one
		if (wallinfos.Count > 0) {
			lastwallinfo = wallinfos.First.Value;
			if (forward) {
				lastwallinfo = wallinfos.Last.Value;
			}
		}

		//left wall
		float xpos = drawPoint.transform.position.x - leftwidth;
		float ypos = (drawPoint.transform.position.y) - floorDepth;
		float zpos = drawPoint.transform.position.z;
		if(!forward){
			zpos = drawPoint.transform.position.z - blocksize / (blocksize / 2f);
		}
		GameObject wall1 = GameObject.Instantiate(wallprefab, new Vector3(xpos,ypos,zpos), Quaternion.identity) as GameObject;
		wall1.name = "w1 " + zpos;

		wall1.transform.parent = wallparent.transform;
		wallset [0] = wall1;

		//right wall
		xpos = drawPoint.transform.position.x + rightwidth;
		ypos = (drawPoint.transform.position.y) - floorDepth;
		if(!forward){
			zpos = drawPoint.transform.position.z - blocksize / (blocksize / 2f);
		}
		GameObject wall2 = GameObject.Instantiate(wallprefab, new Vector3(xpos,ypos,zpos), Quaternion.identity) as GameObject;
		wall2.name = "w2 " + zpos;

		wall2.transform.parent = wallparent.transform;
		//wall2.GetComponent<Renderer> ().material = materials [rightColour];
		wallset [1] = wall2;

		//floor
		xpos = drawPoint.transform.position.x;
		ypos = drawPoint.transform.position.y - floorDepth;

		GameObject floor1 = GameObject.Instantiate(floorPrefab, new Vector3(xpos,ypos,zpos), Quaternion.identity) as GameObject;
		floor1.name = "f1 " + zpos;
		floor1.transform.parent = wallparent.transform;
		floor1.transform.localScale = new Vector3(leftwidth + rightwidth + blocksize,floor1.transform.localScale.y,blocksize);

		floor1.GetComponent<Renderer> ().material = materials [floorColour];
		//make the floor disappear
		floor1.GetComponent<Renderer> ().enabled = false;
		wallset [2] = floor1;

		float[] wallinfo = {leftwidth, rightwidth, (float) leftColour, (float)rightColour, (float)floorColour, leftTilt, rightTilt};

		if (forward) {
			walls.AddLast(wallset);
			wallinfos.AddLast (wallinfo);
		} else {
			walls.AddFirst(wallset);
			wallinfos.AddFirst (wallinfo);
		}
	}

	private void addMoreWalls(bool forward){
		if (forward) {
			//add in the necessary amount of walls forward
			float start = (walls.Last.Value)[0].transform.position.z + (blocksize);
			for (float i = start; i <= focusPoint.transform.position.z; i += blocksize) {
				drawPoint.transform.position = new Vector3 (0, 0, i);
				int mat = getNextMat ();
				AddWallPair (true, getNextLeftWidth(), getNextRightWidth(), mat, mat, 0, getNextLeftTilt(), getNextRightTilt());
				removeWalls (true);
			}

		} else {
			//add in the necessarty amount of walls backward
			float start = (walls.First.Value)[0].transform.position.z- (blocksize);

			for (float i = start; i >= rearfocusPoint.transform.position.z; i -= blocksize) {
				drawPoint.transform.position = new Vector3 (0, 0, i);
				int mat = getNextMat ();
				AddWallPair (false, getNextLeftWidth(), getNextRightWidth(), mat, mat, 0, getNextLeftTilt(), getNextRightTilt());
				removeWalls (false);
			}
		}

	}

	private void initWalls(){
		walls = new LinkedList<GameObject[]> ();
		wallinfos = new LinkedList<float[]> ();



		for (float i = -focusdistance; i < focusdistance; i += blocksize) {
			drawPoint.transform.position = pivot.transform.position + new Vector3 (0, 0, i);
			int mat = getNextMat ();
			AddWallPair (true, getNextLeftWidth(), getNextRightWidth(), mat, mat, 0, 0, 0);
		}
        wallsExist = true;
	}

	private void removeWalls(bool first){
		if (first) {
			GameObject[] toKill = walls.First.Value;
			foreach (GameObject g in toKill) {
				Destroy (g);
			}
			walls.RemoveFirst ();
			wallinfos.RemoveFirst ();
		} else {
			GameObject[] toKill = walls.Last.Value;
			foreach (GameObject g in toKill) {
				Destroy (g);
			}
			walls.RemoveLast ();
			wallinfos.RemoveLast ();
		}
	} 

	private void dump(LinkedList<GameObject[]> l){
		string s = "List:[";
		foreach (GameObject[] go in l) {
			s+= (go [0].name + ", " + go [1].name + ", " + go [2]);
		}
		s+= ("]");
		print (s);
	}

	private int getNextMat(){
        matSwitchcounter++;
        if (matSwitchcounter >= colourBlockSize)
        {
            matSwitchcounter = 0;
            curMat++;
            if (curMat > 3)
            {
                curMat = 1;
            }
        }

		return curMat;
	}

	private float getNextLeftWidth(){
		return blocksize - gapWidth + (roadWidth /2) - blockOffset;
	}

	private float getNextRightWidth(){
		return + (roadWidth /2) + blockOffset;
	}

	private float getNextLeftTilt(){
		return 0f;
	}

	private float getNextRightTilt(){
		return 0f;
	}
		
}
