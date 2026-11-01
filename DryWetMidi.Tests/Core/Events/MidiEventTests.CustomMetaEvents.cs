using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiEventTests
    {
        private sealed class CustomEvent : MetaEvent
        {
            public CustomEvent()
            {
            }

            public byte Value { get; set; }

            protected override MidiEvent CloneEvent() => new CustomEvent { Value = Value };

            protected override int GetContentSize(WritingSettings settings) => 1;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) => Value = reader.ReadByte();

            protected override void WriteContent(MidiWriter writer, WritingSettings settings) => writer.WriteByte(Value);

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                int size,
                CancellationToken cancellationToken = default)
            {
                Value = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken = default)
            {
                await writer.WriteByteAsync(Value, cancellationToken).ConfigureAwait(false);
            }

            public override bool Equals(object obj) => obj is CustomEvent other && other.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();
        }

        private sealed class StructuredCustomMetaEvent : MetaEvent
        {
            public StructuredCustomMetaEvent()
            {
            }

            public StructuredCustomMetaEvent(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            protected override MidiEvent CloneEvent()
            {
                return new StructuredCustomMetaEvent(A, B, C);
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

                var textLength = reader.ReadVlqNumber();
                B = reader.ReadString(textLength);

                C = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                int size,
                CancellationToken cancellationToken = default)
            {
                A = await reader.ReadVlqNumberAsync(cancellationToken).ConfigureAwait(false);

                var textLength = await reader.ReadVlqNumberAsync(cancellationToken).ConfigureAwait(false);
                B = await reader.ReadStringAsync(textLength, cancellationToken).ConfigureAwait(false);

                C = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken = default)
            {
                await writer.WriteVlqNumberAsync(A, cancellationToken).ConfigureAwait(false);
                await writer.WriteVlqNumberAsync(B?.Length ?? 0, cancellationToken).ConfigureAwait(false);
                await writer.WriteStringAsync(B, cancellationToken).ConfigureAwait(false);
                await writer.WriteByteAsync(C, cancellationToken).ConfigureAwait(false);
            }

            public override bool Equals(object obj) => obj is StructuredCustomMetaEvent customEvent &&
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
        }

        private class AsyncCustomMetaEvent : MetaEvent
        {
            public AsyncCustomMetaEvent()
            {
            }

            public byte Value { get; set; }

            public MidiReader ContentReader { get; private set; }

            public MidiWriter ContentWriter { get; private set; }

            public long ContentPosition { get; private set; }

            public long ContentLength { get; private set; }

            public int ContentSize { get; private set; }

            public ReadingSettings ReadSettings { get; private set; }

            public WritingSettings WriteSettings { get; private set; }

            public CancellationToken HookToken { get; private set; }

            public Action BeforeRead { get; set; }

            public Action<AsyncCustomMetaEvent> BeforeWrite { get; set; }

            public TaskCompletionSource<bool> ReadGate { get; set; }

            public TaskCompletionSource<bool> WriteGate { get; set; }

            protected override MidiEvent CloneEvent() => new AsyncCustomMetaEvent
            {
                Value = Value,
                BeforeWrite = BeforeWrite
            };

            protected override int GetContentSize(WritingSettings settings) => 1;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) =>
                throw new AssertionException("Synchronous custom meta-event reading was selected.");

            protected override void WriteContent(MidiWriter writer, WritingSettings settings) =>
                throw new AssertionException("Synchronous custom meta-event writing was selected.");

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                int size,
                CancellationToken cancellationToken = default)
            {
                ContentReader = reader;
                ContentPosition = reader.Position;
                ContentLength = reader.Length;
                ContentSize = size;
                ReadSettings = settings;
                HookToken = cancellationToken;
                BeforeRead?.Invoke();
                if (ReadGate != null)
                    await ReadGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

                Value = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken = default)
            {
                ContentWriter = writer;
                ContentPosition = writer.Length;
                WriteSettings = settings;
                HookToken = cancellationToken;
                BeforeWrite?.Invoke(this);
                if (WriteGate != null)
                    await WriteGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

                await writer.WriteByteAsync(Value, cancellationToken).ConfigureAwait(false);
            }
        }

        private sealed class InheritedAsyncMetaEvent : AsyncCustomMetaEvent
        {
            public InheritedAsyncMetaEvent()
            {
            }

            protected override MidiEvent CloneEvent() => new InheritedAsyncMetaEvent
            {
                Value = Value,
                BeforeWrite = BeforeWrite
            };
        }

        private const byte CustomMetaEventStatusByte = 0x5A;

        [Test]
        public void ReadWriteCustomMetaEvent()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customMetaEventTypes = new EventTypesCollection
            {
                { typeof(StructuredCustomMetaEvent), CustomMetaEventStatusByte }
            };

            var writingSettings = new WritingSettings { CustomMetaEventTypes = customMetaEventTypes };
            var readingSettings = new ReadingSettings { CustomMetaEventTypes = customMetaEventTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new StructuredCustomMetaEvent(expectedA, expectedB, expectedC) { DeltaTime = 100 },
                        new TextEvent("foo"),
                        new MarkerEvent("bar"))),
                writingSettings,
                readingSettings);

            var customMetaEvents = midiFile.GetEvents().OfType<StructuredCustomMetaEvent>().ToArray();
            ClassicAssert.AreEqual(1, customMetaEvents.Length, "Custom meta events count is invalid.");

            var customMetaEvent = customMetaEvents.Single();
            ClassicAssert.AreEqual(100, customMetaEvent.DeltaTime, "Delta-time is invalid.");
            ClassicAssert.AreEqual(expectedA, customMetaEvent.A, "A value is invalid.");
            ClassicAssert.AreEqual(expectedB, customMetaEvent.B, "B value is invalid.");
            ClassicAssert.AreEqual(expectedC, customMetaEvent.C, "C value is invalid.");
        }

        [Test]
        public async Task CustomEvent_UsesAsyncStreamIO_AndSettings()
        {
            var types = new EventTypesCollection { { typeof(CustomEvent), CustomMetaEventStatusByte } };
            var expectedFile = new MidiFile(new TrackChunk(new CustomEvent { Value = 42 }));
            using var output = new AsyncOnlyStream();

            await expectedFile.WriteAsync(output, settings: new WritingSettings
            {
                CustomMetaEventTypes = types,
                WriterSettings = new WriterSettings { UseBuffering = false }
            });

            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            var actualFile = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                CustomMetaEventTypes = types,
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            });

            ClassicAssert.AreEqual(42, ((CustomEvent)((TrackChunk)actualFile.Chunks[0]).Events[0]).Value, "Async custom event value is invalid.");
        }

        [Test]
        public async Task CustomEvent_UnderreadLargePayload_PreservesFollowingEvent(
            [Values(2, 4096, 100000)] int payloadSize,
            [Values] bool nonSeekable,
            [Values] bool lazy)
        {
            using var bytes = new MemoryStream();

            using (var writer = new MidiWriter(bytes, new WriterSettings { UseBuffering = false }))
            {
                writer.WriteString("MThd");
                writer.WriteDword(6);
                writer.WriteWord(0);
                writer.WriteWord(1);
                writer.WriteWord(96);
                writer.WriteString("MTrk");
                writer.WriteDword((uint)(payloadSize + DataTypesUtilities.GetVlqLength(payloadSize) + 11));
                writer.WriteByte(0);
                writer.WriteByte(0xFF);
                writer.WriteByte(CustomMetaEventStatusByte);
                writer.WriteVlqNumber(payloadSize);
                writer.WriteByte(42);
                writer.WriteBytes(new byte[payloadSize - 1]);
                writer.WriteBytes(new byte[] { 0, 0x90, 60, 100, 0, 0xFF, 0x2F, 0 });
            }

            var settings = new ReadingSettings
            {
                CustomMetaEventTypes = new EventTypesCollection { { typeof(CustomEvent), CustomMetaEventStatusByte } },
                EndOfTrackStoringPolicy = EndOfTrackStoringPolicy.Store,
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            };
            bytes.Position = 0;
            var expectedFile = MidiFile.Read(bytes, settings);
            using var input = new AsyncOnlyStream(bytes.ToArray(), !nonSeekable, 113);

            MidiFile actualFile;
            if (lazy)
            {
                await using var reader = await MidiFile.ReadLazyAsync(input, settings);
                var track = new TrackChunk();

                await foreach (var token in reader.EnumerateTokensAsync())
                {
                    if (token is MidiEventToken eventToken)
                        track.Events.Add(eventToken.Event);
                }

                actualFile = new MidiFile(track);
            }
            else
                actualFile = await MidiFile.ReadAsync(input, settings);

            MidiAsserts.AreEqual(
                expectedFile.GetTrackChunks().Single(),
                actualFile.GetTrackChunks().Single(),
                true,
                "Async custom meta-event under-read changed event framing.");
            var events = actualFile.GetTrackChunks().Single().Events;
            ClassicAssert.AreEqual(42, ((CustomEvent)events[0]).Value, "The custom event did not read its first payload byte.");
            ClassicAssert.AreEqual(1, events.OfType<NoteOnEvent>().Count(), "Skipping custom event padding consumed the following note event.");
            ClassicAssert.AreEqual(
                bytes.Length,
                nonSeekable ? input.BytesRead : input.Position,
                "Reading did not consume the full custom payload and following events.");
            ClassicAssert.IsFalse(input.IsDisposed, "Custom event reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task CustomMetaEvent_ReadHook_ReceivesOriginalReaderBeforePayload(
            [Values] bool nonSeekable)
        {
            using var input = new AsyncOnlyStream(new byte[] { 11, 12, 42, 99 }, !nonSeekable, 1);
            var settings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            };
            using var reader = new MidiReader(input, settings.ReaderSettings);
            using var cancellation = new CancellationTokenSource();
            await reader.ReadBytesAsync(2);
            var midiEvent = new InheritedAsyncMetaEvent
            {
                ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
            };

            var pending = midiEvent.ReadAsync(reader, settings, 1, cancellation.Token);

            ClassicAssert.AreSame(reader, midiEvent.ContentReader, "The hook received a staged payload reader.");
            ClassicAssert.AreSame(settings, midiEvent.ReadSettings, "The hook received different reading settings.");
            ClassicAssert.AreEqual(cancellation.Token, midiEvent.HookToken, "The hook received a different token.");
            ClassicAssert.AreEqual(2, midiEvent.ContentPosition, "The hook received a payload-relative position.");
            ClassicAssert.AreEqual(reader.Length, midiEvent.ContentLength, "The hook received a payload-relative length.");
            ClassicAssert.AreEqual(1, midiEvent.ContentSize, "The hook received a different declared payload size.");
            ClassicAssert.AreEqual(2, input.BytesRead, "Payload bytes were read before entering the hook.");
            ClassicAssert.IsFalse(pending.IsCompleted, "Reading ignored the hook's asynchronous completion.");

            midiEvent.ReadGate.SetResult(true);
            await pending;

            ClassicAssert.AreEqual(42, midiEvent.Value, "The hook did not asynchronously read the payload.");
            ClassicAssert.AreEqual(3, reader.Position, "The original reader did not advance with the hook.");
            ClassicAssert.AreEqual(99, await reader.ReadByteAsync(), "The following byte is inaccessible on the original reader.");
            ClassicAssert.IsFalse(input.IsDisposed, "The hook disposed the caller-owned input stream.");
        }

        [Test]
        public async Task CustomMetaEvent_WriteHook_ReceivesOriginalWriterWithoutStaging(
            [Values] bool nonSeekable)
        {
            using var output = new AsyncOnlyStream(seekable: !nonSeekable);
            var settings = new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            };
            await using var writer = new MidiWriter(output, settings.WriterSettings);
            using var cancellation = new CancellationTokenSource();
            await writer.WriteBytesAsync(new byte[] { 11, 12 });
            var midiEvent = new InheritedAsyncMetaEvent
            {
                Value = 42,
                WriteGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
            };

            var pending = midiEvent.WriteAsync(writer, settings, cancellation.Token);

            ClassicAssert.AreSame(writer, midiEvent.ContentWriter, "The hook received a staged payload writer.");
            ClassicAssert.AreSame(settings, midiEvent.WriteSettings, "The hook received different writing settings.");
            ClassicAssert.AreEqual(cancellation.Token, midiEvent.HookToken, "The hook received a different token.");
            ClassicAssert.AreEqual(2, midiEvent.ContentPosition, "The hook received a payload-relative writer length.");
            ClassicAssert.AreEqual(new byte[] { 11, 12 }, output.ToArray(), "Writing started before the hook was ready.");
            ClassicAssert.IsFalse(pending.IsCompleted, "Writing ignored the hook's asynchronous completion.");

            midiEvent.WriteGate.SetResult(true);
            await pending;

            ClassicAssert.AreEqual(3, writer.Length, "The hook did not advance the original writer's length.");
            ClassicAssert.AreEqual(new byte[] { 11, 12, 42 }, output.ToArray(), "The payload was not written directly to the original stream.");
            ClassicAssert.IsFalse(output.IsDisposed, "The hook disposed the caller-owned output stream.");
        }

        [Test]
        public async Task CustomMetaEvent_InheritedAsyncHooks_RoundTrip(
            [Values] bool nonSeekable,
            [Values] bool lazy)
        {
            var types = new EventTypesCollection { { typeof(InheritedAsyncMetaEvent), CustomMetaEventStatusByte } };
            var writingSettings = new WritingSettings
            {
                CustomMetaEventTypes = types,
                WriterSettings = new WriterSettings { UseBuffering = false }
            };
            var readingSettings = new ReadingSettings
            {
                CustomMetaEventTypes = types,
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            };
            using var cancellation = new CancellationTokenSource();
            using var output = new AsyncOnlyStream(seekable: lazy || !nonSeekable);
            var expectedEvent = new InheritedAsyncMetaEvent { Value = 42 };
            AsyncCustomMetaEvent writtenEvent = null;
            expectedEvent.BeforeWrite = midiEvent =>
            {
                writtenEvent = midiEvent;
                ClassicAssert.AreEqual(
                    26,
                    output.ToArray().Length,
                    "The hook was not entered immediately after writing the event header.");
            };

            if (lazy)
            {
                await using var writer = await MidiFile.WriteLazyAsync(output, writingSettings);
                await writer.StartTrackChunkAsync(cancellation.Token);
                await writer.WriteEventAsync(expectedEvent, cancellation.Token);
            }
            else
            {
                await new MidiFile(new TrackChunk(expectedEvent)).WriteAsync(
                    output,
                    settings: writingSettings,
                    cancellationToken: cancellation.Token);
            }

            ClassicAssert.IsNotNull(writtenEvent, "The inherited write hook was not dispatched.");
            ClassicAssert.AreEqual(26, writtenEvent.ContentPosition, "The inherited write hook received an invalid absolute length.");
            ClassicAssert.AreSame(writingSettings, writtenEvent.WriteSettings, "The inherited write hook received different settings.");
            ClassicAssert.AreEqual(cancellation.Token, writtenEvent.HookToken, "The inherited write hook received a different token.");
            using var input = new AsyncOnlyStream(output.ToArray(), !nonSeekable, 1);
            InheritedAsyncMetaEvent actualEvent;

            if (lazy)
            {
                await using var reader = await MidiFile.ReadLazyAsync(input, readingSettings);
                actualEvent = null;

                await foreach (var token in reader.EnumerateTokensAsync(cancellation.Token))
                {
                    if (token is MidiEventToken eventToken && eventToken.Event is InheritedAsyncMetaEvent metaEvent)
                        actualEvent = metaEvent;
                }
            }
            else
            {
                var file = await MidiFile.ReadAsync(input, readingSettings, cancellation.Token);
                actualEvent = file.GetTrackChunks().Single().Events.OfType<InheritedAsyncMetaEvent>().Single();
            }

            ClassicAssert.IsNotNull(actualEvent, "The inherited custom event was not returned.");
            ClassicAssert.AreEqual(42, actualEvent.Value, "The inherited async read override was not dispatched.");
            ClassicAssert.AreEqual(26, actualEvent.ContentPosition, "The inherited read hook received an invalid absolute position.");
            ClassicAssert.AreEqual(nonSeekable ? long.MaxValue : output.ToArray().Length, actualEvent.ContentLength, "The hook received a staged payload length.");
            ClassicAssert.AreSame(readingSettings, actualEvent.ReadSettings, "The inherited read hook received different settings.");
            ClassicAssert.AreEqual(cancellation.Token, actualEvent.HookToken, "The inherited read hook received a different token.");
            ClassicAssert.IsFalse(input.IsDisposed, "Inherited custom reading disposed the input stream.");
            ClassicAssert.IsFalse(output.IsDisposed, "Inherited custom writing disposed the output stream.");
        }

        [Test]
        public async Task CustomMetaEvent_CancellationInsideHook_InterruptsPayloadIO(
            [Values] bool write,
            [Values] bool nonSeekable)
        {
            using var cancellation = new CancellationTokenSource();
            var midiEvent = new InheritedAsyncMetaEvent { Value = 42 };
            using var stream = new AsyncOnlyStream(write ? null : new byte[] { 42 }, !nonSeekable, 1);
            if (write)
            {
                await using var writer = new MidiWriter(stream, new WriterSettings { UseBuffering = false });
                midiEvent.BeforeWrite = _ => cancellation.Cancel();

                Assert.CatchAsync<OperationCanceledException>(
                    async () => await midiEvent.WriteAsync(writer, new WritingSettings(), cancellation.Token),
                    "Cancellation inside the write hook did not interrupt payload writing.");
                ClassicAssert.AreEqual(0, stream.AsyncWritesCount, "Canceled payload writing touched the stream.");
                ClassicAssert.AreEqual(0, writer.Length, "Canceled payload writing advanced the writer.");
            }
            else
            {
                using var reader = new MidiReader(stream, new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering });
                midiEvent.BeforeRead = cancellation.Cancel;

                Assert.CatchAsync<OperationCanceledException>(
                    async () => await midiEvent.ReadAsync(reader, new ReadingSettings(), 1, cancellation.Token),
                    "Cancellation inside the read hook did not interrupt payload reading.");
                ClassicAssert.AreEqual(0, stream.AsyncReadsCount, "Payload bytes were staged before cancellation inside the hook.");
                ClassicAssert.AreEqual(0, reader.Position, "Canceled payload reading advanced the reader.");
            }

            ClassicAssert.AreEqual(cancellation.Token, midiEvent.HookToken, "The hook received a different cancellation token.");
            ClassicAssert.IsFalse(stream.IsDisposed, "Cancellation disposed the caller-owned stream.");
        }
    }
}
