using System;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
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

        private const int RetriesNumber = 3;

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

        [Retry(RetriesNumber)]
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

                SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);

                var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
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
        public void InputDeviceIsReleasedByDispose()
        {
            for (var i = 0; i < 10; i++)
            {
                var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                Assert.DoesNotThrow(() => inputDevice.StartEventsListening());
                inputDevice.Dispose();
            }
        }

        [Test]
        public void InputDeviceIsReleasedByFinalizer()
        {
            for (var i = 0; i < 10; i++)
            {
                {
                    var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                    Assert.DoesNotThrow(() => inputDevice.StartEventsListening());
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [Test]
        public void InputDeviceIsInUse()
        {
            using (var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice1.StartEventsListening();

                using (var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    Assert.Throws<MidiDeviceException>(() => inputDevice2.StartEventsListening());
                }
            }
        }

        #endregion
    }
}
