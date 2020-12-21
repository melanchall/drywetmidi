using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [TestCase(true, 0)]
        [TestCase(true, 100)]
        [TestCase(false, 0)]
        [TestCase(false, 100)]
        public void TrackProgram_NoProgramChanges_MoveToTime(bool useOutputDevice, int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_NoProgramChanges_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_ProgramChangeAtZero_MoveToTime(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_ProgramChangeAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new ProgramChangeEvent(programNumber), moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromBeforeProgramChange_ToBeforeProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime - (moveTo - moveFrom)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromBeforeProgramChange_ToAfterProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromAfterProgramChange_ToAfterProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromAfterProgramChange_ToBeforeProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - programChangeTime),
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime - moveTo),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        #endregion

        #region Private methods

        private void CheckTrackProgram(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice)
        {
            if (useOutputDevice)
                CheckTrackProgramWithOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo);
            else
                CheckTrackProgramWithoutOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo);
        }

        private void CheckTrackProgramWithOutputDevice(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);
            var notes = eventsForPlayback.GetNotes().ToArray();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.TrackProgram = true;

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };
                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= moveFrom);
                        playback.MoveToTime((MetricTimeSpan)moveTo);

                        var timeout = TimeSpan.FromTicks(eventsWillBeSent.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsWillBeSent.Count, timeout);
                        Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                        stopwatch.Stop();

                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents, eventsWillBeSent.ToList());
        }

        private void CheckTrackProgramWithoutOutputDevice(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);
            var notes = eventsForPlayback.GetNotes().ToArray();

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                playback.TrackProgram = true;
                playback.EventPlayed += (_, e) =>
                {
                    lock (playbackContext.ReceivedEventsLockObject)
                    {
                        receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                        sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));
                    }
                };

                stopwatch.Start();
                playback.Start();

                SpinWait.SpinUntil(() => stopwatch.Elapsed >= moveFrom);
                playback.MoveToTime((MetricTimeSpan)moveTo);

                var timeout = TimeSpan.FromTicks(eventsWillBeSent.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsWillBeSent.Count, timeout);
                Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                stopwatch.Stop();

                var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(playbackStopped, "Playback is running after completed.");
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents, eventsWillBeSent.ToList());
        }

        #endregion
    }
}
