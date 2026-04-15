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

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputDeviceTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

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

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        public void GetOutputDeviceByName(string deviceName)
        {
            ClassicAssert.IsNotNull(OutputDevice.GetByName(deviceName), "There is no device.");
        }

        [Test]
        public void GetOutputDeviceByIndex_Valid()
        {
            var devicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.IsNotNull(OutputDevice.GetByIndex(devicesCount / 2), "There is no device.");
        }

        [Test]
        public void GetOutputDeviceByIndex_BelowZero()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => OutputDevice.GetByIndex(-1), "Exception is not thrown.");
        }

        [Test]
        public void GetOutputDeviceByIndex_BeyondDevicesCount()
        {
            var devicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => OutputDevice.GetByIndex(devicesCount), "Exception is not thrown.");
        }

        [Test]
        public void GetAllOutputDevices()
        {
            var outputDevices = OutputDevice.GetAll();
            var outputDevicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.AreEqual(outputDevicesCount, outputDevices.Count, "Output devices count is invalid.");
        }

        [Test]
        public void GetOutputDevicesCount()
        {
            var outputDevicesCount = OutputDevice.GetDevicesCount();
            ClassicAssert.GreaterOrEqual(
                outputDevicesCount,
                MidiDevicesNames.GetAllDevicesNames().Length,
                "Output devices count is invalid.");
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
        public void OutputDeviceIsReleasedByDispose()
        {
            for (var i = 0; i < 10; i++)
            {
                var checkpoints = new TestCheckpoints();

                var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
                outputDevice.TestCheckpoints = checkpoints;

                ClassicAssert.DoesNotThrow(() => outputDevice.SendEvent(new NoteOnEvent()));

                checkpoints.CheckCheckpointsAreNotReached(
                    OutputDeviceCheckpointsNames.ReleaseHandleEntered,
                    OutputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                outputDevice.Dispose();

                checkpoints.CheckCheckpointsReached(
                    OutputDeviceCheckpointsNames.ReleaseHandleEntered,
                    OutputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }

        [Test]
        public void OutputDeviceIsReleasedByFinalizer()
        {
            Func<TestCheckpoints, bool> sendEvent = testCheckpoints =>
            {
                var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
                outputDevice.TestCheckpoints = testCheckpoints;

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
                var checkpoints = new TestCheckpoints();

                checkpoints.CheckCheckpointsAreNotReached(
                    OutputDeviceCheckpointsNames.ReleaseHandleEntered,
                    OutputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);

                ClassicAssert.IsTrue(sendEvent(checkpoints), $"Can't send event on iteration {i}.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                checkpoints.CheckCheckpointsReached(
                    OutputDeviceCheckpointsNames.ReleaseHandleEntered,
                    OutputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle,
                    OutputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle,
                    OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered,
                    OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            }
        }
#endif

        [Test]
        public void DisableEnableOutputDevice()
        {
            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                ClassicAssert.IsTrue(outputDevice.IsEnabled, "Device is not enabled initially.");

                var sentEventsCount = 0;

                outputDevice.PrepareForEventsSending();
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

        [Test]
        public void OutputDeviceToString_User()
        {
            var outputDevice = GetUserOutputDevice();
            ClassicAssert.AreEqual("Output device", outputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        public void GetOutputDeviceHashCode()
        {
            foreach (var outputDevice in OutputDevice.GetAll())
            {
                ClassicAssert.DoesNotThrow(() => outputDevice.GetHashCode(), $"Failed to get hash code for [{outputDevice.Name}].");
            }
        }

        #endregion

        #region Private methods

        private static OutputDevice GetUserOutputDevice()
        {
            return OutputDevice.GetByName(MidiDevicesNames.DeviceA);
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
            var deviceName = MidiDevicesNames.DeviceA;
            var stopwatch = new Stopwatch();

            var timestampedEvents = midiEvents
                .Select(e => new TimestampedEvent(e, TimeSpan.Zero))
                .ToArray();

            var receivedEvents = new List<TimestampedEvent>();
            var sentEvents = new List<TimestampedEvent>();

#if TEST
            var checkpoints = new TestCheckpoints();
#endif

            using (var outputDevice = OutputDevice.GetByName(deviceName))
            {
                outputDevice.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));
                outputDevice.PrepareForEventsSending();

                string errorOnSend = null;
                outputDevice.ErrorOccurred += (_, e) => errorOnSend = e.Exception.Message;

                using (var inputDevice = InputDevice.GetByName(deviceName))
                {
#if TEST
                    inputDevice.TestCheckpoints = checkpoints;
#endif

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

#if TEST
                    var checkpointData = checkpoints.GetCheckpointDataList(InputDeviceCheckpointsNames.MessageDataReceived);
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
                        SaveSysExTraces(inputDevice.SysExParts);
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
