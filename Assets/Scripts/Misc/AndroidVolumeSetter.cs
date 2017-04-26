using UnityEngine;

public class AndroidVolumeSetter: MonoBehaviour 
{
    public float volumePercent=.7f;
    private float lastVolume=-1f;
    
    
    void Start () 
    {
        lastVolume=-1f;
    }

    void SetLevel()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        if(lastVolume!=volumePercent)
        {
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject audioService=context.Call<AndroidJavaObject>("getSystemService","audio");
            float topVal = (float)audioService.Call<int>("getStreamMaxVolume",3);// get music stream max volume
            int setVal=(int)(volumePercent*topVal);
            audioService.Call("setStreamVolume",3,setVal,0);
            lastVolume=volumePercent;
        }  
#endif        
    }
    
    void Update () 
    {
        SetLevel();
    }
}
