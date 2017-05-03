using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PhaseEstimator
{
    // 1 second shift (i.e. less than one swing cycle)
    const int SIN_SHIFT=64;
    // 5 seconds or so of history (but we heavily weight towards start)
    float[] angleHistory=new float[256];
    float[] dtHistory=new float[256];
    
    int angleHistoryPos=0;
    float lastAngle=0;
    float lastTime=-9999f;
    float lastDT=0.1666f;
    float dtAccumulator=0;
    // find maxima and minima
   
    int lastQuadrant=-1;
    int loopCount=0;
    float lastPhaseOut=0;

    // these are phases and amplitude used by the internal PLL style phase matcher
    float currentPhase=0;
    float maxPhaseStep=phaseStepFromCycleTime(2.3f);
    float minPhaseStep=phaseStepFromCycleTime(4.0f);
//    float minPhaseStep=phaseStepFromCycleTime(3.0f);
    float currentPhaseStep=2f;//phaseStepFromCycleTime(2.6f);
    float currentAmplitude=1f;
    
    // these are render phases and ampitudes, used to output a smoothly varying signal
    // their parameters slowly shift to be equal to the matched phase / amplitude
    float renderPhaseSmoothingTime=1f;
    float renderPhaseStep=2f;
    float renderAmplitude=1f;
    float renderPhase=0f;

    
    
    
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
        dtAccumulator+=Time.deltaTime;
        doStep();
        if(Time.time-lastTime<.016f)
        {
            return;
        }
        lastDT=dtAccumulator;
        dtAccumulator=0f;
        lastAngle=angle;
        angleHistory[angleHistoryPos]=angle;
        dtHistory[angleHistoryPos]=lastDT;
        angleHistoryPos++;
        if(angleHistoryPos>=angleHistory.Length)
        {
            angleHistoryPos=0;
        }
        adjustPhaseAmplitude();
    }
    
    
    float scoreSinOffset(float offset)
    {
        float lowPassCoeff=0.05f;
        
        float total=0f;
        float sinPhase=currentPhase-offset;
        int historyPos=angleHistoryPos;
        
        float positionWeight=1;
        float weightStep=.5f/(float)angleHistory.Length;
        
        for(int c=0;c<angleHistory.Length;c++)
        {
            historyPos--;
            if(historyPos<0)historyPos=angleHistory.Length-1;
            
            float sinVal=Mathf.Sin(sinPhase)*currentAmplitude;
            float histVal=angleHistory[historyPos];

            
        // weight more recent points higher, and also weight zero points higher too as they're more perceptible (bottom of swing)
            float weight=positionWeight*(2f-Mathf.Abs(sinVal));
//            positionWeight-=weightStep;
            float mult=-(histVal*sinVal);
            total+=mult;
//            float diff=histVal-sinVal;
//            total+=diff*diff;
            sinPhase-=dtHistory[historyPos]*currentPhaseStep;
        }
        return total;
    }

    void doStep()
    {
        // update internal phase
        currentPhase+=currentPhaseStep*Time.deltaTime;
        
        // update render phase
        renderPhase+=renderPhaseStep*Time.deltaTime;
        
        Debug.Log(currentPhase+":"+renderPhase+":"+(currentPhase-renderPhase));
        // move render phase closer to internal phase etc.
//        float filterConstant=.1f;
        float filterConstant=Time.deltaTime/(renderPhaseSmoothingTime+Time.deltaTime);
        // move renderPhase closer to the currentPhase
        renderPhase= filterConstant * currentPhase + (1f-filterConstant) * renderPhase;
        // move renderPhaseStep closer to the currentPhaseStep
        renderPhaseStep= filterConstant * currentPhaseStep + (1f-filterConstant) * renderPhaseStep;
        // move renderAmplitude close to the current amplitude
        renderAmplitude= filterConstant * currentAmplitude + (1f-filterConstant) * renderAmplitude;
    }
    
    void adjustPhaseAmplitude()
    {
        float sineShift=0;
        
        
        
        float maxAngle=0;
        float minAngle=0;
        
        float minFirstSecond=0;
        float maxFirstSecond=0;
        
        int histPos=angleHistoryPos;
        
        // first calculate amplitude
        for(int c=0;c<angleHistory.Length;c++)
        {
            histPos-=1;
            if(histPos<0)
            {
               histPos=angleHistory.Length-1;
            }
            
            
            if(angleHistory[histPos]>maxAngle)
            {
                maxAngle=angleHistory[histPos];
                if(c<60)maxFirstSecond=maxAngle;
            }
            if(angleHistory[histPos]<minAngle)
            {
                minAngle=angleHistory[histPos];
                if(c<60)minFirstSecond=minAngle;
            }
        }
        
        currentAmplitude=Mathf.Max(1f,Mathf.Max(maxAngle,-minAngle));
        float firstSecondAmplitude=Mathf.Max(maxFirstSecond,-minFirstSecond);
        if(firstSecondAmplitude<5 || (minAngle>-5f || maxAngle<5))
        {
            // nothing to work with so don't update phase estimations
            return;
        }

        float dt=lastDT;
        
        float []positionScores=new float[SIN_SHIFT*2+1];
        float bestScore=0;
        float bestPos=-999;
        // compare all sin buffers
        for(int c=-SIN_SHIFT;c<SIN_SHIFT;c++)
        {
            positionScores[c+SIN_SHIFT]=scoreSinOffset(.5f*dt*(float)c);                       
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
//        float thisPhase=currentPhase;
        float thisPhase=renderPhase;
        // only ever step forward
        if(thisPhase>=lastPhaseOut)
        {
            lastPhaseOut=thisPhase;
        }
        amplitude=renderAmplitude;
//        amplitude=currentAmplitude;
        // phase as 0-4
        phase=Mathf.Repeat(thisPhase,2f*Mathf.PI) * (2f / Mathf.PI);
        swingCycles=(int)(phase/4);
        // quadrant as 0,1,2,3
        quadrant=(int)phase;
        
        cycleTime=cycleTimeFromPhaseStep(renderPhaseStep);
//        cycleTime=cycleTimeFromPhaseStep(currentPhaseStep);
        float compareVal=Mathf.Sin(renderPhase)*amplitude;
        float error=(compareVal-lastAngle)/amplitude;
        Debug.Log(lastAngle+","+compareVal);
//        Debug.Log(amplitude+","+phase+","+lastAngle+","+compareVal+":"+currentPhaseStep+":"+cycleTime+":"+error+":"+quadrant);
    }
    
    
}