using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class rcInputManager : rcMonoBehaviourManual
{
    //
    // StickInternal
    //
    class StickInternal : Stick
    {
        List<RawTouchStream.TouchID> watching = new List<RawTouchStream.TouchID>();
        RawTouchStream.TouchID myTouch;

        //
        // UpdatePhase1
        //
        public void UpdatePhase1(float zDT)
        {
            if (touchEnabled && touchOn)
            {
                for (int i = 0; i < rawTouchStream.numTouches_; ++i)
                {
                    var rawTouch = rawTouchStream.touches_[i];

                    if (rawTouch.state == RawTouchStream.Touch.eState.Down && 
                        rawTouch.owner == null &&
                        touchArea.Contains(rawTouch.normPos))
                    {
                        // Install watch
                        watching.Add(new RawTouchStream.TouchID(rawTouch));
                    }
                }
            }
        }


        //
        // UpdatePhase2
        //
        public void UpdatePhase2(float zDT)
        {
            // Remove dead watch touches
            for (int i = watching.Count - 1; i >= 0; i--)
            {
                var watch = watching[i];
                if (!watch.isAlive)
                {
                    watching.RemoveAt(i);
                    i -= 1;
                    inputMan.DebugLog("removing touch watch, as now dead");
                }
            }

            axisX = 0.0f;
            axisY = 0.0f;

            if (hardwareEnabled)
            {
                if (hardwareStick == PadSticks.Left)
                {
                    axisX = Input.GetAxis("Pad_LS_AxisX");
                    axisY = Input.GetAxis("Pad_LS_AxisY");
                }
                else if (hardwareStick == PadSticks.Right)
                {
                    axisX = Input.GetAxis("Pad_RS_AxisX");
                    axisY = Input.GetAxis("Pad_RS_AxisY");
                }
            }

            touchActive = false;
            if (touchEnabled && touchOn)
            {
                if (myTouch.isSet == false || (myTouch.isSet && myTouch.isAlive == false))
                {
                    for (int i = 0; i < watching.Count; ++i)
                    {
                        var touchID = watching[i];

                        // If touch is still alive and has no owner
                        //if (touchID.isAlive && touchID.touch.owner == null)
                        if (touchID.Touch.owner == null)
                        {
                            var rawTouch = touchID.Touch;

                            float pixelMag = (rawTouch.pixelPos - rawTouch.pixelPosOnDown).magnitude;

                            //Debug.Log("pixelMag: " + pixelMag);

                            if (pixelMag > 5.0f)
                            {
                                // We'll take ownership
                                touchID.Touch.owner = this;
                                inputMan.DebugLog("Stick - taking owner ship of touch");

                                myTouch = touchID;

                                touchPosNormStart = rawTouch.normPos;
                            }
                        }
                    }
                }

                if (myTouch.isSet && myTouch.isAlive)
                {
                    //Debug.Log("touchActive");
                    touchActive = true;

                    var rawTouch = myTouch.Touch;
                    var tmp = rawTouch.normPos - touchPosNormStart;
                    axisX = Mathf.Clamp(tmp.x, -1.0f, 1.0f);
                    axisY = Mathf.Clamp(tmp.y, -1.0f, 1.0f);
                }
            }
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
        public StickInternal(rcInputManager zInputMan, rcInputManager.RawTouchStream zRawTouchStream)
        {
            inputMan = zInputMan;
            rawTouchStream = zRawTouchStream;
            //touchFingerID = -1;
        }

        public override void EnableHardware(PadSticks zPadStick)
        {
            hardwareEnabled = true;
            hardwareStick = zPadStick;
        }

        public override void DisableHardware()
        {
            hardwareEnabled = false;
        }

        public override void EnableTouch(rcMath.Rect zArea)
        {
            touchEnabled = true;
            touchArea = zArea;

            touchOn = true;
        }

        public override void DisableTouch()
        {
            touchEnabled = false;

            touchOn = false;
        }

        rcInputManager inputMan;
        rcInputManager.RawTouchStream rawTouchStream;

        // Real gamepad
        bool hardwareEnabled;
        PadSticks hardwareStick;

        // Touch stick
        bool touchEnabled;
        rcMath.Rect touchArea;
        //int touchFingerID;
        Vector2 touchPosNormStart;
        Vector2 touchPosNormCur;
        //Vector2 touchPos;
        //bool touchUpdated;
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // ----------------------------------------------------------------------------
#if false
    //
    // StickInternal_v1
    //
    class StickInternal_v1 : Stick
    {
        public StickInternal_v1(rcInputManager zInputMan, rcInputManager.RawTouchStream zRawTouchStream)
        {
            //inputMan = zInputMan;
            rawTouchStream = zRawTouchStream;
            touchFingerID = -1;
        }

        public override void EnableHardware(PadSticks zPadStick)
        {
            hardwareEnabled = true;
            hardwareStick = zPadStick;
        }

        public override void DisableHardware()
        {
            hardwareEnabled = false;
        }

        public override void EnableTouch(rcMath.Rect zArea)
        {
            touchEnabled = true;
            touchArea = zArea;
        }

        public override void DisableTouch()
        {
            touchEnabled = false;
        }

        public void Update(float zDT)
        {
            if (hardwareEnabled)
            {
                if (hardwareStick == PadSticks.Left)
                {
                    axisX = Input.GetAxis("Pad_LS_AxisX");
                    axisY = Input.GetAxis("Pad_LS_AxisY");
                }
                else if (hardwareStick == PadSticks.Right)
                {
                    axisX = Input.GetAxis("Pad_RS_AxisX");
                    axisY = Input.GetAxis("Pad_RS_AxisY");
                }
            }

            if (touchEnabled)
            {
                if (touchUpdated == false)
                {
                    touchFingerID = -1;
                    touchPos = Vector2.zero;
                }
                touchUpdated = false;

                for (int i = 0; i < rawTouchStream.numTouches_; ++i)
                {
                    var rawTouch = rawTouchStream.touches_[i];

                    // Assign fingerID to stick
                    if (touchFingerID == -1 &&
                        rawTouch.phase == TouchPhase.Began &&
                        touchArea.Contains(rawTouch.normPos))
                    {
                        touchFingerID = rawTouch.fingerID;
                        touchPosNormStart = rawTouch.normPos;
                    }

                    // Update stick
                    if (touchFingerID == rawTouch.fingerID)
                    {
                        touchUpdated = true;

                        if (rawTouch.phase == TouchPhase.Ended)
                        {
                            //Debug.Log("touchPhase ended");
                            touchPos = Vector2.zero;
                            touchFingerID = -1;
                        }
                        else
                        {
                            touchPosNormCur = rawTouch.normPos;
                            touchPos = touchPosNormCur - touchPosNormStart;
                            //stick._internal.pos *= 3.5f;

                            touchPos.x = Mathf.Clamp(touchPos.x, -1.0f, 1.0f);
                            touchPos.y = Mathf.Clamp(touchPos.y, -1.0f, 1.0f);
                        }

                    }
                }

                if (touchUpdated)
                {
                    axisX = touchPos.x;
                    axisY = touchPos.y;
                }

                tmpTouchFingerID = touchFingerID;
            }
        }

        //rcInputManager inputMan;
        rcInputManager.RawTouchStream rawTouchStream;

        // Real gamepad
        bool hardwareEnabled;
        PadSticks hardwareStick;

        // Touch stick
        bool touchEnabled;
        rcMath.Rect touchArea;
        int touchFingerID;
        Vector2 touchPosNormStart;
        Vector2 touchPosNormCur;
        Vector2 touchPos;
        bool touchUpdated;
    }
#endif
}
