using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class InputDeviceTests
    {
        #region Nested classes

        private sealed class MidiTimeCode
        {
            public MidiTimeCode(MidiTimeCodeType timeCodeType, int hours, int minutes, int seconds, int frames)
            {
                Format = timeCodeType;
                Hours = hours;
                Minutes = minutes;
                Seconds = seconds;
                Frames = frames;
            }

            public MidiTimeCodeType Format { get; }

            public int Hours { get; }

            public int Minutes { get; }

            public int Seconds { get; }

            public int Frames { get; }

            public override string ToString()
            {
                return $"[{Format}] {Hours}:{Minutes}:{Seconds}.{Frames}";
            }
        }

        #endregion

        #region Constants

        private const string DeviceToTestOnName = MidiDevicesNames.DeviceA;
        private static readonly TimeSpan MaximumEventReceivingDelay = TimeSpan.FromMilliseconds(30);

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
            Assert.AreEqual(deviceName, device.Name, "Device retrieved by ID is not the same as retrieved by its name.");
        }

        [Test]
        public void CheckEventsReceiving()
        {
            CheckEventsReceiving(new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NormalSysExEvent(new byte[] { 1, 2, 3, 0xF7 }), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NormalSysExEvent(new byte[] { 4, 5, 6, 0xF7 }), TimeSpan.FromSeconds(2)),
                new EventToSend(new SongSelectEvent((SevenBitNumber)20), TimeSpan.Zero),
                new EventToSend(new TuneRequestEvent(), TimeSpan.FromMilliseconds(200)),
            });
        }

        [Test]
        public void CheckMidiTimeCodeEventReceiving()
        {
            MidiTimeCode midiTimeCodeReceived = null;

            var eventsToSend = new[]
            {
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)100), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.FramesLsb, (FourBitNumber)1), TimeSpan.FromSeconds(1)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.FramesMsb, (FourBitNumber)1), TimeSpan.FromSeconds(2)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)7), TimeSpan.FromSeconds(1)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursMsbAndTimeCodeType, (FourBitNumber)7), TimeSpan.FromSeconds(2)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.MinutesLsb, (FourBitNumber)10), TimeSpan.FromSeconds(1)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)10), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)15), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.MinutesMsb, (FourBitNumber)2), TimeSpan.FromSeconds(2)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsLsb, (FourBitNumber)10), TimeSpan.FromSeconds(1)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsMsb, (FourBitNumber)1), TimeSpan.FromSeconds(2))
            };

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice.MidiTimeCodeReceived += (_, e) => midiTimeCodeReceived = new MidiTimeCode(e.Format, e.Hours, e.Minutes, e.Seconds, e.Frames);
                inputDevice.StartEventsListening();

                SendEvents(eventsToSend, outputDevice);

                var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + MaximumEventReceivingDelay;
                var isMidiTimeCodeReceived = SpinWait.SpinUntil(() => midiTimeCodeReceived != null, timeout);
                Assert.IsTrue(isMidiTimeCodeReceived, $"MIDI time code received for timeout {timeout}.");

                inputDevice.StopEventsListening();
            }

            Assert.AreEqual(MidiTimeCodeType.Thirty, midiTimeCodeReceived.Format, "Format is invalid.");
            Assert.AreEqual(23, midiTimeCodeReceived.Hours, "Hours number is invalid.");
            Assert.AreEqual(42, midiTimeCodeReceived.Minutes, "Minutes number is invalid.");
            Assert.AreEqual(26, midiTimeCodeReceived.Seconds, "Seconds number is invalid.");
            Assert.AreEqual(17, midiTimeCodeReceived.Frames, "Frames number is invalid.");
        }

        [Test]
        public void CheckFileEventsReceiving()
        {
            var filesToTest = TestFilesProvider.GetValidFiles(
                f => f.GetTrackChunks().Count() == 1,
                f =>
                {
                    var tempoMap = f.GetTempoMap();
                    return (TimeSpan)f.GetTimedEvents().Last().TimeAs<MetricTimeSpan>(tempoMap) < TimeSpan.FromSeconds(30);
                })
                .Take(5)
                .ToArray();

            for (var i = 0; i < filesToTest.Length; i++)
            {
                var file = filesToTest[i];
                var tempoMap = file.GetTempoMap();

                var eventsToSend = new List<EventToSend>();
                var currentTime = TimeSpan.Zero;

                foreach (var timedEvent in file.GetTimedEvents().Where(e => !(e.Event is MetaEvent)))
                {
                    var time = (TimeSpan)timedEvent.TimeAs<MetricTimeSpan>(tempoMap);
                    var eventToSend = new EventToSend(timedEvent.Event, time - currentTime);
                    currentTime = time;
                    eventsToSend.Add(eventToSend);
                }

                CheckEventsReceiving(eventsToSend);
            }
        }

        #endregion

        #region Private methods

        private void CheckEventsReceiving(ICollection<EventToSend> eventsToSend)
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

                    var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + MaximumEventReceivingDelay;
                    var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsToSend.Count, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                    inputDevice.StopEventsListening();
                }
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents);
        }

        private void SendEvents(IEnumerable<EventToSend> eventsToSend, OutputDevice outputDevice)
        {
            foreach (var eventToSend in eventsToSend)
            {
                Thread.Sleep(eventToSend.Delay);
                outputDevice.SendEvent(eventToSend.Event);
            }
        }

        private void CompareSentReceivedEvents(IEnumerable<SentEvent> sentEvents, IEnumerable<ReceivedEvent> receivedEvents)
        {
            var eventsPairs = sentEvents.Zip(receivedEvents, (s, r) => new { Sent = s, Received = r }).ToList();
            for (var i = 0; i < eventsPairs.Count; i++)
            {
                var eventsPair = eventsPairs[i];
                var sentEvent = eventsPair.Sent;
                var receivedEvent = eventsPair.Received;

                Assert.IsTrue(
                    MidiEventEquality.AreEqual(sentEvent.Event, receivedEvent.Event, false),
                    $"Received event {receivedEvent.Event} doesn't match sent one {sentEvent.Event}.");

                var delay = receivedEvent.Time - sentEvent.Time;
                Assert.IsTrue(
                    delay <= MaximumEventReceivingDelay,
                    $"Event was received too late (at {receivedEvent.Time} instead of {sentEvent.Time}).");
            }
        }

        private void WarmUpDevice(OutputDevice outputDevice)
        {
            var eventsToSend = Enumerable.Range(0, 100)
                                         .SelectMany(_ => new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() })
                                         .Select(e => new EventToSend(e, TimeSpan.FromMilliseconds(10)))
                                         .ToList();

            outputDevice.PrepareForEventsSending();
            SendEvents(eventsToSend, outputDevice);
        }

        #endregion
    }
}
