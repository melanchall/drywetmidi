using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Tools
{
    internal delegate MidiEvent EventParser(string[] parameters, MidiFileCsvConversionSettings settings);
}
