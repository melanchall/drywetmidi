using System;
using System.IO;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        [Test]
        public void Read_HugeDeclaredMetaEventSize_NotEnoughBytes_Abort()
        {
            var bytes = GetFileWithHugeEvent(0xFF, 0x01);
            Assert.Throws<NotEnoughBytesException>(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Abort));
        }

        [Test]
        public void Read_HugeDeclaredSysExSize_NotEnoughBytes_Abort()
        {
            var bytes = GetFileWithHugeEvent(0xF0);
            Assert.Throws<NotEnoughBytesException>(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Abort));
        }

        [Test]
        public void Read_HugeDeclaredMetaEventSize_NotEnoughBytes_Ignore()
        {
            var bytes = GetFileWithHugeEvent(0xFF, 0x01);
            Assert.DoesNotThrow(() => ReadFromBytes(bytes, NotEnoughBytesPolicy.Ignore));
        }

        private static MidiFile ReadFromBytes(byte[] bytes, NotEnoughBytesPolicy policy)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return MidiFile.Read(stream, new ReadingSettings
                {
                    NotEnoughBytesPolicy = policy,
                    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                });
            }
        }

        private static byte[] GetFileWithHugeEvent(params byte[] statusAndType)
        {
            // VLQ 0x7FFFFFFF (~2 GB)
            var vlq = new byte[] { 0x87, 0xFF, 0xFF, 0xFF, 0x7F };
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[] { 0x4D, 0x54, 0x68, 0x64, 0, 0, 0, 6, 0, 0, 0, 1, 0, 96 }, 0, 14);
                ms.Write(new byte[] { 0x4D, 0x54, 0x72, 0x6B, 0, 0, 0, 20 }, 0, 8);
                ms.WriteByte(0);
                ms.Write(statusAndType, 0, statusAndType.Length);
                ms.Write(vlq, 0, vlq.Length);
                ms.Write(new byte[] { 1, 2, 3 }, 0, 3);
                return ms.ToArray();
            }
        }
    }
}
