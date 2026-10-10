using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private sealed class NonSeekableMemoryStream : MemoryStream
        {
            public NonSeekableMemoryStream(byte[] bytes) : base(bytes) { }
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

            using (var ms = new MemoryStream())
            {
                file.Write(ms);
                ms.Position = 0;
                var read = MidiFile.Read(ms, new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort });
                var events = read.GetTrackChunks().Single().Events;
                ClassicAssert.AreEqual("Name", ((SequenceTrackNameEvent)events[0]).Text, "Track name is invalid.");
                CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 0xF7 }, ((NormalSysExEvent)events[1]).Data, "SysEx data is invalid.");
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
        }

        #endregion

        #region Private methods

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
                return MidiFile.Read(stream, settings);
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
