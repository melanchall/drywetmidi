using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class InteractionAsyncTests
    {
        private sealed class ImmediateProgress : IProgress<AsyncOperationProgress>
        {
            internal readonly List<AsyncOperationProgress> Reports = new List<AsyncOperationProgress>();
            public void Report(AsyncOperationProgress value) => Reports.Add(value);
        }

        private sealed class CancelingComparer : TimedObjectsComparer
        {
            internal Action OnCompare;
            public override int Compare(ITimedObject x, ITimedObject y)
            {
                OnCompare?.Invoke();
                return base.Compare(x, y);
            }
        }

        [Test]
        public async Task Getters_MatchSynchronousResults()
        {
            var track = CreateTrack();
            var file = new MidiFile(track);
            var tracks = new[] { track };
            var events = track.Events;
            MidiAsserts.AreEqual(events.GetNotes(), await events.GetNotesAsync(), "Events notes");
            MidiAsserts.AreEqual(track.GetNotes(), await track.GetNotesAsync(), "Track notes");
            MidiAsserts.AreEqual(tracks.GetNotes(), await tracks.GetNotesAsync(), "Tracks notes");
            MidiAsserts.AreEqual(file.GetNotes(), await file.GetNotesAsync(), "File notes");
            MidiAsserts.AreEqual(events.GetChords(), await events.GetChordsAsync(), "Events chords");
            MidiAsserts.AreEqual(track.GetChords(), await track.GetChordsAsync(), "Track chords");
            MidiAsserts.AreEqual(tracks.GetChords(), await tracks.GetChordsAsync(), "Tracks chords");
            MidiAsserts.AreEqual(file.GetChords(), await file.GetChordsAsync(), "File chords");
            MidiAsserts.AreEqual(file.GetNotes().GetChords(), await CollectAsync(file.GetNotes().GetChordsAsync()), "Notes chords");
            MidiAsserts.AreEqual(events.GetTimedEvents(), await events.GetTimedEventsAsync(), "Events timed events");
            MidiAsserts.AreEqual(track.GetTimedEvents(), await track.GetTimedEventsAsync(), "Track timed events");
            MidiAsserts.AreEqual(tracks.GetTimedEvents(), await tracks.GetTimedEventsAsync(), "Tracks timed events");
            MidiAsserts.AreEqual(file.GetTimedEvents(), await file.GetTimedEventsAsync(), "File timed events");
            var type = ObjectType.Chord | ObjectType.Note | ObjectType.TimedEvent;
            MidiAsserts.AreEqual(events.GetObjects(type), await events.GetObjectsAsync(type), "Events objects");
            MidiAsserts.AreEqual(track.GetObjects(type), await track.GetObjectsAsync(type), "Track objects");
            MidiAsserts.AreEqual(tracks.GetObjects(type), await tracks.GetObjectsAsync(type), "Tracks objects");
            MidiAsserts.AreEqual(file.GetObjects(type), await file.GetObjectsAsync(type), "File objects");
            Assert.That(await file.GetDurationAsync<MidiTimeSpan>(), Is.EqualTo(file.GetDuration<MidiTimeSpan>()));
            Assert.That(await track.GetDurationAsync(TimeSpanType.Midi, TempoMap.Default), Is.EqualTo(track.GetDuration<MidiTimeSpan>(TempoMap.Default)));
            Assert.That(await tracks.GetDurationAsync<MidiTimeSpan>(TempoMap.Default), Is.EqualTo(tracks.GetDuration<MidiTimeSpan>(TempoMap.Default)));
            MidiAsserts.AreEqual(file.GetNotes().GetRests(), await file.GetNotes().GetRestsAsync(), "Rests");
        }

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
                await Tracks().GetTimedEventsAsync(cancellationToken: cancellation.Token));
            Assert.That(visited, Is.EqualTo(1));
        }

        [Test]
        public async Task ProcessNotes_AwaitsSequentiallyAndPersistsTiming()
        {
            var track = CreateTrack();
            var active = 0;
            var visited = new List<long>();
            var count = await track.ProcessNotesAsync(async (note, token) =>
            {
                Assert.That(Interlocked.Increment(ref active), Is.EqualTo(1));
                await Task.Delay(1, token);
                visited.Add(note.Time);
                note.Time += 25;
                note.Length += 5;
                Interlocked.Decrement(ref active);
            }, match: note => note.NoteNumber == (SevenBitNumber)60);
            Assert.That(count, Is.EqualTo(2));
            Assert.That(visited, Is.EqualTo(new[] { 0L, 200L }));
            Assert.That(track.GetNotes().Select(n => n.Time), Is.EqualTo(new[] { 0L, 25L, 225L }));
            Assert.That(track.GetNotes().Where(n => n.NoteNumber == (SevenBitNumber)60).Select(n => n.Length), Is.EqualTo(new[] { 105L, 55L }));
        }

        [Test]
        public async Task ProcessChords_AwaitedCollectionChangesPersist()
        {
            var track = CreateTrack();
            var count = await track.ProcessChordsAsync(async (chord, token) =>
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
                chord.Notes.Remove(chord.Notes.First());
                chord.Notes.Add(new Note((SevenBitNumber)67, 60, 30));
            }, match: chord => chord.Notes.Count > 1, hint: ChordProcessingHint.AllPropertiesCanBeChanged);
            Assert.That(count, Is.EqualTo(1));
            var notes = track.GetNotes();
            Assert.That(notes.Count, Is.EqualTo(3));
            Assert.That(notes, Has.Some.Matches<Note>(n => n.NoteNumber == (SevenBitNumber)67 && n.Time == 30 && n.Length == 60));
            Assert.That(notes, Has.None.Matches<Note>(n => n.NoteNumber == (SevenBitNumber)60 && n.Time == 0));
        }

        [Test]
        public async Task ProcessTimedEvents_AwaitedDeltaTimeIsRestoredAndTimesSorted()
        {
            var track = new TrackChunk(new TextEvent("a") { DeltaTime = 10 }, new TextEvent("b") { DeltaTime = 20 });
            Assert.That(await track.ProcessTimedEventsAsync(async (ev, token) =>
            {
                await Task.Delay(1, token);
                ev.Event.DeltaTime = 999;
                ev.Time = 100 - ev.Time;
            }), Is.EqualTo(2));
            Assert.That(track.GetTimedEvents().Select(e => e.Time), Is.EqualTo(new[] { 70L, 90L }));
            Assert.That(track.Events.OfType<TextEvent>().Select(e => e.Text), Is.EqualTo(new[] { "b", "a" }));
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
            Assert.That(actual, Is.EqualTo(expected));
            MidiAsserts.AreEqual(synchronous.GetTimedEvents(), asynchronous.GetTimedEvents(), "Processed objects");
        }

        [Test]
        public async Task ProcessNotes_HintNoneMatchesSynchronousBehavior()
        {
            var expected = CreateTrack();
            var actual = CreateTrack();
            expected.ProcessNotes(n => n.Time += 30, hint: NoteProcessingHint.None);
            await actual.ProcessNotesAsync(async (n, token) =>
            {
                await Task.Yield();
                n.Time += 30;
            }, hint: NoteProcessingHint.None);
            MidiAsserts.AreEqual(expected.GetTimedEvents(), actual.GetTimedEvents(), "Hint");
        }

        [TestCase("Notes")]
        [TestCase("Chords")]
        [TestCase("TimedEvents")]
        [TestCase("Objects")]
        public void CancellationInFinalSynchronousCallback_DoesNotEraseEvents(string kind)
        {
            using var cancellation = new CancellationTokenSource();
            var track = CreateTrack();
            var expected = track.GetTimedEvents();
            var total = kind == "Notes" ? track.GetNotes().Count :
                kind == "Chords" ? track.GetChords().Count :
                kind == "TimedEvents" ? track.GetTimedEvents().Count :
                track.GetObjects(ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent).Count;
            var count = 0;
            Action<ITimedObject> action = obj =>
            {
                obj.Time += 10;
                if (++count == total)
                    cancellation.Cancel();
            };
            Assert.That(async () => await ProcessSynchronouslyAsync(new[] { track }, kind, action, cancellation.Token),
                Throws.InstanceOf<OperationCanceledException>());
            Assert.That(count, Is.EqualTo(total));
            MidiAsserts.AreEqual(expected, track.GetTimedEvents(), "Canceled rewrite must preserve unrelated events and delta times");
        }

        [TestCase("Notes")]
        [TestCase("Chords")]
        [TestCase("TimedEvents")]
        [TestCase("Objects")]
        public async Task AwaitedCallbacks_IgnoreNullTracksLikeSynchronousCallbacks(string kind)
        {
            var expected = CreateTrack();
            var actual = CreateTrack();
            var expectedCount = await ProcessSynchronouslyAsync(new[] { null, expected, null }, kind, obj => obj.Time += 10, CancellationToken.None);
            var actualCount = await ProcessAwaitedAsync(new[] { null, actual, null }, kind, async (obj, token) =>
            {
                await Task.Yield();
                obj.Time += 10;
            });
            Assert.That(actualCount, Is.EqualTo(expectedCount));
            MidiAsserts.AreEqual(expected.GetTimedEvents(), actual.GetTimedEvents(), "Null tracks must be ignored");
        }

        [Test]
        public async Task CanceledManagerSave_PreservesEventsAndPreviousDeltaTimes()
        {
            using var cancellation = new CancellationTokenSource();
            var track = CreateTrack();
            var comparer = new CancelingComparer();
            using var manager = await track.ManageNotesAsync(comparer: comparer);
            await manager.SaveChangesAsync();
            var expected = track.GetTimedEvents();
            manager.Objects.First().Time += 20;
            comparer.OnCompare = cancellation.Cancel;
            Assert.That(async () => await manager.SaveChangesAsync(cancellationToken: cancellation.Token),
                Throws.InstanceOf<OperationCanceledException>());
            comparer.OnCompare = null;
            MidiAsserts.AreEqual(expected, track.GetTimedEvents(), "Canceled manager save must preserve committed contents");
        }

        [Test]
        public async Task RemoveOperations_MatchSynchronousCountsAndEvents()
        {
            foreach (var type in new[] { ObjectType.Note, ObjectType.Chord, ObjectType.TimedEvent, ObjectType.Note | ObjectType.TimedEvent })
            {
                var expected = CreateTrack();
                var actual = CreateTrack();
                Assert.That(await actual.RemoveObjectsAsync(type, o => o.Time == 0), Is.EqualTo(expected.RemoveObjects(type, o => o.Time == 0)));
                MidiAsserts.AreEqual(expected.GetTimedEvents(), actual.GetTimedEvents(), "Removed objects");
            }
            var track = CreateTrack();
            Assert.That(await track.RemoveChordsAsync(settings: new ChordDetectionSettings { NotesMinCount = 2 }), Is.EqualTo(1));
            Assert.That(await track.RemoveNotesAsync(), Is.EqualTo(1));
            Assert.That(await track.RemoveTimedEventsAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task Managers_CreateAndSaveChangesAsync()
        {
            var track = CreateTrack();
            using (var manager = await track.ManageNotesAsync())
            {
                manager.Objects.First().Length = 25;
                await manager.SaveChangesAsync();
                Assert.That(track.GetNotes().First().Length, Is.EqualTo(25));
            }
            using (var manager = await TimedObjectsManager.CreateAsync(track.Events, ObjectType.Note))
            {
                manager.Objects.First().Time = 10;
                await manager.SaveChangesAsync();
            }
            using (var manager = await TimedObjectsManager<Note>.CreateAsync(track.Events))
                Assert.That(manager.Objects.Count, Is.EqualTo(3));
            using (var manager = await track.ManageChordsAsync())
                Assert.That(manager.Objects.Count, Is.EqualTo(track.GetChords().Count));
            using (var manager = await track.ManageTimedEventsAsync())
                Assert.That(manager.Objects.Count, Is.EqualTo(7));
        }

        [Test]
        public async Task TempoMap_ReplacementAndManagerSaveMatchSynchronous()
        {
            var file = new MidiFile(CreateTrack());
            var tempoMap = TempoMap.Create(new Tempo(400000));
            await file.ReplaceTempoMapAsync(tempoMap);
            Assert.That((await file.GetTempoMapAsync()).GetTempoAtTime(new MidiTimeSpan(0)), Is.EqualTo(new Tempo(400000)));
            using (var manager = await file.ManageTempoMapAsync())
            {
                await manager.ReplaceTempoMapAsync(TempoMap.Create(new Tempo(600000)));
                await manager.SaveChangesAsync();
                Assert.That(file.GetTempoMap().GetTempoAtTime(new MidiTimeSpan(0)), Is.EqualTo(new Tempo(600000)));
            }
        }

        [Test]
        public async Task AsyncEventSource_IsAwaitedAndDisposed()
        {
            var disposed = false;
            async IAsyncEnumerable<MidiEvent> Produce()
            {
                try
                {
                    foreach (var ev in CreateTrack().Events)
                    {
                        await Task.Yield();
                        yield return ev;
                    }
                }
                finally { disposed = true; }
            }
            Assert.That((await Produce().GetNotesAsync()).Count, Is.EqualTo(3));
            Assert.That(disposed, Is.True);
            Assert.That((await Produce().GetChordsAsync()).Count, Is.EqualTo(2));
            Assert.That((await Produce().GetTimedEventsAsync()).Count, Is.EqualTo(7));
            Assert.That((await Produce().GetObjectsAsync(ObjectType.Note)).Count, Is.EqualTo(3));
        }

        [Test]
        public async Task AsyncTrackAndObjectSources_MatchSynchronousResults()
        {
            var tracks = new[] { CreateTrack(), CreateTrack() };
            MidiAsserts.AreEqual(tracks.GetNotes(), await AsAsync(tracks).GetNotesAsync(), "Async tracks notes");
            MidiAsserts.AreEqual(tracks.GetChords(), await AsAsync(tracks).GetChordsAsync(), "Async tracks chords");
            MidiAsserts.AreEqual(tracks.GetTimedEvents(), await AsAsync(tracks).GetTimedEventsAsync(), "Async tracks events");
            MidiAsserts.AreEqual(tracks.GetObjects(ObjectType.Note), await AsAsync(tracks).GetObjectsAsync(ObjectType.Note), "Async tracks objects");
            MidiAsserts.AreEqual(tracks.GetNotes().GetChords(), await AsAsync(tracks.GetNotes()).GetChordsAsync(), "Async notes chords");
            MidiAsserts.AreEqual(tracks.GetNotes().GetRests(), await AsAsync(tracks.GetNotes()).GetRestsAsync(), "Async notes rests");
            MidiAsserts.AreEqual(tracks.GetNotes().GetObjects(ObjectType.TimedEvent), await AsAsync(tracks.GetNotes().Cast<ITimedObject>()).GetObjectsAsync(ObjectType.TimedEvent), "Async timed objects");
            Assert.That(await AsAsync(tracks).GetDurationAsync<MidiTimeSpan>(TempoMap.Default), Is.EqualTo(tracks.GetDuration<MidiTimeSpan>(TempoMap.Default)));
            MidiAsserts.AreEqual(tracks.GetTempoMap(new TicksPerQuarterNoteTimeDivision()), await AsAsync(tracks).GetTempoMapAsync(new TicksPerQuarterNoteTimeDivision()), "Async tempo map");
        }

        [Test]
        public void AsyncSourceFailure_PropagatesAndDisposesProducer()
        {
            var disposed = false;
            async IAsyncEnumerable<MidiEvent> Produce()
            {
                try
                {
                    await Task.Yield();
                    yield return new TextEvent();
                    throw new InvalidOperationException("producer");
                }
                finally { disposed = true; }
            }
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Produce().GetNotesAsync());
            Assert.That(disposed, Is.True);
        }

        [Test]
        public void CancellationDuringDetection_StopsSourceEnumeration()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;
            IEnumerable<MidiEvent> Produce()
            {
                while (visited < 100000)
                {
                    if (++visited == 100)
                        cancellation.Cancel();
                    yield return new TextEvent();
                }
            }
            Assert.ThrowsAsync<OperationCanceledException>(async () => await Produce().GetNotesAsync(cancellationToken: cancellation.Token));
            Assert.That(visited, Is.EqualTo(100));
        }

        [Test]
        public void CancellationDuringEmptyTrackEnumeration_StopsBeforeMaterialization()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;
            IEnumerable<TrackChunk> Produce()
            {
                while (visited < 100000)
                {
                    if (++visited == 100)
                        cancellation.Cancel();
                    yield return new TrackChunk();
                }
            }
            Assert.ThrowsAsync<OperationCanceledException>(async () => await Produce().GetTempoMapAsync(new TicksPerQuarterNoteTimeDivision(), cancellationToken: cancellation.Token));
            Assert.That(visited, Is.EqualTo(100));
        }

        [Test]
        public void CancellationDuringRestsSorting_StopsSourceEnumeration()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;
            IEnumerable<ITimedObject> Produce()
            {
                while (visited < 100000)
                {
                    if (++visited == 100)
                        cancellation.Cancel();
                    yield return new Note((SevenBitNumber)60, 1, 100000 - visited);
                }
            }
            Assert.ThrowsAsync<OperationCanceledException>(async () => await Produce().GetRestsAsync(cancellationToken: cancellation.Token));
            Assert.That(visited, Is.EqualTo(100));
        }

        [Test]
        public void AsyncActionCancellation_StopsBeforeNextCallback()
        {
            using var cancellation = new CancellationTokenSource();
            var callbacks = 0;
            Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateTrack().ProcessNotesAsync(async (note, token) =>
            {
                await Task.Yield();
                callbacks++;
                cancellation.Cancel();
            }, cancellationToken: cancellation.Token));
            Assert.That(callbacks, Is.EqualTo(1));
        }

        [Test]
        public async Task EnumerateObjects_IsLazyAndDisposesSourceOnEarlyExit()
        {
            var visited = 0;
            var disposed = false;
            IEnumerable<MidiEvent> Produce()
            {
                try
                {
                    for (var i = 0; i < 10000; i++)
                    {
                        visited++;
                        yield return new TextEvent();
                    }
                }
                finally { disposed = true; }
            }
            var objects = Produce().EnumerateObjectsAsync(ObjectType.TimedEvent);
            Assert.That(visited, Is.Zero);
            await foreach (var obj in objects)
                break;
            Assert.That(visited, Is.EqualTo(1));
            Assert.That(disposed, Is.True);
        }

        [Test]
        public async Task GetChordsFromNotes_DefersDetectionUntilEnumeration()
        {
            var started = false;
            var disposed = false;
            IEnumerable<Note> Produce()
            {
                try
                {
                    started = true;
                    foreach (var note in CreateTrack().GetNotes())
                        yield return note;
                }
                finally { disposed = true; }
            }
            IAsyncEnumerable<Chord> chords = Produce().GetChordsAsync();
            Assert.That(started, Is.False);
            var result = await CollectAsync(chords);
            Assert.That(started, Is.True);
            Assert.That(disposed, Is.True);
            MidiAsserts.AreEqual(CreateTrack().GetNotes().GetChords(), result, "Async chord enumeration");
        }

        [Test]
        public async Task EnumerateObjects_PreservesDetectionAndOrdering()
        {
            var track = CreateTrack();
            var type = ObjectType.Chord | ObjectType.Note | ObjectType.TimedEvent;
            var result = new List<ITimedObject>();
            await foreach (var obj in track.Events.EnumerateObjectsAsync(type))
                result.Add(obj);
            MidiAsserts.AreEqual(track.Events.EnumerateObjects(type), result, "Lazy objects");
        }

        [Test]
        public void EnumerateObjects_PropagatesSourceFailure()
        {
            IEnumerable<MidiEvent> Produce()
            {
                yield return new TextEvent();
                throw new InvalidOperationException("source");
            }
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var obj in Produce().EnumerateObjectsAsync(ObjectType.TimedEvent)) { }
            });
        }

        [Test]
        public void PreCanceledOperation_DoesNotEnumerateOrMutate()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var called = false;
            Assert.That(async () => await CreateTrack().ProcessNotesAsync(n => called = true, cancellationToken: cancellation.Token), Throws.InstanceOf<OperationCanceledException>());
            Assert.That(called, Is.False);
            Assert.That(async () =>
            {
                await foreach (var obj in CreateTrack().Events.EnumerateObjectsAsync(ObjectType.Note, cancellationToken: cancellation.Token)) { }
            }, Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task Progress_ReportsCompletionAndWork()
        {
            var progress = new ImmediateProgress();
            await CreateTrack().GetNotesAsync(progress: progress);
            Assert.That(progress.Reports.Last().IsCompleted, Is.True);
            Assert.That(progress.Reports.Last().ProcessedItems, Is.GreaterThan(0));
        }

        [Test]
        public async Task GetNotesAsync_ReturnsWhileWorkerIsBlockedAtDeterministicGate()
        {
            using var release = new ManualResetEventSlim();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IEnumerable<MidiEvent> Produce()
            {
                entered.SetResult(true);
                release.Wait();
                yield return new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100);
                yield return new NoteOffEvent((SevenBitNumber)60, SevenBitNumber.MinValue) { DeltaTime = 10 };
            }

            var operation = Produce().GetNotesAsync();
            try
            {
                await entered.Task;
                Assert.That(operation.IsCompleted, Is.False, "The caller must regain control while source enumeration is blocked on the worker.");
                TestContext.Out.WriteLine("Responsiveness: async invocation returned while its worker was held at a deterministic source gate; no elapsed-time threshold was used.");
            }
            finally
            {
                release.Set();
            }
            Assert.That((await operation).Count, Is.EqualTo(1));
        }

        [Test]
        [NonParallelizable]
        public async Task GetNotesAsync_ReportsSeparateThroughputAllocationAndRetainedMemoryDiagnostics()
        {
            const int notesCount = 8192;
            var events = new MidiEvent[notesCount * 2];
            for (var i = 0; i < notesCount; i++)
            {
                events[2 * i] = new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100) { DeltaTime = 1 };
                events[2 * i + 1] = new NoteOffEvent((SevenBitNumber)60, SevenBitNumber.MinValue) { DeltaTime = 10 };
            }
            events.Take(2).GetNotes();
            await events.Take(2).GetNotesAsync();

            var syncRetainedBefore = GC.GetTotalMemory(true);
            var syncAllocatedBefore = GC.GetTotalAllocatedBytes(true);
            var syncTimer = Stopwatch.StartNew();
            var syncNotes = events.GetNotes();
            syncTimer.Stop();
            var syncAllocated = GC.GetTotalAllocatedBytes(true) - syncAllocatedBefore;
            var syncRetained = GC.GetTotalMemory(true) - syncRetainedBefore;
            GC.KeepAlive(syncNotes);

            var asyncRetainedBefore = GC.GetTotalMemory(true);
            var asyncAllocatedBefore = GC.GetTotalAllocatedBytes(true);
            var asyncTimer = Stopwatch.StartNew();
            var asyncNotes = await events.GetNotesAsync();
            asyncTimer.Stop();
            var asyncAllocated = GC.GetTotalAllocatedBytes(true) - asyncAllocatedBefore;
            var asyncRetained = GC.GetTotalMemory(true) - asyncRetainedBefore;
            GC.KeepAlive(asyncNotes);

            TestContext.Out.WriteLine($"GetNotes input: {events.Length} events, {notesCount} notes.");
            TestContext.Out.WriteLine($"Sync throughput observation: {syncTimer.Elapsed.TotalMilliseconds:F3} ms, {notesCount / Math.Max(syncTimer.Elapsed.TotalSeconds, 1e-9):F0} notes/s.");
            TestContext.Out.WriteLine($"Async throughput observation: {asyncTimer.Elapsed.TotalMilliseconds:F3} ms, {notesCount / Math.Max(asyncTimer.Elapsed.TotalSeconds, 1e-9):F0} notes/s.");
            TestContext.Out.WriteLine($"Process-wide allocation deltas: sync={syncAllocated} bytes; async={asyncAllocated} bytes.");
            TestContext.Out.WriteLine($"Post-GC retained-memory deltas with each result rooted: sync={syncRetained} bytes; async={asyncRetained} bytes.");
            TestContext.Out.WriteLine("Diagnostics are single-run observations, include process/test-host noise, and make no speedup, peak-memory, or bounded-memory claim. Throughput excludes retained-memory snapshot collection time.");
            Assert.That(asyncNotes.Count, Is.EqualTo(notesCount));
            MidiAsserts.AreEqual(syncNotes, asyncNotes, "Diagnostic operations must retain equivalent results");
        }

        private static TrackChunk CreateTrack()
        {
            var track = new TrackChunk();
            track.Events.AddObjects(new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 100, 0),
                new Note((SevenBitNumber)64, 100, 0),
                new TimedEvent(new TextEvent("marker"), 150),
                new Note((SevenBitNumber)60, 50, 200)
            });
            return track;
        }

        private static async IAsyncEnumerable<T> AsAsync<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source)
        {
            var result = new List<T>();
            await foreach (var item in source)
                result.Add(item);
            return result;
        }

        private static Task<int> ProcessSynchronouslyAsync(IEnumerable<TrackChunk> tracks, string kind, Action<ITimedObject> action, CancellationToken cancellationToken)
        {
            switch (kind)
            {
                case "Notes": return tracks.ProcessNotesAsync(action, cancellationToken: cancellationToken);
                case "Chords": return tracks.ProcessChordsAsync(action, cancellationToken: cancellationToken);
                case "TimedEvents": return tracks.ProcessTimedEventsAsync(action, cancellationToken: cancellationToken);
                default: return tracks.ProcessObjectsAsync(ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent, action, cancellationToken: cancellationToken);
            }
        }

        private static Task<int> ProcessAwaitedAsync(IEnumerable<TrackChunk> tracks, string kind, Func<ITimedObject, CancellationToken, ValueTask> action)
        {
            switch (kind)
            {
                case "Notes": return tracks.ProcessNotesAsync(action);
                case "Chords": return tracks.ProcessChordsAsync(action);
                case "TimedEvents": return tracks.ProcessTimedEventsAsync(action);
                default: return tracks.ProcessObjectsAsync(ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent, action);
            }
        }
    }
}
