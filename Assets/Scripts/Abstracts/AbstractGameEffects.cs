using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbstractGameEffects : MonoBehaviour {

	protected AbstractDataReader swingData;
	protected SessionManager sessionManager;
	protected SwingBase swingBase;
	protected Transform swingPivot;
	protected Transform swingSeat;
	protected Transform viewPoint;
    protected AndroidFlasher statusFlasher;


	protected float swingAngle;
	protected float sessionTime;
	protected bool inSession;
    protected float lastAngle;

	public float sessionLength = 60f;
	public float climaxTime = 30.0f;
	public bool dontcycle = false;
	public float climaxRatio = 0f;
    public float pauseEndTime= 10.0f;
	protected float offsetTime;
    
    protected float swingPhase=0;
    protected float swingAmplitude=0;
    protected float swingAngVel=0;
    protected int swingQuadrant=0;

	public int maxSessions = 1;

	protected float highAngle;
	protected float lowAngle;

	protected bool suppressEffects;

    public float pauseStartTime = 0f;
    

	//public bool faded;
	//public bool fading = false;
	//public Canvas killer;


	// Use this for initialization
	public void Start () {
		swingBase = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SwingBase> ();
		swingData = GameObject.FindGameObjectWithTag ("Controller").GetComponent<AbstractDataReader> ();
		sessionManager = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SessionManager> ();
		swingSeat = GameObject.FindGameObjectWithTag ("Swing").transform;
		swingPivot = GameObject.FindGameObjectWithTag ("SwingPivot").transform;
		viewPoint = GameObject.FindGameObjectWithTag ("ViewPoint").transform;
        statusFlasher=  GameObject.FindGameObjectWithTag ("Controller").GetComponent<AndroidFlasher>();
	}
	
	// Update is called once per frame
	public void Update () {

        if (Time.time < pauseStartTime)
        {
            return;
        }

        swingAngle = swingBase.getSwingAngle ();
		sessionTime = sessionManager.getSessionTime ();
		inSession = sessionManager.isInSession ();

		if (suppressEffects && !inSession) {
			suppressEffects = false;
		}
		//if(faded & !fading & !inSession){
		//	faded = false;
		//	killer.enabled = false;
		//}
        float timeLeftInGame=0;
        
		if (inSession) {
            float totalGameTime=climaxTime*2.0f+pauseEndTime;
			int gameNumber = (int)(sessionTime / totalGameTime);

			if (gameNumber > maxSessions && maxSessions > 0) {
				suppressEffects = true;
                timeLeftInGame=0;
			}else
            {
                offsetTime = sessionTime - (gameNumber * totalGameTime);
                timeLeftInGame=totalGameTime - offsetTime -pauseEndTime;
                if(timeLeftInGame<0)
                {
                    timeLeftInGame=0;
                }
                if(offsetTime>climaxTime)                
                {
                    if(offsetTime<climaxTime*2.0f)
                    {
                        // going back down
                        offsetTime=climaxTime*2.0f-offsetTime;
                    }else
                    {
                        // pause at end
                        offsetTime=0;
                    }
                }

                if (!dontcycle) {
                    climaxRatio = offsetTime / climaxTime;
                } else {
                    if (sessionTime <= climaxTime) {
                        climaxRatio = offsetTime / climaxTime;
                    } else {
                        climaxRatio = 1f;
                    }
                }
            }
								
		} else {
			if (suppressEffects) {
				suppressEffects = false;
			}
		}

		if (inSession && suppressEffects) {
			inSession = false;
			//if(!faded){
			//	StartCoroutine(fader());
			//}
		}

        if (swingAngle > highAngle) {
            highAngle = swingAngle;
        }
        if (swingAngle < lowAngle) {
            lowAngle = swingAngle;
        }
        if (swingAngle < 0) {
            highAngle = 0;
        }
        if (swingAngle > 0) {
            lowAngle = 0;
        }
        // calculate position of swing in full cycle
        // NOTE: right now there is no smoothing of anything, it relies on input data being quite smooth?
        // Note2: could just get quadrant from accelerometer processing
//    protected float swingPhase;
//    protected float swingAmplitude;
//    protected int swingQuadrant
        // what quadrant of the swing are we in:
        // phase = 0: 0 -> +1, 1: +1 -> 0, 2: 0 -> -1, 3: -1 -> 0
        switch(swingQuadrant)
        {
            case 0:
                if(swingAmplitude==0)
                {
                    swingPhase=0.5f;
                }else
                {
                    swingPhase=Mathf.Max(0,Mathf.Min(swingAngle/swingAmplitude,1));
                }
                if(swingAngle<highAngle-0.1f)
                {
                    swingQuadrant=1;
                    swingAmplitude=highAngle;
                }
                break;
            case 1:
                if(swingAmplitude==0)
                {
                    swingPhase=1.5f;
                }else
                {
                    swingPhase=2-Mathf.Max(0,Mathf.Min(swingAngle/swingAmplitude,1));
                }
                if(swingAngle<0f)
                {
                    swingQuadrant=2;
                }
                break;
            case 2:
                if(swingAmplitude==0)
                {
                    swingPhase=2.5f;
                }else
                {
                    swingPhase=2+Mathf.Max(0,Mathf.Min(-swingAngle/swingAmplitude,1));
                }
                if(swingAngle>lowAngle+0.1f)
                {
                    swingQuadrant=3;
                    swingAmplitude=-lowAngle;
                }
                break;
            case 3:
                if(swingAmplitude==0)
                {
                    swingPhase=3.5f;
                }else
                {
                    swingPhase=4-Mathf.Max(0,Mathf.Min(-swingAngle/swingAmplitude,1));
                }
                if(swingAngle>0f)
                {
                    swingQuadrant=0;
                }                
                break;
        }
        swingAngVel=swingAngle-lastAngle;
        //print("phase:"+swingPhase+",quadrant:"+swingQuadrant+",angVel:"+swingAngVel);
        
        // set status flashlight
        //switch(swingData.getConnectionState())
        //{
        //    case AbstractDataReader.CONNECTION_PARTIAL:
        //        statusFlasher.setFlashPattern(statusFlasher.FLASH_DISCONNECT_ONE);
        //        break;
        //    case AbstractDataReader.CONNECTION_NONE:
        //        statusFlasher.setFlashPattern(statusFlasher.FLASH_DISCONNECT_ALL);            
        //        break;
        //    default:
        //        // connected ok
        //        {
        //            print("connected");
        //            if(inSession)
        //            {
        //                if(timeLeftInGame<=0)
        //                {
        //                    statusFlasher.setFlashPattern(statusFlasher.FLASH_FINISHED);
        //                }else if(timeLeftInGame<10)
        //                {
        //                    statusFlasher.setFlashPattern(statusFlasher.FLASH_FINISHING);
        //                }else
        //                {
        //                    statusFlasher.setFlashPattern(statusFlasher.FLASH_RUNNING);
        //                }
        //            }else
        //            {
        //                statusFlasher.setFlashPattern(null);
        //            }                        
        //        }
        //        break;
        //}

	}

//	private IEnumerator fader(){
//		fading = true;
//		yield return new WaitForSeconds(2f);
//		killer.enabled = true;
//		fading = false;
//		faded = true;
//	}


}
