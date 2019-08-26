using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  This class is used for drawing edges of a mesh when "Render>Edges" is
//  enabled.
public class DrawEdges : MonoBehaviour {

    public Material lineMat;
    public bool doDrawEdges = true;

    public void SetLinesVisible(bool val)
    {
        doDrawEdges = val;
    }

    void OnPostRender()
    {
        if (doDrawEdges)
        {
            GL.PushMatrix();
            
            GL.Begin(GL.LINES);
            lineMat.SetPass(0);

            Color col = new Color(0f, 0f, 0f, 1f);
            foreach (Edge edge in MeshEdit.selMesh.edges.Keys)
            {
                //Debug.Log("Drawing Line");
                GL.Color(col);
                GL.Vertex3(edge.vert1.Pos.x, edge.vert1.Pos.y, edge.vert1.Pos.z);
                GL.Vertex3(edge.vert2.Pos.x, edge.vert2.Pos.y, edge.vert2.Pos.z);
            }

            GL.End();
            GL.PopMatrix(); 
        }
    }
}
