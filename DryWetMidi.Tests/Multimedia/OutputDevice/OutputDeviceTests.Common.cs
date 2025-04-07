using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputDeviceTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void GetOutputDeviceByName(string deviceName)
        {
            ClassicAssert.IsNotNull(OutputDevice.GetByName(deviceName), "There is no device.");
        }

        [Test]
        public void GetOutputDeviceByIndex_Valid()
        {
            var devicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.IsNotNull(OutputDevice.GetByIndex(devicesCount / 2), "There is no device.");
        }

        [Test]
        public void GetOutputDeviceByIndex_BelowZero()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => OutputDevice.GetByIndex(-1), "Exception is not thrown.");
        }

        [Test]
        public void GetOutputDeviceByIndex_BeyondDevicesCount()
        {
            var devicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => OutputDevice.GetByIndex(devicesCount), "Exception is not thrown.");
        }

        [Test]
        public void GetAllOutputDevices()
        {
            var outputDevices = OutputDevice.GetAll();
            var outputDevicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.AreEqual(outputDevicesCount, outputDevices.Count, "Output devices count is invalid.");
        }

        [Test]
        public void GetOutputDevicesCount()
        {
            var outputDevicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.GreaterOrEqual(
                outputDevicesCount,
                MidiDevicesNames.GetAllDevicesNames().Length,
                "Output devices count is invalid.");
        }

        [Timeout(60 * 1000)]
        [Retry(RetriesNumber)]
        [Test]
        public void SendEvent_SysEx()
        {
            SendEvent(new NormalSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }));
        }

        [Timeout(60 * 1000)]
        [Retry(RetriesNumber)]
        [Test]
        public void SendEvent_SysEx_Large([Values(100, 1000, 10000)] int size)
        {
            SendEvent(new NormalSysExEvent(
                Enumerable
                    .Range(0, size)
                    .Select(_ => (byte)0x50)
                    .Concat(new byte[] { 0xF7 })
                    .ToArray()));
        }

        [Retry(RetriesNumber)]
        [TestCase(MidiEventType.ActiveSensing)]
        [TestCase(MidiEventType.Continue)]
        [TestCase(MidiEventType.Reset)]
        [TestCase(MidiEventType.Start)]
        [TestCase(MidiEventType.Stop)]
        [TestCase(MidiEventType.TimingClock)]
        [TestCase(MidiEventType.MidiTimeCode)]
        [TestCase(MidiEventType.SongPositionPointer)]
        [TestCase(MidiEventType.SongSelect)]
        [TestCase(MidiEventType.TuneRequest)]
        [TestCase(MidiEventType.ChannelAftertouch)]
        [TestCase(MidiEventType.ControlChange)]
        [TestCase(MidiEventType.NoteAftertouch)]
        [TestCase(MidiEventType.NoteOff)]
        [TestCase(MidiEventType.NoteOn)]
        [TestCase(MidiEventType.PitchBend)]
        [TestCase(MidiEventType.ProgramChange)]
        public void SendEvent_Short_Default(MidiEventType eventType)
        {
            var midiEvent = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(SysExEvent).IsAssignableFrom(t) && !typeof(MetaEvent).IsAssignableFrom(t))
                .Select(t => (MidiEvent)Activator.CreateInstance(t))
                .First(e => e.EventType == eventType);

            SendEvent(midiEvent);
        }

        [Retry(RetriesNumber)]
        [TestCaseSource(nameof(GetNonDefaultShortEvents))]
        public void SendEvent_Short_NonDefault(MidiEvent midiEvent)
        {
            SendEvent(midiEvent);
        }

        [Test]
        public void OutputDeviceIsReleasedByDispose()
        {
            for (var i = 0; i < 10; i++)
            {
                var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
                ClassicAssert.DoesNotThrow(() => outputDevice.SendEvent(new NoteOnEvent()));
                outputDevice.Dispose();
            }
        }

#if TEST
        [Test]
        public void OutputDeviceIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, bool> sendEvent = testCheckpoints =>
            {
                var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
                outputDevice.TestCheckpoints = testCheckpoints;

                try
                {
                    outputDevice.SendEvent(new NoteOnEvent());
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            };

            for (var i = 0; i < 10; i++)
            {
                var checkpoints = new TestCheckpoints();

                checkpoints.CheckCheckpointNotReached(OutputDeviceCheckpointsNames.HandleFinalizerEntered);
                checkpoints.CheckCheckpointNotReached(OutputDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);

                ClassicAssert.IsTrue(sendEvent(checkpoints), $"Can't send event on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();

                checkpoints.CheckCheckpointReached(OutputDeviceCheckpointsNames.HandleFinalizerEntered);
                checkpoints.CheckCheckpointReached(OutputDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);
            }
        }
#endif

        [Test]
        public void DisableEnableOutputDevice()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                ClassicAssert.IsTrue(outputDevice.IsEnabled, "Device is not enabled initially.");

                var sentEventsCount = 0;

                outputDevice.EventSent += (_, __) => sentEventsCount++;

                outputDevice.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => sentEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not sent.");

                outputDevice.IsEnabled = false;
                ClassicAssert.IsFalse(outputDevice.IsEnabled, "Device is enabled after disabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is sent after device disabled.");

                outputDevice.IsEnabled = true;
                ClassicAssert.IsTrue(outputDevice.IsEnabled, "Device is disabled after enabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not sent after enabling again.");
            }
        }

        [Test]
        public void OutputDeviceToString_User()
        {
            var outputDevice = GetUserOutputDevice();
            ClassicAssert.AreEqual("Output device", outputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        public void GetOutputDeviceHashCode()
        {
            foreach (var outputDevice in OutputDevice.GetAll())
            {
                ClassicAssert.DoesNotThrow(() => outputDevice.GetHashCode(), $"Failed to get hash code for [{outputDevice.Name}].");
            }
        }

        #endregion

        #region Private methods

        private static OutputDevice GetUserOutputDevice()
        {
            return OutputDevice.GetByName(MidiDevicesNames.DeviceA);
        }

        private static IEnumerable<MidiEvent> GetNonDefaultShortEvents() => new MidiEvent[]
        {
            new MidiTimeCodeEvent(MidiTimeCodeComponent.MinutesLsb, (FourBitNumber)10),
            new SongPositionPointerEvent(1234),
            new SongSelectEvent((SevenBitNumber)30),
            new ChannelAftertouchEvent((SevenBitNumber)70) { Channel = (FourBitNumber)7 },
            new ControlChangeEvent((SevenBitNumber)90, (SevenBitNumber)60) { Channel = (FourBitNumber)1 },
            new NoteAftertouchEvent((SevenBitNumber)75, (SevenBitNumber)38) { Channel = (FourBitNumber)2 },
            new NoteOffEvent((SevenBitNumber)127, (SevenBitNumber)21) { Channel = (FourBitNumber)10 },
            new NoteOnEvent((SevenBitNumber)7, (SevenBitNumber)127) { Channel = (FourBitNumber)15 },
            new PitchBendEvent(10000) { Channel = (FourBitNumber)8 },
            new ProgramChangeEvent((SevenBitNumber)127) { Channel = (FourBitNumber)6 },
        };

        private void SendEvent(MidiEvent midiEvent)
        {
            var deviceName = MidiDevicesNames.DeviceA;

            using (var outputDevice = OutputDevice.GetByName(deviceName))
            {
                MidiEvent eventSent = null;
                outputDevice.EventSent += (_, e) => eventSent = e.Event;

                string errorOnSend = null;
                outputDevice.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

                using (var inputDevice = InputDevice.GetByName(deviceName))
                {
                    MidiEvent eventReceived = null;
                    inputDevice.EventReceived += (_, e) => eventReceived = e.Event;

                    string errorOnReceive = null;
                    inputDevice.ErrorOccurred += (_, e) => errorOnReceive = e.Exception.Message;

                    inputDevice.StartEventsListening();

                    outputDevice.PrepareForEventsSending();
                    outputDevice.SendEvent(midiEvent);

                    var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var isEventSentReceived = WaitOperations.Wait(() => eventSent != null && eventReceived != null, timeout);
                    if (!isEventSentReceived)
                    {
                        if (errorOnSend != null)
                            ClassicAssert.Fail($"Failed to send event: {errorOnSend}");
                        else if (errorOnReceive != null)
                            ClassicAssert.Fail($"Failed to receive event: {errorOnReceive}");
                        else
                            ClassicAssert.Fail("Event either not sent ot not received.");
                    }

                    MidiAsserts.AreEqual(midiEvent, eventSent, false, "Sent event is invalid.");
                    MidiAsserts.AreEqual(eventSent, eventReceived, false, "Received event is invalid.");
                }
            }
        }

        #endregion
    }
}
