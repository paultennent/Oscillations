using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ResearchReplay : MonoBehaviour
{    
    public class FrameData
    {
        public float time;
        public float swingAngle;
        public float offsetTime;
        public float climaxRatio;
        public Vector3 camPos;
        public Quaternion camRotation;
        public Quaternion headLook;        
    };

    float startFileTime=0f;
    float currentFileTime=0f;
    bool playing=false;
    
    int currentFrame=0;
    string riderID;
    
    List<FrameData> mReplayItems=new List<FrameData>();

    private byte[] replayBytes=null;
    public string infoText=null;

    
    WWW infoGetter;
    WWW dataGetter;
    private bool loading=false;
    private bool loadFailed=false;
    
    private string gameScene="";
    public string baseURL="http://128.243.29.96/riderdata.php?riderid=%1&binarydata=%2";
    
    
    public bool Open(byte[] data)
    {
        
        Vector3 camPos;
        Quaternion camRotation;
        Quaternion headRotation;
        
        try{
            print(data.Length);
            BinaryReader frameFile = new BinaryReader(new MemoryStream(data,false));
            byte[] logHeader={79,83,67,73,76,79,71,49};
            byte[]fileSig=frameFile.ReadBytes(logHeader.Length);
            if(fileSig.Length==8)
            {
                for(int c=0;c<logHeader.Length;c++)
                {
                    if(logHeader[c]!=fileSig[c])
                    {
                        print("Bad frame file signature");
                        return false;
                    }
                }
            }

            while(true)
            {
                FrameData newFrame=new FrameData();
                newFrame.time=frameFile.ReadSingle();
                newFrame.swingAngle=frameFile.ReadSingle();
                newFrame.offsetTime=frameFile.ReadSingle();
                newFrame.offsetTime=frameFile.ReadSingle();
                newFrame.camPos.x=frameFile.ReadSingle();
                newFrame.camPos.y=frameFile.ReadSingle();
                newFrame.camPos.z=frameFile.ReadSingle();
                newFrame.camRotation.w=frameFile.ReadSingle();
                newFrame.camRotation.x=frameFile.ReadSingle();
                newFrame.camRotation.y=frameFile.ReadSingle();
                newFrame.camRotation.z=frameFile.ReadSingle();
                newFrame.headLook.w=frameFile.ReadSingle();
                newFrame.headLook.x=frameFile.ReadSingle();
                newFrame.headLook.y=frameFile.ReadSingle();
                newFrame.headLook.z=frameFile.ReadSingle();
                float checkVal=frameFile.ReadSingle();
                if(Mathf.Abs(checkVal-(-9999999f))>1f)
                {
                    print("Bad check num in file");
                }
                mReplayItems.Add(newFrame);
            }
        }catch(IOException e)
        {
            // end of file or something
        }
        if(mReplayItems.Count==0)
        {
            print("No replay items");
            // no data
            return false;
        }    
        startFileTime=mReplayItems[0].time;
        return true;
    }
    
    
    public void Start()
    {
    }
    
    public void Update()
    {
 
		if(loading  && infoGetter.isDone && dataGetter.isDone)
        {
            if(!string.IsNullOrEmpty(infoGetter.error) || !string.IsNullOrEmpty(dataGetter.error))
            {
                loadFailed=true;
            }else
            {
                infoText=infoGetter.text;
                string[] lines=infoText.Split('\n');
                foreach(string line in lines)
                {
                    string[] linesplit=line.Split(',');
                    if(linesplit[0]=="scene" && linesplit.Length==2)
                    {
                        gameScene=linesplit[1];
                    }
                }
                replayBytes=dataGetter.bytes;
                Open(replayBytes);
            }
            loading=false;
        }

        if(playing)
        {
            currentFileTime+=Time.deltaTime;
            while(currentFrame<mReplayItems.Count-1 && currentFileTime>mReplayItems[currentFrame+1].time)
            {
                currentFrame+=1;
            }
        }        
    }
    
    // here are the things you have to call from your replay code
    // to set rider id (and hence load data from server),
    // and play/stop playing etc.

   public void Play()
    {
        if(mReplayItems.Count>0)
        {
            playing=true;
            if(currentFrame<mReplayItems.Count)
            {
                currentFileTime=mReplayItems[currentFrame].time;;
            }else
            {
                currentFileTime=startFileTime;
            }
        }
    }
    
    public void Stop()
    {
        playing=false;        
    }
    
    public void Rewind()
    {
        currentFileTime=startFileTime;
        currentFrame=0;
    }
 
    
    public void LoadRiderID(string id)
    {
        this.riderID=id;        
        string infoURL=baseURL.Replace("%1",id).Replace("%2","0");
        string dataURL=baseURL.Replace("%1",id).Replace("%2","1");
        loadFailed=false;
        loading=true;
        infoGetter = new WWW(infoURL);
        dataGetter= new WWW(dataURL);                
    }

    
    public bool IsLoadFailed()
    {
        return loadFailed;
    }
    
    public bool IsLoading()
    {
        return loading;
    }
    
    public bool IsLoaded()
    {
        return (mReplayItems.Count>0);
    }
    
    public string GetSceneName()
    {
        return gameScene;
    }
 
    public FrameData GetCurrentData()
    {
        if(currentFrame>=0 && currentFrame<mReplayItems.Count)
        {
            return mReplayItems[currentFrame];
        }else
        {
            return null;
        }
    }    
    
}