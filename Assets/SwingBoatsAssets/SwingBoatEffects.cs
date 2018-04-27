using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingBoatEffects : MonoBehaviour {

    private BoatScriptReader bsr;
    private bool getNewBoatFrame = true;
    private BoatFrame bf;
    private float lastAngle = 0f;

    public Building building;
    public Transform pivot;

    public Transform camera;
    public Transform forePositioner;
    public Transform aftPositioner;
    public bool isFore = false;

    public float lightIntensityMultiplier = 0.1f;

    public float maxAssumedAngle = 45f;

    void Start()
    {
        bsr = GetComponent<BoatScriptReader>();
        
    }
    
    void Update(){
        if (isFore)
        {
            camera.position = forePositioner.position;
            camera.rotation = forePositioner.rotation;
            camera.parent = forePositioner;
        }else{
           camera.position = aftPositioner.position;
            camera.rotation = aftPositioner.rotation;
            camera.parent = aftPositioner; 
        }
    }

	public void applyEffects(float angle, float time)
    {

        //handle zero crossings to decide if we want a new frame
        if ((lastAngle <= 0 && angle >= 0) || (lastAngle >= 0 && angle <= 0))
        {
            getNewBoatFrame = true;
        }
        lastAngle = angle;

        //get a frame if we're ready to (we'll get a new one each zero crosing to prevent odd effects
        if (getNewBoatFrame)
        {
            bf = bsr.getFrame(time);
            getNewBoatFrame = false;
        }

        //set the swing angle
        float modifiedAngle = angle * bf.SwingAngleMultiplier;
        pivot.localEulerAngles = new Vector3(modifiedAngle, 0f, 0f);

        float ratio = (angle / maxAssumedAngle);
        ratio = Mathf.Clamp(ratio, -1, 1);
        float absRatio = Mathf.Abs(ratio);

        //set the y and z movement
        pivot.localPosition = new Vector3(0f, absRatio.RemapQuick(bf.SwingYPos.min, bf.SwingYPos.max), absRatio.RemapQuick(bf.SwingZPos.min, bf.SwingZPos.max));

        //set the upper and lower stretch
        building.SetExtendUp(absRatio.RemapQuick(bf.UpperExtension.min, bf.UpperExtension.max));
        building.SetExtendDown(absRatio.RemapQuick(bf.LowerExtension.min, bf.LowerExtension.max));

        //set the building rotation
        building.setBuildingRotation(new Vector3(absRatio.RemapQuick(bf.BuildingRotation.min.x, bf.BuildingRotation.max.x), absRatio.RemapQuick(bf.BuildingRotation.min.y, bf.BuildingRotation.max.y), absRatio.RemapQuick(bf.BuildingRotation.min.z, bf.BuildingRotation.max.z)));

        //set the wall rotations
        building.SetWallRotations(absRatio.RemapQuick(bf.NorthWallRotation.min,bf.NorthWallRotation.max), absRatio.RemapQuick(bf.SouthWallRotation.min, bf.SouthWallRotation.max), absRatio.RemapQuick(bf.EastWallRotation.min, bf.EastWallRotation.max), absRatio.RemapQuick(bf.WestWallRotation.min, bf.WestWallRotation.max));

        //set the light intesity
        building.SetLightIntensity(absRatio.RemapQuick(bf.LightIntensity.min, bf.LightIntensity.max) * lightIntensityMultiplier);


    }

}

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float RemapQuick(this float value, float from2, float to2)
    {
        if(from2 == 0 && to2 == 0)
        {
            return 0;
        }
        return (value) / (1f) * (to2 - from2) + from2;
    }

}
