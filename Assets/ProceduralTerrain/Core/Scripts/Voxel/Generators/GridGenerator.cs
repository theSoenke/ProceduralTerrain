using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel.Generators
{
    public abstract class GridGenerator
    {
        protected readonly Dictionary<Vector3, GameObject> loadedObjects;
        protected readonly ObjectPool objectPool;


        protected GridGenerator(ObjectPool objectPool)
        {
            this.objectPool = objectPool;
            Vector3Comparer vector3Comparer = new Vector3Comparer();
            loadedObjects = new Dictionary<Vector3, GameObject>(vector3Comparer);
        }


        public abstract void Update(Vector3 playerPos);

        /// <summary>
        /// Load new objects distance in to the player
        /// </summary>
        /// <param name="center"></param>
        /// <param name="stepSize">Distance between checks for object places</param>
        /// <param name="viewDistance"></param>
        protected abstract void LoadObjects(Vector3 center, int viewDistance, int stepSize);

        /// <summary>
        /// Unload object which too far from the player
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="viewDistance">Distance to the player in which objects are not removed</param>
        protected void UnloadObjects(Vector3 playerPos, int viewDistance)
        {
            var objectsToRemove = new List<Vector3>();

            foreach (var loadedObject in loadedObjects)
            {
                float distance = Vector3.Distance(loadedObject.Key, playerPos);
                if (distance > viewDistance)
                {
                    objectsToRemove.Add(loadedObject.Key);
                }
            }

            int removeCount = objectsToRemove.Count;
            for (int i = 0; i < removeCount; i++)
            {
                Vector3 key = objectsToRemove[i];
                objectPool.ReuseObject(loadedObjects[key]);
                loadedObjects.Remove(key);
            }
        }

        /// <summary>
        /// Custom comparer for Vector3 to reduce GC alloc by preventing boxing
        /// </summary>
        protected class Vector3Comparer : IEqualityComparer<Vector3>
        {
            public bool Equals(Vector3 x, Vector3 y)
            {
                return x == y;
            }

            public int GetHashCode(Vector3 obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
