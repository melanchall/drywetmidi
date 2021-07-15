using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
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

        private const int RetriesNumber = 3;

        #endregion

        #region Extern functions

        [DllImport("SendTestData", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendData(IntPtr portName, byte[] data, int length, int[] indices, int indicesLength);

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
        [Platform("Win")]
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
        [Platform("MacOsX")]
        public void ReceiveData_SingleEventWithStatusByte() => ReceiveData(
            data: new byte[] { 0x90, 0x75, 0x56 },
            indices: new[] { 0 },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56)
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_MultipleEventsWithStatusBytes() => ReceiveData(
            data: new byte[] { 0x90, 0x75, 0x56, 0x80, 0x55, 0x65, 0x90, 0x75, 0x56 },
            indices: new[] { 0, 3, 6 },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
                new NoteOffEvent((SevenBitNumber)0x55, (SevenBitNumber)0x65),
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_MultipleEventsWithRunningStatus() => ReceiveData(
            data: new byte[] { 0x90, 0x15, 0x56, 0x55, 0x65, 0x45, 0x60 },
            indices: new[] { 0 },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x15, (SevenBitNumber)0x56),
                new NoteOnEvent((SevenBitNumber)0x55, (SevenBitNumber)0x65),
                new NoteOnEvent((SevenBitNumber)0x45, (SevenBitNumber)0x60),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_LotOfEventsWithStatusBytes()
        {
            const int eventsCount = 3333;

            ReceiveData(
                data: Enumerable
                    .Range(0, eventsCount)
                    .SelectMany(i => new byte[] { 0x90, 0x75, 0x56 })
                    .ToArray(),
                indices: Enumerable
                    .Range(0, eventsCount)
                    .Select(i => i * 3)
                    .ToArray(),
                expectedEvents: Enumerable
                    .Range(0, eventsCount)
                    .Select(i => new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56))
                    .ToArray());
        }

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_UnexpectedRunningStatus()
        {
            var deviceName = MidiDevicesNames.DeviceA;
            var deviceNamePtr = Marshal.StringToHGlobalAnsi(deviceName);

            var data = new byte[] { 0x56, 0x67, 0x45 };
            var indices = new[] { 0 };

            using (var inputDevice = InputDevice.GetByName(deviceName))
            {
                Exception exception = null;

                inputDevice.ErrorOccurred += (_, e) => exception = e.Exception;
                inputDevice.StartEventsListening();

                SendData(deviceNamePtr, data, data.Length, indices, indices.Length);

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var errorOccurred = WaitOperations.Wait(() => exception != null, timeout);
                Assert.IsTrue(errorOccurred, $"Error was not occurred for [{timeout}].");
                Assert.IsInstanceOf(typeof(MidiDeviceException), exception, "Exception type is invalid");
                Assert.IsInstanceOf(typeof(UnexpectedRunningStatusException), exception.InnerException, "Inner exception type is invalid.");
            }
        }

        #endregion

        #region Private methods

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
