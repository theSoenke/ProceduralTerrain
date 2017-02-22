using System.Collections.Generic;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Reduces instantiation and destroying of gameobjects by reusing objects
    /// </summary>
    public class MultipleObjectPool
    {
        private readonly Transform parent;
        private readonly Dictionary<int, Queue<GameObject>> objectPool;
        private readonly List<GameObject> poolPrefabs;
        private readonly int poolSize;
        private readonly int objectTypes;


        public MultipleObjectPool(List<GameObject> poolPrefabs, Transform parent, int poolSize)
        {
            this.poolPrefabs = poolPrefabs;
            this.parent = parent;
            this.poolSize = poolSize;
            objectTypes = poolPrefabs.Count;

            objectPool = new Dictionary<int, Queue<GameObject>>();
        }

        /// <summary>
        /// Init chunk pool
        /// </summary>
        public void CreatePool()
        {
            for (int i = 0; i < objectTypes; i++)
            {
                for (int j = 0; j < poolSize; j++)
                {
                    CreatePoolObject(i);
                }
            }
        }
        private void CreatePoolObject(int type)
        {
            GameObject poolObject = Object.Instantiate(poolPrefabs[type]);
            poolObject.SetActive(false);
            poolObject.transform.SetParent(parent);

            if (!objectPool.ContainsKey(type))
            {
                objectPool.Add(type, new Queue<GameObject>());
            }
            objectPool[type].Enqueue(poolObject);
        }

        public GameObject GetObject(int type)
        {
            if (objectPool[type].Count == 0)
            {
                CreatePoolObject(type);
            }

            GameObject reusableObject = objectPool[type].Dequeue();
            return reusableObject;
        }

        public void ReuseObject(PoolObject reusableObject)
        {
            reusableObject.gameObject.SetActive(false);
            objectPool[reusableObject.type].Enqueue(reusableObject.gameObject);
        }
    }

    public class PoolObject
    {
        public readonly int type;
        public readonly GameObject gameObject;

        public PoolObject(int type, GameObject gameObject)
        {
            this.type = type;
            this.gameObject = gameObject;
        }
    }
}