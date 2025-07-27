using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.World.Crops;

namespace FarmGame.World.AI
{
    /// <summary>
    /// Worker AI that moves toward crops in a specific state, changes their state,
    /// then returns to its original position.
    /// </summary>
    public class WorkerAI : MonoBehaviour
    {
        [Header("Crop state settings")]
        [SerializeField] private CropState targetState;
        [SerializeField] private CropState nextState;

        [Header("Movement settings")]
        [SerializeField] private float movementSpeed = 1f;
        [SerializeField] private float rotationSpeed = 20f;
        [SerializeField] private float arrivalThreshold = .1f;
        
        // Animation
        private Animator m_animator;
        private readonly int m_isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int m_interactionHash = Animator.StringToHash("Interaction");
        
        // Movement
        private Coroutine m_movementRoutine;
        
        private Quaternion m_startRotation;
        private Quaternion m_targetRotation;
        private Vector3 m_startPosition;
        private Vector3 m_targetPosition;
        private float m_sqrThreshold;
        private float m_currentSqrThreshold;

        private readonly Queue<Crop> m_targetCrops = new Queue<Crop>();
        
        private void OnEnable()
        {
            m_animator = GetComponent<Animator>();
            
            m_movementRoutine = StartCoroutine(MovementRoutine());
            
            m_startRotation = transform.rotation;
            m_startPosition = transform.position;
            m_targetPosition = Vector3.zero;
            m_sqrThreshold = arrivalThreshold * arrivalThreshold;
            m_currentSqrThreshold = 0f;
            
            EventsSubscribe();
        }
        
        private void OnDisable()
        {
            if (m_movementRoutine == null) return;
            
            StopCoroutine(m_movementRoutine);
            m_movementRoutine = null;
            
            EventsUnsubscribe();
        }
        
        /// <summary>
        /// Continuously moves toward the next crop in the target queue,
        /// transitions its state, and returns to idle position when done.
        /// </summary>
        private IEnumerator MovementRoutine()
        {
            bool wasWalking = false;

            while (true)
            {
                if (m_targetPosition == Vector3.zero)
                {
                    // Acquire target
                    AcquireNextTarget();
                }
                else
                {
                    if ((transform.position - m_targetPosition).sqrMagnitude > m_currentSqrThreshold)
                    {
                        // Move
                        transform.position = Vector3.MoveTowards(transform.position, m_targetPosition,
                            movementSpeed * Time.deltaTime);
                    
                        SetWalking(true, ref wasWalking);
                    }
                    else
                    {
                        // Arrived at target position
                        if (m_targetCrops.Count > 0 && m_targetCrops.Peek().transform.position == m_targetPosition)
                        {
                            // Play interaction
                            SetWalking(false, ref wasWalking);
                            m_animator.SetTrigger(m_interactionHash);
                            
                            // Wait for interaction animation to finish
                            yield return WaitForInteractionAnimation();
                        }

                        m_targetPosition = Vector3.zero;
                        SetWalking(false, ref wasWalking);
                    }
                }

                // Rotate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_targetRotation,
                    rotationSpeed * Time.deltaTime);

                yield return null;
            }
        }

        /// <summary>
        /// Coroutine that yields until the Interaction clip has fully played,
        /// and keeps rotating toward m_targetRotation while waiting.
        /// </summary>
        private IEnumerator WaitForInteractionAnimation()
        {
            AnimatorStateInfo stateInfo;

            // Wait until we've actually entered the Interaction state
            do
            {
                // Rotate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_targetRotation,
                    rotationSpeed * Time.deltaTime);

                stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            while (stateInfo.shortNameHash != m_interactionHash);

            // Wait until the animation finishes
            while (stateInfo.normalizedTime < 1f)
            {
                // Rotate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_targetRotation,
                    rotationSpeed * Time.deltaTime);

                stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
        }

        /// <summary>
        /// Determines the next target position and rotation:
        /// if there are crops queued, targets the next crop;
        /// otherwise moves back to the starting position or stays idle.
        /// </summary>
        private void AcquireNextTarget()
        {
            if (m_targetCrops.Count > 0)
            {
                Crop nextTarget = m_targetCrops.Peek();
                m_targetPosition = nextTarget.transform.position;
                m_targetRotation = Quaternion.LookRotation((m_targetPosition - transform.position).normalized);
                
                m_currentSqrThreshold = m_sqrThreshold;
            }
            else if ((transform.position - m_startPosition).sqrMagnitude > m_sqrThreshold)
            {
                m_targetPosition = m_startPosition;
                m_targetRotation = Quaternion.LookRotation((m_startPosition - transform.position).normalized);
                
                m_currentSqrThreshold = 0f;
            }
            else
            {
                m_targetPosition = Vector3.zero;
                m_targetRotation = m_startRotation;
                
                m_currentSqrThreshold = 0f;
            }
        }

        /// <summary>
        /// Updates the walking animation state when <paramref name="walking"/> differs from
        /// the previous frame’s <paramref name="previousState"/>.
        /// </summary>
        private void SetWalking(bool walking, ref bool previousState)
        {
            if (walking == previousState) return;
            
            m_animator.SetBool(m_isWalkingHash, walking);
            previousState = walking;
        }

        #region Events
        private void EventsSubscribe()
        {
            EventBus.Subscribe<EventCropAdvanceState>(OnCropAdvanceState);
        }

        private void EventsUnsubscribe()
        {
            EventBus.Unsubscribe<EventCropAdvanceState>(OnCropAdvanceState);
        }

        /// <summary>
        /// Event triggered by the interaction animation.
        /// </summary>
        public void OnAnimationEventInteraction()
        {
            Crop crop = m_targetCrops.Dequeue();
            crop.SetState(nextState);
        }
        
        /// <summary>
        /// Adds a crop to the target queue if it has entered the desired target state.
        /// </summary>
        private void OnCropAdvanceState(EventCropAdvanceState eventCropAdvanceState)
        {
            if (eventCropAdvanceState.TargetCrop.State == targetState)
            {
                m_targetCrops.Enqueue(eventCropAdvanceState.TargetCrop);
            }
        }
        #endregion
    }
}
