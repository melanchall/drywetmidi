namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Specifies how reading engine should react on Note On events with velocity 0.
    /// The default is <see cref="NoteOff"/>.
    /// </summary>
    public enum SilentNoteOnPolicy : byte
    {
        /// <summary>
        /// Read the event as <see cref="NoteOffEvent"/>.
        /// </summary>
        NoteOff = 0,

        /// <summary>
        /// Read the event as <see cref="NoteOnEvent"/>.
        /// </summary>
        NoteOn
    }
}
