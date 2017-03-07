using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Dual Contouring meshing algorithm for uniform grids
    /// </summary>
    public class DualContouringUniform : MeshBuilder
    {
        private const int MaxParticleIterations = 50;
        private const float ToleranceDensity = 1E-3f;
        private const float ToleranceCoord = 1E-3f;

        private readonly Row[] rows = new Row[3];
        private int verticesCount;
        private readonly List<Vector3> vertices;
        private readonly List<Vector3> normals;
        private readonly List<int> triangles;


        public DualContouringUniform(Isosurface isosurface, Vector3 offset, int chunkSize) : base(isosurface, offset, chunkSize)
        {
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            triangles = new List<int>();
        }

        public override MeshData GenerateMeshData()
        {
            Row.size = chunkSize + 3;
            verticesCount = 0;

            Vector3 vRow = Vector3.down;

            // initialize rows
            for (int i = 0; i < 3; i++)
            {
                rows[i] = new Row
                {
                    pos = vRow
                };
                CalculatePoints(rows[i]);
                vRow.y = i;
            }

            for (int y = 3; y <= chunkSize + 2; y++)
            {
                rows[2].pos = vRow;
                vRow.y = y;

                CalculatePoints(rows[2]);
                CalculateCubes(rows[1]);
                GenerateQuads();

                Row tmp = rows[0];
                rows[0] = rows[1];
                rows[1] = rows[2];
                rows[2] = tmp;
            }

            return new MeshData
            {
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };
        }

        /// <summary>
        /// Calculate points in row and generate cubes
        /// </summary>
        /// <param name="row"></param>
        private void CalculatePoints(Row row)
        {
            for (int x = 0; x < Row.size; x++)
            {
                for (int z = 0; z < Row.size; z++)
                {
                    Vector3 pos = new Vector3(row.pos.x + x - 1, row.pos.y, row.pos.z + z - 1);
                    pos += offset;
                    float densityPoint = GetDensity(pos);

                    row.points[x * Row.size + z] = new Voxel
                    {
                        pos = pos,
                        density = densityPoint
                    };
                    row.vertices[x * Row.size + z] = new Vertex();
                }
            }
        }

        /// <summary>
        /// Calculate vector and density for all 8 corners
        /// </summary>
        /// <param name="row"></param>
        private void CalculateCubes(Row row)
        {
            var corners = new Voxel[8];

            for (int x = 0; x < Row.size - 1; x++)
            {
                for (int z = 0; z < Row.size - 1; z++)
                {
                    int cubeIndex = 0;

                    // Find intersection point with surface for each edge
                    for (int i = 0; i < 8; i++)
                    {
                        int pointX = x + Tables.VertexOffset[i, 0];
                        int pointZ = z + Tables.VertexOffset[i, 2];

                        corners[i] = rows[Tables.VertexOffset[i, 1]].points[pointX * Row.size + pointZ];

                        if (corners[i].density < 0)
                        {
                            cubeIndex |= 1 << i;
                        }
                    }

                    Vertex vertex = row.vertices[x * Row.size + z];
                    vertex.edgeFlags = Tables.EdgeTable[cubeIndex];

                    // No intersection if cube is complety outside surface
                    if (vertex.edgeFlags == 0)
                    {
                        vertex.index = 0;
                        continue;
                    }
                    vertex.index = cubeIndex;
                    GenerateVertex(vertex, corners);
                }
            }
        }

        /// <summary>
        /// Generate vertices and normals for cube
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="corners"></param>
        private void GenerateVertex(Vertex vertex, Voxel[] corners)
        {
            var data = new HermiteData
            {
                intersectionPoints = new List<Vector3>(),
                gradientVectors = new List<Vector3>()
            };

            // Find the point of intersection of the surface in each of the 12 edges
            for (int i = 0; i < 12; i++)
            {
                int n1 = Tables.EdgeConnection[i, 0];
                int n2 = Tables.EdgeConnection[i, 1];

                if ((vertex.edgeFlags & (1 << i)) == 0)
                {
                    continue;
                }

                if (Mathf.Abs(corners[n1].density) < ToleranceDensity)
                {
                    data.intersectionPoints.Add(corners[n1].pos);
                }
                else if (Mathf.Abs(corners[n2].density) < ToleranceDensity)
                {
                    data.intersectionPoints.Add(corners[n2].pos);
                }
                else
                {
                    Vector3 vDiff = corners[n1].pos - corners[n2].pos;
                    if (Mathf.Abs(vDiff.x) > ToleranceDensity)
                    {
                        Vector3 pos = IntersectXAxis(corners[n1], corners[n2]);
                        data.intersectionPoints.Add(pos);
                    }
                    if (Mathf.Abs(vDiff.y) > ToleranceDensity)
                    {
                        Vector3 pos = IntersectYAxis(corners[n1], corners[n2]);
                        data.intersectionPoints.Add(pos);
                    }
                    if (Mathf.Abs(vDiff.z) > ToleranceDensity)
                    {
                        Vector3 pos = IntersectZAxis(corners[n1], corners[n2]);
                        data.intersectionPoints.Add(pos);
                    }
                }

                Vector3 normal = GetNormal(data.intersectionPoints[data.intersectionPoints.Count - 1]);
                data.gradientVectors.Add(normal);
            }

            vertex.pos = SchmitzVertexFromHermiteData(data, 0.001f);
            vertex.normal = GetNormal(vertex.pos);
            vertex.pos -= offset;
        }

        /// <summary>
        /// Generate triangles for row
        /// </summary>
        private void GenerateQuads()
        {
            for (int x = 1; x < chunkSize + 1; x++)
            {
                for (int z = 1; z < chunkSize + 1; z++)
                {
                    var tmpVertices = new Vertex[4];
                    tmpVertices[0] = GetVertexPointer(x, z, 0, 0, 0);

                    for (int i = 0; i < 3; i++)
                    {
                        bool windingOrder;
                        if (i == 0 && (tmpVertices[0].edgeFlags & (1 << 10)) == 1 << 10)
                        {
                            tmpVertices[1] = GetVertexPointer(x, z, 1, 0, 0);
                            tmpVertices[2] = GetVertexPointer(x, z, 1, 1, 0);
                            tmpVertices[3] = GetVertexPointer(x, z, 0, 1, 0);
                            windingOrder = (tmpVertices[0].index & (1 << 6)) == 1 << 6;
                        }
                        else if (i == 1 && (tmpVertices[0].edgeFlags & (1 << 6)) == 1 << 6)
                        {
                            tmpVertices[1] = GetVertexPointer(x, z, 0, 0, 1);
                            tmpVertices[2] = GetVertexPointer(x, z, 0, 1, 1);
                            tmpVertices[3] = GetVertexPointer(x, z, 0, 1, 0);
                            windingOrder = (tmpVertices[0].index & (1 << 7)) == 1 << 7;
                        }
                        else if (i == 2 && (tmpVertices[0].edgeFlags & (1 << 5)) == 1 << 5)
                        {
                            tmpVertices[1] = GetVertexPointer(x, z, 1, 0, 0);
                            tmpVertices[2] = GetVertexPointer(x, z, 1, 0, 1);
                            tmpVertices[3] = GetVertexPointer(x, z, 0, 0, 1);
                            windingOrder = (tmpVertices[0].index & (1 << 5)) == 1 << 5;
                        }
                        else
                        {
                            continue;
                        }

                        var triangle = new Vertex[3];
                        triangle[0] = tmpVertices[0];

                        for (int j = 1; j < 3; j++)
                        {
                            int ja = windingOrder ? j : j + 1;
                            int jb = windingOrder ? j + 1 : j;

                            triangle[1] = tmpVertices[ja];
                            triangle[2] = tmpVertices[jb];

                            AddTriangle(triangle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create new triangle
        /// </summary>
        /// <param name="triangle">Vertices for new triangle</param>
        private void AddTriangle(Vertex[] triangle)
        {
            for (int i = 0; i < 3; i++)
            {
                vertices.Add(triangle[i].pos);
                normals.Add(triangle[i].normal);
                triangles.Add(verticesCount + Tables.WindingOrder[i]);
            }

            verticesCount += 3;
        }

        /// <summary>
        ///  Gets vertex pointer in the cubes of the row
        /// </summary>
        /// <returns>Vertex pointer</returns>
        private Vertex GetVertexPointer(int x, int z, int xi, int yi, int zi)
        {
            int pointX = x + xi;
            int pointZ = z + zi;

            return rows[yi].vertices[pointX * Row.size + pointZ];
        }

        private Vector3 GetNormal(Vector3 p)
        {
            return GetNormal(p.x, p.y, p.z);
        }

        /// <summary>
        /// Calculate normal for point
        /// </summary>
        /// <returns>Normal</returns>
        private Vector3 GetNormal(float x, float y, float z)
        {
            Vector3 grad = new Vector3
            {
                x = GetDensity(x - 1, y, z) - GetDensity(x + 1, y, z),
                y = GetDensity(x, y - 1, z) - GetDensity(x, y + 1, z),
                z = GetDensity(x, y, z - 1) - GetDensity(x, y, z + 1)
            };

            return Vector3.Normalize(-grad);
        }

        /// <summary>
        /// Calculates an approximated vertex for a row.
        /// Based on the algorithm described in the paper "Analysis and Acceleration of High Quality Isosurface Contouring".
        /// </summary>
        /// <param name="data">The hermite data for a row</param>
        /// <param name="threshold">When has a force has a value below it will return the approximated position</param>
        /// <returns>Approximated vertex for the row</returns>
        private static Vector3 SchmitzVertexFromHermiteData(HermiteData data, float threshold)
        {
            threshold *= threshold;

            List<Vector3> xPoints = data.intersectionPoints;
            List<Vector3> grads = data.gradientVectors;
            int pointsCount = xPoints.Count;

            if (pointsCount == 0)
            {
                return new Vector3();
            }

            // start mass point
            // calculated by mean of intersection points
            Vector3 c = new Vector3();
            for (int i = 0; i < pointsCount; i++)
            {
                c += xPoints[i];
            }
            c /= pointsCount;

            for (int i = 0; i < MaxParticleIterations; i++)
            {
                // force that acts on mass
                Vector3 force = new Vector3();

                for (int j = 0; j < pointsCount; j++)
                {
                    Vector3 xPoint = xPoints[j];
                    Vector3 xNormal = grads[j];

                    force += xNormal * -1 * Vector3.Dot(xNormal, c - xPoint);
                }

                // dampen force
                float damping = 1 - (float)i / MaxParticleIterations;
                c += force * damping / pointsCount;

                if (force.sqrMagnitude < threshold)
                {
                    break;
                }
            }

            return c;
        }

        /// <summary>
        /// Interpolate vertex offset for an edge on the X axis
        /// </summary>
        /// <returns>Interpolated vector</returns>
        private Vector3 IntersectXAxis(Voxel p0, Voxel p1)
        {
            float xa, xb;

            if (p0.density < 0)
            {
                xa = p0.pos.x;
                xb = p1.pos.x;
            }
            else
            {
                xa = p1.pos.x;
                xb = p0.pos.x;
            }

            float y = p0.pos.y;
            float z = p0.pos.z;
            float xm;

            while (true)
            {
                xm = (xa + xb) * 0.5f;
                float d = GetDensity(xm, y, z);

                if (Mathf.Abs(d) < ToleranceDensity)
                {
                    break;
                }
                if (Mathf.Abs(xa - xb) < ToleranceCoord)
                {
                    break;
                }

                if (d < 0)
                {
                    xa = xm;
                }
                else
                {
                    xb = xm;
                }
            }

            return new Vector3(xm, y, z);
        }

        /// <summary>
        /// Interpolate vertex offset for an edge on the Y axis
        /// </summary>
        /// <returns>Interpolated vector</returns>
        private Vector3 IntersectYAxis(Voxel p0, Voxel p1)
        {
            float ya, yb;

            if (p0.density < 0)
            {
                ya = p0.pos.y;
                yb = p1.pos.y;
            }
            else
            {
                ya = p1.pos.y;
                yb = p0.pos.y;
            }

            float x = p0.pos.x;
            float z = p0.pos.z;
            float ym;

            while (true)
            {
                ym = (ya + yb) * 0.5f;
                float d = GetDensity(x, ym, z);

                if (Mathf.Abs(d) < ToleranceDensity)
                {
                    break;
                }
                if (Mathf.Abs(ya - yb) < ToleranceCoord)
                {
                    break;
                }

                if (d < 0)
                {
                    ya = ym;
                }
                else
                {
                    yb = ym;
                }
            }

            return new Vector3(x, ym, z);
        }

        /// <summary>
        /// Interpolate vertex offset for an edge on the Z axis
        /// </summary>
        /// <returns>Interpolated vector</returns>
        private Vector3 IntersectZAxis(Voxel p0, Voxel p1)
        {
            float za, zb;

            if (p0.density < 0)
            {
                za = p0.pos.z;
                zb = p1.pos.z;
            }
            else
            {
                za = p1.pos.z;
                zb = p0.pos.z;
            }

            float x = p0.pos.x;
            float y = p0.pos.y;
            float zm;

            while (true)
            {
                zm = (za + zb) * 0.5f;
                float d = GetDensity(x, y, zm);

                if (Mathf.Abs(d) < ToleranceDensity)
                {
                    break;
                }
                if (Mathf.Abs(za - zb) < ToleranceCoord)
                {
                    break;
                }

                if (d < 0)
                {
                    za = zm;
                }
                else
                {
                    zb = zm;
                }
            }

            return new Vector3(x, y, zm);
        }
    }
}
