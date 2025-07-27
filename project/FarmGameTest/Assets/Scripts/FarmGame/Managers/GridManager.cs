using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.Misc;
using FarmGame.World.Crops;
using FarmGame.World.VFX;

namespace FarmGame.Managers
{
    /// <summary>
    /// Handles the grid data and placement.
    /// </summary>
    [DisallowMultipleComponent]
    public class GridManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GridPositionMarker gridPositionMarker;
        
        [Header("Crop data")]
        [SerializeField] private List<CropData> cropData = new List<CropData>();
        
        [Header("VFX settings")]
        [SerializeField] private VFXController vfxCropHarvested;
        
        [Header("Raycast settings")]
        [SerializeField] private LayerMask raycastLayerMask;
        
        [Header("Performance settings")]
        [SerializeField] private float updateInterval = 0.2f;
        
        // Settings and manager references
        private GameSettings m_gameSettings;
        private CropPoolManager m_cropPoolManager;
        private Camera m_mainCamera;
        
        // Position marker
        private WaitForSeconds m_waitPositionMarker;
        private Coroutine m_updatePositionMarkerRoutine;
        private Vector2 m_currentMousePosition;
        private Vector3 m_markerOffsetVector;
        private Vector2Int m_currentGridPos;
        private Vector2Int m_gridPosInvalid;
        private Vector3 m_currentWorldPos;
        
        // Crops
        private readonly Dictionary<Vector2Int, Crop> m_cropGrid = new Dictionary<Vector2Int, Crop>();
        
        /// <summary>
        /// Dictionary storing all placed crops indexed by their grid position.
        /// </summary>
        public Dictionary<Vector2Int, Crop> CropGrid => m_cropGrid;
        
        /// <summary>
        /// List of crop types and their associated data (e.g. cost, growth time).
        /// </summary>
        public List<CropData> CropData => cropData;
        
        // Other
        private WaitForSeconds m_waitTickCrops;
        private Coroutine m_tickCropsRoutine;

        private PlacementMode m_placementMode = PlacementMode.None;
        private CropType m_currentCropToPlace = CropType.None;
        private int m_currentMoney;
        public int CurrentMoney => m_currentMoney;
        
        /// <summary>
        /// Initializes the GridManager with necessary dependencies and starts crop ticking.
        /// </summary>
        public void Initialize(GameSettings gameSettings, CropPoolManager cropPoolManager, Camera mainCamera)
        {
            m_gameSettings = gameSettings;
            m_cropPoolManager = cropPoolManager;
            m_mainCamera = mainCamera;
            
            m_currentMoney = m_gameSettings.initialMoney;
            
            m_waitPositionMarker = new WaitForSeconds(updateInterval);
            m_waitTickCrops = new WaitForSeconds(1f);
            
            m_gridPosInvalid = new Vector2Int(-999, -999);
            m_markerOffsetVector = new Vector3(m_gameSettings.cellSize * 0.5f, 0.01f, m_gameSettings.cellSize * 0.5f);
            
            m_tickCropsRoutine = StartCoroutine(TickCropsRoutine());
            
            ResetPositionMarker();
            EventsSubscribe();
        }
        
        /// <summary>
        /// Assigns a crop to a grid cell.
        /// </summary>
        private void SetCell(Vector2Int gridPosition, Crop crop)
        {
            m_cropGrid[gridPosition] = crop;
        }

        /// <summary>
        /// Retrieves the crop at a specific grid cell, or null if empty.
        /// </summary>
        private Crop GetCell(Vector2Int gridPosition)
        {
            m_cropGrid.TryGetValue(gridPosition, out Crop crop);
            return crop;
        }
        
        /// <summary>
        /// Converts a screen-space mouse position into a valid grid coordinate, if within bounds.
        /// </summary>
        private Vector2Int MouseToGridPosition(Vector2 mousePosition)
        {
            Ray ray = m_mainCamera.ScreenPointToRay(mousePosition);
                
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayerMask))
            {
                Vector3 worldPos = hit.point;
                    
                // Make sure the position is within the grid
                if (worldPos.x < m_gameSettings.cellSize * m_gameSettings.gridSizeX / 2f &&
                    worldPos.x > m_gameSettings.cellSize * -m_gameSettings.gridSizeX / 2f &&
                    worldPos.z < m_gameSettings.cellSize * m_gameSettings.gridSizeZ / 2f &&
                    worldPos.z > m_gameSettings.cellSize * -m_gameSettings.gridSizeZ / 2f)
                {
                    return new Vector2Int(
                        Mathf.FloorToInt(worldPos.x / m_gameSettings.cellSize),
                        Mathf.FloorToInt(worldPos.z / m_gameSettings.cellSize)
                    );
                }
            }

            return m_gridPosInvalid;
        }
        
        /// <summary>
        /// Converts a grid coordinate into a world-space position.
        /// </summary>
        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x * m_gameSettings.cellSize, 0, gridPos.y * m_gameSettings.cellSize);
        }
        
        /// <summary>
        /// Continuously updates the grid marker to follow the cursor while in placement mode.
        /// </summary>
        private IEnumerator UpdatePositionMarkerRoutine()
        {
            while (true)
            {
                m_currentGridPos = MouseToGridPosition(m_currentMousePosition);

                if (m_currentGridPos != m_gridPosInvalid)
                {
                    m_currentWorldPos = GridToWorldPosition(m_currentGridPos);
                        
                    gridPositionMarker.gameObject.transform.position = m_currentWorldPos + m_markerOffsetVector;
                    gridPositionMarker.SetVisible(true);
                    gridPositionMarker.SetBlocked(GetCell(m_currentGridPos));
                }
                else
                {
                    ResetPositionMarker();
                }
                
                yield return m_waitPositionMarker;
            }
        }
        
        /// <summary>
        /// Handles ticking all crops on the grid.
        /// </summary>
        private IEnumerator TickCropsRoutine()
        {
            while (true)
            {
                foreach (Crop crop in m_cropGrid.Values) crop.Tick();

                yield return m_waitTickCrops;
            }
        }

        /// <summary>
        /// Hides and resets the grid position marker.
        /// </summary>
        private void ResetPositionMarker()
        {
            m_currentGridPos = m_gridPosInvalid;
            m_currentWorldPos = Vector3.zero;
                        
            gridPositionMarker.SetVisible(false);
        }
        
        /// <summary>
        /// Stops the position marker update routine and hides it.
        /// </summary>
        private void StopPositionMarkerRoutine()
        {
            if (m_updatePositionMarkerRoutine == null) return;
            
            StopCoroutine(m_updatePositionMarkerRoutine);
            m_updatePositionMarkerRoutine = null;
            
            ResetPositionMarker();
        }
        
        /// <summary>
        /// Stops the crop ticking coroutine.
        /// </summary>
        private void StopTickCropsRoutine()
        {
            if (m_tickCropsRoutine == null) return;
            
            StopCoroutine(m_tickCropsRoutine);
            m_tickCropsRoutine = null;
        }

        private void OnDisable()
        {
            EventsUnsubscribe();
            
            StopPositionMarkerRoutine();
            StopTickCropsRoutine();
        }

        #region Events
        private void EventsSubscribe()
        {
            EventBus.Subscribe<EventInputPlantMode>(OnPlantModeActivated);
            EventBus.Subscribe<EventInputWaterMode>(OnWaterModeActivated);
            EventBus.Subscribe<EventInputHarvestMode>(OnHarvestModeActivated);
            EventBus.Subscribe<EventInputEscape>(OnEscape);
            EventBus.Subscribe<EventCropCornButton>(OnCropCornButton);
            EventBus.Subscribe<EventCropTomatoButton>(OnCropTomatoButton);
            
            EventBus.Subscribe<EventCropAdvanceState>(OnCropAdvanceState);
            
            EventBus.Subscribe<EventInputDrag>(OnInputDrag);
            EventBus.Subscribe<EventInputDrop>(OnInputDrop);
            EventBus.Subscribe<EventInputTap>(OnInputTap);
        }

        private void EventsUnsubscribe()
        {
            EventBus.Unsubscribe<EventInputPlantMode>(OnPlantModeActivated);
            EventBus.Unsubscribe<EventInputWaterMode>(OnWaterModeActivated);
            EventBus.Unsubscribe<EventInputHarvestMode>(OnHarvestModeActivated);
            EventBus.Unsubscribe<EventInputEscape>(OnEscape);
            EventBus.Unsubscribe<EventCropCornButton>(OnCropCornButton);
            EventBus.Unsubscribe<EventCropTomatoButton>(OnCropTomatoButton);
            
            EventBus.Unsubscribe<EventCropAdvanceState>(OnCropAdvanceState);
            
            EventBus.Unsubscribe<EventInputDrag>(OnInputDrag);
            EventBus.Unsubscribe<EventInputDrop>(OnInputDrop);
            EventBus.Unsubscribe<EventInputTap>(OnInputTap);
        }

        private void OnPlantModeActivated(EventInputPlantMode _) => m_placementMode = PlacementMode.Plant;
        private void OnWaterModeActivated(EventInputWaterMode _) => m_placementMode = PlacementMode.Water;
        private void OnHarvestModeActivated(EventInputHarvestMode _) => m_placementMode = PlacementMode.Harvest;
        private void OnEscape(EventInputEscape _) => m_placementMode = PlacementMode.None;

        private void OnCropCornButton(EventCropCornButton _)
        {
            if (m_currentMoney >= cropData[0].plantCost)
            {
                m_currentCropToPlace = CropType.Corn;
                EventBus.Publish(new EventUICropDragStart());
            }
        }

        private void OnCropTomatoButton(EventCropTomatoButton _)
        {
            if (m_currentMoney >= cropData[1].plantCost)
            {
                m_currentCropToPlace = CropType.Tomato;
                EventBus.Publish(new EventUICropDragStart());
            }
        }

        /// <summary>
        /// Checks if a crop was harvested (marked with CropState.None).
        /// </summary>
        private void OnCropAdvanceState(EventCropAdvanceState eventCropAdvanceState)
        {
            // Check if the crop advanced to the final state
            if (eventCropAdvanceState.TargetCrop.State != CropState.None) return;
            
            // Get crop position on the grid
            Vector2Int gridPos = m_cropGrid.FirstOrDefault(kvp => kvp.Value == eventCropAdvanceState.TargetCrop).Key;
            
            // Award money
            m_currentMoney += (eventCropAdvanceState.TargetCrop.CropData.cropType == CropType.Corn) ?
                cropData[0].harvestValue : cropData[1].harvestValue;

            // Fire event
            EventBus.Publish(new EventCropHarvested(eventCropAdvanceState.TargetCrop, vfxCropHarvested));

            // Remove crop
            m_cropPoolManager.ReleaseCrop(eventCropAdvanceState.TargetCrop);
            m_cropGrid.Remove(gridPos);
        }
        
        /// <summary>
        /// Begins updating the grid marker when player drags to plant a crop.
        /// </summary>
        private void OnInputDrag(EventInputDrag eventInputDrag)
        {
            if (m_placementMode != PlacementMode.Plant || m_currentCropToPlace == CropType.None) return;
            
            m_currentMousePosition = eventInputDrag.Position;
            
            if (m_updatePositionMarkerRoutine == null)
            {
                m_updatePositionMarkerRoutine = StartCoroutine(UpdatePositionMarkerRoutine());
            }
        }

        /// <summary>
        /// Places a crop at the current marker position when the player drops during planting mode.
        /// </summary>
        private void OnInputDrop(EventInputDrop _)
        {
            if (m_placementMode != PlacementMode.Plant || m_currentCropToPlace == CropType.None) return;

            if (m_currentGridPos != m_gridPosInvalid && !GetCell(m_currentGridPos))
            {
                // Reduce money
                m_currentMoney -= (m_currentCropToPlace == CropType.Corn) ? cropData[0].plantCost :
                    cropData[1].plantCost;
            
                // Place crop
                Crop crop = m_cropPoolManager.GetCrop();
                crop.gameObject.transform.position = m_currentWorldPos + m_markerOffsetVector;
                crop.Initialize((m_currentCropToPlace == CropType.Corn) ? cropData[0] : cropData[1]);
                        
                SetCell(m_currentGridPos, crop);
            }
            
            StopPositionMarkerRoutine();
            m_currentCropToPlace = CropType.None;
        }

        /// <summary>
        /// Handles crop interactions based on placement mode (e.g. water, harvest).
        /// </summary>
        private void OnInputTap(EventInputTap eventInputTap)
        {
            m_currentCropToPlace = CropType.None;
            
            m_currentMousePosition = eventInputTap.Position;
            m_currentGridPos = MouseToGridPosition(m_currentMousePosition);
            Crop crop = GetCell(m_currentGridPos);

            if (!crop) return;
            
            switch (m_placementMode)
            {
                case PlacementMode.Water when crop.State == CropState.WaterNeeded:
                    crop.SetState(CropState.WaterMarked);
                    break;
                case PlacementMode.Harvest when crop.State == CropState.HarvestNeeded:
                    crop.SetState(CropState.HarvestMarked);
                    break;
            }
        }
        #endregion
    }
}
