namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a note's number is presented in CSV representation: either a number or
    /// a letter (for example, A#5).
    /// </summary>
    public enum NoteNumberFormat
    {
        /// <summary>
        /// A note's number is presented as just a number.
        /// </summary>
        NoteNumber,

        /// <summary>
        /// A note's number is presented as a letter.
        /// </summary>
        Letter
    }
}
