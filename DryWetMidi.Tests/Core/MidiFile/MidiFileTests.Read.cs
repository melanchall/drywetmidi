using System;
using System.IO;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
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
            foreach (var filePath in TestFilesProvider.GetInvalidFilesPaths(DirectoriesNames.ExtraTrackChunk))
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
            foreach (var filePath in TestFilesProvider.GetInvalidFilesPaths(DirectoriesNames.ExtraTrackChunk))
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

        [Obsolete("OBS2")]
        [Test]
        public void Read_ReadFromMemory()
        {
            var settings = new ReadingSettings();
            Assert.AreEqual(BufferingPolicy.UseFixedSizeBuffer, settings.ReaderSettings.BufferingPolicy, "Initial buffering policy is invalid.");

            settings.ReaderSettings.ReadFromMemory = true;
            Assert.AreEqual(BufferingPolicy.BufferAllData, settings.ReaderSettings.BufferingPolicy, "Buffering policy is invalid after ReadFromMemory set.");

            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var expectedMidiFile = MidiFile.Read(filePath);
                var midiFile = MidiFile.Read(filePath, settings);
                MidiAsserts.AreFilesEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");
            }
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
                MidiAsserts.AreFilesEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");
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
                MidiAsserts.AreFilesEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");
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
                MidiAsserts.AreFilesEqual(expectedMidiFile, midiFile, true, $"File '{filePath}' is invalid.");

                if (checkData)
                {
                    CollectionAssert.AreNotEqual(lastBufferData, buffer, "Buffer contains the same data after reading a file.");
                    lastBufferData = buffer.ToArray();
                }
            }
        }

        #endregion

        #region Private methods

        private void ReadFilesWithException<TException>(string directoryName, ReadingSettings readingSettings)
            where TException : Exception
        {
            readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;

            foreach (var filePath in TestFilesProvider.GetInvalidFilesPaths(directoryName))
            {
                MidiFile midiFile = null;
                Assert.Throws<TException>(() => midiFile = MidiFile.Read(filePath, readingSettings), $"Exception not thrown for '{filePath}'.");

                var fileBasePath = TestFilesProvider.GetFileBasePath(filePath);
                var remoteFileAddress = TestFilesProvider.GetRemoteFileAddress(fileBasePath);

                //

                var nonSeekableStream = new NonSeekableStream(filePath);

                MidiFile nonSeekableFile = null;
                Assert.Throws<TException>(() => nonSeekableFile = MidiFile.Read(nonSeekableStream, readingSettings), $"Exception not thrown for file '{filePath}' read from non-seekable stream.");
                MidiAsserts.AreFilesEqual(midiFile, nonSeekableFile, true, $"Non-seekable MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;

                MidiFile inMemoryMidiFile = null;
                Assert.Throws<TException>(() => inMemoryMidiFile = MidiFile.Read(filePath, readingSettings), $"Exception not thrown for '{filePath}' read with putting data in memory.");

                MidiAsserts.AreFilesEqual(midiFile, inMemoryMidiFile, true, $"In-memory MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;

                MidiFile fixedSizeBufferedMidiFile = null;
                Assert.Throws<TException>(() => fixedSizeBufferedMidiFile = MidiFile.Read(filePath, readingSettings), $"Exception not thrown for '{filePath}' read with fixed size buffer.");

                MidiAsserts.AreFilesEqual(midiFile, fixedSizeBufferedMidiFile, true, $"Fixed size buffered MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;
            }
        }

        private void ReadInvalidFiles(string directoryName, ReadingSettings readingSettings)
        {
            readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;

            foreach (var filePath in TestFilesProvider.GetInvalidFilesPaths(directoryName))
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

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;

                MidiFile inMemoryMidiFile = null;
                Assert.DoesNotThrow(() => inMemoryMidiFile = MidiFile.Read(filePath, readingSettings), $"Exception thrown for file '{filePath}'.");

                MidiAsserts.AreFilesEqual(midiFile, inMemoryMidiFile, true, $"In-memory MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;

                MidiFile fixedSizeBufferedMidiFile = null;
                Assert.DoesNotThrow(() => fixedSizeBufferedMidiFile = MidiFile.Read(filePath, readingSettings), $"Exception thrown for file '{filePath}'.");

                MidiAsserts.AreFilesEqual(midiFile, fixedSizeBufferedMidiFile, true, $"In-memory MIDI file '{fileBasePath}' is invalid.");

                //

                readingSettings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;
            }
        }

        #endregion
    }
}
