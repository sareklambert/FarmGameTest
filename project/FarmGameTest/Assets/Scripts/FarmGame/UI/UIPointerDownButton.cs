using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FarmGame.UI
{
    // Helper script that registers the button click event immediately when we start dragging from where the button is
    public class UIPointerDownButton : MonoBehaviour, IPointerDownHandler
    {
        private Button m_button;

        private void Awake()
        {
            m_button = GetComponent<Button>();
        }

        /// <summary>
        /// Invokes the button's click event as soon as pointer down is detected.
        /// Useful for drag-based input flows where tap should trigger instantly.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_button && m_button.interactable)
            {
                m_button.onClick.Invoke();
            }
        }
    }
}
