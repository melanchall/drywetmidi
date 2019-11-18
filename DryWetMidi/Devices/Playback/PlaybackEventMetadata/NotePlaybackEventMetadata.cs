using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// A class encapsulating metadata related to the playback of an event.
    /// </summary>
    public sealed class NotePlaybackEventMetadata
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotePlaybackEventMetadata"/> class.
        /// </summary>
        /// <param name="note">The current musical note.</param>
        /// <param name="startTime">The start time of the note.</param>
        /// <param name="endTime">The end time of the node.</param>
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

        /// <summary>
        /// Gets the raw musical note.
        /// </summary>
        public Note RawNote { get; }

        /// <summary>
        /// Gets the start time of the note.
        /// </summary>
        public TimeSpan StartTime { get; }

        /// <summary>
        /// Gets the end time of the note.
        /// </summary>
        public TimeSpan EndTime { get; }

        /// <summary>
        /// Gets the playback data associated with the raw musical note.
        /// </summary>
        public NotePlaybackData RawNotePlaybackData { get; }

        /// <summary>
        /// Gets the playback data associated with the musical note.
        /// </summary>
        public NotePlaybackData NotePlaybackData { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the custom musical note playback data has been set.
        /// </summary>
        public bool IsCustomNotePlaybackDataSet { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the effective musical note.
        /// </summary>
        /// <returns>The effective musical note.</returns>
        public Note GetEffectiveNote()
        {
            var notePlaybackData = NotePlaybackData;
            if (notePlaybackData == null)
                return null;

            var note = RawNote.Clone();

            note.NoteNumber = notePlaybackData.NoteNumber;
            note.Velocity = notePlaybackData.Velocity;
            note.OffVelocity = notePlaybackData.OffVelocity;
            note.Channel = notePlaybackData.Channel;

            return note;
        }

        /// <summary>
        /// Sets the custom musical note playback data.
        /// </summary>
        /// <param name="notePlaybackData">The custom musical note playback data to set.</param>
        public void SetCustomNotePlaybackData(NotePlaybackData notePlaybackData)
        {
            ThrowIfArgument.IsNull(nameof(notePlaybackData), notePlaybackData);

            NotePlaybackData = notePlaybackData;
            IsCustomNotePlaybackDataSet = true;
        }

        #endregion
    }
}
