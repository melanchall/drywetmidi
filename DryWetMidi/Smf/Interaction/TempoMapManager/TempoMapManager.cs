using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TempoMapManager : IDisposable
    {
        #region Fields

        private readonly IEnumerable<TimedEventsManager> _timedEventsManagers;

        private bool _disposed;

        #endregion

        #region Constructor

        public TempoMapManager(TimeDivision timeDivision, IEnumerable<EventsCollection> eventsCollections)
        {
            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            if (eventsCollections == null)
                throw new ArgumentNullException(nameof(eventsCollections));

            if (!eventsCollections.Any())
                throw new ArgumentException("Collection of EventsCollection is empty.", nameof(eventsCollections));

            _timedEventsManagers = eventsCollections.Select(events => events.ManageTimedEvents()).ToList();

            //

            TempoMap = new TempoMap(timeDivision);

            foreach (var timedEvent in _timedEventsManagers.SelectMany(m => m.Events.Where(IsTimeSignatureEvent)))
            {
                var timeSignatureEvent = timedEvent.Event as TimeSignatureEvent;
                TempoMap.TimeSignatureLine.SetValue(timedEvent.Time,
                                                new TimeSignature(timeSignatureEvent.Numerator,
                                                                  timeSignatureEvent.Denominator));
            }

            foreach (var timedEvent in _timedEventsManagers.SelectMany(m => m.Events.Where(IsTempoEvent)))
            {
                var setTempoEvent = timedEvent.Event as SetTempoEvent;
                TempoMap.TempoLine.SetValue(timedEvent.Time,
                                        new Tempo(setTempoEvent.MicrosecondsPerQuarterNote));
            }
        }

        #endregion

        #region Properties

        public TempoMap TempoMap { get; }

        #endregion

        #region Methods

        public void SetTimeSignature(long time, TimeSignature timeSignature)
        {
            TempoMap.TimeSignatureLine.SetValue(time, timeSignature);
        }

        public void DeleteTimeSignature(long startTime)
        {
            TempoMap.TimeSignatureLine.DeleteValues(startTime);
        }

        public void DeleteTimeSignature(long startTime, long endTime)
        {
            TempoMap.TimeSignatureLine.DeleteValues(startTime, endTime);
        }

        public void SetTempo(long time, Tempo tempo)
        {
            TempoMap.TempoLine.SetValue(time, tempo);
        }

        public void DeleteTempo(long startTime)
        {
            TempoMap.TempoLine.DeleteValues(startTime);
        }

        public void DeleteTempo(long startTime, long endTime)
        {
            TempoMap.TempoLine.DeleteValues(startTime, endTime);
        }

        public void SaveChanges()
        {
            foreach (var events in _timedEventsManagers.Select(m => m.Events))
            {
                events.RemoveAll(IsTempoMapEvent);
            }

            var firstEventsCollection = _timedEventsManagers.First().Events;
            firstEventsCollection.Add(TempoMap.TempoLine.Values.Select(GetSetTempoTimedEvent));
            firstEventsCollection.Add(TempoMap.TimeSignatureLine.Values.Select(GetTimeSignatureTimedEvent));

            foreach (var timedEventsManager in _timedEventsManagers)
            {
                timedEventsManager.SaveChanges();
            }
        }

        private static bool IsTempoMapEvent(TimedEvent timedEvent)
        {
            return IsTempoEvent(timedEvent) || IsTimeSignatureEvent(timedEvent);
        }

        private static bool IsTempoEvent(TimedEvent timedEvent)
        {
            return timedEvent?.Event is SetTempoEvent;
        }

        private static bool IsTimeSignatureEvent(TimedEvent timedEvent)
        {
            return timedEvent?.Event is TimeSignatureEvent;
        }

        private static TimedEvent GetSetTempoTimedEvent(ValueChange<Tempo> tempo)
        {
            if (tempo == null)
                throw new ArgumentNullException(nameof(tempo));

            return new TimedEvent(new SetTempoEvent(tempo.Value.MicrosecondsPerQuarterNote),
                                  tempo.Time);
        }

        private static TimedEvent GetTimeSignatureTimedEvent(ValueChange<TimeSignature> timeSignature)
        {
            if (timeSignature == null)
                throw new ArgumentNullException(nameof(timeSignature));

            return new TimedEvent(new TimeSignatureEvent((byte)timeSignature.Value.Numerator,
                                                         (byte)timeSignature.Value.Denominator),
                                  timeSignature.Time);
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
