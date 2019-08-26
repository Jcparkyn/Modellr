using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ExtensionMethods;

/*  This class is used to generate the default meshes for the "Insert Mesh"
 *  dropdown.
 */
public static class DefaultMeshes {

	public static MeshObject Cube(MeshObject parent = null)
    {
        if (parent == null)
        {
            parent = new MeshObject(MeshEdit.meshFilter);
        }
        MeshObject meshTemp = new MeshObject(MeshEdit.meshFilter);
        List<Vertex> defaultVertices = new List<Vertex>
        {
            new Vertex(-1, -1, -1, parent),
            new Vertex(-1, -1,  1, parent),
            new Vertex( 1, -1,  1, parent),
            new Vertex( 1, -1, -1, parent),
            new Vertex(-1,  1, -1, parent),
            new Vertex(-1,  1,  1, parent),
            new Vertex( 1,  1,  1, parent),
            new Vertex( 1,  1, -1, parent),
        };
        meshTemp.verts = defaultVertices;
        List<Face> defaultFaces = new List<Face>
        {
            new Face(new int[] { 0, 1, 2, 3 }, meshTemp),
            new Face(new int[] { 4, 5, 6, 7 }, meshTemp),
            new Face(new int[] { 0, 1, 5, 4 }, meshTemp),
            new Face(new int[] { 2, 3, 7, 6 }, meshTemp),
            new Face(new int[] { 0, 4, 7, 3 }, meshTemp),
            new Face(new int[] { 1, 2, 6, 5 }, meshTemp)
        };

        foreach(Face face in defaultFaces)
        {
            face.parent = parent;
        }

        parent.verts.AddRange(defaultVertices);
        parent.faces.AddRange(defaultFaces);

        parent.selVerts = new HashSet<Vertex>(defaultVertices);
        parent.selFaces = new HashSet<Face>(defaultFaces);

        return parent;
    }

    public static MeshObject Plane(MeshObject parent = null)
    {
        if (parent == null)
        {
            parent = new MeshObject(MeshEdit.meshFilter);
        }
        List<Vertex> defaultVertices = new List<Vertex>
        {
            new Vertex(-1, 0, -1, parent),
            new Vertex(-1, 0, 1, parent),
            new Vertex(1, 0, 1, parent),
            new Vertex(1, 0, -1, parent)
        };
        List<Face> defaultFaces = new List<Face>
        {
            new Face(defaultVertices, parent)
        };

        parent.verts.AddRange(defaultVertices);
        parent.faces.AddRange(defaultFaces);

        parent.selVerts = new HashSet<Vertex>(defaultVertices);
        parent.selFaces = new HashSet<Face>(defaultFaces);

        return parent;
    }

    public static MeshObject Sphere(MeshObject parent = null)
    {
        if (parent == null)
        {
            parent = new MeshObject(MeshEdit.meshFilter);
        }
        List<Vertex> defaultVertices = new List<Vertex>{};
        int resU = 16;
        int resV = 8;
        for(int u = 0; u < resU; u++)
        {
            for (int v = 1; v < resV; v++)
            {
                float sinV = Mathf.Sin(1f * v / resV * Mathf.PI);
                float x = Mathf.Sin(1f * u / resU * 2 * Mathf.PI) * sinV;
                float y = Mathf.Cos(1f * v / resV * Mathf.PI);
                float z = Mathf.Cos(1f * u / resU * 2 * Mathf.PI) * sinV;
                defaultVertices.Add(new Vertex(x, y, z, parent));
            }
        }
        defaultVertices.Add(new Vertex(0, 1, 0, parent));
        defaultVertices.Add(new Vertex(0, -1, 0, parent));

        List<Face> defaultFaces = new List<Face>{};
        
        for (int i = 0; i < (resU) * (resV - 1) - 1; i++)
        {
            if ((i + 1) % (resV - 1) != 0)
            {
                List<Vertex> verts = new List<Vertex>
                {
                    defaultVertices[i],
                    defaultVertices[i + 1],
                    defaultVertices[(i + resV) % ((resU) * (resV - 1))],
                    defaultVertices[(i + resV - 1) % ((resU) * (resV - 1))],
                };
                defaultFaces.Add(new Face(verts, parent));
            }
        }
        for (int i = 0; i <= resU; i++)
        {
            List<Vertex> vertsTop = new List<Vertex>
            {
                defaultVertices[defaultVertices.Count - 2],
                defaultVertices[i * (resV - 1)],
                defaultVertices[((i + 1) % resU) * (resV - 1)],
            };
            List<Vertex> vertsBottom = new List<Vertex>
            {
                defaultVertices[defaultVertices.Count - 1],
                defaultVertices[(i * (resV - 1) + resV - 2) % (defaultVertices.Count - 2)],
                defaultVertices[((i + 1) % resU) * (resV - 1) + resV - 2],
            };
            defaultFaces.Add(new Face(vertsTop, parent));
            defaultFaces.Add(new Face(vertsBottom, parent));

        }

        parent.verts.AddRange(defaultVertices);
        parent.faces.AddRange(defaultFaces);

        parent.selVerts = new HashSet<Vertex>(defaultVertices);
        parent.selFaces = new HashSet<Face>(defaultFaces);

        return parent;
    }

    public static MeshObject Circle(MeshObject parent = null)
    {
        if (parent == null)
        {
            parent = new MeshObject(MeshEdit.meshFilter);
        }
        List<Vertex> defaultVertices = new List<Vertex> { };
        int res = 24;
        for (int i = 1; i <= res; i++)
        {
            //float sinV = Mathf.Sin(1f * v / resV * Mathf.PI);
            float x = Mathf.Sin(1f * i / res * 2 * Mathf.PI);// * sinV;
            //float y = Mathf.Cos(1f * v / resV * Mathf.PI);
            float z = Mathf.Cos(1f * i / res * 2 * Mathf.PI);// * sinV;
            defaultVertices.Add(new Vertex(x, 0, z, parent));
        }
        List<Face> defaultFaces = new List<Face> { new Face(defaultVertices, parent) };

        parent.verts.AddRange(defaultVertices);
        parent.faces.AddRange(defaultFaces);

        parent.selVerts = new HashSet<Vertex>(defaultVertices);
        parent.selFaces = new HashSet<Face>(defaultFaces);

        return parent;
    }
}
