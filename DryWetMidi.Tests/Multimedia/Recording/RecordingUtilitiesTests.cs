using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class RecordingUtilitiesTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void SaveRecordingToFile()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromSeconds(5)),
                new EventToSend(new ActiveSensingEvent(), TimeSpan.FromMilliseconds(100)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(500)),
            };

            var receivedEvents = new List<ReceivedEvent>();
            var stopwatch = new Stopwatch();

            var waitTimeout = eventsToSend.Aggregate(TimeSpan.Zero, (result, e) => result + e.Delay) +
                SendReceiveUtilities.MaximumEventSendReceiveDelay;

            var inputDevice = TestDeviceManager.GetInputDevice("A");
            var outputDevice = TestDeviceManager.GetOutputDevice("A");

            inputDevice.StartEventsListening();
            inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

            using (var recording = new Recording(tempoMap, inputDevice))
            {
                var sendingThread = new Thread(() =>
                {
                    SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                });

                stopwatch.Start();
                recording.Start();
                sendingThread.Start();

                var threadAliveTimeout = waitTimeout + TimeSpan.FromSeconds(30);
                var threadExited = WaitOperations.Wait(() => !sendingThread.IsAlive, threadAliveTimeout);
                ClassicAssert.IsTrue(threadExited, $"Sending thread is alive after [{threadAliveTimeout}].");

                var eventsReceived = WaitOperations.Wait(() => receivedEvents.Count >= eventsToSend.Length, waitTimeout);
                ClassicAssert.IsTrue(eventsReceived, $"Events are not received for [{waitTimeout}] (received are: {string.Join(", ", receivedEvents)}).");

                recording.Stop();

                var midiFile = recording.ToFile();
                var timedEvents = midiFile.GetTimedEvents();

                var expectedEvents = new[]
                {
                    new TimedEvent(new NoteOnEvent(), TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.Zero, tempoMap)),
                    new TimedEvent(new NoteOffEvent(), TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), tempoMap)),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)40), TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(5.5), tempoMap)),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(6.1), tempoMap))
                };

                MidiAsserts.AreEqual(expectedEvents, timedEvents, false, 10, "Timed events saved incorrectly.");
            }
        }

        #endregion
    }
}
