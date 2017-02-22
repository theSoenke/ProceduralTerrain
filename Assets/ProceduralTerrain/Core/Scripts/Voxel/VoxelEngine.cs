using LibNoise;
using PCG.Voxel.Generators;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace PCG.Voxel
{
    [ExecuteInEditMode]
    public class VoxelEngine : MonoBehaviour
    {
        public TerrainSettings terrainSettings;
        public ObjectSpawnSettings treeSpawnSettings;
        public ObjectSpawnSettings objectSpawnSettings;
        public bool visualizeChunkBounds;
        public int maxChunksPerFrame = 2;
        public Chunk chunkPrefab;
        public GameObject waterPrefab;
        public int customNoiseType;

        public GameObject addObject;
        public GameObject addTree;

        public GameObject playerPrefab;
        public GameObject player;

        public delegate void TerrainReadyDelegate();
        public event TerrainReadyDelegate OnTerrainReady;

        private ChunkGenerator chunkGenerator;
        private WaterGenerator waterGenerator;
        private bool isRunning;
        private bool isReadyForPlayer;


        #region UnityFunctions

        private void Start()
        {
            if (playerPrefab != null && playerPrefab.activeInHierarchy)
            {
                playerPrefab.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Loom.Init();
            }
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            UpdateTerrain();

            // Call terrain ready event
            if (!isReadyForPlayer)
            {
                isReadyForPlayer = chunkGenerator.IsBaseTerrainReady();
                if (isReadyForPlayer)
                {
                    if (OnTerrainReady != null)
                    {
                        OnTerrainReady();
                        player.SetActive(true);
                    }
                }
            }

            /*
             * Fix memory leak of procedural chunk meshes.
             * Unity does not automatically cleanup references to of unused meshes
             */
            if (Time.frameCount % 50 == 0)
            {
                Resources.UnloadUnusedAssets();
            }
        }

        private void OnValidate()
        {
            if (addTree != null)
            {
                ObjectResource treeResource = new ObjectResource
                {
                    prefab = addTree
                };
                treeSpawnSettings.resources.Add(treeResource);
                addTree = null;
            }

            if (addObject != null)
            {
                ObjectResource objectResource = new ObjectResource
                {
                    prefab = addObject
                };
                objectSpawnSettings.resources.Add(objectResource);
                addObject = null;
            }
        }
        #endregion

        /// <summary>
        /// Starts terrain generation
        /// </summary>
        public void GenerateTerrain()
        {
            isReadyForPlayer = false;

            InitNoise();
            DestroyPools();
            InitPlayer();
            InitGenerators();

            if (terrainSettings.infiniteTerrain)
            {
                isRunning = true;
            }
            else
            {
                chunkGenerator.GenerateStaticTerrain();
                if (waterPrefab != null)
                {
                    waterGenerator.Update(transform.position);
                }
            }
        }

        /// <summary>
        /// Destroys all terrain in child gameobjects ot this gameobject
        /// </summary>
        public void DestroyPools()
        {
            isRunning = false;

            var children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                children.Add(child.gameObject);
            }
            children.ForEach(SafeDestroy);
        }

        private void UpdateTerrain()
        {
            if (terrainSettings.infiniteTerrain)
            {
                Vector3 playerPos = player.transform.position;
                chunkGenerator.Update(playerPos);
                if (waterPrefab != null)
                {
                    waterGenerator.Update(playerPos);
                }
            }
        }

        private void InitPlayer()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (playerPrefab == null)
            {
                throw new Exception("Player prefab needs to be assigned");
            }

            if (player == null)
            {
                player = (GameObject)Instantiate(playerPrefab, transform.position, Quaternion.identity);
            }
            player.SetActive(false);
        }

        private void InitNoise()
        {
            if (terrainSettings.randomSeed)
            {
                Random random = new Random();
                terrainSettings.noiseSettings.seed = random.Next();
            }
        }

        private void InitGenerators()
        {
            var generators = new List<ObjectSpawner>();
            Transform terrainCenter = terrainSettings.infiniteTerrain ? player.transform : transform;

            treeSpawnSettings.resources.RemoveAll(t => t.prefab == null);
            if (treeSpawnSettings.resources.Count > 0)
            {
                MultipleObjectPool spawnTreePool = CreateTreePool();

                treeSpawnSettings.chunkSize = terrainSettings.chunkSize;
                treeSpawnSettings.viewDistance = terrainSettings.viewDistance;
                treeSpawnSettings.maxHeight = terrainSettings.maxHeight;
                treeSpawnSettings.seaLevel = terrainSettings.seaLevel;
                treeSpawnSettings.noiseSettings.seed = terrainSettings.noiseSettings.seed;

                var treeGenerator = new ObjectSpawner(spawnTreePool, treeSpawnSettings, terrainCenter);
                generators.Add(treeGenerator);
            }

            objectSpawnSettings.resources.RemoveAll(t => t.prefab == null);
            if (objectSpawnSettings.resources.Count > 0)
            {
                MultipleObjectPool spawnObjectPool = CreateSpawnObjectPool();
                objectSpawnSettings.chunkSize = terrainSettings.chunkSize;
                objectSpawnSettings.viewDistance = terrainSettings.viewDistance;
                objectSpawnSettings.maxHeight = terrainSettings.maxHeight;
                objectSpawnSettings.noiseSettings.seed = terrainSettings.noiseSettings.seed;

                var objectGenerator = new ObjectSpawner(spawnObjectPool, objectSpawnSettings, terrainCenter);
                generators.Add(objectGenerator);
            }

            ObjectPool chunkPool = CreateChunkPool();
            TerrainDensity density = GetTerrainDensity();

            chunkGenerator = new ChunkGenerator(terrainSettings, density, chunkPool, generators, maxChunksPerFrame)
            {
                visualizeBounds = visualizeChunkBounds
            };

            if (waterPrefab != null)
            {
                ObjectPool waterPool = CreateWaterPool();
                waterGenerator = new WaterGenerator(waterPool, terrainSettings.seaLevel);
            }
        }

        private ObjectPool CreateChunkPool()
        {
            if (chunkPrefab == null)
            {
                throw new Exception("Please set a chunk prefab first");
            }

            var chunkContainer = new GameObject("ChunkPool");
            chunkContainer.transform.SetParent(transform);

            int verticalChunks = terrainSettings.maxHeight / terrainSettings.chunkSize * 2;
            int poolSize;
            if (terrainSettings.infiniteTerrain)
            {
                int horizontalChunks = terrainSettings.viewDistance / terrainSettings.chunkSize * 2;
                poolSize = horizontalChunks * horizontalChunks * verticalChunks;
            }
            else
            {
                int xAxisChunkNum = (int)terrainSettings.worldSize.x / terrainSettings.chunkSize;
                int zAxisChunkNum = (int)terrainSettings.worldSize.y / terrainSettings.chunkSize;
                poolSize = xAxisChunkNum * zAxisChunkNum * verticalChunks;
            }
            var chunkPool = new ObjectPool(chunkPrefab.gameObject, chunkContainer.transform, poolSize);
            chunkPool.CreatePool();

            return chunkPool;
        }

        private ObjectPool CreateWaterPool()
        {
            var waterContainer = new GameObject("WaterPool");
            waterContainer.transform.SetParent(transform);

            var waterPool = new ObjectPool(waterPrefab, waterContainer.transform, 10);
            waterPool.CreatePool();

            return waterPool;
        }

        private MultipleObjectPool CreateTreePool()
        {
            var treeContainer = new GameObject("TreePool");
            treeContainer.transform.SetParent(transform);

            int treeTypes = treeSpawnSettings.resources.Count;
            var treePrefabs = new List<GameObject>();

            for (int i = 0; i < treeTypes; i++)
            {
                GameObject treePrefab = treeSpawnSettings.resources[i].prefab;
                treePrefabs.Add(treePrefab);
            }

            var treePool = new MultipleObjectPool(treePrefabs, treeContainer.transform, 300);
            treePool.CreatePool();

            return treePool;
        }

        private MultipleObjectPool CreateSpawnObjectPool()
        {
            var objectContainer = new GameObject("ObjectPool");
            objectContainer.transform.SetParent(transform);

            int objectTypes = objectSpawnSettings.resources.Count;
            var objectPrefabs = new List<GameObject>();

            for (int i = 0; i < objectTypes; i++)
            {
                GameObject objectPrefab = objectSpawnSettings.resources[i].prefab;
                objectPrefabs.Add(objectPrefab);
            }

            var objectPool = new MultipleObjectPool(objectPrefabs, objectContainer.transform, 300);
            objectPool.CreatePool();

            return objectPool;
        }

        private TerrainDensity GetTerrainDensity()
        {
            float amplitude = terrainSettings.noiseSettings.amplitude;
            ModuleBase noiseGenerator = GetNoiseGenerator();

            switch (terrainSettings.terrainDensityType)
            {
                case TerrainDensityType.Heightmap:
                    return new Heightmap(noiseGenerator, amplitude);
                case TerrainDensityType.NoiseTerrain:
                    return new TerrainDensity(noiseGenerator, amplitude);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ModuleBase GetNoiseGenerator()
        {
            return terrainSettings.noiseSettings.GetNoiseGenerator();
        }

        /// <summary>
        /// Destroys gameobject in editor and in gamemode
        /// </summary>
        /// <param name="go">GameObject to destroy</param>
        private static void SafeDestroy(GameObject go)
        {
            if (Application.isPlaying)
            {
                Destroy(go);
            }
            else
            {
                DestroyImmediate(go);
            }
        }
    }
}