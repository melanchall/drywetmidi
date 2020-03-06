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
    public sealed class MidiFileTests
    {
        #region Nested classes

        private sealed class NonSeekableStream : Stream
        {
            #region Fields

            private readonly MemoryStream _memoryStream;

            #endregion

            #region Constructor

            public NonSeekableStream(string filePath)
            {
                _memoryStream = new MemoryStream(File.ReadAllBytes(filePath));
                _memoryStream.Position = 0;
            }

            #endregion

            #region Overrides

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _memoryStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        private abstract class BaseReadingHandler : ReadingHandler
        {
            #region Constructor

            public BaseReadingHandler(TargetScope scope)
                : base(scope)
            {
            }

            #endregion

            #region Properties

            public int BadHandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
            {
                BadHandledCount++;
            }

            public override void OnFinishFileReading(MidiFile midiFile)
            {
                BadHandledCount++;
            }

            public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
            {
                BadHandledCount++;
            }

            public override void OnFinishTrackChunkReading(TrackChunk trackChunk)
            {
                BadHandledCount++;
            }

            public override void OnStartFileReading()
            {
                BadHandledCount++;
            }

            public override void OnStartTrackChunkContentReading(TrackChunk trackChunk)
            {
                BadHandledCount++;
            }

            public override void OnStartTrackChunkReading()
            {
                BadHandledCount++;
            }

            #endregion
        }

        private sealed class FileReadingHandler : BaseReadingHandler
        {
            #region Constructor

            public FileReadingHandler()
                : base(TargetScope.File)
            {
            }

            #endregion

            #region Properties

            public int StartHandledCount { get; private set; }

            public int EndHandledCount { get; private set; }

            public TimeDivision TimeDivision { get; private set; }

            #endregion

            #region Overrides

            public override void OnStartFileReading()
            {
                StartHandledCount++;
            }

            public override void OnFinishFileReading(MidiFile midiFile)
            {
                EndHandledCount++;
            }

            public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
            {
                TimeDivision = timeDivision;
            }

            #endregion
        }

        private sealed class TrackChunkReadingHandler : BaseReadingHandler
        {
            #region Constructor

            public TrackChunkReadingHandler()
                : base(TargetScope.TrackChunk)
            {
            }

            #endregion

            #region Properties

            public int StartHandledCount { get; private set; }

            public int ContentStartHandledCount { get; private set; }

            public int EndHandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnStartTrackChunkReading()
            {
                StartHandledCount++;
            }

            public override void OnStartTrackChunkContentReading(TrackChunk trackChunk)
            {
                ContentStartHandledCount++;
            }

            public override void OnFinishTrackChunkReading(TrackChunk trackChunk)
            {
                EndHandledCount++;
            }

            #endregion
        }

        private sealed class EventReadingHandler : BaseReadingHandler
        {
            #region Constructor

            public EventReadingHandler()
                : base(TargetScope.Event)
            {
            }

            #endregion

            #region Properties

            public int HandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
            {
                HandledCount++;
            }

            #endregion
        }

        private sealed class MixedReadingHandler : ReadingHandler
        {
            #region Constructor

            public MixedReadingHandler()
                : base(TargetScope.File | TargetScope.Event | TargetScope.TrackChunk)
            {
            }

            #endregion

            #region Properties

            public int FileStartHandledCount { get; private set; }

            public int FileEndHandledCount { get; private set; }

            public TimeDivision FileTimeDivision { get; private set; }

            public int TrackChunkStartHandledCount { get; private set; }

            public int TrackChunkContentStartHandledCount { get; private set; }

            public int TrackChunkEndHandledCount { get; private set; }

            public int EventHandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
            {
                EventHandledCount++;
            }

            public override void OnFinishFileReading(MidiFile midiFile)
            {
                FileEndHandledCount++;
            }

            public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
            {
                FileTimeDivision = timeDivision;
            }

            public override void OnFinishTrackChunkReading(TrackChunk trackChunk)
            {
                TrackChunkEndHandledCount++;
            }

            public override void OnStartFileReading()
            {
                FileStartHandledCount++;
            }

            public override void OnStartTrackChunkContentReading(TrackChunk trackChunk)
            {
                TrackChunkContentStartHandledCount++;
            }

            public override void OnStartTrackChunkReading()
            {
                TrackChunkStartHandledCount++;
            }

            #endregion
        }

        private sealed class CustomMetaEvent : MetaEvent
        {
            #region Constructor

            public CustomMetaEvent()
                : base()
            {
            }

            public CustomMetaEvent(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            #endregion

            #region Overrides

            protected override MidiEvent CloneEvent()
            {
                return new CustomMetaEvent(A, B, C);
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

                var bLength = reader.ReadVlqNumber();
                B = reader.ReadString(bLength);

                C = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            #endregion
        }

        private sealed class CustomChunk : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public CustomChunk()
                : base(Id)
            {
            }

            public CustomChunk(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            #endregion

            #region Overrides

            public override MidiChunk Clone()
            {
                return new CustomChunk(A, B, C);
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                return (uint)(DataTypesUtilities.GetVlqLength(A) +
                              DataTypesUtilities.GetVlqLength(B?.Length ?? 0) +
                              (B?.Length ?? 0) +
                              1);
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                A = reader.ReadVlqNumber();

                var bLength = reader.ReadVlqNumber();
                B = reader.ReadString(bLength);

                C = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            #endregion
        }

        private sealed class CustomChunkWithInvalidId : MidiChunk
        {
            #region Constants

            public const string Id = "MTrk";

            #endregion

            #region Constructor

            public CustomChunkWithInvalidId()
                : base(Id)
            {
            }

            #endregion

            #region Overrides

            public override MidiChunk Clone()
            {
                return new CustomChunkWithInvalidId();
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                return 0;
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
            }

            #endregion
        }

        #endregion

        #region Constants

        private static class DirectoriesNames
        {
            public const string InvalidChannelEventParameterValue = "Invalid Channel Event Parameter Value";
            public const string InvalidChannelEvent = "Invalid Channel Event";
            public const string InvalidChunkSize = "Invalid Chunk Size";
            public const string InvalidKeySignatureKey = "Invalid Key Signature Key";
            public const string InvalidKeySignatureScale = "Invalid Key Signature Scale";
            public const string InvalidMetaEventParameterValue = "Invalid Meta Event Parameter Value";
            public const string InvalidSmpteFrames = "Invalid SMPTE Frames";
            public const string NoEndOfTrack = "No End of Track";
            public const string NoHeaderChunk = "No Header Chunk";
            public const string NotEnoughBytes = "Not Enough Bytes";
            public const string UnexpectedRunningStatus = "Unexpected Running Status";
            public const string UnknownChannelEvent = "Unknown Channel Event";
            public const string UnknownFileFormat = "Unknown File Format";
            public const string UnexpectedTrackChunksCount = "Unexpected Track Chunks Count";
            public const string ExtraTrackChunk = "Extra Track Chunk";
            public const string UnknownChunkId = "Unknown Chunk Id";
        }

        #endregion

        #region Properties

        public TestContext TestContext { get; set; }

        #endregion

        #region Set up

        [SetUp]
        public void SetupTest()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        #endregion

        #region Test methods

        [Test]
        public void Read_UnexpectedTrackChunksCount_Abort()
        {
            ReadFilesWithException<UnexpectedTrackChunksCountException>(
                DirectoriesNames.UnexpectedTrackChunksCount,
                new ReadingSettings
                {
                    UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Abort
                });
        }

        [Test]
        public void Read_UnexpectedTrackChunksCount_Ignore()
        {
            ReadInvalidFiles(
                DirectoriesNames.UnexpectedTrackChunksCount,
                new ReadingSettings
                {
                    UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore
                });
        }

        [Test]
        public void Read_ExtraTrackChunk_Read()
        {
            foreach (var filePath in GetInvalidFiles(DirectoriesNames.ExtraTrackChunk))
            {
                var midiFile = MidiFile.Read(filePath, new ReadingSettings
                {
                    ExtraTrackChunkPolicy = ExtraTrackChunkPolicy.Read
                });

                Assert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
            }
        }

        [Test]
        public void Read_ExtraTrackChunk_Skip()
        {
            foreach (var filePath in GetInvalidFiles(DirectoriesNames.ExtraTrackChunk))
            {
                var midiFile = MidiFile.Read(filePath, new ReadingSettings
                {
                    ExtraTrackChunkPolicy = ExtraTrackChunkPolicy.Skip
                });

                CollectionAssert.IsEmpty(midiFile.GetTrackChunks(), "Track chunks count is invalid.");
            }
        }

        [Test]
        public void Read_UnknownChunkId_Abort()
        {
            ReadFilesWithException<UnknownChunkException>(
                DirectoriesNames.UnknownChunkId,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Abort
                });
        }

        [Test]
        public void Read_UnknownChunkId_Skip()
        {
            ReadInvalidFiles(
                DirectoriesNames.UnknownChunkId,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip
                });
        }

        [Test]
        public void Read_UnknownChunkId_ReadAsUnknown()
        {
            ReadInvalidFiles(
                DirectoriesNames.UnknownChunkId,
                new ReadingSettings
                {
                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk
                });
        }

        [Test]
        [Description("Read MIDI file with invalid channel event parameter value and treat that as error.")]
        public void Read_InvalidChannelEventParameterValue_Abort()
        {
            ReadFilesWithException<InvalidChannelEventParameterValueException>(
                DirectoriesNames.InvalidChannelEventParameterValue,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with invalid channel event parameter value and read such values taking lower 7 bits.")]
        public void Read_InvalidChannelEventParameterValue_ReadValid()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChannelEventParameterValue,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                });
        }

        [Test]
        [Description("Read MIDI file with invalid channel event parameter value and snap such values to valid limits (0-127).")]
        public void Read_InvalidChannelEventParameterValue_SnapToLimits()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChannelEventParameterValue,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.SnapToLimits
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_Abort()
        {
            ReadFilesWithException<UnknownChannelEventException>(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.Abort
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_SkipStatusByte()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByte,
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_SkipStatusByteAndOneDataByte()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte,
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_SkipStatusByteAndTwoDataBytes()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndTwoDataBytes,
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_UseCallback_NoCallback()
        {
            ReadFilesWithException<InvalidOperationException>(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.UseCallback
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_UseCallback_Abort()
        {
            ReadFilesWithException<UnknownChannelEventException>(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.UseCallback,
                    UnknownChannelEventCallback = (statusByte, channel) => UnknownChannelEventAction.Abort
                });
        }

        [Test]
        public void Read_InvalidChannelEvent_UseCallback_SkipData()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChannelEvent,
                new ReadingSettings
                {
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.UseCallback,
                    UnknownChannelEventCallback = (statusByte, channel) => UnknownChannelEventAction.SkipData(0),
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                });
        }

        [Test]
        [Description("Read MIDI file with invalid size of a chunk and treat that as error.")]
        public void Read_InvalidChunkSize_Abort()
        {
            ReadFilesWithException<InvalidChunkSizeException>(
                DirectoriesNames.InvalidChunkSize,
                new ReadingSettings
                {
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with invalid size of a chunk and ignore that.")]
        public void Read_InvalidChunkSize_Ignore()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidChunkSize,
                new ReadingSettings
                {
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                });
        }

        [Test]
        [Description("Read MIDI file with invalid key of a Key Signature event and treat that as error.")]
        public void Read_InvalidKeySignatureKey_Abort()
        {
            ReadFilesWithException<InvalidMetaEventParameterValueException>(
                DirectoriesNames.InvalidKeySignatureKey,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with invalid key of a Key Signature event and snap the value to valid limits.")]
        public void Read_InvalidKeySignatureKey_SnapToLimits()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidKeySignatureKey,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                });
        }

        [Test]
        [Description("Read MIDI file with invalid scale of a Key Signature event and treat that as error.")]
        public void Read_InvalidKeySignatureScale_Abort()
        {
            ReadFilesWithException<InvalidMetaEventParameterValueException>(
                DirectoriesNames.InvalidKeySignatureScale,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with invalid scale of a Key Signature event and snap the value to valid limits.")]
        public void Read_InvalidKeySignatureScale_SnapToLimits()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidKeySignatureScale,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                });
        }

        [Test]
        [Description("Read MIDI file with invalid parameter of a meta event and treat that as error.")]
        public void Read_InvalidMetaEventParameterValue_Abort()
        {
            ReadFilesWithException<InvalidMetaEventParameterValueException>(
                DirectoriesNames.InvalidMetaEventParameterValue,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with invalid parameter of a meta event and snap the value to valid limits.")]
        public void Read_InvalidMetaEventParameterValue_SnapToLimits()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidMetaEventParameterValue,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                });
        }

        [Test]
        [Description("Read MIDI file with invalid SMPTE frames number and treat that as error.")]
        public void Read_InvalidSmpteFrames_Abort()
        {
            ReadFilesWithException<InvalidMetaEventParameterValueException>(
                DirectoriesNames.InvalidSmpteFrames,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with invalid SMPTE frames number and snap the value to valid limits.")]
        public void Read_InvalidSmpteFrames_SnapToLimits()
        {
            ReadInvalidFiles(
                DirectoriesNames.InvalidSmpteFrames,
                new ReadingSettings
                {
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
                });
        }

        [Test]
        [Description("Read MIDI file without End Of Track event and treat that as error.")]
        public void Read_NoEndOfTrack_Abort()
        {
            ReadFilesWithException<MissedEndOfTrackEventException>(
                DirectoriesNames.NoEndOfTrack,
                new ReadingSettings
                {
                    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file without End Of Track event and ignore that.")]
        public void Read_NoEndOfTrack_Ignore()
        {
            ReadInvalidFiles(
                DirectoriesNames.NoEndOfTrack,
                new ReadingSettings
                {
                    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore
                });
        }

        [Test]
        [Description("Read MIDI file without header chunk and treat that as error.")]
        public void Read_NoHeaderChunk_Abort()
        {
            ReadFilesWithException<NoHeaderChunkException>(
                DirectoriesNames.NoHeaderChunk,
                new ReadingSettings
                {
                    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Abort,
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                });
        }

        [Test]
        [Description("Read MIDI file without header chunk and ignore that.")]
        public void Read_NoHeaderChunk_Ignore()
        {
            ReadInvalidFiles(
                DirectoriesNames.NoHeaderChunk,
                new ReadingSettings
                {
                    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore,
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                });
        }

        [Test]
        [Description("Read MIDI file in case of not enough bytes to read an object and treat that as error.")]
        public void Read_NotEnoughBytes_Abort()
        {
            ReadFilesWithException<NotEnoughBytesException>(
                DirectoriesNames.NotEnoughBytes,
                new ReadingSettings
                {
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file in case of not enough bytes to read an object and ignore that.")]
        public void Read_NotEnoughBytes_Ignore()
        {
            ReadInvalidFiles(
                DirectoriesNames.NotEnoughBytes,
                new ReadingSettings
                {
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore
                });
        }

        [Test]
        [Description("Read MIDI file with unexpected running status.")]
        public void Read_UnexpectedRunningStatus()
        {
            ReadFilesWithException<UnexpectedRunningStatusException>(
                DirectoriesNames.UnexpectedRunningStatus,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                });
        }

        [Test]
        [Description("Read MIDI file with unknow channel event.")]
        public void Read_UnknownChannelEvent()
        {
            ReadFilesWithException<UnknownChannelEventException>(
                DirectoriesNames.UnknownChannelEvent,
                new ReadingSettings
                {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                });
        }

        [Test]
        [Description("Read MIDI file with unknown format and treat that as error.")]
        public void Read_UnknownFileFormat_Abort()
        {
            ReadFilesWithException<UnknownFileFormatException>(
                DirectoriesNames.UnknownFileFormat,
                new ReadingSettings
                {
                    UnknownFileFormatPolicy = UnknownFileFormatPolicy.Abort
                });
        }

        [Test]
        [Description("Read MIDI file with unknown format and ignore that.")]
        public void Read_UnknownFileFormat_Ignore()
        {
            ReadInvalidFiles(
                DirectoriesNames.UnknownFileFormat,
                new ReadingSettings
                {
                    UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore
                });
        }

        [Test]
        [Description("Check whether a clone of a MIDI file equals to the original file.")]
        public void Clone_Read()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);
                var clonedMidiFile = midiFile.Clone();

                MidiAsserts.AreFilesEqual(clonedMidiFile, midiFile, true, $"Clone of the '{filePath}' doesn't equal to the original file.");
            }
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
        public void Write_StreamIsNotDisposed()
        {
            var midiFile = new MidiFile();

            using (var streamToWrite = new MemoryStream())
            {
                midiFile.Write(streamToWrite);
                Assert.DoesNotThrow(() => { var l = streamToWrite.Length; });
            }
        }

        [Test]
        public void Read_NonSeekableStream()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);

                var nonSeekableStream = new NonSeekableStream(filePath);
                var midiFile2 = MidiFile.Read(nonSeekableStream);

                MidiAsserts.AreFilesEqual(midiFile, midiFile2, true, $"File '{filePath}' is invalid.");
            }
        }

        [TestCase(1024)]
        [TestCase(4096)]
        [TestCase(4)]
        [TestCase(500000)]
        public void Read_NonSeekableStream_BufferSize(int bufferSize)
        {
            Read_NonSeekableStream(new ReaderSettings
            {
                NonSeekableStreamBufferSize = bufferSize
            });
        }

        [TestCase(1)]
        [TestCase(128)]
        [TestCase(500000)]
        public void Read_NonSeekableStream_IncrementalBytesReadingThreshold(int threshold)
        {
            Read_NonSeekableStream(new ReaderSettings
            {
                NonSeekableStreamIncrementalBytesReadingThreshold = threshold
            });
        }

        [TestCase(1)]
        [TestCase(8)]
        [TestCase(100000)]
        public void Read_NonSeekableStream_IncrementalBytesReadingStep(int step)
        {
            Read_NonSeekableStream(new ReaderSettings
            {
                NonSeekableStreamIncrementalBytesReadingThreshold = 1,
                NonSeekableStreamIncrementalBytesReadingStep = step
            });
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
        public void Read_Handler_File()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(1000);
            var handler = new FileReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile { TimeDivision = timeDivision }, handler);
            Assert.AreEqual(1, handler.StartHandledCount, "Start Handled Count is invalid.");
            Assert.AreEqual(1, handler.EndHandledCount, "End Handled Count is invalid.");
            Assert.AreEqual(timeDivision, handler.TimeDivision, "Time division is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_TrackChunk()
        {
            var handler = new TrackChunkReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(new TrackChunk(), new TrackChunk()), handler);
            Assert.AreEqual(2, handler.StartHandledCount, "Start Handled Count is invalid.");
            Assert.AreEqual(2, handler.ContentStartHandledCount, "Content Start Handled Count is invalid.");
            Assert.AreEqual(2, handler.EndHandledCount, "End Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Event_EmptyFile()
        {
            var handler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(), handler);
            Assert.AreEqual(0, handler.HandledCount, "Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Event_EmptyTrackChunk()
        {
            var handler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(new TrackChunk()), handler);
            Assert.AreEqual(0, handler.HandledCount, "Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Event()
        {
            var handler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(new TrackChunk(new TextEvent("test"), new TextEvent("test 2")), new TrackChunk(), new TrackChunk(new SetTempoEvent(100000))), handler);
            Assert.AreEqual(3, handler.HandledCount, "Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_AllHandlers()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(1000);

            var fileReadingHandler = new FileReadingHandler();
            var trackChunkReadingHandler = new TrackChunkReadingHandler();
            var eventReadingHandler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(
                new MidiFile(
                    new TrackChunk(new TextEvent("test"), new TextEvent("test 2")),
                    new TrackChunk(),
                    new TrackChunk(new SetTempoEvent(100000)),
                    new TrackChunk()) { TimeDivision = timeDivision },
                fileReadingHandler,
                trackChunkReadingHandler,
                eventReadingHandler);

            Assert.AreEqual(1, fileReadingHandler.StartHandledCount, "File: Start Handled Count is invalid.");
            Assert.AreEqual(1, fileReadingHandler.EndHandledCount, "File: End Handled Count is invalid.");
            Assert.AreEqual(timeDivision, fileReadingHandler.TimeDivision, "File: Time division is invalid.");
            Assert.AreEqual(0, fileReadingHandler.BadHandledCount, "File: Scope wasn't used correctly.");

            Assert.AreEqual(4, trackChunkReadingHandler.StartHandledCount, "Track chunk: Start Handled Count is invalid.");
            Assert.AreEqual(4, trackChunkReadingHandler.ContentStartHandledCount, "Track chunk: Content Start Handled Count is invalid.");
            Assert.AreEqual(4, trackChunkReadingHandler.EndHandledCount, "Track chunk: End Handled Count is invalid.");
            Assert.AreEqual(0, trackChunkReadingHandler.BadHandledCount, "Track chunk: Scope wasn't used correctly.");

            Assert.AreEqual(3, eventReadingHandler.HandledCount, "Event: Handled Count is invalid.");
            Assert.AreEqual(0, eventReadingHandler.BadHandledCount, "Event: Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Mixed()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(1000);

            var handler = new MixedReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(
                new MidiFile(
                    new TrackChunk(new TextEvent("test"), new TextEvent("test 2")),
                    new TrackChunk(),
                    new TrackChunk(new SetTempoEvent(100000)),
                    new TrackChunk())
                { TimeDivision = timeDivision },
                handler);

            Assert.AreEqual(1, handler.FileStartHandledCount, "File: Start Handled Count is invalid.");
            Assert.AreEqual(1, handler.FileEndHandledCount, "File: End Handled Count is invalid.");
            Assert.AreEqual(timeDivision, handler.FileTimeDivision, "File: Time division is invalid.");

            Assert.AreEqual(4, handler.TrackChunkStartHandledCount, "Track chunk: Start Handled Count is invalid.");
            Assert.AreEqual(4, handler.TrackChunkContentStartHandledCount, "Track chunk: Content Start Handled Count is invalid.");
            Assert.AreEqual(4, handler.TrackChunkEndHandledCount, "Track chunk: End Handled Count is invalid.");

            Assert.AreEqual(3, handler.EventHandledCount, "Event: Handled Count is invalid.");
        }

        [Test]
        public void ReadWriteCustomMetaEvent()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customMetaEventTypes = new EventTypesCollection
            {
                { typeof(CustomMetaEvent), 0x5A }
            };

            var writingSettings = new WritingSettings { CustomMetaEventTypes = customMetaEventTypes };
            var readingSettings = new ReadingSettings { CustomMetaEventTypes = customMetaEventTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new CustomMetaEvent(expectedA, expectedB, expectedC) { DeltaTime = 100 },
                        new TextEvent("foo"),
                        new MarkerEvent("bar"))),
                writingSettings,
                readingSettings);

            var customMetaEvents = midiFile.GetEvents().OfType<CustomMetaEvent>().ToArray();
            Assert.AreEqual(1, customMetaEvents.Length, "Custom meta events count is invalid.");

            var customMetaEvent = customMetaEvents.First();
            Assert.AreEqual(100, customMetaEvent.DeltaTime, "Delta-time is invalid.");
            Assert.AreEqual(expectedA, customMetaEvent.A, "A value is invalid");
            Assert.AreEqual(expectedB, customMetaEvent.B, "B value is invalid");
            Assert.AreEqual(expectedC, customMetaEvent.C, "C value is invalid");
        }

        [Test]
        public void WriteCustomMetaEvent_InvalidStatusBytes()
        {
            var customMetaEventTypes = new EventTypesCollection
            {
                { typeof(CustomMetaEvent), 0x54 }
            };

            var filePath = Path.GetRandomFileName();

            try
            {
                var exception = Assert.Throws<InvalidOperationException>(() =>
                    new MidiFile(
                        new TrackChunk(
                            new CustomMetaEvent(1234567, "Test", 45) { DeltaTime = 100 },
                            new TextEvent("foo"),
                            new MarkerEvent("bar")))
                    .Write(filePath, settings: new WritingSettings { CustomMetaEventTypes = customMetaEventTypes }));

                var error = exception.Message;
                StringAssert.Contains(0x54.ToString(), error, "Exception message doesn't contain invalid status byte.");
                StringAssert.Contains(typeof(SmpteOffsetEvent).Name, error, "Exception message doesn't contain standard event's type name.");
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        // TODO: validation before reading eats time
        // [Test]
        public void ReadCustomMetaEvent_InvalidStatusBytes()
        {
            var customMetaEventTypes = new EventTypesCollection
            {
                { typeof(CustomMetaEvent), 0x54 }
            };

            var exception = Assert.Throws<InvalidOperationException>(() =>
                MidiFile.Read(TestFilesProvider.GetMiscFile_14000events(), new ReadingSettings { CustomMetaEventTypes = customMetaEventTypes }));

            var error = exception.Message;
            StringAssert.Contains(0x54.ToString(), error, "Exception message doesn't contain invalid status byte.");
            StringAssert.Contains(typeof(SmpteOffsetEvent).Name, error, "Exception message doesn't contain standard event's type name.");
        }

        [Test]
        public void ReadWriteCustomChunk()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customChunkTypes = new ChunkTypesCollection
            {
                { typeof(CustomChunk), CustomChunk.Id }
            };

            var readingSettings = new ReadingSettings { CustomChunkTypes = customChunkTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("foo"),
                        new MarkerEvent("bar")),
                    new CustomChunk(expectedA, expectedB, expectedC)),
                null,
                readingSettings);

            var customChunks = midiFile.Chunks.OfType<CustomChunk>().ToArray();
            Assert.AreEqual(1, customChunks.Length, "Custom chunks count is invalid.");

            var customChunk = customChunks.First();
            Assert.AreEqual(expectedA, customChunk.A, "A value is invalid");
            Assert.AreEqual(expectedB, customChunk.B, "B value is invalid");
            Assert.AreEqual(expectedC, customChunk.C, "C value is invalid");
        }

        [Test]
        public void WriteCustomChunk_InvalidId()
        {
            var filePath = Path.GetRandomFileName();

            try
            {
                var exception = Assert.Throws<InvalidOperationException>(() =>
                    new MidiFile(
                        new TrackChunk(
                            new TextEvent("foo"),
                            new MarkerEvent("bar")),
                        new CustomChunkWithInvalidId())
                    .Write(filePath));

                var error = exception.Message;
                StringAssert.Contains(CustomChunkWithInvalidId.Id.ToString(), error, "Exception message doesn't contain invalid ID.");
                StringAssert.Contains(typeof(TrackChunk).Name, error, "Exception message doesn't contain standard chunk's type name.");
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void ReadWriteRead()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                MidiFile midiFile = null;
                MidiFile midiFile2 = null;

                Assert.DoesNotThrow(() =>
                    {
                        midiFile = MidiFile.Read(filePath);
                        midiFile2 = MidiFileTestUtilities.Read(midiFile, null, null);
                    },
                    $"Read/Write/Read failed for '{filePath}'.");

                Assert.IsNotNull(midiFile, "MIDI file is null.");
                MidiAsserts.AreFilesEqual(midiFile, midiFile2, true, $"Reread failed for '{filePath}'.");
            }
        }

        [Test]
        public void Read_PutDataInMemoryBeforeReading()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var expectedMidiFile = MidiFile.Read(filePath);
                var settings = new ReadingSettings();
                settings.ReaderSettings.PutDataInMemoryBeforeReading = true;
                var midiFile = MidiFile.Read(filePath, settings);
                MidiAsserts.AreFilesEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");
            }
        }

        [Test]
        public void CheckValidFilesReadingByReferences()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var referenceMidiFile = TestFilesProvider.GetValidFileReference(filePath, out var noFile);
                if (noFile)
                    continue;

                var midiFile = MidiFile.Read(filePath);
                MidiAsserts.AreFilesEqual(midiFile, referenceMidiFile, false, $"File '{filePath}' read wrong.");
            }
        }

        [Test]
        public void CheckValidFilesAreEqualToSelf()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile1 = MidiFile.Read(filePath);
                var midiFile2 = MidiFile.Read(filePath);
                MidiAsserts.AreFilesEqual(midiFile1, midiFile2, true, $"File '{filePath}' isn't equal to self.");
            }
        }

        [Test]
        public void CheckValidFilesAreNotEqualToAnother()
        {
            var filesPaths = TestFilesProvider.GetValidFilesPaths().ToArray();
            var random = new Random();

            foreach (var filePath in filesPaths)
            {
                var midiFile1 = MidiFile.Read(filePath);
                var midiFile2 = MidiFile.Read(filesPaths.Where(p => p != filePath).ToArray()[random.Next(filesPaths.Length - 1)]);
                MidiAsserts.AreFilesNotEqual(midiFile1, midiFile2, true, $"File '{filePath}' equals to another one.");
            }
        }

        [Test]
        public void Write_Compression_NoCompression()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write_Compression(
                midiFile,
                CompressionPolicy.NoCompression,
                (fileInfo1, fileInfo2) => Assert.AreEqual(fileInfo1.Length, fileInfo2.Length, "File size is invalid."));
        }

        [Test]
        public void Write_Compression_NoteOffAsSilentNoteOn()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write_Compression(
                midiFile,
                CompressionPolicy.NoteOffAsSilentNoteOn,
                (fileInfo1, fileInfo2) =>
                {
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName, new ReadingSettings { SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn });
                    CollectionAssert.IsEmpty(newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<NoteOffEvent>(), "There are Note Off events.");
                });
        }

        [Test]
        public void Write_Compression_UseRunningStatus()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)51),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)51)));

            Write_Compression(
                midiFile,
                CompressionPolicy.UseRunningStatus,
                (fileInfo1, fileInfo2) => Assert.Less(fileInfo2.Length, fileInfo1.Length, "File size is invalid."));
        }

        [Test]
        public void Write_Compression_DeleteUnknownMetaEvents()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254)));

            Write_Compression(
                midiFile,
                CompressionPolicy.DeleteUnknownMetaEvents,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    CollectionAssert.IsNotEmpty(
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<UnknownMetaEvent>(),
                        "There are no Unknown Meta events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    CollectionAssert.IsEmpty(
                        newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<UnknownMetaEvent>(),
                        "There are Unknown Meta events in new file.");
                });
        }

        [Test]
        public void Write_Compression_DeleteDefaultKeySignature()
        {
            var nonDefaultKeySignatureEvent = new KeySignatureEvent(-5, 1);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new KeySignatureEvent(),
                    nonDefaultKeySignatureEvent));

            Write_Compression(
                midiFile,
                CompressionPolicy.DeleteDefaultKeySignature,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().Count(),
                        "Invalid count of Key Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var keySignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        keySignatureEvents.Length,
                        "Invalid count of Key Signature events in new file.");

                    MidiAsserts.AreEventsEqual(keySignatureEvents[0], nonDefaultKeySignatureEvent, false, "Invalid Key Signature event.");
                });
        }

        [Test]
        public void Write_Compression_DeleteDefaultSetTempo()
        {
            var nonDefaultSetTempoEvent = new SetTempoEvent(100000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new SetTempoEvent(),
                    nonDefaultSetTempoEvent));

            Write_Compression(
                midiFile,
                CompressionPolicy.DeleteDefaultSetTempo,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().Count(),
                        "Invalid count of Set Tempo events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var setTempoEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        setTempoEvents.Length,
                        "Invalid count of Set Tempo events in new file.");

                    MidiAsserts.AreEventsEqual(setTempoEvents[0], nonDefaultSetTempoEvent, false, "Invalid Set Tempo event.");
                });
        }

        [Test]
        public void Write_Compression_DeleteDefaultTimeSignature()
        {
            var nonDefaultTimeSignatureEvent = new TimeSignatureEvent(2, 16);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new TimeSignatureEvent(),
                    nonDefaultTimeSignatureEvent));

            Write_Compression(
                midiFile,
                CompressionPolicy.DeleteDefaultTimeSignature,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().Count(),
                        "Invalid count of Time Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var timeSignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        timeSignatureEvents.Length,
                        "Invalid count of Time Signature events in new file.");

                    MidiAsserts.AreEventsEqual(timeSignatureEvents[0], nonDefaultTimeSignatureEvent, false, "Invalid Time Signature event.");
                });
        }

        [Test]
        public void Write_Compression_DeleteUnknownChunks()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)),
                new UnknownChunk("abcd"));

            Write_Compression(
                midiFile,
                CompressionPolicy.DeleteUnknownChunks,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    CollectionAssert.IsNotEmpty(
                        originalMidiFile.Chunks.OfType<UnknownChunk>(),
                        "There are no Unknown chunks in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    CollectionAssert.IsEmpty(
                        newMidiFile.Chunks.OfType<UnknownChunk>(),
                        "There are Unknown chunks in new file.");
                });
        }

        #endregion

        #region Private methods

        private void Read_NonSeekableStream(ReaderSettings readerSettings)
        {
            var filePath = TestFilesProvider.GetMiscFile_14000events();
            var midiFile = MidiFile.Read(filePath);

            var nonSeekableStream = new NonSeekableStream(filePath);
            var midiFile2 = MidiFile.Read(nonSeekableStream, new ReadingSettings { ReaderSettings = readerSettings });

            MidiAsserts.AreFilesEqual(midiFile, midiFile2, true, $"File is invalid.");
        }

        private MidiFile WriteRead(MidiFile midiFile, WritingSettings writingSettings = null, ReadingSettings readingSettings = null)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mid");

            midiFile.Write(filePath, settings: writingSettings);
            midiFile = MidiFile.Read(filePath, readingSettings);

            File.Delete(filePath);
            return midiFile;
        }

        private void ReadFilesWithException<TException>(string directoryName, ReadingSettings readingSettings)
            where TException : Exception
        {
            foreach (var filePath in GetInvalidFiles(directoryName))
            {
                MidiFile midiFile = null;
                Assert.Throws<TException>(() => midiFile = MidiFile.Read(filePath, readingSettings), $"Exception not thrown for '{filePath}'.");

                var fileBasePath = TestFilesProvider.GetFileBasePath(filePath);
                var remoteFileAddress = TestFilesProvider.GetRemoteFileAddress(fileBasePath);

                //

                var nonSeekableStream = new NonSeekableStream(filePath);

                MidiFile nonSeekableFile = null;
                Assert.Throws<TException>(() => nonSeekableFile = MidiFile.Read(nonSeekableStream, readingSettings), $"Exception not thrown for file '{filePath}'.");
                MidiAsserts.AreFilesEqual(midiFile, nonSeekableFile, true, $"Non-seekable MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.PutDataInMemoryBeforeReading = true;

                MidiFile inMemoryMidiFile = null;
                Assert.Throws<TException>(() => inMemoryMidiFile = MidiFile.Read(filePath, readingSettings), $"Exception not thrown for '{filePath}'.");

                MidiAsserts.AreFilesEqual(midiFile, inMemoryMidiFile, true, $"In-memory MIDI file '{fileBasePath}' is invalid.");

                readingSettings.ReaderSettings.PutDataInMemoryBeforeReading = false;
            }
        }

        private void ReadInvalidFiles(string directoryName, ReadingSettings readingSettings)
        {
            foreach (var filePath in GetInvalidFiles(directoryName))
            {
                MidiFile midiFile = null;
                Assert.DoesNotThrow(() => midiFile = MidiFile.Read(filePath, readingSettings), $"Exception thrown for file '{filePath}'.");

                var fileBasePath = TestFilesProvider.GetFileBasePath(filePath);
                var remoteFileAddress = TestFilesProvider.GetRemoteFileAddress(fileBasePath);

                //

                var nonSeekableStream = new NonSeekableStream(filePath);

                MidiFile nonSeekableFile = null;
                Assert.DoesNotThrow(() => nonSeekableFile = MidiFile.Read(nonSeekableStream, readingSettings), $"Exception thrown for file '{filePath}'.");
                MidiAsserts.AreFilesEqual(midiFile, nonSeekableFile, true, $"Non-seekable MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.PutDataInMemoryBeforeReading = true;

                MidiFile inMemoryMidiFile = null;
                Assert.DoesNotThrow(() => inMemoryMidiFile = MidiFile.Read(filePath, readingSettings), $"Exception thrown for file '{filePath}'.");

                MidiAsserts.AreFilesEqual(midiFile, inMemoryMidiFile, true, $"In-memory MIDI file '{fileBasePath}' is invalid.");

                readingSettings.ReaderSettings.PutDataInMemoryBeforeReading = false;
            }
        }

        private IEnumerable<string> GetInvalidFiles(string directoryName)
        {
            return Directory.GetFiles(GetInvalidFilesDirectory(directoryName));
        }

        private string GetInvalidFilesDirectory(string directoryName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, TestFilesProvider.InvalidFilesPath, directoryName);
        }

        private void Write_Compression(MidiFile midiFile, CompressionPolicy compressionPolicy, Action<FileInfo, FileInfo> fileInfosAction)
        {
            MidiFileTestUtilities.Write(
                midiFile,
                filePath =>
                {
                    var fileInfo = new FileInfo(filePath);

                    MidiFileTestUtilities.Write(
                        midiFile,
                        filePath2 =>
                        {
                            var fileInfo2 = new FileInfo(filePath2);

                            fileInfosAction(fileInfo, fileInfo2);
                        },
                        new WritingSettings { CompressionPolicy = compressionPolicy });
                },
                new WritingSettings { CompressionPolicy = CompressionPolicy.NoCompression });
        }

        #endregion
    }
}
