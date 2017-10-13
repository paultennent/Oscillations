using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLReadTest : MonoBehaviour {

	private bool gotData = false;
	private bool failed = false;

	private string url = "http://www.cs.nott.ac.uk/~pszpt/staging/data/20170502172203-10010037-ce12160cf1b75c3f0c.txt";
	private string data;
	private byte[] rawdata;

	void Start(){
		//testing
		//StartGetData(url);
	}

	public void StartGetData(string path, bool raw){
		url = path;
		StartCoroutine(Go(raw));
	}

	public string getData(){
		return data;
	}

	public byte[] getRawData(){
		return rawdata;
	}

	public void reset(){
		gotData = false;
		failed = false;
	}

	IEnumerator Go(bool raw)
	{
		WWW www = new WWW(url);
		yield return www;
		if (www.error == null)
		{
			if (!raw) {
				Process (www.text);
			} else {
				ProcessRaw (www.bytes);
			}
		}
		else
		{
			Debug.Log("ERROR: " + www.error);
			failed = true;
			gotData = true;
		}        
	} 

	void ProcessRaw(byte[] s){
		gotData = true;
		rawdata = s;
	}

	void Process(string s){
		//print(s);
		gotData = true;
		data = s;
	}

	public bool hasGotData(){
		return gotData;
	}

	public bool hasFailed(){
		return failed;
	}


}
