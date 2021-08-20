using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    [Platform("MacOsX")]
    public sealed class VirtualDeviceTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [Test]
        public void CantDisposeVirtualDeviceSubdevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                Assert.Throws<InvalidOperationException>(() => virtualDevice.InputDevice.Dispose(), "Dispose not failed for input subdevice.");
                Assert.Throws<InvalidOperationException>(() => virtualDevice.OutputDevice.Dispose(), "Dispose not failed for output subdevice.");
            }
        }

        [Test]
        public void CreateVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var deviceName = virtualDevice.Name;

                Assert.AreEqual(deviceName, virtualDevice.Name, "Name is invalid.");

                Assert.IsNotNull(virtualDevice.InputDevice, "Input device is null.");
                Assert.IsNotNull(deviceName, virtualDevice.InputDevice.Name, "Input device name is null.");

                Assert.IsNotNull(virtualDevice.OutputDevice, "Output device is null.");
                Assert.IsNotNull(deviceName, virtualDevice.OutputDevice.Name, "Output device name is null.");
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void SendEventToVirtualDevice_SysEx()
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
        public void SendEventToVirtualDevice_Short_Default(MidiEventType eventType)
        {
            var midiEvent = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(SysExEvent).IsAssignableFrom(t) && !typeof(MetaEvent).IsAssignableFrom(t))
                .Select(t => (MidiEvent)Activator.CreateInstance(t))
                .First(e => e.EventType == eventType);

            SendEvent(midiEvent);
        }

        [Retry(RetriesNumber)]
        [TestCaseSource(nameof(GetNonDefaultShortEvents))]
        public void SendEventToVirtualDevice_Short_NonDefault(MidiEvent midiEvent)
        {
            SendEvent(midiEvent);
        }

        [Test]
        public void FindVirtualDeviceSubdevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var deviceName = virtualDevice.Name;

                var timeout = TimeSpan.FromSeconds(5);
                var subdevicesFound = WaitOperations.Wait(() => InputDevice.GetAll().Any(d => d.Name == deviceName) && OutputDevice.GetAll().Any(d => d.Name == deviceName), timeout);

                Assert.IsTrue(subdevicesFound, "Subdevices were not found.");
            }
        }

        [Test]
        public void CheckVirtualDeviceSubdevicesEquality_SameDevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            using (var inputDevice = InputDevice.GetByName(virtualDevice.Name))
            using (var outputDevice = OutputDevice.GetByName(virtualDevice.Name))
            {
                Assert.AreEqual(virtualDevice.InputDevice, inputDevice, "Input device is not equal to virtual input subdevice.");
                Assert.AreEqual(virtualDevice.OutputDevice, outputDevice, "Output device is not equal to virtual output subdevice.");
            }
        }

        [Test]
        public void CheckVirtualDeviceSubdevicesEquality_DifferentDevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceB))
            {
                Assert.AreNotEqual(virtualDevice.InputDevice, inputDevice, "Input device is equal to virtual input subdevice.");
                Assert.AreNotEqual(virtualDevice.OutputDevice, outputDevice, "Output device is equal to virtual output subdevice.");
            }
        }

        #endregion

        #region Private methods

        private string GetVirtualDeviceName() => Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);

        private VirtualDevice GetVirtualDevice()
        {
            var virtualDevice = VirtualDevice.Create(GetVirtualDeviceName());
            return virtualDevice;
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
            using (var virtualDevice = GetVirtualDevice())
            {
                string errorOnVirtualDevice = null;
                virtualDevice.ErrorOccurred += (_, e) => errorOnVirtualDevice = e.Exception.Message;

                var outputDevice = virtualDevice.OutputDevice;
                SendReceiveUtilities.WarmUpDevice(outputDevice);

                var inputDevice = virtualDevice.InputDevice;

                MidiEvent eventSent = null;
                outputDevice.EventSent += (_, e) => eventSent = e.Event;

                string errorOnSend = null;
                outputDevice.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

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
                    var errorBuilder = new StringBuilder();

                    if (errorOnSend != null)
                        errorBuilder.AppendLine($"Failed to send event: {errorOnSend}");
                    if (errorOnReceive != null)
                        errorBuilder.AppendLine($"Failed to receive event: {errorOnReceive}");
                    if (errorOnVirtualDevice != null)
                        errorBuilder.AppendLine($"Failed to route event within virtual device: {errorOnVirtualDevice}");

                    if (errorBuilder.Length == 0)
                        errorBuilder.AppendLine("Event either not sent ot not received.");

                    Assert.Fail(errorBuilder.ToString());
                }

                MidiAsserts.AreEventsEqual(midiEvent, eventSent, false, "Sent event is invalid.");
                MidiAsserts.AreEventsEqual(eventSent, eventReceived, false, "Received event is invalid.");
            }
        }

        #endregion
    }
}
