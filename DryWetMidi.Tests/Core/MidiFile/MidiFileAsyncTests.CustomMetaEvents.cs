using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileAsyncTests
    {
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
            var types = new EventTypesCollection { { typeof(InheritedAsyncMetaEvent), 0x5A } };
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
            var expected = new InheritedAsyncMetaEvent { Value = 42 };
            AsyncCustomMetaEvent written = null;
            expected.BeforeWrite = midiEvent =>
            {
                written = midiEvent;
                ClassicAssert.AreEqual(
                    26,
                    output.ToArray().Length,
                    "The hook was not entered immediately after writing the event header.");
            };

            if (lazy)
            {
                await using var writer = await MidiFile.WriteLazyAsync(output, writingSettings);
                await writer.StartTrackChunkAsync(cancellation.Token);
                await writer.WriteEventAsync(expected, cancellation.Token);
            }
            else
            {
                await new MidiFile(new TrackChunk(expected)).WriteAsync(
                    output,
                    settings: writingSettings,
                    cancellationToken: cancellation.Token);
            }

            ClassicAssert.IsNotNull(written, "The inherited write hook was not dispatched.");
            ClassicAssert.AreEqual(26, written.ContentPosition, "The inherited write hook received an invalid absolute length.");
            ClassicAssert.AreSame(writingSettings, written.WriteSettings, "The inherited write hook received different settings.");
            ClassicAssert.AreEqual(cancellation.Token, written.HookToken, "The inherited write hook received a different token.");
            using var input = new AsyncOnlyStream(output.ToArray(), !nonSeekable, 1);
            InheritedAsyncMetaEvent actual;
            if (lazy)
            {
                await using var reader = await MidiFile.ReadLazyAsync(input, readingSettings);
                actual = null;

                await foreach (var token in reader.EnumerateTokensAsync(cancellation.Token))
                {
                    if (token is MidiEventToken eventToken && eventToken.Event is InheritedAsyncMetaEvent metaEvent)
                        actual = metaEvent;
                }
            }
            else
            {
                var file = await MidiFile.ReadAsync(input, readingSettings, cancellation.Token);
                actual = file.GetTrackChunks().Single().Events.OfType<InheritedAsyncMetaEvent>().Single();
            }

            ClassicAssert.IsNotNull(actual, "The inherited custom event was not returned.");
            ClassicAssert.AreEqual(42, actual.Value, "The inherited async read override was not dispatched.");
            ClassicAssert.AreEqual(26, actual.ContentPosition, "The inherited read hook received an invalid absolute position.");
            ClassicAssert.AreEqual(nonSeekable ? long.MaxValue : output.ToArray().Length, actual.ContentLength, "The hook received a staged payload length.");
            ClassicAssert.AreSame(readingSettings, actual.ReadSettings, "The inherited read hook received different settings.");
            ClassicAssert.AreEqual(cancellation.Token, actual.HookToken, "The inherited read hook received a different token.");
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
                ClassicAssert.AreEqual(0, stream.AsyncWrites, "Canceled payload writing touched the stream.");
                ClassicAssert.AreEqual(0, writer.Length, "Canceled payload writing advanced the writer.");
            }
            else
            {
                using var reader = new MidiReader(stream, new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering });
                midiEvent.BeforeRead = cancellation.Cancel;

                Assert.CatchAsync<OperationCanceledException>(
                    async () => await midiEvent.ReadAsync(reader, new ReadingSettings(), 1, cancellation.Token),
                    "Cancellation inside the read hook did not interrupt payload reading.");
                ClassicAssert.AreEqual(0, stream.AsyncReads, "Payload bytes were staged before cancellation inside the hook.");
                ClassicAssert.AreEqual(0, reader.Position, "Canceled payload reading advanced the reader.");
            }

            ClassicAssert.AreEqual(cancellation.Token, midiEvent.HookToken, "The hook received a different cancellation token.");
            ClassicAssert.IsFalse(stream.IsDisposed, "Cancellation disposed the caller-owned stream.");
        }
    }
}
