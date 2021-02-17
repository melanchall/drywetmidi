using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SplitFileByChannelSettings
    {
        #region Properties

        public bool CopyNonChannelEventsToEachFile { get; set; } = true;

        public Predicate<TimedEvent> TimedEventsFilter { get; set; }

        #endregion
    }
}
