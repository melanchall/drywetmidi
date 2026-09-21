using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Melanchall.DryWetMidi.Tests.Attributes;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class EndpointsConnectorTests
    {
        #region Constants

        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(50);

        #endregion

        #region Test methods

        [TimingCritical]
        [Test]
        public void CheckEventsReceivingOnConnectedEndpoints()
        {
            CheckEventsReceiving(new[]
            {
                new TimestampedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new TimestampedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                new TimestampedEvent(new SongSelectEvent((SevenBitNumber)20), TimeSpan.FromSeconds(1)),
                new TimestampedEvent(new TuneRequestEvent(), TimeSpan.FromMilliseconds(1200)),
            });
        }

        [TimingCritical]
        [Test]
        public void CheckEventsReceivingWithCallback_NoCallback() => CheckEventsReceivingWithCallback(
            eventsToSend: new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
            },
            eventCallback: null,
            expectedReceivedEvents: new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
            });

        [TimingCritical]
        [Test]
        public void CheckEventsReceivingWithCallback_CancelEvent() => CheckEventsReceivingWithCallback(
            eventsToSend: new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
            },
            eventCallback: e => e is ControlChangeEvent ? null : e,
            expectedReceivedEvents: new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
            });

        [TimingCritical]
        [Test]
        public void CheckEventsReceivingWithCallback_ChangeEvents() => CheckEventsReceivingWithCallback(
            eventsToSend: new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
            },
            eventCallback: e =>
            {
                if (e is NoteEvent noteEvent)
                    noteEvent.NoteNumber = (SevenBitNumber)70;

                return e;
            },
            expectedReceivedEvents: new[]
            {
                new TimestampedEvent(new NoteOnEvent { NoteNumber = (SevenBitNumber)70 }, TimeSpan.Zero),
                new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new NoteOffEvent { NoteNumber = (SevenBitNumber)70 }, TimeSpan.FromMilliseconds(500)),
            });

        [TimingCritical]
        [Test]
        public void CheckEventsReceivingWithCallback_ReplaceEvents() => CheckEventsReceivingWithCallback(
            eventsToSend: new[]
            {
                new TimestampedEvent(new NoteOnEvent((SevenBitNumber)10, (SevenBitNumber)20) { Channel = (FourBitNumber)1 }, TimeSpan.Zero),
                new TimestampedEvent(new ControlChangeEvent((SevenBitNumber)11, (SevenBitNumber)21) { Channel = (FourBitNumber)2 }, TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new NoteOffEvent((SevenBitNumber)12, (SevenBitNumber)22) { Channel = (FourBitNumber)3 }, TimeSpan.FromMilliseconds(500)),
            },
            eventCallback: e => new ProgramChangeEvent((SevenBitNumber)42)
            {
                Channel = ((ChannelEvent)e).Channel
            },
            expectedReceivedEvents: new[]
            {
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)42) { Channel = (FourBitNumber)1 }, TimeSpan.Zero),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)42) { Channel = (FourBitNumber)2 }, TimeSpan.FromMilliseconds(250)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)42) { Channel = (FourBitNumber)3 }, TimeSpan.FromMilliseconds(500)),
            });

        [TimingCritical]
        [Test]
        public void CheckEventsReceivingWithCallback_InvokesCallbackOncePerInputEvent()
        {
            var callbackInvocations = 0;

            CheckEventsReceivingWithCallback(
                eventsToSend: new[]
                {
                    new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                    new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                },
                eventCallback: e =>
                {
                    callbackInvocations++;
                    return e;
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new TimestampedEvent(new ControlChangeEvent(), TimeSpan.FromMilliseconds(250)),
                    new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                });

            ClassicAssert.AreEqual(
                3,
                callbackInvocations,
                "Callback was not invoked exactly once for each received input event.");
        }

        #endregion

        #region Private methods

        private static void CheckEventsReceiving(
            IReadOnlyList<TimestampedEvent> eventsToSend)
        {
            var receivedEventsB = new List<TimestampedEvent>();
            var receivedEventsC = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();
            
            var stopwatch = new Stopwatch();

            using (var outputA = OutputEndpoint.GetByName(MidiEndpoints.A))
            {
                outputA.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputA.PrepareForEventsSending();

                using (var inputB = TestDeviceManager.GetInputEndpoint(MidiEndpoints.B))
                using (var inputC = TestDeviceManager.GetInputEndpoint(MidiEndpoints.C))
                {
                    inputB.EventReceived += (_, e) => receivedEventsB.Add(e.GetReceivedTimestampedEvent(stopwatch));
                    inputB.StartEventsListening();

                    inputC.EventReceived += (_, e) => receivedEventsC.Add(e.GetReceivedTimestampedEvent(stopwatch));
                    inputC.StartEventsListening();

                    var timestampsBaseline = inputB.GetCurrentTimestamp();

                    using (var inputA = InputEndpoint.GetByName(MidiEndpoints.A))
                    {
                        inputA.StartEventsListening();

                        using (var outputB = TestDeviceManager.GetOutputEndpoint(MidiEndpoints.B))
                        using (var outputC = TestDeviceManager.GetOutputEndpoint(MidiEndpoints.C))
                        {
                            var midiEndpointsConnector = inputA.Connect(outputB, outputC);
                            ClassicAssert.IsTrue(midiEndpointsConnector.AreEndpointsConnected, "Endpoints aren't connected.");

                            stopwatch.Start();
                            SendReceiveUtilities.SendEvents(eventsToSend, outputA);
                            stopwatch.Stop();

                            var timeout = eventsToSend.Max(e => e.Time) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                            var areEventsReceived = WaitOperations.Wait(
                                () => receivedEventsB.Count == eventsToSend.Count && receivedEventsC.Count == eventsToSend.Count,
                                timeout);
                            ClassicAssert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}. Received events B:{Environment.NewLine}{string.Join(Environment.NewLine, receivedEventsB)}{Environment.NewLine}Received events C:{Environment.NewLine}{string.Join(Environment.NewLine, receivedEventsC)}");

                            midiEndpointsConnector.Disconnect();
                            ClassicAssert.IsFalse(midiEndpointsConnector.AreEndpointsConnected, "Endpoints aren't disconnected.");
                        }
                    }

                    SendReceiveUtilities.CheckReceivedEventsTimestamps(
                        eventsToSend.ToArray(),
                        timestampsBaseline,
                        receivedEventsB.ToArray());

                    SendReceiveUtilities.CheckReceivedEventsTimestamps(
                        eventsToSend.ToArray(),
                        timestampsBaseline,
                        receivedEventsC.ToArray());
                }
            }

            SendReceiveUtilities.CheckTimestampedEvents(sentEvents, eventsToSend, TimeSpan.FromMilliseconds(10));
            SendReceiveUtilities.CheckTimestampedEvents(receivedEventsB, eventsToSend, MaximumEventSendReceiveDelay);
            SendReceiveUtilities.CheckTimestampedEvents(receivedEventsC, eventsToSend, MaximumEventSendReceiveDelay);
        }

        private static void CheckEventsReceivingWithCallback(
            IReadOnlyList<TimestampedEvent> eventsToSend,
            EndpointsConnectorEventCallback eventCallback,
            IReadOnlyList<TimestampedEvent> expectedReceivedEvents)
        {
            var receivedEventsB = new List<TimestampedEvent>();
            var receivedEventsC = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();
            
            var stopwatch = new Stopwatch();

            using (var outputA = OutputEndpoint.GetByName(MidiEndpoints.A))
            {
                outputA.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputA.PrepareForEventsSending();

                using (var inputB = TestDeviceManager.GetInputEndpoint(MidiEndpoints.B))
                using (var inputC = TestDeviceManager.GetInputEndpoint(MidiEndpoints.C))
                {
                    inputB.EventReceived += (_, e) => receivedEventsB.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                    inputB.StartEventsListening();
                    SendReceiveUtilities.WaitEventsReceivingStarted();

                    inputC.EventReceived += (_, e) => receivedEventsC.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                    inputC.StartEventsListening();
                    SendReceiveUtilities.WaitEventsReceivingStarted();

                    using (var inputA = InputEndpoint.GetByName(MidiEndpoints.A))
                    {
                        inputA.StartEventsListening();
                        SendReceiveUtilities.WaitEventsReceivingStarted();

                        using (var outputB = TestDeviceManager.GetOutputEndpoint(MidiEndpoints.B))
                        using (var outputC = TestDeviceManager.GetOutputEndpoint(MidiEndpoints.C))
                        {
                            var midiEndpointsConnector = inputA.Connect(outputB, outputC);
                            midiEndpointsConnector.EventCallback = eventCallback;
                            ClassicAssert.IsTrue(midiEndpointsConnector.AreEndpointsConnected, "Endpoints aren't connected.");

                            stopwatch.Start();
                            SendReceiveUtilities.SendEvents(eventsToSend, outputA);
                            stopwatch.Stop();

                            var timeout = eventsToSend.Max(e => e.Time) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                            var areEventsReceived = WaitOperations.Wait(
                                () => receivedEventsB.Count == expectedReceivedEvents.Count && receivedEventsC.Count == expectedReceivedEvents.Count,
                                timeout);

                            var receivedEventsListB = string.Join(", ", receivedEventsB);
                            var receivedEventsListC = string.Join(", ", receivedEventsC);
                            ClassicAssert.IsTrue(
                                areEventsReceived,
                                $"Events are not received for timeout {timeout}.{Environment.NewLine}Received events (B): {receivedEventsListB}{Environment.NewLine}Received events (C): {receivedEventsListC}");

                            midiEndpointsConnector.Disconnect();
                            ClassicAssert.IsFalse(midiEndpointsConnector.AreEndpointsConnected, "Endpoints aren't disconnected.");
                        }
                    }
                }
            }

            SendReceiveUtilities.CheckTimestampedEvents(
                receivedEventsB,
                expectedReceivedEvents,
                MaximumEventSendReceiveDelay,
                "B");
            SendReceiveUtilities.CheckTimestampedEvents(
                receivedEventsC,
                expectedReceivedEvents,
                MaximumEventSendReceiveDelay,
                "C");
        }

        #endregion
    }
}
