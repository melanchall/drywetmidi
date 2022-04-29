using System;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TimedEventDetectionSettings
    {
        #region Properties

        public Func<TimedEventData, TimedEvent> Constructor { get; set; }

        #endregion
    }
}
