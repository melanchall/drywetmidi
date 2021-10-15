namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Type of a MIDI output device on Windows (see <c>wTechnology</c> field
    /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
    /// MIDIOUTCAPS</see>).
    /// </summary>
    public enum OutputDeviceTechnology
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// MIDI hardware port.
        /// </summary>
        MidiPort = 1,

        /// <summary>
        /// Synthesizer.
        /// </summary>
        Synth = 2,

        /// <summary>
        /// Square wave synthesizer.
        /// </summary>
        SquareSynth = 3,

        /// <summary>
        /// FM synthesizer.
        /// </summary>
        FmSynth = 4,

        /// <summary>
        /// Microsoft MIDI mapper.
        /// </summary>
        Mapper = 5,

        /// <summary>
        /// Hardware wavetable synthesizer.
        /// </summary>
        Wavetable = 6,

        /// <summary>
        /// Software synthesizer.
        /// </summary>
        SoftwareSynth = 7
    }
}
