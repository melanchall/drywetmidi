using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiFileAsyncTests
    {
        private static MidiFile CreateFile() => new MidiFile(new TrackChunk(
            new NoteOnEvent { Velocity = (SevenBitNumber)100 },
            new NoteOffEvent { DeltaTime = 100 }))
        {
            _originalFormat = (ushort)MidiFileFormat.MultiTrack
        };

        [TestCase(false)]
        [TestCase(true)]
        public async Task RoundTrip_AsyncOnlyTransport(bool nonSeekable)
        {
            var expected = CreateFile();
            using var output = new AsyncOnlyStream(seekable: !nonSeekable);
            await expected.WriteAsync(output);
            Assert.That(output.AsyncWrites, Is.GreaterThan(0));
            Assert.That(output.IsDisposed, Is.False);
            using var input = new AsyncOnlyStream(output.ToArray(), !nonSeekable, 3);
            var actual = await MidiFile.ReadAsync(input);
            Assert.That(MidiFile.Equals(expected, actual, out var message), Is.True, message);
            Assert.That(input.AsyncReads, Is.GreaterThan(0));
            Assert.That(input.IsDisposed, Is.False);
        }

        [Test]
        public async Task Lazy_AsyncOnlyTransport_FinalizesHeaders()
        {
            using var output = new AsyncOnlyStream();
            var writer = await MidiFile.WriteLazyAsync(output);
            await writer.StartTrackChunkAsync();
            await writer.WriteEventAsync(new NoteOnEvent());
            await writer.StartTrackChunkAsync();
            await writer.WriteEventAsync(new NoteOffEvent { DeltaTime = 15 });
            await writer.DisposeAsync();
            await writer.DisposeAsync();
            Assert.That(output.IsDisposed, Is.False);

            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            await using var reader = await MidiFile.ReadLazyAsync(input);
            var tokens = new List<MidiToken>();
            await foreach (var token in reader.EnumerateTokensAsync())
                tokens.Add(token);
            Assert.That(tokens.FindAll(t => t is MidiEventToken).Count, Is.EqualTo(4));
            var header = (FileHeaderToken)tokens.Find(t => t is FileHeaderToken);
            Assert.That(header.TracksNumber, Is.EqualTo(2));
        }

        [Test]
        public void ReadAsync_PreservesMemoryLimit()
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray(), false);
            Assert.ThrowsAsync<MidiFileTooLargeException>(async () =>
                await MidiFile.ReadAsync(input, new ReadingSettings { MaxMemorySize = 1 }));
            Assert.That(input.IsDisposed, Is.False);
        }

        [Test]
        public void PreCanceled_DoesNotTouchTransport()
        {
            using var stream = new AsyncOnlyStream();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await CreateFile().WriteAsync(stream, cancellationToken: cancellation.Token));
            Assert.That(stream.AsyncWrites, Is.Zero);
            Assert.That(stream.IsDisposed, Is.False);
        }

        [Test]
        public async Task Reader_RejectsOverlappingOperations()
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray());
            await using var reader = await MidiFile.ReadLazyAsync(input);
            input.ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending = reader.ReadTokenAsync();
            await input.ReadStarted.Task;
            Assert.Throws<InvalidOperationException>(() => reader.ReadToken());
            Assert.ThrowsAsync<InvalidOperationException>(async () => await reader.ReadTokenAsync());
            input.ReadGate.SetResult(true);
            Assert.That(await pending, Is.Not.Null);
        }

        [Test]
        public async Task ReadAsync_CancellationDuringTransport_LeavesStreamOpen()
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray());
            input.ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var cancellation = new CancellationTokenSource();
            var pending = MidiFile.ReadAsync(input, cancellationToken: cancellation.Token);
            await input.ReadStarted.Task;
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(async () => await pending);
            Assert.That(input.IsDisposed, Is.False);
        }

        [Test]
        public async Task PathOverloads_RoundTripAndOwnHandles()
        {
            var path = "async-midi-" + Guid.NewGuid().ToString("N") + ".mid";
            try
            {
                var expected = CreateFile();
                await expected.WriteAsync(path);
                Assert.That(MidiFile.Equals(expected, await MidiFile.ReadAsync(path)), Is.True);
                Assert.ThrowsAsync<IOException>(async () => await expected.WriteAsync(path));
                await using (var writer = await MidiFile.WriteLazyAsync(path, overwriteFile: true))
                {
                    await writer.StartTrackChunkAsync();
                    await writer.WriteEventAsync(new NoteOnEvent());
                }
                await using (var reader = await MidiFile.ReadLazyAsync(path))
                {
                    var count = 0;
                    await foreach (var token in reader.EnumerateTokensAsync())
                        count++;
                    Assert.That(count, Is.EqualTo(5));
                }
                using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally { File.Delete(path); }
        }

        [Test]
        public async Task LazyWriter_FinalizationFailure_ClosesOwnedStream()
        {
            var path = "async-midi-" + Guid.NewGuid().ToString("N") + ".mid";
            try
            {
                var writer = await MidiFile.WriteLazyAsync(path);
                await writer.StartTrackChunkAsync();
                writer.BeforeEndTrackChunkAsync = async cancellationToken =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Codec failure.");
                };
                Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.DisposeAsync());
                await writer.DisposeAsync();
                using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally { File.Delete(path); }
        }

        [Test]
        public async Task CustomCodec_UsesAsyncTransport_AndSettings()
        {
            var types = new EventTypesCollection { { typeof(CustomEvent), 0x5A } };
            var expected = new MidiFile(new TrackChunk(new CustomEvent { Value = 42 }));
            using var output = new AsyncOnlyStream();
            await expected.WriteAsync(output, settings: new WritingSettings
            {
                CustomMetaEventTypes = types,
                WriterSettings = new WriterSettings { UseBuffering = false }
            });
            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            var actual = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                CustomMetaEventTypes = types,
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            });
            Assert.That(((CustomEvent)((TrackChunk)actual.Chunks[0]).Events[0]).Value, Is.EqualTo(42));
        }

        [Test]
        public async Task CustomChunk_UsesInMemoryCodec_WithAbsolutePositions()
        {
            var file = new MidiFile(new CustomChunk { Value = 42 });
            using var output = new AsyncOnlyStream();
            await file.WriteAsync(output);
            using var input = new AsyncOnlyStream(output.ToArray(), false, 2);
            var actual = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(CustomChunk), "Cstm" } }
            });
            var chunk = (CustomChunk)actual.Chunks[0];
            Assert.That(chunk.Value, Is.EqualTo(42));
            Assert.That(chunk.ContentPosition, Is.EqualTo(22));
        }

        [Test]
        public async Task ExistingValidCorpus_AsyncReadAndWriteMatchSynchronous()
        {
            foreach (var path in TestFilesProvider.GetValidFilesPaths())
            {
                var expected = MidiFile.Read(path);
                var actual = await MidiFile.ReadAsync(path);
                Assert.That(MidiFile.Equals(expected, actual, out var message), Is.True, path + ": " + message);
                using var synchronous = new MemoryStream();
                using var asynchronous = new MemoryStream();
                expected.Write(synchronous);
                await expected.WriteAsync(asynchronous);
                Assert.That(asynchronous.ToArray(), Is.EqualTo(synchronous.ToArray()), path);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task ExistingInvalidCorpus_AsyncReadMatchesSynchronousPolicies(bool ignoreTruncatedData)
        {
            foreach (var path in Directory.GetFiles(TestFilesProvider.GetInvalidFilesDirectory(""), "*.mid", SearchOption.AllDirectories))
            {
                var bytes = File.ReadAllBytes(path);
                MidiFile expected = null;
                Exception expectedError = null;
                var settings = new ReadingSettings
                {
                    NotEnoughBytesPolicy = ignoreTruncatedData ? NotEnoughBytesPolicy.Ignore : NotEnoughBytesPolicy.Abort
                };
                try { expected = MidiFile.Read(new MemoryStream(bytes), settings); }
                catch (Exception error) { expectedError = error; }
                MidiFile actual = null;
                Exception actualError = null;
                try { actual = await MidiFile.ReadAsync(new MemoryStream(bytes), settings); }
                catch (Exception error) { actualError = error; }
                Assert.That(actualError?.GetType(), Is.EqualTo(expectedError?.GetType()), path + ": " + actualError);
                if (expectedError == null)
                    Assert.That(MidiFile.Equals(expected, actual, out var message), Is.True, path + ": " + message);
            }
        }

        [Test]
        public async Task ReadAsync_EstimatedMemoryMatchesSynchronous()
        {
            var file = new MidiFile(new TrackChunk(
                new TextEvent("payload"),
                new NormalSysExEvent { Data = new byte[10000] },
                new NoteOnEvent { Velocity = (SevenBitNumber)100 }),
                new UnknownChunk("Test") { Data = new byte[300] });
            using var bytes = new MemoryStream();
            file.Write(bytes);
            var settings = new ReadingSettings { MaxMemorySize = long.MaxValue };
            var expected = MidiFile.Read(new MemoryStream(bytes.ToArray()), settings);
            var actual = await MidiFile.ReadAsync(new MemoryStream(bytes.ToArray()), settings);
            Assert.That(actual.EstimatedMemorySize, Is.EqualTo(expected.EstimatedMemorySize));
        }

        [TestCase(BufferingPolicy.BufferAllData)]
        [TestCase(BufferingPolicy.DontUseBuffering)]
        [TestCase(BufferingPolicy.UseCustomBuffer)]
        [TestCase(BufferingPolicy.UseFixedSizeBuffer)]
        public async Task ReadAsync_PreservesReaderBufferingPolicy(BufferingPolicy policy)
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray(), true, 2);
            var file = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                ReaderSettings = new ReaderSettings { BufferingPolicy = policy, BufferSize = 3, Buffer = new byte[5] }
            });
            Assert.That(MidiFile.Equals(CreateFile(), file), Is.True);
        }

        [Test]
        public async Task LazyWriter_HeaderPatchesRespectInitialPosition_AndCompleteTrackChunks()
        {
            using var output = new MemoryStream();
            output.Write(new byte[] { 1, 2, 3 }, 0, 3);
            await using (var writer = await MidiFile.WriteLazyAsync(output))
            {
                await writer.WriteChunkAsync(new TrackChunk(new NoteOffEvent()));
                await writer.StartTrackChunkAsync();
                await writer.WriteEventAsync(new NoteOffEvent());
            }
            var bytes = output.ToArray();
            Assert.That(bytes[0], Is.EqualTo(1));
            Assert.That(bytes[1], Is.EqualTo(2));
            Assert.That(bytes[2], Is.EqualTo(3));
            using var input = new MemoryStream(bytes, 3, bytes.Length - 3, false);
            var file = await MidiFile.ReadAsync(input);
            Assert.That(file.Chunks.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task LazyPipelines_CanMixSynchronousAndAsynchronousOperations()
        {
            using var bytes = new MemoryStream();
            var writer = MidiFile.WriteLazy(bytes);
            writer.StartTrackChunk();
            await writer.WriteEventAsync(new NoteOffEvent());
            writer.EndTrackChunk();
            await writer.DisposeAsync();
            bytes.Position = 0;
            using var reader = MidiFile.ReadLazy(bytes);
            Assert.That(reader.ReadToken(), Is.TypeOf<ChunkHeaderToken>());
            Assert.That(await reader.ReadTokenAsync(), Is.TypeOf<FileHeaderToken>());
            Assert.That(reader.ReadToken(), Is.TypeOf<ChunkHeaderToken>());
            Assert.That(await reader.ReadTokenAsync(), Is.TypeOf<MidiEventToken>());
        }

        [Test]
        public async Task NonSeekableMalformedLength_BuffersOnlyAvailablePayload()
        {
            using var bytes = new MemoryStream();
            new MidiFile(new UnknownChunk("Test") { Data = new byte[] { 1, 2, 3 } }).Write(bytes);
            var data = bytes.ToArray();
            for (var i = 18; i < 22; i++)
                data[i] = 0xFF;
            using var input = new AsyncOnlyStream(data, false, 2);
            var file = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
            });
            Assert.That(((UnknownChunk)file.Chunks[0]).Data, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public async Task BulkIo_ReportsActualBytes_AndSuccessfulCompletion()
        {
            var writing = new ProgressRecorder();
            using var output = new AsyncOnlyStream();
            await CreateFile().WriteAsync(output, progress: writing);
            var bytes = output.ToArray();
            var written = writing.Reports[writing.Reports.Count - 1];
            Assert.That(written.ProcessedItems, Is.EqualTo(bytes.Length));
            Assert.That(written.Phase, Is.EqualTo("Writing"));
            Assert.That(written.IsCompleted, Is.True);

            var reading = new ProgressRecorder();
            using var input = new AsyncOnlyStream(bytes, false, 2);
            await MidiFile.ReadAsync(input, progress: reading);
            var read = reading.Reports[reading.Reports.Count - 1];
            Assert.That(read.ProcessedItems, Is.EqualTo(bytes.Length));
            Assert.That(read.Phase, Is.EqualTo("Reading"));
            Assert.That(read.IsCompleted, Is.True);
        }

        [Test]
        public async Task ReadProgress_IsThrottled_AndReportsDuringIo()
        {
            using var bytes = new MemoryStream();
            new MidiFile(new UnknownChunk("Test") { Data = new byte[262144] }).Write(bytes);
            var progress = new ProgressRecorder();
            using var input = new AsyncOnlyStream(bytes.ToArray(), false, 4096) { ReadDelayMilliseconds = 5 };
            await MidiFile.ReadAsync(input, progress: progress);
            Assert.That(progress.Reports.Count, Is.GreaterThan(1));
            Assert.That(progress.Reports.Count, Is.LessThan(input.AsyncReads));
            Assert.That(progress.Reports[progress.Reports.Count - 1].ProcessedItems, Is.EqualTo(bytes.Length));
            long previous = 0;
            foreach (var report in progress.Reports)
            {
                Assert.That(report.ProcessedItems, Is.GreaterThanOrEqualTo(previous));
                previous = report.ProcessedItems;
            }
        }

        [Test]
        public void BulkIo_FailureOrCancellation_DoesNotReportCompletion()
        {
            var progress = new ProgressRecorder();
            using var input = new AsyncOnlyStream(new byte[] { 1, 2, 3 });
            Assert.ThrowsAsync<NotEnoughBytesException>(async () => await MidiFile.ReadAsync(input, progress: progress));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            using var output = new AsyncOnlyStream();
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await CreateFile().WriteAsync(output, progress: progress, cancellationToken: cancellation.Token));
            Assert.That(progress.Reports.TrueForAll(report => !report.IsCompleted), Is.True);
        }

        private sealed class ProgressRecorder : IProgress<AsyncOperationProgress>
        {
            internal readonly List<AsyncOperationProgress> Reports = new List<AsyncOperationProgress>();
            public void Report(AsyncOperationProgress value) => Reports.Add(value);
        }

        [Test]
        public async Task LazyWriter_RejectsOverlappingOperations()
        {
            using var output = new AsyncOnlyStream();
            var writer = await MidiFile.WriteLazyAsync(output, new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            });
            output.WriteGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            output.WriteStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending = writer.StartTrackChunkAsync();
            await output.WriteStarted.Task;
            Assert.Throws<InvalidOperationException>(() => writer.WriteEvent(new NoteOnEvent()));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.DisposeAsync());
            output.WriteGate.SetResult(true);
            await pending;
            await writer.DisposeAsync();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task LazyWriter_AsyncFinalizationCallback_CanEmitDeferredEvent(int operation)
        {
            using var output = new AsyncOnlyStream();
            var writer = await MidiFile.WriteLazyAsync(output, new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            });
            await writer.StartTrackChunkAsync();
            writer.BeforeEndTrackChunk = () => throw new AssertionException("Synchronous callback selected.");
            writer.BeforeEndTrackChunkAsync = async cancellationToken =>
            {
                await Task.Yield();
                await writer.WriteEventAsync(new NoteOffEvent { DeltaTime = 20 }, cancellationToken);
            };
            if (operation == 0)
                await writer.EndTrackChunkAsync();
            else if (operation == 1)
                await writer.StartTrackChunkAsync();
            else
                await writer.DisposeAsync();
            writer.BeforeEndTrackChunk = null;
            writer.BeforeEndTrackChunkAsync = null;
            await writer.DisposeAsync();
            using var input = new AsyncOnlyStream(output.ToArray());
            var file = await MidiFile.ReadAsync(input);
            var events = ((TrackChunk)file.Chunks[0]).Events;
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0], Is.TypeOf<NoteOffEvent>());
            Assert.That(events[0].DeltaTime, Is.EqualTo(20));
        }

        [Test]
        public async Task LazyReader_DoesNotReadWholeInput_AndWritesUseBoundedPackets()
        {
            var file = new MidiFile(new UnknownChunk("Test") { Data = new byte[200000] });
            using var output = new AsyncOnlyStream();
            await file.WriteAsync(output, settings: new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            });
            Assert.That(output.MaxWrite, Is.LessThanOrEqualTo(65536));
            using var input = new AsyncOnlyStream(output.ToArray(), false);
            await using var reader = await MidiFile.ReadLazyAsync(input);
            Assert.That(await reader.ReadTokenAsync(), Is.TypeOf<ChunkHeaderToken>());
            Assert.That(input.BytesRead, Is.LessThan(100));
        }

        private sealed class CustomEvent : MetaEvent
        {
            public byte Value { get; set; }
            public CustomEvent() { }
            protected override MidiEvent CloneEvent() => new CustomEvent { Value = Value };
            protected override int GetContentSize(WritingSettings settings) => 1;
            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) => Value = reader.ReadByte();
            protected override void WriteContent(MidiWriter writer, WritingSettings settings) => writer.WriteByte(Value);
        }

        private sealed class CustomChunk : MidiChunk
        {
            public CustomChunk() : base("Cstm") { }
            public byte Value { get; set; }
            public long ContentPosition { get; private set; }
            public override MidiChunk Clone() => new CustomChunk { Value = Value };
            protected override uint GetContentSize(WritingSettings settings) => 1;
            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                ContentPosition = reader.Position;
                Value = reader.ReadByte();
            }
            protected override void WriteContent(MidiWriter writer, WritingSettings settings) => writer.WriteByte(Value);
        }

        private sealed class AsyncOnlyStream : Stream
        {
            private readonly MemoryStream _inner;
            private readonly bool _seekable;
            private readonly int _maxRead;

            internal AsyncOnlyStream(byte[] bytes = null, bool seekable = true, int maxRead = int.MaxValue)
            {
                _inner = bytes == null ? new MemoryStream() : new MemoryStream(bytes);
                _seekable = seekable;
                _maxRead = maxRead;
            }

            internal int AsyncReads { get; private set; }
            internal int AsyncWrites { get; private set; }
            internal int BytesRead { get; private set; }
            internal int MaxWrite { get; private set; }
            internal int ReadDelayMilliseconds { get; set; }
            internal bool IsDisposed { get; private set; }
            internal TaskCompletionSource<bool> ReadGate { get; set; }
            internal TaskCompletionSource<bool> WriteGate { get; set; }
            internal TaskCompletionSource<bool> WriteStarted { get; set; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal TaskCompletionSource<bool> ReadStarted { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal byte[] ToArray() => _inner.ToArray();
            public override bool CanRead => true;
            public override bool CanSeek => _seekable;
            public override bool CanWrite => true;
            public override long Length => _seekable ? _inner.Length : throw new NotSupportedException();
            public override long Position
            {
                get => _seekable ? _inner.Position : throw new NotSupportedException();
                set { if (!_seekable) throw new NotSupportedException(); _inner.Position = value; }
            }
            public override int Read(byte[] buffer, int offset, int count) => throw new AssertionException("Synchronous read.");
            public override void Write(byte[] buffer, int offset, int count) => throw new AssertionException("Synchronous write.");
            public override void Flush() => throw new AssertionException("Synchronous flush.");
            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
            public override void SetLength(long value) => _inner.SetLength(value);
            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AsyncReads++;
                ReadStarted.TrySetResult(true);
                if (ReadGate != null)
                    await ReadGate.Task.WaitAsync(cancellationToken);
                if (ReadDelayMilliseconds > 0)
                    await Task.Delay(ReadDelayMilliseconds, cancellationToken);
                await Task.Yield();
                var read = _inner.Read(buffer, offset, Math.Min(count, _maxRead));
                BytesRead += read;
                return read;
            }
            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AsyncWrites++;
                MaxWrite = Math.Max(MaxWrite, count);
                WriteStarted.TrySetResult(true);
                if (WriteGate != null)
                    await WriteGate.Task.WaitAsync(cancellationToken);
                await Task.Yield();
                _inner.Write(buffer, offset, count);
            }
            public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                if (disposing) _inner.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
