namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// The type of a time span.
    /// </summary>
    public enum TimeSpanType
    {
        /// <summary>
        /// Metric time span which represents hours, minutes and seconds.
        /// </summary>
        Metric,

        /// <summary>
        /// Musical time span which represents a fraction of the whole note's length.
        /// </summary>
        Musical,

        /// <summary>
        /// Bar/beat time span which represents bars, beats and ticks.
        /// </summary>
        BarBeatTicks,

        /// <summary>
        /// Bar/beat time span which represents bars and fractional beats (for example, 1.5 beats).
        /// </summary>
        BarBeatFraction,

        /// <summary>
        /// MIDI time span which represnts an amount of time measured in units of the time division
        /// of a MIDI file.
        /// </summary>
        Midi
    }
}
