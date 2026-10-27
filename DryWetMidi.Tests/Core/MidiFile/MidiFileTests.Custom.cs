using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private class AsyncCustomChunk : MidiChunk
        {
            public AsyncCustomChunk()
                : base("Asyn")
            {
            }

            public byte Value { get; set; }

            public long ContentPosition { get; private set; }

            public CancellationToken HookToken { get; private set; }

            public ReadingSettings ReadSettings { get; private set; }

            public WritingSettings WriteSettings { get; private set; }

            public Action<CancellationToken> BeforeWrite { get; set; }

            public override MidiChunk Clone() => new AsyncCustomChunk { Value = Value };

            protected override uint GetContentSize(WritingSettings settings) => 1;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size) =>
                throw new AssertionException("The synchronous custom chunk read hook was invoked.");

            protected override void WriteContent(MidiWriter writer, WritingSettings settings) =>
                throw new AssertionException("The synchronous custom chunk write hook was invoked.");

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                ContentPosition = reader.Position;
                HookToken = cancellationToken;
                ReadSettings = settings;
                Value = await reader.ReadByteAsync(cancellationToken);
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                ContentPosition = writer.Length;
                HookToken = cancellationToken;
                WriteSettings = settings;
                BeforeWrite?.Invoke(cancellationToken);

                await writer.WriteByteAsync(Value, cancellationToken);
            }
        }

        private sealed class InheritedAsyncCustomChunk : AsyncCustomChunk
        {
            public InheritedAsyncCustomChunk()
            {
            }
        }

        private sealed class AllocatingAsyncCustomChunk : AsyncCustomChunk
        {
            private byte[] _retained;

            public AllocatingAsyncCustomChunk()
            {
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                await base.ReadContentAsync(reader, settings, size, cancellationToken);
                _retained = new byte[100000];
            }
        }

        private sealed class CancelingAsyncCustomChunk : AsyncCustomChunk
        {
            public CancelingAsyncCustomChunk()
            {
            }

            public static CancellationTokenSource Cancellation { get; set; }

            public static CancellationToken ReceivedToken { get; private set; }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                ReceivedToken = cancellationToken;
                await reader.ReadByteAsync(cancellationToken);
                Cancellation.Cancel();

                await reader.ReadByteAsync(cancellationToken);
            }
        }

        private sealed class CustomMetaEvent : MetaEvent
        {
            #region Constructor

            public CustomMetaEvent()
                : base()
            {
            }

            public CustomMetaEvent(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            #endregion

            #region Overrides

            protected override MidiEvent CloneEvent()
            {
                return new CustomMetaEvent(A, B, C);
            }

            protected override int GetContentSize(WritingSettings settings)
            {
                return DataTypesUtilities.GetVlqLength(A) +
                       DataTypesUtilities.GetVlqLength(B?.Length ?? 0) +
                       (B?.Length ?? 0) +
                       1;
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
            {
                A = reader.ReadVlqNumber();

                var bLength = reader.ReadVlqNumber();
                B = reader.ReadString(bLength);

                C = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            public override bool Equals(object obj) => obj is CustomMetaEvent customEvent &&
                customEvent.A == A &&
                customEvent.B == B &&
                customEvent.C == C;

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = 17;
                    result = result * 23 + A.GetHashCode();
                    result = result * 23 + (B?.GetHashCode() ?? 0);
                    result = result * 23 + C.GetHashCode();
                    return result;
                }
            }

            #endregion
        }

        private sealed class CustomChunk : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public CustomChunk()
                : base(Id)
            {
            }

            public CustomChunk(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            #endregion

            #region Overrides

            public override MidiChunk Clone()
            {
                return new CustomChunk(A, B, C);
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                return (uint)(DataTypesUtilities.GetVlqLength(A) +
                              DataTypesUtilities.GetVlqLength(B?.Length ?? 0) +
                              (B?.Length ?? 0) +
                              1);
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                A = reader.ReadVlqNumber();

                var bLength = reader.ReadVlqNumber();
                B = reader.ReadString(bLength);

                C = reader.ReadByte();
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                A = await reader.ReadVlqNumberAsync(cancellationToken).ConfigureAwait(false);

                var bLength = await reader.ReadVlqNumberAsync(cancellationToken).ConfigureAwait(false);
                B = await reader.ReadStringAsync(bLength, cancellationToken).ConfigureAwait(false);

                C = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                await writer.WriteVlqNumberAsync(A, cancellationToken).ConfigureAwait(false);
                await writer.WriteVlqNumberAsync(B?.Length ?? 0, cancellationToken).ConfigureAwait(false);
                await writer.WriteStringAsync(B, cancellationToken).ConfigureAwait(false);
                await writer.WriteByteAsync(C, cancellationToken).ConfigureAwait(false);
            }

            public override bool Equals(object obj) => obj is CustomChunk customChunk &&
                customChunk.A == A &&
                customChunk.B == B &&
                customChunk.C == C;

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = 17;
                    result = result * 23 + A.GetHashCode();
                    result = result * 23 + B.GetHashCode();
                    result = result * 23 + C.GetHashCode();
                    return result;
                }
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public async Task ReadWriteCustomChunk_AsyncHooks(
            [Values] bool lazyWrite,
            [Values] bool inherited,
            [Values] bool seekable)
        {
            AsyncCustomChunk expected = inherited
                ? new InheritedAsyncCustomChunk()
                : new AsyncCustomChunk();
            expected.Value = 42;
            using var cancellation = new CancellationTokenSource();
            using var output = new MidiFileAsyncTests.AsyncOnlyStream(seekable: lazyWrite || seekable);
            var writingSettings = new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            };

            if (lazyWrite)
            {
                await using var writer = await MidiFile.WriteLazyAsync(output, writingSettings);
                await writer.WriteChunkAsync(expected, cancellation.Token);
            }
            else
            {
                await new MidiFile(expected).WriteAsync(
                    output,
                    settings: writingSettings,
                    cancellationToken: cancellation.Token);
            }

            ClassicAssert.AreEqual(cancellation.Token, expected.HookToken, "The write hook received a different cancellation token.");
            ClassicAssert.AreSame(writingSettings, expected.WriteSettings, "The write hook received different settings.");
            ClassicAssert.AreEqual(22, expected.ContentPosition, "The write hook received an invalid absolute position.");
            ClassicAssert.IsFalse(output.IsDisposed, "Async chunk writing disposed the caller-owned stream.");

            var readingSettings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { expected.GetType(), "Asyn" } },
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            };
            using var input = new MidiFileAsyncTests.AsyncOnlyStream(output.ToArray(), seekable, 1);
            var file = await MidiFile.ReadAsync(input, readingSettings, cancellationToken: cancellation.Token);
            var actual = (AsyncCustomChunk)file.Chunks.Single();

            ClassicAssert.AreEqual(expected.GetType(), actual.GetType(), "The registered custom chunk type was not created.");
            ClassicAssert.AreEqual(42, actual.Value, "The async read hook returned an invalid value.");
            ClassicAssert.AreEqual(22, actual.ContentPosition, "The read hook received an invalid absolute position.");
            ClassicAssert.AreEqual(cancellation.Token, actual.HookToken, "The read hook received a different cancellation token.");
            ClassicAssert.AreSame(readingSettings, actual.ReadSettings, "The read hook received different settings.");
            ClassicAssert.IsFalse(input.IsDisposed, "Async chunk reading disposed the caller-owned stream.");

            using var lazyInput = new MidiFileAsyncTests.AsyncOnlyStream(output.ToArray(), seekable, 1);
            await using var lazyReader = await MidiFile.ReadLazyAsync(lazyInput, readingSettings);
            var payload = new System.Collections.Generic.List<byte>();

            await foreach (var token in lazyReader.EnumerateTokensAsync())
            {
                if (token is BytesPacketToken packet)
                    payload.AddRange(packet.Data);
            }

            ClassicAssert.AreEqual(new byte[] { 42 }, payload, "Lazy reading returned different custom chunk bytes.");
            ClassicAssert.IsFalse(lazyInput.IsDisposed, "Lazy chunk reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task ReadCustomChunk_AsyncFraming_InvalidSize(
            [Values] InvalidChunkSizePolicy policy,
            [Values] bool seekable,
            [Values] bool asyncHook)
        {
            var chunkType = asyncHook ? typeof(AsyncCustomChunk) : typeof(CustomChunk);
            var chunkId = asyncHook ? "Asyn" : CustomChunk.Id;
            using var bytes = new MemoryStream();
            new MidiFile(
                new UnknownChunk(chunkId) { Data = new byte[] { 42, 0, 43, 0xFE, 0xFF } },
                new UnknownChunk("Next") { Data = new byte[] { 7 } }).Write(bytes);
            using var input = new MidiFileAsyncTests.AsyncOnlyStream(bytes.ToArray(), seekable, 1);
            var settings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { chunkType, chunkId } },
                InvalidChunkSizePolicy = policy
            };

            if (policy == InvalidChunkSizePolicy.Abort)
            {
                Assert.ThrowsAsync<InvalidChunkSizeException>(
                    async () => await MidiFile.ReadAsync(input, settings),
                    "Async custom chunk under-reading did not enforce the declared size.");
            }
            else
            {
                var file = await MidiFile.ReadAsync(input, settings);

                ClassicAssert.AreEqual(2, file.Chunks.Count, "Ignoring the size mismatch lost the following chunk.");
                var value = asyncHook
                    ? ((AsyncCustomChunk)file.Chunks[0]).Value
                    : ((CustomChunk)file.Chunks[0]).A;

                ClassicAssert.AreEqual(42, value, "The custom chunk value is invalid.");
                ClassicAssert.AreEqual("Next", file.Chunks[1].ChunkId, "Unread custom bytes were not skipped.");
                ClassicAssert.AreEqual(new byte[] { 7 }, ((UnknownChunk)file.Chunks[1]).Data, "The following chunk payload is invalid.");
            }

            ClassicAssert.IsFalse(input.IsDisposed, "Chunk framing disposed the caller-owned stream.");
        }

        [Test]
        public void ReadCustomChunk_AsyncHook_EnforcesRetainedMemoryLimit()
        {
            using var bytes = new MemoryStream();
            new MidiFile(new UnknownChunk("Asyn") { Data = new byte[] { 42 } }).Write(bytes);
            using var input = new MidiFileAsyncTests.AsyncOnlyStream(bytes.ToArray(), false, 1);
            var settings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(AllocatingAsyncCustomChunk), "Asyn" } },
                MaxMemorySize = 10000
            };

            Assert.ThrowsAsync<MidiFileTooLargeException>(
                async () => await MidiFile.ReadAsync(input, settings),
                "Memory allocated by the async custom hook escaped the retained-memory limit.");
            ClassicAssert.IsFalse(input.IsDisposed, "Custom allocation failure disposed the caller-owned stream.");
        }

        [Test]
        public void ReadCustomChunk_AsyncHook_Cancellation()
        {
            using var bytes = new MemoryStream();
            new MidiFile(new UnknownChunk("Asyn") { Data = new byte[] { 42, 43 } }).Write(bytes);
            using var input = new MidiFileAsyncTests.AsyncOnlyStream(bytes.ToArray(), false, 1);
            using var cancellation = new CancellationTokenSource();
            CancelingAsyncCustomChunk.Cancellation = cancellation;
            var settings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(CancelingAsyncCustomChunk), "Asyn" } }
            };

            try
            {
                Assert.CatchAsync<OperationCanceledException>(
                    async () => await MidiFile.ReadAsync(input, settings, cancellationToken: cancellation.Token),
                    "Cancellation inside the custom read hook did not interrupt reading.");
                ClassicAssert.AreEqual(cancellation.Token, CancelingAsyncCustomChunk.ReceivedToken, "The read hook did not receive the caller's token.");
                ClassicAssert.IsFalse(input.IsDisposed, "Canceled custom reading disposed the caller-owned stream.");
            }
            finally
            {
                CancelingAsyncCustomChunk.Cancellation = null;
            }
        }

        [Test]
        public async Task WriteCustomChunk_AsyncHook_FailureLeavesStreamOpen(
            [Values] bool lazy,
            [Values] bool cancel)
        {
            using var cancellation = new CancellationTokenSource();
            var chunk = new AsyncCustomChunk
            {
                BeforeWrite = token =>
                {
                    ClassicAssert.AreEqual(cancellation.Token, token, "The failing hook received a different token.");
                    if (cancel)
                        cancellation.Cancel();
                    else
                        throw new InvalidOperationException("Custom chunk write failure.");
                }
            };
            using var output = new MidiFileAsyncTests.AsyncOnlyStream();
            Func<Task> write;
            MidiTokensWriter writer = null;
            if (lazy)
            {
                writer = await MidiFile.WriteLazyAsync(output);
                write = () => writer.WriteChunkAsync(chunk, cancellation.Token);
            }
            else
            {
                write = () => new MidiFile(chunk).WriteAsync(output, cancellationToken: cancellation.Token);
            }

            try
            {
                if (cancel)
                {
                    Assert.CatchAsync<OperationCanceledException>(
                        async () => await write(),
                        "Cancellation inside the custom write hook did not interrupt writing.");
                }
                else
                {
                    Assert.ThrowsAsync<InvalidOperationException>(
                        async () => await write(),
                        "The async custom hook's failure was not propagated.");
                }

                ClassicAssert.IsFalse(output.IsDisposed, "Failed custom writing disposed the caller-owned stream.");
            }
            finally
            {
                if (writer != null)
                    await writer.DisposeAsync();
            }
        }

        [Test]
        public void ReadWriteCustomMetaEvent()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customMetaEventTypes = new EventTypesCollection
            {
                { typeof(CustomMetaEvent), 0x5A }
            };

            var writingSettings = new WritingSettings { CustomMetaEventTypes = customMetaEventTypes };
            var readingSettings = new ReadingSettings { CustomMetaEventTypes = customMetaEventTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new CustomMetaEvent(expectedA, expectedB, expectedC) { DeltaTime = 100 },
                        new TextEvent("foo"),
                        new MarkerEvent("bar"))),
                writingSettings,
                readingSettings);

            var customMetaEvents = midiFile.GetEvents().OfType<CustomMetaEvent>().ToArray();
            ClassicAssert.AreEqual(1, customMetaEvents.Length, "Custom meta events count is invalid.");

            var customMetaEvent = customMetaEvents.Single();
            ClassicAssert.AreEqual(100, customMetaEvent.DeltaTime, "Delta-time is invalid.");
            ClassicAssert.AreEqual(expectedA, customMetaEvent.A, "A value is invalid");
            ClassicAssert.AreEqual(expectedB, customMetaEvent.B, "B value is invalid");
            ClassicAssert.AreEqual(expectedC, customMetaEvent.C, "C value is invalid");
        }

        [Test]
        public void ReadWriteCustomChunk()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customChunkTypes = new ChunkTypesCollection
            {
                { typeof(CustomChunk), CustomChunk.Id }
            };

            var readingSettings = new ReadingSettings { CustomChunkTypes = customChunkTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("foo"),
                        new MarkerEvent("bar")),
                    new CustomChunk(expectedA, expectedB, expectedC)),
                null,
                readingSettings);

            var customChunks = midiFile.Chunks.OfType<CustomChunk>().ToArray();
            ClassicAssert.AreEqual(1, customChunks.Length, "Custom chunks count is invalid.");

            var customChunk = customChunks.Single();
            ClassicAssert.AreEqual(expectedA, customChunk.A, "A value is invalid");
            ClassicAssert.AreEqual(expectedB, customChunk.B, "B value is invalid");
            ClassicAssert.AreEqual(expectedC, customChunk.C, "C value is invalid");
        }

        [Test]
        public void ReadWriteCustomChunk_NoTypesProvided()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("foo"),
                        new MarkerEvent("bar")),
                    new CustomChunk(expectedA, expectedB, expectedC)),
                null,
                null);

            var customChunks = midiFile.Chunks.OfType<CustomChunk>().ToArray();
            CollectionAssert.IsEmpty(customChunks, "Custom chunks are read.");

            var unknownChunks = midiFile.Chunks.OfType<UnknownChunk>().ToArray();
            ClassicAssert.AreEqual(1, unknownChunks.Length, "Unknown chunks count is invalid.");

            var unknownChunk = unknownChunks.Single();
            ClassicAssert.AreEqual(CustomChunk.Id, unknownChunk.ChunkId, "Unknown chunk ID is invalid.");

            var expectedBytesCount =
                expectedA.GetVlqLength() +
                expectedB.Length.GetVlqLength() +
                expectedB.Length +
                1;
            ClassicAssert.AreEqual(expectedBytesCount, unknownChunk.Data.Length, "Unknown chunk data length is invalid.");
        }

        #endregion
    }
}
