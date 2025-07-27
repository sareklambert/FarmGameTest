using FarmGame.World.Crops;
using FarmGame.World.VFX;

namespace FarmGame.Events
{
    /// <summary>
    /// A crop was harvested.
    /// </summary>
    public class EventCropHarvested
    {
        public Crop TargetCrop { get; private set; }
        public VFXController VFX { get; private set; }

        public EventCropHarvested(Crop crop, VFXController vfx)
        {
            TargetCrop = crop;
            VFX = vfx;
        }
    }
}
