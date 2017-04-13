using System;
using UnityEngine;
using System.Threading;

using Newtonsoft.Json;
using stdInput;

public class rtStdInput : MonoBehaviour
{
    public rcInputManager inputManager = null;
    private static UnityEngine.Vector3 lastMousePos = UnityEngine.Vector3.zero;
    private static Vector3 lastGyroOffset = Vector3.zero;

    private volatile bool stopReading = false;
    private Thread _thread;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Ready to accept input from stdin");

        _thread = new Thread(loop);
        _thread.Start();
    }

    void loop() {
        String s;
        do
        {
            s = Console.ReadLine();
            if (s != null && inputManager != null)
            {
                Command data = JsonConvert.DeserializeObject<Command>(s);
                if (data.type == "key")
                {
                    Debug.Log("Key Pressed: "+data.character+", Code:"+data.keyCode);
                    inputManager.virtualKeyDown = true;
                    inputManager.virtualKeyCode = data.keyCode;
                }
                else if (data.type == "mouse")
                {
                    lastMousePos.Set(data.coords.x, data.coords.y, 0);
                    if (data.action == "up")
                    {
                        Debug.Log("Mouse Up: " + data.coords.x + "x" + data.coords.y);
                        inputManager.virtualMouseDown = false;
                        inputManager.virtualMouseUp = true;
                        inputManager.virtualMousePos = lastMousePos;
                    }
                    else if(data.action == "down")
                    {
                        Debug.Log("Mouse Drag: " + data.coords.x + "x" + data.coords.y);
                        inputManager.virtualMouseDown = true;
                        inputManager.virtualMouseUp = false;
                        inputManager.virtualMousePos = lastMousePos;
                    }
                }
                else if (data.type == "zoom")
                {
                    if (data.action == "down")
                    {
                        inputManager.virtualMouseWheel = data.value;
                        inputManager.virtualMouseWheelActive = true;
                    }
                    else
                    {
                        inputManager.virtualMouseWheelActive = false;
                    }
                    Debug.Log("Zoom By: " + data.value);
                }
                else if (data.type == "gyro")
                {
                    lastGyroOffset.Set((float)data.coords.x / 25.0f, (float)data.coords.y / 25.0f, (float)data.coords.z / 25.0f);
                    inputManager.virtualGyro = lastGyroOffset;

                    //Debug.Log(rtStdInput.inputManager.virtualGyro.x + " " + rtStdInput.inputManager.virtualGyro.y + " " + data.coords.x + " " + data.coords.y);
                }
            }

            if (stopReading)
                break;

        } while (true);
    }

    public void Stop()
    {
        stopReading = true;
        _thread.Interrupt();
    }

    void OnDestroy ()
    {
        Stop();
        Debug.Log("Std Input now disabled");
    }
}
