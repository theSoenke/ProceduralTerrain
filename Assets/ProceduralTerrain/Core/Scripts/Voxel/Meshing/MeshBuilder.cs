using UnityEngine;

namespace PCG.Voxel
{
    public abstract class MeshBuilder
    {
        private readonly Isosurface isosurface;
        protected readonly int chunkSize;
        protected readonly Vector3 offset;

        protected MeshBuilder(Isosurface isosurface, Vector3 offset, int chunkSize)
        {
            this.isosurface = isosurface;
            this.offset = offset;
            this.chunkSize = chunkSize;
        }

        public abstract MeshData GenerateMeshData();

        /// <summary>
        /// Get density for point in world
        /// </summary>
        protected float GetDensity(float x, float y, float z)
        {
            float densityPoint = y < 0 ? y * 1.5f : y; // limit height

            float xCoord = x / chunkSize;
            float yCoord = y / chunkSize;
            float zCoord = z / chunkSize;

            densityPoint += isosurface.GetDensity(xCoord, yCoord, zCoord);

            return densityPoint;
        }

        /// <summary>
        /// Get density for point in world
        /// </summary>
        protected float GetDensity(Vector3 pos)
        {
            return GetDensity(pos.x, pos.y, pos.z);
        }
    }
}
