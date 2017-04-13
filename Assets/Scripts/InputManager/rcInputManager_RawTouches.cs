#if UNITY_EDITOR || true
    //#define TS_TOUCHES  	// Touches from touch server
#else
#if UNITY_IPHONE
    #define UNITY_TOUCHES   // Touches from unity api
#else
	//#define W7_TOUCHES  	// Touches from win7 api
    #define TS_TOUCHES  	// Touches from touch server
#endif
#endif

#define DEBUG_MOUSE_AS_TOUCHES


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class rcInputManager : rcMonoBehaviourManual
{
#if W7_TOUCHES
    W7TouchManager w7touchManager;
#endif
#if TS_TOUCHES
    rcTouchServer touchServer;
#endif

    RawTouchStream rawTouchStream = new RawTouchStream();

#if DEBUG_MOUSE_AS_TOUCHES && !UNITY_TOUCHES    // Note: Unity emulates the mouse using the average touches position
    Vector2 lastMousePixelPos = Vector2.zero;
#endif

    public struct RawTouchInput
    {
        public enum eState { Down, Cont, Up }
        public eState state;
        //public TouchPhase phase;
        public int fingerID;
        public Vector2 pixelPos;
        public int downID;
    }

    //const int MAX_Touches = 1;
    const int MAX_Touches = 32;//16;
    int nextRawTouchInputID;

    List<RawTouchInput> rawTouchInputsCurr = new List<RawTouchInput>(MAX_Touches);
    List<RawTouchInput> rawTouchInputsPrev = new List<RawTouchInput>(MAX_Touches);
    int fixupCount;

    public bool virtualMouseDown;
    public bool virtualMouseUp;
    public Vector3 virtualMousePos;

    public float virtualMouseWheel;
    public bool virtualMouseWheelActive = false;
    public bool virtualKeyDown;
    public int virtualKeyCode;

    public bool GetVirtualKeyDown(string key)
    {
        bool result = false;
        if(virtualKeyDown)
        {
            Debug.Log("key: " + virtualKeyDown + " " + key.ToCharArray()[0]);
            result = virtualKeyCode == key.ToCharArray()[0];
        }
        else
        {
            result = Input.GetKeyDown(key);
        }

        return result;
    }

    //
    // RawTouchStream
    //
    public class RawTouchStream
    {
        public struct TouchID
        {
            public TouchID(Touch zTouch)
            {
                Assert(zTouch.id != -1, "ERROR no touch ID assigned");

                touch = zTouch;
                id = zTouch.id;
            }

            public bool isSet { get { return touch != null; } }
            public bool isAlive { get { return touch.id == id; } }

            public void Clear()
            {
                touch = null;
                id = -1;
            }

            public Touch Touch { get { return touch; } }
            Touch touch;
            int id;
        }

        public class Touch
        {
            public enum eState { Down, ContMoved, ContStationary, Up }

            public eState state;
            public InputBase owner;
            public Vector2 pixelPos, pixelPosOnDown;
            public Vector2 normPos, normPosOnDown;
            //public TouchPhase phase;
            public int id = -1;
            //public int fingerID;
            public bool isOnButton;
            //public bool claimed;
            public bool alive;
            public float timeOnDown;
        }
        
        public int numTouches_ { get { return touches_.Count; } }
        public List<Touch> touches_ = new List<Touch>();
        public List<Touch> touchPool = new List<Touch>();


        //
        // Constructor
        //
        public RawTouchStream()
        {
            // Fill the touch pool
            for (int i = 0; i < MAX_Touches; ++i)
                touchPool.Add(new Touch());
        }


        //
        // UpdateFromInput
        //
        public void UpdateFromInput(List<RawTouchInput> zInputs, int zFrameCount, rcInputManager zInputMan)
        {
            foreach (var it in touches_)
                it.alive = false;

            for (int i = 0; i < zInputs.Count; ++i)
            {
                var input = zInputs[i];

                // Find
                var found = touches_.Find(a => (a.id == input.downID));

                if (found != null)
                {
                    Assert( input.state != RawTouchInput.eState.Down, "ERROR (a)");
                }

                if (input.state == RawTouchInput.eState.Down)
                {
                    // This must be a new touch
                    Assert(touchPool.Count >= 1, "ERROR - touchPool empty!");
                    Assert(input.downID != -1, "ERROR (b) - downID: " + input.downID);

                    var newTouch = touchPool[touchPool.Count - 1];
                    touchPool.RemoveAt(touchPool.Count - 1);
                    touches_.Add(newTouch);

                    // Check new touch is in expected state
                    Assert(newTouch.id == -1, "ERROR - new touch not in expected state");
                    Assert(newTouch.owner == null, "ERROR - new touch not in expected state");

                    // Setup new touch
                    newTouch.id = input.downID;
                    //newTouch.fingerID = input.fingerID;
                    newTouch.state = Touch.eState.Down;
                    newTouch.pixelPos = input.pixelPos;
                    newTouch.pixelPosOnDown = input.pixelPos;
                    newTouch.timeOnDown = Time.realtimeSinceStartup;

                    zInputMan.DebugLog("TouchState.Down - ID: " + newTouch.id + " - frame: " + zFrameCount + " - fingerID: " + input.fingerID);

                    newTouch.alive = true;
                }
                else if(input.state == RawTouchInput.eState.Up)
                {
                    found.alive = true;
                    found.pixelPos = input.pixelPos;
                    found.state = Touch.eState.Up;
                }
                else
                {
                    //Debug.LogWarning("c - " + input.state);

                    Assert(found != null, "ERROR - found should not be null - ID: " + input.downID + " - frame: " + zFrameCount);

                    found.alive = true;
                    found.pixelPos = input.pixelPos;
                    found.state = Touch.eState.ContMoved;
                }
            }

            // Recycle back to pool
            for (int i = touches_.Count - 1; i >= 0; --i)
            {
                var touch = touches_[i];
                if (touch.alive == false)
                {
                    Assert(touch.id != -1, "ERROR - touch id is -1 - frame: " + zFrameCount);

                    zInputMan.DebugLog("Recycling touchID: " + touch.id + " - frame: " + zFrameCount);

                    touches_.RemoveAt(i);
                    touchPool.Add(touch);
                    touch.id = -1;
                    touch.owner = null;

                    i -= 1; // Now there is one less in the list
                }
            }

            foreach (var it in touches_)
            {
                var pos = it.pixelPos;
                pos.x /= (float)Screen.width;
                pos.y /= (float)Screen.height;
                //pos.y = 1.0f - pos.y;
                it.normPos = pos;

                pos = it.pixelPosOnDown;
                pos.x /= (float)Screen.width;
                pos.y /= (float)Screen.height;
                //pos.y = 1.0f - pos.y;
                it.normPosOnDown = pos;
            }
        }
    }


    //
    // GetStateFromPhase
    //
    RawTouchInput.eState GetStateFromPhase(TouchPhase zTouchPhase)
    {
        switch (zTouchPhase)
        {
            case TouchPhase.Began: return RawTouchInput.eState.Down;
            case TouchPhase.Ended: return RawTouchInput.eState.Up;
            case TouchPhase.Canceled: return RawTouchInput.eState.Up;
            default: return RawTouchInput.eState.Cont;
        }
    }


    //
    // Read_RawTouches
    //
    void Read_RawTouches()
    {
        //if (nextRawTouchInputID == 0)
            //Debug.ClearDeveloperConsole();

        rawTouchInputsCurr.Clear();

        int count = 0;

#if W7_TOUCHES

        w7touchManager.UpdateManual();

        count = W7TouchManager.GetTouchCount();
        for (int i = 0; i < count; ++i)
        {
            var t = W7TouchManager.GetTouch(i);

            RawTouchInput rawTouchInput;
            rawTouchInput.fingerID = (int)t.Id;
            rawTouchInput.state = GetStateFromPhase(t.phase);
            rawTouchInput.pixelPos = t.position;
            rawTouchInput.downID = -1;
            rawTouchInputsCurr.Add(rawTouchInput);
        }

#endif

#if TS_TOUCHES

        count = touchServer.touchCount;
        for (int i = 0; i < count; ++i)
        {
            var t = touchServer.GetTouch(i);

            RawTouchInput rawTouchInput;
            rawTouchInput.fingerID = t.fingerId;
            rawTouchInput.state = GetStateFromPhase(t.phase);
            rawTouchInput.pixelPos = t.position;
            rawTouchInput.downID = -1;
            rawTouchInputsCurr.Add(rawTouchInput);
        }

        rawGyro = touchServer.Gyro.attitude;
        rawGyroGravity = new Vector3(-touchServer.Gyro.gravity.y, touchServer.Gyro.gravity.x, touchServer.Gyro.gravity.z);  // Matches with unity 4
        rawGyroAcceleration = new Vector3(-touchServer.Gyro.userAcceleration.y, touchServer.Gyro.userAcceleration.x, touchServer.Gyro.userAcceleration.z);  // Matches with unity 4

#endif

        

#if UNITY_TOUCHES
	
		count = Input.touchCount;
		for (int i = 0; i < count; ++i)
		{
			var t = Input.GetTouch(i);

            RawTouchInput rawTouchInput;
            rawTouchInput.fingerID = t.fingerId;
            rawTouchInput.state = GetStateFromPhase(t.phase);
            rawTouchInput.pixelPos = t.position;
            rawTouchInput.downID = -1;
            rawTouchInputsCurr.Add(rawTouchInput);
		}

        rawGyro = Quaternion.Euler(90.0f, 90.0f, 0.0f) * Input.gyro.attitude * Quaternion.Euler(0.0f, 0.0f, 180.0f);
        rawGyroGravity = Input.gyro.gravity;
        rawGyroAcceleration = Input.gyro.userAcceleration;
		
#endif

        //Input.gyro.

#if DEBUG_MOUSE_AS_TOUCHES && !UNITY_TOUCHES    // Note: Unity emulates the mouse using the average touches position

        bool mouseDown = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || virtualMouseDown;
        bool mouseUp = Input.GetMouseButtonUp(0) || virtualMouseUp;

        if (mouseDown || mouseUp)
        {
            count += 1;

            var mousePos = (virtualMouseDown || virtualMouseUp) ? virtualMousePos : Input.mousePosition;
            Vector2 mousePixelPos = mousePos;
            
            //Debug.Log("MousePos: " + mousePixelPos);

            RawTouchInput rawTouchInput;
            rawTouchInput.fingerID = 99;    // 99 reserved for mouse
            rawTouchInput.pixelPos = mousePixelPos;
            rawTouchInput.downID = -1;

            if (mouseDown)
            {
                rawTouchInput.state = RawTouchInput.eState.Down;
                //DebugLog("mouse-began - frame:" + frameCount);
            }
            else if (mouseUp)
            {
                rawTouchInput.state = RawTouchInput.eState.Up;
                //DebugLog("mouse-ended - frame:" + frameCount);
            }
            else
            {
                if (lastMousePixelPos == mousePixelPos)
                    rawTouchInput.state = RawTouchInput.eState.Cont;
                else
                    rawTouchInput.state = RawTouchInput.eState.Cont;    // May add a moved state sometime
                //DebugLog("mouse-other - frame: " + frameCount);
            }

            rawTouchInputsCurr.Add(rawTouchInput);

            lastMousePixelPos = mousePos;
        }

        // Don't clear mouseDown flag, let it persist until the mouseUp is received in rtWebSocketServer.OnBinaryFrame()
        //virtualMouseDown = false;
        virtualMouseUp = false;

#endif

        RawTouchInputsCleanupData();
        RawTouchInputsManageIDs();
        rawTouchStream.UpdateFromInput(rawTouchInputsCurr, frameCount, this);

        AddDebugStr("src Count: " + count + " / rawTouchInputsCurr.Count: " + rawTouchInputsCurr.Count + " - free: " + rawTouchStream.touchPool.Count + " - fixup: " + fixupCount);
#if false
        for (int i = 0; i < rawTouchInputsCurr.Count; ++i)
        {
            var it = rawTouchInputsCurr[i];
            AddDebugStr("touchID: " + it.downID + " - state: " + it.state);
        }

        AddDebugStr(" - ");
        AddDebugStr("numTouches: " + rawTouchStream.numTouches_);

        for (int i = 0; i < rawTouchStream.numTouches_; ++i)
        {
            var it = rawTouchStream.touches_[i];
            AddDebugStr("touchID: " + it.id + " - state: " + it.state + "pos: " + it.normPosOnDown);
        }
#endif
    }


    //
    // RawTouchInputsCleanupData
    //
    void RawTouchInputsCleanupData()
    {
        // Loop over current
        for (int i=0; i<rawTouchInputsCurr.Count; ++i)
        {
            var curr = rawTouchInputsCurr[i];
            var existedLastFrame = rawTouchInputsPrev.FindIndex(a => (a.fingerID == curr.fingerID));

            // If didn't exist last frame
            if (existedLastFrame == -1)
            {
                // Error
                if (curr.state != RawTouchInput.eState.Down)
                {
                    curr.state = RawTouchInput.eState.Down;
                    fixupCount++;
                    //Debug.Log("fixup (a)");
                }
            }
            // If did exist last frame
            else
            {
                var prev = rawTouchInputsPrev[existedLastFrame];

                // Error
                if (curr.state == RawTouchInput.eState.Down && prev.state != RawTouchInput.eState.Up)
                {
                    curr.state = RawTouchInput.eState.Cont;
                    fixupCount++;
                    //Debug.Log("fixup (b)");
                }

                // 
                if (curr.state == RawTouchInput.eState.Cont && prev.state == RawTouchInput.eState.Up)
                {
                    curr.state = RawTouchInput.eState.Down;
                    fixupCount++;
                    Debug.LogWarning("fixup (d)");
                }
            }

            rawTouchInputsCurr[i] = curr;
        }

        // Loop over previous
        for (int i = 0; i < rawTouchInputsPrev.Count; ++i)
        {
            var prev = rawTouchInputsPrev[i];
            var existsThisFrame = rawTouchInputsCurr.FindIndex(a => (a.fingerID == prev.fingerID));

            // If doesn't exist this frame, but did last (and wasn't an up)
            if (existsThisFrame == -1 && prev.state != RawTouchInput.eState.Up)
            {
                RawTouchInput rawTouchInput;
                rawTouchInput.fingerID = prev.fingerID;
                rawTouchInput.state = RawTouchInput.eState.Up;
                rawTouchInput.pixelPos = prev.pixelPos;
                //rawTouchInput.downID = -1;
                rawTouchInput.downID = prev.downID;

                //Debug.Log("hello b");

                rawTouchInputsCurr.Add(rawTouchInput);

                fixupCount++;
                //Debug.Log("fixup (c) - ID: " + rawTouchInput.downID);
            }
        }
    }


    //
    // RawTouchInputsManageIDs
    //
    void RawTouchInputsManageIDs()
    {
        // Loop over current
        for (int i = 0; i < rawTouchInputsCurr.Count; ++i)
        {
            var curr = rawTouchInputsCurr[i];
            if (curr.state == RawTouchInput.eState.Down)
            {
                curr.downID = nextRawTouchInputID++;
                rawTouchInputsCurr[i] = curr;

                DebugLogRawTouch("ID: " + curr.downID + " - state: Down - frame: " + frameCount + " - finderID: " + curr.fingerID);
            }
            else
            {

                var existedLastFrame = rawTouchInputsPrev.FindIndex(a => (a.fingerID == curr.fingerID));

                Assert(existedLastFrame != -1, "Error abc");

                var prev = rawTouchInputsPrev[existedLastFrame];

                curr.downID = prev.downID;
                rawTouchInputsCurr[i] = curr;



                Assert(curr.downID != -1, "Error here - ID: " + curr.downID + " - frame: " + frameCount + " finderID: " + curr.fingerID);

                DebugLogRawTouch("ID: " + curr.downID + " - state: " + curr.state + " - frame: " + frameCount + " - finderID: " + curr.fingerID);
            }
        }

        // Now copy current to previous, ready for next frame
        rawTouchInputsPrev.Clear();
        for (int i = 0; i < rawTouchInputsCurr.Count; ++i)
            rawTouchInputsPrev.Add(rawTouchInputsCurr[i]);
    }


    //
    // InitRawTouches
    //
    void InitRawTouches()
    {
#if W7_TOUCHES
        w7touchManager = gameObject.AddComponent<W7TouchManager>();
#endif
#if TS_TOUCHES

        var inputMgr = GameObject.Find("/InputMgr");
        if (inputMgr)
            touchServer = inputMgr.GetComponent<rcTouchServer>();
        if (!touchServer)
            touchServer = rcObjectMgr.touchTracker.GetComponent<rcTouchServer>();

#endif
    }
}
