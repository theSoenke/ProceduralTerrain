using LibNoise;

namespace PCG.Voxel
{
    public class Heightmap : TerrainDensity
    {
        public Heightmap(ModuleBase noiseGenerator, float amplitude) : base(noiseGenerator, amplitude)
        {
        }

        public override float GetDensity(float x, float y, float z)
        {
            return (float)noiseGenerator.GetValue(x, 0, z) * amplitude - y;
        }
    }
}
