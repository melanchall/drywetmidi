using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckSingleTimedObjectPlayback_TimedEvent_MetaEvent([Values(0, 100)] long time) => CheckSingleTimedObjectPlayback(
            new TimedEvent(new TextEvent("A"), time));

        [Retry(RetriesNumber)]
        [Test]
        public void CheckSingleTimedObjectPlayback_TimedEvent_ChannelEvent([Values(0, 100)] long time) => CheckSingleTimedObjectPlayback(
            new TimedEvent(new ProgramChangeEvent(), time));

        [Retry(RetriesNumber)]
        [Test]
        public void CheckSingleTimedObjectPlayback_Note([Values(0, 100)] long time, [Values(0, 100)] long length) => CheckSingleTimedObjectPlayback(
            new Note((SevenBitNumber)70, length, time));

        [Retry(RetriesNumber)]
        [Test]
        public void CheckSingleTimedObjectPlayback_Chord_SingleNote([Values(0, 100)] long time, [Values(0, 100)] long length) => CheckSingleTimedObjectPlayback(
            new Chord(
                new Note((SevenBitNumber)70, length, time)));

        [Retry(RetriesNumber)]
        [Test]
        public void CheckSingleTimedObjectPlayback_Chord_MultipleNotes(
            [Values(0, 100)] long time,
            [Values(0, 100)] long length,
            [Values(0, 10, 100)] long secondNoteShift) => CheckSingleTimedObjectPlayback(
            new Chord(
                new Note((SevenBitNumber)70, length, time),
                new Note((SevenBitNumber)70, length, time + secondNoteShift)));

        #endregion

        #region Private methods

        private void CheckSingleTimedObjectPlayback(
            ITimedObject timedObject)
        {
            var playbackContext = new PlaybackContext();
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var expectedReceivedEvents = new[] { timedObject }
                .GetObjects(ObjectType.TimedEvent)
                .Cast<TimedEvent>()
                .Select(e => new ReceivedEvent(e.Event, (TimeSpan)e.TimeAs<MetricTimeSpan>(tempoMap)))
                .ToArray();

            using (var playback = new Playback(new[] { timedObject }, tempoMap))
            {
                playback.EventPlayed += (_, e) => playbackContext.ReceivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                var timeout = expectedReceivedEvents.Last().Time + SendReceiveUtilities.MaximumEventSendReceiveDelay;

                playback.Start();
                stopwatch.Start();

                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, $"Playback was not finished for [{timeout}].");

                stopwatch.Stop();
            }

            CollectionAssert.IsNotEmpty(playbackContext.ReceivedEvents, "No events received.");
            CheckReceivedEvents(playbackContext.ReceivedEvents, expectedReceivedEvents);
        }

        #endregion
    }
}
