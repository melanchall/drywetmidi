using System;
using System.Collections.Generic;
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
    }
}
