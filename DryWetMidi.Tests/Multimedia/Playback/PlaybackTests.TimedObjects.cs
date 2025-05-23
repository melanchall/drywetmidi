﻿using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    new Note((SevenBitNumber)80, length, time + secondNoteShift)));

        #endregion

        #region Private methods

        private void CheckSingleTimedObjectPlayback(
            ITimedObject timedObject)
        {
            var receivedEvents = new List<SentReceivedEvent>();
            var stopwatch = new Stopwatch();
            var tempoMap = TempoMap;

            var expectedReceivedEvents = new[] { timedObject }
                .GetObjects(ObjectType.TimedEvent)
                .Cast<TimedEvent>()
                .Select(e => new SentReceivedEvent(e.Event, (TimeSpan)e.TimeAs<MetricTimeSpan>(tempoMap)))
                .ToArray();

            using (var playback = new Playback(new[] { timedObject }, tempoMap))
            {
                playback.EventPlayed += (_, e) => receivedEvents.Add(new SentReceivedEvent(e.Event, stopwatch.Elapsed));

                var timeout = expectedReceivedEvents.Last().Time + SendReceiveUtilities.MaximumEventSendReceiveDelay;

                playback.Start();
                stopwatch.Start();

                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                ClassicAssert.IsTrue(playbackFinished, $"Playback was not finished for [{timeout}].");

                stopwatch.Stop();
            }

            CollectionAssert.IsNotEmpty(receivedEvents, "No events received.");
            SendReceiveUtilities.CheckReceivedEvents(receivedEvents, expectedReceivedEvents);
        }

        #endregion
    }
}
