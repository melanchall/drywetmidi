using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class InputDeviceTests
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

        #region Extern functions

        [DllImport("SendTestData", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendData(IntPtr portName, byte[] data, int length, int[] indices, int indicesLength);

        #endregion

        #region Test methods

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void FindInputDeviceByName(string deviceName)
        {
            Assert.IsNotNull(InputDevice.GetByName(deviceName), "There is no device.");
        }

        [Test]
        public void FindInputDeviceByIndex_Valid()
        {
            var devicesCount = InputDevice.GetDevicesCount();
            Assert.IsNotNull(InputDevice.GetByIndex(devicesCount / 2), "There is no device.");
        }

        [Test]
        public void FindInputDeviceByIndex_BelowZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => InputDevice.GetByIndex(-1), "Exception is not thrown.");
        }

        [Test]
        public void FindInputDeviceByIndex_BeyondDevicesCount()
        {
            var devicesCount = InputDevice.GetDevicesCount();
            Assert.Throws<ArgumentOutOfRangeException>(() => InputDevice.GetByIndex(devicesCount), "Exception is not thrown.");
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
                var isMidiTimeCodeReceived = WaitOperations.Wait(() => midiTimeCodeReceived != null, timeout);
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
            Func<bool> openDevice = () =>
            {
                var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                try
                {
                    inputDevice.StartEventsListening();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            };

            for (var i = 0; i < 10; i++)
            {
                Assert.IsTrue(openDevice(), $"Can't open device on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [Test]
        public void DisableEnableInputDevice()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                Assert.IsTrue(inputDevice.IsEnabled, "Device is not enabled initially.");

                var receivedEventsCount = 0;

                inputDevice.StartEventsListening();
                inputDevice.EventReceived += (_, __) => receivedEventsCount++;

                outputDevice.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => receivedEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(eventReceived, "Event is not received.");

                inputDevice.IsEnabled = false;
                Assert.IsFalse(inputDevice.IsEnabled, "Device is enabled after disabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, TimeSpan.FromSeconds(5));
                Assert.IsFalse(eventReceived, "Event is received after device disabled.");

                inputDevice.IsEnabled = true;
                Assert.IsTrue(inputDevice.IsEnabled, "Device is disabled after enabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(eventReceived, "Event is not received after enabling again.");
            }
        }

        [Test]
        public void InputDeviceToString_User()
        {
            var inputDevice = GetUserInputDevice();
            Assert.AreEqual("Input device", inputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        public void GetInputDeviceHashCode()
        {
            foreach (var inputDevice in InputDevice.GetAll())
            {
                Assert.DoesNotThrow(() => inputDevice.GetHashCode(), $"Failed to get hash code for [{inputDevice.Name}].");
            }
        }

        #endregion

        #region Private methods

        private static InputDevice GetUserInputDevice()
        {
            return InputDevice.GetByName(MidiDevicesNames.DeviceA);
        }

        private void ReceiveData(byte[] data, int[] indices, ICollection<MidiEvent> expectedEvents)
        {
            var deviceName = MidiDevicesNames.DeviceA;
            var deviceNamePtr = Marshal.StringToHGlobalAnsi(deviceName);

            var receivedEvents = new List<MidiEvent>(expectedEvents.Count);

            using (var inputDevice = InputDevice.GetByName(deviceName))
            {
                inputDevice.EventReceived += (_, e) => receivedEvents.Add(e.Event);
                inputDevice.StartEventsListening();

                SendData(deviceNamePtr, data, data.Length, indices, indices.Length);

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var areEventReceived = WaitOperations.Wait(() => receivedEvents.Count >= expectedEvents.Count, timeout);
                Assert.IsTrue(areEventReceived, $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}).");

                MidiAsserts.AreEqual(
                    expectedEvents,
                    receivedEvents,
                    false,
                    "Received events are invalid.");
            }
        }

        #endregion
    }
}
