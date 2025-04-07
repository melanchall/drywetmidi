using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

        [Test]
        public void Read_SmpteTimeDivision_1()
        {
            var midiFile = MidiFileTestUtilities.Read(
                midiFile: new byte[]
                {
                    0x4D, 0x54, 0x68, 0x64, // MThd
                    0x00, 0x00, 0x00, 0x06, // length = 6
                    0x00, 0x00,             // format = 0
                    0x00, 0x01,             // tracks number = 1
                    0xE2, 0x50,             // time division
                },
                readingSettings: null);

            ClassicAssert.IsInstanceOf(typeof(SmpteTimeDivision), midiFile.TimeDivision, "Invalid time division type.");

            var smpteTimeDivision = (SmpteTimeDivision)midiFile.TimeDivision;
            ClassicAssert.AreEqual(SmpteFormat.Thirty, smpteTimeDivision.Format, "Invaid SMPTE format.");
            ClassicAssert.AreEqual(80, smpteTimeDivision.Resolution, "Invaid SMPTE resolution.");
        }

        [Test]
        public void Read_SmpteTimeDivision_2()
        {
            var originalTimeDivision = new SmpteTimeDivision(SmpteFormat.Thirty, 80);
            var midiFile = MidiFileTestUtilities.Read(
                midiFile: new MidiFile { TimeDivision = originalTimeDivision },
                writingSettings: null,
                readingSettings: null);

            ClassicAssert.IsInstanceOf(typeof(SmpteTimeDivision), midiFile.TimeDivision, "Invalid time division type.");

            var smpteTimeDivision = (SmpteTimeDivision)midiFile.TimeDivision;
            ClassicAssert.AreEqual(SmpteFormat.Thirty, smpteTimeDivision.Format, "Invaid SMPTE format.");
            ClassicAssert.AreEqual(80, smpteTimeDivision.Resolution, "Invaid SMPTE resolution.");

            ClassicAssert.AreEqual(originalTimeDivision, midiFile.TimeDivision, "Invalid time division.");
        }

        [Test]
        public void Read_StopReadingOnExpectedTrackChunksCountReached_Single_EmptyFile() => Read_StopReadingOnExpectedTrackChunksCountReached(
            midiFile: new MidiFile());

        [Test]
        public void Read_StopReadingOnExpectedTrackChunksCountReached_Single_MultipleTrackChunks() => Read_StopReadingOnExpectedTrackChunksCountReached(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new ProgramChangeEvent((SevenBitNumber)70),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("B"))));

        [Test]
        public void Read_StopReadingOnExpectedTrackChunksCountReached_Multiple_EmptyFiles() => Read_StopReadingOnExpectedTrackChunksCountReached(
            midiFiles: new[]
            {
                new MidiFile(),
                new MidiFile()
            });

        [Test]
        public void Read_StopReadingOnExpectedTrackChunksCountReached_Multiple_MultipleTrackChunks() => Read_StopReadingOnExpectedTrackChunksCountReached(
            midiFiles: new[]
            {
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("A"),
                        new NoteOnEvent(),
                        new NoteOffEvent()),
                    new TrackChunk(
                        new ProgramChangeEvent((SevenBitNumber)70),
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                        new TextEvent("B"))),
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("C"),
                        new ProgramChangeEvent((SevenBitNumber)10) { Channel = (FourBitNumber)4 }),
                    new TrackChunk(
                        new NoteOnEvent(),
                        new ProgramChangeEvent((SevenBitNumber)70),
                        new NoteOffEvent())),
            });

        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        public void Read_UnexpectedTrackChunksCount_Abort(byte countInHeaderChunk, byte actualCount)
        {
            // TODO: track chunks without events are excluded...
            ReadInvalidFileWithException<UnexpectedTrackChunksCountException>(
                new MidiFile(Enumerable.Range(0, actualCount).Select(i => new TrackChunk(new ProgramChangeEvent { Channel = (FourBitNumber)i }))),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [10] = 0,
                    [11] = countInHeaderChunk,
                },
                exception =>
                {
                    ClassicAssert.AreEqual(actualCount, exception.ActualCount, "Expected count of chunks is invalid.");
                    ClassicAssert.AreEqual(countInHeaderChunk, exception.ExpectedCount, "Actual count of chunks is invalid.");
                });
        }

        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        public void Read_UnexpectedTrackChunksCount_Ignore(byte countInHeaderChunk, byte actualCount)
        {
            // TODO: track chunks without events are excluded...
            ReadInvalidFile(
                new MidiFile(Enumerable.Range(0, actualCount).Select(i => new TrackChunk(new ProgramChangeEvent { Channel = (FourBitNumber)i }))),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore
                },
                new Dictionary<int, byte>
                {
                    [10] = 0,
                    [11] = countInHeaderChunk,
                },
                midiFile => ClassicAssert.AreEqual(actualCount, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
        }

        [TestCase(1, 2)]
        [TestCase(0, 1)]
        [TestCase(0, 5)]
        public void Read_ExtraTrackChunk_Read(byte countInHeaderChunk, byte actualCount)
        {
            ReadInvalidFile(
                new MidiFile(Enumerable.Range(0, actualCount).Select(i => new TrackChunk(new ProgramChangeEvent { Channel = (FourBitNumber)i }))),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    ExtraTrackChunkPolicy = ExtraTrackChunkPolicy.Read
                },
                new Dictionary<int, byte>
                {
                    [10] = 0,
                    [11] = countInHeaderChunk,
                },
                midiFile => ClassicAssert.AreEqual(actualCount, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
        }

        [TestCase(1, 2)]
        [TestCase(0, 1)]
        [TestCase(0, 5)]
        public void Read_ExtraTrackChunk_Skip(byte countInHeaderChunk, byte actualCount)
        {
            ReadInvalidFile(
                new MidiFile(Enumerable.Range(0, actualCount).Select(i => new TrackChunk(new ProgramChangeEvent { Channel = (FourBitNumber)i }))),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    ExtraTrackChunkPolicy = ExtraTrackChunkPolicy.Skip
                },
                new Dictionary<int, byte>
                {
                    [10] = 0,
                    [11] = countInHeaderChunk,
                },
                midiFile => ClassicAssert.AreEqual(countInHeaderChunk, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
        }

        [TestCase(" Thc")]
        [TestCase("MTrm")]
        public void Read_UnknownChunkId_FirstChunk_Abort(string chunkId)
        {
            ReadInvalidFileWithException<UnknownChunkException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [0] = Convert.ToByte(chunkId[0]),
                    [1] = Convert.ToByte(chunkId[1]),
                    [2] = Convert.ToByte(chunkId[2]),
                    [3] = Convert.ToByte(chunkId[3]),
                },
                exception => ClassicAssert.AreEqual(chunkId, exception.ChunkId, "Chunk ID is invalid in exception."));
        }

        [TestCase(" Thc")]
        [TestCase("MTrm")]
        public void Read_UnknownChunkId_SecondChunk_Abort(string chunkId)
        {
            ReadInvalidFileWithException<UnknownChunkException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [14] = Convert.ToByte(chunkId[0]),
                    [15] = Convert.ToByte(chunkId[1]),
                    [16] = Convert.ToByte(chunkId[2]),
                    [17] = Convert.ToByte(chunkId[3]),
                },
                exception => ClassicAssert.AreEqual(chunkId, exception.ChunkId, "Chunk ID is invalid in exception."));
        }

        [TestCase(" Thc")]
        [TestCase("MTrm")]
        public void Read_UnknownChunkId_OneChunk_Skip(string chunkId)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip
                },
                new Dictionary<int, byte>
                {
                    [14] = Convert.ToByte(chunkId[0]),
                    [15] = Convert.ToByte(chunkId[1]),
                    [16] = Convert.ToByte(chunkId[2]),
                    [17] = Convert.ToByte(chunkId[3]),
                },
                midiFile => ClassicAssert.AreEqual(0, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
        }

        [TestCase(" Thc")]
        [TestCase("MTrm")]
        public void Read_UnknownChunkId_TwoChunks_Skip(string chunkId)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent { Channel = (FourBitNumber)1 }), new TrackChunk(new ProgramChangeEvent { Channel = (FourBitNumber)2 })),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip
                },
                new Dictionary<int, byte>
                {
                    [14] = Convert.ToByte(chunkId[0]),
                    [15] = Convert.ToByte(chunkId[1]),
                    [16] = Convert.ToByte(chunkId[2]),
                    [17] = Convert.ToByte(chunkId[3]),
                },
                midiFile => ClassicAssert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
        }

        [TestCase(" Thc")]
        [TestCase("MTrm")]
        public void Read_UnknownChunkId_ReadAsUnknown(string chunkId)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.MultiTrack,
                null,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk
                },
                new Dictionary<int, byte>
                {
                    [14] = Convert.ToByte(chunkId[0]),
                    [15] = Convert.ToByte(chunkId[1]),
                    [16] = Convert.ToByte(chunkId[2]),
                    [17] = Convert.ToByte(chunkId[3]),
                },
                midiFile =>
                {
                    ClassicAssert.AreEqual(0, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
                    ClassicAssert.AreEqual(1, midiFile.Chunks.OfType<UnknownChunk>().Count(), "Unknown chunks count is invalid.");
                    ClassicAssert.AreEqual(chunkId, midiFile.Chunks.OfType<UnknownChunk>().First().ChunkId, "Chunk ID of unknown chunk is invalid.");
                });
        }

        [TestCase(128)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidChannelEventParameterValue_Abort(byte parameterValue)
        {
            ReadInvalidFileWithException<InvalidChannelEventParameterValueException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [24] = parameterValue
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.ProgramChange, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(parameterValue, exception.Value, "Parameter value is invalid.");
                });
        }

        [TestCase(0)]
        [TestCase(55)]
        [TestCase(127)]
        [TestCase(128)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidChannelEventParameterValue_ReadValid(byte parameterValue)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                },
                new Dictionary<int, byte>
                {
                    [24] = parameterValue
                },
                midiFile =>
                {
                    var programChangeEvent = midiFile.GetEvents().OfType<ProgramChangeEvent>().FirstOrDefault();
                    ClassicAssert.IsNotNull(programChangeEvent, "There is no Program Change event in the file.");
                    ClassicAssert.AreEqual(parameterValue & ((1 << 7) - 1), (int)programChangeEvent.ProgramNumber, "Program number is invalid.");
                });
        }

        [TestCase(0)]
        [TestCase(55)]
        [TestCase(127)]
        [TestCase(128)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidChannelEventParameterValue_SnapToLimits(byte parameterValue)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [24] = parameterValue
                },
                midiFile =>
                {
                    var programChangeEvent = midiFile.GetEvents().OfType<ProgramChangeEvent>().FirstOrDefault();
                    ClassicAssert.IsNotNull(programChangeEvent, "There is no Program Change event in the file.");
                    ClassicAssert.AreEqual(Math.Min((int)parameterValue, 127), (int)programChangeEvent.ProgramNumber, "Program number is invalid.");
                });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_Abort(byte statusByte, byte channel)
        {
            ReadInvalidFileWithException<UnknownChannelEventException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel)
                },
                exception =>
                {
                    ClassicAssert.AreEqual(statusByte, (byte)exception.StatusByte, "Status byte is invalid.");
                    ClassicAssert.AreEqual(channel, (byte)exception.Channel, "Channel is invalid.");
                });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_SkipStatusByte(byte statusByte, byte channel)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent(), new ControlChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByte
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel),
                    [24] = (byte)((statusByte << 4) + channel),
                },
                midiFile =>
                {
                    var controlChangeEvent = midiFile.GetEvents().SingleOrDefault() as ControlChangeEvent;
                    ClassicAssert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEqual(new ControlChangeEvent { DeltaTime = (((statusByte << 4) + channel) & ((1 << 7) - 1)) << 7 }, controlChangeEvent, true, "Event is invalid.");
                });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_SkipStatusByteAndOneDataByte(byte statusByte, byte channel)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent(), new ControlChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel),
                },
                midiFile =>
                {
                    var controlChangeEvent = midiFile.GetEvents().SingleOrDefault() as ControlChangeEvent;
                    ClassicAssert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEqual(new ControlChangeEvent(), controlChangeEvent, true, "Event is invalid.");
                });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_SkipStatusByteAndTwoDataBytes(byte statusByte, byte channel)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new NoteOnEvent(), new ControlChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndTwoDataBytes
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel),
                },
                midiFile =>
                {
                    var controlChangeEvent = midiFile.GetEvents().SingleOrDefault() as ControlChangeEvent;
                    ClassicAssert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEqual(new ControlChangeEvent(), controlChangeEvent, true, "Event is invalid.");
                });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_UseCallback_NoCallback(byte statusByte, byte channel)
        {
            ReadInvalidFileWithException<InvalidOperationException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.UseCallback
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel)
                },
                exception => { });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_UseCallback_Abort(byte statusByte, byte channel)
        {
            ReadInvalidFileWithException<UnknownChannelEventException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.UseCallback,
                    UnknownChannelEventCallback = (sb, c) => UnknownChannelEventAction.Abort
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel)
                },
                exception =>
                {
                    ClassicAssert.AreEqual(statusByte, (byte)exception.StatusByte, "Status byte is invalid.");
                    ClassicAssert.AreEqual(channel, (byte)exception.Channel, "Channel is invalid.");
                });
        }

        [TestCase(0xF, 0x3)]
        public void Read_InvalidChannelEvent_UseCallback_SkipData(byte statusByte, byte channel)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent(), new ControlChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.UseCallback,
                    UnknownChannelEventCallback = (sb, c) => UnknownChannelEventAction.SkipData(0)
                },
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel),
                    [24] = (byte)((statusByte << 4) + channel),
                },
                midiFile =>
                {
                    var controlChangeEvent = midiFile.GetEvents().SingleOrDefault() as ControlChangeEvent;
                    ClassicAssert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEqual(new ControlChangeEvent { DeltaTime = (((statusByte << 4) + channel) & ((1 << 7) - 1)) << 7 }, controlChangeEvent, true, "Event is invalid.");
                });
        }

        [TestCase(0)]
        [TestCase(5)]
        [TestCase(7)]
        public void Read_InvalidChunkSize_HeaderChunk_Abort(byte sizeLastByte)
        {
            ReadInvalidFileWithException<InvalidChunkSizeException>(
                new MidiFile(),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [7] = sizeLastByte
                },
                exception =>
                {
                    ClassicAssert.AreEqual(6, exception.ActualSize, "Actual size is invalid.");
                    ClassicAssert.AreEqual(HeaderChunk.Id, exception.ChunkId, "Chunk ID is invalid.");
                });
        }

        [TestCase(5)]
        [TestCase(255)]
        public void Read_InvalidChunkSize_TrackChunk_Abort(byte sizeLastByte)
        {
            ReadInvalidFileWithException<InvalidChunkSizeException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [21] = sizeLastByte
                },
                exception =>
                {
                    ClassicAssert.AreEqual(7, exception.ActualSize, "Actual size is invalid.");
                    ClassicAssert.AreEqual(TrackChunk.Id, exception.ChunkId, "Chunk ID is invalid.");
                });
        }

        [TestCase(0)]
        [TestCase(5)]
        public void Read_InvalidChunkSize_HeaderChunk_Ignore(byte sizeLastByte)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk()),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                },
                new Dictionary<int, byte>
                {
                    [7] = sizeLastByte
                },
                midiFile =>
                {
                    ClassicAssert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
                });
        }

        [TestCase(5)]
        [TestCase(255)]
        public void Read_InvalidChunkSize_TrackChunk_Ignore(byte sizeLastByte)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                },
                new Dictionary<int, byte>
                {
                    [21] = sizeLastByte
                },
                midiFile =>
                {
                    ClassicAssert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
                });
        }

        [TestCase(-8)]
        [TestCase(8)]
        [TestCase(sbyte.MinValue)]
        [TestCase(sbyte.MaxValue)]
        public void Read_InvalidKeySignatureKey_Abort(sbyte key)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new KeySignatureEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [26] = (byte)key
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.KeySignature, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(KeySignatureEvent.Key), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(key, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(-8)]
        [TestCase(sbyte.MinValue)]
        public void Read_InvalidKeySignatureKey_Negative_SnapToLimits(sbyte key)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new KeySignatureEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [26] = (byte)key
                },
                midiFile =>
                {
                    var keySignatureEvent = midiFile.GetEvents().SingleOrDefault() as KeySignatureEvent;
                    ClassicAssert.IsNotNull(keySignatureEvent, "There is no Key Signature event.");
                    ClassicAssert.AreEqual(Math.Max(key, (sbyte)-7), keySignatureEvent.Key, "Key is invalid");
                });
        }

        [TestCase(8)]
        [TestCase(sbyte.MaxValue)]
        public void Read_InvalidKeySignatureKey_Positive_SnapToLimits(sbyte key)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new KeySignatureEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [26] = (byte)key
                },
                midiFile =>
                {
                    var keySignatureEvent = midiFile.GetEvents().SingleOrDefault() as KeySignatureEvent;
                    ClassicAssert.IsNotNull(keySignatureEvent, "There is no Key Signature event.");
                    ClassicAssert.AreEqual(Math.Min(key, (sbyte)7), keySignatureEvent.Key, "Key is invalid");
                });
        }

        [TestCase(2)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidKeySignatureScale_Abort(byte scale)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new KeySignatureEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [27] = scale
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.KeySignature, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(KeySignatureEvent.Scale), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(scale, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(2)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidKeySignatureScale_SnapToLimits(byte scale)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new KeySignatureEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [27] = scale
                },
                midiFile =>
                {
                    var keySignatureEvent = midiFile.GetEvents().SingleOrDefault() as KeySignatureEvent;
                    ClassicAssert.IsNotNull(keySignatureEvent, "There is no Key Signature event.");
                    ClassicAssert.AreEqual(Math.Min(scale, (byte)1), keySignatureEvent.Scale, "Scale is invalid");
                });
        }

        [TestCase(24)]
        [TestCase((1 << 5) - 1)]
        public void Read_InvalidSmpteFrames_Abort_Hours(byte hours)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [26] = hours
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(SmpteOffsetEvent.Hours), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(hours, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(60)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidSmpteFrames_Abort_Minutes(byte minutes)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [27] = minutes
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(SmpteOffsetEvent.Minutes), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(minutes, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(60)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidSmpteFrames_Abort_Seconds(byte seconds)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [28] = seconds
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(SmpteOffsetEvent.Seconds), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(seconds, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(0, 24)]
        [TestCase(0, byte.MaxValue)]
        [TestCase(1, 25)]
        [TestCase(1, byte.MaxValue)]
        [TestCase(2, 29)]
        [TestCase(2, byte.MaxValue)]
        [TestCase(3, 30)]
        [TestCase(3, byte.MaxValue)]
        public void Read_InvalidSmpteFrames_Abort_Frames(byte type, byte frames)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [26] = (byte)(type << 5),
                    [29] = frames
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(SmpteOffsetEvent.Frames), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(frames, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(100)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidSmpteFrames_Abort_SubFrames(byte subFrames)
        {
            ReadInvalidFileWithException<InvalidMetaEventParameterValueException>(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [30] = subFrames
                },
                exception =>
                {
                    ClassicAssert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    ClassicAssert.AreEqual(nameof(SmpteOffsetEvent.SubFrames), exception.PropertyName, "Property name is invalid.");
                    ClassicAssert.AreEqual(subFrames, exception.Value, "Property value is invalid.");
                });
        }

        [TestCase(24)]
        [TestCase((1 << 5) - 1)]
        public void Read_InvalidSmpteFrames_SnapToLimits_Hours(byte hours)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [26] = hours
                },
                midiFile =>
                {
                    var smpteOffsetEvent = midiFile.GetEvents().SingleOrDefault() as SmpteOffsetEvent;
                    ClassicAssert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    ClassicAssert.AreEqual(Math.Min((byte)23, hours), smpteOffsetEvent.Hours, "Hours number is invalid.");
                });
        }

        [TestCase(60)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidSmpteFrames_SnapToLimits_Minutes(byte minutes)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [27] = minutes
                },
                midiFile =>
                {
                    var smpteOffsetEvent = midiFile.GetEvents().SingleOrDefault() as SmpteOffsetEvent;
                    ClassicAssert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    ClassicAssert.AreEqual(Math.Min((byte)59, minutes), smpteOffsetEvent.Minutes, "Minutes number is invalid.");
                });
        }

        [TestCase(60)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidSmpteFrames_SnapToLimits_Seconds(byte seconds)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [28] = seconds
                },
                midiFile =>
                {
                    var smpteOffsetEvent = midiFile.GetEvents().SingleOrDefault() as SmpteOffsetEvent;
                    ClassicAssert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    ClassicAssert.AreEqual(Math.Min((byte)59, seconds), smpteOffsetEvent.Seconds, "Seconds number is invalid.");
                });
        }

        [TestCase(0, 23, 24)]
        [TestCase(0, 23, byte.MaxValue)]
        [TestCase(1, 24, 25)]
        [TestCase(1, 24, byte.MaxValue)]
        [TestCase(2, 28, 29)]
        [TestCase(2, 28, byte.MaxValue)]
        [TestCase(3, 29, 30)]
        [TestCase(3, 29, byte.MaxValue)]
        public void Read_InvalidSmpteFrames_SnapToLimits_Frames(byte type, byte maxValue, byte frames)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [26] = (byte)(type << 5),
                    [29] = frames
                },
                midiFile =>
                {
                    var smpteOffsetEvent = midiFile.GetEvents().SingleOrDefault() as SmpteOffsetEvent;
                    ClassicAssert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    ClassicAssert.AreEqual(Math.Min(maxValue, frames), smpteOffsetEvent.Frames, "Frames number is invalid.");
                });
        }

        [TestCase(100)]
        [TestCase(byte.MaxValue)]
        public void Read_InvalidSmpteFrames_SnapToLimits_SubFrames(byte subFrames)
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new SmpteOffsetEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                },
                new Dictionary<int, byte>
                {
                    [30] = subFrames
                },
                midiFile =>
                {
                    var smpteOffsetEvent = midiFile.GetEvents().SingleOrDefault() as SmpteOffsetEvent;
                    ClassicAssert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    ClassicAssert.AreEqual(Math.Min((byte)99, subFrames), smpteOffsetEvent.SubFrames, "Sub-frames number is invalid.");
                });
        }

        [Test]
        public void Read_NoEndOfTrack_Abort()
        {
            ReadInvalidFileWithException<MissedEndOfTrackEventException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [26] = 0x90,
                    [27] = 0,
                    [28] = 0,
                },
                exception => { });
        }

        [Test]
        public void Read_NoEndOfTrack_Ignore()
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore,
                    SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
                },
                new Dictionary<int, byte>
                {
                    [26] = 0x90,
                    [27] = 0,
                    [28] = 0,
                },
                midiFile =>
                {
                    var midiEvents = midiFile.GetEvents().ToArray();
                    ClassicAssert.AreEqual(2, midiEvents.Length, "Events count is invalid.");
                    MidiAsserts.AreEqual(new ProgramChangeEvent(), midiEvents[0], true, "First MIDI event is invalid.");
                    MidiAsserts.AreEqual(new NoteOnEvent(), midiEvents[1], true, "Second MIDI event is invalid.");
                });
        }

        [Test]
        public void Read_NoHeaderChunk_Abort()
        {
            ReadInvalidFileWithException<NoHeaderChunkException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Abort,
                },
                new Dictionary<int, byte>(),
                exception => { },
                bytes => bytes.Skip(14).ToArray());
        }

        [Test]
        public void Read_NoHeaderChunk_Ignore()
        {
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore,
                },
                new Dictionary<int, byte>(),
                midiFile =>
                {
                    ClassicAssert.Throws<InvalidOperationException>(() => { var format = midiFile.OriginalFormat; }, "Exception not thrown on get original format.");
                    
                    var programChangeEvent = midiFile.GetEvents().SingleOrDefault() as ProgramChangeEvent;
                    MidiAsserts.AreEqual(new ProgramChangeEvent(), programChangeEvent, true, "Program Change event is invalid.");
                },
                bytes => bytes.Skip(14).ToArray());
        }

        [Test]
        public void Read_NotEnoughBytes_Abort_EndOfTrack([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new TrackChunk(new ProgramChangeEvent())),
            bytes => bytes.Take(bytes.Length - trimBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_ChannelPrefix([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new ChannelPrefixEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_CopyrightNotice([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new CopyrightNoticeEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_CuePoint([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new CuePointEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_DeviceName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new DeviceNameEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_InstrumentName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new InstrumentNameEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_KeySignature([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new KeySignatureEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_Lyric([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new LyricEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_Marker([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new MarkerEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_PortPrefix([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new PortPrefixEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_ProgramName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new ProgramNameEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_SequenceNumber([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new SequenceNumberEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_SequencerSpecific([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new SequencerSpecificEvent(new byte[] { 1, 2 }),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_SequenceTrackName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new SequenceTrackNameEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_SetTempo([Values(1, 2, 3, 4, 5, 6)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new SetTempoEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_SmpteOffset([Values(1, 2, 3, 4, 5, 6, 7, 8)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new SmpteOffsetEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_Text([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new TextEvent("AB"),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_TimeSignature([Values(1, 2, 3, 4, 5, 6, 7)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new TimeSignatureEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_UnknownMeta([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new UnknownMetaEvent(0x60, new byte[] { 1, 2 }),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_ChannelAftertouch([Values(1, 2)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new ChannelAftertouchEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_ControlChange([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new ControlChangeEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_NoteAftertouch([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new NoteAftertouchEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_NoteOff([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new NoteOffEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_NoteOn([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new NoteOnEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_PitchBend([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new PitchBendEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_ProgramChange([Values(1, 2)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new ProgramChangeEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_NormalSysEx([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new NormalSysExEvent(new byte[] { 1, 0xF7 }),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_EscapeSysEx([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(
            new EscapeSysExEvent(new byte[] { 1, 0xF7 }),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Abort_HeaderChunkId([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(),
            bytes => bytes.Take(takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_HeaderChunkSize([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(),
            bytes => bytes.Take(4 /* chunk ID */ + takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_HeaderChunkFileFormat() => Read_NotEnoughBytes_Abort(
            new MidiFile(),
            bytes => bytes.Take(4 /* chunk ID */ + 4 /* chunk size */ + 1).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_HeaderChunkTracksNumber() => Read_NotEnoughBytes_Abort(
            new MidiFile(),
            bytes => bytes.Take(4 /* chunk ID */ + 4 /* chunk size */ + 2 /* file format */ + 1).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_HeaderChunkTimeDivision() => Read_NotEnoughBytes_Abort(
            new MidiFile(),
            bytes => bytes.Take(4 /* chunk ID */ + 4 /* chunk size */ + 2 /* file format */ + 2 /* tracks numer */ + 1).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_TrackChunkId([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new TrackChunk(new TextEvent("A"))),
            bytes => bytes.Take(14 /* header chunk */ + takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_TrackChunkSize([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new TrackChunk(new TextEvent("A"))),
            bytes => bytes.Take(14 /* header chunk */ + 4 /* chunk ID */ + takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_UnknownChunkId([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new UnknownChunk("abcd")),
            bytes => bytes.Take(14 /* header chunk */ + takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_UnknownChunkSize([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new UnknownChunk("abcd")),
            bytes => bytes.Take(14 /* header chunk */ + 4 /* chunk ID */ + takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Abort_UnknownChunkData([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new UnknownChunk("abcd") { Data = new byte[] { 1, 2, 3, 4 } }),
            bytes => bytes.Take(14 /* header chunk */ + 4 /* chunk ID */ + 4 /* chunk size */ + takeBytesCount).ToArray());

        [Test]
        public void Read_NotEnoughBytes_Ignore_EndOfTrack([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new ProgramChangeEvent())),
            bytes => bytes.Take(bytes.Length - trimBytesCount).ToArray(),
            new MidiFile(new TrackChunk(new ProgramChangeEvent())));

        [Test]
        public void Read_NotEnoughBytes_Ignore_ChannelPrefix([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new ChannelPrefixEvent(1),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_CopyrightNotice([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new CopyrightNoticeEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new CopyrightNoticeEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_CuePoint([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new CuePointEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new CuePointEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_DeviceName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new DeviceNameEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new DeviceNameEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_InstrumentName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new InstrumentNameEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new InstrumentNameEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_KeySignature([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new KeySignatureEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_Lyric([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new LyricEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new LyricEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_Marker([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new MarkerEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new MarkerEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_PortPrefix([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new PortPrefixEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_ProgramName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new ProgramNameEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new ProgramNameEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_SequenceNumber([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new SequenceNumberEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_SequencerSpecific([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new SequencerSpecificEvent(new byte[] { 1, 2 }),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new SequencerSpecificEvent(new byte[] { 1, 2 }.Take(2 - trimBytesCount).ToArray()) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_SequenceTrackName([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new SequenceTrackNameEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new SequenceTrackNameEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_SetTempo([Values(1, 2, 3, 4, 5, 6)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new SetTempoEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_SmpteOffset([Values(1, 2, 3, 4, 5, 6, 7, 8)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new SmpteOffsetEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_Text([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new TextEvent("AB"),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new TextEvent("AB".Substring(0, 2 - trimBytesCount)) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_TimeSignature([Values(1, 2, 3, 4, 5, 6, 7)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new TimeSignatureEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_UnknownMeta([Values(1, 2, 3, 4, 5)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new UnknownMetaEvent(0x60, new byte[] { 1, 2 }),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new UnknownMetaEvent(0x60, new byte[] { 1, 2 }.Take(2 - trimBytesCount).ToArray()) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_ChannelAftertouch([Values(1, 2)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new ChannelAftertouchEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_ControlChange([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new ControlChangeEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_NoteAftertouch([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new NoteAftertouchEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_NoteOff([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new NoteOffEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_NoteOn([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new NoteOnEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_PitchBend([Values(1, 2, 3)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new PitchBendEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_ProgramChange([Values(1, 2)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new ProgramChangeEvent(),
            trimBytesCount);

        [Test]
        public void Read_NotEnoughBytes_Ignore_NormalSysEx([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new NormalSysExEvent(new byte[] { 1, 0xF7 }),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new NormalSysExEvent(new byte[] { 1, 0xF7 }.Take(2 - trimBytesCount).ToArray()) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_EscapeSysEx([Values(1, 2, 3, 4)] int trimBytesCount) => Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            new EscapeSysExEvent(new byte[] { 1, 0xF7 }),
            trimBytesCount,
            trimBytesCount > 2 ? null : new[] { new EscapeSysExEvent(new byte[] { 1, 0xF7 }.Take(2 - trimBytesCount).ToArray()) });

        [Test]
        public void Read_NotEnoughBytes_Ignore_HeaderChunkId([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))) { TimeDivision = new TicksPerQuarterNoteTimeDivision(80) },
            bytes => bytes.Take(takeBytesCount).ToArray(),
            new MidiFile { TimeDivision = null });

        [Test]
        public void Read_NotEnoughBytes_Ignore_HeaderChunkSize([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))) { TimeDivision = new TicksPerQuarterNoteTimeDivision(80) },
            bytes => bytes.Take(4 /* chunk ID */ + takeBytesCount).ToArray(),
            new MidiFile { TimeDivision = null });

        [Test]
        public void Read_NotEnoughBytes_Ignore_HeaderChunkFileFormat() => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))) { TimeDivision = new TicksPerQuarterNoteTimeDivision(80) },
            bytes => bytes.Take(4 /* chunk ID */ + 4 /* chunk size */ + 1).ToArray(),
            new MidiFile { TimeDivision = null });

        [Test]
        public void Read_NotEnoughBytes_Ignore_HeaderChunkTracksNumber() => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))) { TimeDivision = new TicksPerQuarterNoteTimeDivision(80) },
            bytes => bytes.Take(4 /* chunk ID */ + 4 /* chunk size */ + 2 /* file format */ + 1).ToArray(),
            new MidiFile { TimeDivision = null });

        [Test]
        public void Read_NotEnoughBytes_Ignore_HeaderChunkTimeDivision() => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))) { TimeDivision = new TicksPerQuarterNoteTimeDivision(80) },
            bytes => bytes.Take(4 /* chunk ID */ + 4 /* chunk size */ + 2 /* file format */ + 2 /* tracks number */ + 1).ToArray(),
            new MidiFile { TimeDivision = null });

        [Test]
        public void Read_NotEnoughBytes_Ignore_TrackChunkId([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))),
            bytes => bytes.Take(14 /* header chunk */ + takeBytesCount).ToArray(),
            new MidiFile());

        [Test]
        public void Read_NotEnoughBytes_Ignore_TrackChunkSize([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(new TextEvent("A"))),
            bytes => bytes.Take(14 /* header chunk */ + 4 /* chunk ID */ + takeBytesCount).ToArray(),
            new MidiFile(new TrackChunk()));

        [Test]
        public void Read_NotEnoughBytes_Ignore_UnknownChunkId([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new UnknownChunk("abcd")),
            bytes => bytes.Take(14 /* header chunk */ + takeBytesCount).ToArray(),
            new MidiFile());

        [Test]
        public void Read_NotEnoughBytes_Ignore_UnknownChunkSize([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new UnknownChunk("abcd") { Data = new byte[] { 1, 2, 3, 4 } }),
            bytes => bytes.Take(14 /* header chunk */ + 4 /* chunk ID */ + takeBytesCount).ToArray(),
            new MidiFile(new UnknownChunk("abcd")));

        [Test]
        public void Read_NotEnoughBytes_Ignore_UnknownChunkData([Values(1, 2, 3)] int takeBytesCount) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new UnknownChunk("abcd") { Data = new byte[] { 1, 2, 3, 4 } }),
            bytes => bytes.Take(14 /* header chunk */ + 4 /* chunk ID */ + 4 /* chunk size */ + takeBytesCount).ToArray(),
            new MidiFile(new UnknownChunk("abcd") { Data = new byte[] { 1, 2, 3, 4 }.Take(takeBytesCount).ToArray() }));

        [TestCase(0x7, 0x3)]
        public void Read_UnexpectedRunningStatus(byte statusByte, byte channel)
        {
            ReadInvalidFileWithException<UnexpectedRunningStatusException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent(), new ControlChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings(),
                new Dictionary<int, byte>
                {
                    [23] = (byte)((statusByte << 4) + channel),
                    [24] = (byte)((statusByte << 4) + channel),
                },
                exception => { });
        }

        [TestCase(3)]
        [TestCase(byte.MaxValue)]
        public void Read_UnknownFileFormat_Abort(byte formatLastByte)
        {
            ReadInvalidFileWithException<UnknownFileFormatException>(
                new MidiFile(),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownFileFormatPolicy = UnknownFileFormatPolicy.Abort
                },
                new Dictionary<int, byte>
                {
                    [9] = formatLastByte
                },
                exception => ClassicAssert.AreEqual(formatLastByte, exception.FileFormat, "File format is invalid."));
        }

        [TestCase(3)]
        [TestCase(byte.MaxValue)]
        public void Read_UnknownFileFormat_Ignore(byte formatLastByte)
        {
            ReadInvalidFile(
                new MidiFile(),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore
                },
                new Dictionary<int, byte>
                {
                    [9] = formatLastByte
                },
                midiFile =>
                {
                    var exception = ClassicAssert.Throws<UnknownFileFormatException>(() => { var format = midiFile.OriginalFormat; }, "Exception not thrown on get original format.");
                    ClassicAssert.AreEqual(formatLastByte, exception.FileFormat, "File format is invalid.");
                });
        }

        [Test]
        public void Read_EndOfTrackStoringPolicy_Omit()
        {
            var originalMidiFile = new MidiFile(new TrackChunk(new TextEvent("A")));
            var midiFile = MidiFileTestUtilities.Read(
                midiFile: originalMidiFile,
                writingSettings: new WritingSettings(),
                readingSettings: new ReadingSettings
                {
                    EndOfTrackStoringPolicy = EndOfTrackStoringPolicy.Omit
                });

            MidiAsserts.AreEqual(originalMidiFile, midiFile, false, "Files are invalid.");
        }

        [Test]
        public void Read_EndOfTrackStoringPolicy_Store_DontChangeFile() => Read_EndOfTrackStoringPolicy_Store(
            originalEvents: new[] { new TextEvent("A") },
            modifyEvents: null,
            expectedEventsAfterReread: new MidiEvent[]
            {
                new TextEvent("A"),
                new EndOfTrackEvent()
            });

        [Test]
        public void Read_EndOfTrackStoringPolicy_Store_AddEventAfter() => Read_EndOfTrackStoringPolicy_Store(
            originalEvents: new[] { new TextEvent("A") },
            modifyEvents: events => events.Add(new TextEvent("B")),
            expectedEventsAfterReread: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new EndOfTrackEvent()
            });

        [Test]
        public void Read_EndOfTrackStoringPolicy_Store_AddEventBefore() => Read_EndOfTrackStoringPolicy_Store(
            originalEvents: new[] { new TextEvent("A") },
            modifyEvents: events => events.Insert(1, new TextEvent("B")),
            expectedEventsAfterReread: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new EndOfTrackEvent()
            });

        [Test]
        public void Read_EndOfTrackStoringPolicy_Store_SetEndOfTrackDeltaTime() => Read_EndOfTrackStoringPolicy_Store(
            originalEvents: new[] { new TextEvent("A") },
            modifyEvents: events => events.Last().DeltaTime = 100,
            expectedEventsAfterReread: new MidiEvent[]
            {
                new TextEvent("A"),
                new EndOfTrackEvent { DeltaTime = 100 }
            });

        [Test]
        public void Read_EndOfTrackStoringPolicy_Store_SetEndOfTrackDeltaTime_AddEventAfter() => Read_EndOfTrackStoringPolicy_Store(
            originalEvents: new[] { new TextEvent("A") },
            modifyEvents: events =>
            {
                events.Last().DeltaTime = 100;
                events.Add(new TextEvent("B"));
            },
            expectedEventsAfterReread: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 100 },
                new EndOfTrackEvent()
            });

        [Test]
        public void Read_EndOfTrackStoringPolicy_Store_SetEndOfTrackDeltaTime_AddEventBefore() => Read_EndOfTrackStoringPolicy_Store(
            originalEvents: new[] { new TextEvent("A") },
            modifyEvents: events =>
            {
                events.Last().DeltaTime = 100;
                events.Insert(1, new TextEvent("B"));
            },
            expectedEventsAfterReread: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new EndOfTrackEvent { DeltaTime = 100 }
            });

        [Test]
        public void Read_StreamIsNotDisposed()
        {
            var midiFile = new MidiFile();

            using (var streamToWrite = new MemoryStream())
            {
                midiFile.Write(streamToWrite);

                using (var streamToRead = new MemoryStream(streamToWrite.ToArray()))
                {
                    var readMidiFile = MidiFile.Read(streamToRead);
                    ClassicAssert.DoesNotThrow(() => { var l = streamToRead.Length; });
                }
            }
        }

        [Test]
        public void Read_DecodeTextCallback_NoCallback()
        {
            const string text = "Just text";

            var midiFile = WriteRead(new MidiFile(new TrackChunk(new TextEvent(text))));
            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();

            ClassicAssert.AreEqual(text, textEvent.Text, "Text decoded incorrectly.");
        }

        [TestCase(null)]
        [TestCase("New text")]
        public void Read_DecodeTextCallback_ReturnNewText(string newText)
        {
            const string text = "Just text";

            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new TextEvent(text))),
                readingSettings: new ReadingSettings
                {
                    DecodeTextCallback = (bytes, settings) => newText
                });

            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();

            ClassicAssert.AreEqual(newText, textEvent.Text, "Text decoded incorrectly.");
        }

        [Test]
        public void Read_TextEncoding_NotSpecifiedOnRead()
        {
            const string text = "Just text";

            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new TextEvent(text))),
                writingSettings: new WritingSettings
                {
                    TextEncoding = Encoding.UTF32
                });

            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();

            ClassicAssert.AreNotEqual(text, textEvent.Text, "Text decoded incorrectly.");
        }

        [Test]
        public void Read_TextEncoding()
        {
            const string text = "Just text";

            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new TextEvent(text))),
                writingSettings: new WritingSettings
                {
                    TextEncoding = Encoding.UTF32
                },
                readingSettings: new ReadingSettings
                {
                    TextEncoding = Encoding.UTF32
                });

            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();

            ClassicAssert.AreEqual(text, textEvent.Text, "Text decoded incorrectly.");
        }

        [Test]
        public void Read_BufferAllData()
        {
            var noBufferingSettings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BufferingPolicy = BufferingPolicy.DontUseBuffering
                }
            };

            var bufferAllDataSettings = new ReadingSettings();
            bufferAllDataSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;

            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var expectedMidiFile = MidiFile.Read(filePath, noBufferingSettings);
                var midiFile = MidiFile.Read(filePath, bufferAllDataSettings);
                MidiAsserts.AreEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");
            }
        }

        [TestCase(4096)]
        [TestCase(1)]
        [TestCase(10000)]
        [TestCase(123)]
        public void Read_UseFixedSizeBuffer(int bufferSize)
        {
            var noBufferingSettings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BufferingPolicy = BufferingPolicy.DontUseBuffering
                }
            };

            var fixedSizeBufferingSettings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer,
                    BufferSize = bufferSize
                }
            };

            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var expectedMidiFile = MidiFile.Read(filePath, noBufferingSettings);
                var midiFile = MidiFile.Read(filePath, fixedSizeBufferingSettings);
                MidiAsserts.AreEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");
            }
        }

        [TestCase(4096, true)]
        [TestCase(1, false)]
        [TestCase(10000, true)]
        [TestCase(123, true)]
        public void Read_UseCustomBuffer(int bufferSize, bool checkData)
        {
            var noBufferingSettings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BufferingPolicy = BufferingPolicy.DontUseBuffering
                }
            };

            var buffer = new byte[bufferSize];
            var customBufferingSettings = new ReadingSettings
            {
                ReaderSettings = new ReaderSettings
                {
                    BufferingPolicy = BufferingPolicy.UseCustomBuffer,
                    Buffer = buffer
                }
            };

            var lastBufferData = buffer.ToArray();
            ClassicAssert.IsTrue(buffer.All(b => b == 0), "Initial buffer contains non-zero bytes.");

            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var expectedMidiFile = MidiFile.Read(filePath, noBufferingSettings);
                var midiFile = MidiFile.Read(filePath, customBufferingSettings);
                MidiAsserts.AreEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");

                if (checkData)
                {
                    CollectionAssert.AreNotEqual(lastBufferData, buffer, "Buffer contains the same data after reading a file.");
                    lastBufferData = buffer.ToArray();
                }
            }
        }

        #endregion

        #region Private methods

        private void Read_StopReadingOnExpectedTrackChunksCountReached(MidiFile midiFile)
        {
            using (var stream = new MemoryStream())
            {
                midiFile.Write(stream, settings: new WritingSettings
                {
                    NoteOffAsSilentNoteOn = false,
                });

                var random = DryWetMidi.Common.Random.Instance;
                var additionalBytes = new byte[random.Next(100, 1000)];
                random.NextBytes(additionalBytes);
                stream.Write(additionalBytes, 0, additionalBytes.Length);

                stream.Position = 0;

                var newMidiFile = MidiFile.Read(stream, new ReadingSettings
                {
                    StopReadingOnExpectedTrackChunksCountReached = true,
                    SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                });

                MidiAsserts.AreEqual(midiFile, newMidiFile, false, "Invalid file.");
            }
        }

        private void Read_StopReadingOnExpectedTrackChunksCountReached(ICollection<MidiFile> midiFiles)
        {
            using (var stream = new MemoryStream())
            {
                foreach (var midiFile in midiFiles)
                {
                    midiFile.Write(stream, settings: new WritingSettings
                    {
                        NoteOffAsSilentNoteOn = false,
                    });
                }

                stream.Position = 0;

                var newMidiFiles = new List<MidiFile>();

                for (var i = 0; i < midiFiles.Count; i++)
                {
                    var newMidiFile = MidiFile.Read(stream, new ReadingSettings
                    {
                        StopReadingOnExpectedTrackChunksCountReached = true,
                        SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                    });
                    newMidiFiles.Add(newMidiFile);
                }

                MidiAsserts.AreEqual(midiFiles, newMidiFiles, false, "Invalid files.");
            }
        }

        private void Read_NotEnoughBytes_Ignore_NonEndOfTrackEvent(
            MidiEvent midiEvent,
            int trimBytesCount,
            params MidiEvent[] expectedEvents) => Read_NotEnoughBytes_Ignore(
            new MidiFile(new TrackChunk(midiEvent)),
            bytes => bytes.Take(bytes.Length - 4 /* EOT */ - trimBytesCount).ToArray(),
            new MidiFile(new TrackChunk(expectedEvents ?? Array.Empty<MidiEvent>())));

        private void Read_NotEnoughBytes_Ignore(
            MidiFile midiFile,
            Func<byte[], byte[]> transformBytes,
            MidiFile expectedMidiFile) => ReadInvalidFile(
            midiFile,
            MidiFileFormat.SingleTrack,
            null,
            new ReadingSettings
            {
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore
            },
            new Dictionary<int, byte>(),
            file =>
            {
                MidiAsserts.AreEqual(expectedMidiFile, file, false, "File is invalid.");
            },
            transformBytes);

        private void Read_NotEnoughBytes_Abort_NonEndOfTrackEvent(MidiEvent midiEvent, int trimBytesCount) => Read_NotEnoughBytes_Abort(
            new MidiFile(new TrackChunk(midiEvent)),
            bytes => bytes.Take(bytes.Length - 4 /* EOT */ - trimBytesCount).ToArray());

        private void Read_NotEnoughBytes_Abort(
            MidiFile midiFile,
            Func<byte[], byte[]> transformBytes) => ReadInvalidFileWithException<NotEnoughBytesException>(
            midiFile,
            MidiFileFormat.SingleTrack,
            null,
            new ReadingSettings
            {
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
            },
            new Dictionary<int, byte>(),
            exception => { },
            transformBytes);

        private void Read_EndOfTrackStoringPolicy_Store(
            ICollection<MidiEvent> originalEvents,
            Action<EventsCollection> modifyEvents,
            ICollection<MidiEvent> expectedEventsAfterReread)
        {
            var readingSettings = new ReadingSettings
            {
                EndOfTrackStoringPolicy = EndOfTrackStoringPolicy.Store
            };

            var originalMidiFile = new MidiFile(new TrackChunk(originalEvents));
            var midiFile = MidiFileTestUtilities.Read(
                midiFile: originalMidiFile,
                writingSettings: new WritingSettings(),
                readingSettings: readingSettings);

            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(originalEvents.Concat(new[] { new EndOfTrackEvent() }))),
                midiFile,
                false,
                "Files are invalid.");

            modifyEvents?.Invoke(midiFile.GetTrackChunks().First().Events);
            midiFile = MidiFileTestUtilities.Read(midiFile, new WritingSettings(), readingSettings);

            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(expectedEventsAfterReread)),
                midiFile,
                false,
                "Files are invalid after re-read.");
        }

        private void ReadInvalidFileWithException<TException>(
            MidiFile midiFile,
            MidiFileFormat format,
            WritingSettings writingSettings,
            ReadingSettings readingSettings,
            Dictionary<int, byte> dataBytesReplacements,
            Action<TException> checkException,
            Func<byte[], byte[]> transformBytes = null)
            where TException : Exception
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;

                midiFile.Write(filePath, format: format, settings: writingSettings);

                var bytes = FileOperations.ReadAllFileBytes(filePath);
                foreach (var byteReplacement in dataBytesReplacements)
                {
                    bytes[byteReplacement.Key] = byteReplacement.Value;
                }

                if (transformBytes != null)
                    bytes = transformBytes(bytes);

                FileOperations.WriteAllBytesToFile(filePath, bytes);

                //

                var exception = ClassicAssert.Throws<TException>(() => MidiFile.Read(filePath, readingSettings), "Exception not thrown.");
                checkException(exception);

                var nonSeekableStream = new NonSeekableStream(filePath);
                ClassicAssert.Throws<TException>(() => MidiFile.Read(nonSeekableStream, readingSettings), $"Exception not thrown for the file read from non-seekable stream.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;
                ClassicAssert.Throws<TException>(() => MidiFile.Read(filePath, readingSettings), $"Exception not thrown for the file read with putting data in memory.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;
                ClassicAssert.Throws<TException>(() => MidiFile.Read(filePath, readingSettings), $"Exception not thrown for the file read with fixed size buffer.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        private void ReadInvalidFile(
            MidiFile midiFile,
            MidiFileFormat format,
            WritingSettings writingSettings,
            ReadingSettings readingSettings,
            Dictionary<int, byte> dataBytesReplacements,
            Action<MidiFile> checkFile,
            Func<byte[], byte[]> transformBytes = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;

                midiFile.Write(filePath, format: format, settings: writingSettings);

                var bytes = FileOperations.ReadAllFileBytes(filePath);
                foreach (var byteReplacement in dataBytesReplacements)
                {
                    bytes[byteReplacement.Key] = byteReplacement.Value;
                }

                if (transformBytes != null)
                    bytes = transformBytes(bytes);

                FileOperations.WriteAllBytesToFile(filePath, bytes);

                //

                var newMidiFile = MidiFile.Read(filePath, readingSettings);
                checkFile(newMidiFile);

                var newMidiFileFromNonSeekableStream = MidiFile.Read(new NonSeekableStream(filePath), readingSettings);
                MidiAsserts.AreEqual(newMidiFile, newMidiFileFromNonSeekableStream, true, "The file from non-seekable stream is invalid.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;
                var newMidiFileWithBufferAllData = MidiFile.Read(filePath, readingSettings);
                MidiAsserts.AreEqual(newMidiFile, newMidiFileWithBufferAllData, true, "The file with buffer all data is invalid.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;
                var newMidiFileWithFixedSizeBuffer = MidiFile.Read(filePath, readingSettings);
                MidiAsserts.AreEqual(newMidiFile, newMidiFileWithFixedSizeBuffer, true, "The file with fixed-size buffer is invalid.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
