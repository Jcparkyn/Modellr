using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ExtensionMethods;
using UnityEngine.UI;


/*  This class used for displaying the trasform gizmo (translate, rotate, scale, extrude)
 *  in the 3D scene, and processing/handling user input (clicking and dragging the gizmo)
 */
public class TransformGizmo : MonoBehaviour {

    public float size = 0.07f;

    float prevPos;
    enum Axis { x, y, z, all };
    bool isDragging = false;
    bool isActive = false;
    Ray axisRay = new Ray();

    public enum GizmoMode { translate, rotate, scale, extrude };
    GizmoMode gizmoMode = GizmoMode.translate;

    public GameObject translateRoot;
    public GameObject rotateRoot;
    public GameObject scaleRoot;
    public GameObject extrudeRoot;

    public Image BUTTON_TRANSLATE;
    public Image BUTTON_ROTATE;
    public Image BUTTON_SCALE;
    public Image BUTTON_EXTRUDE;

    void Update() {
        //Debug.DrawRay(Vector3.zero, Vector3.up*10);
        if (Input.GetMouseButtonDown(0))
        {
            
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1<<9))
            {
                string hitName = hit.transform.gameObject.tag;
                if (hitName == "xAxis")
                {
                    OnBeginDrag(Axis.x);
                }
                else if (hitName == "yAxis")
                {
                    OnBeginDrag(Axis.y);
                }
                else if (hitName == "zAxis")
                {
                    OnBeginDrag(Axis.z);
                }
                else if (hitName == "centerAxis")
                {
                    OnBeginDrag(Axis.all);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnEndDrag();
        }
        if (isDragging)
        {
            WhileDragging();
        }
    }

    void OnBeginDrag (Axis axis)
    {
        if(gizmoMode == GizmoMode.extrude && MeshEdit.selectionMode == 1)
        {
            MeshEdit.selMesh.BeginExtrude(MeshEdit.selMesh.selFaces);
        }
        isDragging = true;
        MeshEdit.OnTransformGizmoBeginDrag();
        switch (axis)
        {
            case Axis.x:
                axisRay = new Ray(transform.position, transform.right);
                break;
            case Axis.y:
                axisRay = new Ray(transform.position, transform.up);
                break;
            case Axis.z:
                axisRay = new Ray(transform.position, transform.forward);
                break;
            case Axis.all:
                axisRay = new Ray(transform.position, Vector3.one);
                break;
        }

        if (gizmoMode != GizmoMode.rotate)
        {
            prevPos = MathX.ScreenToPointOnAxis(axisRay); 
        }
        else
        {
            prevPos = GetMouseAngleFromGizmo();
        }
    }

    void WhileDragging ()
    {
        if (gizmoMode != GizmoMode.rotate)
        {
            Debug.DrawRay(transform.position, axisRay.direction * 10);
            //Ray axisRay = new Ray(transform.position, transform.right);

            float newPos = MathX.ScreenToPointOnAxis(axisRay);
            //DebugPoint(newPos, Color.green);
            float posDelta = newPos - prevPos;
            prevPos = newPos;

            //Debug.Log(prevPos.ToString());
            
            if (gizmoMode == GizmoMode.translate || gizmoMode == GizmoMode.extrude)
            {
                transform.position += axisRay.direction * posDelta;
                MeshEdit.OnTranslateGizmoDragged(axisRay.direction * posDelta);
            } else {
                MeshEdit.OnScaleGizmoDragged(axisRay.direction * posDelta);
                //transform.localScale += axisRay.direction * posDelta;
            }
            SetScaleAuto();
        }
        else
        {
            //float newPos = GetMouseAngleFromGizmo();
            float posDelta = GetMouseAngleFromGizmoRelative();// newPos - prevPos;
            prevPos += posDelta;
            Quaternion rotAngle = Quaternion.AngleAxis(posDelta, axisRay.direction);//Euler(axisRay.direction * posDelta);
            MeshEdit.OnRotateGizmoDragged(rotAngle);
            transform.Rotate(axisRay.direction, posDelta, Space.World);
        }
    }

    void OnEndDrag()
    {
        isDragging = false;
        SetScaleAuto();
        MeshEdit.selMesh.UpdateColliderMesh();
        MeshEdit.SaveUndoState();
        //transform.localRotation = Quaternion.identity;
    }

    public void OnSelectionChange()
    {
        transform.localRotation = Quaternion.identity;
        SetPositionAuto();
        if(gizmoMode == GizmoMode.extrude)
        {
            SetExtrudeAngle();
        }
    }

    public void SetPosition(Vector3 newPos, bool forceEnable = true)
    {
        if (!isActive && forceEnable)
        {
            SetEnabled();
        }
        transform.position = newPos;
        SetScaleAuto();
    }

    public void SetPositionAuto()
    {
        IEnumerable<ISelectable> targetPoints = MeshEdit.selMesh.GetSelectedPoints();
        bool anySelected = targetPoints.Any();
        SetEnabled(anySelected);
        if (anySelected)
        {
            transform.position = targetPoints.PositionAverage();
        }

        SetScale(Vector3.Distance(Camera.main.transform.position, transform.position));
    }

    public void SetExtrudeAngle()
    {
        transform.rotation = Quaternion.LookRotation(MeshEdit.selMesh.selFaces.NormalAverage());
    }

    public void SetDisabled()
    {
        SetEnabled(false);
    }

    public void SetEnabled(bool value = true)
    {
        if(isActive != value)
        {
            isActive = value;
            transform.gameObject.SetActive(value);
        }
    }

    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale * size;
    }

    public void SetScaleAuto()
    {
        SetScale(Vector3.Distance(Camera.main.transform.position, transform.position));
    }

    public void SetGizmoMode(int val)
    {
        transform.localRotation = Quaternion.identity;
        switch (val)
        {
            case 0:
                gizmoMode = GizmoMode.translate;
                break;
            case 1:
                gizmoMode = GizmoMode.rotate;
                break;
            case 2:
                gizmoMode = GizmoMode.scale;
                break;
            case 3:
                gizmoMode = GizmoMode.extrude;
                MeshEdit.selectionMode = 1;
                SetExtrudeAngle();
                break;
            default:
                Debug.LogError("Cannot set gizmo mode to " + val + ". Must be between 0 and 3"); break;
        }
        SetGizmoStates(gizmoMode);
    }

    private void SetGizmoStates(GizmoMode mode)
    {
        Color colorInactive = new Color(1, 1, 1, 0.5f);
        Color colorActive = new Color(1, 1, 1, 1);

        translateRoot.SetActive(mode == GizmoMode.translate);
        BUTTON_TRANSLATE.color = mode == GizmoMode.translate ? colorActive : colorInactive;

        rotateRoot.SetActive(mode == GizmoMode.rotate);
        BUTTON_ROTATE.color = mode == GizmoMode.rotate ? colorActive : colorInactive;

        scaleRoot.SetActive(mode == GizmoMode.scale);
        BUTTON_SCALE.color = mode == GizmoMode.scale ? colorActive : colorInactive;

        extrudeRoot.SetActive(mode == GizmoMode.extrude);
        BUTTON_EXTRUDE.color = mode == GizmoMode.extrude ? colorActive : colorInactive;
    }

    float GetMouseAngleFromGizmoRelative()
    {
        Vector2 relativeMousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float v = Vector2.SignedAngle(Vector2.up, relativeMousePos);
        if (Vector3.Dot(Camera.main.transform.position - transform.position, axisRay.direction) > 0)
        {
            v = -v;
        }
        return v - prevPos;
    }
    float GetMouseAngleFromGizmo()
    {
        Vector2 relativeMousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float v = Vector2.SignedAngle(Vector2.up, relativeMousePos);
        if (Vector3.Dot(Camera.main.transform.position - transform.position, axisRay.direction) > 0)
        {
            v = -v;
        }
        return v;
    }
}
