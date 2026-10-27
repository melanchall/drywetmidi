using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileAsyncTests
    {
        private sealed class AsyncCustomChunk : MidiChunk
        {
            public AsyncCustomChunk()
                : base("Asyn")
            {
            }

            public byte[] Data { get; set; } = Array.Empty<byte>();

            public byte[] Retained { get; private set; }

            public long ContentPosition { get; private set; }

            public long ContentLength { get; private set; }

            public ReadingSettings ReadSettings { get; private set; }

            public WritingSettings WriteSettings { get; private set; }

            public TaskCompletionSource<bool> ReadEntered { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            public TaskCompletionSource<bool> ReadGate { get; set; }

            public Action BeforeWrite { get; set; }

            public override MidiChunk Clone() => new AsyncCustomChunk { Data = (byte[])Data.Clone() };

            protected override uint GetContentSize(WritingSettings settings) => (uint)Data.Length;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                throw new AssertionException("Synchronous custom read hook was selected.");
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                throw new AssertionException("Synchronous custom write hook was selected.");
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                ContentPosition = reader.Position;
                ContentLength = reader.Length;
                ReadSettings = settings;
                ReadEntered.TrySetResult(true);
                if (ReadGate != null)
                    await ReadGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

                Data = await reader.ReadBytesAsync(checked((int)size), cancellationToken).ConfigureAwait(false);
                if (Data.Length > 0 && Data[0] == 255)
                    Retained = new byte[100000];
            }

            protected override Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                WriteSettings = settings;
                BeforeWrite?.Invoke();
                return writer.WriteBytesAsync(Data, cancellationToken);
            }
        }

        private class PartialChunk : MidiChunk
        {
            public PartialChunk()
                : base("Part")
            {
            }

            public byte Value { get; protected set; }

            public override MidiChunk Clone() => new PartialChunk();

            protected override uint GetContentSize(WritingSettings settings) => 2;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                Value = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteBytes(new byte[] { 17, 18 });
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                Value = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                return writer.WriteBytesAsync(new byte[] { 17, 18 }, cancellationToken);
            }
        }

        private sealed class PartialArrayChunk : PartialChunk
        {
            public PartialArrayChunk()
            {
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                Value = (await reader.ReadBytesAsync(1, cancellationToken).ConfigureAwait(false))[0];
            }
        }

        private sealed class SyncOnlyChunk : PartialChunk
        {
            public SyncOnlyChunk()
            {
            }

            protected override Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                throw new NotSupportedException("This custom chunk supports only synchronous reading.");
            }

            protected override Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                throw new NotSupportedException("This custom chunk supports only synchronous writing.");
            }
        }

        [Test]
        public void CustomChunk_SyncOnly_ExplicitlyRejectsAsyncIO()
        {
            using var bytes = new MemoryStream();
            new MidiFile(new SyncOnlyChunk()).Write(bytes);
            var settings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(SyncOnlyChunk), "Part" } },
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
            };
            bytes.Position = 0;
            var synchronous = MidiFile.Read(bytes, settings);

            ClassicAssert.AreEqual(17, ((SyncOnlyChunk)synchronous.Chunks[0]).Value, "Synchronous custom chunk reading failed.");

            using var input = new AsyncOnlyStream(bytes.ToArray());
            using var output = new AsyncOnlyStream();

            Assert.ThrowsAsync<NotSupportedException>(
                async () => await MidiFile.ReadAsync(input, settings),
                "Async reading ignored the custom chunk's explicit unsupported-operation override.");
            Assert.ThrowsAsync<NotSupportedException>(
                async () => await new MidiFile(new SyncOnlyChunk()).WriteAsync(output),
                "Async writing ignored the custom chunk's explicit unsupported-operation override.");
            ClassicAssert.IsFalse(input.IsDisposed, "Unsupported reading disposed the caller's stream.");
            ClassicAssert.IsFalse(output.IsDisposed, "Unsupported writing disposed the caller's stream.");
        }

        [Test]
        public async Task CustomChunk_AsyncHooks_RoundTrip(
            [Values] bool lazyWrite,
            [Values] bool nonSeekable,
            [Values] bool buffering)
        {
            var chunk = new AsyncCustomChunk { Data = new byte[] { 1, 2, 3, 4 } };
            var writingSettings = new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = buffering, BufferSize = 3 }
            };
            using var output = new AsyncOnlyStream();

            if (lazyWrite)
            {
                await using var writer = await MidiFile.WriteLazyAsync(output, writingSettings);
                await writer.WriteChunkAsync(chunk);
                await writer.WriteChunkAsync(new TrackChunk(new TextEvent("following chunk")));
                await writer.WriteChunkAsync(new TrackChunk());
            }
            else
            {
                await new MidiFile(chunk, new TrackChunk(new TextEvent("following chunk")), new TrackChunk())
                    .WriteAsync(output, settings: writingSettings);
            }

            var bytes = output.ToArray();
            var readingSettings = AsyncCustomReadingSettings();
            using var input = new AsyncOnlyStream(bytes, !nonSeekable, 1);
            var actual = await MidiFile.ReadAsync(input, readingSettings);
            var readChunk = (AsyncCustomChunk)actual.Chunks[0];

            ClassicAssert.AreEqual(chunk.Data, readChunk.Data, "Async custom content differs.");
            ClassicAssert.AreEqual(22, readChunk.ContentPosition, "The hook did not receive the original content position.");
            ClassicAssert.AreEqual(nonSeekable ? long.MaxValue : bytes.Length, readChunk.ContentLength, "The hook received a payload-only buffered reader.");
            ClassicAssert.AreSame(readingSettings, readChunk.ReadSettings, "Reading settings were not passed to the hook.");
            ClassicAssert.AreSame(writingSettings, chunk.WriteSettings, "Writing settings were not passed to the hook.");
            ClassicAssert.AreEqual(3, actual.Chunks.Count, "Reading consumed a following chunk.");
            ClassicAssert.IsFalse(input.IsDisposed, "Reading disposed the caller's stream.");
            ClassicAssert.IsFalse(output.IsDisposed, "Writing disposed the caller's stream.");

            using var tokenInput = new AsyncOnlyStream(bytes, !nonSeekable, 1);
            await using var tokenReader = await MidiFile.ReadLazyAsync(tokenInput, readingSettings);
            var payload = new List<byte>();

            await foreach (var token in tokenReader.EnumerateTokensAsync())
            {
                if (token is BytesPacketToken packet)
                    payload.AddRange(packet.Data);
            }

            ClassicAssert.AreEqual(chunk.Data, payload, "Lazy reading changed the custom chunk payload.");
            ClassicAssert.IsFalse(tokenInput.IsDisposed, "Lazy reading disposed the caller's stream.");
        }

        [Test]
        public async Task CustomChunk_ReadHook_EntersBeforePayloadIsBuffered()
        {
            using var input = new AsyncOnlyStream(new byte[] { 1, 2, 3 }, false, 1);
            using var reader = new MidiReader(input, new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering });
            using var cancellation = new CancellationTokenSource();
            var chunk = new AsyncCustomChunk
            {
                ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
            };

            var pending = chunk.ReadAsync(reader, new ReadingSettings(), 3, cancellation.Token);
            await chunk.ReadEntered.Task;

            ClassicAssert.AreEqual(0, input.BytesRead, "The engine buffered the payload before invoking the async hook.");

            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await pending, "Cancellation was not passed to the hook.");
            ClassicAssert.IsFalse(input.IsDisposed, "Cancellation disposed the caller's stream.");
        }

        [Test]
        public async Task CustomChunk_WriteAsync_CancelsDuringHook([Values] bool lazyWrite)
        {
            using var output = new AsyncOnlyStream();
            using var cancellation = new CancellationTokenSource();
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var chunk = new AsyncCustomChunk
            {
                Data = new byte[] { 1, 2, 3 },
                BeforeWrite = () =>
                {
                    output.WriteGate = gate;
                    entered.TrySetResult(true);
                }
            };
            var settings = new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            };
            MidiTokensWriter lazyWriter = null;
            try
            {
                if (lazyWrite)
                    lazyWriter = await MidiFile.WriteLazyAsync(output, settings);

                var pending = lazyWrite
                    ? lazyWriter.WriteChunkAsync(chunk, cancellation.Token)
                    : new MidiFile(chunk).WriteAsync(output, settings: settings, cancellationToken: cancellation.Token);
                await entered.Task;
                cancellation.Cancel();

                Assert.CatchAsync<OperationCanceledException>(
                    async () => await pending,
                    "Cancellation did not interrupt the asynchronous custom content write.");
                ClassicAssert.IsFalse(output.IsDisposed, "Cancellation disposed the caller's output stream.");
            }
            finally
            {
                gate.TrySetResult(true);
                if (lazyWriter != null)
                    await lazyWriter.DisposeAsync();
            }
        }

        [Test]
        public async Task CustomChunk_ReadAsync_RetainedMemoryIsCounted()
        {
            using var output = new AsyncOnlyStream();
            await new MidiFile(new AsyncCustomChunk { Data = new byte[] { 255 } }).WriteAsync(output);
            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            var settings = AsyncCustomReadingSettings();
            settings.MaxMemorySize = 10000;

            Assert.ThrowsAsync<MidiFileTooLargeException>(
                async () => await MidiFile.ReadAsync(input, settings),
                "Memory allocated and retained by the asynchronous hook was not counted.");
        }

        [Test]
        public void CustomChunk_ReadAsync_DeclaredMemoryLimitIsCheckedBeforeHook()
        {
            using var input = new AsyncOnlyStream(new byte[10000]);
            using var reader = new MidiReader(input, new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering });
            var chunk = new AsyncCustomChunk();

            Assert.ThrowsAsync<MidiFileTooLargeException>(
                async () => await chunk.ReadAsync(reader, new ReadingSettings { MaxMemorySize = 100 }, 10000, CancellationToken.None),
                "The declared custom payload was not checked against the memory limit.");

            ClassicAssert.IsFalse(chunk.ReadEntered.Task.IsCompleted, "The hook was invoked after its declared size exceeded the memory limit.");
            ClassicAssert.AreEqual(0, input.BytesRead, "A rejected chunk consumed payload bytes.");
        }

        [Test]
        public async Task CustomChunk_ReadAsync_SizePolicy(
            [Values] InvalidChunkSizePolicy policy,
            [Values] bool arrayRead)
        {
            using var output = new MemoryStream();
            new MidiFile(new PartialArrayChunk(), new UnknownChunk("Next") { Data = new byte[] { 19 } }).Write(output);
            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            var settings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection
                {
                    { arrayRead ? typeof(PartialArrayChunk) : typeof(PartialChunk), "Part" }
                },
                InvalidChunkSizePolicy = policy
            };

            if (policy == InvalidChunkSizePolicy.Abort)
            {
                Assert.ThrowsAsync<InvalidChunkSizeException>(
                    async () => await MidiFile.ReadAsync(input, settings),
                    "A partially consumed custom chunk did not fail the Abort policy.");
            }
            else
            {
                var file = await MidiFile.ReadAsync(input, settings);

                ClassicAssert.AreEqual(17, ((PartialChunk)file.Chunks[0]).Value, "The hook read an invalid value.");
                ClassicAssert.AreEqual(new byte[] { 19 }, ((UnknownChunk)file.Chunks[1]).Data, "Skipping unread content damaged the next chunk.");
            }
        }

        [Test]
        public async Task CustomChunk_ReadAsync_TruncatedPayload([Values] InvalidChunkSizePolicy policy)
        {
            using var output = new AsyncOnlyStream();
            await new MidiFile(new AsyncCustomChunk { Data = new byte[] { 1, 2, 3 } }).WriteAsync(output);
            var bytes = output.ToArray().Take(23).ToArray();
            using var input = new AsyncOnlyStream(bytes, false, 1);
            var settings = AsyncCustomReadingSettings();
            settings.InvalidChunkSizePolicy = policy;

            if (policy == InvalidChunkSizePolicy.Abort)
            {
                Assert.ThrowsAsync<InvalidChunkSizeException>(
                    async () => await MidiFile.ReadAsync(input, settings),
                    "Truncated custom content did not fail size validation.");
            }
            else
            {
                var file = await MidiFile.ReadAsync(input, settings);

                ClassicAssert.AreEqual(new byte[] { 1 }, ((AsyncCustomChunk)file.Chunks[0]).Data, "Truncated content was not preserved.");
            }
        }

        [Test]
        public async Task AsyncPrimitives_RoundTrip([Values] bool buffering)
        {
            using var output = new AsyncOnlyStream();
            await using (var writer = new MidiWriter(output, new WriterSettings { UseBuffering = buffering, BufferSize = 3 }))
            {
                await writer.WriteByteAsync(42);
                await writer.WriteSByteAsync(-12);
                await writer.WriteWordAsync(54321);
                await writer.WriteDwordAsync(0xF1234567);
                await writer.WriteInt16Async(-12345);
                await writer.Write3ByteDwordAsync(0xABCDEF);
                await writer.WriteVlqNumberAsync(int.MaxValue);
                await writer.WriteVlqNumberAsync(long.MaxValue);
                await writer.WriteStringAsync("Test");
                await writer.WriteBytesAsync(new byte[] { 0, 7, 8, 0 }, 1, 2);
            }

            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            using var reader = new MidiReader(input, new ReaderSettings());

            ClassicAssert.AreEqual(42, await reader.ReadByteAsync(), "Byte round-trip failed.");
            ClassicAssert.AreEqual(-12, await reader.ReadSByteAsync(), "Signed byte round-trip failed.");
            ClassicAssert.AreEqual(54321, await reader.ReadWordAsync(), "WORD round-trip failed.");
            ClassicAssert.AreEqual(0xF1234567, await reader.ReadDwordAsync(), "DWORD round-trip failed.");
            ClassicAssert.AreEqual(-12345, await reader.ReadInt16Async(), "INT16 round-trip failed.");
            ClassicAssert.AreEqual(0xABCDEF, await reader.Read3ByteDwordAsync(), "Three-byte DWORD round-trip failed.");
            ClassicAssert.AreEqual(int.MaxValue, await reader.ReadVlqNumberAsync(), "Int VLQ round-trip failed.");
            ClassicAssert.AreEqual(long.MaxValue, await reader.ReadVlqLongNumberAsync(), "Long VLQ round-trip failed.");
            ClassicAssert.AreEqual("Test", await reader.ReadStringAsync(4), "String round-trip failed.");
            ClassicAssert.AreEqual(new byte[] { 7, 8 }, await reader.ReadBytesAsync(2), "Byte range round-trip failed.");
            ClassicAssert.IsTrue(await reader.EndReachedAsync(), "The reader did not reach the end.");
        }

        [Test]
        public void AsyncPrimitives_ValidateArguments()
        {
            using var stream = new MemoryStream();
            using var reader = new MidiReader(stream, new ReaderSettings());
            using var writer = new MidiWriter(stream, new WriterSettings());

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await reader.ReadBytesAsync(-1), "Negative byte count was accepted.");
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await reader.ReadStringAsync(-1), "Negative string count was accepted.");
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await reader.SetPositionAsync(-1), "Negative position was accepted.");
            Assert.Throws<ArgumentNullException>(() => writer.WriteBytesAsync(null), "Null byte array was accepted.");
            Assert.Throws<ArgumentNullException>(() => writer.WriteBytesAsync(null, 0, 0), "Null byte range was accepted.");
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBytesAsync(new byte[2], 3, 0), "Invalid offset was accepted.");
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBytesAsync(new byte[2], 1, 2), "Invalid length was accepted.");
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBytesAsync(new byte[2], 0, -1), "Negative length was accepted.");
        }

        [Test]
        public async Task AsyncPrimitives_ZeroLengthOperations_DoNotTouchStream()
        {
            using var input = new AsyncOnlyStream(new byte[] { 1 });
            using var reader = new MidiReader(input, new ReaderSettings());
            using var output = new AsyncOnlyStream();
            await using var writer = new MidiWriter(output, new WriterSettings { UseBuffering = false });

            var bytes = await reader.ReadBytesAsync(0);
            var text = await reader.ReadStringAsync(0);
            await writer.WriteBytesAsync(Array.Empty<byte>());
            await writer.WriteBytesAsync(new byte[] { 1 }, 1, 0);
            await writer.WriteStringAsync(null);
            await writer.WriteStringAsync(string.Empty);

            ClassicAssert.IsEmpty(bytes, "A zero-length read returned bytes.");
            ClassicAssert.AreEqual(string.Empty, text, "A zero-length string read returned characters.");
            ClassicAssert.AreEqual(0, input.AsyncReads, "A zero-length read touched the stream.");
            ClassicAssert.AreEqual(0, output.AsyncWrites, "A zero-length write touched the stream.");
            ClassicAssert.AreEqual(0, writer.Length, "Zero-length writes changed the output length.");
        }

        [Test]
        public void AsyncPrimitives_PreserveTruncationAndOverflowFailures()
        {
            using var input = new AsyncOnlyStream(new byte[] { 0x80 }, false, 1);
            using var reader = new MidiReader(input, new ReaderSettings());

            Assert.ThrowsAsync<NotEnoughBytesException>(
                async () => await reader.ReadVlqNumberAsync(),
                "A truncated VLQ did not produce the synchronous exception type.");

            using var overflowInput = new AsyncOnlyStream(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, false, 1);
            using var overflowReader = new MidiReader(overflowInput, new ReaderSettings());

            Assert.ThrowsAsync<VlqNumberOverflowException>(
                async () => await overflowReader.ReadVlqNumberAsync(),
                "An overflowing VLQ was accepted.");

            using var shortInput = new AsyncOnlyStream(new byte[] { 1 }, false, 1);
            using var shortReader = new MidiReader(shortInput, new ReaderSettings());

            Assert.ThrowsAsync<NotEnoughBytesException>(
                async () => await shortReader.ReadWordAsync(),
                "A truncated WORD was accepted.");
        }

        [Test]
        public async Task AsyncPrimitives_DisposedAndCanceledOperationsDoNotTouchStream()
        {
            using var input = new AsyncOnlyStream(new byte[] { 1 });
            using var reader = new MidiReader(input, new ReaderSettings());
            using var output = new AsyncOnlyStream();
            var writer = new MidiWriter(output, new WriterSettings { UseBuffering = false });
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await reader.ReadBytesAsync(0, cancellation.Token),
                "An empty canceled read was accepted.");
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await writer.WriteBytesAsync(Array.Empty<byte>(), cancellation.Token),
                "An empty canceled write was accepted.");
            ClassicAssert.AreEqual(0, input.AsyncReads, "A canceled read touched the stream.");
            ClassicAssert.AreEqual(0, output.AsyncWrites, "A canceled write touched the stream.");

            reader.Dispose();
            await writer.DisposeAsync();

            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await reader.ReadByteAsync(),
                "A disposed reader accepted an asynchronous operation.");
            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await reader.ReadBytesAsync(0),
                "A disposed reader accepted a zero-length asynchronous operation.");
            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await writer.WriteByteAsync(1),
                "A disposed writer accepted an asynchronous operation.");
            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await writer.WriteBytesAsync(Array.Empty<byte>()),
                "A disposed writer accepted a zero-length asynchronous operation.");
            ClassicAssert.IsFalse(input.IsDisposed, "Reader disposal closed the caller's stream.");
            ClassicAssert.IsFalse(output.IsDisposed, "Writer disposal closed the caller's stream.");
        }

        private static ReadingSettings AsyncCustomReadingSettings()
        {
            return new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(AsyncCustomChunk), "Asyn" } },
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            };
        }
    }
}
