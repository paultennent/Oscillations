using UnityEngine;
using System;

// NEEDS to sync up to gyro when we have it
// if we don't have gyro, continue based on accelerometer 
// do scoring based on gyro position if we have one

public class SwingTracker 
{
    const int WINDOW_SIZE=160;
    const int WINDOW_SHIFT=20;
//    const float INITIAL_SWING_PERIOD=1.6f;
    const float INITIAL_SWING_PERIOD=1.8f;
    
    float outPhase=0.5f;
    float lastTime=0;
    float swingStep=(2.0f*Mathf.PI) / INITIAL_SWING_PERIOD;
    float scale=0;
    float offset=0;

    float []gyroHistory=new float[WINDOW_SIZE];
    bool []gyroValidHistory=new bool[WINDOW_SIZE];
    float []zHistory=new float[WINDOW_SIZE];
    float []magHistory=new float[WINDOW_SIZE];
    float []timeHistory=new float[WINDOW_SIZE];
    float []sinWindow=new float[WINDOW_SIZE+2*WINDOW_SHIFT+1];
    float []currentErrors=new float[WINDOW_SHIFT*2+1];
    float []offsetHistory=new float[WINDOW_SIZE];
    float []errorHistory=new float[WINDOW_SIZE];
    float []probabilityHistory=new float[WINDOW_SIZE];
    int magHistoryPos=0;
    int magHistoryLen=0;
    
    float accelAngleMultiplier=1.0f;
    
    public float[]DeLoop(float[]array)
    {
        float[]retVal=new float[WINDOW_SIZE];
        int loopPos=magHistoryPos;
        for(int c=0;c<WINDOW_SIZE;c++)
        {
            loopPos-=1;
            if(loopPos<0)
            {
                loopPos+=WINDOW_SIZE;
            }
            retVal[c]=array[loopPos];
        }
        return retVal;
    }
    
    public float[]GetDebugGraph(int count)
    {
        switch(count)
        {
            case 0:
                float[]retVal=new float[WINDOW_SIZE];
                for(int c=0;c<WINDOW_SIZE;c++)
                {
                    retVal[c]=sinWindow[c+WINDOW_SHIFT];
                }
                return retVal;
            case 1:
                return DeLoop(magHistory);
            case 2:
                return DeLoop(offsetHistory);
            case 3:
                return DeLoop(errorHistory);
            case 4:
                return DeLoop(probabilityHistory);
            default:
                return null;
        }
    }
    public float[]GetDebugGraphRange(int num)
    {
        float[]retVal;
        switch(num)
        {
            case 2:
                retVal=new float[2];
                retVal[0]=-20;
                retVal[1]=20;
                return retVal;
            case 3:
                retVal=new float[2];
                retVal[0]=0;
                retVal[1]=5;
                return retVal;
            case 4:
                retVal=new float[2];
                retVal[0]=0;
                retVal[1]=1;
                return retVal;
            default:
                return null;
        }
    }
 
    public float estimatedPeriod=INITIAL_SWING_PERIOD;
    public float swingProbability=0f;
    
    float moveErrorAccumulator=20;
   
    int logCount=0;
   
    public float OnAccelerometerMagnitude(float mag,float magTime,bool hasGyro,float gyroAngle,float zAccel)
    {
        int lastMagHistoryPos=magHistoryPos;
        float dt = magTime-lastTime;
        lastTime=magTime;
        // update the output estimate and phase estimate
        float outVal=Mathf.Sin(outPhase);
        outPhase+=dt*swingStep;
        // update the history buffer
        zHistory[magHistoryPos]=zAccel;
        gyroHistory[magHistoryPos]=gyroAngle;
        gyroValidHistory[magHistoryPos]=hasGyro;
        magHistory[magHistoryPos]=mag; 
        timeHistory[magHistoryPos]=magTime;
        magHistoryPos++;
        if(magHistoryPos>=magHistory.Length)magHistoryPos=0;
        magHistoryLen+=1;
        // wait till we have full history before doing anything
        if(magHistoryLen>=sinWindow.Length)
        {
            // calculate the mean and variance of the history buffer
            // 2 pass method - this is slightly inefficient but only done once per data point
            float meanSum=0;
            for(int c=0;c<magHistory.Length;c++)
            {
                meanSum+=magHistory[c];
            }
            meanSum/=magHistory.Length;
            float varianceSum=0;
            for(int c=0;c<magHistory.Length;c++)
            {
                varianceSum+=(magHistory[c]-meanSum)*(magHistory[c]-meanSum);
            }
            varianceSum/=magHistory.Length;
            
            offset=-meanSum;
            scale=Mathf.Sqrt(varianceSum)/(0.5f*Mathf.Sqrt(2));

            // create a comparison buffer sine wave 
            // starts from WINDOW_SHIFT frames ahead
            float phaseSin=outPhase+dt*swingStep*(float)WINDOW_SHIFT;
            for(int c=0;c<sinWindow.Length;c++)
            {
                sinWindow[c]=Mathf.Sin(phaseSin);
                phaseSin-=dt*swingStep;
            }
//            Debug.Log(sinWindow[WINDOW_SHIFT]+"!!!"+Mathf.Sin(outPhase));

            // assuming we have a sensible history, now check whether our sine buffer fits nicely shifted +- WINDOW_SHIFT along
            // cross correlations with a least squared difference operator
            if(scale>0)
            {
                float invScale=1/scale; 
                for(int c=0;c<currentErrors.Length;c++)
                {
                    int offsetTest=c-WINDOW_SHIFT;
                    // always start comparison from earliest history point
                    int magPos=magHistoryPos;// position in history buffer - from earliest point, goes forward (in time and value)
                    int sinPos=WINDOW_SIZE+WINDOW_SHIFT+offsetTest;// position in sin buffer (from end, goes backwards in value / forwards in time)
                    currentErrors[c]=0;
                    // loop round just like the history buffers (starting from oldest = historyPos+1)

                    // weighting ups by 1 each time round the loop so that it prioritises recent points
                    float weighting=WINDOW_SIZE;
                    float divisor=0;
                    float totalError=0;
                    for(int d=0;d<WINDOW_SIZE;d++)
                    {
                        weighting+=1f;
                        divisor+=weighting;
                        float pointError=(magHistory[magPos]+offset)*invScale - sinWindow[sinPos];
                        totalError+=weighting*pointError*pointError;

                        sinPos-=1;
                        magPos+=1;
                        if(magPos>=magHistory.Length)magPos=0;
                    }
                    currentErrors[c]=totalError/divisor;
                }
                
                float minVal=currentErrors[0];
                int bestPos=0-WINDOW_SHIFT;
                for(int c=0;c<currentErrors.Length;c++)
                {
                    if(currentErrors[c]<minVal)
                    {
                        bestPos=c-WINDOW_SHIFT;
                        minVal=currentErrors[c];
                    }
                }
          
                moveErrorAccumulator=Mathf.Abs(bestPos)*0.03f+moveErrorAccumulator*0.97f;
                float probFromMoveErrors=Mathf.Max(0,1f-moveErrorAccumulator);
                float probFromBestError=Mathf.Min(Mathf.Max(1-(0.5f*currentErrors[WINDOW_SHIFT]-.25f),0),1);
                float probFromRangeDifference=Mathf.Min(1,Mathf.Max(0,currentErrors[0]/currentErrors[bestPos+WINDOW_SHIFT]-1.5f));
                //swingProbability=probFromMoveErrors;
                swingProbability=probFromBestError*probFromMoveErrors*probFromRangeDifference;

                logCount++;
                if((logCount&63)==0)
                {
                    Debug.Log(probFromMoveErrors+":"+probFromBestError+":"+probFromRangeDifference+":"+bestPos);
                }
                
                
                offsetHistory[lastMagHistoryPos]=bestPos;
                errorHistory[lastMagHistoryPos]=currentErrors[WINDOW_SHIFT];
                probabilityHistory[lastMagHistoryPos]=swingProbability;

                if(bestPos>0 || bestPos<0)
                {
                    // shift phase if we detect phase error
//                    Debug.Log(bestPos+"<"+outPhase);
                    outPhase-=0.5f*dt*swingStep*bestPos;
//                    Debug.Log(">"+outPhase);
                    if(bestPos<5 && bestPos>-5)
                    {
                        // shift output frequency very slightly to get it closer
                        swingStep-=0.005f*bestPos;
                        if(swingStep<3.4f)swingStep=3.4f;
                        if(swingStep>4.5f)swingStep=4.5f;
                    }
//                    Debug.Log(bestPos+":"+minVal+":"+estimatedPeriod+":"+dt+":"+swingStep);
                }
                
                estimatedPeriod=(2.0f*Mathf.PI)/swingStep;
                
                

            }            
        }       

        // calculate angle from combination of two
        // and update scaling of accelerometer values
        // 
        //
        float calculatedMaxAngle=0;
        float calculatedCurAngle=0;
        float outAngle=0;
        // can't calculate angle from accel until we're sure we're swinging
        if(swingProbability>0.2f)
        {
//            float maxG=1f+2f/scale;
//            float maxHeightCalc=((maxG)-1f) *self.swingLength / 2f;
//            calculatedMaxAngle=math.acos(1f-(maxHeightCalc/self.swingLength));
            calculatedMaxAngle=Mathf.Acos(1f-scale)*accelAngleMultiplier;
//            Debug.Log(calculatedMaxAngle+"#"+scale);
            calculatedCurAngle=calculatedMaxAngle *Mathf.Sin(outPhase/2f);
            outAngle=calculatedCurAngle*(180.0f/Mathf.PI);
        }
 /*       if(hasGyro)
        {
            outAngle=gyroAngle;
            if(swingProbability>0.8f)
            {
                // correct phase offset (which half of swing we are on)
                if((outAngle<-0.1f && calculatedCurAngle>0.1f) || (outAngle>0.1f && calculatedCurAngle<-0.1f))
                {
                    outPhase+=Mathf.PI*2f;
                }else
                {
                    // correct angle multiplier
                    float accelAngleMultNow = (outAngle / (calculatedCurAngle*accelAngleMultiplier) );
                    accelAngleMultiplier = 0.99f*accelAngleMultiplier + 0.01f*accelAngleMultNow;
                }
            }
        }else
        {
            outAngle = calculatedCurAngle;
        }*/
        //
        
        return outAngle;
        
    }

}
