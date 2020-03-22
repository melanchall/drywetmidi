namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Callback used to decode a string from the specified bytes during reading a text-based
    /// meta event's text.
    /// </summary>
    /// <param name="bytes">Bytes to decode a string from.</param>
    /// <param name="settings">Settings used to read MIDI data.</param>
    /// <returns>A string decoded from given bytes.</returns>
    /// <remarks>
    /// <para>All meta events types derived from <see cref="BaseTextEvent"/> have <see cref="BaseTextEvent.Text"/>
    /// property. Value of this property will be read using this callback if <see cref="ReadingSettings.DecodeTextCallback"/>
    /// is set.</para>
    /// </remarks>
    public delegate string DecodeTextCallback(byte[] bytes, ReadingSettings settings);
}
