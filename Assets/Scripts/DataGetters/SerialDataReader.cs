using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public delegate void GotDataCallback(string data);

public class SerialDataReader : AbstractDataReader {

	public string comPort = "/dev/cu.usbmodem1411";
	public int baudrate = 115200;
	private SerialCommunicator sc;

	// Use this for initialization
	void Start () {
		GotDataCallback callbackMethod = new GotDataCallback(handleIncomingData);
		sc = new SerialCommunicator(comPort, baudrate, callbackMethod);
		Thread serialThread = new Thread(new ThreadStart(sc.go));
		serialThread.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnApplicationQuit() {
		sc.end ();
	}

	void handleIncomingData(string value)
	{
		string[] data = value.Split(','); //My arduino script returns a 3 part value (IE: 12,30,18)

		if (data.Length == 13) //Check if we have a full dataset
		{
			//now store that data
			float[] acc = new float[3];
			acc[0] = float.Parse (data [0]);
			acc [1] = float.Parse (data [1]);
			acc [2] = float.Parse (data [2]);

			accNow = lowpass (acc, accNow);

			float[] mag = new float[3];
			mag [0] = float.Parse (data [3]);
			mag [1] = float.Parse (data [4]);
			mag [2] = float.Parse (data [5]);

			magNow = lowpass (mag, magNow);

			float[] gyro = new float[3];
			gyro [0] = float.Parse (data [6]);
			gyro [1] = float.Parse (data [7]);
			gyro [2] = float.Parse (data [8]);

			gyroNow = lowpass (gyro, gyroNow);

			double[] heading = new double[3];
			heading[0] = double.Parse (data [9]);
			heading[1] = double.Parse (data [10]);
			heading[2] = double.Parse (data [11]);

			headingNow = lowpass (heading, headingNow); 
		}
	}
}

public class SerialCommunicator
{
	SerialPort stream;
	string value = "EMPTY";
	GotDataCallback _callbackMethod;
	string port;
	int baudrate;
	bool alive = true;

	public SerialCommunicator(string port, int baudrate, GotDataCallback callbackMethod)
	{
		this._callbackMethod = callbackMethod;
		this.port = port;
		this.baudrate = baudrate;
		stream = new SerialPort(port, baudrate); //Set the port (com4) and the baud rate
		stream.RtsEnable = true;
		stream.Open(); //Open the Serial Stream.
	}

	public void end()
	{
		alive = false;
	}

	public void go()
	{
		while (alive)
		{
			value = stream.ReadLine(); //Read the information
			if (_callbackMethod != null)
			{
				_callbackMethod(value);
			}
			stream.BaseStream.Flush(); //Clear the serial information so we assure we get new information.
		}
		stream.Close(); //clean up the serial stream
	}

}
