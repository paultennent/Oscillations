using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBuilder : AbstractGameEffects {

	public GameObject wallprefab;
	private GameObject focusPoint;
	private GameObject rearfocusPoint;

	private GameObject drawPoint;

	public Transform wallparent;
	public GameObject pivot;
	public GameObject floorPrefab;
	public float floorDepth = 1f;

	private float floorwidth;
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

		floorwidth = floorPrefab.transform.localScale.x;
		
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();

        if (true || inSession)
        {
            if (!wallsExist)
            {
                initWalls();
            }
        }
        else { return; }


		timeCounter += Time.deltaTime;
		if (timeCounter > 60f) {
			timeCounter = 0f;
		}

		if (lastPivotPos.z < pivot.transform.position.z) {
			//we're going forward
			if (focusPoint.transform.position.z > (walls.Last.Value) [0].transform.position.z + wallprefab.transform.localScale.z) {
				//it's more than a wall since we drew one
				addMoreWalls (true);
			}


		} else if (lastPivotPos.z > pivot.transform.position.z) {
			//we're going backwards
			if (rearfocusPoint.transform.position.z < (walls.First.Value) [0].transform.position.z - wallprefab.transform.localScale.z) {
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
		float ypos = (drawPoint.transform.position.y + wallprefab.transform.localScale.y / 2) - floorDepth;
//		float zpos = drawPoint.transform.position.z + wallprefab.transform.localScale.z / (wallprefab.transform.localScale.z / 2f);
		float zpos = drawPoint.transform.position.z;
		if(!forward){
			zpos = drawPoint.transform.position.z - wallprefab.transform.localScale.z / (wallprefab.transform.localScale.z / 2f);
		}
		GameObject wall1 = GameObject.Instantiate(wallprefab, new Vector3(xpos,ypos,zpos), Quaternion.identity) as GameObject;
		wall1.name = "w1 " + zpos;

		if (wallinfos.Count > 0) {
			//handle rotation for widning
			if (leftwidth != lastwallinfo [0]) { // if last one was the same do nothing
				//figure out the hypotenuse (how long our wall should be
				float opp = leftwidth - lastwallinfo [0];
				float adj = wallprefab.transform.localScale.z;
				float hyp = Mathf.Sqrt (Mathf.Pow (adj, 2f) + Mathf.Pow (opp, 2f));
				//get the angle
				float ang = Mathf.Asin (opp / hyp) * Mathf.Rad2Deg;

				//now lengthen move and rotate the wall
				wall1.transform.localScale = new Vector3 (wall1.transform.localScale.x, wall1.transform.localScale.y, hyp);
				wall1.transform.Translate (Vector3.right * opp / 2f);
				wall1.transform.Rotate (Vector3.down, ang);
			}
		}

		//handle rotation outwards
		Vector3 rotator = new Vector3 (xpos, ypos - wallprefab.transform.localScale.y / 2, zpos);
		wall1.transform.RotateAround (rotator, Vector3.forward, leftTilt);

		wall1.transform.parent = wallparent.transform;
		wall1.GetComponent<Renderer> ().material = materials [leftColour];
		wallset [0] = wall1;

		//right wall
		xpos = drawPoint.transform.position.x + rightwidth;
		ypos = (drawPoint.transform.position.y + wallprefab.transform.localScale.y / 2) - floorDepth;
//		zpos = drawPoint.transform.position.z;
//		zpos = drawPoint.transform.position.z + wallprefab.transform.localScale.z / (wallprefab.transform.localScale.z / 2f);
		if(!forward){
				zpos = drawPoint.transform.position.z - wallprefab.transform.localScale.z / (wallprefab.transform.localScale.z / 2f);
		}
		GameObject wall2 = GameObject.Instantiate(wallprefab, new Vector3(xpos,ypos,zpos), Quaternion.identity) as GameObject;
		wall2.name = "w2 " + zpos;

		if (wallinfos.Count > 0) {
			//handle rotation for widning
			if (rightwidth != lastwallinfo [1]) { // if last one was the same do nothing
				//figure out the hypotenuse (how long our wall should be
				float opp = rightwidth - lastwallinfo [1];
				float adj = wallprefab.transform.localScale.z;
				float hyp = Mathf.Sqrt (Mathf.Pow (adj, 2f) + Mathf.Pow (opp, 2f));
				//get the angle
				float ang = Mathf.Asin (opp / hyp) * Mathf.Rad2Deg;

				//now lengthen move and rotate the wall
				wall2.transform.localScale = new Vector3 (wall2.transform.localScale.x, wall2.transform.localScale.y, hyp);
				wall2.transform.Translate (Vector3.left * opp / 2f);
				wall2.transform.Rotate (Vector3.up, ang);
			}
		}

		//handle rotation outwards

		rotator = new Vector3 (xpos, ypos - wallprefab.transform.localScale.y / 2f, zpos);
		wall2.transform.RotateAround (rotator, Vector3.back, rightTilt);

		wall2.transform.parent = wallparent.transform;
		wall2.GetComponent<Renderer> ().material = materials [rightColour];
		wallset [1] = wall2;

		//floor
		xpos = drawPoint.transform.position.x + ((rightwidth-leftwidth)/2f);
		ypos = drawPoint.transform.position.y - floorDepth;
//		zpos = drawPoint.transform.position.z + (floorPrefab.transform.localScale.z) ;
//		if(!forward){
//			zpos = drawPoint.transform.position.z - (floorPrefab.transform.localScale.z/2f)/ (floorPrefab.transform.localScale.z / 2f) ;
//		}
		GameObject floor1 = GameObject.Instantiate(floorPrefab, new Vector3(xpos,ypos,zpos), Quaternion.identity) as GameObject;
		floor1.name = "f1 " + zpos;
		floor1.transform.parent = wallparent.transform;
		floor1.transform.localScale = new Vector3(leftwidth + rightwidth,floor1.transform.localScale.y,floor1.transform.localScale.z);

		//widen the floor to the previous value if we have shrunk and move if necessary
		if (wallinfos.Count > 0) {
			if (lastwallinfo [0] + lastwallinfo [1] > leftwidth + rightwidth) {
				floor1.transform.localScale = new Vector3(lastwallinfo [0] + lastwallinfo [1],floor1.transform.localScale.y,floor1.transform.localScale.z);
				float newxpos = drawPoint.transform.position.x + ((lastwallinfo [1]-lastwallinfo [0])/2f);
				floor1.transform.position = new Vector3 (newxpos, ypos, zpos);
			}
		}


		floor1.GetComponent<Renderer> ().material = materials [floorColour];
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
			float start = (walls.Last.Value)[0].transform.position.z + (wallprefab.transform.localScale.z);
			for (float i = start; i <= focusPoint.transform.position.z; i += wallprefab.transform.localScale.z) {
				drawPoint.transform.position = new Vector3 (0, 0, i);
				int mat = getNextMat ();
				AddWallPair (true, getNextLeftWidth(), getNextRightWidth(), mat, mat, 0, getNextLeftTilt(), getNextRightTilt());
				removeWalls (true);
			}

		} else {
			//add in the necessarty amount of walls backward
			float start = (walls.First.Value)[0].transform.position.z- (wallprefab.transform.localScale.z);

			for (float i = start; i >= rearfocusPoint.transform.position.z; i -= wallprefab.transform.localScale.z) {
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
		for (float i = -focusdistance; i < focusdistance; i += wallprefab.transform.localScale.z) {
			drawPoint.transform.position = pivot.transform.position + new Vector3 (0, 0, i);
			int mat = getNextMat ();
			AddWallPair (true, 2f, 2f, mat, mat, 0, 0, 0);
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
		if ((timeCounter > 15f && timeCounter < 30f) || (timeCounter > 45)) {
			return 2f + 5f * Mathf.Abs (Mathf.Sin (Time.time));
		}
		return 2f;
	}

	private float getNextRightWidth(){
		if ((timeCounter > 15f && timeCounter < 30f) || (timeCounter > 45)) {
			return 2f + 5f * Mathf.Abs (Mathf.Cos (Time.time));
		}
		return 2f;
	}

	private float getNextLeftTilt(){
		if (timeCounter > 30f) {
			return 135 * Mathf.Abs (Mathf.Sin (Time.time));
		}
		return 0f;
	}

	private float getNextRightTilt(){
		if (timeCounter > 30f) {
			return 135 * Mathf.Abs (Mathf.Cos (Time.time));
		}
		return 0f;
	}
		
}
