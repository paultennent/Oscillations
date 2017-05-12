using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class ReplaySceneSelector : MonoBehaviour {

	[DllImport("__Internal")]
	private static extern string GetFilenames();

	public Text debug;

	private URLReadTest urt;
	private string riderNo = "10010037";
	private bool gotRideData = false;
	private bool gotSwingData = false;

	private string scene;
	private byte[] binData;

	private string serverPath = "http://www.cs.nott.ac.uk/~pszjm2/oscillations/";

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(transform.gameObject);
		riderNo = GetFilenames ();
		print ("UnityRecieved:" + riderNo);
		if (riderNo != null) {
			debug.text = "";//"Rider Number: " + riderNo;
		} else {
			debug.text = "Failed to find rider number.";
		}

		string path = getPath (riderNo,false);
		urt = gameObject.GetComponent<URLReadTest> ();
		urt.StartGetData(path,false);
	}

	void Update(){
		if (!gotRideData) {
			//get the rede data first
			if (urt.hasFailed ()) {
				debug.text = "Failed to find data for rider number: " + riderNo;
			} else {
				if (urt.hasGotData ()) {
					handleRideData (urt.getData ());
					gotRideData = true; 
				}
			}
		} else {
			//now get the swing data
			if (!gotSwingData) {
				if (urt.hasFailed ()) {
					debug.text = "Failed to find data for rider number: " + riderNo;
				} else {
					if (urt.hasGotData ()) {
						handleSwingData (urt.getRawData ());
						gotSwingData = true; 
					}
				}
			}
		}
	}

	private string getPath(string rider, bool bin){
		string p = serverPath + "riderdatafwd.php?riderid=" + riderNo + "&binarydata=";
		if (!bin) {
			return p + "0";
		} else {
			return p + "1";
		}
	}

	public byte[] getData(){
		return binData;
	}

	private void handleRideData(string data){
		string[] lines = Regex.Split(data, "\r\n|\r|\n");
		scene = Regex.Split (lines [3], ",") [1];
		print (scene);
		string path = getPath (riderNo,true);
		urt.reset ();
		urt.StartGetData(path,true);
	}

	private void handleSwingData(byte[] data){
		binData = data;
		print ("Got all data for ride");
		SceneManager.LoadScene (scene+"-replay");
	}
}
