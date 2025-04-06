﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal static class SendReceiveUtilities
    {
        #region Constants

        public const string DeviceToTestOnName = MidiDevicesNames.DeviceA;
        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(30);

        #endregion

        #region Methods

        public static void CheckEventsReceiving(IReadOnlyList<EventToSend> eventsToSend)
        {
            var receivedEvents = new List<ReceivedEvent>();
            var sentEvents = new List<SentEvent>();
            var stopwatch = new Stopwatch();

            using (var outputDevice = OutputDevice.GetByName(DeviceToTestOnName))
            {
                WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var inputDevice = InputDevice.GetByName(DeviceToTestOnName))
                {
                    inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                    inputDevice.StartEventsListening();

                    stopwatch.Start();
                    SendEvents(eventsToSend, outputDevice);
                    stopwatch.Stop();

                    var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count == eventsToSend.Count, timeout);
                    
                    if (!areEventsReceived)
                    {
                        Assert.Fail(
                            $"Events are not received for timeout {timeout}. " +
                            $"Events to send: {string.Join(", ", eventsToSend.Select(e => e.Event))}. " +
                            $"Sent events: {string.Join(", ", sentEvents.Select(e => e.Event))}. " +
                            $"Received events: {string.Join(", ", receivedEvents.Select(e => e.Event))}.");
                    }

                    inputDevice.StopEventsListening();
                }
            }

            CompareSentReceivedEvents(eventsToSend, sentEvents, receivedEvents);
        }

        public static void SendEvents(IEnumerable<EventToSend> eventsToSend, IOutputDevice outputDevice)
        {
            foreach (var eventToSend in eventsToSend)
            {
                WaitOperations.WaitPrecisely(eventToSend.Delay);
                outputDevice.SendEvent(eventToSend.Event);
            }
        }

        public static void CompareSentReceivedEvents(IReadOnlyList<EventToSend> eventsToSend, IReadOnlyList<SentEvent> sentEvents, IReadOnlyList<ReceivedEvent> receivedEvents)
        {
            CompareSentReceivedEvents(eventsToSend, sentEvents, receivedEvents, MaximumEventSendReceiveDelay);
        }

        public static void CompareSentReceivedEvents(
            IReadOnlyList<EventToSend> eventsToSend,
            IReadOnlyList<SentEvent> sentEvents,
            IReadOnlyList<ReceivedEvent> receivedEvents,
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
                Assert.LessOrEqual(
                    delay,
                    maximumEventSendReceiveDelay,
                    $"Event was received too late (at {receivedEvent.Time} instead of {sentEvent.Time}). Delay is too big.");
            }
        }

        public static void CompareReceivedEvents(
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<ReceivedEvent> expectedReceivedEvents,
            TimeSpan maximumEventSendReceiveDelay)
        {
            for (var i = 0; i < expectedReceivedEvents.Count; i++)
            {
                var receivedEvent = receivedEvents[i];
                var expectedReceivedEvent = expectedReceivedEvents[i];

                MidiAsserts.AreEqual(
                    expectedReceivedEvent.Event,
                    receivedEvent.Event,
                    false,
                    $"Received event ({receivedEvent.Event}) doesn't match the expected one ({expectedReceivedEvent.Event}).");

                var delay = (expectedReceivedEvent.Time - receivedEvent.Time).Duration();
                Assert.LessOrEqual(
                    delay,
                    maximumEventSendReceiveDelay,
                    $"Event was received too late (at {receivedEvent.Time} instead of {expectedReceivedEvent.Time}). Delay is too big.");
            }
        }

        public static void WarmUpDevice(OutputDevice outputDevice)
        {
            var eventsToSend = Enumerable.Range(0, 10)
                                         .SelectMany(_ => new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() })
                                         .Select(e => new EventToSend(e, TimeSpan.FromMilliseconds(10)))
                                         .ToList();

            outputDevice.PrepareForEventsSending();
            SendEvents(eventsToSend, outputDevice);
        }

        #endregion
    }
}
