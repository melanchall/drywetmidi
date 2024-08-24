using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a hint which tells the objects processing algorithm how it can optimize its performance.
    /// </summary>
    /// <remarks>
    /// If you want to get the maximum performance of an object processing (for example,
    /// <see cref="TimedObjectUtilities.ProcessObjects(MidiFile, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>),
    /// choose a hint carefully. Note that you can always use <see href="xref:a_managers">an object manager</see> to
    /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
    /// always be faster and will consume less memory.
    /// </remarks>
    [Flags]
    public enum ObjectProcessingHint
    {
        /// <summary>
        /// The processing algorithm will consider that properties that don't affect underlying MIDI
        /// events positions will be changed only. This hint means maximum performance, i.e. the processing will
        /// take less time and consume less memory. If you're going to change, for example, objects times, or
        /// notes collections of chords, your changes won't be saved with this option.
        /// </summary>
        None = 0,

        /// <summary>
        /// <see cref="ITimedObject.Time"/> or <see cref="ILengthedObject.Length"/> of an object can be changed.
        /// </summary>
        TimeOrLengthCanBeChanged = 1,

        /// <summary>
        /// <see cref="Chord.Notes"/> can be changed, i.e. a note can be added to or removed from a chord.
        /// </summary>
        NotesCollectionCanBeChanged = 2,

        /// <summary>
        /// <see cref="Note.Time"/> or <see cref="Note.Length"/> can be changed on a note within
        /// a chord's notes (<see cref="Chord.Notes"/>).
        /// </summary>
        NoteTimeOrLengthCanBeChanged = 4,

        /// <summary>
        /// Default hint. Equals to <see cref="TimeOrLengthCanBeChanged"/>.
        /// </summary>
        Default = TimeOrLengthCanBeChanged,

        /// <summary>
        /// The processing algorithm will consider that everything on an object can be changed.
        /// This hint means minimum performance, i.e. the processing will take more time and consume more memory.
        /// For maximum performance see <see cref="None"/> option.
        /// </summary>
        AllPropertiesCanBeChanged =
            TimeOrLengthCanBeChanged |
            NotesCollectionCanBeChanged |
            NoteTimeOrLengthCanBeChanged,
    }
}
