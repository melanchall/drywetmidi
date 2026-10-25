using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public void Getters_CancelBeforeFilteringNullTracks()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;
            IEnumerable<TrackChunk> Tracks()
            {
                cancellation.Cancel();

                for (var i = 0; i < 1000; i++)
                {
                    visited++;
                    yield return null;
                }
            }

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await Tracks().GetTimedEventsAsync(cancellationToken: cancellation.Token), "Cancellation was not observed before null-track filtering.");
            ClassicAssert.AreEqual(1, visited, "Track enumeration continued after cancellation.");
        }

        [Test]
        public async Task ProcessTimedEvents_AwaitedDeltaTimeIsRestoredAndTimesSorted()
        {
            var track = new TrackChunk(new TextEvent("a") { DeltaTime = 10 }, new TextEvent("b") { DeltaTime = 20 });

            var count = await track.ProcessTimedEventsAsync(async (ev, token) =>
            {
                await Task.Delay(1, token);
                ev.Event.DeltaTime = 999;
                ev.Time = 100 - ev.Time;
            });

            ClassicAssert.AreEqual(2, count, "Invalid processed timed-event count.");
            ClassicAssert.AreEqual(new[] { 70L, 90L }, track.GetTimedEvents().Select(e => e.Time), "Processed event times were not restored and sorted.");
            ClassicAssert.AreEqual(new[] { "b", "a" }, track.Events.OfType<TextEvent>().Select(e => e.Text), "Processed event order is invalid.");
        }

        [Test]
        public void RemoveTimedEvents_CancellationPreservesEvents([Values] bool multipleTracks)
        {
            var track = new TrackChunk(
                new TextEvent("a") { DeltaTime = 10 },
                new TextEvent("b") { DeltaTime = 20 },
                new TextEvent("c") { DeltaTime = 30 });
            var other = new TrackChunk(new TextEvent("other") { DeltaTime = 100 });
            var tracks = new[] { track, other };
            var originals = tracks.Select(t => t.Events.ToArray()).ToArray();
            var deltas = originals.Select(events => events.Select(e => e.DeltaTime).ToArray()).ToArray();
            using var cancellation = new CancellationTokenSource();
            bool Match(TimedEvent ev)
            {
                if (((TextEvent)ev.Event).Text == "b")
                    cancellation.Cancel();

                return ((TextEvent)ev.Event).Text == "a";
            }

            Assert.CatchAsync<OperationCanceledException>(async () =>
            {
                if (multipleTracks)
                    await tracks.RemoveTimedEventsAsync(Match, cancellationToken: cancellation.Token);
                else
                    await track.Events.RemoveTimedEventsAsync(Match, cancellationToken: cancellation.Token);
            }, "Timed-event removal did not observe cancellation.");

            for (var i = 0; i < tracks.Length; i++)
            {
                ClassicAssert.AreEqual(originals[i], tracks[i].Events.ToArray(), $"Canceled removal changed events in track {i}.");
                ClassicAssert.AreEqual(deltas[i], tracks[i].Events.Select(e => e.DeltaTime), $"Canceled removal changed delta-times in track {i}.");
            }
        }
    }
}
