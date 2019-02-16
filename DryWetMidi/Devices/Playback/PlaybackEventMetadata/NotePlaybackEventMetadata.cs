using System;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class NotePlaybackEventMetadata
    {
        #region Constructor

        public NotePlaybackEventMetadata(Note note, bool isNoteOnEvent, TimeSpan startTime, TimeSpan endTime)
        {
            Note = note;
            IsNoteOnEvent = isNoteOnEvent;
            StartTime = startTime;
            EndTime = endTime;
            NoteId = note.GetNoteId();
        }

        #endregion

        #region Properties

        public Note Note { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan EndTime { get; }

        public bool IsNoteOnEvent { get; }

        public NoteId NoteId { get; }

        #endregion
    }
}
