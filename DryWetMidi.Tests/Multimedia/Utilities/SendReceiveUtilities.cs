using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal static class SendReceiveUtilities
    {
        #region Constants

        public const string EndpointToTestOnName = MidiEndpoints.A;
        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(30);

        #endregion

        #region Methods

        public static void WaitEventsReceivingStarted()
        {
            WaitOperations.Wait(TimeSpan.FromSeconds(2));
        }

        public static void CheckEventsReceiving(
            TimestampedEvent[] eventsToSend,
            IOutputEndpoint outputEndpoint,
            IInputEndpoint inputEndpoint,
            TimeSpan? sendReceiveTimeout = null)
        {
            var receivedEvents = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();
            var stopwatch = new Stopwatch();

            void OnEventSent(object sender, MidiEventSentEventArgs args) =>
                sentEvents.Add(new TimestampedEvent(args.Event, stopwatch.Elapsed));

            void OnEventReceived(object sender, MidiEventReceivedEventArgs args) =>
                receivedEvents.Add(new TimestampedEvent(args.Event, stopwatch.Elapsed));

            outputEndpoint.EventSent += OnEventSent;
            inputEndpoint.EventReceived += OnEventReceived;

            stopwatch.Start();
            SendEvents(
                eventsToSend,
                outputEndpoint,
                midiEvent =>
                {
                    sentEvents.Add(new TimestampedEvent(midiEvent, stopwatch.Elapsed));
                    receivedEvents.Add(new TimestampedEvent(midiEvent, stopwatch.Elapsed));
                });

            sendReceiveTimeout = sendReceiveTimeout ?? TimeSpan.FromMilliseconds(50);
            var timeout = (eventsToSend.LastOrDefault()?.Time ?? TimeSpan.Zero) + sendReceiveTimeout.Value;
            var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count == eventsToSend.Length, timeout);

            try
            {
                var expectedReceivedEvents = eventsToSend
                    .Select(e => new TimestampedEvent(e.Event, e.Time))
                    .ToArray();

                CheckTimestampedEvents(
                    sentEvents,
                    expectedReceivedEvents,
                    sendReceiveTimeout,
                    "Invalid sent events.");

                CheckTimestampedEvents(
                    receivedEvents,
                    expectedReceivedEvents,
                    sendReceiveTimeout,
                    "Invalid received events.");
            }
            finally
            {
                outputEndpoint.EventSent -= OnEventSent;
                inputEndpoint.EventReceived -= OnEventReceived;
            }
        }

        public static void SendEvents(
            IEnumerable<TimestampedEvent> eventsToSend,
            IOutputEndpoint outputEndpoint,
            Action<MidiEvent> onSent = null)
        {
            var stopwatch = Stopwatch.StartNew();

            foreach (var eventToSend in eventsToSend)
            {
                while (stopwatch.Elapsed < eventToSend.Time)
                {
                    Thread.Yield();
                }

                var midiEvent = eventToSend.Event;
                if (midiEvent is MetaEvent)
                    onSent?.Invoke(midiEvent);
                else
                    outputEndpoint.SendEvent(midiEvent);
            }
        }

        public static void CheckTimestampedEvents(
            IReadOnlyList<TimestampedEvent> actualEvents,
            IReadOnlyList<TimestampedEvent> expectedEvents,
            TimeSpan? timestampDelta = null,
            string label = null)
        {
            var equalityCheckSettings = new MidiEventEqualityCheckSettings { CompareDeltaTimes = false };

            var actualEventsList = actualEvents.ToList();
            var notReceivedEvents = new List<(TimestampedEvent Event, TimestampedEvent NearestEvent)>();

            foreach (var expectedEvent in expectedEvents)
            {
                TimestampedEvent GetMatchedEvent(TimeSpan delta) => actualEventsList.FirstOrDefault(e =>
                {
                    if (!MidiEvent.Equals(expectedEvent.Event, e.Event, equalityCheckSettings))
                        return false;

                    var expectedTime = expectedEvent.Time;
                    var offsetFromExpectedTime = (e.Time - expectedTime).Duration();

                    return offsetFromExpectedTime <= delta;
                });

                var delay = TimeSpan.FromMilliseconds(expectedEvent.DelayMs);
                var timeDelta = delay + (timestampDelta ?? MaximumEventSendReceiveDelay);
                var actualEvent = GetMatchedEvent(timeDelta);

                if (actualEvent == null)
                    notReceivedEvents.Add((expectedEvent, GetMatchedEvent(timeDelta.MultiplyBy(2))));
                else
                    actualEventsList.Remove(actualEvent);
            }

            var actualEventsString = $"Actual events:{Environment.NewLine}{string.Join(Environment.NewLine, actualEvents)}";

            var labelString = string.IsNullOrEmpty(label) ? string.Empty : $"{label} ";

            CollectionAssert.IsEmpty(
                notReceivedEvents,
                $"{labelString}Following events are not exist:{Environment.NewLine}{string.Join(Environment.NewLine, notReceivedEvents.Select(e => $"{e.Event} (nearest: {e.NearestEvent})"))}{Environment.NewLine}" +
                actualEventsString);

            CollectionAssert.IsEmpty(
                actualEventsList,
                $"{labelString}Following events are unexpectedly exist:{Environment.NewLine}{string.Join(Environment.NewLine, actualEventsList)}{Environment.NewLine}" +
                actualEventsString);
        }

        #endregion
    }
}
