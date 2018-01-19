using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TensorFlow;

using System;
using System.Reflection;

public class TensorFlowSwingTracker
{
    
    private float swingAngle;
    
    public TextAsset graphModel;
    public string inputName="input_1";
    public string outputName="out_0";
    
    public TextAsset testData;
    TFSession session;
    TFGraph graph;
    TFSession.Runner runner;
    
    TFTensor inputTensor;
    TFShape inputShape;
    float[] inputData;
    
    public bool useLog=false;
    int logPos=0;
    
	// Use this for initialization
	public TensorFlowSwingTracker() 
    {
        graphModel=Resources.Load("model-korea-dropout-50") as TextAsset;
        testData=Resources.Load("20171016134432-10000045-6c7b0a05-20021085-Hi") as TextAsset;
        swingAngle=0f;
        // Quaternion q45x=Quaternion.Euler(45,0,0);
        // Quaternion q45y=Quaternion.Euler(0,45,0);
        // Quaternion q45z=Quaternion.Euler(0,0,45);
        // print(q45x.w+","+q45x.x+","+q45x.y+","+q45x.z+":"+q45y+":"+q45z);
        
        Application.targetFrameRate=60;
        if(!SystemInfo.supportsAccelerometer)
        {
            useLog=true;
            Application.targetFrameRate=60;
        }
        inputShape=new TFShape(1,512,7);
        inputData=new float[512*7];
#if UNITY_ANDROID
        TensorFlowSharp.Android.NativeBinding.Init();
#endif		
        graph = new TFGraph ();
        graph.Import (graphModel.bytes);
        session = new TFSession (graph);
        Input.compass.enabled=true;
        Input.gyro.enabled=true;
        
	}

   public Quaternion getCurrentDirection()
    {
        if(UnityEngine.XR.XRDevice.isPresent)
        {
            return UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        }
        return Quaternion.Euler(90,0,0);
    }

    
    
    Vector3 gyro,accel,rotatedAccel,rotatedGyro;
    Quaternion directionCorrection;
    
	// Update is called once per frame
	public float GetAngle() 
    {
        // get:
        // input is:
        // magnitude
        // accel (rotated by head vector)
        // gyro (rotated by head vector)
       
        // shift input data along
        Array.Copy(inputData,7,inputData,0,inputData.Length-7);
//        Array.Copy(inputData,0,inputData,7,inputData.Length-7);
        if(useLog)
        {
           gyro.x= BitConverter.ToSingle(testData.bytes,logPos+4);
           gyro.y= BitConverter.ToSingle(testData.bytes,logPos+8);
           gyro.z= BitConverter.ToSingle(testData.bytes,logPos+12);
           accel.x= BitConverter.ToSingle(testData.bytes,logPos+16);
           accel.y= BitConverter.ToSingle(testData.bytes,logPos+20);
           accel.z= BitConverter.ToSingle(testData.bytes,logPos+24);
           //logRotator.localRotation=Quaternion.Euler(0,0,BitConverter.ToSingle(testData.bytes,logPos+40));
           directionCorrection.w=BitConverter.ToSingle(testData.bytes,logPos+44);           
           directionCorrection.x=BitConverter.ToSingle(testData.bytes,logPos+48);           
           directionCorrection.y=BitConverter.ToSingle(testData.bytes,logPos+52);           
           directionCorrection.z=BitConverter.ToSingle(testData.bytes,logPos+56);           
           logPos+=16*4;
           if(logPos>=testData.bytes.Length)
           {
               logPos=0;
           }
        }else
        {
            gyro=Input.gyro.rotationRateUnbiased;
            accel=Input.acceleration;
            directionCorrection=getCurrentDirection();        
        }
        rotatedAccel=directionCorrection*accel;            
        rotatedGyro=directionCorrection*gyro;            
        float mag=Mathf.Sqrt(accel.x*accel.x+accel.y*accel.y+accel.z*accel.z);            
        const int basePos=511*7;
        inputData[basePos+0]=(mag*2.0f)-2.0f;
        inputData[basePos+1]=rotatedAccel.x;
        inputData[basePos+2]=rotatedAccel.y;
        inputData[basePos+3]=rotatedAccel.z;
        inputData[basePos+4]=rotatedGyro.x;
        inputData[basePos+5]=rotatedGyro.y;
        inputData[basePos+6]=rotatedGyro.z;
        
        inputTensor=TFTensor.FromBuffer(inputShape,inputData,0,512*7);
        // output is swing angle
        runner = session.GetRunner ();
        runner.AddInput(graph[inputName][0], inputTensor);
        TFTensor[] output=runner.Fetch(graph[outputName][0]).Run();
        	
        if(output!=null && output.Length>0){
            swingAngle=(output[0].GetValue()as Single[,])[0,0];

        }
        return swingAngle;
	}
}
    
    