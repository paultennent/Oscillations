using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyfishTileCamMover : AbstractGameEffects
{

    private float curHeight = 0f;
    private float yVelocity = 0f;

    public float velocityDragConstant = .00003f;
    public float upforceConstant = .001f;
    public float gravityConstant = 9.8f;

    public float fadeTime = 5f;

    private Vector3 initialPosition;

    public GameObject pivot;

    public bool launch = false;

    public bool infiniteFall = true;

    private Skirt skirtObj;

    private bool launched = false;
    private float introTime = 5f;

    public float seatDrop = 1.5f;

    float minCurheight = 0f;

    public BubbleScript startBubble;

    public bool SwapAngle = true;

    private bool fadedIn = false;

    // Use this for initialization
    void Start()
    {
        base.Start();
        initialPosition = pivot.transform.position;
        minCurheight = pivot.transform.position.y;
        FadeSphereScript.doFadeIn(fadeTime, Color.black);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (!fadedIn)
        {
            FadeSphereScript.doFadeIn(5f, Color.black);
            fadedIn = true;
        }

        //doDifferentRotation - I think this wants removing...
        if (SwapAngle)
        {
            float ang = -swingAngle + shopping["angle_as_offset_from_perpendicular_to_CofG_direction"];
            viewPoint.localEulerAngles = new Vector3(ang, 0, 0);
        }

        if (sessionTime < introTime || !launched)
        {
            // intro period - slowly reduce the amount of up and down movement
            // and reduce it to just fwd/back movement
            //need to change this to up/down

            Vector3 topPoint = initialPosition + Vector3.up * seatDrop;
            Quaternion rotation = Quaternion.Euler(-swingAngle, 0, 0);
            Vector3 rotationOffset = rotation * Vector3.up * -seatDrop;
            Vector3 seatPoint = topPoint + rotationOffset;
            Vector3 onlyFwdBackPoint = new Vector3(seatPoint.x, topPoint.y, seatPoint.z);

            pivot.transform.position = Vector3.Lerp(seatPoint, onlyFwdBackPoint, offsetTime / 10f);

            if (offsetTime > introTime && swingQuadrant == 0)
            {
                launched = true;
                startBubble.trigger = true;
            }
        }

        else
        {

            float upforce = calculateUpforce();

            yVelocity += upforce * Time.deltaTime;
            curHeight = curHeight + yVelocity * Time.deltaTime;
            if (curHeight < 0 & !infiniteFall)
            {
                curHeight = minCurheight;
                yVelocity = 0;
            }

            float zVal = 0f;
            LayerLayout.LayoutPos curTilePos = LayerLayout.GetLayerLayout().GetBlockAt(pivot.transform.position.y);
            if (curTilePos > LayerLayout.LayoutPos.CAVE_TOP)
            {
                zVal = (swingAngle / 2f) * (0.5f-climaxRatio);
            }

            pivot.transform.position = initialPosition + new Vector3(0, curHeight, zVal);

            //print (sessionTime + ":" + sessionLength);
            if (sessionTime > (sessionLength - fadeTime))
            {
                //print ("Should be fading");
                if (!FadeSphereScript.isFading())
                {
					FadeSphereScript.doFadeOut(fadeTime, Color.black);
                }
            }

        }
    }

    float calculateUpforce()
    {
        float totalForce = swingAngVel * swingAngVel * upforceConstant;
        if (launch == true)
        {
            totalForce = 20f;
        }

        totalForce -= gravityConstant;
        if (yVelocity > 0)
        {
            totalForce -= (yVelocity * yVelocity) * velocityDragConstant;
        }
        else
        {
            totalForce += (yVelocity * yVelocity) * velocityDragConstant;
        }
        return totalForce;
    }
}
