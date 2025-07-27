using System.Collections;
using UnityEngine;

namespace FarmGame.Misc
{
    /// <summary>
    /// Displays a pulsing marker on the grid to indicate valid/invalid placement.
    /// </summary>
    public class GridPositionMarker : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float minScale = 0.8f;
        [SerializeField] private float maxScale = 1.2f;
        [SerializeField] private float speed = 2f;

        [Header("Visual Settings")]
        [SerializeField] private Material materialNormal;
        [SerializeField] private Material materialBlocked;
        
        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.02f;
        
        // Animation
        private float m_baseScale;
        private float m_amplitude;
        private WaitForSeconds m_wait;
        private Coroutine m_scaleRoutine;

        // Visuals
        private MeshRenderer m_meshRenderer;
        
        /// <summary>
        /// Toggles visibility of the marker.
        /// </summary>
        public void SetVisible(bool visible)
        {
            m_meshRenderer.enabled = visible;
        }

        /// <summary>
        /// Changes the material based on whether the current cell is blocked.
        /// </summary>
        /// <param name="blocked">True if the placement is invalid.</param>
        public void SetBlocked(bool blocked)
        {
            Material newMaterial = blocked ? materialBlocked : materialNormal;
            if (m_meshRenderer.sharedMaterial != newMaterial) m_meshRenderer.sharedMaterial = newMaterial;
        }
        
        private void OnEnable()
        {
            m_baseScale = (minScale + maxScale) / 2f;
            m_amplitude = (maxScale - minScale) / 2f;
            m_wait = new WaitForSeconds(updateInterval);

            m_scaleRoutine = StartCoroutine(ScaleRoutine());
            m_meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        
        private void OnDisable()
        {
            if (m_scaleRoutine == null) return;
            
            StopCoroutine(m_scaleRoutine);
            m_scaleRoutine = null;
        }

        /// <summary>
        /// Coroutine that animates the marker scale using a sine wave.
        /// </summary>
        private IEnumerator ScaleRoutine()
        {
            while (true)
            {
                float time = Time.time;
                float scale = m_baseScale + Mathf.Sin(time * speed) * m_amplitude;
                transform.localScale = Vector3.one * scale;
                
                yield return m_wait;
            }
        }
    }
}
