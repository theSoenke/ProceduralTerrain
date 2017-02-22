using LibNoise;
using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel.Generators
{
    public class ObjectSpawner : PlacementSpawner
    {
        private readonly ObjectSpawnSettings objectSpawnSettings;
        private readonly ModuleBase noiseGenerator;


        public ObjectSpawner(MultipleObjectPool objectPool, ObjectSpawnSettings objectSpawnSettings, Transform player) : base(objectPool, player)
        {
            this.objectSpawnSettings = objectSpawnSettings;
            noiseGenerator = objectSpawnSettings.GetNoiseGenerator();
        }

        public override List<PoolObject> SpawnObjects(Chunk chunk)
        {
            var spawnedObjects = new List<PoolObject>();
            Vector3 chunkPos = chunk.transform.position;
            int chunkSize = objectSpawnSettings.chunkSize;
            int minDistance = objectSpawnSettings.minDistance;

            int edgeDistanceX = (int)(chunkPos.x % minDistance);
            int edgeDistanceZ = (int)(chunkPos.z % minDistance);

            for (int x = edgeDistanceX; x < chunkSize; x += minDistance)
            {
                for (int z = edgeDistanceZ; z < chunkSize; z += minDistance)
                {
                    var spawnPos = new Vector3(x + chunkPos.x, chunkPos.y + chunkSize, z + chunkPos.z);

                    if (Vector3.Distance(spawnPos, player.transform.position) > chunkSize)
                    {
                        SpawnObject(spawnPos, chunkSize, spawnedObjects);
                    }
                }
            }

            return spawnedObjects;
        }

        private void SpawnObject(Vector3 spawnPos, int chunkSize, List<PoolObject> trees)
        {
            const float NoisePositionOffset = 0.5f; // offset for noise input. noise would be always 0 for integers
            float spawnProbability = objectSpawnSettings.spawnProbability;
            float terrainMaxHeight = objectSpawnSettings.maxHeight;
            float seaLevel = objectSpawnSettings.seaLevel;

            var noisePos = new Vector3(spawnPos.x + NoisePositionOffset, 0, spawnPos.z + NoisePositionOffset);
            float noise = (float)noiseGenerator.GetValue(noisePos);
            // normalize to [0,1]
            noise = (noise + 1) / 2;

            if (noise > spawnProbability)
            {
                return;
            }

            ObjectResource spawnResource = GetObjectType(noise);
            Vector2 spawnSlopeRange = spawnResource.spawnSlopeRange;
            Vector2 spawnHeightRange = spawnResource.spawnHeightRange;

            RaycastHit hit;
            const int ChunkMask = 8;
            const int LayerMask = 1 << ChunkMask; // ignore all collider except for the chunk collider
            if (!Physics.Raycast(spawnPos, Vector3.down, out hit, chunkSize - 1, LayerMask))
            {
                return;
            }

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            float minSlope = spawnSlopeRange.x;
            float maxSlope = spawnSlopeRange.y;
            if (angle < minSlope || angle > maxSlope)
            {
                // Don't spawn tree when not in slope range
                return;
            }

            float height = spawnPos.y - hit.distance;
            float minHeight = spawnHeightRange.x * terrainMaxHeight;
            float maxHeight = spawnHeightRange.y * terrainMaxHeight;
            if (height < minHeight || height > maxHeight)
            {
                // Don't spawn when not in spawn criteria
                return;
            }

            spawnPos.y = height;

            // Don't spawn trees in water
            if (height > seaLevel)
            {
                GameObject spawnObject = PlaceObject(spawnPos, spawnResource.type);
                Random.InitState((int)(noise * 100));
                int rotation = Random.Range(0, 360);
                spawnObject.transform.Rotate(new Vector3(0, rotation, 0));
                trees.Add(new PoolObject(spawnResource.type, spawnObject));
            }
        }

        private GameObject PlaceObject(Vector3 spawnPos, int type)
        {
            GameObject spawnObject = objectPool.GetObject(type);
            spawnObject.transform.position = spawnPos;
            spawnObject.SetActive(true);
            return spawnObject;
        }

        private ObjectResource GetObjectType(float noise)
        {
            int objectTypesNum = objectSpawnSettings.resources.Count;
            float spawnProbability = objectSpawnSettings.spawnProbability;
            float typeRange = spawnProbability / objectTypesNum;
            int type;

            for (type = 0; type < objectTypesNum; type++)
            {
                if (noise < typeRange * (type + 1))
                {
                    break;
                }
            }

            objectSpawnSettings.resources[type].type = type;
            return objectSpawnSettings.resources[type];
        }
    }
}
