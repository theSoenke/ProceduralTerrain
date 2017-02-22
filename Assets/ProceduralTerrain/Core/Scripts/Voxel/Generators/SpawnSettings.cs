using LibNoise;
using PCG.Noise;

namespace PCG.Voxel
{
    public class SpawnSettings
    {
        public NoiseSettings noiseSettings;
        public bool randomSeed;
        public int chunkSize = 16; //Marching Cubes can have up to 5 triangles per cube(16^3)*5*3=61440. Unity mesh vertices limit of 2^16
        public int maxHeight = 100;
        public int viewDistance = 64;
        public int seaLevel = 0;


        public ModuleBase GetNoiseGenerator()
        {
            return noiseSettings.GetNoiseGenerator();
        }
    }
}