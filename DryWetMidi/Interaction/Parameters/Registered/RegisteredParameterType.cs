namespace Melanchall.DryWetMidi.Interaction
{
    // https://www.midi.org/specifications-old/item/table-3-control-change-messages-data-bytes-2
    public enum RegisteredParameterType : byte
    {
        PitchBendSensitivity,
        ChannelFineTuning,
        ChannelCoarseTuning,
        TuningProgramChange,
        TuningBankSelect,
        ModulationDepthRange

        // TODO: MPE Configurarion
    }
}
