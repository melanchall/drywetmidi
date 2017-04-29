namespace Melanchall.DryMidi
{
    internal interface IEventReader
    {
        MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);
    }
}
