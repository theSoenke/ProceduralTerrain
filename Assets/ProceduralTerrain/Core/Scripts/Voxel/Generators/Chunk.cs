using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace PCG.Voxel.Generators
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class Chunk : MonoBehaviour
    {
        public bool IsReady
        {
            get { return chunkState == ChunkState.Ready; }
        }

        private MeshCollider meshCollider;
        private MeshFilter meshFilter;
        private ChunkState chunkState;
        private Isosurface isosurface;
        private MeshBuilder meshBuilder;
        private List<ObjectSpawner> spawners;
        private List<PoolObject>[] spawnedObjects;

        public Bounds ChunkBounds { get; private set; }


        private enum ChunkState
        {
            Waiting,
            Ready,
            Destroyed
        }

        private void OnEnable()
        {
            meshCollider = GetComponent<MeshCollider>();
            meshFilter = GetComponent<MeshFilter>();
        }

        /// <summary>
        /// Init chunk and start mesh generation
        /// </summary>
        /// <param name="isosurface"></param>
        /// <param name="spawners"></param>
        /// <param name="chunkSize"></param>
        public void CreateChunk(Isosurface isosurface, List<ObjectSpawner> spawners, int chunkSize)
        {
            this.isosurface = isosurface;
            meshBuilder = isosurface.GetMeshBuilder(transform.position, chunkSize);
            this.spawners = spawners;
            spawnedObjects = new List<PoolObject>[2];

            var center = transform.position;
            center.x += (float)chunkSize / 2;
            center.y += (float)chunkSize / 2;
            center.z += (float)chunkSize / 2;
            ChunkBounds = new Bounds(center, new Vector3(chunkSize, chunkSize, chunkSize));

            BuildChunk();
        }

        public void BuildChunk()
        {
            chunkState = ChunkState.Waiting;

            // Multithreading currently does not work in the editor
            if (Application.isEditor && !Application.isPlaying)
            {
                MeshData meshData = meshBuilder.GenerateMeshData();
                GenerateMesh(meshData);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(BuildMeshDataThreaded, meshBuilder);
            }
        }

        public Bounds GetBounds()
        {
            return ChunkBounds;
        }

        public void Destroy()
        {

            if (chunkState != ChunkState.Destroyed)
            {
                meshFilter.mesh = null;
                meshCollider.sharedMesh = null;
            }

            if (spawners != null)
            {
                for (int i = 0; i < spawners.Count; i++)
                {
                    if (spawners[i] != null && spawnedObjects[i] != null)
                    {
                        spawners[i].DestroyObjects(spawnedObjects[i]);
                    }
                }
            }

            chunkState = ChunkState.Destroyed;
        }

        /// <summary>
        /// Wrapper for CalculateDensities when using ThreadPool
        /// </summary>
        private void BuildMeshDataThreaded(object isosurfaceObj)
        {
            MeshBuilder surfaceBuilder = (MeshBuilder)isosurfaceObj;
            MeshData meshData = surfaceBuilder.GenerateMeshData();

            Loom.QueueOnMainThread(() =>
            {
                if (chunkState == ChunkState.Destroyed)
                {
                    return;
                }

                GenerateMesh(meshData);
            });
        }

        /// <summary>
        /// Generate mesh from mesh data
        /// </summary>
        /// <param name="data">Contains data to build mesh</param>
        private void GenerateMesh(MeshData data)
        {
            chunkState = ChunkState.Ready;
            if (data.vertices.Count == 0)
            {
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            else
            {
                mesh.Clear();
            }

            mesh.SetVertices(data.vertices);
            mesh.SetTriangles(data.triangles, 0);
            mesh.uv = new Vector2[data.vertices.Count];
            mesh.SetNormals(data.normals);


            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;

            gameObject.SetActive(true);

            for (int i = 0; i < spawners.Count; i++)
            {
                spawnedObjects[i] = spawners[i].SpawnObjects(this);
            }
        }
    }
}