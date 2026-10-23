using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

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
