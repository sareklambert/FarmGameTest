using System;
using System.Collections.Generic;

namespace FarmGame.System
{
    /// <summary>
    /// This class defines a generic event bus. Used for sending events between objects.
    /// Events are set up as custom classes to be able to carry any data we need.
    /// </summary>
    public static class EventBus
    {
        // Set up event dictionary
        private static Dictionary<Type, Delegate> m_events = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribes a listener to a specific event type.
        /// </summary>
        /// <typeparam name="T">Type of the event to subscribe to.</typeparam>
        /// <param name="listener">Callback to invoke when the event is published.</param>
        public static void Subscribe<T>(Action<T> listener)
        {
            if (!m_events.ContainsKey(typeof(T))) m_events[typeof(T)] = null;
            m_events[typeof(T)] = (Action<T>)m_events[typeof(T)] + listener;
        }

        /// <summary>
        /// Unsubscribes a listener from a specific event type.
        /// </summary>
        /// <typeparam name="T">Type of the event to unsubscribe from.</typeparam>
        /// <param name="listener">Callback to remove.</param>
        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (m_events.ContainsKey(typeof(T))) m_events[typeof(T)] = (Action<T>)m_events[typeof(T)] - listener;
        }

        /// <summary>
        /// Publishes an event to all subscribed listeners.
        /// </summary>
        /// <typeparam name="T">Type of the event being published.</typeparam>
        /// <param name="eventData">The event data to send to listeners.</param>
        public static void Publish<T>(T eventData)
        {
            if (m_events.ContainsKey(typeof(T))) ((Action<T>)m_events[typeof(T)])?.Invoke(eventData);
        }
    }
}
