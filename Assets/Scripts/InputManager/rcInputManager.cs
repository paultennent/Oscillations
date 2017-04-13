//#define DEBUG_LOGGING
//#define DEBUG_ONSCREEN
#define UPDATE_STICKS
#define UPDATE_BUTTONS
#define UPDATE_GYROS
#define UPDATE_TOUCHES
#define UPDATE_GESTURES

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using rcCore;

public partial class rcInputManager : rcMonoBehaviourManual
{
    public enum PadSticks { Left, Right }
    public enum PadButtons { DISABLED = 0, A, B, X, Y, Back, Start, TrgL, TrgR };
    string[] PadButtonNames = { "DISABLED", "Pad_A", "Pad_B", "Pad_X", "Pad_Y", "Pad_Back", "Pad_Start", "Pad_LTrg", "Pad_RTrg" };

    public class InputBase
    {
    }

    // TouchArea
    public abstract class TouchArea : InputBase
    {
        public class Touch
        {
            public Vector2 pixelPos;
            public Vector2 normPos;
            public bool down;
            public bool up;
            public int id;
            public Object userObject;
            public float userTimer;
            public bool takeOwnershipNextFrame;
        }

        public abstract int Count { get; }
        public abstract Touch GetTouch(int zIndex);

        //public abstract void TakeOwnershipOf( Touch zTouch );

        public abstract void Enable(rcMath.Rect zArea, bool zIgnoreOwnership);

        // Helper
        public void Enable(bool zIgnoreOwnership)
        {
            var rect = new rcMath.Rect(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            Enable(rect, zIgnoreOwnership);
        }
    };

    // Stick
    public abstract class Stick : InputBase
    {
        public float axisX;
        public float axisY;
        public bool touchActive;
        public bool touchOn;    // Hack to keep paint mode rotation working

        //public int tmpTouchFingerID;

        public abstract void EnableHardware(PadSticks zPadStick);
        public abstract void DisableHardware();

        public abstract void EnableTouch(rcMath.Rect zArea);
        public abstract void DisableTouch();

        // - Helpers -
        public void EnableTouch()
        {
            var rect = new rcMath.Rect(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            EnableTouch(rect);
        }

		public void constrainStick(float maxVal) {
			axisX = Mathf.Clamp (axisX, -maxVal, maxVal);
			axisY = Mathf.Clamp (axisY, -maxVal, maxVal);
		}
    }

    // Button
    public abstract class Button : InputBase
    {
        public bool down, up, cont, trg, pre;
        public float timeDown = 0;
        public Vector2 pixelPos;

        public abstract void EnableTouch(rcMath.Rect zArea);
        public abstract void EnableTouch(rcMath.Rect zArea, Vector3 zWorldCenter, Camera zCam);
        public abstract void EnableTouch(Bounds zBounds, Camera zCam);
        public abstract void EnableTouch();
        public abstract void DisableTouch();
        public abstract void EnablePad(int zPadIndex, PadButtons zButton);
        public abstract void EnableKey(char zKey);
        public abstract rcMath.Rect GetTouchArea();
        public abstract bool GetIsTouchEnabled();

        public bool touchSwallowOnDown;
        public bool dbgOn;
    }

    // Gryo
    public abstract class Gyro : InputBase
    {
        public Quaternion rotation = Quaternion.identity;
        public bool swipeEnabled;
        public int swipeCount;

        public float dbgSwipePitch;
        public float dbgSwipeYaw;
        public Vector3 dbgBaseRotOffset;

        // Hardware gyro (can also emulated with the gamepad)
        public abstract void EnableHardware(bool zEnabled);
        public abstract void EnableGamepad(bool zDefaultToPad, bool zAllowRoll, float zInitialPitch = 0.0f, float zInitialYaw = 0.0f, float zInitialRoll = 0.0f);
        public abstract void EnableGamepadRTM(bool zDefaultToPad, bool zAllowRoll, float zInitialPitch = 0.0f, float zInitialYaw = 0.0f, float zInitialRoll = 0.0f);
        public abstract void DisableGamepad();

        // Swipe
        public abstract void EnableSwipe(bool zPitchUpdateOn, bool zYawUpdateOn, bool zInvYaw, rcMath.Rect zRect);
        public abstract void DisableSwipe();

        public abstract void ResetSwipePitch(float zValue = 0.0f);
        public abstract void ResetSwipeYaw(float zValue = 0.0f);
        public abstract void SetSwipePitchUpdateOn(bool zValue);
        public abstract void SetSwipeYawUpdateOn(bool zValue);

        // Base Yaw rotation
        public abstract void ResetBaseYawRotation(Quaternion zRot, bool zLimitYaw = false);

        public abstract void SetYawLimit(bool zEnable, float zLimit );

        public abstract void SetAutoSmoothFrom(Quaternion zFromRot);

        public abstract void RefreshNow();

        // Helper
        public void EnableSwipe(bool zUpdatePitch, bool zUpdateYaw, bool zInvYaw = false)
        {
            var rect = new rcMath.Rect(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            EnableSwipe(zUpdatePitch, zUpdateYaw, zInvYaw, rect);
        }


        protected Gyro() {}
    }

    // Gesture
    public abstract class Gesture : InputBase
    {
        public bool TouchActive { get; set; }
        public bool GestureStarted { get; set; }
        public bool GestureEnded { get; set; }

        public Camera WorldCamera { get; set; }

        public abstract void EnableTouch(rcMath.Rect zArea);
        public abstract void DisableTouch();
        public abstract void Reset();
        public abstract List<Vector3> GetWorldPoints();

        // - Helpers -
        public void EnableTouch()
        {
            var rect = new rcMath.Rect(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            EnableTouch(rect);
        }
    }

    public class RaycastHitInfo
    {
        public Camera rayFromCamera;
        public RaycastHit[] hits;
        public bool isTouch;
        public bool justTouched;
        public bool released;
    }

    // - Runtime data -----------------------------------------------------------------------------
    Quaternion rawGyro = Quaternion.identity;
    Vector3 rawGyroGravity = Vector3.zero;
    Vector3 rawGyroAcceleration = Vector3.zero;
    List<RawTouchStream> rawTouchStreams = new List<RawTouchStream>();
    List<TouchAreaInternal> touchAreas = new List<TouchAreaInternal>();
    List<StickInternal> sticks = new List<StickInternal>();
    List<ButtonInternal> buttons = new List<ButtonInternal>();
    List<GyroInternal> gyros = new List<GyroInternal>();
    List<GestureInternal> gestures = new List<GestureInternal>();
    List<RaycastHitInfo> raycasts = new List<RaycastHitInfo>();
    int frameCount;

    // - Debug ------------------------------------------------------------------------------------

    // - Properties -------------------------------------------------------------------------------
    public RawTouchStream RawTouches { get { return rawTouchStream; } }
    public Quaternion RawGyro { get { return rawGyro; } }
    public Vector3 RawGyroGravity { get { return rawGyroGravity; } }
    public Vector3 RawGyroAcceleration { get { return rawGyroAcceleration; } }
    public bool ShowInputRects { get; set; }
    public bool ShowInputRay { get; set; }


    //
    // RegRawTouchStream
    //
    public RawTouchStream RegRawTouchStream()
    {
        var rawTouchStream = new RawTouchStream();
        rawTouchStreams.Add(rawTouchStream);
        return rawTouchStream;
    }

    //
    // RegTouchArea
    //
    public TouchArea RegTouchArea()
    {
        var touchAreaInternal = new TouchAreaInternal(this, rawTouchStream);
        touchAreas.Add(touchAreaInternal);
        return touchAreaInternal;
    }


    //
    // RegGyro
    //
    public Gyro RegGyro()
    {
        var gyroInternal = new GyroInternal(this);
        gyros.Add(gyroInternal);
        return gyroInternal;
    }


    //
    // RegStick
    //
    public Stick RegStick()
    {
        var stickInternal = new StickInternal(this, rawTouchStream);
        sticks.Add(stickInternal);
        return stickInternal;
    }


    //
    // RegButton
    //
    public Button RegButton()
    {
        var buttonInternal = new ButtonInternal(this, rawTouchStream);
        buttons.Add(buttonInternal);
        return buttonInternal;
    }


    //
    // RegGesture
    //
    public Gesture RegGesture()
    {
        var gestureInternal = new GestureInternal(this, rawTouchStream);
        gestures.Add(gestureInternal);
        return gestureInternal;
    }


    //
    // RegRaycast
    //
    public RaycastHitInfo RegRaycast(Camera cam)
    {
        var raycast = new RaycastHitInfo();
        raycast.rayFromCamera = cam;
        raycasts.Add(raycast);
        return raycast;
    }


    //
    // AwakeManual
    //
    public override void AwakeManual()
    {
        base.AwakeManual();

        InitRawTouches();
    }


    //
    // UpdateManual
    //
    public override void UpdateManual(float zRealDT, float zDT)
    {
        base.UpdateManual(zRealDT, zDT);

        ClearDebgStrs();

        UpdatePhase0(zDT);
        UpdatePhase1(zDT);
        UpdatePhase2(zDT, false);
        UpdateDebug();

        ReadInput_Raycasts();

        frameCount++;
    }


    //
    // LateUpdateManual
    //
    public override void LateUpdateManual()
    {
        base.LateUpdateManual();

        UpdatePhase2(Time.deltaTime, true);
    }


    //
    // UpdatePhase0
    //
    void UpdatePhase0(float zDT)
    {
        Read_RawTouches();

        for (int i = 0; i < rawTouchStream.numTouches_; ++i)
            rawTouchStream.touches_[i].isOnButton = false;
    }


    //
    // UpdatePhase1
    //
    void UpdatePhase1(float zDT)
    {
        // Buttons
#if UPDATE_BUTTONS
        foreach (var button in buttons)
            button.UpdatePhase1(zDT);
#endif
        // Sticks
#if UPDATE_STICKS
        foreach (var stick in sticks)
            stick.UpdatePhase1(zDT);
#endif
        // Gyros
#if UPDATE_GYROS
        foreach (var gyro in gyros)
            gyro.UpdatePhase1(zDT);
#endif
        // Touches
#if UPDATE_TOUCHES
        foreach (var touchArea in touchAreas)
            touchArea.UpdatePhase1(zDT);
#endif
        // Gestures
#if UPDATE_GESTURES
        foreach (var gesture in gestures)
            gesture.UpdatePhase1(zDT);
#endif
    }


    //
    // UpdatePhase2
    //
    void UpdatePhase2(float zDT, bool zLateUpdate)
    {
        // Sticks
#if UPDATE_STICKS
        if (!zLateUpdate)
        {
            foreach (var stick in sticks)
                stick.UpdatePhase2(zDT);
        }
#endif
        // Buttons
#if UPDATE_BUTTONS
        if (!zLateUpdate)
        {
            foreach (var button in buttons)
                button.UpdatePhase2(zDT);
        }
#endif
        // Gyros
#if UPDATE_GYROS
        if (!zLateUpdate)
        {
            foreach (var gyro in gyros)
                gyro.UpdatePhase2(zDT);
        }
#endif
        // Touches
#if UPDATE_TOUCHES
        if (!zLateUpdate)
        {
            foreach (var touchArea in touchAreas)
                touchArea.UpdatePhase2(zDT);
        }
#endif

        // Gestures
#if UPDATE_GESTURES
        if (zLateUpdate)
        {
            // This needs to happen in the late update, because the gestures are in world space and
            // therefore rely on the camera position which can be update by the physics engine
            foreach (var gesture in gestures)
                gesture.UpdatePhase2(zDT);
        }
#endif
    }


    //
    // UpdateDebug
    //
    void UpdateDebug()
    {
        // Sticks
#if UPDATE_STICKS
        foreach (var stick in sticks)
            stick.UpdateDbg();
#endif
        // Buttons
#if UPDATE_BUTTONS
        foreach (var button in buttons)
            button.UpdateDbg();
#endif
        // Gyros
#if UPDATE_GYROS
        foreach (var gyro in gyros)
            gyro.UpdateDbg();
#endif
        // Touches
#if UPDATE_TOUCHES
        foreach (var touchArea in touchAreas)
            touchArea.UpdateDbg();
#endif
        // Gestures
#if UPDATE_GESTURES
        foreach (var gesture in gestures)
            gesture.UpdateDbg();
#endif
    }


    //
    // ReadInput_Raycasts
    //
    void ReadInput_Raycasts()
    {
        foreach (var raycast in raycasts)
        {
            var rayFromCamera = raycast.rayFromCamera;
            if (rayFromCamera != null)
            {
                RaycastHit[] hits = null;
                for (int i = 0; i < rawTouchStream.numTouches_; ++i)
                {
                    if (!rawTouchStream.touches_[i].isOnButton)
                    {
                        Ray ray = rayFromCamera.ScreenPointToRay(rawTouchStream.touches_[i].pixelPos);

                        if (ShowInputRay)
                            Debug.DrawRay(ray.origin, ray.direction * 20);

                        hits = Physics.RaycastAll(ray, 100.0f, ~(1 << LayerMask.NameToLayer("GrottoTrackingGyro")));

                        break;
                    }
                }

                if (hits != null)
                {
                    raycast.hits = hits;
                    raycast.justTouched = !raycast.isTouch;
                    raycast.released = false;
                    raycast.isTouch = true;
                }
                else
                {
                    hits = Physics.RaycastAll(rayFromCamera.transform.position, rayFromCamera.transform.forward, 100.0f, ~(1 << LayerMask.NameToLayer("GrottoTrackingFingerTouch")));

                    if (ShowInputRay)
                        Debug.DrawRay(rayFromCamera.transform.position, rayFromCamera.transform.forward * 20);

                    raycast.released = raycast.isTouch;

                    // keep last frame hits if we just released finger touch
                    if (!raycast.released)
                        raycast.hits = hits;

                    raycast.justTouched = false;
                    raycast.isTouch = false;
                }
            }
        }
    }


    //
    // DebugLog
    //
    void DebugLog(string zStr)
    {
#if DEBUG_LOGGING
        Debug.Log(zStr);
#endif
    }


    void DebugLogRawTouch(string zStr)
    {
#if DEBUG_LOGGING
        Debug.Log(zStr);
#endif
    }


    //
    // DebugLogTouch
    //
    void DebugLogTouch(string zStr)
    {
#if DEBUG_LOGGING
//        Debug.Log(zStr);
#endif
    }


    //
    // DebugLogWarning
    //
    void DebugLogWarning(string zStr)
    {
#if DEBUG_LOGGING
        Debug.LogWarning(zStr);
#endif
    }


    //
    // OnRenderObject
    //
#if UNITY_EDITOR
    void OnRenderObject()
    {

        // Gestures
#if UPDATE_GESTURES
        foreach (var gesture in gestures)
            gesture.OnRenderObject();
#endif



        if (!ShowInputRects)
            return;

        rcGL.SetMaterialSolidColorZTestOff();
        rcGL.Begin(true, false);
        rcGL.Color(Color.red);

        foreach (var button in buttons)
        {
            if (button.GetIsTouchEnabled())
            {
                var rect = button.GetTouchArea();
                rcGL.Line(new Vector2(rect.min.x, rect.min.y), new Vector2(rect.max.x, rect.min.y));    // top
                rcGL.Line(new Vector2(rect.min.x, rect.max.y), new Vector2(rect.max.x, rect.max.y));    // botton
                rcGL.Line(new Vector2(rect.min.x, rect.min.y), new Vector2(rect.min.x, rect.max.y));    // left
                rcGL.Line(new Vector2(rect.max.x, rect.min.y), new Vector2(rect.max.x, rect.max.y));    // right
            }
        }

        rcGL.End();
    }
#endif


    //
    // Swap
    //
    static void Swap<T>(ref T x, ref T y)
    {
        T t = y;
        y = x;
        x = t;
    }


    //
    // Assert
    //
    static void Assert(bool zValue, string zStr)
    {
        if (zValue == false)
            Debug.LogError(zStr);
    }


    //
    // AssertWarning
    //
    static void AssertWarning(bool zValue, string zStr)
    {
        if (zValue == false)
            Debug.LogWarning(zStr);
    }


#if DEBUG_ONSCREEN
    List<string> dbgStrings = new List<string>();
#endif
    public void ClearDebgStrs()
    {
#if DEBUG_ONSCREEN
        dbgStrings.Clear();
#endif
    }

    public void AddDebugStr(string zStr)
    {
#if DEBUG_ONSCREEN
        dbgStrings.Add(zStr);
#endif
    }

    void OnGUI()
    {
#if DEBUG_ONSCREEN
        for (int i=0; i<dbgStrings.Count; ++i)
            GUI.Label(new Rect(10, 10 + 15 * i, 400, 20), dbgStrings[i]);
#endif
    }
}