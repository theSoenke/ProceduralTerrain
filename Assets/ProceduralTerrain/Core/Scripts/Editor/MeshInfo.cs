using UnityEditor;
using UnityEngine;

public class MeshInfo : ScriptableObject
{
    [MenuItem("Tools/Mesh Info")]
    public static void ShowMeshInfo()
    {
        int triangles = 0;
        int vertices = 0;
        int meshCount = 0;

        foreach (GameObject go in Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel))
        {
            Component[] meshes = go.GetComponentsInChildren(typeof(MeshFilter));

            foreach (MeshFilter mesh in meshes)
            {
                if (mesh.sharedMesh)
                {
                    vertices += mesh.sharedMesh.vertexCount;
                    triangles += mesh.sharedMesh.triangles.Length / 3;
                    meshCount++;
                }
            }
        }

        string average = (meshCount > 0 ? (" Average of " + vertices / meshCount + " vertices and " + triangles / meshCount + " triangles per mesh.") : "");

        Debug.Log("Vertices: " + vertices +
            " Triangles: " + triangles +
            " Meshes: " + meshCount +
            average);
    }
}