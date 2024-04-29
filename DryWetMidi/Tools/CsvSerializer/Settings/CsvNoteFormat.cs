using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how note numbers (for example, <see cref="NoteAftertouchEvent.NoteNumber"/>) should be
    /// presented in CSV. The default value is <see cref="NoteNumber"/>.
    /// </summary>
    public enum CsvNoteFormat
    {
        /// <summary>
        /// A note number should be presented as just a number. For example, <c>60</c>.
        /// </summary>
        NoteNumber = 0,

        /// <summary>
        /// A note number should be presented as a letter including accidental and octave number.
        /// For example, <c>C4</c>.
        /// </summary>
        Letter
    }
}
