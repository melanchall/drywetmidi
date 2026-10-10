using System.Linq;
using Melanchall.DryWetMidi.Common;
using System.IO;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        [Test]
        public void Read_DataLargerThanMaxDataSize_Throws()
        {
            var bytes = GetSmallFileBytes();
            using (var stream = new MemoryStream(bytes))
            {
                var ex = Assert.Throws<MidiDataTooLargeException>(
                    () => MidiFile.Read(stream, new ReadingSettings { MaxDataSize = bytes.Length - 1 }));
                ClassicAssert.AreEqual(bytes.Length, ex!.DataSize);
            }
        }

        [Test]
        public void Read_DataNotLargerThanMaxDataSize_Reads()
        {
            var bytes = GetSmallFileBytes();
            using (var stream = new MemoryStream(bytes))
            {
                var file = MidiFile.Read(stream, new ReadingSettings { MaxDataSize = bytes.Length });
                ClassicAssert.AreEqual(1, file.GetTrackChunks().Count());
            }
        }

        private static byte[] GetSmallFileBytes()
        {
            var file = new MidiFile(new TrackChunk(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100)));
            using (var stream = new MemoryStream())
            {
                file.Write(stream);
                return stream.ToArray();
            }
        }
    }
}
