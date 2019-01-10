namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Type of an output MIDI device.
    /// </summary>
    public enum OutputDeviceType : ushort
    {
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
        SquareWaveSynth = 3,

        /// <summary>
        /// FM synthesizer.
        /// </summary>
        FmSynth = 4,

        /// <summary>
        /// Microsoft MIDI mapper.
        /// </summary>
        MidiMapper = 5,

        /// <summary>
        /// Hardware wavetable synthesizer.
        /// </summary>
        WavetableSynth = 6,

        /// <summary>
        /// Software synthesizer.
        /// </summary>
        SoftwareSynth = 7
    }
}
