using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Callback used to create tick generator for <see cref="MidiClock"/>.
    /// </summary>
    /// <param name="interval">Interval between two consecutive ticks.</param>
    /// <returns>Tick generator created by the callback.</returns>
    public delegate ITickGenerator CreateTickGeneratorCallback(TimeSpan interval);
}
