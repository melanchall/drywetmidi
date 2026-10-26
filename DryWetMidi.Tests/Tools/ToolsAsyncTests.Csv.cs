using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class ToolsAsyncTests
    {
        [Test]
        public void Csv_ImmediatelyCompletedIo_DoesNotRequireWorker([Values] bool writing)
        {
            using var expected = new MemoryStream();
            CreateFile().SerializeToCsv(expected);

            Exception failure = null;
            var callingThreadId = 0;
            var firstIoThreadId = 0;
            var ioStayedOnCallingThread = true;
            var thread = new Thread(() =>
            {
                try
                {
                    callingThreadId = Thread.CurrentThread.ManagedThreadId;

                    void ObserveIo()
                    {
                        var currentThreadId = Thread.CurrentThread.ManagedThreadId;

                        if (firstIoThreadId == 0)
                            firstIoThreadId = currentThreadId;

                        ioStayedOnCallingThread &= currentThreadId == callingThreadId;
                    }

                    using var stream = new AsyncOnlyStream(writing ? null : expected.ToArray())
                    {
                        CompleteSynchronously = true,
                        OnRead = ObserveIo,
                        OnWrite = ObserveIo
                    };

                    if (writing)
                    {
                        CreateFile().SerializeToCsvAsync(stream).GetAwaiter().GetResult();

                        ClassicAssert.AreEqual(expected.ToArray(), stream.ToArray(), "Inline CSV export differs from synchronous output.");
                    }
                    else
                    {
                        var restored = CsvSerializer.DeserializeFileFromCsvAsync(stream).GetAwaiter().GetResult();

                        MidiAsserts.AreEqual(CreateFile(), restored, false, "Inline CSV import differs from the original MIDI file.");
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
            })
            {
                IsBackground = true
            };

            thread.Start();

            ClassicAssert.IsTrue(thread.Join(TimeSpan.FromSeconds(10)), "Immediately completed CSV I/O did not finish.");
            ClassicAssert.IsNull(failure, $"Immediately completed CSV I/O failed: {failure}");
            ClassicAssert.AreEqual(callingThreadId, firstIoThreadId, "CSV moved to a worker before its first transport operation.");
            ClassicAssert.IsTrue(ioStayedOnCallingThread, "CSV scheduled a worker despite synchronously completed transport operations.");
        }

        [Test]
        public async Task Csv_StreamOverloads_OnlyUseAsyncIo([Values(0, 1, 2, 3)] int family)
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

            ClassicAssert.AreEqual(expected.ToArray(), output.ToArray(), "Async CSV output bytes differ.");
            ClassicAssert.Greater(output.Writes, 0, "CSV serialization did not use async writes.");
            ClassicAssert.AreEqual(1, output.Flushes, "CSV serialization did not flush exactly once.");

            using var input = new AsyncOnlyStream(output.ToArray());
            switch (family)
            {
                case 0:
                    var restoredFile = await CsvSerializer.DeserializeFileFromCsvAsync(input);
                    ClassicAssert.AreEqual(file.GetNotes().Count, restoredFile.GetNotes().Count, "CSV file round-trip note count differs.");
                    break;
                case 1:
                    var chunks = await CsvSerializer.DeserializeChunksFromCsvAsync(input, TempoMap.Default);
                    ClassicAssert.AreEqual(file.Chunks.Count, chunks.Count, "CSV chunks round-trip count differs.");
                    break;
                case 2:
                    ClassicAssert.IsInstanceOf<TrackChunk>(await CsvSerializer.DeserializeChunkFromCsvAsync(input, TempoMap.Default), "CSV chunk round-trip returned an invalid type.");
                    break;
                default:
                    var objects = await CsvSerializer.DeserializeObjectsFromCsvAsync(input, TempoMap.Default);
                    ClassicAssert.AreEqual(track.GetNotes().Count, objects.OfType<Note>().Count(), "CSV objects round-trip note count differs.");
                    break;
            }

            ClassicAssert.Greater(input.Reads, 1, "CSV deserialization did not use incremental async reads.");
            ClassicAssert.IsFalse(input.Disposed, "CSV deserialization disposed the caller-owned stream.");
            ClassicAssert.IsFalse(output.Disposed, "CSV serialization disposed the caller-owned stream.");
        }

        [Test]
        public async Task Csv_PathOverloads_RoundTrip([Values(0, 1, 2, 3)] int family)
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
                        ClassicAssert.AreEqual(file.GetNotes().Count, (await CsvSerializer.DeserializeFileFromCsvAsync(path)).GetNotes().Count, "CSV file path round-trip note count differs.");
                        break;
                    case 1:
                        await file.Chunks.SerializeToCsvAsync(path, false, TempoMap.Default);
                        ClassicAssert.AreEqual(file.Chunks.Count, (await CsvSerializer.DeserializeChunksFromCsvAsync(path, TempoMap.Default)).Count, "CSV chunks path round-trip count differs.");
                        break;
                    case 2:
                        await track.SerializeToCsvAsync(path, false, TempoMap.Default);
                        ClassicAssert.IsInstanceOf<TrackChunk>(await CsvSerializer.DeserializeChunkFromCsvAsync(path, TempoMap.Default), "CSV chunk path round-trip returned an invalid type.");
                        break;
                    default:
                        await file.GetNotes().SerializeToCsvAsync(path, false, TempoMap.Default);
                        ClassicAssert.AreEqual(file.GetNotes().Count, (await CsvSerializer.DeserializeObjectsFromCsvAsync(path, TempoMap.Default)).Count, "CSV objects path round-trip count differs.");
                        break;
                }

                Assert.ThrowsAsync<IOException>(() => file.SerializeToCsvAsync(path, false), "CSV serialization overwrote a file without permission.");

                await file.SerializeToCsvAsync(path, true);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Csv_InvalidOrCanceledPathWrite_DoesNotCreateFile()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tools-async-" + Guid.NewGuid() + ".csv");
            try
            {
                Assert.ThrowsAsync<ArgumentNullException>(() => ((MidiFile)null).SerializeToCsvAsync(path, true), "CSV path serialization accepted a null file.");
                ClassicAssert.IsFalse(File.Exists(path), "Invalid file serialization created a path.");
                Assert.ThrowsAsync<ArgumentNullException>(() => CreateFile().GetTrackChunks().First().SerializeToCsvAsync(path, true, null), "CSV path serialization accepted a null tempo map.");
                ClassicAssert.IsFalse(File.Exists(path), "Invalid tempo-map serialization created a path.");

                using var cancellation = new CancellationTokenSource();
                cancellation.Cancel();

                Assert.CatchAsync<OperationCanceledException>(() => CreateFile().SerializeToCsvAsync(path, true, cancellationToken: cancellation.Token), "Pre-canceled CSV serialization did not throw.");
                ClassicAssert.IsFalse(File.Exists(path), "Pre-canceled serialization created a path.");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public async Task Csv_LargeInput_IsIncremental()
        {
            var file = new MidiFile(new TrackChunk(Enumerable.Range(0, 3000).Select(i => new TextEvent("line\n\"quoted\",é" + i) { DeltaTime = i % 2 })));
            using var output = new AsyncOnlyStream();

            await file.SerializeToCsvAsync(output);

            ClassicAssert.Greater(output.Writes, 1, "Large CSV serialization did not write incrementally.");

            using var input = new AsyncOnlyStream(output.ToArray());
            var restored = await CsvSerializer.DeserializeFileFromCsvAsync(input);

            ClassicAssert.AreEqual(3000, restored.GetTrackChunks().First().Events.Count, "Large CSV round-trip event count differs.");
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

            ClassicAssert.AreEqual(10000, visited, "CSV serialization did not consume the full source.");
            ClassicAssert.GreaterOrEqual(visitedAtFirstWrite, 1, "CSV serialization wrote before visiting a source item.");
            ClassicAssert.LessOrEqual(visitedAtFirstWrite, 9999, "CSV serialization staged the whole source before writing.");
        }

        [Test]
        public void Csv_CodecErrors_PropagateWithoutDeadlock()
        {
            using var input = new AsyncOnlyStream(System.Text.Encoding.UTF8.GetBytes("not a valid CSV MIDI file\n" + new string('x', 20000)));

            Assert.ThrowsAsync<CsvException>(async () =>
                await CsvSerializer.DeserializeFileFromCsvAsync(input).WaitAsync(TimeSpan.FromSeconds(10)), "CSV codec failure did not propagate without deadlock.");

            IEnumerable<ITimedObject> Source()
            {
                yield return new TimedEvent(new TextEvent("first"));
                throw new InvalidOperationException("Source failure.");
            }

            using var output = new AsyncOnlyStream();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await Source().SerializeToCsvAsync(output, TempoMap.Default).WaitAsync(TimeSpan.FromSeconds(10)), "CSV source failure did not propagate without deadlock.");
        }

        [Test]
        public async Task Csv_Progress_CompletesAfterFlush()
        {
            using var output = new AsyncOnlyStream();
            var progress = new InlineProgress();

            await CreateFile().SerializeToCsvAsync(output, progress: progress);

            ClassicAssert.IsTrue(progress.Reports.Last().IsCompleted, "CSV progress did not report completion.");
            ClassicAssert.Greater(progress.Reports.Last().ProcessedItems, 0, "CSV progress reported no work.");
            ClassicAssert.AreEqual(1, output.Flushes, "Completed CSV serialization did not flush exactly once.");
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
            using var actual = new AsyncOnlyStream();

            objects.SerializeToCsv(expected, TempoMap.Default, settings);
            await objects.SerializeToCsvAsync(actual, TempoMap.Default, settings);

            ClassicAssert.AreEqual(expected.ToArray(), actual.ToArray(), "Async CSV multi-record formatting differs.");

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

            ClassicAssert.AreEqual(expectedObjects.Select(obj => (obj.GetType(), obj.Time)), actualObjects.Select(obj => (obj.GetType(), obj.Time)), "Async CSV multi-record object types or times differ.");
        }

        [Test]
        public void Csv_IoErrors_PropagateWithoutDeadlock([Values] bool writing)
        {
            using var stream = new AsyncOnlyStream { FailRead = !writing, FailWrite = writing };

            Assert.ThrowsAsync<IOException>(async () =>
            {
                if (writing)
                    await CreateFile().SerializeToCsvAsync(stream).WaitAsync(TimeSpan.FromSeconds(10));
                else
                    await CsvSerializer.DeserializeFileFromCsvAsync(stream).WaitAsync(TimeSpan.FromSeconds(10));
            }, "CSV transport failure did not propagate without deadlock.");
        }

        [Test]
        public void Csv_Cancellation_InterruptsPendingIo([Values] bool writing)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            using var stream = new AsyncOnlyStream { BlockRead = !writing, BlockWrite = writing };

            Assert.CatchAsync<OperationCanceledException>(async () =>
            {
                if (writing)
                    await CreateFile().SerializeToCsvAsync(stream, cancellationToken: cancellation.Token).WaitAsync(TimeSpan.FromSeconds(10));
                else
                    await CsvSerializer.DeserializeFileFromCsvAsync(stream, cancellationToken: cancellation.Token).WaitAsync(TimeSpan.FromSeconds(10));
            }, "CSV cancellation did not interrupt pending I/O.");
        }
    }
}
