using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPDataReader : AbstractDataReader {

	Thread receiveThread;

	// udpclient object
	UdpClient socket;

	// public
	public string IP = "127.0.0.1";
	//public string IP = "192.168.43.1";
//	public string IP = "10.154.132.223";
	public int port = 2323; // define > init

	// infos
	public string lastReceivedUDPPacket="";
	public string allReceivedUDPPackets=""; // clean up this from time to time!

	private IPEndPoint remoteEndPoint;

	private bool alive = true;

	// Use this for initialization
	void Start () {
		init();
	}

	void OnApplicationQuit(){
		alive = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void init()
	{
		print("UDPSend.init()");


		// status
		print("Sending to "+IP+" : "+port);

		receiveThread = new Thread(
		new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}

	private void sendString(string message)
	{
		try
		{
			byte[] data = Encoding.UTF8.GetBytes(message);
			socket.Send(data, data.Length, remoteEndPoint);
		}
		catch (Exception err)
		{
			print(err.ToString());
		}
	}

	private float floatFromBytes(byte[]array,int pos)
	{
		byte[] byteData = new byte[4];
		Array.Copy (array, pos, byteData, 0, 4);
		if (BitConverter.IsLittleEndian) {
			Array.Reverse (byteData);
		}
		return BitConverter.ToSingle (byteData, 0);
	}

	private  void ReceiveData()
	{
		socket = new UdpClient (new IPEndPoint (IPAddress.Any, 0));
		socket.Client.ReceiveTimeout = 1000;
		socket.Client.SendTimeout = 1000;
		remoteEndPoint = new IPEndPoint (IPAddress.Parse (IP), port);
		//sendString ("Hi");

		while (alive) {
			IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, port);
			byte[] data = socket.Receive (ref anyIP);
			float ang = floatFromBytes (data, 0);
			double[] angs = new double[]{ 0, (float)ang, 0 };
			headingNow = angs;
		}
	}

	public string getLatestUDPPacket()
	{
		allReceivedUDPPackets="";
		return lastReceivedUDPPacket;
	}
	
}