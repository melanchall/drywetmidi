using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Common;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class DevicesConnectorTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(50);

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckEventsReceivingOnConnectedDevices()
        {
            CheckEventsReceiving(new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new SongSelectEvent((SevenBitNumber)20), TimeSpan.Zero),
                new EventToSend(new TuneRequestEvent(), TimeSpan.FromMilliseconds(200)),
            });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckFileEventsReceivingOnConnectedDevices()
        {
            var filesToTestCount = 5;
            var maxFileDuration = TimeSpan.FromSeconds(10);

            var filesToTest = TestFilesProvider.GetValidFiles(
                    f => f.GetTrackChunks().Count() == 1,
                    f => (TimeSpan)f.GetDuration<MetricTimeSpan>() < maxFileDuration)
                .OrderByDescending(f => f.GetDuration<MetricTimeSpan>())
                .Take(filesToTestCount)
                .ToArray();
            Debug.Assert(filesToTest.Length == filesToTestCount, "Not enough files for test.");

            for (var i = 0; i < filesToTest.Length; i++)
            {
                var file = filesToTest[i];
                var tempoMap = file.GetTempoMap();

                var eventsToSend = new List<EventToSend>();
                var currentTime = TimeSpan.Zero;

                foreach (var timedEvent in file.GetTimedEvents().Where(e => !(e.Event is MetaEvent)))
                {
                    var time = (TimeSpan)timedEvent.TimeAs<MetricTimeSpan>(tempoMap);
                    var eventToSend = new EventToSend(timedEvent.Event, time - currentTime);
                    currentTime = time;

                    if (eventToSend.Event is SysExEvent)
                        continue;

                    eventsToSend.Add(eventToSend);
                }

                CheckEventsReceiving(eventsToSend);
            }
        }

        #endregion

        #region Private methods

        private static void CheckEventsReceiving(IReadOnlyList<EventToSend> eventsToSend)
        {
            var receivedEventsB = new List<ReceivedEvent>();
            var receivedEventsC = new List<ReceivedEvent>();
            var sentEvents = new List<SentEvent>();
            var stopwatch = new Stopwatch();

            using (var outputA = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                SendReceiveUtilities.WarmUpDevice(outputA);
                outputA.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var inputB = InputDevice.GetByName(MidiDevicesNames.DeviceB))
                using (var inputC = InputDevice.GetByName(MidiDevicesNames.DeviceC))
                {
                    inputB.EventReceived += (_, e) => receivedEventsB.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                    inputB.StartEventsListening();

                    inputC.EventReceived += (_, e) => receivedEventsC.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                    inputC.StartEventsListening();

                    using (var inputA = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                    {
                        inputA.StartEventsListening();

                        using (var outputB = OutputDevice.GetByName(MidiDevicesNames.DeviceB))
                        using (var outputC = OutputDevice.GetByName(MidiDevicesNames.DeviceC))
                        {
                            var devicesConnector = inputA.Connect(outputB, outputC);
                            Assert.IsTrue(devicesConnector.AreDevicesConnected, "Devices aren't connected.");

                            stopwatch.Start();
                            SendReceiveUtilities.SendEvents(eventsToSend, outputA);
                            stopwatch.Stop();

                            var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                            var areEventsReceived = SpinWait.SpinUntil(
                                () => receivedEventsB.Count == eventsToSend.Count && receivedEventsC.Count == eventsToSend.Count,
                                timeout);
                            Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                            devicesConnector.Disconnect();
                            Assert.IsFalse(devicesConnector.AreDevicesConnected, "Devices aren't disconnected.");
                        }
                    }
                }
            }

            SendReceiveUtilities.CompareSentReceivedEvents(eventsToSend, sentEvents, receivedEventsB, MaximumEventSendReceiveDelay);
            SendReceiveUtilities.CompareSentReceivedEvents(eventsToSend, sentEvents, receivedEventsC, MaximumEventSendReceiveDelay);
        }

        #endregion
    }
}
