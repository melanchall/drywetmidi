using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage tempo map of a MIDI file. More info in the <see href="xref:a_tempo_map">Tempo map</see> article.
    /// </summary>
    /// <seealso cref="Interaction.TempoMap"/>
    public sealed class TempoMapManager : IDisposable
    {
        #region Fields

        private readonly IEnumerable<TimedObjectsManager<TimedEvent>> _timedEventsManagers;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMapManager"/> that can be used
        /// to manage new tempo map with the default time division (96 ticks per quarter note).
        /// </summary>
        public TempoMapManager()
            : this(new TicksPerQuarterNoteTimeDivision())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMapManager"/> with the
        /// specified time division.
        /// </summary>
        /// <param name="timeDivision">Time division of a new tempo that will be managed by this manager.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timeDivision"/> is <c>null</c>.</exception>
        public TempoMapManager(TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            TempoMap = new TempoMap(timeDivision);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMapManager"/> with the specified time division
        /// and events collections.
        /// </summary>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <param name="eventsCollections">Collection of <see cref="EventsCollection"/> which hold events that
        /// represent tempo map of a MIDI file.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="eventsCollections"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="eventsCollections"/> is empty.</exception>
        public TempoMapManager(TimeDivision timeDivision, IEnumerable<EventsCollection> eventsCollections)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(eventsCollections), eventsCollections);
            ThrowIfArgument.IsEmptyCollection(nameof(eventsCollections),
                                              eventsCollections,
                                              $"Collection of {nameof(EventsCollection)} is empty.");

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
        /// <exception cref="ArgumentNullException"><paramref name="timeSignature"/> is <c>null</c>.</exception>
        public void SetTimeSignature(long time, TimeSignature timeSignature)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            TempoMap.TimeSignatureLine.SetValue(time, timeSignature);
        }

        /// <summary>
        /// Sets new time signature that will last from the specified time until next change of
        /// time signature.
        /// </summary>
        /// <param name="time">Time to set the new time signature at.</param>
        /// <param name="timeSignature">New time signature that will last from the specified
        /// time until next change of time signature.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void SetTimeSignature(ITimeSpan time, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            SetTimeSignature(TimeConverter.ConvertFrom(time, TempoMap), timeSignature);
        }

        /// <summary>
        /// Removes all changes of time signature that occurred since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of time signature since.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative.</exception>
        public void ClearTimeSignature(long startTime)
        {
            ThrowIfTimeArgument.StartIsNegative(nameof(startTime), startTime);

            TempoMap.TimeSignatureLine.DeleteValues(startTime);
        }

        /// <summary>
        /// Removes all changes of time signature that occurred since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of time signature since.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startTime"/> is <c>null</c>.</exception>
        public void ClearTimeSignature(ITimeSpan startTime)
        {
            ThrowIfArgument.IsNull(nameof(startTime), startTime);

            ClearTimeSignature(TimeConverter.ConvertFrom(startTime, TempoMap));
        }

        /// <summary>
        /// Removes all changes of time signature that occurred between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of time signature in.</param>
        /// <param name="endTime">End of time range to remove changes of time signature in.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="startTime"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="endTime"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void ClearTimeSignature(long startTime, long endTime)
        {
            ThrowIfTimeArgument.StartIsNegative(nameof(startTime), startTime);
            ThrowIfTimeArgument.EndIsNegative(nameof(endTime), endTime);

            TempoMap.TimeSignatureLine.DeleteValues(startTime, endTime);
        }

        /// <summary>
        /// Removes all changes of time signature that occurred between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of time signature in.</param>
        /// <param name="endTime">End of time range to remove changes of time signature in.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="startTime"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="endTime"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void ClearTimeSignature(ITimeSpan startTime, ITimeSpan endTime)
        {
            ThrowIfArgument.IsNull(nameof(startTime), startTime);
            ThrowIfArgument.IsNull(nameof(endTime), endTime);

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
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is <c>null</c>.</exception>
        public void SetTempo(long time, Tempo tempo)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            TempoMap.TempoLine.SetValue(time, tempo);
        }

        /// <summary>
        /// Sets new tempo that will last from the specified time until next change of tempo.
        /// </summary>
        /// <param name="time">Time to set the new tempo at.</param>
        /// <param name="tempo">New tempo that will last from the specified time until next change
        /// of tempo.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void SetTempo(ITimeSpan time, Tempo tempo)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            SetTempo(TimeConverter.ConvertFrom(time, TempoMap), tempo);
        }

        /// <summary>
        /// Removes all changes of tempo that occurred since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of tempo since.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative.</exception>
        public void ClearTempo(long startTime)
        {
            ThrowIfTimeArgument.StartIsNegative(nameof(startTime), startTime);

            TempoMap.TempoLine.DeleteValues(startTime);
        }

        /// <summary>
        /// Removes all changes of tempo that occurred since the specified time.
        /// </summary>
        /// <param name="startTime">Time to remove changes of tempo since.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startTime"/> is <c>null</c>.</exception>
        public void ClearTempo(ITimeSpan startTime)
        {
            ThrowIfArgument.IsNull(nameof(startTime), startTime);

            ClearTempo(TimeConverter.ConvertFrom(startTime, TempoMap));
        }

        /// <summary>
        /// Removes all changes of tempo that occurred between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of tempo in.</param>
        /// <param name="endTime">End of time range to remove changes of tempo in.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="startTime"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="endTime"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void ClearTempo(long startTime, long endTime)
        {
            ThrowIfTimeArgument.StartIsNegative(nameof(startTime), startTime);
            ThrowIfTimeArgument.EndIsNegative(nameof(endTime), endTime);

            TempoMap.TempoLine.DeleteValues(startTime, endTime);
        }

        /// <summary>
        /// Removes all changes of tempo that occurred between the specified times.
        /// </summary>
        /// <param name="startTime">Start of time range to remove changes of tempo in.</param>
        /// <param name="endTime">End of time range to remove changes of tempo in.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="startTime"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="endTime"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void ClearTempo(ITimeSpan startTime, ITimeSpan endTime)
        {
            ThrowIfArgument.IsNull(nameof(startTime), startTime);
            ThrowIfArgument.IsNull(nameof(endTime), endTime);

            ClearTempo(TimeConverter.ConvertFrom(startTime, TempoMap),
                       TimeConverter.ConvertFrom(endTime, TempoMap));
        }

        /// <summary>
        /// Clears current tempo map removing all changes of tempo and time signature.
        /// </summary>
        public void ClearTempoMap()
        {
            TempoMap.TempoLine.Clear();
            TempoMap.TimeSignatureLine.Clear();
        }

        /// <summary>
        /// Replaces current tempo map with the specified one.
        /// </summary>
        /// <param name="tempoMap">Tempo map to replace the current one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        public void ReplaceTempoMap(TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            TempoMap.TimeDivision = tempoMap.TimeDivision.Clone();
            TempoMap.TempoLine.ReplaceValues(tempoMap.TempoLine);
            TempoMap.TimeSignatureLine.ReplaceValues(tempoMap.TimeSignatureLine);
        }

        /// <summary>
        /// Saves tempo map changes that were made with the <see cref="TempoMapManager"/> updating
        /// underlying events collections.
        /// </summary>
        /// <remarks>
        /// This method will rewrite content of all events collections were used to construct the current
        /// <see cref="TempoMapManager"/> with events were managed by underlying <see cref="TimedObjectsManager"/>
        /// objects of this manager. Also all delta-times of wrapped events will be recalculated according to
        /// the <see cref="TimedEvent.Time"/> of event wrappers.
        /// </remarks>
        public void SaveChanges()
        {
            // We are managing new tempo map

            if (_timedEventsManagers == null)
                return;

            // Update existing tempo map

            foreach (var events in _timedEventsManagers.Select(m => m.Objects))
            {
                events.RemoveAll(IsTempoMapEvent);
            }

            var firstEventsCollection = _timedEventsManagers.First().Objects;
            firstEventsCollection.Add(TempoMap.TempoLine.Select(GetSetTempoTimedEvent));
            firstEventsCollection.Add(TempoMap.TimeSignatureLine.Select(GetTimeSignatureTimedEvent));

            foreach (var timedEventsManager in _timedEventsManagers)
            {
                timedEventsManager.SaveChanges();
            }
        }

        private IEnumerable<TimedEvent> GetTimedEvents(Func<TimedEvent, bool> predicate)
        {
            return _timedEventsManagers.SelectMany(m => m.Objects).Where(predicate);
        }

        private void CollectTimeSignatureChanges()
        {
            foreach (var timedEvent in GetTimedEvents(IsTimeSignatureEvent))
            {
                var timeSignatureEvent = timedEvent.Event as TimeSignatureEvent;
                if (timeSignatureEvent == null)
                    continue;

                TempoMap.TimeSignatureLine.SetValue(
                    timedEvent.Time,
                    new TimeSignature(
                        timeSignatureEvent.Numerator,
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

                TempoMap.TempoLine.SetValue(
                    timedEvent.Time,
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

        private static TimedEvent GetSetTempoTimedEvent(ValueChange<Tempo> tempoChange)
        {
            Debug.Assert(tempoChange != null);

            return new TimedEvent(new SetTempoEvent(tempoChange.Value.MicrosecondsPerQuarterNote),
                                  tempoChange.Time);
        }

        private static TimedEvent GetTimeSignatureTimedEvent(ValueChange<TimeSignature> timeSignatureChange)
        {
            Debug.Assert(timeSignatureChange != null);

            var timeSignature = timeSignatureChange.Value;
            return new TimedEvent(new TimeSignatureEvent((byte)timeSignature.Numerator,
                                                         (byte)timeSignature.Denominator),
                                  timeSignatureChange.Time);
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
