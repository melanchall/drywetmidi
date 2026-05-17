using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Melanchall.DryWetMidi.Tests.Attributes;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class RecordingUtilitiesTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [MultimediaTestRetry]
        [Test]
        public void SaveRecordingToFile()
        {
            var tempoMap = TempoMap.Default;

            var eventsToSend = new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromMilliseconds(5500)),
                new TimestampedEvent(new ActiveSensingEvent(), TimeSpan.FromMilliseconds(5600)),
                new TimestampedEvent(new ProgramChangeEvent((SevenBitNumber)50), TimeSpan.FromMilliseconds(6100)),
            };

            var receivedEvents = new List<TimestampedEvent>();
            var stopwatch = new Stopwatch();

            var waitTimeout = eventsToSend.Max(e => e.Time) + SendReceiveUtilities.MaximumEventSendReceiveDelay;

            var inputEndpoint = TestDeviceManager.GetInputEndpoint("A");
            var outputEndpoint = TestDeviceManager.GetOutputEndpoint("A");

            inputEndpoint.StartEventsListening();
            inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

            using (var recording = new Recording(tempoMap, inputEndpoint))
            {
                var sendingThread = new Thread(() =>
                {
                    SendReceiveUtilities.SendEvents(eventsToSend, outputEndpoint);
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
