using UnityEngine;

namespace FarmGame.Events
{
    /// <summary>
    /// Input event: Tap.
    /// </summary>
    public class EventInputTap
    {
        public Vector2 Position { get; private set; }

        public EventInputTap(Vector2 position)
        {
            Position = position;
        }
    }
}
