using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class InputDeviceTests
    {
        #region Nested classes

        private sealed class EventToSend
        {
            public EventToSend(MidiEvent midiEvent, TimeSpan delay)
            {
                Event = midiEvent;
                Delay = delay;
            }

            public MidiEvent Event { get; }

            public TimeSpan Delay { get; }
        }

        private sealed class ReceivedEvent
        {
            public ReceivedEvent(MidiEvent midiEvent, TimeSpan time)
            {
                Event = midiEvent;
                Time = time;
            }

            public MidiEvent Event { get; }

            public TimeSpan Time { get; }
        }

        #endregion

        #region Constants

        private static readonly TimeSpan EventReceivingDelay = TimeSpan.FromMilliseconds(50);

        #endregion

        #region Test methods

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void FindInputDevice(string deviceName)
        {
            Assert.IsTrue(
                InputDevice.GetAll().Any(d => d.Name == deviceName),
                $"There is no device '{deviceName}' in the system.");
        }

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void CheckInputDeviceId(string deviceName)
        {
            var device = InputDevice.GetByName(deviceName);
            Assert.IsNotNull(device, $"Unable to get device '{deviceName}' by its name.");

            var deviceId = device.Id;
            device = InputDevice.GetById(deviceId);
            Assert.IsNotNull(device, $"Unable to get device '{deviceName}' by its ID.");
            Assert.AreEqual(deviceName, device.Name, "Device retrieved by ID is not the same as retrieved by its name.");//
        }

        [Test]
        public void CheckEventsReceiving()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var eventsReceived = new List<ReceivedEvent>();
            var stopwatch = new Stopwatch();

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2))
            };

            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice.EventReceived += (_, e) => eventsReceived.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                inputDevice.StartEventsListening();

                stopwatch.Start();
                SendEvents(eventsToSend, outputDevice);
                stopwatch.Stop();
            }

            var timeout = TimeSpan.FromSeconds(2) + EventReceivingDelay;
            var areEventsReceived = SpinWait.SpinUntil(() => eventsReceived.Count == 2, timeout);
            Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

            CompareSentReceivedEvents(eventsToSend, eventsReceived);
        }

        #endregion

        #region Private methods

        private static void SendEvents(IEnumerable<EventToSend> eventsToSend, OutputDevice outputDevice)
        {
            foreach (var eventToSend in eventsToSend)
            {
                Thread.Sleep(eventToSend.Delay);
                outputDevice.SendEvent(eventToSend.Event);
            }
        }

        private static void CompareSentReceivedEvents(IEnumerable<EventToSend> sentEvents, IEnumerable<ReceivedEvent> receivedEvents)
        {
            var currentTime = TimeSpan.Zero;

            foreach (var eventsPair in sentEvents.Zip(receivedEvents, (s, r) => new { Sent = s, Received = r }))
            {
                var sentEvent = eventsPair.Sent;
                var receivedEvent = eventsPair.Received;

                currentTime += sentEvent.Delay;

                Assert.IsTrue(
                    MidiEventEquality.AreEqual(sentEvent.Event, receivedEvent.Event, false),
                    $"Received event {receivedEvent.Event} doesn't match sent one {sentEvent.Event}.");

                // TODO: decrease delta!
                Assert.IsTrue(
                    receivedEvent.Time - currentTime <= EventReceivingDelay,
                    $"Event received too late (at {receivedEvent.Time} instead of {currentTime}).");
            }
        }

        #endregion
    }
}
