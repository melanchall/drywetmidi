using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed partial class MidiFileSlicer : IDisposable
    {
        #region Nested classes

        private sealed class TimedEventsHolder : IDisposable
        {
            #region Fields

            private readonly IEnumerator<TimedEvent> _enumerator;
            private readonly Dictionary<int, LinkedList<long>> _noteStartTimes = new();

            private bool _disposed = false;

            #endregion

            #region Constructor

            public TimedEventsHolder(IEnumerator<TimedEvent> timedEventsEnumerator)
            {
                _enumerator = timedEventsEnumerator;
                CanBeEnumerated = _enumerator.MoveNext();
            }

            #endregion

            #region Properties

            public bool CanBeEnumerated { get; private set; }

            public List<TimedEvent> EventsToStartNextPart { get; } = new List<TimedEvent>();

            #endregion

            #region Methods

            public bool MoveToNextEvent()
            {
                return CanBeEnumerated = _enumerator.MoveNext();
            }

            public TimedEvent GetCurrentEvent()
            {
                return _enumerator.Current;
            }

            public bool IsNoteEndingBeforeBoundary(TimedEvent timedEvent, NoteStartDetectionPolicy detectionPolicy)
            {
                if (!(timedEvent.Event is NoteEvent noteEvent))
                    return false;

                var noteId = noteEvent.GetNoteId();
                var noteOn = noteEvent as NoteOnEvent;
                var isNoteOn = noteOn != null && noteOn.Velocity > 0;

                if (!_noteStartTimes.TryGetValue(noteId, out var startTimes))
                {
                    if (!isNoteOn)
                        return noteEvent is NoteOffEvent;

                    _noteStartTimes.Add(noteId, startTimes = new LinkedList<long>());
                }

                if (isNoteOn)
                {
                    startTimes.AddLast(timedEvent.Time);
                    return false;
                }

                if (startTimes.Count == 0)
                    return noteEvent is NoteOffEvent;

                var start = detectionPolicy == NoteStartDetectionPolicy.FirstNoteOn
                    ? startTimes.First!
                    : startTimes.Last!;
                var startsBeforeBoundary = start.Value < timedEvent.Time;
                startTimes.Remove(start);

                return startsBeforeBoundary;
            }

            #endregion

            #region IDisposable

            private void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                    _enumerator.Dispose();

                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion
        }

        #endregion

        #region Constants

        private static readonly HashSet<MidiEventType> ContextEventTypes = new()
        {
            MidiEventType.ChannelAftertouch,
            MidiEventType.ControlChange,
            MidiEventType.NoteAftertouch,
            MidiEventType.PitchBend,
            MidiEventType.ProgramChange,
            MidiEventType.CopyrightNotice,
            MidiEventType.InstrumentName,
            MidiEventType.ProgramName,
            MidiEventType.SequenceTrackName,
            MidiEventType.DeviceName,
            MidiEventType.PortPrefix,
            MidiEventType.SetTempo,
            MidiEventType.ChannelPrefix,
            MidiEventType.SequenceNumber,
            MidiEventType.KeySignature,
            MidiEventType.SmpteOffset,
            MidiEventType.TimeSignature
        };

        #endregion

        #region Fields

        private readonly TimedEventsHolder[] _timedEventsHolders;
        private readonly TimeDivision _timeDivision;
        private readonly long _endTime;

        private long _lastTime;

        private bool _disposed = false;

        #endregion

        #region Constructor

        private MidiFileSlicer(
            TimeDivision timeDivision,
            TimedEvent[][] timedEvents,
            long endTime)
        {
            _timedEventsHolders = timedEvents.Select(e => new TimedEventsHolder(((IEnumerable<TimedEvent>)e).GetEnumerator())).ToArray();
            _timeDivision = timeDivision;
            _endTime = endTime;
            InitializeContext(timedEvents);
        }

        #endregion

        #region Properties

        public bool AllEventsProcessed => _timedEventsHolders.All(c => !c.EventsToStartNextPart.Any() && !c.CanBeEnumerated);

        #endregion

        #region Methods

        public MidiFile GetNextSlice(
            long endTime,
            SliceMidiFileSettings settings,
            long compoundHistoryStartTime = 0)
        {
            var timedEvents = GetNextTimedEvents(
                endTime,
                endTime == long.MaxValue ? Math.Max(_lastTime, _endTime) : endTime,
                settings.PreserveTimes,
                settings.NoteDetectionSettings?.NoteStartDetectionPolicy ?? NoteStartDetectionPolicy.FirstNoteOn,
                compoundHistoryStartTime,
                settings.Markers?.PartStartMarkerEventFactory,
                settings.Markers?.PartEndMarkerEventFactory,
                settings.Markers?.EmptyPartMarkerEventFactory);

            var trackChunks = timedEvents.Select(e => e.ToTrackChunk())
                                         .Where(c => settings.PreserveTrackChunks || c.Events.Any())
                                         .ToList();

            var file = new MidiFile(trackChunks)
            {
                TimeDivision = _timeDivision.Clone()
            };

            return file;
        }

        public static MidiFileSlicer CreateFromFile(MidiFile midiFile)
        {
            var timedEvents = midiFile.GetTrackChunks()
                .Select(c => c.Events.GetTimedEvents().ToArray())
                .ToArray();

            // TODO: what if td is null
            return new MidiFileSlicer(
                midiFile.TimeDivision!,
                timedEvents,
                midiFile.GetDuration<MidiTimeSpan>());
        }

        private IEnumerable<IEnumerable<TimedEvent>> GetNextTimedEvents(
            long endTime,
            long markerEndTime,
            bool preserveTimes,
            NoteStartDetectionPolicy detectionPolicy,
            long compoundHistoryStartTime,
            Func<MidiEvent>? partStartMarkerEventFactory,
            Func<MidiEvent>? partEndMarkerEventFactory,
            Func<MidiEvent>? emptyPartMarkerEventFactory)
        {
            var isPartEmpty = true;
            var contextEvents = GetContextEvents(_lastTime, preserveTimes, compoundHistoryStartTime);

            for (int i = 0; i < _timedEventsHolders.Length; i++)
            {
                var timedEventsHolder = _timedEventsHolders[i];

                var eventsToStartNextPart = timedEventsHolder.EventsToStartNextPart;

                var takenTimedEvents = contextEvents[i];
                var newEventsStartIndex = takenTimedEvents.Count;
                takenTimedEvents.AddRange(eventsToStartNextPart);
                eventsToStartNextPart.Clear();

                if (timedEventsHolder.CanBeEnumerated)
                {
                    do
                    {
                        var timedEvent = timedEventsHolder.GetCurrentEvent();
                        if (timedEvent == null)
                            break;

                        var time = timedEvent.Time;
                        if (time > endTime)
                            break;

                        var isNoteEndingBeforeBoundary = timedEventsHolder.IsNoteEndingBeforeBoundary(timedEvent, detectionPolicy);

                        if (time == endTime)
                        {
                            if (isNoteEndingBeforeBoundary)
                                takenTimedEvents.Add(timedEvent);
                            else
                                eventsToStartNextPart.Add(timedEvent);

                            continue;
                        }

                        takenTimedEvents.Add(timedEvent);
                    }
                    while (timedEventsHolder.MoveToNextEvent());
                }

                isPartEmpty &= newEventsStartIndex >= takenTimedEvents.Count;

                //

                if (!preserveTimes)
                    MoveEventsToStart(takenTimedEvents, newEventsStartIndex, _lastTime);

                //

                if (isPartEmpty && i == _timedEventsHolders.Length - 1 && emptyPartMarkerEventFactory != null)
                    takenTimedEvents.Insert(0, new TimedEvent(emptyPartMarkerEventFactory(), preserveTimes ? _lastTime : 0));

                if (partStartMarkerEventFactory != null)
                    takenTimedEvents.Insert(0, new TimedEvent(partStartMarkerEventFactory(), preserveTimes ? _lastTime : 0));

                if (partEndMarkerEventFactory != null)
                    takenTimedEvents.Add(new TimedEvent(partEndMarkerEventFactory(), preserveTimes ? markerEndTime : markerEndTime - _lastTime));

                //

                yield return takenTimedEvents;
            }

            _lastTime = endTime;
        }

        private static void MoveEventsToStart(List<TimedEvent> takenTimedEvents, int startIndex, long partStartTime)
        {
            for (int j = startIndex; j < takenTimedEvents.Count; j++)
            {
                takenTimedEvents[j].Time -= partStartTime;
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
            {
                foreach (var timedEventsHolder in _timedEventsHolders)
                {
                    timedEventsHolder.Dispose();
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
