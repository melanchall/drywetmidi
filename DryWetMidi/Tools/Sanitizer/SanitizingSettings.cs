using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SanitizingSettings
    {
        #region Fields

        #endregion

        #region Properties

        public ITimeSpan NoteMinLength { get; set; }

        public NoteDetectionSettings NoteDetectionSettings { get; set; }

        public bool RemoveEmptyTrackChunks { get; set; } = true;

        public bool RemoveOrphanedNoteOnEvents { get; set; } = true;

        public bool RemoveOrphanedNoteOffEvents { get; set; } = true;

        public bool RemoveDuplicatedSetTempoEvents { get; set; } = true;

        public bool RemoveDuplicatedTimeSignatureEvents { get; set; } = true;

        public bool RemoveDuplicatedPitchBendEvents { get; set; } = true;

        public bool RemoveEventsOnUnusedChannels { get; set; } = true;

        #endregion
    }
}
