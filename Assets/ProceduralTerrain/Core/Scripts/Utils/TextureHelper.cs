using LibNoise;
using UnityEngine;

/// <summary>
/// Helper class for textures
/// </summary>
public static class TextureHelper
{
    /// <summary>
    /// Returns a grey texture from input heightmap
    /// </summary>
    /// <param name="heightmap"></param>
    /// <returns></returns>
    public static Texture2D HeightmapToTexture(float[,] heightmap)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        var colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightmap[x, y]);
            }
        }

        return ColormapToTexture(colorMap, width, height);
    }

    /// <summary>
    /// Returns a 2D array with values between 0 and 1 from texture
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static float[,] TextureToHeightmap(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        var heightmap = new float[width, height];
        Color[] colors = texture.GetPixels();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = colors[y * height + x];
                heightmap[y, x] = color.a;
            }
        }

        return heightmap;
    }

    /// <summary>
    /// Generate texture from noise input
    /// </summary>
    /// <param name="noiseInput"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Texture2D NoiseToTexture(ModuleBase noiseInput, int width, int height)
    {
        var colors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = (float)x / width;
                float yCoord = (float)y / height;
                float noise = (float)noiseInput.GetValue(xCoord, 0, yCoord);
                colors[y * width + x] = Color.Lerp(Color.black, Color.white, noise);
            }
        }

        return ColormapToTexture(colors, width, height);
    }

    /// <summary>
    /// Returns a texture from array of colors
    /// </summary>
    /// <param name="colorMap"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Texture2D ColormapToTexture(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            anisoLevel = 0
        };
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
}
