using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Nested types

        public enum LazyWriterFinalizationOperation
        {
            EndTrackChunk,
            StartTrackChunk,
            Dispose
        }

        #endregion

        #region Test methods

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
        public async Task PathOverloads_RoundTripAndOwnHandles()
        {
            var path = "async-midi-" + Guid.NewGuid().ToString("N") + ".mid";

            try
            {
                var expectedFile = new MidiFile(new TrackChunk(
                    new NoteOnEvent { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent { DeltaTime = 100 }))
                {
                    _originalFormat = (ushort)MidiFileFormat.MultiTrack
                };
                await expectedFile.WriteAsync(path);

                MidiAsserts.AreEqual(expectedFile, await MidiFile.ReadAsync(path), true, "Async path round-trip file is invalid.");
                Assert.ThrowsAsync<IOException>(async () => await expectedFile.WriteAsync(path), "Writing overwrote an existing file without permission.");

                await using (var writer = await MidiFile.WriteLazyAsync(path, overwriteFile: true))
                {
                    await writer.StartTrackChunkAsync();
                    await writer.WriteEventAsync(new NoteOnEvent());
                }

                await using (var reader = await MidiFile.ReadLazyAsync(path))
                {
                    var tokenCount = 0;

                    await foreach (var token in reader.EnumerateTokensAsync())
                    {
                        tokenCount++;
                    }

                    ClassicAssert.AreEqual(5, tokenCount, "Lazy path reading returned an invalid token count.");
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
        public async Task LazyWriter_RejectsOverlappingOperations()
        {
            using var output = new AsyncOnlyStream();
            var writer = await MidiFile.WriteLazyAsync(output, new WritingSettings
            {
                WriterSettings = new WriterSettings { UseBuffering = false }
            });
            output.WriteGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            output.WriteStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pendingWrite = writer.StartTrackChunkAsync();
            await output.WriteStarted.Task;

            Assert.Throws<InvalidOperationException>(() => writer.WriteEvent(new NoteOnEvent()), "Synchronous writing accepted an overlapping operation.");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.DisposeAsync(), "Disposal accepted an overlapping write.");

            output.WriteGate.SetResult(true);
            await pendingWrite;
            await writer.DisposeAsync();
        }

        [Test]
        public async Task LazyWriter_AsyncFinalizationCallback_CanEmitDeferredEvent(
            [Values] LazyWriterFinalizationOperation operation)
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

            if (operation == LazyWriterFinalizationOperation.EndTrackChunk)
                await writer.EndTrackChunkAsync();
            else if (operation == LazyWriterFinalizationOperation.StartTrackChunk)
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
        public void WriteLazy_ValidFiles()
        {
            foreach (var midiFilePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(midiFilePath);
                WriteLazy(
                    writerActions: (writer, asynchronous) =>
                    {
                        foreach (var chunk in midiFile.Chunks)
                        {
                            if (chunk is TrackChunk trackChunk)
                            {
                                if (asynchronous)
                                    writer.StartTrackChunkAsync(CancellationToken.None).Wait();
                                else
                                    writer.StartTrackChunk();

                                foreach (var midiEvent in trackChunk.Events)
                                {
                                    if (asynchronous)
                                        writer.WriteEventAsync(midiEvent.Clone(), CancellationToken.None).Wait();
                                    else
                                        writer.WriteEvent(midiEvent.Clone());
                                }

                                if (asynchronous)
                                    writer.EndTrackChunkAsync(CancellationToken.None).Wait();
                                else
                                    writer.EndTrackChunk();
                            }
                            else
                            {
                                if (asynchronous)
                                    writer.WriteChunkAsync(chunk.Clone(), CancellationToken.None).Wait();
                                else
                                    writer.WriteChunk(chunk.Clone());
                            }
                        }
                    },
                    expectedMidiFile: midiFile,
                    format: midiFile.OriginalFormat,
                    timeDivision: midiFile.TimeDivision,
                    message: $"{midiFilePath}.");
            }
        }

        [Test]
        public void WriteLazy_EmptyFile() => WriteLazy(
            writerActions: (writer, asynchronous) => { },
            expectedMidiFile: new MidiFile { _originalFormat = (ushort?)MidiFileFormat.MultiTrack });

        [Test]
        public void WriteLazy_EmptyTrackChunks() => WriteLazy(
            writerActions: (writer, asynchronous) =>
            {
                if (asynchronous)
                {
                    writer.StartTrackChunkAsync(CancellationToken.None).Wait();
                    writer.EndTrackChunkAsync(CancellationToken.None).Wait();
                    writer.StartTrackChunkAsync(CancellationToken.None).Wait();
                    writer.EndTrackChunkAsync(CancellationToken.None).Wait();
                }
                else
                {
                    writer.StartTrackChunk();
                    writer.EndTrackChunk();
                    writer.StartTrackChunk();
                    writer.EndTrackChunk();
                }
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk())
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack,
                TimeDivision = new TicksPerQuarterNoteTimeDivision(190)
            },
            timeDivision: new TicksPerQuarterNoteTimeDivision(190));

        [Test]
        public void WriteLazy_Settings() => WriteLazy(
            writerActions: (writer, asynchronous) =>
            {
                if (asynchronous)
                {
                    writer.StartTrackChunkAsync(CancellationToken.None).Wait();
                    writer.WriteEventAsync(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), CancellationToken.None).Wait();
                    writer.WriteEventAsync(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), CancellationToken.None).Wait();
                    writer.EndTrackChunkAsync(CancellationToken.None).Wait();
                }
                else
                {
                    writer.StartTrackChunk();
                    writer.WriteEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue));
                    writer.WriteEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MaxValue));
                    writer.EndTrackChunk();
                }
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MinValue)))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            },
            settings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = true
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        #endregion

        #region Private methods

        private void WriteLazy(
            Action<MidiTokensWriter, bool> writerActions,
            MidiFile expectedMidiFile,
            WritingSettings settings = null,
            ReadingSettings readingSettings = null,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            TimeDivision timeDivision = null,
            string message = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                using (var writer = MidiFile.WriteLazy(filePath, true, format, settings, timeDivision))
                {
                    writerActions(writer, false);
                }

                var actualMidiFile = MidiFile.Read(filePath, readingSettings);
                MidiAsserts.AreEqual(expectedMidiFile, actualMidiFile, true, $"Invalid file. {message}");

                var asyncWriter = MidiFile.WriteLazyAsync(
                    filePath,
                    overwriteFile: true,
                    format: format,
                    settings: settings,
                    timeDivision: timeDivision,
                    cancellationToken: CancellationToken.None).Result;

                try
                {
                    writerActions(asyncWriter, true);
                }
                finally
                {
                    asyncWriter.DisposeAsync().AsTask().Wait();
                }

                actualMidiFile = MidiFileTestUtilities.ReadWithAsyncChecks(filePath, readingSettings);
                MidiAsserts.AreEqual(expectedMidiFile, actualMidiFile, true, $"Invalid async path file. {message}");

                using (var stream = new MemoryStream())
                {
                    asyncWriter = MidiFile.WriteLazyAsync(
                        stream,
                        settings,
                        format,
                        timeDivision,
                        CancellationToken.None).Result;

                    try
                    {
                        writerActions(asyncWriter, true);
                    }
                    finally
                    {
                        asyncWriter.DisposeAsync().AsTask().Wait();
                    }

                    stream.Position = 0;
                    actualMidiFile = MidiFile.ReadAsync(
                        stream,
                        readingSettings,
                        cancellationToken: CancellationToken.None).Result;

                    MidiAsserts.AreEqual(expectedMidiFile, actualMidiFile, true, $"Invalid async stream file. {message}");
                }
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
