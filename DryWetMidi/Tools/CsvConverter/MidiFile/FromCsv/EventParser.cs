using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    internal delegate MidiEvent EventParser(string[] parameters, MidiFileCsvConversionSettings settings);
}
