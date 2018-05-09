﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SwingboatMovement : MonoBehaviour {
    public Text debugText;

    public float triggerAngle=10f;
    MagicReader reader;
    
    public bool isForward=true;
    
    enum GameState
    {
        LOCKED, // locked - showing message to look forward and tap button to start
        UNLOCKED_READY, // unlocked - ready to swing
        IN_GAME, // in game, swing detected
        FADED // after game, stop detected
    };
    GameState mState=GameState.LOCKED;
    
    public bool simulateSine=false;
    public float simulateSineFrequency=0.5f;
    public float simulateSineAmplitude=30.0f;
    int simulateSineState=0;
    
    float lastForwardSwing=0;
    float lastBackwardSwing=0;
    float gameStartTime;
    
    private SwingBoatEffects sbe;

	void Start () 
    {
        reader=GetComponent<MagicReader>();	
        sbe = GetComponent<SwingBoatEffects>();        
        #if !UNITY_EDITOR
            simulateSine=false;
        #endif
	}
	
    void sendServerMessage(int msg)
    {
        reader.sendSensorMessage(msg);
        if(simulateSine)
        {
            simulateSineState=msg;
        }
    }

    void checkDirection()
    {
        Input.gyro.enabled=true;                
        
        Quaternion rot1=Input.gyro.attitude;
        // standard compass angle is for flat phone, and in VR the phone is upright - swap angles so
        // that it works without gimbal lock or flipping when phone tilts back or forward
        
        Quaternion rot2=new Quaternion(rot1.z,rot1.x,rot1.y,rot1.w);        
        float thisRotation=360f-rot2.eulerAngles.z;
        print("direction:"+thisRotation+":"+reader.getMagDirection());

        float angleDiff=Mathf.Abs(thisRotation-reader.getMagDirection());
        if(angleDiff<90 || angleDiff>270)
        {
            isForward=true;
        }else
        {
            isForward=false;
        }
        
        if(debugText)
        {
            debugText.text="Fwd:"+isForward+","+(int)thisRotation+":"+(int)reader.getMagDirection();
        }

        
    }
    
    // update function - don't add anything in here, add game update stuff in DoGameUpdate
	void Update () 
    {
        float gameTime=0.000000001f*(float)reader.getRemoteTimestamp();
        float angle=reader.getAngle();
        //print(angle+":"+gameTime);
        int serverGameState=reader.getGameState();
        bool resetting=false;
        if(simulateSine)
        {
            serverGameState=simulateSineState;
            angle=simulateSineAmplitude*Mathf.Sin(simulateSineFrequency*Time.time*Mathf.PI*2.0f);
            gameTime=Time.time;
        }
        if(serverGameState==0 || serverGameState==1)
        {
            resetting=true;
        }else
        {
            resetting=false;
        }
        // back button pressed, reset game on sensor phone
        // and pause it
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            sendServerMessage(1);
            print("Back pressed");
        }

		if(angle>triggerAngle)
		{
			lastForwardSwing=Time.time;
			if(mState==GameState.UNLOCKED_READY && lastBackwardSwing-Time.time<3)
			{
                // fwd / backward swings in last 3 seconds, send start message to sensor phone
                // which starts the clock running
                sendServerMessage(2);
			}
		}
		if(angle<-triggerAngle)
		{
			lastBackwardSwing=Time.time;
		}
        // 
        if(Time.time-lastBackwardSwing>6 && Time.time-lastForwardSwing>6 && mState==GameState.IN_GAME)
        {
            // haven't seen a swing for ages - fade out the game
            sendServerMessage(3);
            print("Send fade message");
        }
        
        
        // if we're not swinging yet, then tap recenters and unlocks
        // and works out if we are the forward phone or not
        if(Input.GetButtonDown("Tap"))
        {
            if(mState==GameState.LOCKED || mState==GameState.UNLOCKED_READY)
            {
                print("unlocked");
                mState=GameState.UNLOCKED_READY;
                FadeSphereScript.changePauseColour(new Color(0,1,0));
            }
            UnityEngine.XR.InputTracking.Recenter();
            checkDirection();
            sbe.isFore=isForward;
        }
        // if the swing phone is reset, fade out
        if(resetting)
        {
            if(mState==GameState.IN_GAME || mState==GameState.FADED)
            {
                ResetGame();
            }
        }else
        {
            if(serverGameState==3 && mState!=GameState.FADED)
            {
                FadeGame();
                print("Game faded");
            }
            // the swing phone is showing time - start game if we are unlocked and ready
            if(mState==GameState.UNLOCKED_READY && serverGameState==2)
            {
                StartGame();
                // make sure it doesn't fade straight away
                lastBackwardSwing=Time.time;
                lastForwardSwing=Time.time;
            }
            if(mState==GameState.IN_GAME)
            {
                DoGameUpdate(angle,gameTime);
            }
        }
        
        
	}
    
    void StartGame()
    {
        FadeSphereScript.doFadeIn(5f, Color.black);
        mState=GameState.IN_GAME;
        gameStartTime=Time.time;
        print("Start game");

    }

    void ResetGame()
    {
        FadeSphereScript.doFadeOut(5f, Color.black);
        FadeSphereScript.changePauseColour(new Color(1,1,1));
        mState=GameState.LOCKED;
        print("Resetting");
    }
    
    void FadeGame()
    {
        // this fades the game without resetting - so it is in finished state (until back button is pressed)
        FadeSphereScript.doFadeOut(5f, Color.black);
        FadeSphereScript.changePauseColour(new Color(1,0,0));
        mState=GameState.FADED;
    }
    
    // put game update stuff in here
    void DoGameUpdate(float angle,float gameTime)
    {
        sbe.applyEffects(angle,gameTime);
    }
    
    
}
