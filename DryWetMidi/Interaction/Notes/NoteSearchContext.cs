using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a context to search notes within.
    /// </summary>
    /// <example>
    /// <para>
    /// To understand what context means let's take a look at following events sequence
    /// within two track chunks:
    /// </para>
    /// <code language="image">
    /// ┌──────────────────┐
    /// │.A..............Y.│
    /// └──────────────────┘
    /// ┌──────────────────┐
    /// │.....B...X........│
    /// └──────────────────┘
    /// </code>
    /// <para>
    /// where <c>A</c> and <c>B</c> mean Note On events (see <see cref="NoteOnEvent"/>), and <c>X</c>
    /// and <c>Y</c> mean Note Off ones (see <see cref="NoteOffEvent"/>) with the same note number and channel;
    /// <c>.</c> is any other event. So we have two overlapped notes here. We will assume that
    /// <see cref="NoteStartDetectionPolicy.FirstNoteOn"/> used to search a note's start.
    /// </para>
    /// <para>
    /// If we use <see cref="SingleEventsCollection"/> as the context, notes will be constructed in
    /// following way:
    /// </para>
    /// <code language="image">
    /// ┌ ┌──────────────┐ ┐
    /// │.A..............Y.│
    /// └──────────────────┘
    /// ┌──────────────────┐
    /// │.....B...X........│
    /// └──── └───┘ ───────┘
    /// </code>
    /// <para>
    /// or, if highlight all notes:
    /// </para>
    /// <para>
    /// So every Note On event will be combined with Note Off one within the same events collection.
    /// </para>
    /// <para>
    /// But if we use <see cref="AllEventsCollections"/> as the context, notes will be constructed in
    /// a new way:
    /// </para>
    /// <code language="image">
    /// ┌ ┌───────┐ ───────┐
    /// │.A.......│......Y.│
    /// └──────── │ ──── │ ┘
    /// ┌──────── │ ──── │ ┐
    /// │.....B...X......│.│
    /// └──── └──────────┘ ┘
    /// </code>
    /// <para>
    /// So Note On event can be combined with Note Off one within another events collection, i.e.
    /// ends of a note can be placed in different events collections.
    /// </para>
    /// </example>
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
        /// MIDI events that make up a note can be present in different events collections.
        /// </summary>
        AllEventsCollections
    }
}
