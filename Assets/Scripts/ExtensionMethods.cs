using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


//This class is used to store extension methods to act on collections of verts,
//faces etc.
namespace ExtensionMethods
{
    public static class ExtensionMethods {

        public static HashSet<Vertex> GetVerts(this IEnumerable<Face> faces)
        {
            HashSet<Vertex> vertsTotal = new HashSet<Vertex> { };
            foreach (Face face in faces)
            {
                foreach (Vertex vert in face.Verts)
                {
                    vertsTotal.Add(vert);
                }
                //vertsTotal.AddRange(face.V); //TODO: Optimise to only calculate once
            }
            return vertsTotal;
        }

        public static HashSet<Vertex> GetVerts(this IEnumerable<Edge> edges)
        {
            HashSet<Vertex> vertsTotal = new HashSet<Vertex> { };
            foreach (Edge edge in edges)
            {
                vertsTotal.Add(edge.vert1);
                vertsTotal.Add(edge.vert2);
            }
            return vertsTotal;
        }

        public static HashSet<Edge> GetEdges(this IEnumerable<Face> faces)
        {
            HashSet<Edge> edgesTotal = new HashSet<Edge> { };
            foreach (Face face in faces)
            {
                foreach (Edge edge in face.edges)
                {
                    edgesTotal.Add(edge);
                }
                //vertsTotal.AddRange(face.V); //TODO: Optimise to only calculate once
            }
            return edgesTotal;
        }

        public static HashSet<Edge> GetBorderingEdges(this IEnumerable<Face> faces)
        {
            HashSet<Edge> edgesTotal = new HashSet<Edge> { };
            foreach (Face face in faces)
            {
                foreach (Edge edge in face.edges)
                {
                    
                    if (edge.connectedFaces.Intersect(faces).Count() == 1)
                    {
                        edgesTotal.Add(edge); 
                    }
                }
            }
            return edgesTotal;
        }

        public static HashSet<Face> GetTouchingFaces(this IEnumerable<Vertex> verts)
        {
            HashSet<Face> facesTotal = new HashSet<Face> { };
            foreach (Vertex vert in verts)
            {
                foreach (Face face in vert.connectedFaces)
                {
                    facesTotal.Add(face);
                }
            }
            return facesTotal;
        }

        public static Vector3 PositionAverage<T>(this IEnumerable<T> points) where T : ISelectable
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (T vert in points)
            {
                Vector3 point = vert.Pos;
                sum.x += point.x;
                sum.y += point.y;
                sum.z += point.z;
                count += 1;
            }
            sum /= count;
            return sum;
        }

        public static Vector3 NormalAverage(this IEnumerable<Face> faces)
        {
            Vector3 posAvg = PositionAverage(faces);
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (Face face in faces)
            {
                Vector3 point = face.normal;
                float angleFromCenter = Vector3.Angle(point, posAvg - face.Pos);
                int mult = 1;
                if (angleFromCenter < 89)
                {
                    mult = -1;
                }
                //float mult = Vector3.Dot(sum, point);
                sum.x += point.x * mult;
                sum.y += point.y * mult;
                sum.z += point.z * mult;
                count += 1;
            }
            sum /= count;
            return sum;
        }

        public static string ListToString<T>(this List<T> list)
        {
            string output = "";
            foreach (T item in list)
            {
                output += item.ToString() + ", ";
            }
            return output.Remove(output.Length - 1);
        }
    }
}
