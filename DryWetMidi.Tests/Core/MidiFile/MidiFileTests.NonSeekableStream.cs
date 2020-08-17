using System;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
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

        #endregion

        #region Test methods

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
        [TestCase(4)]
#if !COVERAGE
        [TestCase(4096)]
        [TestCase(500000)]
#endif
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

        #endregion
    }
}
