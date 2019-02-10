using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class NotePlaybackEventTag : IPlaybackEventTag
    {
        #region Nested types

        public enum NoteBound
        {
            Start,
            End
        }

        #endregion

        #region Constructor

        public NotePlaybackEventTag(Note note, NoteBound noteBound)
        {
            Note = note;
            Bound = noteBound;
        }

        #endregion

        #region Properties

        public Note Note { get; }

        public NoteBound Bound { get; }

        #endregion
    }
}
