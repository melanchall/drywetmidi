namespace Melanchall.DryWetMidi.Core
{
    internal interface IEventWriter
    {
        #region Methods

        void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte);

        int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte);

        byte GetStatusByte(MidiEvent midiEvent);

        #endregion
    }
}
