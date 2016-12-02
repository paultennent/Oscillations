// NEEDS to sync up to gyro when we have it
// if we don't have gyro, continue based on accelerometer 
// do scoring based on gyro position if we have one
using UnityEngine;
using System;

public class SwingTracker 
{
    int WINDOW_SIZE=0;
    int WINDOW_SHIFT=20;
//    const int WINDOW_SIZE=160;
//    const int WINDOW_SHIFT=20;
//    const float INITIAL_SWING_PERIOD=1.6f;
    const float INITIAL_SWING_PERIOD=1.8f;
    
    public string dbgTxt="";
    
    float outPhase=0.5f;
    float lastTime=0;
    float swingStep=(2.0f*Mathf.PI) / INITIAL_SWING_PERIOD;
    float scale=0;
    float offset=0;

    // buffers for storing history to compare against
    float []gyroHistory;
    bool []gyroValidHistory;
    float []zHistory;
    float []magHistory;
    float []timeHistory;
    float []sinWindow;
    float []currentErrors;
    float []offsetHistory;
    float []errorHistory;
    float []probabilityHistory;
    float []angleHistory;
    int magHistoryPos=0;
    int magHistoryLen=0;

    // things used in acceleration calculation
    float accelAngleMultiplier=1.0f;
    public float estimatedPeriod=INITIAL_SWING_PERIOD;
    public float swingProbability=0f;    
    float moveErrorAccumulator=20;
    int logCount=0;
    int measuringWindowCountLeft=50;// we measure sensor rate before setting window size
    float windowStartTime=0;
    float calculatedMaxAngle=0;
    
    public void SetWindowSize(int windowSize,int windowShift)
    {   
        WINDOW_SIZE=windowSize;
        WINDOW_SHIFT=windowShift;
        gyroHistory=new float[WINDOW_SIZE];
        gyroValidHistory=new bool[WINDOW_SIZE];
        zHistory=new float[WINDOW_SIZE];
        angleHistory=new float[WINDOW_SIZE];
        magHistory=new float[WINDOW_SIZE];
        timeHistory=new float[WINDOW_SIZE];
        sinWindow=new float[WINDOW_SIZE+2*WINDOW_SHIFT+1];
        currentErrors=new float[WINDOW_SHIFT*2+1];
        offsetHistory=new float[WINDOW_SIZE];
        errorHistory=new float[WINDOW_SIZE];
        probabilityHistory=new float[WINDOW_SIZE];
        magHistoryPos=0;
        magHistoryLen=0;
    }
    
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
        if(WINDOW_SIZE==0)
        {
            return null;
        }
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
            case 5:
                return DeLoop(zHistory);
            case 6:
                return DeLoop(angleHistory);
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
            case 5:
                retVal=new float[2];
                retVal[0]=-1;
                retVal[1]=1;
                return retVal;
            default:
                return null;
        }
    }

//    AndroidJavaObject tg=new AndroidJavaObject("android.media.ToneGenerator",5,0x64);
//        ToneGenerator tg = new ToneGenerator(AudioManager.STREAM_NOTIFICATION, ToneGenerator.MAX_VOLUME );
//            tg.Call<bool>("startTone",41);

    
    public void GetMeanVariance(float[]buffer,out float  meanSum, out float  varianceSum)
    {
        meanSum=0;
        for(int c=0;c<buffer.Length;c++)
        {
            meanSum+=buffer[c];
        }
        meanSum/=buffer.Length;
        varianceSum=0;
        for(int c=0;c<buffer.Length;c++)
        {
            varianceSum+=(buffer[c]-meanSum)*(buffer[c]-meanSum);
        }
        varianceSum/=buffer.Length;
    }
    
   
    public float OnAccelerometerMagnitude(float mag,float magTime,bool hasGyro,float gyroAngle,float zAccel)
    {
        if(measuringWindowCountLeft==-1)
        {
            measuringWindowCountLeft=50;
            windowStartTime=magTime;
            return 0;
        }else if(measuringWindowCountLeft>0)
        {
            measuringWindowCountLeft--;
            if(measuringWindowCountLeft==0)
            {
                float timeFor50=(magTime-windowStartTime);
                int countsFor3Seconds=(int)((50f * 3f)/timeFor50);
                SetWindowSize( 160,20);  
//                SetWindowSize( countsFor3Seconds,20);  
//                SetWindowSize( countsFor3Seconds,countsFor3Seconds/15);  
            }
            return 0;
        }
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
            float meanMain=0,varianceMain=0;
            GetMeanVariance(magHistory,out meanMain,out varianceMain);
            
            offset=-meanMain;
            scale=Mathf.Sqrt(varianceMain)/(0.5f*Mathf.Sqrt(2));

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

                
                float TIME_CONSTANT=0.323333333f;
                float moveErrorCoefficient=dt/(TIME_CONSTANT+dt); 
                moveErrorAccumulator=Mathf.Abs(bestPos)*moveErrorCoefficient+moveErrorAccumulator*(1f-moveErrorCoefficient);
                float win1ms=((float)(WINDOW_SHIFT))/20f;
                float probFromMoveErrors=Mathf.Max(0,(win1ms-moveErrorAccumulator)/win1ms);
                float probFromBestError=Mathf.Min(Mathf.Max(1-(currentErrors[WINDOW_SHIFT]-.2f),0),1);
                float probFromRangeDifference=Mathf.Min(1,Mathf.Max(0,currentErrors[0]/currentErrors[bestPos+WINDOW_SHIFT]-1.5f));
                swingProbability=probFromBestError*probFromMoveErrors*probFromRangeDifference;

                logCount++;
                if((logCount&63)==0)
                {
                    Debug.Log(probFromMoveErrors+":"+probFromBestError+":"+probFromRangeDifference+":"+bestPos+":"+WINDOW_SHIFT);
                }
//                dbgTxt=String.Format("{0,5:F2} {1,3:F2} {2,3:F2} {3,5:F2} {4,5:F2} {5,5:F2}",probFromMoveErrors,probFromBestError,probFromRangeDifference,bestPos,moveErrorAccumulator,scale);
                                
                offsetHistory[lastMagHistoryPos]=bestPos;
                errorHistory[lastMagHistoryPos]=currentErrors[WINDOW_SHIFT];
                probabilityHistory[lastMagHistoryPos]=swingProbability;

                
                if(bestPos>0 || bestPos<0)
                {
                    // shift phase if we detect phase error
//                    Debug.Log(bestPos+"<"+outPhase);
                    outPhase-=0.5f*dt*swingStep*bestPos;
//                    Debug.Log(">"+outPhase);
                    if(bestPos<(WINDOW_SHIFT/4) && bestPos>(-WINDOW_SHIFT/4))
                    {
                        // shift output frequency very slightly to get it closer
                        swingStep-=0.005f*bestPos;
                        if(swingStep<3.0f)swingStep=3.0f;
                        if(swingStep>6.2f)swingStep=6.2f;
                    }
//                    Debug.Log(bestPos+":"+minVal+":"+estimatedPeriod+":"+dt+":"+swingStep);
                }
                
                estimatedPeriod=(2.0f*Mathf.PI)/swingStep;                
                

            }            
        }       

      // the main accel magnitude cycles twice per swing phase (once in each direction)
        // need to work out which cycle we are on
        // detect whole cycle error (i.e. we are swinging one way when we think we are swinging the other)

        float comparePhase=outPhase;
        int comparePos=magHistoryPos;
        float positiveZ=0;
        float negativeZ=0;
        for(int c=0;c<WINDOW_SIZE;c++)
        {
            comparePos--;
            if(comparePos<0)
            {
                comparePos=WINDOW_SIZE-1;
            }
            
            if(Mathf.Sin((comparePhase-0.5f*Mathf.PI)*0.5f)>0)
            {
                positiveZ+=zHistory[comparePos];
            }else
            {
                negativeZ+=zHistory[comparePos];
            }
            comparePhase-=dt*swingStep;
        }
        if(negativeZ>0.1 && positiveZ<-.1 && swingProbability>0.2f)
        {
//            outPhase+=Mathf.PI*2f;
        }
        
        // calculate angle from accelerometer data

        float thisMaxAngle=0;
        float calculatedCurAngle=0;
        float outAngle=0;
        // can't calculate angle from accel until we're sure we're swinging, otherwise just drop down to zero
        if(swingProbability>0.2f)
        {
            thisMaxAngle=Mathf.Acos(1f-scale)*accelAngleMultiplier;
        }
        const float MAX_ANGLE_TIME_CONSTANT=1f;
//        const float MAX_ANGLE_TIME_CONSTANT=0.5f;
        float maxAngleCoefficient=dt/(MAX_ANGLE_TIME_CONSTANT+dt); 
        calculatedMaxAngle=thisMaxAngle*maxAngleCoefficient + calculatedMaxAngle*(1f-maxAngleCoefficient);
        calculatedCurAngle=calculatedMaxAngle *Mathf.Sin((outPhase-0.5f*Mathf.PI)/2f);
        calculatedCurAngle*=(180.0f/Mathf.PI);
        outAngle=calculatedCurAngle;
        dbgTxt=String.Format("{0,5:F2} {1,3:F2} {2,3:F2}",calculatedMaxAngle*(180.0f/Mathf.PI),calculatedCurAngle,swingStep);

  /*      if(hasGyro)
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
        angleHistory[lastMagHistoryPos]=calculatedMaxAngle;
        //
        
        return outAngle;
        
    }

}
