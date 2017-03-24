namespace Melanchall.DryMidi
{
    internal interface IMessageReader
    {
        Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);
    }
}
