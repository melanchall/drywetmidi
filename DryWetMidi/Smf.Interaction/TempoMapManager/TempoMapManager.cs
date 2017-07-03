using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a way to manage tempo map of a MIDI file.
    /// </summary>
    /// <remarks>
    /// This manager is wrapper for the <see cref="TimedEventsManager"/> that provides easy manipulation
    /// of specific MIDI events: <see cref="SetTempoEvent"/> and <see cref="TimeSignature"/>. Also it
    /// provides <see cref="TempoMap"/> that can be used to calculate custom representations of time
    /// and length of an object. To start manage tempo map you need to get an instance of the
    /// <see cref="TempoMapManager"/>. To finish managing you need to call the <see cref="SaveChanges"/>
    /// or <see cref="Dispose()"/> method. Since the manager implements <see cref="IDisposable"/> it is
    /// recommended to manage tempo map within using block.
    /// </remarks>
    public sealed class TempoMapManager : IDisposable
    {
        #region Fields

        private readonly IEnumerable<TimedEventsManager> _timedEventsManagers;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMapManager"/> with the specified time division
        /// and events collections.
        /// </summary>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <param name="eventsCollections">Collection of <see cref="EventsCollection"/> which hold events that
        /// represent tempo map of a MIDI file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timeDivision"/> is null. -or-
        /// <paramref name="eventsCollections"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="eventsCollections"/> is empty.</exception>
        public TempoMapManager(TimeDivision timeDivision, IEnumerable<EventsCollection> eventsCollections)
        {
            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            if (eventsCollections == null)
                throw new ArgumentNullException(nameof(eventsCollections));

            if (!eventsCollections.Any())
                throw new ArgumentException("Collection of EventsCollection is empty.", nameof(eventsCollections));

            _timedEventsManagers = eventsCollections.Where(events => events != null)
                                                    .Select(events => events.ManageTimedEvents())
                                                    .ToList();

            //

            TempoMap = new TempoMap(timeDivision);

            CollectTimeSignatureChanges();
            CollectTempoChanges();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets current tempo map built by the <see cref="TempoMapManager"/>.
        /// </summary>
        public TempoMap TempoMap { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets new time signature that will last from the specified time until next change of
        /// time signature.
        /// </summary>
        /// <param name="time">Time to set the new time signature at.</param>
        /// <param name="timeSignature">New time signature that will last from the specified
        /// time until next change of time signature.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="timeSignature"/> is null.</exception>
        public void SetTimeSignature(long time, TimeSignature timeSignature)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (timeSignature == null)
                throw new ArgumentNullException(nameof(timeSignature));

            TempoMap.TimeSignatureLine.SetValue(time, timeSignature);
        }

        /// <summary>
        /// Sets new time signature that will last from the specified time until next change of
        /// time signature.
        /// </summary>
        /// <param name="time">Time to set the new time signature at.</param>
        /// <param name="timeSignature">New time signature that will last from the specified
        /// time until next change of time signature.</param>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="timeSignature"/> is null.</exception>
        public void SetTimeSignature(ITime time, TimeSignature timeSignature)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (timeSignature == null)
                throw new ArgumentNullException(nameof(timeSignature));

            SetTimeSignature(TimeConverter.ConvertFrom(time, TempoMap), timeSignature);
        }

        /// <summary>
        /// Removes all changes of time signature that occured since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of time signature since.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative.</exception>
        public void ClearTimeSignature(long startTime)
        {
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Start time is negative.");

            TempoMap.TimeSignatureLine.DeleteValues(startTime);
        }

        /// <summary>
        /// Removes all changes of time signature that occured since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of time signature since.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startTime"/> is null.</exception>
        public void ClearTimeSignature(ITime startTime)
        {
            if (startTime == null)
                throw new ArgumentNullException(nameof(startTime));

            ClearTimeSignature(TimeConverter.ConvertFrom(startTime, TempoMap));
        }

        /// <summary>
        /// Removes all changes of time signature that occured between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of time signature in.</param>
        /// <param name="endTime">End of time range to remove changes of time signature in.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative. -or-
        /// <paramref name="endTime"/> is negative.</exception>
        public void ClearTimeSignature(long startTime, long endTime)
        {
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Start time is negative.");

            if (endTime < 0)
                throw new ArgumentOutOfRangeException(nameof(endTime), endTime, "End time is negative.");

            TempoMap.TimeSignatureLine.DeleteValues(startTime, endTime);
        }

        /// <summary>
        /// Removes all changes of time signature that occured between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of time signature in.</param>
        /// <param name="endTime">End of time range to remove changes of time signature in.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startTime"/> is null. -or-
        /// <paramref name="endTime"/> is null.</exception>
        public void ClearTimeSignature(ITime startTime, ITime endTime)
        {
            if (startTime == null)
                throw new ArgumentNullException(nameof(startTime));

            if (endTime == null)
                throw new ArgumentNullException(nameof(endTime));

            ClearTimeSignature(TimeConverter.ConvertFrom(startTime, TempoMap),
                               TimeConverter.ConvertFrom(endTime, TempoMap));
        }

        /// <summary>
        /// Sets new tempo that will last from the specified time until next change of tempo.
        /// </summary>
        /// <param name="time">Time to set the new tempo at.</param>
        /// <param name="tempo">New tempo that will last from the specified time until next change
        /// of tempo.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is null.</exception>
        public void SetTempo(long time, Tempo tempo)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (tempo == null)
                throw new ArgumentNullException(nameof(tempo));

            TempoMap.TempoLine.SetValue(time, tempo);
        }

        /// <summary>
        /// Sets new tempo that will last from the specified time until next change of tempo.
        /// </summary>
        /// <param name="time">Time to set the new tempo at.</param>
        /// <param name="tempo">New tempo that will last from the specified time until next change
        /// of tempo.</param>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="tempo"/> is null.</exception>
        public void SetTempo(ITime time, Tempo tempo)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempo == null)
                throw new ArgumentNullException(nameof(tempo));

            SetTempo(TimeConverter.ConvertFrom(time, TempoMap), tempo);
        }

        /// <summary>
        /// Removes all changes of tempo that occured since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of tempo since.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative.</exception>
        public void ClearTempo(long startTime)
        {
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Start time is negative.");

            TempoMap.TempoLine.DeleteValues(startTime);
        }

        /// <summary>
        /// Removes all changes of tempo that occured since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of tempo since.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startTime"/> is null.</exception>
        public void ClearTempo(ITime startTime)
        {
            if (startTime == null)
                throw new ArgumentNullException(nameof(startTime));

            ClearTempo(TimeConverter.ConvertFrom(startTime, TempoMap));
        }

        /// <summary>
        /// Removes all changes of tempo that occured between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of tempo in.</param>
        /// <param name="endTime">End of time range to remove changes of tempo in.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative. -or-
        /// <paramref name="endTime"/> is negative.</exception>
        public void ClearTempo(long startTime, long endTime)
        {
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Start time is negative.");

            if (endTime < 0)
                throw new ArgumentOutOfRangeException(nameof(endTime), endTime, "End time is negative.");

            TempoMap.TempoLine.DeleteValues(startTime, endTime);
        }

        /// <summary>
        /// Removes all changes of tempo that occured between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of tempo in.</param>
        /// <param name="endTime">End of time range to remove changes of tempo in.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startTime"/> is null. -or-
        /// <paramref name="endTime"/> is null.</exception>
        public void ClearTempo(ITime startTime, ITime endTime)
        {
            if (startTime == null)
                throw new ArgumentNullException(nameof(startTime));

            if (endTime == null)
                throw new ArgumentNullException(nameof(endTime));

            ClearTempo(TimeConverter.ConvertFrom(startTime, TempoMap),
                       TimeConverter.ConvertFrom(endTime, TempoMap));
        }

        /// <summary>
        /// Saves tempo map changes that were made with the <see cref="TempoMapManager"/> updating
        /// underlying events collections.
        /// </summary>
        /// <remarks>
        /// This method will rewrite content of all events collections were used to construct the current
        /// <see cref="TempoMapManager"/> with events were managed by underlying <see cref="TimedEventsManager"/>
        /// objects of this manager. Also all delta-times of wrapped events will be recalculated according to
        /// the <see cref="TimedEvent.Time"/> of event wrappers.
        /// </remarks>
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

        private IEnumerable<TimedEvent> GetTimedEvents(Func<TimedEvent, bool> predicate)
        {
            return _timedEventsManagers.SelectMany(m => m.Events).Where(predicate);
        }

        private void CollectTimeSignatureChanges()
        {
            foreach (var timedEvent in GetTimedEvents(IsTimeSignatureEvent))
            {
                var timeSignatureEvent = timedEvent.Event as TimeSignatureEvent;
                if (timeSignatureEvent == null)
                    continue;

                TempoMap.TimeSignatureLine.SetValue(timedEvent.Time,
                                                    new TimeSignature(timeSignatureEvent.Numerator,
                                                                      timeSignatureEvent.Denominator));
            }
        }

        private void CollectTempoChanges()
        {
            foreach (var timedEvent in GetTimedEvents(IsTempoEvent))
            {
                var setTempoEvent = timedEvent.Event as SetTempoEvent;
                if (setTempoEvent == null)
                    continue;

                TempoMap.TempoLine.SetValue(timedEvent.Time,
                                            new Tempo(setTempoEvent.MicrosecondsPerQuarterNote));
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
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
