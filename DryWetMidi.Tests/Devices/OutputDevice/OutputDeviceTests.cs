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
        [TestCase(MidiDevicesNames.MicrosoftGSWavetableSynth)]
        public void FindOutputDevice(string deviceName)
        {
            Assert.IsTrue(
                OutputDevice.GetAll().Any(d => d.Name == deviceName),
                $"There is no device '{deviceName}' in the system.");
        }

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        [TestCase(MidiDevicesNames.MicrosoftGSWavetableSynth)]
        public void CheckOutputDeviceId(string deviceName)
        {
            var device = OutputDevice.GetByName(deviceName);
            Assert.IsNotNull(device, $"Unable to get device '{deviceName}' by its name.");

            var deviceId = device.Id;
            device = OutputDevice.GetById(deviceId);
            Assert.IsNotNull(device, $"Unable to get device '{deviceName}' by its ID.");
            Assert.AreEqual(deviceName, device.Name, "Device retrieved by ID is not the same as retrieved by its name.");//
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

        // TODO
        // [Test]
        public void SetVolume()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.MicrosoftGSWavetableSynth))
            {
                var expectedVolume = new Volume(500);
                outputDevice.Volume = expectedVolume;
                Assert.AreEqual(expectedVolume, outputDevice.Volume, "Volume is invalid.");
            }
        }

        // TODO
        // [Test]
        public void GetVolume()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.MicrosoftGSWavetableSynth))
            {
                Assert.Greater(outputDevice.Volume.LeftVolume, 0, "Left volume is invalid.");
                Assert.Greater(outputDevice.Volume.RightVolume, 0, "Right volume is invalid.");
            }
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
            for (var i = 0; i < 10; i++)
            {
                {
                    var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
                    Assert.DoesNotThrow(() => outputDevice.SendEvent(new NoteOnEvent()));
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [Test]
        public void OutputDeviceIsInUse()
        {
            using (var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputDevice1.SendEvent(new NoteOnEvent());

                using (var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    var exception = Assert.Throws<MidiDeviceException>(() => outputDevice2.SendEvent(new NoteOnEvent()));
                    Assert.AreEqual(
                        "The specified device is already in use.  Wait until it is free, and then try again.",
                        exception.Message,
                        "Exception's message is invalid.");
                }
            }
        }

        #endregion

        #region Private methods

        public void SendEvent(MidiEvent midiEvent)
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                MidiEvent eventSent = null;
                outputDevice.EventSent += (_, e) => eventSent = e.Event;

                string errorOnSend = null;
                outputDevice.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

                using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
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
