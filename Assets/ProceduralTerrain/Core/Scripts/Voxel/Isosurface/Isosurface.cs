using System;
using UnityEngine;

namespace PCG.Voxel
{
    public class Isosurface
    {
        private readonly Density density;
        private readonly IsosurfaceAlgorithm isosurfaceAlgorithm;


        public Isosurface(Density density, IsosurfaceAlgorithm algorithm)
        {
            isosurfaceAlgorithm = algorithm;
            this.density = density;
        }

        public float GetDensity(float x, float y, float z)
        {
            return density.GetDensity(x, y, z);
        }

        public MeshBuilder GetMeshBuilder(Vector3 offset, int chunkSize)
        {
            switch (isosurfaceAlgorithm)
            {
                case IsosurfaceAlgorithm.MarchingCubes:
                    return new MarchingCubes(this, offset, chunkSize);
                case IsosurfaceAlgorithm.DualContouring:
                    return new DualContouringUniform(this, offset, chunkSize);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Available isosurfaces algorithms
    /// </summary>
    public enum IsosurfaceAlgorithm
    {
        MarchingCubes,
        DualContouring
    }
}