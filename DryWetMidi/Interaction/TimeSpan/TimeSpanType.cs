namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// The type of a time span.
    /// </summary>
    public enum TimeSpanType
    {
        /// <summary>
        /// Metric time span which represents hours, minutes and seconds. More info in the
        /// <see href="xref:a_time_length#metric">Time and length: Representations: Metric</see> article.
        /// </summary>
        Metric,

        /// <summary>
        /// Musical time span which represents a fraction of the whole note's length. More info in the
        /// <see href="xref:a_time_length#musical">Time and length: Representations: Musical</see> article.
        /// </summary>
        Musical,

        /// <summary>
        /// Bar/beat time span which represents bars, beats and ticks. More info in the
        /// <see href="xref:a_time_length#bars-beats-and-ticks">Time and length: Representations: Bars, beats and ticks</see> article.
        /// </summary>
        BarBeatTicks,

        /// <summary>
        /// Bar/beat time span which represents bars and fractional beats (for example, 1.5 beats). More info in the
        /// <see href="xref:a_time_length#bars-beats-and-fraction">Time and length: Representations: Bars, beats and fraction</see> article.
        /// </summary>
        BarBeatFraction,

        /// <summary>
        /// MIDI time span which represents an amount of time measured in units of the time division
        /// of a MIDI file. More info in the
        /// <see href="xref:a_time_length#midi">Time and length: Representations: MIDI</see> article.
        /// </summary>
        Midi
    }
}
