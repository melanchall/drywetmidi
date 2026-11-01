using System;
using System.IO;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private sealed class NonSeekableMemoryStream : MemoryStream
        {
            public NonSeekableMemoryStream(byte[] bytes)
                : base(bytes)
            {
            }

            public override bool CanSeek => false;
        }

        #endregion

        #region Constants

        private const byte UnknownMetaEventType = 0x60;

        // VLQ encoding of 0x7FFFFFFF (about 2 GB)
        private static readonly byte[] HugeVlq = new byte[] { 0x87, 0xFF, 0xFF, 0xFF, 0x7F };

        #endregion

        #region Test methods

        [TestCase(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.Text)]
        [TestCase(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.Lyric)]
        [TestCase(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.SequencerSpecific)]
        [TestCase(EventStatusBytes.Global.Meta, UnknownMetaEventType)]
        [TestCase(EventStatusBytes.Global.NormalSysEx)]
        [TestCase(EventStatusBytes.Global.EscapeSysEx)]
        public void Read_HugeDeclaredEventSize_Abort(params byte[] header)
        {
            var bytes = GetFileWithHugeEvent(header);
            Assert.Throws<NotEnoughBytesException>(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Abort, false), "Exception not thrown.");
        }

        [TestCase(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.Text)]
        [TestCase(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.Lyric)]
        [TestCase(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.SequencerSpecific)]
        [TestCase(EventStatusBytes.Global.Meta, UnknownMetaEventType)]
        [TestCase(EventStatusBytes.Global.NormalSysEx)]
        [TestCase(EventStatusBytes.Global.EscapeSysEx)]
        public void Read_HugeDeclaredEventSize_Ignore(params byte[] header)
        {
            var bytes = GetFileWithHugeEvent(header);
            var file = ReadFromBytes(bytes, NotEnoughBytesPolicy.Ignore, false);
            ClassicAssert.AreEqual(1, file.GetTrackChunks().Count(), "Track chunk is not read.");
        }

        [Test]
        public void Read_HugeDeclaredEventSize_Ignore_NonSeekableStream()
        {
            var bytes = GetFileWithHugeEvent(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.Text);
            ReadFromBytes(bytes, NotEnoughBytesPolicy.Ignore, true);
        }

        [Test]
        public void Read_HugeDeclaredEventSize_Abort_NonSeekableStream()
        {
            var bytes = GetFileWithHugeEvent(EventStatusBytes.Global.NormalSysEx);
            Assert.Throws<NotEnoughBytesException>(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Abort, true), "Exception not thrown.");
        }

        [Test]
        public void Read_HugeDeclaredEventSize_FileStream()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, GetFileWithHugeEvent(EventStatusBytes.Global.Meta, EventStatusBytes.Meta.Text));

                Assert.Throws<NotEnoughBytesException>(() =>
                    MidiFile.Read(path, new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort }),
                    "Exception not thrown.");

                var asynchronousException = Assert.Throws<AggregateException>(() =>
                    MidiFile.ReadAsync(
                        path,
                        new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort },
                        cancellationToken: CancellationToken.None).Wait(),
                    "Async path reading did not reject the huge declared event size.");

                ClassicAssert.IsInstanceOf<NotEnoughBytesException>(
                    asynchronousException.InnerException,
                    "Async path reading returned a different exception.");

                using (var synchronousStream = File.OpenRead(path))
                {
                    AssertHugeDeclaredSizeAbort(synchronousStream, false);
                }

                using (var asynchronousStream = File.OpenRead(path))
                {
                    AssertHugeDeclaredSizeAbort(asynchronousStream, true);
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Read_ValidSizedEvent_Unaffected()
        {
            var file = new MidiFile(new TrackChunk(
                new SequenceTrackNameEvent("Name"),
                new NormalSysExEvent(new byte[] { 1, 2, 3, 0xF7 })));

            using (var synchronousStream = new MemoryStream())
            {
                file.Write(synchronousStream);
                synchronousStream.Position = 0;
                var synchronousFile = MidiFile.Read(synchronousStream, new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort });

                AssertValidSizedEvents(synchronousFile);

                using (var asynchronousStream = new MemoryStream(synchronousStream.ToArray()))
                {
                    var asynchronousFile = MidiFile.ReadAsync(
                        asynchronousStream,
                        new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort },
                        cancellationToken: CancellationToken.None).Result;

                    AssertValidSizedEvents(asynchronousFile);
                    MidiAsserts.AreEqual(synchronousFile, asynchronousFile, true, "Async valid event reading differs.");
                }
            }
        }

        [Test]
        public void MidiReader_ReadBytes_ClampedToAvailable()
        {
            using (var reader = new MidiReader(new MemoryStream(new byte[] { 1, 2, 3 }), new ReaderSettings()))
            {
                CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, reader.ReadBytes(int.MaxValue), "Bytes are invalid.");
                ClassicAssert.AreEqual(0, reader.ReadBytes(int.MaxValue).Length, "Bytes are returned at the end of stream.");
            }
        }

        [Test]
        public void MidiReader_ReadBytes_ExactAndPartial()
        {
            using (var reader = new MidiReader(new MemoryStream(new byte[] { 1, 2, 3, 4 }), new ReaderSettings()))
            {
                CollectionAssert.AreEqual(new byte[] { 1, 2 }, reader.ReadBytes(2), "Bytes are invalid.");
                CollectionAssert.AreEqual(new byte[] { 3, 4 }, reader.ReadBytes(2), "Bytes are invalid.");
            }
        }

        [Test]
        public void Read_HugeDeclaredUnknownChunkSize_Skip()
        {
            var bytes = new byte[]
            {
                0x4D, 0x54, 0x68, 0x64, // Header chunk ID (MThd)
                0, 0, 0, 6,             // Header chunk size
                0, 0,                   // File format
                0, 0,                   // Tracks count
                0, 96,                  // Time division
                0x58, 0x58, 0x58, 0x58, // Unknown chunk ID (XXXX)
                0xFF, 0xFF, 0xFF, 0xFF, // Unknown chunk declared size (4 GB)
                1, 2, 3                 // Actual chunk data (3 bytes)
            };
            var settings = new ReadingSettings { UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip };
            var file = MidiFile.Read(new MemoryStream(bytes), settings);
            ClassicAssert.AreEqual(0, file.Chunks.Count, "Unknown chunk is not skipped.");

            using (var stream = new MemoryStream(bytes))
            {
                file = MidiFile.ReadAsync(
                    stream,
                    settings,
                    cancellationToken: System.Threading.CancellationToken.None).Result;

                ClassicAssert.AreEqual(0, file.Chunks.Count, "Unknown chunk is not skipped asynchronously.");
            }
        }

        #endregion

        #region Private methods

        private static void AssertValidSizedEvents(MidiFile file)
        {
            var events = file.GetTrackChunks().Single().Events;

            ClassicAssert.AreEqual("Name", ((SequenceTrackNameEvent)events[0]).Text, "Track name is invalid.");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 0xF7 }, ((NormalSysExEvent)events[1]).Data, "SysEx data is invalid.");
        }

        private static void AssertHugeDeclaredSizeAbort(Stream stream, bool asynchronous)
        {
            var settings = new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort };
            TestDelegate read = () =>
            {
                if (!asynchronous)
                {
                    MidiFile.Read(stream, settings);
                    return;
                }

                try
                {
                    MidiFile.ReadAsync(stream, settings, CancellationToken.None).Wait();
                }
                catch (AggregateException exception)
                {
                    throw exception.InnerException;
                }
            };

            Assert.Throws<NotEnoughBytesException>(read, "Reading did not reject the huge declared event size.");
        }

        private static MidiFile ReadFromBytes(byte[] bytes, NotEnoughBytesPolicy policy, bool nonSeekable)
        {
            var settings = new ReadingSettings
            {
                NotEnoughBytesPolicy = policy,
                MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
            };

            using (var stream = nonSeekable ? (Stream)new NonSeekableMemoryStream(bytes) : new MemoryStream(bytes))
            {
                MidiFile expectedFile;

                try
                {
                    expectedFile = MidiFile.Read(stream, settings);
                }
                catch (Exception exception)
                {
                    using (var asynchronousStream = nonSeekable ? (Stream)new NonSeekableMemoryStream(bytes) : new MemoryStream(bytes))
                    {
                        var aggregate = Assert.Throws<AggregateException>(() => MidiFile.ReadAsync(
                            asynchronousStream,
                            settings,
                            cancellationToken: System.Threading.CancellationToken.None).Wait(), "Async reading did not reject the huge declared event size.");

                        ClassicAssert.AreEqual(exception.GetType(), aggregate!.InnerException!.GetType(), "Async huge declared size exception type differs.");
                    }

                    throw;
                }

                using (var asynchronousStream = nonSeekable ? (Stream)new NonSeekableMemoryStream(bytes) : new MemoryStream(bytes))
                {
                    var actualFile = MidiFile.ReadAsync(
                        asynchronousStream,
                        settings,
                        cancellationToken: System.Threading.CancellationToken.None).Result;

                    MidiAsserts.AreEqual(expectedFile, actualFile, true, "Async huge declared size reading differs.");
                }

                return expectedFile;
            }
        }

        private static byte[] GetFileWithHugeEvent(params byte[] statusAndType)
        {
            var fileHeader = new byte[]
            {
                0x4D, 0x54, 0x68, 0x64, // Header chunk ID (MThd)
                0, 0, 0, 6,             // Header chunk size
                0, 0,                   // File format
                0, 1,                   // Tracks count
                0, 96                   // Time division
            };

            var trackHeader = new byte[]
            {
                0x4D, 0x54, 0x72, 0x6B, // Track chunk ID (MTrk)
                0, 0, 0, 20,            // Track chunk size
                0                       // Delta-time
            };

            var actualEventData = new byte[] { 1, 2, 3 };

            return fileHeader
                .Concat(trackHeader)
                .Concat(statusAndType)  // Status byte (and meta event type)
                .Concat(HugeVlq)        // Declared size of event data
                .Concat(actualEventData)
                .ToArray();
        }

        #endregion
    }
}
