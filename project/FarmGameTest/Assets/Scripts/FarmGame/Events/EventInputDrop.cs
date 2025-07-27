using UnityEngine;

namespace FarmGame.Events
{
    /// <summary>
    /// Input event: Drop.
    /// </summary>
    public class EventInputDrop
    {
        public Vector2 Position { get; private set; }

        public EventInputDrop(Vector2 position)
        {
            Position = position;
        }
    }
}