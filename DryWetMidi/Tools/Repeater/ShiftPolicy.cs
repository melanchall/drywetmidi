namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Specifies how shift for <see cref="Repeater"/> should be calculated. More info in the
    /// <see href="xref:a_repeater">Repeater</see> article.
    /// </summary>
    public enum ShiftPolicy
    {
        /// <summary>
        /// Find max time within an input data and use it to shift each next part.
        /// </summary>
        ShiftByMaxTime = 0,

        /// <summary>
        /// Use fixed shift value from <see cref="RepeatingSettings.Shift"/>.
        /// </summary>
        ShiftByFixedValue,

        /// <summary>
        /// Don't shift each next part (can be useful for custom processing).
        /// </summary>
        None
    }
}
