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
    public sealed partial class InputEndpointTests
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

        #region Test methods

        [WinOnly]
        [DeviceInformationApiRequired]
        [Test]
        public void CheckInputEndpointDeviceInformation([Values(MidiEndpoints.A, MidiEndpoints.B, MidiEndpoints.C)] string endpointName)
        {
            var inputEndpoint = DevicesUtilities.GetInputEndpoint(endpointName);

            var deviceInformation = inputEndpoint.GetDeviceInformation();
            Console.WriteLine($"Device information for [{endpointName}]: [{deviceInformation}]");

            ClassicAssert.IsNotNull(deviceInformation, "There is no device information.");
            ClassicAssert.IsNotNull(deviceInformation.Id, "Device ID is null.");
            ClassicAssert.IsNotNull(deviceInformation.Name, "Device name is null.");
            ClassicAssert.IsNotEmpty(deviceInformation.Name, "Device name is empty.");
            ClassicAssert.IsNotNull(deviceInformation.Manufacturer, "Device manufacturer is null.");
            ClassicAssert.IsNotEmpty(deviceInformation.Manufacturer, "Device manufacturer is empty.");
        }

        [MultimediaTestRetry]
        [MultiClientEndpointsAccessSupportRequired]
        [Test]
        public void CheckInputEndpointMultiClientAccess()
        {
            using (var inputEndpoint1 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            using (var inputEndpoint2 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A))
            {
                var receivedEventsCount = 0;

                inputEndpoint1.EventReceived += (_, e) => receivedEventsCount++;
                inputEndpoint1.StartEventsListening();

                inputEndpoint2.EventReceived += (_, e) => receivedEventsCount++;
                inputEndpoint2.StartEventsListening();

                outputEndpoint.PrepareForEventsSending();
                outputEndpoint.SendEvent(new NoteOnEvent());

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var success = WaitOperations.Wait(() => receivedEventsCount == 2, timeout);
                ClassicAssert.IsTrue(success, "Event is not received by both devices.");
            }
        }

        [TestCase(MidiEndpoints.A)]
        [TestCase(MidiEndpoints.B)]
        public void GetInputEndpointByName(string endpointName)
        {
            ClassicAssert.IsNotNull(DevicesUtilities.GetInputEndpoint(endpointName), "There is no endpoint.");
        }

        [Test]
        public void GetAllInputEndpoints()
        {
            WaitOperations.Wait(() => InputEndpoint.GetEndpointsCount() == InputEndpoint.GetAll().Count, TimeSpan.FromSeconds(5));
            ClassicAssert.AreEqual(InputEndpoint.GetEndpointsCount(), InputEndpoint.GetAll().Count, "Input endpoints count is invalid.");
        }

        [Test]
        public void GetInputEndpointsCount()
        {
            var inputEndpointsCount = InputEndpoint.GetEndpointsCount();
            ClassicAssert.GreaterOrEqual(
                inputEndpointsCount,
                MidiEndpoints.GetAllEndpointsNames().Length,
                "Input endpoints count is invalid.");
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

            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                inputEndpoint.MidiTimeCodeReceived += (_, e) => midiTimeCodeReceived = new MidiTimeCode(e.Format, e.Hours, e.Minutes, e.Seconds, e.Frames);

                outputEndpoint.PrepareForEventsSending();
                inputEndpoint.StartEventsListening();

                SendReceiveUtilities.SendEvents(eventsToSend, outputEndpoint);

                var timeout = eventsToSend.Last().Time + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var isMidiTimeCodeReceived = WaitOperations.Wait(() => midiTimeCodeReceived != null, timeout);
                ClassicAssert.IsTrue(isMidiTimeCodeReceived, $"MIDI time code received for timeout {timeout}.");

                inputEndpoint.StopEventsListening();
            }

            ClassicAssert.AreEqual(MidiTimeCodeType.Thirty, midiTimeCodeReceived.Format, "Format is invalid.");
            ClassicAssert.AreEqual(23, midiTimeCodeReceived.Hours, "Hours number is invalid.");
            ClassicAssert.AreEqual(42, midiTimeCodeReceived.Minutes, "Minutes number is invalid.");
            ClassicAssert.AreEqual(26, midiTimeCodeReceived.Seconds, "Seconds number is invalid.");
            ClassicAssert.AreEqual(17, midiTimeCodeReceived.Frames, "Frames number is invalid.");
        }

#if TEST
        [Test]
        public void InputEndpointIsReleasedByDispose()
        {
            for (var i = 0; i < 10; i++)
            {
                var checkpoints = new TestCheckpoints();

                var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
                inputEndpoint.TestCheckpoints = checkpoints;

                ClassicAssert.DoesNotThrow(() => inputEndpoint.StartEventsListening());

                checkpoints.CheckCheckpointsAreNotReached(
                    InputEndpointCheckpointsNames.ReleaseHandleEntered,
                    InputEndpointCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                inputEndpoint.Dispose();

                checkpoints.CheckCheckpointsReached(
                    InputEndpointCheckpointsNames.ReleaseHandleEntered,
                    InputEndpointCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }

        [Test]
        public void InputEndpointIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, bool> openEndpoint = testCheckpoints =>
            {
                var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
                inputEndpoint.TestCheckpoints = testCheckpoints;

                try
                {
                    inputEndpoint.StartEventsListening();
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
                    InputEndpointCheckpointsNames.ReleaseHandleEntered,
                    InputEndpointCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                ClassicAssert.IsTrue(openEndpoint(checkpoints), $"Can't open endpoint on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                checkpoints.CheckCheckpointsReached(
                    InputEndpointCheckpointsNames.ReleaseHandleEntered,
                    InputEndpointCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    InputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    InputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }
#endif

        [Test]
        public void DisableEnableInputEndpoint()
        {
            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                ClassicAssert.IsTrue(inputEndpoint.IsEnabled, "Endpoint is not enabled initially.");

                var receivedEventsCount = 0;
                inputEndpoint.EventReceived += (_, __) => receivedEventsCount++;
                
                outputEndpoint.PrepareForEventsSending();
                inputEndpoint.StartEventsListening();

                outputEndpoint.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => receivedEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received.");

                inputEndpoint.IsEnabled = false;
                ClassicAssert.IsFalse(inputEndpoint.IsEnabled, "Endpoint is enabled after disabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is received after endpoint disabled.");

                inputEndpoint.IsEnabled = true;
                ClassicAssert.IsTrue(inputEndpoint.IsEnabled, "Endpoint is disabled after enabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => receivedEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not received after enabling again.");
            }
        }

        [Test]
        public void InputEndpointToString_User()
        {
            var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
            ClassicAssert.AreEqual("Input endpoint", inputEndpoint.ToString(), "Endpoint string representation is invalid.");
        }

        [Test]
        public void GetInputEndpointHashCode()
        {
            foreach (var inputEndpoint in InputEndpoint.GetAll())
            {
                ClassicAssert.DoesNotThrow(() => inputEndpoint.GetHashCode(), $"Failed to get hash code for [{inputEndpoint.Name}].");
            }
        }

        [Test]
        public void StartStopEventsListening()
        {
            var receivedEventsCount = 0;
            var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay + SendReceiveUtilities.MaximumEventSendReceiveDelay;

            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                inputEndpoint.EventReceived += (_, __) => receivedEventsCount++;

                outputEndpoint.SendEvent(new NoteOnEvent());
                var success = WaitOperations.Wait(() => receivedEventsCount > 0, timeout);
                ClassicAssert.IsFalse(success, "Event received on just created endpoint.");

                outputEndpoint.PrepareForEventsSending();
                inputEndpoint.StartEventsListening();

                outputEndpoint.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 0, timeout);
                ClassicAssert.IsTrue(success, "Event was not received after first start.");
                ClassicAssert.AreEqual(1, receivedEventsCount, "Received events count is invalid after first start.");

                inputEndpoint.StopEventsListening();

                outputEndpoint.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 1, timeout);
                ClassicAssert.IsFalse(success, "Event received after first stop.");
                ClassicAssert.AreEqual(1, receivedEventsCount, "Received events count is invalid after first stop.");

                inputEndpoint.StartEventsListening();
                outputEndpoint.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 1, timeout);
                ClassicAssert.IsTrue(success, "Event was not received after second start.");
                ClassicAssert.AreEqual(2, receivedEventsCount, "Received events count is invalid after second start.");

                inputEndpoint.StopEventsListening();
                outputEndpoint.SendEvent(new NoteOnEvent());
                success = WaitOperations.Wait(() => receivedEventsCount > 2, timeout);
                ClassicAssert.IsFalse(success, "Event received after second stop.");
                ClassicAssert.AreEqual(2, receivedEventsCount, "Received events count is invalid after second stop.");
            }
        }

        [Test]
        public void HandleSilentNoteOn([Values] SilentNoteOnPolicy silentNoteOnPolicy)
        {
            var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;

            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                MidiEvent midiEvent = null;

                inputEndpoint.SilentNoteOnPolicy = silentNoteOnPolicy;
                inputEndpoint.EventReceived += (_, e) => midiEvent = e.Event;
                inputEndpoint.StartEventsListening();

                outputEndpoint.PrepareForEventsSending();
                outputEndpoint.SendEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MinValue));
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
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                ClassicAssert.Throws<ArgumentOutOfRangeException>(
                    () => inputEndpoint.SysExBufferSize = bufferSize,
                    "There is no exception.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBufferSize_AfterStartEventListening()
        {
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                inputEndpoint.StartEventsListening();
                ClassicAssert.Throws<InvalidOperationException>(
                    () => inputEndpoint.SysExBufferSize = 128,
                    "There is no exception.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBuffersCount_Invalid([Values(0, 1)] int buffersCount)
        {
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                ClassicAssert.Throws<ArgumentOutOfRangeException>(
                    () => inputEndpoint.SysExBuffersCount = buffersCount,
                    "There is no exception.");
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SysExBuffersCount_AfterStartEventListening()
        {
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A))
            {
                inputEndpoint.StartEventsListening();
                ClassicAssert.Throws<InvalidOperationException>(
                    () => inputEndpoint.SysExBuffersCount = 128,
                    "There is no exception.");
            }
        }

        [VirtualDeviceApiRequired]
        [Test]
        public void AccessInputEndpointAfterSomeDeviceRemoval([Values(1, 5)] int previousCount)
        {
            var previousNames = Enumerable
                .Range(0, previousCount)
                .Select(i => DevicesUtilities.GetVirtualDeviceName())
                .ToArray();

            var virtualDevices = new VirtualDevice[previousCount];

            for (var i = 0; i < previousCount; i++)
            {
                virtualDevices[i] = VirtualDevice.Create(previousNames[i]);
            }

            Thread.Sleep(2000);

            var lastVirtualDeviceName = DevicesUtilities.GetVirtualDeviceName();

            using (var lastVirtualDevice = VirtualDevice.Create(lastVirtualDeviceName))
            {
                Thread.Sleep(2000);

                using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(lastVirtualDeviceName))
                {
                    for (var i = 0; i < previousCount; i++)
                    {
                        virtualDevices[i]?.Dispose();
                    }

                    Thread.Sleep(2000);

                    Assert.DoesNotThrow(
                        () => inputEndpoint.StartEventsListening(),
                        "Exception has been thrown.");
                }
            }
        }

        [Test]
        public void CheckInputEndpointsEquality_ViaEquals_SameEndpoints()
        {
            var inputEndpoint1 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
            var inputEndpoint2 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);

            ClassicAssert.AreEqual(inputEndpoint1, inputEndpoint2, "Endpoints are not equal.");
        }

        [Test]
        public void CheckInputEndpointsEquality_ViaEquals_DifferentEndpoints()
        {
            var inputEndpoint1 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
            var inputEndpoint2 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.B);

            ClassicAssert.AreNotEqual(inputEndpoint1, inputEndpoint2, "Endpoints are equal.");
        }

        [Test]
        public void CheckInputEndpointsEquality_ViaOperator_SameEndpoints()
        {
            var inputEndpoint1 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
            var inputEndpoint2 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);

            ClassicAssert.IsTrue(inputEndpoint1 == inputEndpoint2, "Endpoints are not equal via equality.");
            ClassicAssert.IsFalse(inputEndpoint1 != inputEndpoint2, "Endpoints are not equal via inequality.");
        }

        [Test]
        public void CheckInputEndpointsEquality_ViaOperator_DifferentEndpoints()
        {
            var inputEndpoint1 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
            var inputEndpoint2 = DevicesUtilities.GetInputEndpoint(MidiEndpoints.B);

            ClassicAssert.IsFalse(inputEndpoint1 == inputEndpoint2, "Endpoints are equal via equality.");
            ClassicAssert.IsTrue(inputEndpoint1 != inputEndpoint2, "Endpoints are equal via inequality.");
        }

        [Test]
        public void FindInputEndpointInDictionary()
        {
            var label = "X";
            var dictionary = new Dictionary<MidiEndpoint, string>
            {
                [DevicesUtilities.GetInputEndpoint(MidiEndpoints.A)] = label
            };

            var inputEndpoint = DevicesUtilities.GetInputEndpoint(MidiEndpoints.A);
            ClassicAssert.IsTrue(dictionary.TryGetValue(inputEndpoint, out var value), "Failed to find endpoint in dictionary.");
            ClassicAssert.AreEqual(label, value, "Endpoint label is invalid.");
        }

        #endregion

        #region Private methods

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
            var endpointName = MidiEndpoints.A;

            var receivedEvents = new List<MidiEvent>(expectedEvents.Count);
            var checkpoints = new TestCheckpoints();

            using (var dataSender = new DataSender(endpointName))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(endpointName))
            {
#if TEST
                inputEndpoint.TestCheckpoints = checkpoints;
#endif
                inputEndpoint.WaitForCompleteSysExEvent = waitForCompleteSysExEvent;

                inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(e.Event);
                inputEndpoint.StartEventsListening();

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

                var checkpointData = checkpoints.GetCheckpointDataList(InputEndpointCheckpointsNames.MessageDataReceived);
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
            var deviceName = MidiEndpoints.A;

            var receivedEvents = new List<MidiEvent>(expectedEvents.Count);
            var checkpoints = new TestCheckpoints();

            using (var outputEndpoint = OutputEndpoint.GetByName(deviceName))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(deviceName))
            {
#if TEST
                inputEndpoint.TestCheckpoints = checkpoints;
#endif
                inputEndpoint.WaitForCompleteSysExEvent = waitForCompleteSysExEvent;

                inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(e.Event);

                outputEndpoint.PrepareForEventsSending();
                inputEndpoint.StartEventsListening();

                foreach (var packet in packets)
                {
                    var data = packet.Data;
                    outputEndpoint.SendData_Win(data);
                    WaitOperations.Wait(5);
                }

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay + TimeSpan.FromMilliseconds(30);
                var areEventReceived = WaitOperations.Wait(
                    () => receivedEvents.Count >= expectedEvents.Count,
                    timeout);

                var checkpointData = checkpoints.GetCheckpointDataList(InputEndpointCheckpointsNames.MessageDataReceived);
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
