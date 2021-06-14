using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a context to search chords within.
    /// </summary>
    /// <example>
    /// <para>
    /// To understand what context means let's take a look at following events sequence
    /// within two track chunks:
    /// </para>
    /// <code language="image">
    /// ┌───────────────┐
    /// │.A...B...X...Y.│
    /// └───────────────┘
    /// ┌───────────────┐
    /// │.C...Z.........│
    /// └───────────────┘
    /// </code>
    /// <para>
    /// <para>
    /// where <c>A</c>, <c>B</c> and <c>C</c> mean Note On events (see <see cref="NoteOnEvent"/>), and
    /// <c>X</c>, <c>Y</c> and <c>Z</c> mean Note Off ones (see <see cref="NoteOffEvent"/>) with different
    /// note number and channel for simplicity; <c>.</c> is any other event.
    /// </para>
    /// </para>
    /// <para>
    /// If we use <see cref="SingleEventsCollection"/> as the context, chords will be constructed in
    /// following way:
    /// </para>
    /// <code language="image">
    /// ┌ ┌───┬───┬───┐ ┐
    /// │.A...B...X...Y.│
    /// └───────────────┘
    /// ┌───────────────┐
    /// │.C...Z.........│
    /// └ └───┘ ────────┘
    /// </code>
    /// <para>
    /// So chords will be constructed only from notes within the same events collection (track chunk).
    /// </para>
    /// <para>
    /// But if we use <see cref="AllEventsCollections"/> as the context, we'll get a single chord:
    /// </para>
    /// <code language="image">
    /// ┌ ┌───┬───┬───┐ ┐
    /// │.A...B...X...Y.│
    /// └ │ ─ │ ────────┘
    /// ┌ │ ─ │ ────────┐
    /// │.C...Z.........│
    /// └───────────────┘
    /// </code>
    /// <para>
    /// So a chord can be constructed from notes within different events collections.
    /// </para>
    /// </example>
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
        /// MIDI events that make up a chord can be present in different events collections.
        /// </summary>
        AllEventsCollections
    }
}
