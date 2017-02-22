using System.Collections.Generic;
using UnityEngine;
using StopWatch = System.Diagnostics.Stopwatch;


namespace PCG.Voxel.Generators
{
    public class ChunkGenerator : GridGenerator
    {
        public bool visualizeBounds;

        private List<ObjectSpawner> spawners;
        private readonly int maxChunksPerFrame;
        private readonly Queue<Chunk> chunksToGenerate;
        private bool isBaseTerrainGenerated;
        private readonly TerrainSettings terrainSettings;
        private readonly Density density;


        public ChunkGenerator(TerrainSettings terrainSettings, Density density, ObjectPool chunkPool, List<ObjectSpawner> spawners, int maxChunksPerFrame) : base(chunkPool)
        {
            this.terrainSettings = terrainSettings;
            this.density = density;
            this.spawners = spawners;
            this.maxChunksPerFrame = maxChunksPerFrame;
            chunksToGenerate = new Queue<Chunk>();
        }

        public override void Update(Vector3 playerPos)
        {
            LoadObjects(playerPos, terrainSettings.viewDistance, terrainSettings.chunkSize);
            UnloadObjects(playerPos, terrainSettings.viewDistance + terrainSettings.chunkSize);

            for (int i = 0; i < maxChunksPerFrame; i++)
            {
                BuildChunkFromQueue();
            }
        }

        public bool IsBaseTerrainReady()
        {
            if (isBaseTerrainGenerated)
            {
                return true;
            }

            foreach (var chunkGameObject in loadedObjects)
            {
                Chunk chunk = chunkGameObject.Value.GetComponent<Chunk>();

                if (chunk.IsReady) { continue; }
                return false;
            }
            isBaseTerrainGenerated = true;
            return true;
        }

        /// <summary>
        /// Generate terrain with fixed rectangular size
        /// </summary>
        public void GenerateStaticTerrain()
        {
            StopWatch watch = new StopWatch();
            watch.Start();

            Vector2 worldSize = terrainSettings.worldSize;
            int chunkSize = terrainSettings.chunkSize;
            int verticalChunks = terrainSettings.maxHeight / chunkSize;

            int maxX = (int)worldSize.x / chunkSize;
            int minY = -(verticalChunks / 2);
            int maxY = verticalChunks / 2;
            int maxZ = (int)worldSize.y / chunkSize;

            for (int x = 0; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        pos *= chunkSize;
                        PlaceChunk(pos);
                        BuildChunkFromQueue();
                    }
                }
            }

            long duration = watch.ElapsedMilliseconds;
            watch.Stop();
            Debug.Log("Generation took: " + duration + "ms");
        }

        /// <summary>
        /// Check all positions in radius around player whether they already contain a chunk
        /// </summary>
        protected override void LoadObjects(Vector3 playerPos, int horizontalDistance, int stepSize)
        {
            int verticalChunks = terrainSettings.maxHeight / terrainSettings.chunkSize;

            int minX = (int)(playerPos.x - horizontalDistance) / stepSize;
            int maxX = (int)(playerPos.x + horizontalDistance) / stepSize;
            int minY = -verticalChunks;
            int maxY = verticalChunks;
            int minZ = (int)(playerPos.z - horizontalDistance) / stepSize;
            int maxZ = (int)(playerPos.z + horizontalDistance) / stepSize;

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        pos *= stepSize;
                        if (!loadedObjects.ContainsKey(pos))
                        {
                            if (Vector3.Distance(pos, playerPos) < horizontalDistance)
                            {
                                PlaceChunk(pos);
                            }
                        }
                    }
                }
            }
        }

        private void PlaceChunk(Vector3 chunkPos)
        {
            GameObject chunkGameObject = objectPool.GetObject();
            chunkGameObject.name = string.Format("Chunk_{0}_{1}_{2}", chunkPos.x, chunkPos.y, chunkPos.z);
            chunkGameObject.transform.position = chunkPos;
            loadedObjects.Add(chunkPos, chunkGameObject);

            if (visualizeBounds)
            {
                var chunkBounds = chunkGameObject.GetComponent<ChunkBounds>();
                if (chunkBounds == null)
                {
                    chunkGameObject.AddComponent<ChunkBounds>();
                }
            }

            Chunk chunk = chunkGameObject.GetComponent<Chunk>();
            chunksToGenerate.Enqueue(chunk);
        }

        private void BuildChunkFromQueue()
        {
            if (chunksToGenerate.Count > 0)
            {
                var isosurface = new Isosurface(density, terrainSettings.isosurfaceAlgorithm);
                Chunk chunk = chunksToGenerate.Dequeue();
                chunk.Destroy();
                chunk.CreateChunk(isosurface, spawners, terrainSettings.chunkSize);
            }
        }
    }
}
