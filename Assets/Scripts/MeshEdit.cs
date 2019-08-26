using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ExtensionMethods;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

[RequireComponent(typeof(MeshFilter))]
public class MeshEdit : MonoBehaviour
{
    #region Class Variables
    public static MeshObject selMesh; //Static reference to the main MeshObject being edited
    public static int selectionMode = 0; //0:verts, 1:faces
    public static UndoStack<MeshObjectSerializable> undoStack = new UndoStack<MeshObjectSerializable>(length: 60);
    public bool renderAllPoints = false;
    bool GUIreadyForClick = true;
    bool boxSelectArmed = false;

    public static MeshFilter meshFilter; //The MeshFilter (a Unity component) to use for rendering the mesh
    public static MeshCollider meshCollider; //The MeshCollider (a Unity component) to use for hit testing
    public TransformGizmo transformGizmo; //Reference to TransformGizmo object
    public BoxSelection boxSelect; //Reference to BoxSelection object

    //References to UI objects:
    public UIController ui;

    //Styles used for rendering vertices:
    public GUIStyle guiVertUnsel;
    public GUIStyle guiVertSel;
    #endregion

    //------------------------------------------------------------------
    //------------------------------------------------------------------

    void Start ()
    {
        //SaveLoad.LoadLastMeshSavePath();        
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();

        selMesh = DefaultMeshes.Cube(selMesh);
        selMesh.meshCollider = meshCollider;
        selMesh.selVerts.Clear();
        selMesh.selFaces.Clear();
        selMesh.ReconstructMesh();
        //gameObject.GetComponent<MeshCollider>().sharedMesh = selMesh.meshFilter.sharedMesh;
        SaveUndoState();
        //Debug.Log(Application.persistentDataPath);
    }
	
	void Update ()
    {
        //Handle Box Selection Input:
        bool boxSelectArmedFinal = Input.GetKey(KeyCode.LeftAlt) || boxSelectArmed;
        ui.RECT_BOXSELECT.gameObject.SetActive(boxSelectArmedFinal);
        if (boxSelectArmedFinal)
        {
            ui.RECT_BOXSELECT.position = Input.mousePosition;
        }
        if (Input.GetMouseButtonDown(0) && !boxSelect.isSelecting && boxSelectArmedFinal)
        {
            boxSelect.BeginSelection();
            boxSelectArmed = false;
        }
        else if (Input.GetMouseButtonUp(0) && boxSelect.isSelecting)
        {
            boxSelect.EndSelection();
        }


        if (Input.GetButtonDown("Select All"))
        {
            ToggleSelectAll();
        }
        else if (Input.GetButtonDown("Extrude") && selMesh.GetSelectedPoints().Any())
        {
            if (selectionMode == 0)
            {
                selMesh.BeginExtrude(selMesh.selVerts);
            }
            else
            {
                selMesh.BeginExtrude(selMesh.selFaces);
            }
            transformGizmo.SetPositionAuto();
        }
        else if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
        {
            UndoEdit();
        }
        else if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl))
        {
            RedoEdit();
        }
        else if (Input.GetButtonDown("Delete"))
        {
            DeleteSelected();
        }
        else if (Input.GetButtonDown("Fill"))
        {
            TryCreateFace();
        }
        else if (Input.GetButtonDown("Select Linked"))
        {
            BeginSelectLinked();
        }
        else if (Input.GetKeyDown(KeyCode.Home))
        {
            FocusOnSelected();
        }

    }

    public void DeleteSelected()
    {
        if (selectionMode == 0)
        {
            selMesh.DeleteVerts(selMesh.selVerts);
        }
        else
        {
            selMesh.DeleteFaces(selMesh.selFaces);
        }
        transformGizmo.SetPositionAuto();
        SaveUndoState();
    }

    public void FocusOnSelected()
    {
        HashSet<Vertex> selPoints = selMesh.GetSelectedAsVertices();
        CameraMove cam = GameObject.Find("CameraPivot").GetComponent<CameraMove>();
        if (selPoints.Any())
        {
            cam.FocusOnPoint(selPoints.PositionAverage());
        }
        else
        {
            cam.FocusOnPoint(selMesh.meshFilter.transform.position);
        }
    }

    /// <summary>
    /// Code to run in the legacy (Immediate Mode) GUI layer (Called by
    /// MonoBehaviour every UI refresh). This code handles displaying vertices,
    /// and checking when they are clicked.
    /// </summary>
    private void OnGUI()
    {
        foreach (ISelectable point in selMesh.GetAllPoints())
        {
            float widgetSize = 10;

            Vector3 vertScreenPos = Camera.main.WorldToScreenPoint(point.Pos);
            float distToMouse = Vector2.SqrMagnitude(vertScreenPos - Input.mousePosition);
            vertScreenPos.y = Screen.height - vertScreenPos.y;
            vertScreenPos -= new Vector3(1, 1, 0) * widgetSize / 2;

            float mouseDistThreshold = selectionMode == 0 ? 1000 : 3000;
            if (vertScreenPos.z > 0)
            {
                if (renderAllPoints
                    || point.Selected
                    || (distToMouse < mouseDistThreshold && PointIsVisible(point)))
                {
                    GUIStyle guiVertStyle = point.Selected ? guiVertSel : guiVertUnsel;
                    GUI.Label(new Rect(vertScreenPos.x, vertScreenPos.y, widgetSize, widgetSize), "", guiVertStyle);
                    if (distToMouse < 64 && Input.GetMouseButtonDown(0) && GUIreadyForClick)
                    {
                        GUIreadyForClick = false;

                        OnPointClicked(point, Input.GetKey(KeyCode.LeftShift));
                    } 
                }
            }
        }

        if (!Input.GetMouseButtonDown(0))
        {
            GUIreadyForClick = true;
        }

        bool PointIsVisible(ISelectable point)
        {
            Vector3 camPos = Camera.main.transform.position;
            float distanceThreshold = 0.95f;
            //RaycastHit hit;
            bool didHit = Physics.Raycast(camPos, point.Pos - camPos, out RaycastHit hit);
            if (hit.distance / Vector3.Distance(camPos, point.Pos) > distanceThreshold || !didHit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //------------------------------------------------------------------
    //The following methods handle actions for mesh editing and similar
    //operations. These are called by UI buttons and keyboard input in the
    //"Update" method.
    public void ToggleSelectAll()
    {
        if (selectionMode == 0)
        {
            selMesh.ToggleAllVertSelection();
        }
        else
        {
            selMesh.ToggleAllFaceSelection();
        }

        transformGizmo.OnSelectionChange();
    }

    public void TryCreateFace()
    {
        if (selectionMode == 0)
        {
            List<Vertex> selVertsList = selMesh.selVerts.ToList();
            if (selVertsList.Count >= 3)
            {
                selMesh.CreateFace(selVertsList, doReconstruct: true);
                transformGizmo.OnSelectionChange();
                SaveUndoState();
            }
            else
            {
                Debug.Log("Must have at least 3 vertices selected to create face");
            }
        }
        else
        {
            Debug.Log("Must be in vert selection mode to create face");
        }
    }

    public void BeginSelectLinked()
    {
        if (selectionMode == 0)
        {
            HashSet<Vertex> selVertsTemp = new HashSet<Vertex>(selMesh.selVerts);
            foreach (Vertex vert in selVertsTemp)
            {
                selMesh.SelectLinked(vert);
            }
            transformGizmo.SetPositionAuto();
        }
    }
    
    public void SetSelectionMode(int mode)
    {
        if (selectionMode == mode)
        {
            return;
        }
        //Set gizmo mode back to translate if switching to vert selection mode.
        if (mode == 0 && transformGizmo.gizmoMode == TransformGizmo.GizmoMode.extrude)
        {
            //transformGizmo.SetGizmoMode((int)TransformGizmo.GizmoMode.translate);
            ui.DROPDOWN_GIZMOMODE.value = (int)TransformGizmo.GizmoMode.translate;
        }
        //Color colorInactive = new Color(1, 1, 1, 0.4f);
        //Color colorActive = new Color(1, 1, 1, 1);
        //ui.BUTTON_SELMODE_FACE.color = mode == 0 ? colorInactive : colorActive;
        //ui.BUTTON_SELMODE_VERT.color = mode == 1 ? colorInactive : colorActive;
        selectionMode = mode;
        transformGizmo.SetPositionAuto();
    }

    public void SetDoRenderPoints(bool val)
    {
        renderAllPoints = val;
    }

    public void ArmBoxSelection()
    {
        boxSelectArmed = true;
    }

    public void InsertMesh(int meshID)
    {
        switch (meshID)
        {
            case 0:
                DefaultMeshes.Cube(selMesh);
                break;
            case 1:
                DefaultMeshes.Plane(selMesh);
                break;
            case 2:
                DefaultMeshes.Sphere(selMesh);
                break;
            case 3:
                DefaultMeshes.Circle(selMesh);
                break;
            default:
                Debug.LogError($"Mesh ID not valid ({meshID})");
                break;
        }
        //selMesh.verts.AddRange(newMesh.verts);
        //selMesh.faces.AddRange(newMesh.faces);
        selMesh.ReconstructMesh();
        //selMesh.selVerts = new HashSet<Vertex>(newMesh.verts);
        //selMesh.selFaces = new HashSet<Face>(newMesh.faces);
        transformGizmo.OnSelectionChange();
    }

    //------------------------------------------------------------------
    //The following methods are called by TransformGizmo when it is dragged.

    public static void OnTransformGizmoBeginDrag()
    {
        //selMesh.BeginTransform();
    }

    public static void OnTranslateGizmoDragged(Vector3 posDelta)
    {
        selMesh.TranslateSelected(posDelta);
    }

    public static void OnRotateGizmoDragged(Quaternion rotDelta)
    {
        selMesh.RotateSelected(rotDelta);
    }

    public static void OnScaleGizmoDragged(Vector3 scaleDelta)
    {
        selMesh.ScaleSelected(scaleDelta + Vector3.one);
    }
    
    //------------------------------------------------------------------
    //Save and Load related methods. Called by UI events.

    public void SetMeshSavePath(string str)
    {
        SaveLoad.SetMeshSavePath(str);
    }

    public void SaveSelectedMesh ()
    {
        SaveLoad.SaveMesh(selMesh);
    }

    public void SaveSelectedMeshOBJ()
    {
        SaveLoad.ExportOBJ(selMesh);
    }

    /// <summary>
    /// Called by Unity UI "Load OBJ" Button
    /// </summary>
    public void BeginLoadOBJ()
    {
        SaveLoad.RequestObj();
    }

    /// <summary>
    /// Called by JS plugin when user has chosen an .obj file to import
    /// </summary>
    /// <param name="url"></param>
    public void OnObjUploaded(string url)
    {
        Debug.Log("File url returned: " + url);
        //MeshObject mesh = SaveLoad.ImportObjFromPath(fileUrl);
        StartCoroutine(ReadUploadedObj(url));
    }

    IEnumerator ReadUploadedObj(string fileUrl)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(fileUrl))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            //string[] pages = fileUrl.Split('/');
            //int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log("Web Request Error: " + webRequest.error);
            }
            else
            {
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                var newVerts = new List<Vertex>();
                var newFaces = new List<Face>();
                //string objStr = webRequest.downloadHandler.text;
                using (StringReader reader = new StringReader(webRequest.downloadHandler.text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Debug.Log("Line has been read: " + line);
                        //Process Vertex
                        if (line.StartsWith("v "))
                        {
                            newVerts.Add(VertexFromObjString(line.Substring(1)));
                        }
                        //Process Face
                        else if (line.StartsWith("f "))
                        {
                            string[] idStrings = line.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            List<Vertex> faceVerts = new List<Vertex>(idStrings.Length);
                            //var idStringsAbsolute = Regex.Matches(line, @" \d+");
                            //var idStringsRelative = Regex.Matches(line, @"-(\d+)");
                            foreach (Match match in Regex.Matches(line, @" (\d+)"))
                            {
                                //string strCropped = str.Split(new[] { '/' }, 1)[0];
                                faceVerts.Add(newVerts[Int32.Parse(match.Groups[1].Value) - 1]);
                            }
                            foreach (Match match in Regex.Matches(line, @"-(\d+)"))
                            {
                                //string strCropped = str.Split(new[] { '/' }, 1)[0];
                                faceVerts.Add(newVerts[newVerts.Count - Int32.Parse(match.Groups[1].Value)]);
                            }
                            //List<Vertex> faceVerts = idStrings.Select(str => newVerts[int.Parse(str)-1]).ToList();
                            newFaces.Add(new Face(faceVerts, selMesh));
                        }
                    }
                }
                selMesh.verts = newVerts;
                selMesh.faces = newFaces;
                selMesh.ReconstructMesh();
                transformGizmo.SetPositionAuto();
                SaveUndoState();
            }
        }

        Vertex VertexFromObjString(string str)
        {
            string[] numStrings = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            Vector3 vertPos = new Vector3(
                float.Parse(numStrings[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(numStrings[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(numStrings[2], CultureInfo.InvariantCulture.NumberFormat));
            return new Vertex(vertPos, selMesh);
        }
    }

    public void LoadMeshToSelected ()
    {
        MeshObject mesh = SaveLoad.LoadMesh(meshFilter, meshCollider);

        if (mesh != null)
        {
            selMesh = mesh;
            selMesh.ReconstructMesh();
            transformGizmo.SetPositionAuto();
            SaveUndoState(); 
        }
    }
    
    //------------------------------------------------------------------
    //Undo related methods. Called mainly by UI/Keyboard input. Most modelling
    //operations will call SaveUndoState() to add an Undo step after that
    //operation.

    public static void SaveUndoState()
    {
        undoStack.Do(new MeshObjectSerializable(selMesh));
    }

    public void UndoEdit()
    {
        //MeshObjectSerializable undoMesh;
        if (undoStack.TryUndo(out MeshObjectSerializable undoMesh))
        {
            selMesh = undoMesh.ToMeshObject(meshFilter, meshCollider);
            //selMesh.UpdateColliderMesh();
            transformGizmo.SetPositionAuto();
        }
        else
        {
            Debug.Log("Nothing left to undo");
        }
    }

    public void RedoEdit()
    {
        //MeshObjectSerializable redoMesh;
        if (undoStack.TryRedo(out MeshObjectSerializable redoMesh))
        {
            selMesh = redoMesh.ToMeshObject(meshFilter, meshCollider);
            //selMesh.UpdateColliderMesh();
            transformGizmo.SetPositionAuto();
        }
        else
        {
            Debug.Log("Nothing left to redo");
        }
    }
    
    //------------------------------------------------------------------
    //Other methods.

    private void OnPointClicked(ISelectable point, bool additional = false)
    {
        if (additional)
        {
            point.SelectAdditional();
        }
        else
        {
            point.SelectAbsolute();
        }
        transformGizmo.OnSelectionChange();
        SaveUndoState();
    }

    //public void FileDialogResult(string fileUrl)
    //{
    //    Debug.Log("File url returned: " + fileUrl);
    //    //MeshObject mesh = SaveLoad.ImportObjFromPath(fileUrl);
    //    StartCoroutine(ReadUploadedObj(fileUrl));
    //}

    public void ExitApplication()
    {
        Application.Quit();
    }
}