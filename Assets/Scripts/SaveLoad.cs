using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ExtensionMethods;
using System.Runtime.InteropServices;
using UnityEngine.Networking;


/*  This class is used to save and load MeshObjects to memory, and to generate
 *  and save OBJ files.
 */


public static class SaveLoad {

    //File path for saves on Windows:
    //C:\Users\%USERNAME%\AppData\LocalLow\DefaultCompany\Modellr
    public static string filePath = Application.persistentDataPath + "/mesh01.dat";

    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

    [DllImport("__Internal")]
    private static extern void RequestFile();

    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public static void LoadLastMeshSavePath()
    {
        string str = PlayerPrefs.GetString("savePath", "mesh01");
        filePath = Application.persistentDataPath + "/" + str;
        GameObject.Find("InputField_SavePath").GetComponent<InputField>().text = PlayerPrefs.GetString("savePath", "mesh01");
    }

    public static void SetMeshSavePath(string str)
    {
        filePath = Application.persistentDataPath + "/" + str;
        PlayerPrefs.SetString("savePath", str);
        PlayerPrefs.Save();
    }

    public static void SaveMesh(MeshObject mesh)
    {
        MeshObjectSerializable meshCopy = new MeshObjectSerializable(mesh);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(GetFullFilePath(), FileMode.Create);
        bf.Serialize(file, meshCopy);
        file.Close();
    }

    public static void RequestObj()
    {
        UploadFile("Mesh", "OnObjUploaded", ".obj", false);
    }

    public static MeshObject LoadMesh(MeshFilter meshFilter, MeshCollider meshCollider)
    {
        if (File.Exists(GetFullFilePath()))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(GetFullFilePath());
            MeshObjectSerializable mesh_ = (MeshObjectSerializable)bf.Deserialize(file);
            MeshObject mesh = mesh_.ToMeshObject(meshFilter, meshCollider);
            return mesh;
        }
        else
        {
            Debug.Log($"Cannot load because file {GetFullFilePath()} does not exist");
        }
        return null;
    }

    public static void ExportOBJ(MeshObject mesh)
    {
        MeshObjectSerializable meshCopy = new MeshObjectSerializable(mesh);
        //StreamWriter writer = new StreamWriter(filePath + ".obj", false);
        //writer.
        StringBuilder exportText = new StringBuilder();
        foreach(VertexSerializable vert in meshCopy.vertices)
        {
            string line = $"v {vert.pos.x} {vert.pos.y} {vert.pos.z}";
            exportText.AppendLine(line);
        }
        foreach(FaceSerializable face in meshCopy.faces)
        {
            string line = $"f";
            foreach(int index in face.verts)
            {
                line += " " + (index + 1);
            }
            exportText.AppendLine(line);
        }
        byte[] objBytes = ASCIIEncoding.ASCII.GetBytes(exportText.ToString());
        DownloadFile(objBytes, objBytes.Length, "model.obj");
        //writer.Close();
    }

    public static string GetFullFilePath()
    {
        return filePath + ".dat";
    }
}


//MeshObjectSerializable is a struct used to store a simplified version of a
//MeshObject that can be stored in memory.
[System.Serializable]
public struct MeshObjectSerializable
{
    public List<VertexSerializable> vertices;
    public List<FaceSerializable> faces;
    public int[] selVerts;
    public int[] selFaces;

    //Constructor to convert MeshObject to MeshObjectSerializable
    public MeshObjectSerializable(MeshObject mesh)
    {
        Dictionary<Vertex, int> vertIndices = new Dictionary<Vertex, int>(mesh.verts.Count);
        for (int i = 0; i < mesh.verts.Count; i++)
        {
            vertIndices.Add(mesh.verts[i], i);
        }
        Dictionary<Face, int> faceIndices = new Dictionary<Face, int>(mesh.faces.Count);
        for (int i = 0; i < mesh.faces.Count; i++)
        {
            faceIndices.Add(mesh.faces[i], i);
        }

        vertices = mesh.verts.Select(vert => new VertexSerializable(vert)).ToList();
        selVerts = mesh.selVerts.Select(vert => vertIndices[vert]).ToArray();
        selFaces = mesh.selFaces.Select(face => faceIndices[face]).ToArray();

        faces = new List<FaceSerializable>(mesh.faces.Count);
        foreach(Face face in mesh.faces)
        {
            FaceSerializable newFace = new FaceSerializable();
            newFace.verts = new int[face.Verts.Count];
            for (int i = 0; i < face.Verts.Count; i++)
            {
                newFace.verts[i] = vertIndices[face.Verts[i]];
            }
            faces.Add(newFace);
        }

    }

    public MeshObject ToMeshObject(MeshFilter _meshFilter, MeshCollider meshCollider)
    {
        MeshObject mesh = new MeshObject(_meshFilter);
        mesh.meshCollider = meshCollider;
        mesh.verts = vertices.Select(vert => new Vertex(vert.pos, mesh)).ToList();
        mesh.faces = faces.Select(face => new Face(face.verts, mesh)).ToList(); //new List<Face> { };

        if (selVerts != null && selFaces != null)
        {
            foreach (int vertID in selVerts)
            {
                mesh.selVerts.Add(mesh.verts[vertID]);
            }
            foreach (int faceID in selFaces)
            {
                mesh.selFaces.Add(mesh.faces[faceID]);
            }
        }

        //mesh.meshFilter = meshFilter_;
        mesh.ReconstructMesh();
        return mesh;
    }
}

//Serializable versions of Face and Vertex:
[System.Serializable]
public struct FaceSerializable
{
    public int[] verts;
}

[System.Serializable]
public struct VertexSerializable
{
    public float x;
    public float y;
    public float z;

    public Vector3 pos
    {
        get
        {
            return new Vector3(x, y, z);
        }
    }

    public VertexSerializable(Vertex vert)
    {
        x = vert.Pos.x;
        y = vert.Pos.y;
        z = vert.Pos.z;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }
}
