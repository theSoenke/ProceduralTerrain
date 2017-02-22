using System;
using UnityEngine;

namespace PCG
{
    [Serializable]
    public class ObjectResource
    {
        public GameObject prefab;

        [MinMaxSlider(0, 1)]
        public Vector2 spawnHeightRange = new Vector2(0, 1);
        [MinMaxSlider(0, 90)]
        public Vector2 spawnSlopeRange = new Vector2(0, 90);

        public int type;
    }
}