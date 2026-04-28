using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        private sealed class DataPacket
        {
            public DataPacket(params byte[] data)
            {
                Data = data;
            }

            public byte[] Data { get; }
        }

        private sealed class DataPackage
        {
            public DataPackage(params DataPacket[] packets)
            {
                Packets = packets;
            }

            public DataPacket[] Packets { get; }
        }

        #endregion

        #region Constants

        private const int RetriesNumber = 5;

        #endregion

        #region Test methods

        [WinOnly]
        [ParentDeviceApiRequired]
        [Test]
        public void CheckInputDeviceParentDeviceInfo([Values(MidiDevicesNames.DeviceA, MidiDevicesNames.DeviceB, MidiDevicesNames.DeviceC)] string deviceName)
        {
            var inputDevice = InputDevice.GetByName(deviceName);

            var parentDevice = inputDevice.ParentDevice;
            Console.WriteLine($"Parent device for [{deviceName}]: [{parentDevice}]");

            ClassicAssert.IsNotNull(parentDevice, "There is no parent device.");
            ClassicAssert.IsNotNull(parentDevice.Id, "Parent device ID is null.");
            ClassicAssert.IsNotNull(parentDevice.Name, "Parent device name is null.");
            ClassicAssert.IsNotEmpty(parentDevice.Name, "Parent device name is empty.");
            ClassicAssert.IsNotNull(parentDevice.Manufacturer, "Parent device manufacturer is null.");
            ClassicAssert.IsNotEmpty(parentDevice.Manufacturer, "Parent device manufacturer is empty.");
        }

        [MultimediaTestRetry]
        [MultiClientDeviceAccessSupportRequired]
        [Test]
        public void CheckInputDeviceMultiClientAccess()
        {
            using (var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                var receivedEventsCount = 0;

                inputDevice1.EventReceived += (_, e) => receivedEventsCount++;
                inputDevice1.StartEventsListening();

                inputDevice2.EventReceived += (_, e) => receivedEventsCount++;
                inputDevice2.StartEventsListening();

                outputDevice.PrepareForEventsSending();
                outputDevice.SendEvent(new NoteOnEvent());

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var success = WaitOperations.Wait(() => receivedEventsCount == 2, timeout);
                ClassicAssert.IsTrue(success, "Event is not received by both devices.");
            }
        }

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void GetInputDeviceByName(string deviceName)
        {
            ClassicAssert.IsNotNull(InputDevice.GetByName(deviceName), "There is no device.");
        }

        [Test]
        public void GetAllInputDevices()
        {
            var inputDevices = InputDevice.GetAll();
            var inputDevicesCount = InputDevice.GetDevicesCount();
            ClassicAssert.AreEqual(inputDevicesCount, inputDevices.Count, "Input devices count is invalid.");
        }

        [Test]
        public void GetInputDevicesCount()
        {
            var inputDevicesCount = InputDevice.GetDevicesCount();
            ClassicAssert.GreaterOrEqual(
                inputDevicesCount,
                MidiDevicesNames.GetAllDevicesNames().Length,
                "Input devices count is invalid.");
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckMidiTimeCodeEventReceiving()
        {
            MidiTimeCode midiTimeCodeReceived = null;

            var eventsToSend = new[]
            {
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)100), TimeSpan.FromMilliseconds(200)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.FramesLsb, (FourBitNumber)1), TimeSpan.FromMilliseconds(400)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)70), TimeSpan.FromMilliseconds(700)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.FramesMsb, (FourBitNumber)1), TimeSpan.FromMilliseconds(900)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)7), TimeSpan.FromMilliseconds(1000)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursMsbAndTimeCodeType, (FourBitNumber)7), TimeSpan.FromMilliseconds(1200)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)80), TimeSpan.FromMilliseconds(1700)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.MinutesLsb, (FourBitNumber)10), TimeSpan.FromMilliseconds(2000)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)10), TimeSpan.FromMilliseconds(2400)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)15), TimeSpan.FromMilliseconds(2900)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.MinutesMsb, (FourBitNumber)2), TimeSpan.FromMilliseconds(3100)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsLsb, (FourBitNumber)10), TimeSpan.FromMilliseconds(3200)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromMilliseconds(3600)),
                new TimestampedEvent(new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsMsb, (FourBitNumber)1), TimeSpan.FromMilliseconds(4300))
            };

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice.MidiTimeCodeReceived += (_, e) => midiTimeCodeReceived = new MidiTimeCode(e.Format, e.Hours, e.Minutes, e.Seconds, e.Frames);

                outputDevice.PrepareForEventsSending();
                inputDevice.StartEventsListening();

                SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);

                var timeout = eventsToSend.Last().Time + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var isMidiTimeCodeReceived = WaitOperations.Wait(() => midiTimeCodeReceived != null, timeout);
                ClassicAssert.IsTrue(isMidiTimeCodeReceived, $"MIDI time code received for timeout {timeout}.");

                inputDevice.StopEventsListening();
            }

            ClassicAssert.AreEqual(MidiTimeCodeType.Thirty, midiTimeCodeReceived.Format, "Format is invalid.");
            ClassicAssert.AreEqual(23, midiTimeCodeReceived.Hours, "Hours number is invalid.");
            ClassicAssert.AreEqual(42, midiTimeCodeReceived.Minutes, "Minutes number is invalid.");
            ClassicAssert.AreEqual(26, midiTimeCodeReceived.Seconds, "Seconds number is invalid.");
            ClassicAssert.AreEqual(17, midiTimeCodeReceived.Frames, "Frames number is invalid.");
        }

#if TEST
        [Test]
        public void InputDeviceIsReleasedByDispose()
        {
            for (var i = 0; i < 10; i++)
            {
                var checkpoints = new TestCheckpoints();

                var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                inputDevice.TestCheckpoints = checkpoints;

                ClassicAssert.DoesNotThrow(() => inputDevice.StartEventsListening());

                checkpoints.CheckCheckpointsAreNotReached(
                    InputDeviceCheckpointsNames.ReleaseHandleEntered,
                    InputDeviceCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                inputDevice.Dispose();

                checkpoints.CheckCheckpointsReached(
                    InputDeviceCheckpointsNames.ReleaseHandleEntered,
                    InputDeviceCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }

        [Test]
        public void InputDeviceIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, bool> openDevice = testCheckpoints =>
            {
                var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                inputDevice.TestCheckpoints = testCheckpoints;

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
                var checkpoints = new TestCheckpoints();

                checkpoints.CheckCheckpointsAreNotReached(
                    InputDeviceCheckpointsNames.ReleaseHandleEntered,
                    InputDeviceCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                ClassicAssert.IsTrue(openDevice(checkpoints), $"Can't open device on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                checkpoints.CheckCheckpointsReached(
                    InputDeviceCheckpointsNames.ReleaseHandleEntered,
                    InputDeviceCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }
#endif

        [Test]
        public void DisableEnableInputDevice()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                ClassicAssert.IsTrue(inputDevice.IsEnabled, "Device is not enabled initially.");

                var receivedEventsCount = 0;
                inputDevice.EventReceived += (_, __) => receivedEventsCount++;
                
                outputDevice.PrepareForEventsSending();
                inputDevice.StartEventsListening();

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
        public void InputDeviceToString_User()
        {
            var inputDevice = GetUserInputDevice();
            ClassicAssert.AreEqual("Input device", inputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        public void GetInputDeviceHashCode()
        {
            foreach (var inputDevice in InputDevice.GetAll())
            {
                ClassicAssert.DoesNotThrow(() => inputDevice.GetHashCode(), $"Failed to get hash code for [{inputDevice.Name}].");
            }
        }

        [Test]
        public void StartStopEventsListening()
        {
            var receivedEventsCount = 0;
            var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay + SendReceiveUtilities.MaximumEventSendReceiveDelay;

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice.EventReceived += (_, __) => receivedEventsCount++;

                outputDevice.SendEvent(new NoteOnEvent());
                var success = WaitOperations.Wait(() => receivedEventsCount > 0, timeout);
                ClassicAssert.IsFalse(success, "Event received on just created device.");

                outputDevice.PrepareForEventsSending();
                inputDevice.StartEventsListening();

                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 0, timeout);
                ClassicAssert.IsTrue(success, "Event was not received after first start.");
                ClassicAssert.AreEqual(1, receivedEventsCount, "Received events count is invalid after first start.");

                inputDevice.StopEventsListening();

                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 1, timeout);
                ClassicAssert.IsFalse(success, "Event received after first stop.");
                ClassicAssert.AreEqual(1, receivedEventsCount, "Received events count is invalid after first stop.");

                inputDevice.StartEventsListening();
                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 1, timeout);
                ClassicAssert.IsTrue(success, "Event was not received after second start.");
                ClassicAssert.AreEqual(2, receivedEventsCount, "Received events count is invalid after second start.");

                inputDevice.StopEventsListening();
                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 2, timeout);
                ClassicAssert.IsFalse(success, "Event received after second stop.");
                ClassicAssert.AreEqual(2, receivedEventsCount, "Received events count is invalid after second stop.");
            }
        }

        [Test]
        public void HandleSilentNoteOn([Values] SilentNoteOnPolicy silentNoteOnPolicy)
        {
            var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                MidiEvent midiEvent = null;

                inputDevice.SilentNoteOnPolicy = silentNoteOnPolicy;
                inputDevice.EventReceived += (_, e) => midiEvent = e.Event;
                inputDevice.StartEventsListening();

                outputDevice.PrepareForEventsSending();
                outputDevice.SendEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MinValue));
                var success = WaitOperations.Wait(() => midiEvent != null, timeout);
                ClassicAssert.IsTrue(success, "Event is not received.");

                var expectedEvent = silentNoteOnPolicy == SilentNoteOnPolicy.NoteOn
                    ? (MidiEvent)new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MinValue)
                    : new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue);

                MidiAsserts.AreEqual(
                    expectedEvent,
                    midiEvent,
                    false,
                    "Received event is invalid.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBufferSize_Invalid([Values(0, 16, 31)] int bufferSize)
        {
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                ClassicAssert.Throws<ArgumentOutOfRangeException>(
                    () => inputDevice.SysExBufferSize = bufferSize,
                    "There is no exception.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBufferSize_AfterStartEventListening()
        {
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice.StartEventsListening();
                ClassicAssert.Throws<InvalidOperationException>(
                    () => inputDevice.SysExBufferSize = 128,
                    "There is no exception.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBuffersCount_Invalid([Values(0, 1)] int buffersCount)
        {
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                ClassicAssert.Throws<ArgumentOutOfRangeException>(
                    () => inputDevice.SysExBuffersCount = buffersCount,
                    "There is no exception.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBuffersCount_AfterStartEventListening()
        {
            using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice.StartEventsListening();
                ClassicAssert.Throws<InvalidOperationException>(
                    () => inputDevice.SysExBuffersCount = 128,
                    "There is no exception.");
            }
        }

        [VirtualDeviceApiRequired]
        [Test]
        public void AccessInputDeviceAfterSomeDeviceRemoval([Values(1, 5)] int previousCount)
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
            using (var inputDevice = InputDevice.GetByName(lastVirtualDeviceName))
            {
                for (var i = 0; i < previousCount; i++)
                {
                    virtualDevices[i]?.Dispose();
                }

                Thread.Sleep(2000);

                Assert.DoesNotThrow(
                    () => inputDevice.StartEventsListening(),
                    "Exception has been thrown.");
            }
        }

        [Test]
        public void CheckInputDevicesEquality_ViaEquals_SameDevices()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            ClassicAssert.AreEqual(inputDevice1, inputDevice2, "Devices are not equal.");
        }

        [Test]
        public void CheckInputDevicesEquality_ViaEquals_DifferentDevices()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            ClassicAssert.AreNotEqual(inputDevice1, inputDevice2, "Devices are equal.");
        }

        [Test]
        public void CheckInputDevicesEquality_ViaOperator_SameDevices()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            ClassicAssert.IsTrue(inputDevice1 == inputDevice2, "Devices are not equal via equality.");
            ClassicAssert.IsFalse(inputDevice1 != inputDevice2, "Devices are not equal via inequality.");
        }

        [Test]
        public void CheckInputDevicesEquality_ViaOperator_DifferentDevices()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            ClassicAssert.IsFalse(inputDevice1 == inputDevice2, "Devices are equal via equality.");
            ClassicAssert.IsTrue(inputDevice1 != inputDevice2, "Devices are equal via inequality.");
        }

        [Test]
        public void FindOutputDeviceInDictionary()
        {
            var label = "X";
            var dictionary = new Dictionary<MidiDevice, string>
            {
                [OutputDevice.GetByName(MidiDevicesNames.DeviceA)] = label
            };

            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsTrue(dictionary.TryGetValue(outputDevice, out var value), "Failed to find device in dictionary.");
            ClassicAssert.AreEqual(label, value, "Device label is invalid.");
        }

        #endregion

        #region Private methods

        private static InputDevice GetUserInputDevice() =>
            InputDevice.GetByName(MidiDevicesNames.DeviceA);

        private static void WaitAfterReceiveData() =>
            WaitOperations.Wait(2000);

        private static string GetCheckpointDataString(ICollection<object> data) =>
            string.Join(
                "; ",
                data?.Select(d => d == null ? "null" : string.Join(" ", ((byte[])d).Select(b => Convert.ToString(b, 16).PadLeft(2, '0').ToUpper()))) ??
                Array.Empty<string>());

        private void ReceiveData_Mac(
            DataPackage[] packages,
            ICollection<MidiEvent> expectedEvents,
            bool checkCheckpoints = true,
            bool waitForCompleteSysExEvent = true)
        {
            var deviceName = MidiDevicesNames.DeviceA;

            var receivedEvents = new List<MidiEvent>(expectedEvents.Count);
            var checkpoints = new TestCheckpoints();

            using (var dataSender = new DataSender(deviceName))
            using (var inputDevice = InputDevice.GetByName(deviceName))
            {
#if TEST
                inputDevice.TestCheckpoints = checkpoints;
#endif
                inputDevice.WaitForCompleteSysExEvent = waitForCompleteSysExEvent;

                inputDevice.EventReceived += (_, e) => receivedEvents.Add(e.Event);
                inputDevice.StartEventsListening();

                foreach (var package in packages)
                {
                    var data = package.Packets.SelectMany(p => p.Data).ToArray();

                    var index = 0;
                    var indices = new List<int>();

                    foreach (var packet in package.Packets)
                    {
                        indices.Add(index);
                        index += packet.Data.Length;
                    }

                    dataSender.SendData(data, data.Length, indices.ToArray(), indices.Count);
                    WaitOperations.Wait(5);
                }

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay + TimeSpan.FromMilliseconds(30);
                var areEventReceived = WaitOperations.Wait(
                    () => receivedEvents.Count >= expectedEvents.Count,
                    timeout);

                var checkpointData = checkpoints.GetCheckpointDataList(InputDeviceCheckpointsNames.MessageDataReceived);
                ClassicAssert.IsTrue(
                    areEventReceived,
                    $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}). Checkpoint's data: {GetCheckpointDataString(checkpointData)}.");

                MidiAsserts.AreEqual(
                    expectedEvents,
                    receivedEvents,
                    false,
                    "Received events are invalid.");

#if TEST
                if (checkCheckpoints)
                {
                    var expectedCheckpointData = packages.SelectMany(p => new object[] { null }.Concat(p.Packets.Select(pp => pp.Data))).ToArray();
                    if (expectedCheckpointData.Length != checkpointData.Count)
                        ClassicAssert.Fail($"Invalid checkpoint's data count: {GetCheckpointDataString(checkpointData)}.");

                    for (var i = 0; i < expectedCheckpointData.Length; i++)
                    {
                        var expected = expectedCheckpointData.ElementAt(i);
                        var actual = checkpointData.ElementAt(i);

                        if (ReferenceEquals(expected, actual))
                            continue;

                        var expectedBytes = (byte[])expected;
                        var actualBytes = (byte[])actual;
                        CollectionAssert.AreEqual(expectedBytes, actualBytes, $"Bytes of data record {i} are invalid.");
                    }
                }
#endif
            }

            WaitAfterReceiveData();
        }

        private void ReceiveData_Win(
            DataPacket[] packets,
            ICollection<MidiEvent> expectedEvents,
            bool checkCheckpoints = true,
            bool waitForCompleteSysExEvent = true)
        {
            var deviceName = MidiDevicesNames.DeviceA;

            var receivedEvents = new List<MidiEvent>(expectedEvents.Count);
            var checkpoints = new TestCheckpoints();

            using (var outputDevice = OutputDevice.GetByName(deviceName))
            using (var inputDevice = InputDevice.GetByName(deviceName))
            {
#if TEST
                inputDevice.TestCheckpoints = checkpoints;
#endif
                inputDevice.WaitForCompleteSysExEvent = waitForCompleteSysExEvent;

                inputDevice.EventReceived += (_, e) => receivedEvents.Add(e.Event);

                outputDevice.PrepareForEventsSending();
                inputDevice.StartEventsListening();

                foreach (var packet in packets)
                {
                    var data = packet.Data;
                    outputDevice.SendData_Win(data);
                    WaitOperations.Wait(5);
                }

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay + TimeSpan.FromMilliseconds(30);
                var areEventReceived = WaitOperations.Wait(
                    () => receivedEvents.Count >= expectedEvents.Count,
                    timeout);

                var checkpointData = checkpoints.GetCheckpointDataList(InputDeviceCheckpointsNames.MessageDataReceived);
                ClassicAssert.IsTrue(
                    areEventReceived,
                    $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}). Checkpoint's data: {GetCheckpointDataString(checkpointData)}.");

                MidiAsserts.AreEqual(
                    expectedEvents,
                    receivedEvents,
                    false,
                    "Received events are invalid.");

#if TEST
                if (checkCheckpoints)
                {
                    var expectedCheckpointData = packets.Select(pp => pp.Data).ToArray();
                    if (expectedCheckpointData.Length != checkpointData.Count)
                        ClassicAssert.Fail($"Invalid checkpoint's data count: {GetCheckpointDataString(checkpointData)}.");

                    for (var i = 0; i < expectedCheckpointData.Length; i++)
                    {
                        var expected = expectedCheckpointData.ElementAt(i);
                        var actual = checkpointData.ElementAt(i);

                        if (ReferenceEquals(expected, actual))
                            continue;

                        var expectedBytes = (byte[])expected;
                        var actualBytes = (byte[])actual;
                        CollectionAssert.AreEqual(expectedBytes, actualBytes, $"Bytes of data record {i} are invalid.");
                    }
                }
#endif
            }

            WaitAfterReceiveData();
        }

        #endregion
    }
}
