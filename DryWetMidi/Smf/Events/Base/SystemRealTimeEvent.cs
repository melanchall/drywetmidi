namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a system real-time event.
    /// </summary>
    /// <remarks>
    /// MIDI system realtime messages are messages that are not specific to a MIDI channel but
    /// prompt all devices on the MIDI system to respond and to do so in real time.
    /// </remarks>
    public abstract class SystemRealTimeEvent : MidiEvent
    {
        #region Overrides

        internal override sealed void Read(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        internal override sealed void Write(MidiWriter writer, WritingSettings settings)
        {
        }

        internal override sealed int GetSize(WritingSettings settings)
        {
            return 0;
        }

        #endregion
    }
}
