using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TimedEventsManager : IDisposable
    {
        #region Fields

        private readonly EventsCollection _eventsCollection;

        private bool _disposed;

        #endregion

        #region Constructor

        public TimedEventsManager(EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _eventsCollection = eventsCollection;
            Events = new TimedEventsCollection(CreateTimedEvents(eventsCollection));
        }

        #endregion

        #region Properties

        public TimedEventsCollection Events { get; }

        #endregion

        #region Methods

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
