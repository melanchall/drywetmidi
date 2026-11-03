using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class NonSeekableStreamTests
    {
        private sealed class InputStream : Stream
        {
            private readonly MemoryStream _stream;
            private readonly int _maximumReadSize;

            public InputStream(byte[] bytes, int maximumReadSize = int.MaxValue)
            {
                _stream = new MemoryStream(bytes);
                _maximumReadSize = maximumReadSize;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public int ReadsCount { get; private set; }

            public long BytesRead => _stream.Position;

            public bool ThrowAtEnd { get; set; }

            public Exception NextReadFailure { get; set; }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return Read(buffer.AsSpan(offset, count));
            }

            public override int Read(Span<byte> buffer)
            {
                ReadsCount++;
                if (NextReadFailure != null)
                {
                    var exception = NextReadFailure;
                    NextReadFailure = null;
                    throw exception;
                }

                if (ThrowAtEnd && _stream.Position == _stream.Length)
                    throw new IOException("Reading past the logical file boundary.");

                return _stream.Read(buffer.Slice(0, Math.Min(buffer.Length, _maximumReadSize)));
            }

            public override Task<int> ReadAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(Read(buffer, offset, count));
            }

            public override void Flush() => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _stream.Dispose();

                base.Dispose(disposing);
            }
        }

        [Test]
        public async Task EndReached_PreservesPositionAndReplay([Values] bool asynchronous)
        {
            using var input = new InputStream(new byte[] { 10, 20, 30 });
            using var reader = new MidiReader(input, new ReaderSettings());

            ClassicAssert.AreEqual(new byte[] { 10, 20, 30 }, await ReadBytes(reader, 3, asynchronous), "Initial bytes differ.");
            ClassicAssert.IsTrue(await EndReached(reader, asynchronous), "EOF was not detected.");
            var readsCount = input.ReadsCount;
            ClassicAssert.IsTrue(await EndReached(reader, asynchronous), "Repeated EOF detection differs.");

            ClassicAssert.AreEqual(3, reader.Position, "EOF changed the logical position.");
            ClassicAssert.AreEqual(readsCount, input.ReadsCount, "Cached EOF performed another source read.");
            ClassicAssert.AreEqual(long.MaxValue, reader.Length, "The public unknown-length sentinel changed.");

            await SetPosition(reader, 1, asynchronous);

            ClassicAssert.IsFalse(await EndReached(reader, asynchronous), "EOF hid retained replay bytes.");
            ClassicAssert.AreEqual(new byte[] { 20, 30 }, await ReadBytes(reader, 5, asynchronous), "Replay after EOF differs.");
            ClassicAssert.AreEqual(3, reader.Position, "Short replay changed the final position.");
            ClassicAssert.AreEqual(readsCount, input.ReadsCount, "Replay after EOF touched the source.");
        }

        [Test]
        public async Task Position_LargeForwardAndBackwardDistances(
            [Values] bool asynchronous,
            [Values(10L, (long)int.MaxValue + 1, (long)uint.MaxValue, 1L << 32, long.MaxValue)] long position)
        {
            using var input = new InputStream(new byte[] { 10, 20, 30 }, 1);
            using var reader = new MidiReader(input, new ReaderSettings());

            await SetPosition(reader, position, asynchronous);

            ClassicAssert.AreEqual(3, reader.Position, "Forward skipping did not stop at the physical end.");
            ClassicAssert.AreEqual(3, input.BytesRead, "Forward skipping did not consume the available bytes.");

            await SetPosition(reader, 2, asynchronous);

            ClassicAssert.AreEqual(new byte[] { 30 }, await ReadBytes(reader, 1, asynchronous), "Rewind after skipping lost data.");
        }

        [Test]
        public async Task Position_InvalidRewindDoesNotChangeState([Values] bool asynchronous)
        {
            using var input = new InputStream(Enumerable.Range(0, 10).Select(value => (byte)value).ToArray());
            using var reader = new MidiReader(input, new ReaderSettings { NonSeekableStreamBufferSize = 4 });
            await ReadBytes(reader, 10, asynchronous);
            await EndReached(reader, asynchronous);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await SetPosition(reader, 0, asynchronous),
                "An out-of-window rewind was accepted.");
            ClassicAssert.AreEqual(10, reader.Position, "A rejected rewind changed the position.");

            await SetPosition(reader, 6, asynchronous);

            ClassicAssert.AreEqual(new byte[] { 6, 7, 8, 9 }, await ReadBytes(reader, 4, asynchronous), "Rejected rewind corrupted retained history.");
        }

        [Test]
        public async Task EndReached_PeekDoesNotEvictHistory([Values] bool asynchronous)
        {
            using var input = new InputStream(new byte[] { 10, 20, 30, 40, 50 });
            using var reader = new MidiReader(input, new ReaderSettings { NonSeekableStreamBufferSize = 4 });
            await ReadBytes(reader, 4, asynchronous);

            ClassicAssert.IsFalse(await EndReached(reader, asynchronous), "Look-ahead lost the fifth byte.");
            var readsCount = input.ReadsCount;
            ClassicAssert.IsFalse(await EndReached(reader, asynchronous), "Repeated look-ahead lost the fifth byte.");
            await SetPosition(reader, 0, asynchronous);

            ClassicAssert.AreEqual(new byte[] { 10, 20, 30, 40 }, await ReadBytes(reader, 4, asynchronous), "Look-ahead evicted committed history.");
            ClassicAssert.AreEqual(new byte[] { 50 }, await ReadBytes(reader, 1, asynchronous), "Look-ahead was not committed in order.");
            ClassicAssert.AreEqual(readsCount, input.ReadsCount, "Repeated look-ahead or replay touched the source.");
        }

        [Test]
        public async Task Replay_ReturnsBeforeFallibleSourceRead([Values] bool asynchronous)
        {
            using var input = new InputStream(new byte[] { 10, 20, 30, 40 });
            using var wrapper = new StreamWrapper(input, 4);
            var bytes = new byte[3];
            await Read(wrapper, bytes, asynchronous);
            wrapper.Position = 1;
            input.NextReadFailure = new IOException("Injected read failure.");

            var count = await Read(wrapper, bytes, asynchronous);

            ClassicAssert.AreEqual(2, count, "Replay did not return a short read before source IO.");
            ClassicAssert.AreEqual(new byte[] { 20, 30 }, bytes.Take(count).ToArray(), "Replay returned incorrect bytes.");
            ClassicAssert.AreEqual(3, wrapper.Position, "Replay did not commit its logical position.");
            Assert.ThrowsAsync<IOException>(
                async () => await Read(wrapper, bytes, asynchronous),
                "The pending source failure was lost.");
            ClassicAssert.AreEqual(3, wrapper.Position, "Failed source IO changed the position.");
            ClassicAssert.AreEqual(1, await Read(wrapper, bytes, asynchronous), "Reading could not resume after source failure.");
            ClassicAssert.AreEqual(40, bytes[0], "Reading after failure lost source data.");
        }

        [Test]
        public async Task Replay_CancellationPreservesState()
        {
            using var input = new AsyncOnlyStream(new byte[] { 10, 20, 30, 40 }, false);
            using var wrapper = new StreamWrapper(input, 4);
            var bytes = new byte[3];
            ClassicAssert.AreEqual(3, await wrapper.ReadAsync(bytes, 0, bytes.Length, CancellationToken.None), "Initial source reading was incomplete.");
            wrapper.Position = 1;
            input.ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var replayCount = await wrapper.ReadAsync(bytes, 0, bytes.Length, CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(5));

            ClassicAssert.AreEqual(2, replayCount, "Replay unnecessarily waited for source IO.");
            using var cancellation = new CancellationTokenSource();
            var pendingRead = wrapper.ReadAsync(bytes, 0, bytes.Length, cancellation.Token);
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await pendingRead, "Pending source IO ignored cancellation.");
            ClassicAssert.AreEqual(3, wrapper.Position, "Cancellation changed the committed position.");
            input.ReadGate.SetResult(true);

            ClassicAssert.AreEqual(1, await wrapper.ReadAsync(bytes, 0, bytes.Length, CancellationToken.None), "Reading did not resume after cancellation.");
            ClassicAssert.AreEqual(40, bytes[0], "Cancellation lost the next source byte.");
        }

        [Test]
        public async Task Read_EmptyDoesNotTouchSource([Values] bool asynchronous)
        {
            using var input = new InputStream(new byte[] { 42 });
            using var wrapper = new StreamWrapper(input, 4);

            ClassicAssert.AreEqual(0, await Read(wrapper, Array.Empty<byte>(), asynchronous), "An empty read returned bytes.");
            ClassicAssert.AreEqual(0, input.ReadsCount, "An empty read touched the source.");
            ClassicAssert.AreEqual(0, wrapper.Position, "An empty read changed the position.");
            ClassicAssert.IsFalse(wrapper.IsEndReached(), "An empty read incorrectly cached EOF.");
        }

        [Test]
        public void ReplayAndForwardReads_DoNotAllocate([Values(4, 1024, 32768)] int capacity)
        {
            using var input = new InputStream(new byte[capacity + 10000]);
            using var wrapper = new StreamWrapper(input, capacity);
            var bytes = new byte[capacity];
            ClassicAssert.AreEqual(capacity, wrapper.Read(bytes, 0, capacity), "History setup was incomplete.");
            wrapper.Position = 0;
            wrapper.ReadByte();
            var allocatedBeforeReplay = GC.GetAllocatedBytesForCurrentThread();

            for (var index = 1; index < capacity; index++)
            {
                wrapper.ReadByte();
            }

            var replayAllocation = GC.GetAllocatedBytesForCurrentThread() - allocatedBeforeReplay;
            var allocatedBeforeForwardRead = GC.GetAllocatedBytesForCurrentThread();

            for (var index = 0; index < 5000; index++)
            {
                wrapper.ReadByte();
            }

            var forwardAllocation = GC.GetAllocatedBytesForCurrentThread() - allocatedBeforeForwardRead;

            ClassicAssert.AreEqual(0, replayAllocation, "Replay allocated intermediate buffers.");
            ClassicAssert.AreEqual(0, forwardAllocation, "Forward reading allocated replay buffers.");
        }

        [Test]
        public async Task Replay_PreservesOrderAcrossWraparound(
            [Values] bool asynchronous,
            [Values(4, 7, 1024)] int capacity,
            [Values(1, 8)] int maximumReadSize)
        {
            var bytes = Enumerable.Range(0, 140).Select(value => (byte)value).ToArray();
            using var input = new InputStream(bytes, maximumReadSize);
            using var reader = new MidiReader(input, new ReaderSettings { NonSeekableStreamBufferSize = capacity });

            for (var position = 0; position < bytes.Length; position += 7)
            {
                var actualBytes = await ReadBytes(reader, 7, asynchronous);

                ClassicAssert.AreEqual(bytes.Skip(position).Take(7).ToArray(), actualBytes, "Forward data differs after ring wraparound.");
                await EndReached(reader, asynchronous);
                var endPosition = position + 7;
                var rewindPosition = Math.Max(0, endPosition - capacity);
                await SetPosition(reader, rewindPosition, asynchronous);
                var replayBytes = await ReadBytes(reader, endPosition - rewindPosition, asynchronous);

                ClassicAssert.AreEqual(
                    bytes.Skip(rewindPosition).Take(endPosition - rewindPosition).ToArray(),
                    replayBytes,
                    "Replay data differs across the ring boundary.");
                ClassicAssert.AreEqual(endPosition, reader.Position, "Replay did not restore the source frontier.");
            }
        }

        [Test]
        public async Task Read_UsesMinimumReplayCapacity(
            [Values] bool asynchronous,
            [Values(1, 2, 3, 4)] int capacity,
            [Values] bool rmid)
        {
            var expectedFile = new MidiFile(new TrackChunk(
                new NoteOnEvent { Velocity = (SevenBitNumber)64 },
                new NoteOnEvent { Velocity = (SevenBitNumber)64 }));
            using var input = new InputStream(FileBytes(expectedFile, rmid), 1);
            var settings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings { NonSeekableStreamBufferSize = capacity }
            };

            var actualFile = await ReadFile(input, settings, asynchronous);

            MidiAsserts.AreEqual(expectedFile, actualFile, false, "Minimum-capacity reading lost format look-ahead or running status.");
        }

        [Test]
        public async Task Read_StopAtTrackCountPreservesNextFile([Values] bool asynchronous)
        {
            var expectedFile = new MidiFile(new TrackChunk(new TextEvent("payload")));
            var bytes = FileBytes(expectedFile);
            using var input = new InputStream(bytes.Concat(bytes).ToArray(), 3);
            var settings = new ReadingSettings { StopReadingOnExpectedTrackChunksCountReached = true };

            var firstFile = await ReadFile(input, settings, asynchronous);

            ClassicAssert.AreEqual(bytes.Length, input.BytesRead, "Reading consumed a byte from the next MIDI file.");
            var secondFile = await ReadFile(input, settings, asynchronous);

            MidiAsserts.AreEqual(expectedFile, firstFile, false, "The first concatenated file differs.");
            MidiAsserts.AreEqual(expectedFile, secondFile, false, "The second concatenated file differs.");
        }

        [Test]
        public async Task Read_RmidBoundaryDoesNotProbeSource(
            [Values] bool asynchronous,
            [Values] bool lazy)
        {
            var expectedFile = new MidiFile(new TrackChunk(new TextEvent("payload")));
            var bytes = FileBytes(expectedFile, true);
            using var input = new InputStream(bytes, 1) { ThrowAtEnd = true };

            if (lazy)
            {
                using var reader = asynchronous ? await MidiFile.ReadLazyAsync(input) : MidiFile.ReadLazy(input);

                while ((asynchronous ? await reader.ReadTokenAsync() : reader.ReadToken()) != null)
                {
                }
            }
            else
            {
                var actualFile = await ReadFile(input, new ReadingSettings(), asynchronous);

                MidiAsserts.AreEqual(expectedFile, actualFile, false, "RMID reading changed MIDI content.");
            }

            ClassicAssert.AreEqual(bytes.Length, input.BytesRead, "RMID reading did not stop at its declared boundary.");
        }

        [Test]
        public void Read_TruncatedTrackReportsActualSize([Values] bool asynchronous)
        {
            var bytes = DeclaredChunkBytes("MTrk", 8, new byte[] { 0, 0x90, 60, 64 });
            using var input = new InputStream(bytes, 1);
            var settings = new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore };

            var exception = Assert.ThrowsAsync<InvalidChunkSizeException>(
                async () => await ReadFile(input, settings, asynchronous),
                "Strict chunk validation accepted a truncated track.");

            ClassicAssert.AreEqual(4, exception.ActualSize, "Chunk validation used the EOF sentinel instead of bytes consumed.");
            ClassicAssert.AreEqual(8, exception.ExpectedSize, "Chunk validation changed the declared size.");
        }

        [Test]
        public async Task ReadLazy_TruncatedPacketHasActualLength(
            [Values] bool asynchronous,
            [Values(4u, uint.MaxValue)] uint declaredSize)
        {
            using var input = new InputStream(DeclaredChunkBytes("Test", declaredSize, new byte[] { 1, 2, 3 }), 1);
            var settings = new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore };
            using var reader = asynchronous ? await MidiFile.ReadLazyAsync(input, settings) : MidiFile.ReadLazy(input, settings);
            BytesPacketToken packet = null;
            MidiToken token;

            while ((token = asynchronous ? await reader.ReadTokenAsync() : reader.ReadToken()) != null)
            {
                if (token is BytesPacketToken bytesPacket)
                    packet = bytesPacket;
            }

            ClassicAssert.IsNotNull(packet, "Lazy reading lost the available payload.");
            ClassicAssert.AreEqual(22, packet.Position, "The packet position differs.");
            ClassicAssert.AreEqual(3, packet.Length, "The packet length is not the number of bytes consumed.");
        }

        [Test]
        public async Task Read_UnknownChunkAccountsForActualPayload([Values] bool asynchronous)
        {
            var bytes = DeclaredChunkBytes("Test", uint.MaxValue, new byte[] { 1, 2, 3 });
            var settings = new ReadingSettings
            {
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                MaxMemorySize = 10000
            };
            var expectedFile = MidiFile.Read(new MemoryStream(bytes), settings);
            using var input = new InputStream(bytes, 1);

            var actualFile = await ReadFile(input, settings, asynchronous);

            MidiAsserts.AreEqual(expectedFile, actualFile, true, "Unknown-length reading lost available content.");
            ClassicAssert.AreEqual(expectedFile.EstimatedMemorySize, actualFile.EstimatedMemorySize, "Memory accounting used the declared payload size.");
        }

        [Test]
        public void Read_UnknownChunkStopsAtMemoryLimit([Values] bool asynchronous)
        {
            using var input = new InputStream(DeclaredChunkBytes("Test", uint.MaxValue, new byte[20000]), 3);
            var settings = new ReadingSettings { MaxMemorySize = 10000 };

            Assert.ThrowsAsync<MidiFileTooLargeException>(
                async () => await ReadFile(input, settings, asynchronous),
                "Incremental unknown-chunk reading ignored the retained-memory limit.");

            ClassicAssert.Less(input.BytesRead, 20022, "Memory-limit failure consumed the entire payload.");
        }

        private static Task<byte[]> ReadBytes(MidiReader reader, int count, bool asynchronous)
        {
            return asynchronous ? reader.ReadBytesAsync(count, CancellationToken.None) : Task.FromResult(reader.ReadBytes(count));
        }

        private static ValueTask<bool> EndReached(MidiReader reader, bool asynchronous)
        {
            return asynchronous ? reader.EndReachedAsync(CancellationToken.None) : new ValueTask<bool>(reader.EndReached);
        }

        private static ValueTask SetPosition(MidiReader reader, long position, bool asynchronous)
        {
            if (asynchronous)
                return reader.SetPositionAsync(position, CancellationToken.None);

            reader.Position = position;

            return default;
        }

        private static Task<int> Read(Stream stream, byte[] bytes, bool asynchronous)
        {
            return asynchronous
                ? stream.ReadAsync(bytes, 0, bytes.Length, CancellationToken.None)
                : Task.FromResult(stream.Read(bytes, 0, bytes.Length));
        }

        private static Task<MidiFile> ReadFile(Stream stream, ReadingSettings settings, bool asynchronous)
        {
            return asynchronous ? MidiFile.ReadAsync(stream, settings, CancellationToken.None) : Task.FromResult(MidiFile.Read(stream, settings));
        }

        private static byte[] FileBytes(MidiFile file, bool rmid = false)
        {
            using var output = new MemoryStream();
            file.Write(output);
            var bytes = output.ToArray();
            if (!rmid)
                return bytes;

            using var rmidOutput = new MemoryStream();
            using var writer = new MidiWriter(rmidOutput, new WriterSettings { UseBuffering = false });
            writer.WriteString("RIFF");
            writer.WriteDword(0);
            writer.WriteString("RMID");
            writer.WriteString("data");
            writer.WriteDword((uint)bytes.Length);
            writer.WriteBytes(bytes);

            return rmidOutput.ToArray();
        }

        private static byte[] DeclaredChunkBytes(string chunkId, uint size, byte[] payload)
        {
            using var output = new MemoryStream();
            new MidiFile().Write(output);
            using var writer = new MidiWriter(output, new WriterSettings { UseBuffering = false });
            writer.WriteString(chunkId);
            writer.WriteDword(size);
            writer.WriteBytes(payload);

            return output.ToArray();
        }
    }
}
