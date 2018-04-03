using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class QuantizingSettings
    {
        #region Properties

        // TODO: think about eliminating -> new tool Randomizer
        public ITimeSpan Tolerance { get; set; }

        #endregion
    }
}
