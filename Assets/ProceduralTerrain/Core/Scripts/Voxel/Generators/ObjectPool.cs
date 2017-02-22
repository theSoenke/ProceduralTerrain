using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Reduces instantiation and destroying of gameobjects by reusing objects
    /// </summary>
    public class ObjectPool
    {
        private readonly Transform parent;
        private readonly Queue<GameObject> objectPool;
        private readonly GameObject poolPrefab;
        private readonly int poolSize;


        public ObjectPool(GameObject poolPrefab, Transform parent, int poolSize)
        {
            this.poolPrefab = poolPrefab;
            this.parent = parent;
            this.poolSize = poolSize;

            objectPool = new Queue<GameObject>(poolSize);
        }

        /// <summary>
        /// Init chunk pool
        /// </summary>
        public void CreatePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                CreatePoolObject();
            }
        }


        private void CreatePoolObject()
        {
            GameObject poolObject = Object.Instantiate(poolPrefab);
            poolObject.SetActive(false);
            poolObject.transform.SetParent(parent);
            objectPool.Enqueue(poolObject);
        }

        public GameObject GetObject()
        {
            if (objectPool.Count == 0)
            {
                CreatePoolObject();
            }

            GameObject reusableObject = objectPool.Dequeue();
            return reusableObject;
        }

        public void ReuseObject(GameObject reusableObject)
        {
            reusableObject.SetActive(false);
            objectPool.Enqueue(reusableObject);
        }
    }
}