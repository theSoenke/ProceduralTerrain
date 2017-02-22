using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Marching Cubes meshing algorithm
    /// </summary>
    public class MarchingCubes : MeshBuilder
    {
        private const float Target = 0; // The value that represents the surface of mesh
        private const float NormalSmoothing = 90;


        public MarchingCubes(Isosurface isosurface, Vector3 offset, int chunkSize) : base(isosurface, offset, chunkSize + 1)
        {
        }

        public override MeshData GenerateMeshData()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float[,,] voxels = CalculateDensities();

            for (int x = 0; x < chunkSize - 1; x++)
            {
                for (int y = 0; y < chunkSize - 1; y++)
                {
                    for (int z = 0; z < chunkSize - 1; z++)
                    {
                        float[] cube = CreateCube(x, y, z, voxels);
                        MarchCube(new Vector3(x, y, z), cube, vertices, triangles);
                    }
                }
            }

            List<Vector3> normals = NormalSolver.RecalculateNormals(triangles, vertices, NormalSmoothing);

            return new MeshData
            {
                vertices = vertices,
                triangles = triangles,
                normals = normals
            };
        }

        /// <summary>
        /// Calculate densities for voxel array
        /// </summary>
        /// <returns>Voxel values</returns>
        private float[,,] CalculateDensities()
        {
            var voxels = new float[chunkSize, chunkSize, chunkSize];

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        pos += offset;
                        float density = GetDensity(pos);
                        voxels[x, y, z] = density;
                    }
                }
            }

            return voxels;
        }

        /// <summary>
        /// Get the values 8 neighbor values of the cube
        /// </summary>
        private static float[] CreateCube(int x, int y, int z, float[,,] voxels)
        {
            var cube = new float[8];

            for (int i = 0; i < 8; i++)
            {
                cube[i] = voxels[x + Tables.VertexOffset[i, 0], y + Tables.VertexOffset[i, 1], z + Tables.VertexOffset[i, 2]];
            }

            return cube;
        }

        /// <summary>
        /// Find the point of intersection of the surface between points with values v1 and v2 
        /// </summary>
        private static float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;

            if (Mathf.Abs(delta) < 0.0001f)
            {
                return 0.5f;
            }
            return (Target - v1) / delta;
        }

        /// <summary>
        /// Performs the Marching Cubes algorithm on a single cube
        /// </summary>
        private static void MarchCube(Vector3 pos, float[] cube, List<Vector3> vertList, List<int> indexList)
        {
            int cubeIndex = 0;
            var edgeVertex = new Vector3[12];

            // Find vertices inside the surface
            for (int i = 0; i < 8; i++)
            {
                if (cube[i] <= Target)
                {
                    cubeIndex |= 1 << i;
                }
            }

            // Find edges intersected by surface
            int edgeFlags = Tables.EdgeTable[cubeIndex];

            // No intersection if cube is complety outside surface
            if (edgeFlags == 0)
            {
                return;
            }

            // Find intersection point with surface for each edge
            for (int i = 0; i < 12; i++)
            {
                // When intersection for this edge exists
                if ((edgeFlags & (1 << i)) != 0)
                {
                    float offset = GetOffset(cube[Tables.EdgeConnection[i, 0]], cube[Tables.EdgeConnection[i, 1]]);

                    edgeVertex[i].x = pos.x + (Tables.VertexOffset[Tables.EdgeConnection[i, 0], 0] + offset * Tables.EdgeDirection[i, 0]);
                    edgeVertex[i].y = pos.y + (Tables.VertexOffset[Tables.EdgeConnection[i, 0], 1] + offset * Tables.EdgeDirection[i, 1]);
                    edgeVertex[i].z = pos.z + (Tables.VertexOffset[Tables.EdgeConnection[i, 0], 2] + offset * Tables.EdgeDirection[i, 2]);
                }
            }

            // Store found triangles. Up to five per cube possible
            for (int i = 0; i < 5; i++)
            {
                // Stop when triangle list terminates with -1
                if (Tables.TriTable[cubeIndex, 3 * i] < 0)
                {
                    break;
                }

                int idx = vertList.Count;

                for (int j = 0; j < 3; j++)
                {
                    int vert = Tables.TriTable[cubeIndex, 3 * i + j];
                    indexList.Add(idx + Tables.WindingOrder[j]);
                    vertList.Add(edgeVertex[vert]);
                }
            }
        }
    }
}
