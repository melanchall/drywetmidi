using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public sealed partial class GetObjectsUtilitiesTests
    {
        public enum AsyncConstructorKind
        {
            Default,
            Custom,
            Null
        }

        public enum AsyncDetectionStage
        {
            TimedEvent,
            Note,
            Chord
        }

        private sealed class AsyncDetectedTimedEvent : TimedEvent
        {
            public AsyncDetectedTimedEvent(TimedEventData data)
                : base(data.Event, data.Time)
            {
            }

            public override bool Equals(object obj) => obj is AsyncDetectedTimedEvent;

            public override int GetHashCode() => typeof(AsyncDetectedTimedEvent).GetHashCode();
        }

        private sealed class AsyncDetectedNote : Note
        {
            public AsyncDetectedNote(NoteData data)
                : base(data.TimedNoteOnEvent, data.TimedNoteOffEvent)
            {
            }

            public override bool Equals(object obj) => obj is AsyncDetectedNote;

            public override int GetHashCode() => typeof(AsyncDetectedNote).GetHashCode();
        }

        private sealed class AsyncDetectedChord : Chord
        {
            public AsyncDetectedChord(ChordData data)
                : base(data.Notes)
            {
            }

            public override bool Equals(object obj) => obj is AsyncDetectedChord;

            public override int GetHashCode() => typeof(AsyncDetectedChord).GetHashCode();
        }

        private sealed class AsyncObjectEventsSource : IAsyncEnumerable<MidiEvent>
        {
            private readonly MidiEvent[] _events;

            public AsyncObjectEventsSource(params MidiEvent[] events)
            {
                _events = events;
            }

            public TaskCompletionSource<bool> GateEntered { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            public TaskCompletionSource<bool> ReleaseGate { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            public int GateIndex { get; set; } = -1;

            public Exception Failure { get; set; }

            public Action BeforeCompletion { get; set; }

            public int EnumeratorCount { get; private set; }

            public int MoveNextCount { get; private set; }

            public bool IsDisposed { get; private set; }

            public CancellationToken ObservedCancellationToken { get; private set; }

            public IAsyncEnumerator<MidiEvent> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                EnumeratorCount++;

                return Read(cancellationToken).GetAsyncEnumerator(cancellationToken);
            }

            private async IAsyncEnumerable<MidiEvent> Read(
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                ObservedCancellationToken = cancellationToken;

                try
                {
                    for (var index = 0; index <= _events.Length; index++)
                    {
                        MoveNextCount++;
                        cancellationToken.ThrowIfCancellationRequested();

                        if (index == GateIndex)
                        {
                            GateEntered.TrySetResult(true);
                            await ReleaseGate.Task.WaitAsync(cancellationToken);
                        }

                        if (index == _events.Length)
                        {
                            if (Failure != null)
                                throw Failure;

                            BeforeCompletion?.Invoke();

                            yield break;
                        }

                        yield return _events[index];
                    }
                }
                finally
                {
                    IsDisposed = true;
                }
            }
        }

        [Test]
        public async Task EnumerateObjectsAsync_MatchesSynchronousDetection(
            [Values(
                (ObjectType)0,
                ObjectType.TimedEvent,
                ObjectType.Note,
                ObjectType.Chord,
                ObjectType.TimedEvent | ObjectType.Note,
                ObjectType.TimedEvent | ObjectType.Chord,
                ObjectType.Note | ObjectType.Chord,
                ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord)] ObjectType objectType,
            [Values] NoteStartDetectionPolicy noteStartPolicy,
            [Values(0L, 5L, 20L)] long notesTolerance,
            [Values(1, 2, 3)] int notesMinCount,
            [Values] AsyncConstructorKind constructorKind)
        {
            var inputEvents = CreateAsyncDetectionEvents();
            var synchronousObjects = inputEvents
                .Select(midiEvent => midiEvent?.Clone())
                .EnumerateObjects(
                    objectType,
                    CreateAsyncDetectionSettings(noteStartPolicy, notesTolerance, notesMinCount, constructorKind))
                .ToArray();
            var source = new AsyncObjectEventsSource(inputEvents) { GateIndex = 0 };
            var enumeration = source.EnumerateObjectsAsync(
                objectType,
                CreateAsyncDetectionSettings(noteStartPolicy, notesTolerance, notesMinCount, constructorKind));

            var objectsTask = CollectAsyncDetectedObjects(enumeration);
            await source.GateEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));

            ClassicAssert.IsFalse(objectsTask.IsCompleted, "Object detection did not await the source.");

            source.ReleaseGate.SetResult(true);
            var asynchronousObjects = await objectsTask;

            MidiAsserts.AreEqual(
                synchronousObjects,
                asynchronousObjects,
                true,
                0,
                "Asynchronous detection differs from synchronous detection.");
            ClassicAssert.IsTrue(source.IsDisposed, "Completed source enumerator was not disposed.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_DefaultSettings(
            [Values(
                ObjectType.TimedEvent,
                ObjectType.Note,
                ObjectType.Chord,
                ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord)] ObjectType objectType,
            [Values(false, true)] bool emptySource)
        {
            var inputEvents = emptySource ? new MidiEvent[] { null, null } : CreateAsyncDetectionEvents();
            var source = new AsyncObjectEventsSource(inputEvents);

            var actualObjects = await CollectAsyncDetectedObjects(source.EnumerateObjectsAsync(objectType));
            var expectedObjects = inputEvents.EnumerateObjects(objectType).ToArray();

            MidiAsserts.AreEqual(expectedObjects, actualObjects, true, 0, "Default asynchronous detection is invalid.");
            ClassicAssert.IsTrue(source.IsDisposed, "Default-settings source was not disposed.");
        }

        [Test]
        public void EnumerateObjectsAsync_NullSourceIsRejectedEagerly()
        {
            IAsyncEnumerable<MidiEvent> source = null;

            var exception = ClassicAssert.Throws<ArgumentNullException>(
                () => source.EnumerateObjectsAsync(ObjectType.Note),
                "Null asynchronous event source was accepted.");

            ClassicAssert.AreEqual("midiEvents", exception.ParamName, "Null-source parameter name is invalid.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_TimedEventsAreDemandDrivenAndCloned()
        {
            var inputEvents = new MidiEvent[]
            {
                null,
                new ProgramChangeEvent((SevenBitNumber)12) { DeltaTime = 7 },
                new TextEvent("untouched") { DeltaTime = 3 }
            };
            var source = new AsyncObjectEventsSource(inputEvents);
            var enumeration = source.EnumerateObjectsAsync(ObjectType.TimedEvent);

            ClassicAssert.AreEqual(0, source.EnumeratorCount, "Creating detection eagerly acquired the source.");

            var enumerator = enumeration.GetAsyncEnumerator();

            ClassicAssert.AreEqual(0, source.MoveNextCount, "Acquiring detection eagerly consumed the source.");

            try
            {
                ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The first timed event was not emitted.");
                var timedEvent = (TimedEvent)enumerator.Current;

                ClassicAssert.AreEqual(2, source.MoveNextCount, "Timed-event detection read ahead.");
                ClassicAssert.AreEqual(7, timedEvent.Time, "Null events changed the accumulated time.");
                ClassicAssert.AreNotSame(inputEvents[1], timedEvent.Event, "Source event was not cloned.");
                MidiAsserts.AreEqual(inputEvents[1], timedEvent.Event, true, "Cloned event differs from its source.");

                ((ProgramChangeEvent)timedEvent.Event).ProgramNumber = (SevenBitNumber)30;

                ClassicAssert.AreEqual(
                    (SevenBitNumber)12,
                    ((ProgramChangeEvent)inputEvents[1]).ProgramNumber,
                    "Mutating a detected event changed its source.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Early disposal did not dispose the source enumerator.");
            ClassicAssert.AreEqual(2, source.MoveNextCount, "Disposing detection consumed more source events.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_CompletedNotesAndChordsDoNotRequireEndOfSource(
            [Values(false, true)] bool detectChords)
        {
            var source = new AsyncObjectEventsSource(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90),
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)64, (SevenBitNumber)20),
                new NoteOnEvent((SevenBitNumber)67, (SevenBitNumber)70) { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)67, (SevenBitNumber)30) { DeltaTime = 5 },
                new TextEvent("must not be requested"));
            var expectedObject = detectChords
                ? (ITimedObject)new Chord(
                    new Note((SevenBitNumber)60, 10, 0)
                    {
                        Velocity = (SevenBitNumber)90,
                        OffVelocity = (SevenBitNumber)10
                    },
                    new Note((SevenBitNumber)64, 10, 0)
                    {
                        Velocity = (SevenBitNumber)80,
                        OffVelocity = (SevenBitNumber)20
                    })
                : new Note((SevenBitNumber)60, 10, 0)
                {
                    Velocity = (SevenBitNumber)90,
                    OffVelocity = (SevenBitNumber)10
                };
            var enumerator = source
                .EnumerateObjectsAsync(detectChords ? ObjectType.Chord : ObjectType.Note)
                .GetAsyncEnumerator();

            try
            {
                ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The completed object was not emitted.");

                MidiAsserts.AreEqual(expectedObject, enumerator.Current, "The completed object is invalid.");
                ClassicAssert.AreEqual(
                    detectChords ? 6 : 3,
                    source.MoveNextCount,
                    "Detection consumed events beyond the first completed object.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Early object disposal did not dispose the source.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_ConstructorsReceiveClonesAndEventIndices()
        {
            var inputEvents = CreateAsyncDetectionEvents();
            var constructorData = new List<TimedEventData>();
            var source = new AsyncObjectEventsSource(inputEvents);
            var settings = new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data =>
                    {
                        constructorData.Add(data);
                        return new AsyncDetectedTimedEvent(data);
                    }
                }
            };

            var actualObjects = await CollectAsyncDetectedObjects(
                source.EnumerateObjectsAsync(ObjectType.TimedEvent, settings));
            var nonNullEvents = inputEvents.Where(midiEvent => midiEvent != null).ToArray();

            ClassicAssert.AreEqual(nonNullEvents.Length, constructorData.Count, "Constructor invocation count is invalid.");
            var time = 0L;

            for (var index = 0; index < nonNullEvents.Length; index++)
            {
                time += nonNullEvents[index].DeltaTime;
                var data = constructorData[index];

                ClassicAssert.AreEqual(index, data.EventIndex, "Null elements affected constructor event indices.");
                ClassicAssert.AreEqual(0, data.EventsCollectionIndex, "Constructor received an invalid collection index.");
                ClassicAssert.AreEqual(time, data.Time, "Constructor received an invalid absolute time.");
                ClassicAssert.AreNotSame(nonNullEvents[index], data.Event, "Constructor received a source-owned event.");
                ClassicAssert.AreSame(data.Event, ((TimedEvent)actualObjects[index]).Event, "Constructor output was replaced.");
                MidiAsserts.AreEqual(nonNullEvents[index], data.Event, true, "Constructor clone is invalid.");
            }
        }

        [Test]
        public async Task EnumerateObjectsAsync_OverlappingNotesAndDescriptorCompaction(
            [Values] NoteStartDetectionPolicy noteStartPolicy)
        {
            var inputEvents = new List<MidiEvent>();

            for (var index = 0; index < 300; index++)
            {
                inputEvents.Add(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)80) { DeltaTime = 1 });
                inputEvents.Add(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90));
                inputEvents.Add(new TextEvent(index.ToString()));
                inputEvents.Add(new NoteOnEvent((SevenBitNumber)61, (SevenBitNumber)100));
                inputEvents.Add(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 1 });
                inputEvents.Add(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)20) { DeltaTime = 1 });
                inputEvents.Add(new NoteOffEvent((SevenBitNumber)61, (SevenBitNumber)30) { DeltaTime = 1 });
            }

            var settings = CreateAsyncDetectionSettings(noteStartPolicy, 5, 2, AsyncConstructorKind.Default);
            var expectedObjects = inputEvents
                .EnumerateObjects(ObjectType.Note | ObjectType.TimedEvent, settings)
                .ToArray();
            var source = new AsyncObjectEventsSource(inputEvents.ToArray());

            var actualObjects = await CollectAsyncDetectedObjects(
                source.EnumerateObjectsAsync(ObjectType.Note | ObjectType.TimedEvent, settings));

            MidiAsserts.AreEqual(expectedObjects, actualObjects, true, 0, "Pending note descriptor ordering is invalid.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_CancellationWhileAwaitingDoesNotFlushPendingObjects(
            [Values(false, true)] bool detectChords,
            [Values(false, true)] bool enumeratorToken)
        {
            using var cancellation = new CancellationTokenSource();
            var source = new AsyncObjectEventsSource(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new TextEvent("pending"))
            {
                GateIndex = 4
            };
            var enumeration = source.EnumerateObjectsAsync(
                detectChords ? ObjectType.Chord | ObjectType.TimedEvent : ObjectType.Note | ObjectType.TimedEvent,
                cancellationToken: enumeratorToken ? CancellationToken.None : cancellation.Token);
            var enumerator = enumeration.GetAsyncEnumerator(
                enumeratorToken ? cancellation.Token : CancellationToken.None);

            try
            {
                if (!detectChords)
                {
                    ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The completed note was not emitted.");
                }

                var pendingMove = enumerator.MoveNextAsync().AsTask();
                await source.GateEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));

                ClassicAssert.IsFalse(pendingMove.IsCompleted, "Pending detection did not await the source.");
                ClassicAssert.IsTrue(source.ObservedCancellationToken.CanBeCanceled, "Cancellation was not passed upstream.");

                cancellation.Cancel();

                ClassicAssert.CatchAsync<OperationCanceledException>(
                    async () => await pendingMove,
                    "Cancellation flushed pending objects instead of canceling detection.");
                ClassicAssert.IsFalse(await enumerator.MoveNextAsync(), "Canceled detection resumed with pending objects.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Canceled detection did not dispose its source.");
        }

        [Test]
        public void EnumerateObjectsAsync_PreCanceledDoesNotConsumeSource()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var source = new AsyncObjectEventsSource(new NoteOnEvent());
            var enumeration = source.EnumerateObjectsAsync(
                ObjectType.Note,
                cancellationToken: cancellation.Token);

            ClassicAssert.CatchAsync<OperationCanceledException>(
                async () => await CollectAsyncDetectedObjects(enumeration),
                "Pre-canceled detection did not cancel.");

            ClassicAssert.AreEqual(0, source.MoveNextCount, "Pre-canceled detection consumed the source.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_CancellationAtSourceCompletionDoesNotFlushPendingObjects(
            [Values(false, true)] bool detectChords)
        {
            using var cancellation = new CancellationTokenSource();
            var source = new AsyncObjectEventsSource(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new TextEvent("pending"))
            {
                BeforeCompletion = cancellation.Cancel
            };
            var enumerator = source.EnumerateObjectsAsync(
                detectChords ? ObjectType.Chord | ObjectType.TimedEvent : ObjectType.Note | ObjectType.TimedEvent,
                cancellationToken: cancellation.Token)
                .GetAsyncEnumerator();

            try
            {
                if (!detectChords)
                {
                    ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The completed note was not emitted.");
                }

                ClassicAssert.CatchAsync<OperationCanceledException>(
                    async () => await enumerator.MoveNextAsync(),
                    "Cancellation at source completion was treated as a normal end of source.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Cancellation at source completion did not dispose its source.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_CancellationStopsBufferedOutput(
            [Values(false, true)] bool detectChords)
        {
            using var cancellation = new CancellationTokenSource();
            var source = new AsyncObjectEventsSource(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90),
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new NoteOffEvent((SevenBitNumber)64, (SevenBitNumber)20) { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)67, (SevenBitNumber)70) { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)67, (SevenBitNumber)30) { DeltaTime = 5 });
            var settings = new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings { NotesMinCount = 1 }
            };
            var enumerator = source.EnumerateObjectsAsync(
                detectChords ? ObjectType.Chord : ObjectType.Note,
                settings,
                cancellation.Token)
                .GetAsyncEnumerator();

            try
            {
                ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The first object was not emitted.");
                var consumedEventCount = source.MoveNextCount;

                cancellation.Cancel();

                ClassicAssert.CatchAsync<OperationCanceledException>(
                    async () => await enumerator.MoveNextAsync(),
                    "Cancellation did not stop already buffered output.");
                ClassicAssert.AreEqual(
                    consumedEventCount,
                    source.MoveNextCount,
                    "Canceled buffered output consumed additional source events.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Canceled buffered output did not dispose its source.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_CombinesMethodAndEnumeratorCancellationTokens(
            [Values(false, true)] bool cancelEnumeratorToken)
        {
            using var methodCancellation = new CancellationTokenSource();
            using var enumeratorCancellation = new CancellationTokenSource();
            var source = new AsyncObjectEventsSource(new ProgramChangeEvent()) { GateIndex = 0 };
            var enumerator = source
                .EnumerateObjectsAsync(ObjectType.TimedEvent, cancellationToken: methodCancellation.Token)
                .GetAsyncEnumerator(enumeratorCancellation.Token);

            try
            {
                var pendingMove = enumerator.MoveNextAsync().AsTask();
                await source.GateEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));

                if (cancelEnumeratorToken)
                    enumeratorCancellation.Cancel();
                else
                    methodCancellation.Cancel();

                ClassicAssert.CatchAsync<OperationCanceledException>(
                    async () => await pendingMove,
                    "Detection ignored one of its cancellation tokens.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Linked-token cancellation did not dispose its source.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_SourceFailureDoesNotFlushPendingObjects(
            [Values(false, true)] bool detectChords)
        {
            var failure = new InvalidOperationException("Event source failed.");
            var source = new AsyncObjectEventsSource(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new TextEvent("pending"))
            {
                Failure = failure
            };
            var enumerator = source.EnumerateObjectsAsync(
                detectChords ? ObjectType.Chord | ObjectType.TimedEvent : ObjectType.Note | ObjectType.TimedEvent)
                .GetAsyncEnumerator();

            try
            {
                if (!detectChords)
                {
                    ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The completed note was not emitted.");
                }

                var exception = ClassicAssert.ThrowsAsync<InvalidOperationException>(
                    async () => await enumerator.MoveNextAsync(),
                    "Source failure flushed pending objects instead of failing.");

                ClassicAssert.AreSame(failure, exception, "Detection replaced the source exception.");
                ClassicAssert.IsFalse(await enumerator.MoveNextAsync(), "Failed detection resumed with pending objects.");
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ClassicAssert.IsTrue(source.IsDisposed, "Failed detection did not dispose its source.");
        }

        [Test]
        public void EnumerateObjectsAsync_ConstructorFailureDisposesSource(
            [Values] AsyncDetectionStage detectionStage)
        {
            var failure = new InvalidOperationException("Object constructor failed.");
            var source = new AsyncObjectEventsSource(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)10) { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new NoteOffEvent((SevenBitNumber)64, (SevenBitNumber)20) { DeltaTime = 10 });
            var settings = new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings
                {
                    Constructor = data => detectionStage == AsyncDetectionStage.TimedEvent
                        ? throw failure
                        : new TimedEvent(data.Event, data.Time)
                },
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    Constructor = data => detectionStage == AsyncDetectionStage.Note
                        ? throw failure
                        : new Note(data.TimedNoteOnEvent, data.TimedNoteOffEvent)
                },
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 20,
                    Constructor = data => throw failure
                }
            };

            var exception = ClassicAssert.ThrowsAsync<InvalidOperationException>(
                async () => await CollectAsyncDetectedObjects(source.EnumerateObjectsAsync(ObjectType.Chord, settings)),
                "Constructor failure was not propagated.");

            ClassicAssert.AreSame(failure, exception, "Detection replaced the constructor exception.");
            ClassicAssert.IsTrue(source.IsDisposed, "Constructor failure did not dispose its source.");
        }

        private static MidiEvent[] CreateAsyncDetectionEvents()
        {
            return new MidiEvent[]
            {
                null,
                new NoteOffEvent((SevenBitNumber)59, (SevenBitNumber)10),
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90) { DeltaTime = 3 },
                new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)80),
                new ProgramChangeEvent((SevenBitNumber)12) { DeltaTime = 1 },
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)70) { DeltaTime = 1 },
                new NoteOnEvent((SevenBitNumber)65, (SevenBitNumber)60)
                {
                    Channel = (FourBitNumber)1,
                    DeltaTime = 1
                },
                null,
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)20) { DeltaTime = 4 },
                new NoteOffEvent((SevenBitNumber)64, (SevenBitNumber)30) { DeltaTime = 1 },
                new NoteOffEvent((SevenBitNumber)65, (SevenBitNumber)40)
                {
                    Channel = (FourBitNumber)1,
                    DeltaTime = 1
                },
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)50) { DeltaTime = 1 },
                new NoteOffEvent((SevenBitNumber)61, (SevenBitNumber)60) { DeltaTime = 1 },
                new NoteOnEvent((SevenBitNumber)67, (SevenBitNumber)50) { DeltaTime = 86 },
                new NoteOffEvent((SevenBitNumber)67, (SevenBitNumber)70) { DeltaTime = 5 },
                new NoteOnEvent((SevenBitNumber)68, (SevenBitNumber)40) { DeltaTime = 5 },
                new TextEvent("after unmatched note") { DeltaTime = 1 },
                new EndOfTrackEvent() { DeltaTime = 4 }
            };
        }

        private static ObjectDetectionSettings CreateAsyncDetectionSettings(
            NoteStartDetectionPolicy noteStartPolicy,
            long notesTolerance,
            int notesMinCount,
            AsyncConstructorKind constructorKind)
        {
            var settings = new ObjectDetectionSettings
            {
                TimedEventDetectionSettings = new TimedEventDetectionSettings(),
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = noteStartPolicy
                },
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = notesTolerance,
                    NotesMinCount = notesMinCount
                }
            };

            if (constructorKind != AsyncConstructorKind.Default)
            {
                settings.TimedEventDetectionSettings.Constructor = data =>
                    constructorKind == AsyncConstructorKind.Custom ? new AsyncDetectedTimedEvent(data) : null;
                settings.NoteDetectionSettings.Constructor = data =>
                    constructorKind == AsyncConstructorKind.Custom ? new AsyncDetectedNote(data) : null;
                settings.ChordDetectionSettings.Constructor = data =>
                    constructorKind == AsyncConstructorKind.Custom ? new AsyncDetectedChord(data) : null;
            }

            return settings;
        }

        private static async Task<ITimedObject[]> CollectAsyncDetectedObjects(
            IAsyncEnumerable<ITimedObject> timedObjects)
        {
            var result = new List<ITimedObject>();

            await foreach (var timedObject in timedObjects)
            {
                result.Add(timedObject);
            }

            return result.ToArray();
        }
    }
}
