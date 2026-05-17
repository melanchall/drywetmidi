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
using System.IO;
using System.Linq;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputEndpointTests
    {
        #region Setup

        [OneTimeSetUp]
        public static void GlobalSetup()
        {
            var sysExTracesDirectoryPath = GetSysExTracesRootDirectoryPath();
            if (Directory.Exists(sysExTracesDirectoryPath))
                Directory.Delete(sysExTracesDirectoryPath, true);

            Directory.CreateDirectory(sysExTracesDirectoryPath);
        }

        #endregion

        #region Test methods

        [WinOnly]
        [DeviceInformationApiRequired]
        [Test]
        public void CheckOutputEndpointDeviceInformation([Values(MidiEndpoints.A, MidiEndpoints.B, MidiEndpoints.C)] string endpointName)
        {
            var outputEndpoint = OutputEndpoint.GetByName(endpointName);

            var deviceInformation = outputEndpoint.GetDeviceInformation();
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
        public void CheckOutputEndpointMultiClientAccess()
        {
            using (var outputEndpoint1 = OutputEndpoint.GetByName(MidiEndpoints.A))
            using (var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.A))
            using (var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A))
            {
                var receivedEventsCount = 0;
                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;

                inputEndpoint.EventReceived += (_, e) => receivedEventsCount++;
                inputEndpoint.StartEventsListening();

                outputEndpoint1.PrepareForEventsSending();
                outputEndpoint1.SendEvent(new NoteOnEvent());

                var success1 = WaitOperations.Wait(() => receivedEventsCount == 1, timeout);
                ClassicAssert.IsTrue(success1, "Event is not received from first send.");

                outputEndpoint2.PrepareForEventsSending();
                outputEndpoint2.SendEvent(new NoteOnEvent());

                var success2 = WaitOperations.Wait(() => receivedEventsCount == 2, timeout);
                ClassicAssert.IsTrue(success2, "Event is not received from second send.");
            }
        }

        [TestCase(MidiEndpoints.A)]
        [TestCase(MidiEndpoints.B)]
        public void GetOutputEndpointByName(string endpointName)
        {
            ClassicAssert.IsNotNull(OutputEndpoint.GetByName(endpointName), "There is no endpoint.");
        }

        [Test]
        public void GetAllOutputEndpoints()
        {
            var outputEndpoints = OutputEndpoint.GetAll();
            var outputEndpointsCount = OutputEndpoint.GetEndpointsCount();
            ClassicAssert.AreEqual(outputEndpointsCount, outputEndpoints.Count, "Output endpoints count is invalid.");
        }

        [Test]
        public void GetOutputEndpointsCount()
        {
            var outputEndpointsCount = OutputEndpoint.GetEndpointsCount();
            ClassicAssert.GreaterOrEqual(
                outputEndpointsCount,
                MidiEndpoints.GetAllEndpointsNames().Length,
                "Output endpoints count is invalid.");
        }

        [Test]
        public void SendEvent_EscapeSysEx() => Assert.Throws<ArgumentException>(
            () => SendEvents(new[] { new EscapeSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }) }));

        [MultimediaTestRetry]
        [Test]
        public void SendEvent_SysEx_1()
        {
            SendEvents(new[] { new NormalSysExEvent(new byte[] { 0x5F, 0x40, 0xF7 }) });
        }

        [MultimediaTestRetry]
        [Test]
        public void SendEvent_SysEx_2()
        {
            SendEvents(new[] { new NormalSysExEvent(new byte[] { 0xF0, 0x5F, 0x40, 0xF7 }) });
        }

        // TODO: fix very large sys ex sending
        [MultimediaTestRetry]
        [Test]
        public void SendEvent_SysEx_Large([Values(100, 1000, 10000/*, 100000*/)] int size)
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
        public void SendEvent_SysEx_NotTerminated()
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
        public void SendEvent_SysEx_Multiple([Values(2, 5, 10)] int eventsCount, [Values(1, 10, 100, 1000)] int dataSize)
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
        public void SendEvent_Short_Default(MidiEventType eventType)
        {
            var midiEvent = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(SysExEvent).IsAssignableFrom(t) && !typeof(MetaEvent).IsAssignableFrom(t))
                .Select(t => (MidiEvent)Activator.CreateInstance(t))
                .First(e => e.EventType == eventType);

            SendEvents(new[] { midiEvent });
        }

        [MultimediaTestRetry]
        [TestCaseSource(nameof(GetNonDefaultShortEvents))]
        public void SendEvent_Short_NonDefault(MidiEvent midiEvent)
        {
            SendEvents(new[] { midiEvent });
        }

#if TEST
        [Test]
        public void OutputEndpointIsReleasedByDispose()
        {
            for (var i = 0; i < 10; i++)
            {
                var checkpoints = new TestCheckpoints();

                var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A);
                outputEndpoint.TestCheckpoints = checkpoints;

                ClassicAssert.DoesNotThrow(() => outputEndpoint.SendEvent(new NoteOnEvent()));

                checkpoints.CheckCheckpointsAreNotReached(
                    OutputEndpointCheckpointsNames.ReleaseHandleEntered,
                    OutputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                outputEndpoint.Dispose();

                checkpoints.CheckCheckpointsReached(
                    OutputEndpointCheckpointsNames.ReleaseHandleEntered,
                    OutputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }

        [Test]
        public void OutputEndpointIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, bool> sendEvent = testCheckpoints =>
            {
                var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A);
                outputEndpoint.TestCheckpoints = testCheckpoints;

                try
                {
                    outputEndpoint.SendEvent(new NoteOnEvent());
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
                    OutputEndpointCheckpointsNames.ReleaseHandleEntered,
                    OutputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                ClassicAssert.IsTrue(sendEvent(checkpoints), $"Can't send event on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                checkpoints.CheckCheckpointsReached(
                    OutputEndpointCheckpointsNames.ReleaseHandleEntered,
                    OutputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }
#endif

        [Test]
        public void DisableEnableOutputEndpoint()
        {
            using (var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A))
            {
                ClassicAssert.IsTrue(outputEndpoint.IsEnabled, "Endpoint is not enabled initially.");

                var sentEventsCount = 0;

                outputEndpoint.PrepareForEventsSending();
                outputEndpoint.EventSent += (_, __) => sentEventsCount++;

                outputEndpoint.SendEvent(new NoteOnEvent());
                var eventReceived = WaitOperations.Wait(() => sentEventsCount == 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not sent.");

                outputEndpoint.IsEnabled = false;
                ClassicAssert.IsFalse(outputEndpoint.IsEnabled, "Endpoint is enabled after disabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, TimeSpan.FromSeconds(5));
                ClassicAssert.IsFalse(eventReceived, "Event is sent after endpoint disabled.");

                outputEndpoint.IsEnabled = true;
                ClassicAssert.IsTrue(outputEndpoint.IsEnabled, "Endpoint is disabled after enabling.");

                outputEndpoint.SendEvent(new NoteOnEvent());
                eventReceived = WaitOperations.Wait(() => sentEventsCount > 1, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(eventReceived, "Event is not sent after enabling again.");
            }
        }

        [Test]
        public void OutputEndpointToString_User()
        {
            var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.AreEqual("Output endpoint", outputEndpoint.ToString(), "Endpoint string representation is invalid.");
        }

        [Test]
        public void GetOutputEndpointHashCode()
        {
            foreach (var outputEndpoint in OutputEndpoint.GetAll())
            {
                ClassicAssert.DoesNotThrow(() => outputEndpoint.GetHashCode(), $"Failed to get hash code for [{outputEndpoint.Name}].");
            }
        }

        [VirtualDeviceApiRequired]
        [Test]
        public void AccessOutputEndpointAfterSomeEndpointRemoval([Values(1, 5)] int previousCount)
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
            using (var outputEndpoint = OutputEndpoint.GetByName(lastVirtualDeviceName))
            {
                for (var i = 0; i < previousCount; i++)
                {
                    virtualDevices[i]?.Dispose();
                }

                Thread.Sleep(2000);

                Assert.DoesNotThrow(
                    () => outputEndpoint.SendEvent(new NoteOnEvent()),
                    "Exception has been thrown.");
            }
        }

        [Test]
        public void CheckOutputEndpointsEquality_ViaEquals_SameEndpoints()
        {
            var outputEndpoint1 = OutputEndpoint.GetByName(MidiEndpoints.A);
            var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.A);

            ClassicAssert.AreEqual(outputEndpoint1, outputEndpoint2, "Endpoints are not equal.");
        }

        [Test]
        public void CheckOutputEndpointsEquality_ViaEquals_DifferentEndpoints()
        {
            var outputEndpoint1 = OutputEndpoint.GetByName(MidiEndpoints.A);
            var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.B);

            ClassicAssert.AreNotEqual(outputEndpoint1, outputEndpoint2, "Endpoints are equal.");
        }

        [Test]
        public void CheckOutputEndpointsEquality_ViaOperator_SameEndpoints()
        {
            var outputEndpoint1 = OutputEndpoint.GetByName(MidiEndpoints.A);
            var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.A);

            ClassicAssert.IsTrue(outputEndpoint1 == outputEndpoint2, "Endpoints are not equal via equality.");
            ClassicAssert.IsFalse(outputEndpoint1 != outputEndpoint2, "Endpoints are not equal via inequality.");
        }

        [Test]
        public void CheckOutputEndpointsEquality_ViaOperator_DifferentEndpoints()
        {
            var outputEndpoint1 = OutputEndpoint.GetByName(MidiEndpoints.A);
            var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.B);

            ClassicAssert.IsFalse(outputEndpoint1 == outputEndpoint2, "Endpoints are equal via equality.");
            ClassicAssert.IsTrue(outputEndpoint1 != outputEndpoint2, "Endpoints are equal via inequality.");
        }

        [Test]
        public void FindOutputEndpointInDictionary()
        {
            var label = "X";
            var dictionary = new Dictionary<MidiEndpoint, string>
            {
                [OutputEndpoint.GetByName(MidiEndpoints.A)] = label
            };

            var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.IsTrue(dictionary.TryGetValue(outputEndpoint, out var value), "Failed to find endpoint in dictionary.");
            ClassicAssert.AreEqual(label, value, "Endpoint label is invalid.");
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

        private void SendEvents(
            MidiEvent[] midiEvents,
            Action<ICollection<TimestampedEvent>> checkAction = null,
            Action<InputEndpoint> setupInputEndpoint = null)
        {
            var deviceName = MidiEndpoints.A;
            var stopwatch = new Stopwatch();

            var timestampedEvents = midiEvents
                .Select(e => new TimestampedEvent(e, TimeSpan.Zero))
                .ToArray();

            var receivedEvents = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();

#if TEST
            var checkpoints = new TestCheckpoints();
#endif

            using (var outputEndpoint = OutputEndpoint.GetByName(deviceName))
            {
                outputEndpoint.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputEndpoint.PrepareForEventsSending();

                string errorOnSend = null;
                outputEndpoint.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

                using (var inputEndpoint = InputEndpoint.GetByName(deviceName))
                {
#if TEST
                    inputEndpoint.TestCheckpoints = checkpoints;
#endif

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

#if TEST
                    var checkpointData = checkpoints.GetCheckpointDataList(InputEndpointCheckpointsNames.MessageDataReceived);
                    var checkpointDataString = $" (checkpoints data ({checkpointData?.Count ?? 0} record(s)): {GetCheckpointDataString(checkpointData)})";
#else
                    var checkpointDataString = string.Empty;
#endif

                    try
                    {
                        SendReceiveUtilities.CheckTimestampedEvents(
                            sentEvents,
                            timestampedEvents,
                            timeout,
                            $"Sent events are invalid{checkpointDataString}.");

                        SendReceiveUtilities.CheckTimestampedEvents(
                            receivedEvents,
                            timestampedEvents,
                            timeout,
                            $"Received events are invalid{checkpointDataString}.");

                        checkAction?.Invoke(receivedEvents);
                    }
                    catch
                    {
                        SaveSysExTraces(inputEndpoint.SysExParts);
                        throw;
                    }
                }
            }
        }

        private static void SaveSysExTraces(ICollection<byte[]> sysExParts)
        {
            var tracesDirectoryPath = GetSysExTracesDirectoryPath();
            var fileName = GetSysExTracesFileName("OrphanedSysExParts");
            var filePath = Path.Combine(tracesDirectoryPath, $"{fileName}.log");

            string GetHexBytesString(byte[] bytes) =>
                bytes == null || bytes.Length == 0
                    ? "no data"
                    : string.Join(" ", bytes.Select(b => Convert.ToString(b, 16).PadLeft(2, '0').ToUpper()));

            File.WriteAllLines(filePath,
                new[] { $"SysEx parts: {sysExParts?.Count ?? 0}" }
                .Concat(sysExParts.Select((p, i) => $"Part {i + 1} ({p?.Length ?? 0} byte(s)): {GetHexBytesString(p)}"))
                .Concat(new[] { $"Full data ({sysExParts.Sum(p => p?.Length ?? 0)} byte(s)): {GetHexBytesString(sysExParts.SelectMany(p => p).ToArray())}" }));
        }

        private static string GetCheckpointDataString(ICollection<object> data) =>
            string.Join(
                "; ",
                data?.Select(GetCheckpointDataRecordString) ?? Array.Empty<string>());

        private static string GetCheckpointDataRecordString(object data)
        {
            if (data == null)
                return "null";

            var bytes = data as byte[];
            if (bytes != null)
            {
                const int margin = 3;

                var processedData = bytes.Select(b => Convert.ToString(b, 16).PadLeft(2, '0').ToUpper()).ToArray();
                var dataLength = processedData.Length;
                if (dataLength == 0)
                    return "no data";

                var result = $"{dataLength} byte(s): ";
                if (dataLength <= margin * 2)
                    return $"{result}{string.Join(" ", processedData)}";

                return $"{result}{processedData[0]} {processedData[1]} {processedData[2]} ... {processedData[dataLength - 3]} {processedData[dataLength - 2]} {processedData[dataLength - 1]}";
            }
            
            return data.ToString();
        }

        private static string GetSysExTracesFileName(string label)
        {
            var testName = GetTestName();
            var retryCount = TestContext.CurrentContext.CurrentRepeatCount;
            return $"{testName}{(string.IsNullOrWhiteSpace(label) ? string.Empty : $"_{label}")}_{retryCount}";
        }

        private static string GetSysExTracesRootDirectoryPath()
        {
            var tempPath = Path.GetTempPath();

            var artifactsStagingDirectory = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
            var workspaceDirectory = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");

            if (!string.IsNullOrWhiteSpace(artifactsStagingDirectory))
                tempPath = Path.Combine(artifactsStagingDirectory, Environment.GetEnvironmentVariable("BUILD_BUILDID"));
            else if (!string.IsNullOrWhiteSpace(workspaceDirectory))
                tempPath = workspaceDirectory;

            return Path.Combine(tempPath, "SysExTraces");
        }

        private static string GetSysExTracesDirectoryPath()
        {
            var rootPath = GetSysExTracesRootDirectoryPath();

            var directoryPath = Path.Combine(rootPath, GetTestName());
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }

        private static string GetTestName() =>
            TestContext.CurrentContext.Test.Name;

        #endregion
    }
}
