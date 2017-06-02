using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TimedEventsManager : IDisposable
    {
        #region Fields

        private readonly EventsCollection _eventsCollection;
        private readonly List<TimedEvent> _timedEvents = new List<TimedEvent>();
        private readonly TimedEventsComparer _eventsComparer;

        private bool _disposed;

        #endregion

        #region Constructor

        public TimedEventsManager(EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _eventsCollection = eventsCollection;
            _eventsComparer = new TimedEventsComparer(sameTimeEventsComparison);

            _timedEvents.AddRange(CreateTimedEvents(eventsCollection));
        }

        #endregion

        #region Properties

        public IEnumerable<TimedEvent> Events => _timedEvents.OrderBy(e => e, _eventsComparer);

        #endregion

        #region Methods

        public void AddEvents(params TimedEvent[] events)
        {
            AddEvents((IEnumerable<TimedEvent>)events);
        }

        public void AddEvents(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            _timedEvents.AddRange(events);
        }

        public void RemoveEvents(params TimedEvent[] events)
        {
            RemoveEvents((IEnumerable<TimedEvent>)events);
        }

        public void RemoveEvents(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            foreach (var e in events.ToList())
            {
                _timedEvents.Remove(e);
            }
        }

        public void RemoveAllEvents()
        {
            _timedEvents.Clear();
        }

        public void RemoveAllEvents(Predicate<TimedEvent> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            _timedEvents.RemoveAll(match);
        }

        public IEnumerable<TimedEvent> GetEventsAtTime(long time)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return _timedEvents.Where(e => e.Time == time);
        }

        public void SaveChanges()
        {
            _eventsCollection.Clear();

            var time = 0L;

            foreach (var e in Events)
            {
                var midiEvent = e.Event;
                midiEvent.DeltaTime = e.Time - time;
                _eventsCollection.Add(midiEvent);

                time = e.Time;
            }
        }

        private static IEnumerable<TimedEvent> CreateTimedEvents(EventsCollection events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            var time = 0L;

            foreach (var midiEvent in events)
            {
                time += midiEvent.DeltaTime;
                yield return new TimedEvent(midiEvent, time);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                SaveChanges();

            _disposed = true;
        }

        #endregion
    }
}
