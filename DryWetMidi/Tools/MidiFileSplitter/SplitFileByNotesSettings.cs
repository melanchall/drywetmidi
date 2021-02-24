using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SplitFileByNotesSettings
    {
        #region Properties

        public bool CopyNonNoteEventsToEachFile { get; set; } = true;

        public Predicate<TimedEvent> TimedEventsFilter { get; set; }

        public bool IgnoreChannel { get; set; }

        #endregion
    }
}
