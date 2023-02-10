using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

        [Test]
        public void ReadLazy_EmptyFile([Values] MidiFileFormat format) => ReadLazy(
            midiFile: new MidiFile(),
            format: format,
            writingSettings: null,
            readingSettings: null,
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)format, new TicksPerQuarterNoteTimeDivision(), 0) { Position = 8, Length = 6 }
            });

        [Test]
        public void ReadLazy_SingleTrackChunk_SingleEvent([Values] MidiFileFormat format) => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 })),
            format: format,
            writingSettings: null,
            readingSettings: null,
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)format, new TicksPerQuarterNoteTimeDivision(), 1) { Position = 8, Length = 6 },
                
                new ChunkHeaderToken(TrackChunk.Id, 8) { Position = 14, Length = 8 },
                new MidiEventToken(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 }) { Position = 22, Length = 4 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 26, Length = 4 }
            });

        [Test]
        public void ReadLazy_SingleTrackChunk_MultipleEvents([Values] MidiFileFormat format) => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50, Channel = (FourBitNumber)4 })),
            format: format,
            writingSettings: null,
            readingSettings: null,
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)format, new TicksPerQuarterNoteTimeDivision(), 1) { Position = 8, Length = 6 },
                
                new ChunkHeaderToken(TrackChunk.Id, 12) { Position = 14, Length = 8 },
                new MidiEventToken(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 }) { Position = 22, Length = 4 },
                new MidiEventToken(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50, Channel = (FourBitNumber)4 }){ Position = 26, Length = 4 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 30, Length = 4 }
            });

        [Test]
        public void ReadLazy_SingleTrackChunk_MultipleEvents_RunningStatus([Values] MidiFileFormat format) => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50, Channel = (FourBitNumber)4 })),
            format: format,
            writingSettings: new WritingSettings
            {
                UseRunningStatus = true,
                NoteOffAsSilentNoteOn = true
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOff
            },
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)format, new TicksPerQuarterNoteTimeDivision(), 1) { Position = 8, Length = 6 },
                
                new ChunkHeaderToken(TrackChunk.Id, 11) { Position = 14, Length = 8 },
                new MidiEventToken(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 }) { Position = 22, Length = 4 },
                new MidiEventToken(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50, Channel = (FourBitNumber)4 }) { Position = 26, Length = 3 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 29, Length = 4 }
            });

        [Test]
        public void ReadLazy_MultipleTrackChunks([Values(MidiFileFormat.MultiTrack, MidiFileFormat.MultiSequence)] MidiFileFormat format) => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50, Channel = (FourBitNumber)4 })),
            format: format,
            writingSettings: new WritingSettings
            {
                UseRunningStatus = true,
                NoteOffAsSilentNoteOn = true
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOff
            },
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)format, new TicksPerQuarterNoteTimeDivision(), 2) { Position = 8, Length = 6 },

                new ChunkHeaderToken(TrackChunk.Id, 8) { Position = 14, Length = 8 },
                new MidiEventToken(new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)) { Position = 22, Length = 4 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 26, Length = 4 },
                
                new ChunkHeaderToken(TrackChunk.Id, 11) { Position = 30, Length = 8 },
                new MidiEventToken(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100, Channel = (FourBitNumber)4 }) { Position = 38, Length = 4 },
                new MidiEventToken(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50, Channel = (FourBitNumber)4 }) { Position = 42, Length = 3 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 45, Length = 4 }
            });

        [Test]
        public void ReadLazy_UnknownChunk_1() => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } }),
            format: MidiFileFormat.MultiTrack,
            writingSettings: null,
            readingSettings: new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BytesPacketMaxLength = 2
                }
            },
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)MidiFileFormat.MultiTrack, new TicksPerQuarterNoteTimeDivision(), 1) { Position = 8, Length = 6 },

                new ChunkHeaderToken(TrackChunk.Id, 8) { Position = 14, Length = 8 },
                new MidiEventToken(new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)) { Position = 22, Length = 4 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 26, Length = 4 },

                new ChunkHeaderToken("Unkn", 8) { Position = 30, Length = 8 },
                new BytesPacketToken(new byte[] { 1, 2 }) { Position = 38, Length = 2 },
                new BytesPacketToken(new byte[] { 3, 4 }) { Position = 40, Length = 2 },
                new BytesPacketToken(new byte[] { 5, 6 }) { Position = 42, Length = 2 },
                new BytesPacketToken(new byte[] { 7, 8 }) { Position = 44, Length = 2 },
            });

        [Test]
        public void ReadLazy_UnknownChunk_2() => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)),
                new UnknownChunk("Unkn") { Data = new byte[0] }),
            format: MidiFileFormat.MultiTrack,
            writingSettings: null,
            readingSettings: new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BytesPacketMaxLength = 2
                }
            },
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)MidiFileFormat.MultiTrack, new TicksPerQuarterNoteTimeDivision(), 1) { Position = 8, Length = 6 },

                new ChunkHeaderToken(TrackChunk.Id, 8) { Position = 14, Length = 8 },
                new MidiEventToken(new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)) { Position = 22, Length = 4 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 26, Length = 4 },

                new ChunkHeaderToken("Unkn", 0) { Position = 30, Length = 8 }
            });

        [Test]
        public void ReadLazy_UnknownChunk_3() => ReadLazy(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)),
                new UnknownChunk("Unkn") { Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 } }),
            format: MidiFileFormat.MultiTrack,
            writingSettings: null,
            readingSettings: new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BytesPacketMaxLength = 2
                }
            },
            expectedTokens: new MidiToken[]
            {
                new ChunkHeaderToken(HeaderChunk.Id, 6) { Position = 0, Length = 8 },
                new FileHeaderToken((ushort)MidiFileFormat.MultiTrack, new TicksPerQuarterNoteTimeDivision(), 1) { Position = 8, Length = 6 },

                new ChunkHeaderToken(TrackChunk.Id, 8) { Position = 14, Length = 8 },
                new MidiEventToken(new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue)) { Position = 22, Length = 4 },
                new MidiEventToken(new EndOfTrackEvent()) { Position = 26, Length = 4 },

                new ChunkHeaderToken("Unkn", 9) { Position = 30, Length = 8 },
                new BytesPacketToken(new byte[] { 1, 2 }) { Position = 38, Length = 2 },
                new BytesPacketToken(new byte[] { 3, 4 }) { Position = 40, Length = 2 },
                new BytesPacketToken(new byte[] { 5, 6 }) { Position = 42, Length = 2 },
                new BytesPacketToken(new byte[] { 7, 8 }) { Position = 44, Length = 2 },
                new BytesPacketToken(new byte[] { 9 }) { Position = 46, Length = 1 },
            });

        #endregion

        #region Private methods

        public void ReadLazy(
            MidiFile midiFile,
            MidiFileFormat format,
            WritingSettings writingSettings,
            ReadingSettings readingSettings,
            MidiToken[] expectedTokens)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                midiFile.Write(filePath, format: format, settings: writingSettings);

                using (var reader = MidiFile.ReadLazy(filePath, readingSettings))
                {
                    var actualTokens = ReadAllTokens(reader);

                    Assert.AreEqual(expectedTokens.Length, actualTokens.Length, "Invalid tokens count.");

                    for (var i = 0; i < expectedTokens.Length; i++)
                    {
                        var expectedToken = expectedTokens[i];
                        var actualToken = actualTokens[i];
                        Assert.IsTrue(AreTokensEqual(expectedToken, actualToken), $"Invalid token {i}. Actual: {actualToken}. Expected: {expectedToken}.");
                    }
                }
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        private MidiToken[] ReadAllTokens(MidiTokensReader reader) =>
            reader.EnumerateTokens().ToArray();

        private bool AreTokensEqual(MidiToken expectedToken, MidiToken actualToken)
        {
            if (expectedToken.TokenType != actualToken.TokenType)
                return false;

            if (!expectedToken.GetType().Equals(actualToken.GetType()))
                return false;

            Assert.AreEqual(expectedToken.Position, actualToken.Position, "Position is invalid.");
            Assert.AreEqual(expectedToken.Length, actualToken.Length, "Length is invalid.");

            var expectedChunkHeaderToken = expectedToken as ChunkHeaderToken;
            if (expectedChunkHeaderToken != null)
            {
                var actualChunkHeaderToken = actualToken as ChunkHeaderToken;
                return
                    actualChunkHeaderToken != null &&
                    actualChunkHeaderToken.ChunkId == expectedChunkHeaderToken.ChunkId &&
                    actualChunkHeaderToken.ChunkContentSize == expectedChunkHeaderToken.ChunkContentSize;
            }

            var expectedFileHeaderToken = expectedToken as FileHeaderToken;
            if (expectedFileHeaderToken != null)
            {
                var actualFileHeaderToken = actualToken as FileHeaderToken;
                return
                    actualFileHeaderToken != null &&
                    actualFileHeaderToken.FileFormat == expectedFileHeaderToken.FileFormat &&
                    actualFileHeaderToken.TimeDivision.Equals(expectedFileHeaderToken.TimeDivision) &&
                    actualFileHeaderToken.TracksNumber == expectedFileHeaderToken.TracksNumber;
            }

            var expectedMidiEventToken = expectedToken as MidiEventToken;
            if (expectedMidiEventToken != null)
            {
                var actualMidiEventToken = actualToken as MidiEventToken;
                return
                    actualMidiEventToken != null &&
                    MidiEvent.Equals(expectedMidiEventToken.Event, actualMidiEventToken.Event);
            }

            var expectedBytesPacketToken = expectedToken as BytesPacketToken;
            if (expectedBytesPacketToken != null)
            {
                var actualBytesPacketToken = actualToken as BytesPacketToken;
                return
                    actualBytesPacketToken != null &&
                    actualBytesPacketToken.Data.SequenceEqual(expectedBytesPacketToken.Data);
            }

            return false;
        }

        #endregion
    }
}
