using UnityEngine;

public class BoatFrame
{
    public float time;
    public float SwingAngleMultiplier;
    public MaxMin SwingYPos;
    public MaxMin SwingZPos;
    public MaxMin UpperExtension;
    public MaxMin LowerExtension;
    public MaxMin NorthWallRotation;
    public MaxMin SouthWallRotation;
    public MaxMin EastWallRotation;
    public MaxMin WestWallRotation;
    public Vector3MaxMin BuildingRotation;
    public MaxMin LightIntensity;

    public override string ToString()
    {
        string s = "Boat Frame:";
        s += "Time: ";
        s += time;
        s += "\n";
        s += "SwingAngleMultiplier: ";
        s += SwingAngleMultiplier;
        s += "\n";
        s += "SwingYPos: ";
        s += SwingYPos;
        s += "\n";
        s += "SwingZPos: ";
        s += SwingZPos;
        s += "\n";
        s += "UpperExtension: ";
        s += UpperExtension;
        s += "\n";
        s += "LowerExtension: ";
        s += LowerExtension;
        s += "\n";
        s += "NorthWallRotation: ";
        s += NorthWallRotation;
        s += "\n";
        s += "SouthWallRotation: ";
        s += SouthWallRotation;
        s += "\n";
        s += "EastWallRotation: ";
        s += EastWallRotation;
        s += "\n";
        s += "WestWallRotation: ";
        s += WestWallRotation;
        s += "\n";
        s += "BuildingRotation: ";
        s += BuildingRotation;
        s += "\n";
        s += "LightIntensity: ";
        s += LightIntensity;
        return s;
    }
}

public class MaxMin
{
    public float max;
    public float min;
    public MaxMin(float max, float min)
    {
        this.max = max;
        this.min = min;
    }

    public override string ToString()
    {
        return "Max: " + max + " , Min: " + min;
    }
}

public class Vector3MaxMin
{
    public Vector3 max;
    public Vector3 min;
    public Vector3MaxMin(Vector3 max, Vector3 min)
    {
        this.max = max;
        this.min = min;
    }

    public override string ToString()
    {
        return "Max: " + max + " , Min: " + min;
    }
}
