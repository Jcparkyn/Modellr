using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System.Linq;


/// <summary>
/// This is the main class used to store the data for a mesh.
/// It contains all the necessary data to store the mesh, as well
/// as cached data for edges and rendering data.It also contains
/// any methods required to perform operations on the mesh.
/// </summary>
public class MeshObject
{
    public List<Vertex> verts;
    public List<Face> faces;
    //edges are stored as a Dictionary instead of a HashSet due to the need for TryGetValue(T key).
    //HashSet only supports this method in later .NET versions than what can be used by Unity.
    public Dictionary<Edge, Edge> edges = new Dictionary<Edge, Edge> { };

    //The components used by Unity for mesh rendering
    public MeshFilter meshFilter { get; }
    public MeshCollider meshCollider;
    private Mesh MESH_RENDER;

    private Vector3[] VERTS_RENDER;
    private int[] TRIS_RENDER;

    public HashSet<Vertex> selVerts = new HashSet<Vertex> { };
    public HashSet<Face> selFaces = new HashSet<Face> { };

    //private List<Vertex> transformVerts = new List<Vertex> { };

    #region Constructors:
    //------------ Constructors:
    public MeshObject(List<Vertex> vertices_, List<Face> faces_, MeshFilter meshFilter_)
    {
        verts = vertices_;
        faces = faces_;
        meshFilter = meshFilter_;
        ReconstructMesh();
    }
    
    public MeshObject(MeshFilter meshFilter_ = null)
    {
        verts = new List<Vertex> { };
        faces = new List<Face> { };
        meshFilter = meshFilter_;
    }
    #endregion

    #region Methods:
    //------------ Methods:
    public IEnumerable<ISelectable> GetSelectedPoints()
    {
        if (MeshEdit.selectionMode == 0)
        {
            return selVerts.Cast<ISelectable>();
        }
        else
        {
            return selFaces.Cast<ISelectable>();
        }
    }

    public IEnumerable<ISelectable> GetAllPoints()
    {
        if (MeshEdit.selectionMode == 0)
        {
            //IEnumerable<IPositionable> temp = selVerts.Cast<IPositionable>();
            return verts.Cast<ISelectable>();
        }
        else
        {
            return faces.Cast<ISelectable>();
        }
    }

    public void ToggleAllVertSelection()
    {
        if (selVerts.Any())
        {
            DeselectAll();
        }
        else
        {
            SelectAll();
        }
    }

    public void ToggleAllFaceSelection()
    {
        if (selFaces.Any())
        {
            DeselectAll();
        }
        else
        {
            SelectAll();
        }
    }

    public void DeselectAll(int mode = -1) //-1 for both, 0 for verts, 1 for faces
    {
        if (mode != 1)
        {
            selVerts.Clear();
        }
        if (mode != 0)
        {
            selFaces.Clear();
        }
    }

    public void SelectAll(int mode = -1) //-1 for both, 0 for verts, 1 for faces
    {
        if (mode != 1) //Verts
        {
            foreach (Vertex vert in verts)
            {
                vert.Selected = true;
            }
            selVerts = new HashSet<Vertex>(verts); 
        }
        if (mode != 0) //Faces
        {
            foreach (Face face in faces)
            {
                face.Selected = true;
            }
            selFaces = new HashSet<Face>(faces); 
        }
    }

    //Select all vertices linked (connected by faces/edges) to a given vertex
    public void SelectLinked(Vertex vert_)
    {
        foreach (Face face in vert_.connectedFaces)
        {
            foreach (Vertex vert in face.Verts)
            {
                if (!vert.Selected)
                {
                    vert.Selected = true;
                    SelectLinked(vert);
                }
            }
        }
    }

    public HashSet<Vertex> GetSelectedAsVertices()
    {
        if (MeshEdit.selectionMode == 0)
        {
            return selVerts;
        }
        else
        {
            return selFaces.GetVerts();
        }
    }

    //create a face spanning given vertices
    public void CreateFace(List<Vertex> verts_, bool doReconstruct = false)
    {
        Face newFace = new Face(verts_, this);
        faces.Add(newFace);
        if (doReconstruct)
        {
            ReconstructMesh();
        }
    }

    public void DeleteFaces(IEnumerable<Face> facesToDel_)
    {
        HashSet<Face> facesToDel = new HashSet<Face>(facesToDel_);

        foreach (Vertex vert in facesToDel.GetVerts())
        {
            if (vert.connectedFaces.IsSubsetOf(facesToDel_))
            {
                verts.Remove(vert);
            }
        }
        foreach (Face face in facesToDel)
        {
            faces.Remove(face);
        }

        DeselectAll();
        ReconstructMesh();
    }

    public void DeleteVerts(IEnumerable<Vertex> vertsToDel_)
    {
        HashSet<Vertex> vertsToDel = new HashSet<Vertex>(vertsToDel_);
        HashSet<Face> facesToDel = new HashSet<Face>(vertsToDel.GetTouchingFaces());
        foreach (Vertex vert in vertsToDel)
        {
            verts.Remove(vert);
        }
        foreach (Face face in facesToDel)
        {
            faces.Remove(face);
        }
        DeselectAll();
        ReconstructMesh();

    }

    //This method makes the topological changes required to begin a face extrusion.
    public void BeginExtrude(HashSet<Face> faces)
    {
        HashSet<Edge> culledEdges = faces.GetBorderingEdges();
        HashSet<Vertex> culledVerts = culledEdges.GetVerts();
        Dictionary<Vertex, Vertex> vertPairs = new Dictionary<Vertex, Vertex> { };

        foreach (Vertex vert in culledVerts)
        {
            HashSet<Face> selectedFacesTouchingVert = new HashSet<Face>(vert.connectedFaces);
            selectedFacesTouchingVert.IntersectWith(faces);

            Vertex newVert = vert.Clone();
            verts.Add(newVert);
            vertPairs.Add(vert, newVert);
            foreach (Face faceTouching in selectedFacesTouchingVert)
            {
                int index = faceTouching.Verts.IndexOf(vert);
                faceTouching.Verts[index] = newVert;
            }
        }

        foreach (Edge edge in culledEdges)
        {
            List<Vertex> newFaceVerts = new List<Vertex> { edge.vert1, edge.vert2, vertPairs[edge.vert2], vertPairs[edge.vert1] };
            CreateFace(newFaceVerts);
        }

        ReconstructMesh();
    }

    //This method makes the topological changes required to begin a vertex extrusion.
    //Some vertex extrusion functionality is missing and could be improved.
    public void BeginExtrude(HashSet<Vertex> verts_)
    {
        HashSet<Vertex> vertsToExtrude = new HashSet<Vertex> (verts_);
        //HashSet<Edge> culledEdges = new HashSet<Edge> ();
        Dictionary<Vertex, Vertex> vertPairs = new Dictionary<Vertex, Vertex> { };
        DeselectAll();
        foreach (Vertex vert in vertsToExtrude)
        {
            Vertex newVert = vert.Clone();
            verts.Add(newVert);
            //newVert.selected = true;
            selVerts.Add(newVert);
            vertPairs.Add(vert, newVert);
        }
            
        foreach (Edge edge in MeshEdit.selMesh.edges.Keys)
        {
            if (vertsToExtrude.Contains(edge.vert1) && vertsToExtrude.Contains(edge.vert2))
            {
                List<Vertex> newFaceVerts = new List<Vertex> { edge.vert1, edge.vert2, vertPairs[edge.vert2], vertPairs[edge.vert1] };
                CreateFace(newFaceVerts);
            }
        }

        ReconstructMesh();
    }

    public void TranslateSelected(Vector3 offset)
    {
        Matrix4x4 transformMatrix = Matrix4x4.Translate(offset);
        TransformSelected(transformMatrix);
    }

    public void RotateSelected(Quaternion rot)
    {
        var points = GetSelectedAsVertices();
        Vector3 pivotOffset = points.PositionAverage();
        Matrix4x4 offsetMatrix = Matrix4x4.Translate(pivotOffset);
        Matrix4x4 offsetMatrix2 = Matrix4x4.Translate(-pivotOffset);
        Matrix4x4 transformMatrix = offsetMatrix * Matrix4x4.Rotate(rot) * offsetMatrix2;
        TransformVerts(points, transformMatrix);
    }

    public void ScaleSelected(Vector3 scale)
    {
        var points = GetSelectedAsVertices();
        Vector3 pivotOffset = points.PositionAverage();
        Matrix4x4 offsetMatrix = Matrix4x4.Translate(pivotOffset);
        Matrix4x4 offsetMatrix2 = Matrix4x4.Translate(-pivotOffset);
        Matrix4x4 transformMatrix = offsetMatrix * Matrix4x4.Scale(scale) * offsetMatrix2;
        TransformVerts(points, transformMatrix);
    }

    public void TransformSelected(Matrix4x4 transformMatrix)
    {
        TransformVerts(GetSelectedAsVertices(), transformMatrix);
    }

    void TransformVerts(IEnumerable<Vertex> verts, Matrix4x4 transformMatrix)
    {
        foreach (Vertex vertToMove in verts)
        {
            vertToMove.Pos = transformMatrix.MultiplyPoint3x4(vertToMove.Pos);
            foreach (int vertID in vertToMove.renderVerts)
            {
                VERTS_RENDER[vertID] = vertToMove.Pos;
            }
            MESH_RENDER.vertices = VERTS_RENDER;
        }
        MESH_RENDER.RecalculateNormals();
    }

    //public void BeginTransform(List<Vertex> vertsToMove)
    //{
    //    transformVerts = vertsToMove;
    //}

    //public void BeginTransform(List<Face> facesToMove)
    //{
    //    transformVerts = facesToMove.GetVerts().ToList();
    //}

    //public void BeginTransform()
    //{
    //    transformVerts = GetSelectedAsVertices().ToList();
    //}

    /// <summary>
    /// Method used to update, render, and cache data for the mesh when the topology changes.
    /// </summary>
    public void ReconstructMesh()
    {
        List<Vector3> newVerts = new List<Vector3> { };
        List<int> newTris = new List<int> { };
        edges.Clear();

        //clear renderVerts array from all Vertices:
        foreach(Vertex vert in verts)
        {
            vert.renderVerts.Clear();
            vert.connectedFaces.Clear();
        }

        //Create duplicate render vertices and tris for each Face
        foreach (Face face in faces)
        {
            int[] newVertRefs = new int[face.Verts.Count];
            int faceRenderIndex = newVerts.Count; //index of first RENDER_VERTEX of face
            face.edges.Clear();
            for (int j = 0; j < face.Verts.Count; j++) //for each vert in face
            {
                Vertex vert = face.Verts[j];
                int nextVertIndex = (j + 1) % face.Verts.Count; //index of next vert around face


                //Generate edges around face, checking whether they have already been created for another face
                Edge newEdge = new Edge(vert, face.Verts[nextVertIndex]);
                Edge newEdge_;
                if(edges.TryGetValue(newEdge, out newEdge_)) //if identical edge already exists, return reference to existing edge
                {
                    newEdge_.connectedFaces.Add(face);
                    face.edges.Add(newEdge_);
                }
                else
                {
                    newEdge.connectedFaces.Add(face);
                    edges.Add(newEdge, newEdge);
                    face.edges.Add(newEdge);
                }    

                vert.connectedFaces.Add(face);
                newVerts.Add(vert.Pos); //add duplicated vertices to newVerts
                newVerts.Add(vert.Pos); //add second vertex for backface
                newVertRefs[j] = newVerts.Count - 2; //store position of the actual vertex reference (only for first vert)
                vert.renderVerts.Add(newVerts.Count - 1); //add render vertex refs to Vertex.renderVerts
                vert.renderVerts.Add(newVerts.Count - 2); //""
            }

            for (int j = 0; j < face.tris.Length; j+=3) // for each tri (starting at j) in Face.tris
            {
                //Debug.Log("Generating duplicate tris for tri " + j + " in " + face.ToString());

                //apply actual vertex reference (map from vertex indices of face (local) to render vertices (global)),
                //and add forwards and backward wound tris to newTris, using vertices offset by 1 for second tri
                newTris.Add(newVertRefs[face.tris[0 + j]]);
                newTris.Add(newVertRefs[face.tris[1 + j]]);
                newTris.Add(newVertRefs[face.tris[2 + j]]);
                newTris.Add(newVertRefs[face.tris[0 + j]] + 1);
                newTris.Add(newVertRefs[face.tris[2 + j]] + 1);
                newTris.Add(newVertRefs[face.tris[1 + j]] + 1);
            }
        }

        VERTS_RENDER = newVerts.ToArray();
        TRIS_RENDER = newTris.ToArray();

        if (MESH_RENDER == null)
        {
            MESH_RENDER = new Mesh();
            meshFilter.mesh = MESH_RENDER;
        }

        MESH_RENDER.Clear();

        MESH_RENDER.vertices = VERTS_RENDER;
        MESH_RENDER.triangles = TRIS_RENDER;

        MESH_RENDER.RecalculateNormals();
        UpdateColliderMesh();
    }

    public void UpdateColliderMesh()
    {
        meshCollider.sharedMesh = MESH_RENDER;
    }

    public override string ToString()
    {
        return $"Mesh with {verts.Count} verts, {faces.Count} faces, and {TRIS_RENDER.Length / 3} tris.";
    }
    #endregion
}