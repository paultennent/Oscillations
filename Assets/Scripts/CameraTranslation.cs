using UnityEngine;
using System.Collections;

public class CameraTranslation : MonoBehaviour
{

    public float SmoothTime = 1.0f;

    public Vector3 TranslationTarget { get; set; }

    private Vector3 targetPosVelocity;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (TranslationTarget != transform.position)
        {
            // Interpolate towards target
            transform.position = Vector3.SmoothDamp(transform.position, TranslationTarget, ref targetPosVelocity, SmoothTime);
        }
    }


}
