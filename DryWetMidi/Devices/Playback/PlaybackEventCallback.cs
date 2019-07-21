using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public delegate MidiEvent PlaybackEventCallback(MidiEvent midiEvent, TimeSpan time, long rawTime);
}
