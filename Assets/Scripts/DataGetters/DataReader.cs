using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public class DataReader : AbstractDataReader {

	private ArrayList timeData;
	private ArrayList accData;
	private ArrayList gyroData;
	private ArrayList magData;

	private int counter = 0;
	private double timeNow = 0.0;
	//private float[] magNow = {0f,0f,0f};

	private double firstGoodTime = -1.0;

	private volatile double[] lastMsCount = { 0, 0, 0 };
	private volatile bool[] lastMsCountGood = { false, false, false };

	public override double getTimeNow(){
		return timeNow;
	}

	void readTextFile(string file_path)
	{
		StreamReader inp_stm = new StreamReader(file_path);
		int counter = 0;
		while(!inp_stm.EndOfStream)
		{
			string inp_ln = inp_stm.ReadLine();
			if (counter > 0){
				handleData(inp_ln);
			}
			counter++;
		}
		
		inp_stm.Close( );
		//print ("Read " + counter + " lines of data");
	}

	void handleData(string inp_ln){
//		if (inp_ln.Substring (0, 3).Equals ("---")) {
//			handleTime (inp_ln);
//		} else if (inp_ln.Substring (0, 3).Equals ("acc")) {
//			handleAcc (inp_ln);
//		} else if (inp_ln.Substring (0, 3).Equals ("ang")) {
//			handleGyro (inp_ln);
//		} else if (inp_ln.Substring (0, 3).Equals ("mag")) {
//			handleMag (inp_ln);
//		} else {
//			print ("Unknown line type:"+inp_ln.Substring (0, 3));
//		}



		string[] parts = inp_ln.Split (',');
		double t = double.Parse (parts [0]);
		if (firstGoodTime == -1) {
			firstGoodTime = t;
		}
		timeData.Add (t-firstGoodTime);

		float[] f = new float[3];
		f [0] = float.Parse(parts [1].Trim ());
		f [1] = float.Parse(parts [2].Trim ());
		f [2] = float.Parse(parts [3].Trim ());
		accData.Add (f);

		double[] g = new double[3];
		g [0] = double.Parse(parts [4].Trim ());
		g [1] = double.Parse(parts [5].Trim ());
		g [2] = double.Parse(parts [6].Trim ());

		calculateGyroHeading (g, 0, (double) timeData[timeData.Count-1]);
		calculateGyroHeading (g, 1, (double) timeData[timeData.Count-1]);
		calculateGyroHeading (g, 2, (double) timeData[timeData.Count-1]);

		float[] xyz = new float[3];
		xyz [0] = (float)(headingNow [0]);
		xyz [1] = (float)(headingNow [1]);
		xyz [2] = (float)(headingNow [2]);
		
		gyroData.Add(xyz);


	}

	void writeDataToFile(){
		string text = "Time,AccX,AccY,AccZ,GyroX,GyroY,GyroZ\n";
		System.IO.File.AppendAllText ("C:\\hauntedswingoutput.txt", text);
		for (counter = 0; counter<timeData.Count; counter++) {
			text = ((double)timeData [counter]) + "," + ((float[])accData [counter]) [0] + "," + ((float[])accData [counter]) [1] + "," + ((float[])accData [counter]) [2] + "," + ((float[])gyroData [counter]) [0] + "," + ((float[])gyroData [counter]) [1] + "," + ((float[])gyroData [counter]) [2] + "\n";
			System.IO.File.AppendAllText ("C:\\hauntedswingoutput.txt", text);
		}
	}

	void writeHeaders(){
		string text = "Time,AccX,AccY,AccZ,GyroX,GyroY,GyroZ\n";
		System.IO.File.AppendAllText ("C:\\hauntedswingoutput.txt", text);
	}

	void writeLine(){
		string text = ((double)timeData [counter-1]) + "," + accNow[0] + "," + accNow[1] + "," + accNow[2] + "," + gyroNow[0] + "," + gyroNow[1] + "," + gyroNow[2] + "\n";
		System.IO.File.AppendAllText ("C:\\hauntedswingoutput.txt", text);
	}

	Regex timePattern = new Regex ("--- time: s: (\\d*) ms: (\\d*) ---");

	double firstTimeSeen=-1;

	void handleTime(string s){
		//print ("Handle Time:" + s);
		Match m=timePattern.Match (s);
		string stime = m.Groups[1].Value;
		string mstime = m.Groups [2].Value;

/*		int one = s.IndexOf ("s: ") + 3;
		int two = s.IndexOf ("ms:");
		string stime = s.Substring (one,two-one);
		//print ("stime:" + stime);
		one = s.IndexOf ("ms: ") + 4;
		two = s.IndexOf ("---",one);
		string mstime = s.Substring (one,two-one);
		//print ("mstime:" + mstime);
		if(mstime.StartsWith("0")){
			mstime = "0";
		}
*/
		double myTime = double.Parse (stime) + double.Parse (mstime) / 1000000.0;
		if (firstTimeSeen == -1) {
			firstTimeSeen=myTime;
		}

		//tidy for +13 secs
		myTime = myTime - firstTimeSeen;

		timeData.Add (myTime);
	}

	void handleAcc(string s){
		//print ("Handle Acc:" + s);
		float[] f = new float[3];
		s = s.Substring (4);
		string[] sa = s.Split (',');
		f [0] = float.Parse(sa [0].Trim ());
		f [1] = float.Parse(sa [1].Trim ());
		f [2] = float.Parse(sa [2].Trim ());
		accData.Add (f);
	}

	void handleGyro(string s){
		//print ("Handle Gyro:" + s);
		double[] f = new double[3];
		s = s.Substring (4);
		string[] sa = s.Split (',');
		f [0] = double.Parse(sa [0].Trim ());
		f [1] = double.Parse(sa [1].Trim ());
		f [2] = double.Parse(sa [2].Trim ());

		calculateGyroHeading (f, 0, (double) timeData[timeData.Count-1]);
		calculateGyroHeading (f, 1, (double) timeData[timeData.Count-1]);
		calculateGyroHeading (f, 2, (double) timeData[timeData.Count-1]);

		float[] xyz = new float[3];
		xyz [0] = (float)(headingNow [0]);
		xyz [1] = (float)(headingNow [1]);
		xyz [2] = (float)(headingNow [2]);

		gyroData.Add(xyz);
	}

	void calculateGyroHeading(double[] data, int index, double time)
	{

		double gyro = 0;
		gyro = data[index];

		time *= 1000;
		
		if (lastMsCountGood[index])
		{
			//calculate heading
			double timechange = time - lastMsCount[index];
			double timeChangeSeconds = (double)timechange;

			headingNow[index] += timeChangeSeconds * gyro;
			print (headingNow [index]);
		}
		
		lastMsCount[index] = time;
		lastMsCountGood[index] = true;
	}

	void handleMag(string s){
		//print ("Handle Mag:" + s);
		float[] f = new float[3];
		s = s.Substring (4);
		string[] sa = s.Split (',');
		f [0] = float.Parse(sa [0].Trim ());
		f [1] = float.Parse(sa [1].Trim ());
		f [2] = float.Parse(sa [2].Trim ());
		magData.Add (f);
	}


	void init(){
		timeData = new ArrayList ();
		accData = new ArrayList ();
		gyroData = new ArrayList ();
		magData = new ArrayList ();
	}

	public void dump(float[] f, string name){
		print (name + ":" + " x:" + f [0] + " y:" + f [1] + " z:" + f [2]);
	}

	// Use this for initialization
	void Start () {
		init ();
		Load ();
	}

	public void Load()
	{
		readTextFile ("Assets/Data/SwingSample2.csv");
		print ("DataFileLength:"+timeData.Count);
		//writeDataToFile ();
		//writeHeaders ();

	}
	
	// Update is called once per frame
	void Update () {
		double t = Time.time;
//		print ("woo:"+counter+":"+(t+120.0));
		if (counter >= timeData.Count) {
			return;
		}
		double nextTime = (double) timeData[counter];
		while (nextTime < t) {
			counter++;
			nextTime = (double) timeData[counter];
		}
		if ((counter - 1) > 0) {
			accNow = (float[])accData [counter - 1];
			gyroNow = (float[])gyroData [counter - 1];
			timeNow = (double) timeData[counter - 1];
			//writeLine ();
		}
		//dump(accNow,"Acc");
		//dump (gyroNow,"Ang");

	}
}
