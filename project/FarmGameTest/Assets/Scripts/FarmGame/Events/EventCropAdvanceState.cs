using FarmGame.World.Crops;

namespace FarmGame.Events
{
    /// <summary>
    /// Crop advanced a state (planted, needs water, marked, etc.).
    /// </summary>
    public class EventCropAdvanceState
    {
        public Crop TargetCrop { get; private set; }

        public EventCropAdvanceState(Crop crop)
        {
            TargetCrop = crop;
        }
    }
}
