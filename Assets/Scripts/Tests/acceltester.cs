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
    
    Text debugText;
    Text debugText2;

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
        debugText=GameObject.Find("debugtext").GetComponent<Text>();	
        debugText2=GameObject.Find("debugtext2").GetComponent<Text>();	
        //debugText.text=Application.persistentDataPath;
        FindDebugGraphs();
        m_Gyro=new GyroConnector();
        m_Gyro.init();
        m_Gyro.mAccelerometer.startLog(Application.persistentDataPath+"/swing-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".csv");
        replayCSV=null;
	}
    
    void OnDestroy() 
    {
        m_Gyro.stop();
    }

    
	// Update is called once per frame
	void Update () 
    {
        m_Gyro.readData();

        
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

        debugText2.text=m_Gyro.dbgTxt;
        debugText.text="p:"+m_Gyro.mTracker.swingProbability+":"+m_Gyro.mAngle;
        GameObject angler=GameObject.Find("angler");
        if(angler!=null)
        {
            angler.transform.localEulerAngles=new Vector3(0f,0f,m_Gyro.mAngle);
        }
	}
    

}