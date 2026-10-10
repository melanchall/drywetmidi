using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        private static readonly byte[] HugeVlq = new byte[] { 0x87, 0xFF, 0xFF, 0xFF, 0x7F };

        [TestCase(0xFF, 0x01)] // Text
        [TestCase(0xFF, 0x05)] // Lyric
        [TestCase(0xFF, 0x7F)] // Sequencer specific
        [TestCase(0xFF, 0x60)] // Unknown meta
        [TestCase(0xF0)]       // Normal SysEx
        [TestCase(0xF7)]       // Escape SysEx
        public void Read_HugeDeclaredEventSize_Abort(params int[] header)
        {
            var bytes = GetFileWithHugeEvent(header);
            Assert.Throws<NotEnoughBytesException>(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Abort, false));
        }

        [TestCase(0xFF, 0x01)]
        [TestCase(0xFF, 0x05)]
        [TestCase(0xFF, 0x7F)]
        [TestCase(0xFF, 0x60)]
        [TestCase(0xF0)]
        [TestCase(0xF7)]
        public void Read_HugeDeclaredEventSize_Ignore(params int[] header)
        {
            var bytes = GetFileWithHugeEvent(header);
            MidiFile file = null;
            Assert.DoesNotThrow(() => file = ReadFromBytes(bytes, NotEnoughBytesPolicy.Ignore, false));
            ClassicAssert.AreEqual(1, file.GetTrackChunks().Count());
        }

        [Test]
        public void Read_HugeDeclaredEventSize_Ignore_NonSeekableStream()
        {
            var bytes = GetFileWithHugeEvent(0xFF, 0x01);
            Assert.DoesNotThrow(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Ignore, true));
        }

        [Test]
        public void Read_HugeDeclaredEventSize_Abort_NonSeekableStream()
        {
            var bytes = GetFileWithHugeEvent(0xF0);
            Assert.Throws<NotEnoughBytesException>(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Abort, true));
        }

        [Test]
        public void Read_HugeDeclaredEventSize_FileStream()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, GetFileWithHugeEvent(0xFF, 0x01));
                Assert.Throws<NotEnoughBytesException>(() =>
                    MidiFile.Read(path, new ReadingSettings { NotEnoughBytesPolicy = NotEnoughBytesPolicy.Abort }));
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
                ClassicAssert.AreEqual("Name", ((SequenceTrackNameEvent)events[0]).Text);
                CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 0xF7 }, ((NormalSysExEvent)events[1]).Data);
            }
        }

        [Test]
        public void MidiReader_ReadBytes_ClampedToAvailable()
        {
            using (var reader = new MidiReader(new MemoryStream(new byte[] { 1, 2, 3 }), new ReaderSettings()))
            {
                CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, reader.ReadBytes(int.MaxValue));
                ClassicAssert.AreEqual(0, reader.ReadBytes(int.MaxValue).Length);
            }
        }

        [Test]
        public void MidiReader_ReadBytes_ExactAndPartial()
        {
            using (var reader = new MidiReader(new MemoryStream(new byte[] { 1, 2, 3, 4 }), new ReaderSettings()))
            {
                CollectionAssert.AreEqual(new byte[] { 1, 2 }, reader.ReadBytes(2));
                CollectionAssert.AreEqual(new byte[] { 3, 4 }, reader.ReadBytes(2));
            }
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
                return MidiFile.Read(stream, settings);
            }
        }

        private sealed class NonSeekableMemoryStream : MemoryStream
        {
            public NonSeekableMemoryStream(byte[] bytes) : base(bytes) { }
            public override bool CanSeek => false;
        }

        private static byte[] GetFileWithHugeEvent(params int[] statusAndType)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[] { 0x4D, 0x54, 0x68, 0x64, 0, 0, 0, 6, 0, 0, 0, 1, 0, 96 }, 0, 14);
                ms.Write(new byte[] { 0x4D, 0x54, 0x72, 0x6B, 0, 0, 0, 20 }, 0, 8);
                ms.WriteByte(0);
                foreach (var b in statusAndType)
                    ms.WriteByte((byte)b);
                ms.Write(HugeVlq, 0, HugeVlq.Length);
                ms.Write(new byte[] { 1, 2, 3 }, 0, 3);
                return ms.ToArray();
            }
        }
    }
}
