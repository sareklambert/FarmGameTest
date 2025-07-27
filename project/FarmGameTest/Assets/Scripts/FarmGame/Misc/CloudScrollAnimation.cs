using UnityEngine;
using System.Collections;

namespace FarmGame.Misc
{
    /// <summary>
    /// Continuously scrolls the main texture of a material to simulate cloud movement.
    /// </summary>
    public class CloudScrollAnimation : MonoBehaviour
    {
        [SerializeField] private float scrollSpeedX = 0.01f;
        [SerializeField] private float scrollSpeedY = 0.01f;
        [SerializeField] private float updateInterval = 0.02f;

        private Material m_material;
        private Vector2 m_offset;

        private WaitForSeconds m_wait;
        private Coroutine m_updateOffsetRoutine;
        
        private void OnEnable()
        {
            m_material = GetComponent<Renderer>().sharedMaterial;
            m_wait = new WaitForSeconds(updateInterval);

            m_updateOffsetRoutine = StartCoroutine(UpdateOffsetRoutine());
        }
        
        private void OnDisable()
        {
            if (m_updateOffsetRoutine == null) return;
            
            StopCoroutine(m_updateOffsetRoutine);
            m_updateOffsetRoutine = null;
        }
        
        /// <summary>
        /// Coroutine that increments texture offset over time based on scroll speeds.
        /// </summary>
        private IEnumerator UpdateOffsetRoutine()
        {
            while (true)
            {
                m_offset.x += scrollSpeedX * updateInterval;
                m_offset.y += scrollSpeedY * updateInterval;
                m_material.mainTextureOffset = m_offset;

                yield return m_wait;
            }
        }
    }
}
