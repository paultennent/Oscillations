using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

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
    
    List<FrameData> mReplayItems=new List<FrameData>();

    
	public bool Open(string basename, bool isString, byte[] byteData)
    {
        
        Vector3 camPos;
        Quaternion camRotation;
        Quaternion headRotation;
        
        try{
			BinaryReader frameFile;
			if(!isString){
				frameFile = new BinaryReader(File.Open(basename+".bin", FileMode.Open));
			}else{
				frameFile = new BinaryReader(new MemoryStream(byteData));
			}
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
    
    public void Play()
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
    
    public void Stop()
    {
        playing=false;        
    }
    
    public void Rewind()
    {
        currentFileTime=startFileTime;
        currentFrame=0;
    }
    
    public void Start()
    {
        // load test data file
        // hit play
    }
    
    public void Update()
    {
        if(playing)
        {
            currentFileTime+=Time.deltaTime;
            while(currentFrame<mReplayItems.Count-1 && currentFileTime>mReplayItems[currentFrame+1].time)
            {
                currentFrame+=1;
            }
        }        
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

	public static MemoryStream GenerateStreamFromString(string value)
	{
		return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
	}
    
}