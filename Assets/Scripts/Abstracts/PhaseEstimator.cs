using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PhaseEstimator
{
    // 1 second shift (i.e. less than one swing cycle)
    const int SIN_SHIFT=64;
    // 4 seconds or so of history
    float[] angleHistory=new float[256];
    float[] dtHistory=new float[256];
    
    float[] sinBuf=new float[256+SIN_SHIFT*2+1];
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
        dtHistory[angleHistoryPos]=Time.deltaTime;
        angleHistoryPos++;
        if(angleHistoryPos>=angleHistory.Length)
        {
            angleHistoryPos=0;
        }
        calculatePhaseAmplitude();
    }
    
    float currentPhase=0;
    float maxPhaseStep=phaseStepFromCycleTime(2.3f);
    float minPhaseStep=phaseStepFromCycleTime(4.0f);
//    float minPhaseStep=phaseStepFromCycleTime(3.0f);
    float currentPhaseStep=2f;//phaseStepFromCycleTime(2.6f);
    float currentAmplitude=0f;
    
    float calcDifference(float []sinBuf,int startSin,float[]angleHist,int startHist)
    {
        float total=0;
        int historyPos=startHist;
        int sinPos=startSin;
        for(int c=0;c<angleHist.Length;c++)
        {
            historyPos--;
            if(historyPos<0)historyPos=angleHist.Length-1;
            float histVal=angleHist[historyPos];
            float sinVal=sinBuf[sinPos];
            float diff=(histVal-sinVal);
            total+=Mathf.Abs(diff);
            sinPos++;
        }
        return total;
    }
    
    float scoreSinOffset(float offset)
    {
        float total=0f;
        float sinPhase=currentPhase-offset;
        int historyPos=angleHistoryPos;
        for(int c=0;c<angleHistory.Length;c++)
        {
            historyPos--;
            if(historyPos<0)historyPos=angleHistory.Length-1;
            
            float sinVal=Mathf.Sin(sinPhase)*currentAmplitude;
            float histVal=angleHistory[historyPos];
            float diff=histVal-sinVal;
            total+=diff*diff;
            sinPhase-=dtHistory[historyPos]*currentPhaseStep;            
        }
        return total;
    }
    
    void calculatePhaseAmplitude()
    {
        currentPhase+=currentPhaseStep*Time.deltaTime;

        float sineShift=0;
        
        
        
        float maxAngle=0;
        float minAngle=0;
        
        int sinePos=angleHistoryPos;
        
        // first calculate amplitude
        for(int c=0;c<angleHistory.Length;c++)
        {
            sinePos--;
            if(sinePos<0)sinePos=angleHistory.Length-1;
            
            
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

        
        float dt=Time.deltaTime;
        float sinPhase=currentPhase+((float)SIN_SHIFT)*dt*currentPhaseStep;
        for(int c=0;c<sinBuf.Length;c++)
        {
            sinBuf[c]=Mathf.Sin(sinPhase)*currentAmplitude;
            sinPhase-=dt*currentPhaseStep;
        }
        
        float []positionScores=new float[SIN_SHIFT*2+1];
        float bestScore=0;
        float bestPos=-999;
        // compare all sin buffers
        for(int c=-SIN_SHIFT;c<SIN_SHIFT;c++)
        {
            positionScores[c+SIN_SHIFT]=scoreSinOffset(dt*(float)c);
            
            
            positionScores[c+SIN_SHIFT]=calcDifference(sinBuf,c+SIN_SHIFT,angleHistory,angleHistoryPos);
            if(bestPos==-999 || positionScores[c+SIN_SHIFT]<bestScore)
            {
                bestPos=c;
                bestScore=positionScores[c+SIN_SHIFT];
            }
        }
        
        //Debug.Log(minPhaseStep+":"+currentPhaseStep+":"+currentPhase+":"+ bestPos+":"+bestScore+":"+Mathf.Sin(currentPhase)*currentAmplitude+":"+lastAngle);
        if(bestPos>0)
        {
            
//            currentPhase-=Mathf.Min(bestPos,10f)*dt;
            currentPhase-=5f*dt;
            currentPhaseStep-=1f*dt;
        }else if(bestPos<0)
        {
//            currentPhase+=Mathf.Min(-bestPos,10f)*dt;
            currentPhase+=5f*dt;
            currentPhaseStep+=1f*dt;
        }
        currentPhaseStep=Mathf.Max(currentPhaseStep,minPhaseStep);
        currentPhaseStep=Mathf.Min(currentPhaseStep,maxPhaseStep);

        
        
            
/*
        
        // now simple phase locked loop
        
        currentPhase+=currentPhaseStep*Time.deltaTime;
        
        float compareVal=Mathf.Sin(currentPhase)*currentAmplitude;
        float compareSlope=Mathf.Cos(currentPhase);
        float error=(compareVal-lastAngle)/currentAmplitude;
        if(compareSlope<0)
        {
            error=-error;
        }
        
        currentPhase-=.3f*error*Time.deltaTime;
        currentPhaseStep-=.03f*error*Time.deltaTime;
        currentPhaseStep=Mathf.Max(currentPhaseStep,minPhaseStep);
        currentPhaseStep=Mathf.Min(currentPhaseStep,maxPhaseStep);*/
        

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
//        Debug.Log(currentAmplitude+","+currentPhase+","+lastAngle+","+compareVal+":"+currentPhaseStep+":"+cycleTimeFromPhaseStep(currentPhaseStep)+":"+error+":"+quadrant);
    }
    
    
}