using System;
using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Holds all settings for creating a voxel terrain
    /// </summary>
    [Serializable]
    public class TerrainSettings : SpawnSettings
    {
        public IsosurfaceAlgorithm isosurfaceAlgorithm;
        public TerrainDensityType terrainDensityType;
        public Vector2 worldSize = new Vector2(64, 64);
        public bool infiniteTerrain;
    }
}
