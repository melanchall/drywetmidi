using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
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
    public sealed class MidiFileAsyncTests
    {
        [Test]
        public async Task RoundTrip_AsyncOnlyStream([Values] bool nonSeekable)
        {
            var expectedFile = CreateFile();
            using var output = new AsyncOnlyStream(seekable: !nonSeekable);

            await expectedFile.WriteAsync(output);

            ClassicAssert.Greater(output.AsyncWritesCount, 0, "Writing did not use asynchronous stream IO.");
            ClassicAssert.IsFalse(output.IsDisposed, "Writing disposed the caller-owned stream.");

            using var input = new AsyncOnlyStream(output.ToArray(), !nonSeekable, 3);
            var actualFile = await MidiFile.ReadAsync(input);

            MidiAsserts.AreEqual(expectedFile, actualFile, true, "Async round-trip file is invalid.");
            ClassicAssert.Greater(input.AsyncReadsCount, 0, "Reading did not use asynchronous stream IO.");
            ClassicAssert.IsFalse(input.IsDisposed, "Reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task ReadAsync_FixedMetaPadding_DoesNotCountAsRetainedMemory(
            [Values(
                EventStatusBytes.Meta.EndOfTrack,
                EventStatusBytes.Meta.SetTempo,
                EventStatusBytes.Meta.TimeSignature)] byte metaType)
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
                writer.WriteDword(size + 6 + (metaType == EventStatusBytes.Meta.EndOfTrack ? 0u : 4u));
                writer.WriteByte(0);
                writer.WriteByte(0xFF);
                writer.WriteByte(metaType);
                writer.WriteVlqNumber(size);
                var content = new byte[size];
                if (metaType == EventStatusBytes.Meta.SetTempo)
                {
                    content[0] = 7;
                    content[1] = 0xA1;
                    content[2] = 0x20;
                }
                else if (metaType == EventStatusBytes.Meta.TimeSignature)
                {
                    content[0] = 4;
                    content[1] = 2;
                    content[2] = 24;
                    content[3] = 8;
                }

                writer.WriteBytes(content);
                if (metaType != EventStatusBytes.Meta.EndOfTrack)
                    writer.WriteBytes(new byte[] { 0, 0xFF, 0x2F, 0 });
            }

            var settings = new ReadingSettings { MaxMemorySize = 10000 };
            bytes.Position = 0;
            var expectedFile = MidiFile.Read(bytes, settings);
            using var input = new AsyncOnlyStream(bytes.ToArray(), false, 997);
            var actualFile = await MidiFile.ReadAsync(input, settings);

            MidiAsserts.AreEqual(expectedFile, actualFile, true, "Async fixed-meta padding handling differs.");
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
                var expectedFile = MidiFile.Read(bytes, settings);
                var actualFile = await MidiFile.ReadAsync(input, settings);

                MidiAsserts.AreEqual(expectedFile, actualFile, true, "Async decoded-text retention differs.");
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
            ClassicAssert.AreEqual(0, outputStream.AsyncWritesCount, "Pre-canceled writing touched the stream.");
            ClassicAssert.IsFalse(outputStream.IsDisposed, "Pre-canceled writing disposed the caller-owned stream.");
        }

        [Test]
        public async Task ReadAsync_CancellationDuringStreamIO_LeavesStreamOpen()
        {
            using var bytes = new MemoryStream();
            CreateFile().Write(bytes);
            using var input = new AsyncOnlyStream(bytes.ToArray());
            input.ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var cancellation = new CancellationTokenSource();
            var pendingRead = MidiFile.ReadAsync(input, cancellationToken: cancellation.Token);
            await input.ReadStarted.Task;
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await pendingRead, "Cancellation did not interrupt the pending read.");
            ClassicAssert.IsFalse(input.IsDisposed, "Canceled reading disposed the caller-owned stream.");
        }

        [Test]
        public async Task ExistingValidCorpus_AsyncReadAndWriteMatchSynchronous()
        {
            foreach (var path in TestFilesProvider.GetValidFilesPaths())
            {
                var expectedFile = MidiFile.Read(path);
                var actualFile = await MidiFile.ReadAsync(path);

                MidiAsserts.AreEqual(expectedFile, actualFile, true, path + ": Async corpus reading differs.");

                using var synchronousOutputStream = new MemoryStream();
                using var asynchronousOutputStream = new MemoryStream();
                expectedFile.Write(synchronousOutputStream);
                await expectedFile.WriteAsync(asynchronousOutputStream);

                ClassicAssert.AreEqual(synchronousOutputStream.ToArray(), asynchronousOutputStream.ToArray(), path + ": Async corpus writing differs.");
            }
        }

        [Test]
        public async Task ExistingInvalidCorpus_AsyncReadMatchesSynchronousPolicies([Values] bool ignoreTruncatedData)
        {
            foreach (var path in Directory.GetFiles(TestFilesProvider.GetInvalidFilesDirectory(""), "*.mid", SearchOption.AllDirectories))
            {
                var bytes = File.ReadAllBytes(path);
                MidiFile expectedFile = null;
                Exception expectedError = null;
                var settings = new ReadingSettings
                {
                    NotEnoughBytesPolicy = ignoreTruncatedData ? NotEnoughBytesPolicy.Ignore : NotEnoughBytesPolicy.Abort
                };
                try
                {
                    expectedFile = MidiFile.Read(new MemoryStream(bytes), settings);
                }
                catch (Exception error)
                {
                    expectedError = error;
                }

                MidiFile actualFile = null;
                Exception actualError = null;
                try
                {
                    actualFile = await MidiFile.ReadAsync(new MemoryStream(bytes), settings);
                }
                catch (Exception error)
                {
                    actualError = error;
                }

                ClassicAssert.AreEqual(expectedError?.GetType(), actualError?.GetType(), path + ": Async read exception type differs: " + actualError);
                if (expectedError == null)
                    MidiAsserts.AreEqual(expectedFile, actualFile, true, path + ": Async invalid-corpus reading differs.");
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
            var expectedFile = MidiFile.Read(new MemoryStream(bytes.ToArray()), settings);
            var actualFile = await MidiFile.ReadAsync(new MemoryStream(bytes.ToArray()), settings);

            ClassicAssert.AreEqual(expectedFile.EstimatedMemorySize, actualFile.EstimatedMemorySize, "Async estimated memory size differs.");
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
            using var synchronousInput = new MemoryStream(data);
            var expectedFile = MidiFile.Read(synchronousInput, settings);
            using var input = new AsyncOnlyStream(data, false, 2);

            var file = await MidiFile.ReadAsync(input, settings);

            ClassicAssert.AreEqual(new byte[] { 1, 2, 3 }, ((UnknownChunk)file.Chunks[0]).Data, "Malformed-length handling did not preserve the available payload.");
            ClassicAssert.AreEqual(expectedFile.EstimatedMemorySize, file.EstimatedMemorySize, "Malformed-length handling did not account for the actual retained payload.");
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

        private static MidiFile CreateFile() => new MidiFile(new TrackChunk(
            new NoteOnEvent { Velocity = (SevenBitNumber)100 },
            new NoteOffEvent { DeltaTime = 100 }))
        {
            _originalFormat = (ushort)MidiFileFormat.MultiTrack
        };
    }
}
