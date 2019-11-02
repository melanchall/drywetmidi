namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Mode of the arithmetic operation between two <see cref="ITimeSpan"/>.
    /// </summary>
    public enum TimeSpanMode
    {
        /// <summary>
        /// Both time spans represent time.
        /// </summary>
        TimeTime,

        /// <summary>
        /// First time span represents time and second one represents length.
        /// </summary>
        TimeLength,

        /// <summary>
        /// Both time spans represent length.
        /// </summary>
        LengthLength
    }
}
