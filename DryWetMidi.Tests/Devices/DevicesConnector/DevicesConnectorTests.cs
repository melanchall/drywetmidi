using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class DevicesConnectorTests
    {
        #region Constants

        public static readonly TimeSpan MaximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(50);

        #endregion

        #region Test methods

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

        [Test]
        public void CheckFileEventsReceivingOnConnectedDevices()
        {
            var filesToTest = TestFilesProvider.GetValidFiles(
                f => f.GetTrackChunks().Count() == 1,
                f =>
                {
                    var tempoMap = f.GetTempoMap();
                    return (TimeSpan)f.GetTimedEvents().Last().TimeAs<MetricTimeSpan>(tempoMap) < TimeSpan.FromSeconds(30);
                })
                .Take(5)
                .ToArray();

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
            var receivedEvents = new List<ReceivedEvent>();
            var sentEvents = new List<SentEvent>();
            var stopwatch = new Stopwatch();

            using (var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceB))
                {
                    inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                    inputDevice.StartEventsListening();

                    using (var inputOutputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                    {
                        inputOutputDevice.StartEventsListening();

                        using (var outputInputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceB))
                        using (var devicesConnector = inputOutputDevice.Connect(outputInputDevice))
                        {
                            stopwatch.Start();
                            SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                            stopwatch.Stop();

                            var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                            var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsToSend.Count, timeout);
                            Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                        }
                    }
                }
            }

            SendReceiveUtilities.CompareSentReceivedEvents(eventsToSend, sentEvents, receivedEvents, MaximumEventSendReceiveDelay);
        }

        #endregion
    }
}
