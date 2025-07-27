using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FarmGame.World.Crops;

namespace FarmGame.UI
{
    /// <summary>
    /// Displays and updates UI elements showing crop stats per crop type.
    /// </summary>
    public class UICropStatsElement : MonoBehaviour
    {
        /// <summary>
        /// Internal class for tracking crop states by category.
        /// </summary>
        private class CropMonitoredStates
        {
            public int growing;
            public int needsWater;
            public int readyToHarvest;
        }
        
        [SerializeField] private TextMeshProUGUI cropCornText;
        [SerializeField] private TextMeshProUGUI cropTomatoText;
        
        private readonly Dictionary<CropType, CropMonitoredStates> m_cropStats = new Dictionary<CropType, CropMonitoredStates>();
        
        /// <summary>
        /// Registers a new crop type to track, if not already present.
        /// </summary>
        public void RegisterCrop(CropType type)
        {
            if (!m_cropStats.ContainsKey(type)) m_cropStats[type] = new CropMonitoredStates();
            
            UpdateText(type);
        }

        /// <summary>
        /// Increases the count for the given crop state.
        /// </summary>
        public void Increment(CropType type, CropState state)
        {
            UpdateStat(type, state, 1);
        }

        /// <summary>
        /// Decreases the count for the given crop state.
        /// </summary>
        public void Decrement(CropType type, CropState state)
        {
            UpdateStat(type, state, -1);
        }

        /// <summary>
        /// Moves a crop from one state to another.
        /// </summary>
        public void Transition(CropType type, CropState from, CropState to)
        {
            UpdateStat(type, from, -1);
            UpdateStat(type, to, 1);
        }

        /// <summary>
        /// Applies a value delta to the tracked count for a specific crop state.
        /// </summary>
        private void UpdateStat(CropType type, CropState state, int valueChange)
        {
            if (!m_cropStats.TryGetValue(type, out CropMonitoredStates stats)) return;

            switch (state)
            {
                case CropState.WaterNeeded:
                    stats.needsWater += valueChange;
                    break;
                case CropState.WaterMarked:
                case CropState.Seed:
                case CropState.Sprout:
                    stats.growing += valueChange;
                    break;
                case CropState.HarvestNeeded:
                    stats.readyToHarvest += valueChange;
                    break;
            }

            UpdateText(type);
        }

        /// <summary>
        /// Updates the on-screen text for the specified crop type.
        /// </summary>
        private void UpdateText(CropType type)
        {
            if (!m_cropStats.TryGetValue(type, out CropMonitoredStates stats)) return;
            
            string newText = $"{stats.growing} Wachsend\n{stats.needsWater} Brauchen Wasser\n{stats.readyToHarvest} Erntebereit";
            
            if (type == CropType.Corn)
            {
                cropCornText.text = newText;
            }
            else
            {
                cropTomatoText.text = newText;
            }
        }
    }
}
