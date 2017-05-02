using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PhaseEstimator
{
    // 4 seconds or so of history
    float[] angleHistory=new float[256];
    int angleHistoryPos=0;
    float lastAngle=0;
    // find maxima and minima
   
    int lastQuadrant=-1;
    int loopCount=0;
    float lastPhaseOut=0;
  
    static float phaseStepFromCycleTime(float cycleTime)
    {
        return (2f*Mathf.PI)/cycleTime;
    }
    
    static float cycleTimeFromPhaseStep(float phaseStep)
    {
        return (2f*Mathf.PI)/phaseStep;        
    }
    
    public void onAngle(float angle)
    {
        lastAngle=angle;
        angleHistory[angleHistoryPos]=angle;
        angleHistoryPos++;
        if(angleHistoryPos>=angleHistory.Length)
        {
            angleHistoryPos=0;
        }
        calculatePhaseAmplitude();
    }
    
    float currentPhase=0;
    float maxPhaseStep=phaseStepFromCycleTime(2.3f);
    float minPhaseStep=phaseStepFromCycleTime(3.0f);
    float currentPhaseStep=phaseStepFromCycleTime(2.6f);
    float currentAmplitude=0f;
    
    void calculatePhaseAmplitude()
    {
        float maxAngle=0;
        float minAngle=0;
        // first calculate amplitude
        for(int c=0;c<angleHistory.Length;c++)
        {
            if(angleHistory[c]>maxAngle)
            {
                maxAngle=angleHistory[c];
            }
            if(angleHistory[c]<minAngle)
            {
                minAngle=angleHistory[c];
            }
        }
        currentAmplitude=Mathf.Max(1f,Mathf.Max(maxAngle,-minAngle));
        
        // now simple phase locked loop
        
        currentPhase+=currentPhaseStep*Time.deltaTime;
        
        float compareVal=Mathf.Sin(currentPhase)*currentAmplitude;
        float compareSlope=Mathf.Cos(currentPhase);
        float error=(compareVal-lastAngle)/currentAmplitude;
        if(compareSlope<0)
        {
            error=-error;
        }
        
        currentPhase-=3f*error*Time.deltaTime;
        currentPhaseStep-=error*Time.deltaTime;
        currentPhaseStep=Mathf.Max(currentPhaseStep,minPhaseStep);
        currentPhaseStep=Mathf.Min(currentPhaseStep,maxPhaseStep);
        

    }
    
    public void getSwingPhaseAndQuadrant(out float phase,out int quadrant,out float amplitude,out float cycleTime,out int swingCycles)
    {
        // only ever step forward
        if(currentPhase>=lastPhaseOut)
        {
            lastPhaseOut=currentPhase;
        }
        // phase as 0-4
        phase=Mathf.Repeat(lastPhaseOut,2f*Mathf.PI) * (2f / Mathf.PI);
        swingCycles=(int)(phase/4);
        // quadrant as 0,1,2,3
        quadrant=(int)phase;
        
        cycleTime=cycleTimeFromPhaseStep(currentPhaseStep);
        amplitude=currentAmplitude;
        float compareVal=Mathf.Sin(currentPhase)*currentAmplitude;
        float error=(compareVal-lastAngle)/currentAmplitude;
        Debug.Log(currentAmplitude+","+currentPhase+","+lastAngle+","+compareVal+":"+currentPhaseStep+":"+cycleTimeFromPhaseStep(currentPhaseStep)+":"+error+":"+quadrant);
    }
    
    
}