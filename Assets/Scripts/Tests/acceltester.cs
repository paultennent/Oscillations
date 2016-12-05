using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class acceltester : MonoBehaviour {
    GyroConnector    m_Gyro;
    

    SimpleGraph[] debugGraphs;

    public TextAsset replayCSV;
    
    enum Mode
    {
        SHOW_GRAPHS,
    };
    
    Mode mode=Mode.SHOW_GRAPHS;

    Text debugText;
    Text debugText2;
    
    StreamWriter logWriter;
    
    float replayTime=0;
	int replayPos=0;
    float accelHistoryTime=0;
    float [] accelReplayData;
    float [] accelReplayTimes;


    void FindDebugGraphs()
    {
        
        int count=0;
        while(count<10)
        {
            if(GameObject.Find("debuggraph"+(count+1))==null)
            {
                break;
            }
            count+=1;
        }
        debugGraphs=new SimpleGraph[count];
        for(int c=0;c<debugGraphs.Length;c++)
        {
            debugGraphs[c]=GameObject.Find("debuggraph"+(c+1)).GetComponent<SimpleGraph>();
        }
    }
    
	// Use this for initialization
	void Start () {
        Input.gyro.enabled=true;
        debugText=GameObject.Find("debugtext").GetComponent<Text>();	
        debugText2=GameObject.Find("debugtext2").GetComponent<Text>();	
        //debugText.text=Application.persistentDataPath;
        FindDebugGraphs();
        m_Gyro=new GyroConnector();
        m_Gyro.init();
        replayCSV=null;
	}
    
    void OnDestroy() 
    {
        m_Gyro.stop();
    }

    
    float[]angleHistory=new float[512];
    int angleHistoryPos=0;
    
	// Update is called once per frame
	void Update () {
//            AndroidJavaObject tg=new AndroidJavaObject("android.media.ToneGenerator",5,0x64);
//        ToneGenerator tg = new ToneGenerator(AudioManager.STREAM_NOTIFICATION, ToneGenerator.MAX_VOLUME );
//            tg.Call<bool>("startTone",41);

        if(accelReplayTimes!=null)
        {
            replayTime+=Time.deltaTime;
            while(replayPos<(accelReplayTimes.Length-1) && accelReplayTimes[replayPos]<replayTime )
            {
                replayPos++;
                if(replayPos<accelReplayData.Length && replayPos<accelReplayTimes.Length)
                {
                    float mag=(float)accelReplayData[replayPos];
                    float time=(float)accelReplayTimes[replayPos];
//                    m_Tracker.OnAccelerometerMagnitude(mag,time);
                }
            }
        }else
        {
            m_Gyro.readData();
#if UNITY_ANDROID && !UNITY_EDITOR
/*            foreach (AccelerationEvent accEvent in Input.accelerationEvents) 
            {
                float mag=Mathf.Sqrt(accEvent.acceleration.x*accEvent.acceleration.x+accEvent.acceleration.y*accEvent.acceleration.y+accEvent.acceleration.z*accEvent.acceleration.z);            
                accelHistoryTime+=accEvent.deltaTime;
                m_Tracker.OnAccelerometerMagnitude(mag,accelHistoryTime);
                if(logWriter!=null)
                {
                    logWriter.Write(accelHistoryTime+","+mag+","+m_Gyro.mAngle+"\n");
                }
            }*/
#endif            
        }

        
        if(logWriter!=null)
        {
            logWriter.Flush();
        }
        switch(mode)
        {
            case Mode.SHOW_GRAPHS:
            
                for(int c=0;c<debugGraphs.Length;c++)
                {
                    float[] points=m_Gyro.mTracker.GetDebugGraph(c);
                    if(points!=null)
                    {
                        float[] fixedRange=m_Gyro.mTracker.GetDebugGraphRange(c);
                        if(fixedRange!=null)
                        {
                            debugGraphs[c].FixRange(fixedRange);
                        }                        
                        debugGraphs[c].SetPoints(points);
                    }
                }
                break;
        }
        debugText2.text=m_Gyro.dbgTxt;
        debugText.text="p:"+m_Gyro.mTracker.swingProbability+":"+m_Gyro.mAngle;
        GameObject angler=GameObject.Find("angler");
        if(angler!=null)
        {
            angler.transform.localEulerAngles=new Vector3(0f,0f,m_Gyro.mAngle);
        }
	}
    

}