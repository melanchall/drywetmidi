using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal static class SendReceiveUtilities
    {
        #region Constants

        public const string DeviceToTestOnName = MidiDevicesNames.DeviceA;
        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(30);

        #endregion

        #region Methods

        public static void CheckEventsReceiving(
            EventToSend2[] eventsToSend,
            IOutputDevice outputDevice,
            IInputDevice inputDevice,
            TimeSpan? sendReceiveTimeout = null)
        {
            var receivedEvents = new List<SentReceivedEvent>();
            var sentEvents = new List<SentReceivedEvent>();
            var stopwatch = new Stopwatch();

            void OnEventSent(object sender, MidiEventSentEventArgs args) =>
                sentEvents.Add(new SentReceivedEvent(args.Event, stopwatch.Elapsed));

            void OnEventReceived(object sender, MidiEventReceivedEventArgs args) =>
                receivedEvents.Add(new SentReceivedEvent(args.Event, stopwatch.Elapsed));

            outputDevice.EventSent += OnEventSent;
            inputDevice.EventReceived += OnEventReceived;

            stopwatch.Start();
            SendEvents(
                eventsToSend,
                outputDevice,
                midiEvent =>
                {
                    sentEvents.Add(new SentReceivedEvent(midiEvent, stopwatch.Elapsed));
                    receivedEvents.Add(new SentReceivedEvent(midiEvent, stopwatch.Elapsed));
                });

            sendReceiveTimeout = sendReceiveTimeout ?? TimeSpan.FromMilliseconds(50);
            var timeout = (eventsToSend.LastOrDefault()?.Time ?? TimeSpan.Zero) + sendReceiveTimeout.Value;
            var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count == eventsToSend.Length, timeout);

            try
            {
                var expectedReceivedEvents = eventsToSend
                    .Select(e => new SentReceivedEvent(e.Event, e.Time))
                    .ToArray();

                CheckReceivedEvents(
                    sentEvents,
                    expectedReceivedEvents,
                    sendReceiveTimeout,
                    "Invalid sent events.");

                CheckReceivedEvents(
                    receivedEvents,
                    expectedReceivedEvents,
                    sendReceiveTimeout,
                    "Invalid received events.");
            }
            finally
            {
                outputDevice.EventSent -= OnEventSent;
                inputDevice.EventReceived -= OnEventReceived;
            }
        }

        public static void SendEvents(
            IEnumerable<EventToSend2> eventsToSend,
            IOutputDevice outputDevice,
            Action<MidiEvent> onSent = null)
        {
            var stopwatch = Stopwatch.StartNew();

            foreach (var eventToSend in eventsToSend)
            {
                while (stopwatch.Elapsed < eventToSend.Time)
                {
                }

                var midiEvent = eventToSend.Event;
                if (midiEvent is MetaEvent)
                    onSent?.Invoke(midiEvent);
                else
                    outputDevice.SendEvent(midiEvent);
            }
        }

        public static void SendEvents(IEnumerable<EventToSend> eventsToSend, IOutputDevice outputDevice)
        {
            foreach (var eventToSend in eventsToSend)
            {
                WaitOperations.WaitPrecisely(eventToSend.Delay);
                outputDevice.SendEvent(eventToSend.Event);
            }
        }

        public static void CompareSentReceivedEvents(
            IReadOnlyList<EventToSend> eventsToSend,
            IReadOnlyList<SentReceivedEvent> sentEvents,
            IReadOnlyList<SentReceivedEvent> receivedEvents)
        {
            CompareSentReceivedEvents(eventsToSend, sentEvents, receivedEvents, MaximumEventSendReceiveDelay);
        }

        public static void CompareSentReceivedEvents(
            IReadOnlyList<EventToSend> eventsToSend,
            IReadOnlyList<SentReceivedEvent> sentEvents,
            IReadOnlyList<SentReceivedEvent> receivedEvents,
            TimeSpan maximumEventSendReceiveDelay)
        {
            for (var i = 0; i < sentEvents.Count; i++)
            {
                var eventToSend = eventsToSend[i];
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];

                MidiAsserts.AreEqual(sentEvent.Event, eventToSend.Event, false, $"Sent event ({sentEvent.Event}) doesn't match the one that should be sent ({eventToSend.Event}).");
                MidiAsserts.AreEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event ({receivedEvent.Event}) doesn't match the sent one ({sentEvent.Event}).");

                var delay = (receivedEvent.Time - sentEvent.Time).Duration();
                ClassicAssert.LessOrEqual(
                    delay,
                    maximumEventSendReceiveDelay,
                    $"Event was received too late (at {receivedEvent.Time} instead of {sentEvent.Time}). Delay is too big.");
            }
        }

        public static void CheckReceivedEvents(
            IReadOnlyList<SentReceivedEvent> receivedEvents,
            IReadOnlyList<SentReceivedEvent> expectedReceivedEvents,
            TimeSpan? sendReceiveTimeDelta = null,
            string label = null)
        {
            var equalityCheckSettings = new MidiEventEqualityCheckSettings { CompareDeltaTimes = false };

            var actualEventsList = receivedEvents.ToList();
            var notReceivedEvents = new List<(SentReceivedEvent Event, SentReceivedEvent NearestEvent)>();

            foreach (var expectedReceivedEvent in expectedReceivedEvents)
            {
                SentReceivedEvent GetMatchedEvent(TimeSpan delta) => actualEventsList.FirstOrDefault(e =>
                {
                    if (!MidiEvent.Equals(expectedReceivedEvent.Event, e.Event, equalityCheckSettings))
                        return false;

                    var expectedTime = expectedReceivedEvent.Time;
                    var offsetFromExpectedTime = (e.Time - expectedTime).Duration();

                    return offsetFromExpectedTime <= delta;
                });

                var delay = TimeSpan.FromMilliseconds(expectedReceivedEvent.DelayMs);
                var timeDelta = delay + (sendReceiveTimeDelta ?? MaximumEventSendReceiveDelay);
                var actualEvent = GetMatchedEvent(timeDelta);

                if (actualEvent == null)
                    notReceivedEvents.Add((expectedReceivedEvent, GetMatchedEvent(timeDelta.MultiplyBy(2))));
                else
                    actualEventsList.Remove(actualEvent);
            }

            var actualEventsString = $"Actual events:{Environment.NewLine}{string.Join(Environment.NewLine, receivedEvents)}";

            var labelString = string.IsNullOrEmpty(label) ? string.Empty : $"{label} ";

            CollectionAssert.IsEmpty(
                notReceivedEvents,
                $"{labelString}Following events was not received:{Environment.NewLine}{string.Join(Environment.NewLine, notReceivedEvents.Select(e => $"{e.Event} (nearest: {e.NearestEvent})"))}{Environment.NewLine}" +
                actualEventsString);

            CollectionAssert.IsEmpty(
                actualEventsList,
                $"{labelString}Following events are unexpectedly received:{Environment.NewLine}{string.Join(Environment.NewLine, actualEventsList)}{Environment.NewLine}" +
                actualEventsString);
        }

        #endregion
    }
}
