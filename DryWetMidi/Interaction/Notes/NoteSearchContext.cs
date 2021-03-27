using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a context to search notes within.
    /// </summary>
    /// <remarks>
    /// <para>To understand what context means let's take a look at following events sequence
    /// within two track chunks:</para>
    /// <para>
    /// <code>
    /// Track chunk 1: ON1..............OFF1...
    /// Track chunk 2: .....ON2...OFF2.........
    /// </code>
    /// </para>
    /// <para>where <c>ON</c> and <c>OFF</c> means Note On and Note Off events correspondingly
    /// with the same note number and channel; <c>.</c> is any other event. We will assume that
    /// <see cref="NoteStartDetectionPolicy.FirstNoteOn"/> used to search a note's start.</para>
    /// <para>
    /// If we use <see cref="SingleEventsCollection"/> as the context, notes will be constructed in
    /// following way:
    /// </para>
    /// <para>
    /// <code>
    ///                  +-------------------+
    ///                  |                   |
    /// Track chunk 1: [ON1]..............[OFF1]...
    /// Track chunk 2: .....(ON2)...(OFF2).........
    ///                       |        |
    ///                       +--------+
    /// </code>
    /// </para>
    /// <para>
    /// So every Note On event will be combined with Note Off one within the same events collection.
    /// <c>[...]</c> and <c>(...)</c> denote two different instances of <see cref="Note"/>.
    /// </para>
    /// <para>
    /// But if we use <see cref="AllEventsCollections"/> as the context, notes will be constructed in
    /// a new way:
    /// </para>
    /// <para>
    /// <code>
    ///                  +-------------+
    ///                  |             |
    /// Track chunk 1: [ON1]...........|..(OFF1)...
    /// Track chunk 2: .....(ON2)...[OFF2]...|.....
    ///                       |              |
    ///                       +--------------+
    /// </code>
    /// </para>
    /// <para>
    /// So Note On event can be combined with Note Off one within another events collection, i.e.
    /// ends of a note can be placed in different events collections.
    /// </para>
    /// </remarks>
    /// <seealso cref="NoteDetectionSettings"/>
    /// <seealso cref="NotesManagingUtilities"/>
    public enum NoteSearchContext
    {
        /// <summary>
        /// A note should be detected only within single events collection (<see cref="EventsCollection"/>
        /// or <see cref="TrackChunk"/>). It means MIDI events that make up a note should be present
        /// in one events collection.
        /// </summary>
        SingleEventsCollection = 0,

        /// <summary>
        /// A note can be detected within all events collection (<see cref="EventsCollection"/>
        /// or <see cref="TrackChunk"/>) of the source (<see cref="MidiFile"/> for example). It means
        /// MIDI events that make up a note can be present in different events collection.
        /// </summary>
        AllEventsCollections
    }
}
