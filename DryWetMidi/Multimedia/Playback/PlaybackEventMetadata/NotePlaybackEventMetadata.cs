using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class NotePlaybackEventMetadata
    {
        #region Constructor

        public NotePlaybackEventMetadata(Note note, TimeSpan startTime, TimeSpan endTime)
        {
            RawNote = note;
            StartTime = startTime;
            EndTime = endTime;

            RawNotePlaybackData = new NotePlaybackData(RawNote.NoteNumber, RawNote.Velocity, RawNote.OffVelocity, RawNote.Channel);
            NotePlaybackData = RawNotePlaybackData;
        }

        #endregion

        #region Properties

        public Note RawNote { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan EndTime { get; }

        public NotePlaybackData RawNotePlaybackData { get; }

        public NotePlaybackData NotePlaybackData { get; private set; }

        public bool IsCustomNotePlaybackDataSet { get; private set; }

        #endregion

        #region Methods

        public Note GetEffectiveNote()
        {
            var notePlaybackData = NotePlaybackData;
            if (notePlaybackData == null)
                return null;

            var note = (Note)RawNote.Clone();

            note.NoteNumber = notePlaybackData.NoteNumber;
            note.Velocity = notePlaybackData.Velocity;
            note.OffVelocity = notePlaybackData.OffVelocity;
            note.Channel = notePlaybackData.Channel;

            return note;
        }

        public void SetCustomNotePlaybackData(NotePlaybackData notePlaybackData)
        {
            NotePlaybackData = notePlaybackData;
            IsCustomNotePlaybackDataSet = true;
        }

        #endregion
    }
}
