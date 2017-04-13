using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class rcInputManager : rcMonoBehaviourManual
{
    //
    // TouchAreaInternal
    //
    class TouchAreaInternal : TouchArea
    {
        List<RawTouchStream.TouchID> watching = new List<RawTouchStream.TouchID>();
//        RawTouchStream.TouchID myTouch;

        //
        // UpdatePhase1
        //
        public void UpdatePhase1(float zDT)
        {
            for (int i = 0; i < rawTouchStream.numTouches_; ++i)
            {
                var rawTouch = rawTouchStream.touches_[i];

                if (rawTouch.state == RawTouchStream.Touch.eState.Down &&
                    (rawTouch.owner == null || ignoreOwnership))
                    //touchArea.Contains(rawTouch.normPos))
                {
                    // Install watch
                    watching.Add(new RawTouchStream.TouchID(rawTouch));
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
                }
                else if (watch.Touch.owner != null && watch.Touch.owner != this)
                {
                    if (ignoreOwnership == false)
                    {
                        watching.RemoveAt(i);
                        i -= 1;
                    }
                }
            }

            Swap(ref curFrame, ref preFrame);

            curFrame.count = watching.Count;

            for (int i = 0; i < watching.Count; ++i)
            {
                var watch = watching[i];

                // Only skip if the watch has an owner that isn't us
                if ((watch.Touch.owner != null && watch.Touch.owner != this) && ignoreOwnership == false)
                {
                    //Debug.Log("skipping");
                    continue;
                }

                var rawTouch = watch.Touch;
                var touch = curFrame.touches[i];

                touch.pixelPos = rawTouch.pixelPos;
                touch.normPos = rawTouch.normPos;
                touch.down = rawTouch.state == RawTouchStream.Touch.eState.Down;
                touch.up = rawTouch.state == RawTouchStream.Touch.eState.Up;
                touch.id = rawTouch.id;

//                if (touch.down)
//                    Debug.LogWarning("TOUCH DOWN");
//                if (touch.up)
//                    Debug.LogWarning("TOUCH UP");

                if (touch.down)
                {
                    inputMan.DebugLogTouch("TouchArea-Down ID: " + rawTouch.id + " - frame: " + inputMan.frameCount);

                    touch.userObject = null;
                    touch.userTimer = 0.0f;
                    touch.takeOwnershipNextFrame = false;
                }
                else
                {
                    // Transfer the userData
                    for (int j = 0; j < preFrame.count; ++j)
                    {
                        var prevTouch = preFrame.touches[j];

                        if (touch.id == prevTouch.id)
                        {
                            //Debug.Log("transfering");

                            touch.userObject = prevTouch.userObject;
                            touch.userTimer = prevTouch.userTimer;
                            touch.takeOwnershipNextFrame = prevTouch.takeOwnershipNextFrame;

                            if (touch.takeOwnershipNextFrame)
                            {
                                watch.Touch.owner = this;
                                //Debug.LogWarning("takeOwnershipNextFrame");
                            }
                            //if (watch.Touch.owner = this

                            //Debug.Log("trans userObject.name: " + touch.userObject.name);
                            //Debug.Log("trans fingerID:" + touchArea.touches[i].fingerID);
                            break;
                        }
                    }
                }

                if (touch.up)
                {
                    inputMan.DebugLogTouch("TouchArea-Up! ID: " + rawTouch.id + " - frame: " + inputMan.frameCount);
                }
            }
        }


        //
        // UpdateDbg
        //
        public void UpdateDbg()
        {
            inputMan.AddDebugStr("touch - watching count: " + watching.Count);

            for (int i = 0; i < curFrame.count; ++i)
            {
                var c = curFrame.touches[i];
                inputMan.AddDebugStr("touch i: " + i + " userTimer: " + c.userTimer + " takeOwnerShip: " + c.takeOwnershipNextFrame);
            }
        }


        //
        // Constructor
        //
        public TouchAreaInternal(rcInputManager zInputMan, rcInputManager.RawTouchStream zRawTouchStream)
        {
            inputMan = zInputMan;
            rawTouchStream = zRawTouchStream;

            for (int i = 0; i < curFrame.touches.Length; ++i)
                curFrame.touches[i] = new Touch();
            for (int i = 0; i < preFrame.touches.Length; ++i)
                preFrame.touches[i] = new Touch();
        }


        //
        // Enabled
        //
        public override void Enable(rcMath.Rect zArea, bool zIgnoreOwnership)
        {
            ignoreOwnership = zIgnoreOwnership;
//            Debug.LogWarning("ignoreOwnership: " + ignoreOwnership);
        }









        public override int Count { get { return curFrame.count; } }

        public override Touch GetTouch(int zIndex)
        {
            return curFrame.touches[zIndex];
        }

        //public override void TakeOwnershipOf(Touch zTouch)
        //{
            //zTouch.takeOwnershipNextFrame = true;
            
            //touchID.Touch.owner = this;
        //}

        rcInputManager inputMan;
        rcInputManager.RawTouchStream rawTouchStream;

        class Frame
        {
            public int count;
            public Touch[] touches = new Touch[MAX_Touches];
        }
        Frame curFrame = new Frame();
        Frame preFrame = new Frame();

        bool ignoreOwnership;
    }
}
