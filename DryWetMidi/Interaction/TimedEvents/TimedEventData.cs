using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Holds the data for a <see cref="TimedEvent"/> construction.
    /// </summary>
    /// <seealso cref="TimedEventDetectionSettings"/>
    /// <seealso cref="TimedEventsManagingUtilities"/>
    public sealed class TimedEventData
    {
        #region Constructor

        internal TimedEventData(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
        {
            Event = midiEvent;
            Time = time;
            EventsCollectionIndex = eventsCollectionIndex;
            EventIndex = eventIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a raw MIDI event.
        /// </summary>
        public MidiEvent Event { get; }

        /// <summary>
        /// Gets the absolute time of the <see cref="Event"/>.
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Gets the index of events collection where <see cref="Event"/> is placed.
        /// </summary>
        /// <remarks>
        /// Events collection means either <see cref="TrackChunk"/> or <see cref="EventsCollection"/>
        /// (which can be obtained via <see cref="TrackChunk.Events"/>). Note that index is from <c>0</c>
        /// to events collections count minus <c>1</c>. Thus if, for example, a MIDI file has following
        /// chunks:
        /// <list type="number">
        /// <item>track chunk</item>
        /// <item>custom chunk</item>
        /// <item>track chunk</item>
        /// </list>
        /// an event from the last chunk will have <see cref="EventsCollectionIndex"/> set to <c>1</c>, because
        /// custom chunk is not treated as events collection and therefore doesn't take part in indexing.
        /// </remarks>
        public int EventsCollectionIndex { get; }

        /// <summary>
        /// Gets the index of the <see cref="Event"/> within an events collection.
        /// </summary>
        public int EventIndex { get; }

        #endregion
    }
}
