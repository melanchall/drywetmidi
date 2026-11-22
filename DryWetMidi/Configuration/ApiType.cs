namespace Melanchall.DryWetMidi.Configuration
{
    /// <summary>
    /// Specifies the native MIDI API used by DryWetMIDI.
    /// </summary>
    public enum ApiType
    {
        /// <summary>
        /// Core MIDI on macOS.
        /// </summary>
        CoreMidi = 0,

        /// <summary>
        /// Windows Multimedia MIDI (WinMM) on Windows.
        /// </summary>
        WinMM = 1,

        /// <summary>
        /// Windows MIDI Services on Windows.
        /// </summary>
        WindowsMidiServices = 2,
    }
}
