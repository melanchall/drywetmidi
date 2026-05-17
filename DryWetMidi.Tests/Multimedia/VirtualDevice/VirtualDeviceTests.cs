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
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    [VirtualDeviceApiRequired]
    public sealed class VirtualDeviceTests
    {
        #region Fields

        private VirtualDevice _virtualDeviceForEventsSending;

        #endregion

        #region Setup/Cleanup

        [OneTimeSetUp]
        public void SetUp()
        {
            _virtualDeviceForEventsSending = GetVirtualDevice("DwmVirtualDevice");
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            _virtualDeviceForEventsSending.Dispose();
        }

        #endregion

        #region Test methods

        [Test]
        public void CantDisposeVirtualDeviceEndpoints()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                ClassicAssert.Throws<InvalidOperationException>(() => virtualDevice.InputEndpoint.Dispose(), "Dispose not failed for input endpoint.");
                ClassicAssert.Throws<InvalidOperationException>(() => virtualDevice.OutputEndpoint.Dispose(), "Dispose not failed for output endpoint.");
            }
        }

        [Test]
        public void CreateVirtualDevice([Values("My Virtual Device", "Mi Dispositivo Virtual", "我的虚拟设备")] string name)
        {
            using (var virtualDevice = GetVirtualDevice(name))
            {
                ClassicAssert.AreEqual(name, virtualDevice.Name, "Name is invalid.");

                ClassicAssert.IsNotNull(virtualDevice.InputEndpoint, "Input endpoint is null.");
                ClassicAssert.AreEqual(name, virtualDevice.InputEndpoint.Name, "Input endpoint name is null.");
                ClassicAssert.AreEqual("Input endpoint (endpoint of a virtual device)", virtualDevice.InputEndpoint.ToString(), "Input endpoint string representation is invalid.");

                ClassicAssert.IsNotNull(virtualDevice.OutputEndpoint, "Output endpoint is null.");
                ClassicAssert.AreEqual(name, virtualDevice.OutputEndpoint.Name, "Output endpoint name is null.");
                ClassicAssert.AreEqual("Output endpoint (endpoint of a virtual device)", virtualDevice.OutputEndpoint.ToString(), "Output endpoint string representation is invalid.");
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
            virtualDevice.InputEndpoint.TestCheckpoints = checkpoints;
            virtualDevice.OutputEndpoint.TestCheckpoints = checkpoints;

            var inputEndpointFound = WaitOperations.Wait(() => InputEndpoint.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsTrue(inputEndpointFound, $"Input endpoint is not found for [{timeout}].");

            var outputEndpointFound = WaitOperations.Wait(() => OutputEndpoint.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsTrue(outputEndpointFound, $"Output endpoint is not found for [{timeout}].");

            checkpoints.CheckCheckpointsAreNotReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            virtualDevice.Dispose();

            inputEndpointFound = WaitOperations.Wait(() => InputEndpoint.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsFalse(inputEndpointFound, $"Input endpoint is found after virtual device disposed after [{timeout}].");

            outputEndpointFound = WaitOperations.Wait(() => OutputEndpoint.GetAll().Any(d => d.Name == virtualDevice.Name), timeout);
            ClassicAssert.IsFalse(outputEndpointFound, $"Output endpoint is found after virtual device disposed after [{timeout}].");

            checkpoints.CheckCheckpointsReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            ClassicAssert.Throws<ObjectDisposedException>(
                () => { var _ = virtualDevice.InputEndpoint; },
                "Input endpoint of disposed virtual device can be accessed.");
            ClassicAssert.Throws<ObjectDisposedException>(
                () => { var _ = virtualDevice.OutputEndpoint; },
                "Output endpoint of disposed virtual device can be accessed.");
        }

        [Test]
        public void VirtualDeviceIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, string> createVirtualDevice = testCheckpoints =>
            {
                var virtualDevice = GetVirtualDevice();
                virtualDevice.TestCheckpoints = testCheckpoints;
                virtualDevice.InputEndpoint.TestCheckpoints = testCheckpoints;
                virtualDevice.OutputEndpoint.TestCheckpoints = testCheckpoints;

                return virtualDevice.Name;
            };

            var checkpoints = new TestCheckpoints();

            var deviceName = createVirtualDevice(checkpoints);

            checkpoints.CheckCheckpointsAreNotReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var timeout = TimeSpan.FromSeconds(5);

            checkpoints.CheckCheckpointsReached(
                VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle,
                OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

            var inputEndpointFound = WaitOperations.Wait(() => InputEndpoint.GetAll().Any(d => d.Name == deviceName), timeout);
            ClassicAssert.IsFalse(inputEndpointFound, $"Input endpoint is found after virtual device disposed after [{timeout}].");

            var outputEndpointFound = WaitOperations.Wait(() => OutputEndpoint.GetAll().Any(d => d.Name == deviceName), timeout);
            ClassicAssert.IsFalse(outputEndpointFound, $"Output endpoint is found after virtual device disposed after [{timeout}].");
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
        public void FindVirtualDeviceEndpoints()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var deviceName = virtualDevice.Name;

                var timeout = TimeSpan.FromSeconds(5);
                var endpointsFound = WaitOperations.Wait(() => InputEndpoint.GetAll().Any(d => d.Name == deviceName) && OutputEndpoint.GetAll().Any(d => d.Name == deviceName), timeout);

                ClassicAssert.IsTrue(endpointsFound, "Endpoints were not found.");
            }
        }

        [Test]
        public void CheckVirtualDeviceEndpointsEquality_SameEndpoints()
        {
            using (var virtualDevice = GetVirtualDevice())
            using (var inputEndpoint = InputEndpoint.GetByName(virtualDevice.Name))
            using (var outputEndpoint = OutputEndpoint.GetByName(virtualDevice.Name))
            {
                ClassicAssert.AreEqual(virtualDevice.InputEndpoint, inputEndpoint, "Input endpoint is not equal to virtual input endpoint.");
                ClassicAssert.AreEqual(virtualDevice.OutputEndpoint, outputEndpoint, "Output endpoint is not equal to virtual output endpoint.");
            }
        }

        [Test]
        public void CheckVirtualDeviceEndpointsEquality_DifferentEndpoints()
        {
            using (var virtualDevice = GetVirtualDevice())
            using (var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A))
            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.B))
            {
                ClassicAssert.AreNotEqual(virtualDevice.InputEndpoint, inputEndpoint, "Input endpoint is equal to virtual input endpoint.");
                ClassicAssert.AreNotEqual(virtualDevice.OutputEndpoint, outputEndpoint, "Output endpoint is equal to virtual output endpoint.");
            }
        }

        [Test]
        public void DisableEnableVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                ClassicAssert.IsTrue(virtualDevice.IsEnabled, "Device is not enabled initially.");

                var inputEndpoint = virtualDevice.InputEndpoint;
                var outputEndpoint = virtualDevice.OutputEndpoint;

                var receivedEvents = new List<MidiEvent>();

                inputEndpoint.StartEventsListening();
                inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(e.Event);

                outputEndpoint.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => receivedEvents.Count == 1 && receivedEvents.Last() is NoteOnEvent, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received.");

                virtualDevice.IsEnabled = false;
                ClassicAssert.IsFalse(virtualDevice.IsEnabled, "Device is enabled after disabling.");

                outputEndpoint.SendEvent(new NoteOffEvent());
                eventReceived = WaitOperations.Wait(() => receivedEvents.Count > 1 && receivedEvents.Last() is NoteOffEvent, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is received after device disabled.");

                virtualDevice.IsEnabled = true;
                ClassicAssert.IsTrue(virtualDevice.IsEnabled, "Device is disabled after enabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEvents.Count > 1 && receivedEvents.Last() is NoteOnEvent, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received after enabling again.");
            }
        }

        [Test]
        public void DisableEnableInputEndpointOfVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var inputEndpoint = virtualDevice.InputEndpoint;
                var outputEndpoint = virtualDevice.OutputEndpoint;

                ClassicAssert.IsTrue(inputEndpoint.IsEnabled, "Device is not enabled initially.");

                var receivedEventsCount = 0;

                inputEndpoint.StartEventsListening();
                inputEndpoint.EventReceived += (_, __) => receivedEventsCount++;

                outputEndpoint.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => receivedEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received.");

                inputEndpoint.IsEnabled = false;
                ClassicAssert.IsFalse(inputEndpoint.IsEnabled, "Input endpoint is enabled after disabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is received after input endpoint disabled.");

                inputEndpoint.IsEnabled = true;
                ClassicAssert.IsTrue(inputEndpoint.IsEnabled, "Input endpoint is disabled after enabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received after enabling again.");
            }
        }

        [Test]
        public void DisableEnableOutputEndpointOfVirtualDevice()
        {
            using (var virtualDevice = GetVirtualDevice())
            {
                var inputEndpoint = virtualDevice.InputEndpoint;
                var outputEndpoint = virtualDevice.OutputEndpoint;

                ClassicAssert.IsTrue(outputEndpoint.IsEnabled, "Output endpoint is not enabled initially.");

                var sentEventsCount = 0;

                outputEndpoint.EventSent += (_, __) => sentEventsCount++;

                outputEndpoint.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => sentEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not sent.");

                outputEndpoint.IsEnabled = false;
                ClassicAssert.IsFalse(outputEndpoint.IsEnabled, "Output endpoint is enabled after disabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is sent after output endpoint disabled.");

                outputEndpoint.IsEnabled = true;
                ClassicAssert.IsTrue(outputEndpoint.IsEnabled, "Output endpoint is disabled after enabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not sent after enabling again.");
            }
        }

        [Test]
        public void AccessEndpointsAfterSomeDeviceRemoval([Values(1, 5)] int previousCount)
        {
            var previousNames = Enumerable
                .Range(0, previousCount)
                .Select(i => Guid.NewGuid().ToString())
                .ToArray();

            var virtualDevices = new VirtualDevice[previousCount];

            for (var i = 0; i < previousCount; i++)
            {
                virtualDevices[i] = VirtualDevice.Create(previousNames[i]);
            }

            Thread.Sleep(2000);

            const string lastVirtualDeviceName = "Last virtual device";

            using (var lastVirtualDevice = VirtualDevice.Create(lastVirtualDeviceName))
            {
                var inputEndpoint = lastVirtualDevice.InputEndpoint;
                var outputEndpoint = lastVirtualDevice.OutputEndpoint;

                for (var i = 0; i < previousCount; i++)
                {
                    virtualDevices[i]?.Dispose();
                }

                Thread.Sleep(2000);

                Assert.DoesNotThrow(
                    () => inputEndpoint.StartEventsListening(),
                    "Exception has been thrown on input endpoint.");

                Assert.DoesNotThrow(
                    () => outputEndpoint.SendEvent(new NoteOnEvent()),
                    "Exception has been thrown on output endpoint.");
            }
        }

        #endregion

        #region Private methods

        private VirtualDevice GetVirtualDevice(string name = null)
        {
            var deviceName = name ?? Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);
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
            Action<InputEndpoint> setupInputEndpoint = null)
        {
            var stopwatch = new Stopwatch();

            var timestampedEvents = midiEvents
                .Select(e => new TimestampedEvent(e, TimeSpan.Zero))
                .ToArray();

            var receivedEvents = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();

            var virtualDevice = _virtualDeviceForEventsSending;

            var outputEndpoint = virtualDevice.OutputEndpoint;
            var inputEndpoint = virtualDevice.InputEndpoint;

            outputEndpoint.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
            outputEndpoint.PrepareForEventsSending();

            string errorOnSend = null;
            outputEndpoint.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

            inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

            string errorOnReceive = null;
            inputEndpoint.ErrorOccurred += (_, e) => errorOnReceive = e.Exception.Message;

            setupInputEndpoint?.Invoke(inputEndpoint);

            inputEndpoint.StartEventsListening();
            outputEndpoint.PrepareForEventsSending();
            stopwatch.Start();

            var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;

            foreach (var midiEvent in midiEvents)
            {
                outputEndpoint.SendEvent(midiEvent);
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

        #endregion
    }
}
