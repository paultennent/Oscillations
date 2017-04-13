using UnityEngine;
using System.Collections;

namespace rcCore
{
    public class rcGL
    {
        static private Material materialSolidColor;
        static private Material materialSolidColorZTestOff;
        static private Material materialTransparentColor;

        static int curDrawMode = -1;
        static bool cur2D;
        static Vector2 invScale;

        static Color glColor = UnityEngine.Color.black;

        // Begin
        static public void Begin(bool z2D = false, bool zInPixels = false)
        {
            if (z2D)
            {
                GL.PushMatrix();
                GL.LoadOrtho();
            }
            cur2D = z2D;

            if (z2D && zInPixels)
                invScale = new Vector2(1.0f / Screen.width, 1.0f / Screen.height);
            else
                invScale = new Vector2(1.0f, 1.0f);
        }

        // End
        static public void End()
        {
            if (curDrawMode != -1)
            {
                GL.End();
            }
            curDrawMode = -1;

            if (cur2D)
            {
                GL.End();
                GL.PopMatrix();
            }
        }

        static public void Color(Color zColor)
        {
            glColor = zColor;
            GL.Color(zColor);
        }

        // SetLines
        static void SetLines()
        {
            if (curDrawMode != 0)
            {
                if (curDrawMode != -1)
                    GL.End();
                GL.Begin(GL.LINES);
                curDrawMode = 0;
            }

            GL.Color(glColor);
        }

        // SetQuads
        static void SetQuads()
        {
            if (curDrawMode != 1)
            {
                if (curDrawMode != -1)
                    GL.End();
                GL.Begin(GL.QUADS);
                curDrawMode = 1;
            }

            GL.Color(glColor);
        }

        // Rect
        static public void Rect(Vector2 zP, Vector2 zSize, bool zCenter)
        {
            SetQuads();

            if (zCenter)
                zP -= zSize * 0.5f;

            zSize.Scale(invScale);
            zP.Scale(invScale);

            GL.Vertex3(zP.x, zP.y, 0.0f);
            GL.Vertex3(zP.x + zSize.x, zP.y, 0.0f);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, 0.0f);
            GL.Vertex3(zP.x, zP.y + zSize.y, 0.0f);
        }

        // Line (Vector2)
        static public void Line(Vector2 zStart, Vector2 zEnd)
        {
            SetLines();
            zStart.Scale(invScale);
            zEnd.Scale(invScale);
#if true
            GL.Vertex3(zStart.x, zStart.y, 0.0f);
            GL.Vertex3(zEnd.x, zEnd.y, 0.0f);
#else
        GL.Vertex3(zStart.x, zStart.y, 1.0f);
        GL.Vertex3(zEnd.x, zEnd.y, 1.0f);
#endif
        }

        // Line (Vector3)
        static public void Line(Vector3 zStart, Vector3 zEnd)
        {
            SetLines();
            GL.Vertex(zStart);
            GL.Vertex(zEnd);
        }

        // Circle
        static public void Circle(Vector3 zCenter, float zRadius)
        {
            int numSegments = 32;

            float step = (2.0f * Mathf.PI) / (float)numSegments;

            Vector3 lastPoint = zCenter + new Vector3(0.0f, 0.0f, zRadius);

            float rad = step;
            for (int iStep = 0; iStep < numSegments; ++iStep)
            {
                Vector3 point = zCenter + new Vector3(Mathf.Sin(rad) * zRadius, 0.0f, Mathf.Cos(rad) * zRadius);
                rcGL.Line(lastPoint, point);
                lastPoint = point;
                rad += step;
            }
        }

        // CircleXY
        static public void CircleXY(Vector3 zCenter, float zRadius)
        {
            int numSegments = 32;

            float step = (2.0f * Mathf.PI) / (float)numSegments;

            Vector3 lastPoint = zCenter + new Vector3(0.0f, zRadius, 0.0f);

            float rad = step;
            for (int iStep = 0; iStep < numSegments; ++iStep)
            {
                Vector3 point = zCenter + new Vector3(Mathf.Sin(rad) * zRadius, Mathf.Cos(rad) * zRadius, 0.0f);
                rcGL.Line(lastPoint, point);
                lastPoint = point;
                rad += step;
            }
        }


        // Cube (Vector3)
        static public void Cube(Vector3 zP, Vector3 zSize, bool zCenter = true)
        {
            SetQuads();
            if (zCenter)
                zP -= zSize * 0.5f;

            // Front
            GL.Vertex3(zP.x, zP.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, zP.z);
            GL.Vertex3(zP.x, zP.y + zSize.y, zP.z);

            // Back
            GL.Vertex3(zP.x, zP.y, zP.z + zSize.z);
            GL.Vertex3(zP.x + zSize.x, zP.y, zP.z + zSize.z);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, zP.z + zSize.z);
            GL.Vertex3(zP.x, zP.y + zSize.y, zP.z + zSize.z);

            // Top
            GL.Vertex3(zP.x, zP.y + zSize.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, zP.z + zSize.z);
            GL.Vertex3(zP.x, zP.y + zSize.y, zP.z + zSize.z);

            // Bottom
            GL.Vertex3(zP.x, zP.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y, zP.z + zSize.z);
            GL.Vertex3(zP.x, zP.y, zP.z + zSize.z);

            // Left
            GL.Vertex3(zP.x, zP.y, zP.z);
            GL.Vertex3(zP.x, zP.y + zSize.y, zP.z);
            GL.Vertex3(zP.x, zP.y + zSize.y, zP.z + zSize.z);
            GL.Vertex3(zP.x, zP.y, zP.z + zSize.z);

            // Right
            GL.Vertex3(zP.x + zSize.x, zP.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, zP.z);
            GL.Vertex3(zP.x + zSize.x, zP.y + zSize.y, zP.z + zSize.z);
            GL.Vertex3(zP.x + zSize.x, zP.y, zP.z + zSize.z);
        }

        // CubeWire (Vector3)
        static public void CubeWire(Vector3 zP, Vector3 zSize, Quaternion zRot, bool zCenter = true)
        {
            SetQuads();

            var localOffset = zCenter ? -zSize * 0.5f : Vector3.zero;

            // Front
            var frontTL = zRot * (localOffset + new Vector3(0.0f, 0.0f, 0.0f)) + zP;
            var frontTR = zRot * (localOffset + new Vector3(zSize.x, 0.0f, 0.0f)) + zP;
            var frontBR = zRot * (localOffset + new Vector3(zSize.x, zSize.y, 0.0f)) + zP;
            var frontBL = zRot * (localOffset + new Vector3(0.0f, zSize.y, 0.0f)) + zP;

            // Back
            var backTL = zRot * (localOffset + new Vector3(0.0f, 0.0f, zSize.z)) + zP;
            var backTR = zRot * (localOffset + new Vector3(zSize.x, 0.0f, zSize.z)) + zP;
            var backBR = zRot * (localOffset + new Vector3(zSize.x, zSize.y, zSize.z)) + zP;
            var backBL = zRot * (localOffset + new Vector3(0.0f, zSize.y, zSize.z)) + zP;

            Line(frontTL, frontTR);
            Line(frontTR, frontBR);
            Line(frontBR, frontBL);
            Line(frontBL, frontTL);

            Line(backTL, backTR);
            Line(backTR, backBR);
            Line(backBR, backBL);
            Line(backBL, backTL);

            Line(frontTL, backTL);
            Line(frontTR, backTR);
            Line(frontBR, backBR);
            Line(frontBL, backBL);
        }


        // Point ( Vector3)
        static public void Point(Vector3 zP, float zSize)
        {
            SetLines();
            GL.Vertex3(zP.x - zSize, zP.y, zP.z); GL.Vertex3(zP.x + zSize, zP.y, zP.z);
            GL.Vertex3(zP.x, zP.y - zSize, zP.z); GL.Vertex3(zP.x, zP.y + zSize, zP.z);
            GL.Vertex3(zP.x, zP.y, zP.z - zSize); GL.Vertex3(zP.x, zP.y, zP.z + zSize);
        }

        // SetMaterialSolidColor
        static public void SetMaterialSolidColor()
        {
            if (!materialSolidColor)
            {
                materialSolidColor = new Material(Shader.Find("GL/TransparentColor"));
            }

            materialSolidColor.SetPass(0);
        }

        // SetMaterialSolidColorZTestOff
        static public void SetMaterialSolidColorZTestOff()
        {
            if (!materialSolidColorZTestOff)
            {
                materialSolidColorZTestOff = new Material(Shader.Find("GL/SolidColor_ZTestOff"));
            }

            materialSolidColorZTestOff.SetPass(0);
        }

        // SetMaterialTransparentColor
        static public void SetMaterialTransparentColor()
        {
            if (!materialTransparentColor)
            {
                materialTransparentColor = new Material(Shader.Find("GL/TransparentColor"));
            }

            materialTransparentColor.SetPass(0);
        }
    }
}