using System;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class NotePlaybackEventMetadata
    {
        #region Constructor

        public NotePlaybackEventMetadata(Note note, TimeSpan startTime, TimeSpan endTime)
        {
            Note = note;
            StartTime = startTime;
            EndTime = endTime;
            NoteId = note.GetNoteId();

            RawNotePlaybackData = new NotePlaybackData(Note.NoteNumber, Note.Velocity, Note.OffVelocity, Note.Channel);
            NotePlaybackData = RawNotePlaybackData;
        }

        #endregion

        #region Properties

        public Note Note { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan EndTime { get; }

        public NoteId NoteId { get; }

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

            var note = Note.Clone();

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

        public void SetRawNotePlaybackData()
        {
            NotePlaybackData = RawNotePlaybackData;
            IsCustomNotePlaybackDataSet = false;
        }

        #endregion
    }
}
