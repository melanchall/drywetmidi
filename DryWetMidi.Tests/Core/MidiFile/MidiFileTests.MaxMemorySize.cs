using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        [Test]
        public void Read_MaxMemorySizeExceeded_Throws()
        {
            var bytes = GetFileBytesWithEvents(1000);
            using (var stream = new MemoryStream(bytes))
            {
                var ex = Assert.Throws<MidiFileTooLargeException>(
                    () => MidiFile.Read(stream, new ReadingSettings { MaxMemorySize = 10000 }));
                ClassicAssert.AreEqual(10000, ex!.MaxSize);
                ClassicAssert.Greater(ex.EstimatedSize, 10000);
            }
        }

        [Test]
        public void Read_MaxMemorySizeNotExceeded_Reads()
        {
            var bytes = GetFileBytesWithEvents(1000);
            using (var stream = new MemoryStream(bytes))
            {
                var file = MidiFile.Read(stream, new ReadingSettings { MaxMemorySize = 10_000_000 });
                ClassicAssert.AreEqual(1000, file.GetTrackChunks().Single().Events.Count);
            }
        }

        [Test]
        public void Read_MaxMemorySizeExceeded_UnknownChunk_Throws()
        {
            var file = new MidiFile(new UnknownChunk("Abcd") { Data = new byte[100000] });
            using (var stream = new MemoryStream())
            {
                file.Write(stream);
                stream.Position = 0;
                Assert.Throws<MidiFileTooLargeException>(
                    () => MidiFile.Read(stream, new ReadingSettings { MaxMemorySize = 50000 }));
            }
        }

        private static byte[] GetFileBytesWithEvents(int count)
        {
            var file = new MidiFile(new TrackChunk(Enumerable.Range(0, count).Select(_ => new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100))));
            using (var stream = new MemoryStream())
            {
                file.Write(stream);
                return stream.ToArray();
            }
        }
    }
}
