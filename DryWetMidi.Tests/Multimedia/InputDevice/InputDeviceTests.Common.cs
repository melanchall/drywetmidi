using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

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

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void GetInputDeviceByName(string deviceName)
        {
            Assert.IsNotNull(InputDevice.GetByName(deviceName), "There is no device.");
        }

        [Test]
        public void GetInputDeviceByIndex_Valid()
        {
            var devicesCount = InputDevice.GetDevicesCount();
            Assert.IsNotNull(InputDevice.GetByIndex(devicesCount / 2), "There is no device.");
        }

        [Test]
        public void GetInputDeviceByIndex_BelowZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => InputDevice.GetByIndex(-1), "Exception is not thrown.");
        }

        [Test]
        public void GetInputDeviceByIndex_BeyondDevicesCount()
        {
            var devicesCount = InputDevice.GetDevicesCount();
            Assert.Throws<ArgumentOutOfRangeException>(() => InputDevice.GetByIndex(devicesCount), "Exception is not thrown.");
        }

        [Test]
        public void GetAllInputDevices()
        {
            var inputDevices = InputDevice.GetAll();
            var inputDevicesCount = InputDevice.GetDevicesCount();
            Assert.AreEqual(inputDevicesCount, inputDevices.Count, "Input devices count is invalid.");
        }

        [Test]
        public void GetInputDevicesCount()
        {
            var inputDevicesCount = InputDevice.GetDevicesCount();
            Assert.GreaterOrEqual(
                inputDevicesCount,
                MidiDevicesNames.GetAllDevicesNames().Length,
                "Input devices count is invalid.");
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

                checkpoints.CheckCheckpointNotReached(InputDeviceCheckpointsNames.HandleFinalizerEntered);
                checkpoints.CheckCheckpointNotReached(InputDeviceCheckpointsNames.DeviceDisconnectedInHandleFinalizer);
                checkpoints.CheckCheckpointNotReached(InputDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);

                Assert.IsTrue(openDevice(checkpoints), $"Can't open device on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();

                checkpoints.CheckCheckpointReached(InputDeviceCheckpointsNames.HandleFinalizerEntered);
                checkpoints.CheckCheckpointReached(InputDeviceCheckpointsNames.DeviceDisconnectedInHandleFinalizer);
                checkpoints.CheckCheckpointReached(InputDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);
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
        public void InputDeviceToString_User()
        {
            var inputDevice = GetUserInputDevice();
            Assert.AreEqual("Input device", inputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        public void GetInputDeviceHashCode()
        {
            foreach (var inputDevice in InputDevice.GetAll())
            {
                Assert.DoesNotThrow(() => inputDevice.GetHashCode(), $"Failed to get hash code for [{inputDevice.Name}].");
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
                Assert.IsFalse(success, "Event received on just created device.");

                inputDevice.StartEventsListening();
                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 0, timeout);
                Assert.IsTrue(success, "Event was not received after first start.");
                Assert.AreEqual(1, receivedEventsCount, "Received events count is invalid after first start.");

                inputDevice.StopEventsListening();
                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 1, timeout);
                Assert.IsFalse(success, "Event received after first stop.");
                Assert.AreEqual(1, receivedEventsCount, "Received events count is invalid after first stop.");

                inputDevice.StartEventsListening();
                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 1, timeout);
                Assert.IsTrue(success, "Event was not received after second start.");
                Assert.AreEqual(2, receivedEventsCount, "Received events count is invalid after second start.");

                inputDevice.StopEventsListening();
                outputDevice.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 2, timeout);
                Assert.IsFalse(success, "Event received after second stop.");
                Assert.AreEqual(2, receivedEventsCount, "Received events count is invalid after second stop.");
            }
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
                inputDevice.TestCheckpoints = checkpoints;
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
                Assert.IsTrue(
                    areEventReceived,
                    $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}). Checkpoint's data: {GetCheckpointDataString(checkpointData)}.");

                MidiAsserts.AreEqual(
                    expectedEvents,
                    receivedEvents,
                    false,
                    "Received events are invalid.");

                if (checkCheckpoints)
                {
                    var expectedCheckpointData = packages.SelectMany(p => new object[] { null }.Concat(p.Packets.Select(pp => pp.Data))).ToArray();
                    if (expectedCheckpointData.Length != checkpointData.Count)
                        Assert.Fail($"Invalid checkpoint's data count: {GetCheckpointDataString(checkpointData)}.");

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
                inputDevice.TestCheckpoints = checkpoints;
                inputDevice.WaitForCompleteSysExEvent = waitForCompleteSysExEvent;

                inputDevice.EventReceived += (_, e) => receivedEvents.Add(e.Event);
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
                Assert.IsTrue(
                    areEventReceived,
                    $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}). Checkpoint's data: {GetCheckpointDataString(checkpointData)}.");

                MidiAsserts.AreEqual(
                    expectedEvents,
                    receivedEvents,
                    false,
                    "Received events are invalid.");

                if (checkCheckpoints)
                {
                    var expectedCheckpointData = packets.Select(pp => pp.Data).ToArray();
                    if (expectedCheckpointData.Length != checkpointData.Count)
                        Assert.Fail($"Invalid checkpoint's data count: {GetCheckpointDataString(checkpointData)}.");

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
            }

            WaitAfterReceiveData();
        }

        #endregion
    }
}
