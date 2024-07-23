using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a hint which tells the note processing algorithm how it can optimize its performance.
    /// </summary>
    /// <remarks>
    /// If you want to get the maximum performance of a note processing (for example,
    /// <see cref="NotesManagingUtilities.ProcessNotes(Core.MidiFile, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>),
    /// choose a hint carefully. Note that you can always use <see href="xref:a_managers">an object manager</see> to
    /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
    /// always be faster and will consume less memory.
    /// </remarks>
    [Flags]
    public enum NoteProcessingHint
    {
        /// <summary>
        /// <see cref="Note.Time"/> or <see cref="Note.Length"/> of a note can be changed.
        /// </summary>
        TimeOrLengthCanBeChanged = 1,

        /// <summary>
        /// The processing algorithm will consider that only properties that don't affect underlying MIDI
        /// events positions will be changed. This hint means maximum performance, i.e. the processing will
        /// take less time and consume less memory.
        /// </summary>
        None = 0,

        /// <summary>
        /// Default hint. Equals to <see cref="TimeOrLengthCanBeChanged"/>.
        /// </summary>
        Default = TimeOrLengthCanBeChanged,
    }
}
