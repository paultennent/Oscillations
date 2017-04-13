using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FlatPlacement : MonoBehaviour 
{

    public GameObject targetObject;
    public ExternalFace externalFace;

    public Vector2 facePos;

    public enum ExternalFace
    {
        xPos,
        xNeg,
        zPos,
        zNeg
    }

    
	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        //facePos.x = Mathf.Clamp01(facePos.x);
        //facePos.y = Mathf.Clamp01(facePos.y);

        if (facePos.x < -0.5f)
            facePos.x = -0.5f;
        else if (facePos.x > 0.5f)
            facePos.x = 0.5f;
        
        if (facePos.y < -0.5f)
            facePos.y = -0.5f;
        else if (facePos.y > 0.5f)
            facePos.y = 0.5f;

        transform.position = GetPosition();
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetPosition(), 1);
    }

    Vector3 GetPosition()
    {
        //Vector3 pos = targetObject.transform.position;
        Vector3 pos = Vector3.zero; // targetObject.transform.localPosition;
        //var scale = Vector3.one; // targetObject.transform.localScale;

        /*
        if (externalFace == ExternalFace.xPos)
        {
            pos.x += 0.5f * scale.x;

            pos.y -= 0.5f * scale.y;
            pos.z -= 0.5f * scale.z;

            pos.y += facePos.y * scale.y;
            pos.z += facePos.x * scale.z;

            //transform.forward = new Vector3(-1, 0, 0);
        }
        else if (externalFace == ExternalFace.xNeg)
        {
            pos.x -= 0.5f * scale.x;

            pos.y -= 0.5f * scale.y;
            pos.z += 0.5f * scale.z;

            pos.y += facePos.y * scale.y;
            pos.z -= facePos.x * scale.z;

           // transform.forward = new Vector3(1, 0, 0);
        }
        else if (externalFace == ExternalFace.zPos)
        {
            pos.z += 0.5f * scale.z;

            pos.y -= 0.5f * scale.y;
            pos.x += 0.5f * scale.x;

            pos.y += facePos.y * scale.y;
            pos.x -= facePos.x * scale.x;

            //var angle = Vector3.Angle(targetObject.transform.forward, transform.forward);
            //transform.eulerAngles = new Vector3(0.0f, angle, 0.0f);
            
           // transform.forward = new Vector3(0, 0, -1);
        }
        else if (externalFace == ExternalFace.zNeg)
        {
            pos.z -= 0.5f * scale.z;

            pos.y -= 0.5f * scale.y;
            pos.x -= 0.5f * scale.x;

            pos.y += facePos.y * scale.y;
            pos.x += facePos.x * scale.x;

            //transform.forward = new Vector3(0, 0, 1);
        }
        */

        var forward = Vector3.zero;
        if (externalFace == ExternalFace.xPos)
        {
            pos.x += 0.5f;

            pos.y += facePos.y;
            pos.z += facePos.x;

            forward = new Vector3(-1, 0, 0);
        }
        else if (externalFace == ExternalFace.xNeg)
        {
            pos.x -= 0.5f;

            pos.y += facePos.y;
            pos.z += -facePos.x;

            forward = new Vector3(1, 0, 0);
        }
        else if (externalFace == ExternalFace.zPos)
        {
            pos.z += 0.5f;

            pos.y += facePos.y;
            pos.x += facePos.x;

            forward = new Vector3(0, 0, -1);
        }
        else if (externalFace == ExternalFace.zNeg)
        {
            pos.z -= 0.5f;

            pos.y += facePos.y;
            pos.x += -facePos.x;

            forward = new Vector3(0, 0, 1);
        }

        // convert from local space
        pos = targetObject.transform.TransformPoint(pos);

        transform.forward = targetObject.transform.TransformDirection(forward);

        return pos;
    }
}
