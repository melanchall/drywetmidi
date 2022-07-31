namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Defines format of a MIDI data bytes. Depending on this format the specific
    /// MIDI data reading/writing rules can be applied.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see cref="File"/> format specified, a system exclusive  bytes layout is
    /// <c>F0 &lt;length&gt; &lt;bytes to be transmitted after F0&gt;</c>, and <c>0xFF</c>
    /// status byte means a meta event.
    /// </para>
    /// <para>
    /// If <see cref="Device"/> format specified, a system exclusive  bytes layout is
    /// <c>F0 &lt;bytes to be transmitted after F0&gt;</c>, and <c>0xFF</c>
    /// status byte means Reset system real-time event.
    /// </para>
    /// </remarks>
    public enum BytesFormat
    {
        /// <summary>
        /// Format used by MIDI files.
        /// </summary>
        File = 0,

        /// <summary>
        /// Format used in communication with MIDI devices.
        /// </summary>
        Device
    }
}
