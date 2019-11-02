using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Represents time range used in time processing tools.
    /// </summary>
    public interface IBounds
    {
        /// <summary>
        /// Gets minimum and maximum times in MIDI ticks for the current time range.
        /// </summary>
        /// <param name="time">Time bounds should be calculated relative to.</param>
        /// <param name="tempoMap">Tempo map used to calculate bounds.</param>
        /// <returns>Pair where first item is minimum time and the second one is maximum time.</returns>
        Tuple<long, long> GetBounds(long time, TempoMap tempoMap);
    }
}
