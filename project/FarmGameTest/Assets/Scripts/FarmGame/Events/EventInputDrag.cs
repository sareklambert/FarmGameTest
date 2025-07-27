using UnityEngine;

namespace FarmGame.Events
{
    /// <summary>
    /// Input event: Drag.
    /// </summary>
    public class EventInputDrag
    {
        public Vector2 Position { get; private set; }

        public EventInputDrag(Vector2 position)
        {
            Position = position;
        }
    }
}
