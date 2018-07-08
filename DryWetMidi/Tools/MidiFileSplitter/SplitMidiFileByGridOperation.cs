using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class SplitMidiFileByGridOperation : IDisposable
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

            public Dictionary<Type, TimedEvent> EventsToCopyToNextPart { get; } = new Dictionary<Type, TimedEvent>();

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

        private static readonly Type[] EventsTypesToCopyToNextPart = new[]
        {
            typeof(ChannelAftertouchEvent),
            typeof(ControlChangeEvent),
            typeof(NoteAftertouchEvent),
            typeof(PitchBendEvent),
            typeof(ProgramChangeEvent),

            typeof(ChannelPrefixEvent),
            typeof(CopyrightNoticeEvent),
            typeof(DeviceNameEvent),
            typeof(InstrumentNameEvent),
            typeof(KeySignatureEvent),
            typeof(PortPrefixEvent),
            typeof(ProgramNameEvent),
            typeof(SequenceNumberEvent),
            typeof(SequenceTrackNameEvent),
            typeof(SetTempoEvent),
            typeof(SmpteOffsetEvent),
            typeof(TimeSignatureEvent)
        };

        #endregion

        #region Fields

        private readonly TimedEventsHolder[] _timedEventsHolders;

        private bool _disposed = false;

        #endregion

        #region Constructor

        public SplitMidiFileByGridOperation(IEnumerator<TimedEvent>[] timedEventsEnumerators)
        {
            _timedEventsHolders = timedEventsEnumerators.Select(e => new TimedEventsHolder(e)).ToArray();
        }

        #endregion

        #region Properties

        public bool AllEventsProcessed => _timedEventsHolders.All(c => !c.EventsToStartNextPart.Any() && c.Enumerator.Current == null);

        #endregion

        #region Methods

        public IEnumerable<IEnumerable<TimedEvent>> GetTimedEvents(long startTime, long endTime, bool preserveTimes)
        {
            for (int i = 0; i < _timedEventsHolders.Length; i++)
            {
                var timedEventsHolder = _timedEventsHolders[i];

                var timedEventsEnumerator = timedEventsHolder.Enumerator;
                var eventsToCopyToNextPart = timedEventsHolder.EventsToCopyToNextPart;
                var eventsToStartNextPart = timedEventsHolder.EventsToStartNextPart;

                int newEventsStartIndex;
                var noteOnIds = new List<NoteId>();
                var takenTimedEvents = PrepareTakenTimedEvents(eventsToCopyToNextPart,
                                                               noteOnIds,
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
                        TryToMoveEdgeNoteOffsToPreviousPart(timedEvent,
                                                            noteOnIds,
                                                            takenTimedEvents,
                                                            eventsToStartNextPart);
                        continue;
                    }

                    TryToUpdateNotesInformation(timedEvent.Event, noteOnIds);
                    UpdateEventsToCopyToNextPart(eventsToCopyToNextPart, timedEvent);

                    takenTimedEvents.Add(timedEvent);
                }
                while (timedEventsEnumerator.MoveNext());

                if (!preserveTimes)
                    MoveEventsToStart(takenTimedEvents, newEventsStartIndex, startTime);

                yield return takenTimedEvents;
            }
        }

        public static SplitMidiFileByGridOperation Create(MidiFile midiFile)
        {
            var timedEventsEnumerators = midiFile.GetTrackChunks()
                                                 .Select(c => c.GetTimedEvents().GetEnumerator())
                                                 .ToArray();

            return new SplitMidiFileByGridOperation(timedEventsEnumerators);
        }

        private static void TryToUpdateNotesInformation(MidiEvent midiEvent, List<NoteId> noteOnIds)
        {
            var noteOnEvent = midiEvent as NoteOnEvent;
            if (noteOnEvent != null)
                noteOnIds.Add(noteOnEvent.GetNoteId());
            else
            {
                var noteOffEvent = midiEvent as NoteOffEvent;
                if (noteOffEvent != null)
                    noteOnIds.Remove(noteOffEvent.GetNoteId());
            }
        }

        private static void TryToMoveEdgeNoteOffsToPreviousPart(
            TimedEvent timedEvent,
            List<NoteId> noteOnIds,
            List<TimedEvent> takenTimedEvents,
            List<TimedEvent> eventsToStartNextPart)
        {
            var noteOffEvent = timedEvent.Event as NoteOffEvent;
            if (noteOffEvent != null && noteOnIds.Remove(noteOffEvent.GetNoteId()))
                takenTimedEvents.Add(timedEvent);
            else
                eventsToStartNextPart.Add(timedEvent);
        }

        private static void MoveEventsToStart(List<TimedEvent> takenTimedEvents, int startIndex, long partStartTime)
        {
            for (int j = startIndex; j < takenTimedEvents.Count; j++)
            {
                takenTimedEvents[j].Time -= partStartTime;
            }
        }

        private static List<TimedEvent> PrepareTakenTimedEvents(
            Dictionary<Type, TimedEvent> eventsToCopyToNextPart,
            List<NoteId> noteOnIds,
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
                TryToUpdateNotesInformation(timedEvent.Event, noteOnIds);
                UpdateEventsToCopyToNextPart(eventsToCopyToNextPart, timedEvent);
            }

            return takenTimedEvents;
        }

        private static void UpdateEventsToCopyToNextPart(Dictionary<Type, TimedEvent> eventsToCopyToNextPart, TimedEvent timedEvent)
        {
            var midiEventType = timedEvent.Event.GetType();
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
