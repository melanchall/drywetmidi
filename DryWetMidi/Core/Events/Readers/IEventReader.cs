namespace Melanchall.DryWetMidi.Core
{
    internal interface IEventReader
    {
        #region Methods

        MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);

        #endregion
    }
}
