using UnityEngine;
using FarmGame.System;
using FarmGame.Events;
using FarmGame.World.Crops;

namespace FarmGame.Managers
{
    /// <summary>
    /// Manages playback of background music and sound effects in response to game events.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip bgmClipMain;
        [SerializeField] private AudioClip sfxClipUIButtonTap;
        [SerializeField] private AudioClip sfxClipUIDragStart;
        [SerializeField] private AudioClip sfxClipCropPlant;
        [SerializeField] private AudioClip sfxClipCropWater;
        [SerializeField] private AudioClip sfxClipCropHarvest;

        /// <summary>
        /// Configures and begins looping the background music from the main BGM clip.
        /// </summary>
        public void Initialize()
        {
            bgmAudioSource.clip = bgmClipMain;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
        
        /// <summary>
        /// Plays the specified sound effect clip with a slight random pitch variation for auditory variety.
        /// </summary>
        /// <param name="clip">The AudioClip to play as a one-shot SFX.</param>
        private void PlaySfx(AudioClip clip)
        {
            if (!sfxAudioSource || !clip) return;

            sfxAudioSource.pitch = Random.Range(.95f, 1.05f);
            sfxAudioSource.PlayOneShot(clip);
        }
        
        private void OnEnable()
        {
            EventsSubscribe();
        }
        
        private void OnDisable()
        {
            EventsUnsubscribe();
        }

        #region Events
        private void EventsSubscribe()
        {
            EventBus.Subscribe<EventUICropDragStart>(OnUICropDragStart);
            EventBus.Subscribe<EventCropAdvanceState>(OnCropAdvanceState);
            EventBus.Subscribe<EventInputPlantMode>(OnPlantModeActivated);
            EventBus.Subscribe<EventInputWaterMode>(OnWaterModeActivated);
            EventBus.Subscribe<EventInputHarvestMode>(OnHarvestModeActivated);
        }

        private void EventsUnsubscribe()
        {
            EventBus.Unsubscribe<EventUICropDragStart>(OnUICropDragStart);
            EventBus.Unsubscribe<EventCropAdvanceState>(OnCropAdvanceState);
            EventBus.Unsubscribe<EventInputPlantMode>(OnPlantModeActivated);
            EventBus.Unsubscribe<EventInputWaterMode>(OnWaterModeActivated);
            EventBus.Unsubscribe<EventInputHarvestMode>(OnHarvestModeActivated);
        }

        private void OnUICropDragStart(EventUICropDragStart _)
        {
            PlaySfx(sfxClipUIDragStart);
        }

        private void OnCropAdvanceState(EventCropAdvanceState eventCropAdvanceState)
        {
            CropType cropType = eventCropAdvanceState.TargetCrop.CropData.cropType;
            
            switch (eventCropAdvanceState.TargetCrop.State)
            {
                case CropState.Seed:
                    PlaySfx(sfxClipCropPlant);
                    break;
                case CropState.WaterMarked:
                case CropState.HarvestMarked:
                    PlaySfx(sfxClipUIButtonTap);
                    break;
                case CropState.Sprout:
                    PlaySfx(sfxClipCropWater);
                    break;
                case CropState.None:
                    PlaySfx(sfxClipCropHarvest);
                    break;
            }
        }
        
        private void OnPlantModeActivated(EventInputPlantMode _)
        {
            PlaySfx(sfxClipUIButtonTap);
        }

        private void OnWaterModeActivated(EventInputWaterMode _)
        {
            PlaySfx(sfxClipUIButtonTap);
        }

        private void OnHarvestModeActivated(EventInputHarvestMode _)
        {
            PlaySfx(sfxClipUIButtonTap);
        }

        #endregion
    }
}
