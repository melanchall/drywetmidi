using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                finally
                {
                    disposed = true;
                }
            }

            ClassicAssert.AreEqual(3, (await Produce().GetNotesAsync()).Count, "Async event source produced an invalid note count.");
            ClassicAssert.IsTrue(disposed, "Async event source was not disposed.");
            ClassicAssert.AreEqual(2, (await Produce().GetChordsAsync()).Count, "Async event source produced an invalid chord count.");
            ClassicAssert.AreEqual(7, (await Produce().GetTimedEventsAsync()).Count, "Async event source produced an invalid timed-event count.");
            ClassicAssert.AreEqual(3, (await Produce().GetObjectsAsync(ObjectType.Note)).Count, "Async event source produced an invalid object count.");
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
            ClassicAssert.AreEqual(tracks.GetDuration<MidiTimeSpan>(TempoMap.Default), await AsAsync(tracks).GetDurationAsync<MidiTimeSpan>(TempoMap.Default), "Async track-source duration differs.");
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
                finally
                {
                    disposed = true;
                }
            }

            Assert.ThrowsAsync<InvalidOperationException>(async () => await Produce().GetNotesAsync(), "Async producer failure was not propagated.");
            ClassicAssert.IsTrue(disposed, "Failed async producer was not disposed.");
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

            Assert.ThrowsAsync<OperationCanceledException>(async () => await Produce().GetNotesAsync(cancellationToken: cancellation.Token), "Note detection did not observe cancellation.");
            ClassicAssert.AreEqual(100, visited, "Note-detection source enumeration continued after cancellation.");
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
                finally
                {
                    disposed = true;
                }
            }

            var objects = Produce().EnumerateObjectsAsync(ObjectType.TimedEvent);

            ClassicAssert.AreEqual(0, visited, "Lazy object enumeration started before the first move.");

            await foreach (var obj in objects)
            {
                break;
            }

            ClassicAssert.AreEqual(1, visited, "Lazy object enumeration read beyond the requested item.");
            ClassicAssert.IsTrue(disposed, "Early exit did not dispose the object source.");
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
                    {
                        yield return note;
                    }
                }
                finally
                {
                    disposed = true;
                }
            }

            IAsyncEnumerable<Chord> chords = Produce().GetChordsAsync();

            ClassicAssert.IsFalse(started, "Chord detection started before enumeration.");

            var result = await CollectAsync(chords);

            ClassicAssert.IsTrue(started, "Chord enumeration did not start detection.");
            ClassicAssert.IsTrue(disposed, "Chord enumeration did not dispose the source.");
            MidiAsserts.AreEqual(CreateTrack().GetNotes().GetChords(), result, "Async chord enumeration");
        }

        [Test]
        public async Task EnumerateObjects_PreservesDetectionAndOrdering()
        {
            var track = CreateTrack();
            var type = ObjectType.Chord | ObjectType.Note | ObjectType.TimedEvent;
            var result = new List<ITimedObject>();

            await foreach (var obj in track.Events.EnumerateObjectsAsync(type))
            {
                result.Add(obj);
            }

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
                await foreach (var obj in Produce().EnumerateObjectsAsync(ObjectType.TimedEvent))
                {
                }
            }, "Lazy object enumeration did not propagate source failure.");
        }

        [Test]
        public void PreCanceledOperation_DoesNotEnumerateOrMutate()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var called = false;

            Assert.CatchAsync<OperationCanceledException>(async () => await CreateTrack().ProcessNotesAsync(n => called = true, cancellationToken: cancellation.Token), "Pre-canceled note processing did not throw.");
            ClassicAssert.IsFalse(called, "Pre-canceled processing invoked the note callback.");
            Assert.CatchAsync<OperationCanceledException>(async () =>
            {
                await foreach (var obj in CreateTrack().Events.EnumerateObjectsAsync(ObjectType.Note, cancellationToken: cancellation.Token))
                {
                }
            }, "Pre-canceled object enumeration did not throw.");
        }
    }
}
