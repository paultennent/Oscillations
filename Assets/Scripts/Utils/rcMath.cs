using UnityEngine;
using System.Collections;

public class rcMath
{
    // AngleDir
    static public int AngleDir(Vector3 zFrom, Vector3 zTo, Vector3 zUp)
    {
        Vector3 perp = Vector3.Cross(zFrom, zTo);
        float dir = Vector3.Dot(perp, zUp);
        if (dir >= 0.0f)
        {
            return 1;
        }
        else
        {
            //if (dir < 0.0f)
                return -1;
            //else
                //return 0;
        }
    }

    static public int AngleDir(float zFrom, float zTo)
    {
        float fromRads = zFrom * Mathf.Deg2Rad;
        float toRads = zTo * Mathf.Deg2Rad;

        var from = new Vector3(Mathf.Sin(fromRads), 0.0f, Mathf.Cos(fromRads));
        var to = new Vector3(Mathf.Sin(toRads), 0.0f, Mathf.Cos(toRads));
        return AngleDir( from, to, Vector3.up );
    }

    // ContinuousAngle
    static public float ContinuousAngle(Vector3 zFrom, Vector3 zTo, Vector3 zUp)
    {
        float angle = Vector3.Angle(zFrom, zTo);
        if (AngleDir(zFrom, zTo, zUp) == -1)
            return 360.0f - angle;
        else
            return angle;
    }

    // -180 to 180
    static public float RelativeAngle(Vector3 zFrom, Vector3 zTo, Vector3 zUp)
    {
        float angle = Vector3.Angle(zFrom, zTo);
        if (AngleDir(zFrom, zTo, zUp) == -1)
            return -angle;
        else
            return angle;
    }

    // Angle
    static public float Angle(Vector3 zFrom, Vector3 zTo, Vector3 zUp)
    {
        return Vector3.Angle(zFrom, zTo);
    }




    //
    // Line
    //
    public struct Line
    {
        public Line(Vector2 zStart, Vector2 zEnd)
        {
            start = zStart;
            end = zEnd;
        }
        public Vector2 start;
        public Vector2 end;
    }


    //
    // Rect
    //
    public class Rect
    {
        public Vector2 min, max;
        public Vector2 size { get { return new Vector2(max.x - min.x, max.y - min.y); } }

        public Rect()
        {
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);
        }

        public Rect(Rect zRect)
        {
            min = zRect.min;
            max = zRect.max;
        }

        public Rect(Vector2 zMin, Vector2 zMax)
        {
            min = zMin;
            max = zMax;
        }

        public Vector2 Centre { get { return new Vector2(min.x + (max.x - min.x) * 0.5f, min.y + (max.y - min.y) * 0.5f); } }

        public void ExpandBy(Vector2 zPoint)
        {
            min = Vector2.Min(zPoint, min);
            max = Vector2.Max(zPoint, max);
        }

        public void ExpandBy(Rect zRect)
        {
            min = Vector2.Min(zRect.min, min);
            max = Vector2.Max(zRect.max, max);
        }

        public void ClipBy(Rect zRect)
        {
            min = Vector2.Max(zRect.min, min);
            max = Vector2.Min(zRect.max, max);
        }

        public bool Contains(Vector2 zPoint)
        {
            if (zPoint.x >= min.x && zPoint.x < max.x &&
                zPoint.y >= min.y && zPoint.y < max.y)
                return true;
            return false;
        }

        public bool OverlapsWith(Rect zRect)
        {
#if false
            return  (Mathf.Abs(min.x - zRect.min.x) < (size.x + zRect.size.x) * 0.5f) &&
                    (Mathf.Abs(min.y - zRect.min.y) < (size.y + zRect.size.y) * 0.5f);
#else
            return !(   min.x > zRect.max.x ||
                        max.x < zRect.min.x ||
                        min.y > zRect.max.y ||
                        max.y < zRect.min.y);
#endif
        }
    }


    // LineIntersect2D
    public static bool LineIntersect2D(Line zLine1, Line zLine2, out Vector2 zInsectPoint)
    {
        zInsectPoint = new Vector2();

        // Calculate differences
        float xD1 = zLine1.end.x - zLine1.start.x;      // xD1 = p2.x - p1.x;
        float xD2 = zLine2.end.x - zLine2.start.x;      // xD2 = p4.x - p3.x;
        float yD1 = zLine1.end.y - zLine1.start.y;      // yD1 = p2.y - p1.y;
        float yD2 = zLine2.end.y - zLine2.start.y;      // yD2 = p4.y - p3.y;
        float xD3 = zLine1.start.x - zLine2.start.x;    // xD3 = p1.x - p3.x;
        float yD3 = zLine1.start.y - zLine2.start.y;    // yD3 = p1.y - p3.y;  

        // Calculate the lengths of the two lines
        float len1 = Mathf.Sqrt(xD1 * xD1 + yD1 * yD1);
        float len2 = Mathf.Sqrt(xD2 * xD2 + yD2 * yD2);

        // Calculate angle between the two lines.
        float dot = (xD1 * xD2 + yD1 * yD2); // dot product
        float deg = dot / (len1 * len2);

        // If abs(angle)==1 then the lines are parallell, so no intersection is possible  
        if (Mathf.Abs(deg) == 1.0f)
            return false;

        // Find intersection Pt between two lines  
        float div = yD2 * xD1 - xD2 * yD1;
        float ua = (xD2 * yD3 - yD2 * xD3) / div;
        //        float ub = (xD1 * yD3 - yD1 * xD3) / div;
        zInsectPoint.x = zLine1.start.x + ua * xD1;
        zInsectPoint.y = zLine1.start.y + ua * yD1;

        // Calculate the combined length of the two segments between Pt-p1 and Pt-p2
        xD1 = zInsectPoint.x - zLine1.start.x;
        xD2 = zInsectPoint.x - zLine1.end.x;
        yD1 = zInsectPoint.y - zLine1.start.y;
        yD2 = zInsectPoint.y - zLine1.end.y;
        float segmentLen1 = Mathf.Sqrt(xD1 * xD1 + yD1 * yD1) + Mathf.Sqrt(xD2 * xD2 + yD2 * yD2);

        // Calculate the combined length of the two segments between Pt-p3 and Pt-p4
        xD1 = zInsectPoint.x - zLine2.start.x;
        xD2 = zInsectPoint.x - zLine2.end.x;
        yD1 = zInsectPoint.y - zLine2.start.y;
        yD2 = zInsectPoint.y - zLine2.end.y;
        float segmentLen2 = Mathf.Sqrt(xD1 * xD1 + yD1 * yD1) + Mathf.Sqrt(xD2 * xD2 + yD2 * yD2);

        // If the lengths of both sets of segments are the same as  
        // the lenghts of the two lines the point is actually  
        // on the line segment.  

        // If the point isn't on the line, return null  
        if (Mathf.Abs(len1 - segmentLen1) > 0.01f || Mathf.Abs(len2 - segmentLen2) > 0.01f)
            return false;

        return true;
    }


    //
    // ClosestPointOnLine2D
    //
    public static bool ClosestPointOnLine2D(Line zLine, Vector2 zPoint, out Vector2 zClosestPoint)
    {
        // a = vector from test to start of line
        Vector2 a = zPoint - zLine.start;
        float la = a.magnitude;

        // b = vector of line
        Vector2 b = zLine.end - zLine.start;
        float lb = b.magnitude;

        // distance along line to point closest to test point
        float h = (a.x*b.x + a.y*b.y) / lb;

	    // distance from this point to test point
        float d = Mathf.Sqrt( la*la - h*h );

        // closer and within the line segment
        if (d < float.MaxValue && h > 0.0f && h < lb)
        {
            // Calc actual nearest point
            zClosestPoint.x = zLine.start.x + (b.x / lb) * h;	// P1 + normalised vector P1P2 scaled by dist to pt along P1P2
            zClosestPoint.y = zLine.start.y + (b.y / lb) * h;
            return true;
        }

        zClosestPoint = new Vector2();
        return false;
    }


    //
    // ClosestPointOnLine2D_NoLimit
    //
    public static void ClosestPointOnLine2D_NoLimit(Line zLine, Vector2 zPoint, out Vector2 zClosestPoint)
    {
        // a = vector from test to start of line
        Vector2 a = zPoint - zLine.start;
        //float la = a.magnitude;

        // b = vector of line
        Vector2 b = zLine.end - zLine.start;
        float lb = b.magnitude;

        // distance along line to point closest to test point
        float h = (a.x * b.x + a.y * b.y) / lb;

        // distance from this point to test point
        //float d = Mathf.Sqrt(la * la - h * h);

        // closer and within the line segment
        //if (d < float.MaxValue && h > 0.0f && h < lb)
        //if (d < float.MaxValue)
        {
            // Calc actual nearest point
            zClosestPoint.x = zLine.start.x + (b.x / lb) * h;	// P1 + normalised vector P1P2 scaled by dist to pt along P1P2
            zClosestPoint.y = zLine.start.y + (b.y / lb) * h;
        }
    }


    //
    // LineToCircleIntersect
    //
    public static bool LineToCircleIntersect( Line zLine, Vector2 zCirclePos, float zCircleRadius, out Vector2 zResultPos, out Vector2 zResultNormal)
    {
        zResultPos = Vector2.zero;
        zResultNormal = Vector2.zero;

        Vector2 a = zLine.start;
        Vector2 b = zLine.end;
        Vector2 center = zCirclePos;
        float r = zCircleRadius;

	    // offset the line to be relative to the circle
	    a -= center;
	    b -= center;
	
	    float qa = Vector2.Dot(a, a) - 2.0f*Vector2.Dot(a, b) + Vector2.Dot(b, b);
	    float qb = -2.0f*Vector2.Dot(a, a) + 2.0f*Vector2.Dot(a, b);
	    float qc = Vector2.Dot(a, a) - r*r;

	    float det = qb*qb - 4.0f*qa*qc;
	
	    // if the determinant is negative, the line through points a and b doesn't hit the circle
	    if(det >= 0.0f)
        {
		    // compute the nearest of the two 't' values
		    float t = (-qb - Mathf.Sqrt(det))/(2.0f*qa);

		    // change these conditions here if you want a line or ray instead of a line-segment
		    if (0.0f<= t && t <= 1.0f)
            {
			    // Woo, the line segment collides with the circle
			    // Now some other useful info:
			    zResultPos = center + Vector2.Lerp(a, b, t);
			    zResultNormal = Vector2.Lerp(a, b, t).normalized;
			    //float distance = (b - a).magnitude * t;
                return true;
		    }
        }

        return false;
    }


    //
    // MatrixProjectOnPlane
    //
    // Set zDir.w to 0 if you want a parellel projection
    //
    public static void MatrixProjectOnPlane(Plane zPlane, Vector4 zDir, out Matrix4x4 zResult)
    {
        float dot = Vector4.Dot(new Vector4(zPlane.normal.x, zPlane.normal.y, zPlane.normal.z, zPlane.distance), zDir);

        zResult.m00 = zPlane.normal.x * zDir.x - dot;
        zResult.m01 = zPlane.normal.y * zDir.x;
        zResult.m02 = zPlane.normal.z * zDir.x;
        zResult.m03 = zPlane.distance * zDir.x;

        zResult.m10 = zPlane.normal.x * zDir.y;
        zResult.m11 = zPlane.normal.y * zDir.y - dot;
        zResult.m12 = zPlane.normal.z * zDir.y;
        zResult.m13 = zPlane.distance * zDir.y;

        zResult.m20 = zPlane.normal.x * zDir.z;
        zResult.m21 = zPlane.normal.y * zDir.z;
        zResult.m22 = zPlane.normal.z * zDir.z - dot;
        zResult.m23 = zPlane.distance * zDir.z;

        zResult.m30 = zPlane.normal.x * zDir.w;
        zResult.m31 = zPlane.normal.y * zDir.w;
        zResult.m32 = zPlane.normal.z * zDir.w;
        zResult.m33 = zPlane.distance * zDir.w - dot;
    }


    //
    // PerspectiveOffCenter
    //
    public static Matrix4x4 PerspectiveOffCenter(float zLeft, float zRight, float zBottom, float zTop, float zNear, float zFar)
    {
        float x = (2.0f * zNear) / (zRight - zLeft);
        float y = (2.0f * zNear) / (zTop - zBottom);
        float a = (zRight + zLeft) / (zRight - zLeft);
        float b = (zTop + zBottom) / (zTop - zBottom);
        float c = -(zFar + zNear) / (zFar - zNear);
        float d = -(2.0f * zFar * zNear) / (zFar - zNear);
        float e = -1.0f;

        Matrix4x4 m = Matrix4x4.identity;
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;
        return m;
    }


    //
    // CalcFrustum
    //
    public static void CalcFrustum(float zFovRadians, float zAspectRatio, float zNear, float zFar, ref Vector3[] oFrustumPoints)
    {
        float tanFov = Mathf.Tan(zFovRadians / 2.0f);
        float xHither = tanFov * zNear * zAspectRatio;
        float yHither = tanFov * zNear;
        float xYon = tanFov * zFar * zAspectRatio;
        float yYon = tanFov * zFar;

        oFrustumPoints[0] = new Vector3(-xHither, yHither, zNear);		// Top left near
        oFrustumPoints[1] = new Vector3(xHither, yHither, zNear);		// Top right near
        oFrustumPoints[2] = new Vector3(xHither, -yHither, zNear);		// Bottom right near
        oFrustumPoints[3] = new Vector3(-xHither, -yHither, zNear);		// Bottom left near
        oFrustumPoints[4] = new Vector3(-xYon, yYon, zFar);		        // Top left far
        oFrustumPoints[5] = new Vector3(xYon, yYon, zFar);			    // Top right far
        oFrustumPoints[6] = new Vector3(xYon, -yYon, zFar);			    // Bottom right far
        oFrustumPoints[7] = new Vector3(-xYon, -yYon, zFar);			// Bottom left far
    }


    public static float DistanceToLine(Ray ray, Vector3 point) 
    { 
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude; 
    }

    //Wrap a number to keep it within the bounds
    public static float Wrap(float value, float minInclusive, float maxExclusive)
    {
        //rcDebug.Assert(min < max);

        float diff = maxExclusive - minInclusive;
        while (value >= maxExclusive)
            value -= diff;
        while (value < minInclusive)
            value += diff;

        return value;
    }

    public static int Wrap(int value, int minInclusive, int maxExclusive)
    {
        int diff = maxExclusive - minInclusive;
        while (value >= maxExclusive)
            value -= diff;
        while (value < minInclusive)
            value += diff;

        return value;
    }

    // Smoother Lerps
    public static float LerpSin(float from, float to, float amt)
    {
        float adjustedAmt = Mathf.PI * amt - Mathf.PI / 2.0f;
        float p = (Mathf.Sin(adjustedAmt) + 1.0f) / 2.0f;

        return (p * (to - from)) + from;
    }

    static public bool CheckColl2DHorzSq(Vector3 zCheckPos, Vector3 zHitPos, float zHitRadiusSq)
    {
        Vector3 offsetFromHitPos = zCheckPos - zHitPos;
        offsetFromHitPos.y = 0.0f;
        float distFromHitPosSq = offsetFromHitPos.sqrMagnitude;

        if (distFromHitPosSq < zHitRadiusSq)
            return true;
        return false;
    }


    //
    // CurveBezier
    //
    public struct CurveBezier
    {
        public Vector3 p0, p1, p2, p3;
        public bool set;

        public void Set(Vector3 zStartPos, Vector3 zStartTangent, Vector3 zEndPos, Vector3 zEndTangent, float zL0, float zL1)
        {
            p0 = zStartPos;
            p1 = zStartPos + zStartTangent * zL0;
            p2 = zEndPos + zEndTangent * zL1;
            p3 = zEndPos;
            set = true;
        }

        public void GetAtT(float t, out Vector3 oPos, out Vector3 oTan)
        {
            float t2 = t * t;
            float t3 = t * t * t;
            float bp1 = -1.0f * t3 + 3.0f * t2 - 3.0f * t + 1.0f;
            float bp2 = 3.0f * t3 - 6.0f * t2 + 3.0f * t + 0.0f;
            float bp3 = -3.0f * t3 + 3.0f * t2 + 0.0f * t + 0.0f;
            float bp4 = 1.0f * t3 - 0.0f * t2 + 0.0f * t + 0.0f;
            float bt1 = -3.0f * t2 + 6.0f * t - 3.0f;
            float bt2 = 9.0f * t2 - 12.0f * t + 3.0f;
            float bt3 = -9.0f * t2 + 6.0f * t + 0.0f;
            float bt4 = 3.0f * t2 - 0.0f * t + 0.0f;
            oPos = new Vector3(bp1 * p0.x + bp2 * p1.x + bp3 * p2.x + bp4 * p3.x, bp1 * p0.y + bp2 * p1.y + bp3 * p2.y + bp4 * p3.y, bp1 * p0.z + bp2 * p1.z + bp3 * p2.z + bp4 * p3.z);
            oTan = new Vector3(bt1 * p0.x + bt2 * p1.x + bt3 * p2.x + bt4 * p3.x, bt1 * p0.y + bt2 * p1.y + bt3 * p2.y + bt4 * p3.y, bt1 * p0.z + bt2 * p1.z + bt3 * p2.z + bt4 * p3.z).normalized;
        }
    }
}
