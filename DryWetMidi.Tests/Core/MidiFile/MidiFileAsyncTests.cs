using Melanchall.DryWetMidi.Core;
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
            new NoteOnEvent(),
            new NoteOffEvent { DeltaTime = 100 }));

        [TestCase(false)]
        [TestCase(true)]
        public async Task RoundTrip_AsyncOnlyTransport(bool nonSeekable)
        {
            var expected = CreateFile();
            using var output = new AsyncOnlyStream();
            await expected.WriteAsync(output);
            Assert.That(output.AsyncWrites, Is.GreaterThan(0));
            Assert.That(output.IsDisposed, Is.False);
            using var input = new AsyncOnlyStream(output.ToArray(), !nonSeekable, 3);
            var actual = await MidiFile.ReadAsync(input);
            Assert.That(MidiFile.Equals(expected, actual), Is.True);
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
                writer.BeforeEndTrackChunk = () => throw new InvalidOperationException("Codec failure.");
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

        private sealed class CustomEvent : MetaEvent
        {
            public byte Value { get; set; }
            public CustomEvent() { }
            protected override MidiEvent CloneEvent() => new CustomEvent { Value = Value };
            protected override int GetContentSize(WritingSettings settings) => 1;
            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) => Value = reader.ReadByte();
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
                await Task.Yield();
                return _inner.Read(buffer, offset, Math.Min(count, _maxRead));
            }
            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AsyncWrites++;
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
