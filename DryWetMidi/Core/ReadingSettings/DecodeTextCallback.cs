namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Callback used to decode a string from the specified bytes during reading a text-based
    /// meta event.
    /// </summary>
    /// <param name="bytes">Bytes to decode a string from.</param>
    /// <param name="settings">Settings used to read MIDI data.</param>
    /// <returns>A string decoded from given bytes.</returns>
    public delegate string DecodeTextCallback(byte[] bytes, ReadingSettings settings);
}
