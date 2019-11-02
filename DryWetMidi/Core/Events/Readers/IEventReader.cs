namespace Melanchall.DryWetMidi.Core
{
    internal interface IEventReader
    {
        MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);
    }
}
