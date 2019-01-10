namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a system common event.
    /// </summary>
    /// <remarks>
    /// MIDI system common messages are those MIDI messages that prompt all devices on
    /// the MIDI system to respond (are not specific to a MIDI channel), but do not
    /// require an immediate response from the receiving MIDI devices.
    /// </remarks>
    public abstract class SystemCommonEvent : MidiEvent
    {
    }
}
