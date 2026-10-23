using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            internal Action OnWrite;
            internal bool Disposed;

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
                OnWrite?.Invoke();
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

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                if (disposing) _storage.Dispose();
                base.Dispose(disposing);
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

        [TestCase(1, false)]
        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(2, true)]
        public void Quantize_CancelsBeforeFiltering(int canceledPass, bool nullItems)
        {
            using var cancellation = new CancellationTokenSource();
            var pass = 0;
            var visited = new int[2];
            IEnumerable<ITimedObject> Objects()
            {
                pass++;
                for (var i = 0; i < 1000; i++)
                {
                    visited[pass - 1]++;
                    if (pass == canceledPass && nullItems)
                        cancellation.Cancel();
                    yield return pass == canceledPass && nullItems ? null : new Note((SevenBitNumber)60);
                }
            }
            var settings = new QuantizingSettings
            {
                Filter = obj =>
                {
                    if (pass != canceledPass)
                        return true;
                    cancellation.Cancel();
                    return false;
                }
            };

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await new Quantizer().QuantizeAsync(Objects(), new SteppedGrid(new MidiTimeSpan(10)),
                    TempoMap.Default, settings, cancellationToken: cancellation.Token));
            Assert.That(visited[canceledPass - 1], Is.LessThanOrEqualTo(2));
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
            Assert.That(input.Disposed, Is.False);
            Assert.That(output.Disposed, Is.False);
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
        public void Csv_InvalidOrCanceledPathWrite_DoesNotCreateFile()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tools-async-" + Guid.NewGuid() + ".csv");
            try
            {
                Assert.ThrowsAsync<ArgumentNullException>(() => ((MidiFile)null).SerializeToCsvAsync(path, true));
                Assert.That(File.Exists(path), Is.False);
                Assert.ThrowsAsync<ArgumentNullException>(() => CreateFile().GetTrackChunks().First().SerializeToCsvAsync(path, true, null));
                Assert.That(File.Exists(path), Is.False);
                using var cancellation = new CancellationTokenSource();
                cancellation.Cancel();
                Assert.CatchAsync<OperationCanceledException>(() => CreateFile().SerializeToCsvAsync(path, true, cancellationToken: cancellation.Token));
                Assert.That(File.Exists(path), Is.False);
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

        [Test]
        public async Task Csv_Serialization_DoesNotStageWholeInput()
        {
            int visited = 0;
            int visitedAtFirstWrite = -1;
            IEnumerable<ITimedObject> Source()
            {
                for (int i = 0; i < 10000; i++)
                {
                    Interlocked.Increment(ref visited);
                    yield return new TimedEvent(new TextEvent("a buffered CSV record"), i);
                }
            }
            using var output = new AsyncOnlyStream
            {
                OnWrite = () =>
                {
                    if (visitedAtFirstWrite == -1)
                        visitedAtFirstWrite = Volatile.Read(ref visited);
                }
            };
            await Source().SerializeToCsvAsync(output, TempoMap.Default);
            Assert.That(visited, Is.EqualTo(10000));
            Assert.That(visitedAtFirstWrite, Is.InRange(1, 9999));
        }

        [Test]
        public void Csv_CodecErrors_PropagateWithoutDeadlock()
        {
            using var input = new AsyncOnlyStream(System.Text.Encoding.UTF8.GetBytes("not a valid CSV MIDI file\n" + new string('x', 20000)));
            Assert.ThrowsAsync<CsvException>(async () =>
                await CsvSerializer.DeserializeFileFromCsvAsync(input).WaitAsync(TimeSpan.FromSeconds(10)));

            IEnumerable<ITimedObject> Source()
            {
                yield return new TimedEvent(new TextEvent("first"));
                throw new InvalidOperationException("Source failure.");
            }
            using var output = new AsyncOnlyStream();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await Source().SerializeToCsvAsync(output, TempoMap.Default).WaitAsync(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public async Task Csv_Progress_CompletesAfterFlush()
        {
            using var output = new AsyncOnlyStream();
            var progress = new InlineProgress();
            await CreateFile().SerializeToCsvAsync(output, progress: progress);
            Assert.That(progress.Reports.Last().IsCompleted, Is.True);
            Assert.That(progress.Reports.Last().ProcessedItems, Is.GreaterThan(0));
            Assert.That(output.Flushes, Is.EqualTo(1));
        }

        [Test]
        public async Task Csv_MultiRecordObjects_AndFormatting_MatchSync()
        {
            var objects = new ITimedObject[]
            {
                new Chord(new[]
                {
                    new Note((SevenBitNumber)60, 100, 10),
                    new Note((SevenBitNumber)64, 80, 10)
                }),
                new PitchBendSensitivityParameter { Time = 30 },
                new TimedEvent(new NormalSysExEvent(new byte[] { 0, 1, 127 }), 40),
                new TimedEvent(new TextEvent("unicode é 🎼\n\"quoted\";value"), 50)
            };
            var settings = new CsvSerializationSettings
            {
                Delimiter = ';',
                BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
                NoteFormat = CsvNoteFormat.Letter,
                TimeType = TimeSpanType.Metric,
                LengthType = TimeSpanType.Metric
            };
            using var expected = new MemoryStream();
            objects.SerializeToCsv(expected, TempoMap.Default, settings);
            using var actual = new AsyncOnlyStream();
            await objects.SerializeToCsvAsync(actual, TempoMap.Default, settings);
            Assert.That(actual.ToArray(), Is.EqualTo(expected.ToArray()));
            using var input = new AsyncOnlyStream(actual.ToArray());
            var readSettings = new CsvDeserializationSettings
            {
                Delimiter = ';',
                BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
                NoteFormat = CsvNoteFormat.Letter,
                TimeType = TimeSpanType.Metric,
                LengthType = TimeSpanType.Metric
            };
            expected.Position = 0;
            var expectedObjects = CsvSerializer.DeserializeObjectsFromCsv(expected, TempoMap.Default, readSettings);
            var actualObjects = await CsvSerializer.DeserializeObjectsFromCsvAsync(input, TempoMap.Default, readSettings);
            Assert.That(actualObjects.Select(obj => (obj.GetType(), obj.Time)), Is.EqualTo(expectedObjects.Select(obj => (obj.GetType(), obj.Time))));
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
                finally { disposed = true; }
            }
            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default);
            Assert.That(visited, Is.Zero);
            await foreach (var obj in result)
            {
                Assert.That(((Note)obj).Length, Is.EqualTo(10));
                Assert.That(visited, Is.InRange(1, 64));
                break;
            }
            Assert.That(disposed, Is.True);
            Assert.That(visited, Is.InRange(1, 64));
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
                        yield return new Note((SevenBitNumber)60, 100);
                }
                finally { disposed = true; }
            }
            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default);
            await using var enumerator = result.GetAsyncEnumerator(cancellation.Token);
            Assert.That(await enumerator.MoveNextAsync(), Is.True);
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
            Assert.That(disposed, Is.True);
        }

        [Test]
        public async Task Splitter_BatchedEnumeration_ReportsOneCompletion()
        {
            var progress = new InlineProgress();
            var objects = Enumerable.Range(0, 200).Select(i => (ITimedObject)new Note((SevenBitNumber)60, 0, i));
            var result = await Collect(objects.SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default, progress: progress));
            Assert.That(result.Count, Is.EqualTo(200));
            Assert.That(progress.Reports.Count(report => report.IsCompleted), Is.EqualTo(1));
            Assert.That(progress.Reports.Last().ProcessedItems, Is.GreaterThanOrEqualTo(200));
            Assert.That(progress.Reports.Select(report => report.ProcessedItems), Is.Ordered);
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
                        yield return new TimedEvent(new TextEvent(i.ToString()), i);
                    throw failure;
                }
                finally { disposed = true; }
            }
            var progress = new InlineProgress();
            var result = Source().SplitObjectsByStepAsync((MidiTimeSpan)10, TempoMap.Default, progress: progress);
            await using var enumerator = result.GetAsyncEnumerator();
            for (int i = 0; i < validItems; i++)
            {
                Assert.That(await enumerator.MoveNextAsync(), Is.True);
                Assert.That(((TextEvent)((TimedEvent)enumerator.Current).Event).Text, Is.EqualTo(i.ToString()));
            }
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync());
            Assert.That(exception, Is.SameAs(failure));
            Assert.That(disposed, Is.True);
            Assert.That(progress.Reports.Any(report => report.IsCompleted), Is.False);
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
        public void Merger_Cancellation_StopsPreparationBeforeFirstResult()
        {
            using var cancellation = new CancellationTokenSource();
            int visited = 0;
            bool disposed = false;
            IEnumerable<ITimedObject> Source()
            {
                try
                {
                    while (true)
                    {
                        if (++visited == 4)
                            cancellation.Cancel();
                        yield return new Note((SevenBitNumber)60, 100, visited);
                    }
                }
                finally { disposed = true; }
            }
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await Collect(Source().MergeObjectsAsync(TempoMap.Default, cancellationToken: cancellation.Token)));
            Assert.That(visited, Is.EqualTo(4));
            Assert.That(disposed, Is.True);
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

        [Test]
        public void AsyncApis_CoverEverySynchronousOverload()
        {
            var types = new[]
            {
                typeof(CsvSerializer), typeof(Splitter), typeof(Merger), typeof(Quantizer),
                typeof(QuantizerUtilities), typeof(Repeater), typeof(RepeaterUtilities),
                typeof(Resizer), typeof(Sanitizer)
            };
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var sync in methods.Where(m => !m.Name.EndsWith("Async", StringComparison.Ordinal)))
                {
                    var parameters = sync.GetParameters();
                    var asyncMethod = methods.SingleOrDefault(m =>
                        m.Name == sync.Name + "Async" &&
                        m.GetParameters().Length == parameters.Length + 2 &&
                        m.GetParameters().Take(parameters.Length).Select(p => p.ParameterType).SequenceEqual(parameters.Select(p => p.ParameterType)));
                    Assert.That(asyncMethod, Is.Not.Null, type.Name + "." + sync);
                    Assert.That(asyncMethod.GetParameters()[parameters.Length].ParameterType, Is.EqualTo(typeof(IProgress<AsyncOperationProgress>)));
                    Assert.That(asyncMethod.GetParameters().Last().ParameterType, Is.EqualTo(typeof(CancellationToken)));
                    Assert.That(asyncMethod.GetParameters().Last().IsOptional, Is.True);
                }
            }
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
                    if (target == 0) await actual.SplitObjectsByStepAsync(ObjectType.Note, (MidiTimeSpan)40);
                    else if (target == 1) await track.SplitObjectsByStepAsync(ObjectType.Note, (MidiTimeSpan)40, TempoMap.Default);
                    else await tracks.SplitObjectsByStepAsync(ObjectType.Note, (MidiTimeSpan)40, TempoMap.Default);
                    break;
                case 1:
                    expected.SplitObjectsByGrid(ObjectType.Note, grid);
                    if (target == 0) await actual.SplitObjectsByGridAsync(ObjectType.Note, grid);
                    else if (target == 1) await track.SplitObjectsByGridAsync(ObjectType.Note, grid, TempoMap.Default);
                    else await tracks.SplitObjectsByGridAsync(ObjectType.Note, grid, TempoMap.Default);
                    break;
                case 2:
                    expected.SplitObjectsByPartsNumber(ObjectType.Note, 3, TimeSpanType.Midi);
                    if (target == 0) await actual.SplitObjectsByPartsNumberAsync(ObjectType.Note, 3, TimeSpanType.Midi);
                    else if (target == 1) await track.SplitObjectsByPartsNumberAsync(ObjectType.Note, 3, TimeSpanType.Midi, TempoMap.Default);
                    else await tracks.SplitObjectsByPartsNumberAsync(ObjectType.Note, 3, TimeSpanType.Midi, TempoMap.Default);
                    break;
                case 3:
                    expected.SplitObjectsAtDistance(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start);
                    if (target == 0) await actual.SplitObjectsAtDistanceAsync(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start);
                    else if (target == 1) await track.SplitObjectsAtDistanceAsync(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default);
                    else await tracks.SplitObjectsAtDistanceAsync(ObjectType.Note, (MidiTimeSpan)40, LengthedObjectTarget.Start, TempoMap.Default);
                    break;
                default:
                    expected.SplitObjectsAtDistance(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start);
                    if (target == 0) await actual.SplitObjectsAtDistanceAsync(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start);
                    else if (target == 1) await track.SplitObjectsAtDistanceAsync(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default);
                    else await tracks.SplitObjectsAtDistanceAsync(ObjectType.Note, 0.4, TimeSpanType.Midi, LengthedObjectTarget.Start, TempoMap.Default);
                    break;
            }
            Assert.That(actual.GetNotes().Select(n => (n.Time, n.Length)), Is.EqualTo(expected.GetNotes().Select(n => (n.Time, n.Length))));
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
