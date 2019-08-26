using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExtensionMethods;

//This class handles camera movement.
public class CameraMove : MonoBehaviour {

    public float rotateSensitivity = 3f;
    public float panSensitivity = 3f;
    public float zoomSensitivity = 1f;
    public float zoomMin = 0.5f;
    public float zoomMax = 100f;

    public TransformGizmo transformGizmo;
    Transform camTransform;

    float camDistance = 7;

    private Vector3 mousePosPrev;

	void Start () {
        camTransform = transform.GetChild(0);
        camDistance = -camTransform.localPosition.z;
        SetTransformGizmoSize();
    }
	
	void Update () {
        Vector3 mousePosDelta = mousePosPrev - Input.mousePosition;
        mousePosPrev = Input.mousePosition;
        if (Input.GetMouseButton(2)) // Middlemouse 
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                PanView(mousePosDelta);
            }
            else
            {
                RotateView(mousePosDelta);
            }
            
        }
        if(Input.mouseScrollDelta.y != 0) //Scroll
        {
            ZoomView(Input.mouseScrollDelta.y);
        }
	}

    void PanView (Vector3 delta)
    {
        transform.Translate(delta * 0.001f * camDistance * panSensitivity, Space.Self);
        SetTransformGizmoSize();
    }

    void RotateView (Vector3 delta)
    {
        int upsideDown = Vector3.Dot(transform.up, Vector3.down) < 0 ? 1 : -1;
        transform.Rotate(Vector3.up, -delta.x * rotateSensitivity * upsideDown * 0.1f, Space.World);
        transform.Rotate(transform.right, delta.y * rotateSensitivity * 0.1f, Space.World);
        SetTransformGizmoSize();
    }

    void ZoomView (float delta)
    {
        camDistance *= 1 - delta * zoomSensitivity * 0.1f;
        camDistance = Mathf.Clamp(camDistance, zoomMin, zoomMax);
        camTransform.localPosition = new Vector3(0, 0, -camDistance);
        SetTransformGizmoSize();
    }

    public void FocusOnPoint (Vector3 pos)
    {
        //Animate
        transform.position = pos;
        SetTransformGizmoSize();
    }

    void SetTransformGizmoSize ()
    {
        float scale = Vector3.Distance(camTransform.position, transformGizmo.transform.position);
        transformGizmo.SetScale(scale);
    }
}
