using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Collection of the <see cref="TimedEvent"/> objects that wrap MIDI events providing
    /// a way to manage their absolute time.
    /// </summary>
    /// <remarks>
    /// <see cref="TimedEventsCollection"/> can be enumerated returning timed events in order
    /// of increasing <see cref="TimedEvent.Time"/>.
    /// </remarks>
    public sealed class TimedEventsCollection : IEnumerable<TimedEvent>
    {
        #region Fields

        private readonly List<TimedEvent> _timedEvents = new List<TimedEvent>();
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
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        internal TimedEventsCollection(IEnumerable<TimedEvent> events, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            _eventsComparer = new TimedEventsComparer(sameTimeEventsComparison);
            _timedEvents.AddRange(events);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds timed events into this <see cref="TimedEventsCollection"/>.
        /// </summary>
        /// <param name="events">Events to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        public void Add(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            _timedEvents.AddRange(events);
        }

        /// <summary>
        /// Adds timed events into this <see cref="TimedEventsCollection"/>.
        /// </summary>
        /// <param name="events">Events to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        public void Add(params TimedEvent[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            Add((IEnumerable<TimedEvent>)events);
        }

        /// <summary>
        /// Removes timed events from this <see cref="TimedEventsCollection"/>.
        /// </summary>
        /// <param name="events">Events to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        public void Remove(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            foreach (var e in events.ToList())
            {
                _timedEvents.Remove(e);
            }
        }

        /// <summary>
        /// Removes timed events from this <see cref="TimedEventsCollection"/>.
        /// </summary>
        /// <param name="events">Events to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        public void Remove(params TimedEvent[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            Remove((IEnumerable<TimedEvent>)events);
        }

        /// <summary>
        /// Removes all the timed events that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="predicate">Delegate that defines the conditions of the events to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        public void RemoveAll(Predicate<TimedEvent> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _timedEvents.RemoveAll(predicate);
        }

        /// <summary>
        /// Removes all timed events from this <see cref="TimedEventsCollection"/>.
        /// </summary>
        public void Clear()
        {
            _timedEvents.Clear();
        }

        #endregion

        #region IEnumerable<TimedEvent>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TimedEvent> GetEnumerator()
        {
            return _timedEvents.OrderBy(e => e, _eventsComparer).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
