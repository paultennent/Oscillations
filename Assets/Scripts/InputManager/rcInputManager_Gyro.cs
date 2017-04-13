using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class rcInputManager : rcMonoBehaviourManual
{
    public Vector3 virtualGyro;

    //
    // GyroInternal
    //
    class GyroInternal : Gyro
    {
        bool limitYawEnabled;
        float limitYawAmount;
        float limitYawCenter;
        

        public void UpdatePhase1(float zDT)
        {
        }

        public void UpdatePhase2(float zDT)
        {
            Update(zDT);
        }


        //
        // UpdateDbg
        //
        public void UpdateDbg()
        {
        }


        //
        // Constructor
        //
        public GyroInternal(rcInputManager zInputMan)
        {
            inputMan = zInputMan;
            hardwareEnabled = true;
        }

        public override void EnableHardware(bool zEnabled)
        {
            hardwareEnabled = zEnabled;
        }

        public override void EnableGamepad(bool zDefaultToPad, bool zAllowRoll, float zInitialPitch, float zInitialYaw, float zInitialRoll)
        {
            // Don't default to gamepad on the mac
            bool isOSX = Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXEditor;
            if (zDefaultToPad && isOSX)
                zDefaultToPad = false;
            gamepadEnabled = true;
            gamepadOverride = zDefaultToPad;
            gamepadAllowRoll = zAllowRoll;
            gamepadEuler = new Vector3(zInitialPitch, zInitialYaw, zInitialRoll);
            if (gamepadStick == null)
            {
                gamepadStick = inputMan.RegStick();
                gamepadStick.EnableHardware(PadSticks.Left);
            }
        }

        public override void EnableGamepadRTM(bool zDefaultToPad, bool zAllowRoll, float zInitialPitch, float zInitialYaw, float zInitialRoll)
        {
            // Don't default to gamepad on the mac
            bool isOSX = Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXEditor;
            if (zDefaultToPad && isOSX)
                zDefaultToPad = false;
            gamepadEnabled = true;
            gamepadRTMEnabled = true;
            gamepadOverride = zDefaultToPad;
            gamepadAllowRoll = zAllowRoll;
            gamepadEuler = new Vector3(zInitialPitch, zInitialYaw, zInitialRoll);
            if (gamepadStick == null)
            {
                gamepadStick = inputMan.RegStick();
                gamepadStick.EnableHardware(PadSticks.Right);
            }
        }

        public override void DisableGamepad()
        {
            gamepadEnabled = false;
            gamepadRTMEnabled = false;
            gamepadOverride = false;
        }

        public override void EnableSwipe(bool zPitchUpdateOn, bool zYawUpdateOn, bool zInvYaw, rcMath.Rect zRect)
        {
            ResetSwipePitch();
            ResetSwipeYaw();
            SetSwipePitchUpdateOn(zPitchUpdateOn);
            SetSwipeYawUpdateOn(zYawUpdateOn);

            swipeEnabled = true;
            swipeInvYaw = zInvYaw;
            if (swipeStick == null) // Do only once
            {
                swipeStick = inputMan.RegStick();
                swipeStick.EnableTouch(zRect);
            }
        }

        public override void DisableSwipe()
        {
            swipeEnabled = false;
            ResetSwipePitch();
            ResetSwipeYaw();
        }

        public override void ResetSwipePitch(float zValue = 0.0f)
        {
            swipePitch = zValue;
            swipePitchToAddLast = 0.0f;
            swipePitchToAdd = 0.0f;
        }

        public override void ResetSwipeYaw(float zValue = 0.0f)
        {
            swipeYaw = zValue;
            swipeYawToAddLast = 0.0f;
            swipeYawToAdd = 0.0f;
        }

        public override void SetSwipePitchUpdateOn(bool zValue)
        {
            //Debug.LogWarning("SetSwipePitchUpdateOn() : " + zValue);

            swipeUpdatePitch = zValue;
        }
        public override void SetSwipeYawUpdateOn(bool zValue)
        {
            //Debug.LogWarning("SetSwipeYawUpdateOn() : " + zValue);

            swipeUpdateYaw = zValue;
        }

        public override void ResetBaseYawRotation(Quaternion zRot, bool zLimitYaw)
        {
            // Kill the pitch and roll
            var t1 = zRot.eulerAngles;
            t1.x = 0.0f;
            t1.z = 0.0f;

            var t2 = gyroAbsolute.eulerAngles;
            t2.x = 0.0f;
            t2.z = 0.0f;

            baseRotOffset = Quaternion.Inverse(Quaternion.Euler(t2)) * Quaternion.Euler(t1);

/*
            if (zLimitYaw)
            {
                dbgLimitYaw = t2.y;
//                Debug.Log("here a");
            }
            limitYaw = zLimitYaw;
 */
        }

        public override void SetYawLimit(bool zEnable, float zLimit)
        {
            limitYawEnabled = zEnable;
            limitYawAmount = zLimit;
            limitYawCenter = gyroAbsolute.eulerAngles.y;
//            var t2 = gyroAbsolute.eulerAngles;
//            t2.x = 0.0f;
//            t2.z = 0.0f;

        }

        public override void SetAutoSmoothFrom(Quaternion zFromRot)
        {
            autoSmoothFrom = Quaternion.identity;
            autoSmoothT = 0.0f;
            RefreshNow();
            autoSmoothFrom = Quaternion.Inverse(rotation) * zFromRot;
            autoSmoothT = 1.0f;
        }

        public override void RefreshNow()
        {
            Update(0.0f);
        }

        //
        // Update
        //
        void Update(float zDT)
        {
            // Toggle gamepad
            if (gamepadEnabled)
            {
                if (Input.GetButtonDown("Pad_Start"))
                    gamepadOverride = !gamepadOverride;
            }

            // Read gyro
            gyroAbsolute = inputMan.RawGyro;

            // Read gamepad
            if (gamepadEnabled || gamepadRTMEnabled)
            {
                if (gamepadRTMEnabled)
                {
                    gamepadEuler.x = Mathf.Clamp(gamepadEuler.x + 360.0f * gamepadStick.axisY * zDT * 0.3f, -85.0f, 85.0f);  // Pitch
                    float inputX = gamepadStick.axisX;
                    inputX = Mathf.Abs(inputX) < 0.1f ? 0.0f : inputX;

                    gamepadEuler.y = Mathf.Repeat(gamepadEuler.y + 360.0f * inputX * zDT * 0.3f, 360.0f);        // Yaw
                    if (gamepadAllowRoll)
                        gamepadEuler.z = Mathf.Repeat(gamepadEuler.z + 360.0f * -Input.GetAxis("Pad_LS_AxisX") * zDT * 0.1f, 360.0f);    // Roll

                    // Reset Pitch and roll
                    if (Input.GetButtonDown("Pad_X"))
                    {
                        gamepadEuler.x = 0.0f;
                        gamepadEuler.z = 0.0f;
                    }
                }
                else
                {
                    gamepadEuler.x = Mathf.Clamp(gamepadEuler.x + 360.0f * gamepadStick.axisY * zDT * 0.3f, -85.0f, 85.0f);  // Pitch
                    float inputX = gamepadStick.axisX;
                    inputX = Mathf.Abs(inputX) < 0.1f ? 0.0f : inputX;

                    gamepadEuler.y = Mathf.Repeat(gamepadEuler.y + 360.0f * inputX * zDT * 0.3f, 360.0f);        // Yaw
                    if (gamepadAllowRoll)
                        gamepadEuler.z = Mathf.Repeat(gamepadEuler.z + 360.0f * Input.GetAxis("Pad_RS_AxisX") * zDT * 0.3f, 360.0f);    // Roll

                    // Reset Pitch and roll
                    if (Input.GetButtonDown("Pad_X"))
                    {
                        gamepadEuler.x = 0.0f;
                        gamepadEuler.z = 0.0f;
                    }
                }

                // Override with gamepad
                if (gamepadOverride)
                    gyroAbsolute = Quaternion.Euler(gamepadEuler);
            }

            // Kill gyro if hardware is not enabled (this is for swipe only mode)
            if (hardwareEnabled == false)
                gyroAbsolute = Quaternion.identity;

            // Hack to keep paint mode rotation working
            if (swipeStick != null)
                swipeStick.touchOn = (swipeEnabled && (swipeUpdatePitch || swipeUpdateYaw));    

            // Read swipe
            if (swipeEnabled)
            {
                float pitchLimit = 87.0f;

                if (!swipeStick.touchActive)
                {
                    swipePitch = Mathf.Clamp(swipePitch + swipePitchToAdd, -pitchLimit, pitchLimit);
                    swipeYaw = Mathf.Repeat(swipeYaw + swipeYawToAdd, 360.0f);

                    // Track the number of swipe over a certain length
                    if (Mathf.Abs(swipePitchToAdd) > 30.0f || Mathf.Abs(swipeYawToAdd) > 30.0f)
                        swipeCount++;

                    swipePitchToAdd = 0.0f;
                    swipeYawToAdd = 0.0f;
                }
                else
                {
#if true
                    float diffAmount = float.MaxValue;
#else
                    float diffAmount = 20.0f;
#endif

                    if (swipeUpdatePitch)
                    {
                        //var newP = swipeStick._internal.pos.y * 100.0f;
                        var newP = swipeStick.axisY * 100.0f;
                        var diffP = Mathf.Abs(newP - swipePitchToAddLast);
                        if (diffP < diffAmount)  // Fix for bad swipes (when to touches happen at same time far apart)
                            swipePitchToAdd = newP;
                    }
                    if (swipeUpdateYaw)
                    {
                        //var newY = swipeStick._internal.pos.x * 160.0f;
                        var newY = swipeStick.axisX * 160.0f;

                        if (swipeInvYaw)
                            newY = -newY;
                        var diffY = Mathf.Abs(newY - swipeYawToAddLast);
                        if (diffY < diffAmount)  // Fix for bad swipes (when to touches happen at same time far apart)
                            swipeYawToAdd = newY;
                    }
                }

                swipePitchToAddLast = swipePitchToAdd;
                swipeYawToAddLast = swipeYawToAdd;

                dbgSwipePitch = Mathf.Clamp(swipePitch + swipePitchToAdd, -pitchLimit, pitchLimit);
                dbgSwipeYaw = swipeYaw + swipeYawToAdd;

                // Add swipe
                float p = Mathf.Clamp(swipePitch + swipePitchToAdd, -pitchLimit, pitchLimit);
                float y = swipeYaw + swipeYawToAdd;
                var swipeOffset = Quaternion.Euler(new Vector3(p, y, 0.0f));

                gyroAbsolute = swipeOffset * gyroAbsolute;
            }

            var autoSmooth = Quaternion.identity;
            if (autoSmoothT > 0.0f)
            {
                float st = Mathf.SmoothStep(0.0f, 1.0f, autoSmoothT);
                //                Debug.Log("autoSmoothT: " + autoSmoothT + " - st: " + st);
                autoSmooth = Quaternion.Slerp(autoSmoothFrom, Quaternion.identity, 1.0f - st);
                autoSmoothT -= zDT * 1.0f;
            }

            if (limitYawEnabled)
            {
                float curYaw = gyroAbsolute.eulerAngles.y;
                float diff = Mathf.DeltaAngle(limitYawCenter, curYaw);
                float limit = limitYawAmount;

                if (diff >= limit || diff <= -limit )
                {
                    if (diff >= limit)
                    {
                        float d = diff - limit;
                        limitYawCenter += d;
                        baseRotOffset *= Quaternion.Euler(new Vector3(0.0f, -d, 0.0f));

                    }
                    else
                    {
                        float d = diff - -limit;
                        limitYawCenter += d;
                        baseRotOffset *= Quaternion.Euler(new Vector3(0.0f, -d, 0.0f));
                    }



#if false

                    // Kill the pitch and roll
                    var t1 = rotation.eulerAngles;// zRot.eulerAngles;
                    t1.x = 0.0f;
                    t1.z = 0.0f;

                    var t2 = gyroAbsolute.eulerAngles;
                    t2.x = 0.0f;
                    t2.z = 0.0f;

                    baseRotOffset = Quaternion.Inverse(Quaternion.Euler(t2)) * Quaternion.Euler(t1);
#endif
                }

                //rcShowFPS.extraString1 = "limitYaw: " + limitYawCenter.ToString("F1") + " - currentYaw: " + curYaw.ToString("F1") + " - diff:" + diff.ToString("F1");
            }

            // Result
            rotation = baseRotOffset * gyroAbsolute * autoSmooth;

            dbgBaseRotOffset = baseRotOffset.eulerAngles;

            //if (limitYaw)
            //{



                //gyroAbsolute


                //rcShowFPS.extraString = "limitYaw: " + dbgLimitYaw.ToString("F1") + " - currentYaw: " + t2.y.ToString("F1");

                //Debug.Log("aa");

            //}
        }

        rcInputManager inputMan;

        bool hardwareEnabled;

        // Gamepad
        Stick gamepadStick;
        bool gamepadOverride;  // Use the gamepad
        bool gamepadEnabled;
        bool gamepadRTMEnabled;
        bool gamepadAllowRoll;
        Vector3 gamepadEuler;

        // Swipe
        bool swipeUpdateYaw;
        bool swipeUpdatePitch;
        bool swipeInvYaw;
        Stick swipeStick;
        float swipePitch;
        float swipeYaw;
        float swipePitchToAdd;
        float swipePitchToAddLast;
        float swipeYawToAdd;
        float swipeYawToAddLast;

        Quaternion gyroAbsolute = Quaternion.identity;
        Quaternion baseRotOffset = Quaternion.identity;
        Quaternion autoSmoothFrom = Quaternion.identity;
        float autoSmoothT;
    }
}
