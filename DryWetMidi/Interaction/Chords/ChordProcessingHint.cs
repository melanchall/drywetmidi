using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a hint which tells the chord processing algorithm how it can optimize its performance.
    /// </summary>
    /// <remarks>
    /// If you want to get the maximum performance of a chord processing (for example,
    /// <see cref="ChordsManagingUtilities.ProcessChords(Core.MidiFile, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>),
    /// choose a hint carefully. Note that you can always use <see href="xref:a_managers">an object manager</see> to
    /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
    /// always be faster and will consume less memory.
    /// </remarks>
    [Flags]
    public enum ChordProcessingHint
    {
        /// <summary>
        /// <see cref="Chord.Time"/> or <see cref="Chord.Length"/> of a chord can be changed.
        /// </summary>
        TimeOrLengthCanBeChanged = 1,
        
        /// <summary>
        /// <see cref="Chord.Notes"/> can be changed, i.e. a note can be added to or removed from
        /// a chord.
        /// </summary>
        NotesCollectionCanBeChanged = 2,

        /// <summary>
        /// <see cref="Note.Time"/> or <see cref="Note.Length"/> can be changed on a note within
        /// a chord's notes (<see cref="Chord.Notes"/>).
        /// </summary>
        NoteTimeOrLengthCanBeChanged = 4,

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

        /// <summary>
        /// The processing algorithm will consider that everything related to a chord can be changed.
        /// This hint means minimum performance, i.e. the processing will take more time and consume more memory.
        /// For maximum performance see <see cref="None"/> option.
        /// </summary>
        AllPropertiesCanBeChanged =
            TimeOrLengthCanBeChanged |
            NotesCollectionCanBeChanged |
            NoteTimeOrLengthCanBeChanged,
    }
}
