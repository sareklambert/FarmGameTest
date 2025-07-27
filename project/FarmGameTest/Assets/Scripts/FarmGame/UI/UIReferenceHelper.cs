using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.Managers;
using FarmGame.World.Crops;

namespace FarmGame.UI
{
    /// <summary>
    /// Centralizes references and logic for managing UI controls and stats.
    /// Also acts as a forwarder for UI button interactions.
    /// </summary>
    public class UIReferenceHelper : MonoBehaviour
    {
        [SerializeField] private UICropStatsElement uiCropStatsElement;
        [SerializeField] private GameObject uiContainerMoney;
        [SerializeField] private GameObject uiContainerCropCorn;
        [SerializeField] private GameObject uiContainerCropTomato;
        [SerializeField] private Button uiButtonPlant;
        [SerializeField] private Button uiButtonWater;
        [SerializeField] private Button uiButtonHarvest;

        private Image m_uiButtonPlantImage;
        private Image m_uiButtonWaterImage;
        private Image m_uiButtonHarvestImage;
        
        private TextMeshProUGUI m_uiMoneyText;
        private TextMeshProUGUI m_uiCropCornText;
        private TextMeshProUGUI m_uiCropTomatoText;

        /// <summary>
        /// Provides access to the crop stats UI component.
        /// </summary>
        public UICropStatsElement UICropStatsElement => uiCropStatsElement;
        
        /// <summary>
        /// Updates the money text display in the UI.
        /// </summary>
        public void SetMoneyText(int money)
        {
            m_uiMoneyText.text = money.ToString();
        }
        
        /// <summary>
        /// Shows or hides the crop type selection containers.
        /// </summary>
        public void SetCropContainerVisibility(bool isVisible)
        {
            uiContainerCropCorn.SetActive(isVisible);
            uiContainerCropTomato.SetActive(isVisible);
        }

        /// <summary>
        /// Initializes the cost text labels for each crop type.
        /// </summary>
        public void InitializeCropCostTextFields(int costCorn, int costTomato)
        {
            m_uiCropCornText.text = costCorn.ToString();
            m_uiCropTomatoText.text = costTomato.ToString();
        }

        /// <summary>
        /// Resets all placement mode buttons to their default (highlighted) sprite state.
        /// </summary>
        public void ResetPlacementModeButtonPressedVisual()
        {
            m_uiButtonPlantImage.sprite = uiButtonPlant.spriteState.highlightedSprite;
            m_uiButtonWaterImage.sprite = uiButtonWater.spriteState.highlightedSprite;
            m_uiButtonHarvestImage.sprite = uiButtonHarvest.spriteState.highlightedSprite;
        }
        
        /// <summary>
        /// Applies the "pressed" visual state to the specified placement mode button.
        /// </summary>
        public void SetPlacementModeButtonPressedVisual(PlacementMode mode)
        {
            Button button;
            Image image;
            
            switch (mode)
            {
                case PlacementMode.Plant:
                    button = uiButtonPlant;
                    image = m_uiButtonPlantImage;
                    break;
                case PlacementMode.Water:
                    button = uiButtonWater;
                    image = m_uiButtonWaterImage;
                    break;
                case PlacementMode.Harvest:
                    button = uiButtonHarvest;
                    image = m_uiButtonHarvestImage;
                    break;
                default: return;
            }
            
            SpriteState swap = button.spriteState;
            image.sprite = swap.pressedSprite;
        }
        
        private void Awake()
        {
            // Get components
            m_uiButtonPlantImage = uiButtonPlant.GetComponent<Image>();
            m_uiButtonWaterImage = uiButtonWater.GetComponent<Image>();
            m_uiButtonHarvestImage = uiButtonHarvest.GetComponent<Image>();

            m_uiMoneyText = uiContainerMoney.GetComponentInChildren<TextMeshProUGUI>();
            m_uiCropCornText = uiContainerCropCorn.GetComponentInChildren<TextMeshProUGUI>();
            m_uiCropTomatoText = uiContainerCropTomato.GetComponentInChildren<TextMeshProUGUI>();
            
            uiCropStatsElement.RegisterCrop(CropType.Corn);
            uiCropStatsElement.RegisterCrop(CropType.Tomato);
        }

        #region Forward Unity UI button events to event bus
        public void OnPlantButtonClicked()
        {
            EventBus.Publish(new EventInputPlantMode());
        }

        public void OnWaterButtonClicked()
        {
            EventBus.Publish(new EventInputWaterMode());
        }

        public void OnHarvestButtonClicked()
        {
            EventBus.Publish(new EventInputHarvestMode());
        }

        public void OnCropCornButtonClicked()
        {
            EventBus.Publish(new EventCropCornButton());
        }

        public void OnCropTomatoButtonClicked()
        {
            EventBus.Publish(new EventCropTomatoButton());
        }
        #endregion
    }
}
