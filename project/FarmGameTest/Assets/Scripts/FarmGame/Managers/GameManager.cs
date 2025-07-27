using UnityEngine;
using FarmGame.System;

namespace FarmGame.Managers
{
    /// <summary>
    /// Stores manager subcomponents and injects dependencies.
    /// </summary>
    [RequireComponent(typeof(InputManager)), RequireComponent(typeof(GridManager)),
     RequireComponent(typeof(CropPoolManager)), RequireComponent(typeof(UIManager)),
     RequireComponent(typeof(VFXManager)), RequireComponent(typeof(AudioManager))]
    public class GameManager : Singleton<GameManager>
    {
        // Managers
        private InputManager m_inputManager;
        private GridManager m_gridManager;
        private CropPoolManager m_cropPoolManager;
        private UIManager m_uiManager;
        private VFXManager m_vfxManager;
        private AudioManager m_audioManager;
        
        // Settings
        [SerializeField] private GameSettings gameSettings;
        
        // Other
        private Camera m_camera;
        
        private void Start()
        {
            // Get camera reference
            m_camera = Camera.main;
            
            // Get manager references
            m_inputManager = GetComponent<InputManager>();
            m_gridManager = GetComponent<GridManager>();
            m_cropPoolManager = GetComponent<CropPoolManager>();
            m_uiManager = GetComponent<UIManager>();
            m_vfxManager = GetComponent<VFXManager>();
            m_audioManager = GetComponent<AudioManager>();
            
            // Inject dependencies
            m_inputManager.Initialize();
            m_gridManager.Initialize(gameSettings, m_cropPoolManager, m_camera);
            m_cropPoolManager.Initialize(gameSettings);
            m_uiManager.Initialize(m_gridManager, m_camera);
            m_vfxManager.Initialize(m_camera);
            m_audioManager.Initialize();
        }
    }
}
