using System;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public interface IBounds
    {
        Tuple<long, long> GetBounds(long time, TempoMap tempoMap);
    }
}
