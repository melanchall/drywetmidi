using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class DevicesConnectorTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(50);

        #endregion

        #region Test methods

        [MultimediaTestRetry]
        [Test]
        public void CheckEventsReceivingOnConnectedDevices()
        {
            CheckEventsReceiving(new[]
            {
                new TimestampedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new TimestampedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                new TimestampedEvent(new SongSelectEvent((SevenBitNumber)20), TimeSpan.FromSeconds(1)),
                new TimestampedEvent(new TuneRequestEvent(), TimeSpan.FromMilliseconds(1200)),
            });
        }

        [MultimediaTestRetry]
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

        [MultimediaTestRetry]
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

        [MultimediaTestRetry]
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

        #endregion

        #region Private methods

        private static void CheckEventsReceiving(
            IReadOnlyList<TimestampedEvent> eventsToSend)
        {
            var receivedEventsB = new List<TimestampedEvent>();
            var receivedEventsC = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();
            
            var stopwatch = new Stopwatch();

            using (var outputA = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputA.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputA.PrepareForEventsSending();

                using (var inputB = TestDeviceManager.GetInputDevice(MidiDevicesNames.DeviceB))
                using (var inputC = TestDeviceManager.GetInputDevice(MidiDevicesNames.DeviceC))
                {
                    inputB.EventReceived += (_, e) => receivedEventsB.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                    inputB.StartEventsListening();

                    inputC.EventReceived += (_, e) => receivedEventsC.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                    inputC.StartEventsListening();

                    using (var inputA = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                    {
                        inputA.StartEventsListening();

                        using (var outputB = TestDeviceManager.GetOutputDevice(MidiDevicesNames.DeviceB))
                        using (var outputC = TestDeviceManager.GetOutputDevice(MidiDevicesNames.DeviceC))
                        {
                            var devicesConnector = inputA.Connect(outputB, outputC);
                            ClassicAssert.IsTrue(devicesConnector.AreDevicesConnected, "Devices aren't connected.");

                            stopwatch.Start();
                            SendReceiveUtilities.SendEvents(eventsToSend, outputA);
                            stopwatch.Stop();

                            var timeout = eventsToSend.Max(e => e.Time) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                            var areEventsReceived = WaitOperations.Wait(
                                () => receivedEventsB.Count == eventsToSend.Count && receivedEventsC.Count == eventsToSend.Count,
                                timeout);
                            ClassicAssert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                            devicesConnector.Disconnect();
                            ClassicAssert.IsFalse(devicesConnector.AreDevicesConnected, "Devices aren't disconnected.");
                        }
                    }
                }
            }

            SendReceiveUtilities.CheckTimestampedEvents(sentEvents, eventsToSend, TimeSpan.FromMilliseconds(10));
            SendReceiveUtilities.CheckTimestampedEvents(receivedEventsB, eventsToSend, MaximumEventSendReceiveDelay);
            SendReceiveUtilities.CheckTimestampedEvents(receivedEventsC, eventsToSend, MaximumEventSendReceiveDelay);
        }

        private static void CheckEventsReceivingWithCallback(
            IReadOnlyList<TimestampedEvent> eventsToSend,
            DevicesConnectorEventCallback eventCallback,
            IReadOnlyList<TimestampedEvent> expectedReceivedEvents)
        {
            var receivedEventsB = new List<TimestampedEvent>();
            var receivedEventsC = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();
            
            var stopwatch = new Stopwatch();

            using (var outputA = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputA.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputA.PrepareForEventsSending();

                using (var inputB = TestDeviceManager.GetInputDevice(MidiDevicesNames.DeviceB))
                using (var inputC = TestDeviceManager.GetInputDevice(MidiDevicesNames.DeviceC))
                {
                    inputB.EventReceived += (_, e) => receivedEventsB.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                    inputB.StartEventsListening();

                    inputC.EventReceived += (_, e) => receivedEventsC.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                    inputC.StartEventsListening();

                    using (var inputA = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                    {
                        inputA.StartEventsListening();

                        using (var outputB = TestDeviceManager.GetOutputDevice(MidiDevicesNames.DeviceB))
                        using (var outputC = TestDeviceManager.GetOutputDevice(MidiDevicesNames.DeviceC))
                        {
                            var devicesConnector = inputA.Connect(outputB, outputC);
                            devicesConnector.EventCallback = eventCallback;
                            ClassicAssert.IsTrue(devicesConnector.AreDevicesConnected, "Devices aren't connected.");

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

                            devicesConnector.Disconnect();
                            ClassicAssert.IsFalse(devicesConnector.AreDevicesConnected, "Devices aren't disconnected.");
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
