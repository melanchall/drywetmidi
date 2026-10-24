using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class ToolsAsyncTests
    {
        [Test]
        public async Task Splitter_LazyEnumeration_HasBoundedReadAhead_AndDisposes()
        {
            int visited = 0;
            bool disposed = false;
            IEnumerable<ITimedObject> Source()
            {
                try
                {
                    while (true)
                    {
                        visited++;
                        yield return new Note((SevenBitNumber)60, 100);
                    }
                }
                finally
                {
                    disposed = true;
                }
            }

            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default);

            ClassicAssert.AreEqual(0, visited, "Lazy splitting enumerated the source before the first move.");

            await foreach (var obj in result)
            {
                ClassicAssert.AreEqual(10, ((Note)obj).Length, "Lazy splitting returned an invalid note length.");
                ClassicAssert.GreaterOrEqual(visited, 1, "Lazy splitting did not visit the source.");
                ClassicAssert.LessOrEqual(visited, 64, "Lazy splitting exceeded the read-ahead bound.");
                break;
            }

            ClassicAssert.IsTrue(disposed, "Early exit did not dispose the splitting source.");
            ClassicAssert.GreaterOrEqual(visited, 1, "Lazy splitting did not consume a source item.");
            ClassicAssert.LessOrEqual(visited, 64, "Early exit exceeded the splitting read-ahead bound.");
        }

        [Test]
        public async Task Splitter_EnumeratorCancellation_IsObserved()
        {
            using var cancellation = new CancellationTokenSource();
            bool disposed = false;
            IEnumerable<ITimedObject> Source()
            {
                try
                {
                    while (true)
                    {
                        yield return new Note((SevenBitNumber)60, 100);
                    }
                }
                finally
                {
                    disposed = true;
                }
            }

            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default);
            var enumerator = result.GetAsyncEnumerator(cancellation.Token);
            await using var enumeratorDisposal = enumerator.ConfigureAwait(false);

            ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "Splitting returned no item before cancellation.");

            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync(), "Splitting enumerator did not observe cancellation.");
            ClassicAssert.IsTrue(disposed, "Canceled splitting did not dispose the source.");
        }

        [Test]
        public async Task Splitter_BatchedEnumeration_ReportsOneCompletion()
        {
            var progress = new InlineProgress();
            var objects = Enumerable.Range(0, 200).Select(i => (ITimedObject)new Note((SevenBitNumber)60, 0, i));

            var result = await Collect(objects.SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default, progress: progress));

            ClassicAssert.AreEqual(200, result.Count, "Batched splitting returned an invalid item count.");
            ClassicAssert.AreEqual(1, progress.Reports.Count(report => report.IsCompleted), "Batched splitting did not report exactly one completion.");
            ClassicAssert.GreaterOrEqual(progress.Reports.Last().ProcessedItems, 200, "Batched splitting progress omitted processed items.");
            Assert.That(progress.Reports.Select(report => report.ProcessedItems), Is.Ordered, "Batched splitting progress moved backwards.");
        }

        [TestCase(1)]
        [TestCase(63)]
        [TestCase(64)]
        [TestCase(65)]
        public async Task Splitter_BatchedEnumeration_PreservesValidPrefixBeforeFault(int validItems)
        {
            bool disposed = false;
            var failure = new InvalidOperationException("Deferred source failure.");
            IEnumerable<ITimedObject> Source()
            {
                try
                {
                    for (int i = 0; i < validItems; i++)
                    {
                        yield return new TimedEvent(new TextEvent(i.ToString()), i);
                    }

                    throw failure;
                }
                finally
                {
                    disposed = true;
                }
            }

            var progress = new InlineProgress();
            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default, progress: progress);
            var enumerator = result.GetAsyncEnumerator();
            await using var enumeratorDisposal = enumerator.ConfigureAwait(false);

            for (int i = 0; i < validItems; i++)
            {
                ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), $"Splitting omitted valid prefix item {i}.");
                ClassicAssert.AreEqual(i.ToString(), ((TextEvent)((TimedEvent)enumerator.Current).Event).Text, $"Splitting changed valid prefix item {i}.");
            }

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync(), "Splitting did not propagate the deferred source failure.");

            ClassicAssert.AreSame(failure, exception, "Splitting replaced the source exception.");
            ClassicAssert.IsTrue(disposed, "Failed splitting did not dispose the source.");
            ClassicAssert.IsFalse(progress.Reports.Any(report => report.IsCompleted), "Failed splitting reported completion.");
        }

        [Test]
        public async Task Splitter_AllLazyFamilies_MatchSync()
        {
            var objects = CreateFile().GetNotes().Cast<ITimedObject>().ToArray();
            var grid = new SteppedGrid((MidiTimeSpan)40);

            ClassicAssert.AreEqual(objects.SplitObjectsByStep((MidiTimeSpan)40, TempoMap.Default).Count(),
                (await Collect(objects.SplitObjectsByStepAsync((MidiTimeSpan)40, TempoMap.Default))).Count, "Async step splitting count differs.");
            ClassicAssert.AreEqual(objects.SplitObjectsByGrid(grid, TempoMap.Default).Count(),
                (await Collect(objects.SplitObjectsByGridAsync(grid, TempoMap.Default))).Count, "Async grid splitting count differs.");
            ClassicAssert.AreEqual(objects.SplitObjectsByPartsNumber(2, TimeSpanType.Midi, TempoMap.Default).Count(),
                (await Collect(objects.SplitObjectsByPartsNumberAsync(2, TimeSpanType.Midi, TempoMap.Default))).Count, "Async parts-number splitting count differs.");
            ClassicAssert.AreEqual(objects.SplitObjectsAtDistance((MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default).Count(),
                (await Collect(objects.SplitObjectsAtDistanceAsync((MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default))).Count, "Async absolute-distance splitting count differs.");
            ClassicAssert.AreEqual(objects.SplitObjectsAtDistance(0.5, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default).Count(),
                (await Collect(objects.SplitObjectsAtDistanceAsync(0.5, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default))).Count, "Async relative-distance splitting count differs.");

            var file = CreateFile();

            ClassicAssert.AreEqual(file.SplitByChunks().Count(), (await Collect(file.SplitByChunksAsync())).Count, "Async chunk splitting count differs.");
            ClassicAssert.AreEqual(file.SplitByChannel().Count(), (await Collect(file.SplitByChannelAsync())).Count, "Async channel splitting count differs.");
            ClassicAssert.AreEqual(file.SplitByGrid(grid).Count(), (await Collect(file.SplitByGridAsync(grid))).Count, "Async file grid splitting count differs.");
            ClassicAssert.AreEqual(file.SplitByObjects(ObjectType.Note).Count(), (await Collect(file.SplitByObjectsAsync(ObjectType.Note))).Count, "Async file object splitting count differs.");
        }

        [Test]
        public void Cpu_CancellationCheckpoints_StopLongSplitting()
        {
            using var cancellation = new CancellationTokenSource();
            var objects = Enumerable.Range(0, 100).Select(i => (ITimedObject)new Note((SevenBitNumber)60, 100)).ToArray();
            int calls = 0;
            Predicate<ITimedObject> filter = obj =>
            {
                if (++calls == 4)
                    cancellation.Cancel();

                return true;
            };

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await Collect(objects.SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default, filter, cancellationToken: cancellation.Token)), "Long splitting did not observe cancellation.");
            ClassicAssert.AreEqual(4, calls, "Splitting invoked the filter after cancellation.");
            ClassicAssert.AreEqual(1000, objects.SplitObjectsByStep((MidiTimeSpan)10, TempoMap.Default, filter).Count(), "Cancellation damaged subsequent synchronous splitting.");
        }

        [Test]
        public async Task Splitter_MutatingOverloads_MatchSync(
            [Values(0, 1, 2)] int target,
            [Values(0, 1, 2, 3, 4)] int operation)
        {
            var actual = CreateFile();
            var expected = CreateFile();
            var grid = new SteppedGrid((MidiTimeSpan)40);
            var track = actual.GetTrackChunks().First();
            var tracks = actual.GetTrackChunks();

            switch (operation)
            {
                case 0:
                    expected.SplitObjectsByStep(ObjectType.Note, (MidiTimeSpan)40);
                    if (target == 0)
                        await actual.SplitObjectsByStepAsync(ObjectType.Note, (MidiTimeSpan)40);
                    else if (target == 1)
                        await track.SplitObjectsByStepAsync(ObjectType.Note, (MidiTimeSpan)40, TempoMap.Default);
                    else
                        await tracks.SplitObjectsByStepAsync(ObjectType.Note, (MidiTimeSpan)40, TempoMap.Default);

                    break;
                case 1:
                    expected.SplitObjectsByGrid(ObjectType.Note, grid);
                    if (target == 0)
                        await actual.SplitObjectsByGridAsync(ObjectType.Note, grid);
                    else if (target == 1)
                        await track.SplitObjectsByGridAsync(ObjectType.Note, grid, TempoMap.Default);
                    else
                        await tracks.SplitObjectsByGridAsync(ObjectType.Note, grid, TempoMap.Default);

                    break;
                case 2:
                    expected.SplitObjectsByPartsNumber(ObjectType.Note, 3, TimeSpanType.Midi);
                    if (target == 0)
                        await actual.SplitObjectsByPartsNumberAsync(ObjectType.Note, 3, TimeSpanType.Midi);
                    else if (target == 1)
                        await track.SplitObjectsByPartsNumberAsync(ObjectType.Note, 3, TimeSpanType.Midi, TempoMap.Default);
                    else
                        await tracks.SplitObjectsByPartsNumberAsync(ObjectType.Note, 3, TimeSpanType.Midi, TempoMap.Default);

                    break;
                case 3:
                    expected.SplitObjectsAtDistance(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start);
                    if (target == 0)
                        await actual.SplitObjectsAtDistanceAsync(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start);
                    else if (target == 1)
                        await track.SplitObjectsAtDistanceAsync(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default);
                    else
                        await tracks.SplitObjectsAtDistanceAsync(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default);

                    break;
                default:
                    expected.SplitObjectsAtDistance(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start);
                    if (target == 0)
                        await actual.SplitObjectsAtDistanceAsync(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start);
                    else if (target == 1)
                        await track.SplitObjectsAtDistanceAsync(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default);
                    else
                        await tracks.SplitObjectsAtDistanceAsync(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default);

                    break;
            }

            ClassicAssert.AreEqual(expected.GetNotes().Select(n => (n.Time, n.Length)), actual.GetNotes().Select(n => (n.Time, n.Length)), "Async mutating splitting note times or lengths differ.");
        }
    }
}
