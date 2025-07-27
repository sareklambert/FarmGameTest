using UnityEngine;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.UI;
using FarmGame.World.Crops;

namespace FarmGame.Managers
{
    /// <summary>
    /// Handles the UI, its interactions and helper classes.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        // Managers
        private GridManager m_gridManager;
        
        // Helper classes
        [SerializeField] private UIReferenceHelper uiReferenceHelper;
        [SerializeField] private UINeedIconRenderer uiNeedIconRenderer;
        
        /// <summary>
        /// Initializes the UI with data from the GridManager and sets up event subscriptions.
        /// </summary>
        public void Initialize(GridManager gridManager, Camera mainCamera)
        {
            m_gridManager = gridManager;

            uiReferenceHelper.InitializeCropCostTextFields(m_gridManager.CropData[0].plantCost,
                m_gridManager.CropData[1].plantCost);
            uiReferenceHelper.SetCropContainerVisibility(false);
            uiReferenceHelper.SetMoneyText(m_gridManager.CurrentMoney);
            
            uiNeedIconRenderer.Initialize(m_gridManager, mainCamera);
            
            EventsSubscribe();
        }
        
        private void OnDisable()
        {
            EventsUnsubscribe();
        }

        #region Events
        private void EventsSubscribe()
        {
            EventBus.Subscribe<EventInputPlantMode>(OnPlantModeActivated);
            EventBus.Subscribe<EventInputWaterMode>(OnWaterModeActivated);
            EventBus.Subscribe<EventInputHarvestMode>(OnHarvestModeActivated);
            EventBus.Subscribe<EventInputEscape>(OnEscape);
            
            EventBus.Subscribe<EventCropAdvanceState>(OnCropAdvanceState);
            EventBus.Subscribe<EventCropHarvested>(OnCropHarvested);
        }

        private void EventsUnsubscribe()
        {
            EventBus.Unsubscribe<EventInputPlantMode>(OnPlantModeActivated);
            EventBus.Unsubscribe<EventInputWaterMode>(OnWaterModeActivated);
            EventBus.Unsubscribe<EventInputHarvestMode>(OnHarvestModeActivated);
            EventBus.Unsubscribe<EventInputEscape>(OnEscape);
            
            EventBus.Unsubscribe<EventCropAdvanceState>(OnCropAdvanceState);
            EventBus.Unsubscribe<EventCropHarvested>(OnCropHarvested);
        }

        private void OnPlantModeActivated(EventInputPlantMode _)
        {
            uiReferenceHelper.SetCropContainerVisibility(true);
            uiReferenceHelper.ResetPlacementModeButtonPressedVisual();
            uiReferenceHelper.SetPlacementModeButtonPressedVisual(PlacementMode.Plant);
        }

        private void OnWaterModeActivated(EventInputWaterMode _)
        {
            uiReferenceHelper.SetCropContainerVisibility(false);
            uiReferenceHelper.ResetPlacementModeButtonPressedVisual();
            uiReferenceHelper.SetPlacementModeButtonPressedVisual(PlacementMode.Water);
        }

        private void OnHarvestModeActivated(EventInputHarvestMode _)
        {
            uiReferenceHelper.SetCropContainerVisibility(false);
            uiReferenceHelper.ResetPlacementModeButtonPressedVisual();
            uiReferenceHelper.SetPlacementModeButtonPressedVisual(PlacementMode.Harvest);
        }

        private void OnEscape(EventInputEscape _)
        {
            uiReferenceHelper.SetCropContainerVisibility(false);
            uiReferenceHelper.ResetPlacementModeButtonPressedVisual();
        }

        /// <summary>
        /// Responds to a crop's state changing and updates UI counters and icons accordingly.
        /// </summary>
        private void OnCropAdvanceState(EventCropAdvanceState eventCropAdvanceState)
        {
            CropType cropType = eventCropAdvanceState.TargetCrop.CropData.cropType;
            
            switch (eventCropAdvanceState.TargetCrop.State)
            {
                case CropState.Seed:
                    uiReferenceHelper.SetMoneyText(m_gridManager.CurrentMoney);
                    uiReferenceHelper.UICropStatsElement.Increment(cropType, CropState.Seed);
                    break;
                case CropState.WaterNeeded:
                    uiReferenceHelper.UICropStatsElement.Transition(cropType,
                        CropState.Seed, CropState.WaterNeeded);
                    
                    uiNeedIconRenderer.RebuildMatrices(CropState.WaterNeeded);
                    break;
                case CropState.WaterMarked:
                    uiReferenceHelper.UICropStatsElement.Transition(cropType,
                        CropState.WaterNeeded, CropState.WaterMarked);
                    
                    uiNeedIconRenderer.RebuildMatrices(CropState.WaterNeeded);
                    break;
                case CropState.Sprout:
                    uiReferenceHelper.UICropStatsElement.Transition(cropType,
                        CropState.WaterMarked, CropState.Sprout);
                    break;
                case CropState.HarvestNeeded:
                    uiReferenceHelper.UICropStatsElement.Transition(cropType,
                        CropState.Sprout, CropState.HarvestNeeded);
                    
                    uiNeedIconRenderer.RebuildMatrices(CropState.HarvestNeeded);
                    break;
                case CropState.HarvestMarked:
                    uiReferenceHelper.UICropStatsElement.Decrement(cropType, CropState.HarvestNeeded);
                    
                    uiNeedIconRenderer.RebuildMatrices(CropState.HarvestNeeded);
                    break;
            }
        }

        /// <summary>
        /// Updates the money display when a crop is harvested.
        /// </summary>
        private void OnCropHarvested(EventCropHarvested _)
        {
            uiReferenceHelper.SetMoneyText(m_gridManager.CurrentMoney);
        }
        #endregion
    }
}
