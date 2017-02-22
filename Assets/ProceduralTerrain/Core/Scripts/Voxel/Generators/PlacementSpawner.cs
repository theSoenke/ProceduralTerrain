using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel.Generators
{
    public abstract class PlacementSpawner
    {
        protected readonly MultipleObjectPool objectPool;
        protected readonly Transform player;


        protected PlacementSpawner(MultipleObjectPool objectPool, Transform player)
        {
            this.objectPool = objectPool;
            this.player = player;
        }

        public abstract List<PoolObject> SpawnObjects(Chunk chunk);

        public void DestroyObjects(List<PoolObject> spawnedObjects)
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                objectPool.ReuseObject(spawnedObjects[i]);
            }
        }
    }
}