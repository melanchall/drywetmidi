using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SplitFileByChunksSettings
    {
        #region Properties

        public Predicate<MidiChunk> Filter { get; set; }

        #endregion
    }
}
