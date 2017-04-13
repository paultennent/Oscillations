//#define DBGSHOW_Gesture

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class rcInputManager : rcMonoBehaviourManual
{
    //
    // GuestureInternal
    //
    class GestureInternal : Gesture
    {
        List<RawTouchStream.TouchID> m_Watching = new List<RawTouchStream.TouchID>();
        RawTouchStream.TouchID m_MyTouch;

        //
        // UpdatePhase1
        //
        public void UpdatePhase1(float zDT)
        {
            if (m_TouchEnabled)
            {
                for (int i = 0; i < m_RawTouchStream.numTouches_; ++i)
                {
                    var rawTouch = m_RawTouchStream.touches_[i];

                    if (rawTouch.state == RawTouchStream.Touch.eState.Down &&
                        rawTouch.owner == null &&
                        m_TouchArea.Contains(rawTouch.normPos))
                    {
                        // Install watch
                        m_Watching.Add(new RawTouchStream.TouchID(rawTouch));
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
            for (int i = m_Watching.Count - 1; i >= 0; i--)
            {
                var watch = m_Watching[i];
                if (!watch.isAlive)
                {
                    m_Watching.RemoveAt(i);
                    i -= 1;
                    m_InputMan.DebugLog("removing touch watch, as now dead");
                }
            }

            TouchActive = false;
            if (m_TouchEnabled)
            {
                if (m_MyTouch.isSet == false || (m_MyTouch.isSet && m_MyTouch.isAlive == false))
                {
                    for (int i = 0; i < m_Watching.Count; ++i)
                    {
                        var touchID = m_Watching[i];

                        // If touch is still alive and has no owner
                        //if (touchID.isAlive && touchID.touch.owner == null)
                        if (touchID.Touch.owner == null)
                        {
                            var rawTouch = touchID.Touch;

                            float pixelMag = (rawTouch.pixelPos - rawTouch.pixelPosOnDown).magnitude;

                            //Debug.Log("pixelMag: " + pixelMag);

                            if (pixelMag > 5.0f && GestureStarted == false)
                            {
                                // We'll take ownership
                                touchID.Touch.owner = this;
                                m_InputMan.DebugLog("Stick - taking owner ship of touch");

                                m_MyTouch = touchID;


                                GestureStarted = true;

//                                touchPosNormStart = rawTouch.normPos;
                            }
                        }
                    }
                }

                if (m_MyTouch.isSet && m_MyTouch.isAlive)
                {
                    //Debug.Log("touchActive");
                    TouchActive = true;

                     if (m_HistoryPointCount < m_HistoryPoints.Length)
                     {
                         var rawTouch = m_MyTouch.Touch;
                         m_HistoryPoints[m_HistoryPointCount].pixelPos = rawTouch.pixelPos;

                         if (WorldCamera != null)
                         {
                             float yHeight = 0.01f;
                             var worldPos = rcCameraUtils.GetInputWorldSpacePositionHorizontal(WorldCamera, new Vector3(rawTouch.pixelPos.x, rawTouch.pixelPos.y, 0.0f), yHeight);
                             m_HistoryPoints[m_HistoryPointCount].worldPos = worldPos;
                         }

                         m_HistoryPointCount++;
                     }

/*
                    var rawTouch = m_MyTouch.Touch;
                    var tmp = rawTouch.normPos - touchPosNormStart;
                    axisX = Mathf.Clamp(tmp.x, -1.0f, 1.0f);
                    axisY = Mathf.Clamp(tmp.y, -1.0f, 1.0f);
 */
                }
                else
                {
                    if (GestureStarted)
                        GestureEnded = true;
                }
            }
        }


        //
        // UpdateDbg
        //
        public void UpdateDbg()
        {
#if UNITY_EDITOR && DBGSHOW_Gesture
            if (GestureStarted)
            {
                // White segments
                for (int i = 0; i < m_HistoryPointCount - 1; i += 2)
                    m_Dbg.AddLine3D(m_HistoryPoints[i].worldPos, m_HistoryPoints[i + 1].worldPos, Color.white);

                // Red segments
                for (int i = 1; i < m_HistoryPointCount - 1; i += 2)
                    m_Dbg.AddLine3D(m_HistoryPoints[i].worldPos, m_HistoryPoints[i + 1].worldPos, Color.red);
            }
#endif
        }


        //
        // OnRenderObject
        //
        public void OnRenderObject()
        {
#if UNITY_EDITOR && DBGSHOW_Gesture
            m_Dbg.Draw();
#endif
        }


        //
        // Constructor
        //
        public GestureInternal(rcInputManager zInputMan, rcInputManager.RawTouchStream zRawTouchStream)
        {
            m_InputMan = zInputMan;
            m_RawTouchStream = zRawTouchStream;
        }

        // EnableTouch
        public override void EnableTouch(rcMath.Rect zArea)
        {
            m_TouchEnabled = true;
            m_TouchArea = zArea;
        }

        // DisableTouch
        public override void DisableTouch()
        {
            m_TouchEnabled = false;
        }

        // Reset
        public override void Reset()
        {
            GestureStarted = false;
            GestureEnded = false;
            m_HistoryPointCount = 0;
#if UNITY_EDITOR && DBGSHOW_Gesture
            m_Dbg.Clear();
#endif
        }

        // GetWorldPoints
        public override List<Vector3> GetWorldPoints()  // _mlr_todo_ make this more efficient
        {
            var list = new List<Vector3>(m_HistoryPointCount);
            for (int i = 0; i < m_HistoryPointCount; ++i)
                list.Add(m_HistoryPoints[i].worldPos);
            return list;
        }

        //
        // HistoryPoint
        //
        struct HistoryPoint
        {
            public Vector2 screenPosNormalized
            {
                get { return new Vector2(pixelPos.x / Screen.width, pixelPos.y / Screen.height); }
            }
            public Vector2 pixelPos;
            public Vector3 worldPos;
        }
#if UNITY_EDITOR && DBGSHOW_Gesture
        rcDebug m_Dbg = new rcDebug();
#endif
        rcInputManager m_InputMan;
        rcInputManager.RawTouchStream m_RawTouchStream;

        bool m_TouchEnabled;
        rcMath.Rect m_TouchArea;

        HistoryPoint[] m_HistoryPoints = new HistoryPoint[256];
        int m_HistoryPointCount;
    }
}