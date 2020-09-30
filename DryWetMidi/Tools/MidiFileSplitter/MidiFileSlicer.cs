using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class MidiFileSlicer : IDisposable
    {
        #region Nested classes

        private sealed class TimedEventsHolder : IDisposable
        {
            #region Fields

            private bool _disposed = false;

            #endregion

            #region Constructor

            public TimedEventsHolder(IEnumerator<TimedEvent> timedEventsEumerator)
            {
                Enumerator = timedEventsEumerator;
                Enumerator.MoveNext();
            }

            #endregion

            #region Properties

            public IEnumerator<TimedEvent> Enumerator { get; }

            public Dictionary<MidiEventType, TimedEvent> EventsToCopyToNextPart { get; } = new Dictionary<MidiEventType, TimedEvent>();

            public List<TimedEvent> EventsToStartNextPart { get; } = new List<TimedEvent>();

            #endregion

            #region IDisposable

            private void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                    Enumerator.Dispose();

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

        private static readonly MidiEventType[] EventsTypesToCopyToNextPart = new[]
        {
            MidiEventType.ChannelAftertouch,
            MidiEventType.ControlChange,
            MidiEventType.NoteAftertouch,
            MidiEventType.PitchBend,
            MidiEventType.ProgramChange,

            MidiEventType.ChannelPrefix,
            MidiEventType.CopyrightNotice,
            MidiEventType.DeviceName,
            MidiEventType.InstrumentName,
            MidiEventType.KeySignature,
            MidiEventType.PortPrefix,
            MidiEventType.ProgramName,
            MidiEventType.SequenceNumber,
            MidiEventType.SequenceTrackName,
            MidiEventType.SetTempo,
            MidiEventType.SmpteOffset,
            MidiEventType.TimeSignature
        };

        #endregion

        #region Fields

        private readonly TimedEventsHolder[] _timedEventsHolders;
        private readonly TimeDivision _timeDivision;

        private long _lastTime;

        private bool _disposed = false;

        #endregion

        #region Constructor

        private MidiFileSlicer(TimeDivision timeDivision, IEnumerator<TimedEvent>[] timedEventsEnumerators)
        {
            _timedEventsHolders = timedEventsEnumerators.Select(e => new TimedEventsHolder(e)).ToArray();
            _timeDivision = timeDivision;
        }

        #endregion

        #region Properties

        public bool AllEventsProcessed => _timedEventsHolders.All(c => !c.EventsToStartNextPart.Any() && c.Enumerator.Current == null);

        #endregion

        #region Methods

        public MidiFile GetNextSlice(long endTime, SliceMidiFileSettings settings)
        {
            var timedEvents = GetNextTimedEvents(
                endTime,
                settings.PreserveTimes,
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
            var timedEventsEnumerators = midiFile.GetTrackChunks()
                                                 .Select(c => c.GetTimedEvents().GetEnumerator())
                                                 .ToArray();

            return new MidiFileSlicer(midiFile.TimeDivision, timedEventsEnumerators);
        }

        private IEnumerable<IEnumerable<TimedEvent>> GetNextTimedEvents(
            long endTime,
            bool preserveTimes,
            Func<MidiEvent> partStartMarkerEventFactory,
            Func<MidiEvent> partEndMarkerEventFactory,
            Func<MidiEvent> emptyPartMarkerEventFactory)
        {
            var isPartEmpty = true;

            for (int i = 0; i < _timedEventsHolders.Length; i++)
            {
                var timedEventsHolder = _timedEventsHolders[i];

                var timedEventsEnumerator = timedEventsHolder.Enumerator;
                var eventsToCopyToNextPart = timedEventsHolder.EventsToCopyToNextPart;
                var eventsToStartNextPart = timedEventsHolder.EventsToStartNextPart;

                int newEventsStartIndex;
                var takenTimedEvents = PrepareTakenTimedEvents(eventsToCopyToNextPart,
                                                               preserveTimes,
                                                               eventsToStartNextPart,
                                                               out newEventsStartIndex);

                do
                {
                    var timedEvent = timedEventsEnumerator.Current;
                    if (timedEvent == null)
                        break;

                    var time = timedEvent.Time;
                    if (time > endTime)
                        break;

                    if (time == endTime)
                    {
                        if (!TryToMoveEdgeNoteOffsToPreviousPart(timedEvent, takenTimedEvents))
                            eventsToStartNextPart.Add(timedEvent);

                        continue;
                    }

                    UpdateEventsToCopyToNextPart(eventsToCopyToNextPart, timedEvent);

                    takenTimedEvents.Add(timedEvent);
                }
                while (timedEventsEnumerator.MoveNext());

                isPartEmpty &= newEventsStartIndex >= takenTimedEvents.Count;

                //

                if (isPartEmpty && i == _timedEventsHolders.Length - 1 && emptyPartMarkerEventFactory != null)
                {
                    takenTimedEvents.Insert(0, new TimedEvent(emptyPartMarkerEventFactory(), preserveTimes ? _lastTime : 0));
                    newEventsStartIndex++;
                }

                if (partStartMarkerEventFactory != null)
                {
                    takenTimedEvents.Insert(0, new TimedEvent(partStartMarkerEventFactory(), preserveTimes ? _lastTime : 0));
                    newEventsStartIndex++;
                }

                if (partEndMarkerEventFactory != null)
                    takenTimedEvents.Add(new TimedEvent(partEndMarkerEventFactory(), endTime));

                //

                if (!preserveTimes)
                    MoveEventsToStart(takenTimedEvents, newEventsStartIndex, _lastTime);

                //

                yield return takenTimedEvents;
            }

            _lastTime = endTime;
        }

        private static bool TryToMoveEdgeNoteOffsToPreviousPart(
            TimedEvent timedEvent,
            List<TimedEvent> takenTimedEvents)
        {
            var noteOffEvent = timedEvent.Event as NoteOffEvent;
            if (noteOffEvent != null)
            {
                takenTimedEvents.Add(timedEvent);
                return true;
            }

            return false;
        }

        private static void MoveEventsToStart(List<TimedEvent> takenTimedEvents, int startIndex, long partStartTime)
        {
            for (int j = startIndex; j < takenTimedEvents.Count; j++)
            {
                takenTimedEvents[j].Time -= partStartTime;
            }
        }

        private static List<TimedEvent> PrepareTakenTimedEvents(
            Dictionary<MidiEventType, TimedEvent> eventsToCopyToNextPart,
            bool preserveTimes,
            List<TimedEvent> eventsToStartNextPart,
            out int newEventsStartIndex)
        {
            var takenTimedEvents = new List<TimedEvent>(eventsToCopyToNextPart.Values);
            if (!preserveTimes)
                takenTimedEvents.ForEach(e => e.Time = 0);

            newEventsStartIndex = takenTimedEvents.Count;

            takenTimedEvents.AddRange(eventsToStartNextPart);
            eventsToStartNextPart.Clear();

            foreach (var timedEvent in takenTimedEvents)
            {
                UpdateEventsToCopyToNextPart(eventsToCopyToNextPart, timedEvent);
            }

            return takenTimedEvents;
        }

        private static void UpdateEventsToCopyToNextPart(Dictionary<MidiEventType, TimedEvent> eventsToCopyToNextPart, TimedEvent timedEvent)
        {
            var midiEventType = timedEvent.Event.EventType;
            if (EventsTypesToCopyToNextPart.Contains(midiEventType))
                eventsToCopyToNextPart[midiEventType] = timedEvent.Clone();
        }

        #endregion

        #region IDisposable

        void Dispose(bool disposing)
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

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
