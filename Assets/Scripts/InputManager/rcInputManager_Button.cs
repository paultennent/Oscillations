using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class rcInputManager : rcMonoBehaviourManual
{
    //
    // ButtonInternal
    //
    class ButtonInternal : Button
    {
        List<RawTouchStream.TouchID> watching = new List<RawTouchStream.TouchID>();

        //
        // UpdatePhase1
        //
        public void UpdatePhase1(float zDT)
        {
            if (touchEnabled)
            {
                for (int i = 0; i < rawTouchStream.numTouches_; ++i)
                {
                    var rawTouch = rawTouchStream.touches_[i];

                    var t = new Vector2(rawTouch.normPos.x, 1.0f - rawTouch.normPos.y);

                    if (rawTouch.state == RawTouchStream.Touch.eState.Down &&
                        rawTouch.owner == null &&
                        touchArea.Contains(t))
                    {
                        if (touchSwallowOnDown)
                        {
                            rawTouch.owner = this;
                            //Debug.Log("Button - taking owner ship of touch");

                            //Debug.Log("touchSwallowOnDown");
                        }
                        else
                        {
                        
                        }

                        // Install watch
                        watching.Add(new RawTouchStream.TouchID(rawTouch));
                    }
                    
                    
                    
/*                    
                    {
                        var t = rawTouchStream.touches_[i].normPos;
                        t.y = 1.0f - t.y;

                        bool isOnButton = touchArea.Contains(t);

                        if (isOnButton)
                        {
                            if (touchSwallowDown)
                            {
                                rawTouch.owner = this;

                                Debug.Log("touchSwallowDown");
                            }
                        }
                    }
*/
                    //rawTouch.down
                    //if (            


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

            cont = false;

            // Pad button
            if (cont == false && padButton != PadButtons.DISABLED)
            {
                cont = Input.GetButton(inputMan.PadButtonNames[(int)padButton]);
                if (touchArea != null && cont)
                {
                    var tmp = touchArea.Centre;
                    pixelPos.x = tmp.x * (float)Screen.width;
                    pixelPos.y = tmp.y * (float)Screen.height;
                }
            }

            // Touch button
            if (touchEnabled && cont == false)
            {
                for (int i = 0; i < watching.Count; ++i)
                {
                    var touchID = watching[i];

                    // If touch is still alive and has no owner
                    if (touchID.Touch.owner == null || touchID.Touch.owner == this)
                    {
                        var rawTouch = touchID.Touch;
                        var t = new Vector2(rawTouch.normPos.x, 1.0f - rawTouch.normPos.y);

                        if (touchArea.Contains(t))
                        {
                            cont = true;
                            pixelPos = rawTouch.pixelPos;

                            rawTouch.isOnButton |= true;    // _mlr_tofix_ - is this still needed?
                        }

                    }
                }
            }

            // Keyboard button
            if (cont == false && keyEnabled)
            {
                cont = Input.GetKey(key.ToString());
            }

            down = cont & (cont ^ pre);
            up = (!cont) & (cont ^ pre);
            pre = cont;
            timeDown = cont ? timeDown + Time.deltaTime : 0.0f;

#if false
            
            // Touch button
            if (cont == false && touchEnabled)
            {
                for (int i = 0; i < rawTouchStream.numTouches_; ++i)
                {
                    var t = rawTouchStream.touches_[i].normPos;
                    t.y = 1.0f - t.y;

                    bool isOnButton = touchArea.Contains(t);

                    if (!cont)   // Don't overwrite
                    {
                        cont = isOnButton;
                        pixelPos = rawTouchStream.touches_[i].pixelPos;
                    }

                    rawTouchStream.touches_[i].isOnButton |= isOnButton;
                }
            }

            // Keyboard button
            if (cont == false && keyEnabled)
            {
                cont = Input.GetKey(key.ToString());
            }

            down = cont & (cont ^ pre);
            up = (!cont) & (cont ^ pre);
            pre = cont;
            timeDown = cont ? timeDown + Time.deltaTime : 0.0f;
#endif
        }


        //
        // UpdateDbg
        //
        public void UpdateDbg()
        {
            if (dbgOn)
            {
                inputMan.AddDebugStr("button - watching count: " + watching.Count);

                //inputMan.
            }
        }


        //
        // Constructor
        //
        public ButtonInternal(rcInputManager zInputMan, rcInputManager.RawTouchStream zRawTouchStream)
        {
            inputMan = zInputMan;
            rawTouchStream = zRawTouchStream;
        }

        public override void EnableTouch(rcMath.Rect zArea)
        {
            touchEnabled = true;
            touchArea = zArea;
        }

        public override void EnableTouch(rcMath.Rect zArea, Vector3 zWorldCenter, Camera zCam)
        {
            var ss = zCam.WorldToScreenPoint(zWorldCenter);

            ss.x /= Screen.width;
            ss.y /= Screen.height;

            var rect = new rcMath.Rect(zArea);
            rect.min.x += ss.x;
            rect.min.y += ss.y;
            rect.max.x += ss.x;
            rect.max.y += ss.y;

            touchEnabled = true;
            touchArea = rect;
        }

        public override void EnableTouch(Bounds zBounds, Camera zCam)
        {
            var corners = new Vector3[8];
            corners[0] = new Vector3(zBounds.min.x, zBounds.max.y, zBounds.min.z);
            corners[1] = new Vector3(zBounds.max.x, zBounds.max.y, zBounds.min.z);
            corners[2] = new Vector3(zBounds.max.x, zBounds.min.y, zBounds.min.z);
            corners[3] = new Vector3(zBounds.min.x, zBounds.min.y, zBounds.min.z);
            corners[4] = new Vector3(zBounds.min.x, zBounds.max.y, zBounds.max.z);
            corners[5] = new Vector3(zBounds.max.x, zBounds.max.y, zBounds.max.z);
            corners[6] = new Vector3(zBounds.max.x, zBounds.min.y, zBounds.max.z);
            corners[7] = new Vector3(zBounds.min.x, zBounds.min.y, zBounds.max.z);

            var rect = new rcMath.Rect();
            foreach (var p in corners)
            {
                var ss = zCam.WorldToScreenPoint(p);
                rect.ExpandBy(ss);
            }

            rect.min.x /= Screen.width;
            rect.min.y /= Screen.height;
            rect.max.x /= Screen.width;
            rect.max.y /= Screen.height;

            touchEnabled = true;
            touchArea = rect;
        }

        public override void EnableTouch()
        {
            if (touchArea != null)
                touchEnabled = true;
        }

        public override void DisableTouch()
        {
            touchEnabled = false;
        }

        public override void EnablePad(int zPadIndex, PadButtons zButton)
        {
            //padIndex = zPadIndex;
            padButton = zButton;
        }

        public override void EnableKey(char zKey)
        {
            keyEnabled = true;
            key = zKey;
        }

        public override rcMath.Rect GetTouchArea()
        {
            return touchArea;
        }

        public override bool GetIsTouchEnabled()
        {
            return touchEnabled;
        }

        rcInputManager inputMan;
        rcInputManager.RawTouchStream rawTouchStream;

        // Keyboard
        bool keyEnabled;
        char key;

        // Pad
        //int padIndex;
        PadButtons padButton;

        // Touch
        bool touchEnabled;
        rcMath.Rect touchArea;
//        bool touchSwallowDown;
    }
}


#if false
        //
        // Update
        //
        void Update(float zDT)
        {
            Debug.LogError("whatever");

            cont = false;

            // Pad button
            if (cont == false && padButton != PadButtons.DISABLED)
            {
                cont = Input.GetButton(inputMan.PadButtonNames[(int)padButton]);
                if (touchArea != null && cont)
                {
                    var tmp = touchArea.Centre;
                    pixelPos.x = tmp.x * (float)Screen.width;
                    pixelPos.y = tmp.y * (float)Screen.height;
                }
            }

            // Touch button
            if (cont == false && touchEnabled)
            {
                for (int i = 0; i < rawTouchStream.numTouches_; ++i)
                {
                    var t = rawTouchStream.touches_[i].normPos;
                    t.y = 1.0f - t.y;

                    bool isOnButton = touchArea.Contains(t);

                    if (!cont)   // Don't overwrite
                    {
                        cont = isOnButton;
                        pixelPos = rawTouchStream.touches_[i].pixelPos;
                    }

                    rawTouchStream.touches_[i].isOnButton |= isOnButton;
                }
            }

            // Keyboard button
            if (cont == false && keyEnabled)
            {
                cont = Input.GetKey(key.ToString());
            }

            down = cont & (cont ^ pre);
            up = (!cont) & (cont ^ pre);
            pre = cont;
            timeDown = cont ? timeDown + Time.deltaTime : 0.0f;
        }
#endif
