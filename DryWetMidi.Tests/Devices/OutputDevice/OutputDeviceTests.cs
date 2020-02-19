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
        public void SendEvent_MidiEvent()
        {
            var midiEvent = new NoteOnEvent((SevenBitNumber)45, (SevenBitNumber)89)
            {
                Channel = (FourBitNumber)6
            };

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                MidiEvent eventSent = null;
                outputDevice.EventSent += (_, e) => eventSent = e.Event;

                using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    MidiEvent eventReceived = null;
                    inputDevice.EventReceived += (_, e) => eventReceived = e.Event;

                    inputDevice.StartEventsListening();

                    outputDevice.PrepareForEventsSending();
                    outputDevice.SendEvent(new NoteOnEvent((SevenBitNumber)45, (SevenBitNumber)89) { Channel = (FourBitNumber)6 });

                    var timeout = TimeSpan.FromMilliseconds(15);
                    var isEventSentReceived = SpinWait.SpinUntil(() => eventSent != null && eventReceived != null, timeout);
                    Assert.IsTrue(isEventSentReceived, "Event either not sent ot not received.");

                    MidiAsserts.AreEventsEqual(midiEvent, eventSent, false, "Sent event is invalid.");
                    MidiAsserts.AreEventsEqual(eventSent, eventReceived, false, "Received event is invalid.");
                }
            }
        }

        [Test]
        public void SetVolume()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.MicrosoftGSWavetableSynth))
            {
                var expectedVolume = new Volume(500);
                outputDevice.Volume = expectedVolume;
                Assert.AreEqual(expectedVolume, outputDevice.Volume, "Volume is invalid.");
            }
        }

        [Test]
        public void GetVolume()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.MicrosoftGSWavetableSynth))
            {
                Assert.Greater(outputDevice.Volume.LeftVolume, 0, "Left volume is invalid.");
                Assert.Greater(outputDevice.Volume.RightVolume, 0, "Right volume is invalid.");
            }
        }

        #endregion
    }
}
