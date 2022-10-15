using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;

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
                    writerActions: writer =>
                    {
                        foreach (var chunk in midiFile.Chunks)
                        {
                            if (chunk is TrackChunk trackChunk)
                            {
                                writer.StartTrackChunk();

                                foreach (var midiEvent in trackChunk.Events)
                                {
                                    writer.WriteEvent(midiEvent);
                                }

                                writer.EndTrackChunk();
                            }
                            else
                                writer.WriteChunk(chunk);
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
            writerActions: writer => { },
            expectedMidiFile: new MidiFile { _originalFormat = (ushort?)MidiFileFormat.MultiTrack });

        [Test]
        public void WriteLazy_EmptyTrackChunks() => WriteLazy(
            writerActions: writer =>
            {
                writer.StartTrackChunk();
                writer.EndTrackChunk();
                writer.StartTrackChunk();
                writer.EndTrackChunk();
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
            writerActions: writer =>
            {
                writer.StartTrackChunk();
                writer.WriteEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue));
                writer.WriteEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MaxValue));
                writer.EndTrackChunk();
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
            Action<MidiTokensWriter> writerActions,
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
                    writerActions(writer);
                }

                var actualMidiFile = MidiFile.Read(filePath, readingSettings);
                MidiAsserts.AreEqual(expectedMidiFile, actualMidiFile, true, $"Invalid file. {message}");
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
