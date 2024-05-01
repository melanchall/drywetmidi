namespace Melanchall.DryWetMidi.Interaction
{
    // TODO: https://midi.org/midi-1-0-control-change-messages
    /// <summary>
    /// the type of a registered parameter.
    /// </summary>
    public enum RegisteredParameterType : byte
    {
        /// <summary>
        /// Pitch Bend Sensitivity registered parameter.
        /// </summary>
        PitchBendSensitivity,

        /// <summary>
        /// Channel Fine Tuning registered parameter.
        /// </summary>
        ChannelFineTuning,

        /// <summary>
        /// Channel Coarse Tuning registered parameter.
        /// </summary>
        ChannelCoarseTuning,

        /// <summary>
        /// Tuning Program Change registered parameter.
        /// </summary>
        TuningProgramChange,

        /// <summary>
        /// Tuning Bank Select registered parameter.
        /// </summary>
        TuningBankSelect,

        /// <summary>
        /// Modulation Depth Range (Vibrato Depth Range) registered parameter.
        /// </summary>
        ModulationDepthRange

        // TODO: MPE Configuration
    }
}
