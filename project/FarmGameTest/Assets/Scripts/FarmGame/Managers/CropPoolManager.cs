using System.Collections.Generic;
using UnityEngine;
using FarmGame.System;
using FarmGame.World.Crops;

namespace FarmGame.Managers
{
    /// <summary>
    /// Handles object pooling for crops using the generic ObjectPool.
    /// </summary>
    [DisallowMultipleComponent]
    public class CropPoolManager : MonoBehaviour
    {
        [SerializeField] private Crop cropPrefab;
        [SerializeField] private Transform cropRoot;

        /// <summary>
        /// The pool of Crop instances.
        /// </summary>
        private ObjectPool<Crop> m_pool;

        /// <summary>
        /// All currently active (in‑use) crops.
        /// </summary>
        public List<Crop> ActiveCrops { get; private set; } = new List<Crop>();

        /// <summary>
        /// Initializes the pool to the size of the world grid and prewarms it.
        /// </summary>
        public void Initialize(GameSettings settings)
        {
            int maxInstances = settings.gridSizeX * settings.gridSizeZ;
            m_pool = new ObjectPool<Crop>(
                prefab:        cropPrefab,
                initialSize:   maxInstances,
                parent:        cropRoot,
                onGet:         OnGetFromPool,
                onRelease:     OnReleaseToPool,
                prewarm:       true
            );
        }

        /// <summary>
        /// Retrieves a crop from the pool.
        /// </summary>
        public Crop GetCrop()
        {
            return m_pool.Get();
        }

        /// <summary>
        /// Returns a crop to the pool.
        /// </summary>
        public void ReleaseCrop(Crop crop)
        {
            m_pool.Release(crop);
        }

        /// <summary>
        /// Callback invoked when a crop is fetched from the pool.
        /// Adds it to the active crop list.
        /// </summary>
        private void OnGetFromPool(Crop crop)
        {
            ActiveCrops.Add(crop);
        }

        /// <summary>
        /// Callback invoked when a crop is returned to the pool.
        /// Removes it from the active crop list.
        /// </summary>
        private void OnReleaseToPool(Crop crop)
        {
            ActiveCrops.Remove(crop);
        }
    }
}
