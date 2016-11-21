using UnityEngine;
using System.Collections;
using Phidgets;
using Phidgets.Events;
using System;

public class PhidgetDataReader : AbstractDataReader {

	Spatial spatial;

	private volatile double[] lastMsCount = { 0, 0, 0 };
	private volatile bool[] lastMsCountGood = { false, false, false };

	double[] newHeading=new double[3];

	double temp=0;

	// Use this for initialization
	void Start () {
		spatial = new Spatial ();
		spatial.open ();
		spatial.waitForAttachment (1000);
		spatial.DataRate = 8; // set datarate to 64Hz - framerate shouldn't be higher than this anyway
		spatial.SpatialData += new SpatialDataEventHandler(spatial_SpatialData);
	}

	private void spatial_SpatialData(object sender, SpatialDataEventArgs e){
		accNow [0] = (float) e.spatialData [0].Acceleration [0];
		accNow [1] = (float) e.spatialData[0].Acceleration[1];
		accNow [2] = (float) e.spatialData[0].Acceleration[2];
		Array.Copy(headingNow,newHeading,3);
		calculateGyroHeading(e.spatialData, 0); //x axis
		calculateGyroHeading(e.spatialData, 1); //y axis
		calculateGyroHeading(e.spatialData, 2); //z axis
//		headingNow = lowpass(newHeading,headingNow);
		headingNow = newHeading;
	}

	private void calculateGyroHeading(SpatialEventData[] data, int index)
	{
		double gyro = 0;
		for (int i = 0; i < data.Length; i++)
		{
			gyro = data[i].AngularRate[index];
			
			if (lastMsCountGood[index])
			{
				//calculate heading
				double timechange = (double)data[i].Timestamp.TotalMilliseconds - lastMsCount[index]; // in ms
				double timeChangeSeconds = (double)timechange / 1000.0;
				
//				if (index == 1)
//				    print("X Gyro rate: " + gyro.ToString() + " Time: " + timeChangeSeconds.ToString() + ", " + data[i].Timestamp.TotalMilliseconds.ToString());
				
				newHeading[index] += timeChangeSeconds * gyro;
			}
			
			lastMsCount[index] = data[i].Timestamp.TotalMilliseconds;
			lastMsCountGood[index] = true;
		}
	}

}
