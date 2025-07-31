using NUnit.Framework;
using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class DynamicPlaybackAction
        {
            public DynamicPlaybackAction(
                int periodMs,
                Action<Playback, ObservableTimedObjectsCollection> action)
            {
                PeriodMs = periodMs;
                Action = action;
            }

            public int PeriodMs { get; }

            public Action<Playback, ObservableTimedObjectsCollection> Action { get; }

            public override string ToString() =>
                $"After {PeriodMs} ms";
        }

        #endregion

        #region Constants

        private const int OnTheFlyChecksRetriesNumber = 5;

        #endregion

        #region Private methods

        private void CheckPlaybackDataChangesOnTheFly(
            ICollection<ITimedObject> initialObjects,
            DynamicPlaybackAction[] actions,
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            int? repeatsCount = null,
            Action<Playback, ICollection<SentReceivedEvent>> additionalChecks = null,
            TempoMap tempoMap = null)
        {
            var collection = new ObservableTimedObjectsCollection(initialObjects);

            CheckPlayback(
                useOutputDevice: false,
                createPlayback: outputDevice => new Playback(collection, tempoMap ?? TempoMap, outputDevice, new PlaybackSettings
                {
                    CalculateTempoMap = true
                }),
                actions: actions
                    .Select(a => new PlaybackAction(a.PeriodMs, p => a.Action(p, collection)))
                    .ToArray(),
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: setupPlayback,
                repeatsCount: repeatsCount,
                additionalChecks: additionalChecks);
        }

        private void CheckDuration(
            TimeSpan expectedDuration,
            Playback playback)
        {
            var actualDuration = (TimeSpan)playback.GetDuration<MetricTimeSpan>();
            ClassicAssert.Less(
                (expectedDuration - actualDuration).Duration(),
                TimeSpan.FromMilliseconds(4),
                $"Invalid duration. Actual = {actualDuration}. Expected = {expectedDuration}.");
        }

        #endregion
    }
}
