using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class OutputDeviceTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void FindOutputDevice(string deviceName)
        {
            Assert.IsTrue(
                OutputDevice.GetAll().Any(d => d.Name == deviceName),
                $"There is no device '{deviceName}' in the system.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SendEvent_SysEx()
        {
            SendEvent(new NormalSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }));
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
                Assert.DoesNotThrow(() => outputDevice.SendEvent(new NoteOnEvent()));
                outputDevice.Dispose();
            }
        }

        [Test]
        public void OutputDeviceIsReleasedByFinalizer()
        {
            Func<bool> sendEvent = () =>
            {
                var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
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
                Assert.IsTrue(sendEvent(), $"Can't send event on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [Test]
        [Platform("Win")]
        public void OutputDeviceIsInUse()
        {
            using (var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputDevice1.SendEvent(new NoteOnEvent());

                using (var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    var exception = Assert.Throws<MidiDeviceException>(() => outputDevice2.SendEvent(new NoteOnEvent()));
                    Assert.AreEqual(
                        "The device is already in use.",
                        exception.Message,
                        "Exception's message is invalid.");
                }
            }
        }

        [Test]
        public void DisableEnableOutputDevice()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                Assert.IsTrue(outputDevice.IsEnabled, "Device is not enabled initially.");

                var sentEventsCount = 0;

                outputDevice.EventSent += (_, __) => sentEventsCount++;

                outputDevice.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => sentEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(eventReceived, "Event is not sent.");

                outputDevice.IsEnabled = false;
                Assert.IsFalse(outputDevice.IsEnabled, "Device is enabled after disabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, TimeSpan.FromSeconds(5));
                Assert.IsFalse(eventReceived, "Event is sent after device disabled.");

                outputDevice.IsEnabled = true;
                Assert.IsTrue(outputDevice.IsEnabled, "Device is disabled after enabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(eventReceived, "Event is not sent after enabling again.");
            }
        }

        #endregion

        #region Private methods

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

                    var timeout = TimeSpan.FromMilliseconds(15);
                    var isEventSentReceived = WaitOperations.Wait(() => eventSent != null && eventReceived != null, timeout);

                    if (!isEventSentReceived)
                    {
                        if (errorOnSend != null)
                            Assert.Fail($"Failed to send event: {errorOnSend}");
                        else if (errorOnReceive != null)
                            Assert.Fail($"Failed to receive event: {errorOnReceive}");
                        else
                            Assert.Fail("Event either not sent ot not received.");
                    }

                    MidiAsserts.AreEventsEqual(midiEvent, eventSent, false, "Sent event is invalid.");
                    MidiAsserts.AreEventsEqual(eventSent, eventReceived, false, "Received event is invalid.");
                }
            }
        }

        #endregion
    }
}
