using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Data for mesh creation
    /// </summary>
    public class MeshData
    {
        public List<int> triangles;
        public List<Vector3> vertices;
        public List<Vector3> normals;
    }

    /// <summary>
    /// Hermite data structure
    /// </summary>
    public class HermiteData
    {
        public List<Vector3> intersectionPoints = new List<Vector3>();
        public List<Vector3> gradientVectors = new List<Vector3>();
    }

    /// <summary>
    /// Voxel data
    /// </summary>
    public class Voxel
    {
        public Vector3 pos;
        public float density;
    }

    /// <summary>
    /// Vertex data
    /// </summary>
    public class Vertex
    {
        public int index = 0;
        public int edgeFlags;
        public Vector3 pos;
        public Vector3 normal;
    }

    /// <summary>
    /// Y-axis cut of hermite data
    ///  </summary>
    public class Row
    {
        public static int size;
        public Vector3 pos;
        public readonly Voxel[] points = new Voxel[size * size];
        public readonly Vertex[] vertices = new Vertex[size * size];
    }
}
