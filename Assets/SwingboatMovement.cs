using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingboatMovement : MonoBehaviour {

    public float triggerAngle=10f;
    MagicReader reader;
    
    enum GameState
    {
        LOCKED, // locked - showing message to look forward and tap button to start
        UNLOCKED_READY, // unlocked - ready to swing
        IN_GAME, // in game, swing detected
        FADED // after game, stop detected
    };
    GameState mState=GameState.LOCKED;
    
    
    float lastForwardSwing=0;
    float lastBackwardSwing=0;
    float gameStartTime;

	void Start () 
    {
        reader=GetComponent<MagicReader>();		
	}
	
    // update function - don't add anything in here, add game update stuff in DoGameUpdate
	void Update () 
    {
        float gameTime=0.000000001f*(float)reader.getRemoteTimestamp();
        int serverGameState=reader.getGameState();
        bool resetting=reader.getInReset();
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
            reader.sendSensorMessage(1);
            print("Back pressed");
        }

        float angle=reader.getAngle();
		if(angle>triggerAngle)
		{
			lastForwardSwing=Time.time;
			if(mState==GameState.UNLOCKED_READY && lastBackwardSwing-Time.time<3)
			{
                // fwd / backward swings in last 3 seconds, send start message to sensor phone
                // which starts the clock running
                reader.sendSensorMessage(2);
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
            reader.sendSensorMessage(3);
            print("Send fade message");
        }
        
        
        // if we're not swinging yet, then tap recenters and unlocks
        if(Input.GetButtonDown("Tap"))
        {
            if(mState==GameState.LOCKED || mState==GameState.UNLOCKED_READY)
            {
                print("unlocked");
                mState=GameState.UNLOCKED_READY;
                FadeSphereScript.changePauseColour(new Color(0,1,0));
                UnityEngine.XR.InputTracking.Recenter();
            }
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
            if(serverGameState==3)
            {
                FadeGame();
                print("Game faded");
            }
            // the swing phone is showing time - start game if we are unlocked and ready
            if(mState==GameState.UNLOCKED_READY && serverGameState==2)
            {
                StartGame();
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
        
    }
    
}
