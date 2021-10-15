using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Callback used to modify MIDI event before playing.
    /// </summary>
    /// <param name="midiEvent">MIDI event to modify.</param>
    /// <param name="time">Absolute metric time of the event.</param>
    /// <param name="rawTime">Absolute time of the event in MIDI ticks.</param>
    /// <returns>MIDI event that should be played. It can be modified original event or a new one.</returns>
    public delegate MidiEvent PlaybackEventCallback(MidiEvent midiEvent, TimeSpan time, long rawTime);
}
