using System;
using System.Linq;
using System.Threading;
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
        public void SendEvent_Channel()
        {
            SendEvent(new NoteOnEvent((SevenBitNumber)45, (SevenBitNumber)89)
            {
                Channel = (FourBitNumber)6
            });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SendEvent_SysEx()
        {
            SendEvent(new NormalSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SendEvent_SystemCommon()
        {
            SendEvent(new TuneRequestEvent());
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SendEvent_SystemRealTime()
        {
            SendEvent(new StartEvent());
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
                var eventReceived = SpinWait.SpinUntil(() => sentEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(eventReceived, "Event is not sent.");

                outputDevice.IsEnabled = false;
                Assert.IsFalse(outputDevice.IsEnabled, "Device is enabled after disabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = SpinWait.SpinUntil(() => sentEventsCount > 1, TimeSpan.FromSeconds(5));
                Assert.IsFalse(eventReceived, "Event is sent after device disabled.");

                outputDevice.IsEnabled = true;
                Assert.IsTrue(outputDevice.IsEnabled, "Device is disabled after enabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = SpinWait.SpinUntil(() => sentEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(eventReceived, "Event is not sent after enabling again.");
            }
        }

        #endregion

        #region Private methods

        public void SendEvent(MidiEvent midiEvent)
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
                    var isEventSentReceived = SpinWait.SpinUntil(() => eventSent != null && eventReceived != null, timeout);

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
