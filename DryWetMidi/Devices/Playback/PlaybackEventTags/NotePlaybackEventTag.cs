using System;
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

        public NotePlaybackEventTag(Note note, NoteBound noteBound, TimeSpan startTime, TimeSpan endTime)
        {
            Note = note;
            Bound = noteBound;
            StartTime = startTime;
            EndTime = endTime;
        }

        #endregion

        #region Properties

        public Note Note { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan EndTime { get; }

        public NoteBound Bound { get; }

        #endregion
    }
}
