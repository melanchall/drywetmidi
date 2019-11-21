using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    internal delegate object[] EventParametersGetter(MidiEvent midiEvent, MidiFileCsvConversionSettings settings);
}
