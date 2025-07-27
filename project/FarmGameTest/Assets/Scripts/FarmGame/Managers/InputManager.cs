using UnityEngine;
using UnityEngine.InputSystem;
using FarmGame.System;
using FarmGame.Events;

namespace FarmGame.Managers
{
    /// <summary>
    /// Handles mouse/touch tap + drag/drop, and PC hotkeys.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Minimum distance (in screen pixels) to move before input is considered a drag.
        /// </summary>
        [SerializeField] private const float DragThreshold = 20f;
        
        private GameInputActions m_actions;
        private Vector2 m_pointerStart;
        private bool m_isDragging;
        
        /// <summary>
        /// Initializes input bindings and subscribes to input events.
        /// </summary>
        public void Initialize()
        {
            // Instantiate and enable input actions
            m_actions = new GameInputActions();
            m_actions.Enable();

            EventsSubscribe();
        }

        private void OnDisable()
        {
            EventsUnsubscribe();
        }

        /// <summary>
        /// Called when a pointer (mouse/touch) is first pressed down.
        /// Stores the start position to detect drag threshold later.
        /// </summary>
        private void OnPointerStarted(InputAction.CallbackContext _)
        {
            // Record start position and reset drag flag
            m_pointerStart = m_actions.Default.Point.ReadValue<Vector2>();
            m_isDragging = false;
        }

        /// <summary>
        /// Called continuously as the pointer moves.
        /// If held and moved beyond the drag threshold, a drag event is fired.
        /// </summary>
        private void OnPointerMoved(InputAction.CallbackContext context)
        {
            // Return if we're not holding down
            if (m_actions.Default.Click.ReadValue<float>() < 0.5f) return;
            
            Vector2 position = context.ReadValue<Vector2>();

            // Once the finger/mouse moves beyond threshold, we enter the drag mode
            if (!m_isDragging && Vector2.Distance(m_pointerStart, position) > DragThreshold) m_isDragging = true;

            if (m_isDragging) EventBus.Publish(new EventInputDrag(position));
        }

        /// <summary>
        /// Called when the pointer is released.
        /// Determines whether it was a tap or a drag+drop.
        /// </summary>
        private void OnPointerEnded(InputAction.CallbackContext _)
        {
            Vector2 position = m_actions.Default.Point.ReadValue<Vector2>();

            if (m_isDragging)
            {
                EventBus.Publish(new EventInputDrop(position));
            }
            else
            {
                EventBus.Publish(new EventInputTap(position));
            }
            
            m_isDragging = false;
        }
        
        private void OnDestroy()
        {
            // Clean up
            m_actions.Disable();
        }
        
        #region Forward input events to the event bus
        private void EventsSubscribe()
        {
            m_actions.Default.Click.performed += OnPointerStarted;
            m_actions.Default.Click.canceled += OnPointerEnded;
            m_actions.Default.Point.performed += OnPointerMoved;
            
            m_actions.Default.PlantMode.performed += OnHotkeyPlantMode;
            m_actions.Default.WaterMode.performed += OnHotkeyWaterMode;
            m_actions.Default.HarvestMode.performed += OnHotkeyHarvestMode;
            m_actions.Default.Escape.performed += OnHotkeyEscape;
        }

        private void EventsUnsubscribe()
        {
            m_actions.Default.Click.performed -= OnPointerStarted;
            m_actions.Default.Click.canceled -= OnPointerEnded;
            m_actions.Default.Point.performed -= OnPointerMoved;
            
            m_actions.Default.PlantMode.performed -= OnHotkeyPlantMode;
            m_actions.Default.WaterMode.performed -= OnHotkeyWaterMode;
            m_actions.Default.HarvestMode.performed -= OnHotkeyHarvestMode;
            m_actions.Default.Escape.performed -= OnHotkeyEscape;
        }
        
        private void OnHotkeyPlantMode(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputPlantMode());
        }
        private void OnHotkeyWaterMode(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputWaterMode());
        }
        private void OnHotkeyHarvestMode(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputHarvestMode());
        }
        private void OnHotkeyEscape(InputAction.CallbackContext _)
        {
            EventBus.Publish(new EventInputEscape());
        }
        #endregion
    }
}
