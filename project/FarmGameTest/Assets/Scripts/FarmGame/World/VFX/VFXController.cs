using System.Collections;
using UnityEngine;
using FarmGame.System;

namespace FarmGame.World.VFX
{
    /// <summary>
    /// Controls the lifetime of a VFX instance by playing its child <see cref="ParticleSystem"/> components
    /// and returning it to an <see cref="ObjectPool{VFXController}"/> or destroying it when complete.
    /// </summary>
    public class VFXController : MonoBehaviour
    {
        private ObjectPool<VFXController> m_pool;
        private ParticleSystem[] m_particleSystems;
        
        /// <summary>
        /// Initializes this VFXController with its managing pool.
        /// </summary>
        /// <param name="pool">
        /// The <see cref="ObjectPool{VFXController}"/> that will manage return of this instance.
        /// </param>
        public void Initialize(ObjectPool<VFXController> pool)
        {
            m_pool = pool;
        }
        
        /// <summary>
        /// Returns this instance to its pool if available; otherwise destroys the GameObject.
        /// </summary>
        private void Delete()
        {
            if (m_pool != null)
            {
                m_pool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Caches all child <see cref="ParticleSystem"/> components for later duration calculation.
        /// </summary>
        private void Awake()
        {
            m_particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
        
        /// <summary>
        /// Starts the coroutine that waits for the effects to finish before returning.
        /// </summary>
        private void OnEnable()
        {
            StartCoroutine(HandleReturn());
        }
        
        /// <summary>
        /// Waits for the longest-running particle system to complete (plus a small buffer),
        /// then calls <see cref="Delete"/> to recycle or destroy this instance.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> for use with Unity coroutines.
        /// </returns>
        private IEnumerator HandleReturn()
        {
            float maxDuration = 0f;
            
            foreach (ParticleSystem ps in m_particleSystems)
            {
                if (ps.main.duration > maxDuration) maxDuration = ps.main.duration;
            }
            yield return new WaitForSeconds(maxDuration + 0.5f);
            
            Delete();
        }
    }
}
