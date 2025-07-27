using UnityEngine;

namespace FarmGame.System
{
    /// <summary>
    /// This class defines a generic singleton for other classes to inherit from.
    /// Ensures only one instance of a component type exists in the scene.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        // Define global instance variable
        private static T m_instance;

        /// <summary>
        /// Globally accessible instance of the singleton.
        /// Automatically searches or creates one if it doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                // Return the global instance; Set up one if there is none
                if (!m_instance)
                {
                    m_instance = (T)FindAnyObjectByType(typeof(T));
                    if (!m_instance)
                    {
                        SetupInstance();
                    }
                }

                return m_instance;
            }
        }

        // Make sure the instance is unique
        public virtual void Awake()
        {
            RemoveDuplicates();
        }

        /// <summary>
        /// Creates a new instance of the singleton if one does not already exist in the scene.
        /// </summary>
        private static void SetupInstance()
        {
            // Make sure there isn't another instance already
            m_instance = (T)FindAnyObjectByType(typeof(T));
            if (m_instance) return;

            // Set up a new game object with the singleton component
            GameObject gameObj = new GameObject
            {
                name = typeof(T).Name
            };
            m_instance = gameObj.AddComponent<T>();
            DontDestroyOnLoad(gameObj);
        }

        /// <summary>
        /// Ensures that only one instance of the singleton exists by destroying duplicates.
        /// </summary>
        private void RemoveDuplicates()
        {
            if (!m_instance)
            {
                m_instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
