using PCG.Voxel.Generators;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Manages to generate water tiles around player
    /// </summary>
    public class WaterGenerator : GridGenerator
    {
        private readonly int seaLevel;


        public WaterGenerator(ObjectPool objectPool, int seaLevel) : base(objectPool)
        {
            this.seaLevel = seaLevel;
        }

        public override void Update(Vector3 playerPos)
        {
            LoadObjects(playerPos, 100000, 100000);
            UnloadObjects(playerPos, 100000 * 2);
        }

        protected override void LoadObjects(Vector3 playerPos, int horizontalDistance, int stepSize)
        {
            int minX = (int)(playerPos.x - horizontalDistance) / stepSize;
            int maxX = (int)(playerPos.x + horizontalDistance) / stepSize;
            int minZ = (int)(playerPos.z - horizontalDistance) / stepSize;
            int maxZ = (int)(playerPos.z + horizontalDistance) / stepSize;

            for (int x = minX; x < maxX; x++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    float xPos = x * stepSize;
                    float zPos = z * stepSize;

                    Vector3 spawnPoint = new Vector3(xPos, seaLevel, zPos);
                    if (loadedObjects.ContainsKey(spawnPoint))
                    {
                        return;
                    }

                    PlaceWater(spawnPoint);
                }
            }
        }

        private void PlaceWater(Vector3 objectPosition)
        {
            GameObject water = objectPool.GetObject();
            loadedObjects.Add(objectPosition, water);
            water.transform.position = objectPosition;
            water.SetActive(true);
        }
    }
}