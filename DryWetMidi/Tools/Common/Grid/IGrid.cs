using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public interface IGrid
    {
        IEnumerable<long> GetTimes(TempoMap tempoMap);
    }
}
