using UnityEngine;
using System.Collections;

public class AbstractDataReader : MonoBehaviour {

	protected volatile float[]  accNow = {0f,0f,0f};
	protected volatile float[] gyroNow = {0f,0f,0f};
	protected volatile float[] magNow = {0f,0f,0f};
	protected volatile double[] headingNow = { 0, 0, 0 }; //degrees
    
    public const int CONNECTION_FULL=0;
    public const int CONNECTION_PARTIAL=1;
    public const int CONNECTION_NONE=2;
    
    protected int connectionState=CONNECTION_FULL;

	public bool lowPassfilter;
	public float smoothFactor = 0.015f;

	public float[] getAccNow(){
		return accNow;
	}
    
    public int getConnectionState()
    {
        return connectionState;
    }
	
	public float[] getHeadingsNow(){
		//return gyroNow;
		float[] xyz = new float[3];
		xyz [0] = (float)(headingNow [0]);
		xyz [1] = (float)(headingNow [1]);
		xyz [2] = (float)(headingNow [2]);
		return xyz;
	}
	
	public float[] getMagNow(){
		return magNow;
	}

	public float[] getGyroNow(){
		return gyroNow;
	}

	public void dumpVals(){
		print ("Time:" + getTimeNow ());
		print ("Base:"+accNow[0]+":"+accNow[1]+":"+accNow[2]+":"+gyroNow[0]+":"+gyroNow[1]+":"+gyroNow[2]+":"+magNow[0]+":"+magNow[1]+":"+magNow[2]+":");
		print ("Heading:"+headingNow [0] + " : " + headingNow [1] + " : " + headingNow [2]);
	}

	public virtual double getTimeNow(){
		return (double) Time.time;
	}

	protected float[] lowpass(float[]input, float[] output){
		if (output == null || !lowPassfilter){
			return input;
		}
		for(int i=0;i<input.Length;i++){
			output [i] = output [i] + smoothFactor * (input [i] - output [i]);
		}
		return output;
	}

	protected double[] lowpass(double[]input, double[] output){

		if (output == null  || !lowPassfilter){
			return input;
		}
		for(int i=0;i<input.Length;i++){
			output [i] = output [i] + smoothFactor * (input [i] - output [i]);
		}
		return output;
	}

}
