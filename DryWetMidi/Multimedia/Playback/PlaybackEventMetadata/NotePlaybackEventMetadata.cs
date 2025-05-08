using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class NotePlaybackEventMetadata : IInterval<TimeSpan>
    {
        #region Constructor

        public NotePlaybackEventMetadata(
            Note note,
            PlaybackTime startTime,
            PlaybackTime endTime)
        {
            RawNote = note;
            RawNoteId = note.GetNoteId();
            StartTime = startTime;
            EndTime = endTime;

            RawNotePlaybackData = new NotePlaybackData(RawNote.NoteNumber, RawNote.Velocity, RawNote.OffVelocity, RawNote.Channel);
            NotePlaybackData = RawNotePlaybackData;
        }

        #endregion

        #region Properties

        public Note RawNote { get; }

        public NoteId RawNoteId { get; }

        public PlaybackTime StartTime { get; }

        public PlaybackTime EndTime { get; }

        public NotePlaybackData RawNotePlaybackData { get; }

        public NotePlaybackData NotePlaybackData { get; private set; }

        public bool IsCustomNotePlaybackDataSet { get; private set; }

        public TimeSpan Start => StartTime.Time;

        public TimeSpan End => EndTime.Time;

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
