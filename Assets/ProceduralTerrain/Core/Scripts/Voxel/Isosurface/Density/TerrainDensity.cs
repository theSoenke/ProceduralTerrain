using LibNoise;

namespace PCG.Voxel
{
    public class TerrainDensity : Density
    {
        protected readonly float amplitude;
        protected readonly ModuleBase noiseGenerator;


        public TerrainDensity(ModuleBase noiseGenerator, float amplitude)
        {
            this.noiseGenerator = noiseGenerator;
            this.amplitude = amplitude;
        }

        public override float GetDensity(float x, float y, float z)
        {
            return (float)noiseGenerator.GetValue(x, y, z) * amplitude;
        }
    }

    public enum TerrainDensityType
    {
        NoiseTerrain,
        Heightmap
    }
}
