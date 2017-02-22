using System;
using System.Collections.Generic;

namespace PCG.Voxel
{
    [Serializable]
    public class ObjectSpawnSettings : SpawnSettings
    {
        public List<ObjectResource> resources = new List<ObjectResource>();
        public int minDistance = 10;
        public float spawnProbability = 0.5f;
    }
}