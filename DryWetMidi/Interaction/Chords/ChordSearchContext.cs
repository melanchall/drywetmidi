using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a context to search chords within.
    /// </summary>
    /// <remarks>
    /// <para>To understand what context means let's take a look at following events sequence
    /// within two track chunks:</para>
    /// <para>
    /// <code>
    /// Track chunk 1: ON1...ON2...OFF1...OFF2
    /// Track chunk 2: ON3...OFF3.............
    /// </code>
    /// </para>
    /// <para>where <c>ON</c> and <c>OFF</c> means Note On and Note Off events correspondingly
    /// (with different note number and channel for simplicity); <c>.</c> is any other event.</para>
    /// <para>
    /// If we use <see cref="SingleEventsCollection"/> as the context, chords will be constructed in
    /// following way:
    /// </para>
    /// <para>
    /// <code>
    ///                  +-------+--- A --+--------+
    ///                  |       |        |        |
    /// Track chunk 1: [ON1]...[ON2]...[OFF1]...[OFF2]
    /// Track chunk 2: (ON3)...(OFF3).................
    ///                  |        |
    ///                  +-- B ---+
    /// </code>
    /// </para>
    /// <para>
    /// So chords (A and B) will be constructed only from notes within the same events collection.
    /// <c>[...]</c> and <c>(...)</c> denote two different instances of <see cref="Chord"/>.
    /// </para>
    /// <para>
    /// But if we use <see cref="AllEventsCollections"/> as the context, we'll get a single chord:
    /// </para>
    /// <para>
    /// <code>
    ///                +-----+-------+--------+--------+
    ///                |     |       |        |        |
    /// Track chunk 1: |   [ON1]...[ON2]...[OFF1]...[OFF2]
    /// Track chunk 2: |   [ON3]...[OFF3].................
    ///                |     |        |
    ///                +-----+--------+
    /// </code>
    /// </para>
    /// <para>
    /// So a chord can be constructed from notes within different events collections.
    /// </para>
    /// </remarks>
    /// <seealso cref="ChordDetectionSettings"/>
    /// <seealso cref="ChordsManagingUtilities"/>
    public enum ChordSearchContext
    {
        /// <summary>
        /// A chord should be detected only within single events collection (<see cref="EventsCollection"/>
        /// or <see cref="TrackChunk"/>). It means all MIDI events that make up a chord should be present
        /// in one events collection.
        /// </summary>
        SingleEventsCollection = 0,

        /// <summary>
        /// A chord can be detected within all events collection (<see cref="EventsCollection"/>
        /// or <see cref="TrackChunk"/>) of the source (<see cref="MidiFile"/> for example). It means
        /// MIDI events that make up a chord can be present in different events collection.
        /// </summary>
        AllEventsCollections
    }
}
