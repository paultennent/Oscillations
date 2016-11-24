using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingUtilities{

	public static float correctAngle(float swingAngle, float headsetAngle){
		return swingAngle - headsetAngle;
	}
}
