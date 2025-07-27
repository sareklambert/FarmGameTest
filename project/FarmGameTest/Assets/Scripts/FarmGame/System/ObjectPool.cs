using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.System
{
    /// <summary>
    /// Implements a simple generic object pool for <typeparamref name="T"/> instances.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T m_prefab;
        private readonly Stack<T> m_stack;
        private readonly Transform m_parent;
        private readonly Action<T> m_onGet;
        private readonly Action<T> m_onRelease;
        
        public int Count => m_stack.Count;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="prefab">
        /// The prefab of type <typeparamref name="T"/> used to instantiate new objects.
        /// </param>
        /// <param name="initialSize">Initial capacity of the pool.</param>
        /// <param name="parent">
        /// Optional parent <see cref="Transform"/> under which new instances will be created.
        /// </param>
        /// <param name="onGet">
        /// Optional callback invoked whenever an object is retrieved from the pool.
        /// </param>
        /// <param name="onRelease">
        /// Optional callback invoked whenever an object is returned to the pool.
        /// </param>
        /// <param name="prewarm">
        /// If true, immediately pre-instantiates <paramref name="initialSize"/> objects.
        /// </param>
        public ObjectPool(T prefab, int initialSize = 0, Transform parent = null, Action<T> onGet = null,
            Action<T> onRelease = null, bool prewarm = false)
        {
            m_prefab = prefab;
            m_parent = parent;
            m_onGet = onGet;
            m_onRelease = onRelease;
            m_stack = new Stack<T>(initialSize);
            
            if (prewarm) Prewarm(initialSize);
        }
        
        /// <summary>
        /// Retrieves an object from the pool, or creates one if none are available.
        /// </summary>
        /// <returns>An active instance of <typeparamref name="T"/>.</returns>
        public T Get()
        {
            T instance = (m_stack.Count > 0) ? m_stack.Pop() : CreateInstance();
            
            instance.gameObject.SetActive(true);
            m_onGet?.Invoke(instance);
            return instance;
        }
        
        /// <summary>
        /// Returns an object to the pool for later reuse.
        /// </summary>
        /// <param name="instance">The instance to release back into the pool.</param>
        public void Release(T instance)
        {
            m_onRelease?.Invoke(instance);
            instance.gameObject.SetActive(false);
            m_stack.Push(instance);
        }
        
        /// <summary>
        /// Destroys all pooled instances and clears the pool.
        /// </summary>
        public void Clear()
        {
            while (m_stack.Count > 0)
            {
                T inst = m_stack.Pop();
                UnityEngine.Object.Destroy(inst.gameObject);
            }
        }
        
        /// <summary>
        /// Creates a new instance of the prefab.
        /// </summary>
        /// <remarks>
        /// The instance is parented under <see cref="m_parent"/> (if provided)
        /// and initially deactivated.
        /// </remarks>
        /// <returns>A new, inactive instance of <typeparamref name="T"/>.</returns>
        private T CreateInstance()
        {
            T inst = UnityEngine.Object.Instantiate(m_prefab, m_parent);
            inst.gameObject.SetActive(false);
            return inst;
        }
        
        /// <summary>
        /// Pre-instantiates a given number of instances into the pool.
        /// </summary>
        /// <param name="initialSize">The number of instances to prewarm.</param>
        private void Prewarm(int initialSize)
        {
            if (m_stack.Count > 0) return;
            
            for (int i = 0; i < initialSize; ++i) m_stack.Push(CreateInstance());
        }
    }
}
