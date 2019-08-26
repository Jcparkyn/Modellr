using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System.Linq;

/// <summary>
/// Stores all required data of a single mesh face
/// </summary>
public class Face : ISelectable
{
    public MeshObject parent;
    public HashSet<Edge> edges = new HashSet<Edge> { };
    public int[] tris { get; private set; }
    public bool Selected
    {
        get
        {
            return parent.selFaces.Contains(this);
        }
        set
        {
            if (value)
            {
                parent.selFaces.Add(this);
            }
            else
            {
                parent.selFaces.Remove(this);
            }
        }
    }

    public Vector3 Pos
    {
        get
        {
            return verts.PositionAverage();
        }
    }

    public Vector3 normal
    {
        get
        {
            Vector3 u_ = verts[1].Pos - verts[0].Pos;
            Vector3 v_ = verts[2].Pos - verts[0].Pos;
            Vector3 norm = new Vector3();
            norm.x = u_.y * v_.z - u_.z * v_.y;
            norm.y = u_.z * v_.x - u_.x * v_.z;
            norm.z = u_.x * v_.y - u_.y * v_.x;
            return norm;
        }
    }

    private List<Vertex> verts; //Reference to indexes of MeshObject.vertices
    public List<Vertex> Verts //For get/set vertices of face and updating tris[]
    {
        get
        {
            return verts;
        }

        set
        {
            verts = value;
            SetTris();
        }
    }

    //------------ Constructors:
    public Face(int[] v_, MeshObject parent_ = null)
    {
        if (parent_ == null)
        {
            parent_ = MeshEdit.selMesh;
        }
        //V = v_;
        parent = parent_;
        Verts = (from vID in v_ select parent.verts[vID]).ToList();
    }
    public Face(List<Vertex> v_, MeshObject parent_ = null)
    {
        if (parent_ == null)
        {
            parent_ = MeshEdit.selMesh;
        }
        Verts = v_;
        parent = parent_;
    }
    public Face(int v1, int v2, int v3, int v4, MeshObject parent_ = null)
    {
        if (parent_ == null)
        {
            parent_ = MeshEdit.selMesh;
        }
        int[] v_ = { v1, v2, v3, v4 };
        parent = parent_;
        Verts = (from int vID in v_ select parent.verts[vID]).ToList();
    }

    //------------ Methods:

    //This method triangulates the face and stores the triangles as relative
    //vertex references. This is used in MeshObject.ReconstructMesh(), as the
    //mesh needs to be triangulated before rendering.
    private void SetTris()
    {
        if (verts.Count == 3)
        {
            tris = new int[] { 0, 1, 2 };
        }
        else
        {
            tris = new int[3 * (verts.Count - 2)];
            for (int i = 0; i < verts.Count - 2; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
        }
    }

    public override string ToString()
    {
        string vertNums = "";
        foreach (Vertex vert in verts)
        {
            int num = parent.verts.IndexOf(vert);
            vertNums += num + ", ";
        }
        vertNums = vertNums.Substring(0, vertNums.Length - 2);
        return "Face - verts: " + vertNums;
    }

    public void SelectAdditional()
    {
        if (parent.selFaces.Contains(this))
        {
            parent.selFaces.Remove(this);
        }
        else
        {
            parent.selFaces.Add(this);
        }
    }

    public void SelectAbsolute()
    {
        parent.selFaces = new HashSet<Face> { this };
    }
}
