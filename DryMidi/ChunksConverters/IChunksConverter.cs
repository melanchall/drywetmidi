using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    public interface IChunksConverter
    {
        IEnumerable<Chunk> Convert(IEnumerable<Chunk> chunks);
    }
}
