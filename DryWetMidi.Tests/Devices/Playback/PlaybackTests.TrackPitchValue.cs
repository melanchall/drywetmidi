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
        public void TrackPitchValue_NoPitchBend_MoveToTime(bool useOutputDevice, int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
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
        public void TrackPitchValue_NoPitchBend_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackPitchValue(
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
        public void TrackPitchValue_PitchBendAtZero_MoveToTime(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_PitchBendAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new PitchBendEvent(pitchValue), moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromBeforePitchBend_ToBeforePitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - (moveTo - moveFrom)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromBeforePitchBend_ToAfterPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromAfterPitchBend_ToAfterPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromAfterPitchBend_ToBeforePitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new PitchBendEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - pitchBendTime),
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - moveTo),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        #endregion

        #region Private methods

        private void CheckTrackPitchValue(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice)
        {
            if (useOutputDevice)
                CheckTrackPitchValueWithOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo);
            else
                CheckTrackPitchValueWithoutOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo);
        }

        private void CheckTrackPitchValueWithOutputDevice(
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

                using (var playback = eventsForPlayback.GetPlayback(tempoMap, outputDevice))
                {
                    playback.TrackPitchValue = true;

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

        private void CheckTrackPitchValueWithoutOutputDevice(
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

            using (var playback = eventsForPlayback.GetPlayback(tempoMap))
            {
                playback.TrackPitchValue = true;
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
