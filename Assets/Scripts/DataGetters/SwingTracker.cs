//#define DEBUG_OUTPUT

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
    float lastAngle=0;

    // buffers for storing history to compare against
    float []gyroHistory;
    bool []gyroValidHistory;
    float []zHistory;
    float []magHistory;
    float []timeHistory;
    float []sinWindow;
    float []gyroSinWindow;
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
    
    float gyroYOffset=0f;
    
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
        gyroSinWindow=new float[WINDOW_SIZE+2*WINDOW_SHIFT+1];
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
        float[]retVal;
        switch(count)
        {
            case 0:
                retVal=new float[WINDOW_SIZE];
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
                return DeLoop(gyroHistory);
            case 4:
                return DeLoop(probabilityHistory);
            case 5:
                retVal=new float[WINDOW_SIZE];
                for(int c=0;c<WINDOW_SIZE;c++)
                {
                    retVal[c]=gyroSinWindow[c+WINDOW_SHIFT];
                }
                return retVal;            
//                return DeLoop(zHistory);
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
                retVal[0]=-Mathf.PI;
                retVal[1]=Mathf.PI;
                return retVal;
            case 4:
                retVal=new float[2];
                retVal[0]=0;
                retVal[1]=1;
                return retVal;
            case 5:
                retVal=new float[2];
                retVal[0]=-Mathf.PI;
                retVal[1]=Mathf.PI;
                return retVal;
            case 6:
                retVal=new float[2];
                retVal[0]=-Mathf.PI;
                retVal[1]=Mathf.PI;
                return retVal;
            default:
                return null;
        }
    }

//    AndroidJavaObject tg=new AndroidJavaObject("android.media.ToneGenerator",5,0x64);
//        ToneGenerator tg = new ToneGenerator(AudioManager.STREAM_NOTIFICATION, ToneGenerator.MAX_VOLUME );
//            tg.Call<bool>("startTone",41);

    // if we want more efficiency, we could use online estimates of mean/variance that just require removing the old value and putting in the new value
    // once per data point. This is probably fine though.
    public int GetMeanVariance(float[]buffer,out float  meanSum, out float  varianceSum,bool []validBuffer)
    {
        meanSum=0;
        float validCount=0;
        for(int c=0;c<buffer.Length;c++)
        {
            if(validBuffer==null || validBuffer[c]==true)
            {
                meanSum+=buffer[c];
                validCount+=1;
            }
        }
        if(validCount==0)
        {
            meanSum=0;
            varianceSum=0;
            return 0;
        }
        meanSum/=validCount;
        varianceSum=0;
        for(int c=0;c<buffer.Length;c++)
        {
            if(validBuffer==null || validBuffer[c]==true)
            {
                varianceSum+=(buffer[c]-meanSum)*(buffer[c]-meanSum);
            }
        }
        varianceSum/=validCount;
        return (int)validCount;
    }
    
   
    public float OnAccelerometerMagnitude(float mag,float magTime,bool hasGyro,float gyroAngle,float zAccel)
    {
//        Debug.Log(magTime);
//        if(magTime>35)hasGyro=false;
        float outAngle=lastAngle;
        if(hasGyro)outAngle=gyroAngle;
        
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
        int gyroCount=0;
        int lastMagHistoryPos=magHistoryPos;
        float dt = magTime-lastTime;
        lastTime=magTime;
        // update the output estimate and phase estimate
        float outVal=Mathf.Sin(outPhase);
        outPhase+=dt*swingStep;
        // update the history buffer
        zHistory[magHistoryPos]=zAccel;
        if(hasGyro)
        {
            gyroHistory[magHistoryPos]=gyroAngle*(Mathf.PI/180f);
            gyroValidHistory[magHistoryPos]=hasGyro;
        }else
        {
            gyroHistory[magHistoryPos]=0;
//            gyroHistory[magHistoryPos]=gyroAngle*(Mathf.PI/180f);;
            gyroValidHistory[magHistoryPos]=false;
        }
        magHistory[magHistoryPos]=mag; 
        timeHistory[magHistoryPos]=magTime;
        magHistoryPos++;
        if(magHistoryPos>=magHistory.Length)magHistoryPos=0;
        magHistoryLen+=1;
        // wait till we have full history before doing anything - i.e. nothing will work before 4 seconds
        if(magHistoryLen>=sinWindow.Length)
        {
            // how this algorithm works for pure accelerometer:
            // 
            // 1) based on current frequency+phase estimate, calculate sine wave for length of accelerometer magnitude history buffer
            // 2) align vertically&scale by using mean/variance, then score it against the observed accel data for +-20 samples offset
            // 3) shift phase towards best offset
            // 4) shift frequency very slightly also
            // 5) Estimate swinging probability by error, low pass filtered amount we have had to shift recently, and difference multiplier between 0 offset and -20 offset 
            //   (because for a good match, we'd expect there to be a much better match at 0 then -20, whereas for a poor match it is likely
            //   to be roughly uniform.
            // 6) estimate maximum angle by taking maximum acceleration using kinetic energy = potential energy formula.
            //     simplifies out to the line down there: thisMaxAngle=Mathf.Acos(1f-scale)*accelAngleMultiplier;
            // 7) estimate current angle by taking 0.5*phase and applying sine wave to it (max acceleration happens twice per complete swing, hence phase*0.5)
            //
            // With gyro data there's an extra step:
            // 
            // 2g where we have gyro data, generate sine wave using angle calculatation (based on last max angle), plus maybe align vertically & scale using mean/variance, 
            // then score against observed gyro angle data
            // 6g where we have a decent amount of gyro data use that to estimate maximum angle
            // 7g if swing probability is high, use phase output (because it will be lovely and smooth)
            //    otherwise use last gyro data point
              
            
            // calculate the mean and variance of the history buffer
            float meanMain=0,varianceMain=0;
            GetMeanVariance(magHistory,out meanMain,out varianceMain,null);
            
            // mean and variance of the gyro angle history buffer
            float meanGyro=0,varianceGyro=0;
            gyroCount=GetMeanVariance(gyroHistory,out meanGyro,out varianceGyro,gyroValidHistory);
            
            float gyroMaxAngle=Mathf.Sqrt(varianceGyro)/(0.5f*Mathf.Sqrt(2));
            //Debug.Log(gyroMaxAngle);
            
            offset=-meanMain;
            scale=Mathf.Sqrt(varianceMain)/(0.5f*Mathf.Sqrt(2));
/*            if(hasGyro)
            {
                gyroYOffset=gyroHistory[lastMagHistoryPos]-gyroMaxAngle *Mathf.Sin((outPhase-0.5f*Mathf.PI)/2f);
            }*/

            // create a comparison buffer sine wave 
            // starts from WINDOW_SHIFT frames ahead
            float phaseSin=outPhase+dt*swingStep*(float)WINDOW_SHIFT;
            for(int c=0;c<sinWindow.Length;c++)
            {
                sinWindow[c]=Mathf.Sin(phaseSin);
                gyroSinWindow[c]=gyroYOffset+gyroMaxAngle *Mathf.Sin((phaseSin-0.5f*Mathf.PI)/2f);
                phaseSin-=dt*swingStep;
            }
            

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
                    float weighting=0;//WINDOW_SIZE;
                    float divisor=0;
                    float totalError=0;
                    for(int d=0;d<WINDOW_SIZE;d++)
                    {
                        weighting+=1f;
                        divisor+=weighting;
                        if(gyroValidHistory[magPos])
                        {
                            // this should be already scaled
                            float pointError=gyroHistory[magPos] - gyroSinWindow[sinPos];
                            totalError+=weighting*pointError*pointError;
                        }else
                        {
                            float pointError=(magHistory[magPos]+offset)*invScale - sinWindow[sinPos];
                            totalError+=weighting*pointError*pointError;
                        }

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
                if(currentErrors[bestPos+WINDOW_SHIFT]==0)
                {
                    probFromRangeDifference=1;
                }
                swingProbability=probFromBestError*probFromMoveErrors*probFromRangeDifference;

                logCount++;
                if((logCount&63)==0)
                {
                    #if DEBUG_OUTPUT
                    Debug.Log(probFromMoveErrors+":"+probFromBestError+":"+probFromRangeDifference+":"+bestPos+":"+gyroCount+";"+gyroMaxAngle);
                    #endif
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
                
                if(Mathf.Sin((comparePhase-0.5f*Mathf.PI)*0.5f)<0)
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
                #if DEBUG_LOG
                Debug.Log("Switch phase accelerometer!!!!!");
                #endif
                outPhase+=Mathf.PI*2f;
            }
            if(gyroCount>WINDOW_SIZE/8)
            {
                float phaseError=0;
                int magPos=magHistoryPos;// position in history buffer - from earliest point, goes forward (in time and value)
                int sinPos=WINDOW_SIZE+WINDOW_SHIFT;// position in sin buffer (from end, goes backwards in value / forwards in time)
                for(int c=0;c<WINDOW_SIZE;c++)
                {
                    if(gyroValidHistory[magPos])
                    {
                        phaseError+=gyroHistory[magPos]*gyroSinWindow[sinPos];
                    }
                    sinPos-=1;
                    magPos+=1;
                    if(magPos>=magHistory.Length)magPos=0;
                }
                if(phaseError<0)
                {                    
                #if DEBUG_LOG
                    Debug.Log("Switch phase gyro!!!!!");
                #endif
                    outPhase+=Mathf.PI*2f;
                    
                }
            }
            
            // calculate angle from accelerometer data

            float thisMaxAngle=0;
            float calculatedCurAngle=0;
            // can't calculate angle from accel until we're sure we're swinging, otherwise just drop down to zero
            if(swingProbability>0.2f)
            {
                if(gyroCount>WINDOW_SIZE/4)
                {
                    thisMaxAngle=gyroMaxAngle;
                    if(swingProbability<0.5f)calculatedMaxAngle=thisMaxAngle;
                }else
                {
                    thisMaxAngle=Mathf.Acos(1f-scale)*accelAngleMultiplier;
                }
            }
    //        const float MAX_ANGLE_TIME_CONSTANT=1f;
            const float MAX_ANGLE_TIME_CONSTANT=0.5f;
            float maxAngleCoefficient=dt/(MAX_ANGLE_TIME_CONSTANT+dt); 
            calculatedMaxAngle=thisMaxAngle*maxAngleCoefficient + calculatedMaxAngle*(1f-maxAngleCoefficient);
            calculatedCurAngle=calculatedMaxAngle *Mathf.Sin((outPhase-0.5f*Mathf.PI)/2f);
            calculatedCurAngle*=(180.0f/Mathf.PI);
            dbgTxt=String.Format("{0,5:F2} {1,3:F2} {2,3:F2}",calculatedMaxAngle*(180.0f/Mathf.PI),calculatedCurAngle,swingStep);
//            dbgTxt=String.Format("{0,5:F2} {1,3:F2} {2,3:F2}",calculatedMaxAngle*(180.0f/Mathf.PI),calculatedCurAngle,swingStep);

            
            
            // if we have had gyro in the last 5 frames and it is way off the current estimate, then keep it, 
            // assume it means the person is stopping or something
            bool isFarOut=false;
            int gyroPos=magHistoryPos;
            for(int c=0;c<5;c++)
            {
                gyroPos--;
                if(gyroPos<0)
                {
                    gyroPos+=WINDOW_SIZE;
                }            
                if(gyroValidHistory[gyroPos])
                {
                    float diff=(gyroHistory[gyroPos] - angleHistory[gyroPos]);
                    if(Mathf.Abs(diff)>0.0175f)
//                    if(Mathf.Abs(diff)>0.175f)
                    {
    //                    isFarOut=true;
                    }
                }
            }
            // if we have gyro and aren't swinging, use raw reading
            // rather than output a smooth sine wave
            if(hasGyro && (swingProbability<0.5f || isFarOut) )
            {
                outAngle=gyroAngle;
                #if DEBUG_LOG
                Debug.Log("using gyro:"+gyroAngle);
                #endif
            }else if(swingProbability>0.2f && !isFarOut)
            {
                if(hasGyro)
                {
                    float mixVal=2f*swingProbability - 1f;
                    outAngle=calculatedCurAngle*(mixVal) + gyroAngle * (1-mixVal);
                }else
                {
                    outAngle=calculatedCurAngle;
                }
                #if DEBUG_LOG
                Debug.Log("Using calculated:"+outAngle);
                #endif
            }
            else
            {
                #if DEBUG_LOG
                Debug.Log("Using Last angle:"+lastAngle);
                #endif
                if(gyroCount>WINDOW_SIZE/4)
                {
                    outAngle=lastAngle;
                }else
                {
                    outAngle=calculatedCurAngle;
                }
            }
        }

        angleHistory[lastMagHistoryPos]=outAngle*(Mathf.PI / 180.0f);
        lastAngle=outAngle;
        return outAngle;
        
    }

}
