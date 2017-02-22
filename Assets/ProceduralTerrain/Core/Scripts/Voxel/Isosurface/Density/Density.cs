using UnityEngine;

namespace PCG.Voxel
{
    public abstract class Density
    {
        public Operation operation = Operation.Union;


        public enum Operation
        {
            Union,
            Difference,
            Intersection
        }

        public abstract float GetDensity(float x, float y, float z);

        public float GetDensity(Vector3 pos)
        {
            return GetDensity(pos.x, pos.y, pos.z);
        }
    }
}
