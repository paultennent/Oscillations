using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatScriptReader : MonoBehaviour {

    public TextAsset script;

    public List<BoatFrame> frames;

    private int lastFrame = 0;
    private float lastTime = 0;

    private BoatFrame testFrame;

	void Start () {
        readFrames();
	}

    public BoatFrame getFrame(float time)
    {
        //if the time has gobe backwards we must have reset - return lastFrame to 0
        if(time < lastTime)
        {
            Debug.Log("Resetting Frame Clock");
            lastFrame = 0;
        }
        
        //if we're already at the end just keep returning the last frame
        if(lastFrame >= frames.Count - 1)
        {
            return frames[frames.Count - 1];
        }
        else
        {
            //scan through (from the last frame) till we find one with a greater time, then return the previous one
            for(int i = lastFrame; i < frames.Count-1; i++)
            {
                if(frames[i+1].time > time)
                {
                    return frames[lastFrame];
                }
                else
                {
                    lastFrame++;
                }
            }
            return frames[lastFrame];
        }
    }

    private void readFrames()
    {
        frames = new List<BoatFrame>();
        int goodLines = 0;
        string[] lines = System.Text.RegularExpressions.Regex.Split(script.text, "\r\n|\r|\n");
        for(int i = 1; i < lines.Length; i++)
        {
            string[] parts = System.Text.RegularExpressions.Regex.Split(lines[i], ",");
            if(parts.Length == 26)
            {
                goodLines++;
                BoatFrame frame = parse(parts);
                if (frame != null) {
                    frames.Add(frame);
                }
            }
        }
        Debug.Log("Read " + goodLines + " good lines of script");
    }

    private BoatFrame parse(string[] s)
    {
        BoatFrame f = new BoatFrame();
        //do parsing here
        try
        {
            f.time = float.Parse(s[0]);
            f.SwingAngleMultiplier = float.Parse(s[1]);
            f.SwingYPos = new MaxMin(float.Parse(s[2]), float.Parse(s[3]));
            f.SwingZPos = new MaxMin(float.Parse(s[4]), float.Parse(s[5]));
            f.UpperExtension = new MaxMin(float.Parse(s[6]), float.Parse(s[7]));
            f.LowerExtension = new MaxMin(float.Parse(s[8]), float.Parse(s[9]));
            f.NorthWallRotation = new MaxMin(float.Parse(s[10]), float.Parse(s[11]));
            f.SouthWallRotation = new MaxMin(float.Parse(s[12]), float.Parse(s[13]));
            f.EastWallRotation = new MaxMin(float.Parse(s[14]), float.Parse(s[15]));
            f.WestWallRotation = new MaxMin(float.Parse(s[16]), float.Parse(s[17]));
            f.BuildingRotation = new Vector3MaxMin(new Vector3(float.Parse(s[18]), float.Parse(s[20]), float.Parse(s[22])), new Vector3(float.Parse(s[19]), float.Parse(s[21]), float.Parse(s[23])));
            f.LightIntensity = new MaxMin(float.Parse(s[24]), float.Parse(s[25]));
        }
        catch(System.Exception e)
        {
            Debug.Log("Failed to parse frame:" + e.Message);
            return null;
        }
        return f;
    }
}
