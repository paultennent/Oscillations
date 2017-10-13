#define DEBUG_OUTPUT

// NEEDS to sync up to gyro when we have it
// if we don't have gyro, continue based on accelerometer 
// do scoring based on gyro position if we have one
using UnityEngine;
using System;

public class FastSwingTracker 
{
    int WINDOW_SIZE=0;
    int WINDOW_SHIFT=20;
//    const int WINDOW_SIZE=160;
//    const int WINDOW_SHIFT=20;
    const float INITIAL_SWING_PERIOD=1.8f;
//    const float INITIAL_SWING_PERIOD=1.8f;
    
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
    
    public FastSwingTracker()
    {
        SetWindowSize( 160,20);  
        plls=new FixedPLL[100];
        for(int c=0;c<100;c++)
        {
            plls[c]=new FixedPLL(1.0f+c*0.1f);
        }        
    }
    
    class FixedPLL
    {
        public FixedPLL(float step)
        {
            this.step=step;
            phase=0;
            smoothedError=5f;
        }
        
        public float update(float dt,float accValue,float fwdAccel)
        {
            phase+=dt*step;
            float phasePos=Mathf.Repeat(phase,Mathf.PI*2f);
            float fwdBackPos=Mathf.Repeat(phase,Mathf.PI*4f);
            if(fwdBackPos<Mathf.PI*2f)
            {
                // expecting more forward acceleration than backwards in this bit of the swing
                fwdAccum=0.9f*fwdAccum+0.1f*fwdAccel;
            }else
            {
                backAccum=0.9f*backAccum+0.1f*fwdAccel;
            }         
            float error=accValue-Mathf.Cos(phase);
            float absError=Mathf.Abs(error);
            if(phasePos<Mathf.PI)
            {
                error=-error;
            }
            if(error<-0.1f)
            {
                phase-=dt;                               
            }else if(error>0.1f){
                phase+=dt;
            }
            smoothedError=0.95f*smoothedError+0.05f*absError;
            return smoothedError;
        }
        
        public float getPhase()
        {
            if(fwdAccum>backAccum)
            {
                return 2f*Mathf.PI+(phase-Mathf.PI);
            }else
            {
                return phase-Mathf.PI;
            }
        }
        
        public float getStep()
        {
            return step;
        }
        
        float phase;
        float step;
        float smoothedError;
        
        float fwdAccum=0f;
        float backAccum=0f;
        
        
        
    }
    
    FixedPLL[] plls;
    
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
        
        int gyroCount=0;
        int lastMagHistoryPos=magHistoryPos;
        float dt = magTime-lastTime;
        lastTime=magTime;
        // update the output estimate and phase estimate
        float outVal=Mathf.Sin(outPhase);
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
        if(magHistoryLen>=200)
        {
            // calculate the mean and variance of the history buffer
            float meanMain=0,varianceMain=0;
            GetMeanVariance(magHistory,out meanMain,out varianceMain,null);
            
            // mean and variance of the gyro angle history buffer
            float meanGyro=0,varianceGyro=0;
            gyroCount=GetMeanVariance(gyroHistory,out meanGyro,out varianceGyro,gyroValidHistory);
            
            float gyroMaxAngle=Mathf.Sqrt(varianceGyro)/(0.5f*Mathf.Sqrt(2));
            
            offset=-meanMain;
            scale=Mathf.Sqrt(varianceMain)/(0.5f*Mathf.Sqrt(2));
            
/*            if(hasGyro)
            {
                gyroYOffset=gyroHistory[lastMagHistoryPos]-gyroMaxAngle *Mathf.Sin((outPhase-0.5f*Mathf.PI)/2f);
            }*/

            

            // assuming we have a sensible history, now check whether our sine buffer fits nicely shifted +- WINDOW_SHIFT along
            // cross correlations with a least squared difference operator
            if(scale>0)
            {
                float invScale=1/scale; 
                float magScaled=(mag+offset)*invScale;
                float bestPhase=0;
                float bestError=0;
                float bestStep=0;
                for(int c=0;c<100;c++)
                {
                    float error=plls[c].update(dt,magScaled,zAccel);
                    if(error<bestError || c==0)
                    {
                        bestError=error;
                        bestPhase=plls[c].getPhase();
                        bestStep=plls[c].getStep();
                    }
                }
                while(bestPhase-outPhase>Mathf.PI*4f){
                    bestPhase-=Mathf.PI*4f;
                }
                while (outPhase-bestPhase>Mathf.PI){
                    bestPhase+=Mathf.PI*4f;
                }
                outPhase=bestPhase;
                swingProbability=1.0f-Mathf.Min(bestError/1.0f,1.0f);
                swingProbability*=Mathf.Max(scale/20.0f,1f);
                #if DEBUG_OUTPUT
                
                Debug.Log("Best PLL:"+bestError+"["+bestStep+"]"+outPhase);
                #endif
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
/*            if(negativeZ>0.1 && positiveZ<-.1 && swingProbability>0.2f)
            {
                #if DEBUG_LOG
                Debug.Log("Switch phase accelerometer!!!!!");
                #endif
                outPhase+=Mathf.PI*2f;
            }*/
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
			//dbgTxt = "";
            //dbgTxt=String.Format("{0,5:F2} {1,3:F2} {2,3:F2}",calculatedMaxAngle*(180.0f/Mathf.PI),calculatedCurAngle,swingStep);
//            dbgTxt=String.Format("{0,5:F2} {1,3:F2} {2,3:F2}",calculatedMaxAngle*(180.0f/Mathf.PI),calculatedCurAngle,swingStep);
            //Debug.Log(dbgTxt);

            
            
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
                #if DEBUG_OUTPUT
                Debug.Log("Using calculated:"+outAngle);
                #endif
            }
            else
            {
                #if DEBUG_OUTPUT
//                Debug.Log("Using Last angle:"+lastAngle+":"+calculatedCurAngle);
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
