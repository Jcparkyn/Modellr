using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is used for additional math-related methods.
/// These are mostly used by TransfomGizmo for calculating 3D position values based on a 2D cursor position.
/// </summary>
public static class MathX {

    public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
    {

        //get vector from point on line to point in space
        Vector3 linePointToPoint = point - linePoint;

        float t = Vector3.Dot(linePointToPoint, lineVec);

        return linePoint + lineVec * t;
    }

    public static Vector3 VectorAverage(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in points)
        {
            sum.x += point.x;
            sum.y += point.y;
            sum.z += point.z;
        }
        sum /= points.Count;
        return sum;
    }

    public static Vector3 ScreenToPointOnPlane(Plane plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distZ;
        Vector3 hitPoint = new Vector3();
        if (plane.Raycast(ray, out distZ))
        {
            hitPoint = ray.GetPoint(distZ);
        }
        return hitPoint;
    }

    public static float ScreenToPointOnAxis(Ray axis)
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector3 axisCamPerpPoint = ProjectPointOnLine(axis.origin, axis.direction, camPos);
        //DebugPoint(axisCamPerpPoint);
        Plane axisPlaneFacingCam = new Plane(camPos - axisCamPerpPoint, axisCamPerpPoint);
        //Debug.DrawRay(axisCamPerpPoint, camPos - axisCamPerpPoint, Color.red);
        Vector3 pointOnPlane = ScreenToPointOnPlane(axisPlaneFacingCam);
        //DebugPoint(pointOnPlane, Color.blue);
        float posOnAxis = Vector3.Dot(pointOnPlane, axis.direction);
        return posOnAxis;
    }
}
