using System;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
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

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);

                using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                {
                    var receivedEventsNumber = 0;

                    inputDevice.StartEventsListening();
                    inputDevice.EventReceived += (_, __) => receivedEventsNumber++;

                    using (var recording = new Recording(tempoMap, inputDevice))
                    {
                        var sendingThread = new Thread(() =>
                        {
                            SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                        });

                        recording.Start();
                        sendingThread.Start();

                        SpinWait.SpinUntil(() => !sendingThread.IsAlive && receivedEventsNumber == eventsToSend.Length);
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

                        Assert.IsTrue(
                            TimedEventEquality.AreEqual(expectedEvents, timedEvents, false, 10),
                            "Timed events saved incorrectly.");
                    }
                }
            }
        }

        #endregion
    }
}
