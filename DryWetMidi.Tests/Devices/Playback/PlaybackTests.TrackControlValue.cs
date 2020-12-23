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
        public void TrackControlValue_NoControlChanges_MoveToTime(bool useOutputDevice, int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
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
        public void TrackControlValue_NoControlChanges_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackControlValue(
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
        public void TrackControlValue_ControlChangeAtZero_MoveToTime(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_ControlChangeAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlChangeDelay = TimeSpan.FromMilliseconds(800);
            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, controlChangeDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - controlChangeDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, controlChangeDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - controlChangeDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromBeforeControlChange_ToBeforeControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - (moveTo - moveFrom)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromBeforeControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_Default_FromBeforeControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromAfterControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)0;
            var controlValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromAfterControlChange_ToBeforeControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - controlChangeTime),
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - moveTo),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        #endregion

        #region Private methods

        private void CheckTrackControlValue(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice)
        {
            if (useOutputDevice)
                CheckTrackControlValueWithOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo);
            else
                CheckTrackControlValueWithoutOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo);
        }

        private void CheckTrackControlValueWithOutputDevice(
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
                    playback.TrackControlValue = true;

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

        private void CheckTrackControlValueWithoutOutputDevice(
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
                playback.TrackControlValue = true;
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
