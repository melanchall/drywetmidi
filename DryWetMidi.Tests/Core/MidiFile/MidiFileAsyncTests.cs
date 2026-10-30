using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
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
    [TestFixture]
    public sealed partial class MidiFileAsyncTests
    {
        private static MidiFile CreateFile() => new MidiFile(new TrackChunk(
            new NoteOnEvent { Velocity = (SevenBitNumber)100 },
            new NoteOffEvent { DeltaTime = 100 }))
        {
            _originalFormat = (ushort)MidiFileFormat.MultiTrack
        };

        [Test]
        public async Task RoundTrip_AsyncOnlyStream([Values] bool nonSeekable)
        {
            var expected = CreateFile();
            using var output = new AsyncOnlyStream(seekable: !nonSeekable);

            await expected.WriteAsync(output);

            ClassicAssert.Greater(output.AsyncWrites, 0, "Writing did not use asynchronous stream IO.");
            ClassicAssert.IsFalse(output.IsDisposed, "Writing disposed the caller-owned stream.");

            using var input = new AsyncOnlyStream(output.ToArray(), !nonSeekable, 3);
            var actual = await MidiFile.ReadAsync(input);

            MidiAsserts.AreEqual(expected, actual, true, "Async round-trip file is invalid.");
            ClassicAssert.Greater(input.AsyncReads, 0, "Reading did not use asynchronous stream IO.");
            ClassicAssert.IsFalse(input.IsDisposed, "Reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task LazyReaders_ContinueAfterSkippedEvent()
        {
            var bytes = new byte[]
            {
                // MThd header: six-byte payload, format 0, one track, 96 ticks per quarter note.
                0x4D, 0x54, 0x68, 0x64, 0x00, 0x00, 0x00, 0x06,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x60,
                // MTrk header: ten bytes of track data follow.
                0x4D, 0x54, 0x72, 0x6B, 0x00, 0x00, 0x00, 0x0A,
                // Zero delta-time and unknown F1 status; SkipStatusByte skips only F1.
                0x00, 0xF1,
                // Zero delta-time, Note On on channel 0, middle C, velocity 64.
                0x00, 0x90, 0x3C, 0x40,
                // Zero delta-time and End Of Track meta-event with an empty payload.
                0x00, 0xFF, 0x2F, 0x00
            };
            var settings = new ReadingSettings { UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByte };
            using var synchronousInputStream = new MemoryStream(bytes);
            using var synchronous = MidiFile.ReadLazy(synchronousInputStream, settings);
            var expected = synchronous.EnumerateTokens().OfType<MidiEventToken>().Select(t => t.Event.EventType).ToArray();
            ClassicAssert.AreEqual(new[] { MidiEventType.NoteOn, MidiEventType.EndOfTrack }, expected, "Synchronous lazy reader did not skip the unknown status byte.");

            using var inputStream = new AsyncOnlyStream(bytes, false, 1);
            await using var asynchronous = await MidiFile.ReadLazyAsync(inputStream, settings);
            var actual = new List<MidiEventType>();

            await foreach (var token in asynchronous.EnumerateTokensAsync())
            {
                if (token is MidiEventToken midiEvent)
                    actual.Add(midiEvent.Event.EventType);
            }

            ClassicAssert.AreEqual(expected, actual, "Async lazy reader returned different events after the skipped status byte.");
        }

        [Test]
        public async Task ReadAsync_FixedMetaPadding_DoesNotCountAsRetainedMemory([Values(0x2F, 0x51, 0x58)] int metaType)
        {
            const int size = 100000;
            using var bytes = new MemoryStream();
            using (var writer = new MidiWriter(bytes, new WriterSettings { UseBuffering = false }))
            {
                writer.WriteString("MThd");
                writer.WriteDword(6);
                writer.WriteWord(0);
                writer.WriteWord(1);
                writer.WriteWord(96);
                writer.WriteString("MTrk");
                writer.WriteDword(size + 6 + (metaType == 0x2F ? 0u : 4u));
                writer.WriteByte(0);
                writer.WriteByte(0xFF);
                writer.WriteByte((byte)metaType);
                writer.WriteVlqNumber(size);
                var content = new byte[size];
                if (metaType == 0x51)
                {
                    content[0] = 7;
                    content[1] = 0xA1;
                    content[2] = 0x20;
                }
                else if (metaType == 0x58)
                {
                    content[0] = 4;
                    content[1] = 2;
                    content[2] = 24;
                    content[3] = 8;
                }

                writer.WriteBytes(content);
                if (metaType != 0x2F)
                    writer.WriteBytes(new byte[] { 0, 0xFF, 0x2F, 0 });
            }

            var settings = new ReadingSettings { MaxMemorySize = 10000 };
            bytes.Position = 0;
            var expected = MidiFile.Read(bytes, settings);
            using var input = new AsyncOnlyStream(bytes.ToArray(), false, 997);
            var actual = await MidiFile.ReadAsync(input, settings);

            MidiAsserts.AreEqual(expected, actual, true, "Async fixed-meta padding handling differs.");
        }

        [Test]
        public async Task Lazy_AsyncOnlyStream_FinalizesHeaders()
        {
            using var output = new AsyncOnlyStream();
            var writer = await MidiFile.WriteLazyAsync(output);
            await writer.StartTrackChunkAsync();
            await writer.WriteEventAsync(new NoteOnEvent());
            await writer.StartTrackChunkAsync();
            await writer.WriteEventAsync(new NoteOffEvent { DeltaTime = 15 });
            await writer.DisposeAsync();
            await writer.DisposeAsync();

            ClassicAssert.IsFalse(output.IsDisposed, "Lazy writer disposed the caller-owned stream.");

            using var input = new AsyncOnlyStream(output.ToArray(), false, 1);
            await using var reader = await MidiFile.ReadLazyAsync(input);
            var tokens = new List<MidiToken>();

            await foreach (var token in reader.EnumerateTokensAsync())
            {
                tokens.Add(token);
            }

            ClassicAssert.AreEqual(4, tokens.FindAll(t => t is MidiEventToken).Count, "Finalized tracks contain an invalid number of events.");

            var header = (FileHeaderToken)tokens.Find(t => t is FileHeaderToken);
            ClassicAssert.AreEqual(2, header.TracksNumber, "Finalized header has an invalid track count.");
        }

        [Test]
        public async Task ReadAsync_TextCallback_UsesRetainedMemoryLimit([Values] bool discardText)
        {
            using var bytes = new MemoryStream();
            new MidiFile(new TrackChunk(new TextEvent(new string('A', 100000)))).Write(bytes);
            var settings = new ReadingSettings
            {
                MaxMemorySize = 10000,
                DecodeTextCallback = (data, _) => discardText ? string.Empty : new string('A', data.Length)
            };
            bytes.Position = 0;
            using var input = new AsyncOnlyStream(bytes.ToArray(), false, 997);
            if (discardText)
            {
                var expected = MidiFile.Read(bytes, settings);
                var actual = await MidiFile.ReadAsync(input, settings);

                MidiAsserts.AreEqual(expected, actual, true, "Async decoded-text retention differs.");
            }
            else
            {
                Assert.Throws<MidiFileTooLargeException>(() => MidiFile.Read(bytes, settings), "Synchronous reading did not enforce the retained-text limit.");
                Assert.ThrowsAsync<MidiFileTooLargeException>(async () => await MidiFile.ReadAsync(input, settings), "Async reading did not enforce the retained-text limit.");
            }
        }

        [Test]
        public void ReadAsync_PreservesMemoryLimit()
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray(), false);

            Assert.ThrowsAsync<MidiFileTooLargeException>(async () =>
                await MidiFile.ReadAsync(input, new ReadingSettings { MaxMemorySize = 1 }), "Async reading did not enforce the memory limit.");
            ClassicAssert.IsFalse(input.IsDisposed, "Memory-limit failure disposed the caller-owned stream.");
        }

        [Test]
        public void PreCanceled_DoesNotTouchStream()
        {
            using var outputStream = new AsyncOnlyStream();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await CreateFile().WriteAsync(outputStream, cancellationToken: cancellation.Token), "Pre-canceled writing did not throw.");
            ClassicAssert.AreEqual(0, outputStream.AsyncWrites, "Pre-canceled writing touched the stream.");
            ClassicAssert.IsFalse(outputStream.IsDisposed, "Pre-canceled writing disposed the caller-owned stream.");
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

            Assert.Throws<InvalidOperationException>(() => reader.ReadToken(), "Synchronous reading accepted an overlapping operation.");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await reader.ReadTokenAsync(), "Async reading accepted an overlapping operation.");

            input.ReadGate.SetResult(true);

            ClassicAssert.IsNotNull(await pending, "The original pending read did not return a token.");
        }

        [Test]
        public async Task ReadAsync_CancellationDuringStreamIO_LeavesStreamOpen()
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray());
            input.ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var cancellation = new CancellationTokenSource();
            var pending = MidiFile.ReadAsync(input, cancellationToken: cancellation.Token);
            await input.ReadStarted.Task;
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await pending, "Cancellation did not interrupt the pending read.");
            ClassicAssert.IsFalse(input.IsDisposed, "Canceled reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task PathOverloads_RoundTripAndOwnHandles()
        {
            var path = "async-midi-" + Guid.NewGuid().ToString("N") + ".mid";
            try
            {
                var expected = CreateFile();
                await expected.WriteAsync(path);

                MidiAsserts.AreEqual(expected, await MidiFile.ReadAsync(path), true, "Async path round-trip file is invalid.");
                Assert.ThrowsAsync<IOException>(async () => await expected.WriteAsync(path), "Writing overwrote an existing file without permission.");

                await using (var writer = await MidiFile.WriteLazyAsync(path, overwriteFile: true))
                {
                    await writer.StartTrackChunkAsync();
                    await writer.WriteEventAsync(new NoteOnEvent());
                }
                await using (var reader = await MidiFile.ReadLazyAsync(path))
                {
                    var count = 0;

                    await foreach (var token in reader.EnumerateTokensAsync())
                    {
                        count++;
                    }

                    ClassicAssert.AreEqual(5, count, "Lazy path reading returned an invalid token count.");
                }
                using var exclusiveStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally
            {
                File.Delete(path);
            }
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
                    throw new InvalidOperationException("Track finalization failure.");
                };
                Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.DisposeAsync(), "Lazy finalization did not propagate the callback failure.");
                await writer.DisposeAsync();
                using var exclusiveStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public async Task CustomEvent_UsesAsyncStreamIO_AndSettings()
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

            ClassicAssert.AreEqual(42, ((CustomEvent)((TrackChunk)actual.Chunks[0]).Events[0]).Value, "Async custom event value is invalid.");
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
                writer.WriteByte(0x5A);
                writer.WriteVlqNumber(payloadSize);
                writer.WriteByte(42);
                writer.WriteBytes(new byte[payloadSize - 1]);
                writer.WriteBytes(new byte[] { 0, 0x90, 60, 100, 0, 0xFF, 0x2F, 0 });
            }

            var settings = new ReadingSettings
            {
                CustomMetaEventTypes = new EventTypesCollection { { typeof(CustomEvent), 0x5A } },
                EndOfTrackStoringPolicy = EndOfTrackStoringPolicy.Store,
                ReaderSettings = new ReaderSettings { BufferingPolicy = BufferingPolicy.DontUseBuffering }
            };
            bytes.Position = 0;
            var expected = MidiFile.Read(bytes, settings);
            using var input = new AsyncOnlyStream(bytes.ToArray(), !nonSeekable, 113);

            MidiFile actual;
            if (lazy)
            {
                await using var reader = await MidiFile.ReadLazyAsync(input, settings);
                var track = new TrackChunk();

                await foreach (var token in reader.EnumerateTokensAsync())
                {
                    if (token is MidiEventToken eventToken)
                        track.Events.Add(eventToken.Event);
                }

                actual = new MidiFile(track);
            }
            else
                actual = await MidiFile.ReadAsync(input, settings);

            MidiAsserts.AreEqual(
                expected.GetTrackChunks().Single(),
                actual.GetTrackChunks().Single(),
                true,
                "Async custom meta-event under-read changed event framing.");
            var events = actual.GetTrackChunks().Single().Events;
            ClassicAssert.AreEqual(42, ((CustomEvent)events[0]).Value, "The custom event did not read its first payload byte.");
            ClassicAssert.AreEqual(1, events.OfType<NoteOnEvent>().Count(), "Skipping custom event padding consumed the following note event.");
            ClassicAssert.AreEqual(
                bytes.Length,
                nonSeekable ? input.BytesRead : input.Position,
                "Reading did not consume the full custom payload and following events.");
            ClassicAssert.IsFalse(input.IsDisposed, "Custom event reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task CustomChunk_UsesAsyncReaderAndWriter_WithAbsolutePositions([Values] bool lazyWrite)
        {
            var expected = new CustomChunk { Value = 42 };
            using var output = new AsyncOnlyStream();

            if (lazyWrite)
            {
                await using var writer = await MidiFile.WriteLazyAsync(output);
                await writer.WriteChunkAsync(expected);
            }
            else
            {
                await new MidiFile(expected).WriteAsync(output);
            }

            using var input = new AsyncOnlyStream(output.ToArray(), false, 2);
            var actual = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(CustomChunk), "Cstm" } }
            });
            var chunk = (CustomChunk)actual.Chunks[0];

            ClassicAssert.AreEqual(42, chunk.Value, "Async custom chunk value is invalid.");
            ClassicAssert.AreEqual(22, chunk.ContentPosition, "Custom chunk MidiReader received an invalid absolute position.");
            ClassicAssert.AreEqual(22, expected.WritePosition, "Custom chunk MidiWriter received an invalid absolute position.");
            ClassicAssert.IsFalse(input.IsDisposed, "Custom chunk reading disposed the caller-owned stream.");
            ClassicAssert.IsFalse(output.IsDisposed, "Custom chunk writing disposed the caller-owned stream.");
        }

        [Test]
        public async Task ExistingValidCorpus_AsyncReadAndWriteMatchSynchronous()
        {
            foreach (var path in TestFilesProvider.GetValidFilesPaths())
            {
                var expected = MidiFile.Read(path);
                var actual = await MidiFile.ReadAsync(path);

                MidiAsserts.AreEqual(expected, actual, true, path + ": Async corpus reading differs.");

                using var synchronousOutputStream = new MemoryStream();
                using var asynchronousOutputStream = new MemoryStream();
                expected.Write(synchronousOutputStream);
                await expected.WriteAsync(asynchronousOutputStream);

                ClassicAssert.AreEqual(synchronousOutputStream.ToArray(), asynchronousOutputStream.ToArray(), path + ": Async corpus writing differs.");
            }
        }

        [Test]
        public async Task ExistingInvalidCorpus_AsyncReadMatchesSynchronousPolicies([Values] bool ignoreTruncatedData)
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
                try
                {
                    expected = MidiFile.Read(new MemoryStream(bytes), settings);
                }
                catch (Exception error)
                {
                    expectedError = error;
                }

                MidiFile actual = null;
                Exception actualError = null;
                try
                {
                    actual = await MidiFile.ReadAsync(new MemoryStream(bytes), settings);
                }
                catch (Exception error)
                {
                    actualError = error;
                }

                ClassicAssert.AreEqual(expectedError?.GetType(), actualError?.GetType(), path + ": Async read exception type differs: " + actualError);
                if (expectedError == null)
                    MidiAsserts.AreEqual(expected, actual, true, path + ": Async invalid-corpus reading differs.");
            }
        }

        [Test]
        public async Task ReadAsync_EstimatedMemoryMatchesSynchronous()
        {
            var file = new MidiFile(
                new TrackChunk(
                    new TextEvent("payload"),
                    new NormalSysExEvent { Data = new byte[10000] },
                    new NoteOnEvent { Velocity = (SevenBitNumber)100 }),
                new UnknownChunk("Test") { Data = new byte[300] });
            using var bytes = new MemoryStream();
            file.Write(bytes);
            var settings = new ReadingSettings { MaxMemorySize = long.MaxValue };
            var expected = MidiFile.Read(new MemoryStream(bytes.ToArray()), settings);
            var actual = await MidiFile.ReadAsync(new MemoryStream(bytes.ToArray()), settings);

            ClassicAssert.AreEqual(expected.EstimatedMemorySize, actual.EstimatedMemorySize, "Async estimated memory size differs.");
        }

        [Test]
        public async Task ReadAsync_PreservesReaderBufferingPolicy(
            [Values(BufferingPolicy.BufferAllData, BufferingPolicy.DontUseBuffering, BufferingPolicy.UseCustomBuffer, BufferingPolicy.UseFixedSizeBuffer)] BufferingPolicy policy)
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray(), true, 2);
            var file = await MidiFile.ReadAsync(input, new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BufferingPolicy = policy,
                    BufferSize = 3,
                    Buffer = new byte[5]
                }
            });

            MidiAsserts.AreEqual(CreateFile(), file, true, "Async reading with the selected buffering policy differs.");
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
            ClassicAssert.AreEqual(1, bytes[0], "Header patch overwrote the first prefix byte.");
            ClassicAssert.AreEqual(2, bytes[1], "Header patch overwrote the second prefix byte.");
            ClassicAssert.AreEqual(3, bytes[2], "Header patch overwrote the third prefix byte.");

            using var input = new MemoryStream(bytes, 3, bytes.Length - 3, false);
            var file = await MidiFile.ReadAsync(input);

            ClassicAssert.AreEqual(2, file.Chunks.Count, "Lazy writer did not finalize both tracks.");
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

            ClassicAssert.IsInstanceOf<ChunkHeaderToken>(reader.ReadToken(), "Synchronous reading did not return the header chunk token.");
            ClassicAssert.IsInstanceOf<FileHeaderToken>(await reader.ReadTokenAsync(), "Async reading did not return the file header token.");
            ClassicAssert.IsInstanceOf<ChunkHeaderToken>(reader.ReadToken(), "Synchronous reading did not return the track chunk token.");
            ClassicAssert.IsInstanceOf<MidiEventToken>(await reader.ReadTokenAsync(), "Async reading did not return the event token.");
        }

        [Test]
        public async Task NonSeekableMalformedLength_BuffersOnlyAvailablePayload([Values] bool limitMemory)
        {
            using var bytes = new MemoryStream();
            new MidiFile(new UnknownChunk("Test") { Data = new byte[] { 1, 2, 3 } }).Write(bytes);
            var data = bytes.ToArray();

            for (var i = 18; i < 22; i++)
            {
                data[i] = 0xFF;
            }

            var settings = new ReadingSettings
            {
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                MaxMemorySize = limitMemory ? 10000 : (long?)null
            };
            using var expectedInput = new MemoryStream(data);
            var expected = MidiFile.Read(expectedInput, settings);
            using var input = new AsyncOnlyStream(data, false, 2);

            var file = await MidiFile.ReadAsync(input, settings);

            ClassicAssert.AreEqual(new byte[] { 1, 2, 3 }, ((UnknownChunk)file.Chunks[0]).Data, "Malformed-length handling did not preserve the available payload.");
            ClassicAssert.AreEqual(expected.EstimatedMemorySize, file.EstimatedMemorySize, "Malformed-length handling did not account for the actual retained payload.");
            ClassicAssert.IsFalse(input.IsDisposed, "Malformed-length handling disposed the caller's stream.");
        }

        [Test]
        public void NonSeekableUnknownChunk_StopsAtMemoryLimit([Values] bool malformedLength)
        {
            using var bytes = new MemoryStream();
            new MidiFile(new UnknownChunk("Test") { Data = new byte[20000] }).Write(bytes);
            var data = bytes.ToArray();
            if (malformedLength)
            {
                for (var i = 18; i < 22; i++)
                {
                    data[i] = 0xFF;
                }
            }

            using var input = new AsyncOnlyStream(data, false, 997);
            var settings = new ReadingSettings
            {
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                MaxMemorySize = 10000
            };

            Assert.ThrowsAsync<MidiFileTooLargeException>(
                async () => await MidiFile.ReadAsync(input, settings),
                "The memory limit did not reject a genuinely oversized retained unknown payload.");

            ClassicAssert.Less(input.BytesRead, data.Length, "The entire oversized payload was buffered before checking the memory limit.");
            ClassicAssert.IsFalse(input.IsDisposed, "A memory-limit failure disposed the caller's stream.");
        }

        [Test]
        public async Task UndersizedTimeSignature_PreservesSynchronousFraming()
        {
            var data = new byte[]
            {
                0x4D, 0x54, 0x68, 0x64, 0, 0, 0, 6, 0, 1, 0, 1, 0, 96,
                0x4D, 0x54, 0x72, 0x6B, 0, 0, 0, 10,
                0, 0xFF, 0x58, 1, 4, 2, 0, 0xFF, 0x2F, 0
            };
            using var synchronousInput = new MemoryStream(data);
            var expected = MidiFile.Read(synchronousInput);
            using var asynchronousInput = new AsyncOnlyStream(data, false, 2);
            var actual = await MidiFile.ReadAsync(asynchronousInput);

            ClassicAssert.AreEqual(
                expected.GetTrackChunks().Single().Events.OfType<TimeSignatureEvent>().Single().Denominator,
                actual.GetTrackChunks().Single().Events.OfType<TimeSignatureEvent>().Single().Denominator,
                "Async reading changed undersized time-signature framing.");

            using var lazyInput = new AsyncOnlyStream(data, false, 2);
            await using var reader = await MidiFile.ReadLazyAsync(lazyInput);
            var timeSignatures = 0;
            while (await reader.ReadTokenAsync() is MidiToken token)
            {
                if (token is MidiEventToken eventToken && eventToken.Event is TimeSignatureEvent)
                    timeSignatures++;
            }

            ClassicAssert.AreEqual(1, timeSignatures, "Lazy async reading returned an invalid time-signature count.");
        }

        [Test]
        public void BulkIo_FailureOrCancellation_LeavesStreamsOpen()
        {
            using var input = new AsyncOnlyStream(new byte[] { 1, 2, 3 });
            Assert.ThrowsAsync<NotEnoughBytesException>(async () => await MidiFile.ReadAsync(input), "Truncated input did not fail.");
            ClassicAssert.IsFalse(input.IsDisposed, "Failed reading disposed the caller-owned stream.");

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            using var output = new AsyncOnlyStream();
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await CreateFile().WriteAsync(output, cancellationToken: cancellation.Token), "Pre-canceled writing did not throw.");
            ClassicAssert.IsFalse(output.IsDisposed, "Canceled writing disposed the caller-owned stream.");
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

            Assert.Throws<InvalidOperationException>(() => writer.WriteEvent(new NoteOnEvent()), "Synchronous writing accepted an overlapping operation.");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.DisposeAsync(), "Disposal accepted an overlapping write.");

            output.WriteGate.SetResult(true);
            await pending;
            await writer.DisposeAsync();
        }

        [Test]
        public async Task LazyWriter_AsyncFinalizationCallback_CanEmitDeferredEvent([Values(0, 1, 2)] int operation)
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

            ClassicAssert.AreEqual(1, events.Count, "Finalization callback emitted an invalid event count.");
            ClassicAssert.IsInstanceOf<NoteOffEvent>(events[0], "Finalization callback emitted an invalid event type.");
            ClassicAssert.AreEqual(20, events[0].DeltaTime, "Finalization callback emitted an invalid delta-time.");
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
            ClassicAssert.LessOrEqual(output.MaxWrite, 65536, "Async writing exceeded the packet size bound.");

            using var input = new AsyncOnlyStream(output.ToArray(), false);
            await using var reader = await MidiFile.ReadLazyAsync(input);

            ClassicAssert.IsInstanceOf<ChunkHeaderToken>(await reader.ReadTokenAsync(), "Lazy reading did not return the first chunk header.");
            ClassicAssert.Less(input.BytesRead, 100, "Lazy reading buffered the whole payload before returning a token.");
        }

        private sealed class CustomEvent : MetaEvent
        {
            public byte Value { get; set; }

            public CustomEvent()
            {
            }

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

        private sealed class CustomChunk : MidiChunk
        {
            public CustomChunk()
                : base("Cstm")
            {
            }

            public byte Value { get; set; }

            public long ContentPosition { get; private set; }

            public long WritePosition { get; private set; }

            public override MidiChunk Clone() => new CustomChunk { Value = Value };

            protected override uint GetContentSize(WritingSettings settings) => 1;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                ContentPosition = reader.Position;
                Value = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                WritePosition = writer.Length;
                writer.WriteByte(Value);
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                ContentPosition = reader.Position;
                Value = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                WritePosition = writer.Length;
                return writer.WriteByteAsync(Value, cancellationToken);
            }
        }

        internal sealed class AsyncOnlyStream : Stream
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
                set
                {
                    if (!_seekable)
                        throw new NotSupportedException();

                    _inner.Position = value;
                }
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
                if (disposing)
                    _inner.Dispose();

                base.Dispose(disposing);
            }
        }
    }
}
