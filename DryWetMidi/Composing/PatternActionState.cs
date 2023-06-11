namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Defines the state of a pattern action.
    /// </summary>
    /// <remarks>
    /// Pattern actions correspond to methods on <see cref="PatternBuilder"/>. For example,
    /// <see cref="PatternBuilder.Note(MusicTheory.Interval, Interaction.ITimeSpan, Common.SevenBitNumber?)"/>
    /// creates 'Add note' action, so the state will define whether a note will be enabled, disabled or excluded
    /// from pattern at all.
    /// </remarks>
    public enum PatternActionState
    {
        /// <summary>
        /// Action is enabled and will be exported to MIDI data.
        /// </summary>
        Enabled,

        /// <summary>
        /// Action is disabled. It will still occupy time span corresponding to the action,
        /// but MIDI data will not be generated for it.
        /// </summary>
        Disabled,

        /// <summary>
        /// Action is completely excluded from pattern. It won't occupy time span and
        /// MIDI data will not be generated.
        /// </summary>
        Excluded
    }
}
