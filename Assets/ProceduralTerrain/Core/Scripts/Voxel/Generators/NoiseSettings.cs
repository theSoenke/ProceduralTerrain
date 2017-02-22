using LibNoise;
using LibNoise.Generator;
using System;


namespace PCG.Noise
{
    public enum NoiseType
    {
        Perlin,
        Ridge,
        Billow
    }

    public enum NoiseOperator
    {
        Add,
        Min,
        Max,
        Multiply,
        Subtract
    }

    [Serializable]
    public class NoiseSettings
    {
        public int seed;
        public NoiseType noiseType;
        public int octaves = 6;
        public float amplitude = 1;
        public float frequency = 1;
        public float lacunarity = 2;
        public float persistence = 0.5f;


        public ModuleBase GetNoiseGenerator()
        {
            ModuleBase noiseGenerator;

            switch (noiseType)
            {
                case NoiseType.Perlin:
                    noiseGenerator = new Perlin()
                    {
                        Frequency = frequency,
                        Lacunarity = lacunarity,
                        Persistence = persistence,
                        OctaveCount = octaves,
                        Seed = seed,
                        Quality = QualityMode.Medium
                    };
                    break;
                case NoiseType.Billow:
                    noiseGenerator = new Billow()
                    {
                        Frequency = frequency,
                        Lacunarity = lacunarity,
                        Persistence = persistence,
                        OctaveCount = octaves,
                        Seed = seed
                    };
                    break;
                case NoiseType.Ridge:
                    noiseGenerator = new RidgedMultifractal()
                    {
                        Frequency = frequency,
                        Lacunarity = lacunarity,
                        OctaveCount = octaves,
                        Seed = seed
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return noiseGenerator;
        }
    }
}

