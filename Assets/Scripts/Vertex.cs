using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System.Linq;

/// <summary>
/// Stores all required data of a single mesh vertex
/// </summary>
public class Vertex : ISelectable
{
    //Reference to the parent MeshObject of this Vertex
    private MeshObject parent = MeshEdit.selMesh;
    public Vector3 Pos { get; set; }

    //Caches a reference to the rendered vert indexes to allow for more
    //efficient transformations of vertices (without having to re-generate the
    //entire mesh)
    public List<int> renderVerts = new List<int> { };

    //Property used to get/set whether vertex is selected.
    public bool Selected
    {
        get
        {
            return parent.selVerts.Contains(this);
        }
        set
        {
            if (value)
            {
                parent.selVerts.Add(this);
            }
            else
            {
                parent.selVerts.Remove(this);
            }
        }
    }

    public HashSet<Face> connectedFaces = new HashSet<Face> { };

    #region Constructors:
    //------------ Constructors:
    public Vertex(Vector3 pos_)
    {
        parent = MeshEdit.selMesh;
        Pos = pos_;
    }
    public Vertex(Vector3 pos_, MeshObject parent_)
    {
        parent = parent_;
        Pos = pos_;
    }
    public Vertex(float x, float y, float z)
    {
        parent = MeshEdit.selMesh;
        Pos = new Vector3(x, y, z);
    }
    public Vertex(float x, float y, float z, MeshObject parent_)
    {
        parent = parent_;
        Pos = new Vector3(x, y, z);
    }
    #endregion

    #region Methods:
    //------------ Methods:
    public Vertex Clone()
    {
        Vertex newVert = new Vertex(Pos, parent);
        return newVert;
    }

    public void SelectAdditional()
    {
        if (parent.selVerts.Contains(this))
        {
            parent.selVerts.Remove(this);
        }
        else
        {
            parent.selVerts.Add(this);
        }
    }

    public void SelectAbsolute()
    {
        parent.selVerts = new HashSet<Vertex> { this };
    }

    public override string ToString()
    {
        return "Vert: " + Pos.ToString() + ", " + (Selected ? "selected" : "not selected");
    }

    public int GetIndex()
    {
        return parent.verts.IndexOf(this);
    }

    public bool OnEdgeOfFaces(IEnumerable<Face> faces, out HashSet<Face> selectedFacesTouchingVert)
    {
        selectedFacesTouchingVert = new HashSet<Face>(connectedFaces);
        selectedFacesTouchingVert.IntersectWith(faces);
        bool result = selectedFacesTouchingVert.IsProperSubsetOf(connectedFaces);
        return result;
    }
    public bool OnEdgeOfFaces(IEnumerable<Face> faces)
    {
        HashSet<Face> selectedFacesTouchingVert;
        return OnEdgeOfFaces(faces, out selectedFacesTouchingVert);
    }
    #endregion
}