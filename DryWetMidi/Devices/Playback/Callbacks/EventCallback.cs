using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public delegate MidiEvent EventCallback(MidiEvent rawEventData, long rawTime, TimeSpan playbackTime);
}
