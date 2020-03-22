namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on <c>Note On</c> events with velocity of zero.
    /// The default is <see cref="NoteOff"/>.
    /// </summary>
    public enum SilentNoteOnPolicy : byte
    {
        /// <summary>
        /// Read an event as <see cref="NoteOffEvent"/>.
        /// </summary>
        NoteOff = 0,

        /// <summary>
        /// Read an event as <see cref="NoteOnEvent"/>.
        /// </summary>
        NoteOn
    }
}
