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

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

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
                    Assert.AreEqual(actualCount, exception.ActualCount, "Expected count of chunks is invalid.");
                    Assert.AreEqual(countInHeaderChunk, exception.ExpectedCount, "Actual count of chunks is invalid.");
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
                midiFile => Assert.AreEqual(actualCount, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
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
                midiFile => Assert.AreEqual(actualCount, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
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
                midiFile => Assert.AreEqual(countInHeaderChunk, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
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
                exception => Assert.AreEqual(chunkId, exception.ChunkId, "Chunk ID is invalid in exception."));
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
                exception => Assert.AreEqual(chunkId, exception.ChunkId, "Chunk ID is invalid in exception."));
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
                midiFile => Assert.AreEqual(0, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
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
                midiFile => Assert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid."));
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
                    Assert.AreEqual(0, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
                    Assert.AreEqual(1, midiFile.Chunks.OfType<UnknownChunk>().Count(), "Unknown chunks count is invalid.");
                    Assert.AreEqual(chunkId, midiFile.Chunks.OfType<UnknownChunk>().First().ChunkId, "Chunk ID of unknown chunk is invalid.");
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
                    Assert.AreEqual(MidiEventType.ProgramChange, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(parameterValue, exception.Value, "Parameter value is invalid.");
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
                    Assert.IsNotNull(programChangeEvent, "There is no Program Change event in the file.");
                    Assert.AreEqual(parameterValue & ((1 << 7) - 1), (int)programChangeEvent.ProgramNumber, "Program number is invalid.");
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
                    Assert.IsNotNull(programChangeEvent, "There is no Program Change event in the file.");
                    Assert.AreEqual(Math.Min((int)parameterValue, 127), (int)programChangeEvent.ProgramNumber, "Program number is invalid.");
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
                    Assert.AreEqual(statusByte, (byte)exception.StatusByte, "Status byte is invalid.");
                    Assert.AreEqual(channel, (byte)exception.Channel, "Channel is invalid.");
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
                    Assert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEventsEqual(new ControlChangeEvent { DeltaTime = (((statusByte << 4) + channel) & ((1 << 7) - 1)) << 7 }, controlChangeEvent, true, "Event is invalid.");
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
                    Assert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEventsEqual(new ControlChangeEvent(), controlChangeEvent, true, "Event is invalid.");
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
                    Assert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEventsEqual(new ControlChangeEvent(), controlChangeEvent, true, "Event is invalid.");
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
                    Assert.AreEqual(statusByte, (byte)exception.StatusByte, "Status byte is invalid.");
                    Assert.AreEqual(channel, (byte)exception.Channel, "Channel is invalid.");
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
                    Assert.IsNotNull(controlChangeEvent, "There is no Control Change event in the file.");
                    MidiAsserts.AreEventsEqual(new ControlChangeEvent { DeltaTime = (((statusByte << 4) + channel) & ((1 << 7) - 1)) << 7 }, controlChangeEvent, true, "Event is invalid.");
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
                    Assert.AreEqual(6, exception.ActualSize, "Actual size is invalid.");
                    Assert.AreEqual(HeaderChunk.Id, exception.ChunkId, "Chunk ID is invalid.");
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
                    Assert.AreEqual(7, exception.ActualSize, "Actual size is invalid.");
                    Assert.AreEqual(TrackChunk.Id, exception.ChunkId, "Chunk ID is invalid.");
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
                    Assert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
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
                    Assert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
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
                    Assert.AreEqual(MidiEventType.KeySignature, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(KeySignatureEvent.Key), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(key, exception.Value, "Property value is invalid.");
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
                    Assert.IsNotNull(keySignatureEvent, "There is no Key Signature event.");
                    Assert.AreEqual(Math.Max(key, (sbyte)-7), keySignatureEvent.Key, "Key is invalid");
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
                    Assert.IsNotNull(keySignatureEvent, "There is no Key Signature event.");
                    Assert.AreEqual(Math.Min(key, (sbyte)7), keySignatureEvent.Key, "Key is invalid");
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
                    Assert.AreEqual(MidiEventType.KeySignature, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(KeySignatureEvent.Scale), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(scale, exception.Value, "Property value is invalid.");
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
                    Assert.IsNotNull(keySignatureEvent, "There is no Key Signature event.");
                    Assert.AreEqual(Math.Min(scale, (byte)1), keySignatureEvent.Scale, "Scale is invalid");
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
                    Assert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(SmpteOffsetEvent.Hours), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(hours, exception.Value, "Property value is invalid.");
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
                    Assert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(SmpteOffsetEvent.Minutes), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(minutes, exception.Value, "Property value is invalid.");
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
                    Assert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(SmpteOffsetEvent.Seconds), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(seconds, exception.Value, "Property value is invalid.");
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
                    Assert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(SmpteOffsetEvent.Frames), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(frames, exception.Value, "Property value is invalid.");
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
                    Assert.AreEqual(MidiEventType.SmpteOffset, exception.EventType, "Event type is invalid.");
                    Assert.AreEqual(nameof(SmpteOffsetEvent.SubFrames), exception.PropertyName, "Property name is invalid.");
                    Assert.AreEqual(subFrames, exception.Value, "Property value is invalid.");
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
                    Assert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    Assert.AreEqual(Math.Min((byte)23, hours), smpteOffsetEvent.Hours, "Hours number is invalid.");
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
                    Assert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    Assert.AreEqual(Math.Min((byte)59, minutes), smpteOffsetEvent.Minutes, "Minutes number is invalid.");
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
                    Assert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    Assert.AreEqual(Math.Min((byte)59, seconds), smpteOffsetEvent.Seconds, "Seconds number is invalid.");
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
                    Assert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    Assert.AreEqual(Math.Min(maxValue, frames), smpteOffsetEvent.Frames, "Frames number is invalid.");
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
                    Assert.IsNotNull(smpteOffsetEvent, "There is no SMPTE Offset event.");
                    Assert.AreEqual(Math.Min((byte)99, subFrames), smpteOffsetEvent.SubFrames, "Sub-frames number is invalid.");
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
                    Assert.AreEqual(2, midiEvents.Length, "Events count is invalid.");
                    MidiAsserts.AreEventsEqual(new ProgramChangeEvent(), midiEvents[0], true, "First MIDI event is invalid.");
                    MidiAsserts.AreEventsEqual(new NoteOnEvent(), midiEvents[1], true, "Second MIDI event is invalid.");
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
                    Assert.Throws<InvalidOperationException>(() => { var format = midiFile.OriginalFormat; }, "Exception not thrown on get original format.");
                    
                    var programChangeEvent = midiFile.GetEvents().SingleOrDefault() as ProgramChangeEvent;
                    MidiAsserts.AreEventsEqual(new ProgramChangeEvent(), programChangeEvent, true, "Program Change event is invalid.");
                },
                bytes => bytes.Skip(14).ToArray());
        }

        [Test]
        public void Read_NotEnoughBytes_Abort()
        {
            // TODO: test all cases
            ReadInvalidFileWithException<NotEnoughBytesException>(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                },
                new Dictionary<int, byte>(),
                exception => { },
                bytes => bytes.Take(bytes.Length - 1).ToArray());
        }

        [Test]
        public void Read_NotEnoughBytes_Ignore()
        {
            // TODO: test all cases
            ReadInvalidFile(
                new MidiFile(new TrackChunk(new ProgramChangeEvent())),
                MidiFileFormat.SingleTrack,
                null,
                new ReadingSettings
                {
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                },
                new Dictionary<int, byte>(),
                midiFile =>
                {
                    var programChangeEvent = midiFile.GetEvents().SingleOrDefault() as ProgramChangeEvent;
                    MidiAsserts.AreEventsEqual(new ProgramChangeEvent(), programChangeEvent, true, "Program Change event is invalid.");
                },
                bytes => bytes.Take(bytes.Length - 1).ToArray());
        }

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
                exception => Assert.AreEqual(formatLastByte, exception.FileFormat, "File format is invalid."));
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
                    var exception = Assert.Throws<UnknownFileFormatException>(() => { var format = midiFile.OriginalFormat; }, "Exception not thrown on get original format.");
                    Assert.AreEqual(formatLastByte, exception.FileFormat, "File format is invalid.");
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
        public void Read_EndOfTrackStoringPolicy_Store()
        {
            var originalMidiFile = new MidiFile(new TrackChunk(new TextEvent("A")));
            var midiFile = MidiFileTestUtilities.Read(
                midiFile: originalMidiFile,
                writingSettings: new WritingSettings(),
                readingSettings: new ReadingSettings
                {
                    EndOfTrackStoringPolicy = EndOfTrackStoringPolicy.Store
                });

            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(new TextEvent("A"), new EndOfTrackEvent())),
                midiFile,
                false,
                "Files are invalid.");
        }

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
                    Assert.DoesNotThrow(() => { var l = streamToRead.Length; });
                }
            }
        }

        [Test]
        public void Read_DecodeTextCallback_NoCallback()
        {
            const string text = "Just text";

            var midiFile = WriteRead(new MidiFile(new TrackChunk(new TextEvent(text))));
            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();

            Assert.AreEqual(text, textEvent.Text, "Text decoded incorrectly.");
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

            Assert.AreEqual(newText, textEvent.Text, "Text decoded incorrectly.");
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

            Assert.AreNotEqual(text, textEvent.Text, "Text decoded incorrectly.");
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

            Assert.AreEqual(text, textEvent.Text, "Text decoded incorrectly.");
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
            Assert.IsTrue(buffer.All(b => b == 0), "Initial buffer contains non-zero bytes.");

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
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

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

                var exception = Assert.Throws<TException>(() => MidiFile.Read(filePath, readingSettings), "Exception not thrown.");
                checkException(exception);

                var nonSeekableStream = new NonSeekableStream(filePath);
                Assert.Throws<TException>(() => MidiFile.Read(nonSeekableStream, readingSettings), $"Exception not thrown for the file read from non-seekable stream.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;
                Assert.Throws<TException>(() => MidiFile.Read(filePath, readingSettings), $"Exception not thrown for the file read with putting data in memory.");

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;
                Assert.Throws<TException>(() => MidiFile.Read(filePath, readingSettings), $"Exception not thrown for the file read with fixed size buffer.");

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
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

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
