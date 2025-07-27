using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.World.Crops
{
    /// <summary>
    /// ScriptableObject containing all data for a specific crop type,
    /// including growth times, cost/value, and visual stages.
    /// </summary>
    [CreateAssetMenu(fileName = "CropData", menuName = "Scriptable Objects/CropData")]
    public class CropData : ScriptableObject
    {
        private const int ExpectedStageCount = 3;
        
        public CropType cropType;
    
        public int plantCost;
        public int harvestValue;

        public int growthTimeStage1;
        public int growthTimeStage2;

        /// <summary>
        /// List of visuals representing crop appearance at different stages.
        /// Automatically clamped to 3 entries in editor.
        /// </summary>
        public List<CropVisual> visuals;
        
        private void OnValidate()
        {
            TrimOrExpandListToCount(visuals, ExpectedStageCount);
        }

        /// <summary>
        /// Ensures a list is exactly the specified length by trimming or padding with default values.
        /// </summary>
        private static void TrimOrExpandListToCount<T>(List<T> list, int count)
        {
            if (list == null) return;

            while (list.Count > count) list.RemoveAt(list.Count - 1);
            while (list.Count < count) list.Add(default);
        }
    }
}
