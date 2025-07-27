using System.Collections;
using UnityEngine;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.World.VFX;

namespace FarmGame.World.Crops
{
    /// <summary>
    /// Represents a single crop instance that transitions through growth stages,
    /// handles visual updates, and plays animations and VFX during state changes.
    /// </summary>
    public class Crop : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.02f;
        
        [Header("Animation Settings")]
        [SerializeField] private float wobbleDuration = 0.5f;
        [SerializeField] private float wobbleFrequency = 8f;
        [SerializeField] private float wobbleDamping = 5f;
        [SerializeField] private float wobbleAmplitude = 1.2f;
        
        private WaitForSeconds m_wait;
        private Coroutine m_animationRoutine;
        
        private MeshFilter m_meshFilter;
        private MeshRenderer m_meshRenderer;
        
        private CropData m_cropData;
        private CropState m_cropState;
        public CropState State => m_cropState;
        public CropData CropData => m_cropData;

        private int m_growTimer;
        private CropState m_nextState;
        
        /// <summary>
        /// Sets crop data and initializes the first state.
        /// </summary>
        public void Initialize(CropData cropData)
        {
            m_cropData = cropData;
            SetState(CropState.Seed);
        }

        /// <summary>
        /// Transitions the crop into a new state and handles visuals and animations.
        /// Also sets a timer and the next expected state, if applicable.
        /// </summary>
        public void SetState(CropState state)
        {
            m_cropState = state;
            m_nextState = CropState.None;

            switch (m_cropState)
            {
                case CropState.Seed:
                    SetVisuals(m_cropData.visuals[0]);
                    PlayAnimationOneShot();
                    
                    m_growTimer = m_cropData.growthTimeStage1;
                    m_nextState = CropState.WaterNeeded;
                    break;
                case CropState.Sprout:
                    SetVisuals(m_cropData.visuals[1]);
                    PlayAnimationOneShot();
                    
                    m_growTimer = m_cropData.growthTimeStage2;
                    m_nextState = CropState.HarvestNeeded;
                    break;
                case CropState.HarvestNeeded:
                    SetVisuals(m_cropData.visuals[2]);
                    PlayAnimationOneShot();
                    break;
            }
            
            EventBus.Publish(new EventCropAdvanceState(this));
        }
        
        /// <summary>
        /// Advances the crop toward the next state, if one is set and its timer has expired.
        /// </summary>
        public void Tick()
        {
            if (m_nextState == CropState.None) return;
            
            m_growTimer --;
            if (m_growTimer > 0) return;
            
            SetState(m_nextState);
        }
        
        private void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshRenderer = GetComponent<MeshRenderer>();
            
            m_wait = new WaitForSeconds(updateInterval);
        }
        
        /// <summary>
        /// Applies the correct mesh and material for the crop’s current stage.
        /// Chooses instanced or fallback materials based on hardware support.
        /// </summary>
        private void SetVisuals(CropVisual visuals)
        {
            m_meshFilter.sharedMesh = visuals.mesh;
            m_meshRenderer.sharedMaterial = SystemInfo.supportsInstancing ?
                visuals.materialInstanced : visuals.materialFallback;
        }

        /// <summary>
        /// Starts a one-shot spring-like scale animation to emphasize growth or transition.
        /// </summary>
        private void PlayAnimationOneShot()
        {
            if (m_animationRoutine != null) StopCoroutine(m_animationRoutine);
            
            m_animationRoutine = StartCoroutine(AnimationRoutine());
        }
        
        /// <summary>
        /// Coroutine that applies a decaying sine wave to the crop's scale for a "wobble" effect.
        /// </summary>
        private IEnumerator AnimationRoutine()
        {
            // Apply wobble spring tween animation
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < wobbleDuration)
            {
                elapsed += updateInterval;
                float t = elapsed / wobbleDuration;

                float spring = Mathf.Exp(-wobbleDamping * t) *
                               Mathf.Cos(wobbleFrequency * t * Mathf.PI * 2f);

                float scaleFactor = 1f + (spring * (wobbleAmplitude - 1f));
                transform.localScale = originalScale * scaleFactor;

                yield return m_wait;
            }

            // Ensure we end exactly at original scale
            transform.localScale = originalScale;
            m_animationRoutine = null;
        }
    }
}
