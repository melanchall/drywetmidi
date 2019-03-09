namespace Melanchall.DryWetMidi.Smf
{
    internal interface ISysExDataReader
    {
        SysExData Read(MidiReader reader, SysExDataReadingSettings settings);
    }
}
