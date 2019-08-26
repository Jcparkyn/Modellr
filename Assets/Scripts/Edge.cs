using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System.Linq;

/// <summary>
/// Stores all required data of a single mesh face. Edge data is only used internally
/// as a cache to accelerate other mesh operations. The edge data is generated during
/// MeshObject.ReconstructMesh()
/// </summary>
public class Edge
{
    public HashSet<Face> connectedFaces = new HashSet<Face> { };
    public Vertex vert1;
    public Vertex vert2;

    #region Constructors:
    //------------ Constructors:
    public Edge(Vertex v1, Vertex v2)
    {
        vert1 = v1;
        vert2 = v2;
    }
    #endregion

    #region Methods:
    //------------ Methods:

    public void DrawDebug()
    {
        Debug.DrawLine(vert1.Pos, vert2.Pos, Color.black);
    }

    public bool OnEdgeOfFaces(IEnumerable<Face> faces, out HashSet<Face> selectedFacesTouchingEdge)
    {
        selectedFacesTouchingEdge = new HashSet<Face>(connectedFaces);
        selectedFacesTouchingEdge.IntersectWith(faces);
        bool result = selectedFacesTouchingEdge.IsProperSubsetOf(connectedFaces) || connectedFaces.Count == 1;
        return result;
    }
    
    //-------------------------------------------------------------------------------------------

    //Method used to check if two edges are equal. Used by HashSet.
    public override bool Equals(object edge2_)
    {
        Edge edge2 = edge2_ as Edge;
        if (object.ReferenceEquals(vert1, edge2.vert1) && object.ReferenceEquals(vert2, edge2.vert2))
        {
            return true;
        }
        if (object.ReferenceEquals(vert1, edge2.vert2) && object.ReferenceEquals(vert2, edge2.vert1))
        {
            return true;
        }
        return false;
    }
    //Method used to generate a hashcode for a given edge. Used by HashSet.
    public override int GetHashCode()
    {
        return vert1.GetHashCode() + vert2.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
    #endregion
}
