namespace Melanchall.DryWetMidi.Smf
{
    internal interface IEventReader
    {
        MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);
    }
}
