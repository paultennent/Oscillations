using UnityEngine;
using System.Collections;

public static class rcCameraUtils
{
    public static Vector3 GetInputWorldSpacePositionHorizontal(Camera camera, Vector3 InputScreenSpacePosition, float planeYPosition)
    {
        // Convert screen space input position to world space origin and direction vector
        Ray ray = camera.ScreenPointToRay(InputScreenSpacePosition);

        // Intersect ray with the ground plane on which the vehicle currently sits
        Plane plane = new Plane(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, planeYPosition, 0.0f));

        // Cast ray through plane
        float distance = 0.0f;
        bool hit = plane.Raycast(ray, out distance);
        Vector3 InputWorldSpacePosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        if (hit)
        {
            // Get world space position of input
            InputWorldSpacePosition = ray.origin + (ray.direction * distance);
        }

        return InputWorldSpacePosition;
    }

    public static Vector3 GetInputWorldSpacePositionCameraAligned(Camera camera, Vector3 InputScreenSpacePosition, float depth)
    {
        // Convert screen space input position to world space origin and direction vector
        Ray ray = camera.ScreenPointToRay(InputScreenSpacePosition);
            
        // Intersect ray with the world-space plane directly in front of the camera
        Plane plane = new Plane(-camera.transform.forward, camera.transform.position + camera.transform.forward * depth);

        // Cast ray through plane
        float distance = 0.0f;
        bool hit = plane.Raycast(ray, out distance);
        Vector3 InputWorldSpacePosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        if (hit)
        {
            // Get world space position of input
            InputWorldSpacePosition = ray.origin + (ray.direction * distance);
        }

        return InputWorldSpacePosition;
    }

    public static Vector3 GetInputWorldSpaceVectorHorizontal(Camera camera, Vector3 InputScreenSpaceVector)
    {
        Vector3 cameraEuler = camera.transform.rotation.eulerAngles;
        Quaternion cameraQuat = Quaternion.Euler(90.0f, cameraEuler.y, 0.0f);
        Matrix4x4 mtx = Matrix4x4.TRS(camera.transform.position, cameraQuat, new Vector3(1.0f, 1.0f, 1.0f));
        return mtx.MultiplyVector(InputScreenSpaceVector);
    }


    // From http://www.unifycommunity.com/wiki/index.php?title=IsVisibleFrom 
    public static bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        if (renderer == null || camera == null)
            return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
        
}

