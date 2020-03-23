using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Collection of the <see cref="TimedEvent"/> objects that wrap MIDI events providing
    /// a way to manage their absolute time.
    /// </summary>
    /// <remarks>
    /// <see cref="TimedEventsCollection"/> can be enumerated returning timed events in order
    /// of increasing <see cref="TimedEvent.Time"/>.
    /// </remarks>
    public sealed class TimedEventsCollection : TimedObjectsCollection<TimedEvent>
    {
        #region Fields

        private readonly TimedEventsComparer _eventsComparer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMapManager"/> with the specified
        /// collection of the <see cref="TimedEvent"/> and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="events">Events to put into this <see cref="TimedEventsCollection"/>.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <remarks>
        /// If the <paramref name="sameTimeEventsComparison"/> is not specified events with the same time
        /// will be placed into the underlying events collection in order of adding them through the manager.
        /// If you want to specify custom order of such events you need to specify appropriate comparison delegate.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is <c>null</c>.</exception>
        internal TimedEventsCollection(IEnumerable<TimedEvent> events, Comparison<MidiEvent> sameTimeEventsComparison)
            : base(events)
        {
            _eventsComparer = new TimedEventsComparer(sameTimeEventsComparison);
        }

        #endregion

        #region IEnumerable<TimedEvent>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public override IEnumerator<TimedEvent> GetEnumerator()
        {
            return _objects.OrderBy(e => e, _eventsComparer).GetEnumerator();
        }

        #endregion
    }
}
