using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackCurrentTimeAfterSpeedChange(
            [Values(0.5, 1, 10)] double speed,
            [Values(10, 100)] int waitAfterSpeedChangeMs) =>
            CheckPlaybackCurrentTimeAfterSpeedChanges(speed, waitAfterSpeedChangeMs, false);

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackCurrentTimeAfterMultipleSpeedChanges(
            [Values(0.5, 1, 10)] double speed,
            [Values(10, 100)] int waitAfterSpeedChangeMs) =>
            CheckPlaybackCurrentTimeAfterSpeedChanges(speed, waitAfterSpeedChangeMs, true);

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackSpeedRandomChanges()
        {
            var changeAfter = TimeSpan.FromSeconds(1);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100000 }));

            var stopwatch = new Stopwatch();

            using (var playback = midiFile.GetPlayback())
            {
                playback.Start();
                stopwatch.Start();

                WaitOperations.Wait(changeAfter);
                var currentTimeBeforeSpeedChange = playback.GetCurrentTime<MetricTimeSpan>();

                for (var i = 0; i < 300; i++)
                {
                    playback.Speed = 0.1 * (DryWetMidi.Common.Random.Instance.Next(100) + 1);
                    WaitOperations.Wait(DryWetMidi.Common.Random.Instance.Next(100) + 1);
                }
            }
        }

        #endregion

        #region Private methods

        public void CheckPlaybackCurrentTimeAfterSpeedChanges(double speed, int waitAfterSpeedChangeMs, bool variateSpeed)
        {
            var changeAfter = TimeSpan.FromSeconds(1);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100000 }));

            var stopwatch = new Stopwatch();
            var waitTimes = new List<long>(3);
            var speeds = new List<double>
            {
                speed, speed * 0.5, speed * 2
            };

            using (var playback = midiFile.GetPlayback())
            {
                playback.Start();
                stopwatch.Start();

                WaitOperations.Wait(changeAfter);
                var currentTimeBeforeSpeedChange = playback.GetCurrentTime<MetricTimeSpan>();

                playback.Speed = speeds[0];
                waitTimes.Add(WaitOperations.Wait(waitAfterSpeedChangeMs));

                if (variateSpeed)
                {
                    playback.Speed = speeds[1];
                    waitTimes.Add(WaitOperations.Wait(waitAfterSpeedChangeMs));
                    playback.Speed = speeds[2];
                    waitTimes.Add(WaitOperations.Wait(waitAfterSpeedChangeMs));
                }

                var currentTimeAfterSpeedChange = playback.GetCurrentTime<MetricTimeSpan>();

                playback.Stop();
                stopwatch.Stop();

                var delta = (long)Math.Round((currentTimeAfterSpeedChange - currentTimeBeforeSpeedChange).TotalMilliseconds);
                var reference = waitTimes[0] * speeds[0];
                if (variateSpeed)
                {
                    reference += waitTimes[1] * speeds[1];
                    reference += waitTimes[2] * speeds[2];
                }

                ClassicAssert.LessOrEqual(
                    delta,
                    MathUtilities.AddRelativeMargin(reference, 0.2),
                    "Invalid current time delta after speed change.");
            }
        }

        #endregion
    }
}
