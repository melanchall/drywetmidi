using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a time grid which is the set of points in time.
    /// </summary>
    public interface IGrid
    {
        /// <summary>
        /// Gets times produced by the current grid.
        /// </summary>
        /// <param name="tempoMap">Tempo map used to get grid's times.</param>
        /// <returns>Collection of times (in MIDI ticks) of the current grid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        IEnumerable<long> GetTimes(TempoMap tempoMap);
    }
}
