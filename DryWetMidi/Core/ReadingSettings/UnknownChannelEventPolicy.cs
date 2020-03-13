namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on unknown channel event. The default is
    /// <see cref="Abort"/>.
    /// </summary>
    public enum UnknownChannelEventPolicy
    {
        /// <summary>
        /// Abort reading and throw an <see cref="UnknownChannelEventException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Skip only invalid status byte and start next MIDI event reading.
        /// </summary>
        SkipStatusByte,

        /// <summary>
        /// Skip invalid status byte and one data byte, and start next MIDI event reading.
        /// </summary>
        SkipStatusByteAndOneDataByte,

        /// <summary>
        /// Skip invalid status byte and two data bytes, and start next MIDI event reading.
        /// </summary>
        SkipStatusByteAndTwoDataBytes,

        /// <summary>
        /// Use callback to manually decide how reading engine should handle invalid unknown
        /// channel event.
        /// </summary>
        UseCallback
    }
}
