using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbstractGameEffects : MonoBehaviour {
	static AbstractGameEffects s_Singleton;
	public static AbstractGameEffects GetSingleton()
	{
		return s_Singleton;
	}
    
    PhaseEstimator phaseEstimator=new PhaseEstimator();

	protected AbstractDataReader swingData;
	protected SessionManager sessionManager;
	protected SwingBase swingBase;
	protected Transform swingPivot;
	protected Transform swingSeat;
	protected Transform viewPoint;
    protected AndroidCameraHandler statusFlasher;

    [System.NonSerialized ,HideInInspector]
	public float swingAngle;
    [System.NonSerialized ,HideInInspector]
	public float sessionTime;
    [System.NonSerialized ,HideInInspector]
	public bool inSession;
    [System.NonSerialized ,HideInInspector]
    public float lastAngle;

    
    public float debugTimeOffset=0f;
	public float sessionLength = 60f;
	public float climaxTime = 30.0f;
	public bool dontcycle = false;
	public float climaxRatio = 0f;
    public float pauseEndTime= 10.0f;
    [System.NonSerialized ,HideInInspector]
	public float offsetTime;
    public bool countUp=true;
    
    protected int swingCycles=0;
    protected float swingPhase=0;
    protected float swingAmplitude=0;
    protected float swingAngVel=0;
    protected int swingQuadrant=0;
    protected float swingCycleTime=2f;

    protected float[] lastCycleTimes={-1,-1,-1,-1};
    protected bool swingCycleTimeFound=false;
	public int maxSessions = 1;

	protected float highAngle;
	protected float lowAngle;

	protected bool suppressEffects;

    public float pauseStartTime = 0f;
    
    public bool usePLLPhaseEstimation=false;

	public bool disableStatusFlasher = true;

	public Text debugText;

    //stuff from equations script
    float gravity = 9.8f;

    float swingPeriod;
    float halfSwingPeriod;
    float max_sensor_G_reading_in_z_axis_as_crosses_mid_point; //don't have this as it isn't passed

    //predfined - these need correct measurements
    float swing_seat_radius = 1.5f;
    float phone_sensor_radius = 1.55f;
    float centre_of_gravity_radius = 1.25f; //depends on max_sensor_G_reading_in_z_axis_as_crosses_mid_point which we don't have - using approx for now
    float rider_eye_radius = 1f;

    //brendan's shopping list
    public Dictionary<string, float> shopping;
    //used to Calculate deltas
    float last_swing_angle_at_time_t = 0;
    float last_rider_CofG_x_coordinate_at_time_t = 0;
    float last_rider_CofG_z_coordinate_at_time_t = 0;
    float last_rider_eye_x_coordinate_at_time_t = 0;
    float last_rider_eye_z_coordinate_at_time_t = 0;


    public float getSessionTime(){
		return sessionTime;
	}


	// Use this for initialization
	public void Start () {
		s_Singleton = this;
        debugTimeOffset=0f;
		swingBase = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SwingBase> ();
		swingData = GameObject.FindGameObjectWithTag ("Controller").GetComponent<AbstractDataReader> ();
		sessionManager = GameObject.FindGameObjectWithTag ("Controller").GetComponent<SessionManager> ();
		swingSeat = GameObject.FindGameObjectWithTag ("Swing").transform;
		swingPivot = GameObject.FindGameObjectWithTag ("SwingPivot").transform;
		viewPoint = GameObject.FindGameObjectWithTag ("ViewPoint").transform;

        shopping = new Dictionary<string, float>();

        shopping["swing_angle_at_time_t"] = 0;
        shopping["delta_swing_angle_at_time_t"] = 0;
        shopping["swing_angular_velocity_at_time_t"] = 0;
        shopping["rider_CofG_x_coordinate_at_time_t"] = 0;
        shopping["delta_rider_CofG_x_coordinate_at_time_t"] = 0;
        shopping["rider_CofG_z_coordinate_at_time_t"] = 0;
        shopping["delta_rider_CofG_z_coordinate_at_time_t"] = 0;
        shopping["rider_eye_x_coordinate_at_time_t"] = 0;
        shopping["delta_rider_eye_x_coordinate_at_time_t"] = 0;
        shopping["rider_eye_z_coordinate_at_time_t"] = 0;
        shopping["delta_rider_eye_z_coordinate_at_time_t"] = 0;
        shopping["rider_CofG_velocity_in_x"] = 0;
        shopping["rider_CofG_velocity_in_z"] = 0;
        shopping["rider_eye_velocity_in_x"] = 0;
        shopping["rider_eye_velocity_in_z"] = 0;
        shopping["body_linear_speed"] = 0;
        shopping["eye_linear_speed"] = 0;
        shopping["centripetal_Force_per_unit_mass"] = 0;
        shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] = 0;
        shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"] = 0;
        shopping["resultant_force_per_unit_mass_at_CofG"] = 0;
        shopping["angle_as_offset_from_perpendicular_to_CofG_direction"] = 0;
        shopping["Centripetal_G_Force"] = 0;
        shopping["Gravity_G_Force_at_CofG_tangential_to_direction"] = 0;
        shopping["Gravity_G_Force_at_CofG_perpendicular_to_direction"] = 0;
        shopping["resultant_G_Force_at_CofG"] = 0;
    }
	
	// Update is called once per frame
	public void Update () {
		
        if (statusFlasher==null)
        {
            statusFlasher=  AndroidCameraHandler.GetInstance();
        }

        if (Time.time < pauseStartTime)
        {
            return;
        }
        swingAngle = swingBase.getSwingAngle ();
		sessionTime = sessionManager.getSessionTime ()+debugTimeOffset;
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

			if (gameNumber >= maxSessions && maxSessions > 0) {
				suppressEffects = true;
                timeLeftInGame=0;
			}else
            {
                countUp=true;
                offsetTime = sessionTime - (gameNumber * totalGameTime);
                timeLeftInGame=totalGameTime - offsetTime -pauseEndTime;
                if(timeLeftInGame<0)
                {
                    timeLeftInGame=0;
                }
                if(offsetTime>climaxTime)                
                {
                    countUp=false;
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
        
        if(usePLLPhaseEstimation)
        {
            phaseEstimator.onAngle(swingAngle);
            phaseEstimator.getSwingPhaseAndQuadrant(out swingPhase,out swingQuadrant,out swingAmplitude, out swingCycleTime, out swingCycles);
        }else
        {
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
                    if(swingAngle<highAngle-10f)
                    {
                        swingQuadrant=1;
                        swingAmplitude=highAngle;
                        updateCycleTimes(1);
                        swingCycles+=1;
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
                        updateCycleTimes(2);
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
                    if(swingAngle>lowAngle+10f)
                    {
                        swingQuadrant=3;
                        swingAmplitude=-lowAngle;
                        updateCycleTimes(3);
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
                        updateCycleTimes(0);
                    }
                    break;
            }
            //print("["+swingQuadrant+"] angle:"+swingAngle+"\t"+swingAmplitude);
            swingPhase = MapASin (swingPhase);

        }



        if(Time.deltaTime>0)
        {
            swingAngVel=(swingAngle-lastAngle)/Time.deltaTime;
        }else
        {
            swingAngVel=0f;
        }
        lastAngle=swingAngle;
        //print("phase:"+swingPhase+",quadrant:"+swingQuadrant+",angVel:"+swingAngVel);
        
        // set status flashlight
		if (!disableStatusFlasher) {
			if (statusFlasher) {
				switch (swingData.getConnectionState ()) {
				case AbstractDataReader.CONNECTION_PARTIAL:
					statusFlasher.setFlashPattern (statusFlasher.FLASH_DISCONNECT_ONE);
					break;
				case AbstractDataReader.CONNECTION_NONE:
					statusFlasher.setFlashPattern (statusFlasher.FLASH_DISCONNECT_ALL);            
					break;
				default:
                   // connected ok
					{
						if (inSession) {
							if (timeLeftInGame <= 0) {
								statusFlasher.setFlashPattern (statusFlasher.FLASH_FINISHED);
							} else if (timeLeftInGame < 10) {
								statusFlasher.setFlashPattern (statusFlasher.FLASH_FINISHING);
							} else {
								statusFlasher.setFlashPattern (statusFlasher.FLASH_RUNNING);
							}
						} else {
							statusFlasher.setFlashPattern (null);
						}                        
					}
					break;
				}
			}
		}

        swingPeriod = swingCycleTime;
        halfSwingPeriod = swingPeriod / 2f;

        shopping["swing_angle_at_time_t"] = Mathf.Deg2Rad * (swingAngle);
        shopping["delta_swing_angle_at_time_t"] = shopping["swing_angle_at_time_t"] - last_swing_angle_at_time_t;
        last_swing_angle_at_time_t = shopping["swing_angle_at_time_t"];

        shopping["swing_angular_velocity_at_time_t"] = shopping["delta_swing_angle_at_time_t"] / Time.deltaTime;

        shopping["rider_CofG_x_coordinate_at_time_t"] = centre_of_gravity_radius * Mathf.Sin(shopping["swing_angle_at_time_t"]);
        shopping["delta_rider_CofG_x_coordinate_at_time_t"] = shopping["rider_CofG_x_coordinate_at_time_t"] - last_rider_CofG_x_coordinate_at_time_t;
        last_rider_CofG_x_coordinate_at_time_t = shopping["rider_CofG_x_coordinate_at_time_t"];

        shopping["rider_CofG_z_coordinate_at_time_t"] = centre_of_gravity_radius * Mathf.Sin(shopping["swing_angle_at_time_t"]) * Mathf.Tan(shopping["swing_angle_at_time_t"] / 2f);
        shopping["delta_rider_CofG_z_coordinate_at_time_t"] = shopping["rider_CofG_z_coordinate_at_time_t"] - last_rider_CofG_z_coordinate_at_time_t;
        last_rider_CofG_z_coordinate_at_time_t = shopping["rider_CofG_z_coordinate_at_time_t"];

        shopping["rider_eye_x_coordinate_at_time_t"] = rider_eye_radius * Mathf.Sin(shopping["swing_angle_at_time_t"]);
        shopping["delta_rider_eye_x_coordinate_at_time_t"] = shopping["rider_eye_x_coordinate_at_time_t"] - last_rider_eye_x_coordinate_at_time_t;
        last_rider_eye_x_coordinate_at_time_t = shopping["rider_eye_x_coordinate_at_time_t"];

        shopping["rider_eye_z_coordinate_at_time_t"] = 2f * rider_eye_radius * (Mathf.Sin(shopping["swing_angle_at_time_t"]) * Mathf.Sin(shopping["swing_angle_at_time_t"]));
        shopping["delta_rider_eye_z_coordinate_at_time_t"] = shopping["rider_eye_z_coordinate_at_time_t"] - last_rider_eye_z_coordinate_at_time_t;
        last_rider_eye_z_coordinate_at_time_t = shopping["rider_eye_z_coordinate_at_time_t"];

        shopping["rider_CofG_velocity_in_x"] = shopping["delta_rider_CofG_x_coordinate_at_time_t"] / Time.deltaTime;
        shopping["rider_CofG_velocity_in_z"] = shopping["delta_rider_CofG_z_coordinate_at_time_t"] / Time.deltaTime;
        shopping["rider_eye_velocity_in_x"] = shopping["delta_rider_eye_x_coordinate_at_time_t"] / Time.deltaTime;
        shopping["rider_eye_velocity_in_z"] = shopping["delta_rider_eye_z_coordinate_at_time_t"] / Time.deltaTime;

        shopping["body_linear_speed"] = shopping["swing_angular_velocity_at_time_t"] * centre_of_gravity_radius;
        shopping["eye_linear_speed"] = shopping["swing_angular_velocity_at_time_t"] * rider_eye_radius;

        shopping["centripetal_Force_per_unit_mass"] = centre_of_gravity_radius * (shopping["swing_angular_velocity_at_time_t"] * shopping["swing_angular_velocity_at_time_t"]);
        shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] = gravity * Mathf.Sin(shopping["swing_angle_at_time_t"]);
        shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"] = gravity * Mathf.Cos(shopping["swing_angle_at_time_t"]);

        shopping["resultant_force_per_unit_mass_at_CofG"] = Mathf.Sqrt(((shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"]) * (shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"])) + (shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] * shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"]));

        shopping["angle_as_offset_from_perpendicular_to_CofG_direction"] = shopping["swing_angle_at_time_t"] - Mathf.Atan((shopping["centripetal_Force_per_unit_mass"] * Mathf.Sin(shopping["swing_angle_at_time_t"])) / ((shopping["centripetal_Force_per_unit_mass"] * Mathf.Cos(shopping["swing_angle_at_time_t"])) + shopping["resultant_force_per_unit_mass_at_CofG"]));

        shopping["Centripetal_G_Force"] = (centre_of_gravity_radius / gravity) * (shopping["swing_angular_velocity_at_time_t"] * shopping["swing_angular_velocity_at_time_t"]);
        shopping["Gravity_G_Force_at_CofG_tangential_to_direction"] = Mathf.Cos(shopping["swing_angle_at_time_t"]);
        shopping["Gravity_G_Force_at_CofG_perpendicular_to_direction"] = Mathf.Sin(shopping["swing_angle_at_time_t"]);

        shopping["resultant_G_Force_at_CofG"] = (1f / gravity) * (Mathf.Sqrt((shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"]) * (shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"])) + (shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] * shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"]));

		//if (debugText != null) {
		//	debugText.text = "" + swingAngle + ":" + swingQuadrant + ":" + swingPhase;
		//}

    }

    //	private IEnumerator fader(){
    //		fading = true;
    //		yield return new WaitForSeconds(2f);
    //		killer.enabled = true;
    //		fading = false;
    //		faded = true;
    //	}

    void updateCycleTimes(int phase)
    {
        if(lastCycleTimes[phase]>0)
        {
            if(swingCycleTimeFound)
            {
                swingCycleTime=swingCycleTime*0.5f + (Time.time-lastCycleTimes[phase])*0.5f;
            }else
            {
                swingCycleTime=Time.time-lastCycleTimes[phase];
                swingCycleTimeFound=true;
            }
        }
        lastCycleTimes[phase]=Time.time;        
    }

	float MapASin(float phaseIn)
	{
		float intPart=Mathf.Floor (phaseIn);
		float fracPart = phaseIn - intPart;
		// 0 = 0---> 1, 2== 0-->-1
		if (intPart == 0 || intPart == 2) {
			float sinVal = Mathf.Asin (fracPart);
			return intPart + (sinVal / (.5f * Mathf.PI));
		} else {
			// 1 = 1-->0, 3= -1->0
			float cosVal = Mathf.Acos (fracPart);
			return intPart + 1 - (cosVal / (.5f * Mathf.PI));
		}
	}

	public int getSwingQuadrant()
	{
		return swingQuadrant;
	}


}
