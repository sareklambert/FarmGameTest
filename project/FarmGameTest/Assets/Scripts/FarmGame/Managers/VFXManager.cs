using System.Collections.Generic;
using UnityEngine;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.World.VFX;

namespace FarmGame.Managers
{
    /// <summary>
    /// Manages playback of visual effects
    /// </summary>
    [DisallowMultipleComponent]
    public class VFXManager : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private List<VFXController> effectPrefabs;
        [SerializeField] private int defaultPoolSize = 5;
        [SerializeField] private Transform vfxRoot;

        private Camera m_camera;
        
        /// <summary>
        /// Pools for each VFX prefab type, mapped by prefab reference.
        /// </summary>
        private readonly Dictionary<VFXController, ObjectPool<VFXController>>
            m_effectPools = new Dictionary<VFXController, ObjectPool<VFXController>>();
        
        /// <summary>
        /// Initializes object pools and subscribes to crop-related events.
        /// </summary>
        public void Initialize(Camera mainCamera)
        {
            m_camera = mainCamera;
            
            // Create object pools for each VFX
            foreach (VFXController prefab in effectPrefabs)
            {
                CreatePool(prefab);
            }
            
            EventsSubscribe();
        }
        
        /// <summary>
        /// Destroys all pooled instances and clears each pool.
        /// </summary>
        public void ClearPools()
        {
            foreach (ObjectPool<VFXController> pool in m_effectPools.Values)
            {
                pool.Clear();
            }
        }
        
        /// <summary>
        /// Creates an <see cref="ObjectPool{VFXController}"/> for the given prefab
        /// and stores it in the internal dictionary.
        /// </summary>
        /// <param name="prefab">The VFXController prefab to pool.</param>
        private void CreatePool(VFXController prefab)
        {
            ObjectPool<VFXController> pool = null;
            
            pool = new ObjectPool<VFXController>(
                prefab:        prefab,
                initialSize:   defaultPoolSize,
                parent:        vfxRoot,
                onGet:         effect => effect.Initialize(pool),
                onRelease:     _ => { },
                prewarm:       true
            );
            
            m_effectPools[prefab] = pool;
        }
        
        /// <summary>
        /// Plays a VFX at the specified transform by fetching an instance from its pool.
        /// </summary>
        /// <param name="vfx">The VFXController prefab to play.</param>
        /// <param name="t">The transform at which to play the effect.</param>
        private void PlayEffect(VFXController vfx, Transform t)
        {
            VFXController effect = m_effectPools[vfx].Get();
            
            Vector3 toCamera = (m_camera.transform.position - t.position).normalized;
            effect.transform.position = t.position + toCamera;
            effect.transform.rotation = Quaternion.LookRotation(toCamera);
        }
        
        private void OnDisable()
        {
            EventsUnsubscribe();
        }

        #region Events
        private void EventsSubscribe()
        {
            EventBus.Subscribe<EventCropHarvested>(OnCropHarvested);
        }

        private void EventsUnsubscribe()
        {
            EventBus.Unsubscribe<EventCropHarvested>(OnCropHarvested);
        }
        
        private void OnCropHarvested(EventCropHarvested eventCropHarvested)
        {
            PlayEffect(eventCropHarvested.VFX, eventCropHarvested.TargetCrop.transform);
        }
        
        #endregion
    }
}
