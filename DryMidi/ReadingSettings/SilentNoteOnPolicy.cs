namespace Melanchall.DryMidi
{
    /// <summary>
    /// Specifies how reading engine should react on Note On messages with velocity 0.
    /// The default is <see cref="NoteOff"/>.
    /// </summary>
    public enum SilentNoteOnPolicy : byte
    {
        /// <summary>
        /// Read the message as <see cref="NoteOffMessage"/>.
        /// </summary>
        NoteOff = 0,

        /// <summary>
        /// Read the message as <see cref="NoteOnMessage"/>.
        /// </summary>
        NoteOn
    }
}
