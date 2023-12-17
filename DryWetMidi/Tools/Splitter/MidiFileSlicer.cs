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

            private readonly IEnumerator<TimedEvent> _enumerator;

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

            public List<TimedEvent> EventsToCopyToNextPart { get; } = new List<TimedEvent>();

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

        private static readonly Dictionary<MidiEventType, Func<MidiEvent, MidiEvent, bool>> DefaultUpdatePredicates =
            new Dictionary<MidiEventType, Func<MidiEvent, MidiEvent, bool>>
            {
                [MidiEventType.ChannelAftertouch] = (midiEvent, existingMidiEvent) =>
                {
                    var currentChannel = ((ChannelAftertouchEvent)midiEvent).Channel;
                    return ((ChannelAftertouchEvent)existingMidiEvent).Channel == currentChannel;
                },
                [MidiEventType.ControlChange] = (midiEvent, existingMidiEvent) =>
                {
                    var currentControlChangeEvent = (ControlChangeEvent)midiEvent;
                    var currentControlNumber = currentControlChangeEvent.ControlNumber;
                    var currentChannel = currentControlChangeEvent.Channel;

                    var existingControlChangeEvent = (ControlChangeEvent)existingMidiEvent;

                    return existingControlChangeEvent.ControlNumber == currentControlNumber &&
                           existingControlChangeEvent.Channel == currentChannel;
                },
                [MidiEventType.NoteAftertouch] = (midiEvent, existingMidiEvent) =>
                {
                    var currentNoteAftertouchEvent = (NoteAftertouchEvent)midiEvent;
                    var currentNoteNumber = currentNoteAftertouchEvent.NoteNumber;
                    var currentChannel = currentNoteAftertouchEvent.Channel;

                    var existingNoteAftertouchEvent = (NoteAftertouchEvent)existingMidiEvent;

                    return existingNoteAftertouchEvent.NoteNumber == currentNoteNumber &&
                           existingNoteAftertouchEvent.Channel == currentChannel;
                },
                [MidiEventType.PitchBend] = (midiEvent, existingMidiEvent) =>
                {
                    var currentChannel = ((PitchBendEvent)midiEvent).Channel;
                    return ((PitchBendEvent)existingMidiEvent).Channel == currentChannel;
                },
                [MidiEventType.ProgramChange] = (midiEvent, existingMidiEvent) =>
                {
                    var currentChannel = ((ProgramChangeEvent)midiEvent).Channel;
                    return ((ProgramChangeEvent)existingMidiEvent).Channel == currentChannel;
                },
                [MidiEventType.CopyrightNotice] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.InstrumentName] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.ProgramName] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.SequenceTrackName] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.DeviceName] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.PortPrefix] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.SetTempo] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.ChannelPrefix] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.SequenceNumber] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.KeySignature] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.SmpteOffset] = (midiEvent, existingMidiEvent) => true,
                [MidiEventType.TimeSignature] = (midiEvent, existingMidiEvent) => true,
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

        public bool AllEventsProcessed => _timedEventsHolders.All(c => !c.EventsToStartNextPart.Any() && !c.CanBeEnumerated);

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
                                                 .Select(c => c.Events.GetTimedEvents().GetEnumerator())
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

                var eventsToCopyToNextPart = timedEventsHolder.EventsToCopyToNextPart;
                var eventsToStartNextPart = timedEventsHolder.EventsToStartNextPart;

                int newEventsStartIndex;
                var takenTimedEvents = PrepareTakenTimedEvents(
                    eventsToCopyToNextPart,
                    preserveTimes,
                    eventsToStartNextPart,
                    out newEventsStartIndex);

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

                        if (time == endTime)
                        {
                            if (!TryToMoveEdgeNoteOffsToPreviousPart(timedEvent, takenTimedEvents))
                                eventsToStartNextPart.Add(timedEvent);

                            continue;
                        }

                        UpdateEventsToCopyToNextPart(eventsToCopyToNextPart, timedEvent);

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
                    takenTimedEvents.Add(new TimedEvent(partEndMarkerEventFactory(), preserveTimes ? endTime : endTime - _lastTime));

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

        private List<TimedEvent> PrepareTakenTimedEvents(
            List<TimedEvent> eventsToCopyToNextPart,
            bool preserveTimes,
            List<TimedEvent> eventsToStartNextPart,
            out int newEventsStartIndex)
        {
            var takenTimedEvents = new List<TimedEvent>(eventsToCopyToNextPart);
            takenTimedEvents.ForEach(e => e.Time = preserveTimes ? _lastTime : 0);

            newEventsStartIndex = takenTimedEvents.Count;

            takenTimedEvents.AddRange(eventsToStartNextPart);
            eventsToStartNextPart.Clear();

            foreach (var timedEvent in takenTimedEvents)
            {
                UpdateEventsToCopyToNextPart(eventsToCopyToNextPart, timedEvent);
            }

            return takenTimedEvents;
        }

        private static void UpdateEventsToCopyToNextPart(List<TimedEvent> eventsToCopyToNextPart, TimedEvent timedEvent)
        {
            var eventType = timedEvent.Event.EventType;
            var matched = false;

            for (var i = 0; i < eventsToCopyToNextPart.Count && !matched; i++)
            {
                var existingTimedEvent = eventsToCopyToNextPart[i];
                if (existingTimedEvent.Event.EventType != eventType)
                    continue;

                matched = DefaultUpdatePredicates[eventType](timedEvent.Event, existingTimedEvent.Event);
                if (matched)
                {
                    eventsToCopyToNextPart.RemoveAt(i);
                    eventsToCopyToNextPart.Add((TimedEvent)timedEvent.Clone());
                    return;
                }
            }

            if (!matched && DefaultUpdatePredicates.ContainsKey(eventType))
                eventsToCopyToNextPart.Add((TimedEvent)timedEvent.Clone());
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
