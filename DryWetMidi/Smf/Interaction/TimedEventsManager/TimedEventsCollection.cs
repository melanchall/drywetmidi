using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TimedEventsCollection : IEnumerable<TimedEvent>
    {
        #region Fields

        private readonly List<TimedEvent> _timedEvents = new List<TimedEvent>();
        private readonly TimedEventsComparer _eventsComparer;

        #endregion

        #region Constructor

        internal TimedEventsCollection(IEnumerable<TimedEvent> events, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            _eventsComparer = new TimedEventsComparer(sameTimeEventsComparison);
            _timedEvents.AddRange(events);
        }

        #endregion

        #region Methods

        public void Add(MidiEvent midiEvent, long time)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            _timedEvents.Add(new TimedEvent(midiEvent, time));
        }

        public void Add(MidiEvent midiEvent, ITime time, TempoMap tempoMap)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            Add(midiEvent, TimeConverter.ConvertFrom(time, tempoMap));
        }

        public void Add(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            _timedEvents.AddRange(events);
        }

        public void Add(params TimedEvent[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            Add((IEnumerable<TimedEvent>)events);
        }

        public void Remove(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            foreach (var e in events.ToList())
            {
                _timedEvents.Remove(e);
            }
        }

        public void Remove(params TimedEvent[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            Remove((IEnumerable<TimedEvent>)events);
        }

        public void RemoveAll(Predicate<TimedEvent> predicate)
        {
            _timedEvents.RemoveAll(predicate);
        }

        public void Clear()
        {
            _timedEvents.Clear();
        }

        #endregion

        #region IEnumerable<TimedEvent>

        public IEnumerator<TimedEvent> GetEnumerator()
        {
            return _timedEvents.OrderBy(e => e, _eventsComparer).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
