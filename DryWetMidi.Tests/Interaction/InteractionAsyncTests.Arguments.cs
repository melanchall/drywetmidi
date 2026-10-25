using System;
using System.Collections.Generic;
using System.Threading;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public void AsyncSourceGetters_NullSourceReportsPublicParameterName()
        {
            void CheckArgument(Action operation, string expectedParameterName)
            {
                var exception = Assert.Throws<ArgumentNullException>(
                    () => operation(), "Async-source getter accepted a null source.");

                ClassicAssert.AreEqual(expectedParameterName, exception.ParamName, "Async-source getter reported an internal parameter name.");
            }

            IAsyncEnumerable<MidiEvent> midiEvents = null;
            IAsyncEnumerable<TrackChunk> trackChunks = null;
            IAsyncEnumerable<Note> notes = null;
            IAsyncEnumerable<ITimedObject> timedObjects = null;

            CheckArgument(() => midiEvents.GetNotesAsync(), nameof(midiEvents));
            CheckArgument(() => trackChunks.GetNotesAsync(), nameof(trackChunks));
            CheckArgument(() => midiEvents.GetChordsAsync(), nameof(midiEvents));
            CheckArgument(() => trackChunks.GetChordsAsync(), nameof(trackChunks));
            CheckArgument(() => notes.GetChordsAsync(), nameof(notes));
            CheckArgument(() => midiEvents.GetTimedEventsAsync(), nameof(midiEvents));
            CheckArgument(() => trackChunks.GetTimedEventsAsync(), nameof(trackChunks));
            CheckArgument(() => midiEvents.GetObjectsAsync(ObjectType.Note), nameof(midiEvents));
            CheckArgument(() => trackChunks.GetObjectsAsync(ObjectType.Note), nameof(trackChunks));
            CheckArgument(() => timedObjects.GetObjectsAsync(ObjectType.Note), nameof(timedObjects));
        }

        [Test]
        public void ArgumentErrors_MatchSynchronousOperations()
        {
            void CheckArgument(Action synchronous, Action asynchronous)
            {
                var expected = Assert.Throws<ArgumentNullException>(() => synchronous(), "Synchronous operation accepted a null argument.");
                var aggregate = Assert.Throws<AggregateException>(() => asynchronous(), "Async operation accepted a null argument.");

                ClassicAssert.IsInstanceOf<ArgumentNullException>(aggregate.InnerException, "Async argument error type differs.");
                ClassicAssert.AreEqual(expected.ParamName, ((ArgumentNullException)aggregate.InnerException).ParamName, "Async argument error parameter differs.");
            }

            var track = CreateTrack();

            CheckArgument(
                () => ((TrackChunk)null).GetNotes(),
                () => ((TrackChunk)null).GetNotesAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((TrackChunk)null).GetChords(),
                () => ((TrackChunk)null).GetChordsAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((EventsCollection)null).GetTimedEvents(),
                () => ((EventsCollection)null).GetTimedEventsAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((MidiFile)null).GetObjects(ObjectType.Note),
                () => ((MidiFile)null).GetObjectsAsync(ObjectType.Note, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((IEnumerable<ITimedObject>)null).GetRests(),
                () => ((IEnumerable<ITimedObject>)null).GetRestsAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((MidiFile)null).GetTempoMap(),
                () => ((MidiFile)null).GetTempoMapAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.GetDuration<MidiTimeSpan>(null),
                () => track.GetDurationAsync<MidiTimeSpan>(null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.ProcessNotes(null),
                () => track.ProcessNotesAsync((Action<Note>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.ProcessChords(null),
                () => track.ProcessChordsAsync((Action<Chord>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.ProcessTimedEvents(null),
                () => track.ProcessTimedEventsAsync((Action<TimedEvent>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.ProcessObjects(ObjectType.Note, null),
                () => track.ProcessObjectsAsync(ObjectType.Note, (Action<ITimedObject>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.RemoveNotes((Predicate<Note>)null),
                () => track.RemoveNotesAsync((Predicate<Note>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.RemoveChords((Predicate<Chord>)null),
                () => track.RemoveChordsAsync((Predicate<Chord>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => track.RemoveTimedEvents(null),
                () => track.RemoveTimedEventsAsync((Predicate<TimedEvent>)null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((TrackChunk)null).RemoveObjects(ObjectType.Note),
                () => ((TrackChunk)null).RemoveObjectsAsync(ObjectType.Note, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((EventsCollection)null).ManageNotes(),
                () => ((EventsCollection)null).ManageNotesAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((TrackChunk)null).ManageChords(),
                () => ((TrackChunk)null).ManageChordsAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => ((TrackChunk)null).ManageTimedEvents(),
                () => ((TrackChunk)null).ManageTimedEventsAsync(cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => new TimedObjectsManager<Note>(null),
                () => TimedObjectsManager<Note>.CreateAsync(null, cancellationToken: CancellationToken.None).Wait());
            CheckArgument(
                () => new TempoMapManager(null, new[] { new EventsCollection() }),
                () => TempoMapManager.CreateAsync(null, new[] { new EventsCollection() }, cancellationToken: CancellationToken.None).Wait());

            Assert.Throws<InvalidOperationException>(() => new TimedObjectsManager<ITimedObject>(new EventsCollection()), "Synchronous manager accepted an unsupported object type.");

            var unsupportedType = Assert.Throws<AggregateException>(() =>
                TimedObjectsManager<ITimedObject>.CreateAsync(
                    new EventsCollection(),
                    cancellationToken: CancellationToken.None).Wait(), "Async manager accepted an unsupported object type.");

            ClassicAssert.IsInstanceOf<InvalidOperationException>(unsupportedType.InnerException, "Async unsupported-type error differs.");
        }
    }
}
