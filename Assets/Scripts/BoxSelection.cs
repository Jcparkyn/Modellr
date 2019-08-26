using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//  This class handles box selection.
public class BoxSelection : MonoBehaviour {

    Image image;
    RectTransform rectT;
    public TransformGizmo transformGizmo;
    public bool isSelecting = false;

    Vector2 startPos;
    Vector2 endPos;

	void Start () {
        image = GetComponent<Image>();
        rectT = GetComponent<RectTransform>();
	}
	
	void Update () {
        if (isSelecting)
        {
            WhileSelecting();
        }
	}

    public void BeginSelection()
    {
        if (!isSelecting)
        {
            isSelecting = true;
            image.enabled = true;
            startPos = Input.mousePosition;
            endPos = Input.mousePosition;
            UpdateCorners();
        }
    }

    void WhileSelecting()
    {
        endPos = Input.mousePosition;
        UpdateCorners();
    }

    public void EndSelection()
    {
        if (isSelecting)
        {
            isSelecting = false;
            image.enabled = false;

            foreach(ISelectable point in MeshEdit.selMesh.GetAllPoints())
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(point.Pos);
                Rect test = CornersToRect();
                if (test.Contains(screenPos))
                {
                    point.Selected = true;
                }
            }
            transformGizmo.SetPositionAuto();
            transformGizmo.OnSelectionChange();
            MeshEdit.SaveUndoState();
        }
    }

    public void UpdateCorners()
    {
        rectT.offsetMin = Vector2.Min(endPos, startPos);
        rectT.offsetMax = Vector2.Max(endPos, startPos);
    }

    public Rect CornersToRect()
    {
        return new Rect(rectT.offsetMin, rectT.offsetMax - rectT.offsetMin);
    }
}
