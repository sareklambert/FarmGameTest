using System;
using UnityEngine;

namespace FarmGame.World.Crops
{
    [Serializable]
    public struct CropVisual
    {
        public Mesh mesh;
        public Material materialInstanced;
        public Material materialFallback;
    }
}
