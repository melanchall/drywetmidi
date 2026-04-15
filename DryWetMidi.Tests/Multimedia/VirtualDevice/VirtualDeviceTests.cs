using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    [VirtualDeviceApiRequired]
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
                ClassicAssert.Throws<InvalidOperationException>(() => virtualDevice.InputDevice.Dispose(), "Dispose not failed for input subdevice.");
                ClassicAssert.Throws<InvalidOperationException>(() => virtualDevice.OutputDevice.Dispose(), "Dispose not failed for output subdevice.");
            }
        }

        [Test]
        public void CreateVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var deviceName = virtualDevice.Name;

                ClassicAssert.AreEqual(deviceName, virtualDevice.Name, "Name is invalid.");

                ClassicAssert.IsNotNull(virtualDevice.InputDevice, "Input device is null.");
                ClassicAssert.IsNotNull(deviceName, virtualDevice.InputDevice.Name, "Input device name is null.");

                ClassicAssert.IsNotNull(virtualDevice.OutputDevice, "Output device is null.");
                ClassicAssert.IsNotNull(deviceName, virtualDevice.OutputDevice.Name, "Output device name is null.");
            }
        }

#if TEST
        [Test]
        public void DisposeVirtualDevice()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var checkpoints = new TestCheckpoints();

            var virtualDevice = GetVirtualDevice();
            virtualDevice.TestCheckpoints = checkpoints;
            virtualDevice.InputDevice.TestCheckpoints = checkpoints;
            virtualDevice.OutputDevice.TestCheckpoints = checkpoints;

            var inputDeviceFound = WaitOperations.Wait(() => InputDevice.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsTrue(inputDeviceFound, $"Input device is not found for [{timeout}].");

            var outputDeviceFound = WaitOperations.Wait(() => OutputDevice.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsTrue(outputDeviceFound, $"Output device is not found for [{timeout}].");

            checkpoints.CheckCheckpointsAreNotReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            virtualDevice.Dispose();

            inputDeviceFound = WaitOperations.Wait(() => InputDevice.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsFalse(inputDeviceFound, $"Input device is found after virtual device disposed after [{timeout}].");

            outputDeviceFound = WaitOperations.Wait(() => OutputDevice.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsFalse(outputDeviceFound, $"Output device is found after virtual device disposed after [{timeout}].");

            checkpoints.CheckCheckpointsReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
        }

        [Test]
        public void VirtualDeviceIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, string> createVirtualDevice = testCheckpoints =>
            {
                var virtualDevice = GetVirtualDevice();
                virtualDevice.TestCheckpoints = testCheckpoints;
                virtualDevice.InputDevice.TestCheckpoints = testCheckpoints;
                virtualDevice.OutputDevice.TestCheckpoints = testCheckpoints;

                return virtualDevice.Name;
            };

            var checkpoints = new TestCheckpoints();

            var deviceName = createVirtualDevice(checkpoints);

            checkpoints.CheckCheckpointsAreNotReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var timeout = TimeSpan.FromSeconds(5);

            checkpoints.CheckCheckpointsReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            var inputDeviceFound = WaitOperations.Wait(() => InputDevice.GetAll().Any(d => d.Name == deviceName), timeout);
            ClassicAssert.IsFalse(inputDeviceFound, $"Input device is found after virtual device disposed after [{timeout}].");

            var outputDeviceFound = WaitOperations.Wait(() => OutputDevice.GetAll().Any(d => d.Name == deviceName), timeout);
            ClassicAssert.IsFalse(outputDeviceFound, $"Output device is found after virtual device disposed after [{timeout}].");
        }
#endif

        [Test]
        public void SendEventToVirtualDevice_EscapeSysEx() => Assert.Throws<ArgumentException>(
            () => SendEvents(new[] { new EscapeSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }) }));

        [MultimediaTestRetry]
        [Test]
        public void SendEventToVirtualDevice_SysEx_1()
        {
            SendEvents(new[] { new NormalSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }) });
        }

        [MultimediaTestRetry]
        [Test]
        public void SendEventToVirtualDevice_SysEx_2()
        {
            SendEvents(new[] { new NormalSysExEvent(new byte[] { 0xF0, 0x5F, 0x40, 0xF7 }) });
        }

        // TODO: fix very large sys ex sending
        [MultimediaTestRetry]
        [Test]
        public void SendEventToVirtualDevice_SysEx_Large([Values(100, 1000, 10000/*, 100000*/)] int size)
        {
            SendEvents(new[] { new NormalSysExEvent(
                Enumerable
                    .Range(0, size)
                    .Select(_ => (byte)DryWetMidi.Common.Random.Instance.Next(127))
                    .Concat(new byte[] { 0xF7 })
                    .ToArray()) });
        }

        // TODO: failed
        [MultimediaTestRetry]
        // [Test]
        public void SendEventToVirtualDevice_SysEx_NotTerminated()
        {
            var bytes = new byte[] { 0xF0, 0x50 };
            SendEvents(
                new[] { new NormalSysExEvent(bytes) },
                receivedEvents =>
                {
                    var midiEvent = receivedEvents.Single().Event;
                    CollectionAssert.AreEqual(bytes, ((NormalSysExEvent)midiEvent).Data, "Received SysEx data is invalid.");
                });
        }

        [MultimediaTestRetry]
        [Test]
        public void SendEventToVirtualDevice_SysEx_Multiple([Values(2, 5, 10)] int eventsCount, [Values(1, 10, 100, 1000)] int dataSize)
        {
            SendEvents(Enumerable
                .Range(0, eventsCount)
                .Select(_ => new NormalSysExEvent(Enumerable
                    .Range(0, eventsCount)
                    .Select(__ => (byte)0x50)
                    .Concat(new byte[] { 0xF7 })
                    .ToArray()))
                .ToArray());
        }

        [MultimediaTestRetry]
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

            SendEvents(new[] { midiEvent });
        }

        [MultimediaTestRetry]
        [TestCaseSource(nameof(GetNonDefaultShortEvents))]
        public void SendEventToVirtualDevice_Short_NonDefault(MidiEvent midiEvent)
        {
            SendEvents(new[] { midiEvent });
        }

        [Test]
        public void FindVirtualDeviceSubdevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var deviceName = virtualDevice.Name;

                var timeout = TimeSpan.FromSeconds(5);
                var subdevicesFound = WaitOperations.Wait(() => InputDevice.GetAll().Any(d => d.Name == deviceName) && OutputDevice.GetAll().Any(d => d.Name == deviceName), timeout);

                ClassicAssert.IsTrue(subdevicesFound, "Subdevices were not found.");
            }
        }

        [Test]
        public void CheckVirtualDeviceSubdevicesEquality_SameDevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            using (var inputDevice = InputDevice.GetByName(virtualDevice.Name))
            using (var outputDevice = OutputDevice.GetByName(virtualDevice.Name))
            {
                ClassicAssert.AreEqual(virtualDevice.InputDevice, inputDevice, "Input device is not equal to virtual input subdevice.");
                ClassicAssert.AreEqual(virtualDevice.OutputDevice, outputDevice, "Output device is not equal to virtual output subdevice.");
            }
        }

        [Test]
        public void CheckVirtualDeviceSubdevicesEquality_DifferentDevices()
        {
            using (var virtualDevice = GetVirtualDevice())
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceB))
            {
                ClassicAssert.AreNotEqual(virtualDevice.InputDevice, inputDevice, "Input device is equal to virtual input subdevice.");
                ClassicAssert.AreNotEqual(virtualDevice.OutputDevice, outputDevice, "Output device is equal to virtual output subdevice.");
            }
        }

        [Test]
        public void DisableEnableVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                ClassicAssert.IsTrue(virtualDevice.IsEnabled, "Device is not enabled initially.");

                var inputDevice = virtualDevice.InputDevice;
                var outputDevice = virtualDevice.OutputDevice;

                var receivedEvents = new List<MidiEvent>();

                inputDevice.StartEventsListening();
                inputDevice.EventReceived += (_, e) => receivedEvents.Add(e.Event);

                outputDevice.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => receivedEvents.Count == 1 && receivedEvents.Last() is NoteOnEvent, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received.");

                virtualDevice.IsEnabled = false;
                ClassicAssert.IsFalse(virtualDevice.IsEnabled, "Device is enabled after disabling.");

                outputDevice.SendEvent(new NoteOffEvent());
                eventReceived = WaitOperations.Wait(() => receivedEvents.Count > 1 && receivedEvents.Last() is NoteOffEvent, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is received after device disabled.");

                virtualDevice.IsEnabled = true;
                ClassicAssert.IsTrue(virtualDevice.IsEnabled, "Device is disabled after enabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEvents.Count > 1 && receivedEvents.Last() is NoteOnEvent, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received after enabling again.");
            }
        }

        [Test]
        public void DisableEnableInputDeviceOfVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var inputDevice = virtualDevice.InputDevice;
                var outputDevice = virtualDevice.OutputDevice;

                ClassicAssert.IsTrue(inputDevice.IsEnabled, "Device is not enabled initially.");

                var receivedEventsCount = 0;

                inputDevice.StartEventsListening();
                inputDevice.EventReceived += (_, __) => receivedEventsCount++;

                outputDevice.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => receivedEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received.");

                inputDevice.IsEnabled = false;
                ClassicAssert.IsFalse(inputDevice.IsEnabled, "Device is enabled after disabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is received after device disabled.");

                inputDevice.IsEnabled = true;
                ClassicAssert.IsTrue(inputDevice.IsEnabled, "Device is disabled after enabling.");

                outputDevice.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received after enabling again.");
            }
        }

        [Test]
        public void DisableEnableOutputDeviceOfVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var inputDevice = virtualDevice.InputDevice;
                var outputDevice = virtualDevice.OutputDevice;

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

        #endregion

        #region Private methods

        private VirtualDevice GetVirtualDevice()
        {
            var deviceName = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);
            return VirtualDevice.Create(deviceName);
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

        private void SendEvents(
            MidiEvent[] midiEvents,
            Action<ICollection<TimestampedEvent>> checkAction = null,
            Action<InputDevice> setupInputDevice = null)
        {
            var stopwatch = new Stopwatch();

            var timestampedEvents = midiEvents
                .Select(e => new TimestampedEvent(e, TimeSpan.Zero))
                .ToArray();

            var receivedEvents = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();

            using (var virtualDevice = GetVirtualDevice())
            {
                var outputDevice = virtualDevice.OutputDevice;
                var inputDevice = virtualDevice.InputDevice;

                outputDevice.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputDevice.PrepareForEventsSending();

                string errorOnSend = null;
                outputDevice.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

                inputDevice.EventReceived += (_, e) => receivedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

                string errorOnReceive = null;
                inputDevice.ErrorOccurred += (_, e) => errorOnReceive = e.Exception.Message;

                setupInputDevice?.Invoke(inputDevice);

                inputDevice.StartEventsListening();
                outputDevice.PrepareForEventsSending();
                stopwatch.Start();

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;

                foreach (var midiEvent in midiEvents)
                {
                    outputDevice.SendEvent(midiEvent);
                }

                WaitOperations.Wait(() => receivedEvents.Count >= midiEvents.Length, timeout);

                SendReceiveUtilities.CheckTimestampedEvents(
                    sentEvents,
                    timestampedEvents,
                    timeout,
                    $"Sent events are invalid.");

                SendReceiveUtilities.CheckTimestampedEvents(
                    receivedEvents,
                    timestampedEvents,
                    timeout,
                    $"Received events are invalid.");

                checkAction?.Invoke(receivedEvents);
            }
        }

        #endregion
    }
}
