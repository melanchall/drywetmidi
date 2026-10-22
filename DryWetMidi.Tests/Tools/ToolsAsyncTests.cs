using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class ToolsAsyncTests
    {
        private sealed class AsyncOnlyStream : Stream
        {
            private readonly MemoryStream _storage;
            internal int Reads;
            internal int Writes;
            internal int Flushes;
            internal bool FailRead;
            internal bool FailWrite;
            internal bool BlockRead;
            internal bool BlockWrite;

            internal AsyncOnlyStream(byte[] bytes = null) => _storage = bytes == null ? new MemoryStream() : new MemoryStream(bytes);
            internal byte[] ToArray() => _storage.ToArray();
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Flush() => throw new InvalidOperationException("Synchronous flush.");
            public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous read.");
            public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous write.");
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                Reads++;
                await Task.Yield();
                if (BlockRead) await Task.Delay(Timeout.Infinite, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (FailRead) throw new IOException("Read failure.");
                return await _storage.ReadAsync(buffer, offset, Math.Min(count, 17), cancellationToken);
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                Writes++;
                await Task.Yield();
                if (BlockWrite) await Task.Delay(Timeout.Infinite, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (FailWrite) throw new IOException("Write failure.");
                await _storage.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                Flushes++;
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }
        }

        private sealed class CustomRepeater : Repeater
        {
            internal int Calls;
            protected override void ProcessPart(PartProcessingContext context)
            {
                Calls++;
                base.ProcessPart(context);
            }
        }

        private sealed class InlineProgress : IProgress<AsyncOperationProgress>
        {
            internal readonly List<AsyncOperationProgress> Reports = new List<AsyncOperationProgress>();
            public void Report(AsyncOperationProgress value) => Reports.Add(value);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task Csv_StreamOverloads_OnlyUseAsyncIo(int family)
        {
            var track = CreateFile().GetTrackChunks().First();
            var file = new MidiFile(track);
            using var expected = new MemoryStream();
            using var output = new AsyncOnlyStream();
            switch (family)
            {
                case 0:
                    file.SerializeToCsv(expected);
                    await file.SerializeToCsvAsync(output);
                    break;
                case 1:
                    file.Chunks.SerializeToCsv(expected, TempoMap.Default);
                    await file.Chunks.SerializeToCsvAsync(output, TempoMap.Default);
                    break;
                case 2:
                    track.SerializeToCsv(expected, TempoMap.Default);
                    await track.SerializeToCsvAsync(output, TempoMap.Default);
                    break;
                default:
                    track.GetObjects(ObjectType.Note | ObjectType.TimedEvent).SerializeToCsv(expected, TempoMap.Default);
                    await track.GetObjects(ObjectType.Note | ObjectType.TimedEvent).SerializeToCsvAsync(output, TempoMap.Default);
                    break;
            }
            Assert.That(output.ToArray(), Is.EqualTo(expected.ToArray()));
            Assert.That(output.Writes, Is.GreaterThan(0));
            Assert.That(output.Flushes, Is.EqualTo(1));

            using var input = new AsyncOnlyStream(output.ToArray());
            switch (family)
            {
                case 0:
                    var restoredFile = await CsvSerializer.DeserializeFileFromCsvAsync(input);
                    Assert.That(restoredFile.GetNotes().Count, Is.EqualTo(file.GetNotes().Count));
                    break;
                case 1:
                    var chunks = await CsvSerializer.DeserializeChunksFromCsvAsync(input, TempoMap.Default);
                    Assert.That(chunks.Count, Is.EqualTo(file.Chunks.Count));
                    break;
                case 2:
                    Assert.That(await CsvSerializer.DeserializeChunkFromCsvAsync(input, TempoMap.Default), Is.TypeOf<TrackChunk>());
                    break;
                default:
                    var objects = await CsvSerializer.DeserializeObjectsFromCsvAsync(input, TempoMap.Default);
                    Assert.That(objects.OfType<Note>().Count(), Is.EqualTo(track.GetNotes().Count));
                    break;
            }
            Assert.That(input.Reads, Is.GreaterThan(1));
            Assert.That(input.CanRead, Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task Csv_PathOverloads_RoundTrip(int family)
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tools-async-" + Guid.NewGuid() + ".csv");
            var file = CreateFile();
            var track = file.GetTrackChunks().First();
            try
            {
                switch (family)
                {
                    case 0:
                        await file.SerializeToCsvAsync(path, false);
                        Assert.That((await CsvSerializer.DeserializeFileFromCsvAsync(path)).GetNotes().Count, Is.EqualTo(file.GetNotes().Count));
                        break;
                    case 1:
                        await file.Chunks.SerializeToCsvAsync(path, false, TempoMap.Default);
                        Assert.That((await CsvSerializer.DeserializeChunksFromCsvAsync(path, TempoMap.Default)).Count, Is.EqualTo(file.Chunks.Count));
                        break;
                    case 2:
                        await track.SerializeToCsvAsync(path, false, TempoMap.Default);
                        Assert.That(await CsvSerializer.DeserializeChunkFromCsvAsync(path, TempoMap.Default), Is.TypeOf<TrackChunk>());
                        break;
                    default:
                        await file.GetNotes().SerializeToCsvAsync(path, false, TempoMap.Default);
                        Assert.That((await CsvSerializer.DeserializeObjectsFromCsvAsync(path, TempoMap.Default)).Count, Is.EqualTo(file.GetNotes().Count));
                        break;
                }
                Assert.ThrowsAsync<IOException>(() => file.SerializeToCsvAsync(path, false));
                await file.SerializeToCsvAsync(path, true);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Test]
        public async Task Csv_LargeInput_IsIncremental()
        {
            var file = new MidiFile(new TrackChunk(Enumerable.Range(0, 3000).Select(i => new TextEvent("line\n\"quoted\",é" + i) { DeltaTime = i % 2 })));
            using var output = new AsyncOnlyStream();
            await file.SerializeToCsvAsync(output);
            Assert.That(output.Writes, Is.GreaterThan(1));
            using var input = new AsyncOnlyStream(output.ToArray());
            var restored = await CsvSerializer.DeserializeFileFromCsvAsync(input);
            Assert.That(restored.GetTrackChunks().First().Events.Count, Is.EqualTo(3000));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Csv_IoErrors_PropagateWithoutDeadlock(bool writing)
        {
            using var stream = new AsyncOnlyStream { FailRead = !writing, FailWrite = writing };
            Assert.ThrowsAsync<IOException>(async () =>
            {
                if (writing) await CreateFile().SerializeToCsvAsync(stream).WaitAsync(TimeSpan.FromSeconds(10));
                else await CsvSerializer.DeserializeFileFromCsvAsync(stream).WaitAsync(TimeSpan.FromSeconds(10));
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Csv_Cancellation_InterruptsPendingIo(bool writing)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            using var stream = new AsyncOnlyStream { BlockRead = !writing, BlockWrite = writing };
            Assert.CatchAsync<OperationCanceledException>(async () =>
            {
                if (writing) await CreateFile().SerializeToCsvAsync(stream, cancellationToken: cancellation.Token).WaitAsync(TimeSpan.FromSeconds(10));
                else await CsvSerializer.DeserializeFileFromCsvAsync(stream, cancellationToken: cancellation.Token).WaitAsync(TimeSpan.FromSeconds(10));
            });
        }

        [Test]
        public async Task Splitter_LazyEnumeration_DoesNotReadAhead_AndDisposes()
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
                finally { disposed = true; }
            }
            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default);
            Assert.That(visited, Is.Zero);
            await foreach (var obj in result)
            {
                Assert.That(((Note)obj).Length, Is.EqualTo(10));
                Assert.That(visited, Is.EqualTo(1));
                break;
            }
            Assert.That(disposed, Is.True);
            Assert.That(visited, Is.EqualTo(1));
        }

        [Test]
        public async Task Splitter_EnumeratorCancellation_IsObserved()
        {
            using var cancellation = new CancellationTokenSource();
            var result = new ITimedObject[] { new Note((SevenBitNumber)60, 100) }
                .SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default);
            await using var enumerator = result.GetAsyncEnumerator(cancellation.Token);
            Assert.That(await enumerator.MoveNextAsync(), Is.True);
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
        }

        [Test]
        public async Task Splitter_AllLazyFamilies_MatchSync()
        {
            var objects = CreateFile().GetNotes().Cast<ITimedObject>().ToArray();
            var grid = new SteppedGrid((MidiTimeSpan)40);
            Assert.That((await Collect(objects.SplitObjectsByStepAsync((MidiTimeSpan)40, TempoMap.Default))).Count,
                Is.EqualTo(objects.SplitObjectsByStep((MidiTimeSpan)40, TempoMap.Default).Count()));
            Assert.That((await Collect(objects.SplitObjectsByGridAsync(grid, TempoMap.Default))).Count,
                Is.EqualTo(objects.SplitObjectsByGrid(grid, TempoMap.Default).Count()));
            Assert.That((await Collect(objects.SplitObjectsByPartsNumberAsync(2, TimeSpanType.Midi, TempoMap.Default))).Count,
                Is.EqualTo(objects.SplitObjectsByPartsNumber(2, TimeSpanType.Midi, TempoMap.Default).Count()));
            Assert.That((await Collect(objects.SplitObjectsAtDistanceAsync((MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default))).Count,
                Is.EqualTo(objects.SplitObjectsAtDistance((MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default).Count()));
            Assert.That((await Collect(objects.SplitObjectsAtDistanceAsync(0.5, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default))).Count,
                Is.EqualTo(objects.SplitObjectsAtDistance(0.5, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default).Count()));
            var file = CreateFile();
            Assert.That((await Collect(file.SplitByChunksAsync())).Count, Is.EqualTo(file.SplitByChunks().Count()));
            Assert.That((await Collect(file.SplitByChannelAsync())).Count, Is.EqualTo(file.SplitByChannel().Count()));
            Assert.That((await Collect(file.SplitByGridAsync(grid))).Count, Is.EqualTo(file.SplitByGrid(grid).Count()));
            Assert.That((await Collect(file.SplitByObjectsAsync(ObjectType.Note))).Count, Is.EqualTo(file.SplitByObjects(ObjectType.Note).Count()));
        }

        [Test]
        public async Task CpuFamilies_ResultsAndHooksMatchSync()
        {
            var file = CreateFile();
            Assert.That((await file.TakePartAsync((MidiTimeSpan)50)).GetNotes().Count, Is.EqualTo(file.TakePart((MidiTimeSpan)50).GetNotes().Count));
            Assert.That((await file.TakePartAsync((MidiTimeSpan)10, (MidiTimeSpan)50)).GetNotes().Count, Is.EqualTo(file.TakePart((MidiTimeSpan)10, (MidiTimeSpan)50).GetNotes().Count));
            Assert.That((await file.SkipPartAsync((MidiTimeSpan)50)).GetNotes().Count, Is.EqualTo(file.SkipPart((MidiTimeSpan)50).GetNotes().Count));
            Assert.That((await file.CutPartAsync((MidiTimeSpan)10, (MidiTimeSpan)50)).GetNotes().Count, Is.EqualTo(file.CutPart((MidiTimeSpan)10, (MidiTimeSpan)50).GetNotes().Count));
            var files = new[] { file, file };
            Assert.That((await files.MergeSequentiallyAsync()).GetNotes().Count, Is.EqualTo(files.MergeSequentially().GetNotes().Count));
            Assert.That((await files.MergeSimultaneouslyAsync()).GetNotes().Count, Is.EqualTo(files.MergeSimultaneously().GetNotes().Count));
            Assert.That((await Collect(file.GetNotes().MergeObjectsAsync(TempoMap.Default))).Count, Is.EqualTo(file.GetNotes().MergeObjects(TempoMap.Default).Count()));

            var repeater = new CustomRepeater();
            var repeated = await repeater.RepeatAsync(file.GetNotes(), 2, TempoMap.Default);
            Assert.That(repeated.Count, Is.EqualTo(file.GetNotes().Repeat(2, TempoMap.Default).Count));
            Assert.That(repeater.Calls, Is.GreaterThan(0));

            var progress = new InlineProgress();
            await file.ResizeAsync(2.0, progress);
            Assert.That(progress.Reports.Count, Is.GreaterThan(0));
            await file.GetNotes().ResizeObjectsGroupAsync(0.5, TempoMap.Default);
            await file.QuantizeObjectsAsync(ObjectType.Note, new SteppedGrid((MidiTimeSpan)10));
            await file.SanitizeAsync();
        }

        [Test]
        public void Cpu_CancellationCheckpoints_StopLongSplitting()
        {
            using var cancellation = new CancellationTokenSource();
            var objects = Enumerable.Range(0, 100).Select(i => (ITimedObject)new Note((SevenBitNumber)60, 100)).ToArray();
            int calls = 0;
            Predicate<ITimedObject> filter = obj =>
            {
                if (++calls == 4) cancellation.Cancel();
                return true;
            };
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await Collect(objects.SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default, filter, cancellationToken: cancellation.Token)));
            Assert.That(calls, Is.EqualTo(4));
            Assert.That(objects.SplitObjectsByStep((MidiTimeSpan)10, TempoMap.Default, filter).Count(), Is.EqualTo(1000));
        }

        [Test]
        public void Cpu_PreCanceledOperations_DoNotMutate()
        {
            var file = CreateFile();
            var originalLength = file.GetNotes().First().Length;
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(() => file.ResizeAsync(2.0, cancellationToken: cancellation.Token));
            Assert.CatchAsync<OperationCanceledException>(() => file.RepeatAsync(2, cancellationToken: cancellation.Token));
            Assert.CatchAsync<OperationCanceledException>(() => file.SanitizeAsync(cancellationToken: cancellation.Token));
            Assert.That(file.GetNotes().First().Length, Is.EqualTo(originalLength));
        }

        private static MidiFile CreateFile() => new MidiFile(new TrackChunk(
            new SequenceTrackNameEvent("quoted \"track\"\nsecond line"),
            new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100),
            new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)64) { DeltaTime = 100 },
            new NoteOnEvent((SevenBitNumber)62, (SevenBitNumber)90) { DeltaTime = 20 },
            new NoteOffEvent((SevenBitNumber)62, (SevenBitNumber)64) { DeltaTime = 100 }));

        private static async Task<List<T>> Collect<T>(IAsyncEnumerable<T> source)
        {
            var result = new List<T>();
            await foreach (var item in source) result.Add(item);
            return result;
        }
    }

}
