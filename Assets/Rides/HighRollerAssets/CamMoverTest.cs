using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMoverTest : AbstractGameEffects {

	public GameObject pivot;
	public float speed;

    public bool manual = false;
    public Transform wheel;

	// Use this for initialization
	void Start () {
        base.Start();
	}

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (!inSession) {
            wheel.GetComponent<Renderer>().enabled = false;
            return; }

        wheel.GetComponent<Renderer>().enabled = true;
        if (!manual)
        {
            speed = 1 + 100 * climaxRatio; //(100 * Mathf.Abs(Mathf.Sin(Time.time / 100)));
            pivot.transform.Translate(Vector3.forward * Time.deltaTime * speed);
            wheel.RotateAround(pivot.transform.position, Vector3.left, Time.deltaTime * speed);
        }

        else {

            if (Input.GetKey(KeyCode.UpArrow))
            {
                speed = speed + (1 * Time.deltaTime);
                pivot.transform.Translate(Vector3.forward * Time.deltaTime * speed);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                speed = speed + (1 * Time.deltaTime);
                pivot.transform.Translate(Vector3.back * Time.deltaTime * speed);
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                speed = 1;
                pivot.transform.Translate(Vector3.forward * Time.deltaTime * speed);
            }

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                speed = 1;
                pivot.transform.Translate(Vector3.back * Time.deltaTime * speed);
            }
        }
    }
}
