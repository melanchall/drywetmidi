using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public void RemovalScope_NestedSelectionsAreIsolated(
            [Values] bool sameEvent,
            [Values] bool runOnWorker)
        {
            var outerEvent = new TimedEvent(new TextEvent("outer"));
            var innerEvent = sameEvent ? outerEvent : new TimedEvent(new TextEvent("inner"));

            using (var outer = new InteractionAsyncUtilities.RemovalScope())
            {
                outer.Mark(outerEvent);

                ClassicAssert.IsTrue(outer.IsMarked(outerEvent), "The outer scope did not select its event.");
                ClassicAssert.IsTrue(outerEvent.Event.MustBeRemoved, "The outer scope did not set its event's removal flag.");

                void UseInnerScope()
                {
                    using (var inner = new InteractionAsyncUtilities.RemovalScope())
                    {
                        ClassicAssert.IsFalse(inner.IsMarked(outerEvent), "The nested scope consumed an outer selection before marking anything.");

                        inner.Mark(innerEvent);

                        ClassicAssert.AreEqual(sameEvent, inner.IsMarked(outerEvent), "The nested scope treated an outer-only selection as its own.");
                        ClassicAssert.IsTrue(inner.IsMarked(innerEvent), "The nested scope did not select its event.");
                        ClassicAssert.IsTrue(outerEvent.Event.MustBeRemoved, "The nested scope cleared the outer removal flag.");
                    }
                }

                if (runOnWorker)
                    Task.Run(UseInnerScope).GetAwaiter().GetResult();
                else
                    UseInnerScope();

                ClassicAssert.IsTrue(outer.IsMarked(outerEvent), "Disposing the nested scope lost the outer selection.");
                ClassicAssert.IsTrue(outerEvent.Event.MustBeRemoved, "Disposing the nested scope cleared the outer removal flag.");
                ClassicAssert.AreEqual(sameEvent, innerEvent.Event.MustBeRemoved, "Nested disposal did not clear only newly marked events.");
            }

            ClassicAssert.IsFalse(outerEvent.Event.MustBeRemoved, "Disposing the outer scope left its removal flag set.");
            ClassicAssert.IsFalse(innerEvent.Event.MustBeRemoved, "Disposing all scopes left the nested event's removal flag set.");
        }

        [Test]
        public async Task ProcessObjects_AwaitedMatchesSynchronousMutations()
        {
            var synchronous = CreateTrack();
            var asynchronous = CreateTrack();
            var type = ObjectType.Chord | ObjectType.Note | ObjectType.TimedEvent;
            void Mutate(ITimedObject obj)
            {
                obj.Time += 17;
                if (obj is Chord chord)
                    chord.Notes.Add(new Note((SevenBitNumber)70, 40, chord.Time));
            }

            var expected = synchronous.ProcessObjects(type, Mutate, hint: ObjectProcessingHint.AllPropertiesCanBeChanged);
            var actual = await asynchronous.ProcessObjectsAsync(type, async (obj, token) =>
            {
                await Task.Delay(1, token);
                Mutate(obj);
            }, hint: ObjectProcessingHint.AllPropertiesCanBeChanged);

            ClassicAssert.AreEqual(expected, actual, "Async processed object count differs.");
            MidiAsserts.AreEqual(synchronous.GetTimedEvents(), asynchronous.GetTimedEvents(), "Processed objects");
        }

        [Test]
        public void CancellationInFinalSynchronousCallback_DoesNotEraseEvents(
            [Values(NotesOperation, ChordsOperation, TimedEventsOperation, ObjectsOperation)] string kind)
        {
            using var cancellation = new CancellationTokenSource();
            var track = CreateTrack();
            var expected = track.GetTimedEvents();
            var total = kind == NotesOperation ? track.GetNotes().Count :
                kind == ChordsOperation ? track.GetChords().Count :
                kind == TimedEventsOperation ? track.GetTimedEvents().Count :
                track.GetObjects(ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent).Count;
            var count = 0;
            Action<ITimedObject> action = obj =>
            {
                obj.Time += 10;

                if (++count == total)
                    cancellation.Cancel();
            };

            Assert.CatchAsync<OperationCanceledException>(
                async () => await ProcessSynchronouslyAsync(new[] { track }, kind, action, cancellation.Token),
                "Cancellation in the final callback did not stop saving.");
            ClassicAssert.AreEqual(total, count, "Cancellation occurred before the final callback.");
            MidiAsserts.AreEqual(expected, track.GetTimedEvents(), "Canceled rewrite must preserve unrelated events and delta times");
        }

        [Test]
        public async Task AwaitedCallbacks_IgnoreNullTracksLikeSynchronousCallbacks(
            [Values(NotesOperation, ChordsOperation, TimedEventsOperation, ObjectsOperation)] string kind)
        {
            var expected = CreateTrack();
            var actual = CreateTrack();

            var expectedCount = await ProcessSynchronouslyAsync(new[] { null, expected, null }, kind, obj => obj.Time += 10, CancellationToken.None);
            var actualCount = await ProcessAwaitedAsync(new[] { null, actual, null }, kind, async (obj, token) =>
            {
                await Task.Yield();
                obj.Time += 10;
            });

            ClassicAssert.AreEqual(expectedCount, actualCount, "Awaited callback count differs when tracks contain null.");
            MidiAsserts.AreEqual(expected.GetTimedEvents(), actual.GetTimedEvents(), "Null tracks must be ignored");
        }

        [Test]
        public void RemoveObjects_CancellationDoesNotLeaveRemovalFlags(
            [Values(ObjectType.Note, ObjectType.Chord, ObjectType.Note | ObjectType.TimedEvent)] ObjectType type,
            [Values] bool multipleTracks)
        {
            var track = new TrackChunk();
            track.Events.AddObjects(new[]
            {
                new Note((SevenBitNumber)60, 10, 0),
                new Note((SevenBitNumber)62, 10, 100),
                new Note((SevenBitNumber)64, 10, 200)
            });
            var tracks = new[] { track, new TrackChunk() };
            var originalEvents = track.Events.ToArray();
            var deltas = originalEvents.Select(e => e.DeltaTime).ToArray();
            using var cancellation = new CancellationTokenSource();
            bool Match(ITimedObject obj)
            {
                if (obj.Time == 100)
                    cancellation.Cancel();

                return obj.Time == 0;
            }

            Assert.CatchAsync<OperationCanceledException>(async () =>
            {
                if (multipleTracks)
                    await tracks.RemoveObjectsAsync(type, Match, cancellationToken: cancellation.Token);
                else
                    await track.Events.RemoveObjectsAsync(type, Match, cancellationToken: cancellation.Token);
            }, "Object removal did not observe cancellation.");
            ClassicAssert.AreEqual(originalEvents, track.Events.ToArray(), "Canceled removal changed source events.");
            ClassicAssert.AreEqual(deltas, track.Events.Select(e => e.DeltaTime), "Canceled removal changed source delta-times.");
            ClassicAssert.IsTrue(track.Events.All(e => !e.MustBeRemoved), "Canceled removal left removal flags behind.");

            track.ProcessNotes(n => n.Time += 1);

            ClassicAssert.AreEqual(3, track.GetNotes().Count, "Canceled removal damaged subsequent note processing.");
        }

        [Test]
        public void RemoveOperations_PredicateFailureCleansRemovalFlags(
            [Values(NotesOperation, ChordsOperation, MixedOperation)] string operation,
            [Values] bool multipleTracks,
            [Values] bool cancel,
            [Values] bool finalItem)
        {
            var track = CreateTrack();
            var other = CreateTrack();
            other.ProcessTimedEvents(ev => ev.Time += 300);
            var tracks = multipleTracks ? new[] { track, other } : new[] { track };
            var originalEvents = tracks.Select(t => t.Events.ToArray()).ToArray();
            var originalDeltaTimes = originalEvents.Select(events => events.Select(ev => ev.DeltaTime).ToArray()).ToArray();
            var expectedNotesCount = tracks.GetNotes().Count;
            var objectType = ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent;
            var total = operation == NotesOperation ? tracks.GetNotes().Count :
                operation == ChordsOperation ? tracks.GetChords().Count :
                tracks.GetObjects(objectType).Count;
            var failureIndex = finalItem ? total : 2;
            var visited = 0;
            var failure = new InvalidOperationException("Removal predicate failed.");
            using var cancellation = new CancellationTokenSource();

            bool Match(ITimedObject obj)
            {
                if (++visited == failureIndex)
                {
                    if (cancel)
                        cancellation.Cancel();
                    else
                        throw failure;
                }

                return true;
            }

            Task<int> Remove()
            {
                switch (operation)
                {
                    case NotesOperation:
                        return multipleTracks
                            ? tracks.RemoveNotesAsync(note => Match(note), cancellationToken: cancellation.Token)
                            : track.Events.RemoveNotesAsync(note => Match(note), cancellationToken: cancellation.Token);
                    case ChordsOperation:
                        return multipleTracks
                            ? tracks.RemoveChordsAsync(chord => Match(chord), cancellationToken: cancellation.Token)
                            : track.Events.RemoveChordsAsync(chord => Match(chord), cancellationToken: cancellation.Token);
                    default:
                        return multipleTracks
                            ? tracks.RemoveObjectsAsync(objectType, Match, cancellationToken: cancellation.Token)
                            : track.Events.RemoveObjectsAsync(objectType, Match, cancellationToken: cancellation.Token);
                }
            }

            if (cancel)
            {
                Assert.CatchAsync<OperationCanceledException>(
                    async () => await Remove(),
                    "Removal did not observe cancellation from its predicate.");
            }
            else
            {
                var exception = Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await Remove(),
                    "Removal did not propagate the predicate failure.");

                ClassicAssert.AreSame(failure, exception, "Removal replaced the predicate exception.");
            }

            ClassicAssert.AreEqual(failureIndex, visited, "Removal visited objects after the predicate failed or canceled.");

            for (var i = 0; i < tracks.Length; i++)
            {
                ClassicAssert.AreEqual(originalEvents[i].Length, tracks[i].Events.Count, $"Failed removal changed the event count in track {i}.");
                ClassicAssert.AreEqual(originalDeltaTimes[i], tracks[i].Events.Select(ev => ev.DeltaTime), $"Failed removal changed delta-times in track {i}.");
                ClassicAssert.IsTrue(tracks[i].Events.All(ev => !ev.MustBeRemoved), $"Failed removal left removal flags in track {i}.");

                for (var j = 0; j < originalEvents[i].Length; j++)
                {
                    ClassicAssert.AreSame(originalEvents[i][j], tracks[i].Events[j], $"Failed removal replaced or reordered event {j} in track {i}.");
                }
            }

            tracks.ProcessNotes(note => note.Time += 1);

            ClassicAssert.AreEqual(expectedNotesCount, tracks.GetNotes().Count, "Failed removal damaged subsequent note processing.");
        }

        [Test]
        public async Task RemoveOperations_MatchSynchronousCountsAndEvents()
        {
            foreach (var type in new[] { ObjectType.Note, ObjectType.Chord, ObjectType.TimedEvent, ObjectType.Note | ObjectType.TimedEvent })
            {
                var expected = CreateTrack();
                var actual = CreateTrack();

                var expectedCount = expected.RemoveObjects(type, o => o.Time == 0);
                var actualCount = await actual.RemoveObjectsAsync(type, o => o.Time == 0);

                ClassicAssert.AreEqual(expectedCount, actualCount, $"Async removed object count differs for {type}.");
                MidiAsserts.AreEqual(expected.GetTimedEvents(), actual.GetTimedEvents(), "Removed objects");
            }

            var track = CreateTrack();

            ClassicAssert.AreEqual(1, await track.RemoveChordsAsync(settings: new ChordDetectionSettings { NotesMinCount = 2 }), "Invalid removed chord count.");
            ClassicAssert.AreEqual(1, await track.RemoveNotesAsync(), "Invalid removed note count.");
            ClassicAssert.AreEqual(1, await track.RemoveTimedEventsAsync(), "Invalid removed timed-event count.");
        }
    }
}
