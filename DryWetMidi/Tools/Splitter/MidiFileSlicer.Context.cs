using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed partial class MidiFileSlicer
    {
        #region Nested classes

        private sealed record ContextEventKey
        {
            public ContextEventKey(
                MidiEventType eventType,
                int channel,
                int secondaryKey,
                int port,
                int trackIndex)
            {
                EventType = eventType;
                Channel = channel;
                SecondaryKey = secondaryKey;
                Port = port;
                TrackIndex = trackIndex;
            }

            public MidiEventType EventType { get; }

            public int Channel { get; }

            public int SecondaryKey { get; }

            public int Port { get; }

            public int TrackIndex { get; }
        }

        private sealed class ContextEvent
        {
            public ContextEvent(
                TimedEvent timedEvent,
                int trackIndex,
                int port,
                int order)
            {
                TimedEvent = (TimedEvent)timedEvent.Clone();
                TrackIndex = trackIndex;
                Port = port;
                Order = order;
            }

            public TimedEvent TimedEvent { get; }

            public int TrackIndex { get; }

            public int Port { get; }

            public int Order { get; }
        }

        #endregion

        #region Fields

        private const int ControlNumberOffsetToLeastSignificantByte = 32;

        private readonly List<ContextEvent> _contextEvents = new();
        private readonly Dictionary<int, int> _firstTrackByPort = new();

        #endregion

        #region Methods

        private void InitializeContext(TimedEvent[][] timedEvents)
        {
            for (var trackIndex = 0; trackIndex < timedEvents.Length; trackIndex++)
            {
                var port = 0;

                foreach (var timedEvent in timedEvents[trackIndex])
                {
                    if (timedEvent.Event is PortPrefixEvent portPrefix)
                        port = portPrefix.Port;

                    if (timedEvent.Event is ChannelEvent && !_firstTrackByPort.ContainsKey(port))
                        _firstTrackByPort.Add(port, trackIndex);

                    if (ContextEventTypes.Contains(timedEvent.Event.EventType))
                        _contextEvents.Add(new ContextEvent(timedEvent, trackIndex, port, _contextEvents.Count));
                }
            }

            var orderedEvents = _contextEvents.OrderBy(e => e.TimedEvent.Time).ToArray();
            _contextEvents.Clear();
            _contextEvents.AddRange(orderedEvents);
        }

        private List<TimedEvent>[] GetContextEvents(long time, bool preserveTimes, long compoundHistoryStartTime)
        {
            var previousEvents = _contextEvents.TakeWhile(e => e.TimedEvent.Time < time).ToArray();
            var replayChannels = GetChannelsRequiringHistory(previousEvents);
            var snapshots = new Dictionary<ContextEventKey, ContextEvent>();
            var selectedEvents = new List<ContextEvent>();

            foreach (var contextEvent in previousEvents)
            {
                var midiEvent = contextEvent.TimedEvent.Event;
                var channelEvent = midiEvent as ChannelEvent;

                if (channelEvent != null && replayChannels.Contains((contextEvent.Port, channelEvent.Channel)))
                {
                    if (contextEvent.TimedEvent.Time >= compoundHistoryStartTime)
                        selectedEvents.Add(contextEvent);

                    continue;
                }

                var secondaryKey = midiEvent is ControlChangeEvent controlChange
                    ? (int)controlChange.ControlNumber
                    : midiEvent is NoteAftertouchEvent noteAftertouch
                        ? (int)noteAftertouch.NoteNumber
                        : 0;
                var isGlobalEvent = midiEvent is SetTempoEvent || midiEvent is TimeSignatureEvent;
                var key = new ContextEventKey(
                    midiEvent.EventType,
                    channelEvent == null ? 0 : (int)channelEvent.Channel,
                    secondaryKey,
                    channelEvent == null ? 0 : contextEvent.Port,
                    channelEvent != null || isGlobalEvent ? -1 : contextEvent.TrackIndex);
                snapshots[key] = contextEvent;
            }

            selectedEvents.AddRange(snapshots.Values);

            var result = Enumerable.Range(0, _timedEventsHolders.Length)
                .Select(_ => new List<TimedEvent>())
                .ToArray();
            var outputPorts = new int[result.Length];
            var contextTime = preserveTimes ? time : 0;

            foreach (var contextEvent in selectedEvents.OrderBy(e => e.TimedEvent.Time).ThenBy(e => e.Order))
            {
                var midiEvent = contextEvent.TimedEvent.Event;
                var trackIndex = midiEvent is ChannelEvent
                    ? _firstTrackByPort[contextEvent.Port]
                    : midiEvent is SetTempoEvent || midiEvent is TimeSignatureEvent
                        ? 0
                        : contextEvent.TrackIndex;
                var trackEvents = result[trackIndex];

                if (midiEvent is ChannelEvent && outputPorts[trackIndex] != contextEvent.Port)
                {
                    trackEvents.Add(new TimedEvent(new PortPrefixEvent((SevenBitNumber)contextEvent.Port), contextTime));
                    outputPorts[trackIndex] = contextEvent.Port;
                }

                trackEvents.Add(new TimedEvent(midiEvent.Clone(), contextTime));

                if (midiEvent is PortPrefixEvent portPrefix)
                    outputPorts[trackIndex] = portPrefix.Port;
            }

            for (var trackIndex = 0; trackIndex < result.Length; trackIndex++)
            {
                var port = previousEvents
                    .LastOrDefault(e => e.TrackIndex == trackIndex && e.TimedEvent.Event is PortPrefixEvent)?.Port ?? 0;
                if (outputPorts[trackIndex] != port)
                    result[trackIndex].Add(new TimedEvent(new PortPrefixEvent((SevenBitNumber)port), contextTime));
            }

            return result;
        }

        private static HashSet<(int Port, FourBitNumber Channel)> GetChannelsRequiringHistory(ContextEvent[] events)
        {
            var result = new HashSet<(int Port, FourBitNumber Channel)>();
            var controls = new HashSet<(int Port, FourBitNumber Channel, int Number)>();

            foreach (var contextEvent in events)
            {
                if (!(contextEvent.TimedEvent.Event is ControlChangeEvent controlChange))
                    continue;

                var number = (int)controlChange.ControlNumber;
                var channel = (contextEvent.Port, controlChange.Channel);
                controls.Add((contextEvent.Port, controlChange.Channel, number));

                switch ((ControlName)number)
                {
                    case ControlName.BankSelect:
                    case ControlName.LsbForBankSelect:
                    case ControlName.DataEntryMsb:
                    case ControlName.LsbForDataEntry:
                    case ControlName.DataIncrement:
                    case ControlName.DataDecrement:
                    case ControlName.RegisteredParameterNumberMsb:
                    case ControlName.RegisteredParameterNumberLsb:
                    case ControlName.NonRegisteredParameterNumberMsb:
                    case ControlName.NonRegisteredParameterNumberLsb:
                    case ControlName.ResetAllControllers:
                        result.Add(channel);
                        break;
                }
            }

            foreach (var control in controls)
            {
                if (control.Number < ControlNumberOffsetToLeastSignificantByte &&
                    controls.Contains((control.Port, control.Channel, control.Number + ControlNumberOffsetToLeastSignificantByte)))
                    result.Add((control.Port, control.Channel));
            }

            return result;
        }

        #endregion
    }
}
