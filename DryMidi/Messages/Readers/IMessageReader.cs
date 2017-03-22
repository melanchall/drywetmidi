namespace Melanchall.DryMidi
{
    public interface IMessageReader
    {
        Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);
    }
}
