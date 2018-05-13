using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class QuantizingSettings
    {
        #region Properties

        public TimeSpanType DistanceType { get; set; } = TimeSpanType.Midi;

        #endregion
    }
}
