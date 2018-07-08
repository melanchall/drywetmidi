using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Tools
{
    internal delegate object[] EventParametersGetter(MidiEvent midiEvent, MidiFileCsvConversionSettings settings);
}
